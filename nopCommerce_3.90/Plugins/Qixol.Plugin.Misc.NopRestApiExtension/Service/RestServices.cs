using Nop.Core;
using Nop.Core.Infrastructure;
using Qixol.Nop.Promo.Services.Promo;
using Nop.Services.Customers;
using XcellenceIt.Plugin.Misc.NopRestApi.DataClass;

namespace Qixol.Plugin.Misc.RestApi.Service
{
    public partial class RestServices : XcellenceIt.Plugin.Misc.NopRestApi.Service.RestServices, XcellenceIt.Plugin.Misc.NopRestApi.Service.IRestServices
    {
        public new ShoppingCartModelResponse Cart(string apiSecretKey, int storeId, int currencyId, int customerId, int languageId)
        {

            IPromoService promoService = EngineContext.Current.Resolve<IPromoService>();
            ICustomerService customerService = EngineContext.Current.Resolve<ICustomerService>();

            var customer = customerService.GetCustomerById(customerId);

            if (customer == null)
                customer = customerService.InsertGuestCustomer();

            if (customer != null)
                promoService.ProcessShoppingCart(customer, storeId);

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
                promoService.ProcessShoppingCart(customer, storeId);

            return checkoutPaymentMethodResponse;
        }

        public new EstimateShippingResponse EstimateShipping(string apiSecretKey, int storeId, int customerId, int currencyId, EstimateShippingResponse shippingModel)
        {
            var estimateShippingResponse = base.EstimateShipping(apiSecretKey, storeId, customerId, currencyId, shippingModel);

            return estimateShippingResponse;
        }

        public new DiscountBoxResponse ApplyDiscount(string apiSecretKey, int storeId, int customerId, string discountCouponCode)
        {
            IPromoService promoService = EngineContext.Current.Resolve<IPromoService>();
            ICustomerService customerService = EngineContext.Current.Resolve<ICustomerService>();
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();

            var customer = customerService.GetCustomerById(customerId);

            if (customer != null)
            {
                workContext.CurrentCustomer = customer;
                customer.ApplyDiscountCouponCode(discountCouponCode);
                promoService.ProcessShoppingCart(customer, storeId);
            }

            return base.ApplyDiscount(apiSecretKey, storeId, customerId, discountCouponCode);
        }

        public new DiscountBoxResponse RemoveDiscount(string apiSecretKey, int storeId, int customerId, string discountCouponCode)
        {
            IPromoService promoService = EngineContext.Current.Resolve<IPromoService>();
            ICustomerService customerService = EngineContext.Current.Resolve<ICustomerService>();
            IWorkContext workContext = EngineContext.Current.Resolve<IWorkContext>();

            var customer = customerService.GetCustomerById(customerId);

            if (customer != null)
            {
                workContext.CurrentCustomer = customer;
                customer.RemoveDiscountCouponCode(discountCouponCode);
                promoService.ProcessShoppingCart(customer, storeId);
            }

            return base.RemoveDiscount(apiSecretKey, storeId, customerId, discountCouponCode);
        }
    }
}
