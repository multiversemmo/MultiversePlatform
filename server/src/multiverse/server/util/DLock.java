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
import java.util.concurrent.locks.*;

/**
 * detection or debug lock
 */
public class DLock extends ReentrantLock {
    DLock(String name) {
        super();

        setName(name);
    }

    public String toString() {
        return "[DLock lockName=" + getLockName() + ", hashCode=" + hashCode()
                + "]";
    }

    public void setName(String name) {
        lockName = name;
    }

    public String getLockName() {
        return lockName;
    }

    public void lock() {
        Thread currentThread = Thread.currentThread();

        System.out.println("lock: name=" + lockName + ", thread="
                + currentThread);
        boolean acquiredLock = false;
        rLock.lock();
        try {
            // actually acquire the lock now
            try {
                acquiredLock = super.tryLock(lockTimeoutMS,
                        TimeUnit.MILLISECONDS);
            } catch (Exception e) {
                System.err.println("dlock.lock: got exception: " + e);
                System.exit(-1);
            }
            if (acquiredLock) {
                synchronized (DLock.class) {
                    // update mapping of what locks each thread is holding
                    // eg: thread->lock mapping
                    // this is useful for when we detect a deadlock,
                    // we can see what locks each thread is holding
                    LinkedList<DLock> locks = threadLocks.get(currentThread);
                    if (locks == null) {
                        // this thread has no locks right now
                        System.out
                                .println("lock: lockName="
                                        + lockName
                                        + ", this thread has no locks yet, creating entry.  thread="
                                        + currentThread);
                        locks = new LinkedList<DLock>();
                        threadLocks.put(currentThread, locks);
                    }

                    // add entry to the lock order map
                    // we have to find the entry that maps from the last lock
                    // this thread held. we can add ourselves to that map
                    // updating: static DLock.lockOrderMap
                    DLock lastLock = null;
                    if (!locks.isEmpty()) {
                        lastLock = locks.getLast();
                    }
                    locks.add(this);

                    System.out.println("lock: lockName=" + lockName
                            + ", lastLock=" + lastLock);
                    if ((lastLock != null) && (lastLock != this)) {
                        Map<DLock, StackTraceElement[]> subMap = lockOrderMap
                                .get(lastLock);
                        if (subMap == null) {
                            // create a submap
                            subMap = new HashMap<DLock, StackTraceElement[]>();
                            lockOrderMap.put(lastLock, subMap);
                        }
                        // if there is already an entry for this ordering, dont
                        // generate a new stack trace since that can be pricey
                        if (subMap.get(this) == null) {
                            // generate
                            StackTraceElement[] dump = currentThread
                                    .getStackTrace();
                            subMap.put(this, dump);
                        }
                    }
                }
            }
        } finally {
            rLock.unlock();
        }
        if (!acquiredLock) {
            wLock.lock();
            try {
                throwException("acquire lock failed: forcing cycle detection");
                Log.error("DLock.lock: thread with lock error is: "
                        + currentThread);

                // for each thread we know of
                Log
                        .error("DLock.lock: dumping all threads currently holding locks");
                for (Thread thread : threadLocks.keySet()) {
                    LinkedList<DLock> heldLocks = threadLocks.get(thread);
                    for (DLock heldLock : heldLocks) {
                        Log.error("DLock.lock: thread " + thread
                                + ", is holding lock " + heldLock);
                        if (heldLock == this) {
                            Log
                                    .error("DLock.lock: thread has lock in question, dumping stack trace:\n"
                                            + makeStackDumpString(thread
                                                    .getStackTrace()));
                        }
                    }
                }
                Log
                        .error("DLock.lock: DONE dumping all threads currently held locks");
                Log.error("DLock.lock: detecting cycles for lock in question: "
                        + this);
                detectCycleHelper(this, new LinkedList<DLock>(),
                        new HashSet<DLock>());
                Log.error("DLock.lock: detecting all other cycles");
                detectCycle();
                Log.error("deadlock cycle detection complete, exiting process");
                System.exit(-1);
            } finally {
                wLock.unlock();
            }
        }
    }

    public void unlock() {
        wLock.lock();
        try {
            synchronized (DLock.class) {
                Thread currentThread = Thread.currentThread();

                // remove this lock from the thread's list of locks
                LinkedList<DLock> locks = threadLocks.get(currentThread);
                if (locks == null) {
                    // this thread has no locks - something is wrong
                    throwException("unlock, but thread has no previous locks");
                }
                DLock lastLock = locks.getLast();
                if (lastLock == null) {
                    // this thread has no locks
                    throwException("unlock, but thread has no previous locks");
                }
                if (lastLock == this) {
                    locks.removeLast();
                } else {
                    throwException("unlock, last lock did not match this lock");
                }
            }
            super.unlock();
        } finally {
            wLock.unlock();
        }
    }

