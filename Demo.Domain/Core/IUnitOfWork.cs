using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Domain.Core
{
    public interface IUnitOfWork : IDisposable
    {
        //Commit all changes made in a container.
        void Commit();

        // Commit all changes made in  a container.
        void CommitAndRefreshChanges();

        /// <summary>
        /// Rollback changes are not stored in the database at 
        /// this moment. See references of UnitOfWork pattern
        /// </summary>
        void RollbackChanges();

        /// Commit all changes made in a container Async.
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));

        // Commit all changes made in  a container Async.
        Task CommitAndRefreshChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
