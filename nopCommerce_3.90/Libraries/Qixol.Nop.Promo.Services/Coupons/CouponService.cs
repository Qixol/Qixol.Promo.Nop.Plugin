using global::Nop.Core;
using global::Nop.Services.Logging;
using Nop.Core.Data;
using Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib;
using Qixol.Promo.Integration.Lib.Coupon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Qixol.Nop.Promo.Services.Coupons
{
    public partial class CouponService : ICouponService
    {
        #region fields

        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly PromoSettings _promoSettings;
        private readonly IRepository<PromoOrder> _promoOrderRepository;
        private readonly IRepository<PromoOrderCoupon> _promoOrderCouponRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region constructor

        public CouponService(
            ILogger logger,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IRepository<PromoOrder> promoOrderRepository,
            IRepository<PromoOrderCoupon> promoOrderCouponRepository,
            IEventPublisher eventPublisher)
        {
            this._logger = logger;
            this._workContext = workContext;
            this._promoSettings = promoSettings;
            this._promoOrderRepository = promoOrderRepository;
            this._promoOrderCouponRepository = promoOrderCouponRepository;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region methods

        public ValidatedCouponCode ValidateCouponCode(string companyKey, string couponCode, out string couponName)
        {
            couponName = string.Empty;

            BasketServiceManager basketServiceManager = _promoSettings.GetBasketService();

            ValidatedCouponResponse validatedCouponResponse = basketServiceManager.ValidateCouponCode(_promoSettings.CompanyKey, couponCode);

            if (!validatedCouponResponse.Summary.ProcessingResult)
                return null;

            if (validatedCouponResponse.Coupon == null)
                return null;

            if (validatedCouponResponse.Coupon.Codes == null)
                return null;
            
            if (validatedCouponResponse.Coupon.Codes.Count != 1)
                return null;

            couponName = validatedCouponResponse.Coupon.Name;
            
            return validatedCouponResponse.Coupon.Codes.First();
        }

        public IPagedList<PromoOrderCoupon> IssuedCoupons(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var issuedCoupons = (from c in _promoOrderCouponRepository.Table
                                 where c.Issued
                                 select c).ToList();

            var query = (from o in _promoOrderRepository.Table
                         join c in _promoOrderCouponRepository.Table
                         on o.Id equals c.PromoOrderId
                         where o.CustomerId == customerId
                         && c.Issued
                         select c).OrderBy(c => c.Id);

            return new PagedList<PromoOrderCoupon>(query, pageIndex, pageSize);
        }

        //public IPagedList<PromoOrderCoupon> IssuedCoupons(int pageIndex = 0, int pageSize = int.MaxValue)
        //{
        //    var query = (from ic in _promoOrderCouponRepository.Table select ic).OrderBy(c => c.Id);
        //    return new PagedList<PromoOrderCoupon>(query, pageIndex, pageSize);
        //}

        #endregion
    }
}
