using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOCourseSection
    {
        public ApplicationUser ApplicationUser { get; set; } 
        public decimal SumPay { get; set; }
        public decimal SumDiscount { get; set; }
        public CourseSections Course { get; set; }
        public List<Course_Trainee> Course_Trainee { get; set; }
        public List<Materials> MaterialsVideo { get; set; }
        public List<Materials> MaterialsDoc { get; set; }
        public List<Exam> Exam { get; set; }
    }
}