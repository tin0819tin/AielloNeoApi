using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using Newtonsoft.Json.Linq;

using Aiello_Restful_API.Models;
using Aiello_Restful_API.Config;
using Aiello_Restful_API.Controllers;

namespace Aiello_Restful_API.Config
{
    public static class Neo4jDriver
    {
        public static IDriver _driver { get; private set; }

        /// <summary>
        /// Register Neo4jDriver
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// Need to add Encryption and adjust authentication
        public static void Register(string uri, string username, string password)
        {
            var authToken = AuthTokens.None;
            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(username))
                authToken = AuthTokens.Basic(username, password);
            else
                throw new ArgumentNullException();

            _driver = GraphDatabase.Driver(uri, authToken);
        }
    }
}
