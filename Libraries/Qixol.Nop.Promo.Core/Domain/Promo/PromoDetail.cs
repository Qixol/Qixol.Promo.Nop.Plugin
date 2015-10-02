using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Promo
{
    public class PromoDetail : BaseEntity 
    {
        public int PromoId { get; set; }

        public string PromoTypeName { get; set; }

        public string PromoName { get; set; }

        public string YourReference { get; set; }

        public string ReportingCode { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal? BundlePrice { get; set; }

        public decimal? MinimumSpend { get; set; }

        public bool BasketRestrictions { get; set; }

        public bool CouponRestrictions { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public string DisplayText { get; set; }

        public bool AppliesToItems { get; set; }

        public bool AppliesToBasket { get; set; }

        public bool AppliesToDelivery { get; set; }

        public string PromoXml { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}
