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

import java.util.concurrent.locks.Lock;

/**
 * used to gathering metrics.  counts how many times something happens
 * and prints out to the log at intervals.
 * @author cedeno
 *
 */
public class CountMeter {
    public CountMeter(String name) {
        this.name = name;
    }
    
    public void add() {
        boolean logCount = false;
        int currentCount = 0;
        long elapsed;
        lock.lock();
        try {
            this.count++;
            long now = System.currentTimeMillis();
            elapsed = now - lastRun;
            if (elapsed > intervalMS) {
                currentCount = count;
                count = 0;
                logCount = true;
                lastRun = now;
            }
        }
        finally {
            lock.unlock();
        }
        if (logCount && logging) {
            Log.info("CountMeter: counter=" + getName() +
                " count=" + currentCount + " elapsed=" + elapsed);
        }
    }
    
    public int getCount() {
        return count;
    }
    
    public void setName(String name) {
        this.name = name;
    }
    public String getName() {
        return name;
    }

    public void setLogging(boolean enable) {
        logging = enable;
    }
    
    private String name;
    Lock lock = LockFactory.makeLock("CounterLock");
    private int count = 0;
    private long intervalMS = 10000;
    private long lastRun = System.currentTimeMillis();
    private boolean logging = true;
}
