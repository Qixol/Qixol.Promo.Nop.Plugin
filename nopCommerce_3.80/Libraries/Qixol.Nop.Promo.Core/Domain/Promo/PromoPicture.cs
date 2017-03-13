using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Promo
{
    public class PromoPicture : BaseEntity 
    {
        public int PictureId { get; set; }

        public string PromoReference { get; set; }

        public string PromoTypeName { get; set; }

        public bool IsDefaultForType { get; set; }
    }
}
