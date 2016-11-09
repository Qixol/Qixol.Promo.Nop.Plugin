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
    public partial class ShoppingCartController : global::Nop.Web.Controllers.ShoppingCartController
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

        public ShoppingCartController(IProductService productService,
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
            : base(productService, storeContext, workContext,
                    shoppingCartService, pictureService, localizationService,
                    productAttributeService, productAttributeFormatter,
                    productAttributeParser, taxService, currencyService,
                    priceCalculationService, priceFormatter, checkoutAttributeParser,
                    checkoutAttributeFormatter, orderProcessingService, discountService,
                    customerService, giftCardService, countryService,
                    stateProvinceService, shippingService, orderTotalCalculationService,
                    checkoutAttributeService, paymentService, workflowMessageService,
                    permissionService, downloadService, cacheManager, webHelper,
                    customerActivityService, genericAttributeService,
                    addressAttributeFormatter, httpContext, mediaSettings,
                    shoppingCartSettings, catalogSettings, orderSettings, shippingSettings,
                    taxSettings, captchaSettings, addressSettings,
                    rewardPointsSettings)
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

        internal void PromoParseAndSaveCheckoutAttributes(List<ShoppingCartItem> cart, FormCollection form)
        {
            base.ParseAndSaveCheckoutAttributes(cart, form);
        }

        #endregion

        #region Utilities

        public PictureModel PrepareCartItemPictureModel(ShoppingCartItem shoppingCartItem, string productName)
        {
            return base.PrepareCartItemPictureModel(shoppingCartItem, _mediaSettings.CartThumbPictureSize, true, productName);
        }

        /// <summary>
        /// Prepare shopping cart model
        /// </summary>
        /// <param name="model">Model instance</param>
        /// <param name="cart">Shopping cart</param>
        /// <param name="isEditable">A value indicating whether cart is editable</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether we should validate checkout attributes when preparing the model</param>
        /// <param name="prepareEstimateShippingIfEnabled">A value indicating whether we should prepare "Estimate shipping" model</param>
        /// <param name="setEstimateShippingDefaultAddress">A value indicating whether we should prefill "Estimate shipping" model with the default customer address</param>
        /// <param name="prepareAndDisplayOrderReviewData">A value indicating whether we should prepare review data (such as billing/shipping address, payment or shipping data entered during checkout)</param>
        /// <returns>Model</returns>
        [NonAction]
        protected override void PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false)
        {

            if (_promoSettings.Enabled)
            {
                List<string> cartWarnings = _promoService.ProcessShoppingCart();
                // refresh the cart, in case any changes were made
                cart = (from cartItem in _workContext.CurrentCustomer.ShoppingCartItems where cartItem.ShoppingCartType.Equals(ShoppingCartType.ShoppingCart) select cartItem).ToList();
                foreach (string cartWarning in cartWarnings)
                {
                    model.Warnings.Add(cartWarning);
                }
            }

            // Get the base to do most of the work...
            base.PrepareShoppingCartModel(model, cart, isEditable, validateCheckoutAttributes, prepareEstimateShippingIfEnabled, setEstimateShippingDefaultAddress, prepareAndDisplayOrderReviewData);

            if (_promoSettings.Enabled)
            {
                var basketResponse = _promoUtilities.GetBasketResponse();
                if (basketResponse.IsValid())
                {
                    if (basketResponse.TotalDiscount != decimal.Zero)
                    {
                        cart.Where(sci => !sci.Product.CallForPrice)
                            .ToList()
                            .ForEach(sci =>
                            {
                                var cartItemModel = model.Items.Where(mi => mi.Id == sci.Id).FirstOrDefault();

                                //sub total
                                Discount scDiscount;
                                decimal shoppingCartItemDiscountBase;
                                decimal taxRate;
                                decimal tempSubTotal = _promosPriceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscount);
                                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, tempSubTotal, out taxRate);
                                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                                cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                                //display an applied discount amount
                                if (scDiscount != null)
                                {
                                    shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                                    if (shoppingCartItemDiscountBase > decimal.Zero)
                                    {
                                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                                    }
                                }
                            });
                    }

                    model.DiscountBox.Message = string.Empty;
                    model.DiscountBox.CurrentCode = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode);
                    if (!string.IsNullOrEmpty(model.DiscountBox.CurrentCode))
                    {
                        if (basketResponse.CouponIsValid(model.DiscountBox.CurrentCode))
                        {
                            model.DiscountBox.IsApplied = true;
                            model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                        }
                        else
                        {
                            model.DiscountBox.IsApplied = false;
                            model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                        }
                    }
                }
            }
        }

        #endregion

        #region Shopping Cart

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("missed-promotions")]
        public ActionResult MissedPromotions(FormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //validate attributes
            var checkoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Count > 0)
            {
                //something wrong, redisplay the page with warnings
                var model = new ShoppingCartModel();
                PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

            //everything is OK
            if (!_promoSettings.ShowMissedPromotions)
            {
                if (_workContext.CurrentCustomer.IsGuest())
                {
                    if (!_orderSettings.AnonymousCheckoutAllowed)
                        return new HttpUnauthorizedResult();

                    return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
                }

                return RedirectToRoute("Checkout");
            }

            // TODO: confirm flow if not logged in - show Missed promotions or force log in before showing missed promotions?
            if (_workContext.CurrentCustomer.IsGuest())
            {
                if (!_orderSettings.AnonymousCheckoutAllowed)
                    return new HttpUnauthorizedResult();

                // TODO: not redirecting to the route name at present
                //return RedirectToRoute("LoginMissedPromotionsAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
                RouteValueDictionary rvd = new RouteValueDictionary();
                rvd.Add("area", null);
                rvd.Add("returnUrl", "ShoppingCart");
                return RedirectToAction("Login", "Customer", rvd);
            }

            //return RedirectToRoute("MissedPromotions");
            return RedirectToAction("MissedPromotions", "MissedPromotions");
        }

        [ChildActionOnly]
        public ActionResult PromoOrderSummary(bool? prepareAndDisplayOrderReviewData)
        {
            return base.OrderSummary(prepareAndDisplayOrderReviewData);
        }

        #endregion
    }
}
