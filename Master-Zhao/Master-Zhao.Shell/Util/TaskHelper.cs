using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Master_Zhao.Shell.Util
{
    public class TaskHelper
    {
        public static void DispatcherRunTask(Action action)
        {
            Task.Factory.StartNew(action, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
