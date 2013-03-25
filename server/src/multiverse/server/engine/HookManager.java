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

package multiverse.server.engine;

import java.util.*;
import java.util.concurrent.locks.*;
import multiverse.msgsys.*;
import multiverse.server.util.*;

/**
 * Manages hooks for processing messages coming in from a subscription.
 * Hooks are associated with message types, which is a property in a
 * message.
 * <p>
 * The EnginePlugin's onMessage() callback processes an incoming
 * message by calling into its local HookManager and finding all matching hooks
 * for the incoming message's message type
 * for all the hooks.  For each matching hook, it calls the hook's 
 * processMessage() method.
 *
 * @see EnginePlugin#handleMessageImpl
 */
public class HookManager {
    /**
     * Normally created by the EnginePlugin class.
     * 
     * In order to avoid copying the list when getHooks is called, we
     * copy it when we're adding to it.  We do lock around addHook, so
     * that different callers to addHook are synchronized against each
     * other, but we don't lock in getHooks.
     */
    public HookManager() {
    }

    /**
     * Adds a hook to the HookManager.  You can associate more than
     * one hook with a given message type which will be returned
     * in order by getHooks().
     *
     * @param msgType the message type to match
     * @param hook the hook to be called for matching messages
     * @see EnginePlugin#handleMessageImpl
     */
    public void addHook(MessageType msgType, Hook hook) {
	lock.lock();
	try {
	    List<Hook> hookList = hooks.get(msgType);
	    if (hookList == null) {
		hookList = new LinkedList<Hook>();
		hookList.add(hook);
                hooks.put(msgType, hookList);
	    }
            else {
                hookList = new LinkedList<Hook>(hookList);
                hookList.add(hook);
                hooks.put(msgType, hookList);
            }
	}
	finally {
	    lock.unlock();
	}
    }

    /**
     * Returns this list of all hooks matching the message type.  They
     * are returned in the order they were added.  The returned list
     * should be treated as read-only.
     *
     * @param msgType the message type to match
     * @return A list of all hooks matching the passed in message type.
     */
    public List<Hook> getHooks(MessageType msgType) {
        List<Hook> hookList = hooks.get(msgType);
        if (hookList == null)
            return nullList;
        else 
            return hookList;
    }

    private LinkedList<Hook> nullList = new LinkedList<Hook>();
    private Lock lock = LockFactory.makeLock("HookManager");
    private Map<MessageType, List<Hook>> hooks = new HashMap<MessageType, List<Hook>>();
}
