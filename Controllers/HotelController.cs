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
using Namotion.Reflection;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aiello_Restful_API.Controllers
{
    [Route("api/[controller]")] // Need ADJUST!!
    [ApiController]
    public class HotelController : ControllerBase
    {

        private readonly ILogger<HotelController> _logger;
        private readonly HotelCypher _hotelcypher;
        private readonly CityCypher _cityCypher;
        
        public HotelController(ILogger<HotelController> logger, HotelCypher hotelCypher, CityCypher cityCypher)
        {
            _logger = logger;
            _hotelcypher = hotelCypher;
            _cityCypher = cityCypher;
        }
        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<Hotel>> GetHotelList([FromQuery] string city, [FromQuery] string domain, [FromQuery] string displayName, [FromQuery] int asr = -1)
        {

            try
            {
                
                var getResult = _hotelcypher.GetHotelList(city, domain, displayName, asr);

                if (getResult.Count() > 0)
                {
                    _logger.LogInformation("Get Hotel List Success!");
                    return Ok(getResult);
                }
                else
                {
                    _logger.LogError("Result Not Found!");
                    return BadRequest(getResult);
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }

        }

        // GET api/<ValuesController>/name
        //[Route("GetHotelByName")]
        [HttpGet("{name}")]
        public ActionResult<Hotel> GetHotelByName(string name,[Required] string domainname)
        {
            try
            {
                var getResult = _hotelcypher.GetHotel(name, domainname);

                if (getResult != null)
                {
                    _logger.LogInformation("Hotel Read!");
                    return Ok(getResult);
                }
                else
                {
                    _logger.LogError("Result Not Found!");
                    return BadRequest(getResult);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }
        }


        // POST api/<ValuesController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Hotel> PostHotel(Hotel hotel)
        {
            var createResult = new Hotel();
            try
            {
                var cityResult = _cityCypher.GetCity(hotel.city);
                if (cityResult == null)
                {
                    _logger.LogError("No city exist!");
                    return BadRequest(hotel);
                }

                var domainResult = _hotelcypher.CheckDomain(hotel);

                if (!domainResult)
                {
                    createResult = _hotelcypher.CreateHotel(hotel);
                }
                else
                {
                    var checkHotelResult = _hotelcypher.FindHotel2Domain(hotel);

                    if (!checkHotelResult)
                    {
                        createResult = _hotelcypher.CreateHotel(hotel);
                    }
                    else
                    {
                        _logger.LogError("The hotel is existed already! Please use PUT to update.");
                        return Conflict(hotel);
                    }
                }

                if(createResult != null)
                {
                    return CreatedAtAction(nameof(GetHotelByName), new { createResult.name }, hotel);
                }
                else
                {
                    _logger.LogError("Create Hotel Failed!");
                    return BadRequest(hotel);
                }  
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }
        }

        // PUT api/<ValuesController>/hotelname
        [HttpPut("{name}")]
        public ActionResult<Hotel> PutHotel(string name, string domainname, Hotel hotel)
        {
            try
            {
                var putResult = _hotelcypher.UpdateHotel(name, domainname, hotel);

                if (putResult != null)
                {
                    _logger.LogInformation(string.Format("UPDATE Hotel {0} Success!", name));
                    return CreatedAtAction(nameof(GetHotelByName), new { name }, hotel);
                }
                else
                {
                    _logger.LogError("Update Failed!");
                    return BadRequest(hotel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();

            }
            
        }

        // DELETE api/<ValuesController>/5
        //[HttpDelete("{hotelname}")]
        //public ActionResult<Hotel> DeleteHotelbyName([FromBody] string domainname, string hotelname)
        //{
        //    try
        //    {
        //        using (var session = Neo4jDriver._driver.Session())
        //        {
        //            session.WriteTransaction(tx => _hotelcypher.DeleteHotel(tx, domainname, hotelname));
        //            _logger.LogInformation("DELETE Hotel Success!");
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
