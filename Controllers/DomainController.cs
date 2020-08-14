using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using Aiello_Restful_API.Models;
using Aiello_Restful_API.Config;
using Aiello_Restful_API.Controllers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Aiello_Restful_API.ORM;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aiello_Restful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DomainController : ControllerBase
    {
        private readonly ILogger<DomainController> _logger;
        private readonly DomainCypher _domainCypher;
        private readonly IDriver _driver;

        public DomainController(ILogger<DomainController> logger, DomainCypher domainCypher, IDriver driver)
        {
            _logger = logger;
            _domainCypher = domainCypher;
            _driver = driver;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<string>> GetDomainList()
        {
            var listResult = new List<string>();
            try
            {
                using (var session = _driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _domainCypher.GetDomainList(tx);

                        foreach (var record in queryResult)
                        {
                            listResult.Add(record["domain"].As<string>());
                        }

                        return (listResult);
                    });

                    _logger.LogInformation("Get Domain List Success!");
                    return Ok(getResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }
            
        }

        // GET api/<ValuesController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<ValuesController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<ValuesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
