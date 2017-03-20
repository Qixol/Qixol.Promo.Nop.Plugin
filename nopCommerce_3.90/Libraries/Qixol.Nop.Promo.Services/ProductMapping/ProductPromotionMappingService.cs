using global::Nop.Core.Data;
using global::Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ProductMapping
{
    public class ProductPromoMappingService : IProductPromoMappingService
    {
        private readonly IRepository<ProductPromotionMapping> _repository;
        private readonly IEventPublisher _eventPublisher;

        public ProductPromoMappingService(IRepository<ProductPromotionMapping> repository,
                                              IEventPublisher eventPublisher)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
        }

        public void Insert(ProductPromotionMapping itemToInsert)
        {
            _repository.Insert(itemToInsert);
            _eventPublisher.EntityInserted<ProductPromotionMapping>(itemToInsert);
        }

        public void Delete(ProductPromotionMapping itemToDelete)
        {
            _repository.Delete(itemToDelete);
            _eventPublisher.EntityDeleted<ProductPromotionMapping>(itemToDelete);
        }

        public IQueryable<ProductPromotionMapping> RetrieveForProductMapping(int productMappingId)
        {
            return _repository.Table.Where(p => p.ProductMappingId == productMappingId);
        }

        public IQueryable<Qixol.Nop.Promo.Core.Domain.Products.ProductPromotionMapping> RetrieveForPromo(int promoId)
        {
            return _repository.Table.Where(p => p.PromotionId == promoId);
        }

        public IQueryable<ProductPromotionMapping> RetrieveForProductMappingsList(List<int> productMappingIds)
        {
            return _repository.Table.Where(p => productMappingIds.Contains(p.ProductMappingId));
        }
    }
}
