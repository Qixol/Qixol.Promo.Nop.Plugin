using global::Nop.Core.Domain.Catalog;
using global::Nop.Services.Catalog;
using global::Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using global::Nop.Core.Domain.Tax;
using global::Nop.Core.Domain.Vendors;
using Qixol.Promo.Integration.Lib.Import;
using Nop.Core.Infrastructure;

namespace Qixol.Nop.Promo.Core.Domain.Products
{
    public static class ProductExtensions
    {
        public static ProductImportRequestItem ToQixolPromosImport(this Product sourceProduct, List<ProductAttributeConfigItem> productAttributeConfigItems, List<Vendor> vendors, List<TaxCategory> taxCategories)
        {
            //IProductService _productService = DependencyResolver.Current.GetService<IProductService>();
            //IPictureService _pictureService = DependencyResolver.Current.GetService<IPictureService>();
            //ICategoryService categoryService = DependencyResolver.Current.GetService<ICategoryService>();
            IProductService _productService = EngineContext.Current.Resolve<IProductService>();
            IPictureService _pictureService = EngineContext.Current.Resolve<IPictureService>();
            ICategoryService categoryService = EngineContext.Current.Resolve<ICategoryService>();

            List<Product> insertedProducts = new List<Product>();

            ProductImportRequestItem importProduct = new ProductImportRequestItem()
            {
                // Hotfix - 2016-10-17 - START
                //Barcode = string.IsNullOrEmpty(sourceProduct.Gtin) ? string.Empty : sourceProduct.Gtin,
                // Hotfix - 2016-10-17 - END
                Deleted = sourceProduct.Deleted,
                Description = sourceProduct.Name,
                Price = sourceProduct.Price,
                ProductCode = sourceProduct.Id.ToString()
            };

            if (!sourceProduct.Deleted)
            {
                var productAttributes = GetAttributesForProduct(sourceProduct, productAttributeConfigItems, vendors, taxCategories, categoryService);
                var defaultProductPicture = _pictureService.GetPicturesByProductId(sourceProduct.Id, 1).FirstOrDefault();
                importProduct.ImageUrl = _pictureService.GetPictureUrl(defaultProductPicture);
                if (productAttributes != null && productAttributes.Count > 0)
                    importProduct.Attributes.AddRange(productAttributes);
            }

            return importProduct;            
        }

        private static List<ProductImportRequestAttributeItem> GetAttributesForProduct(Product product, List<ProductAttributeConfigItem> productAttributeConfigItems, List<Vendor> vendors, List<TaxCategory> taxCategories, ICategoryService categoryService)
        {
            // Hotfix - 2016-10-17 - START
            var gtinAttributeValue = string.IsNullOrEmpty(product.Gtin) ? string.Empty : product.Gtin;
            // Hotfix - 2016-10-17 - END

            var configItems = productAttributeConfigItems.Where(paci => paci.Enabled).ToList();

            // Hotfix - 2016-10-17 - START
            // REMOVED CODE
            //if (configItems.Count == 0 && string.IsNullOrEmpty(gtinAttributeValue))
            //    return null;
            // ADDED CODE
            if (configItems.Count == 0 && string.IsNullOrEmpty(gtinAttributeValue))
                return null;
            // Hotfix - 2016-10-17 - END

            var returnList = new List<ProductImportRequestAttributeItem>();
            // Hotfix - 2016-10-17 - START
            if (!string.IsNullOrEmpty(gtinAttributeValue))
            {
                ProductImportRequestAttributeItem gtinAttributeItem = new ProductImportRequestAttributeItem()
                {
                    Name = ProductAttributeConfigSystemNames.GTIN,
                    Value = gtinAttributeValue
                };
                returnList.Add(gtinAttributeItem);
            }
            // Hotfix - 2016-10-17 - END
            configItems.ForEach(ci =>
                {
                    var attributeItems = GetValueForConfigItem(ci, product, vendors, taxCategories, categoryService);
                    if(attributeItems != null && attributeItems.Count > 0)
                        returnList.AddRange(attributeItems);
                });

            return returnList;
        }

