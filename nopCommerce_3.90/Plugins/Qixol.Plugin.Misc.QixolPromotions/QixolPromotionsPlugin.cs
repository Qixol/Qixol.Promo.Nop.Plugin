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

namespace Qixol.Plugin.Misc.Promo
{
    public class PromoPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly PromoSettings _promoSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ILogger _logger;
        private readonly IPromoService _promosFeedService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly NopPromoContext _nopPromoContext;
        private readonly IProductAttributeConfigService _productAttributeConfigService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;

        private List<KeyValuePair<string, string>> _stringsList = new List<KeyValuePair<string, string>>();

        #endregion

        #region constructor

        public PromoPlugin(
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            ICurrencyService currencyService,
            ISettingService settingService,
            IWorkContext workContext,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            PromoSettings promosFeedSettings,
            CurrencySettings currencySettings,
            IProductAttributeService productAttributeService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ILogger logger,
            IPromoService promosFeedService,
            IScheduleTaskService scheduleTaskService,
            NopPromoContext nopPromoContext,
            IProductAttributeConfigService productAttributeConfigService,
            WidgetSettings widgetSettings,
            IPluginFinder pluginFinder,
            ILocalizationService localizationService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._currencyService = currencyService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._promoSettings = promosFeedSettings;
            this._currencySettings = currencySettings;
            this._productAttributeService = productAttributeService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._logger = logger;
            this._promosFeedService = promosFeedService;
            this._scheduleTaskService = scheduleTaskService;
            this._nopPromoContext = nopPromoContext;
            this._productAttributeConfigService = productAttributeConfigService;
            this._widgetSettings = widgetSettings;
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
        }

        #endregion

        #region methods

        public void GetConfigurationRoute(out string actionName, out string controllerName, out global::System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PromoAdmin";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Qixol.Plugin.Misc.Promo.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new PromoSettings()
            {
                ProductPictureSize = 125,
                BatchSize = 500,
                SynchronizeCustomerRoles = true,
                SynchronizeProducts = true,
                SynchronizeShippingMethods = true,
                SynchronizeStores = true,
                SynchronizeCheckoutAttributes = true,
                SynchronizeCurrencies = true,

                VariantAttributeFormat = "{0}: {1}",
                VariantAttributesSeparator = ", ",
                MaximumAttributesForVariants = 3,
                ShowPromotionDetailsInBasket = 0,
                ShowHelperMessages = true,
                InitialSetup = true,
                UseSelectedCurrencyWhenSubmittingBaskets = false,
                ShowMissedPromotions = true,
                HideNopDiscountMenuItems = true,

#if DEBUG
                BasketRequestEndpointAddress = @"http://localhost:8733/Design_Time_Addresses/Qixol.Promo.Services/BasketService",
                PromoImportEndpointAddress = @"http://localhost:8733/Design_Time_Addresses/Qixol.Promo.Services/ImportService",
                PromoExportEndpointAddress = @"http://localhost:8733/Design_Time_Addresses/Qixol.Promo.Services/ExportService",
                ServiceEndpointSelection = SettingsEndpointAddress.CUSTOM_SERVICES,
                LogMessages = true,
                QueueHoldPeriodInSeconds = 2,
                ShowAdvancedIntegrationSettings = true,
#else
                QueueHoldPeriodInSeconds = 300,
#endif
                Channel = "WEB",
                StoreGroup = "WEB"
            };

            _settingService.SaveSetting(settings);


            // Setup any database changes
            this._nopPromoContext.Install();

            // Create product attribute config items.
            InsertProductAttributeConfigItems();

            // Deal with strings
            SetupStringResources();
            InstallAllStringResources();

            // set the widget to active (for the Customer Navigation menu only at present...)
            _widgetSettings.ActiveWidgetSystemNames.Add(this.PluginDescriptor.SystemName);
            _settingService.SaveSetting(_widgetSettings);

            // schedule task
            ScheduleTask feedTask = FindScheduledTask(PromoTaskNames.DataFeedTask);
            if (feedTask == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask()
                {
                    Name = "Qixol Promo - Product and Attribute Sync (Push)",
                    Seconds = 300, // 5 mins
                    Type = PromoTaskNames.DataFeedTask,
                    Enabled = false,
                    StopOnError = true
                });
            }

