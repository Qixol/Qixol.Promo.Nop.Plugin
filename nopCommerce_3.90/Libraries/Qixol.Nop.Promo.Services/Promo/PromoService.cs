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

        private void ProcessGeneratedBasketLines(List<BasketResponseItem> generatedLines, IList<ShoppingCartItem> cart, Customer customer, int storeId)
        {
            ProcessSplitLines(generatedLines.Where(i => i.Generated && !i.IsDelivery && i.SplitFromLineId > 0).ToList(), cart, customer, storeId);
            ProcessAddedItems(generatedLines.Where(i => i.Generated && !i.IsDelivery && i.SplitFromLineId == 0).ToList(), cart, customer, storeId);
        }

        private void ProcessSplitLines(List<BasketResponseItem> splitBasketLines, IList<ShoppingCartItem> cart, Customer customer, int storeId)
        {
            if (splitBasketLines == null || !splitBasketLines.Any())
                return;

            splitBasketLines.ForEach(splitBasketLine =>
            {
                ShoppingCartItem sci = (from c in cart where c.Id == splitBasketLine.SplitFromLineId select c).FirstOrDefault();
                if (sci == null)
                    throw new NullReferenceException(string.Format("SplitFromLineId {0} does not exist in the cart", splitBasketLine.SplitFromLineId));
            });
        }

        private void ProcessAddedItems(List<BasketResponseItem> addedBasketLines, IList<ShoppingCartItem> cart, Customer customer, int storeId)
        {
            if (addedBasketLines == null || !addedBasketLines.Any())
                return;

            addedBasketLines.ForEach(addedBasketLine =>
             {
                 int productId = 0;
                 if (!int.TryParse(addedBasketLine.ProductCode, out productId))
                 {
                     // Do we have a checkout attribute?
                     var checkoutAttributes = _attributeValueService.RetrieveAllForAttribute(EntityAttributeName.CheckoutAttribute);
                     var checkoutAttribute = (from ca in checkoutAttributes where ca.Code.Equals((addedBasketLine.ProductCode ?? string.Empty), StringComparison.InvariantCultureIgnoreCase) select ca).FirstOrDefault();
                     if (checkoutAttribute == null)
                         throw new KeyNotFoundException(string.Format("No mapping item for product code {0}", addedBasketLine.ProductCode));
                     // is the checkout attribute already "in" the cart
                     // if it's a single select (not checkbox?) then how do we "remove" the current one and replace it?
                     // what about when the price adjustment value has been changed by the promo engine...?
                 }
                 else
                 {
                     Product product = _productService.GetProductById(productId);

                     string attributesXml = string.Empty;

                     ProductMappingItem productMappingItem = _productMappingService.RetrieveFromVariantCode(productId, addedBasketLine.VariantCode);
                     if (productMappingItem != null && !productMappingItem.NoVariants)
                     {
                         attributesXml = productMappingItem.AttributesXml;
                     }

                     // Add the new item - any additional items were deleted before sending the basket to the promo engine

                     var cartType = ShoppingCartType.ShoppingCart;

                     decimal customerEnteredPriceConverted = decimal.Zero;
                     DateTime? rentalStartDate = null;
                     DateTime? rentalEndDate = null;

                     int quantity = int.Parse(addedBasketLine.Quantity.ToString());

                     var existingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, product, attributesXml);
                     if (existingCartItem != null)
                     {
                         _shoppingCartService.UpdateShoppingCartItem(customer, existingCartItem.Id, attributesXml, existingCartItem.CustomerEnteredPrice, existingCartItem.RentalStartDateUtc, existingCartItem.RentalEndDateUtc, existingCartItem.Quantity + quantity, false);
                     }
                     else
                     {
                         List<string> itemAddToCartWarnings = new List<string>();
                         //add to the cart
                         itemAddToCartWarnings.AddRange(_shoppingCartService.AddToCart(customer,
                          product, cartType, storeId,
                          attributesXml, customerEnteredPriceConverted,
                          rentalStartDate, rentalEndDate, quantity, true));

                         if (itemAddToCartWarnings.Count > 0)
                         {
                             string cartWarningTemplate = _localizationService.GetResource("Plugin.Misc.QixolPromo.ShoppingCart.AddItemWarning");
                             string cartWarningMessage = string.Format(cartWarningTemplate, product.Name);
                             //addToCartWarnings.Add(cartWarningMessage);
                             _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Error, cartWarningMessage, string.Join(", ", itemAddToCartWarnings.ToArray()), customer);
                         }
                     }
                 }
             });
        }

        #endregion

        #region Methods

        #region basket promos methods

        public BasketResponse ProcessShoppingCart(Customer customer, int storeId)
        {
            return ProcessShoppingCart(customer, storeId, false, null);
        }

        public BasketResponse ProcessShoppingCart(Customer customer, int storeId, ShippingOption shippingOption)
        {
            return ProcessShoppingCart(customer, storeId, false, shippingOption);
        }

        public BasketResponse ProcessShoppingCart(Customer customer, int storeId, bool getMissedPromotions)
        {
            return ProcessShoppingCart(customer, storeId, getMissedPromotions, null);
        }

        public BasketResponse ProcessShoppingCart(Customer customer, int storeId, bool getMissedPromotions, ShippingOption shippingOption)
        {
            IList<ShoppingCartItem> cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(storeId)
                .ToList();

            // If there are no items in the cart, do not process
            if (!cart.Any())
                return null;

            try
            {
                var couponCodes = customer.ParseAppliedDiscountCouponCodes();
                BasketRequest basketRequest = cart.ToQixolPromoBasketRequest(shippingOption, couponCodes);

                if (basketRequest == null)
                    return null;

                basketRequest.GetMissedPromotions = getMissedPromotions;

                if (shippingOption != null)
                {
                    basketRequest = basketRequest.SetShipping(cart, shippingOption);
                }

                BasketResponse basketResponse = SendBasketRequestTopromoService(customer, storeId, basketRequest);
                if (basketResponse != null && basketResponse.IsValid())
                {
                    // get the selectedShippingOption - if it's not null will need to save this after adding products to the cart
                    ShippingOption selectedShippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, storeId);
                    string selectedPaymentOption = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, storeId);

                    ProcessGeneratedBasketLines(basketResponse.Items.Where(i => i.Generated && !i.IsDelivery).ToList(), cart, customer, storeId);

                    if (selectedShippingOption != null)
                        _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, selectedShippingOption, storeId);

                    if (!string.IsNullOrEmpty(selectedPaymentOption))
                        _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, selectedPaymentOption, storeId);

                    if (couponCodes != null)
                    {
                        // clear all current codes
                        _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);

                        // add utilized codes or warn where codes have errors
                        basketResponse.Coupons.Where(c => !c.Issued).ToList().ForEach(c =>
                        {

                            if (c.Utilizations.Any())
                            {
                                customer.ApplyDiscountCouponCode(c.CouponCode);
                            }
                            else
                            {
                                //addToCartWarnings.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                            }
                        });
                    }
                }
                else
                {
                    // remove any coupon codes from the customer as this causes an exception in the base code when no basket response exists
                    couponCodes.ToList().ForEach(cc =>
                    {
                        customer.RemoveDiscountCouponCode(cc);
                    });
                }
                return basketResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("ProcessShoppingCart", ex, customer);
            }

            return null;
        }

        public BasketResponse SendConfirmedBasket(global::Nop.Core.Domain.Orders.Order placedOrder)
        {
            BasketRequest basketRequest = BasketRequest.FromXml(placedOrder.Customer.GetAttribute<string>(PromoCustomerAttributeNames.PromoBasketRequest, placedOrder.StoreId));
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

            return SendBasketRequestTopromoService(placedOrder.Customer, placedOrder.StoreId, basketRequest);
        }

        private BasketResponse SendBasketRequestTopromoService(Customer customer, int storeId, BasketRequest basketRequest)
        {
            try
            {
                BasketServiceManager basketServiceManager = _promoSettings.GetBasketService();

                if (_promoSettings.LogMessages)
                {
                    var serializedBasketRequestData = basketRequest.ToXml();
                    _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Information, "Qixol Promos basket request", serializedBasketRequestData, customer);
                }

                _genericAttributeService.SaveAttribute<string>(customer, PromoCustomerAttributeNames.PromoBasketResponse, null, storeId);

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

                if (_promoSettings.LogMessages)
                {
                    var serializedBasketResponseData = basketResponse.ToXml();
                    _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Information, "Qixol Promos basket response", serializedBasketResponseData, customer);
                }

                _genericAttributeService.SaveAttribute<BasketResponse>(customer, PromoCustomerAttributeNames.PromoBasketResponse, basketResponse, storeId);

                return basketResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed in SendBasketRequestToPromoService", ex, customer);
                _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Information, "SendBasketRequestToPromoService", basketRequest.ToXml(), customer);
            }

            return null;
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
