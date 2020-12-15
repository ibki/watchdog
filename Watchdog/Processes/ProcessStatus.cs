using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watchdog.Processes
{
    public enum ProcessStatus
    {
        Running,
        NotRunning,
        Started,
        NotStarted,
        NotResponding,
        NotStopped,
        Stopped,
        Restarted,
        NotRestarted,
    }
}
