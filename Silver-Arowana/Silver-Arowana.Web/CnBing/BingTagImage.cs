using Silver_Arowana.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silver_Arowana.Web.CnBing
{
    public class BingTagImage : ITagImg
    {
        public string Src { get; set; }
        public string Alt { get; set; }
        public string DetailUrl { get; set; }

        #region Bing 
        public int Width { get; set; }

        public int Height { get; set; }
        #endregion
    }
}
