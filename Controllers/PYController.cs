using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using HanLP.csharp.seg.Pinyin;


namespace nlpdemo.Controllers
{
    public class PYController : ApiController
    {
        // POST api/util
        public void Post([FromBody]string value)
        {
        }


        //[HttpGet]
        // GET api/values/5
        public string Get(string pinyin)
        {
            var list = PinyinSeg.Seg_PY(pinyin);
            if (list == null)
                return "Invalid-crude-pinyin";

            var sb = new StringBuilder();
            foreach(var py in list)
            {
                if (sb.Length != 0)
                    sb.Append("<br />").Append(py);
                else
                    sb.Append(py);
            }
            return sb.ToString();
        }

        
    }
}
