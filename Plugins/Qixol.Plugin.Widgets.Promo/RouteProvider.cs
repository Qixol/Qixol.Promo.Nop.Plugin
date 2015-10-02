using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;
using Qixol.Plugin.Widgets.Promo.ViewEngines;

namespace Qixol.Plugin.Widgets.Promo
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            #region Custom View Engine

            global::System.Web.Mvc.ViewEngines.Engines.Insert(0, new PromoViewEngine());

            #endregion

            // Main Banner List
            var promoBanner = routes.MapRoute("Qixol.Promos.PromoBanner",
                    "Admin/PromoBanner/Admin/Banner/Index",
                    new { controller = "PromoBanner", action = "Index" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBanner.DataTokens.Add("area", "admin");

            // For a banner, PICTURES - retrieve / add /  edit / delete
            var promoBannerList = routes.MapRoute("Qixol.Promos.PromoBanner.List",
                    "Admin/PromoBanner/Admin/Banner/List",
                    new { controller = "PromoBanner", action = "BannerPicturesList" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerList.DataTokens.Add("area", "admin");

            var promoBannerAdd = routes.MapRoute("Qixol.Promos.PromoBanner.Add",
                    "Admin/PromoBanner/Admin/Banner/Add",
                    new { controller = "PromoBanner", action = "BannerPictureAdd" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerAdd.DataTokens.Add("area", "admin");

            var promoBannerEdit = routes.MapRoute("Qixol.Promos.PromoBanner.Edit",
                    "Admin/PromoBanner/Admin/Banner/Edit",
                    new { controller = "PromoBanner", action = "BannerPictureEdit" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerEdit.DataTokens.Add("area", "admin");

            var promoBannerDelete = routes.MapRoute("Qixol.Promos.PromoBanner.Delete",
                    "Admin/PromoBanner/Admin/Banner/Delete",
                    new { controller = "PromoBanner", action = "BannerPictureDelete" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerDelete.DataTokens.Add("area", "admin");

            // For a Banner, WIDGET ZONES - retrieve / add /  edit / delete
            var promoBannerWidgetZoneList = routes.MapRoute("Qixol.Promos.PromoBanner.WidgetZoneList",
                    "Admin/PromoBanner/Admin/Banner/WidgetZoneList",
                    new { controller = "PromoBanner", action = "BannerWidgetZonesList" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerWidgetZoneList.DataTokens.Add("area", "admin");

            var promoBannerWidgetZoneAdd = routes.MapRoute("Qixol.Promos.PromoBanner.WidgetZoneAdd",
                    "Admin/PromoBanner/Admin/Banner/AddWidgetZone",
                    new { controller = "PromoBanner", action = "BannerWidgetZoneAdd" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerWidgetZoneAdd.DataTokens.Add("area", "admin");

            var promoBannerWidgetZoneDelete = routes.MapRoute("Qixol.Promos.PromoBanner.WidgetZoneDelete",
                    "Admin/PromoBanner/Admin/Banner/DeleteWidgetZone",
                    new { controller = "PromoBanner", action = "BannerWidgetZoneDelete" },
                    new[] { "Qixol.Plugin.Widgets.Promo.Controllers" }
            );
            promoBannerWidgetZoneDelete.DataTokens.Add("area", "admin");

            var accountIssuedCoupons = routes.MapRoute("CustomerIssuedCoupons",
                    "customer/issuedcoupons",
                    new { controller = "PromoCustomerCoupon", action = "CustomerIssuedCoupons" },
                    new[] { "Qixol.Plugin.Misc.Wdigets.Controllers" }
            );
            routes.Remove(accountIssuedCoupons);
            routes.Insert(0, accountIssuedCoupons);
 
        }

        private void InsertNewMapRoute(RouteCollection routes, string name, string url, string controllerName, string actionName, object routeParams = null, bool isAdmin = true, bool expectsId = false, bool insertAtZeroRoute = true)
        {
            object controllerParams = null;
            if (!expectsId)
                controllerParams = new { controller = controllerName, action = actionName };
            else
            {
                controllerParams = new { controller = controllerName, action = actionName, Id = UrlParameter.Optional };
                if (routeParams == null)
                    routeParams = new { id = @"\d+" };
            }

            var newMapRoute = routes.MapRoute(name, url, controllerParams, routeParams, new[] { "Qixol.Nop.Controllers" } );

            if(isAdmin)
                newMapRoute.DataTokens.Add("area", "admin");

            if (insertAtZeroRoute)
            {
                routes.Remove(newMapRoute);
                routes.Insert(0, newMapRoute);
            }
        }

        public int Priority
        {
            get { return 999; }
        }
    }
}
