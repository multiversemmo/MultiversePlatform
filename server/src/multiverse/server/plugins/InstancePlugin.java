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

import java.util.Map;
import java.util.HashMap;
import java.io.File;
import java.io.Serializable;
import java.util.List;
import java.util.Collection;
import java.util.Iterator;
import java.util.ArrayList;

import multiverse.msgsys.*;

import multiverse.server.util.Log;
import multiverse.server.util.FileUtil;
import multiverse.server.engine.Hook;
import multiverse.server.engine.PluginStatus;
import multiverse.server.engine.Engine;
import multiverse.server.engine.EnginePlugin;
import multiverse.server.engine.Namespace;
import multiverse.server.engine.Searchable;
import multiverse.server.engine.SearchClause;
import multiverse.server.engine.SearchSelection;
import multiverse.server.engine.SearchManager;
import multiverse.server.engine.PropertyMatcher;
import multiverse.server.engine.PropertySearch;
import multiverse.server.engine.TerrainConfig;
import multiverse.server.engine.DefaultWorldLoaderOverride;
import multiverse.server.engine.WorldLoaderOverride;
import multiverse.server.objects.Template;
import multiverse.server.objects.Instance;
import multiverse.server.objects.Entity;
import multiverse.server.objects.Marker;
import multiverse.server.objects.Region;
import multiverse.server.objects.SpawnData;
import multiverse.server.objects.ObjectTypes;
import multiverse.server.objects.EntityManager;
import multiverse.server.objects.EntitySearchable;
import multiverse.server.messages.PopulationFilter;
import multiverse.server.plugins.InstanceClient.RegisterInstanceTemplateMessage;
import multiverse.server.plugins.InstanceClient.CreateInstanceMessage;
import multiverse.server.plugins.InstanceClient.GetInstanceInfoMessage;
import multiverse.server.plugins.InstanceClient.InstanceInfo;
import multiverse.server.plugins.InstanceClient.GetMarkerMessage;
import multiverse.server.plugins.InstanceClient.GetRegionMessage;
import multiverse.server.plugins.WorldManagerClient.SpawnedMessage;
import multiverse.server.plugins.WorldManagerClient.DespawnedMessage;
import multiverse.server.plugins.MobManagerClient;


public class InstancePlugin extends EnginePlugin
{
    public InstancePlugin()
    {
        super("Instance");
        setPluginType("Instance");
        registerWorldLoaderOverrideClass("default",
            DefaultWorldLoaderOverride.class);
    }

    public void onActivate()
    {
        // Instance plugin is not available until all the startup
        // instances have been created.  This is usually done by
        // startup_instance.py which should call setPluginAvailable(true)
        setPluginAvailable(false);

        registerHooks();

        MessageTypeFilter filter = new MessageTypeFilter();
        filter.addType(InstanceClient.MSG_TYPE_REGISTER_INSTANCE_TEMPLATE);
        filter.addType(InstanceClient.MSG_TYPE_CREATE_INSTANCE);
        filter.addType(InstanceClient.MSG_TYPE_LOAD_INSTANCE);
        filter.addType(InstanceClient.MSG_TYPE_GET_INSTANCE_INFO);
        filter.addType(InstanceClient.MSG_TYPE_GET_MARKER);
        filter.addType(InstanceClient.MSG_TYPE_GET_REGION);

        Engine.getAgent().createSubscription(filter,
                this, MessageAgent.RESPONDER);

        PopulationFilter populationFilter =
                new PopulationFilter(ObjectTypes.player);
        Engine.getAgent().createSubscription(populationFilter, this);

        registerPluginNamespace(InstanceClient.NAMESPACE,
                new InstanceGenerateSubObjectHook());

        registerUnloadHook(InstanceClient.NAMESPACE, new InstanceUnloadHook());
        registerDeleteHook(InstanceClient.NAMESPACE, new InstanceDeleteHook());

        SearchManager.registerMatcher(PropertySearch.class, Entity.class,
            new PropertyMatcher.Factory());
        SearchManager.registerSearchable(ObjectTypes.instance,
            new EntitySearchable(ObjectTypes.instance));

        SearchManager.registerMatcher(Marker.Search.class, Marker.class,
            new PropertyMatcher.Factory());
        SearchManager.registerSearchable(Marker.OBJECT_TYPE,
            new MarkerSearch());

        SearchManager.registerMatcher(Region.Search.class, Region.class,
            new PropertyMatcher.Factory());
        SearchManager.registerSearchable(Region.OBJECT_TYPE,
            new RegionSearch());

    }

