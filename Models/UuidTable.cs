using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aiello_Restful_API.Models
{
    public class UuidTable
    {
        public Guid Id { get; set; }

        public DateTimeOffset? RegisteredTime { get; set; }
    }
}
