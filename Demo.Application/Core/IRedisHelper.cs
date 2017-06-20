using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Application.Core
{
    public interface IRedisHelper
    {
        ConnectionMultiplexer GetRedisConn();
    }
}
