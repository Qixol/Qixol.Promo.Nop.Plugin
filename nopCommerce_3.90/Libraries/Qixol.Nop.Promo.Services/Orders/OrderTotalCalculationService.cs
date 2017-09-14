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
        private readonly IPriceCalculationService _priceCalculationService;
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
        private readonly IAttributeValueService _attributeValueService;
        private readonly ITaxServiceExtensions _taxServiceExtensions;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public OrderTotalCalculationService(IWorkContext workContext,
            IStoreContext storeContext,
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
            this._priceCalculationService = priceCalculationService;
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
            this._attributeValueService = attributeValueService;
            this._taxServiceExtensions = taxServiceExtensions;
            this._currencyService = currencyService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets an order discount (applied to order subtotal)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderSubTotal">Order subtotal</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Order discount</returns>
        protected override decimal GetOrderSubtotalDiscount(Customer customer,
            decimal orderSubTotal, out List<DiscountForCaching> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetOrderSubtotalDiscount(customer, orderSubTotal, out appliedDiscounts);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);

            appliedDiscounts = new List<DiscountForCaching>();
            decimal discountAmount = decimal.Zero;

            if (basketResponse == null)
                return discountAmount;

            var currentDiscountAmount = decimal.Zero;
            var currentAppliedDiscounts = new List<DiscountForCaching>();
            var basketLevelPromos = basketResponse.BasketLevelPromotionsExcludingDelivery();
            if (basketLevelPromos != null)
            {
                basketLevelPromos.ForEach(basketLevelPromo =>
                    {
                        discountAmount = basketLevelPromo.DiscountAmount;
                        DiscountForCaching appliedDiscount = new DiscountForCaching()
                        {
                            Name = basketLevelPromo.PromotionName,
                            DiscountAmount = basketLevelPromo.DiscountAmount
                        };
                        currentDiscountAmount += basketLevelPromo.DiscountAmount;
                        currentAppliedDiscounts.Add(appliedDiscount);
                    });
            }

            appliedDiscounts.AddRange(currentAppliedDiscounts);
            discountAmount = currentDiscountAmount;

            return discountAmount;
        }

        /// <summary>
        /// Gets a shipping discount
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shippingTotal">Shipping total</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Shipping discount</returns>
        protected override decimal GetShippingDiscount(Customer customer, decimal shippingTotal, out List<DiscountForCaching> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetShippingDiscount(customer, shippingTotal, out appliedDiscounts);

            appliedDiscounts = new List<DiscountForCaching>();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);
            var deliveryPromos = basketResponse.DeliveryPromos();
            var currentAppliedDiscounts = new List<DiscountForCaching>();
            var currentDiscountAmount = decimal.Zero;

            deliveryPromos.ToList().ForEach(dp =>
            {
                var discountAmount = dp.DiscountAmount;
                if (discountAmount != decimal.Zero)
                {
                    DiscountForCaching appliedDiscount = new DiscountForCaching()
                    {
                        Name = dp.DisplayDetails(),
                        DiscountAmount = discountAmount
                    };
                    currentAppliedDiscounts.Add(appliedDiscount);
                    currentDiscountAmount += appliedDiscount.DiscountAmount;
                }
            });

            appliedDiscounts.AddRange(currentAppliedDiscounts);

            return currentDiscountAmount;
        }

        /// <summary>
        /// Gets an order discount (applied to order total)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Order discount</returns>
        protected override decimal GetOrderTotalDiscount(Customer customer, decimal orderTotal, out List<DiscountForCaching> appliedDiscounts)
        {
            if (!_rewardPointsSettings.Enabled)
                return base.GetOrderTotalDiscount(customer, orderTotal, out appliedDiscounts);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);

            appliedDiscounts = new List<DiscountForCaching>();
            decimal discountAmount = decimal.Zero;

            if (basketResponse == null)
                return discountAmount;

            var currentDiscountAmount = decimal.Zero;
            var currentAppliedDiscounts = new List<DiscountForCaching>();
            var basketLevelPromos = basketResponse.BasketLevelPromotionsIncludingDelivery();
            if (basketLevelPromos != null)
            {
                basketLevelPromos.ForEach(basketLevelPromo =>
                {
                    discountAmount = basketLevelPromo.DiscountAmount;
                    DiscountForCaching appliedDiscount = new DiscountForCaching()
                    {
                        Name = basketLevelPromo.PromotionName,
                        DiscountAmount = basketLevelPromo.DiscountAmount
                    };
                    currentDiscountAmount += basketLevelPromo.DiscountAmount;
                    currentAppliedDiscounts.Add(appliedDiscount);
                });
            }

            appliedDiscounts.AddRange(currentAppliedDiscounts);
            discountAmount = currentDiscountAmount;

            return discountAmount;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        public override void GetShoppingCartSubTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out List<DiscountForCaching> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount)
        {
            SortedDictionary<decimal, decimal> taxRates;
            GetShoppingCartSubTotal(cart, includingTax,
                out discountAmount, out appliedDiscounts,
                out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
        }

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        /// <param name="taxRates">Tax rates (of order sub total)</param>
        public override void GetShoppingCartSubTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out List<DiscountForCaching> appliedDiscounts,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount,
            out SortedDictionary<decimal, decimal> taxRates)
        {
            if (!_promoSettings.Enabled)
            {
                base.GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
                return;
            }

            if (cart == null || !cart.Any())
            {
                base.GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
                return;
            }

            var customer = cart.GetCustomer();
            if (customer == null)
            {
                base.GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
                return;
            }

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);

            if (basketResponse == null)
            {
                base.GetShoppingCartSubTotal(cart, includingTax, out discountAmount, out appliedDiscounts, out subTotalWithoutDiscount, out subTotalWithDiscount, out taxRates);
                return;
            }

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();
            subTotalWithoutDiscount = decimal.Zero;
            subTotalWithDiscount = decimal.Zero;
            taxRates = new SortedDictionary<decimal, decimal>();

            //sub totals
            decimal subTotalExclTaxWithoutDiscount = decimal.Zero;
            decimal subTotalInclTaxWithoutDiscount = decimal.Zero;
            foreach (var shoppingCartItem in cart)
            {
                decimal sciSubTotal = _priceCalculationService.GetSubTotal(shoppingCartItem, true);

                decimal taxRate;
                decimal sciExclTax = _taxService.GetProductPrice(shoppingCartItem.Product, sciSubTotal, false, customer, out taxRate);
                decimal sciInclTax = _taxService.GetProductPrice(shoppingCartItem.Product, sciSubTotal, true, customer, out taxRate);
                subTotalExclTaxWithoutDiscount += sciExclTax;
                subTotalInclTaxWithoutDiscount += sciInclTax;

                //tax rates
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

            //checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService, _storeContext.CurrentStore.Id);
                var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (attributeValues != null)
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

            //subtotal without discount
            subTotalWithoutDiscount = includingTax ? subTotalInclTaxWithoutDiscount : subTotalExclTaxWithoutDiscount;
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

        public virtual void UpdateOrderTotals(UpdateOrderParameters updateOrderParameters, IList<ShoppingCartItem> restoredCart)
        {
            base.UpdateOrderTotals(updateOrderParameters, restoredCart);
        }

        /// <summary>
        /// Gets shopping cart additional shipping charge
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Additional shipping charge</returns>
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

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="subTotal">Subtotal amount; pass null to calculate subtotal</param>
        /// <returns>A value indicating whether shipping is free</returns>
        public override bool IsFreeShipping(IList<ShoppingCartItem> cart, decimal? subTotal = null)
        {
            if (!_promoSettings.Enabled)
                return base.IsFreeShipping(cart);

            // not applicable when using Promo

            return false;
        }

        /// <summary>
        /// Adjust shipping rate (free shipping, additional charges, discounts)
        /// </summary>
        /// <param name="shippingRate">Shipping rate to adjust</param>
        /// <param name="cart">Cart</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Adjusted shipping rate</returns>
        public override decimal AdjustShippingRate(decimal shippingRate,
            IList<ShoppingCartItem> cart, out List<DiscountForCaching> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.AdjustShippingRate(shippingRate, cart, out appliedDiscounts);

            appliedDiscounts = new List<DiscountForCaching>();

            // TODO: this should set discounts in the appliedDiscounts "out" parameter

            decimal additionalShippingCharge = GetShoppingCartAdditionalShippingCharge(cart);

            return shippingRate + additionalShippingCharge;
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shipping total</returns>
        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetShoppingCartShippingTotal(cart, includingTax);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <returns>Shipping total</returns>
        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax)
        {
            decimal taxRate = decimal.Zero;
            return GetShoppingCartShippingTotal(cart, includingTax, out taxRate);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="taxRate">Applied tax rate</param>
        /// <returns>Shipping total</returns>
        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax,
            out decimal taxRate)
        {
            List<DiscountForCaching> appliedDiscounts = new List<DiscountForCaching>();
            return GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="taxRate">Applied tax rate</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <returns>Shipping total</returns>
        public override decimal? GetShoppingCartShippingTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool includingTax,
            out decimal taxRate, out List<DiscountForCaching> appliedDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);

            #region old code

            if (cart == null || !cart.Any())
                return base.GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);

            var customer = cart.GetCustomer();
            if (customer == null)
                return base.GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);
            decimal? shippingTotal = null;

            if (basketResponse == null)
            {
                return base.GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscounts);
            }

            taxRate = Decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();

            var shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);

            if (shippingOption == null)
            {
                // Where there are items in the basket that are not for shipping, we need to ensure we return a zero.
                if (cart.Any(sci => sci.IsShipEnabled))
                    return null;
                else
                    return Decimal.Zero;
            }

            shippingTotal = basketResponse.DeliveryPrice;

            //if ((basketResponse.DeliveryPromotionDiscount > Decimal.Zero))
            //{
            //    if (basketResponse.BasketLevelDiscountIncludesDeliveryAmount())
            //    {
            //        shippingTotal = basketResponse.DeliveryOriginalPrice;
            //    }
            //    else
            //    {
            //        DiscountForCaching appliedDiscount = new DiscountForCaching();
            //        appliedDiscount.DiscountAmount = basketResponse.DeliveryPromotionDiscount;
            //        appliedDiscount.Name = basketResponse.DeliveryPromo().PromotionName;
            //        appliedDiscounts.Add(appliedDiscount);
            //    }
            //}

            #endregion

            #region new code

            decimal? shippingTotalTaxed = shippingTotal;

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





        ///// <summary>
        ///// Gets tax
        ///// </summary>
        ///// <param name="cart">Shopping cart</param>
        ///// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        ///// <returns>Tax total</returns>
        //public override decimal GetTaxTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        //{
        //    if (cart == null)
        //        throw new ArgumentNullException("cart");

        //    SortedDictionary<decimal, decimal> taxRates = null;
        //    return GetTaxTotal(cart, out taxRates, usePaymentMethodAdditionalFee);
        //}

        ///// <summary>
        ///// Gets tax
        ///// </summary>
        ///// <param name="cart">Shopping cart</param>
        ///// <param name="taxRates">Tax rates</param>
        ///// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        ///// <returns>Tax total</returns>
        //public override decimal GetTaxTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart,
        //out SortedDictionary<decimal, decimal> taxRates, bool usePaymentMethodAdditionalFee = true)
        //{
        //    if (cart == null)
        //        throw new ArgumentNullException("cart");

        //    return base.GetTaxTotal(cart, out taxRates, usePaymentMethodAdditionalFee);
        //}






        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
        public override decimal? GetShoppingCartTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart,
            bool? useRewardPonts = null, bool usePaymentMethodAdditionalFee = true)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartTotal(cart, useRewardPonts, usePaymentMethodAdditionalFee);

            if (cart == null || !cart.Any())
                return null;

            var customer = cart.GetCustomer();
            if (customer == null)
                return null;

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return base.GetShoppingCartTotal(cart, useRewardPonts, usePaymentMethodAdditionalFee);

            if (!basketResponse.Summary.ProcessingResult)
                return base.GetShoppingCartTotal(cart, useRewardPonts, usePaymentMethodAdditionalFee);

            decimal tax = GetTaxTotal(cart);

            return basketResponse.BasketTotal + tax;
        }

        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="appliedGiftCards">Applied gift cards</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscounts">Applied discounts</param>
        /// <param name="redeemedRewardPoints">Reward points to redeem</param>
        /// <param name="redeemedRewardPointsAmount">Reward points amount in primary store currency to redeem</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
        public override decimal? GetShoppingCartTotal(IList<global::Nop.Core.Domain.Orders.ShoppingCartItem> cart,
            out decimal discountAmount, out List<DiscountForCaching> appliedDiscounts,
            out List<AppliedGiftCard> appliedGiftCards,
            out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount,
            bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true)
        {
            if (!_promoSettings.Enabled)
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, useRewardPoints, usePaymentMethodAdditionalFee);

            if (cart == null | !cart.Any())
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, useRewardPoints, usePaymentMethodAdditionalFee);

            var customer = cart.GetCustomer();
            if (customer == null)
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, useRewardPoints, usePaymentMethodAdditionalFee);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
            {
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, useRewardPoints, usePaymentMethodAdditionalFee);
            }

            if (!basketResponse.Summary.ProcessingResult)
            {
                return base.GetShoppingCartTotal(cart, out discountAmount, out appliedDiscounts, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, useRewardPoints, usePaymentMethodAdditionalFee);
            }

            discountAmount = decimal.Zero;
            appliedDiscounts = new List<DiscountForCaching>();
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
                useRewardPoints.HasValue && useRewardPoints.Value &&
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

            var shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
            if (shippingOption == null)
            {
                // Where there are items in the basket that are not for shipping, ensure we carry on, otherwise placing orders gets stuck.
                if (cart.Any(sci => sci.IsShipEnabled))
                    return null;
            }

            discountAmount = GetOrderTotalDiscount(customer, basketResponse.BasketTotal, out appliedDiscounts);

            decimal tax = GetTaxTotal(cart);

            return basketResponse.BasketTotal + tax - appliedGiftCards.Sum(agc => agc.AmountCanBeUsed) - redeemedRewardPointsAmount;
        }

        /// <summary>
        /// Calculate how much reward points will be earned/reduced based on certain amount spent
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="amount">Amount (in primary store currency)</param>
        /// <returns>Number of reward points</returns>
        public override int CalculateRewardPoints(Customer customer, decimal amount)
        {
            if (!_promoSettings.Enabled)
                return base.CalculateRewardPoints(customer, amount);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(customer);
            return basketResponse.IssuedPoints();
        }

        #endregion
    }
}
