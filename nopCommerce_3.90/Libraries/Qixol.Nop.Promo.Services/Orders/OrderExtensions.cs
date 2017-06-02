using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using global::Nop.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.ProductMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Promo.Integration.Lib.Basket;

namespace Qixol.Nop.Promo.Services.Orders
{
    public static class OrderExtensions
    {
        #region Utilities

        #endregion

        #region methods

        // NOTE: ALL promotions are held in the PromoOrderItemPromotion table and flagged as basket/delivery etc.
        public static List<PromoOrderItemPromotion> BasketLevelPromotions(this Order order)
        {
            var basketLevelPromotions = new List<PromoOrderItemPromotion>();

            if (order == null)
                return basketLevelPromotions;

            var promoOrderService = EngineContext.Current.Resolve<IPromoOrderService>();

            var promoOrder = promoOrderService.GetPromoOrderByOrderId(order.Id);

            if (promoOrder == null)
                return basketLevelPromotions;

            List<PromoOrderItem> promoOrderItems = (from poi in promoOrder.PromoOrderItems select poi).ToList();

            promoOrderItems.ForEach(poi =>
            {
                poi.PromoOrderItemPromotions.Where(
                    poip => poip.BasketLevel && !poip.DeliveryLevel
                        && !poip.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase)
                        && !poip.PromotionType.Equals("ISSUECOUPON", StringComparison.InvariantCultureIgnoreCase)
                        && !poip.PromotionType.Equals("ISSUEPOINTS", StringComparison.InvariantCultureIgnoreCase)).ToList().ForEach(poip =>
                {
                    var existingPromo = (from blp in basketLevelPromotions where blp.PromotionId == poip.PromotionId select blp).FirstOrDefault();
                    if (existingPromo != null)
                    {
                        existingPromo.DiscountAmount += poip.DiscountAmount;
                    }
                    else
                    {
                        basketLevelPromotions.Add(poip);
                    }
                });
            });

            return basketLevelPromotions;
        }

        public static IList<PromoOrderCoupon> PromoIssuedCoupons(this Order order)
        {
            var promoIssuedCoupons = new List<PromoOrderCoupon>();

            if (order == null)
                return promoIssuedCoupons;

            var promoOrderService = EngineContext.Current.Resolve<IPromoOrderService>();
            var promoOrder = promoOrderService.GetPromoOrderByOrderId(order.Id);

            if (promoOrder == null)
                return promoIssuedCoupons;

            if (promoOrder.PromoOrderCoupons == null)
                return promoIssuedCoupons;

            return promoOrder.PromoOrderCoupons.Where(c => c.Issued).ToList();
        }

        public static IList<PromoOrderItemPromotion> DeliveryPromotions(this Order order)
        {
            var deliveryPromotions = new List<PromoOrderItemPromotion>();

            if (order == null)
                return deliveryPromotions;

            var promoOrderService = EngineContext.Current.Resolve<IPromoOrderService>();

            var promoOrder = promoOrderService.GetPromoOrderByOrderId(order.Id);

            if (promoOrder == null)
                return deliveryPromotions;

            List<PromoOrderItem> promoOrderItems = (from poi in promoOrder.PromoOrderItems where poi.IsDelivery select poi).ToList();

            promoOrderItems.ForEach(poi => {
                poi.PromoOrderItemPromotions.Where(poip => poip.DeliveryLevel).ToList().ForEach(poip =>
                {
                    var existingPromo = (from dp in deliveryPromotions where dp.PromotionId == poip.PromotionId select dp).FirstOrDefault();
                    if (existingPromo != null)
                    {
                        existingPromo.DiscountAmount += poip.DiscountAmount;
                    }
                    else
                    {
                        deliveryPromotions.Add(poip);
                    }
                });
            });

            return deliveryPromotions;
        }

        public static decimal TotalDiscount(this Order order)
        {
            var totalDiscount = decimal.Zero;

            if (order == null)
                return totalDiscount;

            var promoOrderService = EngineContext.Current.Resolve<IPromoOrderService>();

            var promoOrder = promoOrderService.GetPromoOrderByOrderId(order.Id);

            if (promoOrder == null)
                return totalDiscount;

            var basketResponse = BasketResponse.FromXml(promoOrder.ResponseXml);

            if (basketResponse == null)
                return totalDiscount;

            totalDiscount = basketResponse.TotalDiscount;

            return totalDiscount;
        }

        #endregion
    }
}
