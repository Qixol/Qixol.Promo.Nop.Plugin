using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Widgets.Promo.Models;
using Qixol.Nop.Promo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Core.Domain.Orders;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Services.AttributeValues;
using Nop.Services.Directory;
using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Nop.Promo.Services.Banner;
using Qixol.Nop.Promo.Core.Domain.Banner;
using Qixol.Nop.Promo.Services.Localization;
using Nop.Services.Localization;
using Nop.Web.Framework.Themes;
using Qixol.Plugin.Widgets.Promo.Domain;
using System.Globalization;
using Qixol.Plugin.Widgets.Promo.Factories;
using Qixol.Plugin.Widgets.Promo.Services;

namespace Qixol.Plugin.Widgets.Promo.Controllers
{
    public partial class PromoWidgetController : Controller
    {
        #region Fields

        private readonly IProductBoxPromoModelFactory _productBoxPromoModelFactory;
        private readonly IPromoBannerDisplayPictureModelFactory _promoBannerDisplayPictureModelFactory;
        private readonly IProductDetailsPromotionModelFactory _productDetailsPromotionModelFactory;

        private readonly IPromoUtilities _promoUtilities;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IProductMappingService _productMappingService;
        private readonly IProductPromoMappingService _productPromoMappingService;
        private readonly IPromoDetailService _promoDetailService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IAttributeValueService _attributeValueService;
        private readonly ICurrencyService _currencyService;
        private readonly IPromoPictureService _promoPictureService;
        private readonly PromoSettings _promoSettings;
        private readonly PromoWidgetSettings _widgetSettings;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IPromoBannerService _promoBannerService;
        private readonly ILocalizationService _localizationService;
        private readonly IThemeContext _themeContext;
        private readonly IProductPromotionService _productPromotionService;

        #endregion

        #region constructor

        public PromoWidgetController(
            IProductBoxPromoModelFactory productBoxPromoModelFactory,
            IPromoBannerDisplayPictureModelFactory promoBannerDisplayPictureModelFactory,
            IProductDetailsPromotionModelFactory productDetailsPromotionModelFactory,

            IPromoUtilities promoUtilities,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IProductMappingService productMappingService,
            IProductPromoMappingService productPromoMappingService,
            IPromoDetailService promoDetailService,
            IStoreService storeService,
            IStoreContext storeContext,
            IAttributeValueService attributeValueService,
            ICurrencyService currencyService,
            IPromoPictureService promoPictureService,
            IPictureService pictureService,
            IProductService productService,
            PromoWidgetSettings widgetSettings,
            IPromoBannerService promoBannerService,
            ILocalizationService localizationService,
            IThemeContext themeContext,
            IProductPromotionService productPromotionService)
        {
            this._productBoxPromoModelFactory = productBoxPromoModelFactory;
            this._promoBannerDisplayPictureModelFactory = promoBannerDisplayPictureModelFactory;
            this._productDetailsPromotionModelFactory = productDetailsPromotionModelFactory;

            this._promoUtilities = promoUtilities;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._productMappingService = productMappingService;
            this._productPromoMappingService = productPromoMappingService;
            this._promoDetailService = promoDetailService;
            this._promoSettings = promoSettings;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._attributeValueService = attributeValueService;
            this._currencyService = currencyService;
            this._promoPictureService = promoPictureService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._widgetSettings = widgetSettings;
            this._promoBannerService = promoBannerService;
            this._localizationService = localizationService;
            this._themeContext = themeContext;
            this._productPromotionService = productPromotionService;
        }

        #endregion

        #region utilities

