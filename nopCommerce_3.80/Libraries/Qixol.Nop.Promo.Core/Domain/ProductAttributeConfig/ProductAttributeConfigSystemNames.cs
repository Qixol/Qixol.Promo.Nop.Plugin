using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig
{
    public class ProductAttributeConfigSystemNames
    {
        // These all need to be lower case as the XML tokens are passed to/from the Promo Engine in lower case
        public const string VENDOR = "vendor";
        public const string SKU = "sku";
        public const string CATEGORY = "category";
        public const string MANUFACTURER = "manufacturer";
        public const string CATEGORY_BREADCRUMBS = "categorybreadcrumbs";
        public const string MANUFACTURER_PART_NO = "manufacturerpartnumber";
        public const string GTIN = "gtin";
        public const string DISABLE_BUY_BUTTON = "disablebuybutton";
        public const string AVAILABLE_FOR_PREORDER = "availableforpreorder";
        public const string CALL_FOR_PRICE = "callforprice";
        public const string CUSTOMER_ENTERS_PRICE = "customerentersprice";
        public const string IS_GIFT_CARD = "isgiftcard";
        public const string GIFT_CARD_TYPE = "giftcardtype";
        public const string DOWNLOADABLE_PRODUCT = "downloadableproduct";
        public const string IS_RENTAL = "isrental";
        public const string SHIPPING_ENABLED = "shippingenabled";
        public const string FREE_SHIPPING = "freeshipping";
        public const string SHIP_SEPARATELY = "shipseparately";
        public const string TAX_EXCEMPT = "taxexempt";
        public const string TAX_CATEGORY = "taxcategory";
        public const string PRODUCT_SPECIFICATION_ATTRIBS = "specificationattributes";
    }
}
