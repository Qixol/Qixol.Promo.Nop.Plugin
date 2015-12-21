using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ProductMapping
{
    public interface IProductPromoMappingService
    {

        void Insert(ProductPromotionMapping itemToInsert);

        void Delete(ProductPromotionMapping itemToDelete);

        IQueryable<ProductPromotionMapping> RetrieveForProductMapping(int productMappingId);

        IQueryable<ProductPromotionMapping> RetrieveForProductMappingsList(List<int> productMappingIds);

        IQueryable<ProductPromotionMapping> RetrieveForPromo(int promoId);

    }
}
