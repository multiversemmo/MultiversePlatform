/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

package multiverse.server.util;

import java.io.*;
import java.util.*;

/**
*   The MeterManager creates and hands out {@link TimingMeter}
*   instances.  Those instances are looked up by meter "title", a
*   string name for the meter.  Meter instances also have a string
*   "category", so you can turn metering on and off by category.
*   All public methods of MeterManager are static, so the user
*   doesn't have to worry about managing the instance of
*   MeterManager.
*   <p>
*   The workflow is that the user program creates several meters by
*   calling the static MakeMeter method, passing the title and
*   category of the meter.  That method looks up the meter by title,
*   creating it if it doesn't already exists, and returns the meter.
*   Thereafter, the user invokes the {@link TimingMeter#Enter()} and
*   {@link TimingMeter#Exit()} methods, each of which causes the
*   MeterManager to add a record to a collection of entries and
*   exits.  The record has the identity of the meter; whether it's
*   an entry or exit, and the time in processor ticks, captured
*   using the assembler primitive RDTSC.  At any point, the program
*   can call the method {@link #Report(java.lang.String) Report()},
*   which produces a report based on the trace.  
**/

public class MeterManager 
{

    ////////////////////////////////////////////////////////////////////////
    //
    // Protected Members
    //
    ////////////////////////////////////////////////////////////////////////
    
    protected static MeterManager instance = null;
    // An id counter for timers
    protected short timerIdCounter;
    // The time when the meter manager was started
    protected long startTime;
    // The number of microseconds per tick; obviously a fraction
    float microsecondsPerTick;
    // The list of timing meter events
    protected Vector<MeterEvent> eventTrace;
    // Look up meters by title
    protected HashMap<String, TimingMeter> metersByTitle;
    // Look up meters by id
    protected HashMap<Short, TimingMeter> metersById;
    protected static long CaptureCurrentTime()
    {
        return System.nanoTime();
    }
                
    protected String OptionValue(String name, HashMap<String, String> options)
    {
        String value;
        if ((value = options.get(name)) != null)
            return value;
        else
            return "";
    }
                
    protected boolean BooleanOption(String name, HashMap<String, String> options)
    {
        String value = OptionValue(name, options);
        return (value != "" && value != "false");
    }
                
    protected int IntOption(String name, HashMap<String, String> options)
    {
        String value = OptionValue(name, options);
        return (value == "" ? 0 : Integer.parseInt(value));
    }
                
    protected static void BarfOnBadChars(String name, String nameDescription) throws Exception
    {
        if (name.indexOf("\n") >= 0)
            throw new Exception(String.format("Carriage returns are not allowed in %1$s", nameDescription));
        else if (name.indexOf(",") >= 0)
            throw new Exception(String.format("Commas are not allowed in %1$s", nameDescription));
    }
                
    static protected class MeterEvent
    {
        public short meterId;
        public short eventKind;
        public long eventTime;

        public MeterEvent(short meterId, short eventKind, long eventTime)
            {
                this.meterId = meterId;
                this.eventKind = eventKind;
                this.eventTime = eventTime;
            }
    }

    static protected class MeterStackEntry
    {
        public TimingMeter meter;
        public long eventTime;
        public MeterStackEntry(TimingMeter meter, long eventTime)
            {
                this.meter = meter;
                this.eventTime = eventTime;
            }
    }

    protected MeterManager()
    {
        timerIdCounter = 1;
        eventTrace = new Vector<MeterEvent>();
        metersByTitle = new HashMap<String, TimingMeter>();
        metersById = new HashMap<Short, TimingMeter>();
        startTime = CaptureCurrentTime();
        microsecondsPerTick = 1.0f/1000.0f;
        instance = this;
    }
                
    protected TimingMeter GetMeterById(int id)
    {
        TimingMeter meter = metersById.get(id);
        assert meter != null : String.format("Meter for id %1$d is not in the index", id);
        return meter;
    }
                
