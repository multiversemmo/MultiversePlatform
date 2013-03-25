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

package multiverse.server.engine;

import java.util.*;
import java.io.*;
import java.util.concurrent.locks.*;
import java.util.concurrent.*;
import javax.management.*;

import multiverse.server.objects.Entity;
import multiverse.server.objects.EntityManager;
import multiverse.server.objects.Template;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.server.util.*;
import multiverse.server.messages.*;
import multiverse.msgsys.*;


/**
 * The EnginePlugin is the preferred way to extend the server.  Sub-class
 * to add new server functionality.  Plugins typically implement one or
 * more sub-objects, identified by name space.  The sub-class provides
 * its own hooks for sub-object manipulation; creation, load, unload, delete,
 * and custom operations.
 * <p>
 * Plugins must have a unique name.  This identifies the plugin within
 * the messages domain/cluster.  The plugin name should be set in the
 * plugin constructor. {@link #setName(String)}
 * Plugins must have a plugin type.  The
 * plugin type is a string associated with a plugin class.  The plugin
 * type is used to resolve startup dependencies.  The plugin type should
 * be set in the plugin constructor.  {@link #setPluginType(String)}
 * <p>
 * When the multiverse engine starts up, it will load initialization scripts
 * (which can change the network ports and database server, etc) and then bring
 * up the database and network services.
 * <p>
 * After these services are up, the engine loads the remaining game scripts.
 * These scripts will register engine plugins, by calling
 * Engine.registerPlugin(String classFile). This is where the plugin object is
 * constructed.
 * <p>
 * The engine then calls the activate method on all registered plugins.
 * You can call Engine.getPlugin(String pluginName) to retrieve a registered plugin.
 * 
 * @see HookManager
 */
public class EnginePlugin implements MessageCallback, StatusMapCallback {
    /**
     * No args constructor (deprecated).
     */
    public EnginePlugin() {
    }
    
    /**
     * constructor.  
     * @param name Plugin name, must be unique.
     */
    public EnginePlugin(String name) {
        this.setName(name);
    }
    
    /**
     * constructor.
     * 
     * @param name Plugin name which is used in broadcasting 
     * plugin availability and also used
     * to find the plugin with {@link Engine#getPlugin(String)}
     * 
     * @param activateHook called by the Engine when the plugin starts up
     */
    public EnginePlugin(String name, PluginActivateHook activateHook) {
        this(name);
        try {
            registerActivateHook(activateHook);
        }
        catch(MVRuntimeException e) {
            throw new RuntimeException("registerActivateHook failed", e);
        }
    }
    
    /**
     * Returns the name of the plugin. Engine.registerPlugin() will call this
     * method and associate the name with the plugin, which can be retrieved by
     * calling Engine.getPlugin()
     * 
     * @return the name of this plugin
     * @see #setName(String)
     */
    public String getName() {
        return this.name;
    }

    /**
     * Sets the name of the plugin.
     * The name is also used when the plugin is activated.
     * 
     * @param name plugin name, must be unique
     * @see #getName()
     */
    protected void setName(String name) {
        this.name = name;
    }

    /**
     * Setter for pluginType.
     * @param pluginType The string type of the plugin.
     */
    public void setPluginType(String pluginType) {
        this.pluginType = pluginType;
    }

    /**
     * Getter for pluginType.
     * @return The string type of the plugin
     */
    public String getPluginType() {
        return pluginType;
    }
    
    /** Override to provide plugin status.
    */
    public Map<String, String> getStatusMap() {
        return null;
    }
    
    /**
     * Return the status string for the plugin
     * @return A string of name=value pairs, comma separated, or the empty string
     */
    public String getPluginStatus() {
        return Engine.makeStringFromMap(getStatusMap());
    }

    /**
     * Setter for pluginInfo string.
     * @param pluginInfo The new string pluginInfo of the plugin.
     */
    public void setPluginInfo(String pluginInfo) {
        this.pluginInfo = pluginInfo;
    }

    /**
     * Getter for pluginInfo.
     * @return The string pluginInfo of the plugin
     */
    public String getPluginInfo() {
        return pluginInfo;
    }

    /**
     * Setter for percentCPULoad int.
     * @param percentCPULoad The int percentCPULoad of the plugin.
     */
    public void setPercentCPULoad(int percentCPULoad) {
        this.percentCPULoad = percentCPULoad;
    }

    /**
     * Getter for percentCPULoad.
     * @return The int percentCPULoad of the plugin
     */
    public int getPercentCPULoad() {
        return percentCPULoad;
    }

    /**
     * Called on startup to initialize the plugin. You will
     * typically extend this method to create subscriptions, and send out
     * messages to other plugins. You should extend this to do your
     * own logic if needed, no need to call super.onActivate()
     * <p>
     * Plugins are typically registered in config scripts calling
     * Engine.registerPlugin(String classFile).
     */
    public void registerActivateHook(PluginActivateHook hook) {
        lock.lock();
        try {
            activateHookList.add(hook);
        }
        finally {
            lock.unlock();
        }
    }

    private LinkedList<PluginActivateHook> activateHookList = new LinkedList<PluginActivateHook>();


    /**
     * called by the engine to initialize the plugin.
     * you should not override this method since it sends out
     * messages such as the PluginStatusMessage.
     * this method calls onActivate() which you can override
     * to implement your logic, therefore no need to call super.activate()
     * you can alternately use registerActivateHook() if you 
     * are not extending the EnginePlugin class.
     */
    public void activate() {
        if (Log.loggingDebug)
            Log.debug("EnginePlugin.activate: plugin=" + getName());
        
        pluginState = PluginStateMessage.BuiltInStateStarting;
        
        // Establish the hook to dump all the threads in the process.
        // Since there can be multiple plugins in a process, we make
        // sure that there is only one subscription per process.
        dumpAllThreadSubscriptionLock.lock();
        try {
            if (dumpAllThreadSubscription == null) {
                MessageTypeFilter filter = new MessageTypeFilter();
                filter.addType(MSG_TYPE_DUMP_ALL_THREAD_STACKS);
                // Subscribe for DumpAllStackMessages
                dumpAllThreadSubscription = Engine.getAgent().createSubscription(filter, this);
                if (Log.loggingDebug)
                    Log.debug("EnginePlugin.activate: plugin=" + getName() + ", created createSubscription for dumpAllStacks");
                // Register the hook manager
                getHookManager().addHook(MSG_TYPE_DUMP_ALL_THREAD_STACKS,
                    new DumpAllStacksMessageHook());
                if (Log.loggingDebug)
                    Log.debug("EnginePlugin.activate: registered DumpAllStacksMessageHook");
            }
        }
        finally {
            dumpAllThreadSubscriptionLock.unlock();
        }

        Engine.getAgent().getDomainClient().awaitPluginDependents(
                getPluginType(), getName());

        // Assume the plugin will be available after activating.  Activate
        // methods and hooks and reset to false.
        pluginAvailable = true;

        // call out custom onActivate and all activateHooks
        // so that they can actually be ready for incoming messages
        // as a result of the statusmessages.
        onActivate();      
        if (Log.loggingDebug)
            Log.debug("EnginePlugin.activate: plugin=" + getName() + ", onActivate complete, calling activateHooks");

        createManagementObject();

        lock.lock();
        try {
            for (PluginActivateHook activateHook : activateHookList) {
                activateHook.activate();
            }
        }
        finally {
            lock.unlock();
        }
        if (Log.loggingDebug)
            Log.debug("EnginePlugin.activate: plugin=" + getName() + ", activate hooks called");

        if (pluginAvailable)
            Engine.getAgent().getDomainClient().pluginAvailable(
                    getPluginType(), getName());
  
    }
   
    public boolean getPluginAvailable()
    {
        return pluginAvailable;
    }

    public void setPluginAvailable(boolean avail)
    {
        if (! pluginAvailable && avail)
            Engine.getAgent().getDomainClient().pluginAvailable(
                    getPluginType(), getName());
        pluginAvailable = avail;
    }

    /**
     * for developers extending the EnginePlugin object, it may
     * be easier to use the onActivate() method which gets
     * called when the plugin is being activated by the Engine.
     * this is an alternative to calling registerActivateHook()
     * 
     * @author cedeno
     *
     */
    public void onActivate() {    
    }
    
    public interface PluginActivateHook {
        public void activate();
    }
    
    /**
     * processing plugin status message - used internally by the
     * EnginePlugin.  developers should use registerPluginStatusHook()
     * to integrate with plugin status changes without having to know
     * about messages.
     * @author cedeno
     *
     */
    class PluginStateMessageHook implements Hook {
        PluginStateMessageHook() {
        }

        public boolean processMessage(Message m, int flags) {
            PluginStateMessage msg = (PluginStateMessage) m;
            if (Log.loggingDebug)
                Log.debug("PluginStateHook: got plugin status message for plugin: "
                          + msg.getPluginName() + ", state=" + msg.getState() + ", msg=" + m);
            pluginStateMap.put(msg.getPluginName(), msg.getState());
            
            // if its not a response message already,
            // send out a response message so the other plugin knows
            // we're up also
            if (msg.getTargetSession() == null) {
                if (pluginState == PluginStateMessage.BuiltInStateAvailable) {
                    Log.debug("PluginStateHook: not a response msg, sending out a response");
                    PluginStateMessage respMsg = new PluginStateMessage(
                            EnginePlugin.this.getName(), pluginState);
                    respMsg.setTargetSession(msg.getSenderName());
                    respMsg.setTargetPluginName(name);
                    Engine.getAgent().sendBroadcast(respMsg);
                }
            }
            else {
                Log.debug("PluginStatusHook: is response message");
            }
            return true;
        }

