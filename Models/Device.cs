using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using NLog;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Aiello_Restful_API.ORM;

namespace Aiello_Restful_API.Models
{
    public class Device
    {

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string versionPushService { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string versionImage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string versionAPK { get; set; }

        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.All)]
        [JsonConverter(typeof(GuidConverter))]
        public string uuid { get ; set;}

        /*
        public string uuid { get { return uuid; }
            set
            {
                var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();
                try
                {
                    Guid.Parse(uuid);
                }
                catch (ArgumentNullException)
                {
                    //throw;//
                    logger.Error("The string to be parsed is null.");
                }
                catch (FormatException)
                {
                    //throw;//
                    logger.Error($"Error Guid Format: {uuid}!");
                }
            }
        }*/

        /*
        public string property
        { 
            get { return uuid;  } 
            set {
                
                try
                {
                    Guid.Parse(uuid);
                }
                catch (ArgumentNullException)
                {
                    _logger.LogError("The string to be parsed is null.");
                }
                catch (FormatException)
                {
                    _logger.LogError($"Error Guid Format: {uuid}!");
                }
            } 
        }*/

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string mac { get; set; }

        [Required]
        public string hotel { get; set; }

        [Required]
        public string room { get; set; }

        public HashSet<string> deviceStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string createdAt { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string updatedAt { get; set; }
    }
}
