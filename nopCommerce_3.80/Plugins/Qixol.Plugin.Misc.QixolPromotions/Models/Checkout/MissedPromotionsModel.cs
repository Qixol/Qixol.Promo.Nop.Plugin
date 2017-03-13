using Nop.Core.Domain.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models.Checkout
{
    public partial class MissedPromotionsModel
    {
        private IList<object> _missedPromotions;

        public string ContinueShoppingUrl { get; set; }
        
        public int MissedPromotionsPictureId { get; set; }

        public virtual Picture MissedPromotionsPicture { get; }

        public IList<object> MissedPromotions
        {
            get
            {
                return _missedPromotions ?? (_missedPromotions = new List<object>());
            }
            protected set
            {
                _missedPromotions = value;
            }
        }
        public class PromotionTypeSystemName
        {
            public const string BasketReduction = "BASKETREDUCTION";
            public const string BuyOneGetOneFree = "BOGOF";
            public const string ProductsReduction = "PRODUCTSREDUCTION";
            public const string DeliveryReduction = "DELIVERYREDUCTION";
            public const string DealPrice = "DEAL";
            public const string BuyOneGetOneReduced = "BOGOR";
            public const string BundlePrice = "BUNDLE";
            public const string IssueCoupon = "ISSUECOUPON";
            public const string IssuePoints = "ISSUEPOINTS";
            public const string FreeProduct = "FREEPRODUCT";

            //public const string NotSelected = "UNSELECTED";
            public const string Unknown = "UNKNOWN";

        }
    }
}