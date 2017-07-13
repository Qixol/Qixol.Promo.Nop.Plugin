using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Qixol.Nop.Promo.Core.Domain;
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

            IAttributeValueService attributeValueService = EngineContext.Current.Resolve<IAttributeValueService>();

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

        public static IList<BasketResponseItem> CheckoutAttributeItems(this BasketResponse basketResponse)
        {
            var checkoutAttributeBasketResponseItems = new List<BasketResponseItem>();

            if (!BasketResponseIsValid(basketResponse))
                return checkoutAttributeBasketResponseItems;

            ICheckoutAttributeService checkoutAttributeService = EngineContext.Current.Resolve<ICheckoutAttributeService>();

            var checkoutAttributes = checkoutAttributeService.GetAllCheckoutAttributes().ToList();

            checkoutAttributes.ForEach(ca =>
            {
                var items = basketResponse.FindBasketResponseItems(ca);
                if (items != null && items.Any())
                {
                    checkoutAttributeBasketResponseItems.AddRange(items);
                }
            });

            return checkoutAttributeBasketResponseItems;
        }

        public static decimal SubTotalWithoutDiscount(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return Decimal.Zero;

            decimal subTotalWithoutDiscount = decimal.Zero;
            var items = (from i in basketResponse.Items where !i.IsDelivery select i).ToList();
            items.ForEach(item =>
            {
                subTotalWithoutDiscount += item.LineAmount;

                var lineLevelPromotions = (from ap in item.AppliedPromotions where !ap.BasketLevelPromotion && !ap.DeliveryLevelPromotion && ap.DiscountAmount > decimal.Zero select ap).ToList();
                if (lineLevelPromotions.Any())
                {
                    subTotalWithoutDiscount -= lineLevelPromotions.Sum(lp => lp.DiscountAmount);
                }
                // Free products flagged at basket level are actually treated as line level promos
                var basketLevelPromotions = (from ap in item.AppliedPromotions where ap.BasketLevelPromotion && !ap.DeliveryLevelPromotion && ap.DiscountAmount > decimal.Zero select ap).ToList();
                basketLevelPromotions.ForEach(blp =>
                {
                    var ap = (from sap in basketResponse.Summary.AppliedPromotions
                              where
                                    sap.PromotionId == blp.PromotionId &&
                                    sap.InstanceId == blp.InstanceId &&
                                    sap.PromotionType.Equals(PromotionTypeName.FreeProduct)
                              select sap).FirstOrDefault();
                    if (ap != null)
                    {
                        subTotalWithoutDiscount -= ap.DiscountAmount;
                    }
                });
            });

            return subTotalWithoutDiscount;
        }

        public static List<BasketResponseSummaryAppliedPromotion> BasketLevelPromotions(this BasketResponse basketResponse)
        {
            var basketLevelPromotions = new List<BasketResponseSummaryAppliedPromotion>();

            if (!BasketResponseIsValid(basketResponse))
                return basketLevelPromotions;

            var allBbasketLevelPromotions = (from p in basketResponse.Summary.AppliedPromotions
                                                where p.BasketLevelPromotion && !p.DeliveryLevelPromotion && p.DiscountAmount > decimal.Zero &&
                                                !p.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase) &&
                                                !p.PromotionType.Equals(PromotionTypeName.IssueCoupon, StringComparison.InvariantCultureIgnoreCase) &&
                                                !p.PromotionType.Equals(PromotionTypeName.IssuePoints, StringComparison.InvariantCultureIgnoreCase)
                                                select p).ToList();

            allBbasketLevelPromotions.ForEach(a =>
            {
                var existingPromo = (from p in basketLevelPromotions where p.PromotionId == a.PromotionId select p).FirstOrDefault();
                if (existingPromo != null)
                {
                    existingPromo.DiscountAmount += a.DiscountAmount;
                }
                else
                {
                    basketLevelPromotions.Add(a);
                }
            });

            return basketLevelPromotions;
        }

        public static int IssuedPoints(this BasketResponse basketResponse)
        {
            if (!BasketResponseIsValid(basketResponse))
                return 0;

            return Convert.ToInt32(basketResponse.TotalIssuedPoints);
        }

        public static IList<BasketResponseCoupon> IssuedCoupons(this BasketResponse basketResponse)
        {
            var issuedCoupons = new List<BasketResponseCoupon>();

            if (!basketResponse.IsValid())
                return issuedCoupons;

            return (from ic in basketResponse.Coupons where ic.Issued select ic).ToList();
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

        //public static string DeliveryPromoName(this BasketResponse basketResponse)
        //{
        //    if (!basketResponse.IsValid())
        //        return string.Empty;

        //    var deliveryPromo = basketResponse.DeliveryPromo();
        //    if (deliveryPromo != null)
        //        return deliveryPromo.DisplayDetails();

        //    return string.Empty;
        //}

        //public static decimal DeliveryPromoDiscount(this BasketResponse basketResponse)
        //{
        //    if (!basketResponse.IsValid())
        //        return decimal.Zero;

        //    var deliveryPromo = basketResponse.DeliveryPromo();
        //    if (deliveryPromo != null)
        //        return deliveryPromo.DiscountAmount;
        //    else
        //        return decimal.Zero;
        //}

        public static List<BasketResponseSummaryAppliedPromotion> DeliveryPromos(this BasketResponse basketResponse)
        {
            var deliveryPromos = new List<BasketResponseSummaryAppliedPromotion>();

            if (!BasketResponseIsValid(basketResponse))
                return deliveryPromos;

            if (!basketResponse.HasDeliveryDiscount())
                return deliveryPromos;

            var deliveryItems = (from i in basketResponse.Items where i.IsDelivery select i).ToList();

            if (deliveryItems == null)
                return deliveryPromos;

            if (!deliveryItems.Any())
                return deliveryPromos;

            deliveryItems.ForEach(deliveryItem =>
            {
                var deliveryItemPromotions = (from p in deliveryItem.AppliedPromotions where p.BasketLevelPromotion && p.DeliveryLevelPromotion select p).ToList();

                if (deliveryItemPromotions != null && deliveryItemPromotions.Any())
                {
                    deliveryItemPromotions.ForEach(dip =>
                    {
                        var existingPromo = (from dp in deliveryPromos where dp.PromotionId == dip.PromotionId select dp).FirstOrDefault();
                        if (existingPromo != null)
                        {
                            existingPromo.DiscountAmount += dip.DiscountAmount;
                        }
                        else
                        {
                            var deliveryPromo = (from p in basketResponse.Summary.AppliedPromotions where p.PromotionId == dip.PromotionId select p).FirstOrDefault();
                            deliveryPromos.Add(deliveryPromo);
                        }
                    });
                }
            });

            return deliveryPromos;
        }

        #endregion

        #region Line Level methods 

        public static List<string> LineDiscountNames(this BasketResponse basketResponse, ShoppingCartItem shoppingCartItem)
        {
            List<string> promotionNames = new List<string>();

            if (!BasketResponseIsValid(basketResponse))
                return promotionNames;

            List<BasketResponseAppliedPromotion> lineLevelPromotions = basketResponse.LineLevelPromotions(shoppingCartItem);

            if (!lineLevelPromotions.Any())
                return promotionNames;

            lineLevelPromotions.ForEach(lineLevelPromotion =>
            {
                var appliedPromo = (from p in basketResponse.Summary.AppliedPromotions where p.PromotionId == lineLevelPromotion.PromotionId select p).FirstOrDefault();
                if (appliedPromo != null)
                {
                    if ((!appliedPromo.BasketLevelPromotion && !appliedPromo.DeliveryLevelPromotion) ||
                        appliedPromo.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase)) // always treat FreeProduct as line level
                    {
                        promotionNames.Add(appliedPromo.DisplayDetails());
                    }
                }
            });


            return promotionNames;
        }

        public static List<decimal> GetLineDiscountAmounts(this BasketResponse basketResponse, ShoppingCartItem shoppingCartItem)
        {
            var lineDiscountAmounts = new List<decimal>();

            if (!BasketResponseIsValid(basketResponse))
                return lineDiscountAmounts;

            var linePromotions = basketResponse.LineLevelPromotions(shoppingCartItem);

            linePromotions.Where(p => p.DiscountAmount > decimal.Zero).ToList().ForEach(lp =>
            {
                lineDiscountAmounts.Add(lp.DiscountAmount);
            });

            return lineDiscountAmounts;
        }

        public static decimal GetLineTotalDiscountAmount(this BasketResponse basketResponse, ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                return decimal.Zero;

            var lineDiscountAmounts = basketResponse.GetLineDiscountAmounts(shoppingCartItem);

            var lineDiscountAmount = lineDiscountAmounts.Sum();

            return lineDiscountAmount;
        }

        public static List<BasketResponseAppliedPromotion> LineLevelPromotions(this BasketResponse basketResponse, ShoppingCartItem shoppingCartItem)
        {
            var linePromotions = new List<BasketResponseAppliedPromotion>();

            if (shoppingCartItem == null)
                return null;

            if (!BasketResponseIsValid(basketResponse))
                return linePromotions;

            var responseItems = basketResponse.FindBasketResponseItems(shoppingCartItem);

            if (responseItems == null)
                return linePromotions;

            responseItems.ToList().ForEach(responseItem =>
            {
                if (responseItem.AppliedPromotions != null && responseItem.AppliedPromotions.Any())
                {
                    responseItem.AppliedPromotions.Where(ap => ap.DiscountAmount > decimal.Zero).ToList().ForEach(appliedPromotion =>
                    {
                        if (!appliedPromotion.BasketLevelPromotion && !appliedPromotion.DeliveryLevelPromotion)
                        {
                            // Find any instance of this promotion in the current list of line promotions
                            var existingLinePromo = (from ep in linePromotions where ep.PromotionId == appliedPromotion.PromotionId select ep).FirstOrDefault();
                            if (existingLinePromo != null)
                            {
                                existingLinePromo.DiscountAmount += appliedPromotion.DiscountAmount;
                            }
                            else
                            {
                                linePromotions.Add(appliedPromotion);
                            }
                        }
                        else
                        {
                            // check if we have a free product
                            var summaryAppliedPromotion = (from sap in basketResponse.Summary.AppliedPromotions where sap.PromotionId == appliedPromotion.PromotionId && sap.InstanceId == appliedPromotion.InstanceId select sap).FirstOrDefault();
                            if (summaryAppliedPromotion != null)
                            {
                                if (summaryAppliedPromotion.PromotionType.Equals(PromotionTypeName.FreeProduct, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Find any instance of this promotion in the current list of line promotions
                                    var existingLinePromo = (from ep in linePromotions where ep.PromotionId == appliedPromotion.PromotionId select ep).FirstOrDefault();
                                    if (existingLinePromo != null)
                                    {
                                        existingLinePromo.DiscountAmount += appliedPromotion.DiscountAmount;
                                    }
                                    else
                                    {
                                        linePromotions.Add(appliedPromotion);
                                    }
                                }
                            }
                        }
                    });
                }
            });

            return linePromotions;
        }

        public static IList<BasketResponseItem> FindBasketResponseItems(this BasketResponse basketResponse, ShoppingCartItem shoppingCartItem)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            string productCode = shoppingCartItem.ProductId.ToString();
            string variantCode = string.Empty;

            IProductMappingService productMappingService = EngineContext.Current.Resolve<IProductMappingService>();

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromShoppingCartItem(shoppingCartItem);
            if (productMappingItem == null)
                return null;
            variantCode = productMappingItem.VariantCode;

            IList<BasketResponseItem> basketResponseItems =
                (from bri in basketResponse.Items
                 where bri.ProductCode.Equals(shoppingCartItem.ProductId.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                     ((string.IsNullOrEmpty(variantCode) ||
                     (!string.IsNullOrEmpty(variantCode) &&
                     bri.VariantCode.Equals(variantCode, StringComparison.InvariantCultureIgnoreCase))) &&
                     ((!bri.Generated && bri.Id == shoppingCartItem.Id) ||
                     (bri.Generated && bri.SplitFromLineId == shoppingCartItem.Id)))
                 select bri).ToList();

            // If we don't have any matches, check for free products
            // It's probable that neither the Id nor SplitFromLineId will match so just use product code and variant code
            if (basketResponseItems.Count == 0)
            {
                basketResponseItems =
                (from bri in basketResponse.Items
                 where bri.ProductCode.Equals(shoppingCartItem.ProductId.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                     ((string.IsNullOrEmpty(variantCode) ||
                     (!string.IsNullOrEmpty(variantCode) &&
                     bri.VariantCode.Equals(variantCode, StringComparison.InvariantCultureIgnoreCase))) &&
                     bri.Generated)
                 select bri).ToList();
            }

            return basketResponseItems;
        }

        public static IList<BasketResponseItem> FindBasketResponseItems(this BasketResponse basketResponse, Product product, string attributesXml)
        {
            if (!BasketResponseIsValid(basketResponse))
                return null;

            string productCode = product.Id.ToString();
            string variantCode = string.Empty;

            IProductMappingService productMappingService = EngineContext.Current.Resolve<IProductMappingService>();

            ProductMappingItem productMappingItem = productMappingService.RetrieveFromAttributesXml(product, attributesXml);
            if (productMappingItem == null)
                return null;
            variantCode = productMappingItem.VariantCode;

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

        public static IList<BasketResponseItem> FindBasketResponseItems(this BasketResponse basketResponse, CheckoutAttribute checkoutAttribute)
        {
            var basketResponseItems = new List<BasketResponseItem>();

            if (!BasketResponseIsValid(basketResponse))
                return basketResponseItems;

            IAttributeValueService attributeValueService = EngineContext.Current.Resolve<IAttributeValueService>();

            var checkoutAttributeValueMappingItem = attributeValueService.Retrieve(checkoutAttribute.Id, EntityAttributeName.CheckoutAttribute);

            if (checkoutAttributeValueMappingItem != null)
            {
                var items = (from bri in basketResponse.Items where bri.ProductCode.Equals(checkoutAttributeValueMappingItem.Code, StringComparison.InvariantCultureIgnoreCase) select bri);
                if (items != null)
                {
                    basketResponseItems = items.ToList();
                }
            }

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

        #endregion
    }
}
