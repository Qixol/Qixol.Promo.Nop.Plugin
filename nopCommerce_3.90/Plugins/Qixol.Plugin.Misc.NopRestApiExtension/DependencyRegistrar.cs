using Autofac;
using Autofac.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Qixol.Plugin.Misc.RestApi.Service;
using System.Web.Mvc;

namespace Qixol.Plugin.Misc.Promo
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, global::Nop.Core.Configuration.NopConfig config)
        {
            //builder.RegisterType<RestServices>().As<IRestServices>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 202; }
        }
    }
}
