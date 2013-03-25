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

import java.util.List;
import java.util.ArrayList;

import multiverse.msgsys.MessageType;
import multiverse.msgsys.Message;
import multiverse.msgsys.SubjectMessage;
import multiverse.msgsys.TargetMessage;
import multiverse.server.util.Log;
import multiverse.server.math.Point;


import multiverse.server.objects.*;
import multiverse.server.engine.TerrainConfig;
import multiverse.server.engine.Engine;
import multiverse.server.engine.Namespace;
import multiverse.server.engine.BasicWorldNode;

/** API for instance management.  An instance is created from an
instance template and optional override template.  The instance template
specifies the instance world file, initialization script, and load
script.  Instances may be persistent or non-persistent.  Persistent
instances can be unloaded and later loaded.  Persistent instances
will have the same oid when reloaded.
<p>
Note: Instance names may not be unique.
<p>
The initial instances should be created by a "post" script passed
to the InstancePlugin process.  If you're using the supplied
'multiverse.sh' or 'start-multiverse.bat', then the script is
named $MV_HOME/config/myworld/startup_instance.py.
<p>
Instance objects are searchable using
{@link multiverse.server.objects.ObjectTypes#instance ObjectTypes.instance} and
{@link multiverse.server.engine.PropertySearch PropertySearch}.  The
search result is a collection of java.util.Map objects containing the
selected properties.  Select properties by name in the SearchSelection.
If the selection mode is {@link multiverse.server.engine.SearchSelection#RESULT_KEY_ONLY SearchSelection.RESULT_KEY_ONLY}, then the
result is a collection of Long instance oids.  If the selection mode
is {@link multiverse.server.engine.SearchSelection#RESULT_KEYED SearchSelection.RESULT_KEYED}, the result is a collection of
SearchEntry where the key is the instance oid and the value is a
java.util.Map.
@see multiverse.server.engine.SearchManager#searchObjects
*/
public class InstanceClient
{
    /** Register an instance template.  The template specifies the
        instance world file, scripts, and custom properties.  An
        instance template is required to create an instance.
        <p>
        The supported properties are:
        <li>{@link InstanceClient#TEMPL_WORLD_FILE_NAME} -- name of the world file
        <li>{@link InstanceClient#TEMPL_INIT_SCRIPT_FILE_NAME} -- instance initialization script
        <li>{@link InstanceClient#TEMPL_LOAD_SCRIPT_FILE_NAME} -- load script
        The world file should be a {@code .mvw} file.  The init script is
        run once when the instance is created.  The load script is run
        each time a persistent instance is loaded after creation.
        <p>
        The file names undergo variable expansion prior to use.  The supported
        variables are:
        <ul>
        <li>$MV_HOME -- the value of MV_HOME environment variable
        <li>$WORLD_NAME -- the world name
        <li>$WORLD_DIR -- the world config directory: $MV_HOME/config/$WORLD_NAME
        </ul>
        @return True on success, false on failure
    */
    public static boolean registerInstanceTemplate(Template template)
    {
        RegisterInstanceTemplateMessage message =
            new RegisterInstanceTemplateMessage(template);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    /** Create an instance.  The instance properties are taken from the
        registered {@code templateName} merged with the
        {@code override}.  Properties in the override template take
        precedence over those in the registered template.  See
        {@link #registerInstanceTemplate(Template)}.
        <p>
        To make a persistent instance, set the persistent property in
        the registered or override template:
        <pre>
            Java:
            template.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, true)
            Python:
            template.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True))
        </pre>
        <p>
        Instance creation has the following steps:
        <ol>
        <li>Select world manager to host instance (currently hardcoded to
                "WorldManager1").
        <li>Generate instance object via object manager.  Sub-objects are
                created in the instance and world manager plugins.
        <li>Load world file into instance; structures, lights, etc.
        <li>Create world editor defined spawn generators
        <li>Run the instance init script ({@link InstanceClient#TEMPL_INIT_SCRIPT_FILE_NAME})
        </ol>
        @return Instance oid on success, null on failure
    */
    public static Long createInstance(String templateName, Template override)
    {
        CreateInstanceMessage message =
            new CreateInstanceMessage(templateName,override);
        return Engine.getAgent().sendRPCReturnLong(message);
    }

    /** Load a persistent instance.
        <p>
        Instance loading has the following steps:
        <ol>
        <li>Select world manager to host instance (currently hardcoded to
                "WorldManager1").
        <li>Load instance object via object manager.  Sub-objects are
                loaded in the instance and world manager plugins.
        <li>Load world file into instance; structures, lights, etc.
        <li>Load instance contents; persistent objects spawned in the instance
        <li>Create world editor defined spawn generators
        <li>Run the instance load script ({@link InstanceClient#TEMPL_LOAD_SCRIPT_FILE_NAME})
        </ol>
        @return {@link #RESULT_OK} on success, {@link #RESULT_ERROR_UNKNOWN_OBJECT} if the
            instanceOid does not exist, {@link #RESULT_ERROR_NO_WORLD_MANAGER}
            if there's no world manager to host the instance,
            {@link #RESULT_ERROR_RETRY} if the instance is in an intermediate
            state,
            and {@link #RESULT_ERROR_INTERNAL} for any
            other error.
    */
    public static int loadInstance(long instanceOid)
    {
        SubjectMessage message = new SubjectMessage(MSG_TYPE_LOAD_INSTANCE,
                instanceOid);
        return Engine.getAgent().sendRPCReturnInt(message);
    }

    /** Unload a persistent instance.  For a non-persistent instance, this
        is the same as deleting the instance.  Instance content (objects
        spawned in the instance) are automatically unloaded when the
        instance unloads.  However, players in the instance are not
        unloaded.
        <p>
        A MSG_TYPE_INSTANCE_UNLOADED SubjectMessage is published
        after the content is unloaded, but before the instance object
        is unloaded.  The message is published as a broadcast RPC and
        the caller waits for all subscribers to respond.
        <p>
        This operation is actually implemented in the object manager
        plugin.
        @return True on success, false on failure
    */
    public static boolean unloadInstance(long instanceOid)
    {
        SubjectMessage message = new SubjectMessage(MSG_TYPE_UNLOAD_INSTANCE,
                instanceOid);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    /** Delete an instance.  Instance content (objects
        spawned in the instance) are automatically deleted when the
        instance is deleted.  However, players in the instance are not
        deleted.  For a persistent instance, the object and content are
        deleted from the database.
        <p>
        A MSG_TYPE_INSTANCE_DELETED SubjectMessage is published
        after the content is deleted, but before the instance object
        is deleted.  The message is published as a broadcast RPC and
        the caller waits for all subscribers to respond.
        <p>
        This operation is actually implemented in the object manager
        plugin.
        @return True on success, false on failure
    */
    public static boolean deleteInstance(long instanceOid)
    {
        SubjectMessage message = new SubjectMessage(MSG_TYPE_DELETE_INSTANCE,
                instanceOid);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    /** Get instance oid from an instance name.  Instance names may
        not be unique.  In case of duplicate instance names, the
        selected instance is undefined.
        @return Oid on success, null on failure or unknown name.
    */
    public static Long getInstanceOid(String instanceName)
    {
        GetInstanceInfoMessage message =
            new GetInstanceInfoMessage(instanceName,
            InstanceClient.FLAG_OID);
        InstanceInfo info =
            (InstanceInfo) Engine.getAgent().sendRPCReturnObject(message);
        if (info != null || info.oid == null)
            return info.oid;
        else
            return null;
    }

    /** Get loaded instance oids from an instance name.
        Currently does not support getting unloaded instances.
        @return List of oids.  The list is empty if there are no instances
        matching the given name.
    */
    public static List<Long> getInstanceOids(String instanceName)
    {
        GetInstanceInfoMessage message =
            new GetInstanceInfoMessage(instanceName,
            InstanceClient.FLAG_OID | InstanceClient.FLAG_MULTIPLE);
        List<InstanceInfo> info =
            (List<InstanceInfo>) Engine.getAgent().sendRPCReturnObject(message);
        if (info != null) {
            List<Long> oids = new ArrayList<Long>(info.size());
            for (InstanceInfo ii : info)
                oids.add(ii.oid);
            return oids;
        }
        else
            return null;
    }

    /** Get instance oid. */
    public static final int FLAG_OID = 1 << 0;
    /** Get instance name. */
    public static final int FLAG_NAME = 1 << 1;
    /** Get instance template name. */
    public static final int FLAG_TEMPLATE_NAME = 1 << 2;
    /** Get instance skybox. */
    public static final int FLAG_SKYBOX = 1 << 3;
    /** Get instance fog. */
    public static final int FLAG_FOG = 1 << 4;
    /** Get instance ambient light. */
    public static final int FLAG_AMBIENT_LIGHT = 1 << 5;
    /** Get instance directional light. */
    public static final int FLAG_DIR_LIGHT = 1 << 6;
    /** Get instance ocean data. */
    public static final int FLAG_OCEAN = 1 << 7;
    /** Get instance terrain data. */
    public static final int FLAG_TERRAIN = 1 << 8;
    /** Get instance regions. */
    public static final int FLAG_REGION_CONFIG = 1 << 9;
    /** Get current instance player population. */
    public static final int FLAG_PLAYER_POPULATION = 1 << 10;
    /** Get list of InstanceInfo. */
    public static final int FLAG_MULTIPLE = 1 << 11;
    /** Get all available instance information. */
    public static final int FLAG_ALL_INFO = ~FLAG_MULTIPLE;

    /** Get instance information.  Information is selected by the
        {@code flags} parameter.  Information for unloaded instances
        is limited to the 'oid' and 'loaded' status.
        @param instanceOid Instance identifier.
        @param flags Bit-mask of the FLAG_* constants.
        @return Always returns an InstanceInfo.  InstanceInfo.oid will be
        null if the instance does not exist.
    */
    public static InstanceInfo getInstanceInfo(long instanceOid, int flags)
    {
        GetInstanceInfoMessage message =
            new GetInstanceInfoMessage(instanceOid, flags);
        return (InstanceInfo) Engine.getAgent().sendRPCReturnObject(message);
    }

    /** Get instance information by instance name.  Information is selected
        by the {@code flags} parameter.  Currently does not support getting
        information for unloaded instances.
        @param instanceName Instance name.
        @param flags Bit-mask of the FLAG_* constants.
        @return List of InstanceInfo, one for each loaded instance.  List
                will be empty if no instances of the given name are loaded.
    */
    public static List<InstanceInfo> getInstanceInfoByName(String instanceName,
        int flags)
    {
        GetInstanceInfoMessage message =
            new GetInstanceInfoMessage(instanceName, flags | FLAG_MULTIPLE);
        return (List<InstanceInfo>) Engine.getAgent().sendRPCReturnObject(message);
    }

    /** Get marker location. */
    public static final long MARKER_POINT = Marker.PROP_POINT;
    /** Get marker orientation. */
    public static final long MARKER_ORIENTATION = Marker.PROP_ORIENTATION;
    /** Get marker properties. */
    public static final long MARKER_PROPERTIES = Marker.PROP_PROPERTIES;
    /** Get all marker information. */
    public static final long MARKER_ALL = Marker.PROP_ALL;

    /** Get marker location and orientation from a loaded instance.
        @param instanceOid Instance identifier.
        @param markerName Marker name.
        @return Marker object or null if unknown instance or marker.
    */
    public static Marker getMarker(long instanceOid, String markerName)
    {
        return getMarker(instanceOid,markerName,MARKER_POINT|MARKER_ORIENTATION);
    }

    /** Get marker information from a loaded instance.  Information is
        selected by the {@code flags} parameter.
        @param instanceOid Instance identifier.
        @param markerName Marker name.
        @param flags Bit-mask of MARKER_* constants.
        @return Marker object or null if unknown instance or marker.
    */
    public static Marker getMarker(long instanceOid, String markerName,
        long flags)
    {
        //## need caching here, and a flag to enable
        GetMarkerMessage message =
            new GetMarkerMessage(instanceOid,markerName, flags);
        return (Marker)Engine.getAgent().sendRPCReturnObject(message);
    }

    /** Get marker location from a loaded instance.
        @param instanceOid Instance identifier.
        @param markerName Marker name.
        @return Marker object or null if unknown instance or marker.
    */
    public static Point getMarkerPoint(long instanceOid, String markerName)
    {
        Marker marker = getMarker(instanceOid,markerName,MARKER_POINT);
        if (marker != null)
            return marker.getPoint();
        else
            return null;
    }

    /** Get the region boundary (search selection flag). */
    public static final long REGION_BOUNDARY = Region.PROP_BOUNDARY;

    /** Get the region properties (search selection flag). */
    public static final long REGION_PROPERTIES = Region.PROP_PROPERTIES;

    /** Get all region information (search selection flag). */
    public static final long REGION_ALL = Region.PROP_ALL;

    /** Get region information from a loaded instance.  Information is
        selected by the {@code flags} parameter.
        @param instanceOid Instance identifier.
        @param regionName Region name.
        @param flags Bit-mask of REGION_* constants; {@link #REGION_BOUNDARY},
                {@link #REGION_PROPERTIES}, {@link #REGION_ALL}.
        @return Region object or null if unknown instance or region.
    */
    public static Region getRegion(long instanceOid, String regionName,
        long flags)
    {
        //## need caching here, and a flag to enable
        GetRegionMessage message =
            new GetRegionMessage(instanceOid,regionName, flags);
        return (Region)Engine.getAgent().sendRPCReturnObject(message);
    }

    /** Move an object to a different instance.  The destination
        instance, location, and orientation are specified in
        {@code instanceLoc}.  The destination instance must already be loaded.
        Passing {@link InstanceEntryReqMessage#FLAG_PUSH} for {@code flags} pushes the current instance
        and location onto the player's instance restore stack.  A
        subsequent instance entry with {@link InstanceEntryReqMessage#FLAG_POP} will instance back to
        this location.
        <p>
        Passing {@link InstanceEntryReqMessage#FLAG_POP} for {@code flags} removes the top entry from
        the player instance stack and moves the player to that instance
        and location.  The bottom of the instance restore stack is the
        fail-safe location and is never removed.
        <p>
        The destination instance may be the same as the current instance.
        <p>
        Blocks until the instance entry is complete.  Currently only
        supported for player objects.
        <p>
        See {@link InstanceEntryReqMessage} for additional instance
        entry options.
        @param oid Object identifier (only players are supported).
        @param instanceLoc Instance and location.
        @param flags One of FLAG_NONE, FLAG_POP, or FLAG_PUSH from
        @param restoreWnode Location to push onto instance restore stack
        {@link InstanceEntryReqMessage}.
        @return True on success, false on failure.
        @see InstanceEntryReqMessage
    */
    public static boolean objectInstanceEntry(long oid,
        BasicWorldNode instanceLoc, int flags, BasicWorldNode restoreWnode)
    {
        InstanceEntryReqMessage message = new InstanceEntryReqMessage(oid,
            instanceLoc, flags, restoreWnode);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    /** Move an object to a different instance.  See
        {@link #objectInstanceEntry(long,multiverse.server.engine.BasicWorldNode,int,multiverse.server.engine.BasicWorldNode) objectInstanceEntry()}.
    */
    public static boolean objectInstanceEntry(long oid,
        BasicWorldNode instanceLoc, int flags)
    {
        return objectInstanceEntry(oid, instanceLoc, flags, null);
    }

    /** Move an object to a different instance.  The instance is
        identified by name (however instance names may not be
        unique).
        @see #objectInstanceEntry(long,BasicWorldNode,int)
        @see InstanceEntryReqMessage
    */
    public static boolean objectInstanceEntry(long oid, String instanceName,
        BasicWorldNode instanceLoc, int flags)
    {
        Long instanceOid = getInstanceOid(instanceName);
        if (instanceOid == null) {
            Log.error("objectInstanceEntry: unknown instance name="+
                instanceName + " for oid="+oid);
            return false;
        }
        instanceLoc.setInstanceOid(instanceOid);
        InstanceEntryReqMessage message = new InstanceEntryReqMessage(oid,
            instanceLoc, flags);
        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    public static final int RESULT_OK = 0;
    public static final int RESULT_ERROR_UNKNOWN_OBJECT = -1;
    public static final int RESULT_ERROR_INTERNAL = -2;
    public static final int RESULT_ERROR_NO_WORLD_MANAGER = -3;
    public static final int RESULT_ERROR_RETRY = -4;


    public static class RegisterInstanceTemplateMessage extends Message {
        public RegisterInstanceTemplateMessage()
        {
            super(MSG_TYPE_REGISTER_INSTANCE_TEMPLATE);
        }

        public RegisterInstanceTemplateMessage(Template template)
        {
            super(MSG_TYPE_REGISTER_INSTANCE_TEMPLATE);
            this.template = template;
        }

        public Template getTemplate()
        {
            return template;
        }

        public void setTemplate(Template template)
        {
            this.template = template;
        }

        Template template;
        
        private static final long serialVersionUID = 1L;
    }

    public static class CreateInstanceMessage extends Message {
        public CreateInstanceMessage()
        {
            super(MSG_TYPE_CREATE_INSTANCE);
        }

        public CreateInstanceMessage(String templateName, Template override)
        {
            super(MSG_TYPE_CREATE_INSTANCE);
            setTemplateName(templateName);
            setOverrideTemplate(override);
        }

        public String getTemplateName()
        {
            return templateName;
        }

        public void setTemplateName(String templateName)
        {
            this.templateName = templateName;
        }

        public Template getOverrideTemplate()
        {
            return overrideTemplate;
        }

        public void setOverrideTemplate(Template override)
        {
            overrideTemplate = override;
        }

        private String templateName;
        private Template overrideTemplate;
        
        private static final long serialVersionUID = 1L;
    }

    public static class GetInstanceInfoMessage extends Message
    {
        public GetInstanceInfoMessage() {
            super(MSG_TYPE_GET_INSTANCE_INFO);
        }

        public GetInstanceInfoMessage(String instanceName, int flags) {
            super(MSG_TYPE_GET_INSTANCE_INFO);
            setInstanceName(instanceName);
            setFlags(flags);
        }

        public GetInstanceInfoMessage(long instanceOid, int flags) {
            super(MSG_TYPE_GET_INSTANCE_INFO);
            setInstanceOid(instanceOid);
            setFlags(flags);
        }

        public Long getInstanceOid() {
            return instanceOid;
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public String getInstanceName() {
            return instanceName;
        }

        public void setInstanceName(String name) {
            instanceName = name;
        }

        public int getFlags() {
            return flags;
        }

        public void setFlags(int flags) {
            this.flags = flags;
        }

        private Long instanceOid;
        private String instanceName;
        private int flags;
        
        private static final long serialVersionUID = 1L;
    }

    /** Instance information.
        @see #getInstanceInfo(long,int)
    */
    public static class InstanceInfo
    {
        public InstanceInfo()
        {
        }

        /** Instance oid.  Null if the instance does not exist. */
        public Long oid;
        /** True if the instance is loaded, false otherwise. */
        public boolean loaded;
        /** Instance name. */
        public String name;
        /** Template used to create the instance. */
        public String templateName;
        /** Global skybox. */
        public String skybox;
        /** Global fog. */
        public Fog fog;
        /** Global ambient light. */
        public Color ambientLight;
        /** Global directional light. */
        public LightData dirLight;
        /** Global ocean data. */
        public OceanData ocean;
        /** Global terrain data. */
        public TerrainConfig terrainConfig;
        /** Global region configuration (water, forest, grass). */
        public List<String> regionConfig;
        /** Current instance player population. */
        public int playerPopulation;
    }

    public static class GetMarkerMessage extends Message
    {
        public GetMarkerMessage() {
            setMsgType(MSG_TYPE_GET_MARKER);
        }

        public GetMarkerMessage(long instanceOid, String name, long flags) {
            setMsgType(MSG_TYPE_GET_MARKER);
            setInstanceOid(instanceOid);
            setMarkerName(name);
            setFlags(flags);
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public void setMarkerName(String name) {
            this.markerName = name;
        }

        public String getMarkerName() {
            return markerName;
        }

        public long getFlags() {
            return flags;
        }

        public void setFlags(long flags) {
            this.flags = flags;
        }

        private long instanceOid;
        private String markerName;
        private long flags;

        private static final long serialVersionUID = 1L;
    }

    public static class GetRegionMessage extends Message
    {
        public GetRegionMessage() {
            setMsgType(MSG_TYPE_GET_REGION);
        }

        public GetRegionMessage(long instanceOid, String name, long flags) {
            setMsgType(MSG_TYPE_GET_REGION);
            setInstanceOid(instanceOid);
            setRegionName(name);
            setFlags(flags);
        }

        public long getInstanceOid() {
            return instanceOid;
        }

        public void setInstanceOid(long oid) {
            instanceOid = oid;
        }

        public void setRegionName(String name) {
            this.regionName = name;
        }

        public String getRegionName() {
            return regionName;
        }

        public long getFlags() {
            return flags;
        }

        public void setFlags(long flags) {
            this.flags = flags;
        }

        private long instanceOid;
        private String regionName;
        private long flags;

        private static final long serialVersionUID = 1L;
    }

    /** Request object instance entry.
    */
    public static class InstanceEntryReqMessage extends TargetMessage
    {
        public InstanceEntryReqMessage() {
            super();
        }

        /** Object instance entry.
        */
        public InstanceEntryReqMessage(long oid)
        {
            super(MSG_TYPE_INSTANCE_ENTRY_REQ,oid);
        }

        /** Object instance entry to the given location.
        */
        public InstanceEntryReqMessage(long oid, BasicWorldNode instanceLoc)
        {
            super(MSG_TYPE_INSTANCE_ENTRY_REQ,oid);
            setWorldNode(instanceLoc);
        }

        /** Object instance entry to the given location.
        */
        public InstanceEntryReqMessage(long oid, BasicWorldNode instanceLoc,
            int flags)
        {
            super(MSG_TYPE_INSTANCE_ENTRY_REQ,oid);
            setWorldNode(instanceLoc);
            setFlags(flags);
        }

        /** Object instance entry to the given location with a restore
            location.  Use with 'flags' set to FLAG_PUSH.
        */
        public InstanceEntryReqMessage(long oid, BasicWorldNode instanceLoc,
            int flags, BasicWorldNode restoreLoc)
        {
            super(MSG_TYPE_INSTANCE_ENTRY_REQ,oid);
            setWorldNode(instanceLoc);
            setFlags(flags);
            setRestoreNode(restoreLoc);
        }

        /** Object instance entry.
        */
        public InstanceEntryReqMessage(long oid, int flags)
        {
            super(MSG_TYPE_INSTANCE_ENTRY_REQ,oid);
            setFlags(flags);
        }

        /** No flags. */
        public static final int FLAG_NONE = 0;

        /** Push current instance and location onto the instance restore
            stack.
            @see #objectInstanceEntry(long,BasicWorldNode,int)
        */
        public static final int FLAG_PUSH = 1;

        /** Instance to the top of instance restore stack.
            @see #objectInstanceEntry(long,BasicWorldNode,int)
        */
        public static final int FLAG_POP = 2;

        /** Get the destination instance and location.
        */
        public BasicWorldNode getWorldNode()
        {
            return instanceLoc;
        }

        /** Set the destination instance and location.
        */
        public void setWorldNode(BasicWorldNode instanceLoc)
        {
            this.instanceLoc = instanceLoc;
        }

        /** Get the flags.
        */
        public int getFlags()
        {
            return flags;
        }

        /** Set the flags.  One of FLAG_NONE, FLAG_PUSH, or FLAG_POP.
        */
        public void setFlags(int flags)
        {
            this.flags = flags;
        }

        /** Get the restore location override.
        */
        public BasicWorldNode getRestoreNode()
        {
            return restoreLoc;
        }

        /** Set the restore location override.  When FLAG_PUSH is set,
            the object's current location is pushed.  The pushed
            location can be overridden by setting the restore node.
        */
        public void setRestoreNode(BasicWorldNode restoreLoc)
        {
            this.restoreLoc = restoreLoc;
        }

        /** Application defined processing state.  Used by the proxy when
            instance entry is broken into multiple steps.  The value is
            not copied when message is sent between processes.
        */
        public Object getProcessingState() {
            return processingState;
        }

        /** Set the application defined processing state.  Used by the
            proxy when instance entry is broken into multiple steps.  The
            value is not copied when message is sent between processes.
        */
        public void setProcessingState(Object state) {
            processingState = state;
        }

        private BasicWorldNode instanceLoc;
        private int flags;
        private BasicWorldNode restoreLoc;
        private transient Object processingState;
        
        private static final long serialVersionUID = 1L;
    }

    /** Instance sub-object namespace. */
    public static Namespace NAMESPACE = null;

    /** Instance template name.  Available on an instance object.  Ignored
        on an instance template.
    */
    public static final String TEMPL_INSTANCE_TEMPLATE_NAME = "templateName";

    /** Instance world file name.  Available on an instance object and
        template.
        @see #registerInstanceTemplate(Template)
    */
    public static final String TEMPL_WORLD_FILE_NAME = "worldFileName";

    /** Instance init script file name.  Available on an instance object and
        template.
        @see #registerInstanceTemplate(Template)
    */
    public static final String TEMPL_INIT_SCRIPT_FILE_NAME = "initScriptFileName";
    /** Instance load script file name.  Available on an instance object and
        template.
        @see #registerInstanceTemplate(Template)
    */
    public static final String TEMPL_LOAD_SCRIPT_FILE_NAME = "loadScriptFileName";

    /** Instance terrain config file (.mvt).  Overrides world file terrain
        configuration.  Available on an instance object and template.
        @see #registerInstanceTemplate(Template)
    */
    public static final String TEMPL_TERRAIN_CONFIG_FILE = "terrainConfigFile";

    /** Instance name.  Available on an instance object.
    */
    public static final String TEMPL_INSTANCE_NAME = "name";

    /** World loader override name.
    */
    public static final String TEMPL_LOADER_OVERRIDE_NAME = "loaderOverrideName";

    /** Unique name flag (Boolean).  If true, then instance name must be
        unique during instance creation.
    */
    public static final String TEMPL_UNIQUE_NAME = "uniqueName";

    public static final MessageType MSG_TYPE_REGISTER_INSTANCE_TEMPLATE =
        MessageType.intern("mv.REGISTER_INSTANCE_TEMPLATE");
    public static final MessageType MSG_TYPE_CREATE_INSTANCE =
        MessageType.intern("mv.CREATE_INSTANCE");
    public static final MessageType MSG_TYPE_GET_INSTANCE_INFO =
        MessageType.intern("mv.GET_INSTANCE_INFO");
    public static final MessageType MSG_TYPE_GET_MARKER =
        MessageType.intern("mv.GET_MARKER");
    public static final MessageType MSG_TYPE_GET_REGION =
        MessageType.intern("mv.GET_REGION");
    public static final MessageType MSG_TYPE_LOAD_INSTANCE =
        MessageType.intern("mv.LOAD_INSTANCE");
    public static final MessageType MSG_TYPE_UNLOAD_INSTANCE =
        MessageType.intern("mv.UNLOAD_INSTANCE");
    public static final MessageType MSG_TYPE_DELETE_INSTANCE =
        MessageType.intern("mv.DELETE_INSTANCE");
    public static final MessageType MSG_TYPE_LOAD_INSTANCE_CONTENT =
        MessageType.intern("mv.LOAD_INSTANCE_CONTENT");
    public static final MessageType MSG_TYPE_INSTANCE_UNLOADED =
        MessageType.intern("mv.INSTANCE_UNLOADED");
    public static final MessageType MSG_TYPE_INSTANCE_DELETED =
        MessageType.intern("mv.INSTANCE_DELETED");

    public static MessageType MSG_TYPE_INSTANCE_ENTRY_REQ =
        MessageType.intern("mv.INSTANCE_ENTRY_REQ");


// message: CreateInstanceMessage: MSG_TYPE_CREATE_INSTANCE
// message: DeleteInstanceMessage: MSG_TYPE_DELETE_INSTANCE
// message: UnloadInstanceMessage: MSG_TYPE_UNLOAD_INSTANCE
// message: LoadInstanceMessage: MSG_TYPE_LOAD_INSTANCE

// message: GetInstancesMessage: MSG_TYPE_GET_INSTANCES
// message: GetInstanceInfoMessage: MSG_TYPE_GET_INSTANCE_INFO

}


