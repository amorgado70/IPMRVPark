using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IPMRVPark.Services
{
    public class PaymentService
    {
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<reservationitem> reservationitems;
        IRepositoryBase<payment> payments;

        public PaymentService(
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<reservationitem> reservationitems,
            IRepositoryBase<payment> payments
            )
        {
            this.selecteditems = selecteditems;
            this.reservationitems = reservationitems;
            this.payments = payments;
        }
        #region
        // Clean selected items
        public void CleanSelectedItemList(long sessionID)
        {
            // Clean edit items that are in selected table
            var _olditems_to_be_removed = selecteditems.GetAll().
                Where(c => c.idSession == sessionID && c.idReservationItem > 0);
            bool tryResult = false;
            try
            {
                var _oldselecteditem = _olditems_to_be_removed.FirstOrDefault();
                tryResult = !(_oldselecteditem.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
            if (tryResult)// Items found in database, remove them
            {
                foreach (var _olditem in _olditems_to_be_removed)
                {
                    selecteditems.Delete(_olditem.ID);
                }
                selecteditems.Commit();
            }
        }
        #endregion
        #region Payments & Refunds
        public decimal GetProvinceTax(long sessionID)
        {
            return 13; // HST value for Ontario
        }
        public decimal GetCancelationFee(long sessionID)
        {
            return 50; // Value for 2016
        }

    //    public void GetPaymentTotal(HttpContextBase httpContext)
    //    {


    //    // Data to be presented on the view
    //    var _edititems = totals_per_edititem.GetAll().Where(s => s.idSession == _session.ID && s.idCustomer == _session.idCustomer);
    //    int count = 0;
    //    decimal sum = 0;
    //    decimal reservationsum = 0;
    //        foreach (var item in _edititems)
    //        {
    //            count = count + 1;
    //            sum = sum + item.total.Value;
    //            reservationsum = reservationsum + item.reservationAmount.Value;
    //        }

    //decimal dueAmount = Math.Max((sum - reservationsum), 0);
    //decimal refundAmount = Math.Max((reservationsum - sum), 0);
    //// Check if there is a cancellation fee
    //decimal cancelationFee = sessionService.GetCancelationFee(this.HttpContext);
    //        if (sum<reservationsum)
    //        {
    //            if ((reservationsum - sum) < cancelationFee)
    //            {
    //                refundAmount = 0;
    //                dueAmount = cancelationFee - (reservationsum - sum);
    //            }
    //            else
    //            {
    //                refundAmount = refundAmount - cancelationFee;
    //            }
    //        }
    //        else
    //        {
    //            cancelationFee = 0;
    //        }

    //        ViewBag.totalAmount = sum.ToString("N2");
    //        ViewBag.reservationAmount = reservationsum.ToString("N2");
    //        ViewBag.dueAmount = dueAmount.ToString("N2");
    //        ViewBag.refundAmount = refundAmount.ToString("N2");
    //        ViewBag.cancelationFee = cancelationFee.ToString("N2");
    //    }
        #endregion
    }
}
