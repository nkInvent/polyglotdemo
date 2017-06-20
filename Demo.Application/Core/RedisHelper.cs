using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Core
{
    public class RedisHelper: IRedisHelper
    {
        // Redis Connection string info
        private static Lazy<ConnectionMultiplexer> lazyRedisConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

         public ConnectionMultiplexer GetRedisConn()
        {
            return lazyRedisConnection.Value;
        }
    }
}
