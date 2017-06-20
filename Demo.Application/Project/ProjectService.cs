using Demo.Application.Service;
using Demo.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Diagnostics;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Web;
using Demo.Application.Core;

namespace PolyglotDemo.Application.Service
{
    public class ProjectService : IProjectService
    {
        private IProjectRepository _prjRepository;
        private IRedisHelper _redisHelper;

        public ProjectService(IProjectRepository prjRepository, IRedisHelper redisHelper)
        {
            if (prjRepository == (IProjectRepository)null)
                throw new ArgumentNullException("prjRepository");
           
            this._prjRepository = prjRepository;
            this._redisHelper = redisHelper;
           
        }

        public void Add(PROJECT prj)
        {
            _prjRepository.Add(prj);
            _prjRepository.UnitOfWork.Commit();
            
        }

        public List<PROJECT> getAllProject(bool updateCache= false)
        {
            var tenantId = Globals.tenantId;
            List<PROJECT> projects = null;

            //First, check the data from the cache
            var cache = _redisHelper.GetRedisConn().GetDatabase();
           
            string serilizedProjects = cache.StringGet("prjList-" + tenantId);

            if (!String.IsNullOrEmpty(serilizedProjects))
            {
                if (updateCache)
                {
                    projects = _prjRepository.GetAllElements().ToList();
                    cache.StringSet("prjList-" + tenantId, JsonConvert.SerializeObject(projects));
                }
                else projects = JsonConvert.DeserializeObject<List<PROJECT>>(serilizedProjects);                
            }
            else
            {
                projects = _prjRepository.GetAllElements().ToList();
                cache.StringSet("prjList-" + tenantId, JsonConvert.SerializeObject(projects));
            }

            return projects;
        }

        public PROJECT getProjectByID(int id)
        {
            return _prjRepository.GetElementById(id);
        }

    }
}
