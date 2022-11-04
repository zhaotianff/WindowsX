using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.Util
{
    public static class StringExtensions
    {
        public static string GetDateWithoutYear(this string str)
        {
            var arr = str.Split("-");

            if(arr.Length == 3)
            {
                return $"{arr[1]}月{arr[2]}日";
            }

            return str;
        }
    }
}
