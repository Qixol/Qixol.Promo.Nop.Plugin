using global::Nop.Core;
using global::Nop.Core.Caching;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Orders;
using global::Nop.Services.Catalog;
using global::Nop.Services.Discounts;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Nop.Promo.Services.Localization;
using Nop.Services.Logging;

namespace Qixol.Nop.Promo.Services.Catalog
{
    public partial class PromosPriceCalculationService : global::Nop.Services.Catalog.PriceCalculationService, IPromosPriceCalculationService
    {
        #region Fields

        private readonly IPromoUtilities _promoUtilities;
        private readonly PromoSettings _promoSettings;

        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
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

        public PromosPriceCalculationService(
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
            PromoSettings promoSettings,
            ICurrencyService currencyService,
            IPriceCalculationService priceCalculationService,
            ILocalizationService localizationService,
            ILogger logger)
            : base (workContext, storeContext, discountService,
                    categoryService, manufacturerService, productAttributeParser,
                    productService, cacheManager, shoppingCartSettings,
                    catalogSettings)
        {
            this._promoUtilities = promoUtilities;
            this._promoSettings = promoSettings;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._localizationService = localizationService;
            this._logger = logger;
        }

        #endregion

        #region methods

        #region GetFinalPrice

        public override decimal GetFinalPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity, DateTime? rentalStartDate, DateTime? rentalEndDate, out decimal discountAmount, out global::Nop.Core.Domain.Discounts.Discount appliedDiscount)
        {
            if (!_promoSettings.Enabled)
                return base.GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscount);

            discountAmount = 0M;
            appliedDiscount = null;

            bool includeDiscountsInBaseCall = false;

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || basketResponse.Summary == null || !basketResponse.Summary.ProcessingResult)
                return base.GetFinalPrice(product, customer, additionalCharge, includeDiscountsInBaseCall, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscount);

            if (basketResponse.Items == null || basketResponse.Items.Count == 0)
                return base.GetFinalPrice(product, customer, additionalCharge, includeDiscountsInBaseCall, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscount);

            var basketResponseProducts = basketResponse.FindBasketResponseItems(product, string.Empty);

            if (basketResponseProducts == null || basketResponseProducts.Count == 0)
                return base.GetFinalPrice(product, customer, additionalCharge, includeDiscountsInBaseCall, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscount);

            return basketResponseProducts.Sum(bri => bri.LineAmount);
        }

        public override decimal GetFinalPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity, out decimal discountAmount, out global::Nop.Core.Domain.Discounts.Discount appliedDiscount)
        {
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, rentalStartDate, rentalEndDate, out discountAmount, out appliedDiscount);
        }

        public override decimal GetFinalPrice(Product product, global::Nop.Core.Domain.Customers.Customer customer, decimal additionalCharge = 0m, bool includeDiscounts = true, int quantity = 1)
        {
            decimal discountAmount = 0M;
            global::Nop.Core.Domain.Discounts.Discount appliedDiscount = null;
            return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, out discountAmount, out appliedDiscount);
        }

        #endregion

        #region GetSubTotal

        public override decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts, out decimal discountAmount, out global::Nop.Core.Domain.Discounts.Discount appliedDiscount)
        {
            discountAmount = 0M;
            appliedDiscount = null;
            decimal lineSubTotal = _priceCalculationService.GetUnitPrice(shoppingCartItem, false) * shoppingCartItem.Quantity;

            if (!_promoSettings.Enabled)
                return base.GetSubTotal(shoppingCartItem, includeDiscounts, out discountAmount, out appliedDiscount);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            if(!basketResponse.IsValid())
                return lineSubTotal;

            discountAmount = basketResponse.GetLineDiscountAmount(shoppingCartItem);
            if (discountAmount != decimal.Zero)
            {
                if (discountAmount <= lineSubTotal)
                {
                    appliedDiscount = new global::Nop.Core.Domain.Discounts.Discount()
                    {
                        Name = string.Join(", ", basketResponse.LineDiscountNames(shoppingCartItem)
                                                               .Select(n => _localizationService.GetValidatedResource(n))),
                        DiscountAmount = discountAmount
                    };

                    lineSubTotal -= discountAmount;
                }
                else
                {
                    string shortMessage = "PriceCalculationService - GetSubTotal";
                    string fullMessage = string.Format("id: {0}, productId: {1}, attributesXml: {2}, basketResponseXml: {3}", shoppingCartItem.Id, shoppingCartItem.ProductId, shoppingCartItem.AttributesXml, basketResponse.ToXml());
                    _logger.InsertLog(global::Nop.Core.Domain.Logging.LogLevel.Error, shortMessage, fullMessage, _workContext.CurrentCustomer);
                }
            }

            return lineSubTotal;
        }

        public override decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts = true)
        {
            decimal discountAmount;
            global::Nop.Core.Domain.Discounts.Discount appliedDiscount;
            return GetSubTotal(shoppingCartItem, includeDiscounts, out discountAmount, out appliedDiscount);
        }

        #endregion

        #endregion
    }
}
