using System;
using System.Collections.Generic;
using System.Linq;

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

        // Key vs Account pricing
        public decimal? KeyPrice { get; set; }
        public string KeySeller { get; set; }
        public string KeyOfferUrl { get; set; }
        public bool KeyIsOfficial { get; set; }

        public decimal? AccountPrice { get; set; }
        public string AccountSeller { get; set; }
        public string AccountOfferUrl { get; set; }

        // AllKeyShop page URL (for "Apri" button)
        public string AllKeyShopPageUrl { get; set; }

        // All offers for detailed view
        public List<OfferInfo> AllOffers { get; set; }

        public GamePrice()
        {
            Currency = "EUR";
            RetrievedAt = DateTime.Now;
            IsAvailable = true;
            AllOffers = new List<OfferInfo>();
        }
    }

    public class OfferInfo
    {
        public int OfferId { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public string MerchantName { get; set; }
        public int MerchantId { get; set; }
        public bool IsOfficial { get; set; }
        public bool IsAccount { get; set; }
        public string Edition { get; set; }
        public string Region { get; set; }
        public string VoucherCode { get; set; }
        public decimal? PriceWithPaypal { get; set; }
        public decimal? PriceWithCard { get; set; }
        public string BuyUrl { get; set; }

        /// <summary>
        /// The lowest fee price (minimum of priceCard and pricePaypal).
        /// Falls back to base Price if neither fee price is available.
        /// </summary>
        public decimal LowestFeePrice
        {
            get
            {
                var candidates = new List<decimal>();
                if (PriceWithCard.HasValue && PriceWithCard.Value > 0)
                    candidates.Add(PriceWithCard.Value);
                if (PriceWithPaypal.HasValue && PriceWithPaypal.Value > 0)
                    candidates.Add(PriceWithPaypal.Value);
                return candidates.Count > 0 ? candidates.Min() : Price;
            }
        }
    }
}