        private static List<ProductImportRequestAttributeItem> GetValueForConfigItem(ProductAttributeConfigItem item, Product product, List<Vendor> vendors, List<TaxCategory> taxCategories, ICategoryService categoryService)
        {
            var returnItems = new List<ProductImportRequestAttributeItem>();
            switch (item.SystemName)
            {
                case ProductAttributeConfigSystemNames.AVAILABLE_FOR_PREORDER:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.AvailableForPreOrder.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.CALL_FOR_PRICE:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.CallForPrice.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.CATEGORY:
                    product.ProductCategories.ToList()
                                             .ForEach(pc =>
                                             {
                                                 returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = pc.Category.Name });
                                             });
                    break;

                case ProductAttributeConfigSystemNames.CATEGORY_BREADCRUMBS:
                    product.ProductCategories.ToList()
                                             .ForEach(pc =>
                                             {
                                                 var breadCrumb = pc.Category.GetFormattedBreadCrumb(categoryService);
                                                 returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = breadCrumb });
                                             });
                    break;
                case ProductAttributeConfigSystemNames.MANUFACTURER:
                    product.ProductManufacturers.ToList()
                                                .ForEach(pm =>
                                                {
                                                    if (pm.Manufacturer != null)
                                                        returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = pm.Manufacturer.Name });
                                                });
                    break;

                case ProductAttributeConfigSystemNames.CUSTOMER_ENTERS_PRICE:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.CustomerEntersPrice.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.DISABLE_BUY_BUTTON:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.DisableBuyButton.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.DOWNLOADABLE_PRODUCT:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsDownload.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.FREE_SHIPPING:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsFreeShipping.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.GIFT_CARD_TYPE:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsGiftCard ? product.GiftCardType.ToString() : string.Empty });
                    break;

                case ProductAttributeConfigSystemNames.GTIN:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.Gtin });
                    break;

                case ProductAttributeConfigSystemNames.IS_GIFT_CARD:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsGiftCard.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.IS_RENTAL:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsRental.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.MANUFACTURER_PART_NO:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.ManufacturerPartNumber });
                    break;

                case ProductAttributeConfigSystemNames.PRODUCT_SPECIFICATION_ATTRIBS:
                    product.ProductSpecificationAttributes.ToList()
                                                          .ForEach(pcs =>
                                                              {
                                                                  returnItems.Add(new ProductImportRequestAttributeItem()
                                                                      {
                                                                          Name = pcs.SpecificationAttributeOption.SpecificationAttribute.Name,
                                                                          Value = !string.IsNullOrEmpty(pcs.CustomValue) ? pcs.CustomValue : pcs.SpecificationAttributeOption.Name
                                                                      });                                                                  
                                                              });
                    break;

                case ProductAttributeConfigSystemNames.SHIP_SEPARATELY:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.ShipSeparately.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.SHIPPING_ENABLED:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsShipEnabled.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.SKU:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.Sku });
                    break;

                case ProductAttributeConfigSystemNames.TAX_CATEGORY:
                    var taxCategory = taxCategories.Where(tc => tc.Id == product.TaxCategoryId).FirstOrDefault();
                    if (taxCategory != null)
                        returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = taxCategory.Name });
                    break;

                case ProductAttributeConfigSystemNames.TAX_EXCEMPT:
                    returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = product.IsTaxExempt.ToString() });
                    break;

                case ProductAttributeConfigSystemNames.VENDOR:
                    var vendor = vendors.Where(v => v.Id == product.VendorId).FirstOrDefault();
                    if (vendor != null)
                        returnItems.Add(new ProductImportRequestAttributeItem() { Name = item.SystemName, Value = vendor.Name });
                    break;

                default:
                    break;
            }

            return returnItems;
        }
    }
}
