using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc5;
using Demo.Application.Service;
using PolyglotDemo.Application.Service;
using Demo.Infrastructure.Context.Repositories;
using Demo.Domain.Model;
using Demo.Infrastructure.UnitOfWork;
using Demo.Infrastructure.Core.Sharding;
using System.Data.SqlClient;
using Demo.Infrastructure.BoundedContext.UnitOfWork;

using System.Configuration;
using StackExchange.Redis;
using System;
using System.Web;
using Demo.Application.Core;

namespace PolyglotDemo
{

    public static class UnityConfig
    {
        private static string s_server = "XXX.database.windows.net"; 
        private static string s_shardmapmgrdb = "xxx_smm"; 
        private static string s_shard1 = "xxx_s1"; 
        private static string s_shard2 = "xxx_s2";  
        private static string s_userName = "XXX"; 
        private static string s_password = "XXX"; 
        private static string s_applicationName = "XXX";

        // Four tenants
        private static int s_tenantId1 = 1;
        private static int s_tenantId2 = 2;
        private static int s_tenantId3 = 3;
        private static int s_tenantId4 = 4;



        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            container.RegisterType<IProjectService, ProjectService>();
            container.RegisterType<IProjectRepository, ProjectRepository>();
            container.RegisterType<IRedisHelper, RedisHelper>();
          
            //==-----------------------------------------------------------------------

            var StorageMode = System.Configuration.ConfigurationManager.AppSettings.Get("StorageMode");
            if (StorageMode == "SingleTenant")
            {
                container.RegisterType<IStorageUnitOfWork, StorageSqlUoW>();
               
            }
            else if (StorageMode == "MultiTenant")
            {
                #region init shard
                SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder
                {
                    UserID = s_userName,
                    Password = s_password,
                    ApplicationName = s_applicationName
                };

                Sharding sharding = new Sharding(s_server, s_shardmapmgrdb, connStrBldr.ConnectionString);
                sharding.RegisterNewShard(s_server, s_shard1, connStrBldr.ConnectionString, s_tenantId1);
                sharding.RegisterNewShard(s_server, s_shard2, connStrBldr.ConnectionString, s_tenantId2);
                sharding.RegisterNewShard(s_server, s_shard1, connStrBldr.ConnectionString, s_tenantId3);
                sharding.RegisterNewShard(s_server, s_shard2, connStrBldr.ConnectionString, s_tenantId4);
                #endregion

              

                Globals.tenantId = 2;
                container.RegisterType<IStorageUnitOfWork, ElasticScaleContext<int>>(new InjectionConstructor(sharding.ShardMap, Globals.tenantId, connStrBldr.ConnectionString));
            }

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}