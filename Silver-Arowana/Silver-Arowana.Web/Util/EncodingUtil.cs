using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Silver_Arowana.Web.Util
{
    public class EncodingUtil
    {
        public static Encoding GetEncoding(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 5000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream, Encoding.Default))
                    {
                        string str = sr.ReadToEnd();
                        return GetEncodingFromStr(str);
                    }
                }
            }
            catch
            {
                return Encoding.Default;
            }

        }

        private static Encoding GetEncodingFromStr(string source)
        {
            Regex regex = new Regex(RegexUtil.CharsetPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match match = regex.Match(source);
            if (match.Success)
            {
                string charsestStr = match.Groups["charset"].Value;
                if (string.IsNullOrEmpty(charsestStr))
                {
                    int startIndex = match.Value.ToLower().IndexOf("\"");
                    int endIndex = match.Value.ToLower().LastIndexOf("\"");
                    charsestStr = match.Value.Substring(startIndex + 1, endIndex - startIndex - 1);
                }
                return Encoding.GetEncoding(charsestStr);
            }
            return Encoding.Default;
        }
    }
}
