using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ethink.Models.DTO
{
    public class DTOApplicationUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string ImageName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool Block { get; set; }
        public string NickName { get; set; }
        public bool Gender { get; set; }
    }
}