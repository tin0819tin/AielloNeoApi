using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Aiello_Restful_API.Models
{
    public class DeviceStatus
    {
        [Required]
        public string name { get; set; }

        public string createdAt { get; set; }

        public string updatedAt { get; set; }

    }
}
