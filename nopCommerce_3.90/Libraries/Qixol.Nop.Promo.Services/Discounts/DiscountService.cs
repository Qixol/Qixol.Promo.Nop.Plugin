using System;
using System.Collections.Generic;
using System.Linq;
using global::Nop.Core;
using global::Nop.Core.Caching;
using global::Nop.Core.Data;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Discounts;
using global::Nop.Core.Domain.Orders;
using global::Nop.Core.Plugins;
using global::Nop.Services.Catalog;
using global::Nop.Services.Customers;
using global::Nop.Services.Discounts;
using global::Nop.Services.Discounts.Cache;
using global::Nop.Services.Events;
using global::Nop.Services.Localization;
using global::Nop.Services.Orders;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Nop.Services.Common;

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
        private readonly IRepository<DiscountRequirement> _discountRequirementRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="discountRepository">Discount repository</param>
        /// <param name="discountRequirementRepository">Discount requirement repository</param>
        /// <param name="discountUsageHistoryRepository">Discount usage history repository</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="categoryService">Category service</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="workContext">work context</param>
        public DiscountService(ICacheManager cacheManager,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRequirement> discountRequirementRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher,
            IWorkContext workContext,
            IPromoUtilities promoUtilities,
            PromoSettings promoSettings
            ) :
                base(
                    cacheManager,
                    discountRepository,
                    discountRequirementRepository,
                    discountUsageHistoryRepository,
                    storeContext,
                    localizationService,
                    categoryService,
                    pluginFinder,
                    eventPublisher,
                    workContext)
        {
            this._cacheManager = cacheManager;
            this._discountRepository = discountRepository;
            this._discountUsageHistoryRepository = discountUsageHistoryRepository;
            this._localizationService = localizationService;
            this._eventPublisher = eventPublisher;
            this._workContext = workContext;

            this._promoUtilities = promoUtilities;
            this._promoSettings = promoSettings;
            this._storeContext = storeContext;
        }

        #endregion

        #region Nested classes
        #endregion

        #region Utilities

        private Discount MapCouponPromotionToDiscount(BasketResponseCoupon coupon)
        {
            if (coupon == null)
                return null;

            if (coupon.Utilizations == null || !coupon.Utilizations.Any())
                return null;

            var utilization = coupon.Utilizations.FirstOrDefault();

            var discount = new Discount()
            {
                Id = utilization.PromotionId,
                Name = coupon.CouponName,
                CouponCode = coupon.CouponCode,
                RequiresCouponCode = true
            };

            return discount;
        }

        private DiscountForCaching MapCouponPromotionToDiscountForCaching(BasketResponseCoupon coupon)
        {
            var discount = MapCouponPromotionToDiscount(coupon);
            if (discount == null)
                return null;

            return discount.MapDiscount();
        }

        #endregion

        #region Methods

        #region Discounts

        /// <summary>
        /// Delete discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public override void DeleteDiscount(Discount discount)
        {
            if (!_promoSettings.Enabled)
                base.DeleteDiscount(discount);

            throw new NotSupportedException("DeleteDiscount");
        }

        /// <summary>
        /// Gets a discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        public override Discount GetDiscountById(int discountId)
        {
            if (!_promoSettings.Enabled)
                return base.GetDiscountById(discountId);

            if (_workContext == null || _workContext.CurrentCustomer == null)
                return null;

            if (_storeContext == null || _storeContext.CurrentStore == null)
                return null;

            var basketResponse = _workContext.CurrentCustomer.GetAttribute<BasketResponse>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);
            if (basketResponse == null || !basketResponse.IsValid())
                return null;

            Discount discount = null;

            var coupons = basketResponse.Coupons.Where(c => !c.Issued && c.Utilizations.Any()).ToList();
            if (coupons != null && coupons.Any())
            {
                coupons.ForEach(c =>
                {
                    var utilization = c.Utilizations.Where(u => u.PromotionId == discountId).FirstOrDefault();
                    if (utilization != null)
                    {
                        discount = MapCouponPromotionToDiscount(c);
                    }
                });
            }

            return discount;

        }

        public override IList<Discount> GetAllDiscounts(DiscountType? discountType = null,
            string couponCode = "", string discountName = "", bool showHidden = false)
        {
            if (!_promoSettings.Enabled)
                return base.GetAllDiscounts(discountType, couponCode, discountName, showHidden);

            return new List<Discount>();
        }


        /// <summary>
        /// Inserts a discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public override void InsertDiscount(Discount discount)
        {
            if (!_promoSettings.Enabled)
                base.InsertDiscount(discount);

            throw new NotSupportedException("InsertDiscount");
        }

        /// <summary>
        /// Updates the discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public override void UpdateDiscount(Discount discount)
        {
            if (!_promoSettings.Enabled)
                base.UpdateDiscount(discount);

            throw new NotSupportedException("UpdateDiscount");
        }

        #endregion

        #region Discounts (caching)

        /// <summary>
        /// Gets all discounts (cachable models)
        /// </summary>
        /// <param name="discountType">Discount type; null to load all discount</param>
        /// <param name="couponCode">Coupon code to find (exact match)</param>
        /// <param name="discountName">Discount name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discounts</returns>
        public override IList<DiscountForCaching> GetAllDiscountsForCaching(DiscountType? discountType,
            string couponCode = "", string discountName = "", bool showHidden = false)
        {
            if (!_promoSettings.Enabled)
                return base.GetAllDiscountsForCaching(discountType, couponCode, discountName, showHidden);

            if (string.IsNullOrWhiteSpace(couponCode))
                return new List<DiscountForCaching>();

            if (_workContext == null || _workContext.CurrentCustomer == null)
                return new List<DiscountForCaching>();

            if (_storeContext == null || _storeContext.CurrentStore == null)
                return new List<DiscountForCaching>();

            var basketResponse = _workContext.CurrentCustomer.GetAttribute<BasketResponse>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);
            if (basketResponse == null || !basketResponse.IsValid())
                return new List<DiscountForCaching>();

            if (basketResponse.Coupons == null || !basketResponse.Coupons.Any())
                return new List<DiscountForCaching>();

            var discountsForCaching = new List<DiscountForCaching>();
            var basketCoupons = basketResponse.Coupons.Where(c => !c.Issued && c.Utilizations.Any() && string.Compare(c.CouponCode, couponCode, StringComparison.InvariantCultureIgnoreCase) == 0).ToList();

            if (basketCoupons != null && basketCoupons.Any())
            {
                var c = basketCoupons.First();
                discountsForCaching.Add(MapCouponPromotionToDiscountForCaching(c));
            }

            return discountsForCaching;
        }

        #endregion

        #region Discount requirements


        /// <summary>
        /// Delete discount requirement
        /// </summary>
        /// <param name="discountRequirement">Discount requirement</param>
        public override void DeleteDiscountRequirement(DiscountRequirement discountRequirement)
        {
            if (!_promoSettings.Enabled)
                base.DeleteDiscountRequirement(discountRequirement);

            throw new NotSupportedException("DeleteDiscountRequirement");
        }

        /// <summary>
        /// Load discount requirement rule by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found discount requirement rule</returns>
        public override global::Nop.Services.Discounts.IDiscountRequirementRule LoadDiscountRequirementRuleBySystemName(string systemName)
        {
            if (!_promoSettings.Enabled)
                return base.LoadDiscountRequirementRuleBySystemName(systemName);

            throw new NotSupportedException("LoadDiscountRequirementRuleBySystemName");
        }

        /// <summary>
        /// Load all discount requirement rules
        /// </summary>
        /// <param name="customer">Load records allowed only to a specified customer; pass null to ignore ACL permissions</param>
        /// <returns>Discount requirement rules</returns>
        public override IList<global::Nop.Services.Discounts.IDiscountRequirementRule> LoadAllDiscountRequirementRules(Customer customer = null)
        {
            if (!_promoSettings.Enabled)
                return base.LoadAllDiscountRequirementRules(customer);

            return new List<global::Nop.Services.Discounts.IDiscountRequirementRule>();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discount validation result</returns>
        public override global::Nop.Services.Discounts.DiscountValidationResult ValidateDiscount(Discount discount, global::Nop.Core.Domain.Customers.Customer customer)
        {
            if (!_promoSettings.Enabled)
                return base.ValidateDiscount(discount, customer);

            if (discount == null)
                throw new ArgumentNullException("discount");

            if (customer == null)
                throw new ArgumentNullException("customer");

            string[] couponCodesToValidate = customer.ParseAppliedDiscountCouponCodes();
            return ValidateDiscount(discount.MapDiscount(), customer, couponCodesToValidate);
        }

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate</param>
        /// <returns>Discount validation result</returns>
        public override global::Nop.Services.Discounts.DiscountValidationResult ValidateDiscount(Discount discount, global::Nop.Core.Domain.Customers.Customer customer, string[] couponCodesToValidate)
        {
            if (!_promoSettings.Enabled)
                return base.ValidateDiscount(discount, customer, couponCodesToValidate);

            if (discount == null)
                throw new ArgumentNullException("discount");

            if (customer == null)
                throw new ArgumentNullException("customer");

            return ValidateDiscount(discount.MapDiscount(), customer, couponCodesToValidate);
        }

        public override DiscountValidationResult ValidateDiscount(DiscountForCaching discount, Customer customer)
        {
            if (!_promoSettings.Enabled)
                return base.ValidateDiscount(discount, customer);

            if (discount == null)
                throw new ArgumentNullException("discount");

            if (customer == null)
                throw new ArgumentNullException("customer");

            string[] couponCodesToValidate = customer.ParseAppliedDiscountCouponCodes();

            return ValidateDiscount(discount, customer, couponCodesToValidate);
        }

        public override global::Nop.Services.Discounts.DiscountValidationResult ValidateDiscount(DiscountForCaching discount, global::Nop.Core.Domain.Customers.Customer customer, string[] couponCodesToValidate)
        {
            if (!_promoSettings.Enabled)
                return base.ValidateDiscount(discount, customer, couponCodesToValidate);

            if (discount == null)
                throw new ArgumentNullException("discount");

            if (customer == null)
                throw new ArgumentNullException("customer");

            global::Nop.Services.Discounts.DiscountValidationResult discountValidationResult = new global::Nop.Services.Discounts.DiscountValidationResult()
            {
                IsValid = false
            };

            BasketResponse basketResponse = customer.GetAttribute<BasketResponse>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);

            if (basketResponse == null || !basketResponse.IsValid())
            {
                return discountValidationResult;
            }

            if (basketResponse.Coupons == null)
            {
                return discountValidationResult;
            }

            if (!basketResponse.Coupons.Where(c => !c.Issued && c.Utilizations.Any()).Any())
            {
                return discountValidationResult;
            }

            List<BasketResponseCoupon> coupons = new List<BasketResponseCoupon>();
            couponCodesToValidate.ToList().ForEach(couponCodeToValidate =>
            {
                BasketResponseCoupon coupon = (from c in basketResponse.Coupons where string.Compare(c.CouponCode, couponCodeToValidate, StringComparison.InvariantCultureIgnoreCase) == 0 select c).FirstOrDefault();
                if (coupon != null)
                    coupons.Add(coupon);
            });

            if (!coupons.Any())
                return discountValidationResult;

            coupons.ForEach(c =>
            {
                discountValidationResult.IsValid = discountValidationResult.IsValid && c.Utilizations.Any();
                if (!c.Utilizations.Any())
                {
                    discountValidationResult.Errors.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                }
            });

            if (!discountValidationResult.Errors.Any())
                discountValidationResult.IsValid = true;

            return discountValidationResult;
        }

        #endregion

        #region Discount usage history

        //public override DiscountUsageHistory GetDiscountUsageHistoryById(int discountUsageHistoryId)
        //{

        //    DiscountUsageHistory discountUsageHistory = new DiscountUsageHistory();

        //    return base.GetDiscountUsageHistoryById(discountUsageHistoryId);
        //}

        public override global::Nop.Core.IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(int? discountId, int? customerId, int? orderId, int pageIndex, int pageSize)
        {
            return base.GetAllDiscountUsageHistory(discountId, customerId, orderId, pageIndex, pageSize);
        }

        /// <summary>
        /// Update discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public override void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (!_promoSettings.Enabled)
                base.UpdateDiscountUsageHistory(discountUsageHistory);

            throw new NotImplementedException("UpdateDiscountUsageHistory");
        }

        /// <summary>
        /// Delete discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public override void DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (!_promoSettings.Enabled)
                base.DeleteDiscountUsageHistory(discountUsageHistory);

            throw new NotImplementedException("DeleteDiscountUsageHistory");
        }

        #endregion

        #endregion
    }
}
