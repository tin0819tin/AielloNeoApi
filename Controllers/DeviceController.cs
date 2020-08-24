using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Aiello_Restful_API.Models;
using Aiello_Restful_API.Config;
using Aiello_Restful_API.Controllers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Aiello_Restful_API.ORM;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Aiello_Restful_API.Data;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aiello_Restful_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly DeviceCypher _devicecypher;
        private TableContext _context;
        private readonly IDriver _driver;

        public DeviceController(ILogger<DeviceController> logger, DeviceCypher deviceCypher, TableContext context, IDriver driver)
        {
            _logger = logger;
            _devicecypher = deviceCypher;
            _context = context;
            _driver = driver;
        }

        private Guid GetId(string Id)
        {
            return Guid.Parse(Id);
        }

        private DateTimeOffset GetRegisteredTime(string RegisteredTime)
        {
            return DateTimeOffset.Parse(RegisteredTime);
        }

        private static object _thisLock = new object();


        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<List<Device>> GetDeviceList([FromQuery] string hotelName, [FromQuery] string room, [FromQuery] string uuid, [FromQuery] string deviceStatus)
        {                   
            try
            {
                var getResult = _devicecypher.GetDeviceList(hotelName, room, uuid, deviceStatus);
                
                if (getResult.Count() > 0)
                {
                    _logger.LogInformation("Get Device List Success!");
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
        [HttpGet("{mac}")]
        public ActionResult<Device> GetDevicebyMac(string mac)
        {
            
            try
            {
                var getResult = _devicecypher.GetDevicebyMac(mac);

                if (getResult != null)
                {

                    _logger.LogInformation("Device Read!");
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

        private static void DisplayStates(IEnumerable<EntityEntry> entries)
        {
            foreach (var entry in entries)
            {
                Console.WriteLine($"Entity: {entry.Entity.GetType().Name},State: {entry.State.ToString()}");
            }
        }

        // POST api/<ValuesController>
        [HttpPost]
        public ActionResult<Device> PostDevice(Device device)
        {
            Guid getId;

            lock (_thisLock)
            {
                try
                {
                    getId = _context.getId();         
                    _logger.LogInformation("Get uuid from the table!");
                }
                catch (Exception ex)
                {
                    
                    _logger.LogError(ex, "Get Id from SQL server failed!");
                    return BadRequest();
                }

                try
                {                                  
                    using (var session = _driver.Session())
                    {
                               
                        var createDeviceResult = session.WriteTransaction(tx =>
                        {
                            return _devicecypher.AddDevice(tx, device, getId.ToString().ToUpper()).SingleOrDefault();
                        });
                        var result = createDeviceResult?[0].As<string>();
                        

                        if (result != null)
                        {
                            try
                            {                         
                                _context.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                session.WriteTransaction(tx => _devicecypher.DeleteDevice(tx, device));
                                _logger.LogError(ex, "SQL transaction got Error!");                               
                                return BadRequest(device);
                            }
                            
                            _logger.LogInformation(result);
                            device.uuid = getId.ToString().ToUpper();
                            return CreatedAtAction(nameof(GetDevicebyMac), new { device.mac }, device );
                        }
                        else
                        {
                            _logger.LogError("Create Device FAILED!");
                            return BadRequest(device);
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unknown Exception!");
                    return BadRequest(device);
                }
            }

        }

        // PUT api/<ValuesController>/5
        [HttpPut("{mac}")]
        public ActionResult<Device> PutDevice(string mac, Device device)
        {
            try
            {
                var checkDeviceResult = _devicecypher.GetDevicebyMac(mac);

                if (checkDeviceResult == null)
                {
                    _logger.LogError("No Device Found!");
                    return BadRequest(device);
                }

                var putResult = _devicecypher.UpdateDevice(mac, device);

                if (putResult != null)
                {
                    _logger.LogInformation(string.Format("UPDATE Device {0} Success!", mac));
                    return CreatedAtAction(nameof(GetDevicebyMac), new { putResult.mac }, device);
                }
                else
                {
                    _logger.LogError("Update Device Failed!");
                    return BadRequest(device);
                }
                /*
                using (var session = _driver.Session())
                {
                    
                    else
                    {
                        session.WriteTransaction(tx => _devicecypher.UpdateDevice(tx, mac, device));
                        session.WriteTransaction(tx => _devicecypher.DeleteDeviceStatus(tx, mac, device));

                        foreach (string devicestatus in device.deviceStatus)
                        {
                            var createDeviceStatus2Device = session.WriteTransaction(tx =>
                            {
                                return _devicecypher.ConnectDeviceStatus(tx, mac, device, devicestatus).SingleOrDefault();
                            });
                            var updateDeviceStatus = createDeviceStatus2Device?[0].As<string>();

                            if (updateDeviceStatus == null)
                            {
                                _logger.LogError("A DeviceStatus is Not Exist!");
                                device.deviceStatus.Remove(devicestatus);
                            }
                            else
                            {
                                _logger.LogInformation(updateDeviceStatus);
                            }
                        }
                        return CreatedAtAction(nameof(GetDevicebyMac), new { mac }, device);
                    }
                }*/
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown Exception!");
                return BadRequest(device);
            }      
        }

        // DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public void DeleteDevice(int id)
        //{
        //}
    }
}
