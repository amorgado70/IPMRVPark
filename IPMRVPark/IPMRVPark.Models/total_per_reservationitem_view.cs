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
    
    public partial class total_per_reservationitem_view
    {
        public Nullable<long> idPayment { get; set; }
        public long idReservationItem { get; set; }
        public long idRVSite { get; set; }
        public long idSiteType { get; set; }
        public string site { get; set; }
        public string descriptionRVSite { get; set; }
        public long idCustomer { get; set; }
        public string fullName { get; set; }
        public string mainPhone { get; set; }
        public long idStaff { get; set; }
        public System.DateTime checkInDate { get; set; }
        public System.DateTime checkOutDate { get; set; }
        public long duration { get; set; }
        public Nullable<long> weeks { get; set; }
        public Nullable<decimal> weeklyRate { get; set; }
        public Nullable<long> days { get; set; }
        public Nullable<decimal> dailyRate { get; set; }
        public Nullable<decimal> amount { get; set; }
    }
}
