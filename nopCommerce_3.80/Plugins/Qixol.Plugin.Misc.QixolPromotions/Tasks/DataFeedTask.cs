using System;
using Nop.Core.Domain.Media;
using Nop.Services.Configuration;
using Nop.Services.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using System.Linq;
using Nop.Services.Media;
using Nop.Core.Plugins;
using Nop.Core.Domain.Stores;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Services.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Services;
using Qixol.Nop.Promo.Services.AttributeValues;
using Nop.Services.Shipping;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Nop.Services.Customers;
using Qixol.Nop.Promo.Core.Domain.Import;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Services.Vendors;
using Nop.Services.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.Tax;
using Qixol.Promo.Integration.Lib.Import;
using Nop.Services.Orders;
using Qixol.Nop.Promo.Services.Promo;
using Nop.Services.Directory;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Orders;

namespace Qixol.Plugin.Misc.Promo.Tasks
{
    public partial class DataFeedTask : ITask
    {
        #region fields

        private readonly IPluginFinder _pluginFinder;
        private readonly IStoreService _storeService;
        private readonly PromoSettings _promoSettings;
        private readonly IExportQueueService _exportQueueService;
        private readonly IPromoService _qixolPromoService;
        private readonly IPromoUtilities _qixolPromoUtilities;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IShippingService _shippingService;
        private readonly ICustomerService _customerService;
        private readonly IProductAttributeConfigService _productAttributeConfigService;
        private readonly IProductMappingService _productMappingService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IVendorService _vendorService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Constructors

        public DataFeedTask(
            IPluginFinder pluginFinder,
            IStoreService storeService,
            PromoSettings promoSettings,
            IExportQueueService exportQueueService,
            IPromoService qixolPromoService,
            IPromoUtilities qixolPromoUtilities,
            IAttributeValueService attributeValueService,
            IShippingService shippingService,
            ICustomerService customerService,
            IProductAttributeConfigService productAttributeConfigService,
            IProductMappingService productMappingService,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IVendorService vendorService,
            ITaxCategoryService taxCategoryService,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService)
        {
            this._pluginFinder = pluginFinder;
            this._storeService = storeService;
            this._promoSettings = promoSettings;
            this._exportQueueService = exportQueueService;
            this._qixolPromoService = qixolPromoService;
            this._qixolPromoUtilities = qixolPromoUtilities;
            this._attributeValueService = attributeValueService;
            this._shippingService = shippingService;
            this._customerService = customerService;
            this._productAttributeConfigService = productAttributeConfigService;
            this._productMappingService = productMappingService;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
            this._vendorService = vendorService;
            this._taxCategoryService = taxCategoryService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._currencyService = currencyService;
        }

        #endregion

        #region Methods

