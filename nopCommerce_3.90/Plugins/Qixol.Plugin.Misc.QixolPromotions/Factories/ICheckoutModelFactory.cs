using Qixol.Plugin.Misc.Promo.Models.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public interface ICheckoutModelFactory : global::Nop.Web.Factories.ICheckoutModelFactory
    {
        MissedPromotionsModel PrepareMissedPromotionsModel();
    }
}
