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
    public class DeviceStatusController : ControllerBase
    {
        private readonly ILogger<DeviceStatusController> _logger;
        private readonly DeviceStatusCypher _deviceStatusCypher;
        private readonly IDriver _driver;

        public DeviceStatusController(ILogger<DeviceStatusController> logger, DeviceStatusCypher devicestatuscypher, IDriver driver)
        {
            _logger = logger;
            _deviceStatusCypher = devicestatuscypher;
            _driver = driver;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<DeviceStatus>> GetDeviceStatusList()
        {
            var listResult = new List<DeviceStatus>();
            try
            {
                using (var session = _driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _deviceStatusCypher.GetDeviceStatusList(tx);

                        foreach (var record in queryResult)
                        {
                            var node = record["devicestatus"].As<INode>();
                            listResult.Add(new DeviceStatus
                            {
                                name = node["name"].As<string>(),
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
        public ActionResult<DeviceStatus> GetDeviceStatusbyName(string name)
        {
            var matchResult = new DeviceStatus();

            try
            {
                using (var session = _driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _deviceStatusCypher.GetDeviceStatus(tx, name).SingleOrDefault();
                        var roomstate = queryResult?["devicestatus"];

                        if (queryResult == null)
                        {
                            _logger.LogError("No content");
                            return null;
                        }
                        return roomstate.As<INode>().Properties;

                    });
                    var result = JsonConvert.SerializeObject(getResult);
                    var final_result = JsonConvert.DeserializeObject<DeviceStatus>(result);

                    if (final_result != null)
                    {
                        matchResult = final_result;
                        _logger.LogInformation("DeviceStatus Read!");
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
            catch (Exception ex)
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
        public ActionResult<DeviceStatus> PostDeviceStatus(DeviceStatus devicestatus)
        {

            try
            {
                using (var session = _driver.Session())
                {
                    var createResult = session.WriteTransaction(tx =>
                    {
                        return _deviceStatusCypher.AddDeviceStatus(tx, devicestatus).Single()[0].As<INode>();
                    });
                    _logger.LogInformation("DeviceStatus Created");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to Create!");
                return BadRequest(devicestatus);
            }

            return CreatedAtAction(nameof(GetDeviceStatusbyName), new { devicestatus.name }, devicestatus);
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{name}")]
        public ActionResult<DeviceStatus> PutDeviceStatus(string name, DeviceStatus devicestatus)
        {
            try
            {
                using (var session = _driver.Session())
                {
                    session.WriteTransaction(tx => _deviceStatusCypher.UpdateDeviceStatus(tx, name, devicestatus));
                    _logger.LogInformation("UPDATE DeviceStatus Success!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(devicestatus);

            }
            return CreatedAtAction(nameof(GetDeviceStatusbyName), new { devicestatus.name }, devicestatus);
        }

        // DELETE api/<ValuesController>/5
        //[HttpDelete("{name}")]
        //public ActionResult<DeviceStatus> DeleteDeviceStatusbyName(string name)
        //{
        //    try
        //    {
        //        using (var session = Neo4jDriver._driver.Session())
        //        {
        //            session.WriteTransaction(tx => _deviceStatusCypher.DeleteDeviceStatus(tx, name));
        //            _logger.LogInformation("DELETE DeviceStatus Success!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unknown Exception!");
        //        return BadRequest();
        //    }

        //    return Ok();
        //}
    }
}
