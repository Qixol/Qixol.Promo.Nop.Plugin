using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qixol.Nop.Promo.Services.ShoppingCart;
using Nop.Services.Orders;
using Nop.Services.Common;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Shipping;

namespace Qixol.Nop.Promo.Services.Promo
{
    public static class BasketRequestExtensions
    {
        #region extension methods

        public static BasketRequest SetShipping(this BasketRequest basketRequest, IList<ShoppingCartItem> cart, ShippingOption shippingOption = null)
        {
            IGenericAttributeService _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            IShippingService _shippingService = EngineContext.Current.Resolve<IShippingService>();
            IWorkContext _workContext = EngineContext.Current.Resolve<IWorkContext>();
            IStoreContext _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            ICountryService _countryService = EngineContext.Current.Resolve<ICountryService>();
            IStateProvinceService _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            IOrderTotalCalculationService _orderTotalCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();
            IAttributeValueService _attributeValueService = EngineContext.Current.Resolve<IAttributeValueService>();
            PromoSettings _promoSettings = EngineContext.Current.Resolve<PromoSettings>();
            ICurrencyService _currencyService = EngineContext.Current.Resolve<ICurrencyService>();

            var deliveryPrice = decimal.Zero;
            var deliveryMethod = string.Empty;
            var shippingIntegrationCode = string.Empty;

            if (cart.RequiresShipping())
            {
                if (shippingOption == null)
                    shippingOption = GetDefaultShippingOption(_shippingService, _workContext, _storeContext, _countryService, _stateProvinceService, _genericAttributeService);

                string shippingOptionName = (shippingOption != null ? shippingOption.Name : string.Empty);

                shippingIntegrationCode = shippingOptionName;

                // Is there an Integration code for the specified shipping option?
                IList<ShippingMethod> shippingMethods = _shippingService.GetAllShippingMethods();
                var specifiedShippingMethod = (from sm in shippingMethods where sm.Name.Equals(shippingOptionName, StringComparison.InvariantCultureIgnoreCase) select sm).FirstOrDefault();

                if (specifiedShippingMethod == null)
                    specifiedShippingMethod = (from sm in shippingMethods where sm.Name.Equals(shippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase) select sm).FirstOrDefault();

                if (specifiedShippingMethod != null)
                {
                    // TODO: why is there a namespace issue here?
                    Qixol.Nop.Promo.Core.Domain.AttributeValues.AttributeValueMappingItem integrationMappingItem = _attributeValueService.Retrieve(specifiedShippingMethod.Id, EntityAttributeName.DeliveryMethod);
                    if (integrationMappingItem != null && !string.IsNullOrEmpty(integrationMappingItem.Code))
                        shippingIntegrationCode = integrationMappingItem.Code;
                }

                List<global::Nop.Services.Discounts.DiscountForCaching> appliedDiscounts;
                deliveryPrice = _orderTotalCalculationService.AdjustShippingRate(shippingOption != null ? shippingOption.Rate : 0M, cart, out appliedDiscounts);

                // DM Cope with baskets in current currency
                if (_promoSettings.UseSelectedCurrencyWhenSubmittingBaskets && _workContext.WorkingCurrency.Rate != 1)
                    deliveryPrice = _currencyService.ConvertFromPrimaryExchangeRateCurrency(deliveryPrice, _workContext.WorkingCurrency);

                deliveryMethod = shippingIntegrationCode;
            }

            basketRequest.BasketTotal += deliveryPrice;
            basketRequest.DeliveryPrice = deliveryPrice;
            basketRequest.DeliveryMethod = shippingIntegrationCode;

            return basketRequest;

        }

        #endregion

        #region helpers

        private static ShippingOption GetDefaultShippingOption(
            IShippingService shippingService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IGenericAttributeService genericAttributeService)
        {
            // TODO: set these values in the config? - like EstimateShipping but default values are provided?
            int countryId = 80; // UK
            int? stateProvinceId = null;
            string zipPostalCode = "SB2 8BW";
            Address address = new Address
            {
                CountryId = countryId,
                Country = countryService.GetCountryById(countryId),
                StateProvinceId = stateProvinceId,
                StateProvince = stateProvinceId.HasValue ? stateProvinceService.GetStateProvinceById(stateProvinceId.Value) : null,
                ZipPostalCode = zipPostalCode,
            };

            if (workContext.CurrentCustomer.ShippingAddress != null)
            {
                address = workContext.CurrentCustomer.ShippingAddress;
            }

            List<ShoppingCartItem> cart = workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(storeContext.CurrentStore.Id)
                .ToList();

            GetShippingOptionResponse shippingOptionResponse = shippingService.GetShippingOptions(cart, address);

            ShippingOption selectedShippingOption = shippingOptionResponse.ShippingOptions.FirstOrDefault();
            genericAttributeService.SaveAttribute(workContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedShippingOption, selectedShippingOption, storeContext.CurrentStore.Id);

            if (selectedShippingOption == null)
                selectedShippingOption = new ShippingOption()
                {
                    Name = "UNSELECTED",
                    Rate = 0M,
                    Description = "no shipping option selected",
                    ShippingRateComputationMethodSystemName = string.Empty
                };

            return selectedShippingOption;
        }

        #endregion
    }
}
