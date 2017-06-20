using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Domain.Core
{
    public abstract class EntityBase
    {
        [DataType(DataType.DateTime)]
        public DateTime? Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Modified { get; set; }

        //public int TenantId { get; set; }

        public EntityBase()
        {
            this.Created = DateTime.UtcNow;
            this.Modified = DateTime.UtcNow;
            
        }

    }
}
