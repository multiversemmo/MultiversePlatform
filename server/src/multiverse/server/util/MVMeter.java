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
 * simple meter class that can avg out times
 * and print out the data every n units of time
 * @author cedeno
 *
 */
public class MVMeter {
    public MVMeter(String name) {
        setName(name);
    }
    
    public void add(Long time) {
        lock.lock();
        try {

            totalTime += time;
            count++;
            
            // check if we're at an interval and dump if so
            Long currentTime = System.currentTimeMillis();
            Long elapsedTime = currentTime - lastDumpTimeMS;
            
//            Log.debug("MVMeter: adding time to meter " + getName() + ", elapsed=" + elapsedTime + ", interval=" + intervalMS);
            if (elapsedTime > intervalMS) {
//                Log.debug("MVMeter: dumping stats for meter " + getName());
                dumpStats(elapsedTime);
                lastDumpTimeMS = currentTime;
                totalTime = 0;
                count = 0;
            }
//            else {
//                Log.debug("MVMeter: not elapsed for " + getName());
//            }
        }
        finally {
            lock.unlock();
        }
    }

    void dumpStats(Long elapsedMS) {
        long avgTime = totalTime / count;
        Log.info("MVMeter: meter=" + getName() + ", avgTime=" + avgTime + ", totalTime=" + totalTime + ", entries=" + count + ", elapsedMS=" + elapsedMS);
    }
    
    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }
    
    private String name = null;
    private long totalTime = 0;
    private int count = 0;
    private long lastDumpTimeMS = System.currentTimeMillis();
    
    private Lock lock = LockFactory.makeLock("MVMeter");
    
    // default of 10 seconds
    public static final int intervalMS = 10000;
}
