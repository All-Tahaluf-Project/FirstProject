using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOResultExam
    {
        public List<HistorySolutions> HistorySolutions { get; set; }
        public Exam Exam { get; set; }
        public int IdApplicationUser { get; set; }
    }
}