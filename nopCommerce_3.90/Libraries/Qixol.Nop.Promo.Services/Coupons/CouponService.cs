using global::Nop.Core;
using global::Nop.Services.Logging;
using Nop.Core.Data;
using Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.Coupons;
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
        private readonly IRepository<IssuedCoupon> _issuedCouponRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region constructor

        public CouponService(
            ILogger logger,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IRepository<IssuedCoupon> issuedCouponRepository,
            IEventPublisher eventPublisher)
        {
            this._logger = logger;
            this._workContext = workContext;
            this._promoSettings = promoSettings;
            this._issuedCouponRepository = issuedCouponRepository;
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

        public IPagedList<IssuedCoupon> IssuedCoupons(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = (from ic in _issuedCouponRepository.Table where ic.CustomerId == customerId select ic).OrderBy(c => c.Id);
            return new PagedList<IssuedCoupon>(query, pageIndex, pageSize);
        }

        public IPagedList<IssuedCoupon> IssuedCoupons(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = (from ic in _issuedCouponRepository.Table select ic).OrderBy(c => c.Id);
            return new PagedList<IssuedCoupon>(query, pageIndex, pageSize);
        }

        public void InsertIssuedCoupon(IssuedCoupon issuedCoupon)
        {
            if (issuedCoupon == null)
                throw new ArgumentNullException("issuedCoupon");

            _issuedCouponRepository.Insert(issuedCoupon);

            _eventPublisher.EntityInserted(issuedCoupon);
        }

        #endregion
    }
}
