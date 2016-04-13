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
        public ActionResult GetSitesCoord(string query)
        {
            return Json(SiteList(query).Select(c => new { label = c.Label, ID = c.ID }));
        }

        private List<SelectionOptionID> SiteList(string searchString)
        {

            //Return value
            List<SelectionOptionID> results = new List<SelectionOptionID>();

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
                        results.Add(new SelectionOptionID(_site.id, _site.RVSite));
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
