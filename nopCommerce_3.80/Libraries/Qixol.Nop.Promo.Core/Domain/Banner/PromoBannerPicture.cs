using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Banner
{
    public class PromoBannerPicture : BaseEntity 
    {
        public int PromoBannerId { get; set; }

        public int PictureId { get; set; }

        public string PromoReference { get; set; }

        public int DisplaySequence { get; set; }

        public string Comment { get; set; }

        public string Url { get; set; }

    }
}
