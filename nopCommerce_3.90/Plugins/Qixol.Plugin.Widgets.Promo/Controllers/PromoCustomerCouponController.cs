using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Models.ShoppingCart;
using Qixol.Plugin.Widgets.Promo.Models;
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
using Qixol.Nop.Promo.Core.Domain.Coupons;

namespace Qixol.Plugin.Widgets.Promo.Controllers
{
    public partial class PromoCustomerCouponController : Controller
    {
        #region Fields

        private readonly IPromoUtilities _promoUtilities;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IProductMappingService _productMappingService;
        private readonly ICouponService _couponService;
        private readonly PromoSettings _promoSettings;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region constructor

        public PromoCustomerCouponController(
            IPromoUtilities promoUtilities,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IProductMappingService productMappingService,
            ICouponService couponService,
            IDateTimeHelper dateTimeHelper)
        {
            this._promoUtilities = promoUtilities;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._productMappingService = productMappingService;
            this._couponService = couponService;
            this._dateTimeHelper = dateTimeHelper;

            this._promoSettings = promoSettings;
        }

        #endregion

        #region methods

        public ActionResult CustomerIssuedCoupons()
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            IssuedCouponsModel issuedCouponsModel = new IssuedCouponsModel();

            issuedCouponsModel.IsEditable = false;
            issuedCouponsModel.ShowProductImages = false;
            issuedCouponsModel.ShowSku = false;

            var customer = _workContext.CurrentCustomer;

            // TODO: paging for issued coupons
            int pageIndex = 0;
            int pageSize = int.MaxValue;
            List<IssuedCoupon> issuedCoupons = _couponService.IssuedCoupons(customer.Id, pageIndex, pageSize).ToList();

            foreach (IssuedCoupon issuedCoupon in issuedCoupons)
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
                        DisplayDetails = issuedCoupon.CouponDescription
                    });
                }
            }
            return View(issuedCouponsModel);
        }

        #endregion
    }
}