    protected void SaveToFileInternal(String pathname)
    {
        try {
        FileWriter writer = new FileWriter(pathname);
        writer.write(String.format("MeterCount=%1$d\n", metersById.size()));
        for (TimingMeter meter : instance.metersById.values()) {
            writer.write(String.format("%1$s,%2$s,%3$d\n", meter.title, meter.category, meter.meterId));
        }
        } catch (IOException e) {}
    }
                
    protected String IndentCount(int count)
    {
        if (count > 20)
            count = 20;
        String s = "|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-|-";
        return s.substring(0, 2*count);
    }
                
    protected long ToMicroseconds(long ticks)
    {
        return (long)(((float) ticks) * microsecondsPerTick);
    }
                
    protected void DumpEventLogInternal()
    {
        String p = "../MeterEvents.txt";
        try {
            FileWriter writer = new FileWriter(p);
            writer.write(String.format("Dumping meter event log; units are usecs\r\n"));
            int indent = 0;
            Vector<MeterStackEntry> meterStack = new Vector<MeterStackEntry>();
            for (int i=0; i<eventTrace.size(); i++) {
                MeterEvent me = eventTrace.get(i);
                TimingMeter meter = GetMeterById(me.meterId);
                short kind = me.eventKind;
                long t = me.eventTime;
                if (kind == ekEnter) {
                    indent++;
                    writer.write(String.format("%112d %2s%3s %4s.%5s\r\n", 
                                               ToMicroseconds(t - startTime),
                                               IndentCount(indent),
                                               "Enter", 
                                               meter.category, meter.title));
                    meterStack.add(new MeterStackEntry(meter, t));
                }
                else {
                    assert meterStack.size() > 0 : "Meter stack is empty during ekExit";
                    MeterStackEntry s = meterStack.get(meterStack.size() - 1);
                    assert s.meter == meter;
                    writer.write(String.format("%112d %2s%3s %4s.%5s\r\n", 
                                               ToMicroseconds(t - s.eventTime),
                                               IndentCount(indent),
                                               "Exit ", 
                                               meter.category, meter.title));
                    indent--;
                    meterStack.remove(meterStack.size() - 1);
                }
            }
            writer.close();
        }
        catch (Exception e) {}
    }
                
    protected static boolean dumpEventLog = false;
                
    protected void GenerateReport(FileWriter writer, int start, HashMap<String, String> options)
    {
        // For now, ignore options and just print the event trace
        if (dumpEventLog)
            DumpEventLog();

        try {
            // Zero the stack depth and added time
            for (TimingMeter meter : instance.metersById.values()) {
                meter.stackDepth = 0;
                meter.addedTime = 0;
            }
            Vector<MeterStackEntry> meterStack = new Vector<MeterStackEntry>();
            int indent = 0;
            for (int i=0; i<eventTrace.size(); i++) {
                MeterEvent me = eventTrace.get(i);
                TimingMeter meter = GetMeterById(me.meterId);
                short kind = me.eventKind;
                long t = me.eventTime;
                if (kind == ekEnter) {
                    if (meter.accumulate && meter.stackDepth == 0)
                        meter.addedTime = 0;
                    if (i >= start && (!meter.accumulate || meter.stackDepth == 0)) {
                        // Don't display the enter and exit if the
                        // exit is the very next record, and the
                        // elapsed usecs is less than dontDisplayUsecs
                        if (eventTrace.size() > i + 1 && eventTrace.get(i+1).meterId == me.meterId &&
                            eventTrace.get(i+1).eventKind == ekExit && 
                            ToMicroseconds(eventTrace.get(i+1).eventTime - t) < DontDisplayUsecs) {
                            i++;
                            continue;
                        }
                        writer.write(String.format("%112d %2s%3s %4s.%5s\r\n", 
                                                   ToMicroseconds(t - startTime), 
                                                   IndentCount(indent), "Enter", 
                                                   (meter.accumulate ? "*" : " "),
                                                   meter.category, meter.title));
                        if (!meter.accumulate)
                            indent++;
                    }
                    meter.stackDepth++;
                    meterStack.add(new MeterStackEntry(meter, t));
                }
                else if (kind == ekExit) {
                    assert meterStack.size() > 0 : "Meter stack is empty during ekExit";
                    MeterStackEntry s = meterStack.get(meterStack.size() - 1);
                    meter.stackDepth--;
                    assert s.meter == meter;
                    if (meter.stackDepth > 0 && meter.accumulate)
                        meter.addedTime += t - s.eventTime;
                    else if (i >= start) {
                        if (!meter.accumulate)
                            indent--;
                        writer.write(String.format("%112d %2s%3s %4s.%5s\r\n",
                                                   ToMicroseconds(meter.accumulate ? meter.addedTime : t - s.eventTime),
                                                   IndentCount(indent), "Exit ", 
                                                   (meter.accumulate ? "*" : " "),
                                                   meter.category, meter.title));
                    }
                    meterStack.remove(meterStack.size() - 1);
                }
            }
        }
        catch (Exception e) {}
    }

