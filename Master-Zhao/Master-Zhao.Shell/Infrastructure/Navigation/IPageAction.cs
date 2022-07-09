using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.Infrastructure.Navigation
{
    public interface IPageAction
    {
        void Terminate();

        void ShowDefaultPage();
    }
}
