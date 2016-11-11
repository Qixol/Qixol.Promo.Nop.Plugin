using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Controllers;
using Qixol.Nop.Promo.Services.Catalog;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Misc.Promo.Models.MissedPromotions;
using Nop.Web.Models.Catalog;
using Qixol.Promo.Integration.Lib.Basket;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public partial class MissedPromotionsController : global::Nop.Web.Controllers.BasePublicController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPromosPriceCalculationService _promosPriceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly HttpContextBase _httpContext;

        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        #endregion

        #region Constructors

        public MissedPromotionsController(IProductService productService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            IPromosPriceCalculationService promosPriceCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IOrderProcessingService orderProcessingService,
            IDiscountService discountService,
            ICustomerService customerService,
            IGiftCardService giftCardService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService,
            IPaymentService paymentService,
            IWorkflowMessageService workflowMessageService,
            IPermissionService permissionService,
            IDownloadService downloadService,
            ICacheManager cacheManager,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            HttpContextBase httpContext,
            MediaSettings mediaSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            CaptchaSettings captchaSettings,
            AddressSettings addressSettings,
            RewardPointsSettings rewardPointsSettings,
            PromoSettings promoSettings,
            IPromoService promoService,
            IPromoUtilities promoUtilities)
        {
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._promosPriceCalculationService = promosPriceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._orderProcessingService = orderProcessingService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._paymentService = paymentService;
            this._workflowMessageService = workflowMessageService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._genericAttributeService = genericAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._httpContext = httpContext;

            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._captchaSettings = captchaSettings;
            this._addressSettings = addressSettings;
            this._rewardPointsSettings = rewardPointsSettings;

            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._promoUtilities = promoUtilities;
        }

        #endregion

        #region Utilities

        // TODO: code copied from Product controller, PrepareProductDetailsPageModel method...
        private ProductDetailsModel.AddToCartModel PrepareAddToCartModel(ShoppingCartItem matchedCartItem)
        {
            ProductDetailsModel.AddToCartModel model = new ProductDetailsModel.AddToCartModel();
            var matchedProduct = _productService.GetProductById(matchedCartItem.ProductId);

            model.ProductId = matchedCartItem.ProductId;
            model.UpdatedShoppingCartItemId = matchedCartItem.Id;

            //quantity
            model.EnteredQuantity = matchedCartItem.Quantity;
            //allowed quantities
            var allowedQuantities = matchedProduct.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                model.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = matchedCartItem != null && matchedCartItem.Quantity == qty
                });
            }
            //minimum quantity notification
            if (matchedProduct.OrderMinimumQuantity > 1)
            {
                model.MinimumQuantityNotification = string.Format(_localizationService.GetResource("Products.MinimumQuantityNotification"), matchedProduct.OrderMinimumQuantity);
            }

            ////'add to cart', 'add to wishlist' buttons
            //model.AddToCart.DisableBuyButton = product.DisableBuyButton || !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            //model.AddToCart.DisableWishlistButton = product.DisableWishlistButton || !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            //if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            //{
            //    model.AddToCart.DisableBuyButton = true;
            //    model.AddToCart.DisableWishlistButton = true;
            //}
            ////pre-order
            if (matchedProduct.AvailableForPreOrder)
            {
                model.AvailableForPreOrder = !matchedProduct.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                    matchedProduct.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                model.PreOrderAvailabilityStartDateTimeUtc = matchedProduct.PreOrderAvailabilityStartDateTimeUtc;
            }
            //rental
            model.IsRental = matchedProduct.IsRental;

            //customer entered price
            model.CustomerEntersPrice = matchedProduct.CustomerEntersPrice;
            if (model.CustomerEntersPrice)
            {
                decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(matchedProduct.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(matchedProduct.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

                model.CustomerEnteredPrice = matchedCartItem.CustomerEnteredPrice;
                model.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
                    _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                    _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
            }

            return model;
        }

        private MissedPromotionBogofModel PrepareMissedPromotionBogofModel(BasketResponseMissedPromotion missedPromo, List<ShoppingCartItem> cart)
        {
            var missedBogofModel = new MissedPromotionBogofModel()
            {
                PromotionName = missedPromo.DisplayText
            };
            // TODO: It's a BOGOF - there can be only one...
            foreach (var ci in missedPromo.Criteria.CriteriaItems)
            {
                foreach (var item in ci.Items)
                {
                    item.MatchedLineIds.ForEach(i =>
                    {
                        ShoppingCartItem matchedCartItem = (from c in cart where c.Id == i select c).FirstOrDefault();
                        if (matchedCartItem != null)
                        {
                            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
                            {
                                Id = matchedCartItem.Id,
                                Sku = matchedCartItem.Product.FormatSku(matchedCartItem.AttributesXml, _productAttributeParser),
                                ProductId = matchedCartItem.Product.Id,
                                ProductName = matchedCartItem.Product.GetLocalized(x => x.Name),
                                ProductSeName = matchedCartItem.Product.GetSeName(),
                                Quantity = matchedCartItem.Quantity,
                                AttributeInfo = _productAttributeFormatter.FormatAttributes(matchedCartItem.Product, matchedCartItem.AttributesXml),
                            };
                            var shoppingCartController = DependencyResolver.Current.GetService<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();
                            cartItemModel.Picture = shoppingCartController.PrepareCartItemPictureModel(matchedCartItem, cartItemModel.ProductName);
                            missedBogofModel.MatchedCartItemModels.Add(cartItemModel);
                            missedBogofModel.AddToCartModel = PrepareAddToCartModel(matchedCartItem);
                        }
                    });
                }
            }
            return missedBogofModel;
        }

        private MissedPromotionBogorModel PrepareMissedPromotionBogorModel(BasketResponseMissedPromotion missedPromo, List<ShoppingCartItem> cart)
        {
            var missedBogorModel = new MissedPromotionBogorModel()
            {
                PromotionName = missedPromo.DisplayText
            };
            // TODO: It's a BOGOR - there can be only one...
            foreach (var ci in missedPromo.Criteria.CriteriaItems)
            {
                foreach (var item in ci.Items)
                {
                    item.MatchedLineIds.ForEach(i =>
                    {
                        ShoppingCartItem matchedCartItem = (from c in cart where c.Id == i select c).FirstOrDefault();
                        if (matchedCartItem != null)
                        {
                            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
                            {
                                Id = matchedCartItem.Id,
                                Sku = matchedCartItem.Product.FormatSku(matchedCartItem.AttributesXml, _productAttributeParser),
                                ProductId = matchedCartItem.Product.Id,
                                ProductName = matchedCartItem.Product.GetLocalized(x => x.Name),
                                ProductSeName = matchedCartItem.Product.GetSeName(),
                                Quantity = matchedCartItem.Quantity,
                                AttributeInfo = _productAttributeFormatter.FormatAttributes(matchedCartItem.Product, matchedCartItem.AttributesXml),
                            };
                            var shoppingCartController = DependencyResolver.Current.GetService<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();
                            cartItemModel.Picture = shoppingCartController.PrepareCartItemPictureModel(matchedCartItem, cartItemModel.ProductName);
                            missedBogorModel.MatchedCartItemModels.Add(cartItemModel);
                            missedBogorModel.AddToCartModel = PrepareAddToCartModel(matchedCartItem);
                        }
                    });
                }
            }

            return missedBogorModel;
        }

        private MissedPromotionUnknownModel PrepareMissedPromotionUnknownModel(BasketResponseMissedPromotion missedPromo, List<ShoppingCartItem> cart)
        {
            var missedUnknownPromo = new MissedPromotionUnknownModel()
            {
                PromotionName = missedPromo.DisplayText,
                PromotionImageUrl = "/Plugins/Misc.QixolPromo/Content/Images/default-missedpromotion.png",
                SaveFrom = missedPromo.Action.SaveFrom > decimal.Zero ? _priceFormatter.FormatPrice(missedPromo.Action.SaveFrom) : string.Empty
            };
            return missedUnknownPromo;
        }

        #endregion

        #region Missed Promotions

        public ActionResult MissedPromotions()
        {
            #region copied from Checkout/Index (except for call to PromoService)

            var currentCustomer = _workContext.CurrentCustomer;

            var cart = currentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((currentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //validation (cart)
            var checkoutAttributesXml = currentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            //var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            var scWarnings = _promoService.ProcessShoppingCart(true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            #endregion

            #region missed promotions

            var model = new MissedPromotionsModel();

            var basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || basketResponse.MissedPromotions.Count == 0)
            {
                return RedirectToRoute("Checkout");
            }

            foreach (var missedPromo in basketResponse.MissedPromotions)
            {
                // TODO: enum for PromotionType (from integration lib...?)
                switch (missedPromo.PromotionType)
                {
                    case MissedPromotionsModel.PromotionTypeSystemName.BuyOneGetOneFree:
                        model.MissedPromotions.Add(PrepareMissedPromotionBogofModel(missedPromo, cart));
                        break;
                    case MissedPromotionsModel.PromotionTypeSystemName.BuyOneGetOneReduced:
                        model.MissedPromotions.Add(PrepareMissedPromotionBogorModel(missedPromo, cart));
                        break;
                    default:
                        model.MissedPromotions.Add(PrepareMissedPromotionUnknownModel(missedPromo, cart));
                        break;
                }
            }
            return View(model);

            #endregion
        }

        #endregion
    }
}
