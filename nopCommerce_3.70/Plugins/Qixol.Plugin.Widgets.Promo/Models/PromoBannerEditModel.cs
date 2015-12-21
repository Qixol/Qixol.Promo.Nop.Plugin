using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class PromoBannerEditModel
    {
        public string BannerName { get; set; }

        public int BannerId { get; set; }

        public string BannerTransitionType { get; set; }

        public AddPromoBannerPictureModel AddPromoBannerPictureModel { get; set; }

        public AddPromoBannerWidgetZoneModel AddPromoBannerWidgetZoneModel { get; set; }

        public PromoBannerEditModel()
        {
            AddPromoBannerPictureModel = new AddPromoBannerPictureModel();
            AddPromoBannerWidgetZoneModel = new AddPromoBannerWidgetZoneModel();
        }

    }
}
