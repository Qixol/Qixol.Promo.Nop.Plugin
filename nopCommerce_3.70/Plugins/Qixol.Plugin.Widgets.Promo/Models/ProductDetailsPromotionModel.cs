using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class ProductDetailsPromotionModel
    {
        private List<ProductDetailsPromotionItemModel> _productDetailsPromotionItems;

        public bool HasPromo { get; set; }

        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public bool ShowSticker { get; set; }

        public bool ShowPromotionDetails { get; set; }

        public bool HasTierPrices { get; set; }

        public List<ProductDetailsPromotionItemModel> PromotionItems
        {
            get { return _productDetailsPromotionItems ?? (_productDetailsPromotionItems = new List<ProductDetailsPromotionItemModel>()); }
            set { _productDetailsPromotionItems = value; }
        }
    }
}
