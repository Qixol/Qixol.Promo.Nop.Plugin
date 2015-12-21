using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.AttributeValues
{
    public interface IAttributeValueService
    {

        void Insert(AttributeValueMappingItem itemToInsert);

        void Update(AttributeValueMappingItem itemToUpdate, bool publishEvents = true);

        void Delete(AttributeValueMappingItem itemToDelete);

        AttributeValueMappingItem Retrieve(int entityAttributeValueId, string entityAttributeName);

        IQueryable<AttributeValueMappingItem> RetrieveAllForAttribute(string entityAttributeName);

        AttributeValueMappingItem RetrieveById(int id);
    }
}
