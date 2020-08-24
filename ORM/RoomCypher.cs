using Aiello_Restful_API.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Aiello_Restful_API.ORM
{
    public class RoomCypher
    {
        public readonly IDriver _driver;
        private readonly ILogger<RoomCypher> _logger;

        public RoomCypher(IDriver driver, ILogger<RoomCypher> logger)
        {
            _driver = driver;
            _logger = logger;
        }

        public IResult GetRoombyNameCypher(ITransaction tx, string name, string hotelname)
        {
            var getRoombyName = "MATCH (h:Hotel {name:$hotelname})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[:IS_ROOM_STATE_OF]->(rs:RoomState) WITH r,h,f,rs OPTIONAL MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType) RETURN r as room, h.name as hotelName, f.name as floor, rt.name as roomtype, collect(rs.name) as roomstates";

            return tx.Run(getRoombyName, new { name, hotelname });
        }

        public IResult GetRoombyNameNoRoomType(ITransaction tx, string name, string hotelname)
        {
            var getRoombyName = "MATCH (h:Hotel {name:$hotelname})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[:IS_ROOM_STATE_OF]->(rs:RoomState) RETURN r as room, h.name as hotelName, f.name as floor, collect(rs.name) as roomstates, 'No Type' as roomtype";

            return tx.Run(getRoombyName, new { name, hotelname });
        }

        public Room GetRoombyName(string name, string hotelname)
        {
            string floor = "";
            string roomtype = "";
            string hotelName = "";
            var roomStates = new HashSet<string>();

            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetRoombyNameCypher(tx, name, hotelname).SingleOrDefault();

                        if (queryResult == null)
                        {
                            return null;
                        }

                        var room = queryResult?["room"];
                        floor = queryResult["floor"].ToString();
                        if (queryResult?["roomtype"] != null)
                        {
                            roomtype = queryResult?["roomtype"].ToString();
                        }
                        hotelName = queryResult["hotelName"].ToString();
                        foreach (string roomState in queryResult["roomstates"].As<List<string>>())
                        {
                            roomStates.Add(roomState);
                        }

                        return room.As<INode>().Properties;

                    });

                    if (getResult != null)
                    {
                        var result = JsonConvert.SerializeObject(getResult);
                        var final_result = JsonConvert.DeserializeObject<Room>(result);
                        final_result.floor = floor;
                        final_result.roomType = roomtype;
                        final_result.hotelName = hotelName;
                        final_result.roomStates = roomStates;
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

        public IResult GetRoomListCypher(ITransaction tx, string hotelname, string floor, string roomState, string roomType)
        {
            string task = "";

            task += (hotelname != null) ? 1 : 0;
            task += (floor != null) ? 1 : 0;
            task += (roomState != null) ? 1 : 0;
            task += (roomType != null) ? 1 : 0;

            var getRoomList = "";
            var cypherBody0 = "MATCH (h:Hotel {name:$hotelname})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room)-[:IS_ROOM_STATE_OF]->(rs:RoomState) ";
            var cypherBody = "MATCH (h:Hotel {name:$hotelname})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room)-[:IS_ROOM_STATE_OF]->(rs:RoomState) WHERE ";
            var cypherFloor = "f.name = $floor";
            var cypherRoomState = "rs.name = $roomState";
            var cypherAllRoomState = " WITH r,h,f,rt MATCH (r)-[:IS_ROOM_STATE_OF]->(rs:RoomState) ";
            var cypherOptionalRoomType = "WITH r,h,f,rs OPTIONAL MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType)";
            var cypherRoomType = "WITH r,h,f,rs MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$roomType})";
            var cypherReturn = " RETURN r as room, h.name as hotelName, f.name as floor, rt.name as roomType, collect(rs.name) as roomStates";

            switch (task)
            {
                case "1000":
                    getRoomList = cypherBody0 + cypherOptionalRoomType + cypherReturn;
                    break;
                case "1100":
                    getRoomList = cypherBody + cypherFloor + ' ' + cypherOptionalRoomType + cypherReturn;
                    break;
                case "1010":
                    getRoomList = cypherBody + cypherRoomState + ' ' + cypherOptionalRoomType + cypherAllRoomState + cypherReturn;
                    break;
                case "1001":
                    getRoomList = cypherBody0 + cypherRoomType + cypherReturn;
                    break;
                case "1110":
                    getRoomList = cypherBody + cypherFloor + " AND " + cypherRoomState + ' ' + cypherOptionalRoomType + cypherAllRoomState + cypherReturn;
                    break;
                case "1101":
                    getRoomList = cypherBody + cypherFloor + " " + cypherRoomType + cypherReturn;
                    break;
                case "1011":
                    getRoomList = cypherBody + cypherRoomState + " " + cypherRoomType + cypherAllRoomState + cypherReturn;
                    break;
                case "1111":
                    getRoomList = cypherBody + cypherRoomState + " AND " + cypherRoomState + " " + cypherRoomType + cypherAllRoomState + cypherReturn;
                    break;
            }


            return tx.Run(getRoomList, new { hotelname, floor, roomState, roomType });
        }

        public List<Room> GetRoomList(string hotelname, string floor, string roomState, string roomType)
        {
            var listResult = new List<Room>();

            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetRoomListCypher(tx, hotelname, floor, roomState, roomType);

                        foreach (var record in queryResult)
                        {
                            var node = record["room"].As<INode>();
                            var roomStates = new HashSet<string>();
                            foreach (string roomState in record["roomStates"].As<List<string>>())
                            {
                                roomStates.Add(roomState);
                            }

                            listResult.Add(new Room
                            {
                                name = node["name"].As<string>(),
                                hotelName = record["hotelName"].As<string>(),
                                floor = record["floor"].As<string>(),
                                roomStates = roomStates,
                                roomType = record["roomType"].As<string>(),
                                createdAt = node["createdAt"].As<string>(),
                                updatedAt = node["updatedAt"].As<string>()
                            });
                        }

                        return (listResult);
                    });

                    return getResult;
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }

        public IResult ConnectRoomState2Room(ITransaction tx, string name, Room room, string roomState)
        {
            var createRoomState = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$name}) WITH r MATCH (rs:RoomState {name:$roomState}) WITH r, rs MERGE (r)-[:IS_ROOM_STATE_OF]->(rs) WITH r,rs SET r.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomState('+ rs.name +') is connect to Room(' + r.name + ')', r as room";

            return tx.Run(createRoomState, new { room, roomState, name });
        }

        public IResult ConnectRoomType2Room(ITransaction tx, Room room)
        {

            var checkRoomTypeExisted = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) RETURN distinct(rt.name) as roomtype";
            var test = tx.Run(checkRoomTypeExisted, new { room });
            var check_result = test.SingleOrDefault()?["roomtype"];

            if (check_result == null)
            {
                var createRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room { name:$room.name}) WITH r CREATE (rt:RoomType {name:$room.roomType}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET rt.createdAt = datetime({timezone: '+08:00'}), rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')' ";

                return tx.Run(createRoomType, new { room });
            }
            else
            {
                var upddateRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) WITH distinct(rt) MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor })<-[:IS_FLOOR_AT]-(r:Room { name:$room.name}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')'";
                return tx.Run(upddateRoomType, new { room });
            }

        }

        public IResult BuildFloor2Hotel(ITransaction tx, Room room)
        {

            var checkFloor = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor}) RETURN f";
            var checkResult = tx.Run(checkFloor, new { room });

            //CREATE Floor and Build connection with Hotel
            var buildFloor2Hotel = "CREATE (f:Floor {name:$room.floor}) SET f.createdAt = datetime({timezone: '+08:00'}), f.updatedAt = datetime({timezone: '+08:00'}) WITH f MATCH (h:Hotel {name:$room.hotelName}) WHERE NOT EXISTS { ()-[:HAS_FLOOR]->(f)} MERGE (h)-[:HAS_FLOOR]->(f) RETURN 'Floor('+ f.name +') is connect to Hotel(' + h.name + ')' ";
            var buildFloor2HotelNew = "CREATE (f:Floor {name:$room.floor}) SET f.createdAt = datetime({timezone: '+08:00'}), f.updatedAt = datetime({timezone: '+08:00'}) WITH f MATCH (h:Hotel {name:$room.hotelName}) WHERE NOT EXISTS ( ()-[:HAS_FLOOR]->(f)) MERGE (h)-[:HAS_FLOOR]->(f) RETURN 'Floor('+ f.name +') is connect to Hotel(' + h.name + ')' ";
            if (checkResult.Count() == 0)
            {
                return tx.Run(buildFloor2HotelNew, new { room });
            }
            else
            {
                return checkResult;
            }
        }

        public IResult CreateRoomCypher(ITransaction tx, Room room)
        {
            var checkRoomUnderFloor = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$room.name}) RETURN r";
            var checkResult = tx.Run(checkRoomUnderFloor, new { room });


            var buildFloor2Room = "CREATE(r:Room { name:$room.name}) SET r.createdAt = datetime({ timezone:'+08:00'}), r.updatedAt = datetime({ timezone:'+08:00'}) WITH r MATCH (h:Hotel { name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor}) WHERE NOT EXISTS { MATCH (r)-[:IS_FLOOR_AT]->() <-[:HAS_FLOOR]-(h) } MERGE (r)-[:IS_FLOOR_AT]->(f) RETURN 'Floor('+ f.name +') is connect to Room(' + r.name + ')'";
            var buildFloor2RoomNew = "CREATE(r:Room { name:$room.name}) SET r.createdAt = datetime({ timezone:'+08:00'}), r.updatedAt = datetime({ timezone:'+08:00'}) WITH r MATCH (h:Hotel { name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor}) WHERE NOT EXISTS ( (r)-[:IS_FLOOR_AT]->() <-[:HAS_FLOOR]-(h) ) MERGE (r)-[:IS_FLOOR_AT]->(f) RETURN 'Floor('+ f.name +') is connect to Room(' + r.name + ')'";

            if (checkResult.Count() == 0)
            {
                return tx.Run(buildFloor2RoomNew, new { room });
            }
            else
            {
                return checkResult;
            }

        }

        public Room CreateRoom(Room room)
        {
            using(var session = _driver.Session())
            {
                try
                {
                    var createRoomResult = "";

                    using(var tx = session.BeginTransaction())
                    {
                        var createFloor2HotelResult = BuildFloor2Hotel(tx, room).SingleOrDefault();

                        var result1 = createFloor2HotelResult?[0].As<string>();

                        if (result1 == null)
                        {
                            _logger.LogInformation("The Floor is under the Hotel already!");
                        }
                        else
                        {
                            _logger.LogInformation(result1);
                        }

                        var createRoom2FloorResult = CreateRoomCypher(tx, room).SingleOrDefault();

                        var result2 = createRoom2FloorResult?[0].As<string>();

                        if (result2 == null)
                        {
                            _logger.LogError("Same Room is Existed already!");
                            tx.Rollback();
                        }
                        else
                        {
                            _logger.LogInformation(result2);
                        }

                        if (room.roomType != null)
                        {
                            //Add RoomType
                            var createRoomType2RoomResult = ConnectRoomType2Room(tx, room).SingleOrDefault();

                            _logger.LogInformation(createRoomType2RoomResult[0].As<string>());
                        }

                        string roomState = "Available";
                        var createRoomState2RoomResult = ConnectRoomState2Room(tx, room.name, room, roomState).SingleOrDefault();

                        var result3 = createRoomState2RoomResult?[0].As<string>();

                        _logger.LogInformation(result3);


                        if(createRoomState2RoomResult != null)
                        {
                            createRoomResult = JsonConvert.SerializeObject(createRoomState2RoomResult?["room"].As<INode>().Properties);
                            tx.Commit();
                        }     
                    }

                    if (createRoomResult != null)
                    {
                        var final_result = JsonConvert.DeserializeObject<Room>(createRoomResult);
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

        public IResult UpdateRoomFloor(ITransaction tx, string name, Room room)
        {
            var checkFloor = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor}) RETURN f as floor";
            var check_result1 = tx.Run(checkFloor, new { room });
            var floorcheck = check_result1.Count();

            var checkRoomUnderFloor = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$room.name}) RETURN r";
            var check_result2 = tx.Run(checkRoomUnderFloor, new { room });
            var roomcheck = check_result2.Count();
            
            var updateRoom2Floor = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[r1:IS_FLOOR_AT]-(r:Room {name:$name}) DELETE r1 WITH r,h  SET r.name = $room.name, r.updatedAt = datetime({ timezone:'+08:00'})  WITH r,h MATCH (h)-[:HAS_FLOOR]->(f:Floor {name:$room.floor}) WHERE NOT EXISTS { MATCH (r)-[:IS_FLOOR_AT]->() <-[:HAS_FLOOR]-(h) } MERGE (r)-[:IS_FLOOR_AT]->(f) RETURN 'Update Success : Floor('+ f.name +') is connect to Room(' + r.name + ')'";

            var updateRoom2FloorNew = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[r1:IS_FLOOR_AT]-(r:Room {name:$name}) DELETE r1 WITH r,h  SET r.name = $room.name, r.updatedAt = datetime({ timezone:'+08:00'})  WITH r,h MATCH (h)-[:HAS_FLOOR]->(f:Floor {name:$room.floor}) WHERE NOT EXISTS  ((r)-[:IS_FLOOR_AT]->() <-[:HAS_FLOOR]-(h) ) MERGE (r)-[:IS_FLOOR_AT]->(f) RETURN 'Update Success : Floor('+ f.name +') is connect to Room(' + r.name + ')'";


            if (floorcheck == 0)
            {
                return check_result1;
            }
            else if (roomcheck > 0 && name != room.name )
            {
                return check_result2;
            }
            else if(roomcheck > 0 && name == room.name)
            {
                return tx.Run("RETURN 'No Floor and Name UPDATE!' ");
            }
            else
            {
                return tx.Run(updateRoom2FloorNew, new { name, room });
            }
        }

        public IResult DeleteRoomState(ITransaction tx, string name, Room room)
        {
            var deleteRoomState = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[r1:IS_ROOM_STATE_OF]->(rso:RoomState) DELETE r1";

            return tx.Run(deleteRoomState, new { room, name });

        }


        public IResult UpdateRoomType(ITransaction tx, string name, Room room)
        {
            var checkRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$name})--(rto:RoomType) WITH rto MATCH (rto)<-[:IS_ROOM_TYPE_OF]-(rs:Room) RETURN rs";
            var check_result1 = tx.Run(checkRoomType, new { room, name });
            var rest_node_num = check_result1.Count();

            var deleteRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[r1:IS_ROOM_TYPE_OF]->(rto:RoomType) DELETE r1 ";
            var deleteRoomTypeNode = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[r1:IS_ROOM_TYPE_OF]->(rto:RoomType) DELETE r1 WITH rto DELETE rto";

            if (rest_node_num == 1 ) 
            {
                tx.Run(deleteRoomTypeNode, new { room, name });
            }
            else if(rest_node_num > 1)
            {
                tx.Run(deleteRoomType, new { room, name });
            }

            var checkRoomTypeExisted = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) RETURN distinct(rt.name) as roomtype";
            var test = tx.Run(checkRoomTypeExisted, new { room });
            var check_result2 = test.SingleOrDefault()?["roomtype"];

            if (check_result2 == null)
            {
                var createRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room { name:$name}) WITH r CREATE (rt:RoomType {name:$room.roomType}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET r.updatedAt = datetime({timezone: '+08:00'}), rt.createdAt = datetime({timezone: '+08:00'}), rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')' ";

                return tx.Run(createRoomType, new { room, name });
            }
            else
            {
                var upddateRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) WITH distinct(rt) MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor })<-[:IS_FLOOR_AT]-(r:Room { name:$name}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET r.updatedAt = datetime({timezone: '+08:00'}), rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')'";
                return tx.Run(upddateRoomType, new { room, name });
            }
       
        }

        public Room UpdateRoom(string name, Room room)
        {
            using(var session = _driver.Session())
            {
                try
                {
                    var putRoomResult = "";

                    using (var tx = session.BeginTransaction())
                    {
                        if (room.roomType != null)
                        {
                            //Add RoomType
                            var createRoomType2RoomResult = UpdateRoomType(tx, name, room).SingleOrDefault();

                            _logger.LogInformation(createRoomType2RoomResult[0].As<string>());
                        }

                        HashSet<string> set1 = new HashSet<string>() { "Available", "Unavailable", "Occupied" };
                        HashSet<string> set2 = new HashSet<string>() { "MUR", "DND" };
                        HashSet<string> set3 = new HashSet<string>() { "Complaint", "Electricity", "Service", "InService", "Repair" };
                        HashSet<string> set4 = new HashSet<string>() { "Unavailable" };

                        var num1 = room.roomStates.Intersect(set1).Count();
                        var num2 = room.roomStates.Intersect(set2).Count();
                        var num3 = room.roomStates.Intersect(set3).Count();
                        var num4 = room.roomStates.Intersect(set4).Count();
                        IRecord createRoomState2RoomResult = null;

                        if (num1 > 1 || num2 > 1 || (num1 + num2 + num3) == 0 || num2 * num4 != 0)
                        {
                            _logger.LogError("RoomStates exist conflict!");
                            tx.Rollback();
                        }
                        else
                        {
                            DeleteRoomState(tx, name, room);

                            
                            foreach (string roomState in room.roomStates)
                            {
                                createRoomState2RoomResult = ConnectRoomState2Room(tx, name, room, roomState).SingleOrDefault();
                                var updateRoomState = createRoomState2RoomResult?[0].As<string>();

                                if (updateRoomState == null)
                                {
                                    _logger.LogError("A RoomState is Not Exist!");
                                    tx.Rollback();
                                    tx.Dispose();
                                }
                                else
                                {
                                    _logger.LogInformation(updateRoomState);
                                }
                            }
                        }

                        if(createRoomState2RoomResult != null)
                        {
                            putRoomResult = JsonConvert.SerializeObject(createRoomState2RoomResult?["room"].As<INode>().Properties);
                            tx.Commit();
                        }  
                    }
                    
                    if(putRoomResult != "")
                    {
                        var final_result = JsonConvert.DeserializeObject<Room>(putRoomResult);
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
