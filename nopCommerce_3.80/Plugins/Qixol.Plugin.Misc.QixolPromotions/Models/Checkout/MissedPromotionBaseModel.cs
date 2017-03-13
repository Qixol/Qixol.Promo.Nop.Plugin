using Nop.Web.Models.Catalog;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models.Checkout
{
    public abstract partial class MissedPromotionBaseModel
    {
        private IList<MissedPromotionCriteriaModel> _criteria;
        private IList<ShoppingCartModel.ShoppingCartItemModel> _matchedCartItemModels;
        private IList<PromoProductDetailsModel> _unmatchedProductDetailsModels;
        private IList<CategorySimpleModel> _categorySimpleModels;

        public string PromotionImageUrl { get; set; }
        public string SaveFrom { get; set; }
        public string PromotionName { get; set; }

        public IList<MissedPromotionCriteriaModel> Criteria
        {
            get
            {
                return _criteria ?? (_criteria = new List<MissedPromotionCriteriaModel>());
            }
            set
            {
                _criteria = value;
            }
        }

        public IList<ShoppingCartModel.ShoppingCartItemModel> MatchedCartItemModels
        {
            get
            {
                return _matchedCartItemModels ?? (_matchedCartItemModels = new List<ShoppingCartModel.ShoppingCartItemModel>());
            }
            set
            {
                _matchedCartItemModels = value;
            }
        }

        public IList<PromoProductDetailsModel> UnmatchedProductDetailsModels
        {
            get
            {
                return _unmatchedProductDetailsModels ?? (_unmatchedProductDetailsModels = new List<PromoProductDetailsModel>());
            }
            set
            {
                _unmatchedProductDetailsModels = value;
            }
        }

        public IList<CategorySimpleModel> CategorySimpleModels
        {
            get
            {
                return _categorySimpleModels ?? (_categorySimpleModels = new List<CategorySimpleModel>());
            }
            set
            {
                _categorySimpleModels = value;
            }
        }

        public partial class MissedPromotionCriteriaModel
        {
        }
    }
}