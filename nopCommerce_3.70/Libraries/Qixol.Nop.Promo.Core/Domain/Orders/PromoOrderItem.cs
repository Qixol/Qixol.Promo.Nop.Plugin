using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Orders
{
    public class PromoOrderItem : BaseEntity
    {
        private ICollection<PromoOrderItemPromotion> _promoOrderItemPromotions;

        public int OrderId { get; set; }
        public int PromoOrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductCode { get; set; }
        public string VariantCode { get; set; }

        public decimal LineAmount { get; set; }
        public decimal LinePromotionDiscount { get; set; }

        public bool IsDelivery { get; set; }

        public string Barcode { get; set; }
        public bool Generated { get; set; }
        public decimal ManualDiscount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal Price { get; set; }
        public string ProductDescription { get; set; }
        public decimal Quantity { get; set; }
        public int SplitFromLineId { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalIssuedPoints { get; set; }

        public virtual PromoOrder PromoOrder { get; set; }

        public virtual ICollection<PromoOrderItemPromotion> PromoOrderItemPromotions
        {
            get { return _promoOrderItemPromotions ?? (_promoOrderItemPromotions = new List<PromoOrderItemPromotion>()); }
            protected set { _promoOrderItemPromotions = value; }
        }
    }
}
