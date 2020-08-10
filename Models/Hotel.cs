using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aiello_Restful_API.Models
{
    /// <summary>
    /// Hotel節點
    /// </summary>
    public class Hotel
    {
        /// <summary>
        /// HotelID 只給關連條件式綁定用，不顯示在Neo4j上面
        /// </summary>
        [Key]

        [Required]
        public string name { get; set; }

        [Required]
        public string displayName { get; set; }

        [Required]
        public string address { get; set; }

        [Required]
        public string contactPhone { get; set; }

        [Required]
        public string geo { get; set; }

        [Required]
        public string domain { get; set; }

        [Required]
        public string city { get; set; }

        public string description { get; set; }
        public string frontDeskPhone { get; set; }
        public string restaurantPhone { get; set; }
        public string sosPhone { get; set; }
        public string welcomeIntroduction { get; set; }
        public string welcomeIntroduction_cn { get; set; }
        public string welcomeIntroduction_tw { get; set; }
        public int asr { get; set; }

        [Required]
        public string createdAt { get; set; }

        [Required]
        public string updatedAt { get; set; }

    }
}
