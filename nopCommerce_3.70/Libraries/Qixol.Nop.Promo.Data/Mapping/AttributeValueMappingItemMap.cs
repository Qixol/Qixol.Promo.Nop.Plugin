using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class AttributeValueMappingItemMap : EntityTypeConfiguration<AttributeValueMappingItem>
    {
        public readonly static string TABLENAME = "PromoAttributeValueMapping";

        public AttributeValueMappingItemMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);
        }   
    }
}
