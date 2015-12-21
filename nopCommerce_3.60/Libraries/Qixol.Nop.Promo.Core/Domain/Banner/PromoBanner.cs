using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Banner
{
    public class PromoBanner : BaseEntity 
    {
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public string TransitionType { get; set; }
    }
}
