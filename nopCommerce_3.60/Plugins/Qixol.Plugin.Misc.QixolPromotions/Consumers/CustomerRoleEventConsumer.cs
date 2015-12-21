using Nop.Core.Domain.Customers;
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
    public partial class CustomerRoleEventConsumer : IConsumer<EntityUpdated<CustomerRole>>
    {
        #region fields

        private readonly PromoSettings _promoSettings;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IExportQueueService _exportQueueService;

        #endregion

        #region constructor

        public CustomerRoleEventConsumer(PromoSettings promoSettings,
                                         IAttributeValueService attributeValueService,
                                         IExportQueueService exportQueueService)
        {
            this._promoSettings = promoSettings;
            this._attributeValueService = attributeValueService;
            this._exportQueueService = exportQueueService;
        }

        #endregion

        #region handlers

        public void HandleEvent(EntityUpdated<CustomerRole> eventMessage)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeCustomerRoles)
            {
                var existingAttrib = this._attributeValueService.Retrieve(eventMessage.Entity.Id, EntityAttributeName.CustomerRole);
                if (existingAttrib != null && existingAttrib.Synchronized)
                {
                    // The attribute value has been synchronized - which means there will not be an update, so lets insert one...
                    _exportQueueService.InsertQueueItem(EntityAttributeName.CustomerRole, ExportQueueAction.Update, existingAttrib.Id, string.Empty);
                }
            }
        }

        #endregion

    }
}
