﻿using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Models.View;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace IPMRVPark.WebUI.Controllers
{
    public class LoginController : Controller
    {
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<staff> users;
        SessionService sessionService;

        public LoginController(
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<staff> users,
            IRepositoryBase<session> sessions)
        {
            this.ipmevents = ipmevents;
            this.sessions = sessions;
            this.customers = customers;
            this.selecteditems = selecteditems;
            this.users = users;
            sessionService = new SessionService(
                this.sessions,
                this.customers
                );
        }//end Constructor

        const long IDnotFound = -1;

        public ActionResult Home()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Menu()
        {
            ViewBag.UserID = sessionService.GetSessionUserID(this.HttpContext);

            return View();
        }

        public ActionResult Logout()
        {
            return View();
        }

        public ActionResult Login()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            var _ipmevents = ipmevents.GetAll();
            var _session = sessionService.GetSession(this.HttpContext);
            foreach (ipmevent _ipmevent in _ipmevents)
            {
                SelectListItem i = new SelectListItem();
                i.Value = _ipmevent.ID.ToString();
                i.Text = _ipmevent.year.ToString();
                items.Add(i);
                if (_ipmevent.ID == _session.idIPMEvent)
                {
                    i.Selected = true;
                }
            }
            ViewBag.IPMEventYear = items;
            return View();
        }

        [HttpPost]
        public ActionResult GetSessionEmail()
        {
            SelectionOptionID user = new SelectionOptionID(IDnotFound, "");
            var _session = sessionService.GetSession(this.HttpContext);
            if (_session.idStaff != null)
            {
                staff _user = users.GetById(_session.idStaff);
                if (_user != null)
                {
                    user.ID = _session.idStaff.Value;
                    user.Label = _user.person.email;
                };
            };
            return Json(user);
        }

        [HttpPost]
        public ActionResult GetSessionYear()
        {
            string year = ipmevents.GetById(sessionService.GetSession(this.HttpContext).idIPMEvent).year.ToString();
            return Json(year);
        }

        [HttpPost]
        public ActionResult SelectUser(string userEmail)
        {
            SelectionOptionID user = new SelectionOptionID(IDnotFound, "");
            if (userEmail != null)
            {
                var _session = sessionService.GetSession(this.HttpContext);
                var _users = users.GetAll().Where(q => q.person.email == userEmail);
                if (_users.Count() > 0)
                {
                    user.ID = users.GetAll().Where(q => q.person.email == userEmail).First().ID;
                    user.Label = userEmail;
                    _session.idStaff = user.ID;
                }
                else
                {
                    _session.idStaff = null;
                }
                sessions.Update(sessions.GetById(_session.ID));
                sessions.Commit();
            }
            return Json(user);
        }

        [HttpPost]
        public ActionResult ChangeYear(string idIPMEvent)
        {
            var _session = sessionService.GetSession(this.HttpContext);
            _session.idIPMEvent = long.Parse(idIPMEvent);
            sessions.Update(sessions.GetById(_session.ID));
            sessions.Commit();

            return Json(idIPMEvent);
        }

        public ActionResult GetSessionGUID()
        {
            var _session = sessionService.GetSession(this.HttpContext);
            var _IPMEvent = ipmevents.GetById(_session.idIPMEvent);
            string sessionSummary = "sessionID: " + _session.ID +
                " sessionGUID: " + _session.sessionGUID +
                " IPMEvent: " + _IPMEvent.year;
            return Json(sessionSummary);
        }

        public ActionResult GetSessionCustomer()
        {
            SelectionOptionID customer = new SelectionOptionID(-1, "");
            var _session = sessionService.GetSession(this.HttpContext);
            if (_session.idCustomer != null)
            {
                var _customer = customers.GetAll().Where(c => c.id == _session.idCustomer).First();
                if (_customer != null)
                {
                    customer.ID = _session.idCustomer.Value;
                    customer.Label = _customer.fullName + " - Phone: " + _customer.mainPhone;
                };
            };
            return Json(customer);
        }

        [HttpPost]
        public ActionResult SelectCustomer(long idCustomer)
        {
            session _session = sessions.GetById(sessionService.GetSession(this.HttpContext).ID);
            _session.idCustomer = idCustomer;

            long sessionID = _session.ID;
            var _selecteditems = selecteditems.GetAll().Where(s => s.idSession == sessionID);
            foreach (selecteditem item in _selecteditems)
            {
                item.idCustomer = idCustomer;
                selecteditems.Update(item);
            }
            sessions.Update(_session);

            selecteditems.Commit();
            sessions.Commit();
            return Json(idCustomer);
        }
    }

}
