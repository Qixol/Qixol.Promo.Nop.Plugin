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

namespace Qixol.Nop.Promo.Services.Orders
{
    public static class PromoOrderExtensions
    {
        public static PromoOrderItemPromotion BasketLevelPromotion(this PromoOrder promoOrder)
        {
            if (promoOrder == null)
                return null;

            List<PromoOrderItem> promoOrderItems = (from poi in promoOrder.PromoOrderItems select poi).ToList();
                                                
            List<PromoOrderItemPromotion> promoOrderItemPromotions = new List<PromoOrderItemPromotion>();

            promoOrderItems.ForEach(poi => {
                promoOrderItemPromotions.AddRange(poi.PromoOrderItemPromotions
                    .Where(poip => poip.BasketLevel && !poip.DeliveryLevel
                        && !poip.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase)
                        && !poip.PromotionType.Equals("ISSUECOUPON", StringComparison.InvariantCultureIgnoreCase)
                        && !poip.PromotionType.Equals("ISSUEPOINTS", StringComparison.InvariantCultureIgnoreCase)).ToList());
                    });

            if (promoOrderItemPromotions.Count == 0)
                return null;

            // TODO: there should only be 1 basket level promotion - check for this?
            return promoOrderItemPromotions.FirstOrDefault();
        }

        public static bool HasDeliveryDiscount(this PromoOrder promoOrder)
        {
            if (promoOrder == null)
                return false;

            List<PromoOrderItem> promoOrderItems = (from poi in promoOrder.PromoOrderItems select poi).ToList();

            List<PromoOrderItemPromotion> promoOrderItemPromotions = new List<PromoOrderItemPromotion>();

            promoOrderItems.ForEach(poi =>
            {
                promoOrderItemPromotions.AddRange(poi.PromoOrderItemPromotions
                    .Where(poip => poip.DeliveryLevel).ToList());
            });

            if (promoOrderItemPromotions.Count > 0)
                return true;

            return false;
        }

        public static decimal GetDeliveryPromoDiscount(this PromoOrder promoOrder)
        {
            if (promoOrder == null)
                return decimal.Zero;

            var deliveryPromo = promoOrder.DeliveryPromo();
            if (deliveryPromo != null)
                return deliveryPromo.DiscountAmount;
            else
                return decimal.Zero;
        }

        public static PromoOrderItemPromotion DeliveryPromo(this PromoOrder promoOrder)
        {
            if (promoOrder == null)
                return null;

            PromoOrderItemPromotion deliveryPromo = null;

            if (!promoOrder.HasDeliveryDiscount())
                return deliveryPromo;

            List<PromoOrderItem> promoOrderItems = (from poi in promoOrder.PromoOrderItems where poi.IsDelivery select poi).ToList();

            List<PromoOrderItemPromotion> promoOrderItemPromotions = new List<PromoOrderItemPromotion>();

            promoOrderItems.ForEach(poi =>
            {
                promoOrderItemPromotions.AddRange(poi.PromoOrderItemPromotions
                    .Where(poip => poip.BasketLevel && poip.DeliveryLevel).ToList());
            });

            if (promoOrderItemPromotions == null)
                return null;

            return promoOrderItemPromotions.FirstOrDefault();
        }

        public static string GetDeliveryPromoName(this PromoOrder promoOrder, PromoSettings promoSettings)
        {
            if (promoOrder == null)
                return string.Empty;

            var deliveryPromo = promoOrder.DeliveryPromo();
            if (deliveryPromo != null)
                return GetDisplayPromoDetails(promoSettings.ShowPromotionDetailsInBasket, deliveryPromo);

            return string.Empty;
        }

        public static string GetDisplayPromoDetails(int displaySetting, PromoOrderItemPromotion promo)
        {
            switch (displaySetting)
            {
                case PromotionDetailsDisplayOptions.ShowEndUserText:
                    // The display text is not mandatory, so default it to the promotion type if there is no text.
                    if (!string.IsNullOrEmpty(promo.DisplayText))
                        return promo.DisplayText;
                    else
                        return promo.PromotionTypeDisplay;

                case PromotionDetailsDisplayOptions.ShowPromotionName:
                    return promo.PromotionName;

                case PromotionDetailsDisplayOptions.ShowNoText:
                    return string.Empty;

                default:
                    return promo.PromotionTypeDisplay;
            }
        }

