using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class PromoBannerDisplayModel
    {
        public int BannerId { get; set; }

        public string BannerName { get; set; }

        public List<PromoBannerDisplayPictureModel> Pictures { get; set; }

        public string TransitionType { get; set; }

        public PromoBannerDisplayModel()
        {
            Pictures = new List<PromoBannerDisplayPictureModel>();
        }
    }
}
