using System;

namespace AllKeyShopExtension.Models
{
    public class SearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Platform { get; set; }
        public string Year { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public bool InStock { get; set; }

        public string DisplayText => $"{Title} ({Platform} - {Year}) - {(InStock ? Price.ToString("0.00") + "€" : "N/A")}";
    }
}
