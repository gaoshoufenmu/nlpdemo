using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HanLP.csharp;
namespace nlpdemo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            // 搜索企业列表
            routes.MapRoute(
              name: "DefaultOrgCompanySearch",
              url: "pinyin/{t}/{*n}",
              defaults: new { controller = "Pinyin", action = "Pinyin_Parse", t = UrlParameter.Optional, n = string.Empty }
              //defaults: new { controller = "OrgCompany", action = "SearchListView",n="" },
              //constraints:new { n="正则"}
              );
        }

        //private static void Init()
        //{
        //    Config.DataRootDir = HttpContext.Current.Server.MapPath("/");
        //}
    }
}