    protected void registerHooks()
    {
        getHookManager().addHook(
            InstanceClient.MSG_TYPE_REGISTER_INSTANCE_TEMPLATE,
            new RegisterInstanceTemplateHook());
        getHookManager().addHook(
            InstanceClient.MSG_TYPE_CREATE_INSTANCE,
            new CreateInstanceHook());
        getHookManager().addHook(
            InstanceClient.MSG_TYPE_LOAD_INSTANCE,
            new LoadInstanceHook());
        getHookManager().addHook(
            InstanceClient.MSG_TYPE_GET_INSTANCE_INFO,
            new GetInstanceInfoHook());
        getHookManager().addHook(
            InstanceClient.MSG_TYPE_GET_MARKER,
            new GetMarkerHook());
        getHookManager().addHook(
            InstanceClient.MSG_TYPE_GET_REGION,
            new GetRegionHook());
        getHookManager().addHook(
            WorldManagerClient.MSG_TYPE_SPAWNED,
            new PopulationHook());
        getHookManager().addHook(
            WorldManagerClient.MSG_TYPE_DESPAWNED,
            new PopulationHook());
    }

    protected void sendSpawnGenerators(Instance instance) {
        List<SpawnData> spawnDataList = instance.getSpawnData();
        for (SpawnData spawnData : spawnDataList)
            MobManagerClient.createSpawnGenerator(spawnData);
    }

    /** Register a world loader override class.  The registered name
        is referenced by instance property
        {@link multiverse.server.plugins.InstanceClient#TEMPL_LOADER_OVERRIDE_NAME InstanceClient.TEMPL_LOADER_OVERRIDE_NAME}.  An instance of the class
        is created prior to loading the instance world file.  If the
        instance does not specify a world loader override, then
        {@link multiverse.server.engine.DefaultWorldLoaderOverride DefaultWorldLoaderOverride} is used.
        @param name World loader override class registered name.
        @param loaderOverrideClass World loader override class, must implement
            {@link multiverse.server.engine.WorldLoaderOverride}.
    */
    public static void registerWorldLoaderOverrideClass(String name,
        Class loaderOverrideClass)
    {
        synchronized (loaderOverrideClasses) {
            loaderOverrideClasses.put(name,loaderOverrideClass);
        }
    }

    /** Get a registered spawn generator class.
    */
    public static Class getWorldLoaderOverrideClass(String name)
    {
        return loaderOverrideClasses.get(name);
    }

    private static Map<String,Class> loaderOverrideClasses =
        new HashMap<String,Class>();

    class RegisterInstanceTemplateHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            RegisterInstanceTemplateMessage message =
                (RegisterInstanceTemplateMessage) msg;

            Template template = message.getTemplate();

            if (Log.loggingDebug)
                Log.debug("RegisterInstanceTemplateHook: template="+template);

            if (template == null)  {
                Engine.getAgent().sendBooleanResponse(message, Boolean.FALSE);
                return true;
            }

            if (template.getName() == null) {
                Log.error("RegisterInstanceTemplateHook: missing template name");
                Engine.getAgent().sendBooleanResponse(message, Boolean.FALSE);
                return true;
            }

            String worldFileName = (String)template.get(
                InstanceClient.NAMESPACE, InstanceClient.TEMPL_WORLD_FILE_NAME);
            if (worldFileName == null) {
                Log.error("RegisterInstanceTemplateHook: missing world file name, name="+template.getName());
                Engine.getAgent().sendBooleanResponse(message, Boolean.FALSE);
                return true;
            }

            //## check if files exist?

            instanceTemplates.put(template.getName(), template);

            if (Log.loggingDebug)
                Log.debug("RegisterInstanceTemplateHook: added template name="+template.getName());

            Engine.getAgent().sendBooleanResponse(message, Boolean.TRUE);

