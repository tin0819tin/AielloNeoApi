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
using Namotion.Reflection;

namespace Aiello_Restful_API.ORM
{
    public class HotelCypher 
    {
        public readonly IDriver _driver;
        private readonly ILogger<HotelCypher> _logger;

        public HotelCypher(IDriver driver, ILogger<HotelCypher> logger)
        {
            _driver = driver;
            _logger = logger;
        }

        public IResult GetHotelCypher(ITransaction tx, string name, string domainname)
        {
            var cypherQuery = "MATCH (h:Hotel {name:$name}) RETURN h";
            var cypherQueryNew = "MATCH (d:Domain {name:$domainname})-->(h:Hotel {name:$name})-->(c:City) RETURN h as hotel, d.name as domain, c.name as city ";

            return tx.Run(cypherQueryNew, new { name, domainname});
        }

        public IResult GetHotelCypher2(ITransaction tx, string name)
        {
            var cypherQuery = "MATCH (h:Hotel {name:$name}) RETURN h as hotel";

            return tx.Run(cypherQuery, new { name});
        }

        public Hotel GetHotel(string name, string domainname)
        {
            string domain = "";
            string city = "";

            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetHotelCypher(tx, name, domainname).SingleOrDefault();
                        var hotel = queryResult?["hotel"];

                        if (queryResult == null)
                        {
                            return null;
                        }
                        city = queryResult["city"].ToString();
                        domain = queryResult["domain"].ToString();

                        return hotel.As<INode>().Properties;

                    });

                    if(getResult != null)
                    {
                        var result = JsonConvert.SerializeObject(getResult);
                        var final_result = JsonConvert.DeserializeObject<Hotel>(result);
                        final_result.domain = domain;
                        final_result.city = city;
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

        public Hotel GetHotel(string name)
        {

            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetHotelCypher2(tx, name).SingleOrDefault();
                        var hotel = queryResult?["hotel"];

                        if (queryResult == null)
                        {
                            return null;
                        }

                        return hotel.As<INode>().Properties;

                    });

                    if (getResult != null)
                    {
                        var result = JsonConvert.SerializeObject(getResult);
                        var final_result = JsonConvert.DeserializeObject<Hotel>(result);
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

        public IResult GetHotelListCypher(ITransaction tx, string city, string domain, string displayName, int asr)
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

        public List<Hotel> GetHotelList(string city, string domain, string displayName, int asr)
        {
            var listResult = new List<Hotel>();

            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetHotelListCypher(tx, city, domain, displayName, asr);

                        foreach (var record in queryResult)
                        {
                            var node = record["hotel"].As<INode>();
                            var hotelProp = node.TryGetPropertyValue<Dictionary<string, object>>("Properties");
                            var name = hotelProp.TryGetValue("name", out object value) ? value : null;
                            var displayName = hotelProp.TryGetValue("displayName", out object value1) ? value1 : "null";
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
                            var asr = hotelProp.TryGetValue("asr", out object value12) ? value12 : 0;
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

                    return getResult;
                }
                catch (Exception)
                {
                    throw;
                }               
            }

        }

        public IResult CheckDomainCypher(ITransaction tx, Hotel hotel)
        {
            var checkDomain = "MATCH (d:Domain {name:$hotel.domain}) RETURN d";

            return tx.Run(checkDomain, new { hotel });
        }

        public bool CheckDomain(Hotel hotel)
        {
            using (var session = _driver.Session())
            {
                try
                {
                    var domainResult = session.ReadTransaction(tx =>
                    {
                        return CheckDomainCypher(tx, hotel).Any();
                    });

                    return domainResult;
                }
                catch (Exception)
                {
                    throw;
                }              
            }

        }

        /// <summary>
        /// Check whether the hotel had been created under the Domain
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="hotel"></param>
        /// <returns></returns>
        public IResult FindHotel2DomainCypher(ITransaction tx, Hotel hotel)
        {
            var hotelInDomain = "MATCH (d:Domain {name:$hotel.domain})-->(h:Hotel) WITH collect(h.name) AS hotels RETURN $hotel.name IN hotels";

            return tx.Run(hotelInDomain, new { hotel });       
        }

