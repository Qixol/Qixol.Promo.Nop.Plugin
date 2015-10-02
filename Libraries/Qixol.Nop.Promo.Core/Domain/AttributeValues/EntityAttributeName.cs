using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.AttributeValues
{
    public class EntityAttributeName
    {
        public const string DeliveryMethod = "DeliveryMethod";
        public const string Store = "Store";
        public const string CustomerRole = "CustomerRole";
        public const string Product = "Product";
        public const string CheckoutAttribute = "CheckoutAttribute";
        public const string Currency = "Currency";

        /// <summary>
        /// Convert the attribute name to the promo system name.
        /// </summary>
        /// <param name="entityattributeName"></param>
        /// <returns></returns>
        public static string ToPromoAttributeName(string entityattributeName)
        {
            switch (entityattributeName)
            {
                case EntityAttributeName.CustomerRole:
                    return "customergroup";
                case EntityAttributeName.DeliveryMethod:
                    return "deliverymethod";
                case EntityAttributeName.Store:
                    return "store";
                case EntityAttributeName.Currency:
                    return "currencycode";
                default:
                    return "";
            }
        }
    }
}
