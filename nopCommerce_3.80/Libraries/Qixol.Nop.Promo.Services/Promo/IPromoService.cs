using global::Nop.Core.Domain.Stores;
using Nop.Core.Domain.Shipping;
using System.Collections.Generic;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoService
    {
        List<string> ProcessShoppingCart();

        List<string> ProcessShoppingCart(ShippingOption shippingOption);

        List<string> ProcessShoppingCart(bool getMissedPromotions);

        List<string> ProcessShoppingCart(bool getMissedPromotions, ShippingOption shippingOption);

        void SendConfirmedBasket(global::Nop.Core.Domain.Orders.Order placedOrder);

    }
}
