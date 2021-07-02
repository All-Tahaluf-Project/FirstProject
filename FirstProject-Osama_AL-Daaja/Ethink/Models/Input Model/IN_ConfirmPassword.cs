using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.Input_Model
{
    public class IN_ConfirmPassword
    {
        public string CodeEmail { get; set; }
        public string CodeConfirm { get; set; }
        public string ConfirmPassword { get; set; }
        public string NewPassword { get; set; }
        public string UserName { get; set; }
    }
}