        private static final long serialVersionUID = 1L;
    }

    class DumpAllStacksMessageHook implements Hook {
	DumpAllStacksMessageHook() {
        }

        public boolean processMessage(Message m, int flags) {
            if (Log.loggingDebug)
                Log.debug("DumpAllStacksMessageHook: received MSG_TYPE_DUMP_ALL_STACKS");
            // Get all the stacks
            Engine.dumpAllThreadStacks();
            return true;
        }
    }
    
    /** 
     * THIS plugin's state
     */
    String pluginState = PluginStateMessage.BuiltInStateUnknown;    
    
    /**
     * See EnginePlugin.PluginStateMessage.BuiltInState for some possible values
     * @param pluginName
     * @return pluginStateMap; null if state is unknown or not available 
     */
    public String getPluginState(String pluginName) {
        return pluginStateMap.get(pluginName);
    }
    
    // map of pluginname -> status
    private Map<String, String> pluginStateMap = Collections.synchronizedMap(new HashMap<String,String>());

    protected void setMessageHandler(MessageCallback handler)
    {
        messageHandler = handler;
    }

    protected MessageCallback getMessageHandler()
    {
        return messageHandler;
    }

    /**
     * helper method to create a subscription based on a message type
     * and associate it with a hook for processing.
     * 
     * @param hook hook processes any incoming messages
     * @param msgType the message type to match.  see Message.setMsgType()
     */
    public void createSubscription(Hook hook, MessageType msgType, int flags) {
        // register the hook
        getHookManager().addHook(msgType, hook);
     
        // create the filter
        MessageTypeFilter filter = new MessageTypeFilter(msgType);

        // create the subscription
        Engine.getAgent().createSubscription(filter, this, flags);
    }
    
    /**
     * Callback method for subscriptions which by default calls hooks registered
     * with this plugin's hookmanager via EnginePlugin.getHookManager().
     * Typically, you won't need to extend/replace this method unless you want
     * to process messages in some way other than using the HookManager.
     * 
     * @see HookManager
     */
    public void handleMessage(Message msg, int flags)
    {
        if (messageHandler != null)
            messageHandler.handleMessage(msg,flags);
        else
            handleMessageImpl(msg,flags);
    }

    /**
     * Iterate through the Hooks associated with the message type,
     * calling each one's processMessage() method until one of them
     * returns false.
     * @param msg The message to be handled
     * @param flags The flags associated with the message.
     */
    protected void handleMessageImpl(Message msg, int flags)
    {
        MessageType msgType = msg.getMsgType();
        List<Hook> hooks = hookManager.getHooks(msgType);

        if (Log.loggingDebug)
            Log.debug("EnginePlugin.handleMessage: got msg id " + msg.getMsgId() + ", matching " + hooks.size() + " hooks for msgtype "
                      + msgType);

        long timer = 0;
        if (Log.loggingDebug)
            timer = System.currentTimeMillis();
        for (Hook hook : hooks) {
            if (!hook.processMessage(msg, 0)) {
                break;
            }
        }
        if (Log.loggingDebug) {
            long elapsed = System.currentTimeMillis() - timer;
            Log.debug("EnginePlugin.handleMessage: processed msg " + msg.getMsgId() + ", type=" + msgType + ", time in ms=" + elapsed);
            engPluginMeter.add(elapsed);
        }
    }

    /**
     * A Runnable utility class used to handle callbacks that require
     * a thread pool because it takes a long time or blocks.
     */
    class QueuedMessage implements Runnable
    {
        /**
         * Builds a QueuedMessage instance.
         * @param message The message to be queued for execution.
         * @param flags The message flags.
         */
        QueuedMessage(Message message, int flags)
        {
            this.message = message;
            this.flags = flags;
        }

        /**
         * The run method calls handleMessageImpl on the message and
         * flags.
         */
        public void run() {
            try {
                handleMessageImpl(message,flags);
            }
            catch (Exception ex) {
                Log.exception("handleMessageImpl", ex);
                if ((flags & MessageCallback.RESPONSE_EXPECTED) != 0)
                    Engine.getAgent().sendResponse(new ExceptionResponseMessage(message,ex));
            }
        }

        Message message;
        int flags;
    }

    /**
     * This class uses an Executor thread pool to process
     * QueuedMessage instances.
     */
    class PoolMessageHandler implements MessageCallback, ThreadFactory
    {
        PoolMessageHandler() {
            executor = Executors.newFixedThreadPool(10,this);
        }

        /**
         * Create a QueuedMessage instance, grab a thread from the
         * thread pool, and have it execute the QueuedMessage.
         */
        public void handleMessage(Message msg, int flags) {
            executor.execute(new QueuedMessage(msg,flags));
        }

        public Thread newThread(Runnable runnable)
        {
            return new Thread(runnable, getName()+"-"+threadCount++);
        }

        Executor executor;
        int threadCount = 1;
    }

