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
    
    public partial class rvsite_coord_view
    {
        public long id { get; set; }
        public string seq { get; set; }
        public long Year { get; set; }
        public string RVSite { get; set; }
        public Nullable<decimal> longitude { get; set; }
        public Nullable<decimal> latitude { get; set; }
    }
}
