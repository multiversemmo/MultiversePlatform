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

package multiverse.server.plugins;

import java.io.*;
import java.util.*;
import java.util.concurrent.locks.Lock;

import multiverse.msgsys.*;
import multiverse.server.engine.*;
import multiverse.server.math.*;
import multiverse.server.messages.*;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.objects.*;
import multiverse.server.pathing.*;
import multiverse.server.util.*;

/**
 * client for sending/getting messages to the WorldManagerPlugin
 */
public class WorldManagerClient {

    public static final int NO_FLAGS = 0;
    public static final int SAVE_NOW = 1;

    public static boolean setWorldNode(Long oid, BasicWorldNode wnode)
    {
        return setWorldNode(oid, wnode, NO_FLAGS);
    }

    /**
     * sets/initializes the world node for this object. will over-ride any
     * existing worldnode the object may have. it is an error to call this after
     * the object has been spawned
     */
    public static boolean setWorldNode(Long oid, BasicWorldNode wnode,
        int flags)
    {
        Log.debug("WorldManagerClient.setWorldNode: oid="+oid+
                " node="+wnode+" flags="+flags);
        SetWorldNodeReqMessage msg =
            new SetWorldNodeReqMessage(oid, wnode, flags);
        Boolean rc = Engine.getAgent().sendRPCReturnBoolean(msg);
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.setWorldNode: oid="+oid+" got response rc="+rc);
        return rc;
    }

    /**
     * updates the world node on the world manager. this call does not block if
     * the update fails, you will get a dir/loc correction msg
     */
    public static void updateWorldNode(Long oid, BasicWorldNode wnode)
    {
	updateWorldNode(oid, wnode, false, null, null);
    }

    public static void updateWorldNode(Long oid,
        BasicWorldNode wnode, boolean override)
    {
        updateWorldNode(oid,wnode,override,null,null);
    }

    public static void updateWorldNode(Long oid, BasicWorldNode wnode,
        boolean override, Message preMessage, Message postMessage)
    {
        UpdateWorldNodeReqMessage msg = new UpdateWorldNodeReqMessage(oid,
                wnode);
        msg.setOverride(override);
        msg.setPreMessage(preMessage);
        msg.setPostMessage(postMessage);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * queries the world manager for the world node of the object
     */
    public static BasicWorldNode getWorldNode(Long oid) {
	SubjectMessage msg = new SubjectMessage(MSG_TYPE_GETWNODE_REQ, oid);
	BasicWorldNode wnode =
            (BasicWorldNode)Engine.getAgent().sendRPCReturnObject(msg);
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.getWorldNode: oid=" + oid +
                " wnode="+wnode);
	return wnode;
    }

    /**
     * corrects the client to match the world manager. this can be the result of
     * a /goto command or interpolation errors
     */
    public static void correctWorldNode(Long oid, BasicWorldNode wnode) {
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.correctWorldNode: loc=" + wnode.getLoc());
        WorldNodeCorrectMessage msg = new WorldNodeCorrectMessage(oid, wnode);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * reparents the world node on the world manager under the specified parent
     */
    public static void reparentWorldNode(Long oid, Long parentOid) {
        Log.debug("WorldManagerClient.reparentWorldNode: sending message");
        ReparentWNodeReqMessage msg = new ReparentWNodeReqMessage(oid, parentOid);
        Engine.getAgent().sendBroadcast(msg);
        Log.debug("WorldManagerClient.reparentWorldNode: sent message");
    }

    /** Place object into the world.
        @see #spawn(java.lang.Long,multiverse.msgsys.Message,multiverse.msgsys.Message)
     */
    public static Integer spawn(Long oid) {
        return spawn(oid,null,null);
    }

    /** Place object into the world.  The object can perceive nearby
        objects, and other objects can perceive it.  The {@code preMessage}
        is broadcast just before spawning the object.  The {@code postMessage}
        is broadcast just after spawning the object.  The pre/post
        messages will bracket the object's first PerceptionMessage.
        @param oid Object to spawn.
        @param preMessage Message to broadcast before spawning.
        @param postMessage Message to broadcast after spawning.
        @return On success, the number of perceived objects.  Returns null
        if the object is not a mobile perceiver.  Returns -1 if the
        object does not exist.  Returns -2 if the
        object's instance does not exist on this world manager.  Returns -3
        if the object is already spawned.
     */
    public static Integer spawn(Long oid,
        Message preMessage, Message postMessage)
    {
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.spawn: oid=" + oid);
        SpawnReqMessage msg = new SpawnReqMessage(oid);
        msg.setPreMessage(preMessage);
        msg.setPostMessage(postMessage);
        Integer result = (Integer)Engine.getAgent().sendRPCReturnObject(msg);
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.spawn: response for oid=" + oid +
                " result="+result);
        return result;
    }

    /** Remove object from the world.
        @see #despawn(java.lang.Long,multiverse.msgsys.Message,multiverse.msgsys.Message)
     */
    public static boolean despawn(Long oid) {
        return despawn(oid,null,null);
    }

