using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Orders;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial class IssuedCouponsModelFactory : IIssuedCouponsModelFactory
    {
        #region fields

        private readonly IIssuedCouponModelFactory _issuedCouponModelFactory;

        #endregion

        #region constructor

        public IssuedCouponsModelFactory(IIssuedCouponModelFactory issuedCouponModelFactory)
        {
            this._issuedCouponModelFactory = issuedCouponModelFactory;
        }

        #endregion

        #region Utilities
        #endregion

        #region methods

        public IssuedCouponsModel PrepareIssuedCouponsModel(BasketResponse basketResponse, bool showSku)
        {
            if (basketResponse == null)
                return null;

            var issuedCouponsModel = new IssuedCouponsModel()
            {
                IsEditable = false,
                ShowProductImages = false,
                ShowSku = showSku
            };


            basketResponse.IssuedCoupons().ToList().ForEach(issuedCoupon =>
            {
                issuedCouponsModel.Coupons.Add(_issuedCouponModelFactory.PrepareIssuedCouponModel(issuedCoupon));
            });

            return issuedCouponsModel;
        }

        /// <summary>
        /// Prepare the missed promotions model
        /// </summary>
        /// <returns>Missed promotions model</returns>
        public IssuedCouponsModel PrepareIssuedCouponsModel(Order order, bool showSku)
        {
            if (order == null)
                return null;

            var issuedCouponsModel = new IssuedCouponsModel()
            {
                IsEditable = false,
                ShowProductImages = false,
                ShowSku = showSku
            };

            order.PromoIssuedCoupons().ToList().ForEach(ic =>
            {
                issuedCouponsModel.Coupons.Add(_issuedCouponModelFactory.PrepareIssuedCouponModel(ic));
            });

            return issuedCouponsModel;
        }

        #endregion

    }

    public partial class IssuedCouponModelFactory : IIssuedCouponModelFactory
    {
        #region fields
        #endregion

        #region constructor
        #endregion

        #region Utilities
        #endregion

        #region methods

        public IssuedCouponModel PrepareIssuedCouponModel(BasketResponseCoupon issuedCoupon)
        {
            if (issuedCoupon == null)
                return null;

            // handle issued coupon when promo is not correctly configured to require basket confirmation
            if (issuedCoupon.FromPreviousIteration && issuedCoupon.IssuedByPromotionId == 0)
                return null;

            var issuedCouponModel = new IssuedCouponModel()
            {
                Name = issuedCoupon.CouponName,
                // Never show the coupon code until the transaction is complete
                ValidTo = DateTime.MinValue,
                Status = string.Empty,
                IsConfirmed = false, // issuedCoupon.IssuedConfirmed,
                Code = string.Empty, // issuedCoupon.IssuedConfirmed == true ? issuedCoupon.CouponCode : string.Empty,
                DisplayText = string.IsNullOrWhiteSpace(issuedCoupon.DisplayText) ? string.Empty : issuedCoupon.DisplayText
            };

            return issuedCouponModel;
        }

        public IssuedCouponModel PrepareIssuedCouponModel(PromoOrderCoupon issuedCoupon)
        {
            if (issuedCoupon == null)
                return null;

            var issuedCouponModel = new IssuedCouponModel()
            {
                Name = issuedCoupon.CouponName,
                // Never show the coupon code until the transaction is complete
                ValidTo = DateTime.MinValue,
                Status = string.Empty,
                IsConfirmed = issuedCoupon.IssuedConfirmed,
                Code = issuedCoupon.IssuedConfirmed == true ? issuedCoupon.CouponCode : string.Empty,
                DisplayText = string.IsNullOrWhiteSpace(issuedCoupon.DisplayText) ? string.Empty : issuedCoupon.DisplayText
            };

            return issuedCouponModel;
        }

        #endregion
    }
}
