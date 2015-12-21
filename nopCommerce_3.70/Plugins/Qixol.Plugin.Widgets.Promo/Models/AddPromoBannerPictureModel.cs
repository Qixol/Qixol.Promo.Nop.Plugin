using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class AddPromoBannerPictureModel
    {
        [UIHint("Picture")]
        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerPicture.Picture")]
        public int PictureId { get; set; }

        public int BannerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerPicture.PromoRef")]
        public string PromoReference { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerPicture.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerPicture.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.BannerPicture.DisplaySequence")]
        public int DisplaySequence { get; set; }

    }
}
