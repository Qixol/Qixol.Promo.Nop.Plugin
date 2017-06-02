using System.Collections.Generic;

namespace Qixol.Plugin.Misc.Promo.Models.Shared
{
    public partial class LineItemModel
    {
        #region fields

        private IList<PromotionModel> _lineDiscounts;

        #endregion

        #region public properties

        public string ProductSeName { get; set; }

        public string AttributeInfo { get; set; }

        public string LineAmount { get; set; }

        public IList<PromotionModel> LineDiscounts
        {
            get
            {
                return _lineDiscounts ?? (_lineDiscounts = new List<PromotionModel>());
            }
            set
            {
                _lineDiscounts = value;
            }
        }
    }

    #endregion
}
