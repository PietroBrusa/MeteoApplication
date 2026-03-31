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
        private bool _initialized = false;

        public MeteoDatabase()
        {
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

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
            await Init(); // Assicurati che sia inizializzato
            return await _database.Table<MeteoLocation>().ToListAsync();
        }

        public async Task<int> SaveLocationAsync(MeteoLocation location)
        {
            await Init(); // Assicurati che sia inizializzato
            return await _database.InsertAsync(location);
        }

        public async Task<int> DeleteLocationAsync(int id)
        {
            await Init(); // Assicurati che sia inizializzato
            return await _database.DeleteAsync<MeteoLocation>(id);
        }
    }
}