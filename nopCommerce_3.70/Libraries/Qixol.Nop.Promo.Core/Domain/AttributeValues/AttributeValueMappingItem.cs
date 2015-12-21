using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.AttributeValues
{
    public class AttributeValueMappingItem : BaseEntity
    {
        /// <summary>
        /// The name of the attribute which this code/id combination belongs to.  Set values using EntityAttributeName constants.
        /// </summary>
        public string AttributeName { get; set; }
        
        /// <summary>
        /// The ID of the nop source record.
        /// </summary>
        public int AttributeValueId { get; set; }

        /// <summary>
        /// The priority (higher number = higher priority)
        /// </summary>
        public int? Priority { get; set; }

        /// <summary>
        /// The Code to be used in Promo.  This code should not be editable if the Synchronized flag is set to TRUE.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The date when this entry was created.
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Flag indicating whether the data has been synchronized yet.
        /// </summary>
        public bool Synchronized { get; set; }

        /// <summary>
        /// The code that has been synchronized.  If the code changes this is used to remove the previous 
        /// </summary>
        public string SynchronizedCode { get; set; }
    }
}
