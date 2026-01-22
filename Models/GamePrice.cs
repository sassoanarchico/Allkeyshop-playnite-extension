using System;

namespace AllKeyShopExtension.Models
{
    public class GamePrice
    {
        public string GameName { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Seller { get; set; }
        public string Url { get; set; }
        public DateTime RetrievedAt { get; set; }
        public bool IsAvailable { get; set; }

        public GamePrice()
        {
            Currency = "EUR";
            RetrievedAt = DateTime.Now;
            IsAvailable = true;
        }
    }
}
