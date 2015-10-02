using global::Nop.Core.Domain.Catalog;
using global::Nop.Services.Catalog;
using global::Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Linq;
using System.Xml;
using global::Nop.Services.Logging;
using global::Nop.Core.Domain.Orders;
using global::Nop.Core.Domain.Customers;
using global::Nop.Web.Controllers;
using Qixol.Nop.Promo.Core.Domain.Promo;
using global::Nop.Core;
using global::Nop.Services.Common;
using global::Nop.Core.Domain.Shipping;
using global::Nop.Services.Orders;
using Qixol.System.Extensions;
using Qixol.Nop.Promo.Core.Domain.Import;
using System.Runtime.Serialization.Json;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Promo.Integration.Lib;
using Qixol.Promo.Integration.Lib.Import;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Promo.Integration.Lib.Export;

namespace Qixol.Nop.Promo.Services.Promo
{
    public partial class PromoUtilities : IPromoUtilities
    {
        #region fields

        private readonly PromoSettings _promoSettings;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly ILogger _logger;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductMappingService _productMappingService;

        #endregion

        #region constructor

        public PromoUtilities(
            PromoSettings promoSettings,
            IProductService productService,
            IPictureService pictureService,
            ILogger logger,
            IProductAttributeService productAttributeService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            IProductMappingService productMappingService)
        {
            this._promoSettings = promoSettings;
            this._productService = productService;
            this._pictureService = pictureService;
            this._logger = logger;
            this._productAttributeService = productAttributeService;
            this._workContext = workContext;
            this._genericAttributeService = genericAttributeService;
            this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._productMappingService = productMappingService;
        }

        #endregion

        #region product promos methods

        public ProductImportResponse ImportProductsToPromoService(ProductImportRequest qixolProductImport)
        {
            ProductImportResponse importResult = null;

            try
            {
                var importService = _promoSettings.GetImportService();
                importResult = importService.ImportProducts(qixolProductImport);
            }
            catch (Exception ex)
            {
                _logger.Error("Qixol Promos product update", ex);
            }

            return importResult;
        }

        #endregion

        #region attribute import methods

        public AttributeValuesImportResponse ImportAttributesToPromoService(AttributeValuesImportRequest qixolAttributeImport)
        {
            AttributeValuesImportResponse importResult = null;
            try
            {
                var importService = _promoSettings.GetImportService();
                importResult = importService.ImportAttributeValues(qixolAttributeImport);
            }
            catch (Exception ex)
            {
                _logger.Error("Qixol Promos attribute values update", ex);
            }

            return importResult;
        }

        #endregion

        #region Hierarchy Import methods

        public HierarchyValuesImportResponse ImportHierarchyTopromoService(HierarchyValuesImportRequest hierarchyImport)
        {
            HierarchyValuesImportResponse importResult = null;
            try
            {
                var importService = _promoSettings.GetImportService();
                importResult = importService.ImportHierarchyValues(hierarchyImport);
            }
            catch (Exception ex)
            {
                _logger.Error("Qixol Promos hierarchy update", ex);
            }

            return importResult;            
        }

        #endregion

        // TODO: remove this - just use the GetAttribute, but get <BasketResponse> working
        public BasketResponse GetBasketResponse()
        {
            Customer customer = _workContext.CurrentCustomer;
            string basketResponseString = customer.GetAttribute<string>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);

            BasketResponse basketResponse = basketResponseString.ToObject<BasketResponse>();
            return basketResponse;
        }

        private string GetCategoryBreadCrumb(Category category, IList<Category> allCategories)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            var breadCrumbList = new List<string>();

            while (category != null && //category is not null
                !category.Deleted && //category is not deleted
                category.Published) //category is published
            {
                breadCrumbList.Add(category.Name);
                category = allCategories.Where(c => c.Id == category.ParentCategoryId).FirstOrDefault();
                //category = _categoryService.GetCategoryById(category.ParentCategoryId);
            }
            breadCrumbList.Reverse();
            return string.Join("/", breadCrumbList.ToArray());
        }

        public PromotionDetailsByProductResponse ExportPromotionsForProducts(PromotionDetailsByProductRequest request)
        {
            PromotionDetailsByProductResponse exportResult = null;
            try
            {
                var exportService = _promoSettings.GetExportService();
                exportResult = exportService.ExportPromotionsForProducts(request);
            }
            catch (Exception ex)
            {
                _logger.Error("Qixol Promos export promotions for products", ex);
            }

            return exportResult;
        }

        public PromotionDetailsResponse ExportPromotionsForBasketAndDelivery(PromotionDetailsRequest request)
        {
            PromotionDetailsResponse result = null;
            try
            {
                var exportService = _promoSettings.GetExportService();
                result = exportService.ExportPromotionsForBasket(request);
            }
            catch (Exception ex)
            {
                _logger.Error("Qixol Promos export promotions for products", ex);
            }
            return result;
        }
    }
}