        public bool FindHotel2Domain(Hotel hotel)
        {
            using (var session = _driver.Session())
            {
                try
                {
                    var checkHotelResult = session.WriteTransaction(tx =>
                    {
                        return FindHotel2DomainCypher(tx, hotel).Single()[0].As<bool>();
                    });

                    return checkHotelResult;
                }
                catch (Exception)
                {
                    throw;
                }
            }
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

        public IResult CreateHotelCypher(ITransaction tx, Hotel hotel)
        {
            var createHotel = "MERGE (h:Hotel {name:$hotel.name}) ON CREATE SET h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.domain = $hotel.domain, h.city = $hotel.city, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.createdAt = datetime({timezone: '+08:00'}), h.updatedAt = datetime({timezone: '+08:00'}) ";
            var createHotelNew = "CREATE (h:Hotel {name:$hotel.name}) SET h.displayName = $hotel.displayName, h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.createdAt = datetime({timezone: '+08:00'}), h.updatedAt = datetime({timezone: '+08:00'}) RETURN h.name";

            return tx.Run(createHotelNew, new { hotel });
        }

        public IResult CreateBCMnBRT2Hotel(ITransaction tx, Hotel hotel)
        {
            var connectBCM2Hotel = "MATCH (h:Hotel {name:'Aiello_Dev'}) OPTIONAL MATCH (h)-[:HAS_BROADCAST_MESSAGE]->(bcm:BroadcastMessage) WITH bcm MATCH (d:Domain {name:$hotel.domain})--(h1:Hotel {name:$hotel.name}) MERGE (h1)-[:HAS_BROADCAST_MESSAGE]->(bcm) RETURN distinct(h1)";
            var connectBRT2Hotel = "MATCH (h:Hotel {name:'Aiello_Dev'}) OPTIONAL MATCH (h)-[:REPLY_TEMPLATE]->(brt:BotResTemplate) WITH brt MATCH (h1:Hotel {name:$hotel.name}) MERGE (h1)-[:REPLY_TEMPLATE]->(brt) RETURN distinct(h1)";
            tx.Run(connectBCM2Hotel, new { hotel });

            return tx.Run(connectBRT2Hotel, new { hotel });
        }

        public Hotel CreateHotel(Hotel hotel)
        {
            using(var session = _driver.Session())
            {
                try
                {
                    var createHotelResult = "";

                    using (var tx = session.BeginTransaction())
                    {
                        var create1Result = CreateHotelCypher(tx, hotel).Single()[0].As<string>();
                        _logger.LogInformation(create1Result);

                        var createDomain2HotelResult = CreateDomain2Hotel(tx, hotel).Single()[0].As<string>();
                        _logger.LogInformation(createDomain2HotelResult);

                        var createCity2HotelResult = CreateCity2Hotel(tx, hotel).Single()[0].As<string>();
                        _logger.LogInformation(createCity2HotelResult);                        

                        var connectBCMnBRTReseult = CreateBCMnBRT2Hotel(tx, hotel).SingleOrDefault();

                        if (connectBCMnBRTReseult != null)
                        {
                            createHotelResult = JsonConvert.SerializeObject(connectBCMnBRTReseult[0].As<INode>().Properties);
                            tx.Commit();
                        }
                    }

                    //var createHotelResult = session.WriteTransaction(tx =>
                    //{
                    //    CreateHotelCypher(tx, hotel);
                    //    var createDomain2HotelResult = CreateDomain2Hotel(tx, hotel).Single()[0].As<string>();
                    //    _logger.LogInformation(createDomain2HotelResult);

                    //    var createCity2HotelResult = CreateCity2Hotel(tx, hotel).Single()[0].As<string>();
                    //    _logger.LogInformation(createCity2HotelResult);

                    //    var connectBCMnBRTReseult = CreateBCMnBRT2Hotel(tx, hotel).SingleOrDefault();

                    //    if (connectBCMnBRTReseult == null) return null;

                    //    return connectBCMnBRTReseult.As<INode>().Properties;
                    //});

                    if (createHotelResult != null)
                    {
                        var final_result = JsonConvert.DeserializeObject<Hotel>(createHotelResult);
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

        public IResult UpdateHotelCypher(ITransaction tx, string name, string domain, Hotel hotel)
        {
            var updateHotel = "MATCH (h:Hotel {name:$hotel.name}) WHERE h.domain = $hotel.domain SET h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.updatedAt = datetime({timezone: '+08:00'}) RETURN h";
            var updateHotelNew = "MATCH (d:Domain)-->(h:Hotel {name:$name})-->(c:City) WHERE d.name = $hotel.domain AND c.name = $hotel.city SET h.displayName = $hotel.displayName, h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.updatedAt = datetime({timezone: '+08:00'}) RETURN h ";

            var checkDomainandCity = "MATCH (d:Domain {name:$hotel.domain}), (c:City {name:$hotel.city}) RETURN d as domain, c as city";
            var test = tx.Run(checkDomainandCity, new { hotel });
            var check_result = test.SingleOrDefault()?["domain"];
        
            if(check_result == null)
            {
                return test;
            }
          
            var updateHotelNew2 = "MATCH (d:Domain {name:$domain})-[r1:OWN_HOTEL]->(h:Hotel {name:$name})-[r2:IS_LOCATED_AT]->(c:City) DELETE r1, r2 WITH h SET h.displayName = $hotel.displayName, h.address = $hotel.address , h.contactPhone = $hotel.contactPhone, h.geo = $hotel.geo, h.description = $hotel.description, h.frontDeskPhone = $hotel.frontDeskPhone, h.restaurantPhone = $hotel.restaurantPhone, h.sosPhone = $hotel.sosPhone,  h.welcomeIntroduction =  $hotel.welcomeIntroduction, h.welcomeIntroduction_cn = $hotel.welcomeIntroduction_cn, h.welcomeIntroduction_tw = $hotel.welcomeIntroduction_tw, h.`asr` = $hotel.`asr`, h.updatedAt = datetime({timezone: '+08:00'}) RETURN h";
            
            var test1 = tx.Run(updateHotelNew2, new { name, domain, hotel }).Single()[0];

            var createDomainAndCity = "MATCH (h:Hotel {name:$name}) MATCH (d:Domain {name:$hotel.domain}) WITH h,d WHERE NOT EXISTS ( ()-[:OWN_HOTEL]->(h) ) WITH h,d MERGE (d)-[:OWN_HOTEL]->(h) WITH h,d MATCH (c:City {name:$hotel.city}) WITH h,c,d WHERE NOT EXISTS ( (h)-[:IS_LOCATED_AT]->() ) MERGE (h)-[:IS_LOCATED_AT]->(c) RETURN h as hotel, d.name as domain, c.name as city";

            return tx.Run(createDomainAndCity, new { name, hotel });
            
        }

        public Hotel UpdateHotel(string name, string domainname, Hotel hotel)
        {
            string domain = "";
            string city = "";

            using (var session = _driver.Session())
            {
                try
                {
                    var updateResult = session.WriteTransaction(tx => {
                        var queryResult = UpdateHotelCypher(tx, name, domainname, hotel).SingleOrDefault();
                        
                        var new_hotel = queryResult?["hotel"];

                        if (queryResult == null)
                        {
                            _logger.LogError("The Domain or the City is not existed!");
                            return null;
                        }
                        city = queryResult["city"].ToString();
                        domain = queryResult["domain"].ToString();

                        return new_hotel.As<INode>().Properties;
                    });


                    if(updateResult != null)
                    {
                        var result = JsonConvert.SerializeObject(updateResult);
                        var final_result = JsonConvert.DeserializeObject<Hotel>(result);
                        final_result.city = city;
                        final_result.domain = domain;
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

        public IResult DeleteHotel(ITransaction tx, string domainname, string hotelname)
        {
            var deleteHotelbyName = "MATCH (h:Hotel {name:$hotelname}) WHERE h.domain = $domainname  DETACH DELETE h";

            return tx.Run(deleteHotelbyName, new { domainname, hotelname });
        }
    }
}
