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

import multiverse.server.math.Point;
import multiverse.server.math.Quaternion;
import multiverse.server.engine.*;

/** Spawn generator definition.  
 
 */
public class SpawnData extends Entity
{
    public SpawnData() {
        super();
        setNamespace(Namespace.TRANSIENT);
    }
    
    /** Create a SpawnData.
        @param name The spawn generator name.
        @param templateName Template for spawning objects.
        @param factoryName Object factory name.  Register object
                factories with {@link multiverse.server.objects.ObjectFactory#register ObjectFactory.register()}.
        @param instanceOid Instance oid.
        @param loc Spawn area center point.
        @param orient Spawn generator orientation.  Sets spawned object's
                initial orientation.
        @param spawnRadius Spawn area radius.
        @param numSpawns Number of objects to spawn.
        @param respawnTime How long after object "dies" to spawn a replacement.
     */
    public SpawnData(String name,
		     String templateName,
		     String factoryName,
		     long instanceOid,
		     Point loc,
		     Quaternion orient,
		     Integer spawnRadius,
		     Integer numSpawns,
		     Integer respawnTime)
    {
        super(name);
        setNamespace(Namespace.TRANSIENT);
        setTemplateName(templateName);
        setFactoryName(factoryName);
	setInstanceOid(instanceOid);
	setLoc(loc);
	setOrientation(orient);
	setSpawnRadius(spawnRadius);
	setNumSpawns(numSpawns);
	setRespawnTime(respawnTime);
    }

    public String toString() {
        return "[SpawnData: " +
            "oid=" + getOid() +
            ", name=" + getName() +
            ", templateName=" + getTemplateName() +
            ", factoryName=" + getFactoryName() +
            ", instanceOid=" + getInstanceOid() +
            ", loc=" + getLoc() +
            ", orient=" + getOrientation() +
            ", numSpawns=" + getNumSpawns() +
            ", respawnTime=" + getRespawnTime() +
            ", corpseDespawnTime=" + getCorpseDespawnTime() +
            "]";        
    }

    /** Set the spawn generator class name.  If not set, defaults to
        {@link multiverse.mars.objects.SpawnGenerator}.  The class
        name must be registered with
        {@link multiverse.server.plugins.MobManagerPlugin#registerSpawnGeneratorClass MobManagerPlugin.registerSpawnGeneratorClass()}.
        The registered class must be a SpawnGenerator sub-class.  An instance
        is created
        using the no-argument constructor.  Sub-classes may implement
        <code>initialize(SpawnData)</code> to override the
        initialization behavior.
    */
    public void setClassName(String className) {
	this.className = className;
    }

    /** Get the spawn generator class name.
    */
    public String getClassName() {
	return this.className;
    }

    /** Set the template for spawning objects.
    */
    public void setTemplateName(String templateName) {
	this.templateName = templateName;
    }

    /** Get the template for spawning objects.
    */
    public String getTemplateName() {
	return this.templateName;
    }

    /** Set the object factory name.
        Register object factories with
        {@link multiverse.server.objects.ObjectFactory#register ObjectFactory.register()}.
    */
    public void setFactoryName(String factoryName) {
	this.factoryName = factoryName;
    }

    /** Get the object factory name.
    */
    public String getFactoryName() {
	return this.factoryName;
    }

    /** Get the instance oid.
    */
    public long getInstanceOid()
    {
        return instanceOid;
    }

    /** Set the instance oid.
    */
    public void setInstanceOid(long oid)
    {
        instanceOid = oid;
    }

    /** Set spawn area center point.
    */
    public void setLoc(Point loc) {
	this.loc = loc;
    }

    /** Get spawn area center point.
    */
    public Point getLoc() {
	return this.loc;
    }

    /** Set the initial spawned object orientation.
    */
    public void setOrientation(Quaternion orient) {
        this.orient = orient;
    }

    /** Get the initial spawned object orientation.
    */
    public Quaternion getOrientation() {
        return this.orient;
    }

    /** Set the spawn area radius.
    */
    public void setSpawnRadius(Integer spawnRadius) {
	this.spawnRadius = spawnRadius;
    }

    /** Get the spawn area radius.
    */
    public Integer getSpawnRadius() {
	return this.spawnRadius;
    }

    /** Set the number of spawned objects.
    */
    public void setNumSpawns(Integer numSpawns) {
	this.numSpawns = numSpawns;
    }

    /** Get the number of spawned objects.
    */
    public Integer getNumSpawns() {
	return this.numSpawns;
    }

    /** Set the respawn time (seconds).  How long after an object "dies"
        before spawning a replacement.
    */
    public void setRespawnTime(Integer respawnTime) {
	this.respawnTime = respawnTime;
    }

    /** Get the respawn time (seconds).
    */
    public Integer getRespawnTime() {
	return this.respawnTime;
    }

    /** Set the corpse despawn time (seconds).  How long after an object
        "dies" before despawning its corpse.
    */
    public void setCorpseDespawnTime(Integer time) {
        corpseDespawnTime = time;
    }

    /** Get the corpse despawn time (seconds).
    */
    public Integer getCorpseDespawnTime() {
        return corpseDespawnTime;
    }

    private String templateName;
    private String factoryName;
    private String className;
    private long instanceOid;
    private Point loc;
    private Quaternion orient;
    private Integer spawnRadius;
    private Integer numSpawns;
    private Integer respawnTime;
    private Integer corpseDespawnTime;

    private static final long serialVersionUID = 1L;
}
