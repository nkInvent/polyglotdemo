using Demo.Domain.Core;
using Demo.Domain.Model;
using Demo.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Infrastructure.UnitOfWork
{
    public interface IStorageUnitOfWork :  IQueryableUnitOfWork, IUnitOfWork, IDisposable
    {  
        IDbSet<PROJECT> PROJECT { get; }
        IDbSet<PROJECT_VERSION> PROJECT_VERSION { get; }
    }
}
