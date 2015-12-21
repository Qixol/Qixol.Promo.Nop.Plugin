using Nop.Core;
using Qixol.Nop.Promo.Core.Domain.Coupons;
using Qixol.Promo.Integration.Lib.Coupon;

namespace Qixol.Nop.Promo.Services.Coupons
{
    public partial interface ICouponService
    {
        ValidatedCouponCode ValidateCouponCode(string companyKey, string couponCode, out string couponName);

        IPagedList<IssuedCoupon> IssuedCoupons(int customerId, int pageIndex, int pageSize);

        IPagedList<IssuedCoupon> IssuedCoupons(int pageIndex, int pageSize);

        void InsertIssuedCoupon(IssuedCoupon issuedCoupon);
    }
}
