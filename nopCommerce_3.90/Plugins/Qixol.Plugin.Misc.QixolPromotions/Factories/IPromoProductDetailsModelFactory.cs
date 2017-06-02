using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Qixol.Plugin.Misc.Promo.Models;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial interface IPromoProductDetailsModelFactory
    {
        /// <summary>
        /// Prepare the promo product details model
        /// </summary>
        /// <returns>Promo product overview model</returns>
        PromoProductOverviewModel PromoPrepareProductOverviewModel(Product product, ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false);
    }
}
