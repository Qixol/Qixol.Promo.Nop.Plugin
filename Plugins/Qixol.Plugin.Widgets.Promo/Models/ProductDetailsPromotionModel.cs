using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class ProductDetailsPromotionModel
    {
        public bool HasPromo { get; set; }

        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public bool ShowSticker { get; set; }

        public bool ShowPromotionDetails { get; set; }

        public bool HasTierPrices { get; set; }

        public List<ProductDetailsPromotionItemModel> PromotionItems { get; set; }

        public ProductDetailsPromotionModel()
        {
            PromotionItems = new List<ProductDetailsPromotionItemModel>();
        }
    }
}
