
using System.Collections.Generic;

namespace Qixol.Plugin.Misc.Promo.Models.Shared
{
    public partial class PromoWidgetModel
    {
        #region fields

        private IList<LineItemModel> _lineDiscountsModel;
        private IList<IssuedCouponModel> _issuedCouponsModel;
        private IList<PromotionModel> _basketLevelDiscountsExcShippingModel;
        private IList<PromotionModel> _basketLevelDiscountsIncShippingModel;
        private IList<string> _seoListForFailedFreeGifts;

        #endregion

        #region public properties

        public string BasketTotalDiscount { get; set; }

        public IList<LineItemModel> LineDiscountsModel
        {
            get
            {
                return _lineDiscountsModel ?? (_lineDiscountsModel = new List<LineItemModel>());
            }
            set
            {
                _lineDiscountsModel = value;
            }
        }

        public IList<IssuedCouponModel> IssuedCouponsModel
        {
            get
            {
                return _issuedCouponsModel ?? (_issuedCouponsModel = new List<IssuedCouponModel>());
            }
            set
            {
                _issuedCouponsModel = value;
            }
        }

        public string SubTotal { get; set; }

        public IList<PromotionModel> BasketLevelDiscountsExcShippingModel
        {
            get
            {
                return _basketLevelDiscountsExcShippingModel ?? (_basketLevelDiscountsExcShippingModel = new List<PromotionModel>());
            }
            set
            {
                _basketLevelDiscountsExcShippingModel = value;
            }
        }

        public ShippingModel ShippingModel { get; set; }

        public IList<PromotionModel> BasketLevelDiscountsIncShippingModel
        {
            get
            {
                return _basketLevelDiscountsIncShippingModel ?? (_basketLevelDiscountsIncShippingModel = new List<PromotionModel>());
            }
            set
            {
                _basketLevelDiscountsIncShippingModel = value;
            }
        }

        public IList<string> SeoListForFailedFreeGifts
        {
            get
            {
                return _seoListForFailedFreeGifts  ?? (_seoListForFailedFreeGifts = new List<string>());
            }
            set
            {
                _seoListForFailedFreeGifts = value;
            }
        }

        public string OrderTotal { get; set; }

        public string IssuedPoints { get; set; }

        #endregion
    }
}