        public void Execute()
        {
            // If promo isn't enabled - just quit now.
            if (!_promoSettings.Enabled)
                return;

            // If we don't have the required config data, just quit now.
            if (string.IsNullOrEmpty(_promoSettings.CompanyKey)
                || (_promoSettings.ServiceEndpointSelection == SettingsEndpointAddress.CUSTOM_SERVICES
                     && string.IsNullOrEmpty(_promoSettings.PromoImportEndpointAddress)))
                return;

            var entriesProcessedThisRun = new List<int>();
            List<ProductAttributeConfigItem> productAttributeConfigItems = null;
            List<Vendor> vendors = null;
            List<TaxCategory> taxCategories = null;

            bool continueLoop = false;
            do
            {
                // Restore back to default.
                continueLoop = false;

                // We're going to get the list each time, and then filter it down, so that if it changes outside of this code, we will pick up those changes.
                //
                var fiveMinsAgo = DateTime.Now.AddSeconds(-_promoSettings.QueueHoldPeriodInSeconds);
                var pendingQueueItems = _exportQueueService.RetrievePending();
                if (pendingQueueItems != null && pendingQueueItems.Count() > 0)
                {
                    
                    var allItemsToProcess = pendingQueueItems.Where(qi => (DateTime.Compare(fiveMinsAgo, qi.CreatedOnUtc) >= 0
                                                                             || (qi.Action == ExportQueueAction.Delete || qi.Action == ExportQueueAction.All))
                                                                            && !entriesProcessedThisRun.Contains(qi.Id))
                                                              .OrderBy(ob => ob.CreatedOnUtc);                                                            

                    if (allItemsToProcess != null && allItemsToProcess.Count() > 0)
                    {
                        var groupedItemsToProcess = allItemsToProcess.GroupBy(gb => gb.EntityName)
                                                              .FirstOrDefault()
                                                              .ToList();

                        groupedItemsToProcess.ForEach(i =>
                            {
                                // To ensure we only try and process each item once this run!                        
                                entriesProcessedThisRun.Add(i.Id);

                                // Flag the item status as 'Processing'...
                                i.Status = ExportQueueStatus.Processing;
                                _exportQueueService.UpdateItem(i);
                            });


                        bool noResponse = false;
                        bool importResult = false;
                        string importRef = string.Empty;
                        string importMessages = string.Empty;

                        switch (groupedItemsToProcess.First().EntityName)
                        {
                            case EntityAttributeName.CustomerRole:
                            case EntityAttributeName.DeliveryMethod:
                            case EntityAttributeName.Store:
                            case EntityAttributeName.Currency:
                                var attribImportResult = ProcessBasketAttributeItems(groupedItemsToProcess);
                                if (attribImportResult != null && attribImportResult.Summary != null)
                                {
                                    importResult = attribImportResult.Summary.ProcessedSuccessfully;
                                    importRef = attribImportResult.Reference;
                                    if (!importResult)
                                        importMessages = attribImportResult.GetResponseMessages<AttributeValuesImportResponseItem>();
                                }
                                else
                                {
                                    noResponse = true;
                                }
                                break;

                            case EntityAttributeName.Product:
                                if (productAttributeConfigItems == null)
                                    productAttributeConfigItems = _productAttributeConfigService.RetrieveAll().ToList();

                                if (vendors == null)
                                    vendors = _vendorService.GetAllVendors().ToList();

                                if (taxCategories == null)
                                    taxCategories = _taxCategoryService.GetAllTaxCategories().ToList();

                                var productImportResult = ProcessProducts(groupedItemsToProcess, productAttributeConfigItems, vendors, taxCategories);
                                if (productImportResult != null && productImportResult.Summary != null)
                                {
                                    importResult = productImportResult.Summary.ProcessedSuccessfully;
                                    importRef = productImportResult.Reference;
                                    if (!importResult)
                                        importMessages = productImportResult.GetResponseMessages<ProductImportResponseItem>();
                                }
                                else
                                {
                                    noResponse = true;
                                }
                                break;

                            case EntityAttributeName.CheckoutAttribute:

                                var checkoutAttribImportResult = ProcessCheckoutAttributes(groupedItemsToProcess);
                                if (checkoutAttribImportResult != null && checkoutAttribImportResult.Summary != null)
                                {
                                    importResult = checkoutAttribImportResult.Summary.ProcessedSuccessfully;
                                    importRef = checkoutAttribImportResult.Reference;
                                    if (!importResult)
                                        importMessages = checkoutAttribImportResult.GetResponseMessages<ProductImportResponseItem>();
                                }
                                else
                                {
                                    noResponse = true;
                                }
                                break;

                            default:
                                break;
                        }

                        // Now update all the items we've just processed...
                        groupedItemsToProcess.ForEach(i =>
                        {
                            if (!noResponse)
                            {
                                if (importResult)
                                {
                                    i.Status = ExportQueueStatus.Ok;
                                    i.PromoReference = importRef;
                                }
                                else
                                {
                                    i.Status = ExportQueueStatus.ContentFailure;
                                    i.Messages = importMessages;
                                }
                            }
                            else
                            {
                                // It must have been a comms error!!
                                i.Status = ExportQueueStatus.CommsFailure;
                            }

                            i.UpdatedOnUtc = DateTime.UtcNow;
                            _exportQueueService.UpdateItem(i);
                        });


                        continueLoop = true;
                    }
                }

            } while (continueLoop);
        }

        #endregion

        #region Private Helpers 

        /// <summary>
        /// Build the bare-bones of the import request, setting the attribute token based on the queued item entity name.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private AttributeValuesImportRequest BuildBaseBasketAttributeImport(ExportQueueItem item)
        {
            return new AttributeValuesImportRequest()
            {
                CompanyKey = _promoSettings.CompanyKey,
                AttributeToken = EntityAttributeName.ToPromoAttributeName(item.EntityName)
            };
        }

        /// <summary>
        /// Build the bare-bones of the import request for products.
        /// </summary>
        /// <returns></returns>
        private ProductImportRequest BuildBaseProductImport()
        {
            return new ProductImportRequest()
            {
                CompanyKey = _promoSettings.CompanyKey
            };
        }

        /// <summary>
        /// Build the bare-bones of a hierarchy request (for stores).
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private HierarchyValuesImportRequest BuildBaseHierarchyImport(ExportQueueItem item)
        {
            return new HierarchyValuesImportRequest()
            {
                CompanyKey = _promoSettings.CompanyKey,
                HierarchyToken = "store"            // TO BE REVIEWED.
            };
        }

        /// <summary>
        /// Add an item to the import.
        /// </summary>
        /// <param name="import"></param>
        /// <param name="itemName"></param>
        /// <param name="itemDisplayName"></param>
        /// <param name="isDeleted"></param>
        private void AddImportItem(AttributeValuesImportRequest import, string itemName, string itemDisplayName, bool isDeleted = false)
        {
            import.Items.Add(new AttributeValuesImportRequestItem() { Value = itemName, DisplayName = itemDisplayName, Deleted = isDeleted });
        }

        /// <summary>
        /// Add an item to the store hierarchy.
        /// </summary>
        /// <param name="import"></param>
        /// <param name="itemName"></param>
        /// <param name="itemDisplayName"></param>
        /// <param name="isDeleted"></param>
        private void AddHierarchyImportItem(HierarchyValuesImportRequest import, string itemName, string itemDisplayName, bool isDeleted = false)
        {
            HierarchyValuesImportRequestItem channelItem = null;
            HierarchyValuesImportRequestItem storeGroupItem = null;

            if (import.Items != null && import.Items.Count > 0)
            {
                channelItem = import.Items.Where(i => i.AttributeToken == "channel").FirstOrDefault();
                if (channelItem != null)
                {
                    storeGroupItem = channelItem.ChildItems.Where(c => c.AttributeToken == "storegroup").FirstOrDefault();
                }
            }

            if (channelItem == null)
            {
                channelItem = new HierarchyValuesImportRequestItem() { AttributeToken = "channel", Value = _promoSettings.Channel };
                import.Items.Add(channelItem);
            }
            if (storeGroupItem == null)
            {
                storeGroupItem = new HierarchyValuesImportRequestItem() { AttributeToken = "storegroup", Value = _promoSettings.StoreGroup };
                channelItem.ChildItems.Add(storeGroupItem);
            }

            // Finally, add the store to the store group.
            storeGroupItem.ChildItems.Add(new HierarchyValuesImportRequestItem() { AttributeToken = "store", Value = itemName, DisplayName = itemDisplayName, Deleted = isDeleted });

        }

