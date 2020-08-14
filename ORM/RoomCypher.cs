using Aiello_Restful_API.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4j.Driver;
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
        public IResult GetRoombyName(ITransaction tx, string name, string hotelname)
        {
            var getRoombyName = "MATCH (h:Hotel {name:$hotelname})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[:IS_ROOM_TYPE_OF]->(rt:RoomType) WITH r,h,f,rt MATCH (r)-[:IS_ROOM_STATE_OF]->(rs:RoomState) RETURN r as room, h.name as hotelName, f.name as floor, rt.name as roomtype, collect(rs.name) as roomstates";
            
            return tx.Run(getRoombyName, new { name, hotelname });
        }

        public IResult GetRoombyNameNoRoomType(ITransaction tx, string name, string hotelname)
        {
            var getRoombyName = "MATCH (h:Hotel {name:$hotelname})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room {name:$name})-[:IS_ROOM_STATE_OF]->(rs:RoomState) RETURN r as room, h.name as hotelName, f.name as floor, collect(rs.name) as roomstates, 'No Type' as roomtype";

            return tx.Run(getRoombyName, new { name, hotelname });
        }

        public IResult GetRoomList(ITransaction tx, string hotelname, string floor, string roomState, string roomType)
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
                    getRoomList = cypherBody + cypherRoomState + ' ' + cypherOptionalRoomType + cypherReturn; 
                    break;
                case "1001":
                    getRoomList = cypherBody0 + cypherRoomType + cypherReturn; 
                    break;
                case "1110":
                    getRoomList = cypherBody + cypherFloor + " AND " + cypherRoomState + ' ' + cypherOptionalRoomType + cypherReturn;
                    break;
                case "1101":
                    getRoomList = cypherBody + cypherFloor + " " + cypherRoomType + cypherReturn;
                    break;
                case "1011":
                    getRoomList = cypherBody + cypherRoomState + " " + cypherRoomType + cypherReturn;
                    break;
                case "1111":
                    getRoomList = cypherBody + cypherRoomState + " AND " + cypherRoomState + " " + cypherRoomType + cypherReturn;
                    break;
            }

            
            return tx.Run(getRoomList, new { hotelname, floor, roomState, roomType });
        }

        public IResult ConnectRoomState2Room(ITransaction tx, string name, Room room, string roomState)
        {
            var createRoomState = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor {name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room {name:$name}) WITH r MATCH (rs:RoomState {name:$roomState}) WITH r, rs MERGE (r)-[:IS_ROOM_STATE_OF]->(rs) WITH r,rs SET r.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomState('+ rs.name +') is connect to Room(' + r.name + ')'";

            return tx.Run(createRoomState, new { room, roomState, name });
        }

        public IResult ConnectRoomType2Room(ITransaction tx, Room room)
        {

            var checkRoomTypeExisted = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) RETURN rt as roomtype";
            var test = tx.Run(checkRoomTypeExisted, new { room });
            var check_result = test.SingleOrDefault()?["roomtype"];

            if(check_result == null)
            {
                var createRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room { name:$room.name}) WITH r CREATE (rt:RoomType {name:$room.roomType}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET rt.createdAt = datetime({timezone: '+08:00'}), rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')' ";

                return tx.Run(createRoomType, new { room });
            }
            else
            {
                var upddateRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) WITH rt MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor })<-[:IS_FLOOR_AT]-(r:Room { name:$room.name}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')'";
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

        public IResult CreateRoom(ITransaction tx, Room room)
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

            var checkRoomTypeExisted = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) RETURN rt as roomtype";
            var test = tx.Run(checkRoomTypeExisted, new { room });
            var check_result2 = test.SingleOrDefault()?["roomtype"];

            if (check_result2 == null)
            {
                var createRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor})<-[:IS_FLOOR_AT]-(r:Room { name:$name}) WITH r CREATE (rt:RoomType {name:$room.roomType}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET r.updatedAt = datetime({timezone: '+08:00'}), rt.createdAt = datetime({timezone: '+08:00'}), rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')' ";

                return tx.Run(createRoomType, new { room, name });
            }
            else
            {
                var upddateRoomType = "MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor)<-[:IS_FLOOR_AT]-(r:Room) WITH r MATCH (r)-[:IS_ROOM_TYPE_OF]->(rt:RoomType {name:$room.roomType}) WITH rt MATCH (h:Hotel {name:$room.hotelName})-[:HAS_FLOOR]->(f:Floor { name:$room.floor })<-[:IS_FLOOR_AT]-(r:Room { name:$name}) WITH r,rt MERGE (r)-[:IS_ROOM_TYPE_OF]->(rt) WITH r,rt SET r.updatedAt = datetime({timezone: '+08:00'}), rt.updatedAt = datetime({timezone: '+08:00'}) RETURN 'RoomType('+ rt.name +') is connect to Room(' + r.name + ')'";
                return tx.Run(upddateRoomType, new { room, name });
            }
       
        }
    }
}
