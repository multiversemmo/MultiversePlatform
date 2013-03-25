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

import multiverse.server.util.Log;
import multiverse.server.plugins.InstanceClient.InstanceEntryReqMessage;
import multiverse.server.plugins.InstanceClient;
import multiverse.server.engine.BasicWorldNode;
import multiverse.server.engine.Engine;
import multiverse.server.math.MVVector;
import multiverse.msgsys.ResponseCallback;
import multiverse.msgsys.ResponseMessage;
import multiverse.msgsys.BooleanResponseMessage;

/** Perform instance entry when player enters a region.  Register with
WorldManagerPlugin.registerRegionTrigger().  The trigger is controled
by region properties:
<ul>
<li>instanceName -- destination instance name
<li>locMarker -- name of marker spawn point in destination instance
</ul>
*/
public class InstanceEntryRegionTrigger
    implements RegionTrigger, ResponseCallback
{
    /** Change player instance when they enter region.
    */
    public void enter(MVObject obj, Region region)
    {
        if (! obj.getType().isPlayer())
            return;

        String instanceName = (String) region.getProperty("instanceName");
        if (instanceName == null) {
            Log.error("InstanceEntryRegionTrigger: missing instanceName property on region " + region);
            return;
        }

        String markerName = (String) region.getProperty("locMarker");
        if (markerName == null) {
            Log.error("InstanceEntryRegionTrigger: missing locMarker property on region " + region);
            return;
        }

        Long instanceOid = InstanceClient.getInstanceOid(instanceName);
        if (instanceOid == null) {
            Log.error("InstanceEntryRegionTrigger: unknown instanceName=" +
                instanceName);
            return;
        }

        Marker marker = InstanceClient.getMarker(instanceOid, markerName);
        if (marker == null) {
            Log.error("Instance entry event: unknown locMarker=" +
                markerName);
            return;
        }

        BasicWorldNode wnode = new BasicWorldNode();
        wnode.setInstanceOid(instanceOid);
        wnode.setLoc(marker.getPoint());
        wnode.setDir(new MVVector(0,0,0));
        if (marker.getOrientation() != null)
            wnode.setOrientation(marker.getOrientation());

        InstanceEntryReqMessage message =
            new InstanceEntryReqMessage(obj.getOid(), wnode);

        Engine.getAgent().sendRPC(message,this);
    }

    /** Do nothing.
    */
    public void leave(MVObject obj, Region region)
    {
        // do nothing
    }

    public void handleResponse(ResponseMessage response)
    {
        if (Log.loggingDebug) {
            Log.debug("InstanceEntryRegionTrigger: instance entry result=" +
                ((BooleanResponseMessage) response).getBooleanVal());
        }
        // do nothing
    }

}
