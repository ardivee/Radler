using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Utils
{
    public static class BackgroundWorkerUtil
    {
        public static BackgroundWorker worker = new BackgroundWorker();
        public static DoWorkEventArgs workerEvent = null;
    }
}
