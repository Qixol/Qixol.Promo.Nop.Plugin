using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoPictureMap : EntityTypeConfiguration<PromoPicture>
    {
        public readonly static string TABLENAME = "PromoPicture";

        public PromoPictureMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);

            this.Property(p => p.PromoReference).IsOptional();
            this.Property(p => p.PromoTypeName).IsOptional();
        }
    }
}
