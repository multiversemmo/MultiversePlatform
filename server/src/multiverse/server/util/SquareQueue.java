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

import java.util.HashMap;
import java.util.LinkedList;
import java.util.Iterator;
import java.util.List;


public class SquareQueue<K,V> {

    public SquareQueue(String name) {
        this.name = name;
    }

    public synchronized void insert(K key, V value) {
	SubQueue subQueue = subQueues.get(key);
	if (subQueue == null) {
// 	    if (Log.loggingDebug)
//                 Log.debug("SquareQueue.insert " + name + ": creating new queue for key " + key);
            subQueue = newSubQueue(key);
	    subQueue.queue.add(value);
	    queue.add(subQueue);
	    notify();
	}
	else {
	    subQueue.queue.add(value);
            if (subQueue.unqueued) {
                subQueue.unqueued = false;
		queue.add(subQueue);
		notify();
	    }
	}
    }

    public synchronized void insert(List<K> keys, V value) {
        for (K key : keys) {
            insert(key,value);
        }
    }

    public synchronized SubQueue remove()
    		throws InterruptedException {
	while (queue.size() == 0) {
	    wait();
	}
	return queue.poll();
    }

    public synchronized void requeue(SubQueue subQueue) {
        // Remove the entry that the fellow who did getNext got.
//         if (Log.loggingDebug)
//             Log.debug("SquareQueue.requeue " + name + ": setting headValue to null for subQueue " + subQueue.getKey());
        subQueue.headValue = null;
	if (subQueue.size() > 0) {
            subQueue.unqueued = false;
	    queue.add(subQueue);
	    notify();
	}
        else {
            subQueue.unqueued = true;
        }
    }

    public synchronized void removeKey(K key) {
//         if (Log.loggingDebug)
//             Log.debug("SquareQueue.removeKey " + name + ": removing queue for key " + key);
        SubQueue subQueue = subQueues.remove(key);
	if (subQueue == null)
	    return;
	subQueue.queue.clear();
        Iterator<SubQueue> iterator = queue.iterator();
        while (iterator.hasNext())  {
            SubQueue pq = iterator.next();
            if (key.equals(pq.getKey())) {
                iterator.remove();
                break;
            }
        }
    }

    // Used only for tracing
    public synchronized int getSQSize() {
        return queue.size();
    }
    
    public String getName() {
        return name;
    }
    
    public class SubQueue {
	SubQueue(K key) {
	    this.key = key;
	}

	public boolean next() {
	    return getNext(this);
	}

	public K getKey() {
	    return key;
	}

	public V getHeadValue() {
	    return headValue;
 	}

	int size() {
	    return getSubQueueSize(this);
	}

	K key;
	LinkedList<V> queue = new LinkedList<V>();
	V headValue;
        boolean unqueued = false;
    }

    protected synchronized boolean getNext(SubQueue subQueue) {
	V headValue = subQueue.queue.poll();
// 	if (Log.loggingDebug)
//             Log.debug("SquareQueue.getNext " + name + ": for subQueue " + subQueue.getKey() + ", setting headValue to " + headValue);
        if (headValue == null)
	    return false;
	subQueue.headValue = headValue;
	return true;
    }

    protected synchronized int getSubQueueSize(SubQueue subQueue) {
	return subQueue.queue.size();
    }

    protected SubQueue newSubQueue(K key) {
	SubQueue subQueue = new SubQueue(key);
	subQueues.put(key,subQueue);
	return subQueue;
    }

    protected HashMap<K,SubQueue> subQueues = new HashMap<K,SubQueue>();
    protected LinkedList<SubQueue> queue = new LinkedList<SubQueue>();
    protected String name;

    public static void main(String args[]) {
	SquareQueue<Long,String> sq = new SquareQueue<Long,String>("main");

	sq.insert(1L,"goober1");
	sq.insert(2L,"goober2");
	sq.insert(1L,"goober3");

	SquareQueue<Long,String>.SubQueue subQueue;

	try {

	subQueue = sq.remove();
	System.out.println("GOT key "+subQueue.getKey());
	if (subQueue.next()) {
	    System.out.println("HEAD "+subQueue.getHeadValue());
	}
	sq.requeue(subQueue);

	subQueue = sq.remove();
	System.out.println("GOT key "+subQueue.getKey());
	if (subQueue.next()) {
	    System.out.println("HEAD "+subQueue.getHeadValue());
	}
	sq.requeue(subQueue);


	subQueue = sq.remove();
	System.out.println("GOT key "+subQueue.getKey());
	if (subQueue.next()) {
	    System.out.println("HEAD "+subQueue.getHeadValue());
	}
	sq.requeue(subQueue);

	subQueue = sq.remove();
	} catch (InterruptedException ex) { }

    }
}

