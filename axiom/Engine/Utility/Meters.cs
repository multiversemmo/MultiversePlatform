using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Axiom.Utility 
{

/// <summary>
///   The MeterManager creates and hands out TimingMeter
///   instances.  Those instances are looked up by meter "title", a
///   string name for the meter.  Meter instances also have a string
///   "category", so you can turn metering on and off by category.
///   All public methods of MeterManager are static, so the user
///   doesn't have to worry about managing the instance of
///   MeterManager.  
///
///   The workflow is that the user program creates several meters by
///   calling the static MakeMeter method, passing the title and
///   category of the meter.  That method looks up the meter by title,
///   creating it if it doesn't already exists, and returns the meter.
///   Thereafter, the user invokes the TimingMeter.Enter() and
///   TimingMeter.Exit() methods, each of which causes the
///   MeterManager to add a record to a collection of entries and
///   exits.  The record has the identity of the meter; whether it's
///   an entry or exit, and the time in processor ticks, captured
///   using the assembler primitive RDTSC.  At any point, the program
///   can call the method MeterManager.Report, which produces a report
///   based on the trace.  
///
/// </summary>
	public class MeterManager 
	{

#region Protected MeterManager members

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MeterManager));

		internal const int ekEnter = 1;
		internal const int ekExit = 2;
		internal const int ekInfo = 3;

		protected static MeterManager instance = null;
        protected static string meterLogFilename;
        protected static string meterEventsFilename;

		// Are we collecting now?
		public static bool collecting;
		// An id counter for timers
		protected short timerIdCounter;
		// The time when the meter manager was started
		protected long startTime;
		// The number of microseconds per tick; obviously a fraction
		protected float microsecondsPerTick;
        // The cost of calling Stopwatch.GetTimestamp().
        protected float costOfGetTimestamp;
        // True if we've computed the cost of Stopwatch.GetTimestamp()
        protected bool computedCostOfGetTimestamp = false;
		// The list of timing meter events
		internal List<MeterEvent> eventTrace;
		// Look up meters by title&category
		internal Dictionary<string, TimingMeter> metersByName;
		// Look up meters by id
		protected Dictionary<int, TimingMeter> metersById;

        // DEBUG
        private static List<MeterStackEntry> debugMeterStack = new List<MeterStackEntry>();

        private static void DebugAddEvent(TimingMeter meter, MeterEvent evt) {
            if (evt.eventKind == ekEnter) {
                debugMeterStack.Add(new MeterStackEntry(meter, 0, debugMeterStack.Count));
            } else if (evt.eventKind == ekExit) {
                Debug.Assert(debugMeterStack.Count > 0, "Meter stack is empty during ekExit");
                MeterStackEntry s = debugMeterStack[debugMeterStack.Count - 1];
                Debug.Assert(s.meter == meter, "Entered " + s.meter.title + "; Exiting " + meter.title);
                debugMeterStack.RemoveAt(debugMeterStack.Count - 1);
            } else if (evt.eventKind == ekInfo) {
                // just ignore these
            } else {
                Debug.Assert(false);
            }
        }

		protected static long CaptureCurrentTime()
		{
			return Stopwatch.GetTimestamp();
		}
		
		protected string OptionValue(string name, Dictionary<string, string> options)
		{
			string value;
			if (options.TryGetValue(name, out value))
				return value;
			else
				return "";
		}
		
		protected bool BoolOption(string name, Dictionary<string, string> options)
		{
			string value = OptionValue(name, options);
			return (value != "" && value != "false");
		}
		
		protected int IntOption(string name, Dictionary<string, string> options)
		{
			string value = OptionValue(name, options);
			return (value == "" ? 0 : int.Parse(value));
		}
		
		protected static void BarfOnBadChars(string name, string nameDescription)
		{
			if (name.IndexOf("\n") >= 0)
				throw new Exception(string.Format("Carriage returns are not allowed in {0}", nameDescription));
			else if (name.IndexOf(",") >= 0)
				throw new Exception(string.Format("Commas are not allowed in {0}", nameDescription));
		}
		
		protected MeterManager()
		{
			timerIdCounter = 1;
			eventTrace = new List<MeterEvent>();
			metersByName = new Dictionary<string, TimingMeter>();
			metersById = new Dictionary<int, TimingMeter>();
			startTime = CaptureCurrentTime();
			microsecondsPerTick = 1000000.0f / (float)Stopwatch.Frequency;
			instance = this;
		}
		
		protected TimingMeter GetMeterById(int id)
		{
			TimingMeter meter;
			metersById.TryGetValue(id, out meter);
			Debug.Assert(meter != null, string.Format("Meter for id {0} is not in the index", id));
			return meter;
		}
		
		protected void SaveToFileInternal(string pathname)
		{
			FileStream f = new FileStream(pathname, FileMode.Create, FileAccess.Write);
			StreamWriter writer = new StreamWriter(f);
			writer.Write(string.Format("MeterCount={0}\n", metersById.Count));
			foreach (KeyValuePair<int, TimingMeter> pair in instance.metersById) {
				TimingMeter meter = pair.Value;
				writer.Write(string.Format("{0},{1},{2}\n", meter.title, meter.category, meter.meterId));
			}
		}
		
		protected string IndentCount(int count)
		{
			if (count > 20)
				count = 20;
			string s = "|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-";
			return s.Substring(0, 2*count);
		}
		
        protected void ComputeCostOfGetTimestamp() {
            long startTime = Stopwatch.GetTimestamp();
            float microsecondsPerTick = 1000000.0f / (float)Stopwatch.Frequency;
            int cycles = 10000;
            for (int i = 0; i < cycles; i++)
                Stopwatch.GetTimestamp();
            long ticks = Stopwatch.GetTimestamp() - startTime;
            float totalUsecs = ((float)ticks) * microsecondsPerTick;
            costOfGetTimestamp = totalUsecs / (float)cycles;
            log.InfoFormat("MeterManager.ComputeCostOfGetTimestamp: Getting timestamp costs {0} usecs", costOfGetTimestamp);
            computedCostOfGetTimestamp = true;
        }

		protected long ToMicroseconds(long ticks, int eventCount)
		{
			return (long)((((float) ticks) * microsecondsPerTick) - eventCount * costOfGetTimestamp);
		}
		
		protected void DumpEventLog(List<MeterEvent> events)
		{
            if (MeterEventsFile == null)
                // Can't do anything
                return;
            if (File.Exists(MeterEventsFile))
				File.Delete(MeterEventsFile);
			FileStream f = new FileStream(MeterEventsFile, FileMode.Create, FileAccess.Write);
			StreamWriter writer = new StreamWriter(f);
			writer.Write(string.Format("Dumping meter event log on {0} at {1}; GetTimestamp {2:F}; units are usecs\r\n",
                    DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), costOfGetTimestamp));
			int indent = 0;
			List<MeterStackEntry> meterStack = new List<MeterStackEntry>();
			long firstEventTime = 0;
			for (int i=0; i<events.Count; i++) {
				short kind = events[i].eventKind;
				long t = events[i].eventTime;
				if (i == 0)
					firstEventTime = t;
				if (kind == ekInfo) {
					writer.WriteLine(string.Format("{0,12:D}         {1}{2} {3}{4}", 
                                                   ToMicroseconds(t - firstEventTime, i), 
												   IndentCount(indent + 1), "Info ", " ",
												   events[i].info));
					continue;
				}
				TimingMeter meter = GetMeterById(events[i].meterId);
				if (kind == ekEnter) {
					indent++;
					writer.WriteLine(string.Format("{0,12:D}         {1}{2} {3}.{4}", 
                                                   ToMicroseconds(t - firstEventTime, i),
												   IndentCount(indent),
												   "Enter", 
												   meter.category, meter.title));
					meterStack.Add(new MeterStackEntry(meter, t, i));
				} else if (kind == ekExit) {
					Debug.Assert(meterStack.Count > 0, "Meter stack is empty during ekExit");
					MeterStackEntry s = meterStack[meterStack.Count - 1];
                    Debug.Assert(s.meter == meter, "Entered " + s.meter.title + "; Exiting " + meter.title);
					writer.WriteLine(string.Format("{0,12:D} {1,7:D} {2}{3} {4}.{5}", 
                                                   ToMicroseconds(t - firstEventTime, i),
                                                   ToMicroseconds(t - s.eventTime, i - s.eventNumber),
												   IndentCount(indent),
												   "Exit ", 
												   meter.category, meter.title));
					indent--;
					meterStack.RemoveAt(meterStack.Count - 1);
				}
			}
			writer.Close();
		}
		
		protected static bool dumpEventLog = true;
		
		protected void GenerateReport(StreamWriter writer, List<MeterEvent> events, int start, Dictionary<string, string> options)
		{
			// For now, ignore options and just print the events
			if (dumpEventLog)
				DumpEventLog(events);

			// Zero the stack depth and added time
			foreach (KeyValuePair<int, TimingMeter> pair in instance.metersById) {
				TimingMeter meter = pair.Value;
				meter.stackDepth = 0;
				meter.addedTime = 0;
			}
			List<MeterStackEntry> meterStack = new List<MeterStackEntry>();
			int indent = 0;
			long firstEventTime = 0;
			for (int i=0; i<events.Count; i++) {
				short kind = events[i].eventKind;
				long t = events[i].eventTime;
				if (i == 0)
					firstEventTime = t;
				if (kind == ekInfo) {
					writer.WriteLine(string.Format("{0,12:D}         {1}{2} {3}{4}", 
                                                   ToMicroseconds(t - firstEventTime, i), 
												   IndentCount(indent + 1), "Info ", " ",
												   events[i].info));
					continue;
				}
				TimingMeter meter = GetMeterById(events[i].meterId);
				if (kind == ekEnter) {
					if (meter.accumulate && meter.stackDepth == 0)
						meter.addedTime = 0;
					if (i >= start && (!meter.accumulate || meter.stackDepth == 0)) {
						if (!meter.accumulate)
							indent++;
						writer.WriteLine(string.Format("{0,12:D}         {1}{2}{3}{4}.{5}", 
                                                       ToMicroseconds(t - firstEventTime, i), 
													   IndentCount(indent), "Enter", 
													   (meter.accumulate ? "*" : " "),
													   meter.category, meter.title));
					}
					meter.stackDepth++;
					meterStack.Add(new MeterStackEntry(meter, t, i));
				}
				else if (kind == ekExit) {
					Debug.Assert(meterStack.Count > 0, "Meter stack is empty during ekExit");
					MeterStackEntry s = meterStack[meterStack.Count - 1];
					meter.stackDepth--;
					Debug.Assert(s.meter == meter);
					if (meter.stackDepth > 0 && meter.accumulate)
						meter.addedTime += t - s.eventTime;
					else if (i >= start) {
						writer.WriteLine(string.Format("{0,12:D} {1,7:D} {2}{3}{4}{5}.{6}",
                                                       ToMicroseconds(t - firstEventTime, i),
                                                       ToMicroseconds(meter.accumulate ? meter.addedTime : t - s.eventTime, i - s.eventNumber),
													   IndentCount(indent), "Exit ", 
													   (meter.accumulate ? "*" : " "),
													   meter.category, meter.title));
						if (!meter.accumulate)
							indent--;
					}
					meterStack.RemoveAt(meterStack.Count - 1);
				}
			}
		}