            ScheduleTask promoUseTask = FindScheduledTask(PromoTaskNames.PromoSyncTask);
            if (promoUseTask == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask()
                {
                    Name = "Qixol Promo - Promotion Sync (Pull)",
                    Seconds = 86400, // 24 hrs
                    Type = PromoTaskNames.PromoSyncTask,
                    Enabled = false,
                    StopOnError = true
                });
            }

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            // schedule task
            ScheduleTask t = _scheduleTaskService.GetTaskByType(PromoTaskNames.DataFeedTask);
            if (t != null)
                _scheduleTaskService.DeleteTask(t);

            t = _scheduleTaskService.GetTaskByType(PromoTaskNames.PromoSyncTask);
            if (t != null)
                _scheduleTaskService.DeleteTask(t);

            //settings
            _settingService.DeleteSetting<PromoSettings>();

            // remove any database changes
            this._nopPromoContext.Uninstall();


            //locales
            SetupStringResources();
            UninstallAllStringResources();

            base.Uninstall();
        }

        private ScheduleTask FindScheduledTask(string taskTypeName)
        {
            return _scheduleTaskService.GetTaskByType(taskTypeName);
        }

        #endregion

        #region Product Attributes

        private void InsertProductAttributeConfigItems()
        {
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.VENDOR, NameResource = "Admin.Catalog.Products.Fields.Vendor", Enabled = true });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.SKU, NameResource = "Admin.Catalog.Products.Fields.Sku", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.CATEGORY, NameResource = "Admin.Catalog.Products.List.SearchCategory", Enabled = true });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.MANUFACTURER, NameResource = "products.manufacturer", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.CATEGORY_BREADCRUMBS, NameResource = "Plugins.Misc.QixolPromo.ProductAttributes.CategoryBreadcrumb", Enabled = true });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.MANUFACTURER_PART_NO, NameResource = "Admin.Catalog.Products.Fields.ManufacturerPartNumber", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.GTIN, NameResource = "Admin.Catalog.Products.Fields.GTIN", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.DISABLE_BUY_BUTTON, NameResource = "Admin.Catalog.Products.Fields.DisableBuyButton", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.AVAILABLE_FOR_PREORDER, NameResource = "Admin.Catalog.Products.Fields.AvailableForPreOrder", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.CALL_FOR_PRICE, NameResource = "Admin.Catalog.Products.Fields.CallForPrice", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.CUSTOMER_ENTERS_PRICE, NameResource = "Admin.Catalog.Products.Fields.CustomerEntersPrice", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.IS_GIFT_CARD, NameResource = "Admin.Catalog.Products.Fields.IsGiftCard", Enabled = true });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.GIFT_CARD_TYPE, NameResource = "Admin.Catalog.Products.Fields.GiftCardType", Enabled = true });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.DOWNLOADABLE_PRODUCT, NameResource = "Admin.Catalog.Products.Fields.IsDownload", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.IS_RENTAL, NameResource = "Admin.Catalog.Products.Fields.IsRental", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.SHIPPING_ENABLED, NameResource = "Admin.Catalog.Products.Fields.IsShipEnabled", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.FREE_SHIPPING, NameResource = "Admin.Catalog.Products.Fields.IsFreeShipping", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.SHIP_SEPARATELY, NameResource = "Admin.Catalog.Products.Fields.ShipSeparately", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.TAX_EXCEMPT, NameResource = "Admin.Catalog.Products.Fields.IsTaxExempt", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.TAX_CATEGORY, NameResource = "Admin.Catalog.Products.Fields.TaxCategory", Enabled = false });
            _productAttributeConfigService.Insert(new ProductAttributeConfigItem() { SystemName = ProductAttributeConfigSystemNames.PRODUCT_SPECIFICATION_ATTRIBS, NameResource = "Plugins.Misc.QixolPromo.ProductAttributes.SpecificationAttributes", Enabled = true });
        }

        #endregion

        #region Locale Resources

        private void SetupStringResources()
        {
            //locales
            this.InsertStringResource("Plugins.Misc.QixolPromo.Integration", "Integration");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SynchronizedItems", "Synchronized Items");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyDetails", "Company and Store Details");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SynchronizeNow", "Synchronize all when saving");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SynchronizeNow.Hint", "If checked, the synchronization scheduled task will be executed when saving the configuration. This is mainly for use when the initial configuration is complete.");

            this.InsertStringResource("Plugins.Misc.QixolPromo.Enabled", "Enabled");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Enabled.Hint", "Enable the plugin for promos");
            this.InsertStringResource("Plugins.Misc.QixolPromo.LogMessages", "Log messages");
            this.InsertStringResource("Plugins.Misc.QixolPromo.LogMessages.Hint", "Log all request and response messages from interactions with the Qixol Promos Engine");

            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyKey", "Company key");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyKey.Hint", "The company key supplied by the Qixol Promos Engine");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyKey.Helper1", "Not sure what this is?   A company key is assigned to you when you register to evaluate Qixol Promo.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyKey.Helper2", "To register for free and get started with Qixol Promo, click ");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyKey.Helper3", "Once you're registered, you can find your company key in the Manage Company section under Configuration.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CompanyKey.RegisterLink", "here.");

            this.InsertStringResource("Plugins.Misc.QixolPromo.ImportServiceEndpointAddress", "Import Manager Service Address");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ImportServiceEndpointAddress.Hint", "The address of the Qixol Promo import manager service endpoint supplied with Qixol Promo");
            this.InsertStringResource("Plugins.Misc.QixolPromo.BasketRequestServiceEndpointAddress", "Basket Manager Service Address");
            this.InsertStringResource("Plugins.Misc.QixolPromo.BasketRequestServiceEndpointAddress.Hint", "The address of the basket manager service endpoint supplied with Qixol Promo Engine");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ExportServiceEndpointAddress", "Export Manager Service Address");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ExportServiceEndpointAddress.Hint", "The address of the Qixol Promo export manager service endpoint supplied with Qixol Promo");

            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductPictureSize", "Product picture size");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductPictureSize.Hint", "The size of image used for display within the Qixol Promos Engine admin pages");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Store", "Store");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Store.Hint", "The nop commerce store for which the product feed will be generated");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Currency", "Currency");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Currency.Hint", "The currency used for prices in the product feed");
            this.InsertStringResource("Plugins.Misc.QixolPromo.BatchSize", "Batch size");
            this.InsertStringResource("Plugins.Misc.QixolPromo.BatchSize.Hint", "The number of products per batch to use when generating the full feed");

            this.InsertStringResource("Plugins.Misc.QixolPromo.StoreGroup", "Store group");
            this.InsertStringResource("Plugins.Misc.QixolPromo.StoreGroup.Hint", "The Qixol Promos store group reference used by the engine to identify where requests originate");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Channel", "Channel");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Channel.Hint", "The Qixol Promos channel reference used by the engine to identify where requests originate");

            this.InsertStringResource("ShoppingCart.Totals.ShippingDiscount", "Shipping discount");

            this.InsertStringResource("Plugins.Misc.QixolPromo.Shared.IntegrationCode", "Integration Code");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Shared.IntegrationCode.Hint", "The integration code is passed with baskets to Qixol Promo when validating baskets.  Once the code has been synchronized to Promo, it cannot be changed.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Shared.IntegrationCode.SynchronizedMessage", "The code has been synchronized with Qixol Promo and cannot be changed.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Shared.IntegrationCode.NotSynchronizedMessage", "Once this item is synchronized with Qixol Promo, the code cannot be amended.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Shared.Priority", "Priority");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Shared.Priority.Hint", "The priority determines which role will be sent to the promos engine with the basket. Priority 0 is the lowest priority.");

            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncProducts", "Synchronize Products");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncProducts.Hint", "Indicates whether the products will be synchronized between NOP and Qixol Promo.  If this option is not selected, products should be synchronized with Qixol Promo from a different system within your infrastructure.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncShippingMethods", "Synchronize Shipping Methods");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncShippingMethods.Hint", "Indicates whether the shipping methods will be synchronized between NOP and Qixol Promo.  Only shipping methods with an Integration Code defined will be synchronized.  If this option is not selected, shipping methods should be synchronized with Qixol Promo from a different system within your infrastructure.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncCustomerRoles", "Synchronize Customer Roles");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncCustomerRoles.Hint", "Indicates whether the customer roles will be synchronized between NOP and Qixol Promo.  Only customer roles with an Integration Code defined will be synchronized.  If this option is not selected, customer roles should be synchronized with Qixol Promo from a different system within your infrastructure.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncStores", "Synchronize Stores");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncStores.Hint", "Indicates whether the stores will be synchronized between NOP and Qixol Promo.  Only stores with an Integration Code defined will be synchronized.  If this option is not selected, stores should be synchronized with Qixol Promo from a different system within your infrastructure.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncCurrencies", "Synchronize Currencies");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncCurrencies.Hint", "Indicates whether currencies will be synchronized between NOP and Qixol Promo.  If this option is not selected, currencies should be synchronized with Qixol Promo from a different system within your infrastructure.");

            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductAttributes", "Product Attributes");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductAttributes.Name", "Name");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductAttributes.IsEnabled", "Include with synchronized products?");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductAttributes.Category", "Category");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductAttributes.CategoryBreadcrumb", "Category Breadcrumb");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductAttributes.SpecificationAttributes", "Specification Attributes");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ServiceEndpoint", "Integration Services");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Hint", "Indicates whether to submit basket and import requests to the Promo Evaluation services, or Promo Live services.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Evaluation", "Evaluation Company Services");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Live", "Live Company Services");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Custom", "Custom Services");

            this.InsertStringResource("Plugins.Misc.QixolPromo.Account.IssuedCoupons", "Issued coupons");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Coupons.Code", "Code");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Coupons.Status", "Status");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Coupons.ValidTo.Always", "no expiry date");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Coupons.Description", "Description");

            this.InsertStringResource("Plugins.Misc.QixolPromo.BasketValidation", "Basket Validation");
            this.InsertStringResource("Plugins.Misc.QixolPromo.UseSelectedCurrency", "When submitting baskets use selected currency?");
            this.InsertStringResource("Plugins.Misc.QixolPromo.UseSelectedCurrency.Hint", "When submitting baskets to Promo for validation, provide pricing use the currency selected for the current customer.  If this option is not selected, prices will always be provided in the home currency.  If promos are defined which are based on a spend value, or offer a value type discount, and where multiple currencies are supported - it is recommended to select 'true' for this option and ensure the 'Foreign Currency' basket attribute is used when creating promos.");

            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncCheckoutAttributes", "Synchronize Checkout Attributes");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SyncCheckoutAttributes.Hint", "Indicates whether checkout attributes (where those attributes have values which have a price adjustment defined) will be synchronized between NOP and Qixol Promo.");

            this.InsertStringResource("Plugins.Misc.QixolPromo.IntegrationCodes", "Integration Codes");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowPromoDetails", "Show promotion details");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowPromoDetails.Hint", "Select the promo details to be displayed to the user when showing applied promotions in the shopping cart.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowPromoDetails.UserText", "Show promotion end-user/customer description");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowPromoDetails.PromotionName", "Show promotion name");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowPromoDetails.PromotionType", "Show promotion type");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowPromoDetails.Empty", "Don't display");

            this.InsertStringResource("Plugins.Misc.QixolPromo.Advanced", "Advanced");
            this.InsertStringResource("Plugins.Misc.QixolPromo.AdvancedIntegrationSettings", "Advanced Integration Settings");
            this.InsertStringResource("Plugins.Misc.QixolPromo.CartOptions", "Shopping Cart Configuration");
            this.InsertStringResource("Plugins.Misc.QixolPromo.IntegrationCode.ValidationMsg", "The specified code is already in use.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.IntegrationCodes.CustomerRoles.Priority.Help", "If a customer has more than one role then the integration code for the role with the highest numeric priority value will be passed to Promo in the customer group attribute of basket requests.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowHelpMessages", "Show this help page next time?");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowHelpMessages.Hint", "When going to the configuration page, if this box is checked then the Help tab will be shown, if unchecked then the Integration tab will be shown.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.GoToPromo", "Go to Promo");

            this.InsertStringResource("Plugins.Misc.QixolPromo.PromotionsMenu.Integration", "Qixol Promo Configuration");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromotionsMenu.Widgets", "Qixol Promo Widgets");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromotionsMenu.Promo", "Qixol Promo");

            this.InsertStringResource("Plugins.Misc.QixolPromo.RunScheduleTask.Done", "Schedule task was run: {0}");

            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupon.YouWillReceive", "You will receive a coupon with this order");
            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupon.YouReceived", "You received a coupon");
            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupon.Code", "Code");

            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupons.Code", "Code");
            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupons.Status", "Status");
            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupons.ValidTo", "Valid To");
            this.InsertStringResource("Plugin.Misc.QixolPromo.Coupons.Description", "Description");

            this.InsertStringResource("Plugin.Misc.QixolPromo.ShoppingCart.AddItemWarning", "Unable to add promo item {0} to cart.");

            #region Promo Types

            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.BOGOF", "Buy one get one free");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.BOGOR", "Buy one get one reduced");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.BUNDLE", "Bundle");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.DEAL", "Deal");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.PRODUCTSREDUCTION", "Product Reduction");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.FREEPRODUCT", "Free Product");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.ISSUEPOINTS", "Issue Points");
            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.ISSUECOUPON", "Issue Coupon");

            this.InsertStringResource("Plugins.Misc.QixolPromo.PromoType.MULTIPLE", "[Multiple Promos]");

            #endregion

            #region Product Promo Details

            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Title.Offers", "Offers");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Title.YouSave", "You Save");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.GetOneFree", "Get One Free");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.ValidAfter", "Valid after:");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.ValidUntil", "Valid until:");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.RequiredQty", "Required Quantity:");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.RequiredSpend.1", "Spend");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.RequiredSpend.2", "or more on this item to receive this offer.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.RequireAdditionalItems", "** This offer requires additional items to be added to your basket **");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.MinBasketSpend.1", "Spend");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.MinBasketSpend.1", "or more to receive this offer.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.NotForAllVariants", "** This offer is only available for selected options for this item **");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.Availability.From", "Available:");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.Availability.To", "to");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Footer", "Offers are applied when you view your shopping cart");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product.Promos.Item.TieredPricesFrom", "From");

            #endregion

            #region Help Tab

            this.InsertStringResource("Plugins.Misc.QixolPromo.Help", "Help");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ProductTitle", "Qixol Promo nopCommerce Plugin");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Title", "Need help getting started?");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.Title", "Follow these simple steps to get up and running...");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.1.a", "1. Register to evaluate Qixol Promo.  If you haven't already registered, you can ");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.1.b", "click here to register");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.1.c", ", it's quick to setup and free to evaluate.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.2", "2. Copy your Company Key from Qixol Promo.  Your company key is available once you have registered and logged in, by selecting the Configuration area, and then selecting Manage Company.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.3", "3. Click the Enabled tickbox on the 'Integration' tab, and then paste your company key into the Company Key box.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.4.a", "4. Review the configuration on the 'Synchronized Items' tab.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.4.b", "If you are evaluating integration between nopCommerce and Promo, the default settings are recommended.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.4.c", "If you are looking at integrating multiple systems in your infrastructure with Promo, including nopCommerce, you may want to synchronize one or more sets of data from those systems.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.4.d", "The system that 'owns' the data would typically be the system that synchronizes with Promo.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.5.a", "5.  Provide integration codes for shipping methods, customer roles, stores and checkout attributes, on the 'Integration Codes' tab.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.5.b", "Only items with an integration code defined will be synchronized with Qixol Promo, and each code must be unique.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.5.c", "The code is passed to Promo with baskets when they are validated, and can be used in Promo when creating promos, to restrict when they apply.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.6.a", "6.  Click the 'Save' button below to save your settings.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.6.b", "The first time you click Save, all items will be synchronized automatically.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.6.c", "The Synchronization of data is managed by the 'Qixol Promo Data Management' scheduled task.  The task is activated when the 'Enabled' flag is ticked.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Steps.6.d", "Data to be synchronied is held for X minutes before being passed to Qixol Promo.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Resources.Title", "Additional Resources");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Resources.HelpGuide", "Help Guide");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Resources.Tutorials", "Tutorials and getting started videos");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Resources.ProductOverview", "Qixol Promo Product Overview");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Help.Resources.ShowHelperMessagesTitle", "Show Help Messages");
            #endregion

            #region Missed Promotions

            this.InsertStringResource("Plugins.Misc.QixolPromo.MissedPromotions.PageTitle", "Missed Promotions");
            this.InsertStringResource("Plugins.Misc.QixolPromo.MissedPromotions", "Missed Promotions");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowMissedPromotions", "Show Missed Promotions");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ShowMissedPromotions.Hint", "When stepping from the cart to checkout show the missed promotions page.");
            this.InsertStringResource("Plugins.Misc.QixolPromo.MissedPromotion(s)", "Missed Promotion(s)");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ContinueCheckout", "Continue Checkout");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Product(s)", "Product(s)");
            this.InsertStringResource("Plugins.Misc.QixolPromo.SaveFrom", "Save from");
            this.InsertStringResource("Plugins.Misc.QixolPromo.Save", "Save");
            this.InsertStringResource("Plugins.Misc.QixolPromo.ToCompleteThePromotion", "To complete the promotion add products from");
            this.InsertStringResource("Plugins.Misc.QixolPromo.BasketTotalDiscountDescription", "Your total savings");

            #endregion
        }

        private void InsertStringResource(string resourceKey, string text)
        {
            this._stringsList.Add(new KeyValuePair<string, string>(resourceKey, text));
        }

        private void InstallAllStringResources()
        {
            this._stringsList.ForEach(r =>
                {
                    this.AddOrUpdatePluginLocaleResource(r.Key, r.Value);
                });
        }

        private void UninstallAllStringResources()
        {
            this._stringsList.ForEach(r =>
                {
                    this.DeletePluginLocaleResource(r.Key);
                });
        }

        #endregion

        #region Admin area menu entries.

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var promosNode = rootNode.ChildNodes.Where(cn => cn.SystemName == "Promotions").FirstOrDefault();
            if (promosNode != null)
            {
                // It's not inconceivable that hiding the standard menu entries would be annoying to some users (i.e. they already have some discounts 
                // set up and so want to be able to configure them!).  As a result, made this switchable based on a (hidden) setting.
                if (_promoSettings.HideNopDiscountMenuItems)
                {
                    var discountsNode = promosNode.ChildNodes.Where(cn => cn.SystemName == "Discounts").FirstOrDefault();
                    if (discountsNode != null)
                        discountsNode.Visible = false;

                    var campaignsNode = promosNode.ChildNodes.Where(cn => cn.SystemName == "Campaigns").FirstOrDefault();
                    if (campaignsNode != null)
                        campaignsNode.Visible = false;
                }

                promosNode.ChildNodes.Add(new SiteMapNode()
                {
                    Title = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromotionsMenu.Integration"),
                    SystemName = "Misc.QixolPromo",
                    ControllerName = "Plugin",
                    ActionName = "ConfigureMiscPlugin",
                    IconClass = "fa-dot-circle-o",
                    RouteValues = new RouteValueDictionary(new { systemName = "Misc.QixolPromo" }),
                    Visible = true
                });

                promosNode.ChildNodes.Add(new SiteMapNode()
                {
                    Title = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromotionsMenu.Promo"),
                    SystemName = "QixolPromo.Web",
                    IconClass = "fa-arrow-circle-o-right",
                    OpenUrlInNewTab = true,
                    Url = "https://admin.qixolpromo.com",
                    Visible = true
                });
            }
        }

        #endregion

        #region Widget

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PromoWidget";
            controllerName = "Promo";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "Qixol.Plugin.Misc.Promo.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        public IList<string> GetWidgetZones()
        {
            List<string> activeWidgetZoneNames = new List<string>();

            // customer account
            activeWidgetZoneNames.Add("account_navigation_after");

            // shopping cart
            activeWidgetZoneNames.Add("order_summary_content_before");

            // checkout
            activeWidgetZoneNames.Add("opc_content_before");
            activeWidgetZoneNames.Add("checkout_confirm_top");

            // order (customer details) - main widget
            activeWidgetZoneNames.Add("orderdetails_page_top");

            // order (customer details) - line level indicator
            activeWidgetZoneNames.Add("orderdetails_product_line");
            
            ////// order (admin)
            ////activeWidgetZoneNames.Add("xxxx");

            return activeWidgetZoneNames;

            #endregion
        }
    }
}
