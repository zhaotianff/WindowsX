﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Silver_Arowana.Web.Util
{
    public class RegexUtil
    {
        public const string CharsetPattern = @"<meta[\s\S]+?charset=(?<charset>(.*?))""[\s\S]+?>";

        public static Tuple<bool, string> ExtractBingImage(string url)
        {
            string pattern = "url=(?<url>\\S*(.jpg|.png|.bmp))";
            Match match = Regex.Match(url, pattern);
            if (match.Success)
                return new Tuple<bool, string>(true, match.Groups["url"].Value.Replace("%2f", "/").Replace("%3a", ":"));
            return new Tuple<bool, string>(false, url);
        }
    }
}
