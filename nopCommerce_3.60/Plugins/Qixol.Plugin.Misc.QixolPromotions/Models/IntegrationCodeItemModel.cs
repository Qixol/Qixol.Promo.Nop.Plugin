using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models
{
    public class IntegrationCodeItemModel
    {
        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public string EntityAttributeSystemName { get; set; }

        public int AttributeId { get; set; }

        public string IntegrationCode { get; set; }

        public int? Priority { get; set; }

    }
}
