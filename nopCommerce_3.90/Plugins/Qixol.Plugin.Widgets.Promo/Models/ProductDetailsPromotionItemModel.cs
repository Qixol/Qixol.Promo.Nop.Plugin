using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class ProductDetailsPromotionItemModel
    {
        #region private variables

        private List<DiscountRangeModel> _discountRanges;
        private List<PromoAvailabilityItemModel> _availability;

        #endregion

        #region properties

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

        public string YouSaveText { get; set; }

        public bool ShowFromText { get; set; }

        public List<PromoAvailabilityItemModel> Availability
        {
            get { return _availability ?? (_availability = new List<PromoAvailabilityItemModel>()); }
            set { _availability = value; }
        }

        public List<DiscountRangeModel> DiscountRanges
        {
            get { return _discountRanges ?? (_discountRanges = new List<DiscountRangeModel>()); }
            set { _discountRanges = value; }
        }

        #endregion
    }
}
