using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig
{
    public class ProductAttributeConfigItem : BaseEntity 
    {
        /// <summary>
        /// The system name for the product attribute
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// The resource string for the name
        /// </summary>
        public string NameResource { get; set; }

        /// <summary>
        /// Flag indicating whether the attribute is enabled for sync to Promo.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The date and time when the item was created
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// The date and time when the item was updated
        /// </summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
