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
 * very simple timer - not thread safe
 * @author cedeno
 *
 */
public class MVTimer {

    public MVTimer(String name) {
        this.name = name;
    }
    
    public String toString() {
        return "(elapsedTime=" + elapsed() + ")";
    }
    
    public String getName() {
        return name;
    }
    
    private String name = null;
    
    public void start() {
        if (startTime != null) {
            throw new RuntimeException("started twice");
        }
        startTime = System.nanoTime();
    }
    public void stop() {
        if (startTime == null) {
            throw new RuntimeException("stop without start");
        }
        elapsedTime += (System.nanoTime() - startTime);
        startTime = null;
    }
    
    // in milli
    public long elapsed() {
        if (startTime != null) {
            throw new RuntimeException("must be stopped to get elapsed");
        }
        return (long)((double)elapsedTime / (double)1000000);
    }
    
    public void reset() {
        startTime = null;
        elapsedTime = 0L;
    }
    
    Long startTime = null;
    Long elapsedTime = 0L;
    //Lock lock = LockFactory.makeLock("TimerLock");
}
