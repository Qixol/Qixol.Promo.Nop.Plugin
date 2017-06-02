using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Core.Domain.Promo;

using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Seo;
using Nop.Services.Media;
using Qixol.Plugin.Misc.Promo.Models.Checkout;
using global::Nop.Web.Models.Catalog;
using global::Nop.Web.Factories;
using Nop.Services.Discounts;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Catalog;
using Qixol.Plugin.Misc.Promo.Controllers;
using System.Xml.Linq;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Nop.Core.Domain.Media;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial interface IMissedPromotionsModelFactory
    {
        /// <summary>
        /// Prepare the missed promotions model
        /// </summary>
        /// <returns>Missed promotions model</returns>
        MissedPromotionsModel PrepareMissedPromotionsModel();
    }
}
