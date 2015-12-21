using Qixol.Plugin.Misc.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Admin.Extensions;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Qixol.Plugin.Misc.Promo.Extensions.MappingExtensions;

namespace Qixol.Plugin.Misc.Promo.Extensions.MappingExtensions
{
    public static class ProductAttributeConfigItemExtensions
    {
        public static ProductAttributeConfigItemModel ToModel(this ProductAttributeConfigItem item)
        {
            return new ProductAttributeConfigItemModel()
            {
                Id = item.Id,
                Enabled = item.Enabled,
                NameResource = item.NameResource,
                SystemName = item.SystemName
            };
        }
    }
}
