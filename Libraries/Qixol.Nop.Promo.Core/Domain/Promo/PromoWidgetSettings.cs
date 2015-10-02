using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Promo
{
    public class PromoWidgetSettings : ISettings
    {

        public bool ShowStickersInCatalogue { get; set; }

        public bool ShowStickersInProductPage { get; set; }

        public bool ShowPromoDetailsOnProductPage { get; set; }

        public string ProductPagePromoDetailsWidgetZone { get; set; }

    }
}
