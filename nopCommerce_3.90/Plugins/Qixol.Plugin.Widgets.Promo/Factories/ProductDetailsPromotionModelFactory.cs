using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Themes;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Widgets.Promo.Services;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Plugin.Widgets.Promo.Extensions;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public class ProductDetailsPromotionModelFactory : IProductDetailsPromotionModelFactory
    {
        #region private variables

        private readonly IProductDetailsPromotionItemModelFactory _productDetailsPromotionItemModelFactory;

        private readonly PromoSettings _promoSettings;
        private readonly PromoWidgetSettings _widgetSettings;
        private readonly IProductService _productService;
        private readonly IPromoPictureService _promoPictureService;
        private readonly IPictureService _pictureService;
        private readonly IProductPromotionService _productPromotionService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IThemeContext _themeContext;

        #endregion

        #region constructor

        public ProductDetailsPromotionModelFactory(
            IProductDetailsPromotionItemModelFactory productDetailsPromotionItemModelFactory,
            PromoSettings promoSettings,
            PromoWidgetSettings widgetSettings,
            IProductService productService,
            IPromoPictureService promoPictureService,
            IPictureService pictureService,
            IProductPromotionService productPromotionService,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            IThemeContext themeContext)
        {
            this._productDetailsPromotionItemModelFactory = productDetailsPromotionItemModelFactory;
            this._promoSettings = promoSettings;
            this._widgetSettings = widgetSettings;
            this._productService = productService;
            this._promoPictureService = promoPictureService;
            this._pictureService = pictureService;
            this._productPromotionService = productPromotionService;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._priceFormatter = priceFormatter;
            this._localizationService = localizationService;
            this._themeContext = themeContext;
        }

        #endregion

        #region Utilities

        #endregion

        #region methods

        public ProductDetailsPromotionModel PrepareProductDetailsPromoModel(object additionalData)
        {
            ProductDetailsPromotionModel model = new ProductDetailsPromotionModel();
            model.HasPromo = false;

            if (!_promoSettings.Enabled)
                return model;

            if (!(_widgetSettings.ShowStickersInProductPage || _widgetSettings.ShowPromoDetailsOnProductPage))
                return model;

            int productId = 0;
            if (!int.TryParse(additionalData.ToString(), out productId))
                return model;

            var productPromotions = _productPromotionService.PromotionsForProduct(productId);
            if (!productPromotions.HasPromo)
                return model;

            var product = _productService.GetProductById(productId);
            if (product != null)
                model.HasTierPrices = product.HasTierPrices;

            model.Id = productId;
            model.ShowSticker = _widgetSettings.ShowStickersInProductPage;
            model.ShowPromotionDetails = _widgetSettings.ShowPromoDetailsOnProductPage;
            model.HasPromo = true;
            model.ImageUrl = _productPromotionService.GetSingleImageUrl(productPromotions.PromosToDisplay, productPromotions.BaseImageUrl);
            model.PromotionItems = productPromotions.PromosToDisplay.Select(ptd => _productDetailsPromotionItemModelFactory.PrepareProductDetailsPromotionItemModel(ptd, model.HasTierPrices)).ToList();

            return model;
        }

        #endregion
    }
}
