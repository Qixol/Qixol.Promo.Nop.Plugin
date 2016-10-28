using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models.MissedPromotions
{
    public partial class MissedPromotionsModel
    {
        private IList<MissedPromotionBaseModel> _missedPromotions;
        public IList<MissedPromotionBaseModel> MissedPromotions
        {
            get
            {
                return _missedPromotions ?? (_missedPromotions = new List<MissedPromotionBaseModel>());
            }
            protected set
            {
                _missedPromotions = value;
            }
        }
    }
}