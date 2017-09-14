using global::Nop.Core;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Directory;
using global::Nop.Core.Domain.Orders;
using global::Nop.Core.Domain.Shipping;
using global::Nop.Services.Catalog;
using global::Nop.Services.Common;
using global::Nop.Services.Directory;
using global::Nop.Services.Localization;
using global::Nop.Services.Orders;
using global::Nop.Services.Shipping;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qixol.Nop.Promo.Services.Orders;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Qixol.Nop.Promo.Core.Domain;

namespace Qixol.Nop.Promo.Services.ShoppingCart
{
    public static class ShoppingCartExtensions
    {
        #region constructor

        static ShoppingCartExtensions()
        {
            // static class constructor can't take parameters so use the Dependency Resolver in the extension method to find the services
        }

        #endregion

        #region methods

        public static BasketRequest ToQixolPromoBasketRequest(this IList<ShoppingCartItem> cart, ShippingOption shippingOption, string[] couponCodes = null)
        {
            IProductService _productService = EngineContext.Current.Resolve<IProductService>();
            IPriceCalculationService _priceCalculationService = EngineContext.Current.Resolve<IPriceCalculationService>();
            IWorkContext _workContext = EngineContext.Current.Resolve<IWorkContext>();
            IStoreContext _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            IGenericAttributeService _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            PromoSettings _promoSettings = EngineContext.Current.Resolve<PromoSettings>();
            IPromoUtilities _promoUtilities = EngineContext.Current.Resolve<IPromoUtilities>();
            IOrderTotalCalculationService _orderTotalCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();
            ICurrencyService _currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            IShoppingCartService _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
            IProductMappingService _productMappingService = EngineContext.Current.Resolve<IProductMappingService>();
            IAttributeValueService _attributeValueService = EngineContext.Current.Resolve<IAttributeValueService>();
            ICheckoutAttributeParser _checkoutAttributeParser = EngineContext.Current.Resolve<ICheckoutAttributeParser>();
            ICheckoutAttributeService _checkoutAttributeService = EngineContext.Current.Resolve<ICheckoutAttributeService>();
            IGiftCardService _giftCardService = EngineContext.Current.Resolve<IGiftCardService>();
            ICountryService _countryService = EngineContext.Current.Resolve<ICountryService>();
            IStateProvinceService _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            IPluginFinder _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();

            if (cart == null || !cart.Any())
                return null;

            var customer = cart.GetCustomer();
            if (customer == null)
                return null;

            var basketResponse = _promoUtilities.GetBasketResponse(customer);

            // remove the previous response
            _genericAttributeService.SaveAttribute<string>(customer, PromoCustomerAttributeNames.PromoBasketResponse, null, _storeContext.CurrentStore.Id);

            IList<BasketRequestItem> items = new List<BasketRequestItem>();

            Decimal orderTotal = Decimal.Zero;

            #region remove any items added by promo engine
            // remove any items added by promo engine that were NOT split from the original basket

            if (basketResponse != null && basketResponse.Items != null)
            {
                var generatedItems = (from bri in basketResponse.Items where bri.SplitFromLineId == 0 && bri.Generated && !bri.IsDelivery select bri).ToList();
                foreach (var generatedItem in generatedItems)
                {
                    int productId = 0;
                    if (!int.TryParse(generatedItem.ProductCode, out productId))
                    {
                        var attributeValueMappingItems = _attributeValueService.RetrieveAllForAttribute(EntityAttributeName.CheckoutAttribute);
                        var attributeValueMappingItem = (from ca in attributeValueMappingItems where ca.Code.Equals((generatedItem.ProductCode ?? string.Empty), StringComparison.InvariantCultureIgnoreCase) select ca).FirstOrDefault();
                        if (attributeValueMappingItems == null)
                            throw new KeyNotFoundException(string.Format("No attributeValueMappingItem for product code {0}", generatedItem.ProductCode));
                    }
                    else
                    {
                        Product product = _productService.GetProductById(productId);

                        ProductMappingItem productMappingItem = _productMappingService.RetrieveFromVariantCode(productId, generatedItem.VariantCode);

                        string attributesXml = string.Empty;

                        if (productMappingItem != null)
                            attributesXml = productMappingItem.AttributesXml;

                        var addedItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, ShoppingCartType.ShoppingCart, product, attributesXml);
                        if (addedItem != null)
                        {
                            int generatedItemQuantity = Decimal.ToInt32(generatedItem.Quantity);
                            int newQuantity = addedItem.Quantity - generatedItemQuantity;
                            _shoppingCartService.UpdateShoppingCartItem(customer: customer,
                                shoppingCartItemId: addedItem.Id,
                                attributesXml: addedItem.AttributesXml,
                                customerEnteredPrice: addedItem.CustomerEnteredPrice,
                                rentalStartDate: addedItem.RentalStartDateUtc,
                                rentalEndDate: addedItem.RentalEndDateUtc,
                                quantity: newQuantity,
                                resetCheckoutData: false);
                        }
                    }
                }
            }

