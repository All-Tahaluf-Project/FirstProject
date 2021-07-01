using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOAdminPage
    {
        public int CountMaleTrainer { get; set; }
        public int CountFemaleTrainer { get; set; }
        public int CountCourse { get; set; }
        public int CountCourseSection { get; set; }
        public int CountMaleTrainee { get; set; }
        public int CountFemaleTrainee { get; set; }

        public List<Course> CourseRequests { get; set; }

    }
}