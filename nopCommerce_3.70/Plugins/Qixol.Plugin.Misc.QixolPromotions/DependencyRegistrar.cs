using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Qixol.Plugin.Misc.Promo.Controllers;
using Qixol.Nop.Promo.Services;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Discounts;
using Qixol.Nop.Promo.Services.Coupons;
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
using Qixol.Nop.Promo.Services.Common;
using Qixol.Nop.Promo.Core.Domain.Orders;

namespace Qixol.Plugin.Misc.Promo
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, global::Nop.Core.Configuration.NopConfig config)
        {
            builder.RegisterType<ShoppingCartController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            builder.RegisterType<CheckoutController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            builder.RegisterType<CatalogController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
            builder.RegisterType<PromoCheckoutController>().WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));

            builder.RegisterType<CheckoutAttributeFormatter>().As<global::Nop.Services.Orders.ICheckoutAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<promoService>().As<IPromoService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoUtilities>().As<IPromoUtilities>().InstancePerLifetimeScope();
            builder.RegisterType<CouponService>().As<ICouponService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoOrderService>().As<IPromoOrderService>().InstancePerLifetimeScope();

            builder.RegisterType<CheckoutActionFilter>().As<IFilterProvider>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartActionFilter>().As<IFilterProvider>().InstancePerLifetimeScope();

            builder.RegisterType<TaxServiceExtensions>().As<ITaxServiceExtensions>().InstancePerLifetimeScope();
            builder.RegisterType<OrderTotalCalculationService>().As<global::Nop.Services.Orders.IOrderTotalCalculationService>().InstancePerLifetimeScope();
            builder.RegisterType<OrderProcessingService>().As<global::Nop.Services.Orders.IOrderProcessingService>().InstancePerLifetimeScope();
            builder.RegisterType<PriceCalculationService>().As<global::Nop.Services.Catalog.IPriceCalculationService>().InstancePerLifetimeScope();
            builder.RegisterType<PromosPriceCalculationService>().As<IPromosPriceCalculationService>().InstancePerLifetimeScope();
            builder.RegisterType<DiscountService>().As<global::Nop.Services.Discounts.IDiscountService>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartService>().As<global::Nop.Services.Orders.IShoppingCartService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxService>().As<global::Nop.Services.Tax.ITaxService>().InstancePerLifetimeScope();
            builder.RegisterType<MessageTokenProvider>().As<global::Nop.Services.Messages.IMessageTokenProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PdfService>().As<global::Nop.Services.Common.IPdfService>().InstancePerLifetimeScope();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register<IDbContext>(c => new NopPromoContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>("nop_object_context_promo").InstancePerLifetimeScope();

                builder.Register<NopPromoContext>(c => new NopPromoContext(dataProviderSettings.DataConnectionString))
                    .InstancePerLifetimeScope();
            }
            else
            {
                //register named context
                builder.Register<IDbContext>(c => new NopPromoContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>("nop_object_context_promo").InstancePerLifetimeScope();

                builder.Register<NopPromoContext>(c => new NopPromoContext(c.Resolve<DataSettings>().DataConnectionString))
                    .InstancePerLifetimeScope();
            }

            builder.RegisterType<ExportQueueService>().As<IExportQueueService>().InstancePerLifetimeScope();
            builder.RegisterType<AttributeValueService>().As<IAttributeValueService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeConfigService>().As<IProductAttributeConfigService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductMappingService>().As<IProductMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoDetailService>().As<IPromoDetailService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductPromoMappingService>().As<IProductPromoMappingService>().InstancePerLifetimeScope();
            builder.RegisterType<PromoPictureService>().As<IPromoPictureService>().InstancePerLifetimeScope();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ExportQueueItem>>()
                .As<IRepository<ExportQueueItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

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

            builder.RegisterType<EfRepository<PromoOrder>>()
                .As<IRepository<PromoOrder>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoOrderItem>>()
                .As<IRepository<PromoOrderItem>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PromoOrderItemPromotion>>()
                .As<IRepository<PromoOrderItemPromotion>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_promo"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 99; }
        }
    }
}
