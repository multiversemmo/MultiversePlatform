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

package multiverse.mars.objects;

import java.util.Map;
import java.io.Serializable;

import multiverse.server.objects.ProxyExtensionHook;
import multiverse.server.objects.Template;
import multiverse.server.objects.Player;
import multiverse.server.objects.Marker;
import multiverse.server.events.ExtensionMessageEvent;
import multiverse.server.plugins.ProxyPlugin;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.server.plugins.InstanceClient;
import multiverse.server.util.Log;
import multiverse.server.engine.BasicWorldNode;
import multiverse.server.engine.Namespace;
import multiverse.server.math.MVVector;


public class GenerateObjectProxyHook implements ProxyExtensionHook
{
    
    public void processExtensionEvent(ExtensionMessageEvent event,
        Player player, ProxyPlugin proxy)
    {
        Map<String,Serializable> props = event.getPropertyMap();

        if (Log.loggingDebug) {
            String propStr = "";
            for (Map.Entry<String,Serializable> entry : props.entrySet()) {
                propStr += entry.getKey() + "=" + entry.getValue() + " ";
            }
            Log.debug("GenerateObjectProxyHook: " + player + " " + propStr);
        }

        String templateName = (String) props.get("template");

        BasicWorldNode objectLoc;
        BasicWorldNode playerLoc =
            WorldManagerClient.getWorldNode(player.getOid());

        String markerName = (String) props.get("marker");
        if (markerName != null) {
            Marker marker = InstanceClient.getMarker(
                playerLoc.getInstanceOid(), markerName);
            if (marker == null) {
                Log.error("GenerateObjectProxyHook: unknown marker="+markerName);
                return;
            }
            objectLoc = new BasicWorldNode();
            objectLoc.setInstanceOid(playerLoc.getInstanceOid());
            objectLoc.setLoc(marker.getPoint());
            objectLoc.setOrientation(marker.getOrientation());
            objectLoc.setDir(new MVVector(0,0,0));
        }
        else {
            objectLoc = playerLoc;
            objectLoc.setDir(new MVVector(0,0,0));
        }

        boolean persistent = false;
        if (props.get("persistent") != null) {
            Integer persInt = (Integer) props.get("persistent");
            if (persInt != 0)
                persistent = true;
        }

        Template override = new Template();
        override.put(WorldManagerClient.NAMESPACE,
            WorldManagerClient.TEMPL_INSTANCE, objectLoc.getInstanceOid());
        override.put(WorldManagerClient.NAMESPACE,
            WorldManagerClient.TEMPL_LOC, objectLoc.getLoc());
        override.put(WorldManagerClient.NAMESPACE, 
            WorldManagerClient.TEMPL_ORIENT, objectLoc.getOrientation());
        if (persistent)
            override.put(Namespace.OBJECT_MANAGER, 
                ObjectManagerClient.TEMPL_PERSISTENT, persistent);

        Long oid = ObjectManagerClient.generateObject(templateName,override);
        if (oid == null) {
            Log.error("GenerateObjectProxyHook: generateObject failed templateName="+templateName);
            return ;
        }

        if (Log.loggingDebug)
            Log.debug("GenerateObjectProxyHook: generateObject success templateName="+templateName + " oid="+oid);

        Integer result = WorldManagerClient.spawn(oid);
        if (result < 0) {
            Log.error("GenerateObjectProxyHook: spawn failed result=" + result +
                " oid="+oid);
            return ;
        }
    }

}

