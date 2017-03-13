using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoDetailsMap : EntityTypeConfiguration<PromoDetail>
    {
        public readonly static string TABLENAME = "PromoPromotion";

        public PromoDetailsMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);

            this.Property(p => p.BundlePrice).IsOptional();
            this.Property(p => p.DiscountAmount).IsOptional();
            this.Property(p => p.DiscountPercent).IsOptional();
            this.Property(p => p.DisplayText).IsOptional();
            this.Property(p => p.MinimumSpend).IsOptional();
            this.Property(p => p.ReportingCode).IsOptional();
            this.Property(p => p.ValidFrom).IsOptional();
            this.Property(p => p.ValidTo).IsOptional();
            this.Property(p => p.YourReference).IsOptional();
        }

    }
}
