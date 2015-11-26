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
using Qixol.Plugin.Widgets.Promo.Extensions.MappingExtensions;
using Qixol.Nop.Promo.Services.Banner;
using Qixol.Nop.Promo.Core.Domain.Banner;
using Qixol.Nop.Promo.Services.Localization;
using Nop.Services.Localization;
using Nop.Web.Framework.Themes;
using Qixol.Plugin.Widgets.Promo.Domain;
using System.Globalization;

namespace Qixol.Plugin.Widgets.Promo.Controllers
{
    public partial class PromoWidgetController : Controller
    {
        #region Fields

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

        #endregion

        #region constructor

        public PromoWidgetController(
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
            IThemeContext themeContext)
        {
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
        }

        #endregion

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            switch (widgetZone)
            {
                case "account_navigation_after":
                    return PartialView("CustomerNavigationExtension");
                    //return string.Compare(_widgetSettings.ProductPagePromoDetailsWidgetZone, widgetZone, true) == 0 ? GetProductPagePromoDetails(additionalData) : new EmptyResult();

                // This one is needed, so that if we are not showing promotion details (which requires the selection of a widget zone), but we are showing 
                // stickers on the product page, then we just need to setup for one of the zones on the product page so that the stickers are displayed.
                case "productdetails_add_info":
                    if ((_widgetSettings.ShowStickersInProductPage && !_widgetSettings.ShowPromoDetailsOnProductPage)
                        || (string.Compare(_widgetSettings.ProductPagePromoDetailsWidgetZone, widgetZone) == 0))
                    {
                        ProductDetailsPromotionModel productDetailsPromoModel = PrepareProductDetailsPromoModel(additionalData);
                        if (productDetailsPromoModel.HasPromo)
                            return PartialView("_ProductDetailsPromotion", productDetailsPromoModel);
                    }
                    return new EmptyResult();

                case "productbox_addinfo_after":
                    ProductBoxPromotionModel productBoxPromoModel = PrepareProductBoxPromoModel(additionalData);
                    if (productBoxPromoModel.HasPromo)
                        return PartialView("_ProductBoxPromotion", productBoxPromoModel);
                    else
                        return new EmptyResult();

                default:
                    if (string.Compare(_widgetSettings.ProductPagePromoDetailsWidgetZone, widgetZone, true) == 0)
                        return GetProductPagePromoDetails(additionalData);
                    else
                        return GetBannerForWidgetZone(widgetZone, additionalData);
            }
        }
        
        private ActionResult GetBannerForWidgetZone(string widgetZone, object additionalData)
        {
            var allEnabledWidgetZones = _promoBannerService.RetrieveAllEnabledWidgetZones();
            if(!allEnabledWidgetZones.Any(wz => string.Compare(wz.WidgetZoneSystemName, widgetZone, true) == 0))
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
                                                               basketCriteriaChecks = GetBasketCriteriaItems();

                                                           validatedPromo = ValidatedPromo.Create(promo, productMappings, basketCriteriaChecks, productPromotionMappings);
                                                           validatedPromotions.Add(validatedPromo);
                                                       }

