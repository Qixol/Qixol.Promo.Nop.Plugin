using global::Nop.Core.Domain.Stores;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using System.Collections.Generic;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoService
    {
        List<string> ProcessShoppingCart(Customer customer);

        List<string> ProcessShoppingCart(Customer customer, ShippingOption shippingOption);

        List<string> ProcessShoppingCart(Customer customer, bool getMissedPromotions);

        List<string> ProcessShoppingCart(Customer customer, bool getMissedPromotions, ShippingOption shippingOption);

        void SendConfirmedBasket(global::Nop.Core.Domain.Orders.Order placedOrder);

    }
}
