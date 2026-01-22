using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AllKeyShopExtension.Models;
using AllKeyShopExtension.Utilities;
using HtmlAgilityPack;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace AllKeyShopExtension.Services
{
    public class AllKeyShopScraper : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteAPI;
        private readonly HttpClientHelper httpClient;
        private readonly Dictionary<string, GamePrice> priceCache;
        private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(10);

        public AllKeyShopScraper(IPlayniteAPI api)
        {
            playniteAPI = api;
            httpClient = new HttpClientHelper();
            priceCache = new Dictionary<string, GamePrice>();
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }

        public async Task<GamePrice> GetGamePrice(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return null;
            }

            // Check cache first
            var cacheKey = gameName.ToLowerInvariant();
            if (priceCache.ContainsKey(cacheKey))
            {
                var cached = priceCache[cacheKey];
                if (DateTime.Now - cached.RetrievedAt < cacheExpiration)
                {
                    return cached;
                }
            }

            try
            {
                // Search for game on AllKeyShop
                var searchUrl = $"https://www.allkeyshop.com/blog/catalogue/search-{HttpUtility.UrlEncode(gameName)}.html";
                var html = await httpClient.GetStringAsync(searchUrl);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Try to find the first game result
                var gameLink = doc.DocumentNode.SelectSingleNode("//a[contains(@class, 'search-product-link')] | //a[contains(@href, '/blog/buy-')]");
                
                if (gameLink == null)
                {
                    // Try alternative selectors
                    gameLink = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'search-result')]//a[contains(@href, '/blog/buy-')]");
                }

                if (gameLink != null)
                {
                    var gameUrl = gameLink.GetAttributeValue("href", "");
                    if (!gameUrl.StartsWith("http"))
                    {
                        gameUrl = "https://www.allkeyshop.com" + gameUrl;
                    }

                    // Get price from game page
                    return await GetPriceFromGamePage(gameUrl, gameName);
                }

                // If no direct match, try to parse search results
                return ParseSearchResults(doc, gameName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error scraping price for {gameName}: {ex.Message}");
                return null;
            }
        }

        private async Task<GamePrice> GetPriceFromGamePage(string gameUrl, string gameName)
        {
            try
            {
                var html = await httpClient.GetStringAsync(gameUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Look for price information
                var priceNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'price')] | //div[contains(@class, 'price')] | //span[contains(@data-price, '')]");
                
                if (priceNode == null)
                {
                    // Try alternative selectors
                    priceNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'best-price')] | //*[contains(@class, 'lowest-price')]");
                }

                decimal price = 0;
                string seller = null;

                if (priceNode != null)
                {
                    var priceText = priceNode.InnerText;
                    price = ExtractPrice(priceText);
                }

                // Try to find seller
                var sellerNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'seller')] | //div[contains(@class, 'store')]");
                if (sellerNode != null)
                {
                    seller = sellerNode.InnerText.Trim();
                }

                var gamePrice = new GamePrice
                {
                    GameName = gameName,
                    Price = price,
                    Seller = seller,
                    Url = gameUrl,
                    IsAvailable = price > 0
                };

                // Cache result
                var cacheKey = gameName.ToLowerInvariant();
                priceCache[cacheKey] = gamePrice;

                return gamePrice;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error getting price from game page {gameUrl}: {ex.Message}");
                return null;
            }
        }

        private GamePrice ParseSearchResults(HtmlDocument doc, string gameName)
        {
            try
            {
                // Look for price in search results
                var priceNodes = doc.DocumentNode.SelectNodes("//span[contains(@class, 'price')] | //div[contains(@class, 'price')]");
                
                if (priceNodes != null && priceNodes.Count > 0)
                {
                    var firstPrice = priceNodes.First();
                    var priceText = firstPrice.InnerText;
                    var price = ExtractPrice(priceText);

                    // Try to find link
                    var linkNode = firstPrice.Ancestors("a").FirstOrDefault() 
                        ?? firstPrice.SelectSingleNode("ancestor::a");
                    
                    string url = null;
                    if (linkNode != null)
                    {
                        url = linkNode.GetAttributeValue("href", "");
                        if (!url.StartsWith("http"))
                        {
                            url = "https://www.allkeyshop.com" + url;
                        }
                    }

                    var gamePrice = new GamePrice
                    {
                        GameName = gameName,
                        Price = price,
                        Url = url,
                        IsAvailable = price > 0
                    };

                    var cacheKey = gameName.ToLowerInvariant();
                    priceCache[cacheKey] = gamePrice;

                    return gamePrice;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error parsing search results: {ex.Message}");
            }

            return null;
        }

        public async Task<List<FreeGame>> GetFreeGames(string platform)
        {
            var freeGames = new List<FreeGame>();

            try
            {
                // Map platform to AllKeyShop URL format
                var platformUrl = MapPlatformToUrl(platform);
                if (string.IsNullOrEmpty(platformUrl))
                {
                    return freeGames;
                }

                var url = $"https://www.allkeyshop.com/blog/free-games/{platformUrl}/";
                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Find all free game entries
                var gameNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'free-game')] | //article[contains(@class, 'game')] | //div[contains(@class, 'game-item')]");

                if (gameNodes == null)
                {
                    // Try alternative selectors
                    gameNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/blog/buy-')]");
                }

                if (gameNodes != null)
                {
                    foreach (var node in gameNodes)
                    {
                        try
                        {
                            var gameName = ExtractGameName(node);
                            var gameUrl = ExtractGameUrl(node);

                            if (!string.IsNullOrEmpty(gameName))
                            {
                                freeGames.Add(new FreeGame
                                {
                                    GameName = gameName,
                                    Platform = platform,
                                    Url = gameUrl,
                                    DateFound = DateTime.Now
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"Error parsing free game node: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error scraping free games for {platform}: {ex.Message}");
            }

            return freeGames;
        }

        private string MapPlatformToUrl(string platform)
        {
            var platformMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Steam", "steam" },
                { "Epic Games Store", "epic" },
                { "Epic", "epic" },
                { "GOG", "gog" },
                { "Xbox", "xbox" },
                { "PlayStation", "playstation" },
                { "PS4", "playstation" },
                { "PS5", "playstation" },
                { "Nintendo Switch", "nintendo" },
                { "Switch", "nintendo" },
                { "Origin", "origin" },
                { "EA App", "origin" },
                { "Uplay", "uplay" },
                { "Ubisoft", "uplay" }
            };

            return platformMap.ContainsKey(platform) ? platformMap[platform] : platform.ToLowerInvariant();
        }

        private string ExtractGameName(HtmlNode node)
        {
            // Try various selectors for game name
            var nameNode = node.SelectSingleNode(".//h2 | .//h3 | .//a[contains(@class, 'title')] | .//span[contains(@class, 'title')]");
            if (nameNode != null)
            {
                return nameNode.InnerText.Trim();
            }

            // Fallback to link text
            var linkNode = node.SelectSingleNode(".//a");
            if (linkNode != null)
            {
                return linkNode.InnerText.Trim();
            }

            return node.InnerText.Trim();
        }

        private string ExtractGameUrl(HtmlNode node)
        {
            var linkNode = node.SelectSingleNode(".//a[@href]");
            if (linkNode != null)
            {
                var url = linkNode.GetAttributeValue("href", "");
                if (!url.StartsWith("http"))
                {
                    url = "https://www.allkeyshop.com" + url;
                }
                return url;
            }
            return null;
        }

        private decimal ExtractPrice(string priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText))
            {
                return 0;
            }

            // Remove currency symbols and extract number
            var cleaned = Regex.Replace(priceText, @"[^\d.,]", "");
            cleaned = cleaned.Replace(",", ".");

            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                return price;
            }

            return 0;
        }

        public void ClearCache()
        {
            priceCache.Clear();
        }
    }
}
