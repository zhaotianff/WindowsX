using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Infrastructure.MouseEffect.Contracts
{
    public interface ICursorEffect
    {
        void Update(System.Windows.Point cursor, double elapsedSeconds);

        void Clear();
    }
}
