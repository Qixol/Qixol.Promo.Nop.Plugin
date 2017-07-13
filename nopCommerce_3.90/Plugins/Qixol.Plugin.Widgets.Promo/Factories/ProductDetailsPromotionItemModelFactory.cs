using Nop.Services.Localization;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Nop.Promo.Services.Localization;
using Qixol.Plugin.Widgets.Promo.Services;
using Nop.Services.Directory;
using Nop.Core;
using Nop.Services.Catalog;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public class ProductDetailsPromotionItemModelFactory : IProductDetailsPromotionItemModelFactory
    {
        #region private variables

        private readonly IDiscountRangeModelFactory _discountRangeModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IProductPromotionService _productPromotionService;
        private readonly PromoSettings _promoSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region constructor

        public ProductDetailsPromotionItemModelFactory(IDiscountRangeModelFactory discountRangeModelFactory,
            ILocalizationService localizationService,
            IProductPromotionService productPromotionService,
            PromoSettings promoSettings,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IPriceFormatter priceFormatter)
        {
            this._discountRangeModelFactory = discountRangeModelFactory;

            this._localizationService = localizationService;
            this._productPromotionService = productPromotionService;
            this._promoSettings = promoSettings;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._priceFormatter = priceFormatter;
        }

        #endregion

        #region utilities
        #endregion

        #region methods

        public ProductDetailsPromotionItemModel PrepareProductDetailsPromotionItemModel(ValidatedPromo promo, bool productHasTierPrices)
        {
            var model = new ProductDetailsPromotionItemModel()
            {
                Title = promo.Title,
                // If the description is a resource key, this will get it, otherwise use the description.
                Description = _localizationService.GetValidatedResource(promo.Description),
                PromotionTypeName = promo.PromoType,
                ValidFrom = promo.ValidFrom.HasValue ? promo.ValidFrom.Value.ToLocalTime() : promo.ValidFrom,
                ValidTo = promo.ValidTo.HasValue ? promo.ValidTo.Value.ToLocalTime() : promo.ValidTo,
                ImageName = promo.ImageName,
                ImageUrl = _productPromotionService.GetPromoImageUrl(promo.YourReference, promo.PromoType, promo.ImageName),
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


            if (promo.DiscountRanges != null && promo.DiscountRanges.Count > 0)
            {
                // use the first discount range to hold the table titles
                string rangeTitle = (promo.DiscountRanges.First().MinimumQuantity.CompareTo(0) != 0) ? _localizationService.GetResource("Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Quantity") : _localizationService.GetResource("Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Spend");
                string discountTitle = (promo.DiscountRanges.First().DiscountPercentage.CompareTo(decimal.Zero) != 0) ? _localizationService.GetResource("Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Percentage") : _localizationService.GetResource("Plugins.Widgets.QixolPromo.Promotion.DiscountRange.Fixed");
                model.DiscountRanges.Add(new DiscountRangeModel() { Range = rangeTitle, Discount = discountTitle });

                // now set up the ranges
                promo.DiscountRanges.ToList().ForEach(dr =>
                {
                    model.DiscountRanges.Add(_discountRangeModelFactory.PrepareDiscountRangeModel(dr));
                });
            }

            if (model.DiscountAmount.HasValue && model.DiscountAmount.Value > 0)
            {
                var discountAmountForCurrency = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets
                                                    ? promo.DiscountAmount.Value
                                                    : _currencyService.ConvertFromPrimaryStoreCurrency(promo.DiscountAmount.Value, _workContext.WorkingCurrency);
                model.DiscountAmountAsCurrency = _priceFormatter.FormatPrice(discountAmountForCurrency, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, false);
            }

            if (model.MinimumSpend.HasValue && model.MinimumSpend.Value > 0)
            {
                var minimumSpendForCurrency = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets
                                                    ? promo.MinimumSpend.Value
                                                    : _currencyService.ConvertFromPrimaryStoreCurrency(promo.MinimumSpend.Value, _workContext.WorkingCurrency);
                model.MinimumSpendAsCurrency = _priceFormatter.FormatPrice(minimumSpendForCurrency, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, false);
            }

            if (model.RequiredItemSpend.HasValue && model.RequiredItemSpend.Value > 0)
            {
                var rqdItemSpendForCurrency = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets
                                                    ? promo.RequiredItemSpend.Value
                                                    : _currencyService.ConvertFromPrimaryStoreCurrency(promo.RequiredItemSpend.Value, _workContext.WorkingCurrency);
                model.RequiredItemSpendAsCurrency = _priceFormatter.FormatPrice(rqdItemSpendForCurrency, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, false);
            }

            model.YouSaveText = "-";
            model.ShowFromText = false;
            switch (model.PromotionTypeName)
            {
                case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.MultiBuy:
                    model.RequireAdditionalItems = false;
                    model.RequiredItemQty = null;
                    model.RequiredItemSpend = null;
                    break;

                case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.BuyOneGetOneFree:
                    model.YouSaveText = _localizationService.GetResource("Plugins.Misc.QixolPromo.Product.Promos.Item.GetOneFree");     //"Get One Free";
                    break;

                case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.BuyOneGetOneReduced:
                case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.ProductsReduction:
                    if (model.DiscountAmount.HasValue && model.DiscountAmount.Value > 0)
                    {
                        model.YouSaveText = model.DiscountAmountAsCurrency;
                        model.ShowFromText = productHasTierPrices;
                    }
                    else
                    {
                        if (model.DiscountPercent.HasValue && model.DiscountPercent.Value > 0)
                        {
                            model.YouSaveText = string.Format("{0:#.##}%", promo.DiscountPercent.Value);
                        }
                    }
                    break;

                default:
                    break;
            }

            return model;
        }
    }

    #endregion
}
