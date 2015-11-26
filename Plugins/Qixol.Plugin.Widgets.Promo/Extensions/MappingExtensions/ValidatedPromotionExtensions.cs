using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Extensions.MappingExtensions
{
    public static class ValidatedPromoExtensions
    {
        /// <summary>
        /// Build the model for the Validated Promo.
        /// </summary>
        /// <param name="promo"></param>
        /// <returns></returns>
        public static ProductDetailsPromotionItemModel ToModel(this ValidatedPromo promo)
        {
            return new ProductDetailsPromotionItemModel()
                {
                    Title = promo.Title,
                    Description = promo.Description,
                    PromotionTypeName = promo.PromoType,
                    ValidFrom = promo.ValidFrom.HasValue ? promo.ValidFrom.Value.ToLocalTime() : promo.ValidFrom,
                    ValidTo = promo.ValidTo.HasValue ? promo.ValidTo.Value.ToLocalTime() : promo.ValidTo,
                    ImageName = promo.ImageName,
                    DiscountAmount = promo.DiscountAmount,
                    DiscountPercent = promo.DiscountPercent,
                    YourReference = promo.YourReference,

                    ForAllVariants = promo.ForAllVariants,
                    HasMultipleMatchingRestrictions = promo.HasMultipleMatchingRestrictions,
                    HasMultipleRequireAdditionalItems = promo.HasMultipleRequireAdditionalItems,
                    HasMultipleRequiredQty = promo.HasMultipleRequiredQty,
                    HasMultipleRequiredSpend = promo.HasMultipleRequiredSpend,
                    MatchingRestriction = promo.MatchingRestriction,
                    MinimumSpend = promo.MinimumSpend,
                    RequireAdditionalItems = promo.RequireAdditionalItems,
                    RequiredItemQty = promo.RequiredItemQty,
                    RequiredItemSpend = promo.RequiredItemSpend,

                    Availability = promo.Availability != null
                                        ? promo.Availability.Select(ptda => new PromoAvailabilityItemModel() { Start = ptda.Start.ToLocalTime(), End = ptda.End.ToLocalTime() }).ToList()
                                        : null
                };            
        }
    }
}
