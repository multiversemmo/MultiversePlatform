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


package multiverse.example;

import java.util.ArrayList;
import multiverse.msgsys.*;
import multiverse.server.messages.*;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.engine.Engine;

/** Simple perception messaging example.  Monitor the location of
objects perceived by some set of objects.  Contains methods to
dynamically change the set of perceiving objects.
*/
public class PerceptionExample implements MessageCallback
{
    public PerceptionExample()
    {
        perceptionFilter = new PerceptionFilter();
        perceptionFilter.addType(WorldManagerClient.MSG_TYPE_PERCEPTION);
        perceptionFilter.addType(WorldManagerClient.MSG_TYPE_UPDATEWNODE);
        perceptionFilter.setMatchAllSubjects(true);
        PerceptionTrigger perceptionTrigger = new PerceptionTrigger();
        ArrayList<MessageType> triggerTypes = new ArrayList<MessageType>();
        triggerTypes.add(WorldManagerClient.MSG_TYPE_PERCEPTION);
        perceptionTrigger.setTriggeringTypes(triggerTypes);
        perceptionSubId = Engine.getAgent().createSubscription(
            perceptionFilter, this, MessageAgent.NO_FLAGS, perceptionTrigger);
    }

    public synchronized void addObjectMonitoring(long oid) {
        if (perceptionFilter.addTarget(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate(1);
            filterUpdate.addFieldValue(PerceptionFilter.FIELD_TARGETS,
                    new Long(oid));
            Engine.getAgent().applyFilterUpdate(perceptionSubId,
                    filterUpdate);
        }
    }

    public synchronized void removeObjectMonitoring(long oid) {
        if (perceptionFilter.removeTarget(oid)) {
            FilterUpdate filterUpdate = new FilterUpdate(1);
            filterUpdate.removeFieldValue(PerceptionFilter.FIELD_TARGETS,
                    new Long(oid));
            Engine.getAgent().applyFilterUpdate(perceptionSubId,
                    filterUpdate);
        }
    }

    public void handleMessage(Message msg, int flags) {
        if (msg.getMsgType() == WorldManagerClient.MSG_TYPE_PERCEPTION) {
            handlePerception((PerceptionMessage)msg);
        }
        else if (msg.getMsgType() == WorldManagerClient.MSG_TYPE_UPDATEWNODE) {
            handleUpdateWorldNode((WorldManagerClient.UpdateWorldNodeMessage)msg);
        }
    }
  
    void handlePerception(PerceptionMessage message)
    {
    }
    
    void handleUpdateWorldNode(
        WorldManagerClient.UpdateWorldNodeMessage message)
    {
    }
    
    PerceptionFilter perceptionFilter;
    PerceptionTrigger perceptionTrigger;
    long perceptionSubId;
}

