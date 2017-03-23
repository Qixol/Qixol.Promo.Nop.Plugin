using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Nop.Promo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Core.Domain.Orders;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Services.AttributeValues;
using Nop.Services.Directory;
using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Plugin.Misc.Promo.Extensions.MappingExtensions;
using Nop.Services.Localization;
using Qixol.Nop.Promo.Services.Localization;
using Nop.Services.Vendors;
using Nop.Services.Tax;
using Nop.Services.Helpers;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Logging;
using Nop.Services.Shipping;
using Nop.Services.Events;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework.Security.Captcha;
using Nop.Core.Domain.Seo;
using Nop.Core.Caching;
using global::Nop.Web.Models.Catalog;
using System.Xml.Linq;
using Nop.Web.Factories;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public class PromoProductController : global::Nop.Web.Controllers.ProductController
    {
        #region Fields

        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderService _orderService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Constructors

        public PromoProductController(IProductModelFactory productModelFactory,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            ICompareProductsService compareProductsService,
            IWorkflowMessageService workflowMessageService,
            IOrderReportService orderReportService,
            IOrderService orderService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            ICacheManager cacheManager) : base(productModelFactory,
             productService,
             workContext,
             storeContext,
             localizationService,
             webHelper,
             recentlyViewedProductsService,
             compareProductsService,
             workflowMessageService,
             orderReportService,
             orderService,
             aclService,
             storeMappingService,
             permissionService,
             customerActivityService,
             eventPublisher,
             catalogSettings,
             shoppingCartSettings,
             localizationSettings,
             captchaSettings,
             cacheManager)
        {
            this._productModelFactory = productModelFactory;
            this._productService = productService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._recentlyViewedProductsService = recentlyViewedProductsService;
            this._compareProductsService = compareProductsService;
            this._workflowMessageService = workflowMessageService;
            this._orderReportService = orderReportService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._customerActivityService = customerActivityService;
            this._eventPublisher = eventPublisher;
            this._catalogSettings = catalogSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region public methods

        public PromoProductDetailsModel PromoPrepareProductDetailsModel(Product product, ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            PromoProductDetailsModel promoProductDetailsModel = new PromoProductDetailsModel();
            var productDetailsModel = _productModelFactory.PrepareProductDetailsModel(product, updatecartitem, isAssociatedProduct);
            promoProductDetailsModel.ProductDetailsModel = productDetailsModel;

            return promoProductDetailsModel;

        }

        #endregion

        #region Utilities

        #endregion
    }
}

