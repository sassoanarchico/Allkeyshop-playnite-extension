using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using AllKeyShopExtension.Models;
using Newtonsoft.Json;
using Playnite.SDK;

namespace AllKeyShopExtension.Data
{
    public class Database
    {
        private readonly IPlayniteAPI playniteAPI;
        private readonly string dbPath;
        private readonly object lockObject = new object();

        public Database(IPlayniteAPI api)
        {
            playniteAPI = api;
            var extensionDir = Path.GetDirectoryName(GetType().Assembly.Location);
            dbPath = Path.Combine(extensionDir, "allkeyshop.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            lock (lockObject)
            {
                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                }

                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();

                    // Create WatchedGames table
                    var createWatchedGames = @"
                        CREATE TABLE IF NOT EXISTS WatchedGames (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            GameName TEXT NOT NULL UNIQUE,
                            LastPrice REAL,
                            LastSeller TEXT,
                            LastUrl TEXT,
                            LastUpdate TEXT,
                            PriceThreshold REAL,
                            DateAdded TEXT NOT NULL
                        )";

                    // Create FreeGamesHistory table
                    var createFreeGamesHistory = @"
                        CREATE TABLE IF NOT EXISTS FreeGamesHistory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Platform TEXT NOT NULL,
                            GameName TEXT NOT NULL,
                            Url TEXT,
                            DateAdded TEXT NOT NULL,
                            UNIQUE(Platform, GameName)
                        )";

                    // Create Settings table
                    var createSettings = @"
                        CREATE TABLE IF NOT EXISTS Settings (
                            Key TEXT PRIMARY KEY,
                            Value TEXT NOT NULL
                        )";

                    using (var command = new SQLiteCommand(createWatchedGames, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SQLiteCommand(createFreeGamesHistory, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SQLiteCommand(createSettings, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // WatchedGames methods
        public List<WatchedGame> GetAllWatchedGames()
        {
            lock (lockObject)
            {
                var games = new List<WatchedGame>();
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT * FROM WatchedGames ORDER BY DateAdded DESC";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            games.Add(ReadWatchedGame(reader));
                        }
                    }
                }
                return games;
            }
        }

        public WatchedGame GetWatchedGame(int id)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT * FROM WatchedGames WHERE Id = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return ReadWatchedGame(reader);
                            }
                        }
                    }
                }
                return null;
            }
        }

        public WatchedGame GetWatchedGameByName(string gameName)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT * FROM WatchedGames WHERE GameName = @GameName";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameName", gameName);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return ReadWatchedGame(reader);
                            }
                        }
                    }
                }
                return null;
            }
        }

        public void AddWatchedGame(WatchedGame game)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = @"
                        INSERT OR REPLACE INTO WatchedGames 
                        (GameName, LastPrice, LastSeller, LastUrl, LastUpdate, PriceThreshold, DateAdded)
                        VALUES (@GameName, @LastPrice, @LastSeller, @LastUrl, @LastUpdate, @PriceThreshold, @DateAdded)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameName", game.GameName);
                        command.Parameters.AddWithValue("@LastPrice", (object)game.LastPrice ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LastSeller", (object)game.LastSeller ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LastUrl", (object)game.LastUrl ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LastUpdate", game.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@PriceThreshold", (object)game.PriceThreshold ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DateAdded", game.DateAdded.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateWatchedGame(WatchedGame game)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = @"
                        UPDATE WatchedGames 
                        SET LastPrice = @LastPrice, LastSeller = @LastSeller, LastUrl = @LastUrl, 
                            LastUpdate = @LastUpdate, PriceThreshold = @PriceThreshold
                        WHERE Id = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", game.Id);
                        command.Parameters.AddWithValue("@LastPrice", (object)game.LastPrice ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LastSeller", (object)game.LastSeller ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LastUrl", (object)game.LastUrl ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LastUpdate", game.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@PriceThreshold", (object)game.PriceThreshold ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void DeleteWatchedGame(int id)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "DELETE FROM WatchedGames WHERE Id = @Id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // FreeGamesHistory methods
        public List<FreeGame> GetFreeGamesByPlatform(string platform)
        {
            lock (lockObject)
            {
                var games = new List<FreeGame>();
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT * FROM FreeGamesHistory WHERE Platform = @Platform ORDER BY DateAdded DESC";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Platform", platform);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                games.Add(ReadFreeGame(reader));
                            }
                        }
                    }
                }
                return games;
            }
        }

        public List<FreeGame> GetAllFreeGames()
        {
            lock (lockObject)
            {
                var games = new List<FreeGame>();
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT * FROM FreeGamesHistory ORDER BY DateAdded DESC";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            games.Add(ReadFreeGame(reader));
                        }
                    }
                }
                return games;
            }
        }

        public void AddFreeGame(FreeGame game)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = @"
                        INSERT OR IGNORE INTO FreeGamesHistory 
                        (Platform, GameName, Url, DateAdded)
                        VALUES (@Platform, @GameName, @Url, @DateAdded)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Platform", game.Platform);
                        command.Parameters.AddWithValue("@GameName", game.GameName);
                        command.Parameters.AddWithValue("@Url", (object)game.Url ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DateAdded", game.DateFound.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public bool FreeGameExists(string platform, string gameName)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT COUNT(*) FROM FreeGamesHistory WHERE Platform = @Platform AND GameName = @GameName";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Platform", platform);
                        command.Parameters.AddWithValue("@GameName", gameName);
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
        }

        // Settings methods
        public ExtensionSettings GetSettings()
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "SELECT Value FROM Settings WHERE Key = 'ExtensionSettings'";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            return JsonConvert.DeserializeObject<ExtensionSettings>(result.ToString());
                        }
                    }
                }
                return new ExtensionSettings();
            }
        }

        public void SaveSettings(ExtensionSettings settings)
        {
            lock (lockObject)
            {
                using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    connection.Open();
                    var query = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES ('ExtensionSettings', @Value)";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var json = JsonConvert.SerializeObject(settings);
                        command.Parameters.AddWithValue("@Value", json);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // Helper methods
        private WatchedGame ReadWatchedGame(SQLiteDataReader reader)
        {
            return new WatchedGame
            {
                Id = Convert.ToInt32(reader["Id"]),
                GameName = reader["GameName"].ToString(),
                LastPrice = reader["LastPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["LastPrice"]),
                LastSeller = reader["LastSeller"] == DBNull.Value ? null : reader["LastSeller"].ToString(),
                LastUrl = reader["LastUrl"] == DBNull.Value ? null : reader["LastUrl"].ToString(),
                LastUpdate = reader["LastUpdate"] == DBNull.Value ? DateTime.MinValue : DateTime.Parse(reader["LastUpdate"].ToString()),
                PriceThreshold = reader["PriceThreshold"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["PriceThreshold"]),
                DateAdded = DateTime.Parse(reader["DateAdded"].ToString())
            };
        }

        private FreeGame ReadFreeGame(SQLiteDataReader reader)
        {
            return new FreeGame
            {
                GameName = reader["GameName"].ToString(),
                Platform = reader["Platform"].ToString(),
                Url = reader["Url"] == DBNull.Value ? null : reader["Url"].ToString(),
                DateFound = DateTime.Parse(reader["DateAdded"].ToString())
            };
        }
    }
}
