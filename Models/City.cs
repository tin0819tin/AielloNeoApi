using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Aiello_Restful_API.Models
{
    public class City
    {
        [Required]
        public string name { get; set; }
        public string timeZone { get; set; }
        public string name_cn { get; set; }
        public string name_tw { get; set; }

        [Required]
        public string createdAt { get; set; }

        [Required]
        public string updatedAt { get; set; }

    }
}
