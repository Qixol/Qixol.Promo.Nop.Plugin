using Qixol.Nop.Promo.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Orders
{
    public interface IPromoOrderService
    {
        void InsertPromoOrder(PromoOrder promoOrder);
        void UpdatePromoOrder(PromoOrder promoOrder);

        PromoOrder GetPromoOrderByOrderId(int orderId);
    }
}
