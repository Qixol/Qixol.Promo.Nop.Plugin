using global::Nop.Core.Events;
using global::Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.ExportQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Consumers
{
    public partial class AttributeValueMappingEventConsumer : IConsumer<EntityInserted<AttributeValueMappingItem>>, IConsumer<EntityUpdated<AttributeValueMappingItem>>, IConsumer<EntityDeleted<AttributeValueMappingItem>>
    {
        private readonly PromoSettings _promoSettings;
        private readonly IExportQueueService _exportQueueService;

        public AttributeValueMappingEventConsumer(PromoSettings promoSettings, 
                                                  IExportQueueService exportQueueService)
        {
            this._promoSettings = promoSettings;
            this._exportQueueService = exportQueueService;
        }

        public void HandleEvent(EntityInserted<AttributeValueMappingItem> eventMessage)
        {
            if (SyncEntity(eventMessage.Entity))
                _exportQueueService.InsertQueueItem(eventMessage.Entity.AttributeName, ExportQueueAction.Insert, eventMessage.Entity.Id, eventMessage.Entity.Code, true);
        }

        public void HandleEvent(EntityUpdated<AttributeValueMappingItem> eventMessage)
        {
            if (SyncEntity(eventMessage.Entity))
                _exportQueueService.InsertQueueItem(eventMessage.Entity.AttributeName, ExportQueueAction.Update, eventMessage.Entity.Id, eventMessage.Entity.Code, true);
        }
        
        public void HandleEvent(EntityDeleted<AttributeValueMappingItem> eventMessage)
        {
            if (SyncEntity(eventMessage.Entity))
                _exportQueueService.InsertQueueItem(eventMessage.Entity.AttributeName, ExportQueueAction.Delete, eventMessage.Entity.Id, eventMessage.Entity.Code, true);
        }

        /// <summary>
        /// Return a flag indicating whether we want to sync the entity that we are consuming the event for.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool SyncEntity(AttributeValueMappingItem entity)
        {
            bool syncEnabled = false;
            if (_promoSettings.Enabled)
            {
                if (entity.AttributeName == EntityAttributeName.CustomerRole && _promoSettings.SynchronizeCustomerRoles)
                    syncEnabled = true;
                if (entity.AttributeName == EntityAttributeName.DeliveryMethod && _promoSettings.SynchronizeShippingMethods)
                    syncEnabled = true;
                if (entity.AttributeName == EntityAttributeName.Store && _promoSettings.SynchronizeStores)
                    syncEnabled = true;
                if (entity.AttributeName == EntityAttributeName.CheckoutAttribute && _promoSettings.SynchronizeCheckoutAttributes)
                    syncEnabled = true;
                if (entity.AttributeName == EntityAttributeName.Currency && _promoSettings.SynchronizeCurrencies)
                    syncEnabled = true;
            }
            return syncEnabled;
        }

    }
}
