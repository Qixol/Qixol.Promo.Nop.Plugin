﻿using global::Nop.Core;
using global::Nop.Core.Caching;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Orders;
using global::Nop.Services.Catalog;
using global::Nop.Services.Discounts;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Nop.Promo.Services.ShoppingCart;

namespace Qixol.Nop.Promo.Services.Catalog
{
    public partial class PriceCalculationService : global::Nop.Services.Catalog.PriceCalculationService, IPriceCalculationService
    {
        #region Fields

        private readonly IPromoUtilities _promoUtilities;
        private readonly PromoSettings _promoSettings;

        //private readonly IWorkContext _workContext;
        //private readonly IStoreContext _storeContext;
        //private readonly IDiscountService _discountService;
        //private readonly ICategoryService _categoryService;
        //private readonly IProductAttributeParser _productAttributeParser;
        //private readonly IProductService _productService;
        //private readonly ICacheManager _cacheManager;
        //private readonly ShoppingCartSettings _shoppingCartSettings;
        //private readonly CatalogSettings _catalogSettings;

        #endregion

        #region constructor

        public PriceCalculationService(
            IPromoUtilities promoUtilities,
            IWorkContext workContext,
            IStoreContext storeContext,
            IDiscountService discountService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            ICacheManager cacheManager,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings,
            PromoSettings promoSettings)
            : base(workContext, storeContext, discountService, categoryService,
            manufacturerService, productAttributeParser,
            productService, cacheManager, shoppingCartSettings,
            catalogSettings)
        {
            this._promoUtilities = promoUtilities;
            this._promoSettings = promoSettings;
        }

        #endregion

        #region methods

        //#region GetFinalPrice

        //public override decimal GetFinalPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, decimal additionalCharge, bool includeDiscounts,
        //    int quantity, DateTime? rentalStartDate, DateTime? rentalEndDate, out decimal discountAmount, out List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts)
        //{
        //    if (!_promoSettings.Enabled)
        //        return base.GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts);

        //    discountAmount = 0M;
        //    appliedDiscounts = new List<global::Nop.Services.Discounts.DiscountForCaching>();

        //    bool includeDiscountsInBaseCall = false;

        //    BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

        //    if (basketResponse == null || basketResponse.Summary == null || !basketResponse.Summary.ProcessingResult)
        //        return base.GetFinalPrice(product, customer, additionalCharge, includeDiscountsInBaseCall, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts);

        //    if (basketResponse.Items == null || basketResponse.Items.Count == 0)
        //        return base.GetFinalPrice(product, customer, additionalCharge, includeDiscountsInBaseCall, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts);

        //    var basketResponseProducts = basketResponse.FindBasketResponseItems(product, string.Empty);

        //    if (basketResponseProducts == null || basketResponseProducts.Count == 0)
        //        return base.GetFinalPrice(product, customer, additionalCharge, includeDiscountsInBaseCall, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts);

        //    return basketResponseProducts.Sum(bri => bri.LineAmount);
        //}

        //public override decimal GetFinalPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity,
        //    out decimal discountAmount, out List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts)
        //{
        //    DateTime? rentalStartDate = null;
        //    DateTime? rentalEndDate = null;
        //    return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscounts);
        //}

        //public override decimal GetFinalPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, decimal additionalCharge = 0m, bool includeDiscounts = true,
        //    int quantity = 1)
        //{
        //    decimal discountAmount = 0M;
        //    List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts = new List<global::Nop.Services.Discounts.DiscountForCaching>();
        //    return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, out discountAmount, out appliedDiscounts);
        //}

        //#endregion

        #region GetSubTotal

        public override decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts, out decimal discountAmount,
            out List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts, out int? maximumDiscountQty)
        {
            if (!_promoSettings.Enabled)
                return base.GetSubTotal(shoppingCartItem, false, out discountAmount, out appliedDiscounts, out maximumDiscountQty);
            
            if (shoppingCartItem == null || shoppingCartItem.Customer == null)
                return base.GetSubTotal(shoppingCartItem, false, out discountAmount, out appliedDiscounts, out maximumDiscountQty);

            var promotions = new List<DiscountForCaching>();
            shoppingCartItem.Promotions().ToList().ForEach(p =>
            {
                promotions.Add(new DiscountForCaching()
                {
                    DiscountAmount = p.DiscountAmount,
                    Name = p.DisplayDetails(shoppingCartItem.Customer)
                });
            });

            appliedDiscounts = new List<DiscountForCaching>();
            appliedDiscounts.AddRange(promotions);

            // TODO: does this need to be set...?
            maximumDiscountQty = null;

            discountAmount = shoppingCartItem.LineDiscountAmount();

            return shoppingCartItem.LineAmount();
        }

        public override decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts = true)
        {
            decimal discountAmount;
            List<DiscountForCaching> appliedDiscounts;
            int? maximumDiscountQty;
            return GetSubTotal(shoppingCartItem, includeDiscounts, out discountAmount, out appliedDiscounts, out maximumDiscountQty);
        }

        #endregion

        //#region GetUnitPrice

        //public override decimal GetUnitPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, ShoppingCartType shoppingCartType,
        //    int quantity, string attributesXml, decimal customerEnteredPrice, DateTime? rentalStartDate, DateTime? rentalEndDate, bool includeDiscounts,
        //    out decimal discountAmount, out List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts)
        //{
        //    return base.GetUnitPrice(product, customer, shoppingCartType, quantity, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate, false,
        //        out discountAmount, out appliedDiscounts);
        //}

        //public override decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts, out decimal discountAmount,
        //    out List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts)
        //{
        //    if (shoppingCartItem == null)
        //        throw new ArgumentNullException("shoppingCartItem");

        //    return GetUnitPrice(shoppingCartItem.Product,
        //        shoppingCartItem.Customer,
        //        shoppingCartItem.ShoppingCartType,
        //        shoppingCartItem.Quantity,
        //        shoppingCartItem.AttributesXml,
        //        shoppingCartItem.CustomerEnteredPrice,
        //        shoppingCartItem.RentalStartDateUtc,
        //        shoppingCartItem.RentalEndDateUtc,
        //        false,
        //        out discountAmount,
        //        out appliedDiscounts);
        //}

        //public override decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts = false)
        //{
        //    decimal discountAmount = 0M;
        //    List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts;
        //    return GetUnitPrice(shoppingCartItem, false, out discountAmount, out appliedDiscounts);
        //}

        //#endregion

        #endregion
    }
}
