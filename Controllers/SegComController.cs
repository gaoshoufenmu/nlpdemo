using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using HanLP.csharp.seg.CRF;
using HanLP.csharp.corpus.tag;
using nlpdemo.utils;
namespace nlpdemo.Controllers
{
    public class SegComController : ApiController
    {
        public string Get(string comname)
        {
            var list = Com_CRFSegment.Segment(comname);
            if (list == null || list.Count == 0)
                return "Check_your_company_name's_validity";

            var sb = new StringBuilder();
            foreach (var s in list)
            {
                if (sb.Length != 0)
                    sb.Append("<br />");
                sb.Append(s.word).Append("&emsp;&emsp;&emsp;&emsp;").Append(s.nature).Append("&emsp;&emsp;&emsp;&emsp;");
                if (s.nc == NatCom.MA)
                    sb.Append("地区");
                else if (s.nc == NatCom.C)
                    sb.Append("主成分");
                else if (s.nc == NatCom.OF)
                    sb.Append("组织形式");
                else if (s.nc == NatCom.W)
                    sb.Append("符号");
                else if (s.nc == NatCom.E)
                    sb.Append("字母");
                else if (s.nc == NatCom.T)
                    sb.Append("行业");
                else
                    sb.Append("其他");
                sb.Append("&emsp;&emsp;&emsp;&emsp;备注: ").Append(string.IsNullOrWhiteSpace(s.ext1) ? "-" : s.ext1);
            }
            return sb.ToString();
        }

        [HttpGet]
        public string SegAddr(string addr)
        {
            if (string.IsNullOrWhiteSpace(addr))
                return "Invalid input";
            var list = Tokenizer.CRF_Seg(addr);
            var sb = new StringBuilder();

            foreach (var t in list)
            {
                if (sb.Length > 0)
                    sb.Append("<br />");
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }
    }
}
