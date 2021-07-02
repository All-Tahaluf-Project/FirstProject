using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ethink.Models.Input_Model
{
    public class IN_PaymentLog
    {
        [Required]
        public string Code { get; set; }




        public int Id { get; set; }
        [Required]
        public decimal Value { get; set; }
        [Required]
        public int IdCard { get; set; }
        [Required]
        public System.DateTime Date { get; set; }
        [Required]
        public Nullable<int> IdCourseSection { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}