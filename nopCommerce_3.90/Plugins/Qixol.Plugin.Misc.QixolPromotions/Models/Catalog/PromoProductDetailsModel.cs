using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Nop.Web.Models.Catalog;

namespace Qixol.Plugin.Misc.Promo.Models
{
    public class PromoProductDetailsModel
    {
        private IDictionary<string, string> _attributes;

        public decimal AdditionalQuantityNeededToTriggerPromotion { get; set; }

        public ProductDetailsModel ProductDetailsModel { get; set; }

        public IDictionary<string, string> Attributes
        {
            get
            {
                return _attributes ?? (_attributes = new Dictionary<string, string>());
            }
            set
            {
                _attributes = value;
            }
        }
    }
}
