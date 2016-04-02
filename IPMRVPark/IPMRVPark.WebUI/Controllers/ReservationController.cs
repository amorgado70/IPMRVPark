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
            sessionService = new SessionService(this.sessions);
        }//end Constructor

        #region common

        const long newReservationMode = -1;
        const long IDnotFound = -1;

        private long GetSessionUserID()
        {
            session _session = sessionService.GetSession(this.HttpContext);
            if (_session.idStaff != null)
            {
                return _session.idStaff.Value;
            }
            else
            {
                return IDnotFound;
            }            
        }

        private bool GetSessionCustomer(ref customer_view customer)
        {
            // Read customer from session
            session _session = sessionService.GetSession(this.HttpContext);
            bool customerFound = false;
            try //checks if customer is in database
            {
                customer = customers.GetAll().Where(c => c.id == _session.idCustomer).FirstOrDefault();
                customerFound = !(customer.Equals(default(session)));
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
            // Customer found in database
            return customerFound;
        }

        public long GetCustomerID()
        {
            customer_view _customer = new customer_view();
            if (GetSessionCustomer(ref _customer))
            {
                return (_customer.id);
            }
            else
            {
                return IDnotFound;
            }
        }

        public string GetCustomerNamePhone()
        {
            customer_view _customer = new customer_view();
            if (GetSessionCustomer(ref _customer))
            {
                return (_customer.fullName + ", " + _customer.mainPhone);
            }
            else
            {
                return string.Empty;
            }
        }

        // Convert dates in number of days counting from today
        private void GetTimeRange(
            out int min, out int max,
            out int checkIn, out int checkOut,
            long selectedID = newReservationMode)
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

            min = (int)(start - now).TotalDays + 1;
            max = (int)(end - now).TotalDays + 1;
            checkIn = (int)(checkInDate - now).TotalDays - 7;
            checkOut = (int)(checkOutDate - now).TotalDays + 1;
        }
        #endregion

        #region New Reservation - Site Selected

        // Partial View for CRUD of Selected Item 
        public ActionResult CRUDSelectedItem(long selectedID = newReservationMode)
        {
            int min;
            int max;
            int checkIn;
            int checkOut;
            GetTimeRange(
                out min, out max,
                out checkIn, out checkOut,
                selectedID);

            ViewBag.minDate = min;
            ViewBag.maxDate = max;
            ViewBag.checkInDate = checkIn;
            ViewBag.checkOutDate = checkOut;

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

            ViewBag.UserID = GetSessionUserID();

            return PartialView();
        }

        // New Reservation Page - Site Selection
        public ActionResult NewReservation()
        {
            cleanSelectedItemList();
            ViewBag.UserID = GetSessionUserID();

            return View();
        }

        // Edit Selected Item
        public ActionResult EditSelected(long selectedID = newReservationMode)
        {
            ViewBag.SelectedID = selectedID;
            ViewBag.UserID = GetSessionUserID();

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
            string result = amount.ToString("C");

            return Json(new
            {
                amount = amount.ToString("C"),
                type = site.description,
                weeklyRate = weeklyRate.ToString("C"),
                dailyRate = dailyRate.ToString("C")
            });
        }

        // Sum and Count for Selected Items
        private void CalcSelectItem(out int count, out decimal sum)
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

        // For Partial View : Selected Site List
        public ActionResult UpdateSelectedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_selecteditem.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            int count = 0;
            decimal sum = 0;
            CalcSelectItem(out count, out sum);

            if (count > 0)
            {
                ViewBag.totalAmount = sum.ToString("C");
            }

            return PartialView("SelectedList", _selecteditem);
        }

        // For Partial View : Show Reservation Summary
        public ActionResult ShowSelectionSummary()
        {
            session _session = sessionService.GetSession(this.HttpContext);
            var _selecteditem = totals_per_selecteditem.GetAll();
            _selecteditem = _selecteditem.Where(q => q.idSession == _session.ID).OrderByDescending(o => o.idSelected);

            int count = 0;
            decimal sum = 0;
            CalcSelectItem(out count, out sum);

            ViewBag.totalAmount = sum.ToString("C");
            ViewBag.Customer = GetCustomerNamePhone();

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
                totalAmount = "( " + count + " ) CAD" + sum.ToString("C");
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
            ViewBag.UserID = GetSessionUserID();

            return View();
        }

        // Partial View for CRUD of Reserved Site
        public ActionResult CRUDReservedItem(long selectedID = newReservationMode)
        {
            int min;
            int max;
            int checkIn;
            int checkOut;
            GetTimeRange(
                out min, out max,
                out checkIn, out checkOut,
                selectedID);

            ViewBag.minDate = min;
            ViewBag.maxDate = max;
            ViewBag.checkInDate = checkIn;
            ViewBag.checkOutDate = checkOut;

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

            ViewBag.UserID = GetSessionUserID();

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
            ViewBag.UserID = GetSessionUserID();
            cleanEditItemList();

            return View();
        }

        // For Partial View : Reserved Site List
        public ActionResult UpdateReservedList()
        {
            var _session = sessionService.GetSession(this.HttpContext);

            long idCustomer = GetCustomerID();      
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
                ViewBag.totalAmount = sum.ToString("C");
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
            var _session = sessionService.GetSession(this.HttpContext);
            long idCustomer = GetCustomerID();
            ViewBag.Customer = GetCustomerNamePhone();

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
            decimal cancelationFee = 50;
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

            ViewBag.totalAmount = sum.ToString("C");
            ViewBag.reservationAmount = reservationsum.ToString("C");
            ViewBag.dueAmount = dueAmount.ToString("C");
            ViewBag.refundAmount = refundAmount.ToString("C");
            ViewBag.cancelationFee = cancelationFee.ToString("C");

            return View(_edititems);
        }
        #endregion
    }
}