        /// <summary>
        /// Get the list of attributes values (nop entities - but we only need the ID and the name).
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        private List<InternalAttributeItem> RetrieveAttributes(string entityName)
        {
            List<InternalAttributeItem> returnList = null;
            switch (entityName)
            {
                case EntityAttributeName.DeliveryMethod:
                    returnList = _shippingService.GetAllShippingMethods()
                                                 .Select(sm => new InternalAttributeItem(sm.Id, sm.Name))
                                                 .ToList();
                    break;

                case EntityAttributeName.CustomerRole:
                    returnList = _customerService.GetAllCustomerRoles()
                                                 .Select(sm => new InternalAttributeItem(sm.Id, sm.Name))
                                                 .ToList();
                    break;

                case EntityAttributeName.Store:
                    returnList = _storeService.GetAllStores()
                                              .Select(sm => new InternalAttributeItem(sm.Id, sm.Name))
                                              .ToList();
                    break;

                case EntityAttributeName.Currency:
                    returnList = _currencyService.GetAllCurrencies()
                                                 .Select(c => new InternalAttributeItem(c.Id, c.Name))
                                                 .ToList();
                    break;

                default:
                    break;
            }
            return returnList;
        }

        /// <summary>
        /// For the list of specified attribute IDs - loop through them and flag them as synchronized.
        /// </summary>
        /// <param name="attribIds"></param>
        private void FlagAttributeAsSynchonized(List<int> attribIds)
        {
            if (attribIds == null || attribIds.Count == 0)
                return;

            attribIds.ForEach(attribId =>
                {
                    var attribute = _attributeValueService.RetrieveById(attribId);
                    if (attribute != null)
                    {
                        attribute.Synchronized = true;
                        attribute.SynchronizedCode = attribute.Code;
                        _attributeValueService.Update(attribute, false);
                    }
                });
        }

        #endregion

        #region Basket Attributes Handling 

