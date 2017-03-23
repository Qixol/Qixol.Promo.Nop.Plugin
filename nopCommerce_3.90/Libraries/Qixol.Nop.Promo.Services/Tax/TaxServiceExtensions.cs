using global::Nop.Core;
using global::Nop.Core.Domain.Common;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Tax;
using global::Nop.Core.Plugins;
using global::Nop.Services.Common;
using global::Nop.Services.Directory;
using global::Nop.Services.Tax;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Core.Domain.Shipping;

namespace Qixol.Nop.Promo.Services.Tax
{

    public class TaxServiceExtensions : global::Nop.Services.Tax.TaxService, ITaxServiceExtensions
    {
        #region fields

        private readonly PromoSettings _promoSettings;
        private readonly IPromoUtilities _promoUtilities;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region constructors

        public TaxServiceExtensions(IAddressService addressService,
            IWorkContext workContext,
            IStoreContext storeContext,
            TaxSettings taxSettings,
            IPluginFinder pluginFinder,
            IGeoLookupService geoLookupService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILogger logger,
            CustomerSettings customerSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings,
            PromoSettings promoSettings,
            //IpromoService promoService,
            IPromoUtilities promoUtilities,
            ITaxServiceExtensions taxServiceExtensions)
            : base(addressService, workContext, storeContext, taxSettings,
                                                    pluginFinder, geoLookupService, countryService, stateProvinceService,
                                                    logger, customerSettings, shippingSettings, addressSettings)
        {
            this._promoSettings = promoSettings;
            //this._promoService = promoService;
            this._promoUtilities = promoUtilities;
            this._taxSettings = taxSettings;
        }

        #endregion

        #region methods

        public decimal GetCheckoutAttributePrice(global::Nop.Core.Domain.Orders.CheckoutAttributeValue cav, bool includingTax, Customer customer, out decimal taxRate, bool includeDiscounts)
        {
            if (!_promoSettings.Enabled)
                return base.GetCheckoutAttributePrice(cav, includingTax, customer, out taxRate);

            if (cav == null)
                throw new ArgumentNullException("cav");

            taxRate = decimal.Zero;
            decimal price = cav.PriceAdjustment;

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            // checkout attribute value promos
            if (includeDiscounts && (basketResponse != null))
            {
                var checkoutAttributeItem = basketResponse.CheckoutAttributeItem(cav.CheckoutAttribute);
                if (checkoutAttributeItem != null)
                {
                    price = checkoutAttributeItem.LineAmount;
                }
            }

            if (cav.CheckoutAttribute.IsTaxExempt)
            {
                return price;
            }

            bool priceIncludesTax = _taxSettings.PricesIncludeTax;
            int taxClassId = cav.CheckoutAttribute.TaxCategoryId;
            return GetProductPrice(null, taxClassId, price, includingTax, customer,
                priceIncludesTax, out taxRate);
        }

        #endregion
    }
}
