using Nop.Core.Events;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;
using Qixol.Nop.Promo.Services;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Services.Catalog;
using System.Collections.Generic;
using Nop.Services.Media;
using System.Linq;
using System.IO;
using System.ServiceModel;
using System.Text;
using Qixol.Nop.Promo.Services.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Promo;

namespace Qixol.Plugin.Misc.Promo.Infrastructure.Cache
{
    public partial class ProductEventConsumer : IConsumer<EntityInserted<Product>>, IConsumer<EntityUpdated<Product>>, 
                                                IConsumer<EntityInserted<ProductAttributeMapping>>, IConsumer<EntityUpdated<ProductAttributeMapping>>, IConsumer<EntityDeleted<ProductAttributeMapping>>,
                                                IConsumer<EntityInserted<ProductAttributeCombination>>, IConsumer<EntityUpdated<ProductAttributeCombination>>, IConsumer<EntityDeleted<ProductAttributeCombination>>,
                                                IConsumer<EntityInserted<ProductAttributeValue>>, IConsumer<EntityUpdated<ProductAttributeValue>>, IConsumer<EntityDeleted<ProductAttributeValue>>
        
    {
        #region fields

        private readonly PromoSettings _promoSettings;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        private readonly IPromoUtilities _promoUtilities;
        private readonly IExportQueueService _exportQueueService;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region constructor

        public ProductEventConsumer(PromoSettings promoSettings,
            IPromoService promosFeedService,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IPictureService pictureService,
            IPromoUtilities promoUtilities,
            IExportQueueService exportQueueService,
            IStoreMappingService storeMappingService)
        {
            this._promoSettings = promoSettings;
            this._productAttributeService = productAttributeService;
            this._productService = productService;
            this._pictureService = pictureService;
            this._promoUtilities = promoUtilities;
            this._exportQueueService = exportQueueService;
            this._storeMappingService = storeMappingService;
        }

        #endregion

        #region product handlers

        public void HandleEvent(EntityInserted<Product> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity))
                InsertQueueItemForProduct(eventMessage.Entity, ExportQueueAction.Insert);                
        }

        public void HandleEvent(EntityUpdated<Product> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity))
                if (eventMessage.Entity.Deleted)
                    InsertQueueItemForProduct(eventMessage.Entity, ExportQueueAction.Delete);                    
                else
                    InsertQueueItemForProduct(eventMessage.Entity, ExportQueueAction.Update);
        }
       
        #endregion

        #region ProductAttributeMappings handlers

        public void HandleEvent(EntityInserted<ProductAttributeMapping> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity.Product, eventMessage.Entity.ProductId))
                InsertQueueItemForProduct(eventMessage.Entity.Product, ExportQueueAction.Update);
        }

        public void HandleEvent(EntityUpdated<ProductAttributeMapping> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity.Product, eventMessage.Entity.ProductId))
                InsertQueueItemForProduct(eventMessage.Entity.Product, ExportQueueAction.Update);                
        }

        public void HandleEvent(EntityDeleted<ProductAttributeMapping> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity.Product, eventMessage.Entity.ProductId))
                InsertQueueItemForProduct(eventMessage.Entity.Product, ExportQueueAction.Update);                
        }

        #endregion

        #region ProductAttributeCombination handlers 

        public void HandleEvent(EntityInserted<ProductAttributeCombination> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity.Product, eventMessage.Entity.ProductId))
                InsertQueueItemForProduct(eventMessage.Entity.Product, ExportQueueAction.Update);                
        }

        public void HandleEvent(EntityUpdated<ProductAttributeCombination> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity.Product, eventMessage.Entity.ProductId))
                InsertQueueItemForProduct(eventMessage.Entity.Product, ExportQueueAction.Update);                
        }

        public void HandleEvent(EntityDeleted<ProductAttributeCombination> eventMessage)
        {
            if (CanSynchronizeProduct(eventMessage.Entity.Product, eventMessage.Entity.ProductId))
                InsertQueueItemForProduct(eventMessage.Entity.Product, ExportQueueAction.Update);
        }

        #endregion

        #region ProductAttributeValue handlers

        public void HandleEvent(EntityInserted<ProductAttributeValue> eventMessage)
        {
            HandleProductAttributeValueChange(eventMessage.Entity);
        }

        public void HandleEvent(EntityUpdated<ProductAttributeValue> eventMessage)
        {
            HandleProductAttributeValueChange(eventMessage.Entity);
        }

        public void HandleEvent(EntityDeleted<ProductAttributeValue> eventMessage)
        {
            HandleProductAttributeValueChange(eventMessage.Entity);
        }

        #endregion

        #region private methods

        private void HandleProductAttributeValueChange(ProductAttributeValue productAttributeValue)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeProducts)
            {
                var attributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeValue.ProductAttributeMappingId);
                if (CanSynchronizeProduct(attributeMapping.Product))
                    InsertQueueItemForProduct(attributeMapping.Product, ExportQueueAction.Update);
            }
        }

        private bool CanSynchronizeProduct(Product product, int productId = 0)
        {
            if (!_promoSettings.Enabled || !_promoSettings.SynchronizeProducts)
                return false;

            if (product == null && productId > 0)
                product = _productService.GetProductById(productId);

            //if (product.ProductType != ProductType.SimpleProduct)
            //    return false;

            if (_promoSettings.StoreId > 0)
            {
                if (product.LimitedToStores)
                {
                    // Check the store is the one that we are synchronizing.
                    var mappings = _storeMappingService.GetStoreMappings<Product>(product);
                    if (mappings != null && !mappings.Any(m => m.StoreId == _promoSettings.StoreId))
                        return false;
                }
            }

            return true;
        }

        private void InsertQueueItemForProduct(Product product, string action)
        {
            List<int> productIds = new List<int>();

            if (product.ProductType == ProductType.GroupedProduct)
                productIds.AddRange(_productService.GetAssociatedProducts(product.Id).Select(p => p.Id).ToList());
            else
                productIds.Add(product.Id);

            // We may have more than one, if the product is a grouped product.
            productIds.ForEach(productId =>
                {
                    _exportQueueService.InsertQueueItem(EntityAttributeName.Product, action, productId, string.Empty, true);
                });            
        }

        #endregion
    }
}
