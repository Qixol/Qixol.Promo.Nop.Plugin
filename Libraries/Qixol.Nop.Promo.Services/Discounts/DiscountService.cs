using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Nop.Core.Data;
using global::Nop.Core.Domain.Discounts;
using global::Nop.Core.Caching;
using global::Nop.Services.Events;
using global::Nop.Core;
using global::Nop.Services.Common;
using global::Nop.Core.Plugins;
using Qixol.Nop.Promo.Services.Coupons;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;

namespace Qixol.Nop.Promo.Services.Discounts
{
    public partial class DiscountService : global::Nop.Services.Discounts.DiscountService, global::Nop.Services.Discounts.IDiscountService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : coupon code
        /// </remarks>
        private const string DISCOUNTS_ALL_KEY = "Nop.discount.all-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string DISCOUNTS_PATTERN_KEY = "Nop.discount.";

        #endregion

        #region Fields

        private readonly IPromoUtilities _promoUtilities;
        private readonly PromoSettings _promoSettings;

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region constructor

        public DiscountService(
            IPromoUtilities promoUtilities,
            PromoSettings promoSettings,
            ICacheManager cacheManager,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRequirement> discountRequirementRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher) : 
                base(
                    cacheManager,
                    discountRepository,
                    discountRequirementRepository,
                    discountUsageHistoryRepository,
                    storeContext,
                    genericAttributeService,
                    pluginFinder,
                    eventPublisher)
        {
            this._promoUtilities = promoUtilities;
            this._promoSettings = promoSettings;

            this._discountRepository = discountRepository;
            this._discountUsageHistoryRepository = discountUsageHistoryRepository;
            this._cacheManager = cacheManager;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region methods

        public override void DeleteDiscount(Discount discount)
        {
            if (!_promoSettings.Enabled)
                base.DeleteDiscount(discount);
            
            throw new NotSupportedException("DeleteDiscount");
        }

        public override void DeleteDiscountRequirement(DiscountRequirement discountRequirement)
        {
            if (!_promoSettings.Enabled)
                base.DeleteDiscountRequirement(discountRequirement);
            
            throw new NotSupportedException("DeleteDiscountRequirement");
        }

        public override void DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (!_promoSettings.Enabled)
                base.DeleteDiscountUsageHistory(discountUsageHistory);

            throw new NotImplementedException("DeleteDiscountUsageHistory");
        }

        public override global::Nop.Core.IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(int? discountId, int? customerId, int? orderId, int pageIndex, int pageSize)
        {
            return base.GetAllDiscountUsageHistory(discountId, customerId, orderId, pageIndex, pageSize);
        }

        //public override IList<Discount> GetAllDiscounts(DiscountType? discountType, string couponCode = "", bool showHidden = false)
        //{
        //    if (!_promoSettings.Enabled)
        //        return base.GetAllDiscounts(discountType, couponCode, showHidden);

        //    return new List<Discount>();
        //}

        public override Discount GetDiscountByCouponCode(string couponCode, bool showHidden = false)
        {
            if (!_promoSettings.Enabled)
                return base.GetDiscountByCouponCode(couponCode, showHidden);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return null;

            if (!basketResponse.Summary.ProcessingResult)
                return null;

            if (basketResponse.Coupons == null)
                return null;

            if (basketResponse.Coupons.Count == 0)
                return null;

            var coupon = (from c in basketResponse.Coupons where string.Compare(c.CouponCode, couponCode, StringComparison.InvariantCultureIgnoreCase) == 0 select c).FirstOrDefault();

            if (coupon == null)
                return null;

            if (!coupon.Utilized)
                return null;

            // FromPreviousIteration will only be set if the company is not set to require confirming baskets - the coupon is still valid
            //if (coupon.FromPreviousIteration)
            //    return null;

            Discount discount = new Discount()
            {
                CouponCode = couponCode,
                Name = coupon.CouponName,
                RequiresCouponCode = true
            };

            return discount;
        }

        public override Discount GetDiscountById(int discountId)
        {
            return null; // "NotSupportedException"
        }

        //public override DiscountUsageHistory GetDiscountUsageHistoryById(int discountUsageHistoryId)
        //{

        //    DiscountUsageHistory discountUsageHistory = new DiscountUsageHistory();

        //    return base.GetDiscountUsageHistoryById(discountUsageHistoryId);
        //}

        public override void InsertDiscount(Discount discount)
        {
            throw new NotSupportedException("InsertDiscount");
        }

        //public override void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        //{
        //    if (discountUsageHistory == null)
        //        throw new ArgumentNullException("discountUsageHistory");

        //    _discountUsageHistoryRepository.Insert(discountUsageHistory);

        //    _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

        //    //event notification
        //    _eventPublisher.EntityInserted(discountUsageHistory);
        //}

        public override bool IsDiscountValid(Discount discount, global::Nop.Core.Domain.Customers.Customer customer, string couponCodeToValidate)
        {
            if (!_promoSettings.Enabled)
                return base.IsDiscountValid(discount, customer, couponCodeToValidate);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return false;

            if (!basketResponse.Summary.ProcessingResult)
                return false;

            if (basketResponse.Coupons == null)
                    return false;

            if (basketResponse.Coupons == null)
                return false;

            if (basketResponse.Coupons.Count == 0)
                return false;

            var coupon = (from c in basketResponse.Coupons where c.CouponCode.Equals(couponCodeToValidate, StringComparison.InvariantCultureIgnoreCase) select c).FirstOrDefault();

            if (coupon == null)
                return false;

            return coupon.Utilized;
        }

        public override bool IsDiscountValid(Discount discount, global::Nop.Core.Domain.Customers.Customer customer)
        {
            if (!_promoSettings.Enabled)
                return base.IsDiscountValid(discount, customer);

            if (discount.RequiresCouponCode)
            {
                return IsDiscountValid(discount, customer, discount.CouponCode);
            }
            else
            {
                throw new NotImplementedException("IsDiscountValid");
            }
        }

        public override IList<global::Nop.Services.Discounts.IDiscountRequirementRule> LoadAllDiscountRequirementRules()
        {
            if (!_promoSettings.Enabled)
                return base.LoadAllDiscountRequirementRules();

            return new List<global::Nop.Services.Discounts.IDiscountRequirementRule>(); // "NotSupportedException"
        }

        public override global::Nop.Services.Discounts.IDiscountRequirementRule LoadDiscountRequirementRuleBySystemName(string systemName)
        {
            if (!_promoSettings.Enabled)
                return base.LoadDiscountRequirementRuleBySystemName(systemName);

            throw new NotSupportedException("LoadDiscountRequirementRuleBySystemName");
        }

        public override void UpdateDiscount(Discount discount)
        {
            if (!_promoSettings.Enabled)
            {
                base.UpdateDiscount(discount);
                return;
            }

            throw new NotSupportedException("UpdateDiscount");
        }

        public override void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (!_promoSettings.Enabled)
            {
                base.UpdateDiscountUsageHistory(discountUsageHistory);
            }

            throw new NotImplementedException("UpdateDiscountUsageHistory");
        }

        #endregion
    }
}
