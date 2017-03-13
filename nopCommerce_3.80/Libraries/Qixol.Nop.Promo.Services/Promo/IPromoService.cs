using global::Nop.Core.Domain.Stores;
using System.Collections.Generic;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoService
    {
        List<string> ProcessShoppingCart();

        List<string> ProcessShoppingCart(bool getMissedPromotions);

        void SendConfirmedBasket(global::Nop.Core.Domain.Orders.Order placedOrder);

    }
}
