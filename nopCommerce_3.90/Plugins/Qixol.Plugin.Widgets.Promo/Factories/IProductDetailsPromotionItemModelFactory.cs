using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public interface IProductDetailsPromotionItemModelFactory
    {
        ProductDetailsPromotionItemModel PrepareProductDetailsPromotionItemModel(ValidatedPromo promo, bool productHasTierPrices);
    }
}