#endregion Protected MeterManager members

#region Public MeterManager methods - - all static

		public static void Init()
		{
			if (instance == null)
				instance = new MeterManager();
		}
		
        public static bool Collecting {
            get {
                return MeterManager.collecting;
            }
            set {
                if (value == true && !instance.computedCostOfGetTimestamp)
                    instance.ComputeCostOfGetTimestamp();
                MeterManager.collecting = value;
            }
        }
		
		public static List<MeterEvent> ReturnEvents() {
            List<MeterEvent> events = instance.eventTrace;
            instance.eventTrace = new List<MeterEvent>();
            return events;
        }
        
        // Enable or disable meters by category
		public static void EnableCategory(string categoryName, bool enable)
		{
			Init();
			foreach (KeyValuePair<int, TimingMeter> pair in instance.metersById) {
				TimingMeter meter = pair.Value;
				if (meter.category == categoryName)
					meter.enabled = enable;
			}
		}

		// Enable or disable only a single category
		public static void EnableOnlyCategory(string categoryName, bool enable)
		{
			Init();
			foreach (KeyValuePair<int, TimingMeter> pair in instance.metersById) {
				TimingMeter meter = pair.Value;
				meter.enabled = (meter.category == categoryName ? enable : !enable);
			}
		}

		// Look up the timing meter by title; if it doesn't exist
		// create one with the title and category
		public static TimingMeter GetMeter(string title, string category)
		{
			string name = title + "&" + category;
			TimingMeter meter;
			Init();
			if (instance.metersByName.TryGetValue(name, out meter))
				return meter;
			else {
				BarfOnBadChars(title, "TimingMeter title");
				BarfOnBadChars(category, "TimingMeter category");
				short id = instance.timerIdCounter++;
				meter = new TimingMeter(title, category, id);
				instance.metersByName.Add(name, meter);
				instance.metersById.Add(id, meter);
				return meter;
			}
		}

		public static TimingMeter GetMeter(string title, string category, bool accumulate)
		{
			TimingMeter meter = GetMeter(title, category);
			meter.accumulate = true;
            return meter;
		}
		
		
		public static int AddEvent(TimingMeter meter, short eventKind, string info)
		{
			long time = CaptureCurrentTime();
            short meterId = (meter == null) ? (short)0 : meter.meterId;
            MeterEvent meterEvent = new MeterEvent(meterId, eventKind, time, info);
#if DEBUG
//            DebugAddEvent(meter, meterEvent);
#endif
    		instance.eventTrace.Add(meterEvent);
			return instance.eventTrace.Count;
		}
	
		public static void ClearEvents()
		{
			Init();
			instance.eventTrace.Clear();
		}

		public static void SaveToFile(string pathname)
		{
			instance.SaveToFileInternal(pathname);
		}
		
		public static long StartTime()
		{
			Init();
			return instance.startTime;
		}
		
		public static void AddInfoEvent(string info, params Object[] parms)
		{
			if (MeterManager.collecting)
				AddEvent(null, ekInfo, parms.Length == 0 ? info : string.Format(info, parms));
		}
		
		public static void Report(string title)
		{
			Report(title, instance.eventTrace, null, 0, "");
		}
		
		public static void Report(string title, List<MeterEvent> events)
        {
			Report(title, events, null, 0, "");
		}
		
		public static void Report(string title, List<MeterEvent> events, StreamWriter writer, int start, string optionsString)
		{
			bool opened = false;
			if (writer == null) {
                if (MeterLogFile == null)
                    // We cannot generate a report
                    return;
				FileStream f = new FileStream(MeterLogFile,
                                              (File.Exists(MeterLogFile) ? FileMode.Append : FileMode.Create), 
											  FileAccess.Write);
                writer = new StreamWriter(f);
				writer.Write(string.Format("\r\n\r\n\r\nStarting meter report on {0} at {1} for {2}; GetTimestamp {3:F}; units are usecs\r\n", 
                        DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), title, instance.costOfGetTimestamp));
				opened = true;
			}
			instance.GenerateReport(writer, events, start, null);
			if (opened)
				writer.Close();
		}

