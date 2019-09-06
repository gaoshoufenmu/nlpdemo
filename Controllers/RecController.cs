using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using static nlpdemo.Controllers.Util;

namespace nlpdemo.Controllers
{
    public class RecController : ApiController
    {
        public string Get(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Invalid input";

            input = input.ToLower();
            var res = Scan(input);
            if (res != null)
                return res;

            var rangeflag = Range_Detect(input);
            if (rangeflag == 1)
                return "date";
            else if (rangeflag == 2)
                return "date range";
            else if (rangeflag == 3)
                return "number range";

            var data = Get_Feature_128(input);
            var dataset = Matrix<double>.Build.Sparse(128, 1);
            dataset.SetColumn(0, data.Select(d => d / 128.0).ToArray());

            var reg = new Softmax(128, 6, 0.001);
            reg.Load_Theta(System.Web.HttpContext.Current.Server.MapPath("/") + "/data/theta_single_byte_128.txt");
            var prediction = reg.Predict(dataset);

            var terms = new string[2];
            
            for (int i = 0; i < 2; i++)
            {
                switch(prediction[0, i])
                {
                    case 0:
                        terms[i] = "com_code";
                        break;
                    case 1:
                        terms[i] = "area_code";
                        break;
                    case 2:
                        terms[i] = "com_number";
                        break;
                    case 3:
                        terms[i] = "date";
                        break;
                    case 4:
                        terms[i] = "credit_code";
                        break;
                    case 5:
                        terms[i] = "phone_number";
                        break;
                }
            }
            if (terms[1] == null)
                return terms[0];
            else
                return $"{terms[0].ToUpper()}, or {terms[1].ToUpper()} not imposible";
        }


        
    }

