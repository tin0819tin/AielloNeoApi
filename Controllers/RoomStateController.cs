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
    public class RoomStateController : ControllerBase
    {
        private readonly ILogger<RoomStateController> _logger;
        private readonly RoomStateCypher _roomStateCypher;
        private readonly IDriver _driver;

        public RoomStateController(ILogger<RoomStateController> logger, RoomStateCypher roomstatecypher, IDriver driver)
        {
            _logger = logger;
            _roomStateCypher = roomstatecypher;
            _driver = driver;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<RoomState>> GetRoomStateList()
        {
            var listResult = new List<RoomState>();
            try
            {
                using (var session = _driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _roomStateCypher.GetRoomStateList(tx);

                        foreach (var record in queryResult)
                        {
                            var node = record["roomstate"].As<INode>();
                            listResult.Add(new RoomState
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
        public ActionResult<RoomState> GetRoomStatebyName(string name)
        {
            var matchResult = new RoomState();

            try
            {
                using (var session = _driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _roomStateCypher.GetRoomState(tx, name).SingleOrDefault();
                        var roomstate = queryResult?["roomstate"];
                        
                        if (queryResult == null)
                        {
                            _logger.LogError("No content");
                            return null;
                        }
                        return roomstate.As<INode>().Properties;

                    });
                    var result = JsonConvert.SerializeObject(getResult);
                    var final_result = JsonConvert.DeserializeObject<RoomState>(result);

                    if (final_result != null)
                    {
                        matchResult = final_result;
                        _logger.LogInformation("RoomState Read!");
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
        public ActionResult<RoomState> PostRoomState(RoomState roomstate)
        {

            try
            {
                using (var session = _driver.Session())
                {
                    var createResult = session.WriteTransaction(tx =>
                    {
                        return _roomStateCypher.AddRoomState(tx, roomstate).Single()[0].As<INode>();
                    });
                    _logger.LogInformation("RoomState Created");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to Create!");
                return BadRequest(roomstate);
            }

            return CreatedAtAction(nameof(GetRoomStatebyName), new { roomstate.name }, roomstate);
        }


        // PUT api/<ValuesController>/5
        [HttpPut("{name}")]
        public ActionResult<RoomState> PutRoomState(string name, RoomState roomstate)
        {
            try
            {
                using (var session = _driver.Session())
                {
                    session.WriteTransaction(tx => _roomStateCypher.UpdateRoomState(tx, name, roomstate));
                    _logger.LogInformation("UPDATE RoomState Success!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(roomstate);

            }
            return CreatedAtAction(nameof(GetRoomStatebyName), new { roomstate.name }, roomstate);
        }

        // DELETE api/<ValuesController>/5
        //[HttpDelete("{name}")]
        //public ActionResult<RoomState> DeleteRoomStatebyName(string name)
        //{
        //    try
        //    {
        //        using (var session = Neo4jDriver._driver.Session())
        //        {
        //            session.WriteTransaction(tx => _roomStateCypher.DeleteRoomState(tx, name));
        //            _logger.LogInformation("DELETE RoomState Success!");
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
