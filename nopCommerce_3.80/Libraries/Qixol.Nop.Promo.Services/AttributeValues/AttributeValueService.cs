using global::Nop.Core.Data;
using global::Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.AttributeValues
{
    public class AttributeValueService : IAttributeValueService
    {
        private readonly IRepository<AttributeValueMappingItem> _repository;
        private readonly IEventPublisher _eventPublisher;

        public AttributeValueService(IRepository<AttributeValueMappingItem> repository,
                                  IEventPublisher eventPublisher)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
        }

        public void Insert(AttributeValueMappingItem itemToInsert)
        {
            itemToInsert.CreatedOnUtc = DateTime.UtcNow;
            this._repository.Insert(itemToInsert);
            this._eventPublisher.EntityInserted<AttributeValueMappingItem>(itemToInsert);
        }

        public void Update(AttributeValueMappingItem itemToUpdate, bool publishEvents = true)
        {
            this._repository.Update(itemToUpdate);
            if (publishEvents)
                this._eventPublisher.EntityUpdated<AttributeValueMappingItem>(itemToUpdate);
        }

        public void Delete(AttributeValueMappingItem itemToDelete)
        {
            this._repository.Delete(itemToDelete);
            this._eventPublisher.EntityDeleted<AttributeValueMappingItem>(itemToDelete);
        }

        public AttributeValueMappingItem Retrieve(int entityAttributeValueId, string entityAttributeName)
        {
            return this._repository.Table.Where(eavm => eavm.AttributeName == entityAttributeName && eavm.AttributeValueId == entityAttributeValueId).FirstOrDefault();
        }

        public IQueryable<AttributeValueMappingItem> RetrieveAllForAttribute(string entityAttributeName)
        {
            return this._repository.Table.Where(eavm => eavm.AttributeName == entityAttributeName);
        }

        public AttributeValueMappingItem RetrieveById(int id)
        {
            return this._repository.Table.Where(eavm => eavm.Id == id).FirstOrDefault();
        }
    }
}
