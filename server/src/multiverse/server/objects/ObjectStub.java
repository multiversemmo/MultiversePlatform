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

import java.util.*;

import multiverse.server.engine.*;
import multiverse.server.plugins.*;

public class ObjectStub extends Entity implements EntityWithWorldNode {

    public ObjectStub() {
        super();
        setNamespace(Namespace.MOB);
    }

    public ObjectStub(Long oid, InterpolatedWorldNode node, String template) {
	setOid(oid);
	setWorldNode(node);
	setTemplateName(template);
    }

    public String toString() {
        return "[ObjectStub: oid=" + getOid() + " node="+node + "]";
    }
    
    public Entity getEntity() {
        return (Entity)this;
    }
    
    public long getInstanceOid() {
        return node.getInstanceOid();
    }

    public InterpolatedWorldNode getWorldNode() { return node; }
    public void setWorldNode(InterpolatedWorldNode node) { this.node = node; }
    InterpolatedWorldNode node;

    public void setDirLocOrient(BasicWorldNode bnode) {
        if (node != null)
            node.setDirLocOrient(bnode);
    }
    
    public String getTemplateName() { return templateName; }
    public void setTemplateName(String template) { templateName = template; }
    String templateName;

    public void updateWorldNode() {
        WorldManagerClient.updateWorldNode(getOid(), new BasicWorldNode(node));
    }

    public void spawn() {
        long oid = getOid();
        MobManagerPlugin.getTracker(getInstanceOid()).addLocalObject(oid, (Integer)EnginePlugin.getObjectProperty(oid, Namespace.WORLD_MANAGER, "reactionRadius"));
        WorldManagerClient.spawn(oid);
	for (Behavior behav : behaviors) {
	    behav.activate();
	}
    }
    protected boolean spawned = false;

    public void despawn() {
        unload();
	WorldManagerClient.despawn(getOid());
    }

    public void unload() {
	for (Behavior behav : behaviors) {
	    behav.deactivate();
	}
        long oid = getOid();
        if (MobManagerPlugin.getTracker(getInstanceOid()) != null)
            MobManagerPlugin.getTracker(getInstanceOid()).removeLocalObject(oid);
	EntityManager.removeEntityByNamespace(oid, Namespace.MOB);
    }

    public void addBehavior(Behavior behav) {
	behav.setObjectStub(this);
	behaviors.add(behav);
        behav.initialize();
    }
    public void removeBehavior(Behavior behav) {
	behaviors.remove(behav);
    }
    public List<Behavior> getBehaviors() {
	return new ArrayList<Behavior>(behaviors);
    }
    public void setBehaviors(List<Behavior> behavs) {
	behaviors = new ArrayList<Behavior>(behavs);
    }
    protected List<Behavior> behaviors = new ArrayList<Behavior>();

    private static final long serialVersionUID = 1L;
}
