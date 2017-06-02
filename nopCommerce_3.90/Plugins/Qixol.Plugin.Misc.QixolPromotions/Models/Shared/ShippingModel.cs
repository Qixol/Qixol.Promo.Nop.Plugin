using System.Collections.Generic;

namespace Qixol.Plugin.Misc.Promo.Models.Shared
{
    public partial class ShippingModel
    {
        private IList<PromotionModel> _shippingPromotions;

        public string ShippingAmount { get; set; }

        public string OriginalShippingAmount { get; set; }

        public IList<PromotionModel> ShippingPromotions
        {
            get
            {
                return _shippingPromotions ?? (_shippingPromotions = new List<PromotionModel>());
            }
            set
            {
                _shippingPromotions = value;
            }
        }
    }
}
