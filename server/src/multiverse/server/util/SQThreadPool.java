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

public class SQThreadPool implements Runnable {
    static final int MIN_THREADS = 8;
    static final int MAX_THREADS = 40;
    static final int MIN_RUN_TIME = 15000;  // 5 seconds

    public SQThreadPool(SquareQueue sq, SQCallback callback) {
	this.sq = sq;
	this.callback = callback;

	total = MIN_THREADS;
	for (int ii = 0; ii < total; ii++)  {
	    new Thread(this,"SQ-"+sq.getName()+"-"+threadId).start();
            threadId++;
	}
    }

    protected static ThreadLocal<SQThreadPool> selfPool = new ThreadLocal<SQThreadPool>();

    class ThreadStatus {
	ThreadStatus(int status) {
	    this.status = status;
        }
	public int status;
    }
    protected static ThreadLocal<ThreadStatus> threadStatus = new ThreadLocal<ThreadStatus>();
    static final int STATUS_NORMAL = 0;
    static final int STATUS_BLOCKED = 1;

    public static SQThreadPool getRunningPool() {
	return selfPool.get();
    }

    public synchronized void runningThreadWillBlock() {
	ThreadStatus myStatus = threadStatus.get();
	if (myStatus == null) {
	    throw new RuntimeException("Not an SQ thread");
	}
	if (myStatus.status == STATUS_BLOCKED) {
	    throw new RuntimeException("Nested blocking for SQ thread");
	}

	blocking++;
	myStatus.status = STATUS_BLOCKED;
	if (Log.loggingDebug)
	    Log.debug("SQ-"+sq.getName()+": runningThreadWillBlock: "+blocking+"/"+running+"/"+total);
	if (blocking == total && total < MAX_THREADS) {
	    total++;
            if (Log.loggingDebug)
		Log.debug("SQ-"+sq.getName()+": Starting new thread");
	    new Thread(this,"SQ-"+sq.getName()+"-"+threadId).start();
            threadId++;
	}
    }
    public synchronized void doneBlocking() {
	ThreadStatus myStatus = threadStatus.get();
	if (myStatus == null) {
	    throw new RuntimeException("Not an SQ thread");
	}
	if (myStatus.status != STATUS_BLOCKED) {
	    throw new RuntimeException("Nested blocking for SQ thread");
	}

	blocking--;
	myStatus.status = STATUS_NORMAL;
    }

    private boolean retiring() {
	// Conditions for retirement:
	// - At least MIN_THREADS in the thread pool
	// - At least 2 (including self) unblocked threads
	// - Less than MIN_THREADS queues in the SQ
	if (total > MIN_THREADS && (total - blocking > 1) &&
		    sq.getSQSize() < MIN_THREADS)
	    return true;
	else
	    return false;
    }

    public void run() {
        if (Log.loggingInfo)
            Log.info("SQ-"+sq.getName()+": Started new thread");
        String title = "SQThreadPool " + sq.getName();
	selfPool.set(this);
	threadStatus.set(new ThreadStatus(STATUS_NORMAL));
        SquareQueue.SubQueue pq = null;
	long startTime = System.currentTimeMillis();

        while (true) {
            try {
//                 if (Log.loggingDebug)
//                     Log.debug(title + ": about to remove");
                pq = sq.remove();
                try {
//                    Long oid = (Long)pq.getKey();
//                     String fullTitle = title + "(oid " + oid + ")";
//                     Log.debug(fullTitle + ": removed; about to call pq.next");
                    if (pq.next()) {
//                         if (Log.loggingDebug)
//                             Log.debug(fullTitle + ": got pq.next; sq.size() is " + sq.getSQSize() + ", head value is " + pq.getHeadValue());
			synchronized (this) {
			    running++;
			}
                        callback.doWork(pq.getHeadValue(),pq.getKey());
//                         if (Log.loggingDebug)
//                             Log.debug(fullTitle + ": finished callback");
                    }
//                     if (Log.loggingDebug)
//                         Log.debug(fullTitle + ": requeuing");
                }
                finally {
		    long runTime = (System.currentTimeMillis() - startTime);
		    boolean retire = false;
		    synchronized (this) {
			running--;
			if (runTime > MIN_RUN_TIME && retiring())
			    retire = true;
		    }
                    sq.requeue(pq);
		    if (retire)
			break;
                }
            }
            catch (Exception e) {
                Log.exception(title, e);
		// Clean up our blocking status (in case we got an exception
		// while in the blocked state)
		ThreadStatus myStatus = threadStatus.get();
		if (myStatus.status == STATUS_BLOCKED) {
		    doneBlocking();
		}
            }
        }

	synchronized (this) {
	    total--;
	    if (Log.loggingInfo)
		Log.info("SQ-"+sq.getName()+": Retiring thread: "+
			blocking+"/"+running+"/"+total);
	}
    }

    public SquareQueue getSquareQueue() {
	return sq;
    }

    protected SquareQueue sq;
    protected SQCallback callback;

    protected int total = 0;
    protected int running = 0;
    protected int blocking = 0;
    protected int threadId = 1;


    // Testing code below

    static class TestSQCallback<K,V> implements SQCallback<K,V> {
	public void doWork(K key, V value) {
	    System.out.println("CALLBACK key="+key+" value="+value);
	    SQThreadPool localPool = SQThreadPool.getRunningPool();
	    System.out.println("CALLBACK localPool "+localPool);
	}
    }

    public static void main(String args[]) {
	SquareQueue<Long,String> sq = new SquareQueue<Long,String>("test");

	sq.insert(1L,"goober1");
	sq.insert(2L,"goober2");
	sq.insert(1L,"goober3");

        TestSQCallback<Long,String> callback = new TestSQCallback<Long,String>();
	/* SQThreadPool pool = */ new SQThreadPool(sq, callback);

	SQThreadPool localPool = SQThreadPool.getRunningPool();
        System.out.println("localPool "+localPool);

	Object o= new Object();
	synchronized (o) {
	try { o.wait(); } catch (InterruptedException ex)  { }
	}
    }

}

