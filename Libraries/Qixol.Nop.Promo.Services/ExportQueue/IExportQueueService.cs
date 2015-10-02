using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ExportQueue
{
    public interface IExportQueueService
    {

        void InsertQueueItem(ExportQueueItem itemToInsert, bool removeExistingUnprocessed);

        void InsertQueueItem(string entityName, string action, int entityKey, string entityCode = "", bool removeExistingUnprocessed = true, string entityVariant = "");

        void InsertQueueItemForAll(string entityName);

        void UpdateItem(ExportQueueItem itemToUpdate);

        IQueryable<ExportQueueItem> RetrievePending();
    }
}
