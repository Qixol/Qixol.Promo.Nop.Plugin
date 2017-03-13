using Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Qixol.Nop.Promo.Services.ProductMapping
{
    public interface IProductMappingService
    {

        ProductMappingItem RetrieveFromAttributesXml(Product product, string attributesXml);

        ProductMappingItem RetrieveFromVariantCode(int productId, string variantcode);

        ProductMappingItem RetrieveFromShoppingCartItem(ShoppingCartItem shoppingCartItem);

        IQueryable<ProductMappingItem> RetrieveAllVariantsByProductId(int sourceEntityId, string sourceEntityName, bool getGroupedProducts);

        void Insert(ProductMappingItem item);

        void Delete(ProductMappingItem item);

    }
}
