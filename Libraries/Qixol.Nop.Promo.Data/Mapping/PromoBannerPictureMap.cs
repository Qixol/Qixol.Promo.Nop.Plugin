using Qixol.Nop.Promo.Core.Domain.Banner;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoBannerPictureMap : EntityTypeConfiguration<PromoBannerPicture>
    {
        public readonly static string TABLENAME = "PromoBannerPicture";

        public PromoBannerPictureMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }
    }
}
