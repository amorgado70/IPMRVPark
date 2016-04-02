using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;

namespace IPMRVPark.WebUI.Controllers
{
    public class ReservationController : Controller
    {
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<placeinmap> placesinmap;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<rvsite_available_view> rvsites_available;
        IRepositoryBase<total_per_selecteditem_view> totals_per_selecteditem;
        IRepositoryBase<total_per_reservationitem_view> totals_per_reservationitem;
        IRepositoryBase<total_per_edititem_view> totals_per_edititem;
        SessionService sessionService;

        public ReservationController(
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<placeinmap> placesinmap,
            IRepositoryBase<rvsite_available_view> rvsites_available,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<total_per_selecteditem_view> totals_per_selecteditem,
            IRepositoryBase<total_per_reservationitem_view> totals_per_reservationitem,
            IRepositoryBase<total_per_edititem_view> totals_per_edititem,
            IRepositoryBase<session> sessions)
        {
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.sessions = sessions;
            this.placesinmap = placesinmap;
            this.selecteditems = selecteditems;
            this.totals_per_selecteditem = totals_per_selecteditem;
            this.totals_per_reservationitem = totals_per_reservationitem;
            this.totals_per_edititem = totals_per_edititem;
            this.rvsites_available = rvsites_available;
            sessionService = new SessionService(this.sessions, this.customers);
        }//end Constructor

        #region common

        const long newReservationMode = -1;
        const long IDnotFound = -1;

        // Convert dates in number of days counting from today
        private void CreateViewBagsForDates(long selectedID = newReservationMode)
        {
            session _session = sessionService.GetSession(this.HttpContext);
            ipmevent _IPMEvent = ipmevents.GetById(_session.idIPMEvent);

            // Read and convert the dates to a value than can be used by jQuery Datepicker
            DateTime start = _IPMEvent.startDate.Value;
            DateTime end = _IPMEvent.endDate.Value;
            DateTime now = DateTime.Now;
            DateTime checkInDate = DateTime.MinValue;
            DateTime checkOutDate = DateTime.MinValue;

            // Parameters for Edit Reservation, NOT used for New Reservation
            if (selectedID != newReservationMode)
            {
                selecteditem _selecteditem = selecteditems.GetById(selectedID);
                checkInDate = _selecteditem.checkInDate;
                checkOutDate = _selecteditem.checkOutDate;
            }

            if (checkInDate == DateTime.MinValue)
            {
                if (_session.checkInDate != null)
                {
                    checkInDate = _session.checkInDate.Value;
                };
            };
            if (checkOutDate == DateTime.MinValue)
            {
                if (_session.checkOutDate != null)
                {
                    checkOutDate = _session.checkOutDate.Value;
                };
            };

            if (!(checkInDate >= start && checkInDate <= end))
            {
                checkInDate = start;
            };
            if (!(checkOutDate >= checkInDate && checkOutDate <= end))
            {
                checkOutDate = end;
            };

            int min = (int)(start - now).TotalDays + 1;
            int max = (int)(end - now).TotalDays + 1;
            int checkIn = (int)(checkInDate - now).TotalDays - 7;
            int checkOut = (int)(checkOutDate - now).TotalDays + 1;

            ViewBag.minDate = min;
            ViewBag.maxDate = max;
            ViewBag.checkInDate = checkIn;
            ViewBag.checkOutDate = checkOut;
        }
        #endregion

        #region New Reservation - Site Selected

        // Partial View for CRUD of Selected Item 
        public ActionResult CRUDSelectedItem(long selectedID = newReservationMode)
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            CreateViewBagsForDates(selectedID);

            // Parameters for Edit Reservation, NOT used for New Reservation
            if (selectedID != newReservationMode)
            {
                selecteditem _selecteditem = selecteditems.GetById(selectedID);
                ViewBag.SelectedID = selectedID;
                ViewBag.SiteID = _selecteditem.idRVSite;
                placeinmap _placeinmap = placesinmap.GetById(_selecteditem.idRVSite);
                ViewBag.SiteName = _placeinmap.site;
            }
            else
            {
                ViewBag.SiteID = newReservationMode;
            }           

