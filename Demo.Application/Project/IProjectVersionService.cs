using Demo.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Application.Service
{
    public interface IProjectVersionService
    {
        List<PROJECT_VERSION> getAllProjectVersion();
        PROJECT_VERSION getProjectVersionByID(int id);
    }
}
