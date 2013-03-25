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

import java.util.concurrent.locks.*;

public class LockFactory {

    public static ReentrantLock makeLock(String name) {
        if (USE_DLOCK) {
            return new DLock(name);
        }
        else {
            return new ReentrantLock();
        }
    }

    public static boolean USE_DLOCK = false;


    public static void main(String[] args) {
        Lock A = LockFactory.makeLock("A");
        Lock B = LockFactory.makeLock("B");
        Lock C = LockFactory.makeLock("C");
        Lock D = LockFactory.makeLock("D");
        Lock Z = LockFactory.makeLock("Z");
        Lock Y = LockFactory.makeLock("Y");

        A.lock();
        B.lock();
        C.lock();
        A.lock();
        A.unlock();
        C.unlock();
        D.lock();
        D.unlock();
        B.unlock();
        A.unlock();
        Z.lock();
        Y.lock();
        Y.unlock();
        Z.unlock();
        
        DLock.detectCycle();
//         System.out.println("LockFactory done: cycleFound=" + found);
    }
}
