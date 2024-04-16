using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Web.Model
{
    public interface ITagImg
    {
        public string Src { get; set; }

        public string Alt { get; set; }

        public string DetailUrl { get; set; }

    }
}
