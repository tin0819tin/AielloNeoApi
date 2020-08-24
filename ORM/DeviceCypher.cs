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
    public class DeviceCypher
    {
        public readonly IDriver _driver;
        private readonly ILogger<DeviceCypher> _logger;

        public DeviceCypher(IDriver driver, ILogger<DeviceCypher> logger)
        {
            _driver = driver;
            _logger = logger;
        }

        public IResult GetDeciveListCypher(ITransaction tx, string hotel, string room, string uuid, string deviceStatus)
        {
            string task = "";

            task += (hotel != null) ? 1 : 0;
            task += (room != null) ? 1 : 0;
            task += (uuid != null) ? 1 : 0;
            task += (deviceStatus != null) ? 1 : 0;

            var getDevice = "";
            var cypherBody0 = "MATCH (h:Hotel)--(:Floor)--(:Room)--(d:Device) ";
            var cypherBody = "MATCH (h:Hotel)--(:Floor)--(r:Room)--(d:Device) WHERE ";
            var cypherHotel = " h.name = $hotel ";
            var cypheRoom = " r.name = $room";
            var cypheruuid = " d.uuid = $uuid ";
            var cypherOptionalDeviceStatus = " WITH h,r,d OPTIONAL MATCH (d)-[:IS_DEVICE_STATUS_OF]-(ds:DeviceStatus) ";
            var cypherDeviceStatus = " WITH h,r,d MATCH (d)-[:IS_DEVICE_STATUS_OF]-(dss:DeviceStatus {name:$deviceStatus}) WITH h,r,d MATCH (d)-[:IS_DEVICE_STATUS_OF]-(ds:DeviceStatus ) ";
            var cypherReturn = " RETURN h.name as hotel, r.name as room, collect(distinct(d)) as device, collect(ds.name) as deviceStatuses";

            switch (task)
            {
                case "1000":
                    getDevice = cypherBody + cypherHotel +　cypherOptionalDeviceStatus + cypherReturn;
                    break;
                case "1100":
                    getDevice = cypherBody + cypherHotel + "AND" + cypheRoom + cypherOptionalDeviceStatus + cypherReturn;
                    break;
                case "0010":
                    getDevice = cypherBody + cypheruuid + cypherOptionalDeviceStatus + cypherReturn;
                    break;
                case "1001":
                    getDevice = cypherBody + cypherHotel + cypherDeviceStatus + cypherReturn;
                    break;
                case "1101":
                    getDevice = cypherBody + cypherHotel + "AND" + cypheRoom + cypherDeviceStatus + cypherReturn;
                    break;
            }

            return tx.Run(getDevice, new { hotel, room, uuid, deviceStatus });
        }

        public List<Device> GetDeviceList(string hotelName, string room, string uuid, string deviceStatus)
        {
            var listResult = new List<Device>();

            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetDeciveListCypher(tx, hotelName, room, uuid, deviceStatus);

                        foreach (var record in queryResult)
                        {
                            
                            var deviceStatuses = new HashSet<string>();
                            foreach (string deviceStatus in record["deviceStatuses"].As<List<string>>())
                            {
                                deviceStatuses.Add(deviceStatus);                                                        
                            }

                            foreach (INode node in record["device"].As<List<INode>>())
                            {
                                listResult.Add(new Device
                                {
                                    versionAPK = node["versionAPK"].As<string>(),
                                    versionImage = node["versionImage"].As<string>(),
                                    versionPushService = node["versionPushService"].As<string>(),
                                    mac = node["mac"].As<string>(),
                                    uuid = node["uuid"].As<string>(),
                                    hotel = record["hotel"].As<string>(),
                                    room = record["room"].As<string>(),
                                    deviceStatus = deviceStatuses,
                                    createdAt = node["createdAt"].As<string>(),
                                    updatedAt = node["updatedAt"].As<string>()
                                });
                            }                            
                        }

                        return (listResult);
                    });


                    return getResult;
                }
                catch(Exception)
                {
                    //return new List<Device>();
                    throw;
                }       
            }
        }

        public IResult GetDevicebyMacCypher(ITransaction tx, string mac)
        {
            var getDevice = "MATCH (d:Device {mac:$mac})-[:IS_BOUND_TO]->(r:Room)--(:Floor)--(h:Hotel) OPTIONAL MATCH (d)-[:IS_DEVICE_STATUS_OF]->(ds:DeviceStatus) RETURN d as device, r.name as room, h.name as hotel, collect(ds.name) as deviceStatus";

            return tx.Run(getDevice, new { mac });
        }

        public Device GetDevicebyMac(string mac)
        {
            string hotel = "";
            string room = "";
            var deviceStatus = new HashSet<string>();

            using (var session = _driver.Session())
            {
                var getDeviceResult = session.ReadTransaction(tx =>
                {
                    var queryResult = GetDevicebyMacCypher(tx, mac).SingleOrDefault();

                    if (queryResult == null)
                    {
                        return null;
                    }
                    var device = queryResult?["device"];
                    hotel = queryResult?["hotel"].As<string>();
                    room = queryResult?["room"].As<string>();


                    if (queryResult?["deviceStatus"] != null)
                    {
                        foreach (string devicestatus in queryResult?["deviceStatus"].As<List<string>>())
                        {
                            deviceStatus.Add(devicestatus);
                        }
                    }

                    return device.As<INode>().Properties;
                });

                if(getDeviceResult != null)
                {
                    var result = JsonConvert.SerializeObject(getDeviceResult);
                    var final_result = JsonConvert.DeserializeObject<Device>(result);
                    final_result.hotel = hotel;
                    final_result.room = room;
                    final_result.deviceStatus = deviceStatus;
                    return final_result;
                }
                else
                {
                    return null;
                }            
            }
        }

        public IResult AddDevice(ITransaction tx, Device device, string uuid)
        {
            var checkDeviceExist = "MATCH (d:Device {mac:$device.mac}) RETURN d";
            var check_result = tx.Run(checkDeviceExist, new { device });
            if(check_result.Count() != 0)
            {
                return check_result;
            }

            var createDevice = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room}) WITH r CREATE (d:Device) SET d.versionPushService = $device.versionPushService, d.versionImage = $device.versionImage, d.versionAPK = $device.versionAPK, d.uuid = $uuid, d.mac = $device.mac, d.createdAt = datetime({ timezone:'+08:00'}), d.updatedAt = datetime({ timezone:'+08:00'}) WITH r,d MERGE (d)-[:IS_BOUND_TO]->(r) RETURN 'Device('+ d.mac +') is connect to Room(' + r.name + ')' ";

            return tx.Run(createDevice, new { device, uuid }); 
        }

        public IResult DeleteDevice(ITransaction tx, Device device)
        {
            var deleteDevice = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})<-[:IS_BOUND_TO]-(d:Device {mac:$device.mac}) DETACH DELETE d";

            //using(var session = driver.Session())
            //{
            //    session.WriteTransaction(tx => tx.Run(deleteDevice));


            //}
            return tx.Run(deleteDevice, new { device });
        }

        public IResult UpdateDeviceCypher(ITransaction tx, string mac, Device device)
        {
            var updateDevice = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})--(d:Device {mac:$mac}) SET d.versionPushService = $device.versionPushService, d.versionImage = $device.versionImage, d.versionAPK = $device.versionAPK, d.updatedAt = datetime({ timezone:'+08:00'}) RETURN d";
             
            return tx.Run(updateDevice, new { mac, device });
        }

        public IResult ConnectDeviceStatus(ITransaction tx, string mac, Device device, string deviceStatus)
        {
            var connectDeviceStatus = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})--(d:Device {mac:$mac}) WITH d MATCH (ds:DeviceStatus {name:$deviceStatus}) MERGE (d)-[:IS_DEVICE_STATUS_OF]-(ds) RETURN 'Device('+ d.mac +') is connect to DeviceStatus(' + ds.name + ')', d as device";

            return tx.Run(connectDeviceStatus, new { mac, device, deviceStatus });
        }

        public IResult DeleteDeviceStatus(ITransaction tx, string mac, Device device)
        {
            var connectDeviceStatus = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})--(d:Device {mac:$mac}) WITH d MATCH (d)-[r1:IS_DEVICE_STATUS_OF]-(ds:DeviceStatus) DELETE r1 ";

            return tx.Run(connectDeviceStatus, new { mac, device });
        }

        public Device UpdateDevice(string mac, Device device)
        {
            using(var session = _driver.Session())
            {
                try
                {
                    var putResult = "";

                    using(var tx = session.BeginTransaction())
                    {
                        UpdateDeviceCypher(tx, mac, device);
                        DeleteDeviceStatus(tx, mac, device);
                        IRecord createDeviceStatus2Device = null;

                        foreach (string devicestatus in device.deviceStatus)
                        {
                            createDeviceStatus2Device = ConnectDeviceStatus(tx, mac, device, devicestatus).SingleOrDefault();
                            //var createDeviceStatus2Device = session.WriteTransaction(tx =>
                            //{
                            //    return ConnectDeviceStatus(tx, mac, device, devicestatus).SingleOrDefault();
                            //});
                            var updateDeviceStatus = createDeviceStatus2Device?[0].As<string>();

                            if (updateDeviceStatus == null)
                            {
                                _logger.LogError("A DeviceStatus is Not Exist!");
                                tx.Rollback();
                                tx.Dispose();
                            }
                            else
                            {
                                _logger.LogInformation(updateDeviceStatus);
                            }
                        }

                        if(createDeviceStatus2Device != null)
                        {
                            putResult = JsonConvert.SerializeObject(createDeviceStatus2Device?["device"].As<INode>().Properties);
                            tx.Commit();
                        }
                    }

                    if(putResult != "")
                    {
                        var final_result = JsonConvert.DeserializeObject<Device>(putResult);
                        return final_result;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

    }
}
