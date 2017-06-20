using Demo.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Application.Service
{
    public interface IProjectService
    {
        List<PROJECT> getAllProject(bool updateCache = false);
        PROJECT getProjectByID(int ID);

        void Add(PROJECT prj);
    }
}