            return true;
        }
    }

    class CreateInstanceHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            CreateInstanceMessage message = (CreateInstanceMessage) msg;
            return (new CreateInstanceHook()).handleMessage(message);
        }

        private boolean handleMessage(CreateInstanceMessage message)
        {
            try {
                Long instanceOid = createInstance(message.getTemplateName(),
                    message.getOverrideTemplate());
                Engine.getAgent().sendLongResponse(message, instanceOid);
            }
            finally {
                if (uniqueNameFlag)
                    releaseUniqueName(instanceName);
            }
            return true;
        }

        private Long createInstance(String templateName,
            Template overrideTemplate)
        {
            if (Log.loggingDebug)
                Log.debug("CreateInstanceHook: templateName="+templateName+
                        " override="+overrideTemplate);

            if (templateName == null)  {
                return null;
            }

            Template template = instanceTemplates.get(templateName);

            if (template == null) {
                Log.error("CreateInstanceHook: unknown template name="+
                    templateName);
                return null;
            }

            Template mergedTemplate;
            try {
                mergedTemplate = (Template) template.clone();
            } catch (CloneNotSupportedException ex) {
                return null;
            }

            mergedTemplate = mergedTemplate.merge(overrideTemplate);

            // If there's no world manager plugin in the merged template,
            // then select one.
            String worldManagerPlugin = (String)
                mergedTemplate.get(WorldManagerClient.INSTANCE_NAMESPACE,
                    WorldManagerClient.TEMPL_WORLDMGR_NAME);
            if (worldManagerPlugin == null || worldManagerPlugin.equals("")) {
                PluginStatus plugin = selectWorldManagerPlugin();
                if (plugin != null) {
                    if (Log.loggingDebug)
                        Log.debug("CreateInstanceHook: assigned world manager " + plugin.plugin_name +
                            " host=" + plugin.host_name);
                    mergedTemplate.put(WorldManagerClient.INSTANCE_NAMESPACE,
                        WorldManagerClient.TEMPL_WORLDMGR_NAME,
                        plugin.plugin_name);
                    worldManagerPlugin = plugin.plugin_name;
                }
                else {
                    Log.error("CreateInstanceHook: no world manager for instance, templateName=" +
                        templateName);
                    return null;
                }
            }

            mergedTemplate.put(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_INSTANCE_TEMPLATE_NAME, templateName);

            uniqueNameFlag = (Boolean) mergedTemplate.get(
                InstanceClient.NAMESPACE, InstanceClient.TEMPL_UNIQUE_NAME);
            if (uniqueNameFlag == null)
                uniqueNameFlag = false;

            instanceName =
                (String)mergedTemplate.get(InstanceClient.NAMESPACE,
                    InstanceClient.TEMPL_INSTANCE_NAME);

            if (uniqueNameFlag) {
                Long instanceOid = waitForUniqueName(instanceName);
                if (instanceOid != null) {
                    Log.debug("CreateInstanceHook: instance name already exists, name=" + instanceName + " instanceOid=" + instanceOid);
                    return null;
                }
            }

            String worldFileName = (String)mergedTemplate.get(
                InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_WORLD_FILE_NAME);
            worldFileName = FileUtil.expandFileName(worldFileName);
            mergedTemplate.put( InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_WORLD_FILE_NAME, worldFileName);

            if (! fileExist(worldFileName)) {
                Log.error("CreateInstanceHook: world file not found fileName="+
                    worldFileName);
                return null;
            }

            String initScript = (String)mergedTemplate.get(
                InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME);
            if (initScript != null) {
                initScript = FileUtil.expandFileName(initScript);
                mergedTemplate.put( InstanceClient.NAMESPACE,
                    InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, initScript);
            }

            if (initScript != null && ! fileExist(initScript)) {
                Log.error("CreateInstanceHook: init file not found fileName="+
                    initScript);
                return null;
            }

            String loadScript = (String)mergedTemplate.get(
                InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_LOAD_SCRIPT_FILE_NAME);
            if (loadScript != null) {
                loadScript = FileUtil.expandFileName(loadScript);
                mergedTemplate.put( InstanceClient.NAMESPACE,
                    InstanceClient.TEMPL_LOAD_SCRIPT_FILE_NAME, loadScript);
            }

            if (loadScript != null && ! fileExist(loadScript)) {
                Log.error("CreateInstanceHook: load script file not found fileName="+
                    loadScript);
                return null;
            }

            Long instanceOid = ObjectManagerClient.generateObject(
                ObjectManagerClient.BASE_TEMPLATE, mergedTemplate);

            if (instanceOid == null) {
                Log.error("CreateInstanceHook: generateObject failed"+
                        " name="+mergedTemplate.get(InstanceClient.NAMESPACE, InstanceClient.TEMPL_INSTANCE_NAME)+
                        " templateName="+templateName+
                        " wmName="+worldManagerPlugin);
                return null;
            }

            Log.info("InstancePlugin: CREATE_INSTANCE instanceOid="+instanceOid+
                    " name=["+instanceName+"]"+
                    " templateName=["+templateName+"]"+
                    " wmName="+worldManagerPlugin);

            Instance instance = (Instance) EntityManager.getEntityByNamespace(
                instanceOid, InstanceClient.NAMESPACE);

            String loaderName = instance.getWorldLoaderOverrideName();
            if (loaderName == null)
                loaderName = "default";
            instance.setWorldLoaderOverride(createLoaderOverride(loaderName));

            if (! instance.loadWorldFile()) {
                Log.error("CreateInstanceHook: load world file failed"+
                        " fileName="+instance.getWorldFileName());
                return null;
            }

            sendSpawnGenerators(instance);

            if (! instance.runInitScript()) {
                Log.error("CreateInstanceHook: init world script failed"+
                        " fileName="+instance.getInitScriptFileName());
                return null;
            }

            instance.setWorldLoaderOverride(null);
            instance.setState(Instance.STATE_AVAILABLE);
//## publish instance ready (or some such)

            return instanceOid;
        }

        String instanceName;
        Boolean uniqueNameFlag;
    }

    private Long waitForUniqueName(String instanceName)
    {
        Long instanceOid = null;
        synchronized (pendingUniqueNames) {
            while (pendingUniqueNames.contains(instanceName)) {
                try {
                    pendingUniqueNames.wait();
                } catch (InterruptedException ex)  { /* ignore */ }
            }
            Instance instance = getInstance(instanceName);
            if (instance == null)
                instanceOid = getPersistentInstanceOid(instanceName);
            else
                instanceOid = instance.getOid();

            if (instanceOid == null)
                pendingUniqueNames.add(instanceName);

            return instanceOid;
        }
    }

    private void releaseUniqueName(String instanceName)
    {
        synchronized (pendingUniqueNames) {
            pendingUniqueNames.remove(instanceName);
            pendingUniqueNames.notifyAll();
        }
    }

    class LoadInstanceHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            SubjectMessage message = (SubjectMessage) msg;
            long instanceOid = message.getSubject();

            Instance instance = (Instance) EntityManager.getEntityByNamespace(
                instanceOid, InstanceClient.NAMESPACE);
            if (instance != null) {
                if (Log.loggingDebug)
                    Log.debug("LoadInstanceHook: instance already loaded instanceOid=" +
                        instanceOid + " state=" + instance.getState());
                if (instance.getState() == Instance.STATE_AVAILABLE)
                    Engine.getAgent().sendIntegerResponse(message,
                        InstanceClient.RESULT_OK);
                else
                    Engine.getAgent().sendIntegerResponse(message,
                        InstanceClient.RESULT_ERROR_RETRY);
                return true;
            }

            List<Namespace> namespaces =
                Engine.getDatabase().getObjectNamespaces(instanceOid);
            if (namespaces == null) {
                Log.debug("LoadInstanceHook: unknown instanceOid=" + instanceOid);
                Engine.getAgent().sendIntegerResponse(message,
                    InstanceClient.RESULT_ERROR_UNKNOWN_OBJECT);
                return true;
            }
            if (! namespaces.contains(InstanceClient.NAMESPACE)) {
                Log.error("LoadInstanceHook: not an instance oid=" +
                    instanceOid);
                Engine.getAgent().sendIntegerResponse(message,
                    InstanceClient.RESULT_ERROR_INTERNAL);
                return true;
            }

            // Select a world manager plugin to host instance
            PluginStatus plugin = selectWorldManagerPlugin();
            if (plugin != null) {
                if (Log.loggingDebug)
                    Log.debug("LoadInstanceHook: assigned world manager " +
                        plugin.plugin_name + " host=" + plugin.host_name +
                        " for instanceOid="+instanceOid);
                WorldManagerClient.hostInstance(instanceOid, plugin.plugin_name);
            }
            else {
                Log.error("LoadInstanceHook: no world manager for instance, instanceOid=" +
                    instanceOid);
                Engine.getAgent().sendIntegerResponse(message,
                    InstanceClient.RESULT_ERROR_NO_WORLD_MANAGER);
                return true;
            }

            Long result = ObjectManagerClient.loadObject(instanceOid);
            if (result == null || result < 0) {
                Engine.getAgent().sendIntegerResponse(message,
                    InstanceClient.RESULT_ERROR_INTERNAL);
                return true;
            }

            instance = (Instance) EntityManager.getEntityByNamespace(
                instanceOid, InstanceClient.NAMESPACE);

            Log.info("InstancePlugin: LOAD_INSTANCE instanceOid="+instanceOid+
                    " name=["+instance.getName()+"]"+
                    " templateName=["+instance.getTemplateName()+"]"+
                    " wmName="+plugin.plugin_name);

            instance.setState(Instance.STATE_LOAD);

            String loaderName = instance.getWorldLoaderOverrideName();
            if (loaderName == null)
                loaderName = "default";
            instance.setWorldLoaderOverride(createLoaderOverride(loaderName));

            if (! instance.loadWorldFile()) {
//## unload instance object
                Log.error("LoadInstanceHook: load world file failed"+
                    " fileName="+instance.getWorldFileName()+
                    " instanceOid="+instanceOid
                );
                Engine.getAgent().sendIntegerResponse(message,
                    InstanceClient.RESULT_ERROR_INTERNAL);
                return true;
            }

            SubjectMessage loadContentMessage =
                new SubjectMessage(InstanceClient.MSG_TYPE_LOAD_INSTANCE_CONTENT,instanceOid);
            Engine.getAgent().sendRPC(loadContentMessage);

            sendSpawnGenerators(instance);

            if (! instance.runLoadScript()) {
//## unload instance object
                Log.error("LoadInstanceHook: init world script failed"+
                    " fileName="+instance.getInitScriptFileName()+
                    " instanceOid="+instanceOid
                );
                Engine.getAgent().sendIntegerResponse(message,
                    InstanceClient.RESULT_ERROR_INTERNAL);
                return true;
            }

            instance.setWorldLoaderOverride(null);
            instance.setState(Instance.STATE_AVAILABLE);

            Engine.getAgent().sendIntegerResponse(message,
                InstanceClient.RESULT_OK);
            return true;
        }

    }

    class InstanceGenerateSubObjectHook extends GenerateSubObjectHook {
        public InstanceGenerateSubObjectHook() {
            super(InstancePlugin.this);
        }

        public SubObjData generateSubObject(Template template,
            Namespace namespace, Long instanceOid)
        {
            Instance instance = new Instance(instanceOid);
            instance.setType(ObjectTypes.instance);

            instance.setName((String)
                template.get(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_INSTANCE_NAME));
            instance.setTemplateName((String)
                template.get(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_INSTANCE_TEMPLATE_NAME));
            instance.setWorldFileName((String)
                template.get(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_WORLD_FILE_NAME));
            instance.setInitScriptFileName((String)
                template.get(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME));
            instance.setLoadScriptFileName((String)
                template.get(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_LOAD_SCRIPT_FILE_NAME));
            instance.setWorldLoaderOverrideName((String)
                template.get(InstanceClient.NAMESPACE,
                InstanceClient.TEMPL_LOADER_OVERRIDE_NAME));

            String terrainConfigFile =
                (String) template.get(InstanceClient.NAMESPACE,
                    InstanceClient.TEMPL_TERRAIN_CONFIG_FILE);
            if (terrainConfigFile != null) {
                TerrainConfig terrainConfig = new TerrainConfig();
                terrainConfig.setConfigType(TerrainConfig.configTypeFILE);
                terrainConfig.setConfigData(terrainConfigFile);
                instance.setTerrainConfig(terrainConfig);
            }

            // copy properties from template to object
            Map<String, Serializable> props =
                template.getSubMap(Namespace.INSTANCE);
            for (Map.Entry<String, Serializable> entry : props.entrySet()) {
                String key = entry.getKey();
                Serializable value = entry.getValue();
                if (!key.startsWith(":")) {
                    instance.setProperty(key, value);
                }
            }

            Boolean persistent = (Boolean)template.get(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT);
            if (persistent == null)
                persistent = false;
            instance.setPersistenceFlag(persistent);
            instance.setState(Instance.STATE_GENERATE);

            EntityManager.registerEntityByNamespace(instance,
                InstanceClient.NAMESPACE);

            if (persistent)
                Engine.getPersistenceManager().persistEntity(instance);

            return new SubObjData();
        }
    }

    class InstanceUnloadHook implements UnloadHook {
        public void onUnload(Entity ee) {
            // save ourselves before unload
            Instance instance = (Instance) ee;
            instance.setState(Instance.STATE_UNLOAD);
            if (instance.getPersistenceFlag())
                Engine.getPersistenceManager().persistEntity(instance);
            //## unload spawn generators and object tracker
            Log.info("InstancePlugin: INSTANCE_UNLOAD instanceOid="+instance.getOid() +
                    " name=["+instance.getName()+"]"+
                    " templateName=["+instance.getTemplateName()+"]");
        }
    }

    class InstanceDeleteHook implements DeleteHook {
        public void onDelete(Long oid, Namespace namespace)
        {
        }
        public void onDelete(Entity ee)
        {
            Instance instance = (Instance) ee;
            instance.setState(Instance.STATE_DELETE);
            //?? nothing to do, objmgr will delete instance obj
            //## delete spawn generators and object tracker
            Log.info("InstancePlugin: INSTANCE_DELETE instanceOid="+instance.getOid() +
                    " name=["+instance.getName()+"]"+
                    " templateName=["+instance.getTemplateName()+"]");
        }
    }

    class GetInstanceInfoHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            GetInstanceInfoMessage message = (GetInstanceInfoMessage) msg;

            if ((message.getFlags() & InstanceClient.FLAG_MULTIPLE) != 0 &&
                    message.getInstanceOid() == null) {
                List<InstanceInfo> list = getMultipleInstanceInfo(
                    message.getFlags(), message.getInstanceName());
                Engine.getAgent().sendObjectResponse(msg, list);
            }
            else {
                InstanceInfo info = getInstanceInfo(message.getFlags(),
                    message.getInstanceOid(), message.getInstanceName());
                Engine.getAgent().sendObjectResponse(msg, info);
            }
            return true;
        }

        public InstanceInfo getInstanceInfo(int infoFlags, Long instanceOid,
            String instanceName)
        {
            InstanceInfo info = new InstanceInfo();

            Instance instance = null;
            if (instanceOid == null) {
                if (instanceName == null) {
                    return info;
                }
                instance = getInstance(instanceName);
                if (instance == null) {
                    instanceOid = getPersistentInstanceOid(instanceName);
                    if (instanceOid == null) {
                        return info;
                    }
                }
                else {
                    instanceOid = instance.getOid();
                    info.loaded = instance.getState() == Instance.STATE_AVAILABLE;
                }
            }
            else {
                instance = (Instance) EntityManager.getEntityByNamespace(
                    instanceOid, InstanceClient.NAMESPACE);
                if (instance == null) {
                    return info;
                }
                info.loaded = instance.getState() == Instance.STATE_AVAILABLE;
            }

            if ((infoFlags & InstanceClient.FLAG_OID) != 0)
                info.oid = instanceOid;

            // Instance not loaded, so return what we got from DB
            if (instance == null) {
                return info;
            }

            getInstanceInfo(instance, infoFlags, info);

            return info;
        }

        public List<InstanceInfo> getMultipleInstanceInfo(int infoFlags,
            String instanceName)
        {
            Entity[] entities = (Entity[]) EntityManager.getAllEntitiesByNamespace(InstanceClient.NAMESPACE);

            List<InstanceInfo> list = new ArrayList<InstanceInfo>();
            for (Entity entity : entities) {
                if (! (entity instanceof Instance) ||
                        ! instanceName.equals(((Instance)entity).getName()))
                    continue;
                Instance instance = (Instance) entity;
                InstanceInfo info = new InstanceInfo();
                info.loaded = instance.getState() == Instance.STATE_AVAILABLE;
                info.oid = instance.getOid();
                getInstanceInfo(instance, infoFlags, info);
                list.add(info);
            }
            return list;
        }

        public void getInstanceInfo(Instance instance, int infoFlags,
            InstanceInfo info)
        {
            if ((infoFlags & InstanceClient.FLAG_NAME) != 0)
                info.name = instance.getName();
            if ((infoFlags & InstanceClient.FLAG_TEMPLATE_NAME) != 0)
                info.templateName = instance.getTemplateName();

            if ((infoFlags & InstanceClient.FLAG_SKYBOX) != 0)
                info.skybox = instance.getGlobalSkybox();
            if ((infoFlags & InstanceClient.FLAG_FOG) != 0)
                info.fog = instance.getGlobalFog();
            if ((infoFlags & InstanceClient.FLAG_AMBIENT_LIGHT) != 0)
                info.ambientLight = instance.getGlobalAmbientLight();
            if ((infoFlags & InstanceClient.FLAG_DIR_LIGHT) != 0)
                info.dirLight = instance.getGlobalDirectionalLight();
            if ((infoFlags & InstanceClient.FLAG_OCEAN) != 0)
                info.ocean = instance.getOceanData();
            if ((infoFlags & InstanceClient.FLAG_TERRAIN) != 0)
                info.terrainConfig = instance.getTerrainConfig();
            if ((infoFlags & InstanceClient.FLAG_REGION_CONFIG) != 0)
                info.regionConfig = instance.getRegionConfig();
            if ((infoFlags & InstanceClient.FLAG_PLAYER_POPULATION) != 0)
                info.playerPopulation = instance.getPlayerPopulation();
        }
    }

    class GetMarkerHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            GetMarkerMessage message = (GetMarkerMessage) msg;

            Instance instance = (Instance) EntityManager.getEntityByNamespace(                         message.getInstanceOid(), InstanceClient.NAMESPACE);
            if (instance == null) {
                Log.error("GetMarkerHook: unknown instanceOid=" +
                    message.getInstanceOid());
                Engine.getAgent().sendObjectResponse(msg, null);
                return true;
            }

            Marker marker = instance.getMarker(message.getMarkerName());
            Log.debug("GetMarkerHook: name="+message.getMarkerName()
                + " instanceOid=" + message.getInstanceOid() + " " + marker);

            if (marker == null) {
                Log.error("GetMarkerHook: unknown markerName=" +
                    message.getMarkerName() + " instanceOid=" +
                    message.getInstanceOid());
                Engine.getAgent().sendObjectResponse(msg, null);
                return true;
            }

            Marker result = new Marker();
            long fetchFlags = message.getFlags();
            if ((fetchFlags & InstanceClient.MARKER_POINT) != 0)
                result.setPoint(marker.getPoint());
            if ((fetchFlags & InstanceClient.MARKER_ORIENTATION) != 0)
                result.setOrientation(marker.getOrientation());
            if ((fetchFlags & InstanceClient.MARKER_PROPERTIES) != 0)
                result.setProperties(marker.getPropertyMapRef());
            Engine.getAgent().sendObjectResponse(msg, result);
            return true;
        }
    }

    class GetRegionHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            GetRegionMessage message = (GetRegionMessage) msg;

            Instance instance = (Instance) EntityManager.getEntityByNamespace(
                message.getInstanceOid(), InstanceClient.NAMESPACE);
            if (instance == null) {
                Log.error("GetRegionHook: unknown instanceOid=" +
                    message.getInstanceOid());
                Engine.getAgent().sendObjectResponse(msg, null);
                return true;
            }

            Region region = instance.getRegion(message.getRegionName());
            Log.debug("GetRegionHook: name="+message.getRegionName()
                + " instanceOid=" + message.getInstanceOid() + " " + region);

            if (region == null) {
                Log.error("GetRegionHook: unknown regionName=" +
                    message.getRegionName() + " instanceOid=" +
                    message.getInstanceOid());
                Engine.getAgent().sendObjectResponse(msg, null);
                return true;
            }

            Region result = new Region(region.getName());
            result.setPriority(region.getPriority());
            long fetchFlags = message.getFlags();
            if ((fetchFlags & InstanceClient.REGION_BOUNDARY) != 0)
                result.setBoundary(region.getBoundary());
            if ((fetchFlags & InstanceClient.REGION_PROPERTIES) != 0)
                result.setProperties(region.getPropertyMapRef());
            Engine.getAgent().sendObjectResponse(msg, result);
            return true;
        }
    }

    class PopulationHook implements Hook {
        public boolean processMessage(Message msg, int flags)
        {
            long instanceOid;
            int delta;
            if (msg instanceof SpawnedMessage) {
                SpawnedMessage message = (SpawnedMessage) msg;
                instanceOid = message.getInstanceOid();
                delta = 1;
            }
            else if (msg instanceof DespawnedMessage) {
                DespawnedMessage message = (DespawnedMessage) msg;
                instanceOid = message.getInstanceOid();
                delta = -1;
            }
            else
                return true;

            Instance instance = (Instance) EntityManager.getEntityByNamespace(
                instanceOid, InstanceClient.NAMESPACE);
            if (instance == null) {
                Log.error("PopulationHook: unknown instanceOid=" +
                    instanceOid + " msg="+msg);
                return true;
            }

            int population = instance.changePlayerPopulation(delta);
            if (populationChangeCallback != null)
            	populationChangeCallback.onInstancePopulationChange(instanceOid, instance.getName(), population);
            return true;
        }
    }

    private Instance getInstance(String name)
    {
        Entity[] entities = (Entity[]) EntityManager.getAllEntitiesByNamespace(InstanceClient.NAMESPACE);

        for (Entity entity : entities) {
            if (entity instanceof Instance) {
                if (name.equals(((Instance)entity).getName()))
                    return (Instance)entity;
            }
        }
        return null;
    }

    /** Get persistent instance oid by name.  The lookup is performed
        on the database, independent of loaded instances.
    */
    public Long getPersistentInstanceOid(String name)
    {
        return Engine.getDatabase().getOidByName(name, InstanceClient.NAMESPACE);
    }

    public class MarkerSearch implements Searchable
    {
        public Collection runSearch(SearchClause search,
            SearchSelection selection)
        {
            Marker.Search markerSearch = (Marker.Search) search;
            
            Instance instance;
            instance = (Instance) EntityManager.getEntityByNamespace(
                markerSearch.getInstanceOid(), InstanceClient.NAMESPACE);
            if (instance == null) {
                Log.error("runSearch: unknown instanceOid="+markerSearch.getInstanceOid());
                return null;
            }

            return instance.runMarkerSearch(search,selection);
        }
    }

    public class RegionSearch implements Searchable
    {
        public Collection runSearch(SearchClause search,
            SearchSelection selection)
        {
            Region.Search regionSearch = (Region.Search) search;
            
            Instance instance;
            instance = (Instance) EntityManager.getEntityByNamespace(
                regionSearch.getInstanceOid(), InstanceClient.NAMESPACE);
            if (instance == null) {
                Log.error("runSearch: unknown instanceOid="+regionSearch.getInstanceOid());
                return null;
            }

            return instance.runRegionSearch(search,selection);
        }
    }

    private boolean fileExist(String fileName)
    {
        File file = new File(FileUtil.expandFileName(fileName));
        return file.exists() && file.canRead();
    }

    private WorldLoaderOverride createLoaderOverride(String loaderName)
    {
        Class loaderClass = null;
        try {
            loaderClass =
                loaderOverrideClasses.get(loaderName);
            if (loaderClass == null) {
                Log.error("World loader override class not registered, name=" +
                    loaderName);
            }
            return (WorldLoaderOverride) loaderClass.newInstance();
        }
        catch (Exception ex) {
            Log.exception("failed instantiating world loader, name=" +
                loaderName + " class="+loaderClass.getName(), ex);
            return null;
        }

    }

    protected final PluginStatus selectWorldManagerPlugin()
    {
        List<PluginStatus> plugins =
            Engine.getDatabase().getPluginStatus("WorldManager");
        Iterator<PluginStatus> iterator = plugins.iterator();
        while (iterator.hasNext()) {
            PluginStatus plugin = iterator.next();
            if (plugin.run_id != Engine.getAgent().getDomainStartTime())
                iterator.remove();
        }

        if (plugins.size() == 0)
            return null;
        return selectBestWorldManager(plugins);
    }

    // Return world manager with least entities
    protected PluginStatus selectBestWorldManager(List<PluginStatus> plugins)
    {
        PluginStatus selection = null;
        int selectionEntityCount = Integer.MAX_VALUE;
        for (PluginStatus plugin : plugins) {
            Map<String,String> status = Engine.makeMapOfString(plugin.status);
            int entityCount;
            try {
                entityCount = Integer.parseInt(status.get("entities"));
            }
            catch (Exception e) {
                Log.exception("selectBestWorldManager: wmgr "+
                    plugin.plugin_name+
                    " invalid entity count: "+status.get("entities"), e);
                continue;
            }
            if (entityCount < selectionEntityCount) {
                selection = plugin;
                selectionEntityCount = entityCount;
            }
        }
        return selection;
    }
    
    /**
     * This must be a base class rather than an interface because we
     * want to create callbacks in Python.
     */
    public static class PopulationChangeCallback {
    	public void onInstancePopulationChange(long instanceOid, String name, int population) {
        }
    }
    
    /**
     * Register the callback that supplies the population of an instance when it changes.
     * @param populationChangeCallback
     */
    public void registerPopulationChangeCallback(PopulationChangeCallback populationChangeCallback) {
    	this.populationChangeCallback = populationChangeCallback;
    }
    
    Map<String,Template> instanceTemplates = new HashMap<String,Template>();
    List<String> pendingUniqueNames = new ArrayList<String>();
    PopulationChangeCallback populationChangeCallback = null;
}

