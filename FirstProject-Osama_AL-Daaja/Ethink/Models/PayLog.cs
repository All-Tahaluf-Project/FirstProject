//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ethink.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PayLog
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public int IdCard { get; set; }
        public System.DateTime Date { get; set; }
        public Nullable<int> IdCourseSection { get; set; }
        public bool Status { get; set; }
    
        public virtual CourseSections CourseSections { get; set; }
        public virtual PayCard PayCard { get; set; }
    }
}
