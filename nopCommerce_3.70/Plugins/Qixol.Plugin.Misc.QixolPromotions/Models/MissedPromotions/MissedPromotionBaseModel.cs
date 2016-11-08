using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models.MissedPromotions
{
    public abstract partial class MissedPromotionBaseModel
    {
        private IList<MissedPromotionCriteriaModel> _criteria;
        private IList<ShoppingCartModel.ShoppingCartItemModel> _matchedCartItemModels;
        private global::Nop.Web.Models.Catalog.ProductDetailsModel.AddToCartModel _addToCartModel;
        private string _promotionType;

        public string PromotionImageUrl { get; set; }

        // TODO: this really should not be needed - the various sub-models should be sufficient...
        public string PromotionType
        {
            get
            {
                return _promotionType ?? (_promotionType = MissedPromotionsModel.PromotionTypeSystemName.Unknown);
            }
            set
            {
                _promotionType = value;
            }
        }

        public string PromotionName { get; set; }
        public string Category { get; set; }

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

        public global::Nop.Web.Models.Catalog.ProductDetailsModel.AddToCartModel AddToCartModel
        {
            get
            {
                return _addToCartModel ?? (_addToCartModel = new global::Nop.Web.Models.Catalog.ProductDetailsModel.AddToCartModel());
            }
            set
            {
                _addToCartModel = value;
            }
        }

        public partial class MissedPromotionCriteriaModel
        {

        }
    }

}