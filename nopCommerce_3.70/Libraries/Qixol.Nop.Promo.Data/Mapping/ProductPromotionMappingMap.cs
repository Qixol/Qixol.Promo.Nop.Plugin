using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class ProductPromoMappingMap : EntityTypeConfiguration<ProductPromotionMapping>
    {
        public readonly static string TABLENAME = "PromoPromotionProductMapping";

        public ProductPromoMappingMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);

            this.Property(p => p.MatchingRestrictions).IsOptional();
            this.Property(p => p.MultipleProductRestrictions).IsOptional();
            this.Property(p => p.RequiredQty).IsOptional();
            this.Property(p => p.RequiredSpend).IsOptional();
        }

    }
}
