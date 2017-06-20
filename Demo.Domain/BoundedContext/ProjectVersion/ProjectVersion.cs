using Demo.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Domain.Model
{
    public class PROJECT_VERSION : EntityBase
    {
        public int ID { get; set; }
        public int PROJECT_ID { get; set; }
        public int VERSION { get; set; }
        public bool LOCKED { get; set; }
        public string DESCRIPTION { get; set; }
        public string LOCK_DATE { get; set; }
    }
}
