﻿using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;
using IPMRVPark.Models.View;
using System.Text.RegularExpressions;

namespace IPMRVPark.WebUI.Controllers
{
    public class SearchController : Controller
    {
        IRepositoryBase<customer_view> customers;
        IRepositoryBase<ipmevent> ipmevents;
        IRepositoryBase<session> sessions;
        IRepositoryBase<selecteditem> selecteditems;
        IRepositoryBase<rvsite_available_view> rvsites_available;
        SessionService sessionService;

        public SearchController(
            IRepositoryBase<customer_view> customers,
            IRepositoryBase<ipmevent> ipmevents,
            IRepositoryBase<rvsite_available_view> rvsites_available,
            IRepositoryBase<selecteditem> selecteditems,
            IRepositoryBase<session> sessions)
        {
            this.customers = customers;
            this.ipmevents = ipmevents;
            this.selecteditems = selecteditems;
            this.sessions = sessions;
            this.rvsites_available = rvsites_available;
            sessionService = new SessionService(this.sessions);
        }//end Constructor

        // Search results for customer autocomplete dropdown list
        public ActionResult SearchCustomerByNameOrPhoneResult(string query)
        {
            return Json(SearchCustomerByNameOrPhoneList(query).Select(c => new { label = c.Label, ID = c.ID }));
        }
        private List<SelectionOptionID> SearchCustomerByNameOrPhoneList(string searchString)
        {
            //Return value
            List<SelectionOptionID> results = new List<SelectionOptionID>();
            if (searchString != null)
            {
                //Regex for phone number
                Regex rgx = new Regex("[^0-9]");

                //Read customer data
                var allCustomers = customers.GetAll();

                //Check if search is by phone number or by customer name
                if (searchString.Any(char.IsDigit))
                {
                    searchString = rgx.Replace(searchString, "");
                    //Filter by phone number
                    foreach (customer_view customer in allCustomers)
                    {
                        string phoneNumber = rgx.Replace(customer.mainPhone, "");
                        if (phoneNumber.Contains(searchString))
                        {
                            results.Add(new SelectionOptionID(customer.id, customer.fullName + " - Phone: " + customer.mainPhone));
                        }
                        if (results.Count() > 5)
                        {
                            results.Add(new SelectionOptionID(-1, "..."));
                            return results;
                        }
                    };
                }
                else
                {
                    //Filter by customer name
                    allCustomers = allCustomers.Where(s => s.fullName.ToUpper().Contains(searchString));
                    if (allCustomers != null)
                        foreach (customer_view customer in allCustomers)
                        {
                            {
                                results.Add(new SelectionOptionID(customer.id, customer.fullName + " - Phone: " + customer.mainPhone));
                            }
                            if (results.Count() > 5)
                            {
                                results.Add(new SelectionOptionID(-1, "..."));
                                return results;
                            }
                        };
                }
            }
            return results;
        }

        // Search results for selected site autocomplete dropdown list
        public ActionResult SearchSiteByNameResult(string query)
        {
            return Json(SearchSiteByName(query).Select(c => new { label = c.Label, ID = c.ID }));
        }

        private List<SelectionOptionID> SearchSiteByName(string searchString)
        {
            //Return value
            List<SelectionOptionID> results = new List<SelectionOptionID>();

            //Regex for site name
            Regex rgx = new Regex("[^a-zA-Z0-9]");

            //Read RVSite available
            var allRVSites = rvsites_available.GetAll();

            //Remove characters from search string
            searchString = rgx.Replace(searchString, "").ToUpper();

            if (searchString != null)
            {
                //Filter by RV Site
                foreach (rvsite_available_view rvsite in allRVSites)
                {
                    string rvsiteShort = rgx.Replace(rvsite.site, "").ToUpper();
                    if (rvsiteShort.Contains(searchString))
                    {
                        results.Add(new SelectionOptionID(rvsite.id, rvsite.site));
                    }
                    if (results.Count() > 25)
                    {
                        results.OrderBy(q => q.Label).ToList();
                        results.Add(new SelectionOptionID(-1, "..."));
                        return results;
                    }
                }
            }

            return results.OrderBy(q => q.Label).ToList();
        }
    }
}
