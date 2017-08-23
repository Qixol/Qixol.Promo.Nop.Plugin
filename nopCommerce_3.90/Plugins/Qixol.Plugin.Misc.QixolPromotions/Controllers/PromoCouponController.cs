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
using Qixol.Promo.Integration.Lib.Coupon;
using Nop.Services.Helpers;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Nop.Services.Orders;
using Qixol.Plugin.Misc.Promo.Factories;
using Qixol.Nop.Promo.Services.Coupons;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public partial class PromoCouponController : Controller
    {
        #region Fields

        private readonly IIssuedCouponsModelFactory _issuedCouponsModelFactory;

        private readonly IPromoUtilities _promoUtilities;
        private readonly IOrderService _orderService;
        private readonly IPromoOrderService _promoOrderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IProductMappingService _productMappingService;
        private readonly PromoSettings _promoSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICouponService _couponService;

        #endregion

        #region constructor

        public PromoCouponController(
            IIssuedCouponsModelFactory issuedCouponsModelFactory,

            IPromoUtilities promoUtilities,
            IPromoOrderService promoOrderService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IProductMappingService productMappingService,
            IDateTimeHelper dateTimeHelper,
            ICouponService couponService)
        {
            this._issuedCouponsModelFactory = issuedCouponsModelFactory;

            this._promoUtilities = promoUtilities;
            this._promoOrderService = promoOrderService;
            this._orderService = orderService;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._productMappingService = productMappingService;
            this._dateTimeHelper = dateTimeHelper;
            this._couponService = couponService;
            this._promoSettings = promoSettings;
        }

        #endregion

        #region methods

        [ChildActionOnly]
        public ActionResult IssuedCoupons(ShoppingCartModel shoppingCartModel)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            var basketResponse = _promoUtilities.GetBasketResponse(_workContext.CurrentCustomer);

            var issuedCouponsModel = _issuedCouponsModelFactory.PrepareIssuedCouponsModel(basketResponse, shoppingCartModel.ShowSku);

            return View(issuedCouponsModel);
        }

        public ActionResult CustomerIssuedCoupons()
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            IssuedCouponsModel issuedCouponsModel = new IssuedCouponsModel();

            issuedCouponsModel.IsEditable = false;
            issuedCouponsModel.ShowProductImages = false;
            issuedCouponsModel.ShowSku = false;

            var customer = _workContext.CurrentCustomer;

            int pageIndex = 0;
            int pageSize = int.MaxValue;
            var issuedCoupons = _couponService.IssuedCoupons(customer.Id, pageIndex, pageSize).ToList();

            foreach (var issuedCoupon in issuedCoupons)
            {
                string couponName = string.Empty;
                ValidatedCouponCode validatedCouponCode = _couponService.ValidateCouponCode(_promoSettings.CompanyKey, issuedCoupon.CouponCode, out couponName);
                if (validatedCouponCode != null)
                {
                    issuedCouponsModel.Coupons.Add(new IssuedCouponModel()
                    {
                        Code = validatedCouponCode.Code,
                        Status = validatedCouponCode.Status,
                        Name = couponName,
                        IsConfirmed = true,
                        ValidTo = _dateTimeHelper.ConvertToUserTime(validatedCouponCode.ValidTo, DateTimeKind.Utc),
                        DisplayText =  issuedCoupon.DisplayText
                    });
                }
            }
            return View(issuedCouponsModel);
        }

        #endregion
    }
}

