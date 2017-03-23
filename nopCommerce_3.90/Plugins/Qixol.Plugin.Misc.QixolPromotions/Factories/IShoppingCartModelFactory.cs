using Nop.Core.Domain.Orders;
using Nop.Web.Models.Media;
using Qixol.Plugin.Misc.Promo.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public interface IShoppingCartModelFactory : global::Nop.Web.Factories.IShoppingCartModelFactory
    {
        PictureModel PrepareCartItemPictureModel(ShoppingCartItem shoppingCartItem, string productName);
    }
}
