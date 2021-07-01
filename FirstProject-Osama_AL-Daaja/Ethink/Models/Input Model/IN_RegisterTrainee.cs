using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ethink.Models.Input_Model
{
    public class IN_RegisterTrainee
    {
        [Required]
        [DisplayName("UserName")]
        public string UserName { get; set; }
        [Required]
        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DisplayName("Email")]
        public string Email { get; set; }
        [Required]
        [DisplayName("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [Required]
        [DisplayName("NickName")]
        public string NickName { get; set; }
        [Required]
        [DisplayName("Gender")]
        [DefaultValue(false)]
        public bool Gender { get; set; }
    }
}