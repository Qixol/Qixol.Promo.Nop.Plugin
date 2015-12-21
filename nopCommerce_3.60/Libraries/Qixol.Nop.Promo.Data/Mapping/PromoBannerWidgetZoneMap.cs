using Qixol.Nop.Promo.Core.Domain.Banner;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoBannerWidgetZoneMap : EntityTypeConfiguration<PromoBannerWidgetZone>
    {
        public readonly static string TABLENAME = "PromoBannerWidgetZone";

        public PromoBannerWidgetZoneMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }
    }
}
