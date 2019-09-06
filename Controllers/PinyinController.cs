using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace nlpdemo.Controllers
{
    public class PinyinController : Controller
    {
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Pinyin Page";

            return View();
        }

        //public ActionResult Pinyin_Parse(string t, string n)
        //{

        //}
    }
}