                                                       if (validatedPromo.ValidToDisplay)
                                                           bannerToDisplay.Pictures.Add(GetBannerPictureModel(ps, s.TransitionType));
                                                   }
                                               }
                                               else
                                               {
                                                   // If there isn't a promo reference - we'll have to always display it!
                                                   bannerToDisplay.Pictures.Add(GetBannerPictureModel(ps, s.TransitionType));
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

        private PromoBannerDisplayPictureModel GetBannerPictureModel(PromoBannerPicture promoBannerPicture, string transitionType)
        {
            var returnItem = new PromoBannerDisplayPictureModel()
            {
                Comment = promoBannerPicture.Comment,
                DisplaySequence = promoBannerPicture.DisplaySequence,
                PictureId = promoBannerPicture.PictureId,
                Url = promoBannerPicture.Url,
                TransitionType = transitionType
            };

            returnItem.PictureUrl = _pictureService.GetPictureUrl(returnItem.PictureId);
            return returnItem;
        }

        private ActionResult GetProductPagePromoDetails(object additionalData = null)
        {
            ProductDetailsPromotionModel productDetailsPromoModel = PrepareProductDetailsPromoModel(additionalData);
            if (productDetailsPromoModel.HasPromo)
                return PartialView("_ProductDetailsPromotion", productDetailsPromoModel);
            else
                return new EmptyResult();
        }

        #region utilities

        private ProductBoxPromotionModel PrepareProductBoxPromoModel(object additionalData)
        {
            ProductBoxPromotionModel model = new ProductBoxPromotionModel();
            model.HasPromo = false;

            if (_promoSettings.Enabled && _widgetSettings.ShowStickersInCatalogue)
            {
                int productId = 0;
                if (int.TryParse(additionalData.ToString(), out productId))
                {
                    var productValidation = ValidateProductForPromos(productId);
                    if (productValidation.HasPromo)
                    {
                        model.Id = productId;
                        model.HasPromo = true;
                        model.ImageUrl = GetSingleImageUrl(productValidation.PromosToDisplay, productValidation.BaseImageUrl);
                    }
                }
            }

            return model;
        }

        private ProductDetailsPromotionModel PrepareProductDetailsPromoModel(object additionalData)
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

            var productValidation = ValidateProductForPromos(productId);
            if (!productValidation.HasPromo)
                return model;

            var product = _productService.GetProductById(productId);
            if (product != null)
                model.HasTierPrices = product.HasTierPrices;

            model.Id = productId;
            model.ShowSticker = _widgetSettings.ShowStickersInProductPage;
            model.ShowPromotionDetails = _widgetSettings.ShowPromoDetailsOnProductPage;
            model.HasPromo = true;
            model.ImageUrl = GetSingleImageUrl(productValidation.PromosToDisplay, productValidation.BaseImageUrl);
            model.PromotionItems = productValidation.PromosToDisplay.Select(ptd => ptd.ToModel()).ToList();
            model.PromotionItems.ForEach(pi =>
                    {
                        // If the description is a resource key, this will get it, otherwise use the description.
                        pi.Description = _localizationService.GetValidatedResource(pi.Description);

                        pi.ImageUrl = GetPromoImageUrl(pi.YourReference, pi.PromotionTypeName, pi.ImageName);

                        if (pi.DiscountAmount.HasValue && pi.DiscountAmount.Value > 0)
                        {
                            var discountAmountForCurrency = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets
                                                                ? pi.DiscountAmount.Value
                                                                : _currencyService.ConvertFromPrimaryStoreCurrency(pi.DiscountAmount.Value, _workContext.WorkingCurrency);
                            pi.DiscountAmountAsCurrency = _priceFormatter.FormatPrice(discountAmountForCurrency, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, false);
                        }

                        if (pi.MinimumSpend.HasValue && pi.MinimumSpend.Value > 0)
                        {
                            var minimumSpendForCurrency = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets
                                                                ? pi.MinimumSpend.Value
                                                                : _currencyService.ConvertFromPrimaryStoreCurrency(pi.MinimumSpend.Value, _workContext.WorkingCurrency);
                            pi.MinimumSpendAsCurrency = _priceFormatter.FormatPrice(minimumSpendForCurrency, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, false);
                        }

                        if (pi.RequiredItemSpend.HasValue && pi.RequiredItemSpend.Value > 0)
                        {
                            var rqdItemSpendForCurrency = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets
                                                                ? pi.RequiredItemSpend.Value
                                                                : _currencyService.ConvertFromPrimaryStoreCurrency(pi.RequiredItemSpend.Value, _workContext.WorkingCurrency);
                            pi.RequiredItemSpendAsCurrency = _priceFormatter.FormatPrice(rqdItemSpendForCurrency, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, false);
                        }

                        pi.YouSaveText = "-";
                        pi.ShowFromText = false;
                        switch (pi.PromotionTypeName)
                        {
                            case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.BuyOneGetOneFree:
                                pi.YouSaveText = _localizationService.GetResource("Plugins.Misc.QixolPromo.Product.Promos.Item.GetOneFree");     //"Get One Free";
                                break;

                            case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.BuyOneGetOneReduced:
                            case Qixol.Nop.Promo.Core.Domain.PromotionTypeName.ProductsReduction:
                                if (pi.DiscountAmount.HasValue && pi.DiscountAmount.Value > 0)
                                {
                                    pi.YouSaveText = pi.DiscountAmountAsCurrency;
                                    pi.ShowFromText = product.HasTierPrices;
                                }
                                else
                                {
                                    if (pi.DiscountPercent.HasValue && pi.DiscountPercent.Value > 0)
                                    {
                                        pi.YouSaveText = string.Format("{#.##}%", pi.DiscountPercent.Value);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    });

            return model;
        }

        private string GetSingleImageUrl(List<ValidatedPromo> promosToDisplay, string baseUrl)
        {
            string returnUrl = baseUrl;
            if (promosToDisplay.Count == 1)
            {
                var promo = promosToDisplay.FirstOrDefault();
                returnUrl = GetPromoImageUrl(promo.YourReference, promo.PromoType, promo.ImageName);
            }
            else
            {
                var multiplePromosImage = _promoPictureService.RetrieveForPromo(string.Empty, PromotionTypeName.Multiple_Promos);
                if (multiplePromosImage != null)
                    returnUrl = _pictureService.GetPictureUrl(multiplePromosImage.PictureId);
            }
            return returnUrl;
        }

        private string GetPromoImageUrl(string promoReference, string promoType, string promoBaseImageName)
        {
            var promoImage = _promoPictureService.RetrieveForPromo(promoReference, promoType);
            if (promoImage != null)
                return _pictureService.GetPictureUrl(promoImage.PictureId);
            else
                return GetImageUrl(promoBaseImageName);
        }
        

        /// <summary>
        ///  For the product id specified, establish which promos could be triggered by retrieving saved promo details
        ///   and combining that with the known current basket level criteria (i.e. store, customer group), and for the current UTC time.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        private ValidatedProductPromoDetails ValidateProductForPromos(int productId)
        {
            var returnDetails = new ValidatedProductPromoDetails() { ProductId = productId };
            var mappings = _productMappingService.RetrieveAllVariantsByProductId(productId, EntityAttributeName.Product, true);

            if (mappings != null && mappings.Count() > 0)
            {
                var mappingIds = mappings.Select(m => m.Id).ToList();
                var productPromoMappings = _productPromoMappingService.RetrieveForProductMappingsList(mappingIds);
                if (productPromoMappings != null && productPromoMappings.Count() > 0)
                {
                    var promoIds = productPromoMappings.GroupBy(gb => gb.PromotionId).Select(s => s.FirstOrDefault().PromotionId).ToList();
                    if (promoIds != null && promoIds.Count > 0)
                    {
                        var basketCriteriaChecks = GetBasketCriteriaItems();
                        promoIds.ForEach(promoId =>
                        {
                            var promo = _promoDetailService.RetrieveByPromoId(promoId);
                            if (promo != null)
                            {
                                // Note have to put this in a variable otherwise it won't add to the list!!
                                var validatedPromo = ValidatedPromo.Create(promo, mappings, basketCriteriaChecks, productPromoMappings);
                                returnDetails.PromoDetails.Add(validatedPromo);
                            }                                
                        });
                    }
                    returnDetails.BaseImageUrl = GetImageUrl("promo_offers");
                }
            }

            return returnDetails;
        }

        /// <summary>
        /// Get the current basket criteria so we can revalidate promos to see if they are now available...
        /// </summary>
        /// <returns></returns>
        private List<KeyValuePair<string, string>> GetBasketCriteriaItems()
        {
            var basketCriteriaChecks = new List<KeyValuePair<string, string>>();

            // ** Current Store
            var allStores = _storeService.GetAllStores();
            if (allStores.Count > 1)
            {
                // If we have a store code, add it as a criteria
                var storeCode = _attributeValueService.Retrieve(_storeContext.CurrentStore.Id, EntityAttributeName.Store);
                if (storeCode != null && !string.IsNullOrEmpty(storeCode.Code))
                    basketCriteriaChecks.Add(new KeyValuePair<string, string>(EntityAttributeName.ToPromoAttributeName(EntityAttributeName.Store), storeCode.Code));
            }

            // ** Role
            if (_workContext.CurrentCustomer != null && _workContext.CurrentCustomer.CustomerRoles != null)
            {
                // The user can be in multiple roles, so use the Priority assigned to the Integration code to establish
                // which one to use.
                //
                var rolesList = _workContext.CurrentCustomer.CustomerRoles.ToList();
                var attribsList = _attributeValueService.RetrieveAllForAttribute(EntityAttributeName.CustomerRole).ToList();

                if (attribsList != null && attribsList.Count > 0)
                {
                    var selectedRole = (from r in rolesList
                                        join a in attribsList
                                            on r.Id equals a.AttributeValueId
                                        orderby a.Priority == null ? 0 : a.Priority descending
                                        select a).FirstOrDefault();

                    if (selectedRole != null)
                        basketCriteriaChecks.Add(new KeyValuePair<string, string>(EntityAttributeName.ToPromoAttributeName(EntityAttributeName.CustomerRole), selectedRole.Code));
                }
            }

            // ** Currency
            if (_workContext.WorkingCurrency != null)
                basketCriteriaChecks.Add(new KeyValuePair<string, string>(EntityAttributeName.ToPromoAttributeName(EntityAttributeName.Currency), _workContext.WorkingCurrency.CurrencyCode));                

            return basketCriteriaChecks;
        }

        private string GetImageUrl(string baseImageName)
        {
            return string.Format("/Plugins/Widgets.QixolPromo/Themes/{0}/Content/images/{1}.png", _themeContext.WorkingThemeName, baseImageName);
        }

        #endregion

    }
}
