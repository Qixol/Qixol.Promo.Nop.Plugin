using Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ProductMapping
{
    public interface IProductMappingService
    {

        ProductMappingItem RetrieveFromAttributesXml(Product product, string attributesXml);

        ProductMappingItem RetrieveFromVariantCode(int productId, string variantcode);

        IQueryable<ProductMappingItem> RetrieveAllVariantsByProductId(int sourceEntityId, string sourceEntityName, bool getGroupedProducts);

        void Insert(ProductMappingItem item);

        void Delete(ProductMappingItem item);
    }
}
