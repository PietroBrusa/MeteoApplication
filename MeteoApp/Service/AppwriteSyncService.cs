using System.Globalization;
using Appwrite;
using Appwrite.Services;

namespace MeteoApp;

/// <summary>
/// Mirrors locations + thresholds + the FCM device token to Appwrite.
/// All operations are best-effort: failures are logged and swallowed so local-only
/// flows keep working offline.
/// </summary>
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

    /// <summary>
    /// Creates or updates the cloud document for a location.
    /// </summary>
    /// <returns>The Appwrite document id (existing or newly generated), or the input id on failure.</returns>
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
            return location.AppwriteDocumentId;
        }
    }

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
