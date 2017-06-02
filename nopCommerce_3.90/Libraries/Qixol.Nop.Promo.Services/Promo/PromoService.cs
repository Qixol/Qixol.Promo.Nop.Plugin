using System;
using System.Collections.Generic;
using System.Linq;
using global::Nop.Core;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Directory;
using global::Nop.Core.Domain.Stores;
using global::Nop.Services.Catalog;
using global::Nop.Services.Common;
using global::Nop.Services.Configuration;
using global::Nop.Services.Directory;
using global::Nop.Services.Media;
using global::Nop.Services.Tax;
using global::Nop.Services.Logging;
using Qixol.Nop.Promo.Core.Domain;
using global::Nop.Services.Orders;
using global::Nop.Services.Stores;
using global::Nop.Services.Shipping;
using global::Nop.Core.Domain.Shipping;
using Qixol.Nop.Promo.Core.Domain.Products;
using System.ServiceModel;
using Qixol.Nop.Promo.Core.Domain.Promo;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Orders;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Promo.Integration.Lib;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Services.ShoppingCart;
using Nop.Services.Customers;
using Nop.Services.Localization;

namespace Qixol.Nop.Promo.Services.Promo
{
    public class promoService : IPromoService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ILogger _logger;
        private readonly IStoreService _storeService;
        private readonly IShippingService _shippingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IProductMappingService _productMappingService;
        private readonly IPromoUtilities _promoUtilities;
        private readonly PromoSettings _promoSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region constructor