            cart = customer.ShoppingCartItems.ToList();

            #endregion

            #region build basket items

            // If we no longer have items in the shopping cart, do not return a basket Request so we don't call Promo unnecessarily
            if (cart.Count < 1)
                return null;

            foreach (var shoppingCartItem in cart)
            {
                Product product = _productService.GetProductById(shoppingCartItem.ProductId);

                string productCode = product.Id.ToString();
                string barcode = product.Gtin;
                string variantCode = string.Empty;

                var productMappingItem = _productMappingService.RetrieveFromShoppingCartItem(shoppingCartItem);
                if (productMappingItem != null)
                    variantCode = productMappingItem.VariantCode;

                // DM Cope with baskets in current currency
                decimal usePrice = _priceCalculationService.GetUnitPrice(shoppingCartItem, includeDiscounts: false);
                if (_promoSettings.UseSelectedCurrencyWhenSubmittingBaskets && _workContext.WorkingCurrency.Rate != 1)
                    usePrice = _currencyService.ConvertFromPrimaryExchangeRateCurrency(usePrice, _workContext.WorkingCurrency);

                BasketRequestItem item = new BasketRequestItem()
                {
                    // Hotfix - 2016-10-17 - START
                    //Barcode = barcode,
                    // Hotfix - 2016-10-17 - END
                    Id = shoppingCartItem.Id,
                    Price = usePrice,
                    ProductCode = productCode,
                    Quantity = (byte) shoppingCartItem.Quantity,
                    VariantCode = variantCode
                };

                items.Add(item);

                // DM Cope with baskets in current currency
                decimal subTotalAmount = _priceCalculationService.GetSubTotal(shoppingCartItem, includeDiscounts: false);
                if (_promoSettings.UseSelectedCurrencyWhenSubmittingBaskets && _workContext.WorkingCurrency.Rate != 1)
                    subTotalAmount = _currencyService.ConvertFromPrimaryExchangeRateCurrency(subTotalAmount, _workContext.WorkingCurrency);

                orderTotal += subTotalAmount;
            }

            #endregion

            #region coupons

            IList<BasketRequestCoupon> coupons = new List<BasketRequestCoupon>();
            if (couponCodes != null)
            {
                couponCodes.ToList().ForEach(cc =>
                {
                    BasketRequestCoupon coupon = new BasketRequestCoupon()
                    {
                        Code = cc,
                    };
                    coupons.Add(coupon);
                });
            }

            #endregion

            #region customer role mapping

