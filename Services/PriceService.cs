using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllKeyShopExtension.Data;
using AllKeyShopExtension.Models;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace AllKeyShopExtension.Services
{
    public class PriceService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly Database database;
        private readonly AllKeyShopScraper scraper;
        private readonly IPlayniteAPI playniteAPI;

        public PriceService(Database db, AllKeyShopScraper scraperService, IPlayniteAPI api)
        {
            database = db;
            scraper = scraperService;
            playniteAPI = api;
        }

        public List<WatchedGame> GetAllWatchedGames()
        {
            return database.GetAllWatchedGames();
        }

        public WatchedGame GetWatchedGame(int id)
        {
            return database.GetWatchedGame(id);
        }

        public WatchedGame GetWatchedGameByName(string gameName)
        {
            return database.GetWatchedGameByName(gameName);
        }

        public bool AddWatchedGame(string gameName, decimal? priceThreshold = null)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return false;
            }

            // Check if game already exists
            var existing = database.GetWatchedGameByName(gameName);
            if (existing != null)
            {
                return false; // Game already watched
            }

            var watchedGame = new WatchedGame
            {
                GameName = gameName,
                PriceThreshold = priceThreshold,
                DateAdded = DateTime.Now
            };

            database.AddWatchedGame(watchedGame);
            return true;
        }

        public void RemoveWatchedGame(int id)
        {
            database.DeleteWatchedGame(id);
        }

        public async Task<bool> UpdateGamePrice(int watchedGameId)
        {
            var watchedGame = database.GetWatchedGame(watchedGameId);
            if (watchedGame == null)
            {
                return false;
            }

            return await UpdateGamePrice(watchedGame);
        }

        public async Task<bool> UpdateGamePrice(WatchedGame watchedGame)
        {
            try
            {
                var gamePrice = await scraper.GetGamePrice(watchedGame.GameName);
                
                if (gamePrice != null && gamePrice.IsAvailable && gamePrice.Price > 0)
                {
                    watchedGame.LastPrice = gamePrice.Price;
                    watchedGame.LastSeller = gamePrice.Seller;
                    watchedGame.LastUrl = gamePrice.Url;
                    watchedGame.LastUpdate = DateTime.Now;

                    database.UpdateWatchedGame(watchedGame);
                    return true;
                }
                else
                {
                    logger.Warn($"Could not retrieve price for {watchedGame.GameName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error updating price for {watchedGame.GameName}: {ex.Message}");
                return false;
            }
        }

        public async Task UpdateAllPrices()
        {
            var watchedGames = database.GetAllWatchedGames();
            var updateTasks = new List<Task>();

            foreach (var game in watchedGames)
            {
                updateTasks.Add(UpdateGamePrice(game));
            }

            await Task.WhenAll(updateTasks);
        }

        public List<WatchedGame> GetGamesNeedingUpdate(TimeSpan maxAge)
        {
            var allGames = database.GetAllWatchedGames();
            var cutoffTime = DateTime.Now - maxAge;
            
            return allGames.Where(g => g.LastUpdate < cutoffTime).ToList();
        }

        public bool CheckPriceAlert(WatchedGame game)
        {
            if (game.PriceThreshold.HasValue && game.LastPrice.HasValue)
            {
                return game.LastPrice.Value <= game.PriceThreshold.Value;
            }
            return false;
        }

        public List<WatchedGame> GetGamesWithPriceAlerts()
        {
            var allGames = database.GetAllWatchedGames();
            return allGames.Where(CheckPriceAlert).ToList();
        }
    }
}
