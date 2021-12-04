using Master_Zhao.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Web.Interface
{
    public interface IImageSearcher
    {
        public Task<List<ITagImg>> SearchImageAsync(string keyword, int page = 1);

    }
}
