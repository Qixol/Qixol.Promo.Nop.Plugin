using Nop.Core.Domain.Catalog;
using Nop.Web.Models.Catalog;
using Qixol.Plugin.Misc.Promo.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public interface ICatalogModelFactory : global::Nop.Web.Factories.ICatalogModelFactory
    {
        IList<CategorySimpleModel> PromoPrepareCategorySimpleModels(int rootCategoryId, Category category);
    }
}