#endregion Public MeterManager methods
        public static string MeterLogFile
        {
            get
            {
                return meterLogFilename;
            }
            set
            {
                meterLogFilename = value;
            }
        }
        public static string MeterEventsFile
        {
            get
            {
                return meterEventsFilename;
            }
            set
            {
                meterEventsFilename = value;
            }
        }
	}

	public class MeterEvent
	{
		internal short meterId;
		internal short eventKind;
		internal long eventTime;
		internal string info;

		internal MeterEvent(short meterId, short eventKind, long eventTime, string info)
		{
			this.meterId = meterId;
			this.eventKind = eventKind;
			this.eventTime = eventTime;
			this.info = info;
		}
	}

	internal class MeterStackEntry
	{
		internal TimingMeter meter;
		internal long eventTime;
		internal int eventNumber;
        internal MeterStackEntry(TimingMeter meter, long eventTime, int eventNumber)
		{
			this.meter = meter;
			this.eventTime = eventTime;
            this.eventNumber = eventNumber;
		}
	}
	
	
	public class TimingMeter
	{
		internal TimingMeter(string title, string category, short meterId)
		{
			this.title = title;
			this.category = category;
			this.meterId = meterId;
			this.enabled = true;
			this.accumulate = false;
		}

		public string title;
		public string category;
		public bool enabled;
		public bool accumulate;
		public long addedTime;
		public long addStart;
		public int stackDepth;
		internal short meterId;

		public void Enter()
		{
			if (MeterManager.collecting && enabled)
				MeterManager.AddEvent(this, MeterManager.ekEnter, "");
		}

		public void Exit()
		{
			if (MeterManager.collecting && enabled)
				MeterManager.AddEvent(this, MeterManager.ekExit, "");
		}
	
	}

    public class AutoTimer : IDisposable {
        TimingMeter meter;
        public AutoTimer(TimingMeter meter) {
            this.meter = meter;
            meter.Enter();
        }
        public void Dispose() {
            meter.Exit();
            meter = null;
        }
    }
}

