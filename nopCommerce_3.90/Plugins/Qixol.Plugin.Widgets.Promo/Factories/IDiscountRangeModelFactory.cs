using Qixol.Plugin.Widgets.Promo.Models;
using Qixol.Promo.Integration.Lib.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public interface IDiscountRangeModelFactory
    {
        DiscountRangeModel PrepareDiscountRangeModel(ExportPromotionDetailsDiscountRange entity);
    }
}
