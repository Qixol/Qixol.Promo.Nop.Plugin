using Nop.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Data.Mapping
{
    public class NopPromoContext : DbContext, IDbContext
    {
        public NopPromoContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Add in any new tables to the model.
            modelBuilder.Configurations.Add(new ExportQueueItemMap());
            modelBuilder.Configurations.Add(new ProductMappingItemMap());
            modelBuilder.Configurations.Add(new AttributeValueMappingItemMap());
            modelBuilder.Configurations.Add(new ProductAttributeConfigItemMap());
            modelBuilder.Configurations.Add(new PromoDetailsMap());
            modelBuilder.Configurations.Add(new ProductPromoMappingMap());
            modelBuilder.Configurations.Add(new PromoPictureMap());
            modelBuilder.Configurations.Add(new PromoBannerMap());
            modelBuilder.Configurations.Add(new PromoBannerPictureMap());
            modelBuilder.Configurations.Add(new PromoBannerWidgetZoneMap());
            modelBuilder.Configurations.Add(new PromoIssuedCouponMap());

            modelBuilder.Configurations.Add(new PromoOrderMap());
            modelBuilder.Configurations.Add(new PromoOrderItemMap());
			modelBuilder.Configurations.Add(new PromoOrderItemPromotionMap());

            base.OnModelCreating(modelBuilder);
        }

      /// <summary>
        /// Install the required database changes
        /// </summary>
        public void Install()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<NopPromoContext>(null);

            // DM TODO:  Check to see whether the tables already exist, as this will make the install fail.
            //create the table
            var dbScript = ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
            Database.ExecuteSqlCommand(dbScript);

            // Commit to the database
            SaveChanges();
        }

        /// <summary>
        /// Uninstall the database changes
        /// </summary>
        public void Uninstall()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<NopPromoContext>(null);

            // Drop tables where applicable.
            CheckAndDropTable(ExportQueueItemMap.TABLENAME);
            CheckAndDropTable(ProductMappingItemMap.TABLENAME);
            CheckAndDropTable(AttributeValueMappingItemMap.TABLENAME);
            CheckAndDropTable(ProductAttributeConfigItemMap.TABLENAME);
            CheckAndDropTable(ProductPromoMappingMap.TABLENAME);
            CheckAndDropTable(PromoDetailsMap.TABLENAME);
            CheckAndDropTable(PromoPictureMap.TABLENAME);
            CheckAndDropTable(PromoBannerMap.TABLENAME);
            CheckAndDropTable(PromoBannerPictureMap.TABLENAME);
            CheckAndDropTable(PromoBannerWidgetZoneMap.TABLENAME);
            CheckAndDropTable(PromoIssuedCouponMap.TABLENAME);

            CheckAndDropTable(PromoOrderCouponMap.TABLENAME);
            CheckAndDropTable(PromoOrderItemPromotionMap.TABLENAME);
            CheckAndDropTable(PromoOrderItemMap.TABLENAME);
            CheckAndDropTable(PromoOrderMap.TABLENAME);
        }

        private void CheckAndDropTable(string tableName)
        {
            if (Database.SqlQuery<int>("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}", tableName).Any<int>())
            {
                var dbScript = "DROP TABLE [" + tableName + "]";
                Database.ExecuteSqlCommand(dbScript);
            }
        }


        #region IDBContext 

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : global::Nop.Core.BaseEntity, new()
        {
            throw new NotImplementedException();
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : global::Nop.Core.BaseEntity
        {
            return base.Set<TEntity>();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        #endregion


        public bool AutoDetectChangesEnabled
        {
            get
            {
                return this.Configuration.AutoDetectChangesEnabled;
            }
            set
            {
                this.Configuration.AutoDetectChangesEnabled = value;
            }
        }

        public void Detach(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
        }

        public bool ProxyCreationEnabled
        {
            get
            {
                return this.Configuration.ProxyCreationEnabled;
            }
            set
            {
                this.Configuration.ProxyCreationEnabled = value;
            }
        }
    }
}
