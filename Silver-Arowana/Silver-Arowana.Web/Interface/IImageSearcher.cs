using Silver_Arowana.Web.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silver_Arowana.Web.Interface
{
    public interface IImageSearcher
    {
        public Task<List<ITagImg>> SearchImageAsync(string keyword, int page = 1);

    }
}
