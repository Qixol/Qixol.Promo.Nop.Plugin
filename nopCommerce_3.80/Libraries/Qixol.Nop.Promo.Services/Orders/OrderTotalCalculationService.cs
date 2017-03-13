using global::Nop.Core;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Common;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Discounts;
using global::Nop.Core.Domain.Orders;
using global::Nop.Core.Domain.Shipping;
using global::Nop.Core.Domain.Tax;
using global::Nop.Services.Catalog;
using global::Nop.Services.Common;
using global::Nop.Services.Discounts;
using global::Nop.Services.Orders;
using global::Nop.Services.Payments;
using global::Nop.Services.Shipping;
using global::Nop.Services.Tax;
using Nop.Services.Directory;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.Catalog;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Services.Tax;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Orders
{
    public partial class OrderTotalCalculationService : global::Nop.Services.Orders.OrderTotalCalculationService, IOrderTotalCalculationService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPromosPriceCalculationService _promosPriceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;
        private readonly IGiftCardService _giftCardService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRewardPointService _rewardPointService;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IPromoUtilities _promoUtilities;
        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IAttributeValueService _attributeValueService;
        private readonly ITaxServiceExtensions _taxServiceExtensions;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public OrderTotalCalculationService(IWorkContext workContext,
            IStoreContext storeContext,
            IPromosPriceCalculationService promosPriceCalculationService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IShippingService shippingService,
            IPaymentService paymentService,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDiscountService discountService,
            IGiftCardService giftCardService,
            IGenericAttributeService genericAttributeService,
            IRewardPointService rewardPointService,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            IPromoUtilities promoUtilities,
            PromoSettings promoSettings,
            IPromoService promoService,
            IAttributeValueService attributeValueService,
            ITaxServiceExtensions taxServiceExtensions,
            ICurrencyService currencyService)
            : base(workContext,
                storeContext,
                priceCalculationService,
                taxService,
                shippingService,
                paymentService,
                checkoutAttributeParser,
                discountService,
                giftCardService,
                genericAttributeService,
                rewardPointService,
                taxSettings,
                rewardPointsSettings,
                shippingSettings,
                shoppingCartSettings,
                catalogSettings)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._promosPriceCalculationService = promosPriceCalculationService;
            this._taxService = taxService;
            this._shippingService = shippingService;
            this._paymentService = paymentService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._discountService = discountService;
            this._giftCardService = giftCardService;
            this._genericAttributeService = genericAttributeService;
            this._rewardPointService = rewardPointService;
            this._taxSettings = taxSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._shippingSettings = shippingSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._promoUtilities = promoUtilities;
            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._priceCalculationService = priceCalculationService;
            this._attributeValueService = attributeValueService;
            this._taxServiceExtensions = taxServiceExtensions;
            this._currencyService = currencyService;
        }

        #endregion

        #region methods

        public override decimal AdjustShippingRate(decimal shippingRate, IList<ShoppingCartItem> cart, out List<Discount> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.AdjustShippingRate(shippingRate, cart, out appliedDiscounts);

            appliedDiscounts = new List<Discount>();

            // TODO: this should set discounts in the appliedDiscounts "out" parameter

            decimal additionalShippingCharge = GetShoppingCartAdditionalShippingCharge(cart);

            return shippingRate + additionalShippingCharge;
        }

        public override int CalculateRewardPoints(Customer customer, decimal amount)
        {
            if (!_promoSettings.Enabled)
                return base.CalculateRewardPoints(customer, amount);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            return basketResponse.IssuedPoints();
        }

        protected override decimal GetShippingDiscount(Customer customer, decimal shippingTotal, out List<Discount> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetShippingDiscount(customer, shippingTotal, out appliedDiscounts);

            appliedDiscounts = new List<Discount>();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            var discountAmount = basketResponse.DeliveryPromoDiscount();
            if (discountAmount != decimal.Zero)
            {
                Discount appliedDiscount = new Discount()
                {
                    Name = basketResponse.DeliveryPromoName(),
                    DiscountAmount = discountAmount
                };
                appliedDiscounts.Add(appliedDiscount);
            }

            return discountAmount;
        }

        #region GetShoppingCartShippingTotal

        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetShoppingCartShippingTotal(cart, includingTax);
        }

        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax)
        {
            decimal taxRate = decimal.Zero;
            return GetShoppingCartShippingTotal(cart, includingTax, out taxRate);
        }

        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax, out decimal taxRate)
        {
            List<Discount> appliedDiscounts = new List<Discount>();
            return GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);
        }

        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax,
            out decimal taxRate, out List<global::Nop.Core.Domain.Discounts.Discount> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);

            #region old code

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            decimal? shippingTotal = null;

            if (basketResponse == null)
            {
                return base.GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);
            }

            taxRate = Decimal.Zero;
            appliedDiscounts = new List<Discount>();

            var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);

            if (shippingOption == null)
            {
                // Where there are items in the basket that are not for shipping, we need to ensure we return a zero.
                if (cart.Any(sci => sci.IsShipEnabled))
                    return null;
                else
                    return Decimal.Zero;
            }                

            shippingTotal = basketResponse.DeliveryPrice;

            if ((basketResponse.DeliveryPromotionDiscount > Decimal.Zero))
            {
                if (basketResponse.BasketLevelDiscountIncludesDeliveryAmount())
                {
                    shippingTotal = basketResponse.DeliveryOriginalPrice;
                }
                else
                {
                    Discount appliedDiscount = new Discount();
                    appliedDiscount.DiscountAmount = basketResponse.DeliveryPromotionDiscount;
                    appliedDiscount.Name = basketResponse.DeliveryPromo().PromotionName;
                    appliedDiscounts.Add(appliedDiscount);
                }
            }

            #endregion

            #region new code

            decimal? shippingTotalTaxed = shippingTotal;
            Customer customer = cart.GetCustomer();

            if (shippingTotal.HasValue)
            {
                if (shippingTotal.Value < decimal.Zero)
                    shippingTotal = decimal.Zero;

                //round
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                    shippingTotal = RoundingHelper.RoundPrice(shippingTotal.Value);

                shippingTotalTaxed = _taxService.GetShippingPrice(shippingTotal.Value,
                    includingTax,
                    customer,
                    out taxRate);

                //round
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                    shippingTotalTaxed = RoundingHelper.RoundPrice(shippingTotalTaxed.Value);
            }

            #endregion

            return shippingTotalTaxed;
        }

        #endregion

        #region GetShoppingCartSubTotal

        public override void GetShoppingCartSubTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax, out decimal discountAmount,
            out List<global::Nop.Core.Domain.Discounts.Discount> appliedDiscounts, out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount, out SortedDictionary<decimal, decimal> taxRates)
        {
            if (!_promoSettings.Enabled)
            {
                base.GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
                return;
            }

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null)
            {
                base.GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
                return;
            }

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<Discount>();
            subTotalWithoutDiscount = decimal.Zero;
            subTotalWithDiscount = decimal.Zero;
            taxRates = new SortedDictionary<decimal, decimal>();

            Customer customer = cart.GetCustomer();

            if (cart.Count == 0)
                return;

            decimal subTotalExclTaxWithoutDiscount = decimal.Zero;
            decimal subTotalInclTaxWithoutDiscount = decimal.Zero;
            foreach (var shoppingCartItem in cart)
            {
                decimal usePrice = _priceCalculationService.GetUnitPrice(shoppingCartItem, false);
                decimal sciSubTotal = _promosPriceCalculationService.GetSubTotal(shoppingCartItem, true);

                decimal taxRate;
                decimal sciExclTax = _taxService.GetProductPrice(shoppingCartItem.Product, sciSubTotal, false, customer, out taxRate);
                decimal sciInclTax = _taxService.GetProductPrice(shoppingCartItem.Product, sciSubTotal, true, customer, out taxRate);
                subTotalExclTaxWithoutDiscount += sciExclTax;
                subTotalInclTaxWithoutDiscount += sciInclTax;

                decimal sciTax = sciInclTax - sciExclTax;
                if (taxRate > decimal.Zero && sciTax > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                    {
                        taxRates.Add(taxRate, sciTax);
                    }
                    else
                    {
                        taxRates[taxRate] = taxRates[taxRate] + sciTax;
                    }
                }
            }

            // checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
                var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (attributeValues != null && attributeValues.Count > 0)
                {
                    foreach (var attributeValue in attributeValues)
                    {
                        decimal taxRate;

                        decimal caExclTax = _taxServiceExtensions.GetCheckoutAttributePrice(attributeValue, false, customer, out taxRate, true);
                        decimal caInclTax = _taxServiceExtensions.GetCheckoutAttributePrice(attributeValue, true, customer, out taxRate, true);
                        subTotalExclTaxWithoutDiscount += caExclTax;
                        subTotalInclTaxWithoutDiscount += caInclTax;

                        //tax rates
                        decimal caTax = caInclTax - caExclTax;
                        if (taxRate > decimal.Zero && caTax > decimal.Zero)
                        {
                            if (!taxRates.ContainsKey(taxRate))
                            {
                                taxRates.Add(taxRate, caTax);
                            }
                            else
                            {
                                taxRates[taxRate] = taxRates[taxRate] + caTax;
                            }
                        }
                    }
                }
            }

            if (includingTax)
                subTotalWithoutDiscount = subTotalInclTaxWithoutDiscount;
            else
                subTotalWithoutDiscount = subTotalExclTaxWithoutDiscount;

            if (subTotalWithoutDiscount < decimal.Zero)
                subTotalWithoutDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithoutDiscount = RoundingHelper.RoundPrice(subTotalWithoutDiscount);

            //We calculate discount amount on order subtotal excl tax (discount first)
            //calculate discount amount ('Applied to order subtotal' discount)
            decimal discountAmountExclTax = GetOrderSubtotalDiscount(customer, subTotalExclTaxWithoutDiscount, out appliedDiscounts);
            if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTaxWithoutDiscount;
            decimal discountAmountInclTax = discountAmountExclTax;
            //subtotal with discount (excl tax)
            decimal subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;
            decimal subTotalInclTaxWithDiscount = subTotalExclTaxWithDiscount;

            //add tax for shopping items & checkout attributes
            var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (KeyValuePair<decimal, decimal> kvp in tempTaxRates)
            {
                decimal taxRate = kvp.Key;
                decimal taxValue = kvp.Value;

                if (taxValue != decimal.Zero)
                {
                    //discount the tax amount that applies to subtotal items
                    if (subTotalExclTaxWithoutDiscount > decimal.Zero)
                    {
                        decimal discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalExclTaxWithoutDiscount);
                        discountAmountInclTax += discountTax;
                        taxValue = taxRates[taxRate] - discountTax;
                        if (_shoppingCartSettings.RoundPricesDuringCalculation)
                            taxValue = RoundingHelper.RoundPrice(taxValue);
                        taxRates[taxRate] = taxValue;
                    }

                    //subtotal with discount (incl tax)
                    subTotalInclTaxWithDiscount += taxValue;
                }
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                discountAmountInclTax = RoundingHelper.RoundPrice(discountAmountInclTax);
                discountAmountExclTax = RoundingHelper.RoundPrice(discountAmountExclTax);
            }

            if (includingTax)
            {
                subTotalWithDiscount = subTotalInclTaxWithDiscount;
                discountAmount = discountAmountInclTax;
            }
            else
            {
                subTotalWithDiscount = subTotalExclTaxWithDiscount;
                discountAmount = discountAmountExclTax;
            }

            if (subTotalWithDiscount < decimal.Zero)
                subTotalWithDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithDiscount = RoundingHelper.RoundPrice(subTotalWithDiscount);

        }

        public override void GetShoppingCartSubTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax, out decimal discountAmount,
            out List<global::Nop.Core.Domain.Discounts.Discount> appliedDiscounts, out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount)
        {
            SortedDictionary<decimal, decimal> taxRates = new SortedDictionary<decimal, decimal>();
            GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
        }

        #endregion

        #region GetShoppingCartTotal

        public override decimal? GetShoppingCartTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, out decimal discountAmount,
            out List<global::Nop.Core.Domain.Discounts.Discount> appliedDiscounts, out List<AppliedGiftCard> appliedGiftCards, out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount, bool ignoreRewardPonts = false, bool usePaymentMethodAdditionalFee = true)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, ignoreRewardPonts, usePaymentMethodAdditionalFee);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
            {
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, ignoreRewardPonts, usePaymentMethodAdditionalFee);
            }

            if (!basketResponse.Summary.ProcessingResult)
            {
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, ignoreRewardPonts, usePaymentMethodAdditionalFee);
            }

            Customer customer = cart.GetCustomer();

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<Discount>();
            decimal resultTemp = basketResponse.BasketTotal;

            #region Applied gift cards

            appliedGiftCards = new List<AppliedGiftCard>();
            if (!cart.IsRecurring())
            {
                //we don't apply gift cards for recurring products
                var giftCards = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer);
                if (giftCards != null)
                    foreach (var gc in giftCards)
                        if (resultTemp > decimal.Zero)
                        {
                            decimal remainingAmount = gc.GetGiftCardRemainingAmount();
                            decimal amountCanBeUsed = decimal.Zero;
                            if (resultTemp > remainingAmount)
                                amountCanBeUsed = remainingAmount;
                            else
                                amountCanBeUsed = resultTemp;

                            //reduce subtotal
                            resultTemp -= amountCanBeUsed;

                            var appliedGiftCard = new AppliedGiftCard();
                            appliedGiftCard.GiftCard = gc;
                            appliedGiftCard.AmountCanBeUsed = amountCanBeUsed;
                            appliedGiftCards.Add(appliedGiftCard);
                        }
            }

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = RoundingHelper.RoundPrice(resultTemp);

            #endregion

            redeemedRewardPoints = 0;
            redeemedRewardPointsAmount = Decimal.Zero;

            decimal orderTotal = basketResponse.BasketTotal - appliedGiftCards.Sum(agc => agc.AmountCanBeUsed);

            #region Reward points

            if (_rewardPointsSettings.Enabled &&
                !ignoreRewardPonts &&
                customer.GetAttribute<bool>(SystemCustomerAttributeNames.UseRewardPointsDuringCheckout,
                    _genericAttributeService, _storeContext.CurrentStore.Id))
            {
                int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(customer.Id, _storeContext.CurrentStore.Id);
                if (CheckMinimumRewardPointsToUseRequirement(rewardPointsBalance))
                {
                    decimal rewardPointsBalanceAmount = ConvertRewardPointsToAmount(rewardPointsBalance);
                    if (orderTotal > decimal.Zero)
                    {
                        if (orderTotal > rewardPointsBalanceAmount)
                        {
                            redeemedRewardPoints = rewardPointsBalance;
                            redeemedRewardPointsAmount = rewardPointsBalanceAmount;
                        }
                        else
                        {
                            redeemedRewardPointsAmount = orderTotal;
                            redeemedRewardPoints = ConvertAmountToRewardPoints(redeemedRewardPointsAmount);
                        }
                    }
                }
            }

            #endregion

            var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
            if (shippingOption == null)
            {
                // Where there are items in the basket that are not for shipping, ensure we carry on, otherwise placing orders gets stuck.
                if(cart.Any(sci => sci.IsShipEnabled))
                    return null;                
            }

            discountAmount = basketResponse.OrderDiscountTotal();
            if (discountAmount > 0)
            {
                Discount appliedDiscount = new Discount()
                {
                    Name = basketResponse.BasketLevelPromotion().PromotionName,
                    DiscountAmount = discountAmount
                };
                appliedDiscounts.Add(appliedDiscount);
            }

            decimal tax = GetTaxTotal(cart);

            return basketResponse.BasketTotal + tax - appliedGiftCards.Sum(agc => agc.AmountCanBeUsed) - redeemedRewardPointsAmount;
        }

        public override decimal? GetShoppingCartTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool ignoreRewardPonts = false, bool usePaymentMethodAdditionalFee = true)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartTotal(cart, ignoreRewardPonts, usePaymentMethodAdditionalFee);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return base.GetShoppingCartTotal(cart, ignoreRewardPonts, usePaymentMethodAdditionalFee);

            if (!basketResponse.Summary.ProcessingResult)
                return base.GetShoppingCartTotal(cart, ignoreRewardPonts, usePaymentMethodAdditionalFee);

            decimal tax = GetTaxTotal(cart);

            return basketResponse.BasketTotal + tax;
        }

        #endregion

        #region GetTaxTotal

        //public override decimal GetTaxTotal(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        //{
        //    if (cart == null)
        //        throw new ArgumentNullException("cart");

        //    SortedDictionary<decimal, decimal> taxRates = null;
        //    return GetTaxTotal(cart, out taxRates, usePaymentMethodAdditionalFee);
        //}

        //public override decimal GetTaxTotal(IList<Nop.Core.Domain.Orders.ShoppingCartItem> cart, out SortedDictionary<decimal, decimal> taxRates, bool usePaymentMethodAdditionalFee = true)
        //{
        //    if (cart == null)
        //        throw new ArgumentNullException("cart");

        //    return base.GetTaxTotal(cart, out taxRates, usePaymentMethodAdditionalFee);
        //}

        #endregion

        public override decimal GetShoppingCartAdditionalShippingCharge(IList<ShoppingCartItem> cart)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartAdditionalShippingCharge(cart);

            decimal additionalShippingCharge = decimal.Zero;

            foreach (var sci in cart)
                if (sci.IsShipEnabled && !sci.IsFreeShipping)
                    additionalShippingCharge += sci.AdditionalShippingCharge;

            return additionalShippingCharge;
        }

        public override bool IsFreeShipping(IList<ShoppingCartItem> cart, decimal? subTotal = null)
        {
            if (!_promoSettings.Enabled)
                return base.IsFreeShipping(cart);

            // not applicable when using Promo

            return false;
        }

        #region Utilities

        //public override decimal GetShoppingCartAdditionalShippingCharge(IList<ShoppingCartItem> cart)
        //{
        //    decimal additionalShippingCharge = decimal.Zero;

        //    bool isFreeShipping = IsFreeShipping(cart);
        //    if (isFreeShipping)
        //        return decimal.Zero;

        //    foreach (var sci in cart)
        //        if (sci.IsShipEnabled && !sci.IsFreeShipping)
        //            additionalShippingCharge += sci.AdditionalShippingCharge;

        //    return additionalShippingCharge;
        //}

        protected override decimal GetOrderSubtotalDiscount(Customer customer,
            decimal orderSubTotal, out List<Discount> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetOrderSubtotalDiscount(customer, orderSubTotal, out appliedDiscounts);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            appliedDiscounts = new List<Discount>();
            decimal discountAmount = decimal.Zero;

            if (basketResponse == null)
                return discountAmount;

            if (!basketResponse.BasketLevelDiscountIncludesDeliveryAmount())
            {
                var basketLevelPromo = basketResponse.BasketLevelPromotion();
                if (basketLevelPromo != null)
                {
                    discountAmount = basketLevelPromo.DiscountAmount;
                    Discount appliedDiscount = new Discount()
                    {
                        Name = basketResponse.BasketLevelPromotion().PromotionName,
                        DiscountAmount = discountAmount
                    };
                    appliedDiscounts.Add(appliedDiscount);
                }
            }

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return discountAmount;
        }

        protected override decimal GetOrderTotalDiscount(Customer customer, decimal orderTotal, out List<Discount> appliedDiscounts)
        {
            if (!_rewardPointsSettings.Enabled)
                return base.GetOrderTotalDiscount(customer, orderTotal, out appliedDiscounts);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            appliedDiscounts = new List<Discount>();
            decimal discountAmount = decimal.Zero;

            if (basketResponse == null)
                return discountAmount;

            if (basketResponse.BasketLevelDiscountIncludesDeliveryAmount())
            {
                discountAmount = basketResponse.BasketLevelPromotion().DiscountAmount;
                Discount appliedDiscount = new Discount()
                {
                    Name = basketResponse.BasketLevelPromotion().PromotionName,
                    DiscountAmount = discountAmount
                };
                appliedDiscounts.Add(appliedDiscount);
            }

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return discountAmount;
        }
        #endregion

        #endregion
    }
}
