using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOFinancialReportTreainee
    {
        public Course_Trainee Course_Trainee { get; set; }
        public decimal SumPay { get; set; }
        public decimal SumDiscount { get; set; }
    }
}