using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.Input_Model
{
    public class IN_AddCredit
    {
        public int IdCard { get; set; }
        public bool Status { get; set; }
        public decimal Value { get; set; }
    }
}