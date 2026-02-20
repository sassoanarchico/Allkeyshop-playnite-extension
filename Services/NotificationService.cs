using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AllKeyShopExtension.Models;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace AllKeyShopExtension.Services
{
    public class NotificationService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteAPI;
        private readonly ExtensionSettings settings;

        public NotificationService(IPlayniteAPI api, ExtensionSettings extensionSettings)
        {
            playniteAPI = api;
            settings = extensionSettings;
        }

        public void NotifyNewFreeGames(List<FreeGame> newGames)
        {
            if (!settings.NotificationsEnabled || newGames == null || newGames.Count == 0)
            {
                return;
            }

            try
            {
                // Group by platform
                var gamesByPlatform = newGames.GroupBy(g => g.Platform);

                foreach (var platformGroup in gamesByPlatform)
                {
                    var platform = platformGroup.Key;
                    var games = platformGroup.ToList();

                    var message = BuildFreeGamesMessage(games);
                    var text = $"🎮 Nuovi giochi gratis su {platform}!\n{message}";

                    logger.Info($"Sending free games notification: {text}");

                    playniteAPI.Notifications.Add(new NotificationMessage(
                        $"allkeyshop-free-{platform}",
                        text,
                        NotificationType.Info,
                        () =>
                        {
                            if (games.Count > 0 && !string.IsNullOrEmpty(games[0].Url))
                            {
                                System.Diagnostics.Process.Start(games[0].Url);
                            }
                        }
                    ));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error showing free games notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a Playnite notification when a game's price drops below the configured threshold.
        /// Uses a stable notification ID per game to avoid duplicate alerts.
        /// </summary>
        public void NotifyPriceAlert(WatchedGame game)
        {
            if (!settings.NotificationsEnabled || !settings.PriceAlertsEnabled)
            {
                logger.Debug($"Price alert skipped for {game.GameName}: Notifications={settings.NotificationsEnabled}, PriceAlerts={settings.PriceAlertsEnabled}");
                return;
            }

            try
            {
                // Determine best current price
                decimal? bestPrice = null;
                string bestSeller = null;
                if (game.KeyPrice.HasValue && game.AccountPrice.HasValue)
                {
                    if (game.KeyPrice.Value <= game.AccountPrice.Value)
                    { bestPrice = game.KeyPrice; bestSeller = game.KeySeller; }
                    else
                    { bestPrice = game.AccountPrice; bestSeller = game.AccountSeller; }
                }
                else if (game.KeyPrice.HasValue)
                { bestPrice = game.KeyPrice; bestSeller = game.KeySeller; }
                else if (game.AccountPrice.HasValue)
                { bestPrice = game.AccountPrice; bestSeller = game.AccountSeller; }
                else if (game.LastPrice.HasValue)
                { bestPrice = game.LastPrice; bestSeller = game.LastSeller; }

                if (!bestPrice.HasValue) return;

                var text = $"💰 {game.GameName} - Prezzo sceso a {bestPrice.Value:0.00}€" +
                           (bestSeller != null ? $" ({bestSeller})" : "") +
                           $" | Soglia: {game.PriceThreshold.Value:0.00}€";

                // Use stable ID per game to avoid duplicate notifications
                var notificationId = $"allkeyshop-price-alert-{game.Id}";

                logger.Info($"Sending price alert: {text}");

                var url = !string.IsNullOrEmpty(game.LastUrl) ? game.LastUrl
                        : !string.IsNullOrEmpty(game.AllKeyShopPageUrl) ? game.AllKeyShopPageUrl
                        : null;

                playniteAPI.Notifications.Add(new NotificationMessage(
                    notificationId,
                    text,
                    NotificationType.Info,
                    () =>
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            System.Diagnostics.Process.Start(url);
                        }
                    }
                ));
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error showing price alert notification for {game.GameName}: {ex.Message}");
            }
        }

        public void NotifyPriceUpdate(WatchedGame game, decimal oldPrice)
        {
            if (!settings.NotificationsEnabled)
            {
                return;
            }

            try
            {
                var priceChange = game.LastPrice.Value - oldPrice;
                var direction = priceChange > 0 ? "aumentato" : "diminuito";
                var changeText = Math.Abs(priceChange).ToString("0.00") + "€";

                var text = $"📊 {game.GameName}: prezzo {direction} di {changeText}. Nuovo prezzo: {game.LastPrice.Value:0.00}€";

                logger.Info($"Sending price update notification: {text}");

                playniteAPI.Notifications.Add(new NotificationMessage(
                    $"allkeyshop-price-update-{game.Id}",
                    text,
                    NotificationType.Info,
                    () =>
                    {
                        if (!string.IsNullOrEmpty(game.LastUrl))
                        {
                            System.Diagnostics.Process.Start(game.LastUrl);
                        }
                    }
                ));
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error showing price update notification: {ex.Message}");
            }
        }

        private string BuildFreeGamesMessage(List<FreeGame> games)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Trovati {games.Count} nuovo/i gioco/i gratis:");

            foreach (var game in games.Take(5)) // Show max 5 games
            {
                sb.AppendLine($"• {game.GameName}");
            }

            if (games.Count > 5)
            {
                sb.AppendLine($"... e altri {games.Count - 5} giochi");
            }

            return sb.ToString();
        }

        public void ShowInfo(string title, string message)
        {
            playniteAPI.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                title,
                NotificationType.Info
            ));
        }

        public void ShowError(string title, string message)
        {
            playniteAPI.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                title,
                NotificationType.Error
            ));
        }
    }
}
