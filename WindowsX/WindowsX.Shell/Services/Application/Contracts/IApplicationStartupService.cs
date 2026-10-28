using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Services.Application.Contracts
{
    public interface IApplicationStartupService
    {
        bool IsEnabled();
        void SetEnabled(bool enabled);
    }
}
