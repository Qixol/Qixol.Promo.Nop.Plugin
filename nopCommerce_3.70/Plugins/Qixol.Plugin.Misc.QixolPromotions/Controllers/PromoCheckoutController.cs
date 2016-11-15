using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Core.Domain.Promo;

using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Seo;
using Nop.Services.Media;
using Qixol.Plugin.Misc.Promo.Models.Checkout;
using global::Nop.Web.Models.Catalog;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    [NopHttpsRequirement(SslRequirement.Yes)]
    public partial class PromoCheckoutController : global::Nop.Web.Controllers.CheckoutController
    {
		#region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IRewardPointService _rewardPointService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly HttpContextBase _httpContext; 
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        

        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly AddressSettings _addressSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;

        #endregion

        #region Constructors

        public PromoCheckoutController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IShoppingCartService shoppingCartService, 
            ILocalizationService localizationService, 
            ITaxService taxService, 
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService, 
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService, 
            IPaymentService paymentService,
            IPluginFinder pluginFinder,
            IOrderTotalCalculationService orderTotalCalculationService,
            IRewardPointService rewardPointService,
            ILogger logger,
            IOrderService orderService,
            IWebHelper webHelper,
            HttpContextBase httpContext,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            OrderSettings orderSettings, 
            RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings,
            PromoSettings promoSettings,
            IPromoService promoService,
            IPromoUtilities promoUtilities,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductService productService)
            : base (workContext, storeContext, storeMappingService,
                    shoppingCartService, localizationService, taxService, currencyService,
                    priceFormatter, orderProcessingService, customerService,
                    genericAttributeService, countryService, stateProvinceService,
                    shippingService, paymentService, pluginFinder,
                    orderTotalCalculationService, rewardPointService, logger, orderService, webHelper, httpContext,
                    addressAttributeParser, addressAttributeService,
                    addressAttributeFormatter, orderSettings, rewardPointsSettings,
                    paymentSettings, shippingSettings, addressSettings)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._shoppingCartService = shoppingCartService;
            this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._pluginFinder = pluginFinder;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._rewardPointService = rewardPointService;
            this._logger = logger;
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._httpContext = httpContext;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;

            this._orderSettings = orderSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._addressSettings = addressSettings;

            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._promoUtilities = promoUtilities;

            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productService = productService;
        }

        #endregion

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

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("PromoCheckoutOnePage");

            //validation (cart)
            var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            //var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            var scPromoWarnings = _promoService.ProcessShoppingCart(true);
            if (scPromoWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            // TODO: Native nop cart validation - is this still required?
            var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");
            //validation (each shopping cart item)
            foreach (ShoppingCartItem sci in cart)
            {
                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
                    sci.ShoppingCartType,
                    sci.Product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false);
                if (sciWarnings.Count > 0)
                    return RedirectToRoute("ShoppingCart");
            }

            return RedirectToRoute("PromoCheckoutMissedPromotions");
        }

        public ActionResult PromoOnePageCheckout()
        {
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (!_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("Checkout");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var scPromoWarnings = _promoService.ProcessShoppingCart(true);
            if (scPromoWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            var model = new OnePageCheckoutModel
            {
                ShippingRequired = cart.RequiresShipping(),
                DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep
            };
            return View("OnePageCheckout", model);
        }

        [ChildActionOnly]
        public ActionResult PromoCheckoutProgress(PromoCheckoutProgressStep step)
        {
            var model = new Models.Checkout.PromoCheckoutProgressModel { PromoCheckoutProgressStep = step };
            return PartialView(model);
        }

        [NonAction]
        protected override CheckoutShippingMethodModel PrepareShippingMethodModel(IList<ShoppingCartItem> cart, Address shippingAddress)
        {
            var model = new CheckoutShippingMethodModel();

            var getShippingOptionResponse = _shippingService
                .GetShippingOptions(cart, shippingAddress,
                "", _storeContext.CurrentStore.Id);
            if (getShippingOptionResponse.Success)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                                                       SystemCustomerAttributeNames.OfferedShippingOptions,
                                                       getShippingOptionResponse.ShippingOptions,
                                                       _storeContext.CurrentStore.Id);

                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodModel.ShippingMethodModel
                    {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                        ShippingOption = shippingOption,
                    };

                    //adjust rate
                    Discount appliedDiscount = null;
                    decimal shippingTotal = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, cart, out appliedDiscount);

                    if (_promoSettings.Enabled)
                        _promoService.ProcessShoppingCart();

                    BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

                    if (basketResponse != null && basketResponse.Summary != null && basketResponse.Summary.ProcessingResult)
                    {
                        var deliveryPromo = basketResponse.DeliveryPromo();
                        if (deliveryPromo != null)
                        {
                            shippingTotal = basketResponse.DeliveryPrice; 
                        }
                    }

                    decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                    decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase,
                                                                                    _workContext.WorkingCurrency);
                    soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

                    model.ShippingMethods.Add(soModel);
                }

                //find a selected (previously) shipping method
                var selectedShippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(
                        SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                if (selectedShippingOption != null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.ToList()
                        .Find(so =>
                            !String.IsNullOrEmpty(so.Name) &&
                            so.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
                            !String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
                            so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }
                //if no option has been selected, let's do it for the first one
                if (model.ShippingMethods.FirstOrDefault(so => so.Selected) == null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }

                //notify about shipping from multiple locations
                if (_shippingSettings.NotifyCustomerAboutShippingFromMultipleLocations)
                {
                    model.NotifyCustomerAboutShippingFromMultipleLocations = getShippingOptionResponse.ShippingFromMultipleLocations;
                }
            }
            else
            {
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);
            }

            return model;
        }
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
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if ((_workContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
                return new HttpUnauthorizedResult();

            var scWarnings = _promoService.ProcessShoppingCart(true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            var model = PrepareMissedPromotionsModel();

            if (model == null || model.MissedPromotions.Count == 0)
                return RedirectToRoute("CheckoutBillingAddress");

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OpcMissedPromotionsForm()
        {
            var missedPromotionsModel = PrepareMissedPromotionsModel();
            return PartialView("OpcMissedPromotions", missedPromotionsModel);
        }

        private MissedPromotionsModel PrepareMissedPromotionsModel()
        {
            var model = new MissedPromotionsModel();

            model.ContinueShoppingUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (String.IsNullOrEmpty(model.ContinueShoppingUrl))
            {
                model.ContinueShoppingUrl = Url.RouteUrl("HomePage");
            }

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (cart.Count == 0)
                return model;

            var basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || basketResponse.MissedPromotions.Count == 0)
            {
                return model;
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

            return model;
        }

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
    }
}
