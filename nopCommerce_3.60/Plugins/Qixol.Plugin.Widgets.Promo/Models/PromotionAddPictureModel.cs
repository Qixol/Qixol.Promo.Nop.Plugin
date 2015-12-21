using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class PromoAddPictureModel
    {
        public int Id { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoPicture.Picture")]
        public int PictureId { get; set; }

        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForType")]
        public bool IsDefaultForType { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForTypeName")]
        public string DefaultForTypeName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.PromoPicture.PromoReference")]
        public string PromoReference { get; set; }

        public bool SystemDefault { get; set; }

        public IList<SelectListItem> PromoTypes { get; set; }

        public PromoAddPictureModel()
        {
            PromoTypes = new List<SelectListItem>();
        }
    }
}
