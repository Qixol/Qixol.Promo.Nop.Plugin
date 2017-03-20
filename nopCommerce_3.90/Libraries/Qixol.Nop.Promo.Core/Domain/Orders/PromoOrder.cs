using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Orders
{
    public class PromoOrder : BaseEntity
    {
        private ICollection<PromoOrderItem> _promoOrderItems;
        private ICollection<PromoOrderCoupon> _promoOrderCoupons;

        public int OrderId { get; set; }

        public string RequestXml { get; set; }
        public string ResponseXml { get; set; }

        public virtual ICollection<PromoOrderItem> PromoOrderItems
        {
            get { return _promoOrderItems ?? (_promoOrderItems = new List<PromoOrderItem>()); }
            protected set { _promoOrderItems = value; }
        }

        public virtual ICollection<PromoOrderCoupon> PromoOrderCoupons
        {
            get { return _promoOrderCoupons ?? (_promoOrderCoupons = new List<PromoOrderCoupon>()); }
            protected set { _promoOrderCoupons = value; }
        }

        public decimal DeliveryOriginalPrice { get; set; }
    }
}
