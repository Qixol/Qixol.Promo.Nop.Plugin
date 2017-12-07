using global::Nop.Core.Data;
using global::Nop.Services.Events;
using Nop.Core.Caching;
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
        #region constants

        private const string PRODUCTATTRIBUTECONFIGITEMS_ALL_KEY = "Qixol.Promo.ProductAttributeConfigItems.All";
        private const string PRODUCTATTRIBUTECONFIGITEMS_PATTERN_KEY = "Qixol.Promo.ProductAttributeConfigItems";

        #endregion

        #region fields

        private readonly IRepository<ProductAttributeConfigItem> _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region constructor

        public ProductAttributeConfigService(IRepository<ProductAttributeConfigItem> repository,
                                  IEventPublisher eventPublisher, ICacheManager cacheManager)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
            this._cacheManager = cacheManager;
        }

        #endregion

        #region methods

        /// <summary>
        /// Insert a new ProductAttributeConfigItem 
        /// </summary>
        /// <param name="item"></param>
        public void Insert(ProductAttributeConfigItem item)
        {
            item.CreatedUtc = DateTime.UtcNow;
            item.UpdatedUtc = DateTime.UtcNow;

            _repository.Insert(item);
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTECONFIGITEMS_PATTERN_KEY);
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
            _cacheManager.RemoveByPattern(PRODUCTATTRIBUTECONFIGITEMS_PATTERN_KEY);
            _eventPublisher.EntityUpdated<ProductAttributeConfigItem>(item);
        }

        /// <summary>
        /// Retrieve all ProductAttributeConfigItems
        /// </summary>
        /// <returns></returns>
        public IList<ProductAttributeConfigItem> GetAllProductAttributeConfigItems()
        {
            string key = PRODUCTATTRIBUTECONFIGITEMS_ALL_KEY;

            return _cacheManager.Get(key, () =>
            {
                return _repository.Table.ToList();
            });
        }

        #endregion
    }
}
