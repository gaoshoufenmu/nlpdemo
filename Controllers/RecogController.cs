using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;


namespace nlpdemo.Controllers
{
    public class RecogController : Controller
    {
        // GET: Recog
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChsIndex()
        {
            return View();
        }
    }

    
}