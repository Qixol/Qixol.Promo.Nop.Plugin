using global::Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.ExportQueue
{
    public class ExportQueueItem : BaseEntity 
    {
        /// <summary>
        /// The name of the entity (i.e. product, customer group, delivery method, etc.).  Looked up against the constants in ExportQueueEntityName.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// The ID of the entity (i.e. product Id, or delivery method id, etc)
        /// </summary>
        public int EntityKey { get; set; }

        /// <summary>
        /// For Products - the entity variant of a specific item...
        /// </summary>
        public string EntityVariant { get; set; }

        /// <summary>
        /// When deleting - the code of the item being deleted.
        /// </summary>
        public string EntityCode { get; set; }

        /// <summary>
        /// The action being performed.  Looked up against the constants in ExportQueueAction.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The status of the processing.  Looked up against hte constants in ExportQueueStatus
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// When the item is sent to Promo it will be allocated a unique reference.
        /// </summary>
        public string PromoReference { get; set; }

        /// <summary>
        /// Any additional messages (i.e. if it failed!)
        /// </summary>
        public string Messages { get; set; }

        /// <summary>
        /// The date of creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// The date of last update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

    }
}
