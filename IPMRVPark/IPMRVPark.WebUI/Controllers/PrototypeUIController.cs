using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IPMRVPark.WebUI.Controllers
{
    public class PrototypeUIController : Controller
    {
        //
        // GET: /PrototypeUI/

        public ActionResult ReservationOrderIndex()
        {
            return View();
        }

    }
}
