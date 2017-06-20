using Demo.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Domain.Model
{
    public class PROJECT : EntityBase
    {
        public int ID { get; set; }
        public string UPC { get; set; }
        public string CLIENT_NAME { get; set; }
        public string TEMPLATE_RATING_SCALE { get; set; }
    }
}
