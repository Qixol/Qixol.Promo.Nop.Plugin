using global::Nop.Core.Data;
using global::Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ProductAttributeConfig
{
    public class ProductAttributeConfigService : IProductAttributeConfigService
    {
        private readonly IRepository<ProductAttributeConfigItem> _repository;
        private readonly IEventPublisher _eventPublisher;

        public ProductAttributeConfigService(IRepository<ProductAttributeConfigItem> repository,
                                  IEventPublisher eventPublisher)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Insert a new ProductAttributeConfigItem 
        /// </summary>
        /// <param name="item"></param>
        public void Insert(ProductAttributeConfigItem item)
        {
            item.CreatedUtc = DateTime.UtcNow;
            item.UpdatedUtc = DateTime.UtcNow;

            _repository.Insert(item);
            _eventPublisher.EntityInserted<ProductAttributeConfigItem>(item);
        }

        /// <summary>
        /// Update an existing ProductAttributeConfigItem
        /// </summary>
        /// <param name="item"></param>
        public void Update(ProductAttributeConfigItem item)
        {
            item.UpdatedUtc = DateTime.UtcNow;
            _repository.Update(item);
            _eventPublisher.EntityUpdated<ProductAttributeConfigItem>(item);
        }

        /// <summary>
        /// Retrieve all ProductAttributeConfigItems
        /// </summary>
        /// <returns></returns>
        public IQueryable<ProductAttributeConfigItem> RetrieveAll()
        {
            return _repository.Table;
        }
    }
}