            AttributeValueMappingItem customerGroupAttributeValueMappingItem = null;
            string customerGroupIntegrationCode = string.Empty;
            int priority = int.MinValue;
            foreach (CustomerRole customerRole in customer.CustomerRoles)
            {
                AttributeValueMappingItem attributeValueMappingItem = _attributeValueService.Retrieve(customerRole.Id, EntityAttributeName.CustomerRole);
                if (attributeValueMappingItem != null && !string.IsNullOrEmpty(attributeValueMappingItem.Code))
                {
                    if (attributeValueMappingItem.Priority.HasValue)
                    {
                        if (attributeValueMappingItem.Priority.Value > priority)
                        {
                            priority = attributeValueMappingItem.Priority.Value;
                            customerGroupAttributeValueMappingItem = attributeValueMappingItem;
                        }
                    }
                    else
                    {
                        customerGroupAttributeValueMappingItem = attributeValueMappingItem;
                    }
                }
            }

            if (customerGroupAttributeValueMappingItem != null)
            {
                customerGroupIntegrationCode = customerGroupAttributeValueMappingItem.Code;
            }

            #endregion

            #region store mapping

            string storeIntegrationCode = string.Empty;

            AttributeValueMappingItem storeAttributeValueMappingItem = _attributeValueService.Retrieve(_storeContext.CurrentStore.Id, EntityAttributeName.Store);

            if (storeAttributeValueMappingItem != null && !string.IsNullOrEmpty(storeAttributeValueMappingItem.Code))
                storeIntegrationCode = storeAttributeValueMappingItem.Code;

            #endregion

            #region checkout attributes

            string checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
            List<CheckoutAttribute> checkoutAttributes = _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttributesXml).ToList();

            int basketItemId = (from i in items orderby i.Id descending select i.Id).FirstOrDefault();

            foreach (CheckoutAttribute checkoutAttribute in checkoutAttributes)
            {
                List<string> checkoutAttributeValueIds = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, checkoutAttribute.Id).ToList();

                decimal priceAdjustment = decimal.Zero;

                foreach (string checkoutAttributeValueId in checkoutAttributeValueIds)
                {
                    int id;
                    if (int.TryParse(checkoutAttributeValueId, out id))
                    {
                        CheckoutAttributeValue checkoutAttributeValue = _checkoutAttributeService.GetCheckoutAttributeValueById(id);

                        if (checkoutAttributeValue.PriceAdjustment > 0)
                        {
                            priceAdjustment += checkoutAttributeValue.PriceAdjustment;
                        }
                    }
                }

                string checkoutAttributeValueVariantCode = null;

                if (checkoutAttributeValueIds.Count == 1)
                {
                    checkoutAttributeValueVariantCode = checkoutAttributeValueIds.First();
                }

