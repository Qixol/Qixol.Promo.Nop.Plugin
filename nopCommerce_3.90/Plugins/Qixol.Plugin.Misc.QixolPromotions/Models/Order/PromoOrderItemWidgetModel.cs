using global::Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using Qixol.Plugin.Misc.Promo.Models.Shared;

namespace Qixol.Plugin.Misc.Promo.Models.Order
{
    public partial class PromoOrderItemWidgetModel : BaseNopModel
    {
        #region fields

        private IList<PromotionModel> _promotions;

        #endregion

        #region public properties

        public int OrderItemId { get; set; }

        public IList<PromotionModel> Promotions
        {
            get
            {
                return _promotions ?? (_promotions = new List<PromotionModel>());
            }
            set
            {
                _promotions = value;
            }
        }

        public string LineAmount { get; set; }

        #endregion
    }
}
