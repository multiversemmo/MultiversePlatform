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

/**
 * This class maintains a histogram of times, and is used to report
 * queue delay times in the server.
 */
public class TimeHistogram implements Runnable {

    public TimeHistogram(String name) {
        this.name = name;
        this.timeBounds = defaultTimeBounds;
        this.reportingInterval = 5000;
        this.start();
    }
    
    public TimeHistogram(String name, Integer reportingInterval) {
        this.name = name;
        this.reportingInterval = reportingInterval;
        this.timeBounds = defaultTimeBounds;
        this.start();
    }
    
    public TimeHistogram(String name, Integer reportingInterval, Long[] timeBounds) {
        this.name = name;
        this.reportingInterval = reportingInterval;
        this.timeBounds = timeBounds;
        this.start();
    }
    
    public void stop() {
        running = false;
    }

    // Time given in nanoseconds
    public synchronized void addTime(long time) {
        pointCount++;
//         Log.info("TimeHistogram.addTime: pointCount " + pointCount + ", time " + time);
        for (int i=0; i<bucketCount; i++) {
            if (time < timeBounds[i]) {
                histogram[i]++;
                return;
            }
        }
        histogram[bucketCount]++;
    }
    
    protected void start()
    {
        running = true;
        bucketCount = timeBounds.length;
        pointCount = 0;
        histogram = new Integer[bucketCount + 1];
        for (int i=0; i<bucketCount + 1; i++)
            histogram[i] = 0;
        (new Thread(this, name)).start();
    }

    public void run() {
//         Log.info("In " + name + " TimeHistorgram.run");
        while (running) {
            try {
                Thread.sleep(reportingInterval);
                report();
            } catch (InterruptedException ex) {}
        }
    }

    protected synchronized void report() {
        long low = 0;
        String s = "";
        if (pointCount == 0)
            s = "No points in reporting interval";
        else {
            int total = 0;
            for (int i=0; i<bucketCount; i++) {
                if (histogram[i] > 0) {
                    total += histogram[i];
                    s += "[" + formatTime(low) + "-" + formatTime(timeBounds[i]) + "]: " + histogramString(i) + "  ";
                }
                low = timeBounds[i];
            }
            if (histogram[bucketCount] > 0)
                s += "[>" + formatTime(timeBounds[bucketCount - 1]) + "]: " + histogramString(bucketCount);
            s = "Samples " + total + " " + s;
        }
        pointCount = 0;
        for (int i=0; i<bucketCount + 1; i++)
            histogram[i] = 0;
        Log.info("Histogram (" + reportingInterval + " ms): " + s);
    }
    
    protected String formatTime(long t) {
        if (t < 1000000)
            return (t / 1000) + "us";
        else
            return (t / 1000000) + "ms";
    }
    
    protected String histogramString(int index) {
        return "" + histogram[index] + "(" + ((histogram[index] * 100) / pointCount) + "%)";
    }
    
    protected String name;
    protected int reportingInterval;   // In milliseconds
    protected Boolean running;
    protected int bucketCount;
    protected int pointCount;
    protected Integer[] histogram;
    protected Long[] timeBounds;
    // These numbers are in milliseconds
    protected Long[] defaultTimeBounds = new Long[] { 
        // 10 usec - 1 ms
        10000L, 20000L, 50000L, 100000L, 200000L, 500000L, 1000000L,
        // 2 ms - 1 sec
        2000000L, 5000000L, 10000000L, 20000000L, 40000000L, 100000000L, 150000000L, 200000000L, 500000000L, 1000000000L,
        // 2 sec - 100 sec
        2000000000L, 5000000000L, 10000000000L, 20000000000L, 50000000000L, 100000000000L };
}

    
        
