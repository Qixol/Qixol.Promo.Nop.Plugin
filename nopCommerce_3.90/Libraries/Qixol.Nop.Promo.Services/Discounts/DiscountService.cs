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

            this._promoUtilities = promoUtilities;
            this._promoSettings = promoSettings;
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

        public override Discount GetDiscountById(int discountId)
        {
            return null; // "NotSupportedException"
        }

        //public override IList<Discount> GetAllDiscounts(DiscountType? discountType, string couponCode = "", bool showHidden = false)
        //{
        //    if (!_promoSettings.Enabled)
        //        return base.GetAllDiscounts(discountType, couponCode, showHidden);

        //    return new List<Discount>();
        //}


        /// <summary>
        /// Inserts a discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public override void InsertDiscount(Discount discount)
        {
            throw new NotSupportedException("InsertDiscount");
        }

        /// <summary>
        /// Updates the discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public override void UpdateDiscount(Discount discount)
        {
            if (!_promoSettings.Enabled)
            {
                base.UpdateDiscount(discount);
                return;
            }

            throw new NotSupportedException("UpdateDiscount");
        }

        #endregion

        #region Discounts (caching)

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

            return new List<global::Nop.Services.Discounts.IDiscountRequirementRule>(); // "NotSupportedException"
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
            return ValidateDiscount(discount, customer, couponCodesToValidate);
        }

        /// <summary>
        /// Validate discount
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodesToValidate">Coupon codes to validate</param>
        /// <returns>Discount validation result</returns>
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
                IsValid = false,
                Errors = new List<string>()
            };

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return discountValidationResult;

            if (!basketResponse.Summary.ProcessingResult)
                return discountValidationResult;

            if (basketResponse.Coupons == null)
                return discountValidationResult;

            if (basketResponse.Coupons == null)
                return discountValidationResult;

            if (basketResponse.Coupons.Count == 0)
                return discountValidationResult;

            List<BasketResponseCoupon> coupons = new List<BasketResponseCoupon>();
            for (int thisCouponCodeIndex = 0; thisCouponCodeIndex < couponCodesToValidate.Length; thisCouponCodeIndex++)
            {
                BasketResponseCoupon coupon = (from c in basketResponse.Coupons where string.Compare(c.CouponCode, couponCodesToValidate[thisCouponCodeIndex], StringComparison.InvariantCultureIgnoreCase) == 0 select c).FirstOrDefault();
                if (coupon != null)
                    coupons.Add(coupon);
            }

            if (!coupons.Any())
                return discountValidationResult;

            coupons.ForEach(c =>
            {
                discountValidationResult.IsValid = discountValidationResult.IsValid && c.Utilized;
                if (!c.Utilized)
                {
                    discountValidationResult.Errors.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                }
            });

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

        //public override void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        //{
        //    if (discountUsageHistory == null)
        //        throw new ArgumentNullException("discountUsageHistory");

        //    _discountUsageHistoryRepository.Insert(discountUsageHistory);

        //    _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

        //    //event notification
        //    _eventPublisher.EntityInserted(discountUsageHistory);
        //}

        /// <summary>
        /// Update discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public override void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (!_promoSettings.Enabled)
            {
                base.UpdateDiscountUsageHistory(discountUsageHistory);
            }

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
