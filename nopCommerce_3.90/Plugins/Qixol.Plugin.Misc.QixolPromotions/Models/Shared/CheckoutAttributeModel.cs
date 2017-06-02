using System.Collections.Generic;

namespace Qixol.Plugin.Misc.Promo.Models.Shared
{
    public partial class CheckoutAttributeModel
    {
        #region fields

        private IList<PromotionModel> _attributeDiscounts;

        #endregion

        #region public properties

        public string CheckoutAttributeId { get; set; }

        public string CheckoutAttributeValueId { get; set; }

        public string LineAmount { get; set; }

        public IList<PromotionModel> LineDiscounts
        {
            get
            {
                return _attributeDiscounts ?? (_attributeDiscounts = new List<PromotionModel>());
            }
            set
            {
                _attributeDiscounts = value;
            }
        }
    }

    #endregion
}
