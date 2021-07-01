using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTODetailsTraineeForTrainer
    {
        public ApplicationUser ApplicationUser { get; set; }
        public TraineeExam TraineeExam { get; set; }
    }
}