using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;
using Nop.Services.Logging;
using Qixol.Nop.Promo.Services;
using Nop.Core.Domain.Tasks;
using Nop.Services.Tasks;
using Qixol.Nop.Promo.Core.Domain.Tasks;
using Qixol.Nop.Promo.Data;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Nop.Services.Cms;
using Nop.Core.Domain.Cms;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Data.Mapping;
using Nop.Services.Common;
using Nop.Web.Framework.Menu;

namespace Qixol.Plugin.Misc.RestApi
{
    public class RestApiPlugin : BasePlugin
    {
        #region Fields

        private readonly PromoSettings _promoSettings;
        private readonly IPluginFinder _pluginFinder;

        #endregion

        #region constructor

        public RestApiPlugin(
            PromoSettings promosFeedSettings,
            IPluginFinder pluginFinder)
        {
            this._promoSettings = promosFeedSettings;
            this._pluginFinder = pluginFinder;
        }

        #endregion

        #region methods

        //public void GetConfigurationRoute(out string actionName, out string controllerName, out global::System.Web.Routing.RouteValueDictionary routeValues)
        //{
        //    actionName = "Configure";
        //    controllerName = "PromoAdmin";
        //    routeValues = new RouteValueDictionary() { { "Namespaces", "Qixol.Plugin.Misc.Promo.Controllers" }, { "area", null } };
        //}

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            // confirm that Promo is installed & Enabled
            // confirm that Xcellent plugin is installed and Enabled

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }

        #endregion

    }
}
