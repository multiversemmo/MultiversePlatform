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

import java.io.IOException;
import java.io.ObjectInputStream;
import java.util.concurrent.locks.Lock;

import multiverse.server.objects.*;
import multiverse.server.util.*;
import multiverse.msgsys.*;

public abstract class Behavior implements MessageCallback, MessageDispatch,
    java.io.Serializable
{
    public Behavior() {
        super();
        setupTransient();
    }

    public Behavior(SpawnData data) {
	super();
	setupTransient();
    }
    
    private void setupTransient() {
        lock = LockFactory.makeLock("BehavLock");
    }
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }
    
    public ObjectStub getObjectStub() {
	return obj;
    }
    public void setObjectStub(ObjectStub obj) {
	this.obj = obj;
    }
    protected ObjectStub obj;

    public void initialize() {
    }

    public abstract void activate();
    public abstract void deactivate();
    public abstract void handleMessage(Message msg, int flags);

    public void dispatchMessage(Message message, int flags,
        MessageCallback callback)
    {
        Engine.defaultDispatchMessage(message, flags, callback);
    }

    public static class CommandMessage extends SubjectMessage {
        public CommandMessage() {
            super();
        }
        
        public CommandMessage(ObjectStub obj) {
	    super(MSG_TYPE_COMMAND, obj.getOid());
	}

        public CommandMessage(String cmd) {
	    super();
	    setMsgType(MSG_TYPE_COMMAND);
            this.cmd = cmd;
	}

        public CommandMessage(ObjectStub obj, String cmd) {
	    super(MSG_TYPE_COMMAND, obj.getOid());
            this.cmd = cmd;
	}

        public void setCmd(String cmd) {
            this.cmd = cmd;
        }
        
        public String getCmd() {
            return cmd;
        }
        
        private String cmd;
        
        private static final long serialVersionUID = 1L;
    }

    public static class EventMessage extends SubjectMessage {

        public EventMessage() {
            super();
            setMsgType(MSG_TYPE_EVENT);
        }
        
	EventMessage(ObjectStub obj) {
	    super(MSG_TYPE_EVENT, obj.getOid());
	}

        public void setEvent(String event) {
            this.event = event;
        }

        public String getEvent() {
            return event;
        }

        private String event;
        
        private static final long serialVersionUID = 1L;
    }

    transient protected Lock lock = null;
    
    public static MessageType MSG_TYPE_COMMAND = MessageType.intern("mv.COMMAND");
    public static MessageType MSG_TYPE_EVENT = MessageType.intern("mv.EVENT");
}