        public promoService(
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            ICurrencyService currencyService,
            ISettingService settingService,
            IWorkContext workContext,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            CurrencySettings currencySettings,
            IProductAttributeService productAttributeService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ILogger logger,
            IStoreService storeService,
            IPromoUtilities promoUtilities,
            PromoSettings promoSettings,
            IShippingService shippingService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IProductMappingService productMappingService,
            IShoppingCartService shoppingCartService,
            IAttributeValueService attributeValueService,
            IOrderService orderService,
            ICustomerService customerService,
            ILocalizationService localizationService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._currencyService = currencyService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._currencySettings = currencySettings;
            this._productAttributeService = productAttributeService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._logger = logger;
            this._promoUtilities = promoUtilities;
            this._storeService = storeService;
            this._shippingService = shippingService;
            this._genericAttributeService = genericAttributeService;
            this._storeContext = storeContext;
            this._productMappingService = productMappingService;
            this._shoppingCartService = shoppingCartService;
            this._attributeValueService = attributeValueService;
            this._orderService = orderService;
            this._promoSettings = promoSettings;
            this._customerService = customerService;
            this._localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private Currency GetUsedCurrency()
        {
            Currency currency = null; // _currencyService.GetCurrencyById(_factFinderProductFeedSettings.CurrencyId);
            if (currency == null || !currency.Published)
                currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            return currency;
        }

        #endregion

        #region Methods

        #region basket promos methods

        public List<string> ProcessShoppingCart()
        {
            return ProcessShoppingCart(false, null);
        }

        public List<string> ProcessShoppingCart(ShippingOption shippingOption = null)
        {
            return ProcessShoppingCart(false, shippingOption);
        }
        public List<string> ProcessShoppingCart(bool getMissedPromotions = false)
        {
            return ProcessShoppingCart(getMissedPromotions, null);
        }

        public List<string> ProcessShoppingCart(bool getMissedPromotions = false, ShippingOption shippingOption = null)
        {
            var addToCartWarnings = new List<string>();
            Customer customer = _workContext.CurrentCustomer;

            IList<ShoppingCartItem> cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            // If there are no items in the cart, do not process
            if (cart.Count < 1)
                return addToCartWarnings;

            try
            {
                BasketRequest basketRequest = cart.ToQixolPromosBasketRequest();


                if (basketRequest != null)
                {
                    basketRequest.GetMissedPromotions = getMissedPromotions;

                    if (shippingOption != null)
                    {
                        basketRequest = basketRequest.SetShipping(cart, shippingOption);
                    }

                    BasketResponse basketResponse = SendBasketRequestTopromoService(basketRequest);
                    if ((basketResponse != null) &&
                        (basketResponse.Summary != null) &&
                        (basketResponse.Summary.ProcessingResult))
                    {
                        // get the selectedShippingOption - if it's not null will need to save this after adding products to the cart
                        ShippingOption selectedShippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        string selectedPaymentOption = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);

                        #region generated basket items

                        IList<BasketResponseItem> newBasketItems = (from i in basketResponse.Items where i.Generated && !i.IsDelivery select i).ToList();

                        foreach (BasketResponseItem newBasketItem in newBasketItems)
                        {
                            int productId = 0;
                            if (!int.TryParse(newBasketItem.ProductCode, out productId))
                            {
                                // Do we have a checkout attribute?
                                var checkoutAttributes = _attributeValueService.RetrieveAllForAttribute(EntityAttributeName.CheckoutAttribute);
                                var checkoutAttribute = (from ca in checkoutAttributes where ca.Code.Equals((newBasketItem.ProductCode ?? string.Empty), StringComparison.InvariantCultureIgnoreCase) select ca).FirstOrDefault();
                                if (checkoutAttribute == null)
                                    throw new KeyNotFoundException(string.Format("No mapping item for product code {0}", newBasketItem.ProductCode));

                                // is the checkout attribute already "in" the cart
                                // if it's a single select (not checkbox?) then how do we "remove" the current one and replace it?
                                // what about when the price adjustment value has been changed by the promo engine...?
                                // 
                            }

                            else
                            {
                                if (newBasketItem.SplitFromLineId != 0)
                                {
                                    // The item has not been added - we need to roll it back into the original line with the discount - happens in GetSubTotal in the PriceCalculationService
                                    ShoppingCartItem sci = (from c in cart where c.Id == newBasketItem.SplitFromLineId select c).FirstOrDefault();
                                    if (sci == null)
                                        throw new NullReferenceException(string.Format("SplitFromLineId {0} does not exist in the cart", newBasketItem.SplitFromLineId));
                                }
                                else
                                {
                                    Product product = _productService.GetProductById(productId);

                                    string attributesXml = string.Empty;

                                    ProductMappingItem productMappingItem = _productMappingService.RetrieveFromVariantCode(productId, newBasketItem.VariantCode);
                                    if (productMappingItem != null && !productMappingItem.NoVariants)
                                    {
                                        attributesXml = productMappingItem.AttributesXml;
                                    }

                                    // Add the new item - any additional items were deleted before sending the basket to the promo engine

                                    var cartType = ShoppingCartType.ShoppingCart;

                                    // TODO: Customer entered price...?
                                    decimal customerEnteredPriceConverted = decimal.Zero;
                                    // TODO: rental start/end dates
                                    DateTime? rentalStartDate = null;
                                    DateTime? rentalEndDate = null;

                                    int quantity = int.Parse(newBasketItem.Quantity.ToString());

                                    List<string> itemAddToCartWarnings = new List<string>();
                                    //add to the cart
                                    itemAddToCartWarnings.AddRange(_shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                                        product, cartType, _storeContext.CurrentStore.Id,
                                        attributesXml, customerEnteredPriceConverted,
                                        rentalStartDate, rentalEndDate, quantity, true));

                                    if (itemAddToCartWarnings.Count > 0)
                                    {
                                        string cartWarningTemplate = _localizationService.GetResource("Plugin.Misc.QixolPromo.ShoppingCart.AddItemWarning", _workContext.WorkingLanguage.Id);
                                        string cartWarningMessage = string.Format(cartWarningTemplate, product.Name);
                                        addToCartWarnings.Add(cartWarningMessage);
                                        _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Error, cartWarningMessage, string.Join(", ", itemAddToCartWarnings.ToArray()), customer);
                                    }
                                }
                            }
                        }

                        #endregion

                        if (selectedShippingOption != null)
                            _genericAttributeService.SaveAttribute<ShippingOption>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, selectedShippingOption, _storeContext.CurrentStore.Id);

                        if (!string.IsNullOrEmpty(selectedPaymentOption))
                            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPaymentMethod, selectedPaymentOption, _storeContext.CurrentStore.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("ProcessShoppingCart", ex, _workContext.CurrentCustomer);
                // TODO: show the customer an error message?
                // addToCartWarnings.Add(ex.Message);
            }

