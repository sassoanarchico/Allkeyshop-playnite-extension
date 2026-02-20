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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Playnite.SDK;

namespace AllKeyShopExtension.Services
{
    public class AllKeyShopScraper : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteAPI;
        private readonly HttpClientHelper httpClient;
        private readonly Dictionary<string, GamePrice> priceCache;
        private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(10);

        private const string SEARCH_API_URL = "https://www.allkeyshop.com/blog/wp-admin/admin-ajax.php";
        private const string BASE_URL = "https://www.allkeyshop.com";
        private const string OFFER_REDIRECT_URL = "https://www.allkeyshop.com/redirection/offer/eur/{0}?locale=en&merchant={1}";

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

        /// <summary>
        /// Search for games on AllKeyShop using the quicksearch AJAX API.
        /// Returns a list of search results for the user to choose from.
        /// </summary>
        public async Task<List<SearchResult>> SearchGamesAsync(string query)
        {
            var results = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(query))
                return results;

            try
            {
                var encodedQuery = HttpUtility.UrlEncode(query);
                var searchUrl = $"{SEARCH_API_URL}?action=quicksearch&search_name={encodedQuery}&currency=eur&locale=en&platform=all";

                logger.Info($"Searching AllKeyShop: {query}");
                var json = await httpClient.GetStringAsync(searchUrl);

                var response = JObject.Parse(json);
                var resultsHtml = response["results"]?.ToString();

                if (string.IsNullOrEmpty(resultsHtml))
                {
                    logger.Warn($"No search results HTML for: {query}");
                    return results;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml($"<html><body>{resultsHtml}</body></html>");

                var rows = doc.DocumentNode.SelectNodes("//li[contains(@class, 'ls-results-row') and not(contains(@class, 'ls-last-row'))]");
                if (rows == null)
                {
                    logger.Warn($"No result rows found for: {query}");
                    return results;
                }

                foreach (var row in rows)
                {
                    try
                    {
                        var link = row.SelectSingleNode(".//a[contains(@class, 'ls-results-row-link')]");
                        if (link == null) continue;

                        var href = link.GetAttributeValue("href", "");
                        if (string.IsNullOrEmpty(href) || !href.Contains("/blog/buy-")) continue;

                        var titleNode = row.SelectSingleNode(".//h2[contains(@class, 'ls-results-row-game-title')]");
                        var infoNode = row.SelectSingleNode(".//div[contains(@class, 'ls-results-row-game-infos')]");
                        var priceNode = row.SelectSingleNode(".//div[contains(@class, 'ls-results-row-price')]");

                        var title = titleNode?.InnerText?.Trim() ?? "";
                        var info = infoNode?.InnerText?.Trim() ?? "";
                        var platform = row.GetAttributeValue("data-platforms", "");

                        // Parse platform and year from info like "PC - 2022"
                        var infoParts = info.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        var platformDisplay = infoParts.Length > 0 ? infoParts[0].Trim() : platform;
                        var year = infoParts.Length > 1 ? infoParts[1].Trim() : "";

                        // Parse price
                        var stock = priceNode?.GetAttributeValue("data-stock", "") ?? "";
                        var priceStr = priceNode?.GetAttributeValue("data-price", "0") ?? "0";
                        decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price);

                        // Parse image URL
                        var imgDiv = row.SelectSingleNode(".//div[contains(@class, 'ls-results-row-image-ratio')]");
                        var style = imgDiv?.GetAttributeValue("style", "") ?? "";
                        var imageUrl = "";
                        var imgMatch = Regex.Match(style, @"url\('([^']+)'\)");
                        if (imgMatch.Success)
                            imageUrl = imgMatch.Groups[1].Value;

                        results.Add(new SearchResult
                        {
                            Title = HtmlEntity.DeEntitize(title),
                            Url = href,
                            Platform = platformDisplay,
                            Year = year,
                            Price = price,
                            ImageUrl = imageUrl,
                            InStock = stock == "in_stock"
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Debug($"Error parsing search result row: {ex.Message}");
                    }
                }

                logger.Info($"Found {results.Count} search results for: {query}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error searching AllKeyShop for: {query}");
            }

            return results;
        }

        /// <summary>
        /// Get detailed price information from a game's AllKeyShop page.
        /// Parses the gamePageTrans JSON embedded in the HTML to get all offers.
        /// </summary>
        public async Task<GamePrice> GetGamePriceFromPage(string pageUrl, string gameName)
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
                return null;

            // Check cache
            var cacheKey = pageUrl.ToLowerInvariant();
            if (priceCache.ContainsKey(cacheKey))
            {
                var cached = priceCache[cacheKey];
                if (DateTime.Now - cached.RetrievedAt < cacheExpiration)
                    return cached;
            }

            try
            {
                logger.Info($"Fetching game page: {pageUrl}");
                var html = await httpClient.GetStringAsync(pageUrl);

                // Extract gamePageTrans JSON from inline script
                var gamePageTransJson = ExtractGamePageTrans(html);
                if (gamePageTransJson == null)
                {
                    logger.Warn($"Could not find gamePageTrans in page: {pageUrl}");
                    return null;
                }

                var gamePageTrans = JObject.Parse(gamePageTransJson);

                // Parse prices array
                var pricesArray = gamePageTrans["prices"] as JArray;
                if (pricesArray == null || pricesArray.Count == 0)
                {
                    logger.Warn($"No prices found in gamePageTrans for: {pageUrl}");
                    return new GamePrice
                    {
                        GameName = gameName,
                        AllKeyShopPageUrl = pageUrl,
                        IsAvailable = false,
                        RetrievedAt = DateTime.Now
                    };
                }

                var allOffers = new List<OfferInfo>();
                var editions = gamePageTrans["editions"] as JObject;
                var regions = gamePageTrans["regions"] as JObject;

                foreach (var priceItem in pricesArray)
                {
                    try
                    {
                        var offer = new OfferInfo
                        {
                            OfferId = priceItem["id"]?.Value<int>() ?? 0,
                            Price = priceItem["price"]?.Value<decimal>() ?? 0,
                            OriginalPrice = priceItem["originalPrice"]?.Value<decimal>() ?? 0,
                            MerchantName = priceItem["merchantName"]?.ToString() ?? "",
                            MerchantId = priceItem["merchant"]?.Value<int>() ?? 0,
                            IsOfficial = priceItem["isOfficial"]?.Value<bool>() ?? false,
                            IsAccount = priceItem["account"]?.Value<bool>() ?? false,
                            VoucherCode = priceItem["voucher_code"]?.ToString(),
                            PriceWithPaypal = priceItem["pricePaypal"]?.Value<decimal>(),
                            PriceWithCard = priceItem["priceCard"]?.Value<decimal>(),
                        };

                        // Resolve edition name
                        var editionId = priceItem["edition"]?.ToString();
                        if (editions != null && !string.IsNullOrEmpty(editionId) && editions[editionId] != null)
                        {
                            offer.Edition = editions[editionId]?["name"]?.ToString() ?? "Standard";
                        }
                        else
                        {
                            offer.Edition = "Standard";
                        }

                        // Resolve region name
                        var regionId = priceItem["region"]?.ToString();
                        if (regions != null && !string.IsNullOrEmpty(regionId) && regions[regionId] != null)
                        {
                            offer.Region = regions[regionId]?["region_name"]?.ToString() ?? regionId;
                        }
                        else
                        {
                            offer.Region = regionId ?? "";
                        }

                        // Build buy URL
                        offer.BuyUrl = string.Format(OFFER_REDIRECT_URL, offer.OfferId, offer.MerchantId);

                        allOffers.Add(offer);
                    }
                    catch (Exception ex)
                    {
                        logger.Debug($"Error parsing offer: {ex.Message}");
                    }
                }

                // Find best key offer (account=false, Standard edition preferred)
                // Sort by LowestFeePrice (includes card/paypal fees)
                var keyOffers = allOffers
                    .Where(o => !o.IsAccount && o.Price > 0)
                    .OrderBy(o => o.LowestFeePrice)
                    .ToList();

                var standardKeyOffers = keyOffers.Where(o => o.Edition == "Standard").ToList();
                var bestKey = standardKeyOffers.FirstOrDefault() ?? keyOffers.FirstOrDefault();

                // Find best account offer (account=true, Standard edition preferred)
                var accountOffers = allOffers
                    .Where(o => o.IsAccount && o.Price > 0)
                    .OrderBy(o => o.LowestFeePrice)
                    .ToList();

                var standardAccountOffers = accountOffers.Where(o => o.Edition == "Standard").ToList();
                var bestAccount = standardAccountOffers.FirstOrDefault() ?? accountOffers.FirstOrDefault();

                // Best overall price (keys preferred)
                var bestOverall = bestKey ?? bestAccount;

                var gamePrice = new GamePrice
                {
                    GameName = gameName,
                    AllKeyShopPageUrl = pageUrl,
                    IsAvailable = bestOverall != null,
                    RetrievedAt = DateTime.Now,
                    AllOffers = allOffers,
                };

                if (bestOverall != null)
                {
                    gamePrice.Price = bestOverall.LowestFeePrice;
                    gamePrice.Seller = bestOverall.MerchantName;
                    gamePrice.Url = bestOverall.BuyUrl;
                }

                if (bestKey != null)
                {
                    gamePrice.KeyPrice = bestKey.LowestFeePrice;
                    gamePrice.KeySeller = bestKey.MerchantName;
                    gamePrice.KeyOfferUrl = bestKey.BuyUrl;
                    gamePrice.KeyIsOfficial = bestKey.IsOfficial;
                }

                if (bestAccount != null)
                {
                    gamePrice.AccountPrice = bestAccount.LowestFeePrice;
                    gamePrice.AccountSeller = bestAccount.MerchantName;
                    gamePrice.AccountOfferUrl = bestAccount.BuyUrl;
                }

                // Cache result
                priceCache[cacheKey] = gamePrice;

                logger.Info($"Scraped prices for {gameName}: Key={gamePrice.KeyPrice} ({gamePrice.KeySeller}), Account={gamePrice.AccountPrice} ({gamePrice.AccountSeller}), Total offers={allOffers.Count}");

                return gamePrice;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error getting price from page {pageUrl}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Legacy method: Search + auto-pick first result. Used for backward compatibility.
        /// </summary>
        public async Task<GamePrice> GetGamePrice(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                return null;

            // Check cache first
            var cacheKey = gameName.ToLowerInvariant();
            if (priceCache.ContainsKey(cacheKey))
            {
                var cached = priceCache[cacheKey];
                if (DateTime.Now - cached.RetrievedAt < cacheExpiration)
                    return cached;
            }

            try
            {
                // Search for the game
                var searchResults = await SearchGamesAsync(gameName);

                if (searchResults.Count == 0)
                {
                    logger.Warn($"No search results found for: {gameName}");
                    return null;
                }

                // Try to find best match - prefer PC platform and exact/closest match
                var pcResults = searchResults.Where(r =>
                    r.Platform.IndexOf("pc", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.Platform.IndexOf("PC", StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                var bestMatch = pcResults.FirstOrDefault() ?? searchResults.First();

                logger.Info($"Auto-selected search result: {bestMatch.Title} ({bestMatch.Url})");

                // Get detailed prices from the game page
                var gamePrice = await GetGamePriceFromPage(bestMatch.Url, gameName);

                if (gamePrice != null)
                {
                    // Cache under game name too
                    priceCache[cacheKey] = gamePrice;
                }

                return gamePrice;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error scraping price for {gameName}");
                return null;
            }
        }

        /// <summary>
        /// Extract the gamePageTrans JSON string from the HTML page.
        /// It's embedded in a script tag like: var gamePageTrans = {...};
        /// </summary>
        private string ExtractGamePageTrans(string html)
        {
            try
            {
                // Pattern: var gamePageTrans = {JSON};
                // The JSON is on a single line in a <script> tag
                var match = Regex.Match(html, @"var\s+gamePageTrans\s*=\s*(\{.+?\});\s*$",
                    RegexOptions.Multiline);

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                // Alternative: look for it in a script block
                match = Regex.Match(html, @"var\s+gamePageTrans\s*=\s*(\{.+?\});\s*//",
                    RegexOptions.Singleline);

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                // Try finding the specific script tag
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var scripts = doc.DocumentNode.SelectNodes("//script[@id='aks-offers-js-extra']");
                if (scripts != null)
                {
                    foreach (var script in scripts)
                    {
                        var content = script.InnerText;
                        match = Regex.Match(content, @"var\s+gamePageTrans\s*=\s*(\{.+\});\s*$",
                            RegexOptions.Multiline);
                        if (match.Success)
                            return match.Groups[1].Value;
                    }
                }

                logger.Warn("Could not extract gamePageTrans from HTML");
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error extracting gamePageTrans");
                return null;
            }
        }

        /// <summary>
        /// Scrape free game deals from the AllKeyShop widget.
        /// Parses splide__slide entries for free games across all platforms.
        /// </summary>
        public async Task<List<FreeGame>> GetDailyGameDeals()
        {
            var freeGames = new List<FreeGame>();

            try
            {
                var url = "https://widget.allkeyshop.com/lib/generate/widget?widgetType=deals&locale=en_GB&currency=eur&typeList=free&console=all&backgroundColor=transparent&priceBackgroundColor=147ac3&borderWidth=0&borderColor=000000&apiKey=aks";
                logger.Info("Fetching free games from AllKeyShop widget...");
                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Parse splide__slide entries - each is a free game
                var slides = doc.DocumentNode.SelectNodes("//div[contains(@class, 'splide__slide')]");
                if (slides == null || slides.Count == 0)
                {
                    logger.Warn("No game slides found in AllKeyShop widget");
                    return freeGames;
                }

                foreach (var slide in slides)
                {
                    try
                    {
                        // Get platform/drm from data attributes
                        var console = slide.GetAttributeValue("data-console", "");
                        var drm = slide.GetAttributeValue("data-drm", "");

                        // Get the link URL
                        var link = slide.SelectSingleNode(".//a[contains(@class, 'splide__slide__container')]");
                        var gameUrl = link?.GetAttributeValue("href", "") ?? "";

                        // Get the game title from the cover image alt attribute
                        var coverImg = slide.SelectSingleNode(".//img[contains(@class, 'game-cover')]");
                        var title = HtmlEntity.DeEntitize(coverImg?.GetAttributeValue("alt", "") ?? "");

                        // Get the free game type (Free to keep, Free DLC, Free with Prime, Gamepass, etc.)
                        var typeSpan = slide.SelectSingleNode(".//span[contains(@class, 'free-game-type')]");
                        var freeType = typeSpan?.InnerText?.Trim() ?? "Free";

                        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(gameUrl))
                            continue;

                        // Build a platform label: combine DRM and console info
                        var platformLabel = !string.IsNullOrEmpty(drm) && drm != "none"
                            ? $"{char.ToUpper(drm[0])}{drm.Substring(1)}"
                            : !string.IsNullOrEmpty(console)
                                ? $"{char.ToUpper(console[0])}{console.Substring(1)}"
                                : "PC";

                        freeGames.Add(new FreeGame
                        {
                            GameName = title,
                            Platform = $"{platformLabel} - {freeType}",
                            Url = gameUrl,
                            DateFound = DateTime.Now
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Debug($"Error parsing game slide: {ex.Message}");
                    }
                }

                logger.Info($"Found {freeGames.Count} free games from widget");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error scraping AllKeyShop free games widget");
            }

            return freeGames;
        }

        public async Task<List<FreeGame>> GetFreeGames(string platform)
        {
            // Legacy method - now just delegates to GetDailyGameDeals
            return await GetDailyGameDeals();
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

        public void ClearCache()
        {
            priceCache.Clear();
        }
    }
}
