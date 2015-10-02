using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Coupons
{
    public class IssuedCoupon : BaseEntity
    {
        public int CustomerId { get; set; }
        public string CouponCode { get; set; }
        public string CouponDescription { get; set; }
    }
}
