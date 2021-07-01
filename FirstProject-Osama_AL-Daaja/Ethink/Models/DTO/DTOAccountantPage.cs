using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOAccountantPage
    {
        public decimal SumPay { get; set; }
        public CourseSections CourseSections { get; set; }
        public int CountOfPay { get; set; }
        public int CountOfPayProblem { get; set; }
        public int CountOfPayNotProblem { get; set; }

        public List<PayLog> PayLog { get; set; }
        public List<PaySalary> PaySalary { get; set; }

    }
}