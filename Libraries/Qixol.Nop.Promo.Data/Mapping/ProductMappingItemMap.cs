using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class ProductMappingItemMap : EntityTypeConfiguration<ProductMappingItem>
    {
        public readonly static string TABLENAME = "PromoProductMapping";

        public ProductMappingItemMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }   
    }
}