            return PartialView();
        }

        // New Reservation Page - Site Selection
        public ActionResult NewReservation()
        {
            cleanSelectedItemList();
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        // Edit Selected Item
        public ActionResult EditSelected(long selectedID = newReservationMode)
        {
            ViewBag.SelectedID = selectedID;
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        // Update session's check-in and check-out dates
        [HttpPost]
        public ActionResult SelectCheckInOutDates(DateTime checkInDate, DateTime checkOutDate)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            _session.checkInDate = checkInDate;
            _session.checkOutDate = checkOutDate;
            sessions.Update(sessions.GetById(_session.ID));
            sessions.Commit();

            return Json(checkInDate);
        }

        // Select site button, add site to selected table
        [HttpPost]
        public ActionResult SelectSite(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            var _session = sessionService.GetSession(this.HttpContext);

            // Add selected item to the database
            var _selecteditem = new selecteditem();
            _selecteditem.checkInDate = checkInDate;
            _selecteditem.checkOutDate = checkOutDate;
            _selecteditem.idRVSite = idRVSite;
            _selecteditem.idSession = _session.ID;
            _selecteditem.idIPMEvent = _session.idIPMEvent;
            _selecteditem.idStaff = _session.idStaff;
            _selecteditem.idCustomer = _session.idCustomer;
            _selecteditem.isSiteChecked = true;
            _selecteditem.createDate = DateTime.Now;
            _selecteditem.lastUpdate = DateTime.Now;

            selecteditems.Insert(_selecteditem);
            selecteditems.Commit();

            return Json(idRVSite);
        }

        // Calculate total for site selected on the dropdown list
        [HttpPost]
        public ActionResult GetSiteData(long idRVSite, DateTime checkInDate, DateTime checkOutDate)
        {
            double amount = 0;
            double weeklyRate = 0;
            double dailyRate = 0;

            var site = rvsites_available.GetAll().Where(s => s.id == idRVSite).First();
            if (site != null)
            {
                int duration = (int)(checkOutDate - checkInDate).TotalDays;
                int weeks = duration / 7;
                int days = duration % 7;
                weeklyRate = Convert.ToDouble(site.weeklyRate);
                dailyRate = Convert.ToDouble(site.dailyRate);
                amount = weeklyRate * weeks +
                    dailyRate * days;
            }
            string result = amount.ToString("N2");

            return Json(new
            {
                amount = amount.ToString("N2"),
                type = site.description,
                weeklyRate = weeklyRate.ToString("N2"),
                dailyRate = dailyRate.ToString("N2")
            });
        }

        // Sum and Count for Selected Items
        private void CreateViewBagForSelectedTotal()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_selecteditem.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            int count = 0;
            decimal sum = 0;
            foreach (var i in _selecteditem)
            {
                count = count + 1;
                sum = sum + i.amount.Value;
            }

            if (count > 0)
            {
                ViewBag.totalAmount = sum.ToString("N2");
            }
        }

        // For Partial View : Selected Site List
        public ActionResult UpdateSelectedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_selecteditem.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            CreateViewBagForSelectedTotal();

