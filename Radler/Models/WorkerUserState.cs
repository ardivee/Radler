using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radler.Models
{
    public class WorkerUserState
    {
        public string Message { get; set; }

        public WorkerUserState()
        {

        }

        public WorkerUserState(string message)
        {
            Message = message;
        }
    }
}