        public static List<string> GetLineDiscountNames(this PromoOrder promoOrder, OrderItem orderItem, PromoSettings promoSettings)
        {
            if (promoOrder == null)
                return null;

            if (orderItem == null)
                return null;

            List<PromoOrderItemPromotion> lineLevelPromotions = promoOrder.LineLevelPromotions(orderItem, promoSettings)
                                                                                     .Where(lp => lp.DiscountAmount != decimal.Zero || lp.PointsIssued != decimal.Zero)
                                                                                     .GroupBy(gb => gb.PromotionId)
                                                                                     .Select(s => s.First())
                                                                                     .ToList();

            if (lineLevelPromotions == null || lineLevelPromotions.Count == 0)
                return null;

            List<string> promotionNames = new List<string>();
            foreach (PromoOrderItemPromotion lineLevelPromo in lineLevelPromotions)
            {
                promotionNames.Add(GetDisplayPromoDetails(promoSettings.ShowPromotionDetailsInBasket, lineLevelPromo));
            }

            return promotionNames;
        }

        public static List<PromoOrderItemPromotion> LineLevelPromotions(this PromoOrder promoOrder, OrderItem orderItem, PromoSettings promoSettings)
        {
            if (promoOrder == null)
                return null;

            List<PromoOrderItem> items = new List<PromoOrderItem>();
            IProductMappingService productMappingService = EngineContext.Current.Resolve<IProductMappingService>();

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromAttributesXml(orderItem);
            if (productMappingItem != null)
            {
                items = (from i in promoOrder.PromoOrderItems
                         where
                             i.ProductCode.Equals(productMappingItem.EntityId.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                             ((productMappingItem.NoVariants ||
                             (!productMappingItem.NoVariants &&
                             !string.IsNullOrEmpty(productMappingItem.VariantCode) &&
                             i.VariantCode.Equals(productMappingItem.VariantCode, StringComparison.InvariantCultureIgnoreCase))))
                         select i).ToList();
            }

            if (items.Count == 0)
                return null;

            List<PromoOrderItemPromotion> lineLevelPromos = new List<PromoOrderItemPromotion>();

            foreach (var item in items)
            {
                IList<PromoOrderItemPromotion> itemLineLevelPromos = promoOrder.LineLevelPromotions(item.Id);
                if (itemLineLevelPromos != null)
                    lineLevelPromos.AddRange(itemLineLevelPromos);
            }

            return lineLevelPromos;
        }

        public static IList<PromoOrderItemPromotion> LineLevelPromotions(this PromoOrder promoOrder, int lineId)
        {
            if (promoOrder == null)
                return null;

            IList<PromoOrderItemPromotion> lineLevelPromotions = new List<PromoOrderItemPromotion>();

            var item = (from i in promoOrder.PromoOrderItems where i.Id == lineId select i).FirstOrDefault();

            if (item == null || item.PromoOrderItemPromotions == null || item.PromoOrderItemPromotions.Count == 0)
                return null;

            var itemPromos = (from ap in item.PromoOrderItemPromotions where (!ap.BasketLevel && !ap.DeliveryLevel) || ap.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase) select ap).ToList();

            return itemPromos;
        }

        public static decimal GetLineDiscountAmount(this PromoOrder promoOrder, OrderItem orderItem, PromoSettings promoSettings)
        {
            if (promoOrder == null)
                return decimal.Zero;

            decimal totalDiscountsForLines = Decimal.Zero;
            var responseItems = promoOrder.FindBasketResponseItems(orderItem, promoSettings);
            if (responseItems != null && responseItems.Count > 0)
                totalDiscountsForLines = responseItems.Sum(l => l.LinePromotionDiscount);

            return totalDiscountsForLines;
        }

        public static IList<PromoOrderItem> FindBasketResponseItems(this PromoOrder promoOrder, OrderItem orderItem, PromoSettings promoSettings)
        {
            if (promoOrder == null)
                return null;

            IProductMappingService productMappingService = EngineContext.Current.Resolve<IProductMappingService>();

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromAttributesXml(orderItem);
            if (productMappingItem == null)
                return null;

            string productCode = orderItem.ProductId.ToString();
            string variantCode = productMappingItem.VariantCode;

            IList<PromoOrderItem> basketResponseItems = (from bri in promoOrder.PromoOrderItems
                                                             where bri.ProductCode.Equals(orderItem.ProductId.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                                                                ((string.IsNullOrEmpty(variantCode) ||
                                                                    (!string.IsNullOrEmpty(variantCode) &&
                                                                        bri.VariantCode.Equals(variantCode, StringComparison.InvariantCultureIgnoreCase))))
                                                             select bri).ToList();

            return basketResponseItems;
        }

        public static string GetBasketLevelPromotionName(this PromoOrder promoOrder, PromoSettings settings)
        {
            var promo = promoOrder.BasketLevelPromotion();
            if (promo != null)
                return GetDisplayPromoDetails(settings.ShowPromotionDetailsInBasket, promo);
            return string.Empty;
        }
    }
}
