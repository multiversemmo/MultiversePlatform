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

package multiverse.msgsys;

import java.util.*;
import java.util.concurrent.locks.*;

import multiverse.server.util.*;

/**
 * matches a type and a target session id
 */
public class MessageTypeSessionIdFilter extends MessageTypeFilter {

    public MessageTypeSessionIdFilter() {
	super();
	setupTransient();
    }

    public MessageTypeSessionIdFilter(String targetSessionId) {
	super();
	setupTransient();
	setTargetSessionId(targetSessionId);
    }

    public MessageTypeSessionIdFilter(String targetSessionId,
				      MessageType type) {
	super();
	setupTransient();
	setTargetSessionId(targetSessionId);
	addType(type);
    }

    public MessageTypeSessionIdFilter(MessageType type,
				      String targetSessionId,
				      boolean matchNullFlag) {
	super();
	setupTransient();
	setTargetSessionId(targetSessionId);
	addType(type);
	matchesNullSessionId(matchNullFlag);
    }

    void setupTransient() {
	lock = LockFactory.makeLock("MessageTypeSessionIdFilterLock");
    }

    public String getName() {
	return "MessageTypeSessionIdFilter";
    }

    public void matchesNullSessionId(boolean flag) {
	this.matchesNullSessionId = flag;
    }
    public boolean matchesNullSessionId() {
	return this.matchesNullSessionId;
    }

    public void setTargetSessionId(String s) {
	this.targetSessionId = s;
    }
    public String getTargetSessionId() {
	return this.targetSessionId;
    }

    public boolean matchesRemaining(Message msg) {
        MessageType msgType = msg.getMsgType();
	lock.lock();
	try {
            boolean typeMatched = false;
            for (MessageType t : types) {
                 if (msgType == t) {
                    typeMatched = true;
                    break;
                }
            }
            if (! typeMatched) {
                return false;
            }
//	    if (! types.contains(msgType)) {
//                if (Log.loggingDebug) {
//                    Log.debug("MessageTypeSessionIdFilter: msgType: " + msgType + " didnt match, typesSize=" + types.size());
//                }
//                return false;
//	    }
	}
	finally {
	    lock.unlock();
	}

        String msgTargetSessionId = null;
        if (msg instanceof ITargetSessionId)
            msgTargetSessionId = ((ITargetSessionId)msg).getTargetSessionId();

        // matches type for far
        if (matchesNullSessionId() && (msgTargetSessionId == null)) {
            return true;
        }

	// needs to match session id
	if (msgTargetSessionId == null) {
	    return false;
	}
	return msgTargetSessionId.equals(getTargetSessionId());
    }

    public Set<MessageType> getTypes() {
        return types;
    }

    transient Lock lock = null;
    static final Logger log = new Logger("MessageTypeSessionIdFilter");

    // set of types we match
    Set<MessageType> types = new HashSet<MessageType>();

    String targetSessionId = null;
    boolean matchesNullSessionId = false;

    private static final long serialVersionUID = 1L;
}
