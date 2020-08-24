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
    public class DeviceStatusCypher
    {
        public readonly IDriver _driver;

        public DeviceStatusCypher(IDriver driver)
        {
            _driver = driver;
        }

        public IResult GetDeviceStatus(ITransaction tx, string name)
        {
            var getDeviceStatus = "MATCH (ds:DeviceStatus {name:$name}) RETURN ds as devicestatus";

            return tx.Run(getDeviceStatus, new { name });

        }

        public IResult GetDeviceStatusList(ITransaction tx)
        {
            var getDeviceStatusList = "MATCH (ds:DeviceStatus) RETURN ds as devicestatus";

            return tx.Run(getDeviceStatusList);
        }


        public IResult AddDeviceStatus(ITransaction tx, DeviceStatus devicestatus)
        {
            var createDeviceStatus = "MERGE (ds:DeviceStatus {name:$devicestatus.name}) ON CREATE SET ds.createdAt = datetime({timezone: '+08:00'}), ds.updatedAt = datetime({timezone: '+08:00'}) RETURN ds";

            return tx.Run(createDeviceStatus, new { devicestatus });
        }

        public IResult UpdateDeviceStatus(ITransaction tx, string name, DeviceStatus devicestatus)
        {
            var updateDeviceStatus = "MATCH (ds:DeviceStatus {name:$name}) SET ds.name = $devicestatus.name, ds.updatedAt = datetime({timezone: '+08:00'}) RETURN ds";

            return tx.Run(updateDeviceStatus, new { name, devicestatus });
        }

        public IResult DeleteDeviceStatus(ITransaction tx, string name)
        {
            var deleteDeviceStatusbyName = "MATCH (ds:DeviceStatus {name:$name}) DETACH DELETE ds";

            return tx.Run(deleteDeviceStatusbyName, new { name });
        }

    }
}
