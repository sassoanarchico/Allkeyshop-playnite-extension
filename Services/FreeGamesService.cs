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
    public class FreeGamesService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly Database database;
        private readonly AllKeyShopScraper scraper;
        private readonly IPlayniteAPI playniteAPI;

        public FreeGamesService(Database db, AllKeyShopScraper scraperService, IPlayniteAPI api)
        {
            database = db;
            scraper = scraperService;
            playniteAPI = api;
        }

        public List<FreeGame> GetFreeGamesByPlatform(string platform)
        {
            return database.GetFreeGamesByPlatform(platform);
        }

        public List<FreeGame> GetAllFreeGames()
        {
            return database.GetAllFreeGames();
        }

        public async Task<List<FreeGame>> CheckForNewFreeGames(string platform)
        {
            var newGames = new List<FreeGame>();

            try
            {
                var scrapedGames = await scraper.GetFreeGames(platform);
                
                foreach (var game in scrapedGames)
                {
                    if (!database.FreeGameExists(game.Platform, game.GameName))
                    {
                        // New free game found
                        database.AddFreeGame(game);
                        newGames.Add(game);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error checking for new free games on {platform}: {ex.Message}");
            }

            return newGames;
        }

        /// <summary>
        /// Check for daily game deals from AllKeyShop (no platform needed).
        /// </summary>
        public async Task<List<FreeGame>> CheckForDailyDeals()
        {
            var newGames = new List<FreeGame>();

            try
            {
                var scrapedGames = await scraper.GetDailyGameDeals();
                
                foreach (var game in scrapedGames)
                {
                    if (!database.FreeGameExists(game.Platform, game.GameName))
                    {
                        database.AddFreeGame(game);
                        newGames.Add(game);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error checking for daily game deals");
            }

            return newGames;
        }

        public async Task<List<FreeGame>> CheckForNewFreeGames(List<string> platforms)
        {
            var allNewGames = new List<FreeGame>();

            foreach (var platform in platforms)
            {
                var newGames = await CheckForNewFreeGames(platform);
                allNewGames.AddRange(newGames);
            }

            return allNewGames;
        }

        public List<FreeGame> GetRecentFreeGames(TimeSpan maxAge)
        {
            var allGames = database.GetAllFreeGames();
            var cutoffTime = DateTime.Now - maxAge;
            
            return allGames.Where(g => g.DateFound >= cutoffTime).ToList();
        }

        public Dictionary<string, List<FreeGame>> GetFreeGamesByPlatform(List<string> platforms)
        {
            var result = new Dictionary<string, List<FreeGame>>();
            
            foreach (var platform in platforms)
            {
                var games = database.GetFreeGamesByPlatform(platform);
                if (games.Count > 0)
                {
                    result[platform] = games;
                }
            }

            return result;
        }
    }
}
