using global::Nop.Core.Domain.Orders;
using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Promo.Integration.Lib.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Core.Infrastructure;

namespace Qixol.Nop.Promo.Services.Orders
{
    public static class CheckoutAttributeExtensions
    {
        /// <summary>
        /// Build the products to be synchronized for the Checkout attribute.  Based on the configuration of the attribute, this could result in multiple products,
        /// one variant for each checkout attribute value.
        /// </summary>
        /// <param name="checkoutAttribute"></param>
        /// <param name="attribMappingItem"></param>
        /// <param name="isFirst"></param>
        /// <returns></returns>
        public static List<ProductImportRequestItem> ToQixolPromosImport(this CheckoutAttribute checkoutAttribute, AttributeValueMappingItem attribMappingItem)
        {
            IPictureService _pictureService = EngineContext.Current.Resolve<IPictureService>();
            //IPictureService _pictureService = DependencyResolver.Current.GetService<IPictureService>();

            var returnItems = new List<ProductImportRequestItem>();
            var baseProduct = new ProductImportRequestItem()
            {
                Description = checkoutAttribute.Name,
                ProductCode = attribMappingItem.Code,
                ImageUrl = _pictureService.GetDefaultPictureUrl()
            };

            baseProduct.Attributes.Add(new ProductImportRequestAttributeItem() { Name = "ischeckoutattribute", Value = "true" });
            baseProduct.Attributes.Add(new ProductImportRequestAttributeItem() { Name = "checkoutattributename", Value = checkoutAttribute.Name });

            switch (checkoutAttribute.AttributeControlType)
            {
                case global::Nop.Core.Domain.Catalog.AttributeControlType.ColorSquares:
                case global::Nop.Core.Domain.Catalog.AttributeControlType.DropdownList:
                case global::Nop.Core.Domain.Catalog.AttributeControlType.RadioList:
                    // All of the control types allow the user to select a value (potentially with a price associated) - so we'll need to create a variant for each.
                    // NOTE:  Not coping with 'Checkboxes' where we would potentially have to generate all permutations of those checkboxes.  Roadmap item (to be confirmed).
                    if (checkoutAttribute.CheckoutAttributeValues != null && checkoutAttribute.CheckoutAttributeValues.Count > 0)
                    {
                        // The checkout attribute values do not have to be unique...
                        List<string> usedVariantCodes = new List<string>();
                        checkoutAttribute.CheckoutAttributeValues.ToList()
                                                                 .ForEach(cav =>
                                                                 {
                                                                     var productVariant = baseProduct.Clone();
                                                                     productVariant.VariantCode = cav.Id.ToString();
                                                                     productVariant.Description += string.Concat(" - ", cav.Name);
                                                                     productVariant.Price = cav.PriceAdjustment > 0 ? cav.PriceAdjustment : 0;
                                                                     productVariant.Attributes.Add(new ProductImportRequestAttributeItem() { Name = "checkoutattributevalue", Value = cav.Name });
                                                                     returnItems.Add(productVariant);
                                                                 });
                    }
                    else
                    {
                        // The control type indicates there should be values - but there aren't!  so just create the base product.
                        returnItems.Add(baseProduct);
                    }

                    break;
                default:
                    // We're not using variants - so just return the basic product.
                    returnItems.Add(baseProduct);
                    break;
            }

            return returnItems;
        }

        public static string ProductCode(this CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                return null;

            IAttributeValueService attributeValueService = DependencyResolver.Current.GetService<IAttributeValueService>();

            Qixol.Nop.Promo.Core.Domain.AttributeValues.AttributeValueMappingItem integrationMappingItem = attributeValueService.Retrieve(checkoutAttribute.Id, EntityAttributeName.CheckoutAttribute);
            if (integrationMappingItem != null && !string.IsNullOrEmpty(integrationMappingItem.Code))
                return integrationMappingItem.Code;

            return null;
        }
    }
}
