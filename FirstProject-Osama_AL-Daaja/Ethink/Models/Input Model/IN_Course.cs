using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ethink.Models.Input_Model
{
    public class IN_Course
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Name")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Category")]
        public int IdCategory { get; set; }
        [Required]
        [DisplayName("StartDate")]
        [DisplayFormat(DataFormatString = "{dd MMM yyyy}")]
        public System.DateTime StartDate { get; set; }
        [Required]
        [DisplayName("EndDate")]
        [DisplayFormat(DataFormatString = "{dd MMM yyyy}")]
        public System.DateTime EndDate { get; set; }
        [Required]
        [DisplayName("Price")]
        public decimal Price { get; set; }
        [DisplayName("Image")]
        public string Image { get; set; }
    }
}