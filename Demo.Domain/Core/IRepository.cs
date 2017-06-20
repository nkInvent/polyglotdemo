namespace Demo.Domain.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRepository<TEntity>
         where TEntity : EntityBase
    {
        IUnitOfWork UnitOfWork { get; }

        void Add(TEntity item);

        void Remove(TEntity item);

        void Attach(TEntity item);

        /// <summary>
        ///Set entity modified into this repository, really in UnitOfWork. 
        /// </summary>
        /// <param name="item">Item to attach</param>
        void SetModified(TEntity item);

        /// <summary>
        /// Sets modified entity into the repository. 
        /// When calling Commit() method in UnitOfWork 
        /// these changes will be saved into the storage
        /// </summary>
        /// <param name="persisted">The persisted item</param>
        /// <param name="current">The current item</param>
        void Merge(TEntity persisted, TEntity current);

        TEntity GetElementById(int id);

        Task<TEntity> GetElementByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        IQueryable<TEntity> GetAllElements();

        IQueryable<TEntity> GetAllElements(params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default(CancellationToken), params Expression<Func<TEntity, object>>[] includes);

     }
}
