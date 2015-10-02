using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class PromoBannerModel
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoBanner.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoBanner.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoBanner.TransitionType")]
        public string TransitionType { get; set; }

        public string TransitionTypeName { get; set; }

        public IList<SelectListItem> AvailableTransitionTypes { get; set; }
    }
}
