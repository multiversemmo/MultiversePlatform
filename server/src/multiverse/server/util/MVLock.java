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

import java.util.concurrent.TimeUnit;
import java.util.*;
import java.util.concurrent.locks.ReentrantLock;

/**
 * this lock keeps track of its call stack.
 * it can also timeout and throw an exception (to help debug 
 * deadlocks).  the exception will have all locks 
 * when you call lock() it adds to the stack.
 * when you call unlock() it removes from the stack.
 *
 * you can get the lock's stack by calling getStack()
 */
public class MVLock extends ReentrantLock {
    public MVLock(String name) {
	super();

	// add this lock to the global lock set
	synchronized(lockSet) {
	    lockSet.add(this);
	}
	this.lockName = name;
    }

    public void setName(String name) {
	lockName = name;
    }

    public String getLockName() {
	return lockName;
    }

    public void lock() {
        lock(DefaultLockTimeoutMS);
    }

    public void lock(long lockTimeoutMS) {
// 	System.out.println("MVLock.lock: locking lock " + getLockName());

	// put the stack trace onto the lock's list
	Throwable t = new Throwable();
	synchronized(this) {
	    stackTraceList.add(new LStack(LStack.LOCK_ACTION, 
					  t.getStackTrace()));
	}

	boolean acquiredLock = false;
	try {
	    acquiredLock = super.tryLock(lockTimeoutMS, TimeUnit.MILLISECONDS);
	}
	catch(Exception e) {
	    System.err.println("mvlock.lock: got exception: " + e);
	    System.exit(-1);
	}
	if (acquiredLock) {
	    // System.out.println("MVLock.lock: acquired lock " + getLockName());
	    synchronized(this) {
		stackTraceList.add(new LStack(LStack.LOCK_ACQUIRED, 
					      t.getStackTrace()));
	    }
	}
	else {
	    throwException();
	}
    }

    public void unlock() {
	// System.out.println("MVLock.unlock: unlocking " + getLockName());

	// add to the lock's stack trace
	Throwable t = new Throwable();
	/* LStack stack = */ new LStack(LStack.UNLOCK_ACTION, 
				  t.getStackTrace());
	super.unlock();
	
	// if we dont own the lock anymore, clear the stack trace
	synchronized(this) {
	    if (! this.isHeldByCurrentThread()) {
		// System.out.println("MVLock.unlock: fully released lock, clearing stack");
		stackTraceList.clear();
	    }
	    else {
		// System.out.println("MVLock.unlock: unlocked, but still holds lock");
	    }
	}
    }

    /**
     * returns a copy of the stacktracelist
     */
    synchronized List getStackTraceList() {
	return new LinkedList<LStack>(stackTraceList);
    }

    /**
     * returns a string describing the stack trace
     */
    synchronized void getStackTraceString() {
	System.err.println("-----------------------------------------\n" +
                           "stacktrace for lock " + getLockName() + "\n");
	Iterator iter = stackTraceList.iterator();
	while(iter.hasNext()) {
	    LStack lstack = (LStack) iter.next();
	    System.err.println("trace=" + lstack.toString());
	}
// 	s += "ENDOFTHREAD\n\n";
// 	return s;
    }
					  
    void throwException() {
	System.err.println("MVLock: apparent deadlock, lock in question is: " +
                           lockName + ", thread=" + 
                           Thread.currentThread().getName() + "\n" +
                           ", the lock's stack trace follows:\n");
        Thread.dumpStack();
	String msg = "MVLock: apparent deadlock, lock in question is: " +
	    lockName + ", thread=" + Thread.currentThread().getName();
        getStackTraceString();

	// go through each lock in the system and print out each lock's
	// stracktrace
	synchronized(lockSet) {
	    System.err.println("MVLock: going through global lock set to print debug info, total number of locks: " + lockSet.size());
	    Iterator iter = lockSet.iterator();
	    int i=0;
	    while (iter.hasNext()) {
		MVLock l = (MVLock) iter.next();
		if (l.isLocked()) {
		    System.err.println("lock being used: " + i);
		    l.getStackTraceString();
		}
		i++;
	    }
	}
	System.err.println("MVLock: deadlock info:\n" + msg + 
			   "\n----End of deadlock info----");
	throw new RuntimeException(msg);
    }

    String threadName = null;
    String lockName = "unknown";

    /**
     * list of LStack objects
     */
    private List<LStack> stackTraceList = new LinkedList<LStack>();

    /**
     * how long to wait before we think we are in deadlock
     */
//    private static int timeOutMS = 1000;

    /**
     * class to hold the action (lock/unlock and the stack trace).
     * this gets added to the lock's list of stack traces
     */
    static class LStack {
	// 0 is 'lock' and 1 is 'unlock'
	LStack(int action, StackTraceElement[] array) {
	    this.threadName = Thread.currentThread().getName();
	    this.action = action;
	    this.stackArray = array;
            this.time = System.currentTimeMillis();
	}

	public String toString() {
	    String actionString = null;
	    if (action == 0) {
		actionString = "LOCK_ATTEMPT";
	    }
	    else if (action == 1) {
		actionString = "UNLOCK";
	    }
	    else if (action == 2) {
		actionString = "ACQUIRED";
	    }
	    else {
		actionString = "UNKNOWN";
	    }

	    if (stackArray == null) {
		return (actionString + ":stack is empty");
	    }

	    String msg = "\n" +
		",thread=" + threadName + 
		",action=" + actionString + 
                ",time=" + this.time + "\n";
	    for (int i=0; i<stackArray.length; i++) {
		msg += "  stack" + i + "=" + stackArray[i].toString() + "\n";
	    }
	    return msg;
	}
	String threadName = null;
	int action = -1;
	StackTraceElement[] stackArray = null;
        long time = -1;

	public static final int LOCK_ACTION = 0;
	public static final int UNLOCK_ACTION = 1;
	public static final int LOCK_ACQUIRED = 2;
    }

    /**
     * global - set of all MVLocks.  we use this so we can see
     * where all the locks are in the stack when we get a deadlock
     */
    static Set<MVLock> lockSet = new HashSet<MVLock>();

    /**
     * time in MS before the deadlock code kicks in.
     */
    public static void setDeadlockTimeout(int timeoutMS) {
	System.err.println("SET DEADLOCK TIMEOUT TO " + timeoutMS);
	DefaultLockTimeoutMS = timeoutMS;
    }
    static int DefaultLockTimeoutMS = 30000;

    private static final long serialVersionUID = 1L;
}
