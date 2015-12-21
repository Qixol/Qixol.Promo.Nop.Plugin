using Nop.Core.Events;
using Nop.Core.Domain.Shipping;
using Nop.Services.Events;
using Qixol.Nop.Promo.Services;
using Nop.Services.Catalog;
using System.Collections.Generic;
using Nop.Services.Media;
using System.Linq;
using System.IO;
using System.ServiceModel;
using System.Text;
using Nop.Services.Shipping;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Services.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using Qixol.Nop.Promo.Core.Domain.Promo;

namespace Qixol.Plugin.Misc.Promo.Infrastructure.Cache
{
    public partial class ShippingMethodEventConsumer : IConsumer<EntityUpdated<ShippingMethod>>
    {
        #region fields

        private readonly PromoSettings _promoSettings;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IExportQueueService _exportQueueService;

        #endregion

        #region constructor

        public ShippingMethodEventConsumer(PromoSettings promoSettings,
                                           IAttributeValueService attributeValueService,
                                           IExportQueueService exportQueueService)
        {
            this._promoSettings = promoSettings;
            this._attributeValueService = attributeValueService;
            this._exportQueueService = exportQueueService;
        }

        #endregion

        #region handlers

        public void HandleEvent(EntityUpdated<ShippingMethod> eventMessage)
        {
            if (_promoSettings.Enabled && _promoSettings.SynchronizeShippingMethods)
            {

                var existingAttrib = this._attributeValueService.Retrieve(eventMessage.Entity.Id, EntityAttributeName.DeliveryMethod);
                if (existingAttrib != null && existingAttrib.Synchronized)
                {
                    // The attribute value has been synchronized - which means there will not be an update, so lets insert one...
                    _exportQueueService.InsertQueueItem(EntityAttributeName.DeliveryMethod, ExportQueueAction.Update, existingAttrib.Id, string.Empty);
                }
            }
        }

        #endregion

    }
}
