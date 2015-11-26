using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Promo
{
    public class ValidatedPromo
    {
        #region public properties

        /// <summary>
        /// The promo title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The promo description (End-user/Customer description as entered against the Promo)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The promo system type.
        /// </summary>
        public string PromoType { get; set; }

        /// <summary>
        /// Where the promo starts during the day it was validated for - this will be the start time (date part is irrelevant)
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Where the promo ends during the day it was validated for - this will be the end time (date part is irrelevant)
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Flag indicating whether the promo has validated successfully for the current UTC date/time.
        /// </summary>
        public bool ValidForTime { get; set; }

        /// <summary>
        /// Flag indicating whether the promo has validated against the provided basket criteria.
        /// </summary>
        public bool ValidForCriteria { get; set; }

        /// <summary>
        /// Flag indicating whether the promo is valid for all variants of the current product.
        /// </summary>
        public bool ForAllVariants { get; set; }

        /// <summary>
        /// A total minimum spend amount (of the order) required for this promo to be applied.
        /// </summary>
        public decimal? MinimumSpend { get; set; }

        /// <summary>
        /// The discount amount to be applied by this promo (not always provided! - as we won't always know at this point).
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// The discount percentage to be applied by this promo (not always provided! - as we won't always know at this point).
        /// </summary>
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// Flag indicating whether the variants that this promo is validated against has multiple different Required Qtys defined.
        /// </summary>
        public bool HasMultipleRequiredQty { get; set; }

        /// <summary>
        /// Flag indicating whether the variants that this promo is validated against has multiple different Required Spends defined.
        /// </summary>
        public bool HasMultipleRequiredSpend { get; set; }

        /// <summary>
        /// Flag indicating whether the variants that this promo is validated against has multiple different Require Additional Items defined.
        /// </summary>
        public bool HasMultipleRequireAdditionalItems { get; set; }

        /// <summary>
        /// Flag indicating whether the variants that this promo is validated against has multiple different Matching Restrictions defined.
        /// </summary>
        public bool HasMultipleMatchingRestrictions { get; set; }

        /// <summary>
        /// The required quantity for the item, for the promo to be triggered.  
        /// </summary>
        public int? RequiredItemQty { get; set; }

        /// <summary>
        /// Flag indicating whether for hte promo to be applied, additional items must be added to the basket (other than the current item being validated).
        /// </summary>
        public bool? RequireAdditionalItems { get; set; }

        /// <summary>
        /// Setting which indicates whether when we require a qty > 1 or spend amount is defined, that the items must be the Same, Different or there is no restriction.
        /// </summary>
        public string MatchingRestriction { get; set; }
       
        /// <summary>
        /// For the product being validated - this is the required item spend for the promo to trigger.
        /// </summary>
        public decimal? RequiredItemSpend { get; set; }
  
        /// <summary>
        /// Flag indicating whether the promo is valid to be displayed to the user.
        /// </summary>
        public bool ValidToDisplay
        {
            get
            {
                return (this.ValidForTime && this.ValidForCriteria);
            }
        }

        /// <summary>
        /// Short image name for the promo, formatted based on the Promo Type.
        /// </summary>
        public string ImageName
        {
            get
            {
                return string.Format("promo_{0}", this.PromoType.Trim().ToLower());
            }
        }

        /// <summary>
        /// List of timebands when the promo is available.
        /// </summary>
        public List<ValidatedPromoAvailability> Availability { get; set; }

        /// <summary>
        /// The unique reference specified by the user when creating the promo in Promo.
        /// </summary>
        public string YourReference { get; set; }

        /// <summary>
        /// The id of the promotion.
        /// </summary>
        public int PromotionId { get; set; }

        #endregion

        #region ctor

        public ValidatedPromo()
        {
            Availability = new List<ValidatedPromoAvailability>();
        }

        #endregion

        #region helper methods

        public static ValidatedPromo Create(PromoDetail promo, IQueryable<ProductMappingItem> mappings, List<KeyValuePair<string, string>> basketCriteriaChecks, IQueryable<ProductPromotionMapping> productPromoMappings)
        {
            // Reload the XML into the Integration object, so that we can make use of its validation routines.
            // We also use this for the available times.
            var integrationPromo = Qixol.Promo.Integration.Lib.Export.ExportPromotionDetailsItem.RetrieveFromXml(promo.PromoXml);
            // the Xml only has the TIME for the valid from / to, and then only if the promotion starts/expires on that day.
            // for completeness, reset these to the full datetime (or min/max if not present)
            integrationPromo.ValidFrom = promo.ValidFrom.HasValue ? promo.ValidFrom.Value : DateTime.MinValue;
            integrationPromo.ValidTo = promo.ValidTo.HasValue ? promo.ValidTo.Value : DateTime.MaxValue;
            if (integrationPromo.ValidTo.CompareTo(DateTime.MinValue) == 0)
                integrationPromo.ValidTo = DateTime.MaxValue;

            List<IGrouping<int, ProductPromotionMapping>> requiredQuantities = null;
            List<IGrouping<decimal, ProductPromotionMapping>> requiredSpend = null;
            List<IGrouping<bool, ProductPromotionMapping>> multipleProductRestrictions = null;
            List<IGrouping<string, ProductPromotionMapping>> matchingRestrictions = null;

            if(productPromoMappings != null)
            {
                requiredQuantities = productPromoMappings.GroupBy(gb => gb.RequiredQty).ToList();
                requiredSpend = productPromoMappings.GroupBy(gb => gb.RequiredSpend).ToList();
                multipleProductRestrictions = productPromoMappings.GroupBy(gb => gb.MultipleProductRestrictions).ToList();
                matchingRestrictions = productPromoMappings.GroupBy(gb => gb.MatchingRestrictions).ToList();            
            }

            var returnItem = new ValidatedPromo()
            {
                // Set basic details
                PromotionId = promo.PromoId,
                Title = promo.PromoName,
                PromoType = promo.PromoTypeName,
                Description = string.IsNullOrEmpty(promo.DisplayText) ? promo.PromoName : promo.DisplayText,
                ValidFrom = promo.ValidFrom,
                ValidTo = promo.ValidTo,
                MinimumSpend = promo.MinimumSpend,
                DiscountAmount = promo.DiscountAmount,
                DiscountPercent = promo.DiscountPercent,
                YourReference = promo.YourReference,

                HasMultipleRequiredQty = requiredQuantities != null && requiredQuantities.Count > 1,
                HasMultipleRequiredSpend = requiredSpend != null && requiredSpend.Count > 1,
                HasMultipleMatchingRestrictions = matchingRestrictions != null && matchingRestrictions.Count > 1,
                HasMultipleRequireAdditionalItems = multipleProductRestrictions != null && multipleProductRestrictions.Count > 1,

                RequiredItemQty = (requiredQuantities != null && requiredQuantities.Count == 1) ? requiredQuantities.FirstOrDefault().First().RequiredQty : default(int?),
                RequiredItemSpend = (requiredSpend != null && requiredSpend.Count == 1) ? requiredSpend.FirstOrDefault().First().RequiredSpend : default(decimal?),
                RequireAdditionalItems = (multipleProductRestrictions != null && multipleProductRestrictions.Count == 1) ? multipleProductRestrictions.FirstOrDefault().First().MultipleProductRestrictions : false,
                MatchingRestriction = (matchingRestrictions != null && matchingRestrictions.Count == 1) ? matchingRestrictions.FirstOrDefault().First().MatchingRestrictions : string.Empty,

                // Revalidate the Criteria and Time restrictions for this promo.
                ValidForCriteria = integrationPromo.ValidateUnmatchedCriteria(basketCriteriaChecks),
                ValidForTime = integrationPromo.ValidateForTime(DateTime.UtcNow),

                // Where timebands are specified, return them
                Availability = (integrationPromo.AvailableTimes != null && integrationPromo.AvailableTimes.Count > 0) ?
                                    integrationPromo.AvailableTimes.Select(a => new ValidatedPromoAvailability() { Start = a.StartTime, End = a.EndTime }).ToList()
                                    : null,

            };

            if (mappings != null && productPromoMappings != null)
            {
                // Check to see whether all variants for the product are valid for this promo.
                returnItem.ForAllVariants = mappings.Count() == 1 ? true
                                    : (mappings.Count() == productPromoMappings.Where(ppm => ppm.PromotionId == promo.PromoId).Count());

            }

            return returnItem;
        }

        #endregion

    }
}