            return addToCartWarnings;
        }

        public void SendConfirmedBasket(global::Nop.Core.Domain.Orders.Order placedOrder)
        {
            var customer = _workContext.CurrentCustomer;
            BasketRequest basketRequest = BasketRequest.FromXml(customer.GetAttribute<string>(PromoCustomerAttributeNames.PromoBasketRequest, _storeContext.CurrentStore.Id));
            basketRequest.Confirmed = true;

            #region Custom Attributes

            List<BasketRequestCustomAttribute> customAttributes = new List<BasketRequestCustomAttribute>();

            // order id
            customAttributes.Add(new BasketRequestCustomAttribute()
            {
                Name = "orderid",
                Value = placedOrder.Id.ToString()
            });

            // Reward points
            if ((placedOrder.RedeemedRewardPointsEntry != null) && (placedOrder.RedeemedRewardPointsEntry.Points > 0))
            {
                customAttributes.Add(new BasketRequestCustomAttribute()
                {
                    Name = "rewardpointswereused",
                    Value = "true"
                });

                customAttributes.Add(new BasketRequestCustomAttribute()
                {
                    Name = "rewardpointsused",
                    Value = placedOrder.RedeemedRewardPointsEntry.Points.ToString()
                });

                customAttributes.Add(new BasketRequestCustomAttribute()
                {
                    Name = "rewardpointsvalue",
                    Value = _currencyService.ConvertCurrency(placedOrder.RedeemedRewardPointsEntry.UsedAmount, placedOrder.CurrencyRate).ToString()
                });
            }

            // Gift cards
            if ((placedOrder.GiftCardUsageHistory != null) && (placedOrder.GiftCardUsageHistory.Sum(g => g.UsedValue) > decimal.Zero))
            {
                customAttributes.Add(new BasketRequestCustomAttribute()
                {
                    Name = "giftcardswereused",
                    Value = "true"
                });

                customAttributes.Add(new BasketRequestCustomAttribute()
                {
                    Name = "giftcardsusedvalue",
                    Value = placedOrder.GiftCardUsageHistory.Sum(g => g.UsedValue).ToString()
                });
            }

            if (_promoSettings.UseSelectedCurrencyWhenSubmittingBaskets)
                customAttributes.Add(new BasketRequestCustomAttribute() { Name = "incustomercurrency", Value = "True" });

            basketRequest.CustomAttributes.AddRange(customAttributes);

            #endregion

            BasketResponse basketResponse = SendBasketRequestTopromoService(basketRequest);

            if (basketResponse == null)
                throw new NopException("sending confirmed basket failed");
        }

        private BasketResponse SendBasketRequestTopromoService(BasketRequest basketRequest)
        {
            try
            {
                BasketServiceManager basketServiceManager = _promoSettings.GetBasketService();

                if (_promoSettings.LogMessages)
                {
                    var serializedBasketRequestData = basketRequest.ToXml();
                    _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Information, "Qixol Promos basket request", serializedBasketRequestData, _workContext.CurrentCustomer);
                }

                _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, PromoCustomerAttributeNames.PromoBasketResponse, null, _storeContext.CurrentStore.Id);

                BasketResponse basketResponse = basketServiceManager.SubmitBasket(basketRequest);

                // Grab from the request whether this basket is being submitted in customer currency or not.  This will then be serialized and saved with the basket response.
                var inCustomerCurrencyAttrib = basketRequest.CustomAttributes.Where(ca => string.Compare(ca.Name, "incustomercurrency", true) == 0).FirstOrDefault();
                if (inCustomerCurrencyAttrib != null && !string.IsNullOrEmpty(inCustomerCurrencyAttrib.Value))
                {
                    bool inCustomerCurrency = false;
                    bool.TryParse(inCustomerCurrencyAttrib.Value, out inCustomerCurrency);

                    if (inCustomerCurrency)
                        ConvertResponseFromCurrency(basketResponse, basketRequest.CurrencyCode);
                }

                var serializedBasketResponseData = basketResponse.ToXml();
                if (_promoSettings.LogMessages)
                {
                    _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Information, "Qixol Promos basket response", serializedBasketResponseData, _workContext.CurrentCustomer);
                }

                _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, PromoCustomerAttributeNames.PromoBasketResponse, serializedBasketResponseData, _storeContext.CurrentStore.Id);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed in SendBasketRequestToPromoService", ex, _workContext.CurrentCustomer);
                _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Information, "SendBasketRequestToPromoService", basketRequest.ToXml(), _workContext.CurrentCustomer);
            }

            return _promoUtilities.GetBasketResponse();

        }

        private void ConvertResponseFromCurrency(BasketResponse response, string requestCurrencyCode)
        {
            var fromCurrency = _currencyService.GetCurrencyByCode(requestCurrencyCode);
            if (fromCurrency == null || fromCurrency.Rate == 1)
                return;

            // Only bother doing this if the response is valid.
            if (response == null || response.Summary == null || !response.Summary.ProcessingResult)
                return;

            // The basket was submitted in the currency selected by the customer.  This is so that if there are promotions that are dependant on value (i.e. buy one get one for a pound!),
            // these promotions can be configured per currency.  
            // To avoid converting on the fly (which was first attempted!) - we'll convert all values back to the base currency before the response is captured, then
            // it is done in one place and all will be fine.

            // Firstly deal with header values
            response.BasketDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.BasketDiscount, fromCurrency);
            response.BasketTotal = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.BasketTotal, fromCurrency);
            response.DeliveryOriginalPrice = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.DeliveryOriginalPrice, fromCurrency);
            response.DeliveryPrice = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.DeliveryPrice, fromCurrency);
            response.DeliveryPromotionDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.DeliveryPromotionDiscount, fromCurrency);
            response.DeliveryTotalDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.DeliveryTotalDiscount, fromCurrency);
            response.LinesTotalDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.LinesTotalDiscount, fromCurrency);
            response.OriginalBasketTotal = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.OriginalBasketTotal, fromCurrency);
            response.TotalDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(response.TotalDiscount, fromCurrency);
            //response.DeliveryManualDiscount
            //response.ManualDiscount

            // Loop through response items BUT don't include any items added (which have not been split), as these will be 'Free products' - and the prices will already
            //  be in base currency.
            response.Items.Where(ri => !(ri.Generated && ri.SplitFromLineId == 0))
                          .ToList()
                          .ForEach(ri =>
            {
                ri.LineAmount = _currencyService.ConvertToPrimaryExchangeRateCurrency(ri.LineAmount, fromCurrency);
                ri.LinePromotionDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(ri.LinePromotionDiscount, fromCurrency);
                ri.OriginalAmount = _currencyService.ConvertToPrimaryExchangeRateCurrency(ri.OriginalAmount, fromCurrency);
                ri.OriginalPrice = _currencyService.ConvertToPrimaryExchangeRateCurrency(ri.OriginalPrice, fromCurrency);
                ri.Price = _currencyService.ConvertToPrimaryExchangeRateCurrency(ri.Price, fromCurrency);
                ri.TotalDiscount = _currencyService.ConvertToPrimaryExchangeRateCurrency(ri.TotalDiscount, fromCurrency);
                //ri.ManualDiscount

                if (ri.AppliedPromotions != null)
                {
                    ri.AppliedPromotions.ForEach(ap =>
                    {
                        ap.DiscountAmount = _currencyService.ConvertToPrimaryExchangeRateCurrency(ap.DiscountAmount, fromCurrency);
                    });
                }
            });

            if (response.Summary.AppliedPromotions != null)
            {
                response.Summary.AppliedPromotions.ForEach(ap =>
                {
                    ap.DiscountAmount = _currencyService.ConvertToPrimaryExchangeRateCurrency(ap.DiscountAmount, fromCurrency);
                });
            }
        }

        #endregion

        #endregion
    }
}
