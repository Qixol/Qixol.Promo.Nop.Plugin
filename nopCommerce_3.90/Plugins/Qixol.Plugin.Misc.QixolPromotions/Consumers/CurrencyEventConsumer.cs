using Nop.Core.Domain.Directory;
using Nop.Core.Events;
using Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
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
    public class CurrencyEventConsumer : IConsumer<EntityUpdated<Currency>>, IConsumer<EntityInserted<Currency>>, IConsumer<EntityDeleted<Currency>>
    {
        #region fields

        private readonly PromoSettings _promoSettings;
        private readonly IExportQueueService _exportQueueService;
        private readonly IAttributeValueService _attributeValueService;

        #endregion

        #region constructor 

        public CurrencyEventConsumer(PromoSettings promoSettings,
                                     IAttributeValueService attributeValueService,
                                     IExportQueueService exportQueueService)
        {
            this._promoSettings = promoSettings;
            this._exportQueueService = exportQueueService;
            this._attributeValueService = attributeValueService;
        }

        #endregion

        #region handlers 

        /// <summary>
        /// Handle updating of currency.
        /// </summary>
        /// <param name="eventMessage"></param>
        public void HandleEvent(EntityUpdated<Currency> eventMessage)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeCurrencies)
            {
                var existingItem = _attributeValueService.Retrieve(eventMessage.Entity.Id, EntityAttributeName.Currency);
                if (existingItem != null)
                {
                    if (eventMessage.Entity.Published)
                    {
                        existingItem.Code = eventMessage.Entity.CurrencyCode;
                        _attributeValueService.Update(existingItem);
                    }
                    else
                    {
                        _attributeValueService.Delete(existingItem);
                    }
                }
                else
                {
                    if (eventMessage.Entity.Published)
                    {
                        _attributeValueService.Insert(new Qixol.Nop.Promo.Core.Domain.AttributeValues.AttributeValueMappingItem()
                        {
                            AttributeName = EntityAttributeName.Currency,
                            AttributeValueId = eventMessage.Entity.Id,
                            Code = eventMessage.Entity.CurrencyCode
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Handle insertion of a currency.
        /// </summary>
        /// <param name="eventMessage"></param>
        public void HandleEvent(EntityInserted<Currency> eventMessage)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeCurrencies)
            {
                if (eventMessage.Entity.Published)
                {
                    _attributeValueService.Insert(new AttributeValueMappingItem()
                        {
                            AttributeName = EntityAttributeName.Currency,
                            AttributeValueId = eventMessage.Entity.Id,
                            Code = eventMessage.Entity.CurrencyCode
                        });
                }
            }
        }

        /// <summary>
        /// Handle deletion of a currency.
        /// </summary>
        /// <param name="eventMessage"></param>
        public void HandleEvent(EntityDeleted<Currency> eventMessage)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeCurrencies)
            {
                var existingItem = _attributeValueService.Retrieve(eventMessage.Entity.Id, EntityAttributeName.Currency);
                if (existingItem != null)
                    _attributeValueService.Delete(existingItem);
            }
        }

        #endregion



    }
}
