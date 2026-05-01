# Meteo Notification Function

Appwrite Function that runs every 30 minutes, checks the temperature for every saved
location, and sends an FCM push notification when the temperature crosses the
configured `tempThresholdMin` / `tempThresholdMax`.

Implements the server-side requirement from `ProjectRequirement.pdf`:

> The server will periodically check temperature changes for each saved location.
> The server will send notifications to user devices when the temperature exceeds
> the configured thresholds. Notifications will be sent using Firebase.

Pattern follows slide 6.1 §32-33 (Node.js + `firebase-admin`) and slide 8.1 (Appwrite SDK).

## How the anti-spam logic works

Each location document carries a `lastNotifiedState` field with one of:

- `normal` — temperature is between min and max
- `below` — temperature is below `tempThresholdMin`
- `above` — temperature is above `tempThresholdMax`

A push is sent only on a transition **out of `normal`** (or between `below` ↔ `above`).
Same-state runs (e.g. `below` → `below`) stay silent — this prevents notification
fatigue (slide 6.1 §17).

## Setup checklist

The setup is split between three places: the Appwrite Console, the Firebase Console,
and this folder.

### 1. Appwrite collection — add `lastNotifiedState` attribute

In Appwrite Console → Databases → `locations` collection → Attributes → Create:

- Type: **String**
- Key: `lastNotifiedState`
- Size: `16`
- Required: **No**
- Default: leave empty

### 2. Firebase — generate Service Account key

Firebase Console → Project Settings (⚙) → **Service accounts** tab →
**Generate new private key**. Save the JSON file securely; it will be pasted
into an Appwrite environment variable in step 4.

### 3. Appwrite — create API Key for this function

Appwrite Console → Project → **Integrations** → API Keys → **Create API Key**.

Required scopes:

- `databases.read`
- `documents.read`
- `documents.write`

Copy the value; you'll paste it as `APPWRITE_API_KEY` below.

### 4. Appwrite — create the Function

Appwrite Console → Functions → **Create function**:

- Runtime: **Node.js 22.0** (or newer)
- Entrypoint: `src/main.js`
- Schedule: `*/30 * * * *` (every 30 minutes)
- Timeout: `60s`
- Permissions: leave default (the function uses its own API key, not user sessions)

Then in **Settings → Variables**, add the values from `.env.example`:

| Key | Value |
| --- | --- |
| `APPWRITE_ENDPOINT` | `https://fra.cloud.appwrite.io/v1` |
| `APPWRITE_PROJECT_ID` | `69f37144002337d6b4dd` |
| `APPWRITE_DATABASE_ID` | `69f371ae001041d3fa90` |
| `APPWRITE_COLLECTION_ID` | `locations` |
| `APPWRITE_API_KEY` | (from step 3) |
| `OPENWEATHER_API_KEY` | (same key the MAUI client uses) |
| `FIREBASE_SERVICE_ACCOUNT` | (paste the whole JSON from step 2 as a single line) |

> **Tip for `FIREBASE_SERVICE_ACCOUNT`**: open `serviceAccountKey.json`, copy the
> entire content, and paste it as the variable value. Appwrite handles multi-line
> values fine; you don't need to escape newlines manually.

### 5. Deploy

The simplest way is to ZIP this folder and upload via the Appwrite Console:

```sh
cd MeteoApplication/server/notification-function
zip -r function.zip src package.json
```

Then upload `function.zip` in Functions → Deployments → **Create deployment**.

Alternatively, if you connect the Function to this GitHub repo, point the
"Root directory" to `MeteoApplication/server/notification-function/` and Appwrite
will auto-deploy on push.

### 6. Test

In the Appwrite Console open the function and click **Execute now**. Check the
**Executions** tab for logs like:

```
[scan] 3 locations to check
[FCM] sent projects/.../messages/0:1234567890
[done] pushed=1 skipped=0 errors=0
```

To force a transition for testing, set `tempThresholdMax` to `5` on a location
where the current temperature is, say, `12°C`. The next run should push and the
document's `lastNotifiedState` should flip to `above`.

## Local development (optional)

```sh
npm install
# Set the env vars somewhere (e.g. a real .env loaded by dotenv) and import
# src/main.js. Note the function expects Appwrite-style ({req, res, log, error}),
# so a real local invocation needs a stub. Easier to test by running it on Appwrite.
```
