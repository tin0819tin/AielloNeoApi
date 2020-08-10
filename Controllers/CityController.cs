using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
    public class CityController : ControllerBase
    {
        private readonly ILogger<CityController> _logger;
        private readonly CityCypher _cityCypher;

        public CityController(ILogger<CityController> logger, CityCypher citycypher)
        {
            _logger = logger;
            _cityCypher = citycypher;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<City>> GetCityList()
        {
            var listResult = new List<City>();
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _cityCypher.GetCityList(tx);

                        foreach (var record in queryResult)
                        {
                            var node = record["city"].As<INode>();
                            listResult.Add(new City
                            {
                                name = node["name"].As<string>(),
                                name_cn = node["name_cn"].As<string>(),
                                name_tw = node["name_tw"].As<string>(),
                                timeZone = node["timeZone"].As<string>(),
                                createdAt = node["createdAt"].As<string>(),
                                updatedAt = node["updatedAt"].As<string>()
                            });
                        }

                        return (listResult);
                    });


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
        [HttpGet("{name}")]
        public ActionResult<City> GetCitybyName(string name)
        {
            var matchResult = new City();
            
            try
            {
                using(var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        if(!_cityCypher.GetCity(tx, name).Any())
                        {
                            _logger.LogError("No content");
                            return null;
                        }
                        return _cityCypher.GetCity(tx, name).Single()[0].As<INode>().Properties;
                        
                    });
                    var result = JsonConvert.SerializeObject(getResult);
                    var final_result = JsonConvert.DeserializeObject<City>(result);

                    if (final_result != null)
                    {
                        matchResult = final_result;
                        _logger.LogInformation("City Read!");
                        return Ok(matchResult);
                    }
                    else
                    {
                        matchResult = null;
                        _logger.LogError("Result Not Found!");
                        return BadRequest(matchResult);
                    }
                }
            }
            catch(Exception ex)
            {
                matchResult = null;
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(matchResult);
            }
        }

        // POST api/<ValuesController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<City> PostCity(City city)
        {

            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var createResult = session.WriteTransaction(tx =>
                    {
                        return _cityCypher.AddCity(tx, city).Single()[0].As<INode>();
                    });
                    _logger.LogInformation("City Created");
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Fail to Create!");
                return BadRequest(city);
            }
            
            return CreatedAtAction(nameof(GetCitybyName), new { name = city.name }, city);
        }


        // PUT api/<ValuesController>/5
        [HttpPut("{name}")]
        public ActionResult<City> PutCity(string name, City city)
        {
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    session.WriteTransaction(tx => _cityCypher.UpdateCity(tx, city));
                    _logger.LogInformation("UPDATE City Success!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(city);

            }
            return CreatedAtAction(nameof(GetCitybyName), new { name = city.name }, city);
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{name}")]
        public ActionResult<City> DeleteCitybyName(string name)
        {
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    session.WriteTransaction(tx => _cityCypher.DeleteCity(tx, name));
                    _logger.LogInformation("DELETE City Success!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }

            return Ok();
        }
    }
}
