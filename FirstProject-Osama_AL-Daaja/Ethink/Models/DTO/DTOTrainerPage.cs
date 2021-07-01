using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOTrainerPage
    {
        public int CountCourseSection { get; set; }
        public int CountCourseSectionMaleTrainee { get; set; }
        public int CountCourseSectionFemaleTrainee { get; set; }

        public double SumOfMarks { get; set; }
        public double SumOfTotalMarks { get; set; }
        public double SumOfAVG { get; set; }

        public double MinMark { get; set; }
        public double MaxMark { get; set; }
    }
}