                if (priceAdjustment > decimal.Zero)
                {
                    basketItemId++;

                    // DM Cope with baskets in current currency
                    if (_promoSettings.UseSelectedCurrencyWhenSubmittingBaskets && _workContext.WorkingCurrency.Rate != 1)
                        priceAdjustment = _currencyService.ConvertFromPrimaryExchangeRateCurrency(priceAdjustment, _workContext.WorkingCurrency);

                    BasketRequestItem checkoutAttributeBasketItem = new BasketRequestItem()
                    {
                        Id = basketItemId,
                        ProductCode = checkoutAttribute.ProductCode(),
                        VariantCode = checkoutAttributeValueVariantCode,
                        Price = priceAdjustment,
                        Quantity = 1,
                        IsDelivery = false
                    };

                    items.Add(checkoutAttributeBasketItem);

                    orderTotal += priceAdjustment;
                }
            }

            #endregion

            #region gift cards

            // DO NOT APPLY GIFT CARDS at this point
            // as they are a form of tender, only send with a confirmed basket
            // the promo engine needs the full ordertotal for calculations

            #region Applied gift cards

            //decimal resultTemp = orderTotal;
            //List<BasketRequestCustomAttribute> customAttributes = new List<BasketRequestCustomAttribute>();
            //List<AppliedGiftCard> appliedGiftCards = new List<AppliedGiftCard>();

            //if (!cart.IsRecurring())
            //{
            //    //we don't apply gift cards for recurring products
            //    var giftCards = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer);
            //    if (giftCards != null)
            //    {
            //        foreach (var gc in giftCards)
            //        {
            //            if (resultTemp > decimal.Zero)
            //            {
            //                decimal remainingAmount = gc.GetGiftCardRemainingAmount();
            //                decimal amountCanBeUsed = decimal.Zero;
            //                if (resultTemp > remainingAmount)
            //                    amountCanBeUsed = remainingAmount;
            //                else
            //                    amountCanBeUsed = resultTemp;

            //                //reduce subtotal
            //                resultTemp -= amountCanBeUsed;

            //                var appliedGiftCard = new AppliedGiftCard();
            //                appliedGiftCard.GiftCard = gc;
            //                appliedGiftCard.AmountCanBeUsed = amountCanBeUsed;
            //                appliedGiftCards.Add(appliedGiftCard);
            //            }
            //        }

            //        customAttributes.Add(new BasketRequestCustomAttribute()
            //        {
            //            Name = "giftcardscanbeused",
            //            Value = "true"
            //        });

            //        customAttributes.Add(new BasketRequestCustomAttribute()
            //        {
            //            Name = "giftcardscanbeusedvalue",
            //            Value = appliedGiftCards.Sum(agc => agc.AmountCanBeUsed).ToString()
            //        });
            //    }
            //}

            //if (resultTemp < decimal.Zero)
            //    resultTemp = decimal.Zero;
            ////if (_shoppingCartSettings.RoundPricesDuringCalculation)
            ////    resultTemp = RoundingHelper.RoundPrice(resultTemp);

            //orderTotal = resultTemp;

            #endregion

            #endregion

            #region basket

            var basketUniqueReference = customer.GetAttribute<Guid>(PromoCustomerAttributeNames.PromoBasketUniqueReference, _storeContext.CurrentStore.Id);
            if (basketUniqueReference == Guid.Empty)
            {
                basketUniqueReference = Guid.NewGuid();
                _genericAttributeService.SaveAttribute<Guid>(customer, PromoCustomerAttributeNames.PromoBasketUniqueReference, basketUniqueReference, _storeContext.CurrentStore.Id);
            }

            BasketRequest basketRequest = new BasketRequest()
            {
                Id = basketUniqueReference.ToString(),
                CompanyKey = _promoSettings.CompanyKey,
                CustomerId = customer.Id.ToString(),
                BasketTotal = orderTotal,
                SaleDateTime = DateTime.UtcNow,
                Store = storeIntegrationCode,
                StoreGroup = _promoSettings.StoreGroup,
                Channel = _promoSettings.Channel,
                CustomerGroup = customerGroupIntegrationCode,
                //DeliveryPrice = deliveryPrice,
                //DeliveryMethod = shippingIntegrationCode,
                Coupons = coupons.ToList(),
                Items = items.ToList(),
                //CustomAttributes = customAttributes,
                Confirmed = false,
                CurrencyCode = _workContext.WorkingCurrency != null ? _workContext.WorkingCurrency.CurrencyCode : string.Empty
            };

            basketRequest.CustomerId = customer.CustomerGuid.ToString();

            // DM Cope with baskets in current currency
            // Add a flag to the basket request indicating whether the values have been passed in customer currency or not.
            //  This will be used when we are extracting values from the cached basket response, but may also be useful on the Promo side in the future (i.e.
            //  when setting up promotions!).
            basketRequest.AddCustomAttribute("incustomercurrency", _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets.ToString());

            // Add identifier for integration source ("nopCommerce")
            var promoPlugin = _pluginFinder.GetPluginDescriptorBySystemName("Misc.QixolPromo");
            basketRequest.AddCustomAttribute("integrationsource", string.Format("nopCommerce - plugin v{0}", promoPlugin.Version));

            #endregion

            #region shipping

            var selectedShippingOption = shippingOption;
            if (selectedShippingOption == null)
            {
                selectedShippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
            }
            basketRequest = basketRequest.SetShipping(cart, selectedShippingOption);

            #endregion

            #region save request

            string basketRequestString = basketRequest.ToXml();
            _genericAttributeService.SaveAttribute<string>(customer, PromoCustomerAttributeNames.PromoBasketRequest, basketRequestString, _storeContext.CurrentStore.Id);

            #endregion

            return basketRequest;
        }


        public static List<BasketResponseAppliedPromotion> Promotions(this ShoppingCartItem shoppingCartItem)
        {
            var linePromotions = new List<BasketResponseAppliedPromotion>();

            if (shoppingCartItem == null || shoppingCartItem.Customer == null)
                return linePromotions;

            var customer = shoppingCartItem.Customer;

            var basketResponseItems = MatchedResponseItems(shoppingCartItem);

            if (!basketResponseItems.Any())
                return linePromotions;


            var promoUtilities = (IPromoUtilities) EngineContext.Current.Resolve<IPromoUtilities>();
            var basketResponse = promoUtilities.GetBasketResponse(customer);

            foreach (var item in basketResponseItems)
            {
                item.AppliedPromotions.ForEach(ap =>
                {
                    if ((!ap.BasketLevelPromotion && !ap.DeliveryLevelPromotion) && ap.DiscountAmount > decimal.Zero)
                    {
                        var existingPromo = (from lp in linePromotions where lp.PromotionId == ap.PromotionId select lp).FirstOrDefault();
                        if (existingPromo != null)
                        {
                            existingPromo.DiscountAmount += ap.DiscountAmount;
                        }
                        else
                        {
                            linePromotions.Add(ap);
                        }
                    }
                    else
                    {
                        // treat basket level free product promotions as line level
                        if (ap.BasketLevelPromotion && (ap.DiscountAmount > decimal.Zero))
                        {
                            var summaryAppliedPromo = (from p in basketResponse.Summary.AppliedPromotions
                                                       where p.PromotionId == ap.PromotionId && p.InstanceId == ap.InstanceId
                                                       select p).FirstOrDefault();
                            if (summaryAppliedPromo != null && summaryAppliedPromo.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var existingPromo = (from lp in linePromotions where lp.PromotionId == ap.PromotionId select lp).FirstOrDefault();
                                if (existingPromo != null)
                                {
                                    existingPromo.DiscountAmount += ap.DiscountAmount;
                                }
                                else
                                {
                                    linePromotions.Add(ap);
                                }
                            }
                        }
                    }
                });
            }

            return linePromotions;
        }

        public static Decimal LineAmount(this ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null || shoppingCartItem.Customer == null)
                return decimal.Zero;

            var customer = shoppingCartItem.Customer;

            var basketResponseItems = MatchedResponseItems(shoppingCartItem);

            if (!basketResponseItems.Any())
                return decimal.Zero;

            var promoUtilities = (IPromoUtilities) EngineContext.Current.Resolve<IPromoUtilities>();
            var basketResponse = promoUtilities.GetBasketResponse(customer);

            if (!basketResponse.IsValid())
                return decimal.Zero;

            decimal lineTotal = decimal.Zero;

            basketResponseItems.ForEach(bri =>
            {
                lineTotal += bri.LineAmount;

                // convert Free Product at basket level into Free Product at line level
                bri.AppliedPromotions.ForEach(ap =>
                {
                    if (ap.BasketLevelPromotion && ap.DiscountAmount > decimal.Zero)
                    {
                        basketResponse.Summary.AppliedPromotions.Where(p => p.PromotionId == ap.PromotionId && p.InstanceId == ap.InstanceId).ToList().ForEach(sap =>
                        {
                            if (sap.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase))
                            {
                                lineTotal -= ap.DiscountAmount;
                            }
                        });
                    }
                });
            });


            return lineTotal;
        }

        public static Decimal LineDiscountAmount(this ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null || shoppingCartItem.Customer == null)
                return decimal.Zero;

            var customer = shoppingCartItem.Customer;

            var basketResponseItems = MatchedResponseItems(shoppingCartItem);

            if (!basketResponseItems.Any())
                return decimal.Zero;

            var promoUtilities = (IPromoUtilities) EngineContext.Current.Resolve<IPromoUtilities>();
            var basketResponse = promoUtilities.GetBasketResponse(customer);

            if (!basketResponse.IsValid())
                return decimal.Zero;

            decimal lineTotalDiscount = decimal.Zero;

            basketResponseItems.ForEach(bri =>
            {
                lineTotalDiscount += bri.LinePromotionDiscount;

                // convert Free Product at basket level into Free Product at line level
                bri.AppliedPromotions.ForEach(ap =>
                {
                    if (ap.BasketLevelPromotion && ap.DiscountAmount > decimal.Zero)
                    {
                        basketResponse.Summary.AppliedPromotions.Where(p => p.PromotionId == ap.PromotionId && p.InstanceId == ap.InstanceId).ToList().ForEach(sap =>
                        {
                            if (sap.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase))
                            {
                                lineTotalDiscount += ap.DiscountAmount;
                            }
                        });
                    }
                });
            });


            return lineTotalDiscount;
        }

        public static Decimal SubTotal(this IList<ShoppingCartItem> shoppingCartItems)
        {
            decimal subTotal = decimal.Zero;
            IPromoUtilities promoUtilities = EngineContext.Current.Resolve<IPromoUtilities>();

            if (shoppingCartItems == null)
                return subTotal;

            if (!shoppingCartItems.Any())
                return subTotal;

            var customer = shoppingCartItems.GetCustomer();
            if (customer == null)
                return subTotal;

            // sum line totals
            shoppingCartItems.ToList().ForEach(sci =>
            {
                subTotal += sci.LineAmount();
            });

            // include any checkout attributes
            var basketResponse = promoUtilities.GetBasketResponse(customer);
            if (basketResponse == null)
                return subTotal;

            basketResponse.CheckoutAttributeItems().ToList().ForEach(cai =>
            {
                subTotal += cai.LineAmount;
                // handle "free product" at basket level 
                cai.AppliedPromotions.ForEach(ap =>
                {
                    if (ap.BasketLevelPromotion && ap.DiscountAmount > decimal.Zero)
                    {
                        basketResponse.Summary.AppliedPromotions.Where(p => p.PromotionId == ap.PromotionId && p.InstanceId == ap.InstanceId).ToList().ForEach(sap =>
                        {
                            if (sap.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase))
                            {
                                subTotal -= ap.DiscountAmount;
                            }
                        });
                    }
                });
            });


            return subTotal;
        }

        #endregion

        #region helpers

        private static List<BasketResponseItem> MatchedResponseItems(ShoppingCartItem shoppingCartItem)
        {
            var basketResponseItems = new List<BasketResponseItem>();

            if (shoppingCartItem == null)
                return basketResponseItems;

            var promoUtilities = (IPromoUtilities) EngineContext.Current.Resolve<IPromoUtilities>();
            var productMappingService = (IProductMappingService) EngineContext.Current.Resolve<IProductMappingService>();
            var basketResponse = promoUtilities.GetBasketResponse(shoppingCartItem.CustomerId);

            if (!basketResponse.IsValid())
                return basketResponseItems;

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromShoppingCartItem(shoppingCartItem);
            if (productMappingItem != null)
            {
                basketResponseItems = (from i in basketResponse.Items
                                       where
                                           i.ProductCode.Equals(productMappingItem.EntityId.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                                           ((productMappingItem.NoVariants ||
                                           (!productMappingItem.NoVariants &&
                                           !string.IsNullOrEmpty(productMappingItem.VariantCode) &&
                                           i.VariantCode.Equals(productMappingItem.VariantCode, StringComparison.InvariantCultureIgnoreCase))))
                                       select i).ToList();
            }

            return basketResponseItems;
        }

        #endregion
    }
}
