using Qixol.Nop.Promo.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class PromoOrderItemPromotionMap : EntityTypeConfiguration<PromoOrderItemPromotion>
    {
        public readonly static string TABLENAME = "PromoOrderItemPromotion";

        public PromoOrderItemPromotionMap()
        {
            this.ToTable(TABLENAME);
            this.HasKey(x => x.Id);

            this.HasRequired(promoOrderItemPromotion => promoOrderItemPromotion.PromoOrderItem)
                .WithMany(o => o.PromoOrderItemPromotions)
                .HasForeignKey(promoOrderItemPromotion => promoOrderItemPromotion.PromoOrderItemId);
        }
    }
}
