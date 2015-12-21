using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ExportQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Consumers
{
    public class CheckoutAttributeEventConsumer : IConsumer<EntityInserted<CheckoutAttribute>>, IConsumer<EntityUpdated<CheckoutAttribute>>, IConsumer<EntityDeleted<CheckoutAttribute>>,
                                                  IConsumer<EntityInserted<CheckoutAttributeValue>>, IConsumer<EntityUpdated<CheckoutAttributeValue>>, IConsumer<EntityDeleted<CheckoutAttributeValue>>
    {

        private readonly PromoSettings _promoSettings;
        private readonly IExportQueueService _exportQueueService;
        private readonly IAttributeValueService _attributeValueService;

        public CheckoutAttributeEventConsumer(PromoSettings promoSettings,
                                              IExportQueueService exportQueueService,
                                              IAttributeValueService attributeValueService)
        {
            this._promoSettings = promoSettings;
            this._exportQueueService = exportQueueService;
            this._attributeValueService = attributeValueService;
        }

        public void HandleEvent(EntityInserted<CheckoutAttribute> eventMessage)
        {
            HandleCheckoutAttributeChange(eventMessage.Entity.Id, ExportQueueAction.Insert);            
        }

        public void HandleEvent(EntityUpdated<CheckoutAttribute> eventMessage)
        {
            HandleCheckoutAttributeChange(eventMessage.Entity.Id, ExportQueueAction.Update);
        }

        public void HandleEvent(EntityDeleted<CheckoutAttribute> eventMessage)
        {
            HandleCheckoutAttributeChange(eventMessage.Entity.Id, ExportQueueAction.Delete);
        }

        public void HandleEvent(EntityInserted<CheckoutAttributeValue> eventMessage)
        {
            HandleCheckoutAttributeChange(eventMessage.Entity.CheckoutAttributeId, ExportQueueAction.Insert);
        }

        public void HandleEvent(EntityUpdated<CheckoutAttributeValue> eventMessage)
        {
            HandleCheckoutAttributeChange(eventMessage.Entity.CheckoutAttributeId, ExportQueueAction.Update);
        }

        public void HandleEvent(EntityDeleted<CheckoutAttributeValue> eventMessage)
        {
            HandleCheckoutAttributeChange(eventMessage.Entity.CheckoutAttributeId, ExportQueueAction.Update);
        }

        private void HandleCheckoutAttributeChange(int checkoutAttributeId, string action)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeCheckoutAttributes)
            {
                var attribMapping = _attributeValueService.Retrieve(checkoutAttributeId, EntityAttributeName.CheckoutAttribute);
                if (attribMapping != null)
                    _exportQueueService.InsertQueueItem(EntityAttributeName.CheckoutAttribute, action, attribMapping.Id);
            }
        }
    }
}
