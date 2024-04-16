using WindowsX.Shell.Model.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WindowsX.Shell.Util
{
    /// <summary>
    /// https://github.com/zhaotianff/Windows-run-tool/blob/main/Windows-run-tool/Helper/RegexHelper.cs
    /// </summary>
    public class RegexHelper
    {
        public static List<ExecuteItem> MatchMsSettingRunItems(string input)
        {
            //学艺不精啊哎          
            var tdPattern = "<td>(?<td>(.*))</td>";
            var mssettingPattern = "ms-settings:\\S+";

            var list = new List<ExecuteItem>();
            var tdList = Regex.Matches(input, tdPattern);

            for (int i = 0; i < tdList.Count; i += 2)
            {
                ExecuteItem runItem = new ExecuteItem();
                runItem.Description = RegexGetSpanValue(tdList[i].Groups["td"].Value);
                var match = Regex.Match(tdList[i + 1].Groups["td"].Value, mssettingPattern);

                if (match.Success == false)
                    continue;

                if (match.Value.Contains("<br>") || match.Value.Contains("<br/>"))
                {
                    var subSettingArray = match.Value.Replace("<br>", ";").Replace("<br/>", ";").Split(';');
                    foreach (var subSetting in subSettingArray)
                    {
                        if (Regex.IsMatch(subSetting, mssettingPattern) == false)
                            continue;

                        ExecuteItem subRunItem = new ExecuteItem();
                        subRunItem.Description = runItem.Description;
                        subRunItem.Name = RegexReplaceChinese(RegexReplaceSpan(subSetting));
                        subRunItem.Path = subRunItem.Name;
                        list.Add(subRunItem);
                    }
                }
                else
                {
                    runItem.Name = RegexReplaceChinese(RegexReplaceSpan(match.Value));
                    runItem.Path = runItem.Name;
                    list.Add(runItem);
                }
            }

            return list;
        }

        public static List<ExecuteItem> MatchControlPanelRunItems(string input)
        {
            //先忽略/page
            //先使用简单粗暴的正则，有时间就优化一下
            var list = new List<ExecuteItem>();

            var itemRegexPattern = @"<h3(.*)</h3>\s*<ul>\s*(?<=<ul>)[\s\S]*?(?=</ul>)";
            var h3TagPattern = @"<h3[\s\S]*?>(?<value>[\s\S]*?)</h3>";
            var strongPattern = @"</strong>:\s+(?<value>[\w\.]+)";
            var matches = Regex.Matches(input, itemRegexPattern);

            foreach (Match match in matches)
            {
                var matchH3 = Regex.Match(match.Value, h3TagPattern);
                var matchStrong = Regex.Match(match.Value, strongPattern);

                if (matchH3.Success == false || matchStrong.Success == false)
                    continue;

                ExecuteItem runItem = new ExecuteItem();
                runItem.Description = matchH3.Groups["value"].Value;
                runItem.Name = runItem.Description;
                runItem.Path = "control /name " + matchStrong.Groups["value"].Value;
                list.Add(runItem);
            }
            return list;
        }

        private static string RegexGetSpanValue(string input)
        {
            //只测试了英文和中文
            input = input.Replace("&amp;", " ");
            var spanPattern = "(?<=>)[a-zA-Z\\s\u0391-\uFFE5-_]+(?=</)";
            var match = Regex.Match(input, spanPattern);

            if (match.Success)
                return match.Value;
            else
                return input;
        }

        private static string RegexReplaceSpan(string input)
        {
            var spanPattern = "<span(.*)\">|</span>|<span";
            return Regex.Replace(input, spanPattern, "");
        }

        private static string RegexReplaceChinese(string input)
        {
            var kanjiPattern = "[\u0391-\uFFE5]+";
            return Regex.Replace(input, kanjiPattern, "");
        }
    }
}