        /// <summary>
        /// Send the required basket attribute values to Promo.  If there are none to send, just return a response indicating it was successful.
        /// </summary>
        /// <param name="exportItems"></param>
        /// <returns></returns>
        private ImportResponse<AttributeValuesImportResponseItem> ProcessBasketAttributeItems(List<ExportQueueItem> exportItems)
        {
            ImportResponse<AttributeValuesImportResponseItem> result = null;

            var attribsToUpdate = new List<int>();
            var attribValuesImportRequest = BuildBaseBasketAttributeImport(exportItems.First());
            var hierarchyImportRequest = BuildBaseHierarchyImport(exportItems.First());
            var allNopAttributeValues = RetrieveAttributes(exportItems.First().EntityName);
            var allAttributes = _attributeValueService.RetrieveAllForAttribute(exportItems.First().EntityName).ToList();
            bool useHierarchyImport = exportItems.First().EntityName == EntityAttributeName.Store;

            for (int i = 0; i < 2; i++)
            {
                List<InternalAttributeToProcess> itemsToImport = new List<InternalAttributeToProcess>();

                if (i == 0)
                {
                    // Deal with updating items individually.                    
                    exportItems.Where(ei => ei.Action != ExportQueueAction.All)
                               .ToList()
                               .ForEach(ei => 
                                {
                                    var attrib = allAttributes.Where(atrb => atrb.Id == ei.EntityKey).FirstOrDefault();
                                    InternalAttributeItem nopItem = null;
                                    if(attrib != null)
                                        nopItem = allNopAttributeValues.Where(nopAttrib => nopAttrib.Id == attrib.AttributeValueId).FirstOrDefault();

                                    bool importItem = true;
                                    if (attrib != null && string.IsNullOrEmpty(attrib.Code))
                                    {
                                        if (!string.IsNullOrEmpty(attrib.SynchronizedCode))
                                        {
                                            itemsToImport.Add(new InternalAttributeToProcess()
                                            {
                                                Code = attrib.SynchronizedCode,
                                                AttributeId = attrib.Id,
                                                MappingItem = attrib,
                                                IsDelete = true
                                            });
                                        }

                                        importItem = false;
                                    }

                                    if (importItem)
                                    {
                                        itemsToImport.Add(
                                            new InternalAttributeToProcess()
                                            {
                                                DisplayName = nopItem != null ? nopItem.Name : string.Empty,
                                                IsDelete = (ei.Action == ExportQueueAction.Delete),
                                                Code = attrib != null ? attrib.Code : ei.EntityCode,
                                                AttributeId = attrib != null ? attrib.Id : 0,
                                                MappingItem = attrib
                                            });
                                    }
                                });
                }
                else
                {
                    // Deal with updating 'all' items.
                    if (exportItems.Any(ei => ei.Action == ExportQueueAction.All))
                    {
                        itemsToImport = (from nopItem in allNopAttributeValues
                                         join attrib in allAttributes
                                         on nopItem.Id equals attrib.AttributeValueId
                                         where !string.IsNullOrEmpty(attrib.Code)
                                         select new InternalAttributeToProcess() { Code = attrib.Code, DisplayName = nopItem.Name, AttributeId = attrib.Id, MappingItem = attrib }).ToList();

                        // We're synchronizing all items - but the list above excludes any items where the code is empty - so look for
                        //  items we've previously synchronized, that no longer have a code, and delete them.
                        allAttributes.Where(a => string.IsNullOrEmpty(a.Code) && !string.IsNullOrEmpty(a.SynchronizedCode))
                                     .ToList()
                                     .ForEach(a =>
                                     {
                                         itemsToImport.Add(new InternalAttributeToProcess()
                                         {
                                             Code = a.SynchronizedCode,
                                             AttributeId = a.Id,
                                             MappingItem = a,
                                             IsDelete = true
                                         });
                                     });
                    }
                }

                if (itemsToImport != null && itemsToImport.Count() > 0)
                {
                    itemsToImport.ToList().ForEach(iti =>
                    {
                        //var attrib = allAttributes.Where(atrb => atrb.Id == ei.EntityKey).FirstOrDefault();
                        if (iti.MappingItem != null
                            && !string.IsNullOrEmpty(iti.MappingItem.SynchronizedCode)
                            && string.Compare(iti.MappingItem.Code, iti.MappingItem.SynchronizedCode, true) != 0)
                        {                            
                            // The code has changed - so add an import item to delete the old one.
                            if (!useHierarchyImport)
                                AddImportItem(attribValuesImportRequest, iti.MappingItem.SynchronizedCode, string.Empty, true);
                            else
                                AddHierarchyImportItem(hierarchyImportRequest, iti.MappingItem.SynchronizedCode, string.Empty, true);
                        }

                        if (!useHierarchyImport)
                            AddImportItem(attribValuesImportRequest, iti.Code, iti.DisplayName, iti.IsDelete);
                        else
                            AddHierarchyImportItem(hierarchyImportRequest, iti.Code, iti.DisplayName, iti.IsDelete);

                        attribsToUpdate.Add(iti.AttributeId);
                    });
                }
            }

            if ((!useHierarchyImport && (attribValuesImportRequest.Items == null || attribValuesImportRequest.Items.Count == 0))
                || (useHierarchyImport && (hierarchyImportRequest.Items == null || hierarchyImportRequest.Items.Count == 0)))
            {
                result = new AttributeValuesImportResponse();
                result.Summary = new ImportResponseSummary() { ProcessedSuccessfully = true };
            }
            else
            {
                if (!useHierarchyImport)
                    result = _qixolPromoUtilities.ImportAttributesToPromoService(attribValuesImportRequest);
                else
                    result = _qixolPromoUtilities.ImportHierarchyTopromoService(hierarchyImportRequest);                    
                  
                if (result != null && result.Summary != null && result.Summary.ProcessedSuccessfully)
                    FlagAttributeAsSynchonized(attribsToUpdate);
            }

            return result;
        }

        #endregion

        #region Checkout Attribute Handling

        private ProductImportResponse ProcessCheckoutAttributes(List<ExportQueueItem> checkoutAttributesToProcess)
        {
            var attribsToUpdate = new List<int>();
            ProductImportResponse result = null;
            List<KeyValuePair<int, bool>> caIdsToProcess = new List<KeyValuePair<int, bool>>();

            // Build a list of the product Ids to process
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    caIdsToProcess.AddRange(
                        checkoutAttributesToProcess.Where(p => p.Action != ExportQueueAction.All)
                                                   .Select(p => new KeyValuePair<int, bool>(p.EntityKey, p.Action == ExportQueueAction.Delete))
                                                   .ToList());
                }
                else
                {
                    // In theory, if we have an 'All' action, the only other items in the queue should be deletes.
                    if (checkoutAttributesToProcess.Any(ei => ei.Action == ExportQueueAction.All))
                    {
                        var allCheckoutAttributes = _attributeValueService.RetrieveAllForAttribute(EntityAttributeName.CheckoutAttribute);
                        caIdsToProcess.AddRange(allCheckoutAttributes.ToList().Select(p => new KeyValuePair<int, bool>(p.Id, false)).ToList());
                    }
                }
            }

