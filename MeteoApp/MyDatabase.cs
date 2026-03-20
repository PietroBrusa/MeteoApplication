using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;

namespace MeteoApp
{
    public static class Constants
    {
        public const string DatabaseFilename = "meteo_locations.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

    public class MeteoDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public MeteoDatabase()
        {
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            _database.CreateTableAsync<MeteoLocation>().Wait();
        }

        public async Task<List<MeteoLocation>> GetLocationsAsync()
        {
            return await _database.Table<MeteoLocation>().ToListAsync();
        }

        public async Task<int> SaveLocationAsync(MeteoLocation location)
        {
            return await _database.InsertAsync(location);
        }

        public async Task<int> DeleteLocationAsync(int id)
        {
            return await _database.DeleteAsync<MeteoLocation>(id);
        }
    }
}