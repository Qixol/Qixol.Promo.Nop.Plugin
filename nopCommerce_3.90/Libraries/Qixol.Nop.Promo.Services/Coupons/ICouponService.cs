using Nop.Core;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Promo.Integration.Lib.Coupon;

namespace Qixol.Nop.Promo.Services.Coupons
{
    public partial interface ICouponService
    {
        ValidatedCouponCode ValidateCouponCode(string companyKey, string couponCode, out string couponName);

        IPagedList<PromoOrderCoupon> IssuedCoupons(int customerId, int pageIndex, int pageSize);

        //IPagedList<PromoOrderCoupon> IssuedCoupons(int pageIndex, int pageSize);
    }
}
