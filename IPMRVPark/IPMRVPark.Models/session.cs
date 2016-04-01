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
    
    public partial class session
    {
        public session()
        {
            this.payments = new HashSet<payment>();
            this.selecteditems = new HashSet<selecteditem>();
        }
    
        public long ID { get; set; }
        public string sessionGUID { get; set; }
        public long idIPMEvent { get; set; }
        public Nullable<long> idStaff { get; set; }
        public Nullable<bool> isLoggedIn { get; set; }
        public Nullable<long> idCustomer { get; set; }
        public Nullable<System.DateTime> checkInDate { get; set; }
        public Nullable<System.DateTime> checkOutDate { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> lastUpdate { get; set; }
    
        public virtual customer customer { get; set; }
        public virtual ipmevent ipmevent { get; set; }
        public virtual ICollection<payment> payments { get; set; }
        public virtual ICollection<selecteditem> selecteditems { get; set; }
        public virtual staff staff { get; set; }
    }
}