    ////////////////////////////////////////////////////////////////////////
    //
    // Public Members
    //
    ////////////////////////////////////////////////////////////////////////
    
    // Are we collecting now?
    public static boolean Collecting;
    // Don't display Enter/Exit pair in MeterLog.txt if the elapsed
    // time between the two is less than this number of microseconds
    public static int DontDisplayUsecs = 3;

    public static short ekEnter = 1;
    public static short ekExit = 2;

    public static void Init()
    {
        if (instance == null)
            instance = new MeterManager();
    }
                
    // Enable or disable meters by category
    public static void EnableCategory(String categoryName, boolean enable)
    {
        Init();
        for (TimingMeter meter : instance.metersById.values()) {
            if (meter.category == categoryName)
                meter.enabled = enable;
        }
    }

    // Look up the timing meter by title; if it doesn't exist
    // create one with the title and category
    public static TimingMeter GetMeter(String title, String category)
    {
        TimingMeter meter;
        Init();
        if ((meter = instance.metersByTitle.get(title)) != null)
            return meter;
        else {
            try {
                BarfOnBadChars(title, "TimingMeter title");
                BarfOnBadChars(title, "TimingMeter category");
            }
            catch(Exception e) {
                return null;
            }
            short id = instance.timerIdCounter++;
            meter = new TimingMeter(title, category, id);
            instance.metersByTitle.put(title, meter);
            instance.metersById.put(id, meter);
            return meter;
        }
    }

    public static TimingMeter GetMeter(String title, String category, boolean accumulate)
    {
        TimingMeter meter = GetMeter(title, category);
        meter.accumulate = true;
        return meter;
    }
                
                
    public static int AddEvent(TimingMeter meter, short eventKind)
    {
        long time = CaptureCurrentTime();
        instance.eventTrace.add(new MeterEvent(meter.meterId, eventKind, time));
        return instance.eventTrace.size();
    }
        
    public static void ClearEvents()
    {
        Init();
        instance.eventTrace.clear();
    }

    public static void SaveToFile(String pathname)
    {
        instance.SaveToFileInternal(pathname);
    }
                
    public static long StartTime()
    {
        Init();
        return instance.startTime;
    }
                
    public static void Report(String title)
    {
        Report(title, null, 0, "");
    }
                
    public static void Report(String title, FileWriter writer, int start, String optionsString)
    {
        boolean opened = false;
        if (writer == null) {
            String p = "../MeterLog.txt";
            try {
                writer = new FileWriter(p, true);
                writer.write(String.format("\r\n\r\n\r\nStarting meter report for %1s; starting event %2d; units are usecs\r\n", 
                                           title, start));
                opened = true;
            }
            catch(IOException e) {
                return;
            }
        }
        instance.GenerateReport(writer, start, null);
        if (opened)
                try {
                        writer.close();
                } catch (IOException e) {}
    }

    public static void DumpEventLog()
    {
        Init();
        instance.DumpEventLogInternal();
    }


}
