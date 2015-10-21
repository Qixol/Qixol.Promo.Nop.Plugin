using Nop.Web.Framework.Themes;

namespace Qixol.Plugin.Misc.Promo.ViewEngines
{
    public class PromoViewEngine : ThemeableRazorViewEngine
    {
        public PromoViewEngine()
        {
            PartialViewLocationFormats =
                new[]
                {
                    // TODO: Add themes folders (check core solution - uses {1} & {2} parameters?
                    // also should mean we can remove theme lookup from cshtml
                    "~/Plugins/Misc.QixolPromo/Views/Order/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/ShoppingCart/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Customer/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Admin/{0}.cshtml"
                };

            ViewLocationFormats =
                new[]
                {
                    "~/Plugins/Misc.QixolPromo/Views/Order/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/ShoppingCart/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Customer/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Admin/{0}.cshtml"
                };

            /*
            AreaPartialViewLocationFormats =
                new[]
                {
                    "~/Plugins/Misc.QixolPromo/Views/Admin/Discount/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Admin/{0}.cshtml"
                };

            AreaViewLocationFormats =
                new[]
                {
                    "~/Plugins/Misc.QixolPromo/Views/Admin/Discount/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Admin/{0}.cshtml"
                };
            */
        }
    }
}
