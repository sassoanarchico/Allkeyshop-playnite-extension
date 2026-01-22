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

        public WatchedGame()
        {
            DateAdded = DateTime.Now;
            LastUpdate = DateTime.MinValue;
        }
    }
}
