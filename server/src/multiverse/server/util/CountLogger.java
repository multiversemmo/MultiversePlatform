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

import java.util.*;

/**
 * This class provides APIs to register and unregister objects which
 * derive from the internal static Counter class.  It runs every n
 * milliseconds, as determined by a constructor argument, and when it
 * runs, it interrogates the registered objects, using methods defined
 * on the Counter class, getting the current count and a string.  It
 * logs a single line every time it runs.  That line contains, for
 * each registered object, the string description of the counter, and
 * the difference between the current count and count during the
 * previous iteration.
 */
public class CountLogger implements Runnable {
    
    /**
     * The constructor args determine how often the CountLogger
     * instance runs, and at the log level the log lines
     * @param runInterval The number of milliseconds between runs
     * @param logLevel The level at which the information is logged.
     */
    public CountLogger(String name, int runInterval, int logLevel) {
        this.name = name;
        this.runInterval = runInterval;
        this.logLevel = logLevel;
    }
    
    /**
     * The constructor args determine how often the CountLogger
     * instance runs, and at the log level the log lines
     * @param name Logger name.
     * @param runInterval Milliseconds between runs.
     * @param logLevel The level at which the information is logged.
     * @param showAllNonzeroCounters 
     */
    public CountLogger(String name, int runInterval, int logLevel, boolean showAllNonzeroCounters) {
        this.name = name;
        this.runInterval = runInterval;
        this.logLevel = logLevel;
        this.showAllNonzeroCounters = showAllNonzeroCounters;
    }
    
    /**
     * Create a counter with the given name and add it to the list of
     * counters
     */
    public Counter addCounter(String name) {
        Counter counter = new Counter(name);
        addCounter(counter);
        return counter;
    }
    
    /**
     * Create a counter with the given name and initial count and add
     * it to the list of counters
     */
    public Counter addCounter(String name, long count) {
        Counter counter = new Counter(name, count);
        addCounter(counter);
        return counter;
    }
    
    /**
     * Add a counter created by calling the Counter constructor to the
     * list of counters
     */
    public void addCounter(Counter counter) {
        synchronized(counters) {
            counters.add(counter);
        }
    }
    
    /**
     * Remove the Counter arg from the list of counters
     */
    public void removeCounter(Counter counter) {
        synchronized(counters) {
            counters.remove(counter);
        }
    }
    
    /**
     * Start counter logging thread.
     */
    public void start() {
        if (running)
            Log.error("CountLogger.start: CountLogger thread is already running!");
        else {
            countLoggerThread = new Thread(this,name);
            running = true;
            countLoggerThread.start();
        }
    }
    
    /**
     * Stop counter logging thread.
     */
    public void stop() {
        if (!running)
            Log.error("CountLogger.stop: CountLogger thread isn't running!");
        else
            running = false;
    }

    /** Control generation of log messages.
    */
    public void setLogging(boolean enable)
    {
        logging = enable;
    }

    /**
     * The run method loops sleeping for the run interval, then logs a
     * line containing for each counter the counter name and the count
     * delta since the last time it ran.
     */
    public void run() {
        while (running) {
            try {
                Thread.sleep(runInterval);
            } catch (Exception e) {
                Log.exception("CountLogger.run: error in Thread.sleep", e);
            }
            boolean logging = this.logging && (Log.getLogLevel() <= logLevel);
            String s = "";
            synchronized(counters) {
                for (Counter counter : counters) {
                    long delta = counter.count - counter.lastCount;
                    if (delta == 0 && !showAllNonzeroCounters)
                        continue;
                    if (logging) {
                        if (! s.equals(""))
                            s += ", ";
                        if (showAllNonzeroCounters)
                            s += counter.name + " " + delta + "|" + counter.count;
                        else
                            s += counter.name + " " + delta;
                    }
                    counter.lastCount = counter.count;
                }
            }
            if (logging)
                Log.logAtLevel(logLevel, name + ": " + (s.equals("") ? "No non-zero counters" : s));
        }
    }
    
    /**
     * The counter class, managed by the CountLogger instance
     */
    public static class Counter {
        
        public Counter(String name) {
            this.name = name;
            this.count = 0;
            this.lastCount = 0;
        }
        
        public Counter(String name, long count) {
            this.name = name;
            this.count = count;
            this.lastCount = count;
        }
        
        /**
         * Increment by 1
         */
        public void add() {
            count++;
        }
        
        /**
         * Increment by an arbitrary number
         */
        public void add(long addend) {
            count += addend;
        }
        
        public long getCount() {
            return count;
        }

        public String name;
        public long count;
        public long lastCount;
    }

    protected List<Counter> counters = new LinkedList<Counter>();
    protected Thread countLoggerThread = null;
    protected boolean running = false;

    protected String name;
    protected int runInterval;
    protected int logLevel;
    protected boolean logging = true;
    protected boolean showAllNonzeroCounters = false;
}
