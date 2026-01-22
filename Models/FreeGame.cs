using System;

namespace AllKeyShopExtension.Models
{
    public class FreeGame
    {
        public string GameName { get; set; }
        public string Platform { get; set; }
        public string Url { get; set; }
        public DateTime DateFound { get; set; }
        public string Description { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public FreeGame()
        {
            DateFound = DateTime.Now;
        }
    }
}
