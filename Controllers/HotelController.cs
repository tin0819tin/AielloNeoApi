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

        public HotelController(ILogger<HotelController> logger, HotelCypher hotelCypher)
        {
            _logger = logger;
            _hotelcypher = hotelCypher;
        }
        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<Hotel>> GetHotelList([FromQuery] string city, [FromQuery] string domain, [FromQuery] string displayName, [FromQuery] int asr = -1)
        {
            var listResult = new List<Hotel>();

            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _hotelcypher.GetHotelList(tx, city, domain, displayName, asr);

                        foreach (var record in queryResult)
                        {
                            var node = record["hotel"].As<INode>();
                            var hotelProp = node.TryGetPropertyValue<Dictionary<string,object>>("Properties");
                            var name = hotelProp.TryGetValue("name", out object value) ?value : null;
                            var displayName = hotelProp.TryGetValue("displayName", out object value1)? value1 : "null";
                            var description = hotelProp.TryGetValue("description", out object value2) ? value2 : "null";
                            var address = hotelProp.TryGetValue("address", out object value3) ? value3 : "null";
                            var contactPhone = hotelProp.TryGetValue("contactPhone", out object value4) ? value4 : "null";
                            var geo = hotelProp.TryGetValue("geo", out object value5) ? value5 : "null";
                            var frontDeskPhone = hotelProp.TryGetValue("frontDeskPhone", out object value6) ? value6 : "null";
                            var restaurantPhone = hotelProp.TryGetValue("restaurantPhone", out object value7) ? value7 : "null";
                            var sosPhone = hotelProp.TryGetValue("sosPhone", out object value8) ? value8 : "null";
                            var welcomeIntroduction = hotelProp.TryGetValue("welcomeIntroduction", out object value9) ? value9 : "null";
                            var welcomeIntroduction_cn = hotelProp.TryGetValue("welcomeIntroduction_cn", out object value10) ? value10 : "null";
                            var welcomeIntroduction_tw = hotelProp.TryGetValue("welcomeIntroduction_tw", out object value11) ? value11 : "null";
                            var asr = hotelProp.TryGetValue("asr", out object value12) ? value12: 0;
                            var createdAt = hotelProp.TryGetValue("createdAt", out object value13) ? value13 : "null";
                            var updatedAt = hotelProp.TryGetValue("updatedAt", out object value14) ? value14 : "null";

                            listResult.Add(new Hotel
                            {
                                //name = node["name"].As<string>(),
                                name = name.As<string>(),
                                displayName = displayName.As<string>(),
                                address = address.As<string>(),                            
                                contactPhone = contactPhone.As<string>(),
                                geo = geo.As<string>(),
                                domain = record["domain"].As<string>(),
                                city = record["city"].As<string>(),
                                description = description.As<string>(),
                                frontDeskPhone = frontDeskPhone.As<string>(),
                                restaurantPhone = restaurantPhone.As<string>(),
                                sosPhone = sosPhone.As<string>(),
                                welcomeIntroduction = welcomeIntroduction.As<string>(),
                                welcomeIntroduction_cn = welcomeIntroduction_cn.As<string>(),
                                welcomeIntroduction_tw = welcomeIntroduction_tw.As<string>(),
                                asr = asr.As<int>(),
                                createdAt = createdAt.As<string>(),
                                updatedAt = updatedAt.As<string>()
                                
                            });
                        }

                        return (listResult); 
                    });

                    if (getResult.Count() > 0)
                    {
                        _logger.LogInformation("Get Hotel List Success!");
                        return Ok(getResult);
                    }
                    else
                    {
                        _logger.LogError("Result Not Found!");
                        return BadRequest();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }

        }