        private ActionResult BannerForWidgetZone(string widgetZone, object additionalData)
        {
            var allEnabledWidgetZones = _promoBannerService.RetrieveAllEnabledWidgetZones();
            if (!allEnabledWidgetZones.Any(wz => string.Compare(wz.WidgetZoneSystemName, widgetZone, true) == 0))
                return new EmptyResult();

            var bannersToDisplay = new List<PromoBannerDisplayModel>();

            List<PromoDetail> allPromos = null;
            List<ValidatedPromo> validatedPromotions = new List<ValidatedPromo>();
            IQueryable<ProductMappingItem> productMappings = null;
            IQueryable<ProductPromotionMapping> productPromotionMappings = null;
            List<KeyValuePair<string, string>> basketCriteriaChecks = null;
            List<int> promoIdsValidForProduct = null;

            // If we're looking at a product page, then we can further restrict the Pictures to be displayed
            //  by checking whether the promotion is valid for this product.
            if (IsProductWidgetZone(widgetZone))
            {
                int productId = 0;
                if (int.TryParse(additionalData.ToString(), out productId))
                {
                    productMappings = _productMappingService.RetrieveAllVariantsByProductId(productId, EntityAttributeName.Product, true);
                    if (productMappings != null && productMappings.Count() > 0)
                    {
                        var mappingIds = productMappings.Select(m => m.Id).ToList();
                        productPromotionMappings = _productPromoMappingService.RetrieveForProductMappingsList(mappingIds);
                        promoIdsValidForProduct = productPromotionMappings.GroupBy(gb => gb.PromotionId).Select(s => s.FirstOrDefault().PromotionId).ToList();
                    }
                }
            }

            // if we get here - one or more of the banners has the widget zone defined for it.
            var allBanners = _promoBannerService.RetrieveAllBanners();
            allBanners.Where(s => s.Enabled && allEnabledWidgetZones.ToList().Any(wz => wz.PromoBannerId == s.Id && string.Compare(wz.WidgetZoneSystemName, widgetZone, true) == 0))
                      .ToList()
                      .ForEach(s =>
                      {
                          var bannerToDisplay = new PromoBannerDisplayModel()
                          {
                              BannerId = s.Id,
                              BannerName = s.Name,
                              TransitionType = s.TransitionType
                          };

                          var picturesForBanner = _promoBannerService.RetrievePicturesForBanner(s.Id);
                          picturesForBanner.ToList()
                                           .ForEach(ps =>
                                           {
                                               if (!string.IsNullOrEmpty(ps.PromoReference))
                                               {
                                                   if (allPromos == null)
                                                       allPromos = _promoDetailService.RetrieveAll().ToList();

                                                   var promo = allPromos.Where(p => string.Compare(p.YourReference, ps.PromoReference, true) == 0
                                                                                    && (promoIdsValidForProduct == null || promoIdsValidForProduct.Contains(p.PromoId))).FirstOrDefault();
                                                   if (promo != null)
                                                   {
                                                       var validatedPromo = validatedPromotions.Where(vp => vp.PromotionId == promo.PromoId).FirstOrDefault();
                                                       if (validatedPromo == null)
                                                       {
                                                           if (basketCriteriaChecks == null)
                                                               basketCriteriaChecks = _productPromotionService.BasketCriteriaItems();

                                                           validatedPromo = ValidatedPromo.Create(promo, productMappings, basketCriteriaChecks, productPromotionMappings);
                                                           validatedPromotions.Add(validatedPromo);
                                                       }

                                                       if (validatedPromo.ValidToDisplay)
                                                           bannerToDisplay.Pictures.Add(_promoBannerDisplayPictureModelFactory.PreparePromoBannerDisplayPictureModel(ps, s.TransitionType));
                                                   }
                                               }
                                               else
                                               {
                                                   // If there isn't a promo reference - we'll have to always display it!
                                                   bannerToDisplay.Pictures.Add(_promoBannerDisplayPictureModelFactory.PreparePromoBannerDisplayPictureModel(ps, s.TransitionType));
                                               }
                                           });

                          if (bannerToDisplay.Pictures.Count > 0)
                          {
                              // Ensure pictures are sorted by the display sequence.
                              bannerToDisplay.Pictures = bannerToDisplay.Pictures.OrderBy(ob => ob.DisplaySequence).ToList();

                              string width = "100";
                              // set the width of pictures (if stacking)
                              if (s.TransitionType.CompareTo(NivoTransition.STACKHORIZONTAL.TransitionType) == 0)
                              {
                                  width = (100M / bannerToDisplay.Pictures.Count).ToString(CultureInfo.InvariantCulture);
                              }
                              bannerToDisplay.Pictures.ForEach(p => { p.Width = width; });
                              bannersToDisplay.Add(bannerToDisplay);
                          }
                      });

            if (bannersToDisplay.Count > 0)
                return PartialView("_PromoPictureBanner", bannersToDisplay);
            else
                return new EmptyResult();
        }

        private bool IsProductWidgetZone(string widgetZone)
        {
            // May want to review whether this is good enough!
            return widgetZone.ToLower().StartsWith("productdetails_");
        }

        private ActionResult ProductPagePromoDetails(object additionalData = null)
        {
            ProductDetailsPromotionModel productDetailsPromoModel = _productDetailsPromotionModelFactory.PrepareProductDetailsPromoModel(additionalData);
            if (productDetailsPromoModel.HasPromo)
                return PartialView("_ProductDetailsPromotion", productDetailsPromoModel);
            else
                return new EmptyResult();
        }

        #endregion

        #region methods

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            switch (widgetZone)
            {
                // This one is needed, so that if we are not showing promotion details (which requires the selection of a widget zone), but we are showing 
                // stickers on the product page, then we just need to setup for one of the zones on the product page so that the stickers are displayed.
                case "productdetails_add_info":
                    if ((_widgetSettings.ShowStickersInProductPage && !_widgetSettings.ShowPromoDetailsOnProductPage)
                        || (string.Compare(_widgetSettings.ProductPagePromoDetailsWidgetZone, widgetZone) == 0))
                    {
                        ProductDetailsPromotionModel productDetailsPromoModel = _productDetailsPromotionModelFactory.PrepareProductDetailsPromoModel(additionalData);
                        if (productDetailsPromoModel.HasPromo)
                            return PartialView("_ProductDetailsPromotion", productDetailsPromoModel);
                    }
                    return new EmptyResult();

                case "productbox_addinfo_after":
                    ProductBoxPromotionModel productBoxPromoModel = _productBoxPromoModelFactory.PrepareProductBoxPromoModel(additionalData);
                    if (productBoxPromoModel.HasPromo)
                        return PartialView("_ProductBoxPromotion", productBoxPromoModel);
                    else
                        return new EmptyResult();

                default:
                    if (string.Compare(_widgetSettings.ProductPagePromoDetailsWidgetZone, widgetZone, true) == 0)
                        return ProductPagePromoDetails(additionalData);
                    else
                        return BannerForWidgetZone(widgetZone, additionalData);
            }
        }

        #endregion

    }
}
