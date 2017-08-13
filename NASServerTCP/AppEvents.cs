using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NASClientTCP
{
    class AppEvents
    {
        const string localMachine = ".";
        private string processName;

        public AppEvents(string logName) :
             this(logName, Process.GetCurrentProcess().ProcessName)
        { }

        public AppEvents(string logName, string source) :
            this(logName, source, localMachine)
        { }

        public AppEvents(string logName, string source,
            string machineName = localMachine)
        {
            this.LogName = logName;
            this.SourceName = source;
            this.MachineName = machineName;
            Log = new EventLog(LogName, MachineName, SourceName);
        }

        private EventLog Log { get; set; } = null;
        public string LogName { get; set; }
        public string SourceName { get; set; }
        public string MachineName { get; set; } = localMachine;

        public void WriteToLog(string message, EventLogEntryType type,
            CategoryType category, EventIDType eventID)
        {
            if (Log == null)
                throw (new ArgumentNullException(nameof(Log),
                    "This Event Log has not been opened or has been closed."));
            EventLogPermission evtPermission =
                new EventLogPermission(EventLogPermissionAccess.Write, MachineName);
            evtPermission.Demand();

            Log.WriteEntry(message, type, (int)eventID, (short)category);
        }

        public void WriteToLog(string message, EventLogEntryType type,
            CategoryType category, EventIDType eventID, byte[] rawData)
        {
            if (Log == null)
                throw (new ArgumentNullException(nameof(Log),
                    "This Event Log has not been opened or has been closed."));
            EventLogPermission evtPermission =
                new EventLogPermission(EventLogPermissionAccess.Write, MachineName);
            evtPermission.Demand();

            Log.WriteEntry(message, type, (int)eventID, (short)category, rawData);
        }

        public IEnumerable<EventLogEntry> GetEntries()
        {
            EventLogPermission evtPermission =
                new EventLogPermission(EventLogPermissionAccess.Administer,
                MachineName);
            evtPermission.Demand();

            return Log?.Entries.Cast<EventLogEntry>().Where(evt =>
                evt.Source == SourceName);
        }

        public void ClearLog()
        {
            EventLogPermission evtPermission =
                new EventLogPermission(EventLogPermissionAccess.Administer,
                MachineName);

            evtPermission.Demand();

            if (!IsNonCustomLog())
                Log?.Clear();

        }

        public void CloseLog()
        {
            Log?.Close();
            Log = null;
        }

        public void DeleteLog()
        {
            if (!IsNonCustomLog())
                if (EventLog.Exists(LogName, MachineName))
                    EventLog.Delete(LogName, MachineName);
            CloseLog();
        }

        public bool IsNonCustomLog()
        {
            if (LogName == string.Empty ||
               LogName == "Application" ||
               LogName == "Security" ||
               LogName == "Setup" ||
               LogName == "System")
            {
                return true;
            }
            return false;
        }

        public enum EventIDType
        {
            NA = 0,
            Read = 1,
            Write = 2,
            ExceptionThrown = 3,
            BufferOverflowCondition = 4,
            SecurityFailure = 5,
            SecurityPotentiallyCompromised = 6
        }

        public enum CategoryType : short
        {
            None = 0,
            WriteToDB = 1,
            ReadFromDB = 2,
            WriteToFile = 3,
            ReadFromFile = 4,
            AppStartUp = 5,
            AppShutDown = 6,
            UserInput = 7
        }
    }

}
