using Qixol.Nop.Promo.Core.Domain.Coupons;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoIssuedCouponMap : EntityTypeConfiguration<IssuedCoupon>
    {
        public readonly static string TABLENAME = "PromoIssuedCoupon";

        public PromoIssuedCouponMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }
    }
}
