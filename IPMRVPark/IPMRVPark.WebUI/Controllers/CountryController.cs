﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Data;
using IPMRVPark.Contracts.Repositories;

namespace IPMRVPark.WebUI.Controllers
{
    public class CountryController : Controller
    {
        IRepositoryBase<countrycode> countrycodes;
        public CountryController(IRepositoryBase<countrycode> countrycodes)
        {
            this.countrycodes = countrycodes;
        }//end Constructor

        // GET: list with filter
        public ActionResult Index(string searchString)
        {
            var countrycode = countrycodes.GetAll().OrderBy(c => c.name);

            if (!String.IsNullOrEmpty(searchString))
            {
                countrycode = countrycode.Where(s => s.name.Contains(searchString)).OrderBy(c => c.name);
            }

            return View(countrycode);
        }

        // GET: /Details/5
        public ActionResult CountryDetails(string id)
        {
            var countrycode = countrycodes.GetById(id);
            if (countrycode == null)
            {
                return HttpNotFound();
            }
            return View(countrycode);
        }

        // GET: /Create
        public ActionResult CreateCountry()
        {
            var countrycode = new countrycode();
            return View(countrycode);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCountry(countrycode countrycode)
        {
            var _country = new countrycode();
            _country.code = countrycode.code;
            _country.name = countrycode.name;
            _country.createDate = DateTime.Now;
            _country.lastUpdate = DateTime.Now;
            countrycodes.Insert(_country);
            countrycodes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Edit/5
        public ActionResult EditCountry(string id)
        {
            countrycode countrycode = countrycodes.GetById(id);
            if (countrycode == null)
            {
                return HttpNotFound();
            }
            return View(countrycode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCountry(countrycode countrycode)
        {
            var _country = countrycodes.GetById(countrycode.code);

            _country.code = countrycode.code;
            _country.name = countrycode.name;
            _country.lastUpdate = DateTime.Now;
            countrycodes.Update(_country);
            countrycodes.Commit();

            return RedirectToAction("Index");
        }

        // GET: /Delete/5
        public ActionResult DeleteCountry(string id)
        {
            countrycode countrycode = countrycodes.GetById(id);
            if (countrycode == null)
            {
                return HttpNotFound();
            }
            return View(countrycode);
        }

        [HttpPost, ActionName("DeleteCountry")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(string id)
        {
            countrycodes.Delete(countrycodes.GetById(id));
            countrycodes.Commit();
            return RedirectToAction("Index");
        }

    }
}
