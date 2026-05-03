using System.Globalization;
using Appwrite;
using Appwrite.Services;

namespace MeteoApp;

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

    public async Task<string?> SyncLocationAsync(MeteoLocation location, string? deviceToken)
    {
        if (string.IsNullOrEmpty(deviceToken))
            return location.AppwriteDocumentId;

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

                return doc.Id;
            }
            else
            {
                var doc = await _databases.UpdateDocument(
                    databaseId: Secret.AppwriteDatabaseId,
                    collectionId: Secret.AppwriteCollectionId,
                    documentId: location.AppwriteDocumentId,
                    data: data);

                return doc.Id;
            }
        }
        catch (Exception)
        {
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

        }
        catch (Exception) { }
    }
}
