using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Stores;
using Nop.Services.Tasks;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Tasks
{
    public class PromoSyncTask : ITask 
    {
        private readonly PromoSettings _promoSettings;
        private readonly IPromoDetailService _promoDetailService;
        private readonly IProductPromoMappingService _productPromoMappingService;
        private readonly IProductMappingService _productMappingService;
        private readonly IProductService _productService;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IPromoUtilities _promoUtilities;
        private readonly IStoreService _storeService;

        public PromoSyncTask(PromoSettings promoSettings,
                            IPromoDetailService promoDetailService,
                            IProductPromoMappingService productPromoMappingService,
                            IProductService productService,
                            IProductMappingService productMappingService,
                            IAttributeValueService attributeValueService,
                            IPromoUtilities promoUtilities,
                            IStoreService storeService)
        {
            this._promoSettings = promoSettings;
            this._promoDetailService = promoDetailService;
            this._productPromoMappingService = productPromoMappingService;
            this._productService = productService;
            this._productMappingService = productMappingService;
            this._attributeValueService = attributeValueService;
            this._promoUtilities = promoUtilities;
            this._storeService = storeService;
        }

        public void Execute()
        {
            // If promo isn't enabled - just quit now.
            if (!_promoSettings.Enabled)
                return;

            // If we don't have the required config data, just quit now.
            if (string.IsNullOrEmpty(_promoSettings.CompanyKey)
                || (_promoSettings.ServiceEndpointSelection == SettingsEndpointAddress.CUSTOM_SERVICES
                     && string.IsNullOrEmpty(_promoSettings.PromoExportEndpointAddress)))
                return;

            List<int> promotionsUpdatedThisRun = new List<int>();
            int batch = 0;
            var allProducts = _productService.SearchProducts(productType: ProductType.SimpleProduct, storeId: _promoSettings.StoreId);
            var batchProducts = allProducts.Skip(batch).Take(_promoSettings.BatchSize);

            while (batchProducts.Count() > 0)
            {
                Dictionary<string, int> savedProductMappings = new Dictionary<string, int>();

                var itemPromoDetailsRequest = BuildExportProductPromotionsRequest(batchProducts, savedProductMappings);
                var itemPromoDetailsResponse = _promoUtilities.ExportPromotionsForProducts(itemPromoDetailsRequest);
                if (itemPromoDetailsResponse == null)
                    break;

                ProcessPromotionDetails(itemPromoDetailsResponse.Promotions, promotionsUpdatedThisRun);
                ProcessProductDetails(itemPromoDetailsResponse.Products, savedProductMappings);
                
                batch += _promoSettings.BatchSize;
                batchProducts = allProducts.Skip(batch).Take(_promoSettings.BatchSize);
            }

            var promoDetailsRequest = BuildExportBasketDeliveryPromotionsRequest();
            var promoDetailsResponse = _promoUtilities.ExportPromotionsForBasketAndDelivery(promoDetailsRequest);
            if(promoDetailsResponse != null)
                ProcessPromotionDetails(promoDetailsResponse.Promotions, promotionsUpdatedThisRun);

        }

        /// <summary>
        /// Build the export request for item level promos.  Pass the products for this batch.
        /// </summary>
        /// <param name="batchProducts"></param>
        /// <param name="store"></param>
        /// <param name="savedProductMappings"></param>
        /// <returns></returns>
        private PromotionDetailsByProductRequest BuildExportProductPromotionsRequest(IEnumerable<Product> batchProducts, Dictionary<string, int> savedProductMappings)
        {
            // Build the request first...
            var itemPromoDetailsRequest = new PromotionDetailsByProductRequest()
            {
                ValidateForTime = false,            // Looking for the whole day. 
                CompanyKey = _promoSettings.CompanyKey,
                ValidationDate = DateTime.UtcNow.Date,
                Channel = _promoSettings.Channel,
                StoreGroup = _promoSettings.StoreGroup

                // Note:  Can't pass the store at this point, because there might be multiple stores and the store defined in setttings relates to products to be synchronized.
                //Store = store
            };

            batchProducts.ToList().ForEach(product =>
            {
                var productMappings = _productMappingService.RetrieveAllVariantsByProductId(product.Id, EntityAttributeName.Product, false);
                productMappings.ToList().ForEach(productMapping =>
                {
                    itemPromoDetailsRequest.AddProduct(product.Id.ToString(), productMapping.VariantCode);
                    savedProductMappings.Add(string.Format("<product>{0}</product><variant>{1}</variant>", product.Id.ToString(), productMapping.VariantCode), productMapping.Id);
                });
            });

            return itemPromoDetailsRequest;
        }

        /// <summary>
        /// Build the export request for basket and delivery level promos.
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        private PromotionDetailsRequest BuildExportBasketDeliveryPromotionsRequest()
        {
            return new PromotionDetailsRequest()
            {
                ValidateForTime = false,            // Looking for the whole day. 
                CompanyKey = _promoSettings.CompanyKey,
                ValidationDate = DateTime.UtcNow.Date,
                Channel = _promoSettings.Channel,
                StoreGroup = _promoSettings.StoreGroup

                // Note:  Can't pass the store at this point, because there might be multiple stores and the store defined in setttings relates to products to be synchronized.
                //Store = store
            };
        }

        /// <summary>
        /// For each of the products returned, look for those which have promos, and insert them into the database.
        /// </summary>
        /// <param name="products"></param>
        /// <param name="savedProductMappings"></param>
        private void ProcessProductDetails(List<ExportValidatedProductItem> products, Dictionary<string, int> savedProductMappings)
        {
            products.ToList()
                    .ForEach(product =>
                    {
                        var lookupSavedItemKey = string.Format("<product>{0}</product><variant>{1}</variant>", product.ProductCode, product.VariantCode);
                        var productMappingId = savedProductMappings[lookupSavedItemKey];

                        // Get all existing items, and delete them.
                        var existingItems = _productPromoMappingService.RetrieveForProductMapping(productMappingId);
                        existingItems.ToList().ForEach(ei => { _productPromoMappingService.Delete(ei); });

                        if (product.Promotions != null && product.Promotions.Count > 0)
                        {
                            product.Promotions.ToList().ForEach(promotion =>
                            {
                                _productPromoMappingService.Insert(GetProductMapping(promotion, productMappingId));
                            });
                        }
                    });
        }

        /// <summary>
        /// For each promo, remove it if already exists, and then insert it.
        /// </summary>
        /// <param name="promotions"></param>
        /// <param name="promotionsUpdatedThisRun"></param>
        private void ProcessPromotionDetails(List<ExportPromotionDetailsItem> promotions, List<int> promotionsUpdatedThisRun)
        {
            promotions.ForEach(promotion =>
            {
                if (!promotionsUpdatedThisRun.Contains(promotion.Id))
                {
                    // If the promo is already defined, delete it.
                    var existingCheck = _promoDetailService.RetrieveByPromoId(promotion.Id);
                    if (existingCheck != null)
                        _promoDetailService.Delete(existingCheck);

                    // Insert the promo details.
                    _promoDetailService.Insert(GetPromotionDetail(promotion));
                    promotionsUpdatedThisRun.Add(promotion.Id);
                }
            });
        }

        private PromoDetail GetPromotionDetail(ExportPromotionDetailsItem promotion)
        {
            var newPromoDetails = new PromoDetail()
            {
                PromoId = promotion.Id,
                PromoName = promotion.PromotionName,
                PromoTypeName = promotion.PromotionType,
                YourReference = promotion.YourReference,
                ReportingCode = promotion.ReportingCode,
                DiscountAmount = promotion.DiscountAmount,
                DiscountPercent = promotion.DiscountPercent,
                BundlePrice = promotion.BundlePrice,
                MinimumSpend = promotion.MinimumSpend,
                BasketRestrictions = promotion.HasAdditionalBasketRestrictions,
                CouponRestrictions = promotion.HasCouponRestrictions,
                ValidFrom = DateTime.Compare(promotion.ValidFrom, DateTime.MinValue) == 0 ? default(DateTime?) : promotion.ValidFrom,
                ValidTo = DateTime.Compare(promotion.ValidTo, DateTime.MinValue) == 0 ? default(DateTime?) : promotion.ValidTo,
                DisplayText = promotion.DisplayText,
                AppliesToItems = true,
                AppliesToBasket = false,
                AppliesToDelivery = false,
                PromoXml = promotion.AllXml,
                CreatedDate = DateTime.Now
            };

            return newPromoDetails;
        }

        private ProductPromotionMapping GetProductMapping(ExportValidatedPromotionItem promotion, int productMappingId)
        {
            return new ProductPromotionMapping()
            {
                ProductMappingId = productMappingId,
                RequiredQty = promotion.RequiredQty,
                RequiredSpend = promotion.RequiredSpend,
                PromotionId = promotion.Promotion.Id,
                MatchingRestrictions = promotion.MatchingRestrictions,
                MultipleProductRestrictions = promotion.MultipleProductRestrictions,
                CreatedDate = DateTime.Now.Date
            };
        }
    }
}
