import { Client, Databases, Query } from 'node-appwrite';
import admin from 'firebase-admin';

let firebaseInitialized = false;

// Lazy init so cold starts share one Firebase app instance
function initFirebase() {
    if (firebaseInitialized) return;

    const serviceAccountJson = process.env.FIREBASE_SERVICE_ACCOUNT;
    if (!serviceAccountJson) {
        throw new Error('FIREBASE_SERVICE_ACCOUNT env var is missing');
    }

    const serviceAccount = JSON.parse(serviceAccountJson);
    admin.initializeApp({
        credential: admin.credential.cert(serviceAccount),
    });
    firebaseInitialized = true;
}

// Localized notification bodies — keys match AppResources.resx in the MAUI app
const messages = {
    en: {
        above: (temp, t) => `Temperature is ${temp.toFixed(1)}°C, above your ${t.toFixed(1)}°C threshold`,
        below: (temp, t) => `Temperature is ${temp.toFixed(1)}°C, below your ${t.toFixed(1)}°C threshold`,
    },
    it: {
        above: (temp, t) => `Temperatura ${temp.toFixed(1)}°C, sopra la soglia ${t.toFixed(1)}°C`,
        below: (temp, t) => `Temperatura ${temp.toFixed(1)}°C, sotto la soglia ${t.toFixed(1)}°C`,
    },
    de: {
        above: (temp, t) => `Temperatur ${temp.toFixed(1)}°C, über der Grenze ${t.toFixed(1)}°C`,
        below: (temp, t) => `Temperatur ${temp.toFixed(1)}°C, unter der Grenze ${t.toFixed(1)}°C`,
    },
};

function buildBody(lang, type, temp, threshold) {
    const dict = messages[lang] || messages.en;
    return dict[type](temp, threshold);
}

// Returns 'below' | 'above' | 'normal'
function computeState(temp, min, max) {
    if (temp < min) return 'below';
    if (temp > max) return 'above';
    return 'normal';
}

async function fetchWeather(lat, lon, lang, apiKey) {
    const url = `https://api.openweathermap.org/data/2.5/weather?lat=${lat}&lon=${lon}&appid=${apiKey}&units=metric&lang=${lang}`;
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error(`OpenWeatherMap ${response.status}: ${await response.text()}`);
    }
    return await response.json();
}

async function sendPush(token, title, body, log) {
    const message = { token, notification: { title, body } };
    try {
        const id = await admin.messaging().send(message);
        log(`[FCM] sent ${id}`);
        return { sent: true, invalidToken: false };
    } catch (err) {
        // Stale tokens are normal: app uninstalled, data wiped, etc.
        const invalid =
            err.code === 'messaging/registration-token-not-registered' ||
            err.code === 'messaging/invalid-registration-token';
        log(`[FCM] error code=${err.code} msg=${err.message}`);
        return { sent: false, invalidToken: invalid };
    }
}

export default async ({ req, res, log, error }) => {
    try {
        initFirebase();

        const appwrite = new Client()
            .setEndpoint(process.env.APPWRITE_ENDPOINT)
            .setProject(process.env.APPWRITE_PROJECT_ID)
            .setKey(process.env.APPWRITE_API_KEY);

        const databases = new Databases(appwrite);
        const databaseId = process.env.APPWRITE_DATABASE_ID;
        const collectionId = process.env.APPWRITE_COLLECTION_ID;
        const owmApiKey = process.env.OPENWEATHER_API_KEY;

        // Pull only the locations that actually want notifications
        const result = await databases.listDocuments(databaseId, collectionId, [
            Query.equal('notificationsEnabled', true),
            Query.limit(500),
        ]);

        log(`[scan] ${result.documents.length} locations to check`);

        let pushed = 0;
        let skipped = 0;
        let errors = 0;

        for (const doc of result.documents) {
            try {
                const {
                    $id,
                    name,
                    lat,
                    lon,
                    deviceToken,
                    tempThresholdMin,
                    tempThresholdMax,
                    language,
                    lastNotifiedState,
                } = doc;

                if (!deviceToken) {
                    skipped++;
                    continue;
                }

                const lang = language || 'en';
                const weather = await fetchWeather(lat, lon, lang, owmApiKey);
                const currentTemp = weather.main.temp;

                const newState = computeState(currentTemp, tempThresholdMin, tempThresholdMax);
                const previousState = lastNotifiedState || 'normal';

                // Anti-spam (slide 6.1 §17): notify only on transitions out of "normal".
                // Transitions normal→below, normal→above, below→above, above→below = push.
                // Same-state runs (below→below, normal→normal) are silent.
                const shouldNotify = newState !== 'normal' && newState !== previousState;

                if (shouldNotify) {
                    const threshold = newState === 'below' ? tempThresholdMin : tempThresholdMax;
                    const body = buildBody(lang, newState, currentTemp, threshold);
                    const result = await sendPush(deviceToken, name, body, log);
                    if (result.sent) pushed++;
                    if (result.invalidToken) {
                        // Drop stale token so we don't keep retrying it
                        await databases.updateDocument(databaseId, collectionId, $id, {
                            deviceToken: '',
                        });
                        log(`[cleanup] cleared stale token for ${name}`);
                    }
                }

                // Persist state transitions even when no push is sent
                // (e.g. above→normal). Otherwise we'd never recover from "above" state.
                if (newState !== previousState) {
                    await databases.updateDocument(databaseId, collectionId, $id, {
                        lastNotifiedState: newState,
                    });
                }
            } catch (err) {
                errors++;
                error(`[loc ${doc.$id} ${doc.name}] ${err.message}`);
            }
        }

        log(`[done] pushed=${pushed} skipped=${skipped} errors=${errors}`);

        return res.json({
            success: true,
            processed: result.documents.length,
            pushed,
            skipped,
            errors,
        });
    } catch (err) {
        error(`[fatal] ${err.message}\n${err.stack}`);
        return res.json({ success: false, error: err.message }, 500);
    }
};
