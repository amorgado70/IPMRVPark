//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IPMRVPark.WebUI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class provincecode
    {
        public provincecode()
        {
            this.addresses = new HashSet<address>();
        }
    
        public string code { get; set; }
        public string countryCode { get; set; }
        public string name { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> lastUpdate { get; set; }
    
        public virtual ICollection<address> addresses { get; set; }
        public virtual countrycode countrycode1 { get; set; }
    }
}