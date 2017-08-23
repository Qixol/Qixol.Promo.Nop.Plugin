using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Nop.Promo.Services.Promo;
using Nop.Services.Customers;
using Nop.Services.Orders;
using XcellenceIt.Plugin.Misc.NopRestApi.Service;
using XcellenceIt.Plugin.Misc.NopRestApi.DataClass;
using Nop.Core.Domain.Directory;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Core.Domain.Localization;
using Nop.Services.Localization;
using Microsoft.Owin.BuilderProperties;

namespace Qixol.Plugin.Misc.RestApi.Service
{
    public class RestServices : XcellenceIt.Plugin.Misc.NopRestApi.Service.RestServices, XcellenceIt.Plugin.Misc.NopRestApi.Service.IRestServices
    {
        public new ShoppingCartModelResponse Cart(string apiSecretKey, int storeId, int currencyId, int customerId, int languageId)
        {

            IPromoService promoService = EngineContext.Current.Resolve<IPromoService>();
            ICustomerService customerService = EngineContext.Current.Resolve<ICustomerService>();

            var customer = customerService.GetCustomerById(customerId);

            // TODO: we really should have the guest customer by this point
            // If we create the guest they'll get an empty cart anyway
            if (customer == null)
                customer = customerService.InsertGuestCustomer();

            if (customer != null)
                promoService.ProcessShoppingCart(customer);

            return base.Cart(apiSecretKey, storeId, currencyId, customerId, languageId);
        }

        public new CheckoutPaymentMethodResponse SelectShippingMethod(string apiSecretKey, int storeId, int customerId, int currencyId, int languageId, string shippingoption)
        {
            // use the base method to set the selected shipping method and capture the JSON return
            var checkoutPaymentMethodResponse = base.SelectShippingMethod(apiSecretKey, storeId, customerId, currencyId, languageId, shippingoption);

            // now update the promo basket using the (now) selected shipping method
            IPromoService promoService = EngineContext.Current.Resolve<IPromoService>();
            ICustomerService customerService = EngineContext.Current.Resolve<ICustomerService>();

            var customer = customerService.GetCustomerById(customerId);
            if (customer != null)
                promoService.ProcessShoppingCart(customer);

            return checkoutPaymentMethodResponse;
        }

        public new EstimateShippingResponse EstimateShipping(string apiSecretKey, int storeId, int customerId, int currencyId, EstimateShippingResponse shippingModel)
        {
            var estimateShippingResponse = base.EstimateShipping(apiSecretKey, storeId, customerId, currencyId, shippingModel);

            return estimateShippingResponse;
        }
    }
}
