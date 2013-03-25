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

package multiverse.mars.behaviors;

import java.util.*;

import multiverse.msgsys.*;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.engine.Behavior;
import multiverse.server.engine.Engine;

public class ChatResponseBehavior extends Behavior implements MessageCallback {
    public void initialize() {
	MessageTypeFilter filter = new MessageTypeFilter();
	filter.addType(WorldManagerClient.MSG_TYPE_COM);
	eventSub = Engine.getAgent().createSubscription(filter, this);
    }
    
    public void activate() {
    }

    public void deactivate() {
	if (eventSub != null) {
	    Engine.getAgent().removeSubscription(eventSub);
	    eventSub = null;
	}
        //return true;
    }

    public void handleMessage(Message msg, int flags) {
        String response = null;
        if (msg instanceof WorldManagerClient.ComMessage) {
            WorldManagerClient.ComMessage comMsg = (WorldManagerClient.ComMessage)msg;
            response = responses.get(comMsg.getString());

        }
        else if (msg instanceof WorldManagerClient.TargetedComMessage) {
            WorldManagerClient.TargetedComMessage comMsg = (WorldManagerClient.TargetedComMessage)msg;
            response = responses.get(comMsg.getString());
        }
        if (response != null) {
            WorldManagerClient.sendChatMsg(obj.getOid(), 1, response);
        }
    }
        
    public void addChatResponse(String trigger, String response) {
	responses.put(trigger, response);
    }

    Map<String, String> responses = new HashMap<String, String>();
    Long eventSub = null;

    private static final long serialVersionUID = 1L;
}
