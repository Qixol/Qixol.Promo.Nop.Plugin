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

namespace Qixol.Nop.Promo.Services.Orders
{
    public static class OrderItemExtensions
    {
        #region Utilities

        private static IList<PromoOrderItem> MatchedPromoOrderItems(this OrderItem orderItem)
        {
            var matchedPromoOrderItems = new List<PromoOrderItem>();

            if (orderItem == null)
                return matchedPromoOrderItems;

            IProductMappingService productMappingService = EngineContext.Current.Resolve<IProductMappingService>();
            IPromoOrderService promoOrderService = EngineContext.Current.Resolve<IPromoOrderService>();

            var promoOrder = promoOrderService.GetPromoOrderByOrderId(orderItem.OrderId);

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromAttributesXml(orderItem);
            if (promoOrder != null && productMappingItem != null)
            {
                matchedPromoOrderItems = (from oi in promoOrder.PromoOrderItems where oi.ProductCode == productMappingItem.EntityId.ToString() && oi.VariantCode == productMappingItem.VariantCode select oi).ToList();
            }

            return matchedPromoOrderItems;
        }

        #endregion

        #region methods

        public static Decimal LineTotalDiscount(this OrderItem orderItem)
        {
            if (orderItem == null)
                return decimal.Zero;

            var promoOrderItems = orderItem.MatchedPromoOrderItems();

            var lineDiscount = decimal.Zero;

            if (promoOrderItems != null)
            {
                promoOrderItems.ToList().ForEach(poi =>
                {
                    lineDiscount += poi.LinePromotionDiscount;

                    // convert Free Product at basket level into Free Product at line level
                    poi.PromoOrderItemPromotions.ToList().ForEach(poip =>
                    {
                        if (poip.BasketLevel && !poip.DeliveryLevel && (poip.DiscountAmount > decimal.Zero) && poip.PromotionType.Equals(PromotionTypeName.FreeProduct))
                        {
                            lineDiscount -= poip.DiscountAmount;
                        }
                    });
                });
            }

            return lineDiscount;
        }

        public static Decimal LineTotalAmount(this OrderItem orderItem)
        {
            if (orderItem == null)
                return decimal.Zero;

            var promoOrderItems = orderItem.MatchedPromoOrderItems();

            var lineAmount = decimal.Zero;

            if (promoOrderItems != null)
            {
                promoOrderItems.ToList().ForEach(poi =>
                {
                    lineAmount += poi.LineAmount;

                    // convert Free Product at basket level into Free Product at line level
                    poi.PromoOrderItemPromotions.ToList().ForEach(poip =>
                    {
                        if (poip.BasketLevel && !poip.DeliveryLevel && (poip.DiscountAmount > decimal.Zero) && poip.PromotionType.Equals(PromotionTypeName.FreeProduct))
                        {
                            lineAmount -= poip.DiscountAmount;
                        }
                    });
                });
            }

            return lineAmount;
        }

        public static IList<PromoOrderItemPromotion> Promotions(this OrderItem orderItem)
        {
            var promotions = new List<PromoOrderItemPromotion>();

            if (orderItem == null)
                return promotions;

            var promoOrderItems = orderItem.MatchedPromoOrderItems();
            if (promoOrderItems != null)
            {
                promoOrderItems.ToList().ForEach(poi =>
                {
                    poi.PromoOrderItemPromotions.ToList().ForEach(poip =>
                    {
                        if (!poip.BasketLevel && !poip.DeliveryLevel && (poip.DiscountAmount > decimal.Zero))
                        {
                            var existingPromo = (from ep in promotions where ep.PromotionId == poip.PromotionId select ep).FirstOrDefault();
                            if (existingPromo != null)
                            {
                                existingPromo.DiscountAmount += poip.DiscountAmount;
                            }
                            else
                            {
                                promotions.Add(poip);
                            }
                        }
                        else
                        {
                            // free products
                            if (poip.BasketLevel && poip.DiscountAmount > decimal.Zero && poip.PromotionType.Equals(PromotionTypeName.FreeProduct))
                            {
                                var existingPromo = (from ep in promotions where ep.PromotionId == poip.PromotionId select ep).FirstOrDefault();
                                if (existingPromo != null)
                                {
                                    existingPromo.DiscountAmount += poip.DiscountAmount;
                                }
                                else
                                {
                                    promotions.Add(poip);
                                }
                            }
                        }
                    });
                });
            }

            return promotions;
        }

        #endregion
    }
}
