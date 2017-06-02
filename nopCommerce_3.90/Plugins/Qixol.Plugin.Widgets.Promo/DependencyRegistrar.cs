using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Qixol.Nop.Promo.Services;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Discounts;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Nop.Promo.Services.Catalog;
using Qixol.Nop.Promo.Services.ExportQueue;
using Qixol.Nop.Promo.Data;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Services.Tax;
using Qixol.Nop.Promo.Services.Messages;
using Nop.Core.Caching;
using Qixol.Nop.Promo.Data.Mapping;
using Qixol.Nop.Promo.Core.Domain.Banner;
using Qixol.Nop.Promo.Services.Banner;

namespace Qixol.Plugin.Widgets.Promo
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, global::Nop.Core.Configuration.NopConfig config)
        {
            builder.RegisterType<PromoUtilities>().As<IPromoUtilities>().InstancePerLifetimeScope();
            builder.RegisterType<AttributeValueService>().As<IAttributeValueService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductMappingService>().As<IProductMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoDetailService>().As<IPromoDetailService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductPromoMappingService>().As<IProductPromoMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoPictureService>().As<IPromoPictureService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoBannerService>().As<IPromoBannerService>().InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ProductMappingItem>>()
                .As<IRepository<ProductMappingItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<AttributeValueMappingItem>>()
                .As<IRepository<AttributeValueMappingItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ProductAttributeConfigItem>>()
                .As<IRepository<ProductAttributeConfigItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoDetail>>()
                .As<IRepository<PromoDetail>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ProductPromotionMapping>>()
                .As<IRepository<ProductPromotionMapping>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoPicture>>()
                .As<IRepository<PromoPicture>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoBanner>>()
                .As<IRepository<PromoBanner>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoBannerPicture>>()
                .As<IRepository<PromoBannerPicture>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoBannerWidgetZone>>()
                .As<IRepository<PromoBannerWidgetZone>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 99; }
        }
    }
}
