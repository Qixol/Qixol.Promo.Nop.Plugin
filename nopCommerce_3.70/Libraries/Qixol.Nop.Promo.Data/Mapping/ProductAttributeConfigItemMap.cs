using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class ProductAttributeConfigItemMap : EntityTypeConfiguration<ProductAttributeConfigItem>
    {
        public readonly static string TABLENAME = "PromoProductAttributeConfig";

        public ProductAttributeConfigItemMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }
    }
}
