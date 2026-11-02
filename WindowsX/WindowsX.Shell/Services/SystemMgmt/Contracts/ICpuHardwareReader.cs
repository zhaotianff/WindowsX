using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsX.Shell.Model.SystemMgmt;

namespace WindowsX.Shell.Services.SystemMgmt.Contracts
{
    public interface ICpuHardwareReader
    {
        CpuHardwareInformation Read(CancellationToken cancellationToken);
    }
}
