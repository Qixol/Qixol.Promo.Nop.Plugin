using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Core.Domain.Promo;

using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Seo;
using Nop.Services.Media;
using Qixol.Plugin.Misc.Promo.Models.Checkout;
using global::Nop.Web.Models.Catalog;
using System.Xml.Linq;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Qixol.Plugin.Misc.Promo.Factories;
using Nop.Web.Factories;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    [NopHttpsRequirement(SslRequirement.Yes)]
    public partial class PromoCheckoutController : global::Nop.Web.Controllers.CheckoutController
    {
        #region Fields

        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IMissedPromotionsModelFactory _missedPromotionsModelFactory;

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly HttpContextBase _httpContext; 
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;

        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        #endregion

		#region Ctor

        public PromoCheckoutController(ICheckoutModelFactory checkoutModelFactory,
            IMissedPromotionsModelFactory missedPromotionsModelFactory,
            IWorkContext workContext,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService, 
            ILocalizationService localizationService, 
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService, 
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService, 
            IPaymentService paymentService,
            IPluginFinder pluginFinder,
            IOrderTotalCalculationService orderTotalCalculationService,
            ILogger logger,
            IOrderService orderService,
            IWebHelper webHelper,
            HttpContextBase httpContext,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            OrderSettings orderSettings, 
            RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings,
            CustomerSettings customerSettings,
            PromoSettings promoSettings,
            IPromoService promoService,
            IPromoUtilities promoUtilities)
            : base(checkoutModelFactory, workContext, storeContext,
                    shoppingCartService, localizationService,
                    orderProcessingService, customerService,
                    genericAttributeService, countryService, stateProvinceService,
                    shippingService, paymentService, pluginFinder,
                    orderTotalCalculationService, logger, orderService, webHelper, httpContext,
                    addressAttributeParser, addressAttributeService,
                    orderSettings, rewardPointsSettings,
                    paymentSettings, shippingSettings, addressSettings, customerSettings)
        {
            this._checkoutModelFactory = checkoutModelFactory;
            this._missedPromotionsModelFactory = missedPromotionsModelFactory;

            this._workContext = workContext;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._orderProcessingService = orderProcessingService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._pluginFinder = pluginFinder;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._logger = logger;
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._httpContext = httpContext;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;

            this._orderSettings = orderSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._addressSettings = addressSettings;
            this._customerSettings = customerSettings;

            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._promoUtilities = promoUtilities;
        }

        #endregion

        #region Utilities

        public ActionResult ContinueShopping()
        {
            var returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (String.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.RouteUrl("HomePage");
            }

            return Json(new
            {
                continue_shopping_url = returnUrl
            });
        }

        #endregion

        #region public action methods

        public ActionResult PromoIndex()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            //reset checkout data
            _customerService.ResetCheckoutData(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id);

            if (!_promoSettings.Enabled || !_promoSettings.ShowMissedPromotions)
            {
                if (_orderSettings.OnePageCheckoutEnabled)
                {
                    return RedirectToRoute("CheckoutOnePage");
                }
                else
                {
                    return RedirectToRoute("CheckoutBillingAddress");
                }
            }
            else
            {
                if (_orderSettings.OnePageCheckoutEnabled)
                {
                    return RedirectToRoute("PromoCheckoutOnePage");
                }
                else
                {
                    return RedirectToRoute("PromoCheckoutMissedPromotions");
                }
            }
        }

        public ActionResult MissedPromotions()
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (!_promoSettings.Enabled || !_promoSettings.ShowMissedPromotions)
                return RedirectToRoute("CheckoutBillingAddress");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            _promoService.ProcessShoppingCart(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id, true);

            var model = _missedPromotionsModelFactory.PrepareMissedPromotionsModel();

            model.ContinueShoppingUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (String.IsNullOrEmpty(model.ContinueShoppingUrl))
            {
                model.ContinueShoppingUrl = Url.RouteUrl("HomePage");
            }

            if (model == null || model.MissedPromotions.Count == 0)
                return RedirectToRoute("CheckoutBillingAddress");

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult PromoCheckoutProgress(PromoCheckoutProgressStep step, bool showMissedPromotions)
        {
            var model = new Models.Checkout.PromoCheckoutProgressModel { PromoCheckoutProgressStep = step, ShowMissedPromotions = showMissedPromotions };
            return PartialView("CheckoutProgress", model);
        }

        [ChildActionOnly]
        public ActionResult OpcMissedPromotionsForm()
        {
            var missedPromotionsModel = _missedPromotionsModelFactory.PrepareMissedPromotionsModel();
            missedPromotionsModel.ContinueShoppingUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (String.IsNullOrEmpty(missedPromotionsModel.ContinueShoppingUrl))
            {
                missedPromotionsModel.ContinueShoppingUrl = Url.RouteUrl("HomePage");
            }

            return PartialView("OpcMissedPromotions", missedPromotionsModel);
        }

        #endregion
    }
}
