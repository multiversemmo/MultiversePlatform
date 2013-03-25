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

public class TimingMeter {
    
    protected TimingMeter(String title, String category, short meterId) {
        this.title = title;
        this.category = category;
        this.meterId = meterId;
        this.enabled = true;
        this.accumulate = false;
    }

    public String title;

    public String category;

    public boolean enabled;

    public boolean accumulate;

    public long addedTime;

    public long addStart;

    public int stackDepth;

    protected short meterId;

    public void Enter() {
        if (MeterManager.Collecting && enabled)
            MeterManager.AddEvent(this, MeterManager.ekEnter);
    }

    public void Exit() {
        if (MeterManager.Collecting && enabled)
            MeterManager.AddEvent(this, MeterManager.ekExit);
    }
}