    public static class Util
    {
        public static string Scan(string input)
        {
            int flag = 0;
            foreach (var c in input)
            {
                if (c == '@')
                    return "mail address";
                if (c >= 'a' && c <= 'z')
                    flag |= 1;
                else if (c >= '0' && c <= '9')
                    flag |= 2;
                else if (c == '.')
                    flag |= 4;
                else
                    flag |= 8;
            }
            if (flag == 1)
                return "pinyin";
            if ((flag & 0x5) == 5)
                return "web site";
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>0: is not range</returns>
        public static int Range_Detect(string input)
        {
            var len = input.Length;
            int idx_connect = -1;
            int flag = 0;                   // 字符类型的或运算结果
            for (int i = 0; i < len; i++)
            {
                var c = input[i];
                if (c == '-')
                {
                    if (idx_connect != -1 && idx_connect != i - 1)      // 支持两个数值中间有多个连续 '-' 符号
                        return 0;
                    if (flag == 0)
                        return 0;
                    flag |= 1;              // 遇到 '-'， 最低位设置为 1
                    idx_connect = i;
                }
                else if ((c >= '0' && c <= '9') || c == '.')
                    flag |= 2;
                else
                    return 0;
            }
            if (flag != 3)
                return 0;
            var segs = input.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var fst = segs[0];
            var snd = segs[1];

            var dot1 = fst.Contains('.');
            var dot2 = snd.Contains('.');
            if (dot1 || dot2)
            {
                return 3;                       // number range
            }
            else
            {
                if (fst.Length >= 4 && snd.Length >= 2)
                {
                    var isdate1 = IsInDateRange(fst);
                    var isdate2 = IsInDateRange(snd);
                    if (isdate1 && isdate2)
                        return snd.Length == 2 ? 1 : 2;                       // date range
                }

                if (fst[0] == '0' && (fst.Length == 3 || fst.Length == 4))
                    return 0;                           // may be district code + phone number
                else if ((snd.Length == 7 || snd.Length == 8) && (fst.Length == 3 || snd.Length == 4))
                    return 0;                           // may be 全国热线电话
            }
            return 3;                           // otherwise, number range
        }

        public static bool IsInDateRange(string input, bool flag = false)
        {
            var len = input.Length;
            if (len == 4 && input.CompareTo("1900") >= 0 && input.CompareTo("2101") <= 0)
                return true;

            if (len == 2)
            {
                var end = flag ? "31" : "12";

                return input.CompareTo("01") >= 0 && input.CompareTo(end) <= 0;
            }

            if (len == 8 || len == 6)
            {
                var p4 = input.Substring(0, 4);
                var p2 = input.Substring(4, 2);
                var p6b = IsInDateRange(p4) && IsInDateRange(p2);
                if (len == 6)
                    return p6b;

                return p6b && IsInDateRange(input.Substring(6));
            }
            return false;
        }

        public static int[] Get_Feature_128(string input)
        {
            var len = input.Length;
            var tp = new bool[6];
            var flag = 0;
            for (int i = 0; i < len; i++)
            {
                var c = input[i];
                if (c >= '0' && c <= '9')
                {
                    tp[0] = true;
                    flag |= 1;
                }
                else if (char.IsLetter(c))
                {
                    tp[1] = true;
                    flag |= 2;
                }
                else if (c == '.')
                {
                    tp[2] = true;
                    flag |= 4;
                }
                else if (c == '-')
                {
                    tp[3] = true;
                    flag |= 4;
                }
                else if (c == '/')
                {
                    tp[4] = true;
                    flag |= 4;
                }
                else if (c == '(')
                {
                    tp[5] = true;
                    flag |= 4;
                }
            }
            var tp_info = new int[48];
            var arr_8 = new[] { 8, 8, 8, 8, 8, 8, 8, 8 };
            var arr_5 = new[] { 5, 5, 5, 5, 5, 5, 5, 5 };
            var arr_4 = new[] { 4, 4, 4, 4, 4, 4, 4, 4 };
            for (int i = 0; i < 6; i++)
                if (tp[i])
                    Array.Copy(arr_5, 0, tp_info, 8 * i, 8);

            var fix_len = new int[24];
            var val_info = new int[24];
            if (flag <= 3)
            {
                if (len == 9)
                    Array.Copy(arr_8, 0, fix_len, 0, 8);
                else if (len == 18)
                    Array.Copy(arr_8, 0, fix_len, 16, 8);
                else if (flag == 1)
                {
                    if (len == 11)
                    {
                        if (input[0] == '0' || (input[0] == '1' && "3578".Contains(input[1])))
                            Array.Copy(arr_5, 0, val_info, 16, 8);
                    }
                    else if (len == 2)
                        Array.Copy(arr_4, 0, val_info, 0, 8);
                    else if (len == 4)
                    {
                        var num_4 = int.Parse(input);
                        if (num_4 >= 1900 && num_4 <= 2100)
                            Array.Copy(arr_8, 0, val_info, 8, 8);
                        else
                            Array.Copy(arr_4, 0, val_info, 0, 8);
                    }
                    else if (len == 6)
                    {
                        var p4 = int.Parse(input.Substring(0, 4));
                        var p2 = int.Parse(input.Substring(4));
                        if (p4 >= 1900 && p4 <= 2100 && p2 >= 1 && p2 <= 12)
                            Array.Copy(arr_8, 0, val_info, 8, 8);
                        else
                            Array.Copy(arr_4, 0, val_info, 0, 8);
                    }
                    else if (len == 7)
                        Array.Copy(arr_5, 0, val_info, 16, 8);
                    else if (len == 8)
                    {
                        var p4 = int.Parse(input.Substring(0, 4));
                        var p2 = int.Parse(input.Substring(4, 2));
                        var p22 = int.Parse(input.Substring(6));
                        if (p4 >= 1900 && p4 <= 2100 && p2 >= 1 && p2 <= 12 && p22 >= 1 && p22 <= 31)
                            Array.Copy(arr_8, 0, val_info, 8, 8);
                        else
                            Array.Copy(arr_5, 0, val_info, 16, 8);
                    }
                    else if (len == 10 || len == 12)
                    {
                        if (input[0] == '0')
                            Array.Copy(arr_5, 0, val_info, 16, 8);
                    }
                }
            }
            else
            {
                var splitter = '0';
                if (tp[2])
                    splitter = '.';
                else if (tp[3])
                    splitter = '-';
                else if (tp[4])
                    splitter = '/';
                if (splitter != '0')
                {
                    var segs = input.Split(splitter);
                    int fst, snd, thd;
                    if (segs.Length == 3)
                    {
                        if (int.TryParse(segs[0], out fst) && int.TryParse(segs[1], out snd) && int.TryParse(segs[2], out thd))
                        {
                            if (fst >= 1900 && fst <= 2100 && snd >= 1 && snd <= 12 && thd >= 1 && thd <= 31)
                                Array.Copy(arr_8, 0, val_info, 8, 8);
                            else if (Check_Phone(segs))
                                Array.Copy(arr_5, 0, val_info, 16, 8);
                        }
                    }
                    else if (segs.Length == 2)
                    {
                        if (int.TryParse(segs[0], out fst) && int.TryParse(segs[1], out snd))
                        {
                            if (fst >= 1900 && fst <= 2100 && snd >= 1 && snd <= 12)
                                Array.Copy(arr_8, 0, val_info, 8, 8);
                            else if (Check_Phone(segs))
                                Array.Copy(arr_5, 0, val_info, 16, 8);
                        }
                    }
                }
                else
                    Array.Copy(arr_5, 0, val_info, 16, 8);
            }

            var data = new int[128];
            Array.Copy(fix_len, 0, data, 0, 24);
            Array.Copy(val_info, 0, data, 24 + 16, 24);
            Array.Copy(tp_info, 0, data, 80, 48);
            return data;
        }

        public static bool Check_Phone(string[] segs)
        {
            var longflag = false;
            foreach (var seg in segs)
            {
                if (seg.Length > 4)
                {
                    if (longflag) return false;
                    longflag = true;
                }
                if (seg.Length > 10)
                    return false;
            }
            return true;
        }
    }

    public class Softmax
    {
        private int n;
        private int k;
        private double lambda;
        private Matrix<double> theta;

        public Softmax(int n, int k, double lambda)
        {
            this.n = n;
            this.k = k;
            this.lambda = lambda;
        }

        public void Load_Theta(string path)
        {
            theta = Matrix<double>.Build.Sparse(6, n);
            int i = 0;
            foreach (var line in File.ReadLines(path))
            {
                var segs = line.Split(' ');
                theta.SetRow(i, segs.Select(s => double.Parse(s)).ToArray());
                i++;
            }
        }

        public int[,] Predict(Matrix<double> input_mtx)
        {
            var theta_x = theta.Multiply(input_mtx);
            var hypothesis = Matrix<double>.Exp(theta_x);
            var probs = hypothesis.Divide(hypothesis.ColumnSums()[0]);

            var predictions = new int[probs.ColumnCount, 2];
            for (int i = 0; i < probs.ColumnCount; i++)
            {
                double max = -1;
                double submax = -1;
                var idx = -1;
                var subidx = -1;
                for(int j = 0; j < probs.RowCount; j++)
                {
                    if(probs[j,i] > max)
                    {
                        if (idx >= 0)
                        {
                            subidx = idx;
                            submax = max;
                        }
                        max = probs[j, i];
                        idx = j;
                    }
                }
                predictions[i,0] = idx;
                if (max / submax > 2)
                    predictions[i, 1] = -1;
                else
                    predictions[i, 1] = subidx;
            }
            return predictions;
        }
    }
}
