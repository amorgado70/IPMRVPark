//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IPMRVPark.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class paydoctype
    {
        public paydoctype()
        {
            this.payments = new HashSet<payment>();
        }
    
        public long ID { get; set; }
        public string description { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> lastUpdate { get; set; }
    
        public virtual ICollection<payment> payments { get; set; }
    }
}
