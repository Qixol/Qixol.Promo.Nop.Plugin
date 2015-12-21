using Nop.Web.Framework;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class AddPromoBannerWidgetZoneModel
    {
        public int BannerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerWidgets.Zone")]
        public string SelectedWidgetZone { get; set; }

        public string FirstWidgetZone { get; set; }

        public IList<SelectListItem> AvailableWidgetZones { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerWidgets.CustomWidgetZone")]
        public string CustomWidgetZone { get; set; }
    }
}
