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
    
    public partial class total_per_session_view
    {
        public long idSession { get; set; }
        public Nullable<long> idCustomer { get; set; }
        public Nullable<long> idStaff { get; set; }
        public Nullable<decimal> total_amount { get; set; }
    }
}
