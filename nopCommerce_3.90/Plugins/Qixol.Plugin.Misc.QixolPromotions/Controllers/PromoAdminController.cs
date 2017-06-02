using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Threading;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Nop.Promo.Services;
using Qixol.Nop.Promo.Core.Domain;
using Nop.Services.Tasks;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Qixol.Nop.Promo.Core.Domain.Tasks;
using Qixol.Nop.Promo.Services.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Nop.Web.Framework.Kendoui;
using Qixol.Nop.Promo.Services.AttributeValues;
using Nop.Web.Framework.Mvc;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Qixol.Nop.Promo.Services.Promo;
using System.Reflection;
using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Misc.Promo.Extensions.MappingExtensions;
using Nop.Services.Shipping;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    [AdminAuthorize]
    public partial class PromoAdminController : BasePluginController
    {
        #region fields

        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IStoreService _storeService;
        private readonly PromoSettings _promoSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly IPromoService _promoService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IExportQueueService _exportQueueService;
        private readonly IProductAttributeConfigService _productAttributeConfigService;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IShippingService _shippingService;
        private readonly ICustomerService _customerService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPromoPictureService _promoPictureService;
        private readonly IPictureService _pictureService;

        #endregion

        #region constructor

        public PromoAdminController(
            IProductService productService, ICurrencyService currencyService,
            ILocalizationService localizationService, IPluginFinder pluginFinder,
            ILogger logger, IWebHelper webHelper, IStoreService storeService,
            PromoSettings promoSettings, ISettingService settingService,
            IPermissionService permissionService, ICategoryService categoryService,
            IPromoService promoService,
            IScheduleTaskService scheduleTaskService,
            IExportQueueService exportQueueService,
            IProductAttributeConfigService productAttributeConfigService,
            IAttributeValueService attributeValueService,
            IShippingService shippingService,
            ICustomerService customerService,
            ICheckoutAttributeService checkoutAttributeService,
            IPromoPictureService promoPictureService,
            IPictureService pictureService)
        {
            this._productService = productService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._pluginFinder = pluginFinder;
            this._logger = logger;
            this._webHelper = webHelper;
            this._storeService = storeService;
            this._promoSettings = promoSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._categoryService = categoryService;
            this._promoService = promoService;
            this._scheduleTaskService = scheduleTaskService;
            this._exportQueueService = exportQueueService;
            this._productAttributeConfigService = productAttributeConfigService;
            this._attributeValueService = attributeValueService;
            this._shippingService = shippingService;
            this._customerService = customerService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._promoPictureService = promoPictureService;
            this._pictureService = pictureService;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new PromoConfigureModel();

            // general
            model.Enabled = _promoSettings.Enabled;
            model.LogMessages = _promoSettings.LogMessages;
            model.ShowHelperMessages = _promoSettings.ShowHelperMessages;
            
            // Connection Details
            model.CompanyKey = _promoSettings.CompanyKey;
            model.PromoImportServiceEndpointAddress = _promoSettings.PromoImportEndpointAddress;
            model.BasketRequestServiceEndpointAddress = _promoSettings.BasketRequestEndpointAddress;
            model.PromoExportServiceEndpointAddress = _promoSettings.PromoExportEndpointAddress;

            // Product Feed
            //model.ProductPictureSize = _promoSettings.ProductPictureSize;
            model.StoreId = _promoSettings.StoreId;
            //model.BatchSize = _promoSettings.BatchSize;

            // Company and Store details
            model.StoreGroup = _promoSettings.StoreGroup;
            model.Channel = _promoSettings.Channel;
            model.SelectedServicesEndPoint = _promoSettings.ServiceEndpointSelection;

            model.SynchronizeCustomerRoles = _promoSettings.SynchronizeCustomerRoles;
            model.SynchronizeProducts = _promoSettings.SynchronizeProducts;
            model.SynchronizeShippingMethods = _promoSettings.SynchronizeShippingMethods;
            model.SynchronizeStores = _promoSettings.SynchronizeStores;
            model.SynchronizeCheckoutAttributes = _promoSettings.SynchronizeCheckoutAttributes;
            model.SynchronizeCurrencies = _promoSettings.SynchronizeCurrencies;

            model.UseSelectedCurrencyWhenSubmittingBaskets = _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets;
            model.ShowMissedPromotions = _promoSettings.ShowMissedPromotions;

            model.ShowPromotionNameOption = _promoSettings.ShowPromotionDetailsInBasket;
            model.DisplayVersion = GetDisplayVersion();

            model.SynchronizeAllWhenSaving = _promoSettings.InitialSetup;
            model.CanUpdateSynchronizeAll = !_promoSettings.InitialSetup;
            model.ShowAdvancedIntegrationSettings = _promoSettings.ShowAdvancedIntegrationSettings;

            SetupModelLists(model);

            return View("~/Plugins/Misc.QixolPromo/Views/Admin/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        [FormValueRequired("save")]
        public ActionResult Configure(PromoConfigureModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            bool enabledChanged = !_promoSettings.Equals(model.Enabled);
            
            //save settings
            // general
            _promoSettings.Enabled = model.Enabled;
            _promoSettings.LogMessages = model.LogMessages;
            _promoSettings.ShowHelperMessages = model.ShowHelperMessages;

            // Connection Details
            _promoSettings.CompanyKey = model.CompanyKey;
            _promoSettings.PromoImportEndpointAddress = model.PromoImportServiceEndpointAddress;
            _promoSettings.PromoExportEndpointAddress = model.PromoExportServiceEndpointAddress;
            _promoSettings.BasketRequestEndpointAddress = model.BasketRequestServiceEndpointAddress;
            _promoSettings.StoreId = model.StoreId;

            // Settings no longer updated via the Config area.
            //_promoSettings.ProductPictureSize = model.ProductPictureSize;
            //_promoSettings.CurrencyId = model.CurrencyId;
            //_promoSettings.BatchSize = model.BatchSize;

            // Company and Store
            _promoSettings.StoreGroup = model.StoreGroup;
            _promoSettings.Channel = model.Channel;

            _promoSettings.SynchronizeCustomerRoles = model.SynchronizeCustomerRoles;
            _promoSettings.SynchronizeProducts = model.SynchronizeProducts;
            _promoSettings.SynchronizeShippingMethods = model.SynchronizeShippingMethods;
            _promoSettings.SynchronizeStores = model.SynchronizeStores;
            _promoSettings.SynchronizeCheckoutAttributes = model.SynchronizeCheckoutAttributes;
            _promoSettings.SynchronizeCurrencies = model.SynchronizeCurrencies;

            _promoSettings.UseSelectedCurrencyWhenSubmittingBaskets = model.UseSelectedCurrencyWhenSubmittingBaskets;
            _promoSettings.ShowMissedPromotions = model.ShowMissedPromotions;

            _promoSettings.ShowPromotionDetailsInBasket = model.ShowPromotionNameOption;
            _promoSettings.ServiceEndpointSelection = model.SelectedServicesEndPoint;

            bool doSyncNow = (_promoSettings.Enabled
                                && !string.IsNullOrEmpty(_promoSettings.CompanyKey)
                                && (model.SynchronizeAllWhenSaving || _promoSettings.InitialSetup));

            bool isInitialSetup = _promoSettings.InitialSetup;
            if (doSyncNow && _promoSettings.InitialSetup)
                _promoSettings.InitialSetup = false;

            _settingService.SaveSetting(_promoSettings);

            string formKey = "checkbox_product_attribute";
            var checkedProductAttributes = form[formKey] != null ? form[formKey].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
            var productAttributes = _productAttributeConfigService.RetrieveAll().ToList();
            foreach (var productAttribute in productAttributes)
            {
                productAttribute.Enabled = checkedProductAttributes.Contains(productAttribute.Id);
                _productAttributeConfigService.Update(productAttribute);
            }

            if (enabledChanged)
            {
                // The enabled flag has changed - so go and find the scheduled task and enable/disable appropriately.
                ScheduleTask scheduleTask = _scheduleTaskService.GetTaskByType(PromoTaskNames.DataFeedTask);
                if (scheduleTask != null)
                {
                    scheduleTask.Enabled = _promoSettings.Enabled;
                    _scheduleTaskService.UpdateTask(scheduleTask);
                }

                scheduleTask = _scheduleTaskService.GetTaskByType(PromoTaskNames.PromoSyncTask);
                if (scheduleTask != null)
                {
                    scheduleTask.Enabled = _promoSettings.Enabled;
                    _scheduleTaskService.UpdateTask(scheduleTask);
                }
            }

            if (doSyncNow)
                QueueSyncNowItems(isInitialSetup);

            ModelState.Clear();

            //redisplay the form
            return Configure();
        }

        private void QueueSyncNowItems(bool isInitialSetup)
        {
            int syncCount = 0;
            if (_promoSettings.SynchronizeProducts)
            {
                _exportQueueService.InsertQueueItemForAll(EntityAttributeName.Product);
                syncCount++;
            }

            if (_promoSettings.SynchronizeCustomerRoles)
            {
                _exportQueueService.InsertQueueItemForAll(EntityAttributeName.CustomerRole);
                syncCount++;
            }

            if (_promoSettings.SynchronizeShippingMethods)
            {
                _exportQueueService.InsertQueueItemForAll(EntityAttributeName.DeliveryMethod);
                syncCount++;
            }

            if (_promoSettings.SynchronizeStores)
            {
                _exportQueueService.InsertQueueItemForAll(EntityAttributeName.Store);
                syncCount++;
            }

            if (_promoSettings.SynchronizeCurrencies)
            {
                if (isInitialSetup)
                    SynchronizeAllCurrencies();

                _exportQueueService.InsertQueueItemForAll(EntityAttributeName.Currency);
                syncCount++;
            }

            if (_promoSettings.SynchronizeCheckoutAttributes)
            {
                _exportQueueService.InsertQueueItemForAll(EntityAttributeName.CheckoutAttribute);
                syncCount++;
            }

            if (syncCount > 0)
            {
                ScheduleTask scheduleTask = _scheduleTaskService.GetTaskByType(PromoTaskNames.DataFeedTask);
                Task task = new Task(scheduleTask);
                ExecuteTask(task);

                scheduleTask = _scheduleTaskService.GetTaskByType(PromoTaskNames.PromoSyncTask);
                task = new Task(scheduleTask);
                ExecuteTask(task);
            }
        }

        private List<IntegrationCodeItemModel> GetIntegrationCodesBaseList(string systemName)
        {
            switch (systemName)
            {
                case EntityAttributeName.Store:
                    return _storeService.GetAllStores()
                                          .Select(s => new IntegrationCodeItemModel()
                                              {
                                                  EntityId = s.Id,
                                                  EntityName = s.Name,
                                                  EntityAttributeSystemName = EntityAttributeName.Store
                                              }).ToList();

                case EntityAttributeName.DeliveryMethod:
                    var shippingMethods = _shippingService.GetAllShippingMethods()
                                           .Select(s => new IntegrationCodeItemModel()
                                                {
                                                    EntityId = s.Id,
                                                    EntityName = s.Name,
                                                    EntityAttributeSystemName = EntityAttributeName.DeliveryMethod
                                                }).ToList();
                    var pickupPoints = _shippingService.LoadAllPickupPointProviders();
                    return shippingMethods;

                case EntityAttributeName.CustomerRole:
                    return _customerService.GetAllCustomerRoles()
                                           .Select(s => new IntegrationCodeItemModel()
                                           {
                                               EntityId = s.Id,
                                               EntityName = s.Name,
                                               EntityAttributeSystemName = EntityAttributeName.CustomerRole
                                           }).ToList();

                case EntityAttributeName.CheckoutAttribute:
                    return _checkoutAttributeService.GetAllCheckoutAttributes()
                                           .Select(s => new IntegrationCodeItemModel()
                                           {
                                               EntityId = s.Id,
                                               EntityName = s.Name,
                                               EntityAttributeSystemName = EntityAttributeName.CheckoutAttribute
                                           }).ToList();

                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// When first synchronizing, sync all currencies that are currenty published.
        /// </summary>
        private void SynchronizeAllCurrencies()
        {
            var allCurrencies = _currencyService.GetAllCurrencies();
            allCurrencies.Where(c => c.Published)
                         .ToList()
                         .ForEach(currency =>
                         {
                             _attributeValueService.Insert(new AttributeValueMappingItem()
                             {
                                 AttributeName = EntityAttributeName.Currency,
                                 AttributeValueId = currency.Id,
                                 Code = currency.CurrencyCode
                             });
                         });
        }

        private string GetDisplayVersion()
        {
            var promoPlugin = _pluginFinder.GetPluginDescriptorBySystemName("Misc.QixolPromo");
            return promoPlugin.Version;

            /*
            var thisAssembly = Assembly.GetCallingAssembly();
            return string.Format("v{0}", thisAssembly.GetName().Version.ToString());
            */
        }

        [HttpPost]
        public ActionResult IntegrationCodesList(DataSourceRequest command, string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return new NullJsonResult();

            IQueryable<AttributeValueMappingItem> allAttributeValues = _attributeValueService.RetrieveAllForAttribute(systemName);
            var modelItems = GetIntegrationCodesBaseList(systemName);

            if (modelItems != null && allAttributeValues != null)
            {
                modelItems.ForEach(i =>
                    {
                        var codeAttrib = allAttributeValues.Where(av => av.AttributeValueId == i.EntityId).FirstOrDefault();
                        if (codeAttrib != null)
                        {
                            i.AttributeId = codeAttrib.Id;
                            i.IntegrationCode = codeAttrib.Code;
                            i.Priority = codeAttrib.Priority.HasValue ? codeAttrib.Priority.Value : 0;
                        }
                    });
            }

            var gridModel = new DataSourceResult
            {
                Data = modelItems,
                Total = modelItems != null ? modelItems.Count() : 0
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult IntegrationCodeUpdate(IntegrationCodeItemModel model)
        {
            if (!string.IsNullOrEmpty(model.IntegrationCode))
            {
                // Check for existing uses of this code!  - but only if we're NOT clearing it out.
                IQueryable<AttributeValueMappingItem> allAttributeValues = _attributeValueService.RetrieveAllForAttribute(model.EntityAttributeSystemName);
                if (allAttributeValues != null && allAttributeValues.Count() > 0
                    && allAttributeValues.Any(av => string.Compare(av.Code, model.IntegrationCode, true) == 0 && av.AttributeValueId != model.EntityId))
                {
                    ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Misc.QixolPromo.IntegrationCode.ValidationMsg"));
                    return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
                }
            }

            if (model.AttributeId == 0)
            {
                // It's a new code.
                var newAttribItem = new AttributeValueMappingItem()
                {
                    AttributeName = model.EntityAttributeSystemName,
                    AttributeValueId = model.EntityId,
                    Code = model.IntegrationCode
                };
                if (model.Priority.HasValue)
                    newAttribItem.Priority = model.Priority.Value;

                _attributeValueService.Insert(newAttribItem);
            }
            else
            {
                var existingAttribItem = _attributeValueService.Retrieve(model.EntityId, model.EntityAttributeSystemName);
                if (existingAttribItem != null)
                {
                    existingAttribItem.Code = model.IntegrationCode;
                    if (model.Priority.HasValue)
                        existingAttribItem.Priority = model.Priority.Value;
                    _attributeValueService.Update(existingAttribItem);
                }
            }

            return new NullJsonResult();
        }

        private void SetupModelLists(PromoConfigureModel model)
        {
            //stores
            model.AvailableStores.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString() });

            //currencies
            foreach (var c in _currencyService.GetAllCurrencies())
                model.AvailableCurrencies.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });

            model.ServicesEndpointsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Evaluation"), Value = SettingsEndpointAddress.EVALUATION_SERVICES.ToString() });
            model.ServicesEndpointsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Live"), Value = SettingsEndpointAddress.LIVE_SERVICES.ToString() });

#if DEBUG
            model.ServicesEndpointsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ServiceEndpoint.Custom"), Value = SettingsEndpointAddress.CUSTOM_SERVICES.ToString() });
            model.AllowCustomEndpoint = true;
#endif

            model.ShowPromotionNameOptionsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ShowPromoDetails.UserText"), Value = PromotionDetailsDisplayOptions.ShowEndUserText.ToString() });
            model.ShowPromotionNameOptionsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ShowPromoDetails.PromotionName"), Value = PromotionDetailsDisplayOptions.ShowPromotionName.ToString() });
            model.ShowPromotionNameOptionsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ShowPromoDetails.PromotionType"), Value = PromotionDetailsDisplayOptions.ShowPromotionType.ToString() });
            model.ShowPromotionNameOptionsList.Add(new SelectListItem() { Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.ShowPromoDetails.Empty"), Value = PromotionDetailsDisplayOptions.ShowNoText.ToString() });

            model.ProductConfigItems = _productAttributeConfigService.RetrieveAll()
                                                                     .ToList()
                                                                     .Select(pac => pac.ToModel())
                                                                     .ToList();

            if (model.ProductConfigItems != null && model.ProductConfigItems.Count > 0)
            {
                model.ProductConfigItems.ForEach(pac =>
                {
                    pac.NameText = _localizationService.GetResource(pac.NameResource);
                });

                model.ProductConfigItems = model.ProductConfigItems.OrderBy(pac => pac.NameText).ToList();
            }

        }

        private void ExecuteTask(Task task)
        {
            try
            {
                // set the task to enabled so it will run
                task.Enabled = true;
                task.Execute(true, false, true); // do not dispose - otherwise we can get an exception that DbContext is disposed, only run on one instance of a web farm
                SuccessNotification(string.Format(_localizationService.GetResource("Plugins.Misc.QixolPromo.RunScheduleTask.Done"), task.Name));
            }
            catch (Exception e)
            {
                ErrorNotification(e);
            }
        }

        #endregion


    }
}
