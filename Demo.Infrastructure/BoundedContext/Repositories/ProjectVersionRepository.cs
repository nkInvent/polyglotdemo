using Demo.Domain.Model;
using Demo.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Infrastructure.Context.Repositories
{
    public class ProjectVersionRepository : Repository<PROJECT_VERSION>, IProjectVersionRepository
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        /// <param name="unitOfWork">Associated unit of work</param>
        public ProjectVersionRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

    }
}