            if (caIdsToProcess.Count > 0)
            {
                int batch = 0;
                var batchCAIds = caIdsToProcess.Skip(batch).Take(_promoSettings.BatchSize);
                List<ProductImportResponse> allBatchResults = new List<ProductImportResponse>();
                bool commsError = false;

                while (batchCAIds.Count() > 0)
                {
                    var importRequest = BuildBaseProductImport();
                    var productsToAdd = new List<ProductImportRequestItem>();
                    var productMappings = new List<InternalProductVariantItem>();

                    // For all product Ids, get the import details.           
                    batchCAIds.ToList().ForEach(p =>
                    {
                        InternalProductImportDetails importDetails = null;
                        if (p.Value)       // Meaning DELETED
                            importDetails = GetDeletedCheckoutAttributeProduct(p.Key);
                        else
                        {
                            importDetails = GetCheckoutAttributeProduct(p.Key);
                            if(importDetails != null)
                                attribsToUpdate.Add(p.Key);
                        }

                        if (importDetails != null)
                        {
                            if (importDetails.ImportProducts != null)
                                productsToAdd.AddRange(importDetails.ImportProducts);
                            if (importDetails.VariantItems != null)
                                productMappings.AddRange(importDetails.VariantItems);
                        }
                    });

                    importRequest.Products.AddRange(productsToAdd);

                    // Send to Promo
                    var batchResult = _qixolPromoUtilities.ImportProductsToPromoService(importRequest);

                    if (batchResult != null)
                        allBatchResults.Add(batchResult);

                    if (batchResult != null && batchResult.Summary != null && batchResult.Summary.ProcessedSuccessfully)
                    {
                        // Now update the variant mappings...
                        productMappings.ForEach(v =>
                        {
                            if (v.Delete)
                            {
                                if (v.ExistingItem != null)
                                    _productMappingService.Delete(v.ExistingItem);
                            }
                            else
                            {
                                _productMappingService.Insert(new ProductMappingItem()
                                {
                                    AttributesXml = v.AttributesXml,
                                    EntityName = EntityAttributeName.CheckoutAttribute,
                                    EntityId = v.ProductId,
                                    VariantCode = v.VariantCode,
                                    NoVariants = v.NoVariants
                                });
                            }
                        });

                        batch += _promoSettings.BatchSize;
                        batchCAIds = caIdsToProcess.Skip(batch).Take(_promoSettings.BatchSize);
                    }
                    else
                    {
                        // This should not happen!!
                        commsError = true;
                        break;
                    }
                }

                if (!commsError)
                {
                    result = new ProductImportResponse();
                    result.Summary = new ImportResponseSummary() { ProcessedSuccessfully = allBatchResults.Where(br => br.Summary != null && !br.Summary.ProcessedSuccessfully).Count() == 0 };
                    result.Summary.Messages = new List<ImportResponseMessage>();

                    allBatchResults.ForEach(br =>
                    {
                        //result.Summary.Messages.Add(new ImportResponseMessage() { Code = br.Reference, Message = br.GetMessages() });
                    });

                    if(result.Summary.ProcessedSuccessfully)
                        FlagAttributeAsSynchonized(attribsToUpdate);
                }
            }
            else
            {
                result = new ProductImportResponse();
                result.Summary = new ImportResponseSummary() { ProcessedSuccessfully = true };
            }

