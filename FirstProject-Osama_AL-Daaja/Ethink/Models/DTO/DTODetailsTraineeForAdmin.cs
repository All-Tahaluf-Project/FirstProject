using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTODetailsTraineeForAdmin
    {
        public ApplicationUser ApplicationUser { get; set; }
        public Course_Trainee Course_Trainee { get; set; }
    }
}