using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class PromoPictureModel
    {
        public int Id { get; set; }

        public int PictureId { get; set; }

        public string PictureUrl { get; set; }

        public bool IsDefaultForType { get; set; }

        public string DefaultForTypeName { get; set; }

        public string DefaultForTypeDisplay { get; set; }

        public string PromoReference { get; set; }

        public bool SystemDefault { get; set; }
    }
}
