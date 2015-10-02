using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Banner
{
    public class PromoBannerWidgetZone : BaseEntity
    {
        public int PromoBannerId { get; set; }

        public string WidgetZoneSystemName { get; set; }

    }
}
