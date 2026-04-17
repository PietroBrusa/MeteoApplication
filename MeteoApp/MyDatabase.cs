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

        // SharedCache allows multiple connections to read the same DB file
        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

    public class MeteoDatabase
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _initialized = false;

        public MeteoDatabase()
        {
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        // Lazy initializer — creates the table only on first use
        private async Task Init()
        {
            if (!_initialized)
            {
                await _database.CreateTableAsync<MeteoLocation>();
                _initialized = true;
            }
        }

        public async Task<List<MeteoLocation>> GetLocationsAsync()
        {
            await Init();
            return await _database.Table<MeteoLocation>().ToListAsync();
        }

        public async Task<int> SaveLocationAsync(MeteoLocation location)
        {
            await Init();
            return await _database.InsertAsync(location);
        }

        public async Task<int> DeleteLocationAsync(int id)
        {
            await Init();
            return await _database.DeleteAsync<MeteoLocation>(id);
        }
    }
}