    void throwException(String error) {
        wLock.lock();
        try {
            Log.error("DLock.throwException: lock in question is: "
                    + this + ", thread=" + Thread.currentThread().getName()
                    + ", msg=" + error);
            Log.dumpStack();
        } finally {
            wLock.unlock();
        }
    }

    /**
     * goes through the lock ordering map and looks for any cycles returns a set
     * of cycles
     */
    public static Set<List<DLock>> detectCycle() {
        synchronized (DLock.class) {
            Log.error("DLock.detectCycles: finding all child nodes");

            // find all child nodes
            Set<DLock> childNodes = getChildNodes();
            Log.error("DLock.detectCycle: found " + childNodes.size()
                    + " child nodes");

            // find all top nodes
            Set<DLock> topNodes = getTopNodes(childNodes);
            Log.error("DLock.detectCycle: found " + topNodes.size()
                    + " top nodes");

            // for each top node, do a depth first traversal
            HashSet<DLock> cycleNodes = new HashSet<DLock>();
            Set<List<DLock>> allCycles = new HashSet<List<DLock>>();
            for (DLock node : topNodes) {
                Log.error("DLock.detectCycle: doing depth first for top node "
                        + node);
                List<DLock> history = new LinkedList<DLock>();
                allCycles.addAll(detectCycleHelper(node, history, cycleNodes));
            }

            // now for each non-top-node, check for cycles.
            // for these, delete them if there is already
            // a top-node-cycle containing the full sub-cycle
            Log
                    .error("DLock.detectCycle: doing depth first for all child nodes");
            for (DLock node : childNodes) {
                Log
                        .error("DLock.detectCycle: doing depth first for child node "
                                + node);
                List<DLock> history = new LinkedList<DLock>();
                Set<List<DLock>> cycles = detectCycleHelper(node, history,
                        cycleNodes);

                // prune the cycles if it is already in the top node cycle
                for (List<DLock> cycle : cycles) {
                    if (!isSubset(cycle, allCycles)) {
                        allCycles.add(cycle);
                    }
                }
            }

            Log.error("DLock.detectCycle: printing out all detected cycles");
            for (List<DLock> cycle : allCycles) {
                printCycle(cycle);
            }
            Log.error("DLock.detectCycle: done with printout");
            return allCycles;
        }
    }

    protected static boolean isSubset(List<DLock> set, Set<List<DLock>> allSets) {
        for (List<DLock> testSet : allSets) {
            if (isSubset(set, testSet)) {
                return true;
            }
        }
        return false;
    }

    protected static boolean isSubset(List<DLock> subset, List<DLock> superset) {
        for (DLock element : subset) {
            if (!superset.contains(element)) {
                return false;
            }
        }
        return true;
    }

    // helper method, returns all child nodes (nodes that have parents)
    protected static Set<DLock> getChildNodes() {
        Set<DLock> childNodes = new HashSet<DLock>();
        for (Map<DLock, StackTraceElement[]> subMap : lockOrderMap.values()) {
            childNodes.addAll(subMap.keySet());
        }
        return childNodes;
    }

    // helper method, looks in lockOrderMap and returns all top nodes
    // assumes we have a lock
    // pass in a set of all childNodes
    protected static Set<DLock> getTopNodes(Set<DLock> childNodes) {
        Set<DLock> topNodes = new HashSet<DLock>();
        for (DLock node : lockOrderMap.keySet()) {
            if (!childNodes.contains(node)) {
                topNodes.add(node);
            }
        }
        return topNodes;
    }

    // helper method, assumes we have already locked the DLock class
    // traverses the tree, by taking 'node' and going depth first
    // for its children.
    // history is the 'stack' of nodes youre descending (path)
    // cycleNodes are the nodes that cause cycles - used to short circuit
    // returns a set of cycles
    // keeps track of the traversed nodes in 'history'
    protected static Set<List<DLock>> detectCycleHelper(DLock node,
            List<DLock> history, HashSet<DLock> cycleNodes) {
        if (Log.loggingDebug)
            Log.debug("DLock.detectCyleHelper: considering node " + node
                      + ", numCycleNodes=" + cycleNodes.size());

        // is the node we're at in the history already
        if (history.contains(node)) {
            if (Log.loggingDebug)
                Log.debug("DLock.detectCycleHelper: node " + node
                          + " already in history, cycle detected");
            history.add(node);
            // stackDump(history);
            Set<List<DLock>> cycle = new HashSet<List<DLock>>();
            cycle.add(history);
            cycleNodes.add(node);
            return cycle;
        }

        // no cycle, add this current node to the node's we've visited
        history.add(node);

        // find the children nodes of current node
        Map<DLock, StackTraceElement[]> subMap = lockOrderMap.get(node);
        if (subMap == null) {
            // we're at a leaf node, no cycles
            Log.debug("DLock.detectCycleHelper: node is leaf, returning");
            return new HashSet<List<DLock>>();
        }
        Set<DLock> childSet = subMap.keySet();
        if ((childSet == null) || (childSet.isEmpty())) {
            Log.debug("DLock.detectCycleHelper: node is leaf, returning");
            return new HashSet<List<DLock>>();
        }

        // descend into each child
        Set<List<DLock>> cycles = new HashSet<List<DLock>>();
        for (DLock child : childSet) {
            if (cycleNodes.contains(child)) {
                Log.debug("DLock.detectCycleHelper: child already causes cycle, skipping - currentNode="
                          + node + ", child=" + child);
                continue;
            }
            if (Log.loggingDebug)
                Log.debug("DLock.detectCycleHelper: currentNode=" + node
                          + ", decending, child=" + child + ", numChildren="
                          + childSet.size());
            // copy the history since its only valid going down
            List<DLock> historyCopy = new LinkedList<DLock>(history);
            Set<List<DLock>> childCycles = detectCycleHelper(child,
                    historyCopy, cycleNodes);
            cycles.addAll(childCycles);
        }
        return cycles;
    }

