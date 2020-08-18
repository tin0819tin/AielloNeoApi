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
        public IResult GetDeciveList(ITransaction tx, string hotel, string room, string uuid, string deviceStatus)
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
            var cypherReturn = " RETURN h.name as hotel, r.name as room, d as device, collect(ds.name) as deviceStatuses";

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
            }

            return tx.Run(getDevice, new { hotel, room, uuid, deviceStatus });
        }

        public IResult GetDevicebyMac(ITransaction tx, string mac)
        {
            var getDevice = "MATCH (d:Device {mac:$mac})-[:IS_BOUND_TO]->(r:Room)--(:Floor)--(h:Hotel) OPTIONAL MATCH (d)-[:IS_DEVICE_STATUS_OF]->(ds:DeviceStatus) RETURN d as device, r.name as room, h.name as hotel, ds.name as deviceStatus";

            return tx.Run(getDevice, new { mac });
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

        public void DeleteDevice(IDriver driver)
        {
            var deleteDevice = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})<-[:IS_BOUND_TO]-(d:Device {mac:$device.mac}) DETACH DELETE d";
            
            using(var session = driver.Session())
            {
                session.WriteTransaction(tx => tx.Run(deleteDevice));


            }

        }

        public IResult UpdateDevice(ITransaction tx, string mac, Device device)
        {
            var updateDevice = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})--(d:Device {mac:$mac}) SET d.versionPushService = $device.versionPushService, d.versionImage = $device.versionImage, d.versionAPK = $device.versionAPK, d.updatedAt = datetime({ timezone:'+08:00'}) RETURN d";
             
            return tx.Run(updateDevice, new { mac, device });
        }

        public IResult ConnectDeviceStatus(ITransaction tx, string mac, Device device, string deviceStatus)
        {
            var connectDeviceStatus = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})--(d:Device {mac:$mac}) WITH d MATCH (ds:DeviceStatus {name:$deviceStatus}) MERGE (d)-[:IS_DEVICE_STATUS_OF]-(ds) RETURN 'Device('+ d.mac +') is connect to DeviceStatus(' + ds.name + ')'";

            return tx.Run(connectDeviceStatus, new { mac, device, deviceStatus });
        }

        public IResult DeleteDeviceStatus(ITransaction tx, string mac, Device device)
        {
            var connectDeviceStatus = "MATCH (h:Hotel {name:$device.hotel})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$device.room})--(d:Device {mac:$mac}) WITH d MATCH (d)-[r1:IS_DEVICE_STATUS_OF]-(ds:DeviceStatus) DELETE r1 ";

            return tx.Run(connectDeviceStatus, new { mac, device });
        }

    }
}
