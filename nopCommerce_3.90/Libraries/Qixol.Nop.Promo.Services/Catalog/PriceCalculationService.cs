using global::Nop.Core;
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

        #endregion
    }
}
