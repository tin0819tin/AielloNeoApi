﻿using System;
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
    public class CityCypher
    {
        public readonly IDriver _driver;

        public CityCypher(IDriver driver)
        {
            _driver = driver;
        }

        public IResult GetCityCypher(ITransaction tx, string name)
        {
            var getCity = "MATCH (c:City {name:$name}) RETURN c as city";

            return tx.Run(getCity, new { name });

        }

        public City GetCity(string name)
        {
                     
            using (var session = _driver.Session())
            {
                try
                {
                    var getResult = session.ReadTransaction(tx =>
                    {
                        var queryResult = GetCityCypher(tx, name).SingleOrDefault();
                        var city = queryResult?["city"];

                        if (queryResult == null)
                        {
                            return null;
                        }
                        return city.As<INode>().Properties;

                    });

                    if(getResult != null)
                    {
                        var result = JsonConvert.SerializeObject(getResult);
                        var final_result = JsonConvert.DeserializeObject<City>(result);
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

        public IResult GetCityList(ITransaction tx)
        {
            var getCityList = "MATCH (c:City) RETURN c as city";

            return tx.Run(getCityList);
        }


        public IResult AddCity(ITransaction tx, City city)
        {
            var createCity = "MERGE (c:City {name:$city.name}) ON CREATE SET c.name = $city.name, c.timeZone = $city.timeZone, c.name_cn = $city.name_cn, c.name_tw = $city.name_tw, c.createdAt = datetime({timezone: '+08:00'}), c.updatedAt = datetime({timezone: '+08:00'}) RETURN c";

            return tx.Run(createCity, new { city });
        }

        public IResult UpdateCity(ITransaction tx, City city)
        {
            var updateCity = "MATCH (c:City {name:$city.name}) SET c.name = $city.name, c.timeZone = $city.timeZone, c.name_cn = $city.name_cn, c.name_tw = $city.name_tw, c.updatedAt = datetime({timezone: '+08:00'}) RETURN c";

            return tx.Run(updateCity, new { city });
        }

        public IResult DeleteCity(ITransaction tx, string name)
        {
            var deleteCitybyName = "MATCH (c:City {name:$name}) DETACH DELETE c";

            return tx.Run(deleteCitybyName, new { name });
        }

    }
}
