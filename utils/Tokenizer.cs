using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HanLP.csharp.seg;
using HanLP.csharp.seg.common;
using HanLP.csharp.seg.CRF;

namespace nlpdemo.utils
{
    public class Tokenizer
    {
        private static Segment _crf = new CRFSegment().SetCustomDictionary(true);

        public static List<Term> CRF_Seg(string input) => _crf.Seg(input);
    }
}