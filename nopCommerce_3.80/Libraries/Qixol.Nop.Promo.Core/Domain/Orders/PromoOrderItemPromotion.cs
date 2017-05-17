using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Orders
{
    public class PromoOrderItemPromotion : BaseEntity
    {
        public int PromoOrderItemId { get; set; }

        public int PromotionId { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool BasketLevel { get; set; }
        public bool DeliveryLevel { get; set; }

        [Obsolete("no longer used", false)]
        public int ForLineId { get; set; }
        public int Instance { get; set; }
        public decimal PointsIssued { get; set; }

        public string DisplayText { get; set; }
        public string PromotionTypeDisplay { get; set; }
        public string PromotionType { get; set; }
        public string PromotionName { get; set; }

        public string ExternalIdentifier { get; set; }
        public string ReportingGroupCode { get; set; }

        public virtual PromoOrderItem PromoOrderItem { get; set; }
    }
}
