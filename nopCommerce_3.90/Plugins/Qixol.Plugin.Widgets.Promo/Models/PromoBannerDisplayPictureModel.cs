using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class PromoBannerDisplayPictureModel
    {
        public int PictureId { get; set; }

        public string PictureUrl { get; set; }

        public string Comment { get; set; }

        public string Url { get; set; }

        public int DisplaySequence { get; set; }

        public string TransitionType { get; set; }

        public string Width { get; set; }
    }
}
