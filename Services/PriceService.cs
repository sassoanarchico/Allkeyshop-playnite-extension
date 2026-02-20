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

        public AllKeyShopScraper Scraper => scraper;

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

        /// <summary>
        /// Add a watched game with a specific AllKeyShop page URL (from search results).
        /// </summary>
        public bool AddWatchedGame(string gameName, string allKeyShopPageUrl, decimal? priceThreshold = null)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                return false;

            var existing = database.GetWatchedGameByName(gameName);
            if (existing != null)
                return false;

            var watchedGame = new WatchedGame
            {
                GameName = gameName,
                AllKeyShopPageUrl = allKeyShopPageUrl,
                PriceThreshold = priceThreshold,
                DateAdded = DateTime.Now
            };

            database.AddWatchedGame(watchedGame);
            return true;
        }

        /// <summary>
        /// Add a watched game by name only (legacy/backward compat).
        /// </summary>
        public bool AddWatchedGame(string gameName, decimal? priceThreshold = null)
        {
            return AddWatchedGame(gameName, null, priceThreshold);
        }

        public void RemoveWatchedGame(int id)
        {
            database.DeleteWatchedGame(id);
        }

        public void UpdateThreshold(int gameId, decimal? newThreshold)
        {
            var game = database.GetWatchedGame(gameId);
            if (game != null)
            {
                game.PriceThreshold = newThreshold;
                database.UpdateWatchedGame(game);
                logger.Info($"Updated threshold for '{game.GameName}' to {newThreshold}");
            }
        }

        public async Task<bool> UpdateGamePrice(int watchedGameId)
        {
            var watchedGame = database.GetWatchedGame(watchedGameId);
            if (watchedGame == null)
                return false;

            return await UpdateGamePrice(watchedGame);
        }

        public async Task<bool> UpdateGamePrice(WatchedGame watchedGame)
        {
            try
            {
                GamePrice gamePrice;

                // If we have a specific AllKeyShop page URL, use it directly
                if (!string.IsNullOrEmpty(watchedGame.AllKeyShopPageUrl))
                {
                    gamePrice = await scraper.GetGamePriceFromPage(watchedGame.AllKeyShopPageUrl, watchedGame.GameName);
                }
                else
                {
                    // Fallback: search by name (legacy games without URL)
                    gamePrice = await scraper.GetGamePrice(watchedGame.GameName);
                }

                if (gamePrice != null)
                {
                    // Update AllKeyShopPageUrl if we didn't have one
                    if (string.IsNullOrEmpty(watchedGame.AllKeyShopPageUrl) && !string.IsNullOrEmpty(gamePrice.AllKeyShopPageUrl))
                    {
                        watchedGame.AllKeyShopPageUrl = gamePrice.AllKeyShopPageUrl;
                    }

                    // Update URL for buy link
                    if (!string.IsNullOrEmpty(gamePrice.Url))
                    {
                        watchedGame.LastUrl = gamePrice.Url;
                    }

                    if (gamePrice.IsAvailable && gamePrice.Price > 0)
                    {
                        watchedGame.LastPrice = gamePrice.Price;
                        watchedGame.LastSeller = gamePrice.Seller ?? "N/A";
                    }

                    // Update key price
                    if (gamePrice.KeyPrice.HasValue && gamePrice.KeyPrice > 0)
                    {
                        watchedGame.KeyPrice = gamePrice.KeyPrice;
                        watchedGame.KeySeller = gamePrice.KeySeller;
                    }

                    // Update account price
                    if (gamePrice.AccountPrice.HasValue && gamePrice.AccountPrice > 0)
                    {
                        watchedGame.AccountPrice = gamePrice.AccountPrice;
                        watchedGame.AccountSeller = gamePrice.AccountSeller;
                    }

                    watchedGame.LastUpdate = DateTime.Now;
                    database.UpdateWatchedGame(watchedGame);

                    logger.Info($"Updated {watchedGame.GameName}: Key={watchedGame.KeyPrice} ({watchedGame.KeySeller}), Account={watchedGame.AccountPrice} ({watchedGame.AccountSeller})");
                    return true;
                }
                else
                {
                    logger.Warn($"Could not retrieve price data for {watchedGame.GameName}");
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
            // Update sequentially to respect rate limiting
            foreach (var game in watchedGames)
            {
                await UpdateGamePrice(game);
            }
        }

        public List<WatchedGame> GetGamesNeedingUpdate(TimeSpan maxAge)
        {
            var allGames = database.GetAllWatchedGames();
            var cutoffTime = DateTime.Now - maxAge;
            
            return allGames.Where(g => g.LastUpdate < cutoffTime).ToList();
        }

        public bool CheckPriceAlert(WatchedGame game)
        {
            if (game.PriceThreshold.HasValue)
            {
                // Check against best price (key or account)
                decimal? bestPrice = null;
                if (game.KeyPrice.HasValue && game.AccountPrice.HasValue)
                    bestPrice = Math.Min(game.KeyPrice.Value, game.AccountPrice.Value);
                else if (game.KeyPrice.HasValue)
                    bestPrice = game.KeyPrice;
                else if (game.AccountPrice.HasValue)
                    bestPrice = game.AccountPrice;
                else if (game.LastPrice.HasValue)
                    bestPrice = game.LastPrice;

                if (bestPrice.HasValue)
                    return bestPrice.Value <= game.PriceThreshold.Value;
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
