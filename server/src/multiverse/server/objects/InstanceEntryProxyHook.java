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

package multiverse.server.objects;

import java.util.Map;
import java.io.Serializable;

import multiverse.server.util.Log;
import multiverse.server.plugins.*;
import multiverse.server.plugins.InstanceClient.InstanceEntryReqMessage;
import multiverse.server.events.ExtensionMessageEvent;
import multiverse.server.math.*;
import multiverse.server.engine.BasicWorldNode;


public class InstanceEntryProxyHook implements ProxyExtensionHook
{
    public void processExtensionEvent(ExtensionMessageEvent event,
        Player player, ProxyPlugin proxy)
    {
        Map<String,Serializable> props = event.getPropertyMap();
        int flags = 0;
        Long instanceOid = null;

        if (Log.loggingDebug) {
            String propStr = "";
            for (Map.Entry<String,Serializable> entry : props.entrySet()) {
                propStr += entry.getKey() + "=" + entry.getValue() + " ";
            }
            Log.debug("processInstanceEntryEvent: "+propStr);
        }

        String flagStr = (String) props.get("flags");
        if (flagStr != null) {
            if (flagStr.equals("push"))
                flags |= InstanceEntryReqMessage.FLAG_PUSH;    
            else if (flagStr.equals("pop"))
                flags |= InstanceEntryReqMessage.FLAG_POP;    
        }

        if ((flags & InstanceEntryReqMessage.FLAG_POP) != 0) {
            // Pop player's instance restore stack
            InstanceClient.objectInstanceEntry(player.getOid(),
                null, flags);
            return;
        }

        String instanceName = (String) props.get("instanceName");
        if (instanceName == null) {
            instanceOid = (Long) props.get("instanceOid");
            if (instanceOid == null) {
                Log.error("Instance entry event: missing instanceName and instanceOid");
                return;
            }
        }
        else {
            instanceOid = InstanceClient.getInstanceOid(instanceName);
            if (instanceOid == null) {
                Log.error("Instance entry event: unknown instanceName=" +
                    instanceName);
                return;
            }
        }

        Marker marker = null;
        String markerName = (String) props.get("locMarker");
        if (markerName == null) {
            marker = new Marker();
            marker.setPoint((Point) props.get("locPoint"));
            if (marker.getPoint() == null) {
                Log.error("Instance entry event: missing locMarker and locPoint");
                return;
            }
        }
        else {
            marker = InstanceClient.getMarker(instanceOid, markerName);
            if (marker == null) {
                Log.error("Instance entry event: unknown marker=" +
                    markerName);
                return;
            }
        }

        Quaternion orient = (Quaternion) props.get("orientation");
        if (orient != null && marker != null)
            marker.setOrientation(orient);

        Marker restoreMarker = null;
        Long currentInstanceOid = null;
        if ((flags & InstanceEntryReqMessage.FLAG_PUSH) != 0) {
            String restoreMarkerName = (String) props.get("restoreMarker");
            if (restoreMarkerName == null) {
                restoreMarker = new Marker();
                restoreMarker.setPoint((Point) props.get("restorePoint"));
                if (restoreMarker.getPoint() == null)
                    restoreMarker = null;
                else {
                    BasicWorldNode currentLoc =
                        WorldManagerClient.getWorldNode(player.getOid());
                    currentInstanceOid = currentLoc.getInstanceOid();
                }
            }
            else {
                BasicWorldNode currentLoc =
                    WorldManagerClient.getWorldNode(player.getOid());
                currentInstanceOid = currentLoc.getInstanceOid();
                restoreMarker = InstanceClient.getMarker(currentInstanceOid,
                    restoreMarkerName);
                if (restoreMarker == null) {
                    Log.error("Instance entry event: unknown restore marker=" +
                        restoreMarkerName);
                    return;
                }
            }
        }
        else {
            if (props.get("restoreMarker") != null ||
                    props.get("restorePoint") != null)
                Log.warn("processInstanceEntryEvent: ignoring restore marker because flag push is not set");
        }

        Quaternion restoreOrient = (Quaternion) props.get("restoreOrientation");
        if (restoreOrient != null && restoreMarker != null)
            restoreMarker.setOrientation(restoreOrient);

        BasicWorldNode wnode = null;
        if (marker != null) {
            wnode = new BasicWorldNode();
            wnode.setInstanceOid(instanceOid);
            wnode.setLoc(marker.getPoint());
            wnode.setDir(new MVVector(0,0,0));
            if (marker.getOrientation() != null)
                wnode.setOrientation(marker.getOrientation());
        }

        BasicWorldNode restoreWnode = null;
        if (restoreMarker != null) {
            restoreWnode = new BasicWorldNode();
            restoreWnode.setInstanceOid(currentInstanceOid);
            restoreWnode.setLoc(restoreMarker.getPoint());
            restoreWnode.setDir(new MVVector(0,0,0));
            if (restoreMarker.getOrientation() != null)
                restoreWnode.setOrientation(restoreMarker.getOrientation());
        }

        InstanceClient.objectInstanceEntry(player.getOid(),
            wnode, flags, restoreWnode);
    }

}
