using System.Collections.Generic;

namespace AllKeyShopExtension.Models
{
    public class ExtensionSettings
    {
        public List<string> EnabledPlatforms { get; set; }
        public int PriceUpdateIntervalMinutes { get; set; }
        public int FreeGamesCheckIntervalMinutes { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool PriceAlertsEnabled { get; set; }

        public ExtensionSettings()
        {
            EnabledPlatforms = new List<string>();
            PriceUpdateIntervalMinutes = 60; // Default 1 hour
            FreeGamesCheckIntervalMinutes = 120; // Default 2 hours
            NotificationsEnabled = true;
            PriceAlertsEnabled = true;
        }

        public bool IsPlatformEnabled(string platform)
        {
            return EnabledPlatforms.Contains(platform);
        }

        public void EnablePlatform(string platform)
        {
            if (!EnabledPlatforms.Contains(platform))
            {
                EnabledPlatforms.Add(platform);
            }
        }

        public void DisablePlatform(string platform)
        {
            EnabledPlatforms.Remove(platform);
        }
    }
}