    /**
     * given the sequence of nodes which led to a cycle, this method will print
     * out the stack dumps for each lock sequence assumes we have synchronized
     * the DLock.class object
     */
    protected static void stackDump(List<DLock> nodes) {
        if (nodes == null) {
            System.err.println("ERROR: DLock.stackDump: nodes is null");
            return;
        }
        if (nodes.isEmpty()) {
            System.err.println("ERROR: DLock.stackDump: nodes list is empty");
            return;
        }

        System.err.println("Found Cycle, dumping history");
        Iterator<DLock> iter = nodes.iterator();
        DLock prev = null;
        DLock cur = null;
        for (prev = iter.next(); iter.hasNext(); prev = cur) {
            cur = iter.next();

            // turn the stacktrace dump into a string
            StackTraceElement[] stackArray = lockOrderMap.get(prev).get(cur);
            String stackTrace = "";
            for (int i = 0; i < stackArray.length; i++) {
                stackTrace += "  stack" + i + "=" + stackArray[i].toString()
                        + "\n";
            }

            // print it out
            System.err.println("ERROR: DLock.stackDump: had lock "
                    + prev.getLockName() + " when locked " + cur.getLockName()
                    + ", dump:\n" + stackTrace);
        }
    }

    // assumes we have the lock
    protected static void printCycle(List<DLock> cycle) {
        String s = "Cycle: ";
        DLock prev = null;
        for (DLock node : cycle) {
            s += "\nlock=" + node;

            // find the stack dump
            if (prev != null) {
                s += ", stackdump:\n";
                Map<DLock, StackTraceElement[]> subMap = lockOrderMap.get(prev);
                StackTraceElement[] trace = subMap.get(node);

                // print out the stack trace
                for (int i = 0; i < trace.length; i++) {
                    s += "  stack" + i + "=" + trace[i].toString() + "\n";
                }
            }

            prev = node;
        }
        Log.error("DLock.printCycle: " + s);
    }

    /**
     * turn the stacktrace dump into a string
     */
    public static String makeStackDumpString(StackTraceElement[] stackArray) {
        String stackTrace = "";
        for (int i = 0; i < stackArray.length; i++) {
            stackTrace += "  stack" + i + "=" + stackArray[i].toString() + "\n";
        }
        return stackTrace;
    }

    String threadName = null;

    String lockName = "unknown";

    // we keep track of the lock order so we can detect cycles in locking
    // lock we last acquired -> lock we just got -> stack trace
    static Map<DLock, Map<DLock, StackTraceElement[]>> lockOrderMap = new HashMap<DLock, Map<DLock, StackTraceElement[]>>();

    // we keep track of each thread's current locks (in order) so when
    // the thread does a new lock, we can store the entry of the current
    // lock its holding into the lockOrderMap
    static Map<Thread, LinkedList<DLock>> threadLocks = new HashMap<Thread, LinkedList<DLock>>();

    static int lockTimeoutMS = 30000;

    // the reentrant read write locks arent really used for reading writing.
    // they seperate the two classes of locks, the readlock is held while any
    // lock
    // is trying to lock. the writelock is held only when a thread has
    // discovered a deadlock
    // and is printing out information. we therefore need all readers to finish
    // updating
    // their info so when we print out the deadlock info, it is in a consistent
    // state
    static ReentrantReadWriteLock rwLock = new ReentrantReadWriteLock();

    static ReentrantReadWriteLock.ReadLock rLock = rwLock.readLock();

    static ReentrantReadWriteLock.WriteLock wLock = rwLock.writeLock();

    public static void main(String args[]) {
        DLock a = new DLock("A");
        DLock b = new DLock("B");
        a.lock();
        b.lock();
        a.lock();

        a.unlock();
        b.unlock();
        a.unlock();
        System.out.println("detecting cycles..");
        DLock.detectCycle();
        System.out.println("done");
    }

    private static final long serialVersionUID = 1L;
}
