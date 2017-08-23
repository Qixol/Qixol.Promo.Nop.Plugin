using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Qixol.Plugin.Misc.Promo
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            #region website routes

            #endregion
        }

        public int Priority
        {
            get { return 999; }
        }
    }
}
