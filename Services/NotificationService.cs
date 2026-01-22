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

                    var title = $"Nuovi giochi gratis su {platform}!";
                    var message = BuildFreeGamesMessage(games);

                    playniteAPI.Notifications.Add(new NotificationMessage(
                        Guid.NewGuid().ToString(),
                        title,
                        NotificationType.Info,
                        () =>
                        {
                            // Open AllKeyShop when notification is clicked
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

        public void NotifyPriceAlert(WatchedGame game)
        {
            if (!settings.NotificationsEnabled || !settings.PriceAlertsEnabled)
            {
                return;
            }

            try
            {
                var title = $"Alert Prezzo: {game.GameName}";
                var message = $"Il prezzo è sceso a {game.LastPrice:C}! " +
                             $"Soglia impostata: {game.PriceThreshold:C}";

                playniteAPI.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    title,
                    NotificationType.Info,
                    () =>
                    {
                        // Open AllKeyShop when notification is clicked
                        if (!string.IsNullOrEmpty(game.LastUrl))
                        {
                            System.Diagnostics.Process.Start(game.LastUrl);
                        }
                    }
                ));
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error showing price alert notification: {ex.Message}");
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
                var changeText = Math.Abs(priceChange).ToString("C");

                var title = $"Prezzo aggiornato: {game.GameName}";
                var message = $"Il prezzo è {direction} di {changeText}. " +
                             $"Nuovo prezzo: {game.LastPrice:C}";

                playniteAPI.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    title,
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