            return PartialView("SelectedList", _selecteditem);
        }

        // For Partial View : Show Reservation Summary
        public ActionResult ShowSelectionSummary()
        {
            session _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_selecteditem.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            CreateViewBagForSelectedTotal();

            ViewBag.Customer = sessionService.GetCustomerNamePhone(this.HttpContext);

            if (_selecteditem.Count() > 0)
            {
                return PartialView("SelectionSummary", _selecteditem);
            }
            else
            {
                return PartialView("../Login/EmptyPartial");
            }
        }

        public void CalculateSelectionTotal(out int count, out decimal sum)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_selecteditem.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);
            count = 0;
            sum = 0;
            foreach (var i in _selecteditem)
            {
                count = count + 1;
                sum = sum + i.amount.Value;
            }
        }

        // Selected sites total
        public ActionResult GetSelectionTotal()
        {
            int count;
            decimal sum;
            CalculateSelectionTotal(out count, out sum);

            string totalAmount = "";
            if (count > 0)
            {
                totalAmount = "( " + count + " )  $" + sum.ToString("N2");
            }

            return Json(totalAmount);
        }

        // Update on selected site
        public ActionResult UpdateSelected(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = selecteditems.GetById(id);

            _selecteditem.checkInDate = _session.checkInDate.Value;
            _selecteditem.checkOutDate = _session.checkOutDate.Value;
            _selecteditem.lastUpdate = DateTime.Now;

            selecteditems.Update(_selecteditem);
            selecteditems.Commit();

            return RedirectToAction("NewReservation");
        }

        // Delete on selected site
        public ActionResult RemoveSelected(int id)
        {
            selecteditems.Delete(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("NewReservation");
        }

        // Delete all selected sites
        public ActionResult RemoveAllSelected()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var allSelected = totals_per_selecteditem.GetAll();
            allSelected = allSelected.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            if (allSelected.Count() > 0)
            {
                foreach (var _selected in allSelected)
                {
                    selecteditems.Delete(selecteditems.GetById(_selected.idSelected));
                }
                selecteditems.Commit();
            }

            return RedirectToAction("NewReservation");
        }

        // Clean selected items
        private void cleanSelectedItemList()
        {
            session _session = sessionService.GetSession(this.HttpContext);

            // Clean edit items that are in selected table
            var _olditems_to_be_removed = selecteditems.GetAll().
                Where(c => c.idSession == _session.ID && c.idReservationItem > 0);
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

        #region Edit Reservation - Site Reserved

        // Update Reserved Site
        public ActionResult UpdateReserved(int id)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = selecteditems.GetById(id);

            _selecteditem.checkInDate = _session.checkInDate.Value;
            _selecteditem.checkOutDate = _session.checkOutDate.Value;
            _selecteditem.lastUpdate = DateTime.Now;

            selecteditems.Update(_selecteditem);
            selecteditems.Commit();

            return RedirectToAction("EditReservation");
        }

        // Reinsert Reserved Site
        public ActionResult ReinsertReserved(int id)
        {
            var _selecteditem = selecteditems.GetById(id);
            _selecteditem.isSiteChecked = true;
            selecteditems.Update(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("EditReservation");
        }

        // Remove Reserved Site
        public ActionResult RemoveReserved(int id)
        {
            var _selecteditem = selecteditems.GetById(id);
            _selecteditem.isSiteChecked = false;
            selecteditems.Update(selecteditems.GetById(id));
            selecteditems.Commit();
            return RedirectToAction("EditReservation");
        }

        // Remove All Reserved Sites
        public ActionResult RemoveAllReserved()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var allSelected = totals_per_selecteditem.GetAll();
            allSelected = allSelected.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            if (allSelected.Count() > 0)
            {
                foreach (var i in allSelected)
                {
                    var _selecteditem = selecteditems.GetById(i.idSelected);
                    _selecteditem.isSiteChecked = false;
                    selecteditems.Update(_selecteditem);
                }
                selecteditems.Commit();
            }

            return RedirectToAction("EditReservation");
        }


        // Edit Reserved Site
        public ActionResult EditReserved(long selectedID = newReservationMode)
        {
            ViewBag.SelectedID = selectedID;
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        // Partial View for CRUD of Reserved Site
        public ActionResult CRUDReservedItem(long selectedID = newReservationMode)
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            CreateViewBagsForDates(selectedID);

            // Parameters for Edit Reservation, NOT used for New Reservation
            if (selectedID != newReservationMode)
            {
                selecteditem _selecteditem = selecteditems.GetById(selectedID);
                ViewBag.SelectedID = selectedID;
                ViewBag.SiteID = _selecteditem.idRVSite;
                placeinmap _placeinmap = placesinmap.GetById(_selecteditem.idRVSite);
                ViewBag.SiteName = _placeinmap.site;
            }
            else
            {
                ViewBag.SiteID = newReservationMode;
            }

            return PartialView();
        }

        // Clean edit items
        private void cleanEditItemList()
        {
            session _session = sessionService.GetSession(this.HttpContext);

            // Clean edit items that are in selected table
            var _olditems_to_be_removed = selecteditems.GetAll().
                Where(c => c.idSession == _session.ID || c.idCustomer == _session.idCustomer);
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

        // Search reservation page
        public ActionResult SearchReservation()
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);
            cleanEditItemList();

            return View();
        }

        // For Partial View : Reserved Site List
        public ActionResult UpdateReservedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);

            long idCustomer = sessionService.GetCustomerID(this.HttpContext);      
            var _reserveditems = totals_per_reservationitem.GetAll();
            if (idCustomer != -1)
            {
                _reserveditems = _reserveditems.Where(q => q.idCustomer == idCustomer).OrderByDescending(o => o.idRVSite);
            }

            int count = 0;
            decimal sum = 0;
            foreach (var i in _reserveditems)
            {
                count = count + 1;
                sum = sum + i.amount.Value;
            }

            if (count > 0)
            {
                ViewBag.totalAmount = sum.ToString("N2");
            }

            return PartialView("../Reservation/ReservedList", _reserveditems);
        }

        public ActionResult GoToEditReservation()
        {
            cleanEditItemList();
            return RedirectToAction("EditReservation");
        }

        public ActionResult EditReservation()
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            var _session = sessionService.GetSession(this.HttpContext);
            long idCustomer = sessionService.GetCustomerID(this.HttpContext);
            ViewBag.Customer = sessionService.GetCustomerNamePhone(this.HttpContext);

            var _reserveditems = totals_per_reservationitem.GetAll();
            if (idCustomer != IDnotFound)
            {
                _reserveditems = _reserveditems.Where(q => q.idCustomer == idCustomer).OrderByDescending(o => o.idRVSite);
            }

            foreach (var item in _reserveditems)
            {
                // If reserved item is not in the selected item table
                var _checkitem = totals_per_selecteditem.GetAll().Where(s => s.idRVSite == item.idRVSite).FirstOrDefault();
                if (_checkitem == null)
                {
                    // Add reserved item as selected item                
                    selecteditem _selecteditem = new selecteditem();
                    _selecteditem.checkInDate = item.checkInDate;
                    _selecteditem.checkOutDate = item.checkOutDate;
                    _selecteditem.idRVSite = item.idRVSite;
                    _selecteditem.idSession = _session.ID;
                    _selecteditem.idIPMEvent = _session.idIPMEvent;
                    _selecteditem.idStaff = _session.idStaff;
                    _selecteditem.idCustomer = item.idCustomer;
                    _selecteditem.isSiteChecked = true;
                    _selecteditem.createDate = DateTime.Now;
                    _selecteditem.lastUpdate = DateTime.Now;
                    _selecteditem.idReservationItem = item.idReservationItem;
                    selecteditems.Insert(_selecteditem);
                }
            }
            selecteditems.Commit();

            // Data to be presented on the view
            var _edititems = totals_per_edititem.GetAll().Where(s => s.idSession == _session.ID && s.idCustomer == _session.idCustomer);
            int count = 0;
            decimal sum = 0;
            decimal reservationsum = 0;
            foreach (var item in _edititems)
            {
                count = count + 1;
                sum = sum + item.total.Value;
                reservationsum = reservationsum + item.reservationAmount.Value;
            }

            decimal dueAmount = Math.Max((sum - reservationsum), 0);
            decimal refundAmount = Math.Max((reservationsum - sum), 0);
            // Check if there is a cancellation fee
            decimal cancelationFee = sessionService.GetCancelationFee(this.HttpContext);
            if (sum < reservationsum)
            {
                if ((reservationsum - sum) < cancelationFee)
                {
                    refundAmount = 0;
                    dueAmount = cancelationFee - (reservationsum - sum);
                }
                else
                {
                    refundAmount = refundAmount - cancelationFee;
                }
            }
            else
            {
                cancelationFee = 0;
            }

            ViewBag.totalAmount = sum.ToString("N2");
            ViewBag.reservationAmount = reservationsum.ToString("N2");
            ViewBag.dueAmount = dueAmount.ToString("N2");
            ViewBag.refundAmount = refundAmount.ToString("N2");
            ViewBag.cancelationFee = cancelationFee.ToString("N2");

            return View(_edititems);
        }
        #endregion
    }
}
