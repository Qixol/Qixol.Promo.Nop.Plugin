using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Products
{
    public class ProductMappingItem : BaseEntity
    {
        /// <summary>
        /// The entity name for the product.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// The Id of the Product...
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Flag indicating that the product has no variants.  As a result, there should only ever be one record for this product.
        /// </summary>
        public bool NoVariants { get; set; }

        /// <summary>
        /// The attribute XML for this specific combination of attributes.
        /// </summary>
        public string AttributesXml { get; set; }

        /// <summary>
        /// The variant code allocated for this combination.
        /// </summary>
        public string VariantCode { get; set; }

        /// <summary>
        /// The date and time when this entry was created.
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}
