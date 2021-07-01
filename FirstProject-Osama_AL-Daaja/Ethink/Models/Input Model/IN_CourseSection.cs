using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ethink.Models.Input_Model
{
    public class IN_CourseSection
    {
        public int Id { get; set; }
        [Required]
        public int Number { get; set; }
        public int IdCourse { get; set; }
        [Required]
        public int MaxCount { get; set; }
        [Required]
        public int IdTrainer { get; set; }
    }
}