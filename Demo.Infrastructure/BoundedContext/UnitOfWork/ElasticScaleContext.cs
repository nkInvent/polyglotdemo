
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Entity;
using System.Data.SqlClient;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Demo.Domain.Model;
using Demo.Infrastructure.UnitOfWork;
using System.Linq;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Demo.Infrastructure.BoundedContext.UnitOfWork
{
    public class ElasticScaleContext<T> : DbContext, IStorageUnitOfWork
    {
        // Let's use the standard DbSets from the EF tutorial
        //public DbSet<Blog> Blogs { get; set; }
        //public DbSet<Post> Posts { get; set; }
        //public DbSet<User> Users { get; set; }

        #region Ctor
        // Regular constructor calls should not happen.
        // 1.) Use the protected c'tor with the connection string parameter to intialize a new shard. 
        // 2.) Use the public c'tor with the shard map parameter in the regular application calls with a tenant id.
        public ElasticScaleContext()
        {

        }

        // C'tor to deploy schema and migrations to a new shard
        protected internal ElasticScaleContext(string connectionString)
            : base(SetInitializerForConnection(connectionString))
        {
        }

        // Only static methods are allowed in calls into base class c'tors
        private static string SetInitializerForConnection(string connnectionString)
        {
            // We want existence checks so that the schema can get deployed
            Database.SetInitializer<ElasticScaleContext<T>>(new CreateDatabaseIfNotExists<ElasticScaleContext<T>>());
            return connnectionString;
        }

        // C'tor for data dependent routing. This call will open a validated connection routed to the proper
        // shard by the shard map manager. Note that the base class c'tor call will fail for an open connection
        // if migrations need to be done and SQL credentials are used. This is the reason for the 
        // separation of c'tors into the DDR case (this c'tor) and the internal c'tor for new shards.
        public ElasticScaleContext(ShardMap shardMap, T shardingKey, string connectionStr)
            : base(OpenDDRConnection(shardMap, shardingKey, connectionStr), true /* contextOwnsConnection */)
        {
        }

        /// <summary>
        /// Wrapper function for ShardMap.OpenConnectionForKey() that automatically sets SESSION_INFO to the correct
        /// tenantId before returning a connection. As a best practice, you should only open connections using this 
        /// method to ensure that SESSION_INFO is always set before executing a query.
        /// </summary>
        public static SqlConnection OpenDDRConnection(ShardMap shardMap, T shardingKey, string connectionStr)
        {
            // No initialization
            Database.SetInitializer<ElasticScaleContext<T>>(null);

            // Ask shard map to broker a validated connection for the given key
            SqlConnection conn = null;
            try
            {
                conn = shardMap.OpenConnectionForKey(shardingKey, connectionStr, ConnectionOptions.Validate);

                // Set TenantId in SESSION_CONTEXT to shardingKey to enable Row-Level Security filtering
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"exec sp_set_session_context @key=N'TenantId', @value=@shardingKey";
                cmd.Parameters.AddWithValue("@shardingKey", shardingKey);
                cmd.ExecuteNonQuery();

                return conn;
            }
            catch (Exception)
            {
                if (conn != null)
                {
                    conn.Dispose();
                }

                throw;
            }
        }

        #endregion
        //==========================================================================================================================
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