    /**
     * @see #registerPluginNamespaces
     * @return a copy of the collection of Namespace objects
     * associated with the plugin.
     */
    public Collection<Namespace> getPluginNamespaces() {
        lock.lock();
        try {
            return new ArrayList<Namespace>(localNamespaces);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Register the plugin namespace, used to identify the kind of
     * subobject created by this plugin.
     * @param namespace  The namespace object associated with the plugin
     * @param genSubObjHook  Called when sub-object is created in this
     *  plugin's namespace.  Triggered by a generate sub-object message
     *  from the object manager.
     */
    public void registerPluginNamespace(Namespace namespace,
            GenerateSubObjectHook genSubObjHook)
    {
        List<Namespace> namespaces = new LinkedList<Namespace>();
        namespaces.add(namespace);
        
        registerPluginNamespaces(namespaces, genSubObjHook,
            null, null,
            null, null,
            null, null);
    }

    /**
     * Register plugin namespaces, used to identify the kind of
     * subobject created by this plugin.
     * @param namespaces The namespaces associated with the plugin
     * @param genSubObjHook  Called when sub-object is created in this
     *  plugin's namespace.  Triggered by a generate sub-object message
     *  from the object manager.
     */
    public void registerPluginNamespaces(Collection<Namespace> namespaces, 
            GenerateSubObjectHook genSubObjHook)
    {
        registerPluginNamespaces(namespaces, genSubObjHook,
            null, null,
            null, null,
            null, null);
    }

    /**
     * Register plugin namespaces, used to identify the kind of
     * subobject created by this plugin.
     * @param namespaces The namespaces associated with the plugin
     * @param genSubObjHook  Called when sub-object is created in this
     *  plugin's namespace.  Triggered by a generate sub-object message
     *  from the object manager.
     * @param selectionFilter A namespace filter for sub-object generation
     *  and loading.  This filter should match sub-objects this plugin
     *  should own.  The filter can be as simple as matching namespace
     *  (one plugin owns all the sub-objects), or more sophisticated.
     *  For example matching odd or even numbered OIDs (sub-objects
     *  split between two plugin instances), or matching locations
     *  (sub-objects split by geometry).
     * @param subObjectFilter namespace filter for sub-object manipulation
     *  messages.  This filter should match sub-objects loaded or generated
     *  into this plugin.
     */
    public void registerPluginNamespaces(Collection<Namespace> namespaces, 
            GenerateSubObjectHook genSubObjHook,
            INamespaceFilter selectionFilter,
            INamespaceFilter subObjectFilter)
    {
        registerPluginNamespaces(namespaces, genSubObjHook,
            null, null,
            null, null,
            selectionFilter, subObjectFilter);
    }

    /**
     * Register namespaces this plugin is authoritative
     * for.  This method will create a subscription for
     * GenerateSubObjectMessage, and add the passed in hook to process
     * these messages.  You should not create a second subscription.
     * This method also creates a subscription for message type
     * ObjectManagerClient.MSG_TYPE_SUB_OBJECT_DEPS_READY which
     * the object manager sends when object creation
     * dependencies are resolved.  This method registers an internal
     * hook to process dependency messages.
     * 
     * @param namespaces Plugin's namespaces.
     * @param genSubObjHook  Called when sub-object is created in this
     *  plugin's namespace.  Triggered by a generate sub-object message
     *  from the object manager.
     * @param loadSubObjHook Override load sub obj hook, 'null' to use
     *  default hook.
     * @param saveSubObjHook Override save sub obj hook, 'null' to use
     *  default hook.
     * @param unloadSubObjHook Override unload sub obj hook, 'null' to use
     *  default hook.
     * @param deleteSubObjHook Override delete sub obj hook, 'null' to use
     *  default hook.
     * @param selectionFilter A namespace filter for sub-object generation
     *  and loading.  This filter should match sub-objects this plugin
     *  should own.  The filter can be as simple as matching namespace
     *  (one plugin owns all the sub-objects), or more sophisticated.
     *  For example matching odd or even numbered OIDs (sub-objects
     *  split between two plugin instances), or matching locations
     *  (sub-objects split by geometry).
     * @param subObjectFilter namespace filter for sub-object manipulation
     *  messages.  This filter should match sub-objects loaded or generated
     *  into this plugin.
     */
    public void registerPluginNamespaces(Collection<Namespace> namespaces, 
            GenerateSubObjectHook genSubObjHook,
            Hook loadSubObjHook,
            Hook saveSubObjHook,
            Hook unloadSubObjHook,
            Hook deleteSubObjHook,
            INamespaceFilter selectionFilter,
            INamespaceFilter subObjectFilter)
    {
        if (Log.loggingDebug) {
            String s = "";
            for (Namespace namespace : namespaces) {
                if (s != "")
                    s += ",";
                s += namespace;
            }
            Log.debug("EnginePlugin.registerPluginNamespaces: namespaces " + s);
        }
        lock.lock();
        try {
            localNamespaces = new ArrayList<Namespace>(namespaces);

            if (saveSubObjHook == null) {
                saveSubObjHook = new SaveSubObjHook(this);
            }
            if (loadSubObjHook == null) {
                loadSubObjHook = new LoadSubObjHook(this);
            }
            if (unloadSubObjHook == null) {
                unloadSubObjHook = new UnloadSubObjHook(this);
            }
            if (deleteSubObjHook == null) {
                deleteSubObjHook = new DeleteSubObjHook(this);
            }

            if (selectionFilter == null) {
                selectionFilter = new NamespaceFilter();
            }
            if (subObjectFilter == null) {
                subObjectFilter = new NamespaceFilter();
            }

            getHookManager().addHook(ObjectManagerClient.MSG_TYPE_GENERATE_SUB_OBJECT, genSubObjHook);            
            getHookManager().addHook(ObjectManagerClient.MSG_TYPE_SET_SUBPERSISTENCE, new SubPersistenceHook());
            getHookManager().addHook(ObjectManagerClient.MSG_TYPE_SAVE_SUBOBJECT, saveSubObjHook);
            getHookManager().addHook(ObjectManagerClient.MSG_TYPE_LOAD_SUBOBJECT, loadSubObjHook);
            getHookManager().addHook(ObjectManagerClient.MSG_TYPE_UNLOAD_SUBOBJECT, unloadSubObjHook);
            getHookManager().addHook(ObjectManagerClient.MSG_TYPE_DELETE_SUBOBJECT, deleteSubObjHook);
	    getHookManager().addHook(EnginePlugin.MSG_TYPE_GET_PROPERTY, new GetPropertyHook());
	    getHookManager().addHook(EnginePlugin.MSG_TYPE_SET_PROPERTY, new SetPropertyHook());
	    getHookManager().addHook(EnginePlugin.MSG_TYPE_SET_PROPERTY_NONBLOCK, new SetPropertyHook());
            
            // subscribe for sub object generation messages
            // matching this namespace
            selectionFilter.addType(ObjectManagerClient.MSG_TYPE_GENERATE_SUB_OBJECT);
            selectionFilter.addType(ObjectManagerClient.MSG_TYPE_LOAD_SUBOBJECT);
            selectionFilter.setNamespaces(namespaces);
            EnginePlugin.selectionFilter = selectionFilter;
            selectionSubscription =
                Engine.getAgent().createSubscription(selectionFilter, this,
                        MessageAgent.RESPONDER);

            // subscribe for sub object manipulation
            subObjectFilter.addType(ObjectManagerClient.MSG_TYPE_SAVE_SUBOBJECT);
            subObjectFilter.addType(ObjectManagerClient.MSG_TYPE_UNLOAD_SUBOBJECT);
            subObjectFilter.addType(ObjectManagerClient.MSG_TYPE_DELETE_SUBOBJECT);
            subObjectFilter.addType(ObjectManagerClient.MSG_TYPE_SET_SUBPERSISTENCE);
            subObjectFilter.addType(MSG_TYPE_GET_PROPERTY);
            subObjectFilter.addType(MSG_TYPE_SET_PROPERTY);
            subObjectFilter.setNamespaces(namespaces);
            subObjectSubscription =
                Engine.getAgent().createSubscription(subObjectFilter, this,
                        MessageAgent.RESPONDER);

            //## this is broken; the WM wants a SubObjectFilter so it's
            //## oid-specific.  We could instantiate subObjectFilter.getClass(),
            //## but we won't know how the passed-in object was initialized.
            //## Real fix is to support MessageType-specific responder
            //## flags for MessageTypeFilter and PerceptionFilter
	    NamespaceFilter propertyNonblockFilter =
                new NamespaceFilter(namespaces);
	    propertyNonblockFilter.addType(MSG_TYPE_SET_PROPERTY_NONBLOCK);
	    propertySubscription =
                Engine.getAgent().createSubscription(propertyNonblockFilter, this);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Register a hook to be called when one of this plugin's sub object instances is saved.
     * @param namespace  The namespace in which this plugin's sub object instances reside.
     * @param saveHook the hook to be used when your sub object is marked dirty and
     * needs to be saved.  it should handle deep copy if needed.  this will be called
     * by the persistenceManager when it wants to save the object.
     * if saveHook is null, it will use the default handler which does not do a deep copy
     */
    public void registerSaveHook(Namespace namespace, SaveHook saveHook) {
        Engine.getPersistenceManager().registerSaveHook(namespace, saveHook);
    }
    
    /**
     * Register a hook to be called when one of this plugin's sub object instances is loaded.
     * @param namespace  The namespace in which this plugin's sub object instances reside.
     * @param loadHook  The hook to be called when the sub object is loaded.
     */
    public void registerLoadHook(Namespace namespace, LoadHook loadHook) {
        loadHookMap.put(namespace, loadHook);
    }

    /**
     * Register a hook to be called when one of this plugin's sub object instances is unloaded.
     * @param namespace  The namespace in which this plugin's sub object instances reside.
     * @param unloadHook  The hook to be called when the sub object is unloaded.
     */
    public void registerUnloadHook(Namespace namespace, UnloadHook unloadHook) {
        unloadHookMap.put(namespace, unloadHook);
    }

    /**
     * Register a hook to be called when one of this plugin's sub object instances is deleted.
     * @param namespace The namespace in which this plugin's sub object instances reside.
     * @param deleteHook The hook to be called when the sub object is deleted.
     */
    public void registerDeleteHook(Namespace namespace, DeleteHook deleteHook) {
        deleteHookMap.put(namespace, deleteHook);
    }

    /**
     * The interface definition to be satisfied by load hook objects, containing
     * the onLoad method.
     */
    public interface LoadHook {
        /**
         * A LoadHook/Namespace pair must be registered by calling
         * EnginePlugin.registerLoadHook().  The onLoad() method will
         * be called after loading the sub object.  The entity
         * argument can be queried for it's namespace.
         */
        public void onLoad(Entity entity);
    }

    /**
     * The interface definition to be satisfied by unload hook objects,
     * containing the onUnload method.
     */
    public interface UnloadHook {
        /**
         * A UnloadHook/Namespace pair must be registered by calling
         * EnginePlugin.registerUnloadHook().  The onUnload() method will
         * be called after unloading the sub object.  The entity
         * argument can be queried for it's namespace.
         */
        public void onUnload(Entity entity);
    }

    /**
     * The interface definition to be satisfied by delete hook objects,
     * containing the onDelete method.
     */
    public interface DeleteHook {
        /**
         * A DeleteHook/Namespace pair must be registered by calling
         * EnginePlugin.registerDeleteHook().  The onDelete() method will
         * be called after unloading the sub object.  The entity
         * argument can be queried for it's namespace.
         */
        public void onDelete(Entity entity);

        /** Called when sub-object is deleted when it's not loaded.
            May be called with a non-existant oid.  Implementations
            should silently ignore this case.
        */
        public void onDelete(Long oid, Namespace namespace);
    }

    /**
     * The interface definition to be satisfied by save hook objects, containing
     * the onSave method.
     */
    public interface SaveHook {
        /**
         * A SaveHook/Namespace pair must be registered by calling
         * EnginePlugin.registerSaveHook().  The onSave() method will
         * be called by the PersistenceManager prior to saving the sub
         * object.
         */
        public void onSave(Entity e, Namespace namespace);
    }

    /**
     * A collection of Namespace objects associated with the plugin,
     * initialized by the call to
     * EnginePlugin.registerPluginNamespaces().
     * @see #registerPluginNamespaces(Namespace, GenerateSubObjectHook)
     */
    Collection<Namespace> localNamespaces = null;
    
    /**
     * Creates a subscription for transfer messages.  Transfer messages
     * get sent when an object switches ownership from one plugin
     * to another instance of that plugin usually due to load
     * balancing.
     */
    public void registerTransferHook(Filter filter, Hook hook) {
        getHookManager().addHook(MSG_TYPE_TRANSFER_OBJECT, hook);
        Engine.getAgent().createSubscription(filter, this);
    }
    
    /**
     * Transfers this object from this instance of this plugin, to another instance
     * of this plugin.
     * 
     * This is done by sending out a transfer message, which the other plugin
     * is listening for when it called registerTransferHook(), probably in its
     * onActivate() method.
     * 
     * The caller is responsible for releasing any resources associated
     * with the object.
     * 
     * This call blocks until it gets a response message.
     * 
     * @param propMap - we pass in HashMap because its serializable unlike Map
     * @param entity
     * 
     * @return success value
     */
    public boolean transferObject(HashMap<String, Serializable> propMap, Entity entity)  {
        TransferObjectMessage transferMessage = new TransferObjectMessage(propMap, entity);
        return Engine.getAgent().sendRPCReturnBoolean(transferMessage);
    }
    
    /**
     * Base class for message filters used for the transfer object
     * subscription.  You must extend the abstract method matchesMap().
     * @see #registerTransferHook(Filter, Hook)
     */
    abstract public static class TransferFilter extends MessageTypeFilter {
        public TransferFilter() {
            super();
            addType(MSG_TYPE_TRANSFER_OBJECT);
        }
        
        /**
         * Called when we know that the message type matches the filter,
         * to determine if the other message fields match the filter.
         * @param msg
         * @return true if the message matches.
         */
        public boolean matchRemaining(Message msg) {
            if (msg instanceof TransferObjectMessage) {
                TransferObjectMessage transferMsg = (TransferObjectMessage)msg;
                Map propMap = transferMsg.getPropMap();
                return matchesMap(propMap, msg);
            }
            else
                return false;
        }
        
        /**
         * Matches the property map to the message
         * @param propMap passed in by the caller of the message
         * @param msg the message sent by the caller
         * @return true if the message matched
         */
        abstract public boolean matchesMap(Map propMap, Message msg);
    }
    
    MVMeter engPluginMeter = new MVMeter("EnginePluginOnMessageMeter");
    
    /**
     * Returns the HookManager associated with this plugin. The
     * HookManager's job is to match incoming subscription message
     * with a hook that handles the message. Hooks are matched by
     * calling msg.getMsgType() and comparing it with the hook's
     * registered message type.
     * 
     * @return HookManager associated with this plugin.
     */
    public HookManager getHookManager() {
        return hookManager;
    }

    /**
     * Returns the ObjectLockManager associated with this plugin.
     * Plugins do NOT share ObjectLockManagers, therefore locking one
     * does not affect the other.
     */
    protected ObjectLockManager getObjectLockManager() {
        return this.objLockManager;
    }
    
    /**
     * Subscription for the object manager plugin SubObjectDepsReady 
     * message.
     */
    private Long subObjectDepReadySub = null;

    /**
     * A lock serializing access to depsOutstanding collection.
     */
    private Lock depLock = LockFactory.makeLock("subObjectDepReadySub");

    /**
     * Add the list of namespaces paired with the hook to call to the
     * list of outstanding dependencies for the oid.  The caller of
     * the hook will remove elements from the namespace list for the
     * oid.
     */
    Map<Long, Map<Namespace, Hook>> depsOutstanding = Collections.synchronizedMap(new HashMap<Long, Map<Namespace, Hook>>());
    
    /**
     * Associates the oid/namespace pair with the callbacks in the depsOutstanding map.
     */
    private void addToDepsOutstanding(Long oid, Namespace namespace, Hook callback) {
        Map<Namespace, Hook> deps = depsOutstanding.get(oid);
        if (deps == null) {
            deps = new HashMap<Namespace, Hook>();
            depsOutstanding.put(oid, deps);
        }
        Hook previousHook = deps.get(namespace);
        if (previousHook != null)
            Log.error("EnginePlugin.addToNamespaceDeps: for oid " + oid + " and namespace " + namespace + ", hook already exists");
        else {
            deps.put(namespace, callback);
            logDepsOutstanding("addToDepsOutstanding", oid, namespace);
        }
    }
    
    /**
     * Removes the oid/namespace pair from the depsOutstanding map.
     */
    private Hook removeFromDepsOutstanding(Long oid, Namespace namespace) {
        logDepsOutstanding("removeFromDepsOutstanding", oid, namespace);
        Map<Namespace, Hook> deps = depsOutstanding.get(oid);
        if (deps == null)
            Log.error("EnginePlugin.removeFromDepsOutstanding: Map<Namespace, Hook> for oid " + oid + " not found");
        else {
            Hook hook = deps.remove(namespace);
            if (hook == null)
                Log.error("EnginePlugin.removeFromDepsOutstanding: for oid " + oid +
                ", namespace " + namespace + ", deps do not contain Hook");
            else {
                if (deps.size() == 0)
                    deps.remove(oid);
                return hook;
            }
        }
        return null;
    }

    /**
     * Overloading of sendSubObjectResponse used when there are no dependent Namespaces
     * @param origMsg the original message to which we are responding
     * @param oid oid of the sub object that was created
     * @param oidNamespace the Namespace in which the new subobject should be created
     */
    public void sendSubObjectResponse(Message origMsg, Long oid, Namespace oidNamespace) {
        if (Log.loggingDebug)
            Log.debug("EnginePlugin.sendSubObjectResponse: origMsg=" + origMsg + ", oid=" + oid + ", namespace " + oidNamespace);
        sendSubObjectResponse(origMsg, oid, oidNamespace, (LinkedList<Namespace>)null, null);
    }
    
    /**
     * Overloading of sendSubObjectResponse that takes a single dependent Namespace
     * @param origMsg the original message to which we are responding
     * @param oid oid of the sub object that was created
     * @param oidNamespace the Namespace in which the new subobject should be created
     * @param depNamespace the dependent Namespace object
     * @param callback the callback when we dependencies are all ready.k
     */
    public void sendSubObjectResponse(Message origMsg, Long oid, Namespace oidNamespace,
            Namespace depNamespace, Hook callback) {
        if (Log.loggingDebug)
            Log.debug("EnginePlugin.sendSubObjectResponse: origMsg=" + origMsg + ", oid=" + oid + 
                    ", oidNamespace " + oidNamespace + ", depNs=" + depNamespace);

        LinkedList<Namespace> deps = new LinkedList<Namespace>();
        deps.add(depNamespace);
        sendSubObjectResponse(origMsg, oid, oidNamespace, deps, callback);
    }
    
    /**
     * The plugin calls this after generating a sub object to let the
     * object manager know the sub object is ready.  In the case that
     * the plugin needs other namespace data before creating the sub
     * object, you can pass in a list of dependent namespaces, and the
     * object manager plugin will send a notification message when all
     * dependent sub objects have been created.  Also sets the newly
     * created entity's namespace
     * @param msg the original message to which we are responding
     * @param oid oid of the sub object that was created; the same as the master oid
     * @param oidNamespace the Namespace in which the new subobject should be created
     * @param depNamespaces collection of dependent Namespace objects
     * @param callback the callback when we dependencies are all ready.
     */
    public void sendSubObjectResponse(Message msg, Long oid, Namespace oidNamespace,
            LinkedList<Namespace> depNamespaces, Hook callback) {
        INamespaceMessage origMsg = (INamespaceMessage)msg;
        if (Log.loggingDebug)
            Log.debug("EnginePlugin.sendSubObjectResponse: origMsg=" + origMsg + ", oid=" + oid + ", depNs=" + depNamespaces + ", hook=" + callback);

        depLock.lock();
        try {
            if (Log.loggingDebug)
                Log.debug("sendSubObjectResponse: oid=" + oid);
            // if we have dependencies, then we must make sure we have subscribed to
            // the MSG_TYPE_SUB_OBJECT_DEPS_READY msg
            if ((depNamespaces != null) && (! depNamespaces.isEmpty())) {
                if (Log.loggingDebug)
                    Log.debug("sendSubObjectResponse: oid=" + oid + ", depNamespaces=" + depNamespaces.toString());
                if (subObjectDepReadySub == null) {
                    if (Log.loggingDebug)
                        Log.debug("sendSubObjectResponse: oid=" + oid + ", need to create deps ready sub");
                    // associate a hook
                    getHookManager().addHook(
                        ObjectManagerClient.MSG_TYPE_SUB_OBJECT_DEPS_READY,
                        new SubObjectDepsReadyHook());
                    
                    // create a subscription
                    Filter namespaceFilter = 
                        new NamespaceFilter(
                            ObjectManagerClient.MSG_TYPE_SUB_OBJECT_DEPS_READY,
                            getPluginNamespaces());
                    
                    subObjectDepReadySub =
                        Engine.getAgent().createSubscription(namespaceFilter,
                            this, MessageAgent.RESPONDER);
                    Log.debug("sendSubObjectResponse: created depsReady sub");
                }
                else {
                    Log.debug("sendSubObjectResponse: depsReady sub already in place");
                }
                
                // add the sub object oid to a local set (meaning its not ready yet)
                // when we get a SubObjectDepsReadyMessage, the hook
                // -- SubObjectDepsReadyHook -- will remove it from the set
                if (callback == null) {
                    Log.error("EnginePlugin.sendSubObjectResponse: callback is null");
                }
                addToDepsOutstanding(oid, oidNamespace, callback);
            }
            
            // set the sub object's namespace
            Namespace namespace = origMsg.getNamespace();
            if (namespace == null) {
                Log.error("EnginePlugin.sendSubObjectResponse: namespace is null");
            }
            Entity subObj = EntityManager.getEntityByNamespace(oid, namespace);
            if (subObj == null) {
                Log.error("EnginePlugin.sendSubObjectResponse: subObj is null for oid " + oid);
            }
            if (Log.loggingDebug)
                Log.debug("EnginePlugin.sendSubObjectResponse: set entity " + subObj.getOid() + " namespace to " + namespace);
        }
        finally {
            depLock.unlock();
        }

        // send a response message
        Engine.getAgent().sendObjectResponse(msg, depNamespaces);
    }
    
    /**
     * The object manager plugin received a setPersistence message and
     * it sent out a SubPersistenceMessage to each namespace in the
     * master object.  We are handling it here, and we set the
     * persistence property on the object.
     */
    class SubPersistenceHook implements Hook {
        public boolean processMessage(Message m, int flags) {
            
            ObjectManagerClient.SetPersistenceMessage msg = (ObjectManagerClient.SetPersistenceMessage) m;
            Long masterOid = msg.getSubject();
            Namespace namespace = msg.getNamespace();
            Boolean persistVal = msg.getPersistVal();
            
            if (Log.loggingDebug)
                Log.debug("SubPersistenceHook: masterOid=" + masterOid + ", namespace=" + namespace);
            
            // find the sub object
            Entity subObj = EntityManager.getEntityByNamespace(masterOid, namespace);
            if (subObj == null) {
                Log.error("SubPersistenceHook: could not find sub object");
                Engine.getAgent().sendBooleanResponse(m, false);
            }

            if (subObj.getPersistenceFlag() == persistVal) {
                Engine.getAgent().sendBooleanResponse(m, true);
                return true;
            }

            // set the property on the object, also set object to dirty if persistent
            subObj.setPersistenceFlag(persistVal);
            if (Log.loggingDebug)
                Log.debug("SubPersistenceHook: masterOid=" + masterOid + ", set persist flag on subOid" + subObj.getOid() + ", to val=" + persistVal);
            if (persistVal) {
                Engine.getPersistenceManager().setDirty(subObj);
                Log.debug("SubPersistenceHook: set subobject dirty");
            }

            if (! persistVal) {
                Engine.getDatabase().deleteObjectData(masterOid, namespace);
            }

            // send a response
            Engine.getAgent().sendBooleanResponse(m, true);
            return true;
        }
    }
    
    /**
     * A hook to process the GenerateSubObjectMessage, sent by the object manager.
     */
    public abstract static class GenerateSubObjectHook implements Hook {
	
        /**
         * Bind the plugin instance to the plugin data member.
         * @param plugin The plugin instance creating the hook.
         */
        public GenerateSubObjectHook(EnginePlugin plugin) {
	    this.plugin = plugin;
	}

        /**
         * Generates the sub-object based on the template, namespace
         * and oid contained in the GenerateSubObjectMessage by
         * calling the plugin's generateSubObject method, and then
         * sends the response by calling sendSubObjectResponse().
         * @param m The GenerateSubObjectMessage message
         * @param flags The message flags
         */
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.GenerateSubObjectMessage msg = (ObjectManagerClient.GenerateSubObjectMessage) m;
            Template template = msg.getTemplate();
            Namespace namespace = msg.getNamespace();
            Long masterOid = msg.getSubject();
            if (masterOid == null) {
                Log.error("GenerateSubObjectHook: no master oid");
                return false;
            }
            SubObjData subObjData = generateSubObject(template, namespace, masterOid);
            Entity newObj = EntityManager.getEntityByNamespace(masterOid, namespace);
            if (newObj == null) {
                throw new RuntimeException("could not find newly created subobject, oid=" + masterOid + ", namespace " + namespace);
            }

            // send a response message
            plugin.sendSubObjectResponse(m,
                    masterOid, 
                    namespace,
                    subObjData.namespaces, 
                    subObjData.dependencyHook);            
            return true;
        }
       
        /**
         * Returns a SubObjData instance.
         * @param template The template used to create the sub-object
         * @return A SubObjData instance, when all dependencies are
         * resolved.  (The list of dependencies can be null).
         */
        public abstract SubObjData generateSubObject(Template template, Namespace namespace, Long masterOid);

	/**
         * The plugin instance.
         */
        public EnginePlugin plugin = null;
    }
    
    /**
     * Returned by GenerateSubObject hook's generateSubObject()
     * method.  This is not thread safe.
     */
    public static class SubObjData {
        
        /**
         * No-arg constructor, used if there are no Namespace dependencies.
         */
        public SubObjData() {
        }
        
        /**
         * Constructor used in the case there is a single Namespace dependency.
         * @param namespace The Namespace dependency.
         * @param dependencyHook The hook to call when the dependency is satisfied.
         */
        public SubObjData(Namespace namespace, Hook dependencyHook) {
            this.namespaces = new LinkedList<Namespace>();
            if (namespace != null)
                namespaces.add(namespace);
            this.dependencyHook = dependencyHook;
        }
        
        /**
         * Constructor used in the case there are multiple Namespace dependencies.
         * @param namespaces The Namespace dependency list.
         * @param dependencyHook The hook to call when each dependency is satisfied.
         */
        public SubObjData(LinkedList<Namespace> namespaces, Hook dependencyHook) {
            if (namespaces != null)
                this.namespaces = new LinkedList<Namespace>(namespaces);
            else
                this.namespaces = new LinkedList<Namespace>();
            this.dependencyHook = dependencyHook;
        }
        
        /**
         * The hook called when each dependency is satisfied.
         */
        public Hook dependencyHook = null;
        
        /**
         * The list of dependent namespaces.
         */
        LinkedList<Namespace> namespaces = null;
    }
    
    /**
     * A hook called to persist a sub-object.
     */
    public static class SaveSubObjHook implements Hook {
        public SaveSubObjHook(EnginePlugin plugin) {
            this.pluginRef = plugin;
        }
        
        /**
         * Persists the sub-object identified by the oid and namespace
         * in the OIDNamespaceMessage, and sends a boolean response,
         * true if the persist succeeded and false if it could not
         * find the sub-object
         * @param m The OIDNamespaceMessage message
         * @param flags The message flags
         * @return True if the sub-object was persisted; false otherwise.
         */
        public boolean processMessage(Message m, int flags) {
            OIDNamespaceMessage msg = (OIDNamespaceMessage) m;
            Long masterOid = msg.getSubject();
            Namespace namespace = msg.getNamespace();

            // find the sub object
            Entity subObj = EntityManager.getEntityByNamespace(masterOid, namespace);
            if (subObj == null) {
                Log.error("SaveSubObjHook: could not find sub object for masterOid " + masterOid);
                Engine.getAgent().sendBooleanResponse(m, false);
                return true;
            }
            
            if (! subObj.getPersistenceFlag()) {
                Log.error("SaveSubObjHook: ignoring save of non-persistent sub-object oid=" + masterOid + " namespace=" + namespace);
                Engine.getAgent().sendBooleanResponse(m, false);
                return true;
            }

            // save this entity
            if (Log.loggingDebug)
                Log.debug("SaveSubObjHook: saving object, subOid=" + subObj.getOid());
            Engine.getPersistenceManager().persistEntity(subObj);
            
            if (Log.loggingDebug)
                Log.debug("SaveSubObjHook: saved object subOid=" + subObj.getOid());
            Engine.getAgent().sendBooleanResponse(m, true);
            
            return true;
        }
        
        // so we can get to plugin stuff, like the subobjmgr
        EnginePlugin pluginRef = null;
    }
    
    /**
     * A hook called to load sub-object.
     */
    public static class LoadSubObjHook implements Hook {
        public LoadSubObjHook(EnginePlugin plugin) {
            this.pluginRef = plugin;
        }
        
        /**
         * Process the LoadSubObjectMessage, containing the sub-object
         * oid and namespace, registering the entity by namespace, and
         * calling the load hook if there is one.
         * @param m The OIDNamespaceMessage message
         * @param flags The message flags
         * @return True if the entity was found; false otherwise.
         */
        public boolean processMessage(Message m, int flags) {
            ObjectManagerClient.LoadSubObjectMessage msg = (ObjectManagerClient.LoadSubObjectMessage) m;
            Long masterOid = msg.getSubject();
            Namespace namespace = msg.getNamespace();
            
            // load from database
            Entity entity = null;
            try {
                entity = Engine.getDatabase().loadEntity(masterOid, namespace);
            }
            catch (MVRuntimeException e) {
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }

            if (entity == null) {
                Log.error("LoadSubObjHook: no such entity with oid " + masterOid + " and namespace " + namespace);
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }
            
            // register with the sub object manager
            EntityManager.registerEntityByNamespace(entity, namespace);
            
            // call the load object hook
            LoadHook loadHook = pluginRef.loadHookMap.get(namespace);
            if (loadHook != null) {
                loadHook.onLoad(entity);
            }

            if (Log.loggingDebug)
                Log.debug("LoadSubObjHook: called loadhook, loaded object oid " + masterOid + " and namespace " + namespace);
            Engine.getAgent().sendBooleanResponse(m, true);
            
            return true;
        }
        
        EnginePlugin pluginRef = null;
    }
    
    /**
     */
    public static class UnloadSubObjHook implements Hook {
        public UnloadSubObjHook(EnginePlugin plugin) {
            this.plugin = plugin;
        }
        
        /**
         * Process the UnloadSubObjectMessage, containing the sub-object
         * oid and namespace, removing the entity by namespace.
         * @param m The UnloadSubObjectMessage message
         * @param flags The message flags
         * @return n/a
         */
        public boolean processMessage(Message m, int flags)
        {
            ObjectManagerClient.UnloadSubObjectMessage msg;
            msg = (ObjectManagerClient.UnloadSubObjectMessage) m;

            long oid = msg.getSubject();
            Namespace namespace = msg.getNamespace();

            Entity entity = EntityManager.getEntityByNamespace(oid,namespace);
            if (entity == null) {
                Log.error("UnloadSubObjectMessage: no such entity oid="+oid+
                        " ns="+namespace);
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }

            boolean rc = EntityManager.removeEntityByNamespace(oid,namespace);

            if (rc) {
                // call the unload object hook
                UnloadHook unloadHook = plugin.unloadHookMap.get(namespace);
                if (unloadHook != null) {
                    try {
                        unloadHook.onUnload(entity);
                    }
                    catch (Exception e) {
                        Log.exception("UnloadHook.onUnload oid="+oid+" "+
                                unloadHook.getClass().getName()+
                                "",e);
                    }
                }
            }

            if (Log.loggingDebug)
                Log.debug("UnloadSubObjectMessage: unloaded oid="+oid+
                        " ns="+namespace+" result="+rc);

            if (entity.getPersistenceFlag() &&
                    Engine.getPersistenceManager().isDirty(entity)) {
                Engine.getPersistenceManager().persistEntity(entity);
            }

            Engine.getAgent().sendBooleanResponse(msg, rc);

            return true;
        }

        EnginePlugin plugin;
    }

    /**
     */
    public static class DeleteSubObjHook implements Hook {
        public DeleteSubObjHook(EnginePlugin plugin) {
            this.plugin = plugin;
        }
        
        /**
         * Process the DeleteSubObjectMessage, containing the sub-object
         * oid and namespace, remove the entity by namespace and
         * call DeleteHook (if any).  The default implementation does
         * not delete from the database because the ObjectManager will
         * delete all sub-object database rows.
         * @param m The DeleteSubObjectMessage message
         * @param flags The message flags
         * @return n/a
         */
        public boolean processMessage(Message m, int flags)
        {
            ObjectManagerClient.DeleteSubObjectMessage msg;
            msg = (ObjectManagerClient.DeleteSubObjectMessage) m;

            long oid = msg.getSubject();
            Namespace namespace = msg.getNamespace();

            DeleteHook deleteHook = plugin.deleteHookMap.get(namespace);

            Entity entity = EntityManager.getEntityByNamespace(oid,namespace);
            if (entity == null) {
                Log.error("DeleteSubObjectMessage: no such entity oid="+oid+
                        " ns="+namespace);

                // Call delete hook for the not-loaded case
                if (deleteHook != null) {
                    try {
                        deleteHook.onDelete(oid,namespace);
                    }
                    catch (Exception e) {
                        Log.exception("DeleteHook.onDelete oid="+oid+
                                " ns="+namespace+" "+
                                deleteHook.getClass().getName(), e);
                    }
                }
                
                Engine.getAgent().sendBooleanResponse(msg, false);
                return false;
            }

            entity.getLock().lock();
            try {
                if (entity.isDeleted()) {
                    Log.debug("DeleteSubObjectMessage: already deleted oid="+oid);
                    return true;
                }
                entity.setDeleted();
            } finally {
                entity.getLock().unlock();
            }

            boolean rc = EntityManager.removeEntityByNamespace(oid,namespace);
            if (rc && deleteHook != null) {
                try {
                    deleteHook.onDelete(entity);
                }
                catch (Exception e) {
                    Log.exception("DeleteHook.onDelete oid="+oid+" "+
                            deleteHook.getClass().getName(), e);
                }
            }
            
            if (Log.loggingDebug)
                Log.debug("DeleteSubObjectMessage: deleted oid="+oid+
                        " ns="+namespace+" result="+rc);

            Engine.getAgent().sendBooleanResponse(msg, rc);

            return true;
        }

        EnginePlugin plugin;
    }

    /**
     * A utility method to log depsOutstanding for the given oid
     */
    protected void logDepsOutstanding(String prefix, Long oid, Namespace ns) {
        if (Log.loggingDebug) {
            String s = "";
            Map<Namespace, Hook> deps = depsOutstanding.get(oid);
            if (deps == null)
                s = "None";
            else {
                for (Namespace dep : deps.keySet()) {
                    if (s != "")
                        s += ",";
                    s += dep.getName();
                }
            }
            Log.debug(prefix + ": masterOid " + oid + ", namespace " + ns + ", deps " + s);
        }
    }
    
    /**
     * The object manager is telling us that all dependencies for the
     * sub object are done.  This implementation updates a map and
     * also signals a condition for anyone waiting for it.
     */
    class SubObjectDepsReadyHook implements Hook {

        /**
         * Process the SubObjectDepsReadyMessage, calling
         * removeFromDepsOutstanding() to record the fact that the
         * deps are satisfied, getting back the deps ready hook.
         * Finally call the hook.
         * @param m The SubObjectDepsReadyMessage message
         * @param flags The message flags
         * @return True if the oid was found in depOutstanding; false otherwise.
         */
        public boolean processMessage(Message m, int flags) {

            // TODO: Why doesn't this assert depLock?  The tip server
            // doesn't assert it either, but it seems like it should
            // be locking.
            ObjectManagerClient.SubObjectDepsReadyMessage msg =
                (ObjectManagerClient.SubObjectDepsReadyMessage) m;
            Long masterOid = msg.getSubject();
            Namespace ns = msg.getNamespace();

            logDepsOutstanding("SubObjectDepsReadyHook.processMessage", masterOid, ns);

            // find the appropriate sub object id
            Hook cb = removeFromDepsOutstanding(masterOid, ns);
            if (cb == null) {
                Log.error("SubObjectDepsReadyHook: the sub oid was not in the wait list: " + masterOid + ", namespace " + ns);
                return false;
            }

            // execute the callback
            cb.processMessage(msg, 0);

            // send a response message telling the object manager plugin
            // that we are done processing our deps ready message
            Engine.getAgent().sendBooleanResponse(m, Boolean.TRUE);
            return true;
        }
    }
    
    /**
     * Get property message for an sub object in a particular
     * namespace.  Capable of getting a single property, or a set of
     * property values.
     */
    public static class GetPropertyMessage extends OIDNamespaceMessage {

        /**
         * No-arg constructor, required by marshalling
         */
        public GetPropertyMessage() {
            super();
        }
        
        /**
         * GetPropertyMessage constructor for cases where we want the
         * value of a single key.
         * @param oid The oid of the object.
         * @param namespace The Namespace containing the sub-object.
         * @param key The key whose value will be fetched.
         */
        public GetPropertyMessage(Long oid, Namespace namespace, String key) {
            super(MSG_TYPE_GET_PROPERTY, oid, namespace);
            addKey(key);
        }
        
        /**
         * GetPropertyMessage constructor for cases where we want the
         * values of a list of keys.
         * @param oid The oid of the object.
         * @param namespace The Namespace containing the sub-object.
         * @param keys The list of keys whose values will be fetched.
         */
        public GetPropertyMessage(Long oid, Namespace namespace, List<String> keys) {
            super(MSG_TYPE_GET_PROPERTY, oid, namespace);
            this.keys = keys;
        }

        /**
         * Method to produce a human-readable version of the key list
         */
        public String toString() {
            String s = "";
            for (Serializable key : keys) {
                if (s != "")
                    s += ",";
                s += key;
            }
            return "[GetPropertyMessage oid=" + getSubject() + ", keys=" + s + ", super=" + super.toString() + "]";
        }

        /**
         * Add another key to the list of keys for which this message
         * will fetch values.
         * @param key The key to be added to the list of keys for
         * which values will be fetched.
         */
        public void addKey(String key) {
            if (!keys.contains(key))
                keys.add(key);
        }
        
        /**
         * Remove a key from the list of keys for which this message
         * will fetch values.
         * @param key The key to be removed from the list of keys for
         * which values will be fetched.
         * @return The key, if it was previously in the list of keys, else null.
         */
        public Serializable removeKey(Serializable key) {
            return keys.remove(key);
        }
        
        /**
         * Return the list of keys for which this message will fetch
         * values
         * @return The list of keys for which values will be fetched.
         */
        public List<String> getKeys() {
            return keys;
        }

        /**
         * The list of keys in the message.
         */
        List<String> keys = new LinkedList<String>();

        private static final long serialVersionUID = 1L;
    }

    /**
     * Set property message for an sub object in a particular
     * namespace.  Capable of setting a single property, or a set of
     * property values.
     */
    public static class SetPropertyMessage extends OIDNamespaceMessage {

    	/**
         * No-arg constructor, required by marshalling.
         */
        public SetPropertyMessage() {
            super();
        }
        
        /**
         * Constructor for the case that we want to set the value of a single key.
         * @param oid  The oid of the subobject whose key/value pair will be set.
         * @param namespace  The namespace containing the subobject
         * @param key  The String key
         * @param val  The Serializable value
         * @param reqResponse  True if the message requires a response; false otherwise
         */
        public SetPropertyMessage(Long oid, Namespace namespace, String key,
                Serializable val, boolean reqResponse)
        {
            super(MSG_TYPE_SET_PROPERTY, oid, namespace);
            if (! reqResponse)
                setMsgType(MSG_TYPE_SET_PROPERTY_NONBLOCK);
            propMap.put(key, val);
            setRequestResponse(reqResponse);
        }

        /**
         * Constructor for the case that we want to set the values of more than one key.
         * @param oid  The oid of the subobject whose key/value pair will be set.
         * @param namespace  The namespace containing the subobject.
         * @param propMap  The map of keys to values.
         * @param reqResponse  True if the message requires a response; false otherwise.
         */
        public SetPropertyMessage(Long oid, Namespace namespace,
                Map<String, Serializable> propMap, boolean reqResponse)
        {
            super(MSG_TYPE_SET_PROPERTY, oid, namespace);
            if (! reqResponse)
                setMsgType(MSG_TYPE_SET_PROPERTY_NONBLOCK);
            this.propMap = propMap;
            setRequestResponse(reqResponse);
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
            return propMap.get(key);
        }
        
        /**
         * Return true of the message contains the key argument; false otherwise
         * @param key  The key to look for
         * @return True if the key is contained in the list of keys;
         * false otherwise.
         */
        public boolean containsKey(String key) {
            return propMap.containsKey(key);
        }
        
        /**
         * Associate the value with the key.
         * @deprecated Use {@link #setProperty(String key, Serializable val)} instead
         */
        public void put(String key, Serializable val) {
            setProperty(key, val);
        }

        /**
         * Add (or replace) the key/value pair
         * @param key  The string key
         * @param val  The Serializable value to be put.
         * @return the previous value associated with the key, or null.
         */
        public Serializable setProperty(String key, Serializable val) {
            return propMap.put(key, val);
        }
        
        /**
         * Return the property map of keys and values
         * @return The property map.
         */
        public Map<String, Serializable> getPropMap() {
            return propMap;
        }
            
        /**
         * Set whether this message requires a response.
         * @param val The boolean value which, if true, indicates that
         * the message requires a response
         */
        public void setRequestResponse(boolean val) {
            reqResponse = val;
            if (reqResponse)
                setMsgType(MSG_TYPE_SET_PROPERTY);
            else
                setMsgType(MSG_TYPE_SET_PROPERTY_NONBLOCK);
        }
        
        /**
         * Get whether this message requires a response.
         * @return true if the message requires a response.
         */
        public boolean getRequestResponse() { return this.reqResponse; }

        /**
         * Message data member that says if the message requires a response.
         */
        private boolean reqResponse = false;
        
        /**
         * The message's property map.
         */
        Map<String, Serializable> propMap = new HashMap<String, Serializable>();
 
        private static final long serialVersionUID = 1L;
    }

    /**
     * A hook called whenever a GetPropertyMessage is received.
     */
    class GetPropertyHook implements Hook {
        
        /**
         * Casts the msg argument to a GetPropertyMessage, and calls
         * the getPropertyImpl method.  Plugins can choose to override
         * the default implementation of getPropertyImpl.
         * @param m The GetPropertyMessage message
         * @param flags The message flags
         * @return The return value of getPropertyImpl().
         */
        public boolean processMessage(Message msg, int flags) {
            GetPropertyMessage rMsg = (GetPropertyMessage) msg;
	    return getPropertyImpl(rMsg);
        }
    }

    /**
     * Process the GetPropertyMessage, looking up the entity by oid
     * and namespace, and getting the properties identified by the
     * list of keys in the message.  Plugins can choose to override
     * the default implementation of getPropertyImpl.
     * @param msg The GetPropertyMessage instance.
     * @return True if the entity could be found; false otherwise.
     */
    protected boolean getPropertyImpl(GetPropertyMessage msg) {
	Namespace ns = msg.getNamespace();
	Long oid = msg.getSubject();

	List<Serializable> vals = new LinkedList<Serializable>();
	Entity e = EntityManager.getEntityByNamespace(oid, ns);
	if (e == null) {
	    Log.error("EnginePlugin.GetPropertyHook: could not find subobj for oid " + oid);
            Engine.getAgent().sendObjectResponse(msg, null);
            return false;
        }
        List<String> keys = msg.getKeys();
        for (String key : keys)
            vals.add(e.getProperty(key));
	
	Engine.getAgent().sendObjectResponse(msg, vals);
	if (Log.loggingDebug) {
	    String s = "";
            for (int i=0; i<keys.size(); i++) {
                String key = keys.get(i);
                Serializable val = vals.get(i);
                if (s != "")
                    s += ",";
                s += key + "=" + val;
            }
            Log.debug("EnginePlugin.GetPropertyHook: sent response, oid=" + oid + ", " + s);
        }
	return true;
    }

    /**
     * A hook called whenever a SetPropertyMessage is received.
     */
    class SetPropertyHook implements Hook {

        /**
         * Casts the msg argument to a SetPropertyMessage, and calls
         * the setPropertyImpl method.  Plugins can choose to override
         * the default implementation of setPropertyImpl.
         * @param m The SetPropertyMessage message
         * @param flags The message flags
         * @return The return value of setPropertyImpl().
         */
        public boolean processMessage(Message msg, int flags) {
            SetPropertyMessage rMsg = (SetPropertyMessage) msg;
	    return setPropertyImpl(rMsg);
        }
    }

    /**
     * Process the SetPropertyMessage, looking up the entity by oid
     * and namespace, and setting the properties identified by the
     * list of keys in the message.  In addition, sends a
     * PropertyMessage with the keys and values gotten from the
     * SetPropertyMessage.  Plugins can choose to override the default
     * implementation of setPropertyImpl.
     * @param msg The SetPropertyMessage instance.
     * @return True if the entity could be found; false otherwise.
     */
    protected boolean setPropertyImpl(SetPropertyMessage msg) {
	Long oid = msg.getSubject();
	Namespace ns = msg.getNamespace();
        String s = "";
        if (Log.loggingDebug) {
            for (Map.Entry<String, Serializable> entry : msg.getPropMap().entrySet()) {
                String key = entry.getKey();
                Serializable val = entry.getValue();
                if (s != "")
                    s += ",";
                s += key + "=" + val;
            }
            if (Log.loggingDebug)
                Log.debug("EnginePlugin.setPropertyImpl: oid=" + oid + " props " + s);
        }
        boolean reqResp = msg.getRequestResponse();

        List<Serializable> previousVals = null;
        if (reqResp)
            previousVals = new LinkedList<Serializable>();
	Entity entity = EntityManager.getEntityByNamespace(oid, ns);
	if (entity == null) {
	    Log.error("EnginePlugin.setPropertyImpl: could not find obj / subobj for oid " + oid + ", namespace " + ns);
            if (reqResp)
                Engine.getAgent().sendObjectResponse(msg, previousVals);
            return false;
	}
	PropertyMessage propMsg = new PropertyMessage(oid);
        for (Map.Entry<String, Serializable> entry : msg.getPropMap().entrySet()) {
            String key = entry.getKey();
            Serializable newValue = entry.getValue();
            Serializable previousValue = entity.setProperty(key, newValue);
            propMsg.setProperty(key, newValue);
            if (reqResp)
                previousVals.add(previousValue);
        }
	if (reqResp) {
	    Engine.getAgent().sendObjectResponse(msg, previousVals);
	}
	Engine.getAgent().sendBroadcast(propMsg);

	// set the entity as dirty since we want to save this new property to the db
	Engine.getPersistenceManager().setDirty(entity);
	return true;
    }

    /**
     * EnginePlugin entrypoint which gets the value of the object property associated with the given key 
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param key  The String key
     * @return The value associated with the key, or null if none exists.
     */
    public static Serializable getObjectProperty(Long oid, Namespace namespace,
						 String key) {
//long start = System.nanoTime();

        GetPropertyMessage msg = new GetPropertyMessage(oid, namespace, key);
        List<Serializable> vals = (List<Serializable>)Engine.getAgent().sendRPCReturnObject(msg);
//            long stop = System.nanoTime();
//            Log.info("GETPROP "+oid+" "+key+
//                        " time "+(stop-start)/1000 + " us");
        if (vals == null)
            return null;
        return vals.get(0);
    }
    
    /**
     * EnginePlugin entrypoint which gets a list of the values of the object properties associated with the given list of keys 
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param keys  A list of keys
     * @return  A list of values corresponding to those keys
     */
    public static List<Serializable> getObjectProperties(Long oid, Namespace namespace,
                 List<String> keys) {
        GetPropertyMessage msg = new GetPropertyMessage(oid, namespace, keys);
        return (List<Serializable>)Engine.getAgent().sendRPCReturnObject(msg);
    }

    /**
     * EnginePlugin entrypoint which gets a list of the values of the object properties associated with the 
     * list of keys supplied as varargs 
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param keys  A list of keys
     * @return  A list of values corresponding to those keys
     */
    public static List<Serializable> getObjectProperties(Long oid, Namespace namespace, String ... keys) {
        List<String> list = new LinkedList<String>();
        for (String s : keys)
            list.add(s);
        return getObjectProperties(oid, namespace, list);
    }

    /**
     * EnginePlugin entrypoint which sets the object property identified by the key/value pair.
     * This entrypoint waits for a response.
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param key  The String key
     * @param value  The Serializable value to which to set the object's key property
     * @return  The previous value associated with the key
     */
    public static Serializable setObjectProperty(Long oid, Namespace namespace,
						 String key, Serializable value) {
        SetPropertyMessage msg = new SetPropertyMessage(oid, namespace, key, value, true);
        return (Serializable)Engine.getAgent().sendRPCReturnObject(msg);
    }

    /**
     * EnginePlugin entrypoint which sets the object property identified by the key/value pair.
     * This entrypoint does not wait for a response.
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param key  The String key
     * @param value  The Serializable value to which to set the object's key property
     */
    public static void setObjectPropertyNoResponse(Long oid, Namespace namespace,
						   String key, Serializable value) {
        SetPropertyMessage msg = new SetPropertyMessage(oid, namespace, key, value, false);
        Engine.getAgent().sendBroadcast(msg);
    }

    /**
     * EnginePlugin entrypoint which sets the object properties identified by the 
     * key/value pair in the propMap argument.
     * This entrypoint waits for a response.
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param propMap  The map of keys and values
     * @return  A list of the previous values associated with the keys.
     */
    public static List<Serializable> setObjectProperties(Long oid, Namespace namespace, Map<String, Serializable> propMap) {
        SetPropertyMessage msg = new SetPropertyMessage(oid, namespace, propMap, true);
        return (List<Serializable>)Engine.getAgent().sendRPCReturnObject(msg);
    }

    /**
     * EnginePlugin entrypoint which sets the object properties identified by the 
     * alternating key and value arguments supplied as varargs.
     * This entrypoint waits for a response.
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param keysAndValues  The varargs sequence of key and value arguments
     * @return  A list of the previous values associated with the keys.
     */
    public static List<Serializable> setObjectProperties(Long oid, Namespace namespace, Serializable ... keysAndValues) {
        Map<String, Serializable> propMap = processKeysAndValues("setObjectProperties", keysAndValues);
        if (propMap == null)
            return new LinkedList<Serializable>();
        else
            return setObjectProperties(oid, namespace, propMap);
    }

    /**
     * EnginePlugin entrypoint which sets the object properties identified by the 
     * key/value pair in the propMap argument.
     * This entrypoint does not wait for a response.
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param propMap  The map of keys and values
     */
    public static void setObjectPropertiesNoResponse(Long oid, Namespace namespace, Map<String, Serializable> propMap) {
        SetPropertyMessage msg = new SetPropertyMessage(oid, namespace, propMap, false);
        Engine.getAgent().sendBroadcast(msg);
    }
    
    /**
     * EnginePlugin entrypoint which sets the object properties identified by the 
     * alternating key and value arguments supplied as varargs.
     * This entrypoint does not wait for a response.
     * @param oid  The oid of the object
     * @param namespace  The namespace containing the object
     * @param keysAndValues  The varargs sequence of key and value arguments
     */
    public static void setObjectPropertiesNoResponse(Long oid, Namespace namespace, Serializable ... keysAndValues) {
        Map<String, Serializable> propMap = processKeysAndValues("setObjectPropertiesNoResponse", keysAndValues);
        if (propMap == null)
            return;
        else 
            setObjectPropertiesNoResponse(oid, namespace, propMap);
    }

    /**
     * A utility function to turn an array of keys and values into a propMap.
     */
    protected static Map<String, Serializable> processKeysAndValues(String what, Serializable [] keysAndValues) {
        int len = keysAndValues.length;
        if ((len & 1) != 0) {
            Log.dumpStack("Odd number of args to " + what);
            return null;
        }
        else if (len == 0)
            return null;
        else {
            Map<String, Serializable> propMap = new HashMap<String, Serializable>();
            for (int i=0; i<len; i+=2)
                propMap.put((String)keysAndValues[i], keysAndValues[i+1]);
            return propMap;
        }
    }
    
    /**
     * Message used by plugins to announce various control 
     * states.  for example, when a plugin comes up, it
     * by default sends out an "Available" state so that
     * other plugins know about its existance.
     */
    public static class PluginStateMessage extends Message {
        
        /**
         * No-args constructor, required by marshalling.
         */
        public PluginStateMessage() {
            super();
            setMsgType(MSG_TYPE_PLUGIN_STATE);
        }
        
        /**
         * The constructor used by plugins.
         * @param pluginName The string name of the plugin
         * @param state The state of the plugin, one of
         * BuiltInStateAvailable, BuiltInStateUnknown and
         * BuiltInStateStarting.
         */
        public PluginStateMessage(String pluginName, String state) {
            super();
            setMsgType(MSG_TYPE_PLUGIN_STATE);
            setPluginName(pluginName);
            setState(state);
        }
        
        /**
         * Setter for pluginName.
         * @param pluginName The string name of the plugin.
         */
        public void setPluginName(String pluginName) {
            this.pluginName = pluginName;
        }

        /**
         * Getter for pluginName.
         * @return The string name of the plugin
         */
        public String getPluginName() {
            return pluginName;
        }
        
        /**
         * Setter for plugin state.
         * @param state The string plugin state.
         */
        public void setState(String state) {
            this.state = state;
        }
        
        /**
         * Getter for plugin state.
         * @return The string plugin state.
         */
        public String getState() {
            return state;
        }
        
        /**
         * Setter for the string sessionId.  When this is a response
         * messsage, it needs to indicate both the sessionID and
         * plugin name its responding to.
         * @param sessionId The string sessionId.
         */
        public void setTargetSession(String sessionId) {
            this.sessionId = sessionId;
        }

        /**
         * Getter for the sessionId
         * @return The string sessionId.
         */
        public String getTargetSession() {
            return sessionId;
        }
        
        /**
         * Setter for the string targetPluginName.  When this is a
         * response message, it needs to indicate what the target
         * plugin for the response, also needs the session id.
         * @param pluginName The string target plugin name.
         */
        public void setTargetPluginName(String pluginName) {
            this.targetPluginName = pluginName;
        }

        /**
         * Getter for the string targetPluginName.
         * @return The string target plugin name.
         */
        public String getTargetPluginName() {
            return targetPluginName;
        }

        private String pluginName;
        private String state;
        private String targetPluginName;
        private String sessionId;
        
        /**
         * The string that indicates that the plugin has finished
         * initialization.
         */
        public static final String BuiltInStateAvailable = "Available";

        /**
         * The string that indicates that the plugin hasn't been heard
         * from.
         */
        public static final String BuiltInStateUnknown = "Unknown";

        /**
         * The string that indicates that the plugin has notified us
         * that it has started initialization.
         */
        public static final String BuiltInStateStarting = "Starting";

        private static final long serialVersionUID = 1L;
    }

    /**
     * The message used to transfer control of an object from one
     * world manager to another.
     */
    public static class TransferObjectMessage extends Message {

        /**
         * No-args constructor, required by marshalling.
         */
        public TransferObjectMessage() {
            super(MSG_TYPE_TRANSFER_OBJECT);
        }
        
        /**
         * The normal constructor.
         * @param propMap A map of properties to be applied by the
         * target world manager.
         * @param entity The entity to be transferred.
         */
        public TransferObjectMessage(HashMap<String, Serializable> propMap, Entity entity) {
            super(MSG_TYPE_TRANSFER_OBJECT);
            setPropMap(propMap);
            setEntity(entity);
        }
       
        /**
         * Getter for the propMap.
         * @return The propMap.
         */
        public HashMap<String, Serializable> getPropMap() {
            return propMap;
        }

        /**
         * Setter for the propMap.
         * @param propMap The propMap value.
         */
        public void setPropMap(HashMap<String, Serializable> propMap) {
            this.propMap = propMap;
        }

        /**
         * Getter for the entity.
         * @return The entity.
         */
        public Entity getEntity() {
            return entity;
        }

        /**
         * Setter for the entity.
         * @param entity The entity value.
         */
        public void setEntity(Entity entity) {
            this.entity = entity;
        }
        
        private HashMap<String, Serializable> propMap;
        private Entity entity;
        
        private static final long serialVersionUID = 1L;
    }

    private MessageCallback messageHandler = new PoolMessageHandler();
    private HookManager hookManager = new HookManager();
    private Map<Namespace, LoadHook> loadHookMap =
        Collections.synchronizedMap(new HashMap<Namespace, LoadHook>());
    private Map<Namespace, UnloadHook> unloadHookMap =
        Collections.synchronizedMap(new HashMap<Namespace, UnloadHook>());
    private Map<Namespace, DeleteHook> deleteHookMap =
        Collections.synchronizedMap(new HashMap<Namespace, DeleteHook>());

    private String name = null;
    private String pluginType;
    private String pluginInfo;
    private int percentCPULoad = 0;
    private boolean pluginAvailable = false;

    protected Lock lock = LockFactory.makeLock("EnginePluginLock");
    private ObjectLockManager objLockManager = new ObjectLockManager();
    
    public static MessageType MSG_TYPE_PLUGIN_STATE = MessageType.intern("mv.PLUGIN_STATE");

    public static MessageType MSG_TYPE_GET_PROPERTY = MessageType.intern("mv.GET_PROPERTY");

    public static MessageType MSG_TYPE_SET_PROPERTY = MessageType.intern("mv.SET_PROPERTY");

    public static MessageType MSG_TYPE_SET_PROPERTY_NONBLOCK = MessageType.intern("mv.SET_PROPERTY_NONBLOCK");


    protected static Lock dumpAllThreadSubscriptionLock = LockFactory.makeLock("DumpAllThreadsLock");

    // Record the subscriptions since we might want to close them on shutdown
    protected static Long dumpAllThreadSubscription = null;
    protected static Long pluginStateSubscription = null;
    protected static Long subObjectSubscription = null;
    protected static Long selectionSubscription = null;
    protected static INamespaceFilter selectionFilter = null;
    protected static Long saveSubObjectSubscription = null;
    protected static Long loadSubObjectSubscription = null;
    protected static Long unloadSubObjectSubscription = null;
    protected static Long deleteSubObjectSubscription = null;
    protected static Long setSubObjectPersistenceSubscription = null;
    protected static Long propertySubscription = null;
    
    /**
     * Message from proxy server, telling all processes to dump their
     * thread stacks to the log.
     */
    public static MessageType MSG_TYPE_DUMP_ALL_THREAD_STACKS = MessageType.intern("mv.DUMP_ALL_THREAD_STACKS");

    /**
     * Message requesting transfer of an entity to a different world
     * manager.
     */
    public static final MessageType MSG_TYPE_TRANSFER_OBJECT = MessageType.intern("mv.TRANSFER_OBJECT");

    private void createManagementObject() {
        if (Engine.getManagementAgent() == null)
            return;
        Object mbean = createMBeanInstance();
        if (mbean == null)
            return;
        try {
            ObjectName name = new ObjectName("net.multiverse:plugin="+getName());
            Engine.getManagementAgent().registerMBean(mbean, name);
            Log.debug("Registered "+getName()+" with JMX management agent");
        } catch (javax.management.JMException ex) {
            Log.exception("createManagementObject: exception in registerMBean", ex);
        }
    }
 
    protected Object createMBeanInstance() {
        return null;
    }

}