            return result;
        }

        private InternalProductImportDetails GetDeletedCheckoutAttributeProduct(int checkoutAttributeId)
        {
            var mappingItem = _attributeValueService.RetrieveById(checkoutAttributeId);
            if (mappingItem == null)
                return null;

            var currentProductConfig = _productMappingService.RetrieveAllVariantsByProductId(mappingItem.AttributeValueId, EntityAttributeName.CheckoutAttribute, false).ToList();
            if (currentProductConfig == null || currentProductConfig.Count == 0)
                return null;

            var returnItems = new List<ProductImportRequestItem>();
            var returnVariantDetails = new List<InternalProductVariantItem>();
            currentProductConfig.ForEach(cpc =>
                {
                    returnItems.Add(new ProductImportRequestItem()
                    {
                        ProductCode = mappingItem.Code,
                        VariantCode = cpc.VariantCode,
                        Deleted = true
                    });

                    returnVariantDetails.Add(new InternalProductVariantItem()
                    {
                        ProductId = cpc.EntityId,
                        AttributesXml = cpc.AttributesXml,
                        VariantCode = cpc.VariantCode,
                        Delete = true,
                        ExistingItem = cpc
                    });
                });

            return new InternalProductImportDetails()
            {
                ImportProducts = returnItems,
                VariantItems = returnVariantDetails
            };
        }

        private InternalProductImportDetails GetCheckoutAttributeProduct(int checkoutAttributeId)
        {
            var mappingItem = _attributeValueService.RetrieveById(checkoutAttributeId);
            if (mappingItem == null)
                return null;

            var checkoutAttrib = _checkoutAttributeService.GetCheckoutAttributeById(mappingItem.AttributeValueId);
            if (checkoutAttrib == null)
                return null;

            var returnItems = new List<ProductImportRequestItem>();
            var returnVariantDetails = new List<InternalProductVariantItem>();
            var currentProductConfig = _productMappingService.RetrieveAllVariantsByProductId(checkoutAttrib.Id, EntityAttributeName.CheckoutAttribute, false).ToList();

            if (!string.IsNullOrEmpty(mappingItem.SynchronizedCode) 
                    && string.Compare(mappingItem.Code, mappingItem.SynchronizedCode, true) != 0 
                    && currentProductConfig != null
                    && currentProductConfig.Count > 0)
            {
                // We have previously synchronized a code, and we have a new code to sync, so we need to remove the old ones first...
                returnItems.AddRange(
                    currentProductConfig.Select(p => new ProductImportRequestItem()
                                                          {
                                                               ProductCode = mappingItem.SynchronizedCode,
                                                               VariantCode = p.VariantCode,
                                                               Deleted = true
                                                          }).ToList());
            }

            if (!string.IsNullOrEmpty(mappingItem.Code))
            {
                var productsToImport = checkoutAttrib.ToQixolPromosImport(mappingItem);
                if (productsToImport == null || productsToImport.Count == 0)
                    return null;

                if (productsToImport.Any(p => string.IsNullOrEmpty(p.VariantCode)))
                {
                    // This item does not have any variants.
                    if (currentProductConfig != null && currentProductConfig.Count > 0 && currentProductConfig.Any(cpc => cpc.NoVariants == false))
                    {
                        // We have previously sent variants for this product, but there don't appeart to be any now...
                        currentProductConfig.Where(cpc => !cpc.NoVariants)
                                            .ToList()
                                            .ForEach(cpc =>
                                            {
                                                returnItems.Add(new ProductImportRequestItem()
                                                {
                                                    ProductCode = mappingItem.Code,
                                                    VariantCode = cpc.VariantCode,
                                                    Deleted = true
                                                });

                                                returnVariantDetails.Add(new InternalProductVariantItem()
                                                {
                                                    ProductId = cpc.EntityId,
                                                    AttributesXml = cpc.AttributesXml,
                                                    VariantCode = cpc.VariantCode,
                                                    Delete = true,
                                                    ExistingItem = cpc
                                                });
                                            });
                    }
                    returnItems.AddRange(productsToImport);

                    var existingConfigItem = currentProductConfig.Where(cpc => cpc.NoVariants).FirstOrDefault();
                    if (existingConfigItem == null)
                        returnVariantDetails.Add(new InternalProductVariantItem()
                        {
                            ProductId = checkoutAttrib.Id,
                            NoVariants = true
                        });
                }
                else
                {
                    // Firstly deal with deleted variants.
                    currentProductConfig.ForEach(cpc =>
                    {
                        var foundItem = productsToImport.Where(p => p.VariantCode == cpc.VariantCode).FirstOrDefault();
                        if (foundItem == null)
                        {
                            // This variant used to exist, but no longer does!  so create a delete for this particular variant.
                            returnItems.Add(new ProductImportRequestItem()
                            {
                                ProductCode = mappingItem.Code,
                                VariantCode = cpc.VariantCode,
                                Deleted = true
                            });

                            // Add an entry indicating we need to delete this item.
                            returnVariantDetails.Add(new InternalProductVariantItem()
                            {
                                AttributesXml = cpc.AttributesXml,
                                VariantCode = cpc.VariantCode,
                                ProductId = cpc.EntityId,
                                Delete = true,
                                ExistingItem = cpc
                            });
                        }
                    });

                    productsToImport.ForEach(p =>
                    {
                        var productVariantItem = new InternalProductVariantItem()
                        {
                            AttributesXml = string.Empty,
                            ProductId = checkoutAttrib.Id,
                            VariantCode = p.VariantCode
                        };

                        var existingItem = currentProductConfig.Where(cpc => cpc.VariantCode == p.VariantCode).FirstOrDefault();
                        if (existingItem == null)
                            productVariantItem.ExistingItem = existingItem;

                        returnVariantDetails.Add(productVariantItem);
                    });

                    returnItems.AddRange(productsToImport);
                }
            }

            return new InternalProductImportDetails()
            {
                ImportProducts = returnItems,
                VariantItems = returnVariantDetails
            };
        }

        #endregion

        #region Product Handling

        private ProductImportResponse ProcessProducts(List<ExportQueueItem> productsToProcess, List<ProductAttributeConfigItem> productAttributeConfigItems, List<Vendor> vendors, List<TaxCategory> taxCategories)
        {
            ProductImportResponse result = null;           
            List<int> productIdsToProcess = new List<int>();

            // Build a list of the product Ids to process
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    productIdsToProcess.AddRange(
                        productsToProcess.Where(p => p.Action != ExportQueueAction.All)
                                         .Select(p => p.EntityKey)
                                         .ToList());
                }
                else
                {
                    // In theory, if we have an 'All' action, the only other items in the queue should be deletes.
                    if (productsToProcess.Any(ei => ei.Action == ExportQueueAction.All))
                    {
                        var allProducts = _productService.SearchProducts(productType: ProductType.SimpleProduct, storeId: _promoSettings.StoreId);
                        productIdsToProcess.AddRange(
                                allProducts.Select(p => p.Id)
                                           .ToList());
                    }
                }
            }

            if (productIdsToProcess.Count > 0)
            {
                int batch = 0;
                var batchProductIds = productIdsToProcess.Skip(batch).Take(_promoSettings.BatchSize);
                List<ProductImportResponse> allBatchResults = new List<ProductImportResponse>();
                bool commsError = false;

                while (batchProductIds.Count() > 0)
                {
                    var importRequest = BuildBaseProductImport();
                    var productsToAdd = new List<ProductImportRequestItem>();
                    var productMappings = new List<InternalProductVariantItem>();

                    // For all product Ids, get the import details.           
                    batchProductIds.ToList().ForEach(p =>
                    {
                        var importDetails = GetImportProduct(p, productAttributeConfigItems, vendors, taxCategories);
                        if (importDetails != null)
                        {
                            productsToAdd.AddRange(importDetails.ImportProducts);
                            productMappings.AddRange(importDetails.VariantItems);
                        }
                    });

                    importRequest.Products.AddRange(productsToAdd);

                    // Send to Promo
                    var batchResult = _qixolPromoUtilities.ImportProductsToPromoService(importRequest);

                    if(batchResult != null)
                        allBatchResults.Add(batchResult);

                    if (batchResult != null && batchResult.Summary != null && batchResult.Summary.ProcessedSuccessfully)
                    {
                        // Now update the variant mappings...
                        productMappings.ForEach(v =>
                        {
                            if (v.Delete)
                            {
                                if (v.ExistingItem != null)
                                    _productMappingService.Delete(v.ExistingItem);
                            }
                            else
                            {
                                _productMappingService.Insert(new ProductMappingItem()
                                {
                                    AttributesXml = v.AttributesXml,
                                    EntityName = EntityAttributeName.Product,
                                    EntityId = v.ProductId,
                                    VariantCode = v.VariantCode,
                                    NoVariants = v.NoVariants
                                });
                            }
                        });

                        batch += _promoSettings.BatchSize;
                        batchProductIds = productIdsToProcess.Skip(batch).Take(_promoSettings.BatchSize);
                    }
                    else
                    {
                        // This should not happen!!
                        commsError = true;
                        break;
                    }
                }

                if (!commsError)
                {
                    result = new ProductImportResponse();
                    result.Summary = new ImportResponseSummary() { ProcessedSuccessfully = allBatchResults.Where(br => br.Summary != null && !br.Summary.ProcessedSuccessfully).Count() == 0 };
                    result.Summary.Messages = new List<ImportResponseMessage>();

                    allBatchResults.ForEach(br =>
                    {
                        //result.Summary.Messages.Add(new ImportResponseMessage() { Code = br.Reference, Message = br.GetMessages() });
                    });
                }
            }
            else
            {
                result = new ProductImportResponse();
                result.Summary = new ImportResponseSummary() { ProcessedSuccessfully = true };
            }

            return result;    
        }

        private InternalProductImportDetails GetImportProduct(int productId, List<ProductAttributeConfigItem> productAttributeConfigItems, List<Vendor> vendors, List<TaxCategory> taxCategories)
        {
            var productDetails = _productService.GetProductById(productId);
            if (productDetails.ProductType == ProductType.GroupedProduct)
                return null;
            
            var returnItems = new List<ProductImportRequestItem>();
            var returnVariantDetails = new List<InternalProductVariantItem>();
            var currentProductConfig = _productMappingService.RetrieveAllVariantsByProductId(productId, EntityAttributeName.Product, false).ToList();

            // Get the base product details to be imported.
            var baseImportProduct = productDetails.ToQixolPromosImport(productAttributeConfigItems, vendors, taxCategories);

            if (productDetails.ParentGroupedProductId > 0)
            {
                // This item is a grouped item, so we will need to combine some details from the Group Parent item.
                var groupParentProduct = _productService.GetProductById(productDetails.ParentGroupedProductId);
                var groupedImportProduct = groupParentProduct.ToQixolPromosImport(productAttributeConfigItems, vendors, taxCategories);
                if (groupedImportProduct.Attributes != null && groupedImportProduct.Attributes.Count > 0)
                {                    
                    baseImportProduct.Attributes.AddRange(
                                groupedImportProduct.Attributes
                                                    .Where(ga => 
                                                            !baseImportProduct.Attributes
                                                                              .Any(ba => string.Compare(ga.Name, ba.Name, true) == 0 
                                                                                            && string.Compare(ga.Value, ba.Value, true) == 0))
                                                    .ToList());

                }
            }

            int attributeMappingsWithValues = 0;
            if (productDetails.ProductAttributeMappings != null)
                attributeMappingsWithValues = productDetails.ProductAttributeMappings.Where(pam => pam.ProductAttributeValues != null && pam.ProductAttributeValues.Count > 0).Count();

            if (attributeMappingsWithValues == 0
                || ((_promoSettings.MaximumAttributesForVariants > 0)
                    && (attributeMappingsWithValues > _promoSettings.MaximumAttributesForVariants)
                    && (productDetails.ProductAttributeCombinations == null || productDetails.ProductAttributeCombinations.Count == 0)))
            {
                // This product does not have any variants.
                if (currentProductConfig != null && currentProductConfig.Count > 0 && currentProductConfig.Any(cpc => cpc.NoVariants == false))
                {
                    // We have previously sent variants for this product, but there don't appeart to be any now...
                    currentProductConfig.Where(cpc => !cpc.NoVariants)
                                        .ToList()
                                        .ForEach(cpc =>
                        {
                            returnItems.Add(new ProductImportRequestItem()
                                {

                                    ProductCode = productDetails.Id.ToString(),
                                    VariantCode = cpc.VariantCode,
                                    Deleted= true                                    
                                });

                            returnVariantDetails.Add(new InternalProductVariantItem()
                                {
                                    ProductId = cpc.EntityId,
                                    AttributesXml = cpc.AttributesXml,
                                    VariantCode = cpc.VariantCode,
                                    Delete = true,
                                    ExistingItem = cpc
                                });
                        });
                }

                returnItems.Add(baseImportProduct);

                var existingConfigItem = currentProductConfig.Where(cpc => cpc.NoVariants).FirstOrDefault();
                if (productDetails.Deleted)
                {
                    if (existingConfigItem != null)
                        returnVariantDetails.Add(new InternalProductVariantItem()
                            {
                                ProductId = existingConfigItem.EntityId,
                                ExistingItem = existingConfigItem,
                                Delete = true
                            });
                }
                else
                {
                    if (existingConfigItem == null)
                        returnVariantDetails.Add(new InternalProductVariantItem()
                            {
                                ProductId = productDetails.Id,
                                NoVariants = true
                            });
                }
            }
            else
            {
                // This product has variants.
                var allCombinations = new List<InternalProductVariantItem>();
                if (productDetails.ProductAttributeCombinations == null || productDetails.ProductAttributeCombinations.Count == 0)
                {
                    // the product has no defined attribute combinations, which means we need to generate a list of the possible combinations that we can use...
                    var generatedCombinations = _productAttributeParser.GenerateAllCombinations(productDetails);
                    allCombinations = generatedCombinations.Select(gc => new InternalProductVariantItem()
                        {
                            ProductId = productDetails.Id,
                            AttributesXml = gc,
                            Generated = true
                        }).ToList();
                }
                else
                {
                    // The product has a defined list of attribute combinations, so use them....
                    allCombinations = productDetails.ProductAttributeCombinations.Select(pac =>
                        new InternalProductVariantItem()
                        {
                            AttributesXml = pac.AttributesXml,
                            ProductId = pac.ProductId,
                            ProductAttributeCombinationId = pac.Id,
                            Generated = false
                        }).ToList();
                }

                // Firstly deal with deleted combinations.
                currentProductConfig.ForEach(cpc =>
                    {
                        var foundCombination = allCombinations.Where(c => c.ProductId == cpc.EntityId && string.Compare(cpc.AttributesXml, c.AttributesXml, true) == 0).FirstOrDefault();
                        if (foundCombination == null)
                        {
                            // This combination used to exist, but no longer does!  so create a delete for this particular variant.
                            returnItems.Add(new ProductImportRequestItem()
                                {
                                    ProductCode = productDetails.Id.ToString(),
                                    VariantCode = cpc.VariantCode,                                  
                                    Deleted = true
                                });

                            // Add an entry indicating we need to delete this item.
                            returnVariantDetails.Add(new InternalProductVariantItem()
                                                            {
                                                                AttributesXml = cpc.AttributesXml,
                                                                VariantCode = cpc.VariantCode,
                                                                ProductId = cpc.EntityId,
                                                                Delete = true,
                                                                ExistingItem = cpc
                                                            });
                        }
                    });

                // Now look for which variants to send...
                allCombinations.ForEach(c =>
                    {                       
                        var existingItemCheck = currentProductConfig.Where(cpc => cpc.EntityId == c.ProductId && string.Compare(cpc.AttributesXml, c.AttributesXml, true) == 0).FirstOrDefault();
                        var productAttribValues = _productAttributeParser.ParseProductAttributeValues(c.AttributesXml);

                        if (existingItemCheck == null)
                        {
                            // We need to figure out what the variant code will be for this item...
                            string variantCode = string.Empty;
                            productAttribValues.ToList()
                                               .ForEach(pav =>
                                                   {
                                                       string attributeName = pav.ProductAttributeMapping.ProductAttribute.Name;
                                                       string attributeValue = pav.Name;

                                                       variantCode = string.Concat(variantCode,
                                                                                   string.Format(_promoSettings.VariantAttributeFormat, attributeName, attributeValue),
                                                                                   _promoSettings.VariantAttributesSeperator);
                                                   });
                            if (variantCode.EndsWith(_promoSettings.VariantAttributesSeperator))
                                variantCode = variantCode.Substring(0, variantCode.Length - _promoSettings.VariantAttributesSeperator.Length);

                            c.VariantCode = variantCode;

                            returnVariantDetails.Add(c);
                        }
                        else
                        {
                            c.VariantCode = existingItemCheck.VariantCode;
                        }

                        var variantImportProduct = baseImportProduct.Clone();
                        if (!variantImportProduct.Deleted)
                        {
                            decimal totalPriceAdjustment = 0;
                            var importAttributes = variantImportProduct.Attributes != null ? variantImportProduct.Attributes : new List<ProductImportRequestAttributeItem>();
                            productAttribValues.ToList()
                                               .ForEach(pav =>
                                               {
                                                   totalPriceAdjustment += pav.PriceAdjustment;
                                                   string attributeName = pav.ProductAttributeMapping.ProductAttribute.Name;
                                                   string attributeValue = pav.Name;
                                                   importAttributes.Add(new ProductImportRequestAttributeItem()
                                                   {
                                                       Name = attributeName,
                                                       Value = attributeValue
                                                   });
                                               });

                            variantImportProduct.Attributes = new List<ProductImportRequestAttributeItem>();
                            variantImportProduct.Attributes.AddRange(importAttributes);
                            variantImportProduct.Price += totalPriceAdjustment;
                        }

                        variantImportProduct.VariantCode = c.VariantCode;
                        returnItems.Add(variantImportProduct);
                    });

            }

            return new InternalProductImportDetails()
            {
                ImportProducts = returnItems,
                VariantItems = returnVariantDetails
            };
        }

        #endregion

        #region Internal Helper Classes 

        private class InternalAttributeItem
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public InternalAttributeItem(int id, string name)
            {
                this.Id = id;
                this.Name = name;
            }
        }

        private class InternalAttributeToProcess
        {
            public bool IsDelete { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public int AttributeId { get; set; }
            public AttributeValueMappingItem MappingItem { get; set; }
        }

        private class InternalProductVariantItem
        {
            public int ProductId { get; set; }
            public string AttributesXml { get; set; }
            public bool Generated { get; set; }
            public int ProductAttributeCombinationId { get; set; }
            public string VariantCode { get; set; }
            public bool Delete { get; set; }
            public ProductMappingItem ExistingItem { get; set; }
            public bool NoVariants { get; set; }
        }

        private class InternalProductImportDetails
        {
            public List<ProductImportRequestItem> ImportProducts { get; set; }
            public List<InternalProductVariantItem> VariantItems { get; set; }
        }

        #endregion

    }
}