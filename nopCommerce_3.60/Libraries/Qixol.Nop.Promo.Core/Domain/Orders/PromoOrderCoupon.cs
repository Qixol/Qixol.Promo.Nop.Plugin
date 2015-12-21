using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Orders
{
    public class PromoOrderCoupon : BaseEntity
    {
        public int OrderId { get; set; }
        public int PromoOrderId { get; set; }

        public virtual PromoOrder PromoOrder { get; set; }

        public bool Issued { get; set; }
        public string CouponCode { get; set; }

        public string CouponName { get; set; }
        public bool IssuedConfirmed { get; set; }
        public string DisplayText { get; set; }
    }
}
