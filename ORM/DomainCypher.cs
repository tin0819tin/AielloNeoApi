using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aiello_Restful_API.Models;
using Aiello_Restful_API.Config;
using Aiello_Restful_API.Controllers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace Aiello_Restful_API.ORM
{
    public class DomainCypher
    {
        public IResult GetDomainList(ITransaction tx)
        {
            var getListCypher = "MATCH (d:Domain) RETURN d.name as domain";

            return tx.Run(getListCypher);
        }
    }
}
