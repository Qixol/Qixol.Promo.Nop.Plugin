using global::Nop.Core;
using global::Nop.Core.Domain.Common;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Tax;
using global::Nop.Core.Plugins;
using global::Nop.Services.Common;
using global::Nop.Services.Directory;
using global::Nop.Services.Tax;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Tax
{
    public class TaxService : global::Nop.Services.Tax.TaxService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IGeoLookupService _geoLookupService;
        private readonly ICountryService _countryService;
        private readonly CustomerSettings _customerSettings;
        private readonly AddressSettings _addressSettings;

        private readonly PromoSettings _promoSettings;
        //private readonly IpromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        private readonly ITaxServiceExtensions _taxServiceExtensions;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="addressService">Address service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="geoLookupService">GEO lookup service</param>
        /// <param name="countryService">Country service</param>
        /// <param name="customerSettings">Customer settings</param>
        /// <param name="addressSettings">Address settings</param>
        public TaxService(IAddressService addressService,
            IWorkContext workContext,
            TaxSettings taxSettings,
            IPluginFinder pluginFinder,
            IGeoLookupService geoLookupService,
            ICountryService countryService,
            CustomerSettings customerSettings,
            AddressSettings addressSettings,
            PromoSettings promoSettings,
            //IpromoService promoService,
            IPromoUtilities promoUtilities,
            ITaxServiceExtensions taxServiceExtensions)
            : base(addressService, workContext, taxSettings,
                                                    pluginFinder, geoLookupService, countryService,
                                                    customerSettings, addressSettings)
        {
            this._addressService = addressService;
            this._workContext = workContext;
            this._taxSettings = taxSettings;
            this._pluginFinder = pluginFinder;
            this._geoLookupService = geoLookupService;
            this._countryService = countryService;
            this._customerSettings = customerSettings;
            this._addressSettings = addressSettings;

            this._promoSettings = promoSettings;
            //this._promoService = promoService;
            this._promoUtilities = promoUtilities;
            this._taxServiceExtensions = taxServiceExtensions;
        }

        #endregion

        #region Methods

        public override decimal GetCheckoutAttributePrice(global::Nop.Core.Domain.Orders.CheckoutAttributeValue cav, bool includingTax, Customer customer, out decimal taxRate)
        {
            if (!_promoSettings.Enabled)
                return base.GetCheckoutAttributePrice(cav, includingTax, customer, out taxRate);

            return _taxServiceExtensions.GetCheckoutAttributePrice(cav, includingTax, customer, out taxRate, false);
        }

        #endregion
    }
}
