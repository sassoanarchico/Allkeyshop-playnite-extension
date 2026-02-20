using System;

namespace AllKeyShopExtension.Models
{
    public class WatchedGame
    {
        public int Id { get; set; }
        public string GameName { get; set; }
        public decimal? LastPrice { get; set; }
        public string LastSeller { get; set; }
        public string LastUrl { get; set; }
        public DateTime LastUpdate { get; set; }
        public decimal? KeyPriceThreshold { get; set; }
        public decimal? AccountPriceThreshold { get; set; }
        public DateTime DateAdded { get; set; }

        // Separate key and account pricing
        public decimal? KeyPrice { get; set; }
        public string KeySeller { get; set; }
        public decimal? AccountPrice { get; set; }
        public string AccountSeller { get; set; }

        // AllKeyShop page URL (for "Open" button)
        public string AllKeyShopPageUrl { get; set; }

        // Game thumbnail URL (from search results)
        public string ImageUrl { get; set; }

        // Display helpers
        public string KeyPriceDisplay => KeyPrice.HasValue ? $"{KeyPrice.Value:0.00}€ ({KeySeller})" : "N/A";
        public string AccountPriceDisplay => AccountPrice.HasValue ? $"{AccountPrice.Value:0.00}€ ({AccountSeller})" : "N/A";
        public string BestPriceDisplay
        {
            get
            {
                if (LastPrice.HasValue) return $"{LastPrice.Value:0.00}€";
                return "N/A";
            }
        }

        public string ThresholdDisplay
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (KeyPriceThreshold.HasValue)
                    parts.Add($"K:{KeyPriceThreshold.Value:0.00}\u20ac");
                if (AccountPriceThreshold.HasValue)
                    parts.Add($"A:{AccountPriceThreshold.Value:0.00}\u20ac");
                return parts.Count > 0 ? string.Join(" | ", parts) : "\u2014";
            }
        }

        // Legacy compat: returns true if any threshold is set
        public bool HasAnyThreshold => KeyPriceThreshold.HasValue || AccountPriceThreshold.HasValue;

        public WatchedGame()
        {
            DateAdded = DateTime.Now;
            LastUpdate = DateTime.MinValue;
        }
    }
}
