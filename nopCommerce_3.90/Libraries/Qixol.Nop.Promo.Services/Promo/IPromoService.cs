using global::Nop.Core.Domain.Stores;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Qixol.Promo.Integration.Lib.Basket;
using System.Collections.Generic;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoService
    {
        BasketResponse ProcessShoppingCart(Customer customer, int storeId);

        BasketResponse ProcessShoppingCart(Customer customer, int storeId, ShippingOption shippingOption);

        BasketResponse ProcessShoppingCart(Customer customer, int storeId, bool getMissedPromotions);

        BasketResponse ProcessShoppingCart(Customer customer, int storeId, bool getMissedPromotions, ShippingOption shippingOption);

        BasketResponse SendConfirmedBasket(global::Nop.Core.Domain.Orders.Order placedOrder);

    }
}
