using Nop.Core.Data;
using Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Orders
{
    public class PromoOrderService : IPromoOrderService
    {
        private readonly IRepository<PromoOrder> _promoOrderRepository;
        private readonly IRepository<PromoOrderItem> _promoOrderItemRepository;
        private readonly IRepository<PromoOrderItemPromotion> _promoOrderItemPromotionRepository;
        private readonly IEventPublisher _eventPublisher;

        public PromoOrderService(
            IRepository<PromoOrder> promoOrderRepository,
            IRepository<PromoOrderItem> promoOrderItemRepository,
            IRepository<PromoOrderItemPromotion> promoOrderItemPromotionRepository,
            IEventPublisher eventPublisher
            )
        {
            this._promoOrderRepository = promoOrderRepository;
            this._promoOrderItemRepository = promoOrderItemRepository;
            this._promoOrderItemPromotionRepository = promoOrderItemPromotionRepository;
            this._eventPublisher = eventPublisher;
        }

        public void InsertPromoOrder(PromoOrder promoOrder)
        {
            if (promoOrder == null)
                throw new ArgumentNullException("promoOrder");

            _promoOrderRepository.Insert(promoOrder);
            _eventPublisher.EntityInserted<PromoOrder>(promoOrder);
        }

        public void UpdatePromoOrder(PromoOrder promoOrder)
        {
            if (promoOrder == null)
                throw new ArgumentNullException("promoOrder");

            _promoOrderRepository.Update(promoOrder);
            _eventPublisher.EntityInserted<PromoOrder>(promoOrder);
        }


        public PromoOrder GetPromoOrderByOrderId(int orderId)
        {
            return (from po in _promoOrderRepository.Table where po.OrderId == orderId select po).FirstOrDefault();
        }
    }
}
