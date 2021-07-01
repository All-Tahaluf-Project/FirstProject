using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOCourse
    {
        public Course Course { get; set; }
        public List<CourseSections> CourseSections { get; set; }
        public List<DiscountPrice> DiscountPrice { get; set; }
    }

}