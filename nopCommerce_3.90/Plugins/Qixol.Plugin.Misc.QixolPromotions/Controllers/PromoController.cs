using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Nop.Promo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Core.Domain.Orders;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Services.AttributeValues;
using Nop.Services.Directory;
using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Plugin.Misc.Promo.Extensions.MappingExtensions;
using Nop.Services.Localization;
using Qixol.Nop.Promo.Services.Localization;
using Qixol.Plugin.Misc.Promo.Models.ShoppingCart;
using Qixol.Plugin.Misc.Promo.Models.Shared;
using Nop.Services.Orders;
using Qixol.Nop.Promo.Services.ShoppingCart;
using global::Nop.Services.Seo;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Plugin.Misc.Promo.Factories;
using Qixol.Plugin.Misc.Promo.Models.Order;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public partial class PromoController : Controller
    {
        #region Fields

        private readonly IIssuedCouponModelFactory _issuedCouponModelFactory;

        private readonly IPromoUtilities _promoUtilities;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IProductMappingService _productMappingService;
        private readonly IProductPromoMappingService _productPromoMappingService;
        private readonly IPromoDetailService _promoDetailService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IAttributeValueService _attributeValueService;
        private readonly ICurrencyService _currencyService;
        private readonly IPromoPictureService _promoPictureService;
        private readonly PromoSettings _promoSettings;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPromoOrderService _promoOrderService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;

        #endregion

        #region constructor

        public PromoController(
            IIssuedCouponModelFactory issuedCouponModelFactory,
            IPromoUtilities promoUtilities,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IOrderService orderService,
            IPromoOrderService promoOrderService,
            IProductMappingService productMappingService,
            IProductPromoMappingService productPromoMappingService,
            IPromoDetailService promoDetailService,
            IStoreService storeService,
            IStoreContext storeContext,
            IAttributeValueService attributeValueService,
            ICurrencyService currencyService,
            IPromoPictureService promoPictureService,
            IPictureService pictureService,
            IProductService productService,
            ILocalizationService localizationService,
            IProductAttributeFormatter productAttributeFormatter)
        {
            this._issuedCouponModelFactory = issuedCouponModelFactory;
            this._promoUtilities = promoUtilities;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._productMappingService = productMappingService;
            this._productPromoMappingService = productPromoMappingService;
            this._promoDetailService = promoDetailService;
            this._promoSettings = promoSettings;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._attributeValueService = attributeValueService;
            this._currencyService = currencyService;
            this._promoPictureService = promoPictureService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._localizationService = localizationService;
            this._orderService = orderService;
            this._promoOrderService = promoOrderService;
            this._productAttributeFormatter = productAttributeFormatter;
        }

        #endregion

        #region Utilities

        private ActionResult ShoppingCartWidget()
        {
            PromoWidgetModel model = new PromoWidgetModel();

            var basketResponse = _promoUtilities.GetBasketResponse(_workContext.CurrentCustomer);

            if (basketResponse == null)
                return new EmptyResult();

            #region cart items - line level promotions

            var cartItems = _workContext.CurrentCustomer.ShoppingCartItems.ToList();
            var lineItemsModel = new List<LineItemModel>();

            cartItems.ForEach(cartItem =>
            {
                var promotions = cartItem.Promotions();
                if (promotions.Any())
                {
                    LineItemModel lineItemModel = new LineItemModel();
                    // used for matching to displayed cart
                    lineItemModel.ProductSeName = cartItem.Product.GetSeName();
                    lineItemModel.AttributeInfo = _productAttributeFormatter.FormatAttributes(cartItem.Product, cartItem.AttributesXml);
                    lineItemModel.LineAmount = _priceFormatter.FormatPrice(cartItem.LineAmount(), true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

                    promotions.ForEach(p =>
                    {
                        PromotionModel lineItemDiscountModel = new PromotionModel()
                        {
                            PromotionId = p.PromotionId.ToString(),
                            PromotionName = p.DisplayDetails(_workContext.CurrentCustomer),
                            DiscountAmount = _priceFormatter.FormatPrice(p.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
                        };
                        lineItemModel.LineDiscounts.Add(lineItemDiscountModel);
                    });

                    lineItemsModel.Add(lineItemModel);
                }
            });

            model.LineDiscountsModel = lineItemsModel;

            #endregion

            #region issued coupons

            var issuedCouponsModel = new List<IssuedCouponModel>();

            basketResponse.Coupons.Where(c => c.Issued).ToList().ForEach(c =>
            {
                issuedCouponsModel.Add(_issuedCouponModelFactory.PrepareIssuedCouponModel(c));
            });

            model.IssuedCouponsModel = issuedCouponsModel;

            #endregion

            #region checkout attributes

            // handled in the checkout attribute formatter

            #endregion

            #region subtotal

            model.SubTotal = _priceFormatter.FormatPrice(cartItems.SubTotal(), true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            #endregion

            #region basket level promotions (excluding shipping)

            var basketLevelDiscountsExcShipping = new List<PromotionModel>();
            basketResponse.BasketLevelPromotions().ForEach(blp =>
            {
                basketLevelDiscountsExcShipping.Add(new PromotionModel()
                {
                    PromotionId = blp.PromotionId.ToString(),
                    PromotionName = blp.DisplayDetails(),
                    DiscountAmount = _priceFormatter.FormatPrice(-1 * blp.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
                });
            });
            model.BasketLevelDiscountsExcShippingModel = basketLevelDiscountsExcShipping;

            #endregion

            #region shipping

            ShippingModel shippingModel = new ShippingModel();
            shippingModel.ShippingAmount = _priceFormatter.FormatPrice(basketResponse.DeliveryPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);
            shippingModel.OriginalShippingAmount = _priceFormatter.FormatPrice(basketResponse.DeliveryOriginalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            basketResponse.DeliveryPromos().ForEach(dp =>
            {
                shippingModel.ShippingPromotions.Add(new PromotionModel()
                {
                    PromotionId = dp.PromotionId.ToString(),
                    PromotionName = dp.DisplayDetails(),
                    DiscountAmount = _priceFormatter.FormatPrice(-1 * dp.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
                });
            });

            model.ShippingModel = shippingModel;

            #endregion

            #region basket level promotions (including shipping)

            //var basketLevelDiscountsIncShipping = new List<PromotionModel>();
            //basketResponse.BasketLevelPromotionsIncludingShipping().ForEach(blp =>
            //{
            //    basketLevelDiscountsIncShipping.Add(new PromotionModel()
            //    {
            //        PromotionId = blp.PromotionId.ToString(),
            //        PromotionName = blp.DisplayText,
            //        DiscountAmount = _priceFormatter.FormatPrice(-1 * blp.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
            //    });
            //});
            //model.BasketLevelDiscountsIncShippingModel = basketLevelDiscountsIncShipping;

            #endregion

            #region OrderTotal

            model.OrderTotal = _priceFormatter.FormatPrice(basketResponse.BasketTotal, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            #endregion

            #region IssuedPoints

            if (basketResponse.IssuedPoints() > decimal.Zero)
            {
                model.IssuedPoints = string.Format(_localizationService.GetLocaleStringResourceByName("ShoppingCart.Totals.RewardPoints.WillEarn.Point").ResourceValue, basketResponse.IssuedPoints());
            }

            #endregion

            #region basket total discount

            model.BasketTotalDiscount = _priceFormatter.FormatPrice(basketResponse.TotalDiscount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            #endregion

            return View("ShoppingCart/PromoWidget", model);
        }

        private ActionResult OrderWidget(int orderId)
        {
            PromoWidgetModel model = new PromoWidgetModel();

            var order = _orderService.GetOrderById(orderId);
            var promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);

            if (order == null)
                return new EmptyResult();

            if (promoOrder == null)
                return new EmptyResult();

            #region issued coupons

            var issuedCouponsModel = new List<IssuedCouponModel>();

            promoOrder.PromoOrderCoupons.Where(c => c.Issued).ToList().ForEach(c =>
            {
                issuedCouponsModel.Add(_issuedCouponModelFactory.PrepareIssuedCouponModel(c));
            });

            model.IssuedCouponsModel = issuedCouponsModel;

            #endregion

            #region checkout attributes

            // handled in the checkout attribute formatter

            #endregion

            #region subtotal

            // stored in the database - no need to update via widget

            #endregion

            #region basket level promotions (excluding shipping)

            var basketLevelDiscountsExcShipping = new List<PromotionModel>();
            order.BasketLevelPromotions().ForEach(blp =>
            {
                basketLevelDiscountsExcShipping.Add(new PromotionModel()
                {
                    PromotionId = blp.PromotionId.ToString(),
                    PromotionName = blp.DisplayText,
                    DiscountAmount = _priceFormatter.FormatPrice(-1 * blp.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
                });
            });
            model.BasketLevelDiscountsExcShippingModel = basketLevelDiscountsExcShipping;

            #endregion

            #region shipping

            ShippingModel shippingModel = new ShippingModel();
            shippingModel.ShippingAmount = _priceFormatter.FormatPrice(order.OrderShippingInclTax, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);
            shippingModel.OriginalShippingAmount = _priceFormatter.FormatPrice(promoOrder.DeliveryOriginalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            order.DeliveryPromotions().ToList().ForEach(dp =>
            {
                shippingModel.ShippingPromotions.Add(new PromotionModel()
                {
                    PromotionId = dp.PromotionId.ToString(),
                    PromotionName = dp.DisplayDetails(),
                    DiscountAmount = _priceFormatter.FormatPrice(-1 * dp.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
                });
            });

            model.ShippingModel = shippingModel;

            #endregion

            //#region basket level promotions (including shipping)

            ////var basketLevelDiscountsIncShipping = new List<PromotionModel>();
            ////basketResponse.BasketLevelPromotionsIncludingShipping().ForEach(blp =>
            ////{
            ////    basketLevelDiscountsIncShipping.Add(new PromotionModel()
            ////    {
            ////        PromotionId = blp.PromotionId.ToString(),
            ////        PromotionName = blp.DisplayText,
            ////        DiscountAmount = _priceFormatter.FormatPrice(-1 * blp.DiscountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)
            ////    });
            ////});
            ////model.BasketLevelDiscountsIncShippingJson = JsonConvert.SerializeObject(basketLevelDiscountsIncShipping, jsSettings);

            //#endregion

            #region OrderTotal

            // value is correct from database - no need to override

            #endregion

            #region IssuedPoints

            // this value should be correct as we use nopCommerce's underlying mechanism to store the points

            #endregion

            #region basket total discount

            model.BasketTotalDiscount = _priceFormatter.FormatPrice(order.TotalDiscount(), true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            #endregion

            return View("Order/PromoWidget", model);
        }

        private ActionResult OrderItemWidget(int itemId)
        {
            var model = new PromoOrderItemWidgetModel();

            var orderItem = _orderService.GetOrderItemById(itemId);

            if (orderItem == null)
                return new EmptyResult();

            model.OrderItemId = itemId;

            var promotions = orderItem.Promotions();
            if (promotions == null || !promotions.Any())
                return new EmptyResult();

            promotions.ToList().ForEach(p =>
            {
                var localDiscountAmount = _currencyService.ConvertFromPrimaryExchangeRateCurrency(p.DiscountAmount, _workContext.WorkingCurrency);
                var displayDiscountAmount = _priceFormatter.FormatPrice(localDiscountAmount, true, false);
                model.Promotions.Add(new PromotionModel()
                {
                    DiscountAmount = displayDiscountAmount,
                    PromotionId = p.PromotionId.ToString(),
                    PromotionName = p.DisplayDetails()
                });
            });

            return View("Order/PromoOrderItemWidget", model);
        }

        #endregion

        #region methods

        public ActionResult PromoWidget(string widgetZone, object additionalData = null)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            switch (widgetZone)
            {
                // customer account
                case "account_navigation_after":
                    return PartialView("CustomerNavigationExtension");

                // shopping cart
                case "order_summary_content_before":
                    return ShoppingCartWidget();

                // checkout
                case "opc_content_before":
                case "checkout_confirm_top":
                    return ShoppingCartWidget();

                // order details page (customer) - main widget
                case "orderdetails_page_top":
                    int orderId;
                    if (int.TryParse(additionalData.ToString(), out orderId))
                    {
                        return OrderWidget(orderId);
                    }
                    break;

                // order details page (customer) - line widget
                case "orderdetails_product_line":
                    int itemId;
                    if (int.TryParse(additionalData.ToString(), out itemId))
                    {
                        return OrderItemWidget(itemId);
                    }
                    break;

                //// order details page (admin)
                //case "xxxx":
                //    int orderId;
                //    if (int.TryParse(additionalData.ToString(), out orderId))
                //    {
                //        return OrderWidget(orderId);
                //    }
                //    break;

                default:
                    return new EmptyResult();
            }

            return new EmptyResult();
        }

        #endregion
    }
}

