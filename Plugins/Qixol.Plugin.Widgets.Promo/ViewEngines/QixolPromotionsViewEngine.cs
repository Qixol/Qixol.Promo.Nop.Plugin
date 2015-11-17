using Nop.Web.Framework.Themes;

namespace Qixol.Plugin.Widgets.Promo.ViewEngines
{
    public class PromoViewEngine : ThemeableRazorViewEngine
    {
        public PromoViewEngine()
        {
            PartialViewLocationFormats =
                new[]
                {
                    // themes
                    "~/Plugins/Widgets.QixolPromo/Themes/{2}/Views/Customer/{0}.cshtml",
                    "~/Plugins/Widgets.QixolPromo/Themes/{2}/Views/Shared/{0}.cshtml",
                    "~/Plugins/Widgets.QixolPromo/Themes/{2}/Views/Admin/{0}.cshtml",

                    //default
                    "~/Plugins/Widgets.QixolPromo/Views/Customer/{0}.cshtml",
                    "~/Plugins/Widgets.QixolPromo/Views/Shared/{0}.cshtml",
                    "~/Plugins/Widgets.QixolPromo/Views/Admin/{0}.cshtml",
                };

            ViewLocationFormats =
                new[]
                {
                    // themes
                    "~/Plugins/Widgets.QixolPromo/Themes/{2}/Views/Customer/{0}.cshtml",

                    // default
                    "~/Plugins/Widgets.QixolPromo/Views/Customer/{0}.cshtml",
                };

            AreaPartialViewLocationFormats =
                new[]
                {
                    "~/Plugins/Widgets.QixolPromo/Views/Admin/{0}.cshtml",
                    "~/Plugins/Widgets.QixolPromo/Views/Shared/{0}.cshtml",
                };

            AreaViewLocationFormats =
                new[]
                {
                    "~/Plugins/Widgets.QixolPromo/Views/Admin/{0}.cshtml",
                    "~/Plugins/Widgets.QixolPromo/Views/Shared/{0}.cshtml",
                };
        }
    }
}
