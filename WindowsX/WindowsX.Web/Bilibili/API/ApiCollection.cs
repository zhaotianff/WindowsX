using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Web.Bilibili.API
{
    public class ApiCollection
    {
        public static string GetAlbumApi = "https://api.bilibili.com/x/web-interface/view?bvid={0}";

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// "aid":380147327
        /// "cid":469768162
        /// </remarks>
        /// 
        public static string GetVideoApi = "https://api.bilibili.com/x/player/playurl?fnval=80&avid={a}&cid={c}";
    }
}
