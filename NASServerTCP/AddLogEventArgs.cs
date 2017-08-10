using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class AddLogEventArgs
    {
        private EventLogEntry entry;

        public AddLogEventArgs(EventLogEntry e)
        {
            this.entry = e;
        }

        public EventLogEntry Entry
        {
            get { return entry; }
            set { entry = value; }
        }
    }
}
