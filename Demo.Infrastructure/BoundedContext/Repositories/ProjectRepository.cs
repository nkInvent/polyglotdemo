using Demo.Domain.Model;
using Demo.Infrastructure.Core;
using Demo.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Infrastructure.Context.Repositories
{
    public class ProjectRepository : Repository<PROJECT>, IProjectRepository
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="unitOfWork">Associated unit of work</param>
        public ProjectRepository(IStorageUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

    }
}
