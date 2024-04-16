using WindowsX.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Web.Interface
{
    public interface IImageSearcher
    {
        public Task<List<ITagImg>> SearchImageAsync(string keyword, int page = 1);

    }
}
