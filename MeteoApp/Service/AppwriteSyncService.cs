using System.Globalization;
using Appwrite;
using Appwrite.Services;

namespace MeteoApp;

// Mirrors locations + thresholds + the FCM device token to Appwrite (slide 8.1).
// The backend (next phase) reads this collection to decide who to push.
//
// Sync is best-effort: any network/Appwrite failure is logged and swallowed so
// that local-only flows (DB save, list update) continue to work offline.
public class AppwriteSyncService
{
    private readonly Client _client;
    private readonly Databases _databases;

    public AppwriteSyncService()
    {
        _client = new Client()
            .SetEndpoint(Secret.AppwriteEndpoint)
            .SetProject(Secret.AppwriteProjectId)
            .SetKey(Secret.AppwriteApiKey);

        _databases = new Databases(_client);
    }

    // Creates or updates the cloud document for a location.
    // Returns the Appwrite document id (existing one or newly generated).
    public async Task<string?> SyncLocationAsync(MeteoLocation location, string? deviceToken)
    {
        if (string.IsNullOrEmpty(deviceToken))
        {
            System.Diagnostics.Debug.WriteLine("[Appwrite] No device token — skipping sync");
            return location.AppwriteDocumentId;
        }

        var data = new Dictionary<string, object>
        {
            { "deviceToken", deviceToken },
            { "name", location.Name },
            { "lat", location.Latitude },
            { "lon", location.Longitude },
            { "notificationsEnabled", location.NotificationsEnabled },
            { "tempThresholdMin", location.TempThresholdMin },
            { "tempThresholdMax", location.TempThresholdMax },
            { "language", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName },
        };

        try
        {
            if (string.IsNullOrEmpty(location.AppwriteDocumentId))
            {
                // First sync — create a new document, server assigns the id
                var doc = await _databases.CreateDocument(
                    databaseId: Secret.AppwriteDatabaseId,
                    collectionId: Secret.AppwriteCollectionId,
                    documentId: ID.Unique(),
                    data: data);

                System.Diagnostics.Debug.WriteLine($"[Appwrite] Created document {doc.Id} for {location.Name}");
                return doc.Id;
            }
            else
            {
                // Subsequent sync — update existing document in place
                var doc = await _databases.UpdateDocument(
                    databaseId: Secret.AppwriteDatabaseId,
                    collectionId: Secret.AppwriteCollectionId,
                    documentId: location.AppwriteDocumentId,
                    data: data);

                System.Diagnostics.Debug.WriteLine($"[Appwrite] Updated document {doc.Id} for {location.Name}");
                return doc.Id;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Appwrite] Sync error for {location.Name}: {ex.Message}");
            return location.AppwriteDocumentId;  // unchanged on failure
        }
    }

    // Removes the cloud document. Safe to call with empty id (no-op).
    public async Task DeleteLocationAsync(string appwriteDocumentId)
    {
        if (string.IsNullOrEmpty(appwriteDocumentId)) return;

        try
        {
            await _databases.DeleteDocument(
                databaseId: Secret.AppwriteDatabaseId,
                collectionId: Secret.AppwriteCollectionId,
                documentId: appwriteDocumentId);

            System.Diagnostics.Debug.WriteLine($"[Appwrite] Deleted document {appwriteDocumentId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Appwrite] Delete error {appwriteDocumentId}: {ex.Message}");
        }
    }
}
