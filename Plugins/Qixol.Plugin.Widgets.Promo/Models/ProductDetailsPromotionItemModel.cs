using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class ProductDetailsPromotionItemModel
    {
        public string Title { get; set; }

        public string PromotionTypeName { get; set; }

        public string ImageUrl { get; set; }

        public string ImageName { get; set; }

        public string Description { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public bool ForAllVariants { get; set; }

        public decimal? MinimumSpend { get; set; }

        public string MinimumSpendAsCurrency { get; set; }

        public bool HasMultipleRequiredQty { get; set; }

        public bool HasMultipleRequiredSpend { get; set; }

        public bool HasMultipleRequireAdditionalItems { get; set; }

        public bool HasMultipleMatchingRestrictions { get; set; }

        public int? RequiredItemQty { get; set; }

        public bool? RequireAdditionalItems { get; set; }

        public string MatchingRestriction { get; set; }

        public decimal? RequiredItemSpend { get; set; }

        public string RequiredItemSpendAsCurrency { get; set; }

        public decimal? DiscountAmount { get; set; }

        public string DiscountAmountAsCurrency { get; set; }

        public decimal? DiscountPercent { get; set; }

        public string YourReference { get; set; }

        public List<PromoAvailibilityItemModel> Availibility { get; set; }

        public ProductDetailsPromotionItemModel()
        {
            Availibility = new List<PromoAvailibilityItemModel>();            
        }
    }
}
