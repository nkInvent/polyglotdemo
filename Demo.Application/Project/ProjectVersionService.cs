using Demo.Application.Service;
using Demo.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyglotDemo.Application.Service
{
    public class ProjectVersionService : IProjectVersionService
    {
        private IProjectVersionRepository _prjVerRepository;

        public ProjectVersionService(IProjectVersionRepository prjVerRepository)
        {
            if (prjVerRepository == (IProjectVersionRepository)null)
                throw new ArgumentNullException("prjVersionRepository");
           
            this._prjVerRepository = prjVerRepository;
           
        }

        List<PROJECT_VERSION> IProjectVersionService.getAllProjectVersion()
        {
            return _prjVerRepository.GetAllElements().ToList();
        }


        PROJECT_VERSION IProjectVersionService.getProjectVersionByID(int id)
        {
            return _prjVerRepository.GetElementById(id);
        }

      }
}
