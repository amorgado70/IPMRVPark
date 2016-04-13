using System;
using System.Linq;
using System.Web.Mvc;
using IPMRVPark.Models;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IPMRVPark.WebUI.Controllers
{
    public class RVSiteCoordViewController : Controller
    {
        IRepositoryBase<rvsite_coord_view> sites;

        public RVSiteCoordViewController(
            IRepositoryBase<rvsite_coord_view> sites
            )
        {
            this.sites = sites;
        }//end Constructor

        public ActionResult Index()
        {
            var _sites = sites.GetAll();

            return View(_sites);
        }

        [HttpPost]
        public ActionResult GetSites(string query)
        {
            return Json(SiteList(query).Select(c => new { ID = c.ID, label = c.Label, Latitude = c.Latitude, Longitude = c.Longitude }));
        }

        private List<SiteCoord> SiteList(string searchString)
        {

            //Return value
            List<SiteCoord> results = new List<SiteCoord>();

            //Regex for site name
            Regex rgx = new Regex("[^a-zA-Z0-9]");

            //Read RVSite available
            var _sites = sites.GetAll();

            //Remove characters from search string
            searchString = rgx.Replace(searchString, "").ToUpper();

            if (searchString != null)
            {
                //Filter by RV Site
                foreach (var _site in _sites)
                {
                    string rvsiteShort = rgx.Replace(_site.RVSite, "").ToUpper();
                    if (rvsiteShort.Contains(searchString))
                    {
                        results.Add(new SiteCoord(_site.id, _site.RVSite, _site.latitude, _site.longitude));
                    }
                }
            }
            return results.OrderBy(q => q.Label).ToList();
        }

    }
}