/*
        [HttpGet("city")]
        public ActionResult<List<Hotel>> GetHotelListbyCity(string city)
        {
            var listResult = new List<Hotel>();
            //.Single()[0].As<List<object>>()
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _hotelcypher.GetHotelListbyCity(tx,city);

                        foreach (var record in queryResult)
                        {
                            var node = record["hotel"].As<INode>();
                            listResult.Add(new Hotel
                            {
                                name = node["name"].As<string>(),
                                displayName = node["displayName"].As<string>(),
                                address = node["address"].As<string>(),
                                contactPhone = node["contactPhone"].As<string>(),
                                geo = node["geo"].As<string>(),
                                domain = record["domain"].As<string>(),
                                city = record["city"].As<string>(),
                                description = node["description"].As<string>(),
                                frontDeskPhone = node["frontDeskPhone"].As<string>(),
                                restaurantPhone = node["restaurantPhone"].As<string>(),
                                sosPhone = node["sosPhone"].As<string>(),
                                welcomeIntroduction = node["welcomeIntroduction"].As<string>(),
                                welcomeIntroduction_cn = node["welcomeIntroduction_cn"].As<string>(),
                                welcomeIntroduction_tw = node["welcomeIntroduction_tw"].As<string>(),
                                asr = node["asr"].As<int>(),
                                createdAt = node["createdAt"].As<string>(),
                                updatedAt = node["updatedAt"].As<string>()
                            });
                        }

                        return (listResult);
                    });

                    if (getResult.Count() > 0)
                    {
                        _logger.LogInformation("Get Hotel List Success!");
                        return Ok(getResult);
                    }
                    else
                    {
                        _logger.LogError(string.Format("No hotel under {0}!", city));
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }

        }

        [HttpGet("domain")]
        public ActionResult<List<Hotel>> GetHotelListbyDomain(string domain)
        {
            var listResult = new List<Hotel>();
            //.Single()[0].As<List<object>>()
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _hotelcypher.GetHotelListbyDomain(tx, domain);

                        foreach (var record in queryResult)
                        {
                            var node = record["hotel"].As<INode>();
                            listResult.Add(new Hotel
                            {
                                name = node["name"].As<string>(),
                                displayName = node["displayName"].As<string>(),
                                address = node["address"].As<string>(),
                                contactPhone = node["contactPhone"].As<string>(),
                                geo = node["geo"].As<string>(),
                                domain = record["domain"].As<string>(),
                                city = record["city"].As<string>(),
                                description = node["description"].As<string>(),
                                frontDeskPhone = node["frontDeskPhone"].As<string>(),
                                restaurantPhone = node["restaurantPhone"].As<string>(),
                                sosPhone = node["sosPhone"].As<string>(),
                                welcomeIntroduction = node["welcomeIntroduction"].As<string>(),
                                welcomeIntroduction_cn = node["welcomeIntroduction_cn"].As<string>(),
                                welcomeIntroduction_tw = node["welcomeIntroduction_tw"].As<string>(),
                                asr = node["asr"].As<int>(),
                                createdAt = node["createdAt"].As<string>(),
                                updatedAt = node["updatedAt"].As<string>()
                            });
                        }

                        return (listResult);
                    });

                    if(getResult.Count() > 0)
                    {
                        _logger.LogInformation("Get Hotel List Success!");
                        return Ok(getResult);
                    }
                    else
                    {
                        _logger.LogError(string.Format("No hotel under {0}!", domain));
                        return BadRequest();
                    }                 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }

        }

        [HttpGet("asr")]
        public ActionResult<List<Hotel>> GetHotelListbyAsr(int asr)
        {
            var listResult = new List<Hotel>();
            //.Single()[0].As<List<object>>()
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _hotelcypher.GetHotelListbyAsr(tx, asr);

                        foreach (var record in queryResult)
                        {
                            var node = record["hotel"].As<INode>();
                            listResult.Add(new Hotel
                            {
                                name = node["name"].As<string>(),
                                displayName = node["displayName"]?.As<string>(),
                                address = node["address"].As<string>(),
                                contactPhone = node["contactPhone"].As<string>(),
                                geo = node["geo"].As<string>(),
                                domain = record["domain"].As<string>(),
                                city = record["city"].As<string>(),
                                description = node["description"]?.As<string>(),
                                frontDeskPhone = node["frontDeskPhone"].As<string>(),
                                restaurantPhone = node["restaurantPhone"].As<string>(),
                                sosPhone = node["sosPhone"].As<string>(),
                                welcomeIntroduction = node["welcomeIntroduction"].As<string>(),
                                welcomeIntroduction_cn = node["welcomeIntroduction_cn"].As<string>(),
                                welcomeIntroduction_tw = node["welcomeIntroduction_tw"].As<string>(),
                                asr = node["asr"].As<int>(),
                                createdAt = node["createdAt"].As<string>(),
                                updatedAt = node["updatedAt"].As<string>()
                            });
                        }

                        return (listResult);
                    });

                    if (getResult.Count() > 0)
                    {
                        _logger.LogInformation("Get Hotel List Success!");
                        return Ok(getResult);
                    }
                    else
                    {
                        _logger.LogError(string.Format("No hotel under asr({0})!", asr));
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest();
            }

        }
        //public async Task<Hotel> GetHotelbyname(string name)
*/

        // GET api/<ValuesController>/name
        //[Route("GetHotelByName")]
        [HttpGet("{name}")]
        public ActionResult<Hotel> GetHotelByName(string name, string domainname)
        {

            var matchResult = new Hotel();
            string domain = "";
            string city = "";

            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _hotelcypher.GetHotel(tx, name, domainname).SingleOrDefault();
                        var hotel = queryResult?["hotel"];
                        
                        if (queryResult == null)
                        {
                            _logger.LogError("No content");
                            return null;
                        }
                        city = queryResult["city"].ToString();
                        domain = queryResult["domain"].ToString();

                        return hotel.As<INode>().Properties;

                    });

                    var result = JsonConvert.SerializeObject(getResult);
                    var final_result = JsonConvert.DeserializeObject<Hotel>(result);

                    if (final_result != null)
                    {
                        matchResult = final_result;
                        matchResult.city = city;
                        matchResult.domain = domain;
                        _logger.LogInformation("Hotel Read!");
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

/*
        // GET api/<ValuesController>/name
        //[Route("{name}")]
        [HttpGet("displayName")]
        public ActionResult<Hotel> GetHotelByDisplayName(string displayName, [Required] string domainname)
        {

            var matchResult = new Hotel();
            string domain = "";
            string city = "";

            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = _hotelcypher.GetHotelbyDisplayName(tx, displayName, domainname).SingleOrDefault();
                        var hotel = queryResult?["hotel"];

                        if (queryResult == null)
                        {
                            _logger.LogError("No content");
                            return null;
                        }
                        city = queryResult["city"].ToString();
                        domain = queryResult["domain"].ToString();

                        return hotel.As<INode>().Properties;

                    });

                    var result = JsonConvert.SerializeObject(getResult);
                    var final_result = JsonConvert.DeserializeObject<Hotel>(result);

                    if (final_result != null)
                    {
                        matchResult = final_result;
                        matchResult.city = city;
                        matchResult.domain = domain;
                        _logger.LogInformation("Hotel Read!");
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
*/


        // POST api/<ValuesController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Hotel> PostHotel(Hotel hotel)
        {

            //var newHotel = new Hotel();

            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    var domainResult = session.ReadTransaction(tx =>
                    {
                        return _hotelcypher.CheckDomain(tx, hotel).Any();
                    });
                    if (!domainResult)
                    {

                        session.WriteTransaction(tx =>_hotelcypher.CreateHotel(tx, hotel));

                        var createDomain2HotelResult = session.WriteTransaction(tx =>
                        {
                            return _hotelcypher.CreateDomain2Hotel(tx, hotel).Single()[0].As<string>();
                        });
                        _logger.LogInformation(createDomain2HotelResult);

                        //check City (optional)

                        var createCity2HotelResult = session.WriteTransaction(tx =>
                        {
                            return _hotelcypher.CreateCity2Hotel(tx, hotel).Single()[0].As<string>();
                        });
                        _logger.LogInformation(createCity2HotelResult);

                        _logger.LogInformation(_hotelcypher.CreateBCMnBRT2Hotel(session, hotel));

                        return CreatedAtAction(nameof(GetHotelByName), new { hotel.name }, hotel);

                    }
                    else
                    {
                        var checkHotelResult = session.WriteTransaction(tx =>
                        {
                            return _hotelcypher.FindHotel2Domain(tx, hotel).Single()[0].As<bool>();
                        });

                        if (!checkHotelResult)
                        {
                            session.WriteTransaction(tx => _hotelcypher.CreateHotel(tx, hotel));

                            var createDomain2HotelResult = session.WriteTransaction(tx =>
                            {
                                return _hotelcypher.CreateDomain2Hotel(tx, hotel).Single()[0].As<string>();
                            });
                            _logger.LogInformation(createDomain2HotelResult);

                            //check City (optional)

                            var createCity2HotelResult = session.WriteTransaction(tx =>
                            {
                                return _hotelcypher.CreateCity2Hotel(tx, hotel).Single()[0].As<string>();
                            });
                            _logger.LogInformation(createCity2HotelResult);

                            _logger.LogInformation(_hotelcypher.CreateBCMnBRT2Hotel(session, hotel));

                            return CreatedAtAction(nameof(GetHotelByName), new { hotel.name }, hotel);
                        }
                        else
                        {
                            _logger.LogError("The hotel is existed already! Please use PUT to update.");
                            return Conflict(hotel);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(hotel);
            }
        }

        // PUT api/<ValuesController>/hotelname
        [HttpPut("{name}")]
        public ActionResult<Hotel> PutHotel(string name, string domainname, Hotel hotel)
        {
            var updateHotel = new Hotel();
            string domain = "";
            string city = "";

            try
            {
                using(var session = Neo4jDriver._driver.Session())
                {
                    var updateResult = session.WriteTransaction(tx => {
                        //tx.Rollback()
                        
                        var queryResult = _hotelcypher.UpdateHotel(tx, name, domainname, hotel).SingleOrDefault();
                        var new_hotel = queryResult?["hotel"]; 

                        if(queryResult == null)
                        {
                            _logger.LogError("The Domain or the City is not existed!");
                            return null;
                        }
                        city = queryResult["city"].ToString();
                        domain = queryResult["domain"].ToString();

                        return new_hotel.As<INode>().Properties;
                    });

                    var result = JsonConvert.SerializeObject(updateResult);
                    var final_result = JsonConvert.DeserializeObject<Hotel>(result);

                    if (final_result != null)
                    {
                        updateHotel = final_result;
                        updateHotel.city = city;
                        updateHotel.domain = domain;
                        updateHotel.name = name;

                        _logger.LogInformation(string.Format("UPDATE Hotel {0} Success!", name));
                        return CreatedAtAction(nameof(GetHotelByName), new { name }, hotel);
                    }
                    else
                    {
                        updateHotel = null;
                        _logger.LogError("Update Failed!");
                        return BadRequest(updateHotel);
                    }
                  
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(hotel);

            }
            
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{hotelname}")]
        public ActionResult<Hotel> DeleteHotelbyName([FromBody] string domainname, string hotelname)
        {
            try
            {
                using (var session = Neo4jDriver._driver.Session())
                {
                    session.WriteTransaction(tx => _hotelcypher.DeleteHotel(tx, domainname, hotelname));
                    _logger.LogInformation("DELETE Hotel Success!");
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
