using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Aiello_Restful_API.Models
{
    public class Room
    {
        [Key]

        [Required]
        public string name { get; set; }

        [Required]
        public string hotelName { get; set; }


        [Required]
        public string floor { get; set; }

        [Required]
        public HashSet<string> roomStates { get; set; }


        public string roomType { get; set; }

        
        public string createdAt { get; set; }

        
        public string updatedAt { get; set; }

    }

}
