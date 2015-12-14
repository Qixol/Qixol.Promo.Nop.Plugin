using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qixol.Nop.Promo.Services.Promo
{
    public static class BasketResponseExtensions
    {
        #region extension methods

        public static bool IsValid(this BasketResponse basketResponse)
        {
            return BasketResponseIsValid(basketResponse);
        }

        #region Basket Level methods

        public static BasketResponseItem CheckoutAttributeItem(this BasketResponse basketResponse, global::Nop.Core.Domain.Orders.CheckoutAttribute attribute)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            BasketResponseItem checkoutAttributeItem = null;

            IAttributeValueService attributeValueService = DependencyResolver.Current.GetService<IAttributeValueService>();

            var checkoutAttributeMappingItem = attributeValueService.Retrieve(attribute.Id, EntityAttributeName.CheckoutAttribute);
            if (checkoutAttributeMappingItem != null)
            {
                var checkoutAttributeItems = basketResponse.FindBasketResponseItems(checkoutAttributeMappingItem.Code);

                if (checkoutAttributeItems != null && checkoutAttributeItems.Count > 0)
                {
                    checkoutAttributeItem = checkoutAttributeItems.First();
                    // we should only have one occurrence
                    // however, because the mechanism could add what appears to be a duplicate using hte "free product" promo
                    // if there is more than one, we select the generated item as this will have the promo applied
                    if (checkoutAttributeItems.Count > 1)
                    {
                        checkoutAttributeItem = (from cai in checkoutAttributeItems where cai.Generated select cai).FirstOrDefault();
                        if (checkoutAttributeItem == null)
                            checkoutAttributeItem = checkoutAttributeItems.First();
                    }
                }
            }

            return checkoutAttributeItem;
        }

        public static decimal SubTotalWithoutDiscount(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return Decimal.Zero;

            decimal subTotalWithoutDiscount = decimal.Zero;
            var items = (from i in basketResponse.Items where !i.IsDelivery select i).ToList();
            foreach (BasketResponseItem item in items)
            {
                subTotalWithoutDiscount += item.LineAmount;
                // Free products flagged at basket level are actually treated as line level promos
                var lineLevelPromotions = basketResponse.LineLevelPromotions(item.Id);
                if (lineLevelPromotions != null && lineLevelPromotions.Count > 0)
                {
                    foreach (var lineLevelPromotion in lineLevelPromotions)
                    {
                        subTotalWithoutDiscount -= lineLevelPromotion.DiscountAmount;
                    }
                }
            }

            return subTotalWithoutDiscount;
        }

        public static bool BasketLevelDiscountIncludesDeliveryAmount(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return false;

            var basketLevelPromo = basketResponse.BasketLevelPromotion();

            if (basketLevelPromo == null)
                return false;

            var deliveryItem = (from i in basketResponse.Items where i.IsDelivery select i).FirstOrDefault();

            if (deliveryItem == null)
                return true;

            var nonDeliveryPromosAppliedToDelivery = (from p in deliveryItem.AppliedPromotions where !p.DeliveryLevelPromotion select p).ToList();

            if (nonDeliveryPromosAppliedToDelivery == null)
                return false;

            return true;
        }

        public static string GetBasketLevelPromotionName(this BasketResponse basketResponse, PromoSettings settings)
        {
            var promo = basketResponse.BasketLevelPromotion();
            if (promo != null)
                return GetDisplayPromoDetails(settings.ShowPromotionDetailsInBasket, promo);
            return string.Empty;
        }

        public static BasketResponseSummaryAppliedPromotion BasketLevelPromotion(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            var basketLevelAppliedPromotions = (from p in basketResponse.Summary.AppliedPromotions
                                                where p.BasketLevelPromotion && !p.DeliveryLevelPromotion &&
                                                !p.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase) &&
                                                !p.PromotionType.Equals("ISSUECOUPON", StringComparison.InvariantCultureIgnoreCase) &&
                                                !p.PromotionType.Equals("ISSUEPOINTS", StringComparison.InvariantCultureIgnoreCase)
                                                select p).ToList();

            if (basketLevelAppliedPromotions == null || basketLevelAppliedPromotions.Count == 0)
                return null;

            // TODO: there should only be 1 basket level promotion - check for this?
            return basketLevelAppliedPromotions.FirstOrDefault();
        }

        public static decimal OrderDiscountTotal(this BasketResponse basketResponse)
        {
            Decimal orderDiscountTotal = decimal.Zero;

            if (!BasketResponseIsValid(basketResponse))
                return orderDiscountTotal;

            if (basketResponse.BasketLevelDiscountIncludesDeliveryAmount())
            {
                var basketLevelPromo = basketResponse.BasketLevelPromotion();
                if (basketLevelPromo != null)
                {
                    orderDiscountTotal = basketLevelPromo.DiscountAmount;
                }
            }

            return orderDiscountTotal;
        }

        public static int GetIssuedPoints(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return 0;

            return Convert.ToInt32(basketResponse.TotalIssuedPoints);
        }


        #endregion

        #region Delivery methods 

        public static bool HasDeliveryDiscount(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return false;

            var deliveryLinePromo = (from p in basketResponse.Summary.AppliedPromotions where p.DeliveryLevelPromotion select p).FirstOrDefault();

            if (deliveryLinePromo != null && deliveryLinePromo.DiscountAmount > 0)
                    return true;

            return false;
        }

        public static string GetDeliveryPromoName(this BasketResponse basketResponse, PromoSettings promoSettings)
        {
            if (!basketResponse.IsValid())
                return string.Empty;

            var deliveryPromo = basketResponse.DeliveryPromo();
            if (deliveryPromo != null)
                return GetDisplayPromoDetails(promoSettings.ShowPromotionDetailsInBasket, deliveryPromo);

            return string.Empty;
        }

        public static decimal GetDeliveryPromoDiscount(this BasketResponse basketResponse)
        {
            if (!basketResponse.IsValid())
                return decimal.Zero;

            var deliveryPromo = basketResponse.DeliveryPromo();
            if (deliveryPromo != null)
                return deliveryPromo.DiscountAmount;
            else
                return decimal.Zero;
        }

        public static BasketResponseSummaryAppliedPromotion DeliveryPromo(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            BasketResponseSummaryAppliedPromotion deliveryPromo = null;

            if (!basketResponse.HasDeliveryDiscount())
                return deliveryPromo;

            var deliveryItem = (from i in basketResponse.Items where i.IsDelivery select i).FirstOrDefault();

            if (deliveryItem == null)
                return null;

            var deliveryPromoLine = (from p in deliveryItem.AppliedPromotions where p.BasketLevelPromotion && p.DeliveryLevelPromotion select p).FirstOrDefault();

            if (deliveryPromoLine == null)
                return null;

            deliveryPromo = (from p in basketResponse.Summary.AppliedPromotions where p.PromotionId == deliveryPromoLine.PromotionId select p).FirstOrDefault();

            return deliveryPromo;
        }

        #endregion

        #region Line Level methods 

        public static List<BasketResponseAppliedPromotion> LineLevelPromotions(this BasketResponse basketResponse, Product product, PromoSettings promoSettings, string attributesXml)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            List<BasketResponseItem> items = new List<BasketResponseItem>();
            IProductMappingService productMappingService = DependencyResolver.Current.GetService<IProductMappingService>();

            if (!string.IsNullOrEmpty(attributesXml))
            {
                if (product.ProductAttributeMappings != null && product.ProductAttributeMappings.Count > promoSettings.MaximumAttributesForVariants)
                    attributesXml = string.Empty;
            }

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromAttributesXml(product.Id, attributesXml);
            if (productMappingItem != null)
            {
                items = (from i in basketResponse.Items
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

            List<BasketResponseAppliedPromotion> lineLevelPromos = new List<BasketResponseAppliedPromotion>();

            foreach (var item in items)
            {
                IList<BasketResponseAppliedPromotion> itemLineLevelPromos = basketResponse.LineLevelPromotions(item.Id);
                if (itemLineLevelPromos != null)
                    lineLevelPromos.AddRange(itemLineLevelPromos);
            }
            
            return lineLevelPromos;
        }

        public static IList<BasketResponseAppliedPromotion> LineLevelPromotions(this BasketResponse basketResponse, int lineId)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            IList<BasketResponseAppliedPromotion> lineLevelPromotions = new List<BasketResponseAppliedPromotion>();

            var item = (from i in basketResponse.Items where i.Id == lineId select i).FirstOrDefault();

            if (item == null || item.AppliedPromotions == null || item.AppliedPromotions.Count == 0)
                return null;

            var itemPromos = (from ap in item.AppliedPromotions where (!ap.BasketLevelPromotion && !ap.DeliveryLevelPromotion) select ap).ToList();

            // Always treat free product promotions as line level
            var freeProductPromos = (from sp in basketResponse.Summary.AppliedPromotions where sp.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase) select sp).ToList();

            if (freeProductPromos != null)
            {
                freeProductPromos.ForEach(fpp =>
                    {
                        var itemFreePromos = (from ip in item.AppliedPromotions where ip.PromotionId == fpp.PromotionId && ip.DiscountAmount > 0 select ip).ToList();
                        itemPromos.AddRange(itemFreePromos);
                    });
            }

            return itemPromos;
        }

        public static List<string> GetLineDiscountNames(this BasketResponse basketResponse, Product product, PromoSettings promoSettings, string attributesXml)
        {
            List<string> promotionNames = new List<string>();

            if (!BasketResponseIsValid(basketResponse))
                return promotionNames;

            List<BasketResponseAppliedPromotion> lineLevelPromotions = basketResponse.LineLevelPromotions(product, promoSettings, attributesXml);

            if (lineLevelPromotions == null || lineLevelPromotions.Count == 0)
                return promotionNames;
            
            lineLevelPromotions = lineLevelPromotions.Where(lp => lp.DiscountAmount != decimal.Zero || lp.PointsIssued != decimal.Zero)
                                                                                     .GroupBy(gb => gb.PromotionId)
                                                                                     .Select(s => s.First())
                                                                                     .ToList();

            if (lineLevelPromotions == null || lineLevelPromotions.Count == 0)
                return promotionNames;

            foreach (BasketResponseAppliedPromotion lineLevelPromo in lineLevelPromotions)
            {
                var appliedPromo = (from p in basketResponse.Summary.AppliedPromotions where p.PromotionId == lineLevelPromo.PromotionId select p).FirstOrDefault();
                if (appliedPromo != null)
                {
                    if ((!appliedPromo.BasketLevelPromotion && !appliedPromo.DeliveryLevelPromotion) ||
                        appliedPromo.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase)) // always treat FreeProduct as line level
                    {
                        promotionNames.Add(GetDisplayPromoDetails(promoSettings.ShowPromotionDetailsInBasket, appliedPromo));
                    }
                }
            }

            return promotionNames;
        }

        public static decimal GetLineDiscountAmount(this BasketResponse basketResponse, Product product, PromoSettings promoSettings, string attributesXml)
        {
            if (!BasketResponseIsValid(basketResponse))
                return decimal.Zero;

            decimal totalDiscountsForLines = Decimal.Zero;
            var responseItems = basketResponse.FindBasketResponseItems(product, promoSettings, attributesXml);

            foreach (var responseItem in responseItems)
            {
                if (responseItem.AppliedPromotions.Count > 0)
                {
                    foreach (var appliedPromotion in responseItem.AppliedPromotions)
                    {
                        if (appliedPromotion.AssociatedLine == responseItem.Id)
                        {
                            if (!appliedPromotion.BasketLevelPromotion && !appliedPromotion.DeliveryLevelPromotion)
                            {
                                totalDiscountsForLines += appliedPromotion.DiscountAmount;
                            }
                            else
                            {
                                // check if we have a free product
                                var summaryAppliedPromotion = (from sap in basketResponse.Summary.AppliedPromotions where sap.PromotionId == appliedPromotion.PromotionId && sap.InstanceId == appliedPromotion.InstanceId select sap).FirstOrDefault();
                                if (summaryAppliedPromotion != null)
                                {
                                    if (summaryAppliedPromotion.PromotionType.Equals("FREEPRODUCT", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        totalDiscountsForLines += appliedPromotion.DiscountAmount; // We only want the amount for this line, not for the promotion which could be applied across multiple lines
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return totalDiscountsForLines;
        }

        public static IList<BasketResponseItem> FindBasketResponseItems(this BasketResponse basketResponse, Product product, PromoSettings promoSettings, string attributesXml)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            string productCode = product.Id.ToString();
            string variantCode = string.Empty;

            if (!string.IsNullOrEmpty(attributesXml))
            {
                IProductMappingService productMappingService = DependencyResolver.Current.GetService<IProductMappingService>();
                if (product.ProductAttributeMappings != null && product.ProductAttributeMappings.Count > promoSettings.MaximumAttributesForVariants)
                    attributesXml = string.Empty;
                ProductMappingItem productMappingItem = productMappingService.RetrieveFromAttributesXml(product.Id, attributesXml);
                if (productMappingItem == null)
                    return null;
                variantCode = productMappingItem.VariantCode;
            }

            IList<BasketResponseItem> basketResponseItems = (from bri in basketResponse.Items
                                                             where bri.ProductCode.Equals(product.Id.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                                                                ((string.IsNullOrEmpty(variantCode) ||
                                                                    (!string.IsNullOrEmpty(variantCode) &&
                                                                        bri.VariantCode.Equals(variantCode, StringComparison.InvariantCultureIgnoreCase))))
                                                             select bri).ToList();

            return basketResponseItems;
        }

        public static IList<BasketResponseItem> FindBasketResponseItems(this BasketResponse basketResponse, string productMappingCode)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            IList<BasketResponseItem> basketResponseItems = (from bri in basketResponse.Items
                                                             where bri.ProductCode.Equals(productMappingCode, StringComparison.InvariantCultureIgnoreCase)
                                                             select bri).ToList();

            return basketResponseItems;
        }
        #endregion

        #region Coupon methods

        public static bool CouponIsValid(this BasketResponse basketResponse, string couponCode)
        {
            if (!basketResponse.IsValid())
                return false;

            var basketCoupon = (from c in basketResponse.Coupons where c.CouponCode.Equals(couponCode, StringComparison.InvariantCultureIgnoreCase) select c).FirstOrDefault();

            if (basketCoupon == null)
                return false;

            if (basketCoupon.Utilized)
                return true;

            return false;
        }

        #endregion

        #endregion

        #region helper methods

        private static bool BasketResponseIsValid(BasketResponse basketResponse)
        {
            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return false;

            if (!basketResponse.Summary.ProcessingResult)
                return false;

            if (basketResponse.Items.Count == 0)
                return false;

            return true;
        }


        public static string GetDisplayPromoDetails(int displaySetting, BasketResponseSummaryAppliedPromotion promo)
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

        #endregion
    }
}