    /** Remove object from the world.  The object can no longer perceive
        objects around them, and other objects can no longer perceive it.
        The {@code preMessage}
        is broadcast just before despawning the object.  The {@code postMessage}
        is broadcast just after despawning the object.  The pre/post
        messages will bracket the object's final PerceptionMessage.
        @param oid Object to despawn.
        @param preMessage Message to broadcast before despawning.
        @param postMessage Message to broadcast after despawning.
        @return True on success, false on failure.
    */
    public static boolean despawn(Long oid,
        Message preMessage, Message postMessage)
    {
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.despawn: oid=" + oid);
        DespawnReqMessage msg = new DespawnReqMessage(oid);
        msg.setPreMessage(preMessage);
        msg.setPostMessage(postMessage);
        Boolean rc = Engine.getAgent().sendRPCReturnBoolean(msg);
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.despawn: response oid=" + oid +
                " rc="+rc);
        return rc;
    }

    public static ObjectInfo getObjectInfo(Long oid) {
        ObjInfoReqMessage msg = new ObjInfoReqMessage(oid);
        ObjInfoRespMessage respMsg = (ObjInfoRespMessage)Engine.getAgent().sendRPC(msg);
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.getObjectInfo: oid=" + oid +
                " info=" + respMsg.getObjInfo());
        return respMsg.getObjInfo();
    }

    /**
     * returns the current display context for this obj
     */
    public static DisplayContext getDisplayContext(Long oid) {
        DisplayContextReqMessage msg = new DisplayContextReqMessage(oid);
        DisplayContext dc = (DisplayContext)Engine.getAgent().sendRPCReturnObject(msg);
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.getDisplayContext: oid=" + oid +
                " dc=" + dc);
        return dc;
    }

    /**
     * modifies the object's current display context. can either add or remove
     * depending on the 'action' passed in. passed in submeshes which are/are
     * not in the obj's dc are silently ignored
     */
    public static void modifyDisplayContext(Long oid,
            byte action, String base,
            List<DisplayContext.Submesh> submeshes) {
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.modifyDisplayContext: oid=" + oid +
                " action=" + action + " base=" + base +
                " submeshCount=" + submeshes.size());
        ModifyDisplayContextMessage msg = new ModifyDisplayContextMessage(oid,
                action, base, submeshes, null, null);
        Engine.getAgent().sendBroadcast(msg);
    }

    public static void modifyDisplayContext(Long oid, byte action,
            List<DisplayContext.Submesh> submeshes) {
        if (Log.loggingDebug)
            Log.debug("WorldManagerClient.modifyDisplayContext: oid=" + oid +
                " action=" + action +
                " submeshCount=" + submeshes.size());
        ModifyDisplayContextMessage msg = new ModifyDisplayContextMessage(oid,
                action, submeshes);
        Engine.getAgent().sendBroadcast(msg);
    }

    public static final byte modifyDisplayContextActionReplace = (byte)1;
    public static final byte modifyDisplayContextActionAdd = (byte)2;
    public static final byte modifyDisplayContextActionAddChild = (byte)3;
    public static final byte modifyDisplayContextActionRemove = (byte)4;
    public static final byte modifyDisplayContextActionRemoveChild = (byte)5;

    /**
     * update notifyObj about updateObj - sends over all game related
     * information about the object modelinfo, animation, sound this message
     * returns immediately - the world manager plugin will send out the
     * appropriate messages
     */
    public static void updateObject(Long notifyObj, Long updateObj) {
        UpdateMessage msg = new UpdateMessage(notifyObj, updateObj);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * forces the world manager to send out a UpdateWorldNodeMessage so everyone
     * subscribed to it gets an update
     */
    public static void refreshWNode(Long objId) {
        RefreshWNodeMessage msg = new RefreshWNodeMessage(objId);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * an object wants to send a chat msg - msg gets sent to world manager
     * non-blocking call
     */
    public static void sendChatMsg(Long objId, int channelId,
            String text) {
        ComReqMessage msg = new ComReqMessage(objId, channelId, text);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * sends message to the object (this is not a request message)
     */
    public static void sendObjChatMsg(Long objOid,
            int channelId, String text) {
        WorldManagerClient.TargetedComMessage comMsg =
                new WorldManagerClient.TargetedComMessage(objOid,
                objOid, channelId, text);
        Engine.getAgent().sendBroadcast(comMsg);
    }

    /**
     * sends system chat message to all players (this is not a request message)
     */
    public static void sendSysChatMsg(String text) {
        WorldManagerClient.SysChatMessage sysMsg = new WorldManagerClient.SysChatMessage(text);
        Engine.getAgent().sendBroadcast(sysMsg);
    }

    /**
     * an object requests to change its orientation - msg gets send to wm
     * non-blocking call
     */
    public static void sendOrientMsg(Long objId, Quaternion q) {
        OrientReqMessage msg = new OrientReqMessage(objId, q);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * send out this world manager's perceiver region targetSessionId can be
     * null
     */
    public static void sendPerceiverRegionsMsg(long instanceOid,
        Geometry region, String targetSessionId)
    {
        if (region == null) {
            throw new RuntimeException("region geometry is null");
        }
        PerceiverRegionsMessage msg = new PerceiverRegionsMessage(instanceOid,
            region);
        if (targetSessionId != null) {
            msg.setTargetSessionId(targetSessionId);
        }
        Engine.getAgent().sendBroadcast(msg);
    }

    public static Boolean hostInstance(long instanceOid, String wmPluginName)
    {
        HostInstanceMessage message = new HostInstanceMessage(wmPluginName,
            instanceOid);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    /**
     * Gets the value for the key in the object's properties.
     * @param oid oid for the object with the property
     * @param key string for the key/value pair
     * @return the value for the matching key
     * @deprecated Replaced by {@link multiverse.server.engine.EnginePlugin#getObjectProperty(
     *                          Long, Namespace, String)}
     */
    @Deprecated
    public static Serializable getObjectProperty(Long oid, String key) {
	return EnginePlugin.getObjectProperty(oid, Namespace.WORLD_MANAGER, key);
    }

    /**
     * Sets the value for the key in the object's properties returns the old
     * mapping, or null if none existed.
     * @param oid oid for the object with the property
     * @param key string for the key/value pair
     * @param val Serializable the value for key/value pair
     * @return The previous value of the property, or null
     * @deprecated Replaced by {@link multiverse.server.engine.EnginePlugin#setObjectProperty(
     *                          Long, Namespace, String, Serializable)}
     */
    @Deprecated
    public static Serializable setObjectProperty(Long oid,
            String key, Serializable val) {
	return EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, key, val);
    }

    /**
     * Sets the value for the key in the object's properties.  Does not return a result.
     * @param oid oid for the object with the property
     * @param key string for the key/value pair
     * @param val Serializable the value for key/value pair
     * @deprecated Replaced by {@link multiverse.server.engine.EnginePlugin#setObjectPropertyNoResponse(
     *                          Long, Namespace, String, Serializable)}
     */
    @Deprecated
    public static void setObjectPropertyNoResponse(Long oid,
            String key, Serializable val) {
	EnginePlugin.setObjectPropertyNoResponse(oid, Namespace.WORLD_MANAGER, key, val);
    }
    
    // /////////////////////////////////////////////////////////
    //
    // begin messages
    //
    // /////////////////////////////////////////////////////////

    /**
     * basic information about an object objType is the getType() call in Entity
     */
    public static class ObjectInfo implements Serializable {
        public ObjectInfo() {
        }

        public String toString() {
            return "[ObjectInfo: name=" + name + ", oid=" + oid + ", loc="
                    + loc + ", orient=" + orient + ", scale=" + scale
                    + ", objType=" + objType + ", followsTerrain="
                    + followsTerrain + "]";
        }

        public MVByteBuffer toBuffer(Long notifyOid) {
            MVByteBuffer buf = new MVByteBuffer(220);
            buf.putLong(notifyOid);
            buf.putInt(8);
            buf.putLong(oid);
            buf.putString((name == null) ? "unknown" : name);
            buf.putPoint((loc == null) ? (new Point()) : loc);
            buf.putQuaternion((orient == null) ? (new Quaternion()) : orient);
            buf.putMVVector((scale == null) ? (new MVVector(1.0f, 1.0f, 1.0f))
                    : scale);
            buf.putInt(objType.getTypeId());
            buf.putInt(followsTerrain ? 1 : 0);
            buf.putMVVector(dir);
            buf.putLong(lastInterp);
            buf.flip();
            return buf;
        }
        
        /**
         * object must be serializable
         */
        public void setProperty(String key, Serializable val) {
            if (propMap == null)
                propMap = new HashMap<String, Serializable>();
            propMap.put(key, val);
        }

        public Serializable getProperty(String key) {
            if (propMap == null)
                return null;
            else
                return propMap.get(key);
        }

        public long instanceOid;

        public Long oid;

        public String name;

        public Point loc;

        public Quaternion orient;

        public MVVector scale;

        public ObjectType objType;

        public boolean followsTerrain;

        public MVVector dir;
        
        public long lastInterp;
        
        private Map<String, Serializable> propMap = null;

        private static final long serialVersionUID = 1L;
    }

    /**
     * information about a road. we dont use the MVObject because it would be
     * confuses if you thought you could actually manipulate it as a real
     * object. it would also have an oid, etc.
     */
    public static class RoadInfo implements ClientMessage, Serializable {

        public RoadInfo() {
        }
        
        public RoadInfo(RoadSegment segment) {
            setOid(segment.getOid());
            setName(segment.getName());
            setStart(segment.getStart());
            setEnd(segment.getEnd());
        }

        public RoadInfo(Long oid, String name, Point start, Point end) {
            setOid(oid);
            setName(name);
            setStart(start);
            setEnd(end);
        }

        public String toString() {
            return "[RoadInfo: oid=" + getOid() + ", name=" + getName()
                    + ", start=" + getStart() + ", end=" + getEnd() + "]";
        }

        public Long getOid() {
            return oid;
        }

        public void setOid(Long oid) {
            this.oid = oid;
        }

        public String getName() {
            return name;
        }

        public void setName(String name) {
            this.name = name;
        }

        public Point getStart() {
            return start;
        }

        public void setStart(Point start) {
            this.start = start;
        }

        public Point getEnd() {
            return end;
        }

        public void setEnd(Point end) {
            this.end = end;
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(200);
            buf.putLong(getOid());
            buf.putInt(54);
            buf.putString(getName());
            buf.putInt(2);
            buf.putPoint(start);
            buf.putPoint(end);
            buf.flip();
            return buf;
        }

        private Long oid;

        private String name;

        private Point start;

        private Point end;

        private static final long serialVersionUID = 1L;
    }

    public static class FreeRoadMessage extends TargetMessage
        implements ClientMessage, Serializable
    {

        public FreeRoadMessage() {
            super(MSG_TYPE_FREE_ROAD);
        }

        public FreeRoadMessage(Long oid) {
            super(MSG_TYPE_FREE_ROAD, oid, oid);
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(20);
            buf.putLong(getTarget());
            buf.putInt(69);
            buf.flip();
            return buf;
        }

        private static final long serialVersionUID = 1L;
    }

    public static class TerrainReqMessage extends SubjectMessage {
        public TerrainReqMessage() {
            super(MSG_TYPE_TERRAIN_REQ);
        }
        TerrainReqMessage(Long oid) {
            super(MSG_TYPE_TERRAIN_REQ, oid);
        }
        private static final long serialVersionUID = 1L;
    }

    public static class ObjInfoReqMessage extends SubjectMessage {
        public ObjInfoReqMessage() {
            super();
        }
        ObjInfoReqMessage(Long oid) {
            super(MSG_TYPE_OBJINFO_REQ, oid);
        }
        private static final long serialVersionUID = 1L;
    }

    public static class ObjInfoRespMessage extends ResponseMessage implements ITargetSessionId {

        public ObjInfoRespMessage() {
            super();
        }

        public ObjInfoRespMessage(Message msg, String targetSessionId, ObjectInfo objInfo) {
            super(msg);
            setTargetSessionId(targetSessionId);
            setObjInfo(objInfo);
        }
        
        public String getTargetSessionId() {
            return targetSessionId;
        }

        public void setTargetSessionId(String targetSessionId) {
            this.targetSessionId = targetSessionId;
        }
        
        public void setObjInfo(ObjectInfo objInfo) {
            this.objInfo = objInfo;
        }
        public ObjectInfo getObjInfo() {
            return objInfo;
        }
        
        private String targetSessionId;
        
        private ObjectInfo objInfo = null;

        private static final long serialVersionUID = 1L;
    }
    
    public static class DisplayContextReqMessage extends SubjectMessage {
        public DisplayContextReqMessage() {
            super(MSG_TYPE_DC_REQ);
        }
        DisplayContextReqMessage(Long oid) {
            super(MSG_TYPE_DC_REQ, oid);
        }
        private static final long serialVersionUID = 1L;
    }

    public static class SetAmbientLightMessage extends TargetMessage {
        
        public SetAmbientLightMessage() {
            setMsgType(MSG_TYPE_SET_AMBIENT);
        }

        public SetAmbientLightMessage(Long oid, Color color) {
            super(MSG_TYPE_SET_AMBIENT, oid, oid);
            setColor(color);
        }

        public void setColor(Color color) {
            this.color = color;
        }
        
        public Color getColor() {
            return color;
        }
        
        private Color color;
        
        private static final long serialVersionUID = 1L;
    }
    
    public static class FogMessage extends TargetMessage {

        public FogMessage() {
            super(MSG_TYPE_FOG);
        }

        public FogMessage(Long oid, FogRegionConfig fogConfig) {
            super(MSG_TYPE_FOG, oid, oid);
            setFogConfig(fogConfig);
        }

        public FogMessage(Long oid, Fog fog) {
            super(MSG_TYPE_FOG, oid, oid);
            FogRegionConfig config = new FogRegionConfig();
            config.setColor(fog.getColor());
            config.setNear(fog.getStart());
            config.setFar(fog.getEnd());
            setFogConfig(config);
        }

        public MVByteBuffer toBuffer() {
            int msgId = Engine.getEventServer().getEventID(this.getClass());
            MVByteBuffer buf = new MVByteBuffer(32);

            buf.putLong(0); // not used
            buf.putInt(msgId);
            buf.putColor(fogConfig.getColor());
            buf.putInt(fogConfig.getNear());
            buf.putInt(fogConfig.getFar());
            buf.flip();
            return buf;
        }

        public void setFogConfig(FogRegionConfig fogConfig) {
            this.fogConfig = fogConfig;
        }

        public FogRegionConfig getFogConfig() {
            return this.fogConfig;
        }

        private FogRegionConfig fogConfig = null;

        private static final long serialVersionUID = 1L;
    }

    /**
     * Message from the WorldManagerPlugin saying that a particular mob/player
     * perceives a road.
     * 
     * @author cedeno
     */
    public static class RoadMessage extends TargetMessage {

        public RoadMessage() {
            super(MSG_TYPE_ROAD);
        }

        public RoadMessage(Long oid, Set<Road> roads) {
            super(MSG_TYPE_ROAD, oid, oid);
            setRoads(roads);
        }

        public void setRoads(Set<Road> roads) {
            this.roads = roads;
        }

        public Set<Road> getRoads() {
            return new HashSet<Road>(this.roads);
        }

        /**
         * returns a list of road messages which should be sent to the client
         * 
         * @return list of road messages which should be sent to the client
         */
        public List<MVByteBuffer> toBuffer() {
            List<MVByteBuffer> bufList = new LinkedList<MVByteBuffer>();

            for (Road road : roads) {
                MVByteBuffer buf = new MVByteBuffer(1000);
                buf.putLong(road.getOid());
                buf.putInt(54);
                buf.putString(road.getName());
                List<Point> points = road.getPoints();
                buf.putInt(points.size());
                for (Point p : points) {
                    buf.putPoint(p);
                }
                buf.putInt(road.getHalfWidth());
                buf.flip();
                bufList.add(buf);
            }
            return bufList;
        }

        private Set<Road> roads = new HashSet<Road>();

        private static final long serialVersionUID = 1L;
    }

    public static class SpawnReqMessage extends SubjectMessage
        implements BracketedMessage
    {

        public SpawnReqMessage() {
            super(MSG_TYPE_SPAWN_REQ);
        }

        SpawnReqMessage(Long oid) {
            super(MSG_TYPE_SPAWN_REQ, oid);
        }

        public Message getPreMessage()
        {
            return preMessage;
        }

        public void setPreMessage(Message message)
        {
            preMessage = message;
        }

        public Message getPostMessage()
        {
            return postMessage;
        }

        public void setPostMessage(Message message)
        {
            postMessage = message;
        }

        private Message preMessage;
        private Message postMessage;

        private static final long serialVersionUID = 1L;
    }

    public static class SpawnedMessage extends SubjectMessage {

        public SpawnedMessage() {
            super(MSG_TYPE_SPAWNED);
        }

        public SpawnedMessage(Long oid, long instanceOid,
                ObjectType objectType)
        {
            super(MSG_TYPE_SPAWNED, oid);
            setInstanceOid(instanceOid);
            setType(objectType);
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setType(ObjectType type) {
            objectType = type;
        }

        public ObjectType getType() {
            return objectType;
        }

        private long instanceOid;
        private ObjectType objectType;

        private static final long serialVersionUID = 1L;
    }
    
    public static class DespawnReqMessage extends SubjectMessage
        implements BracketedMessage
    {
        public DespawnReqMessage() {
            super(MSG_TYPE_DESPAWN_REQ);
        }
        DespawnReqMessage(Long oid) {
            super(MSG_TYPE_DESPAWN_REQ, oid);
        }

        public Message getPreMessage()
        {
            return preMessage;
        }

        public void setPreMessage(Message message)
        {
            preMessage = message;
        }

        public Message getPostMessage()
        {
            return postMessage;
        }

        public void setPostMessage(Message message)
        {
            postMessage = message;
        }

        private Message preMessage;
        private Message postMessage;

        private static final long serialVersionUID = 1L;
    }

    public static class DespawnedMessage extends SubjectMessage {

        public DespawnedMessage() {
            super(MSG_TYPE_DESPAWNED);
        }

        public DespawnedMessage(Long oid, long instanceOid,
                ObjectType type)
        {
            super(MSG_TYPE_DESPAWNED, oid);
            setInstanceOid(instanceOid);
            setType(type);            
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setType(ObjectType type) {
            this.type = type;
        }

        public ObjectType getType() {
            return type;
        }

        private long instanceOid;
        private ObjectType type;

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * request to the world manager asking to set the world node for the obj
     * matching oid
     */
    public static class SetWorldNodeReqMessage extends SubjectMessage {

        public SetWorldNodeReqMessage() {
            super(MSG_TYPE_SETWNODE_REQ);
        }

        SetWorldNodeReqMessage(Long oid, BasicWorldNode wnode, int flags) {
            super(MSG_TYPE_SETWNODE_REQ, oid);

            setWorldNode(wnode);
            setFlags(flags);
        }

        public void setWorldNode(BasicWorldNode wnode) {
            this.wnode = wnode;
        }

        public BasicWorldNode getWorldNode() {
            return wnode;
        }

        public void setFlags(int flags) {
            this.flags = flags;
        }

        public int getFlags() {
            return flags;
        }

        private BasicWorldNode wnode = null;
        private int flags;

        private static final long serialVersionUID = 1L;
    }

    public static class UpdateWorldNodeReqMessage extends SubjectMessage
        implements BracketedMessage
    {

        public UpdateWorldNodeReqMessage() {
            super();
        }
        
        public UpdateWorldNodeReqMessage(Long oid, BasicWorldNode wnode) {
            super(MSG_TYPE_UPDATEWNODE_REQ, oid);
            setWorldNode(wnode);
        }

        public void setWorldNode(BasicWorldNode wnode) {
            this.wnode = wnode;
        }

        public BasicWorldNode getWorldNode() {
            return wnode;
        }
        
	public void setOverride(boolean override) {
	    this.override = override;
	}

	public boolean getOverride() {
	    return override;
	}

        public Message getPreMessage()
        {
            return preMessage;
        }

        public void setPreMessage(Message message)
        {
            preMessage = message;
        }

        public Message getPostMessage()
        {
            return postMessage;
        }

        public void setPostMessage(Message message)
        {
            postMessage = message;
        }

        private Message preMessage;
        private Message postMessage;

        private BasicWorldNode wnode = null;
	protected boolean override = false;

        private static final long serialVersionUID = 1L;
    }

    /**
     * telling client/mob to update their world node
     */
    public static class UpdateWorldNodeMessage extends SubjectMessage implements
            ClientMessage {

        public UpdateWorldNodeMessage() {
            super(MSG_TYPE_UPDATEWNODE);
        }

        public UpdateWorldNodeMessage(Long oid, BasicWorldNode wnode) {
            super(MSG_TYPE_UPDATEWNODE, oid);
            setWorldNode(wnode);
        }
        
        public String toString() {
            return "[UpdateWorldNodeMessage " + getWorldNode() + "]";
        }

        public void setWorldNode(BasicWorldNode wnode) {
            this.wnode = wnode;
        }

        public BasicWorldNode getWorldNode() {
            return wnode;
        }

        public MVByteBuffer toBuffer() {
            BasicWorldNode bnode = getWorldNode();

            MVByteBuffer buf = new MVByteBuffer(64);
            buf.putLong(getSubject());
            buf.putInt(2);
            buf.putLong(System.currentTimeMillis());

            buf.putMVVector(bnode.getDir());
            buf.putPoint(bnode.getLoc());
            buf.flip();
            return buf;
        }

        private BasicWorldNode wnode = null;

        public void setEventBuf(MVByteBuffer buf)
        {
            eventBuf = buf;
        }

        public MVByteBuffer getEventBuf()
        {
            return eventBuf;
        }

        transient MVByteBuffer eventBuf;

        private static final long serialVersionUID = 1L;
    }

    public static class DirLocOrientMessage extends SubjectMessage
        implements ClientMessage
    {

        public DirLocOrientMessage() {
            super(MSG_TYPE_DIR_LOC_ORIENT);
        }
        
        public DirLocOrientMessage(Long oid, BasicWorldNode wnode) {
            super(MSG_TYPE_DIR_LOC_ORIENT,oid);
            setWorldNode(wnode);
        }

        public String toString() {
            return "[DirLocOrient oid=" + getSubject() + ", wnode="
                    + getWorldNode() + "]";
        }

        public void setWorldNode(BasicWorldNode wnode) {
            this.wnode = wnode;
        }

        public BasicWorldNode getWorldNode() {
            return wnode;
        }

        public MVByteBuffer toBuffer() {
            BasicWorldNode bnode = getWorldNode();

            MVByteBuffer buf = new MVByteBuffer(128);
            buf.putLong(getSubject());
            buf.putInt(79);
            buf.putLong(System.currentTimeMillis());

            MVVector dir = bnode.getDir();
            buf.putMVVector(dir == null ? new MVVector() : dir);
            Point loc = bnode.getLoc();
            buf.putPoint(loc == null ? new Point() : loc);
            Quaternion q = bnode.getOrientation();
            buf.putQuaternion(q == null ? new Quaternion() : q);
            buf.flip();
            return buf;
        }

        /* Used by simpleclient/PlayerClient.java */
        public void fromBuffer(MVByteBuffer buf) {
            Long oid = buf.getLong();
            int msgNumber = buf.getInt();
            if (msgNumber != 79) {
                Log.error("DirLocOrientMessage.fromBuffer: msgNumber " + msgNumber + " is not 79");
                return;
            }
            // Ignore the time
            buf.getLong();
            MVVector dir = buf.getMVVector();
            Point loc = buf.getPoint();
            Quaternion orient = buf.getQuaternion();
            BasicWorldNode wnode = new BasicWorldNode();
            wnode.setDir(dir);
            wnode.setLoc(loc);
            wnode.setOrientation(orient);
            setWorldNode(wnode);
            setSubject(oid);
        }

        private BasicWorldNode wnode = null;

        private static final long serialVersionUID = 1L;
    }

    /**
     * telling client/mob to correct their location to match world node
     */
    public static class WorldNodeCorrectMessage extends SubjectMessage implements
            ClientMessage {
        public WorldNodeCorrectMessage() {
            super();
            setMsgType(MSG_TYPE_WNODECORRECT);
        }
        
        public WorldNodeCorrectMessage(Long oid, BasicWorldNode wnode) {
            super(MSG_TYPE_WNODECORRECT, oid);
            setWorldNode(wnode);
        }

        public String toString() {
            return "[WorldNodeCorrectMessage oid=" + getSubject() + ", wnode="
                    + getWorldNode() + "]";
        }

        public void setWorldNode(BasicWorldNode wnode) {
            this.wnode = wnode;
        }

        public BasicWorldNode getWorldNode() {
            return wnode;
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(128);
            buf.putLong(getSubject());
            buf.putInt(79);
            buf.putLong(System.currentTimeMillis());

            MVVector dir = wnode.getDir();
            buf.putMVVector(dir == null ? new MVVector() : dir);
            Point loc = wnode.getLoc();
            buf.putPoint(loc == null ? new Point() : loc);
            Quaternion q = wnode.getOrientation();
            buf.putQuaternion(q == null ? new Quaternion() : q);
            buf.flip();
            return buf;
        }

        /* Used by simpleclient/PlayerClient.java */
        public void fromBuffer(MVByteBuffer buf) {
            Long oid = buf.getLong();
            int msgNumber = buf.getInt();
            if (msgNumber != 79) {
                Log.error("WorldNodeCorrectMessage.fromBuffer: msgNumber " + msgNumber + " is not 79");
                return;
            }
            // Ignore the time
            buf.getLong();
            MVVector dir = buf.getMVVector();
            Point loc = buf.getPoint();
            Quaternion orient = buf.getQuaternion();
            BasicWorldNode wnode = new BasicWorldNode();
            wnode.setDir(dir);
            wnode.setLoc(loc);
            wnode.setOrientation(orient);
            setWorldNode(wnode);
            setSubject(oid);
        }

        private BasicWorldNode wnode = null;

        private static final long serialVersionUID = 1L;
    }

    public static class ReparentWNodeReqMessage extends SubjectMessage {

	public ReparentWNodeReqMessage() {
	    super(MSG_TYPE_REPARENT_WNODE_REQ);
	}

	public ReparentWNodeReqMessage(Long oid, Long parentOid) {
	    super(MSG_TYPE_REPARENT_WNODE_REQ, oid);
	    setParentOid(parentOid);
	}

	public String toString() {
	    return "[ReparentWNodeReqMessage oid= + getSubject()" + ", parent=" + parentOid + "]";
	}

	public void setParentOid(Long parentOid) {
	    this.parentOid = parentOid;
	}

	public Long getParentOid() {
	    return parentOid;
	}

        private Long parentOid;
        
        private static final long serialVersionUID = 1L;
    }

    /** Information about a perceived object.  Used in a PerceptionMessage,
        @see multiverse.server.messages.PerceptionMessage.ObjectNote
    */
    public static class PerceptionInfo {
        public PerceptionInfo() {
        }

        public PerceptionInfo(WorldManagerClient.ObjectInfo info) {
            objectInfo = info;
        }

        public WorldManagerClient.ObjectInfo objectInfo;
        public DisplayContext displayContext;
    }


    /**
     * world manager is telling another world manager that one of its local
     * objects has come into its fixed perceiver region includes target session
     * id so it can filtered to the correct world manager
     */
    public static class NewRemoteObjectMessage extends SubjectMessage implements ITargetSessionId {

        public NewRemoteObjectMessage() {
            super(MSG_TYPE_NEW_REMOTE_OBJ);
        }

        public NewRemoteObjectMessage(String targetSessionId,
                long instanceOid, Long newObjId,
                Point loc, Quaternion orient, int perceptionRadius,
		ObjectType type) {
            super(MSG_TYPE_NEW_REMOTE_OBJ, newObjId);
            setTargetSessionId(targetSessionId);
            setInstanceOid(instanceOid);
            setLoc(loc);
            setOrient(orient);
            setPerceptionRadius(perceptionRadius);
	    setType(type);
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setLoc(Point loc) {
            this.loc = loc;
        }

        public Point getLoc() {
            return loc;
        }

        public void setOrient(Quaternion orient) {
            this.orient = orient;
        }

        public Quaternion getOrient() {
            return orient;
        }
        
        public void setPerceptionRadius(int perceptionRadius) {
            this.perceptionRadius = perceptionRadius;
        }
        
        public int getPerceptionRadius() {
            return perceptionRadius;
        }

        public void setType(ObjectType type) {
            this.type = type;
        }

        public ObjectType getType() {
            return type;
        }

        public String getTargetSessionId() {
            return targetSessionId;
        }
    
        public void setTargetSessionId(String targetSessionId) {
            this.targetSessionId = targetSessionId;
        }

        private long instanceOid;

        private Point loc;

        private Quaternion orient;

	private ObjectType type;

        int perceptionRadius;

        private String targetSessionId;

        private static final long serialVersionUID = 1L;
    }

    public static class FreeRemoteObjectMessage extends SubjectMessage implements ITargetSessionId{

        public FreeRemoteObjectMessage() {
            super(MSG_TYPE_FREE_REMOTE_OBJ);
        }

        public FreeRemoteObjectMessage(String targetSessionId,
                long instanceOid, Long objId)
        {
            super(MSG_TYPE_FREE_REMOTE_OBJ, objId);
            setTargetSessionId(targetSessionId);
            setInstanceOid(instanceOid);
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public String getTargetSessionId() {
            return targetSessionId;
        }
    
        public void setTargetSessionId(String targetSessionId) {
            this.targetSessionId = targetSessionId;
        }

        private long instanceOid;
        private String targetSessionId;

        private static final long serialVersionUID = 1L;
    }

    /**
     * Update the notifyOid about updateOid.  The subject is the updateOid
     * since owners will subscribe to their objects.
     */
    public static class UpdateMessage extends SubjectMessage
        implements HasTarget
    {

        public UpdateMessage() {
            super(MSG_TYPE_UPDATE_OBJECT);
        }

        UpdateMessage(Long notifyOid, Long updateOid) {
            super(MSG_TYPE_UPDATE_OBJECT, updateOid);
            target = notifyOid;
            if ((notifyOid == null) || (updateOid == null)) {
                throw new RuntimeException("null oid");
            }
        }

        public long getTarget()
        {
            return target;
        }

        public void setTarget(long target)
        {
            this.target = target;
        }

        Long target;

        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from world server regarding the display context MSG_OID set
     * to the dc subject
     */
    public static class DisplayContextMessage extends SubjectMessage {

        public DisplayContextMessage() {
            super(MSG_TYPE_DISPLAY_CONTEXT);
        }

        public DisplayContextMessage(Long dcObjOid, DisplayContext dc) {
            super(MSG_TYPE_DISPLAY_CONTEXT, dcObjOid);
            setDisplayContext(dc);
        }

        public DisplayContext getDisplayContext() {
            return dc;
        }

        public void setDisplayContext(DisplayContext dc) {
            this.dc = dc;
        }

        private DisplayContext dc;
        
        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from world server telling us to detach an item from an existing dc
     */
    public static class DetachMessage extends SubjectMessage {

        public DetachMessage() {
            super(MSG_TYPE_DETACH);
        }
        
        public DetachMessage(Long dcObjOid, Long objBeingDetached, String socketName) {
            super(MSG_TYPE_DETACH, dcObjOid);
            setSocketName(socketName);
            setObjBeingDetached(objBeingDetached);
        }

        public String getSocketName() {
            return socketName;
        }

        public void setSocketName(String socket) {
            this.socketName = socket;
        }
        
        public Long getObjBeingDetached() {
            return objBeingDetached;
        }
        public void setObjBeingDetached(Long oid) {
            this.objBeingDetached = oid;
        }
        private String socketName;
        private Long objBeingDetached = null;
        private static final long serialVersionUID = 1L;
    }
    
    /**
     * notification from proxy/mobserver regarding chat msg MSG_OID is person
     * saying something
     */
    public static class ComReqMessage extends SubjectMessage {
        // for bridgemessage

        public ComReqMessage() {
            super();
        }
        
        public ComReqMessage(Long objOid, int channel, String msgString) {
            super(MSG_TYPE_COM_REQ, objOid);
            setChannel(channel);
            setString(msgString);
        }

        public String getString() {
            return msgString;
        }

        public void setString(String msgString) {
            this.msgString = msgString;
        }

        public int getChannel() {
            return channel;
        }

        public void setChannel(int channel) {
            this.channel = channel;
        }

        int channel = -1;
        private String msgString;

        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from worldserver saying a mob said something MSG_OID is
     * person saying something
     */
    public static class ComMessage extends SubjectMessage
        implements ClientMessage
    {

        public ComMessage() {
            super(MSG_TYPE_COM);
        }

        public ComMessage(Long objOid, int channel, String msgString) {
            super(MSG_TYPE_COM,objOid);
            setChannel(channel);
            setString(msgString);
        }

        public String toString() {
            return "[ComMessage: objOid=" + getSubject() +
                ", channel=" + getChannel() + ", msg=" + getString() + "]";
        }
        
        public String getString() {
            return msgString;
        }

        public void setString(String msgString) {
            this.msgString = msgString;
        }

        public int getChannel() {
            return channel;
        }

        public void setChannel(int channel) {
            this.channel = channel;
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(400);
            buf.putLong(this.getSubject());
            buf.putInt(3);
            buf.putInt(this.getChannel());
            buf.putString(this.getString());
            buf.flip();
            return buf;
        }

        public void fromBuffer(MVByteBuffer buf) {
            /* Long oid = */ buf.getLong();
            int msgNumber = buf.getInt();
            if (msgNumber != 3) {
                Log.error("ComMessage.fromBuffer: msgNumber " + msgNumber + " is not 3");
                return;
            }
            channel = buf.getInt();
            msgString = buf.getString();
        }

        int channel = -1;
        private String msgString;

        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from worldserver saying a mob said something MSG_OID is
     * person saying something
     */
    public static class TargetedComMessage extends TargetMessage
        implements ClientMessage
    {

        public TargetedComMessage() {
            super(MSG_TYPE_COM);
        }

        public TargetedComMessage(Long targetOid, Long subjectOid,
                int channel, String msgString)
        {
            super(MSG_TYPE_COM, targetOid, subjectOid);
            setChannel(channel);
            setString(msgString);
        }

        public String toString() {
            return "[ComMessage: targetOid=" + getTarget() +
                ", subjectOid=" + getSubject() + ", channel=" +
                getChannel() + ", msg=" + getString() + "]";
        }
        
        public String getString() {
            return msgString;
        }

        public void setString(String msgString) {
            this.msgString = msgString;
        }

        public int getChannel() {
            return channel;
        }

        public void setChannel(int channel) {
            this.channel = channel;
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(400);
            buf.putLong(this.getSubject());
            buf.putInt(3);
            buf.putInt(this.getChannel());
            buf.putString(this.getString());
            buf.flip();
            return buf;
        }

        public void fromBuffer(MVByteBuffer buf) {
            subject = buf.getLong();
            int msgNumber = buf.getInt();
            if (msgNumber != 3) {
                Log.error("ComMessage.fromBuffer: msgNumber " + msgNumber + " is not 3");
                return;
            }
            channel = buf.getInt();
            msgString = buf.getString();
        }

        int channel = -1;
        private String msgString;

        private static final long serialVersionUID = 1L;
    }

    /**
     * system message to be displayed in chat window for all players
     */
    public static class SysChatMessage extends Message implements ClientMessage {

        public SysChatMessage() {
            super(MSG_TYPE_SYS_CHAT);
        }

        public SysChatMessage(String msgString) {
            super(MSG_TYPE_SYS_CHAT);
            setString(msgString);
        }

        public String getString() {
            return msgString;
        }

        public void setString(String msgString) {
            this.msgString = msgString;
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(200);
            buf.putLong(-1);
            buf.putInt(3);
            buf.putInt(0);
            buf.putString(this.getString());
            buf.flip();
            return buf;
        }

        private String msgString;

        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from proxy
     */
    public static class OrientReqMessage extends SubjectMessage {

        public OrientReqMessage() {
            super(MSG_TYPE_ORIENT_REQ);
        }

        public OrientReqMessage(Long objOid, Quaternion q) {
            super(MSG_TYPE_ORIENT_REQ, objOid);
            setQuaternion(q);
        }

        public Quaternion getQuaternion() {
            return q;
        }

        public void setQuaternion(Quaternion q) {
            this.q = q;
        }

        Quaternion q;
        
        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from worldserver
     */
    public static class OrientMessage extends SubjectMessage implements
            ClientMessage {

        public OrientMessage() {
            super(MSG_TYPE_ORIENT);
        }

        public OrientMessage(Long objOid, Quaternion q) {
            super(MSG_TYPE_ORIENT, objOid);
            setQuaternion(q);
        }

        public Quaternion getQuaternion() {
            return q;
        }

        public void setQuaternion(Quaternion q) {
            this.q = q;
        }

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(32);
            buf.putLong(getSubject());
            buf.putInt(9);
            buf.putQuaternion(getQuaternion());
            buf.flip();
            return buf;
        }

        private Quaternion q;

        private static final long serialVersionUID = 1L;
    }

    /**
     * notification from world server regarding animation. copies the animation
     * list when setting/getting MSG_OID set to the notifyOid
     */
    public static class AnimationMessage extends SubjectMessage {

        public AnimationMessage() {
            super(MSG_TYPE_ANIMATION);
            setupTransient();
        }

        public AnimationMessage(Long objOid, Long notifyOid, AnimationCommand anim) {
            super(MSG_TYPE_ANIMATION, objOid);
            setupTransient();
            List<AnimationCommand> l = new LinkedList<AnimationCommand>();
            l.add(anim);
            setAnimationList(l);
        }

        public AnimationMessage(Long objOid, List<AnimationCommand> animList) {
            super(MSG_TYPE_ANIMATION, objOid);
            setupTransient();
            setAnimationList(animList);
        }

        public List<AnimationCommand> getAnimationList() {
            lock.lock();
            try {
                return new LinkedList<AnimationCommand>(animationList);
            } finally {
                lock.unlock();
            }
        }

        public void setAnimationList(List<AnimationCommand> animList) {
            lock.lock();
            try {
                animationList = new LinkedList<AnimationCommand>(animList);
            } finally {
                lock.unlock();
            }
        }

        void setupTransient() {
            lock = LockFactory.makeLock("AnimationMessageLock");
        }

        private LinkedList<AnimationCommand> animationList;
        
        transient protected Lock lock = null;
        private static final long serialVersionUID = 1L;
    }

    /**
     * general property about an obj/mob this is a targeted state, such as
     * whether a quest is available generalized states should use
     * PropertyMessage
     */
    public static class TargetedPropertyMessage extends TargetMessage
        implements Serializable
    {

        public TargetedPropertyMessage() {
            super(MSG_TYPE_TARGETED_PROPERTY);
            setupTransient();
        }
        
        public TargetedPropertyMessage(MessageType msgType) {
            super(msgType);
            setupTransient();
        }
        
        public TargetedPropertyMessage(MessageType msgType, Long target) {
            super(msgType, target);
            setupTransient();
        }

        public TargetedPropertyMessage(Long target, Long subject) {
            super(MSG_TYPE_TARGETED_PROPERTY, target, subject);
            setupTransient();
        } 
        
        public TargetedPropertyMessage(MessageType msgType, Long target, Long subject) {
            super(msgType, target, subject);
            setupTransient();   
        }

        /**
         * Associate the value with the key.
         * @deprecated Use {@link #setProperty(String key, Serializable val)} instead
         */
        public void put(String key, Serializable val) {
            setProperty(key, val);
        }
    
        /**
         * Associate the value with the key.
         * @param key A String key.
         * @param val A Serializable value.
         */
        public void setProperty(String key, Serializable val) {
            lock.lock();
            try {
                propertyMap.put(key, val);
            } finally {
                lock.unlock();
            }
        }

        /**
         * Get the value associated with a key.
         * @deprecated Use {@link #getProperty(String key)} instead
         */
        public Serializable get(String key) {
            return getProperty(key);
        }

        /**
         * Return the value associated with a key.
         * @param key A String key.
         * @return The Serializable value associated with the key, or null if none exists.
         */
        public Serializable getProperty(String key) {
            lock.lock();
            try {
                return propertyMap.get(key);
            } finally {
                lock.unlock();
            }
        }

        public Set<String> keySet() {
            lock.lock();
            try {
                return propertyMap.keySet();
            } finally {
                lock.unlock();
            }
        }

        public Map<String,Serializable> getPropertyMapRef()
        {
            return propertyMap;
        }

        public MVByteBuffer toBuffer(String version) {
            return toBufferInternal(version, null);
        }
        
        public MVByteBuffer toBuffer(String version, Set<String> filteredProps) {
            return toBufferInternal(version, filteredProps);
        }

        public MVByteBuffer toBufferInternal(String version, Set<String> filteredProps) {
            lock.lock();
            try {
                MVByteBuffer buf = new MVByteBuffer(2048);
                buf.putLong(getSubject());
                buf.putInt(62);
                buf.putFilteredPropertyMap(propertyMap, filteredProps);
                buf.flip();
                return buf;
            } finally {
                lock.unlock();
            }
        }

        void setupTransient() {
            lock = LockFactory.makeLock("TargetedPropertyMessageLock");
        }

        transient protected Lock lock = null;

        Map<String, Serializable> propertyMap = new HashMap<String, Serializable>();

        private static final long serialVersionUID = 1L;
    }

    /** ExtensionMessage about an object (the subject).  Generally sent
        to clients that can perceive the message subject.  The
        default message type is MSG_TYPE_EXTENSION.
     */
    public static class ExtensionMessage
        extends multiverse.server.messages.PropertyMessage
    {

        public ExtensionMessage() {
            super(MSG_TYPE_EXTENSION);
        }

        /** Create ExtensionMessage.
            @param objOid Message subject.
        */
        public ExtensionMessage(Long objOid) {
            super(MSG_TYPE_EXTENSION, objOid);
        }

        /** Create ExtensionMessage.
            @param msgType Message type.
            @param subType Extension message sub-type.
            @param objOid Message subject.
        */
        public ExtensionMessage(MessageType msgType, String subType, Long objOid) {
            super(msgType, objOid);
            setExtensionType(subType);
        }

        /** Create ExtensionMessage.
            @param msgType Message type.
            @param objOid Message subject.
            @param propertyMap Message properties.
        */
        public ExtensionMessage(MessageType msgType, Long objOid, Map<String, Serializable> propertyMap) {
            super(msgType, objOid);
            this.propertyMap = propertyMap;
        }

        /** Set the extension message sub-type.  The sub-type is used
            when an extension message is sent to the client.
        */
        public void setExtensionType(String type) {
            setProperty("ext_msg_subtype", type);
        }

        /** Get the extension message sub-type.
        */
        public String getExtensionType() {
            return (String) getProperty("ext_msg_subtype");
        }

        /*
         * Omit the target, because in all cases this is going to the
         * client specified by the targetOid.
         */
        public MVByteBuffer toBuffer(String version) {
            lock.lock();
            try {
                MVByteBuffer buf = new MVByteBuffer(2048);
                buf.putLong(getSubject());
                buf.putInt(83);
                byte flags = 0;
                buf.putByte(flags);
                buf.putPropertyMap(propertyMap);
                buf.flip();
                return buf;
            } finally {
                lock.unlock();
            }
        }
        
        private static final long serialVersionUID = 1L;
    }
    
    /** TargetedExtensionMessage is an extension message sent to a
        specific object (the target).  If the target is a player, then
        the message can be sent to the player's client.
        The default message type is MSG_TYPE_EXTENSION.
     */
    public static class TargetedExtensionMessage extends TargetedPropertyMessage
    {

        public TargetedExtensionMessage() {
            super(MSG_TYPE_EXTENSION);
        }

        /** Create TargetedExtensionMessage.
            @param target Message target.
        */
        public TargetedExtensionMessage(Long target) {
            super(MSG_TYPE_EXTENSION, target);
        }

        /** Create TargetedExtensionMessage.
            @param target Message target.
            @param subject Message subject.
        */
        public TargetedExtensionMessage(Long target, Long subject) {
            super(MSG_TYPE_EXTENSION, target, subject);
        }

        /** Create TargetedExtensionMessage.
            @param subType Extension message sub-type.
            @param target Message target.
        */
        public TargetedExtensionMessage(String subType, Long target) {
            super(MSG_TYPE_EXTENSION, target);
            setExtensionType(subType);
        }

        /** Create TargetedExtensionMessage.
            @param msgType Message type.
            @param subType Extension message sub-type.
            @param target Message target.
            @param subject Message subject.
        */
        public TargetedExtensionMessage(MessageType msgType, String subType,
            Long target, Long subject)
        {
            super(msgType, target, subject);
            if (subType != null)
                setExtensionType(subType);
        }

        /** Create TargetedExtensionMessage.
            @param msgType Message type.
            @param target Message target.
            @param subject Message subject.
            @param clientTargeted True if the message is sent from one client
                to another client (a P2P message).
            @param propertyMap Message properties.
        */
        public TargetedExtensionMessage(MessageType msgType,
            Long target, Long subject,
            Boolean clientTargeted, Map<String, Serializable> propertyMap)
        {
            super(msgType, target, subject);
            this.clientTargeted = clientTargeted;
            this.propertyMap = propertyMap;
        }

        /** Set the extension message sub-type.  The sub-type is used
            when an extension message is sent to the client.
        */
        public void setExtensionType(String type) {
            setProperty("ext_msg_subtype", type);
        }

        /** Get the extension message sub-type.
        */
        public String getExtensionType() {
            return (String) getProperty("ext_msg_subtype");
        }
        
        public Set<String> getKeys() {
            return propertyMap.keySet();
        }
        
        /*
         * Omit the target, because in all cases this is going to the
         * client specified by the targetOid.
         */
        public MVByteBuffer toBuffer(String version) {
            lock.lock();
            try {
                MVByteBuffer buf = new MVByteBuffer(2048);
                buf.putLong(getSubject());
                buf.putInt(83);
                Long oid = getTarget();
                byte flags = (byte)(((oid != null) ? 1 : 0) | (clientTargeted ? 2 : 0));
                buf.putByte(flags);
                if (oid != null)
                    buf.putLong(oid);
                buf.putPropertyMap(propertyMap);
                buf.flip();
                return buf;
            } finally {
                lock.unlock();
            }
        }
        
        private boolean clientTargeted = false;

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * messeage requesting the world manager to send an updateworldnode message
     * to give everyone an update
     */
    public static class RefreshWNodeMessage extends SubjectMessage {

        public RefreshWNodeMessage() {
            super(MSG_TYPE_REFRESH_WNODE);
        }

        public RefreshWNodeMessage(Long objOid) {
            super(MSG_TYPE_REFRESH_WNODE, objOid);
        }

        private static final long serialVersionUID = 1L;
    }

    /**
     * usually from world server, when it first comes up sends this message out
     * to announce its perceiver region currently the message is limited to 1
     * region.
     */
    public static class PerceiverRegionsMessage extends Message implements ITargetSessionId {

        public PerceiverRegionsMessage() {
            super(MSG_TYPE_PERCEIVER_REGIONS);
        }

        public PerceiverRegionsMessage(long instanceOid, Geometry region) {
            super();
            setMsgType(MSG_TYPE_PERCEIVER_REGIONS);
            setInstanceOid(instanceOid);
            setRegion(region);
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setRegion(Geometry g) {
            this.region = g;
        }

        public Geometry getRegion() {
            return region;
        }

        public String getTargetSessionId() {
            return targetSessionId;
        }
    
        public void setTargetSessionId(String targetSessionId) {
            this.targetSessionId = targetSessionId;
        }

        private long instanceOid;
        private Geometry region = null;
        private String targetSessionId;

        private static final long serialVersionUID = 1L;
    }

    public static class NewRegionMessage extends Message {

        public NewRegionMessage() {
            super(MSG_TYPE_NEW_REGION);
        }

        public NewRegionMessage(long instanceOid, Region region) {
            super(MSG_TYPE_NEW_REGION);
            setInstanceOid(instanceOid);
            setRegion(region);
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }
        public long getInstanceOid() {
            return instanceOid;
        }

        public void setRegion(Region r) {
            this.region = r;
        }

        public Region getRegion() {
            return this.region;
        }

        private long instanceOid;
        private Region region;

        private static final long serialVersionUID = 1L;
    }

    /**
     * Control sounds played on the client.  A sound message is a set of
     * sound start commands and sound stop commands.  Additionally, the
     * message may have a clear all sound flag.  The sounds are processed
     * in the order: clear flag, stop commands, start commands.
     *
     * A sound message describes either point sounds or ambient sounds.
     * Point sounds emit from a single object and attentuate over
     * distance.  Ambient sounds have no source object and do not
     * attenuate.
     *
     * The 'add' methods start sounds, the 'remove' methods
     * stop sounds.
     *
     * Start sounds are described with a {@link SoundData} object.
     * Stop sounds are identified by their (file) name.
     * 
     */
    public static class SoundMessage extends SubjectMessage
        implements ClientMessage, HasTarget
    {

        public SoundMessage() {
            super(MSG_TYPE_SOUND);
        }

        public SoundMessage(Long oid) {
            super(MSG_TYPE_SOUND, oid);
	    setType(soundTypePoint);
        }

        public long getTarget()
        {
            return target;
        }

        public void setTarget(long target)
        {
            this.target = target;
        }

        public static final byte soundTypePoint = (byte)1;
        public static final byte soundTypeAmbient = (byte)2;
        

	/** Set the sounds to start.
	*/
        public void setSoundData(List<SoundData> soundData) {
	    this.soundData= new LinkedList<SoundData>(soundData);
	}
	/** Get the sounds to start.
	*/
	public List<SoundData> getSoundData() {
	    return soundData;
	}
	/** Add a sound to start.
	*/
        public void addSound(SoundData data) {
	    if (soundData == null)
		soundData = new LinkedList<SoundData>();
	    soundData.add(data);
	}

        byte soundType;

	/** Set the sound message type.
	*/
        public void setType(byte type) {
	    this.soundType= type;
        }

	/** Get the sound message type.
	*/
	public byte getType() {
	    return soundType;
	}

        /** Set the clear flag.  Current sounds are cleared before starting new
	 * sounds.
         */
        public void setClearFlag(boolean val) {
            this.clearFlag = val;
        }

	/** Get the clear flag.
	*/
        public boolean getClearFlag() {
            return this.clearFlag;
        }

	/** Add a sound.
	    @param fileName the sound name
	    @param looping start a looping sound.  Sets the "Loop"
	    property to "true" or "false".
	*/
        public void addSound(String fileName, boolean looping) {
	    addSound(fileName,looping,(float)1.0);
        }

	/** Add a sound.
	    @param fileName the sound name
	    @param looping start a looping sound.  Sets the "Loop"
	    property to "true" or "false".
	    @param gain the initial sound gain (volume).  Ranges from 0.0
	    to 1.0.
	*/
        public void addSound(String fileName, boolean looping, float gain) {
	    if (soundData == null)
		soundData = new LinkedList<SoundData>();
	    HashMap<String,String> properties = new HashMap<String,String>();
	    if (looping)
		properties.put("Loop","true");
	    else
		properties.put("Loop","false");
	    properties.put("Gain",""+gain);
	    SoundData data = new SoundData(fileName,"Positional", properties);
	    soundData.add(data);
        }

        /** Stop a sound.
	*/
	public void removeSound(String fileName) {
	    if (soundOff == null)
		soundOff= new LinkedList<String>();
	    soundOff.add(fileName);
	}

	public String toString() {
	    String typeStr="POINT";
	    if (soundType == soundTypeAmbient)
	        typeStr="AMBIENT";
	    return "[SoundMessage: OID=" + getSubject() +
		", TYPE=" + typeStr +
		", ON=" + soundData +
		", OFF=" + soundOff +
		", CLEAR=" + clearFlag;
	}

        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(400);
	    if (soundType == soundTypeAmbient)
		buf.putLong(0L);
	    else
		buf.putLong(getSubject());
            buf.putInt(78);

            if (Log.loggingDebug)
                Log.debug("sending SoundControl: " + this);

            try {
                boolean cflag = getClearFlag();
                int numEntries = 0;
		if (soundData != null)
		    numEntries= soundData.size();
                if (cflag)
                    numEntries++;
		else if (soundOff != null)
		    numEntries+= soundOff.size();
                buf.putInt(numEntries);

                if (cflag) {
                    buf.putString("clear");
                }
		else if (soundOff != null) {
		    for (String fileName : soundOff) {
			buf.putString("off");
			buf.putString(fileName);
		    }
		}

		if (soundData != null)  {
		    for (SoundData data : soundData) {
			buf.putString("on");
			buf.putString(data.getFileName());
			Map<String,Serializable> props= new HashMap<String, Serializable>(data.getProperties());
			buf.putPropertyMap(props);
		    }
		}
            } finally {
            }
            buf.flip();
            return buf;
	}

	List<SoundData> soundData = null;
	List<String> soundOff = null;

        private boolean clearFlag;
        private long target = -1;

        private static final long serialVersionUID = 1L;
    }

    /**
     * Modify object display context message.
     */
    public static class ModifyDisplayContextMessage extends SubjectMessage {

        public ModifyDisplayContextMessage() {
            super(MSG_TYPE_MODIFY_DC);
        }

        public ModifyDisplayContextMessage(Long oid,
                byte action, String base,
                Collection<DisplayContext.Submesh> submeshes,
                String childDCHandle, DisplayContext childDC) {
            super(MSG_TYPE_MODIFY_DC, oid);
            setAction(action);
            setBase(base);
            setSubmeshes(submeshes);
            setChildDCHandle(childDCHandle);
            setChildDC(childDC);
        }

        public ModifyDisplayContextMessage(Long oid,
                byte action, Collection<DisplayContext.Submesh> submeshes) {
            super(MSG_TYPE_MODIFY_DC, oid);
            setAction(action);
            setSubmeshes(submeshes);
        }

        public ModifyDisplayContextMessage(Long oid,
                byte action, DisplayContext.Submesh submesh) {
            super(MSG_TYPE_MODIFY_DC, oid);
            setAction(action);

            List<DisplayContext.Submesh> l = new LinkedList<DisplayContext.Submesh>();
            l.add(submesh);

            // we dont need to call setSubmeshes since we made the list obj
            // (ie, we dont need to copy it)
            this.submeshes = l;
        }

        public void setAction(byte action) {
            this.action = action;
        }

        public byte getAction() {
            return action;
        }

        public void setBase(String base) {
            this.base = base;
        }

        public String getBase() {
            return base;
        }

        public void setSubmeshes(Collection<DisplayContext.Submesh> submeshes) {
            lock.lock();
            try {
                if (submeshes != null) {
                    this.submeshes = new LinkedList<DisplayContext.Submesh>(
                            submeshes);
                }
            } finally {
                lock.unlock();
            }
        }

        public List<DisplayContext.Submesh> getSubmeshes() {
            lock.lock();
            try {
                if (this.submeshes == null) {
                    return null;
                }
                return new LinkedList<DisplayContext.Submesh>(this.submeshes);
            } finally {
                lock.unlock();
            }
        }

        public void setChildDCHandle(String handle) {
            this.handle = handle;
        }

        public String getChildDCHandle() {
            return this.handle;
        }

        String handle;

        public void setChildDC(DisplayContext dc) {
            this.childDC = dc;
        }

        public DisplayContext getChildDC() {
            return this.childDC;
        }

        DisplayContext childDC = null;

        byte action;

        String base = null;

        List<DisplayContext.Submesh> submeshes;

        transient Lock lock = LockFactory.makeLock("ModifyDCMsgLock");

        private static final long serialVersionUID = 1L;
    }
    
    /**
     * world server is telling an object about a dir light
     * @author cedeno
     *
     */
    public static class NewDirLightMessage extends TargetMessage {

        public NewDirLightMessage() {
            super(MSG_TYPE_NEW_DIRLIGHT);
        }

        public NewDirLightMessage(Long objOid, Long lightOid, LightData lightData) {
            super(MSG_TYPE_NEW_DIRLIGHT, objOid, lightOid);
            setLightData(lightData);
        }
        
        public void setLightData(LightData lightData) {
            this.lightData = lightData;
        }
        public LightData getLightData() {
            return lightData;
        }

        private LightData lightData;
        
        private static final long serialVersionUID = 1L;
    }    

    public static class FreeObjectMessage extends TargetMessage
        implements ClientMessage
    {
        public FreeObjectMessage()
        {
        }

        public FreeObjectMessage(Long playerOid, Long objOid)
        {
            super(MSG_TYPE_FREE_OBJECT, playerOid, objOid);
        }

        public MVByteBuffer toBuffer()
        {
            MVByteBuffer buf = new MVByteBuffer(24);
            buf.putLong(getTarget());
            buf.putInt(10);
            buf.putLong(getSubject());
            buf.flip();
            return buf;
        }

        private static final long serialVersionUID = 1L;
    }


    /**
     * This message is constructed by the mobserver, and sent to the
     * world manager and the clients.  It provides the information
     * necessary for any of these guys to interpolate a mob path.
     *
     * The message attributes are:
     *   o The mob oid
     *   o The kind of interpolation, either "linear" or "spline"
     *   o A string with one character for each path segment, saying 
     *     if the path segment is over terrain or not.
     *   o I sequence of path points, one more than the length of the 
     *     string in characters.
     *
     * @author stryker
     *
     */
    public static class MobPathReqMessage extends MobPathMessageBaseClass {

        public MobPathReqMessage() {
            super();
        }
        
        public MobPathReqMessage(Long oid, long startTime, String interpKind, float speed, 
             String terrainString, List<Point> pathPoints) {
            super(oid, startTime, interpKind, speed, terrainString, pathPoints);
        }
        
        // This overloading is used to cancel a path when we're
        // arrived at the destination
        public MobPathReqMessage(Long oid) {
            super(oid, (long)0, "linear", 0f, "", new LinkedList<Point>());
        }
        
        protected MessageType getMobPathMsgType() {
            return MSG_TYPE_MOB_PATH_REQ;
        }
        
        protected String getMobPathMsgTypeTitle() {
            return "MobPathMessageReq";
        }

        private static final long serialVersionUID = 1L;
    }
    
    public static class MobPathMessage extends MobPathMessageBaseClass {

        public MobPathMessage() {
            super();
        }
        
        public MobPathMessage(Long oid, long startTime, String interpKind, float speed, 
             String terrainString, List<Point> pathPoints) {
            super(oid, startTime, interpKind, speed, terrainString, pathPoints);
        }
        
        protected MessageType getMobPathMsgType() {
            return MSG_TYPE_MOB_PATH;
        }
        
        protected String getMobPathMsgTypeTitle() {
            return "MobPathMessage";
        }

        // The path has expired if the start time plus the time on the
        // path is earlier than now
        public boolean pathExpired() {
            if (pathPoints == null || pathPoints.size() < 2)
                return true;
            float pathTime = 0f;
            Point curr = pathPoints.get(0);
            for (int i=1; i<pathPoints.size(); i++) {
                Point next = pathPoints.get(i);
                float dist = Point.distanceTo(curr, next);
                float diffTime = dist / speed;
                pathTime += diffTime;
                curr = next;
            }
            return (startTime + pathTime < System.currentTimeMillis());
        }
        
        private static final long serialVersionUID = 1L;
    }

    public static class MobPathCorrectionMessage extends MobPathMessageBaseClass {

        public MobPathCorrectionMessage() {
            super();
        }
        
        public MobPathCorrectionMessage(Long oid, long startTime, String interpKind, float speed, 
             String terrainString, List<Point> pathPoints) {
            super(oid, startTime, interpKind, speed, terrainString, pathPoints);
        }
        
        protected MessageType getMobPathMsgType() {
            return MSG_TYPE_MOB_PATH_CORRECTION;
        }
        
        protected String getMobPathMsgTypeTitle() {
            return "MobPathCorrectionMessage";
        }

        private static final long serialVersionUID = 1L;
    }


    // A base class for MobPathReqMessage and MobPathMessage
    abstract public static class MobPathMessageBaseClass extends SubjectMessage /* implements ClientMessage */ {

        abstract protected MessageType getMobPathMsgType();
        abstract protected String getMobPathMsgTypeTitle();

        public MobPathMessageBaseClass() {
            super();
        }
        
        public MobPathMessageBaseClass(Long oid, Long startTime, String interpKind, float speed, 
                                       String terrainString, List<Point> pathPoints) {
            super();
            setMsgType(getMobPathMsgType());
            setSubject(oid);
            setStartTime(startTime);
            setInterpKind(interpKind);
            setSpeed(speed);
            setTerrainString(terrainString);
            setPathPoints(pathPoints);
        }

        public String toString() {
            return "[" + getMobPathMsgTypeTitle() + " oid=" + getSubject() + ", interpKind=" + interpKind + ", speed=" + speed +
                ", terrainString=" + terrainString + ", pathPoints=" + getPathPoints() + ", super=" + super.toString() + "]";
        }
        
        public void setStartTime(long startTime) {
            this.startTime = startTime;
        }

        public long getStartTime() {
            return startTime;
        }
        
        public void setInterpKind(String interpKind) {
            this.interpKind = interpKind;
        }

        public String getInterpKind() {
            return interpKind;
        }

        public void setSpeed(float speed) {
            this.speed = speed;
        }

        public float getSpeed() {
            return speed;
        }

        public void setTerrainString(String terrainString) {
            this.terrainString = terrainString;
        }

        public String getTerrainString() {
            return terrainString;
        }

        public void setPathPoints(List<Point> pathPoints) {
            this.pathPoints = pathPoints;
        }

        public List<Point> getPathPoints() {
            return pathPoints;
        }

        /**
         * Returns the position of the mob at the given time.
         * @param when The time for which the position should be returned.
         * @return The Point position at the given time.  If there are
         * no pathPoints, returns null.  If there is just one path
         * point, returns that point.  If the time is before the
         * startTime, returns the first point, and if the time is
         * greater than the time of the last point, returns the last
         * point.
         */
        public Point getPositionAtTime(Long when) {
            if (pathPoints == null || pathPoints.size() == 0)
                return null;
            else if (when <= startTime)
                return pathPoints.get(0);
            else {
                PathInterpolator interp = (interpKind == "linear" ?
                    new PathLinear(0L, startTime, speed, terrainString, pathPoints) :
                    new PathSpline(0L, startTime, speed, terrainString, pathPoints));
                PathLocAndDir locAndDir = interp.interpolate(when);
                if (locAndDir == null)
                    return pathPoints.get(pathPoints.size() - 1);
                else
                    return locAndDir.getLoc();
            }
        }
        
        public MVByteBuffer toBuffer() {
            MVByteBuffer buf = new MVByteBuffer(400);
            buf.putLong(getSubject());
            buf.putInt(73);
            buf.putLong(startTime);
            buf.putString(interpKind);
            buf.putFloat(speed);
            buf.putString(terrainString);
            buf.putInt(pathPoints.size());
            for (Point point : pathPoints) {
                buf.putPoint(point);
            }
            buf.flip();
            return buf;
        }
        
        long startTime;
        String interpKind = "linear";
        float speed;
        String terrainString = "";
        List<Point> pathPoints = null;
    }
    
    public static class LoadSubObjectMessage
        extends ObjectManagerClient.LoadSubObjectMessage
    {
        public LoadSubObjectMessage() {
            super();
        }

        public LoadSubObjectMessage(long oid, Namespace namespace,
                Point location, long instanceOid) {
            super(oid,namespace);
            this.location = location;
            this.instanceOid = instanceOid;
        }
 
        public Point getLocation() {
            return location;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        private Point location;
        private long instanceOid;

        private static final long serialVersionUID = 1L;
    }

    public static class HostInstanceMessage extends Message
    {
        public HostInstanceMessage()
        {
        }

        public HostInstanceMessage(String pluginName, long instanceOid)
        {
            super(MSG_TYPE_HOST_INSTANCE);
            setPluginName(pluginName);
            setInstanceOid(instanceOid);
        }

        public String getPluginName() {
            return pluginName;
        }

        public void setPluginName(String name) {
            pluginName = name;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setInstanceOid(long instanceOid) {
            this.instanceOid = instanceOid;
        }

        private String pluginName;
        private long instanceOid;
        
        private static final long serialVersionUID = 1L;
    }

    public static class PlayerPathWMReqMessage extends Message {

        public PlayerPathWMReqMessage() {
            super(MSG_TYPE_PLAYER_PATH_WM_REQ);
        }

        public PlayerPathWMReqMessage(long playerOid, long instanceOid, String roomId,
            MVVector start, float speed, Quaternion startOrientation,
            MVVector dest, Quaternion destOrientation, List<MVVector> boundary, 
            List<List<MVVector>> obstacles, float avatarWidth) {
            super(MSG_TYPE_PLAYER_PATH_WM_REQ);
            this.playerOid = playerOid;
            this.instanceOid = instanceOid;
            this.start = start;
            this.speed = speed;
            this.startOrientation = startOrientation;
            this.dest = dest;
            this.destOrientation = destOrientation;
            this.boundary = boundary;
            this.obstacles = obstacles;
            this.avatarWidth = avatarWidth;
        }
        
        public float getAvatarWidth() {
            return avatarWidth;
        }

        public void setAvatarWidth(float avatarWidth) {
            this.avatarWidth = avatarWidth;
        }

        public List<MVVector> getBoundary() {
            return boundary;
        }

        public void setBoundary(List<MVVector> boundary) {
            this.boundary = boundary;
        }

        public MVVector getDest() {
            return dest;
        }

        public void setDest(MVVector dest) {
            this.dest = dest;
        }

        public Quaternion getDestOrientation() {
            return destOrientation;
        }

        public void setDestOrientation(Quaternion destOrientation) {
            this.destOrientation = destOrientation;
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setInstanceOid(long instanceOid) {
            this.instanceOid = instanceOid;
        }

        public List<List<MVVector>> getObstacles() {
            return obstacles;
        }

        public void setObstacles(List<List<MVVector>> obstacles) {
            this.obstacles = obstacles;
        }

        public long getPlayerOid() {
            return playerOid;
        }

        public void setPlayerOid(long playerOid) {
            this.playerOid = playerOid;
        }

        public String getRoomId() {
            return roomId;
        }

        public void setRoomId(String roomId) {
            this.roomId = roomId;
        }

        public float getSpeed() {
            return speed;
        }

        public void setSpeed(float speed) {
            this.speed = speed;
        }

        public MVVector getStart() {
            return start;
        }

        public void setStart(MVVector start) {
            this.start = start;
        }

        public Quaternion getStartOrientation() {
            return startOrientation;
        }

        public void setStartOrientation(Quaternion startOrientation) {
            this.startOrientation = startOrientation;
        }

        private float avatarWidth;
        private List<MVVector> boundary;
        private MVVector dest;
        private Quaternion destOrientation;
        private long instanceOid;
        private List<List<MVVector>> obstacles;
        private long playerOid;
        private String roomId;
        private float speed;
        private MVVector start;
        private Quaternion startOrientation;
        
        private static final long serialVersionUID = 1L;
    }

    /**
     * transient property used by the world manager plugin to see when 
     * this object was last saved
     */
    public static final String WMGR_LAST_SAVED_PROP = (String)Entity.registerTransientPropertyKey("wmgr.lastSaved");
    
    /**
     * how often a world manager object is saved to the database
     */
    public static Long WMGR_SAVE_INTERVAL_MS = 60000L;
    
    // msg "type" field for binding an object request
    public static MessageType MSG_TYPE_NEW_DIRLIGHT = MessageType.intern("mv.NEW_DIRLIGHT");
    public static MessageType MSG_TYPE_FREE_OBJECT = MessageType.intern("mv.FREE_OBJECT");
    
    // world manager plugin sends this out, setting the ambient light
    public static MessageType MSG_TYPE_SET_AMBIENT = MessageType.intern("mv.SET_AMBIENT");
    
    public static MessageType MSG_TYPE_TERRAIN_REQ = MessageType.intern("mv.TERRAIN_REQ");

    public static MessageType MSG_TYPE_OBJINFO_REQ = MessageType.intern("mv.OBJINFO_REQ");

    public static MessageType MSG_TYPE_DC_REQ = MessageType.intern("mv.DC_REQ");

    public static MessageType MSG_TYPE_FOG = MessageType.intern("mv.FOG");

    // requesting spawn
    public static MessageType MSG_TYPE_SPAWN_REQ = MessageType.intern("mv.SPAWN_REQ");

    // spawn notification
    public static MessageType MSG_TYPE_SPAWNED = MessageType.intern("mv.SPAWNED");
    
    // requesting despawn
    public static MessageType MSG_TYPE_DESPAWN_REQ = MessageType.intern("mv.DESPAWN_REQ");

    // despawn notification
    public static MessageType MSG_TYPE_DESPAWNED = MessageType.intern("mv.DESPAWNED");
    
    public static MessageType MSG_TYPE_UPDATE_OBJECT = MessageType.intern("mv.UPDATE_OBJECT");

    public static MessageType MSG_TYPE_DISPLAY_CONTEXT = MessageType.intern("mv.DISPLAY_CONTEXT");

    public static MessageType MSG_TYPE_ANIMATION = MessageType.intern("mv.ANIMATION");

    public static MessageType MSG_TYPE_SETWNODE_REQ = MessageType.intern("mv.SETWNODE_REQ");

    public static MessageType MSG_TYPE_UPDATEWNODE_REQ = MessageType.intern("mv.UPDATEWNODE_REQ");

    public static MessageType MSG_TYPE_GETWNODE_REQ = MessageType.intern("mv.GETWNODE_REQ");

    public static MessageType MSG_TYPE_TARGETED_PROPERTY = MessageType.intern("mv.TARGETED_PROPERTY");

    public static MessageType MSG_TYPE_COM_REQ = MessageType.intern("mv.COM_REQ");

    public static MessageType MSG_TYPE_COM = MessageType.intern("mv.COM");

    public static MessageType MSG_TYPE_SYS_CHAT = MessageType.intern("mv.SYS_CHAT");

    public static MessageType MSG_TYPE_UPDATEWNODE = MessageType.intern("mv.UPDATEWNODE");

    public static MessageType MSG_TYPE_WNODECORRECT = MessageType.intern("mv.WNODECORRECT");

    public static MessageType MSG_TYPE_ORIENT_REQ = MessageType.intern("mv.ORIENT_REQ");

    public static MessageType MSG_TYPE_ORIENT = MessageType.intern("mv.ORIENT");

    public static MessageType MSG_TYPE_REFRESH_WNODE = MessageType.intern("mv.REFRESH_WNODE");

    public static MessageType MSG_TYPE_PERCEIVER_REGIONS = MessageType.intern("mv.PERCEIVER_REGIONS");

    public static MessageType MSG_TYPE_NEW_REMOTE_OBJ = MessageType.intern("mv.NEW_REMOTE_OBJ");

    public static MessageType MSG_TYPE_FREE_REMOTE_OBJ = MessageType.intern("mv.FREE_REMOTE_OBJ");

    public static MessageType MSG_TYPE_ROAD = MessageType.intern("mv.ROAD");

    public static MessageType MSG_TYPE_FREE_ROAD = MessageType.intern("mv.FREE_ROAD");

    public static MessageType MSG_TYPE_NEW_REGION = MessageType.intern("mv.NEW_REGION");

    public static MessageType MSG_TYPE_SOUND = MessageType.intern("mv.SOUND");

    public static MessageType MSG_TYPE_MODIFY_DC = MessageType.intern("mv.MODIFY_DC");

    public static MessageType MSG_TYPE_DETACH = MessageType.intern("mv.DETACH");

    public static MessageType MSG_TYPE_REPARENT_WNODE_REQ = MessageType.intern("mv.REPARENT_WNODE_REQ");
    
    public static MessageType MSG_TYPE_EXTENSION = MessageType.intern("mv.EXTENSION");
    
    public static MessageType MSG_TYPE_MOB_PATH = MessageType.intern("mv.MOB_PATH");
    
    public static MessageType MSG_TYPE_MOB_PATH_REQ = MessageType.intern("mv.MOB_PATH_REQ");
    
    public static MessageType MSG_TYPE_MOB_PATH_CORRECTION = MessageType.intern("mv.MOB_PATH_CORRECTION");

    public static MessageType MSG_TYPE_DIR_LOC_ORIENT = MessageType.intern("mv.DIR_LOC_ORIENT");

    public static MessageType MSG_TYPE_PERCEPTION = MessageType.intern("mv.PERCEPTION");
    public static MessageType MSG_TYPE_PERCEPTION_INFO = MessageType.intern("mv.PERCEPTION_INFO");
    
    public static MessageType MSG_TYPE_P2P_EXTENSION = MessageType.intern("mv.P2P_EXTENSION");
    public static MessageType MSG_TYPE_HOST_INSTANCE = MessageType.intern("mv.HOST_INSTANCE");

    public static MessageType MSG_TYPE_PLAYER_PATH_WM_REQ = MessageType.intern("mv.PLAYER_PATH_WM_REQ");

    public final static String WORLD_PROP_NOMOVE = "world.nomove";

    public final static String WORLD_PROP_NOTURN = "world.noturn";
    
    /**
     * location, as specified by templates for the world manager plugin
     */
    public static String TEMPL_LOC = ":loc";
    public static String TEMPL_INSTANCE = ":instance";
    
    public static String TEMPL_NAME = ":entityName";
    public static String TEMPL_ORIENT = ":orient";
    
    public static String TEMPL_SCALE = ":scale";
    public static String TEMPL_PERCEPTION_RADIUS = ":percRadius";
    
    public static String TEMPL_FOLLOWS_TERRAIN = ":followsTerrain";

    public static String TEMPL_RUN_THRESHOLD = "runThreshold";

    public static String TEMPL_TERRAIN_DECAL_DATA = "terrainDecal";
    public static String TEMPL_SOUND_DATA_LIST = "soundData";
    /**
     * object type ("MOB", "USER", "ITEM", "STRUCT", "TDECAL")
     * "LIGHT" is not used
     */
    public static String TEMPL_OBJECT_TYPE = ":objType";
    public static ObjectType TEMPL_OBJECT_TYPE_MOB = ObjectTypes.mob;
    public static ObjectType TEMPL_OBJECT_TYPE_PLAYER = ObjectTypes.player;
    public static ObjectType TEMPL_OBJECT_TYPE_LIGHT = ObjectTypes.light;
    public static ObjectType TEMPL_OBJECT_TYPE_ITEM = ObjectTypes.item;
    public static ObjectType TEMPL_OBJECT_TYPE_STRUCTURE = ObjectTypes.structure;
    public static ObjectType TEMPL_OBJECT_TYPE_TERRAIN_DECAL = ObjectTypes.terrainDecal;
    public static ObjectType TEMPL_OBJECT_TYPE_POINT_SOUND = ObjectTypes.pointSound;

    /**
     * display context to use for the object
     */
    public static String TEMPL_DISPLAY_CONTEXT = ":displayContext";
    
    public static final String TEMPL_WORLDMGR_NAME = ":wmName";

    /**
     * Used in the Entity and ObjectInfo property maps; value is a
     * MobPathMessageBaseClass instance
     */
    public static String MOB_PATH_PROPERTY = "MobPathMsg";
    
    public static String MSG_PROP_LOC = "msgPropLoc"; // location property

    public static Namespace NAMESPACE = null;
    public static Namespace INSTANCE_NAMESPACE = null;
}
