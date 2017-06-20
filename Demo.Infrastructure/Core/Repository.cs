namespace Demo.Infrastructure.Core
{
    using Demo.Domain.Core;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;



    // Repository base class
    public class Repository<TEntity> : IRepository<TEntity>
       where TEntity : EntityBase, new()
    {
        IQueryableUnitOfWork _UnitOfWork;

        /// <summary>
        /// Create a new instance of repository
        /// </summary>
        /// <param name="unitOfWork">Associated Unit Of Work</param>
        public Repository(IQueryableUnitOfWork unitOfWork)
        {
            if (unitOfWork == (IUnitOfWork)null)
                throw new ArgumentNullException("unitOfWork");

            _UnitOfWork = unitOfWork;
        }


        #region IRepository Members

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _UnitOfWork;
            }
        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="item"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        public virtual void Add(TEntity item)
        {
            if (item == (TEntity)null)
                throw new ArgumentNullException(typeof(TEntity).ToString());
            GetSet().Add(item); // add new item in this set
        }
        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="item"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        public virtual void Remove(TEntity item)
        {
            if (item == (TEntity)null)
                throw new ArgumentNullException(typeof(TEntity).ToString());
            //attach item if not exist
            _UnitOfWork.Attach(item);
            //set as "removed"
            GetSet().Remove(item);
        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="item"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        public virtual void Attach(TEntity item)
        {
            if (item == (TEntity)null)
                throw new ArgumentNullException(typeof(TEntity).ToString());
            _UnitOfWork.Attach<TEntity>(item);

        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="item"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        public virtual void SetModified(TEntity item)
        {
            if (item == (TEntity)null)
                throw new ArgumentNullException(typeof(TEntity).ToString());
            _UnitOfWork.SetModified(item);

        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="persisted"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        /// <param name="current"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        public virtual void Merge(TEntity persisted, TEntity current)
        {
            _UnitOfWork.ApplyCurrentValues(persisted, current);
        }
        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="id"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        /// <returns><see cref="Domain.Core.IRepository{TEntity}"/></returns>
        public virtual TEntity GetElementById(int id)
        {
                return GetSet().Find(id);
        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <param name="id"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        /// <param name="cancellationToken"><see cref="Domain.Core.IRepository{TEntity}"/></param>
        /// <returns><see cref="Domain.Core.IRepository{TEntity}"/></returns>
        public Task<TEntity> GetElementByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            
                return Task.FromResult(this.GetSet().Find(id));
            
        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository{TEntity}"/>
        /// </summary>
        /// <returns><see cref="Domain.Core.IRepository{TEntity}"/></returns>
        public virtual IQueryable<TEntity> GetAllElements()
        {
            return GetSet();
        }

        /// <summary>
        /// <see cref="Domain.Core.IRepository.GetAllElements{TEntity}"/>
        /// </summary>
        /// <param name="includes"><see cref="Domain.Core.IRepository.GetAllElements{TEntity}"/></param>
        /// <returns><see cref="Domain.Core.IRepository.GetAllElements{TEntity}"/></returns>
        public virtual IQueryable<TEntity> GetAllElements(params Expression<Func<TEntity, object>>[] includes)
        {
            return includes
           .Aggregate(
               GetSet().AsQueryable(),
               (current, include) => current.Include(include)
           );
        }


        #endregion

        #region Private Methods

        IDbSet<TEntity> GetSet()
        {
            return _UnitOfWork.CreateSet<TEntity>() as IDbSet<TEntity>;
        }

        public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default(CancellationToken), params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
