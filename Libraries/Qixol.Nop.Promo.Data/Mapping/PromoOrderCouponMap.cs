using Qixol.Nop.Promo.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoOrderCouponMap : EntityTypeConfiguration<PromoOrderCoupon>
    {
        public readonly static string TABLENAME = "PromoOrderCoupon";

        public PromoOrderCouponMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);

            this.HasRequired(promoOrderCoupon => promoOrderCoupon.PromoOrder)
                .WithMany(o => o.PromoOrderCoupons)
                .HasForeignKey(promoOrderCoupon => promoOrderCoupon.PromoOrderId);
        }
    }
}
