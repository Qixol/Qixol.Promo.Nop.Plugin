using global::Nop.Core;
using global::Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Products
{
    public class ProductPromotionMapping : BaseEntity 
    {
        public int PromotionId { get; set; }

        public int ProductMappingId { get; set; }

        public int RequiredQty { get; set; }

        public decimal RequiredSpend { get; set; }

        public bool MultipleProductRestrictions { get; set; }

        public string MatchingRestrictions { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
