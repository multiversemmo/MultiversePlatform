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
import java.util.concurrent.locks.*;

public class Logger {
    public Logger(String subj) {
	this.subj = subj;
    }

    public void net(String s) {
        if (!Log.loggingNet || !Logger.subjectStatus(subj)) {
            return;
        }
	Log.net(subj + ", " + s);
    }

    public void debug(String s) {
        if (!Log.loggingDebug || !Logger.subjectStatus(subj)) {
            return;
        }
        Log.debug(subj + ", " + s);
    }

    public void info(String s) {
        if (!Log.loggingInfo || !Logger.subjectStatus(subj)) {
            return;
        }
	Log.info(subj + ": " + s);
    }

    public void warn(String s) {
        if (!Log.loggingWarn || !Logger.subjectStatus(subj)) {
            return;
        }
	Log.warn(subj + ": " + s);
    }

    public void error(String s) {
	Log.error(subj + ": " + s);
    }

    public void dumpStack() {
        Log.dumpStack(subj + ": ");
    }
    
    public void dumpStack(String context) {
        Log.dumpStack(subj + ": " + context);
    }
    
    public void dumpStack(String context, Thread thread) {
        Log.dumpStack(subj + ": " + context, thread);
    }

    public void exception(Exception e) {
        Log.exception(subj + ": ", e);
    }
    
    public void exception(String context, Exception e) {
        Log.exception(subj + ": " + context, e);
    }

    private String subj = null;

    public static void logSubject(String subj) {
        staticLock.lock();
        try {
            Logger.subjSet.add(subj);
        }
        finally {
            staticLock.unlock();
        }
    }

    private static boolean subjectStatus(String subj) {
        staticLock.lock();
        try {
            // if we didnt specify, then probably we just log everything
            if (subjSet.isEmpty()) {
                return true;
            }
            return subjSet.contains(subj);
        }
        finally {
            staticLock.unlock();
        }
    }
    private static Set<String> subjSet = new HashSet<String>();
    private static Lock staticLock = LockFactory.makeLock("LoggerStaticLock");
}
