using global::Qixol.Promo.Integration.Lib;
using global::Qixol.Promo.Integration.Lib.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Products
{
    public static class ImportProductExtensions
    {

        /// <summary>
        /// Create a copy of the importProduct.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ProductImportRequestItem Clone(this ProductImportRequestItem source)
        {
            var returnItem = new ProductImportRequestItem()
            {
                Barcode = source.Barcode,
                Deleted = source.Deleted,
                Description = source.Description,
                ImageUrl = source.ImageUrl,
                Price = source.Price,
                ProductCode = source.ProductCode,
                VariantCode = source.VariantCode
            };

            if (source.Attributes != null && source.Attributes.Count > 0)
            {
                var clonedAttributes = new List<ProductImportRequestAttributeItem>();
                source.Attributes.ToList()
                                 .ForEach(sa =>
                                     {
                                         clonedAttributes.Add(new ProductImportRequestAttributeItem()
                                             {
                                                 Name = sa.Name,
                                                 Value = sa.Value
                                             });
                                     });
                returnItem.Attributes.AddRange(clonedAttributes);
            }

            return returnItem;
        }

        public static string GetResponseMessages<T>(this ImportResponse<T> response) where T : ImportResponseItemBase
        {
            if (response == null || response.Summary == null || response.Summary.Messages == null || response.Summary.Messages.Count == 0)
                return string.Empty;

            var strBuilder = new StringBuilder();
            response.Summary.Messages.ForEach(msg =>
                {
                    strBuilder.AppendLine(string.Format("{0} - {1}", msg.Code, msg.Message));
                });

            return strBuilder.ToString();
        }

        
    }
}
