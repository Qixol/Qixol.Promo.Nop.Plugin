using global::Nop.Core.Data;
using global::Nop.Services.Events;
using Qixol.Nop.Promo.Data;
using Qixol.Nop.Promo.Core.Domain.ExportQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ExportQueue
{
    public class ExportQueueService : IExportQueueService
    {
        private readonly IRepository<ExportQueueItem> _repository;
        private readonly IEventPublisher _eventPublisher;

        public ExportQueueService(IRepository<ExportQueueItem> repository,
                                  IEventPublisher eventPublisher)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Insert the specified item into the queue.
        /// </summary>
        /// <param name="itemToInsert">The item which is to be inserted into the queue.</param>
        /// <param name="removeExistingUnprocessed">Flag indicating whether all other references (that have not yet been processed) should be deleted from the queue, as this entry will supercede them.</param>
        public void InsertQueueItem(ExportQueueItem itemToInsert, bool removeExistingUnprocessed)
        {

            if (this._repository.Table.Any(i => i.EntityName == itemToInsert.EntityName
                                                && (i.Status == ExportQueueStatus.Pending || i.Status == ExportQueueStatus.CommsFailure)
                                                && i.Action == ExportQueueAction.All))
            {
                // There is a pending 'All' entry - so no point in inserting anything else for this entity!
                return;
            }

            if (removeExistingUnprocessed)
            {
                // Look for any entries in the table which use the same key, and have not been processed, and remove them.
                //var itemsToRemove = this._promoContext.
                var itemsToRemove = this._repository.Table.Where(i => i.EntityKey == itemToInsert.EntityKey
                                                                        && i.EntityName == itemToInsert.EntityName
                                                                        && i.EntityVariant == itemToInsert.EntityVariant
                                                                        && (i.Status == ExportQueueStatus.Pending || i.Status == ExportQueueStatus.CommsFailure)).ToList();
                if (itemsToRemove != null && itemsToRemove.Count > 0)
                {
                    // There are some items in the queue that relate to this one!
                    itemsToRemove.ForEach(ir =>
                            {
                                this._repository.Delete(ir);
                                _eventPublisher.EntityDeleted<ExportQueueItem>(ir);
                            });
                }
            }

            itemToInsert.CreatedOnUtc = DateTime.Now;
            itemToInsert.UpdatedOnUtc = DateTime.Now;
            itemToInsert.Status = ExportQueueStatus.Pending;

            _repository.Insert(itemToInsert);
            _eventPublisher.EntityInserted<ExportQueueItem>(itemToInsert);
        }

        /// <summary>
        /// Insert an item into the queue using the specified details.
        /// </summary>
        /// <param name="entityName">The entity name, from the supported values defined in ExportQueueEntityName.</param>
        /// <param name="action">The action for the entity, from the suppored values defined in ExportQueueAction.</param>
        /// <param name="entityKey">The ID of the entity item.</param>
        /// <param name="removeExistingUnprocessed">Flag indicating whether all other references (that have not yet been processed) should be deleted from the queue, as this entry will supercede them.</param>
        public void InsertQueueItem(string entityName, string action, int entityKey, string entityCode = "", bool removeExistingUnprocessed = true, string entityVariant = "")
        {
            var newItem = new ExportQueueItem()
            {
                Action = action,
                EntityKey = entityKey,
                EntityName = entityName,
                EntityVariant = entityVariant,
                EntityCode = entityCode
            };

            InsertQueueItem(newItem, removeExistingUnprocessed);
        }

        /// <summary>
        /// Save changes to the specified item.
        /// </summary>
        /// <param name="itemToUpdate">The item which has been updated.</param>
        public void UpdateItem(ExportQueueItem itemToUpdate)
        {
            itemToUpdate.UpdatedOnUtc = DateTime.Now;
            _repository.Update(itemToUpdate);
            _eventPublisher.EntityUpdated<ExportQueueItem>(itemToUpdate);
        }

        /// <summary>
        /// Retrieve all pending queue items, including those which have failed due to comms failures.
        /// </summary>
        /// <returns>A queryable list of pending queue items.</returns>
        public IQueryable<ExportQueueItem> RetrievePending()
        {
            return _repository.Table.Where(i => i.Status == ExportQueueStatus.Pending || i.Status == ExportQueueStatus.CommsFailure).OrderBy(ob => ob.CreatedOnUtc);
        }

        /// <summary>
        /// For the entity name specified, insert a queue item with a action of ALL.  This means we will clear down all other entries for that entity that have
        /// not yet been processed as these are superceded by the fact we are pushing up ALL of that entity.
        /// </summary>
        /// <param name="entityName"></param>
        public void InsertQueueItemForAll(string entityName)
        {
            var itemsToRemove = _repository.Table.Where(i => (i.Status == ExportQueueStatus.Pending || i.Status == ExportQueueStatus.CommsFailure)
                                                             && i.EntityName == entityName && i.Action != ExportQueueAction.Delete).ToList();
            itemsToRemove.ForEach(i =>
                {
                    _repository.Delete(i);
                    _eventPublisher.EntityDeleted<ExportQueueItem>(i);
                });

            this.InsertQueueItem(new ExportQueueItem()
                {
                    Action = ExportQueueAction.All,
                    EntityName = entityName
                }, false);
        }
    }
}
