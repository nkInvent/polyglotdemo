using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Demo.Domain.Model;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Demo.Infrastructure.UnitOfWork
{
    public class StorageSqlUoW : DbContext, IStorageUnitOfWork
    {
        public StorageSqlUoW():base("name=DERISK2")
        {

        }

        #region Fileds
        private IDbSet<PROJECT> _projects;
        private IDbSet<PROJECT_VERSION> _projectVerions;
        #endregion

        #region Properties
        public IDbSet<PROJECT> PROJECT
        {
            get
            {
                if (this._projects == null)
                    this._projects = (IDbSet<PROJECT>)this.Set<PROJECT>();
                return this._projects;
            }
        }

        public IDbSet<PROJECT_VERSION> PROJECT_VERSION
        {
            get
            {
                if (this._projectVerions == null)
                    this._projectVerions = (IDbSet<PROJECT_VERSION>)this.Set<PROJECT_VERSION>();
                return this._projectVerions;
            }
        }
        #endregion

        #region IQueryableUnitOfWork
        public virtual IQueryable<TEntity> CreateSet<TEntity>() where TEntity : class, new()
        {
            return (IDbSet<TEntity>)this.Set<TEntity>();
        }

        public virtual void Attach<TEntity>(TEntity item) where TEntity : class
        {
            this.Entry<TEntity>(item).State = EntityState.Unchanged;
        }

        public virtual void SetModified<TEntity>(TEntity item) where TEntity : class
        {
            this.Entry<TEntity>(item).State = EntityState.Modified;
        }

        public virtual void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class
        {
            this.Entry<TEntity>(original).CurrentValues.SetValues((object)current);
        }

        #endregion

        #region IUnitOfWork
        public virtual void Commit()
        {
            try
            {
                this.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                throw this.GetDBValidationExptions(ex);
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await this.SaveChangesAsync(cancellationToken);
            }
            catch (DbEntityValidationException ex)
            {
                throw this.GetDBValidationExptions(ex);
            }
        }

        public void CommitAndRefreshChanges()
        {
            bool saveFailed = false;

            do
            {
                try
                {
                    base.SaveChanges();

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    ex.Entries.ToList()
                              .ForEach(entry =>
                              {
                                  entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                              });

                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var entry in this.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                        {
                            entry.State = EntityState.Detached;
                        }
                    }

                    throw GetDBValidationExptions(dbEx);

                }
            } while (saveFailed);
        }

        public async Task CommitAndRefreshChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool saveFailed = false;

            do
            {
                try
                {
                    await base.SaveChangesAsync(cancellationToken);

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    ex.Entries.ToList()
                              .ForEach(entry =>
                              {
                                  entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                              });

                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var entry in this.ChangeTracker.Entries())
                    {
                        if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                        {
                            entry.State = EntityState.Detached;
                        }
                    }

                    throw GetDBValidationExptions(dbEx);
                }

            } while (saveFailed);
        }

        public void RollbackChanges()
        {
            this.ChangeTracker.Entries().ToList().ForEach((entry => entry.State = EntityState.Unchanged));
        }

        #endregion

        #region ISsql
        public IEnumerable<TEntity> ExecuteQuery<TEntity>(string sqlQuery, params object[] parameters)
        {
            return (IEnumerable<TEntity>)this.Database.SqlQuery<TEntity>(sqlQuery, parameters);
        }

        public int ExecuteCommand(string sqlCommand, params object[] parameters)
        {
            return this.Database.ExecuteSqlCommand(sqlCommand, parameters);
        }

        #endregion

        #region DbContext
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        #endregion

        #region local
        private Exception GetDBValidationExptions(DbEntityValidationException dbEx)
        {
            string message = string.Empty;
            foreach (DbEntityValidationResult validationResult in dbEx.EntityValidationErrors)
            {
                foreach (DbValidationError dbValidationError in (IEnumerable<DbValidationError>)validationResult.ValidationErrors)
                    message = message + string.Format("Property: {0} Error: {1}", (object)dbValidationError.PropertyName, (object)dbValidationError.ErrorMessage);
            }
            return new Exception("ValidationError", new Exception(message));
        }
        #endregion
    }
}
