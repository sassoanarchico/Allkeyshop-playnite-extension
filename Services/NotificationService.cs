using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AllKeyShopExtension.Models;
using Microsoft.Toolkit.Uwp.Notifications;
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

        /// <summary>
        /// Sends a Windows toast notification (appears in Windows notification center).
        /// Falls back silently if toast notifications are unsupported.
        /// </summary>
        private void SendWindowsToast(string title, string body, string url = null)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(body);

                if (!string.IsNullOrEmpty(url))
                {
                    builder.AddArgument("url", url);
                }

                builder.Show();
                logger.Debug($"Windows toast sent: {title}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to send Windows toast notification: {ex.Message}");
            }
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
                    var text = $"🎮 New free games on {platform}!\n{message}";

                    logger.Info($"Sending free games notification: {text}");

                    var firstUrl = games.FirstOrDefault(g => !string.IsNullOrEmpty(g.Url))?.Url;

                    // Playnite notification
                    playniteAPI.Notifications.Add(new NotificationMessage(
                        $"allkeyshop-free-{platform}",
                        text,
                        NotificationType.Info,
                        () =>
                        {
                            if (!string.IsNullOrEmpty(firstUrl))
                            {
                                System.Diagnostics.Process.Start(firstUrl);
                            }
                        }
                    ));

                    // Windows toast
                    SendWindowsToast(
                        $"Free Games on {platform}!",
                        string.Join(", ", games.Take(3).Select(g => g.GameName)) +
                            (games.Count > 3 ? $" (+{games.Count - 3} more)" : ""),
                        firstUrl
                    );
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error showing free games notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends Playnite + Windows toast notifications when a game's key or account price
        /// drops below its respective threshold. Fires independently for each type.
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
                var url = !string.IsNullOrEmpty(game.LastUrl) ? game.LastUrl
                        : !string.IsNullOrEmpty(game.AllKeyShopPageUrl) ? game.AllKeyShopPageUrl
                        : null;

                // Key price alert
                if (game.KeyPriceThreshold.HasValue && game.KeyPrice.HasValue
                    && game.KeyPrice.Value <= game.KeyPriceThreshold.Value)
                {
                    var sellerText = !string.IsNullOrEmpty(game.KeySeller) ? $" ({game.KeySeller})" : "";
                    var text = $"🔑 {game.GameName} - Key price dropped to {game.KeyPrice.Value:0.00}€{sellerText}" +
                               $" | Threshold: {game.KeyPriceThreshold.Value:0.00}€";
                    var notificationId = $"allkeyshop-key-price-alert-{game.Id}";

                    logger.Info($"Sending key price alert: {text}");

                    playniteAPI.Notifications.Add(new NotificationMessage(
                        notificationId, text, NotificationType.Info,
                        () => { if (!string.IsNullOrEmpty(url)) System.Diagnostics.Process.Start(url); }
                    ));

                    SendWindowsToast(
                        $"Key Price Alert: {game.GameName}",
                        $"Key: {game.KeyPrice.Value:0.00}€{sellerText} (Threshold: {game.KeyPriceThreshold.Value:0.00}€)",
                        url
                    );
                }

                // Account price alert
                if (game.AccountPriceThreshold.HasValue && game.AccountPrice.HasValue
                    && game.AccountPrice.Value <= game.AccountPriceThreshold.Value)
                {
                    var sellerText = !string.IsNullOrEmpty(game.AccountSeller) ? $" ({game.AccountSeller})" : "";
                    var text = $"👤 {game.GameName} - Account price dropped to {game.AccountPrice.Value:0.00}€{sellerText}" +
                               $" | Threshold: {game.AccountPriceThreshold.Value:0.00}€";
                    var notificationId = $"allkeyshop-account-price-alert-{game.Id}";

                    logger.Info($"Sending account price alert: {text}");

                    playniteAPI.Notifications.Add(new NotificationMessage(
                        notificationId, text, NotificationType.Info,
                        () => { if (!string.IsNullOrEmpty(url)) System.Diagnostics.Process.Start(url); }
                    ));

                    SendWindowsToast(
                        $"Account Price Alert: {game.GameName}",
                        $"Account: {game.AccountPrice.Value:0.00}€{sellerText} (Threshold: {game.AccountPriceThreshold.Value:0.00}€)",
                        url
                    );
                }
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
                var direction = priceChange > 0 ? "increased" : "decreased";
                var changeText = Math.Abs(priceChange).ToString("0.00") + "€";

                var text = $"📊 {game.GameName}: price {direction} by {changeText}. New price: {game.LastPrice.Value:0.00}€";

                logger.Info($"Sending price update notification: {text}");

                // Playnite notification
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

                // Windows toast for price drops only (not increases)
                if (priceChange < 0)
                {
                    SendWindowsToast(
                        $"Price updated: {game.GameName}",
                        $"Price dropped by {changeText}. New price: {game.LastPrice.Value:0.00}€"
                    );
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error showing price update notification: {ex.Message}");
            }
        }

        private string BuildFreeGamesMessage(List<FreeGame> games)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Found {games.Count} new free game(s):");

            foreach (var game in games.Take(5)) // Show max 5 games
            {
                sb.AppendLine($"• {game.GameName}");
            }

            if (games.Count > 5)
            {
                sb.AppendLine($"... and {games.Count - 5} more games");
            }

            return sb.ToString();
        }

        public void ShowInfo(string title, string message)
        {
            playniteAPI.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                $"{title}: {message}",
                NotificationType.Info
            ));
        }

        public void ShowError(string title, string message)
        {
            playniteAPI.Notifications.Add(new NotificationMessage(
                Guid.NewGuid().ToString(),
                $"{title}: {message}",
                NotificationType.Error
            ));
        }
    }
}
