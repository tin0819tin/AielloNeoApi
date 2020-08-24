using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
    public class RoomStateCypher
    {
        public readonly IDriver _driver;

        public RoomStateCypher(IDriver driver)
        {
            _driver = driver;
        }

        public IResult GetRoomState(ITransaction tx, string name)
        {
            var getRoomState = "MATCH (r:RoomState {name:$name}) RETURN r as roomstate";

            return tx.Run(getRoomState, new { name });

        }

        public IResult GetRoomStateList(ITransaction tx)
        {
            var getRoomStateList = "MATCH (r:RoomState) RETURN r as roomstate";

            return tx.Run(getRoomStateList);
        }


        public IResult AddRoomState(ITransaction tx, RoomState roomstate)
        {
            var createRoomState = "MERGE (r:RoomState {name:$roomstate.name}) ON CREATE SET r.createdAt = datetime({timezone: '+08:00'}), r.updatedAt = datetime({timezone: '+08:00'}) RETURN r";

            return tx.Run(createRoomState, new { roomstate });
        }

        public IResult UpdateRoomState(ITransaction tx, string name, RoomState roomstate)
        {
            var updateRoomState = "MATCH (r:RoomState {name:$name}) SET r.name = $roomstate.name, r.updatedAt = datetime({timezone: '+08:00'}) RETURN r";

            return tx.Run(updateRoomState, new { name, roomstate });
        }

        public IResult DeleteRoomState(ITransaction tx, string name)
        {
            var deleteRoomStatebyName = "MATCH (r:RoomState {name:$name}) DETACH DELETE r";

            return tx.Run(deleteRoomStatebyName, new { name });
        }
    }
}
