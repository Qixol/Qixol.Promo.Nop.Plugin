using Qixol.Nop.Promo.Core.Domain.Banner;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoBannerMap : EntityTypeConfiguration<PromoBanner>
    {
        public readonly static string TABLENAME = "PromoBanner";

        public PromoBannerMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }
    }
}
