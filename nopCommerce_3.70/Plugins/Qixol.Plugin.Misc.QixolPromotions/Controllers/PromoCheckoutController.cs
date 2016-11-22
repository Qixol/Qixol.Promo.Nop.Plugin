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
using System.Xml.Linq;

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

            ////validation (cart)
            //var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            ////var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            //var scPromoWarnings = _promoService.ProcessShoppingCart(true);
            //if (scPromoWarnings.Count > 0)
            //    return RedirectToRoute("ShoppingCart");

            //// TODO: Native nop cart validation - is this still required?
            //var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            //if (scWarnings.Count > 0)
            //    return RedirectToRoute("ShoppingCart");
            ////validation (each shopping cart item)
            //foreach (ShoppingCartItem sci in cart)
            //{
            //    var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer,
            //        sci.ShoppingCartType,
            //        sci.Product,
            //        sci.StoreId,
            //        sci.AttributesXml,
            //        sci.CustomerEnteredPrice,
            //        sci.RentalStartDateUtc,
            //        sci.RentalEndDateUtc,
            //        sci.Quantity,
            //        false);
            //    if (sciWarnings.Count > 0)
            //        return RedirectToRoute("ShoppingCart");
            //}

            //return RedirectToRoute("PromoCheckoutMissedPromotions");
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

            var scWarnings = _promoService.ProcessShoppingCart(true);
            if (scWarnings.Count > 0)
                return RedirectToRoute("ShoppingCart");

            var model = PrepareMissedPromotionsModel();

            if (model == null || model.MissedPromotions.Count == 0)
                return RedirectToRoute("CheckoutBillingAddress");

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult PromoCheckoutProgress(PromoCheckoutProgressStep step, bool showMissedPromotions)
        {
            var model = new Models.Checkout.PromoCheckoutProgressModel { PromoCheckoutProgressStep = step,  ShowMissedPromotions = showMissedPromotions };
            return PartialView("CheckoutProgress", model);
        }

        [ChildActionOnly]
        public ActionResult OpcMissedPromotionsForm()
        {
            var missedPromotionsModel = PrepareMissedPromotionsModel();
            return PartialView("OpcMissedPromotions", missedPromotionsModel);
        }

        #endregion

        #region Utilities

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
                        MissedPromotionBogofModel missedPromotionBogofModel = new MissedPromotionBogofModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionBogofModel));
                        break;
                    case MissedPromotionsModel.PromotionTypeSystemName.BuyOneGetOneReduced:
                        MissedPromotionBogorModel missedPromotionBogorModel = new MissedPromotionBogorModel();
                        var bogofModel = PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionBogorModel);
                        model.MissedPromotions.Add(bogofModel);
                        break;
                    case MissedPromotionsModel.PromotionTypeSystemName.DealPrice:
                        MissedPromotionDealModel missedPromotionDealModel = new MissedPromotionDealModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionDealModel));
                        break;
                    default:
                        MissedPromotionUnknownModel missedPromotionUnknownModel = new MissedPromotionUnknownModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionUnknownModel));
                        break;
                }
            }

            return model;
        }

        private MissedPromotionBaseModel PrepareMissedPromotionModel(BasketResponseMissedPromotion missedPromotion, List<ShoppingCartItem> cart, BasketResponse basketResponse, MissedPromotionBaseModel missedPromotionModel)
        {
            string promoDisplayDetails = string.Empty;
            switch (_promoSettings.ShowPromotionDetailsInBasket)
            {
                case PromotionDetailsDisplayOptions.ShowEndUserText:
                    // The display text is not mandatory, so default it to the promotion type if there is no text.
                    if (!string.IsNullOrEmpty(missedPromotion.DisplayText))
                        promoDisplayDetails = missedPromotion.DisplayText;
                    else
                        promoDisplayDetails = missedPromotion.PromotionTypeDisplay;
                    break;

                case PromotionDetailsDisplayOptions.ShowPromotionName:
                    promoDisplayDetails = missedPromotion.PromotionName;
                    break;

                case PromotionDetailsDisplayOptions.ShowNoText:
                    promoDisplayDetails = string.Empty;
                    break;

                default:
                    promoDisplayDetails = missedPromotion.PromotionTypeDisplay;
                    break;
            }
            missedPromotionModel.PromotionName = promoDisplayDetails;
            missedPromotionModel.PromotionImageUrl = "/Plugins/Misc.QixolPromo/Content/Images/default-missedpromotion.png";
            missedPromotionModel.SaveFrom = missedPromotion.Action.SaveFrom > decimal.Zero ? _priceFormatter.FormatPrice(missedPromotion.Action.SaveFrom) : string.Empty;

            var fullyMatchedCriteria = (from mc in missedPromotion.Criteria.CriteriaItems where mc.FullyMatched select mc).ToList();
            var partiallyMatchedCriteria = (from mc in missedPromotion.Criteria.CriteriaItems where !mc.FullyMatched select mc).ToList();

            var shoppingCartController = DependencyResolver.Current.GetService<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();

            foreach (var fullyMatchedCriterium in fullyMatchedCriteria)
            {
                var matchedItems = (from fmc in fullyMatchedCriterium.Items where fmc.IsMatched select fmc).ToList();
                if (matchedItems != null)
                {
                    foreach (var matchedItem in matchedItems)
                    { 
                        var basketResponseItem = (from bri in basketResponse.Items where bri.Id == matchedItem.MatchedLineIds.FirstOrDefault() select bri).FirstOrDefault();
                        if (basketResponseItem != null)
                        {

                            int originalCartItemId = basketResponseItem.Generated ? basketResponseItem.SplitFromLineId : basketResponseItem.Id;
                            ShoppingCartItem matchedCartItem = (from c in cart where c.Id == originalCartItemId select c).FirstOrDefault();
                            if (matchedCartItem == null)
                            {
                                throw new ArgumentOutOfRangeException("Missed Promotion Matched Item Line not found in cart");
                            }

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
                            cartItemModel.Picture = shoppingCartController.PrepareCartItemPictureModel(matchedCartItem, cartItemModel.ProductName);
                            missedPromotionModel.MatchedCartItemModels.Add(cartItemModel);
                        }
                    }
                }
            }

            var promoProductController = DependencyResolver.Current.GetService<PromoProductController>();
            foreach (var partiallyMatchedCriterium in partiallyMatchedCriteria)
            {
                foreach (var i in partiallyMatchedCriterium.Items)
                {
                    #region shopping cart items

                    var basketResponseItem = (from bri in basketResponse.Items where bri.Id == partiallyMatchedCriterium.Items.FirstOrDefault().MatchedLineIds.FirstOrDefault() select bri).FirstOrDefault();

                    if (basketResponseItem != null)
                    {
                        int originalCartItemId = basketResponseItem.Generated ? basketResponseItem.SplitFromLineId : basketResponseItem.Id;
                        ShoppingCartItem matchedCartItem = (from c in cart where c.Id == originalCartItemId select c).FirstOrDefault();
                        if (matchedCartItem == null)
                        {
                            throw new ArgumentOutOfRangeException("Missed Promotion Matched Item Line not found in cart");
                        }

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
                        cartItemModel.Picture = shoppingCartController.PrepareCartItemPictureModel(matchedCartItem, cartItemModel.ProductName);
                        missedPromotionModel.MatchedCartItemModels.Add(cartItemModel);

                        if (missedPromotionModel.GetType() == typeof(MissedPromotionBogofModel) || missedPromotionModel.GetType() == typeof(MissedPromotionBogorModel))
                        {
                            var product = _productService.GetProductById(cartItemModel.ProductId);

                            if (product != null && matchedCartItem != null)
                            {
                                var productDetailsModel = promoProductController.PromoPrepareProductDetailsModel(product);
                                productDetailsModel.ProductDetailsModel.AddToCart.UpdatedShoppingCartItemId = cartItemModel.Id;
                                productDetailsModel.ProductDetailsModel.AddToCart.AllowedQuantities = new List<SelectListItem>() { new SelectListItem() { Text = "1", Selected = true, Value = "1" } };
                                productDetailsModel.Attributes = ParseAttributeXml(matchedCartItem.AttributesXml);

                                missedPromotionModel.UnmatchedProductDetailsModels.Add(productDetailsModel);
                            }
                        }
                    }

                    #endregion

                    #region missed product details

                    int productId = 0;
                    if (int.TryParse(i.ProductCode, out productId))
                    {
                        var variantCode = i.VariantCode;

                        var product = _productService.GetProductById(productId);

                        if (product != null)
                        {
                            var productDetailsModel = promoProductController.PromoPrepareProductDetailsModel(product);
                            missedPromotionModel.UnmatchedProductDetailsModels.Add(productDetailsModel);
                        }
                    }

                    #endregion
                }
            }

            return missedPromotionModel;
        }

        private IDictionary<string, string> ParseAttributeXml(string attributeXml)
        {
            IDictionary<string, string> attributesList = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(attributeXml))
                return attributesList;

            var xmlDoc = XDocument.Parse(attributeXml);

            var attributes = xmlDoc.Descendants().Where(d => d.Name == "Attributes").ToList();
            foreach (var a in attributes)
            {
                var productAttributes = (from pa in a.Descendants() where pa.Name == "ProductAttribute" select pa).ToList();

                foreach (var productAttribute in productAttributes)
                {
                    var id = productAttribute.Attributes().Where(attrib => attrib.Name == "ID").FirstOrDefault().Value;

                    var productAttributeValue = (from pav in productAttribute.Descendants() where pav.Name == "ProductAttributeValue" select pav).FirstOrDefault();
                    var value = productAttributeValue.Descendants().Where(v => v.Name == "Value").FirstOrDefault().Value;

                    attributesList.Add(id, value);
                }
            }

            return attributesList;

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
