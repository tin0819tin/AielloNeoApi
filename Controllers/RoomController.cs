using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
    public class RoomController : ControllerBase
    {
        private readonly ILogger<RoomController> _logger;
        private readonly RoomCypher _roomcypher;
        private readonly HotelCypher _hotelcypher;

        public RoomController(ILogger<RoomController> logger, RoomCypher roomCypher, HotelCypher hotelCypher)
        {
            _logger = logger;
            _roomcypher = roomCypher;
            _hotelcypher = hotelCypher;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<Room> GetRoomList([Required][FromQuery] string hotelname, [FromQuery] string floor, [FromQuery] string roomState, [FromQuery] string roomType)
        {           
            try
            {
                var getResult = _roomcypher.GetRoomList(hotelname, floor, roomState, roomType);

                if (getResult.Count() > 0)
                {
                    _logger.LogInformation("Get Room List Success!");
                    return Ok(getResult);
                }
                else
                {
                    _logger.LogError("Result Not Found!");
                    return BadRequest();
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
        public ActionResult<Room> GetRoombyName(string name, string hotelname)
        {
            try
            {
                var getResult = _roomcypher.GetRoombyName(name, hotelname);
                
                if (getResult != null)
                {
                    _logger.LogInformation("Room Read!");
                    return Ok(getResult);
                }
                else
                {
                    _logger.LogError("Result Not Found!");
                    return BadRequest(getResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }
        }

        // POST api/<ValuesController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Room> PostRoom(Room room)
        {
            var createResult = new Room();
            try
            {
                var checkHotelResult = _hotelcypher.GetHotel(room.hotelName);
                if(checkHotelResult == null)
                {
                    _logger.LogError("Hotel not exists!");
                    return BadRequest(room);
                }
                
                createResult = _roomcypher.CreateRoom(room);

                if(createResult != null)
                {
                    _logger.LogInformation("Create Room Successful!");
                    return CreatedAtAction(nameof(GetRoombyName), new { createResult.name, createResult.hotelName }, room);
                }
                else
                {
                    _logger.LogError("Create Room Failed!");
                    return BadRequest(room);
                }

                /*using (var session = _driver.Session())
                {       

                    var createFloor2HotelResult = session.WriteTransaction(tx =>
                    {
                        return _roomcypher.BuildFloor2Hotel(tx, room).SingleOrDefault();
                    });
                    var result1 = createFloor2HotelResult?[0].As<string>();

                    if (result1 == null)
                    {
                        _logger.LogInformation("The Floor is under the Hotel already!");
                    }
                    else
                    {
                        _logger.LogInformation(result1);
                    }

                    var createRoom2FloorResult = session.WriteTransaction(tx =>
                    {
                        return _roomcypher.CreateRoomCypher(tx, room).SingleOrDefault();
                    });
                    var result2 = createRoom2FloorResult?[0].As<string>();

                    if (result2 == null)
                    {
                        _logger.LogError("Same Room is Existed already!");
                        return BadRequest(room);
                    }
                    else
                    {
                        _logger.LogInformation(result2);
                    }

                    string roomState = "Available";
                    var createRoomState2RoomResult = session.WriteTransaction(tx =>
                    {
                        return _roomcypher.ConnectRoomState2Room(tx, room.name, room, roomState).SingleOrDefault();
                    });
                    var result3 = createRoomState2RoomResult?[0].As<string>();

                    _logger.LogInformation(result3);
                   

                    if (room.roomType != null)
                    {
                        //Add RoomType
                        var createRoomType2RoomResult = session.WriteTransaction(tx =>
                        {
                            return _roomcypher.ConnectRoomType2Room(tx, room).SingleOrDefault();
                        });

                        _logger.LogInformation(createRoomType2RoomResult[0].As<string>());
                    }


                    return CreatedAtAction(nameof(GetRoombyName), new { room.name, room.hotelName }, room);
                }*/
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(room);
            }
            
            
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{name}")]
        public ActionResult<Room> PutRoom(string name,[FromQuery] Room room)
        {          
            try
            {
                var putResult = _roomcypher.UpdateRoom(name, room);

                if(putResult != null)
                {
                    _logger.LogInformation(string.Format("UPDATE Room {0} Success!", name));
                    return CreatedAtAction(nameof(GetRoombyName), new { room.name, room.hotelName }, room);
                }
                else
                {
                    _logger.LogError("Update Room Failed!");
                    return BadRequest(room);
                }
                /*
               using (var session = _driver.Session())
               {
                    
                    var updateFloorResult = session.WriteTransaction(tx =>
                    {
                        return _roomcypher.UpdateRoomFloor(tx, name, room).SingleOrDefault();
                    });
                    var updatefloor = updateFloorResult?[0].As<string>();

                    if (updatefloor == null)
                    {
                        _logger.LogError("The floor is Not Existed or Same Room is Existed already!");
                        return BadRequest(room);
                    }
                    else
                    {
                        _logger.LogInformation(updatefloor);
                    }
                    
                var queryResult = _roomcypher.GetRoombyName(name, room.hotelName);

                    if(queryResult == null)
                    {
                        _logger.LogError("No Room Found");
                        return BadRequest(room);
                    }

                    HashSet<string> set1 = new HashSet<string>() { "Available", "Unavailable", "Occupied" };
                    HashSet<string> set2 = new HashSet<string>() { "MUR", "DND" };
                    HashSet<string> set3 = new HashSet<string>() { "Complaint", "Electricity", "Service", "InService", "Repair" };
                    HashSet<string> set4 = new HashSet<string>() { "Unavailable" };

                    var num1 = room.roomStates.Intersect(set1).Count();
                    var num2 = room.roomStates.Intersect(set2).Count();
                    var num3 = room.roomStates.Intersect(set3).Count();
                    var num4 = room.roomStates.Intersect(set4).Count();

                    if (num1 > 1 || num2 > 1 || (num1 + num2 + num3) == 0 || num2 * num4 != 0)
                    {
                        _logger.LogError("RoomStates exist conflict!");
                        return BadRequest(room);
                    }
                    else
                    {       
                        session.WriteTransaction(tx => _roomcypher.DeleteRoomState(tx, name, room));
                        
                        foreach (string roomState in room.roomStates)
                        {
                            var createRoomState2RoomResult = session.WriteTransaction(tx =>
                            {
                                return _roomcypher.ConnectRoomState2Room(tx, name, room, roomState).SingleOrDefault();
                            });
                            var updateRoomState = createRoomState2RoomResult?[0].As<string>();

                            if (updateRoomState == null)
                            {
                                _logger.LogError("A RoomState is Not Exist!");
                                room.roomStates.Remove(roomState);
                            }
                            else
                            {
                                _logger.LogInformation(updateRoomState);
                            }
                        }
                    }

                    if (room.roomType != null)
                    {
                        //Add RoomType
                        var createRoomType2RoomResult = session.WriteTransaction(tx =>
                        {
                            return _roomcypher.UpdateRoomType(tx, name, room).SingleOrDefault();
                        });

                        _logger.LogInformation(createRoomType2RoomResult[0].As<string>());
                    }

                    return CreatedAtAction(nameof(GetRoombyName), new { room.name, room.hotelName }, room);
                }*/
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(room);
            }

            
        }

        // DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
