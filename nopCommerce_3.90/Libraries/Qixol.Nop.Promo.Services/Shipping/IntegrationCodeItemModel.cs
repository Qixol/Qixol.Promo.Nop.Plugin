using Nop.Core.Domain.Shipping;

namespace Qixol.Nop.Promo.Services.Promo
{
    internal class IntegrationCodeItemModel : ShippingMethod
    {
        public object EntityAttributeSystemName { get; set; }
        public int EntityId { get; set; }
        public object EntityName { get; set; }
    }
}