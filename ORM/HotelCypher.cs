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
using System.Runtime.InteropServices.ComTypes;

namespace Aiello_Restful_API.ORM
{
    public class HotelCypher
    {
        public IResult GetHotel(ITransaction tx, string name, string domainname)
        {
            var cypherQuery = "MATCH (h:Hotel {name:$name}) RETURN h";
            var cypherQueryNew = "MATCH (d:Domain {name:$domainname})-->(h:Hotel {name:$name})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city ";

            return tx.Run(cypherQueryNew, new { name, domainname});
        }

        public IResult GetHotelbyDisplayName(ITransaction tx, string displayname, string domainname)
        {
            var cypherQuery = "MATCH (d:Domain {name:$domainname})-->(h:Hotel {displayName:$displayname})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city ";

            return tx.Run(cypherQuery, new { displayname, domainname });
        }

        public IResult GetHotelList(ITransaction tx, string city, string domain, string displayName, int asr)
        {
            string task = "";


            task += (city != null) ? "1" : "0";
            task += (domain != null) ? "1" : "0";
            task += (displayName != null) ? "1" : "0";
            task += ( 0 < asr && asr < 5) ? "1" : "0";

            var getHotelListNew = "";

            switch (task)
            {
                case "0000":
                    getHotelListNew = "MATCH (d:Domain)-->(h:Hotel)-->(c:City) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1000":
                    getHotelListNew = "MATCH (d:Domain)-->(h:Hotel)-->(c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0100":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})--> (h: Hotel)-- > (c:City) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0010":
                    getHotelListNew = "MATCH (d:Domain)-->(h:Hotel {displayName:$displayName})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0001":
                    getHotelListNew = "MATCH (d:Domain)-->(h:Hotel {asr:$asr})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1100":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})-->(h: Hotel)-->(c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1010":
                    getHotelListNew = "MATCH(d: Domain)--> (h: Hotel {displayName:$displayName})-- > (c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1001":
                    getHotelListNew = "MATCH(d: Domain )--> (h: Hotel {asr:$asr})-- > (c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0110":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})-->(h: Hotel {displayName:$displayName})-- > (c:City) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0101":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})-->(h: Hotel {asr:$asr})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0011":
                    getHotelListNew = "MATCH(d: Domain )--> (h: Hotel {displayName:$displayName, asr:$asr})-- > (c:City ) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1110":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})--> (h: Hotel {displayName:$displayName })-- > (c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1101":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})--> (h: Hotel {asr:$asr})-- > (c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1011":
                    getHotelListNew = "MATCH(d: Domain )--> (h: Hotel {displayName:$displayName, asr:$asr})-- > (c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "0111":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})--> (h: Hotel {displayName:$displayName, asr:$asr})-- > (c:City ) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
                case "1111":
                    getHotelListNew = "MATCH(d: Domain { name:$domain})--> (h: Hotel {displayName:$displayName, asr:$asr})-- > (c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";
                    break;
            }

            return tx.Run(getHotelListNew, new { city, domain, displayName, asr } );
        }

        public IResult GetHotelListbyCity(ITransaction tx, string city)
        {
            var getHotelList = "MATCH (d:Domain)-->(h:Hotel)-->(c:City {name:$city}) RETURN h as hotel, d.name as domain, c.name as city";

            return tx.Run(getHotelList, new { city });
        }

        public IResult GetHotelListbyDomain(ITransaction tx, string domain)
        {
            var getHotelList = "MATCH (d:Domain {name:$domain})-->(h:Hotel)-->(c:City) RETURN h as hotel, d.name as domain, c.name as city";

            return tx.Run(getHotelList, new { domain });
        }

        public IResult GetHotelListbyAsr(ITransaction tx, int asr)
        {
            var getHotelList = "MATCH (d:Domain)-->(h:Hotel {asr:$asr})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city";

            return tx.Run(getHotelList, new { asr });
        }

        public IResult CheckDomain(ITransaction tx, Hotel hotel)
        {
            var checkDomain = "MATCH (d:Domain {name:$hotel.domain}) RETURN d";

            return tx.Run(checkDomain, new { hotel });
        }

        /// <summary>
        /// Check whether the hotel had been created under the Domain
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="hotel"></param>
        /// <returns></returns>
        public IResult FindHotel2Domain(ITransaction tx, Hotel hotel)
        {
            var hotelInDomain = "MATCH (d:Domain {name:$hotel.domain})-->(h:Hotel) WITH collect(h.name) AS hotels RETURN $hotel.name IN hotels";

            return tx.Run(hotelInDomain, new { hotel });       
        }

        /// <summary>
        /// Build Domain and connect Domain to Hotel
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="hotel"></param>
        /// <returns></returns>
        public IResult CreateDomain2Hotel(ITransaction tx, Hotel hotel)
        {
            var createDomain = "MATCH (h:Hotel {name:$hotel.name}) MERGE (d:Domain {name:$hotel.domain})-[:OWN_HOTEL]->(h) RETURN 'Domain('+ d.name +') is connect to Hotel(' + h.name + ')' ";
            var createDomainNew = "MATCH (h:Hotel {name:$hotel.name}) MERGE (d:Domain {name:$hotel.domain}) MERGE (d)-[:OWN_HOTEL]->(h) RETURN 'Domain('+ d.name +') is connect to Hotel(' + h.name + ')'";
            var createDomainNew3 = "MATCH (h:Hotel {name:$hotel.name}) WITH h MERGE (d:Domain {name:$hotel.domain}) WITH d,h WHERE NOT EXISTS { MATCH ()-[:OWN_HOTEL]->(h) } WITH h,d MERGE (d)-[:OWN_HOTEL]->(h) RETURN 'Domain('+ d.name +') is connect to Hotel(' + h.name + ')' ";

            var createDomainNew4 = "MATCH (h:Hotel {name:$hotel.name}) WITH h MERGE (d:Domain {name:$hotel.domain}) WITH d,h WHERE NOT EXISTS ( ()-[:OWN_HOTEL]->(h) ) WITH h,d MERGE (d)-[:OWN_HOTEL]->(h) RETURN 'Domain('+ d.name +') is connect to Hotel(' + h.name + ')' ";

            return tx.Run(createDomainNew4, new { hotel });
        }

        /// <summary>
        /// Connect Hotel to City
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="hotel"></param>
        /// <returns></returns>
        public IResult CreateCity2Hotel(ITransaction tx, Hotel hotel)
        {
            var connectCity = "MATCH (h:Hotel {name:$hotel.name}), (c:City {name:$hotel.city}) MERGE (h)-[:IS_LOCATED_AT]->(c) RETURN 'City('+ c.name +') is connect to Hotel(' + h.name + ')' ";
            var connectCityNew = "MATCH (h:Hotel {name:$hotel.name}) MATCH (c:City {name:$hotel.city}) WHERE h.domain = $hotel.domain MERGE (h)-[:IS_LOCATED_AT]->(c) RETURN 'City('+ c.name +') is connect to Hotel(' + h.name + ')' ";
            var connectCityNew2 = "MATCH (h:Hotel {name:$hotel.name}) MATCH (c:City {name:$hotel.city}) WITH h,c WHERE NOT EXISTS { MATCH (h)-[:IS_LOCATED_AT]->() } WITH h,c MERGE (h)-[:IS_LOCATED_AT]->(c) RETURN 'City('+ c.name +') is connect to Hotel(' + h.name + ')' ";

            var connectCityNew3 = "MATCH (h:Hotel {name:$hotel.name}) MATCH (c:City {name:$hotel.city}) WITH h,c WHERE NOT EXISTS ( (h)-[:IS_LOCATED_AT]->() ) WITH h,c MERGE (h)-[:IS_LOCATED_AT]->(c) RETURN 'City('+ c.name +') is connect to Hotel(' + h.name + ')' ";

            return tx.Run(connectCityNew3, new { hotel });
        }

        public IResult CreateHotel(ITransaction tx, Hotel hotel)
        {
            var createHotel = "MERGE (h:Hotel {name:$hotel.name}) ON CREATE SET h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.domain = $hotel.domain, h.city = $hotel.city, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.createdAt = datetime({timezone: '+08:00'}), h.updatedAt = datetime({timezone: '+08:00'}) ";
            var createHotelNew = "CREATE (h:Hotel {name:$hotel.name}) SET h.displayName = $hotel.displayName, h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.createdAt = datetime({timezone: '+08:00'}), h.updatedAt = datetime({timezone: '+08:00'}) ";

            return tx.Run(createHotelNew, new { hotel });
        }

        public string CreateBCMnBRT2Hotel(Neo4j.Driver.ISession session, Hotel hotel)
        {
            var connectBCM2Hotel = "MATCH (h:Hotel {name:'Aiello_Dev'}) OPTIONAL MATCH (h)-[:HAS_BROADCAST_MESSAGE]->(bcm:BroadcastMessage) WITH bcm MATCH (h1:Hotel {name:$hotel.name}) MERGE (h1)-[:HAS_BROADCAST_MESSAGE]->(bcm) RETURN h1";
            var connectBRT2Hotel = "MATCH (h:Hotel {name:'Aiello_Dev'}) OPTIONAL MATCH (h)-[:REPLY_TEMPLATE]->(brt:BotResTemplate) WITH brt MATCH (h1:Hotel {name:$hotel.name}) MERGE (h1)-[:REPLY_TEMPLATE]->(brt) RETURN h1";
            session.WriteTransaction(tx => tx.Run(connectBCM2Hotel, new { hotel }));
            session.WriteTransaction(tx => tx.Run(connectBRT2Hotel, new { hotel }));

            return "Connect to BroadcastMessage and BotResTemplate Success!";
        }

        public IResult UpdateHotel(ITransaction tx, string name, string domain, Hotel hotel)
        {
            var updateHotel = "MATCH (h:Hotel {name:$hotel.name}) WHERE h.domain = $hotel.domain SET h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.updatedAt = datetime({timezone: '+08:00'}) RETURN h";
            var updateHotelNew = "MATCH (d:Domain)-->(h:Hotel {name:$name})-->(c:City) WHERE d.name = $hotel.domain AND c.name = $hotel.city SET h.displayName = $hotel.displayName, h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.updatedAt = datetime({timezone: '+08:00'}) RETURN h ";

            var checkDomainandCity = "MATCH (d:Domain {name:$hotel.domain}), (c:City {name:$hotel.city}) RETURN d as domain, c as city";
            var test = tx.Run(checkDomainandCity, new { hotel });
            var check_result = test.SingleOrDefault()?["domain"];
        
            if(check_result == null)
            {
                return null;
            }
          
            var updateHotelNew2 = "MATCH (d:Domain {name:$domain})-[r1:OWN_HOTEL]->(h:Hotel {name:$name})-[r2:IS_LOCATED_AT]->(c:City) DELETE r1, r2 WITH h SET h.displayName = $hotel.displayName, h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.updatedAt = datetime({timezone: '+08:00'}) RETURN h";
            
            var test1 = tx.Run(updateHotelNew2, new { name, domain, hotel }).Single()[0];

            var createDomainAndCity = "MATCH (h:Hotel {name:$name}) MATCH (d:Domain {name:$hotel.domain}) WITH h,d WHERE NOT EXISTS ( ()-[:OWN_HOTEL]->(h) ) WITH h,d MERGE (d)-[:OWN_HOTEL]->(h) WITH h,d MATCH (c:City {name:$hotel.city}) WITH h,c,d WHERE NOT EXISTS ( (h)-[:IS_LOCATED_AT]->() ) MERGE (h)-[:IS_LOCATED_AT]->(c) RETURN h as hotel, d.name as domain, c.name as city";

            return tx.Run(createDomainAndCity, new { name, hotel });
            
        }

        public IResult DeleteHotel(ITransaction tx, string domainname, string hotelname)
        {
            var deleteHotelbyName = "MATCH (h:Hotel {name:$hotelname}) WHERE h.domain = $domainname  DETACH DELETE h";

            return tx.Run(deleteHotelbyName, new { domainname, hotelname });
        }
    }
}
