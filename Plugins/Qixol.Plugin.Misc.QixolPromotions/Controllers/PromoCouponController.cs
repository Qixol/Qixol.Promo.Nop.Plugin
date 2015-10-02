using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Models.ShoppingCart;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Nop.Promo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Services.Coupons;
using Qixol.Promo.Integration.Lib.Coupon;
using Nop.Services.Helpers;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public partial class PromoCouponController : Controller
    {
        #region Fields

        private readonly IPromoUtilities _promoUtilities;
        private readonly IPromoOrderService _promoOrderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IProductMappingService _productMappingService;
        private readonly ICouponService _couponService;
        private readonly PromoSettings _promoSettings;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region constructor

        public PromoCouponController(
            IPromoUtilities promoUtilities,
            IPromoOrderService promoOrderService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IProductMappingService productMappingService,
            ICouponService couponService,
            IDateTimeHelper dateTimeHelper)
        {
            this._promoUtilities = promoUtilities;
            this._promoOrderService = promoOrderService;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._productMappingService = productMappingService;
            this._couponService = couponService;
            this._dateTimeHelper = dateTimeHelper;

            this._promoSettings = promoSettings;
        }

        #endregion

        #region methods

        [ChildActionOnly]
        public ActionResult IssuedCoupons(ShoppingCartModel shoppingCartModel)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            IssuedCouponsModel issuedCouponsModel = new IssuedCouponsModel()
            {
                IsEditable = shoppingCartModel.IsEditable,
                ShowProductImages = shoppingCartModel.ShowProductImages,
                ShowSku = shoppingCartModel.ShowSku
            };

            var basketResponse = _promoUtilities.GetBasketResponse();
            BuildCouponsModel(issuedCouponsModel, basketResponse);
            return View(issuedCouponsModel);
        }

        [ChildActionOnly]
        public ActionResult IssuedCouponsForOrder(int orderId, bool showSku)
        {
            if (!_promoSettings.Enabled || orderId <= 0)
                return new EmptyResult();

            IssuedCouponsModel issuedCouponsModel = new IssuedCouponsModel()
            {
                IsEditable = false,
                ShowProductImages = false,
                ShowSku = showSku
            };

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);
            BuildCouponsModel(issuedCouponsModel, promoOrder);
            return View("IssuedCoupons", issuedCouponsModel);

        }

        private void BuildCouponsModel(IssuedCouponsModel model, PromoOrder promoOrder)
        {
            if (promoOrder != null)
            {
                var issuedCoupons = (from ic in promoOrder.PromoOrderCoupons where ic.Issued select ic).ToList();

                foreach (var issuedCoupon in issuedCoupons)
                {
                    model.Coupons.Add(new IssuedCouponModel()
                    {
                        Name = issuedCoupon.CouponName,
                        // Never show the coupon code until the transaction is complete
                        ValidTo = DateTime.MinValue,
                        Status = string.Empty,
                        IsConfirmed = issuedCoupon.IssuedConfirmed,
                        Code = issuedCoupon.IssuedConfirmed == true ? issuedCoupon.CouponCode : string.Empty,
                        DisplayText = issuedCoupon.DisplayText
                    });
                }
            }

        }

        private void BuildCouponsModel(IssuedCouponsModel model, BasketResponse basketResponse)
        {
            if (basketResponse != null)
            {
                var issuedCoupons = (from ic in basketResponse.Coupons where ic.Issued select ic).ToList();

                foreach (var issuedCoupon in issuedCoupons)
                {
                    model.Coupons.Add(new IssuedCouponModel()
                    {
                        Name = issuedCoupon.CouponName,
                        // Never show the coupon code until the transaction is complete
                        ValidTo = DateTime.MinValue,
                        Status = string.Empty,
                        IsConfirmed = issuedCoupon.IssuedConfirmed,
                        Code = issuedCoupon.IssuedConfirmed == true ? issuedCoupon.CouponCode : string.Empty,
                        DisplayText = issuedCoupon.DisplayText
                    });
                }
            }

        }

        #endregion
    }
}

