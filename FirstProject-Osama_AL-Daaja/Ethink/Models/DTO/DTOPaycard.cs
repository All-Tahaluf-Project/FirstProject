using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOPaycard
    {
        public PayCard PayCard { get; set; }
        public PayLog PayLog { get; set; }
    }

    public class DTOPayLog
    {
        public int IdCard { get; set; }
        public bool Status { get; set; }
    }
}