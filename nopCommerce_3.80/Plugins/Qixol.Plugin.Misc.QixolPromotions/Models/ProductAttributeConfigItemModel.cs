using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Misc.Promo.Models
{
    public class ProductAttributeConfigItemModel
    {
        /// <summary>
        /// The id of the source item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The system name for the product attribute
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// The resource string for the name
        /// </summary>
        public string NameResource { get; set; }

        /// <summary>
        /// The resolved display name for this item
        /// </summary>
        public string NameText { get; set; }

        /// <summary>
        /// Flag indicating whether the attribute is enabled for sync to Promo.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
