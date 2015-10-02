using Qixol.Nop.Promo.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoOrderItemMap : EntityTypeConfiguration<PromoOrderItem>
    {
        public readonly static string TABLENAME = "PromoOrderItem";

        public PromoOrderItemMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);

            this.HasRequired(promoOrderItem => promoOrderItem.PromoOrder)
                .WithMany(o => o.PromoOrderItems)
                .HasForeignKey(promoOrderItem => promoOrderItem.PromoOrderId);
        }
    }
}
