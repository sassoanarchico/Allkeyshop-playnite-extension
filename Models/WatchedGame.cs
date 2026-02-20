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
        public decimal? PriceThreshold { get; set; }
        public DateTime DateAdded { get; set; }

        // Separate key and account pricing
        public decimal? KeyPrice { get; set; }
        public string KeySeller { get; set; }
        public decimal? AccountPrice { get; set; }
        public string AccountSeller { get; set; }

        // AllKeyShop page URL (for "Apri" button)
        public string AllKeyShopPageUrl { get; set; }

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

        public WatchedGame()
        {
            DateAdded = DateTime.Now;
            LastUpdate = DateTime.MinValue;
        }
    }
}
