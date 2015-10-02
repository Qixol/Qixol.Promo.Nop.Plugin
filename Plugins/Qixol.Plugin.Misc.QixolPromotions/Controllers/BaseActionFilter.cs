using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public abstract class BaseActionFilter : ActionFilterAttribute, IFilterProvider
    {
        public abstract void ActionExecuting(ActionExecutingContext filterContext, string actionName);

        public abstract void ActionExecuted(ActionExecutedContext filterContext, string actionName);

        public abstract List<KeyValuePair<global::System.Type, string>> GetControllerActions();

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var controllerActions = this.GetControllerActions();
            if(controllerActions == null || controllerActions.Count == 0)
                return new List<Filter>();

            var controllerAction = controllerActions.Where(ca => ca.Key == controllerContext.Controller.GetType()
                                                            && ca.Value.Equals(actionDescriptor.ActionName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (!default(KeyValuePair<global::System.Type, string>).Equals(controllerAction))
                return new List<Filter>() { new Filter(this, FilterScope.Action, 0) };
            else
                return new List<Filter>();
        }

        private bool SharedValidation(string actionName, string controllerName, global::System.Type controllerType)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return false;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return false;

            var promoSettings = EngineContext.Current.Resolve<PromoSettings>();
            if (!promoSettings.Enabled)
                return false;

            var controllerActions = this.GetControllerActions();
            return controllerActions.Exists(ca => ca.Key == controllerType && ca.Value.Equals(actionName, StringComparison.InvariantCultureIgnoreCase));

        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.Controller.ToString();

            if (!SharedValidation(actionName, controllerName, filterContext.Controller.GetType()))
                return;

            ActionExecuting(filterContext, actionName);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            string controllerName = filterContext.Controller.ToString();

            if (!SharedValidation(actionName, controllerName, filterContext.Controller.GetType()))
                return;

            ActionExecuted(filterContext, actionName);            
        }

        public void BaseActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);            
        }

        public void BaseActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
        }

    }
}
