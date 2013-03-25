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

#region Using directives

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using log4net;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;
using Axiom.Animating;
using Axiom.Input;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.ParticleSystems;

using Multiverse.Gui;
using Multiverse.Generator;
using Multiverse.Network;
using Multiverse.CollisionLib;
using Multiverse.Config;
using Multiverse.Lib.LogUtil;
using Multiverse.Voice;

using TimeTool = Multiverse.Utility.TimeTool;

#endregion

namespace Multiverse.Base
{
	
	/// <summary>
	///		Delegate for object node events
	/// </summary>
	public delegate void ObjectEventHandler(object sender, ObjectNode objNode);

    public delegate void PlayerInitializedHandler(object sender, EventArgs args);

    /// <summary>
    /// Event argument class for global object property change events
    /// </summary>
    public class ObjectPropertyChangeEventArgs : EventArgs
    {
        protected long oid;
        protected string propName;

        public ObjectPropertyChangeEventArgs(long oid, string propName)
        {
            this.propName = propName;
            this.oid = oid;
        }

        public long Oid
        {
            get
            {
                return oid;
            }
        }

        public string PropName
        {
            get
            {
                return propName;
            }
        }
    }

    /// <summary>
    /// Delegate global for object property change events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ObjectPropertyChangeEventHandler(object sender, ObjectPropertyChangeEventArgs args);

    ///<summary>
    ///    A callback delegate for loading state changes.  
    ///<param name="msg">An ExtensionMessage instance. The msg has
    ///properties that may be meaningful to some handlers.
    ///</param>
    ///<param name="starting">True if the msg announces that we've
    ///started the loading process; false if the loading process is
    ///ending
    ///</param>
    ///</summary>
    public delegate void LoadingStateChangeHandler(ExtensionMessage msg, bool starting);

    public class WorldManager : IWorldManager
    {
		public class ObjectStub {
			public Vector3 scale;
			public bool followTerrain;
			public string name;
			public SceneNode sceneNode;
			public ObjectNodeType objectType;
            public Vector3 direction;
            public long lastInterp;
        }

		public class PreloadEntry {
			public bool loaded;
			public bool cancelled;
			public string entityName;
			public string meshFile;
            public List<SubmeshInfo> submeshList;
		}

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(WorldManager));
                    
        protected StaticGeometryHelper staticGeometryHelper;

		protected float CollisionRange = 50 * Client.OneMeter;
        protected long DirUpdateInterval = 100; // time in ticks between direction updates
        protected long OrientUpdateInterval = 100; // time in ticks between orientation updates
        protected long PlayerUpdateTimer = 5000; // time in ticks between sending player updates
        protected long MaxLoadTime = 10000; // maximum number of milliseconds to wait before aborting the load of a mesh

		long playerId = 0;
		bool playerInitialized = false;
        bool terrainInitialized = false;
        bool playerStubInitialized = false;
        bool loadCompleted = false;
        long nextLocalOid = int.MaxValue;
        bool dirLocOrientSupported = true;

		// The sound manager object
		SoundManager soundManager = null;

        // The scene manager object
        SceneManager sceneManager = null;
        // The network helper object
        NetworkHelper networkHelper = null;
        // The collision detector
        CollisionAPI collisionManager = null;
        // The player object
        Player player = null;

		Thread asyncLoadThread = null;
        object asyncLoadLock = null;
        bool shuttingDown = false;
        bool nowLoading = false;

		// Timer for updating the server with the player's position and other information
		System.Timers.Timer playerUpdateTimer;

		// Complete objects
		Dictionary<long, WorldEntity> nodeDictionary;

		// Stub object (got NewObject, but no ModelInfo)
		Dictionary<long, ObjectStub> objectStubs;

		// Intermediate entities - there are three states for entries here.
		// 1) No entry - we haven't requested a load of the entry yet
		// 2) Entry exists, loaded is false - we haven't loaded the mesh and materials
		// 3) Entry exists, loaded is true  - we have loaded the mesh and materials
		Dictionary<long, PreloadEntry> preloadDictionary;

		// A list of projectile interpolations to be handled
		List<TimeInterpolator> timeInterpolators;

        // A list of ambient sound sources        
        protected List<SoundSource> ambientSources = new List<SoundSource>();

        // keep track of whether fog has been set or not
        protected Animation fogAnim = null;
		
		public event ObjectEventHandler ObjectAdded;
		public event ObjectEventHandler ObjectRemoved;
        public event PlayerInitializedHandler PlayerInitializedEvent;

        public event LoadingStateChangeHandler OnLoadingStateChange;

		int entityCounter = 0;

        protected bool logTerrainConfig = false;

        protected Client client;

        // Dictionary used to keep track of decals that are sent from the server
        protected Dictionary<long, Axiom.SceneManagers.Multiverse.DecalElement> decals = new Dictionary<long,Axiom.SceneManagers.Multiverse.DecalElement>();

        // The LODSpec used in the call to SetWorldParams
        protected Axiom.SceneManagers.Multiverse.ILODSpec worldLODSpec = null;

        // Handlers for global object property change events
        Dictionary<string, List<ObjectPropertyChangeEventHandler>> objectPropertyChangeHandlers = new Dictionary<string,List<ObjectPropertyChangeEventHandler>>();

		VoiceManager voiceMgr = null;
        
        public WorldManager() {
		}

		public void Dispose() {
			shuttingDown = true;
			if (asyncLoadThread != null) {
				asyncLoadThread.Interrupt();
				asyncLoadThread.Join();
				asyncLoadThread = null;
			}
			if (networkHelper != null) {
				networkHelper.Dispose();
				networkHelper = null;
			}
            DisposeVoiceManager();
		}

		public void DisposeVoiceManager() {
            if (voiceMgr != null) {
                voiceMgr.onVoiceAllocation -= OnVoiceAllocation;
                voiceMgr.onVoiceDeallocation -= OnVoiceDeallocation;
                voiceMgr.Dispose();
                voiceMgr = null;
            }
        }
        
        public void Init(Window guiWindow, Client client) {
            this.client = client;

			// Set up the sound manager
			soundManager = new SoundManager();

			nodeDictionary = new Dictionary<long, WorldEntity>();

            // Set up the static geometry helpers
            staticGeometryHelper = new StaticGeometryHelper(this, StaticGeometryKind.BigOrLittleNode, 1000, 10, 1, 5);

			objectStubs = new Dictionary<long, ObjectStub>();

			preloadDictionary = new Dictionary<long, PreloadEntry>();
			
			timeInterpolators = new List<TimeInterpolator>();
			
			playerUpdateTimer = new System.Timers.Timer();
            playerUpdateTimer.Enabled = true;
            playerUpdateTimer.Interval = PlayerUpdateTimer;
            playerUpdateTimer.Elapsed +=
               new System.Timers.ElapsedEventHandler(this.SendPlayerData);

			asyncLoadLock = new object();
			asyncLoadThread = new Thread(new ThreadStart(this.AsyncLoadEntities));
            asyncLoadThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            asyncLoadThread.Name = "Async Resource Loader";
            // TODO: Ideally, this thread should start, and we shouldn't load from the
            //       OnFrameStarted method.  For some machines, we have to disable this.
			asyncLoadThread.Start();

            MessageDispatcher.Instance.RegisterPreHandler(WorldMessageType.ModelInfo,
                                                          new WorldMessageHandler(this.PreHandleModelInfo));
            MessageDispatcher.Instance.RegisterPreHandler(WorldMessageType.OldModelInfo,
                                                          new WorldMessageHandler(this.PreHandleModelInfo));

//            MessageDispatcher.Instance.RegisterHandler(MessageType.Location,
//                                                       new MessageHandler(this.HandleLocation));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Direction,
													   new WorldMessageHandler(this.HandleDirection));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.NewObject,
													   new WorldMessageHandler(this.HandleNewObject));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.FreeObject,
													   new WorldMessageHandler(this.HandleFreeObject));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Orientation,
													   new WorldMessageHandler(this.HandleOrientation));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.DirLocOrient,
                                                       new WorldMessageHandler(this.HandleDirLocOrient));
            //MessageDispatcher.Instance.RegisterHandler(MessageType.StatUpdate,
            //                                           new MessageHandler(this.HandleStatUpdate));
            //MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Animation,
            //                                           new WorldMessageHandler(this.HandleAnimation));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Attach,
													   new WorldMessageHandler(this.HandleAttach));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Detach,
													   new WorldMessageHandler(this.HandleDetach));
            //MessageDispatcher.Instance.RegisterHandler(MessageType.Sound,
            //                                           new MessageHandler(this.HandleSound));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AmbientLight,
													   new WorldMessageHandler(this.HandleAmbientLight));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.NewLight,
													   new WorldMessageHandler(this.HandleNewLight));
            //MessageDispatcher.Instance.RegisterHandler(MessageType.StateMessage,
            //                                           new MessageHandler(this.HandleState));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.TerrainConfig,
													   new WorldMessageHandler(this.HandleTerrainConfig));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.RegionConfig,
													   new WorldMessageHandler(this.HandleRegionConfig));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.SkyboxMaterial,
													   new WorldMessageHandler(this.HandleSkyboxMaterial));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.ModelInfo,
                                                       new WorldMessageHandler(this.HandleModelInfo));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.OldModelInfo,
                                                       new WorldMessageHandler(this.HandleModelInfo));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.RoadInfo,
													   new WorldMessageHandler(this.HandleRoadInfo));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Fog,
													   new WorldMessageHandler(this.HandleFogMessage));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AddParticleEffect,
                                                       new WorldMessageHandler(this.HandleAddParticleEffect));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.RemoveParticleEffect,
                                                       new WorldMessageHandler(this.HandleRemoveParticleEffect));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Extension,
													   new WorldMessageHandler(this.HandleProjectileTrackingMessages));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Extension,
													   new WorldMessageHandler(this.HandleClientParameterMessage));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.InvokeEffect,
                                                       new WorldMessageHandler(this.HandleInvokeEffect));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.MobPath,
                                                       new WorldMessageHandler(this.HandleMobPath));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.SoundControl,
                                                       new WorldMessageHandler(this.HandleSoundControl));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AuthorizedLoginResponse,
                                                       new WorldMessageHandler(this.HandleAuthorizedLoginResponse));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.NewDecal,
                                                       new WorldMessageHandler(this.HandleNewDecal));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.FreeDecal,
                                                       new WorldMessageHandler(this.HandleFreeDecal));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.LoadingState,
                                                       new WorldMessageHandler(this.HandleLoadingState));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Extension,
													   new WorldMessageHandler(this.HandleNewLoadingState));

            // Initialize the network helper
			networkHelper.Init();
			collisionManager = new CollisionAPI(client.logCollisions);

            PrimeMover.InitPrimeMover(this, collisionManager);
			
			if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
                Axiom.SceneManagers.Multiverse.SceneManager mvsm =
                    (Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;
                // Tell mvsm to create a tile manager, using half of CollisionRange
                mvsm.SetCollisionInterface(collisionManager, CollisionRange / 2);
            }

            ParameterRegistry.RegisterSubsystemHandlers("WorldManager", setParameterHandler, getParameterHandler);

            logTerrainConfig = client.LogTerrainConfig;
        }

        protected void OnVoiceAllocation(long oid, byte voiceNumber, bool positional) {
            MobNode node = (MobNode)GetObjectNode(oid);
            if (node == null)
                log.DebugFormat("WorldManager.OnVoiceAllocation: Could not find MobNode for oid " + oid);
            else {
                log.DebugFormat("WorldManager.OnVoiceAllocation: oid {0}, positional {1}, node {2}",
                    oid, positional, node);
                node.PositionalSoundEmitter = positional;
                
            }
        }
        
        protected void OnVoiceDeallocation(long oid) {
            MobNode node = (MobNode)GetObjectNode(oid);
            if (node == null)
                log.DebugFormat("WorldManager.OnVoiceAllocation: Could not find MobNode for oid " + oid);
            else {
                log.DebugFormat("WorldManager.OnVoiceDeallocation: oid {0}, positional {1}, node {2}",
                    oid, node.PositionalSoundEmitter, node);
                node.PositionalSoundEmitter = false;
            }
        }
        
        public void RequestShutdown(string message) {
            if (client != null)
                client.RequestShutdown(message);
        }

        private bool setParameterHandler(string parameterName, string parameterValue) {
            switch (parameterName) {
                case "CollisionRange":
                    try {
                        CollisionRange = float.Parse(parameterValue);
                    } catch (Exception) {
                        return false;
                    }
                    break;
                case "DirUpdateInterval":
                    try {
                        DirUpdateInterval = long.Parse(parameterValue);
                    } catch (Exception) {
                        return false;
                    }
                    break;
                case "OrientUpdateInterval":
                    try {
                        OrientUpdateInterval = long.Parse(parameterValue);
                    } catch (Exception) {
                        return false;
                    }
                    break;
                case "PlayerUpdateTimer":
                    try {
                        PlayerUpdateTimer = long.Parse(parameterValue);
                    } catch (Exception) {
                        return false;
                    }
                    break;
                case "MaxLoadTime":
                    try {
                        MaxLoadTime = long.Parse(parameterValue);
                    } catch (Exception) {
                        return false;
                    }
                    break;
			    case "SecondsBetweenFPSAverages":
                    try {
                        Root.Instance.SecondsBetweenFPSAverages = float.Parse(parameterValue);
                    } catch (Exception) {
                        return false;
                    }
                    break;
				default:
                    return false;
            }
            return true;
        }

        private bool getParameterHandler(string parameterName, out string parameterValue) {
            switch (parameterName) {
                case "Help":
                    parameterValue = ParameterHelp();
                    return true;
                case "CollisionRange":
                    parameterValue = CollisionRange.ToString();
                    break;
                case "DirUpdateInterval":
                    parameterValue = DirUpdateInterval.ToString();
                    break;
                case "OrientUpdateInterval":
                    parameterValue = OrientUpdateInterval.ToString();
                    break;
                case "PlayerUpdateTimer":
                    parameterValue = PlayerUpdateTimer.ToString();
                    break;
                case "MaxLoadTime":
                    parameterValue = MaxLoadTime.ToString();
                    break;
			    case "SecondsBetweenFPSAverages":
                    parameterValue = Root.Instance.SecondsBetweenFPSAverages.ToString();
                    break;
                default:
                    parameterValue = "";
                    return false;
            }
            return true;
        }

        private string ParameterHelp() {
            return
                "float CollisionRange: The range to check for collidable objects such as trees, in millimeters; default " +
                "is 50 meters (50000 millimeters).  This should not be set after startup." +
                "\n" +
                "long DirUpdateInterval: The wait time before sending an update of the player's direction to " +
                "the server, in milliseconds; default is 100 milliseconds" +
                "\n" +
                "long OrientUpdateInterval: The wait time before sending an update of the player's " +
                "orientation to the server, in milliseconds; default is 100 milliseconds" +
                "\n" +
                "long PlayerUpdateTimer: The maximum time before sending an update of the player's " +
				"position and orientation to the server, in milliseconds; default is 5000 milliseconds" +
                "\n" +
                "long MaxLoadTime: The maximum time before giving up on loading a model, in milliseconds; " +
				"default is 10,000 milliseconds" +
				"\n" +
				"float SecondsBetweenFPSAverages: The floating point interval, in seconds, between " +
				"computations of the frames-per-second rate; default is 1 second" +
                "\n";
        }
        public long GetLocalOid() {
            return nextLocalOid++;
        }

		public void ToggleRenderCollisionVolumes()
		{
			if (collisionManager != null)
				collisionManager.ToggleRenderCollisionVolumes(sceneManager, true);
		}

		public void RebuildStaticGeometryAfterLoading(ExtensionMessage msg, bool loadingState) {
            staticGeometryHelper.RebuildIfFinishedLoading(msg, loadingState);
        }
        
        public void EnableStaticGeometry(bool enable, bool force) 
        {
            staticGeometryHelper.Enabled = enable;
            staticGeometryHelper.Force = force;
        }
        
		/// <summary>
		///   Add an animation for the player - - called from bindings.xml
		/// </summary>
		public void RunAnimation(string animation)
		{
			RunAnimation(animation, 0.0f, 0.0f, 1.0f, true, true);
		}
			
		/// <summary>
		///   Add an animation for the player - - called from bindings.xml
		/// </summary>
		public void RunAnimation(string animation, float startOffset, float endOffset, 
                                 float speed, bool looping, bool clearOthers)
		{
			if (clearOthers) {
				Player.ClearAnimationQueue();
				log.InfoFormat("Cleared animation queue for: {0}", Player.Oid);
			}
			Player.QueueAnimation(animation, startOffset, endOffset, speed, 1.0f, looping);
		}

		/// <summary>
		///   Get the first preload entry that has not been loaded
		/// </summary>
		/// <returns></returns>
		private PreloadEntry GetUnloadedPreloadEntry() {
			PreloadEntry rv = null;
			Monitor.Enter(preloadDictionary);
			try {
				foreach (PreloadEntry entry in preloadDictionary.Values) {
					if (entry.loaded)
						continue;
					return entry;
				}
			} finally {
				Monitor.Exit(preloadDictionary);
			}
			return rv;
		}

		/// <summary>
		///   This method can be called from another thread and will preload the entity.
		///   TODO: Lock the underlying EntityManager.entityList
		/// </summary>
        [STAThread]
		public void AsyncLoadEntities() {
            while (!shuttingDown) {
                try {
                    Monitor.Enter(asyncLoadLock);
                    try {
                        try {
                            if (!LoadPreloadEntry()) {
                                Monitor.Wait(asyncLoadLock);
                                continue;
                            }
                        } catch (ThreadInterruptedException) {
                            continue;
                        } catch (ThreadAbortException e) {
                            LogUtil.ExceptionLog.ErrorFormat("Aborting thread due to {0}", e);
                            return;
                        }
                    }
                    // A handler for _any_ exception
                    catch (Exception e) {
                        LogUtil.ExceptionLog.ErrorFormat("AsyncLoadEntities caught exception: {0}", e);
                    }
                } finally {
                     Monitor.Exit(asyncLoadLock);
                }
			}
		}

		/// <summary>
		///   Used to manually change the model being used
		/// </summary>
		/// <param name="node"></param>
		/// <param name="meshFile"></param>
		/// <param name="subMeshNames"></param>
		/// <param name="materialNames"></param>
		public void SetModel(ObjectNode node, string meshFile, List<SubmeshInfo> submeshList) {
            int sleepTime = 100; // we will be sleeping for 100ms each time 
            int maxTries = (int)(MaxLoadTime / sleepTime);
            for (int tries = 0; tries < maxTries; ++tries) {
                if (PrepareModel(node.Oid, meshFile, submeshList)) {
					ApplyModel(node);
					return;
				}
				Thread.Sleep(sleepTime);
			}
			log.Error("Timed out trying to change model");
		}

		/// <summary>
        ///   Provides a check on whether the mesh and materials that
		///   this model needs are already loaded, short-circuiting
        ///   the AsyncLoad thread processing.
		/// </summary>
		/// <param name="oid"></param>
		/// <param name="meshFile"></param>
        /// <param name="submeshList"></param>
		/// <returns>true if the model mesh and materials have already been loaded</returns>
        protected bool ModelResourcesAlreadyAvailable(long oid, string meshFile, List<SubmeshInfo> submeshList) {
            if (submeshList != null) {
                foreach (SubmeshInfo submeshInfo in submeshList) {
                    if (!MaterialManager.Instance.HasResource(submeshInfo.MaterialName))
                        return false;
                }
            }
            Debug.Assert(meshFile != null && meshFile != "");
            if (!MeshManager.Instance.HasResource(meshFile))
                return false;
            log.DebugFormat("In ModelResourcesAlreadyAvailable, found all resources for oid {0}, meshFile {1}", oid, meshFile);
            return true;
        }

		/// <summary>
		///   Call this method to set the mesh for a model.  
		///   When it returns true, the model is ready.
		///   Counter to intuition, this returns true on an error, 
		///   since returning true means you can stop waiting and move on.
		///   The error is dealt with by the later call to SetModel.
		/// </summary>
		/// <param name="oid"></param>
		/// <param name="meshFile"></param>
		/// <param name="submeshNames"></param>
		/// <param name="materialNames"></param>
		/// <returns>true when the model has been loaded, or we have given up</returns>
		protected bool PrepareModel(long oid, string meshFile, List<SubmeshInfo> submeshList) {
			Monitor.Enter(preloadDictionary);
			try {
				// have we already started loading this model?
				if (preloadDictionary.ContainsKey(oid)) {
					// we have already started.  if we are done, we can proceed
					PreloadEntry entry = preloadDictionary[oid];
					if (entry.loaded || entry.cancelled) {
						//Logger.Log(0, "finished preparing model for " + oid);
						return true;
					//} else {
						//Logger.Log(0, "still preparing model for " + oid);
					}
                } else {
					// start loading the model
                    PreloadEntry entry = new PreloadEntry();
                    bool loaded = ModelResourcesAlreadyAvailable(oid, meshFile, submeshList);
                    entry.loaded = loaded;
					entry.cancelled = false;
                    entry.meshFile = meshFile;
					entry.submeshList = submeshList;
					entry.entityName = string.Format("entity.{0}.{1}", oid, entityCounter++);
					// entry.entity = null;
					preloadDictionary[oid] = entry;
                    if (loaded)
                        return true;
				}
			} finally {
				Monitor.Exit(preloadDictionary);
			}

            // Notify the async load thread, so that can start up again
            Monitor.Enter(asyncLoadLock);
            try {
                Monitor.PulseAll(asyncLoadLock);
            } finally {
                Monitor.Exit(asyncLoadLock);
            }
            //Logger.Log(0, "preparing model for " + oid);

			//Logger.Log(0, "PrepareModel: Returning false for model {0}", meshFile);
            return false;
		}

        private static string ArrayToString(string[] array) {
            if (array == null)
                return null;
            StringBuilder buf = new StringBuilder();
            buf.Append("[ ");
            foreach (string str in array) {
                buf.Append("'");
                buf.Append(str);
                buf.Append("' ");
            }
            buf.Append("]");
            return buf.ToString();
        }

		#region Pre-Message handlers
		public void PreHandleModelInfo(BaseWorldMessage message) {
			ModelInfoMessage modelInfo = (ModelInfoMessage)message;
			Debug.Assert(modelInfo.ModelInfo.Count == 1);
			MeshInfo meshInfo = modelInfo.ModelInfo[0];
			bool rv = PrepareModel(modelInfo.Oid, meshInfo.MeshFile, meshInfo.SubmeshList);
			message.DelayHandling = !rv;
		}
		#endregion

		#region Message handlers
		public void HandleNewObject(BaseWorldMessage message) {
			if (!this.TerrainInitialized) {
				log.Error("Ignoring new object message, since terrain is not initialized");
				return;
			}
			NewObjectMessage newObj = (NewObjectMessage)message;
			AddObjectStub(newObj.ObjectId, newObj.Name, newObj.Location,
                          newObj.Direction, newObj.LastInterp,
						  newObj.Orientation, newObj.ScaleFactor, 
						  newObj.ObjectType, newObj.FollowTerrain);
		}
		public void HandleModelInfo(BaseWorldMessage message) {
			ModelInfoMessage modelInfo = (ModelInfoMessage)message;
			Debug.Assert(modelInfo.ModelInfo.Count == 1);
			MeshInfo meshInfo = modelInfo.ModelInfo[0];
			AddObject(modelInfo.Oid, meshInfo.MeshFile);
		}
		public void HandleFreeObject(BaseWorldMessage message) {
			FreeObjectMessage freeObj = (FreeObjectMessage)message;
			RemoveObject(freeObj.ObjectId);
		}
		public void HandleDirection(BaseWorldMessage message) {
			DirectionMessage dirMessage = (DirectionMessage)message;
			long now = WorldManager.CurrentTime;
			log.DebugFormat("Got dir message for {0} at {1}:{2}; dir {3}, loc {4}",
                            (dirMessage.Oid == playerId ? "Player" : dirMessage.Oid.ToString()),
                            now, dirMessage.Timestamp, dirMessage.Direction, dirMessage.Location);
			SetDirLoc(dirMessage.Oid, dirMessage.Timestamp, dirMessage.Direction,
					  dirMessage.Location);
		}
		public void HandleOrientation(BaseWorldMessage message) {
			OrientationMessage orientMessage = (OrientationMessage)message;
			SetOrientation(orientMessage.Oid, orientMessage.Orientation);
		}
		public void HandleDirLocOrient(BaseMessage message) {
			DirLocOrientMessage dirMessage = (DirLocOrientMessage)message;
			long now = WorldManager.CurrentTime;
			log.DebugFormat("Got DirLocOrient message for {0} at {1}:{2}; dir {3}, loc {4}, orient {5}",
                            (dirMessage.Oid == playerId ? "Player" : dirMessage.Oid.ToString()),
                            now, dirMessage.Timestamp, dirMessage.Direction, dirMessage.Location,
                            dirMessage.Orientation);
			SetDirLoc(dirMessage.Oid, dirMessage.Timestamp, dirMessage.Direction, dirMessage.Location);
            SetOrientation(dirMessage.Oid, dirMessage.Orientation);
		}
        //public void HandleStatUpdate(BaseMessage message) {
        //    StatUpdateMessage statUpdateMessage = (StatUpdateMessage)message;
        //    ObjectNode objNode = GetObjectNode(statUpdateMessage.Oid);
        //    if (objNode == null) {
        //        Trace.TraceWarning("Got stat update message for nonexistent object: " + message.Oid);
        //        return;
        //    }
        //    objNode.UpdateStats(statUpdateMessage.StatValues);
        //}
        //public void HandleState(BaseMessage message) {
        //    StateMessage stateMessage = (StateMessage)message;
        //    ObjectNode objNode = GetObjectNode(stateMessage.Oid);
        //    if (objNode == null) {
        //        Trace.TraceWarning("Got state message for nonexistent object: " + message.Oid);
        //        return;
        //    }
        //    objNode.UpdateStates(stateMessage.States);
        //}

        //public void HandleAnimation(BaseWorldMessage message)
        //{
        //    AnimationMessage animMessage = (AnimationMessage)message;
        //    ObjectNode objNode = GetObjectNode(animMessage.Oid);
        //    if (objNode == null)
        //    {
        //        Trace.TraceWarning("Got animation message for nonexistent object: " + message.Oid);
        //        return;
        //    }
        //    if (animMessage.Clear)
        //    {
        //        objNode.ClearAnimationQueue();
        //        Logger.Log(1, "Cleared animation queue for: " + objNode.Oid);
        //    }

        //    List<AnimationEntry> animations = animMessage.Animations;
        //    foreach (AnimationEntry animEntry in animations)
        //    {
        //        objNode.QueueAnimation(animEntry);
        //        Logger.Log(1, "Queued animation " +
        //            animEntry.animationName + " for " + objNode.Oid +
        //            " with loop set to " + animEntry.loop);
        //    }

        //}

		public void HandleAttach(BaseWorldMessage message) {
			AttachMessage attachMessage = (AttachMessage)message;
            ObjectNode parentObj = GetObjectNode(attachMessage.Oid);
			if (parentObj == null) {
				log.WarnFormat("Got attach message for nonexistent object: {0}", message.Oid);
				return;
			}
			string entityName =
                parentObj.Name + "_attach_" + attachMessage.ObjectId;
			// sceneManager.RemoveEntity(entityName);
			Entity entity;
			Monitor.Enter(sceneManager);
			try {
				entity = sceneManager.CreateEntity(entityName,
												   attachMessage.MeshFile);
                parentObj.AttachObject(attachMessage.SlotName, attachMessage.ObjectId, entity);
			} catch (AxiomException e) {
                LogUtil.ExceptionLog.ErrorFormat("Ignoring attach, due to AxiomException: {0}", e);
			} finally {
				Monitor.Exit(sceneManager);
			}
		}
		public void HandleDetach(BaseWorldMessage message) {
			DetachMessage detachMessage = (DetachMessage)message;
            ObjectNode parentObj = GetObjectNode(detachMessage.Oid);
			if (parentObj == null) {
                log.WarnFormat("Got detach message for nonexistent object: {0}", message.Oid);
				return;
			}
			Monitor.Enter(sceneManager);
			try {
                MovableObject sceneObj = parentObj.DetachObject(detachMessage.ObjectId);
				if (sceneObj is Entity) {
					log.InfoFormat("Removing entity: {0}", sceneObj);
					sceneManager.RemoveEntity((Entity)sceneObj);
				}
			} finally {
				Monitor.Exit(sceneManager);
			}
		}


        //public void HandleSound(BaseMessage message) {
        //    SoundMessage soundMessage = (SoundMessage)message;
        //    ObjectNode node = GetObjectNode(soundMessage.Oid);
        //    if (node == null) {
        //        Trace.TraceWarning("Got sound message for invalid object: " + soundMessage.Oid);
        //        return;
        //    }
        //    Logger.Log(1, "Sound entries for " + soundMessage.Oid + 
        //               (soundMessage.Clear ? " (cleared)" : ""));

        //    if (soundMessage.Clear)
        //        node.ClearSounds();
			
        //    foreach (SoundEntry entry in soundMessage.Sounds) {
        //        Logger.Log(1, "  sound entry: " + entry);
        //        SoundSource source = soundManager.GetSoundSource(entry.soundName, false);
        //        if (source == null) {
        //            Trace.TraceWarning("Unable to allocate sound source");
        //            return;
        //        }

        //        source.Position = node.Position;
        //        source.Looping = entry.loop;
        //        source.Gain = entry.soundGain;
        //        Logger.Log(1, "Attaching sound {0}, looping {1}, gain {2} to node {3} at position {4}",
        //                   entry.soundName, entry.loop, entry.soundGain, soundMessage.Oid, node.Position);
        //        node.AttachSound(source);
        //        source.Play();
        //    }
        //}
		public void HandleAmbientLight(BaseWorldMessage message) {
			AmbientLightMessage ambientLightMessage = (AmbientLightMessage)message;
			log.InfoFormat("HandleAmbientLight color {0}", ambientLightMessage.Color.To_0_255_String());
			Monitor.Enter(sceneManager);
			try {
				sceneManager.AmbientLight = ambientLightMessage.Color;
			} finally {
				Monitor.Exit(sceneManager);
			}
		}
		public void HandleNewLight(BaseWorldMessage message) {
			NewLightMessage newLightMessage = (NewLightMessage)message;
			log.InfoFormat("HandleNewLight oid: {0}, light type: {1}, diffuse: {2}, specular: {3}",
					       newLightMessage.ObjectId, newLightMessage.LightType, newLightMessage.Diffuse,
                           newLightMessage.Specular);
			Monitor.Enter(sceneManager);
			try {
				long oid = newLightMessage.ObjectId;

				Light light = null;
				try
				{
					light = sceneManager.CreateLight(newLightMessage.Name);
				}
				catch (AxiomException e)
				{
					LogUtil.ExceptionLog.ErrorFormat("Unable to create light: {0}", e);
					return;
				}
				light.Diffuse = newLightMessage.Diffuse;
				light.Specular = newLightMessage.Specular;
				light.SetAttenuation(newLightMessage.AttenuationRange,
									 newLightMessage.AttenuationConstant,
									 newLightMessage.AttenuationLinear,
									 newLightMessage.AttenuationQuadratic);
				switch (newLightMessage.LightType) {
					case LightNodeType.Point:
						light.Type = Axiom.Graphics.LightType.Point;
						light.Position = newLightMessage.Location;
                        light.CastShadows = false;
						break;
					case LightNodeType.Directional:
						light.Type = Axiom.Graphics.LightType.Directional;
						light.Direction = newLightMessage.Orientation * Vector3.UnitZ;
						// TODO: Eventually, I should be able to remove this, 
						//       but for now, I need to set the position
						light.Position = -1 * light.Direction;
                        light.CastShadows = true;
                        log.InfoFormat("HandleNewLight Direction: {0}", light.Direction);
						break;
					case LightNodeType.Spotlight:
						light.Type = Axiom.Graphics.LightType.Spotlight;
						light.Direction = newLightMessage.Orientation * Vector3.UnitZ;
						light.Position = newLightMessage.Location;
						light.SetSpotlightRange(newLightMessage.SpotlightInnerAngle,
												newLightMessage.SpotlightOuterAngle,
												newLightMessage.SpotlightFalloff);
                        light.CastShadows = false;
						break;
				}
				nodeDictionary[oid] = new LightEntity(oid, light);
			} finally {
				Monitor.Exit(sceneManager);
			}
		}

		public void HandleTerrainConfig(BaseWorldMessage message) {
            log.InfoFormat("In handleterrainconfig");

			TerrainConfigMessage terrainConfigMessage = (TerrainConfigMessage)message;
			Monitor.Enter(sceneManager);
			try {
				string s = terrainConfigMessage.ConfigString;
				if (terrainConfigMessage.ConfigKind == "file") {
					Stream fileStream = ResourceManager.FindCommonResourceData(s);
					StreamReader reader = new StreamReader(fileStream);
					s = reader.ReadToEnd();
					reader.Close();
				}
                log.InfoFormat("about to call setupterrain");

                if (logTerrainConfig)
                    log.InfoFormat("s = {0}", s);
                SetupTerrain(s);
                log.InfoFormat("called setupterrain");
                lock (nodeDictionary) {
                    foreach (WorldEntity node in nodeDictionary.Values) {
                        ObjectNode objNode = node as ObjectNode;
                        if (objNode == null)
                            continue;
                        if (objNode.FollowTerrain) {
                            Vector3 loc = objNode.Position;
							loc.y = GetHeightAt(loc);
                            objNode.Position = loc;
						}
					}
				}

                // raise an event for scripts that want to mess with the world on startup
                ClientAPI.TriggerWorldInitialized = true;
			} finally {
				Monitor.Exit(sceneManager);
			}
            log.InfoFormat("Left handleterrainconfig");
        }

		public void HandleRegionConfig(BaseWorldMessage message) {
			RegionConfigMessage regionConfigMessage = (RegionConfigMessage)message;
			Monitor.Enter(sceneManager);
			try {
				SetupBoundary(regionConfigMessage.ConfigString);
			} finally {
				Monitor.Exit(sceneManager);
			}
		}

		public void HandleSkyboxMaterial(BaseWorldMessage message) {
			SkyboxMaterialMessage skyboxMaterialMessage = (SkyboxMaterialMessage)message;
			Monitor.Enter(sceneManager);
			try {
				sceneManager.SetSkyBox(true, skyboxMaterialMessage.Material, Client.HorizonDistance);
                // Set load completed here
                loadCompleted = true;
			} finally {
				Monitor.Exit(sceneManager);
			}
		}
		public void HandleRoadInfo(BaseWorldMessage message) {
			RoadInfoMessage roadInfo = (RoadInfoMessage)message;
			if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
				Axiom.SceneManagers.Multiverse.SceneManager mvsm =
					(Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;
				Axiom.SceneManagers.Multiverse.Road road =
					mvsm.CreateRoad(roadInfo.Name);
				road.AddPoints(roadInfo.Points);
                if (roadInfo.HalfWidth >= 0)
                    road.HalfWidth = roadInfo.HalfWidth;
			}
		}
        public void HandleSoundControl(BaseWorldMessage message) {
            SoundControlMessage soundMessage = (SoundControlMessage)message;
            List<SoundSource> soundSources = null;
            bool ambient = false;
            if (soundMessage.Oid == 0) { // Ambient sound
                soundSources = ambientSources;
                ambient = true;
            } else {
                ObjectNode objectNode;
                try {
                    objectNode = GetObjectNode(soundMessage.Oid);
                } catch (KeyNotFoundException) {
                    log.InfoFormat("Invalid oid of {0} in SoundControl message", soundMessage.Oid);
                    return;
                }
                soundSources = objectNode.SoundSources;
            }
            if (soundMessage.ClearSounds) {
                foreach (SoundSource source in soundSources)
                    SoundManager.Instance.Release(source);
                soundSources.Clear();
            }
            foreach (string soundName in soundMessage.FreeSoundEntries) {
                foreach (SoundSource source in soundSources)
                    if (source.Name == soundName)
                        SoundManager.Instance.Release(source);
                soundSources.RemoveAll(delegate(SoundSource source) { return source.Name == soundName; });
            }
            foreach (string soundName in soundMessage.NewSoundEntries.Keys) {
                PropertyMap soundProps = soundMessage.NewSoundEntries[soundName];
                SoundSource source = SoundManager.Instance.GetSoundSource(soundName, ambient);
                // Set a default gain of 1.0
                source.Gain = 1.0f;
                source.Looping = false;
                // Check the message properties
                object val;
                if (soundProps.Properties.TryGetValue("Loop", out val)) {
                    bool looping = false;
                    if (val is string)
                        looping = bool.Parse((string)val);
                    else if (val is bool)
                        looping = (bool)val;
                    source.Looping = looping;
                }
                if (soundProps.Properties.TryGetValue("Gain", out val)) {
                    float gain = 0.15f;
                    if (val is string)
                        gain = float.Parse((string)val);
                    else if (val is float)
                        gain = (float)val;
                    source.Gain = gain;
                }
                if (soundProps.Properties.TryGetValue("MinAttenuationDistance", out val)) {
                    float minAttenuationDistance = 0;
                    if (val is string)
                        minAttenuationDistance = float.Parse((string)val);
                    else if (val is float)
                        minAttenuationDistance = (float)val;
                    source.MinAttenuationDistance = minAttenuationDistance;
                }
                if (soundProps.Properties.TryGetValue("MaxAttenuationDistance", out val)) {
                    float maxAttenuationDistance = 0;
                    if (val is string)
                        maxAttenuationDistance = float.Parse((string)val);
                    else if (val is float)
                        maxAttenuationDistance = (float)val;
                    source.MaxAttenuationDistance = maxAttenuationDistance;
                }
                soundSources.Add(source);
                log.InfoFormat("Playing sound {0} on {1}", source.Name, soundMessage.Oid);
                source.Play();
            }
        }
        private void HandleAuthorizedLoginResponse(BaseWorldMessage message) {
            AuthorizedLoginResponseMessage loginResponse = (AuthorizedLoginResponseMessage)message;
            char[] delims = { ',' };
            string[] portions = loginResponse.Version.Split(delims, 2);
            string worldServerVersion = portions[0];
            if (portions.Length > 1)
                ServerCapabilities = portions[1];
            log.InfoFormat("World Server Version: {0}", worldServerVersion);
        }

        private void HandleNewDecal(BaseWorldMessage message)
        {
            NewDecalMessage newDecalMessage = message as NewDecalMessage;

            log.InfoFormat("NewDecalMessage: OID={0}, ImageName={1}", newDecalMessage.Oid, newDecalMessage.ImageName);

            if (decals.ContainsKey(newDecalMessage.Oid))
            {
                log.ErrorFormat("NewDecalMessage: Decal already exists: OID={0}", newDecalMessage.Oid);
            }
            else
            {
                Axiom.SceneManagers.Multiverse.DecalElement element = this.DecalManager.CreateDecalElement(newDecalMessage.ImageName, 
                    newDecalMessage.PositionX, newDecalMessage.PositionZ,
                    newDecalMessage.SizeX, newDecalMessage.SizeZ, 
                    newDecalMessage.Rotation, newDecalMessage.ExpireTime, 0, newDecalMessage.Priority);
                decals.Add(newDecalMessage.Oid, element);
            }
        }

        private void HandleFreeDecal(BaseWorldMessage message)
        {
            FreeDecalMessage freeDecalMessage = message as FreeDecalMessage;

            log.InfoFormat("FreeDecalMessage: OID={0}", freeDecalMessage.Oid);

            if (decals.ContainsKey(freeDecalMessage.Oid))
            {
                Axiom.SceneManagers.Multiverse.DecalElement element = decals[freeDecalMessage.Oid];
                this.DecalManager.RemoveDecalElement(element);
                decals.Remove(freeDecalMessage.Oid);
            }
            else
            {
                log.ErrorFormat("FreeDecalMessage: Decal doesn't exist: OID={0}", freeDecalMessage.Oid);
            }
        }

        protected NumericKeyFrame fogColorStartFrame;
        protected NumericKeyFrame fogColorEndFrame;
        protected NumericKeyFrame fogNearStartFrame;
        protected NumericKeyFrame fogNearEndFrame;
        protected NumericKeyFrame fogFarStartFrame;
        protected NumericKeyFrame fogFarEndFrame;
        protected AnimationState fogAnimState;

		public void HandleFogMessage(BaseWorldMessage message) {
			FogMessage fogMessage = (FogMessage)message;
            if (fogAnim != null)
            {
                ClientAPI.StopSceneAnimation(fogAnimState);

                fogColorStartFrame.NumericValue = sceneManager.FogColor;
                fogColorEndFrame.NumericValue = fogMessage.FogColor;

                fogNearStartFrame.NumericValue = sceneManager.FogStart;
                fogNearEndFrame.NumericValue = (float)fogMessage.FogStart;

                fogFarStartFrame.NumericValue = sceneManager.FogEnd;
                fogFarEndFrame.NumericValue = (float)fogMessage.FogEnd;

                fogAnimState.IsEnabled = true;
                ClientAPI.PlaySceneAnimation(fogAnimState, 1.0f, false);
            }
            else
            {
                sceneManager.SetFog(FogMode.Linear, fogMessage.FogColor,
                                    1.0f, fogMessage.FogStart, fogMessage.FogEnd);

                if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager)
                {
                    Axiom.SceneManagers.Multiverse.SceneManager mvsm =
                        (Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;

                    float animLen = 5;

                    fogAnim = sceneManager.CreateAnimation("sceneFog", animLen);
                    fogAnimState = sceneManager.CreateAnimationState("sceneFog");
                    fogAnimState.IsEnabled = false;
                    fogAnim.InterpolationMode = InterpolationMode.Linear;

                    NumericAnimationTrack track = fogAnim.CreateNumericTrack(0, mvsm.FogConfig.CreateAnimableValue("FogColor"));
                    fogColorStartFrame = track.CreateKeyFrame(0) as Axiom.Animating.NumericKeyFrame;
                    fogColorEndFrame = track.CreateKeyFrame(animLen) as Axiom.Animating.NumericKeyFrame;

                    track = fogAnim.CreateNumericTrack(1, mvsm.FogConfig.CreateAnimableValue("FogNear"));
                    fogNearStartFrame = track.CreateKeyFrame(0) as Axiom.Animating.NumericKeyFrame;
                    fogNearEndFrame = track.CreateKeyFrame(animLen) as Axiom.Animating.NumericKeyFrame;

                    track = fogAnim.CreateNumericTrack(2, mvsm.FogConfig.CreateAnimableValue("FogFar"));
                    fogFarStartFrame = track.CreateKeyFrame(0) as Axiom.Animating.NumericKeyFrame;
                    fogFarEndFrame = track.CreateKeyFrame(animLen) as Axiom.Animating.NumericKeyFrame;


                    fogColorStartFrame.NumericValue = fogMessage.FogColor;
                    fogColorEndFrame.NumericValue = fogMessage.FogColor;

                    fogNearStartFrame.NumericValue = (float)fogMessage.FogStart;
                    fogNearEndFrame.NumericValue = (float)fogMessage.FogStart;

                    fogFarStartFrame.NumericValue = (float)fogMessage.FogEnd;
                    fogFarEndFrame.NumericValue = (float)fogMessage.FogEnd;
                }
            }
		}
        
		public void HandleAddParticleEffect(BaseWorldMessage message) {
            AddParticleEffectMessage particleMessage = (AddParticleEffectMessage)message;
            ObjectNode parentObj = GetObjectNode(particleMessage.Oid);
            if (parentObj == null) {
                log.WarnFormat("Got add particle effect message for nonexistent object: {0}", message.Oid);
                return;
            }
			string entityName = "_particle_" + particleMessage.ObjectId;
            // sceneManager.RemoveEntity(entityName);
            Monitor.Enter(sceneManager);
            try {
                ParticleSystem particleSystem = ParticleSystemManager.Instance.CreateSystem(entityName, particleMessage.EffectName);
                if (particleMessage.VelocityMultiplier != 1.0f)
                    particleSystem.ScaleVelocity(particleMessage.VelocityMultiplier);
                if (particleMessage.ParticleSizeMultiplier != 1.0f) {
                    particleSystem.DefaultWidth *= (particleMessage.ParticleSizeMultiplier);
                    particleSystem.DefaultHeight *= (particleMessage.ParticleSizeMultiplier);
                }
                // particleSystem.LocalSpace = particleMessage.GetFlag(AddParticleEffectMessage.Flags.LocalSpace);
				// If the add particle message has a color, assign it to the particle system color
				log.InfoFormat("Particle system booleans {0}, color {1}", 
                               particleMessage.ParticleBooleans, 
                               (particleMessage.Color != null) ? particleMessage.Color.ToString() : "null");
                if (particleMessage.Color != null)
                    log.InfoFormat("Color is deprecated, and no longer affects the system.");
                // particleSystem.Color = particleMessage.Color;
				// 				if (particleMessage.Orientation != Quaternion.Identity)
                // 					node.Rotate(particleMessage.Orientation, TransformSpace.Parent);
                parentObj.AttachObject(particleMessage.SlotName, particleMessage.ObjectId, particleSystem);
            } catch (AxiomException e) {
                LogUtil.ExceptionLog.ErrorFormat("Ignoring add particle effect, due to AxiomException: {0}", e);
            } finally {
                Monitor.Exit(sceneManager);
            }
            log.InfoFormat("Got add particle effect for: {0}", particleMessage.ObjectId);
        }

        public void HandleRemoveParticleEffect(BaseWorldMessage message) {
            RemoveParticleEffectMessage particleMessage = (RemoveParticleEffectMessage)message;
            RemoveParticleEffect(particleMessage.Oid, particleMessage.ObjectId);
		}
		
		public void RemoveParticleEffect(long parentOid, long particleOid) {
            string entityName = "_particle_" + particleOid;
			Monitor.Enter(sceneManager);
			try {
				if (parentOid == 0) {
					// It's a projectile; just remove its scene node from the root node
					ObjectNode particleObj = GetObjectNode(particleOid);
					if (particleObj == null) {
						log.WarnFormat("Got remove projectile for nonexistent projectile: {0}", particleOid);
						return;
					}
					particleObj.SceneNode.RemoveFromParent();
				} else {
					ObjectNode parentObj = GetObjectNode(parentOid);
					if (parentObj == null) {
						log.WarnFormat("Got remove particle effect message for nonexistent object: {0}", parentOid);
						return;
					}
					parentObj.DetachObject(particleOid);
				}
                ParticleSystemManager.Instance.RemoveSystem(entityName);
			} catch (AxiomException e) {
                LogUtil.ExceptionLog.ErrorFormat("Ignoring remove particle effect, due to AxiomException: {0}", e);
            } finally {
                Monitor.Exit(sceneManager);
            }
            log.InfoFormat("Removed particle effect for: {0}", particleOid);
        }

		public void HandleProjectileTrackingMessages(BaseWorldMessage message) {
			ExtensionMessage msg = (ExtensionMessage)message;
			if (!msg.Properties.ContainsKey("ext_msg_subtype"))
				return;
			string op = msg.Properties["ext_msg_subtype"].ToString();
			if (op != "TrackObjectInterpolation" && op != "TrackLocationInterpolation")
				return;
			// Check to see which is failing
			ProjectileTracker tracker = new ProjectileTracker(this,
															  msg.GetLongProperty("Oid"),
															  msg.GetLongProperty("ProjectileOid"),
															  msg.GetIntProperty("TimeToImpact"),
															  msg.GetLongProperty("Timestamp"),
															  msg.GetFloatProperty("Height"));
			
			if (op == "TrackObjectInterpolation") {
				tracker.TargetOid = msg.GetLongProperty("TargetOid");
				tracker.TargetSocket = msg.GetStringProperty("TargetSocket");
			}
			else
				tracker.TargetLocation = msg.GetVector3Property("Location");
			AddTrackingInterpolation(tracker);
		}
		
		public void AddTrackingInterpolation(ProjectileTracker tracker) {
            log.InfoFormat("Adding tracking interpolator {0}", tracker);
			Monitor.Enter(sceneManager);
			try {
				if (tracker.TargetSocket != "")
					tracker.InitializeObjectTarget(this);
				tracker.InitializeProjectile(this);
				timeInterpolators.Add(tracker);
			} catch (AxiomException e) {
                LogUtil.ExceptionLog.ErrorFormat("Ignoring tracking interpolation for projectile {0}, parent {1}  effect, due to exception {3}",
							    tracker.ProjectileOid, tracker.ParentOid, e);
            } finally {
                Monitor.Exit(sceneManager);
            }			
		}

		public void HandleClientParameterMessage(BaseWorldMessage message) {
			ExtensionMessage msg = (ExtensionMessage)message;
			if (!msg.Properties.ContainsKey("ext_msg_subtype") ||
                msg.Properties["ext_msg_subtype"].ToString() != "ClientParameter")
				return;
			foreach(KeyValuePair<string, object> pair in msg.Properties) {
				string name = pair.Key;
				string value = (string)pair.Value;
				try {
					log.InfoFormat("Setting parameter {0} = {1}", name, value);
					ParameterRegistry.SetParameter(name, value);
				}
				catch (Exception e) {
					LogUtil.ExceptionLog.ErrorFormat("Setting parameter {0} = {1} failed!  The exception was '{2}'",
								    name, value, e.Message);
				}
			}
		}

        public void HandleInvokeEffect(BaseWorldMessage message)
        {
            InvokeEffectMessage effectMessage = (InvokeEffectMessage)message;

            // log the effect and args
            log.InfoFormat("HandleInvokeEffect starting effect {0} with instanceID {1}, args:",
                           effectMessage.EffectName, message.Oid);
            foreach (KeyValuePair<string, object> kvp in effectMessage.Args)
                log.InfoFormat("  {0}: {1}", kvp.Key, kvp.Value.ToString());

            string effectCommand = String.Format("ClientAPI.InvokeEffect('{0}', {1}, _invokeEffectArgs)", effectMessage.EffectName, effectMessage.Oid);

            Dictionary<string, object> scriptLocals = new Dictionary<string, object>();
            scriptLocals["_invokeEffectArgs"] = effectMessage.Args;

            Multiverse.Interface.UiScripting.RunScript(effectCommand, scriptLocals);
        }
		
        public void HandleMobPath(BaseWorldMessage message)
        {
            MobPathMessage pathMsg = (MobPathMessage)message;

            List<Vector3> pathPoints = pathMsg.PathPoints;
            long mobOid = pathMsg.Oid;
            MobNode mobNode = (MobNode)GetObjectNode(mobOid);            

            if (mobNode == null)
                log.ErrorFormat("Received MobPathMessage for oid {0}, but that object is not a MobNode!", mobOid);
            // If there are no points, this message cancels any
            // previous path interpolator for the object
            else if (pathPoints.Count == 0) {
                mobNode.Interpolator = null;
                log.InfoFormat("Removed path interpolator for oid {0}, name {1}", mobOid, mobNode.Name);
                mobNode.LastDirection = Vector3.Zero;
            } else {
                long now = WorldManager.CurrentTime;
                PathInterpolator pathInterpolator = 
                    (pathMsg.InterpKind.ToLower() == "spline") ?
                    (PathInterpolator)new PathSpline(mobOid, pathMsg.StartTime, pathMsg.Speed, pathMsg.TerrainString, pathPoints) :
                    (PathInterpolator)new PathLinear(mobOid, pathMsg.StartTime, pathMsg.Speed, pathMsg.TerrainString, pathPoints);
                mobNode.Interpolator = pathInterpolator;
                log.InfoFormat("Added path interpolator for oid {0}, name {1}, first point {2}, interpolator {3}", 
                               mobOid, mobNode.Name, pathPoints[0], pathInterpolator.ToString());
            }
        }
		
        ///<summary>
        ///    A temp indicating if we've received the first loading
        ///    state change to false.
        ///</summary>
        private static int loadingStateCycles = 0;
        
        ///<summary>
        ///    If we're talking to a 1.1 server, which is the only
        ///    source of the old LoadingStateMessage, only react to
        ///    the first time the client receives loading state =
        ///    false.
        ///</summary>
        private void HandleLoadingState(BaseWorldMessage message)
        {
            LoadingStateMessage stateMessage = message as LoadingStateMessage;
            if (Client.ServerRelease1_5)
                log.InfoFormat("WorldManager.HandleLoadingState: loadingState {0} Ignoring message because talking to a release 1.5 or later server",
                               stateMessage.LoadingState);
            else if (!stateMessage.LoadingState && loadingStateCycles == 0) {
                log.InfoFormat("LoadingStateMessage: state = {0}", stateMessage.LoadingState);
                HandleLoadingStateChange(null, stateMessage.LoadingState);
                loadingStateCycles++;
            }
        }

        private void HandleNewLoadingState(BaseWorldMessage message)
        {
            ExtensionMessage msg = message as ExtensionMessage;
            if (!msg.Properties.ContainsKey("ext_msg_subtype"))
                return;
            string s = msg.Properties["ext_msg_subtype"].ToString();
            bool startingLoad = s == "mv.SCENE_BEGIN";
            bool endingLoad = s == "mv.SCENE_END";
            if (!startingLoad && !endingLoad)
                return;
            HandleLoadingStateChange(msg, startingLoad);
        }
    
        ///<summary>
        ///    The default handler for an old or new loading state
        ///    message.
        ///</summary>
        private void HandleLoadingStateChange(ExtensionMessage message, bool startingLoad) {
            LoadingStateChangeHandler handler = OnLoadingStateChange;
            nowLoading = startingLoad;
            string s = (startingLoad ? "start" : "stop");
            if (handler != null) {
                log.InfoFormat("WorldManager.HandleLoadingStateChange: Running event handler {0} for loading state: {1} loading", handler, s);
                handler(message, startingLoad);
            }
            else {
                log.InfoFormat("WorldManager.HandleLoadingStateChange: Running default actions for loading state {0} loading", s);
                // Should either put up a loading screen, or not
                // UpdateRenderTargets, but not both, because stopping
                // updating of render targets will prevent the loading
                // screen from being visible.
                if (startingLoad) {
                    // We're starting the load process, so mark the
                    // loadWindow visible first, and whack static
                    // geometry last.
                    Client.Instance.LoadWindowVisible = startingLoad;
                    //Client.Instance.UpdateRenderTargets = !startingLoad;
                    //staticGeometryHelper.RebuildIfFinishedLoading(message, startingLoad);
                }
                else {
                    // We're ending the load process, so mark the
                    // whack static geometry first, and make the
                    // loadWindow invisible last.
                    //staticGeometryHelper.RebuildIfFinishedLoading(message, startingLoad);
                    //Client.Instance.UpdateRenderTargets = !startingLoad;
                    Client.Instance.LoadWindowVisible = startingLoad;
                }
            }
        }

        #endregion


		/// <summary>
		///   Update the time offset.
		/// </summary>
		/// <param name="timestamp">timestamp received from the server</param>
		public void AdjustTimestamp(long timestamp)
		{
			networkHelper.AdjustTimestamp(timestamp);
		}
		
		/// <summary>
		///   We already hold the lock on the sceneManager and the nodeDictionary.
		///   This doesn't remove the node from the dictionary, but it does remove
		///   it from the scene, and cleans up.
		/// </summary>
		/// <param name="oid"></param>
		private void RemoveNode(ObjectNode node) {
			if (ObjectRemoved != null)
				ObjectRemoved(this, node);

            // Static geometry tracking code
            if (node.ObjectType == ObjectNodeType.Prop) {
                staticGeometryHelper.NodeRemoved(node);
            }

            // Sound management code
            node.ClearSounds();

            // TODO: Pull out the ObjectNode's collider
            
            // Pull out the ObjectNode's attachments
            //foreach (Node childNode in node.Attachments) {
                // TODO: actually get rid of the attachments
                // It looks like the DestroySceneNode call later will
                // remove all scene node style attachment points, but
                // the TagPoint style attachments will stick around
            //}

            // Entity entity = node.Entity;
            // node.Entity = null;
            // sceneManager.RemoveEntity(entity);
            // SceneNode sceneNode = node.SceneNode;
            // node.SceneNode = null;
            // Remove the node's scenenode, entity (and child scene nodes)
            // sceneManager.DestroySceneNode(sceneNode.Name);
		}

		/// <summary>
		///   We already hold the lock on the sceneManager and the nodeDictionary.
		///   This doesn't remove the node from the dictionary, but it does remove
		///   it from the scene, and cleans up.
		/// </summary>
		/// <param name="oid"></param>
		private void RemoveNode(LightEntity node) {
			sceneManager.RemoveLight(node.Light);
		}

		// Removes a node from the nodeDictionary and also does whatever
		// cleanup needs to be done on that node.
		private void RemoveNode(long oid) {
			Monitor.Enter(sceneManager);
			try {
				lock (nodeDictionary) {
					WorldEntity node = nodeDictionary[oid];
					if (node is ObjectNode) {
						ObjectNode objNode = node as ObjectNode;
                        log.DebugFormat("Removing node {0}, oid {1}, mesh {2}",
                                        objNode.Name, oid, objNode.Entity.Mesh.Name);
                        RemoveNode(objNode);
                    }
					else if (node is LightEntity)
						RemoveNode(node as LightEntity);
					nodeDictionary.Remove(oid);
					node.Dispose();
				}
			} finally {
				Monitor.Exit(sceneManager);
			}
		}

		public void ClearWorld() {
			if (sceneManager == null)
				return; // we haven't really started up yet
			// Remove all objects from the scene
			Monitor.Enter(sceneManager);
			try {
				lock (nodeDictionary) {
                    // Clone the world dictionary, so that if an object's 
                    // Dispose method tries to remove it from the dictionary, 
                    // it won't cause a concurrent modification exception.
                    List<WorldEntity> nodes = new List<WorldEntity>(nodeDictionary.Values);
					foreach (WorldEntity node in nodes) {
                        try {
                            if (node is ObjectNode)
                                RemoveNode(node as ObjectNode);
                            else if (node is LightEntity)
                                RemoveNode(node as LightEntity);
                        } catch (Exception e) {
                            LogUtil.ExceptionLog.ErrorFormat("Exception caught while removing node: {0}", e);
                            // Just ignore the exception here
                        }
                        node.Dispose();
                    }
					nodeDictionary.Clear();
				}

                // Clear out any other scene manager entries
				sceneManager.ClearScene();

                // Clear our ambient sounds
                foreach (SoundSource source in ambientSources)
                    SoundManager.Instance.Release(source);
                ambientSources.Clear();

				SetPlayer(null);
				playerId = 0;
				playerInitialized = false;
				terrainInitialized = false;
                playerStubInitialized = false;
                loadCompleted = false;

			} finally {
				Monitor.Exit(sceneManager);
			}
		}

        protected void SetupSubEntities(Entity entity, List<SubmeshInfo> submeshList) {
            // Big clause for a little log message
            if (submeshList != null) {
                string[] submeshNames = new string[submeshList.Count];
                string[] materialNames = new string[submeshList.Count];
                for (int i = 0; i < submeshList.Count; ++i) {
                    SubmeshInfo submeshInfo = submeshList[i];
                    submeshNames[i] = submeshInfo.SubmeshName;
                    materialNames[i] = submeshInfo.MaterialName;
                }
                log.InfoFormat("Submesh names for {0}:", entity.Name);
                for (int i = 0; i < submeshNames.Length; i++ )
                    log.InfoFormat("  {0}", submeshNames[i]);
                log.InfoFormat("Material names for {0}:", entity.Name);
                for (int i = 0; i < materialNames.Length; i++ )
                    log.InfoFormat("  {0}", materialNames[i]);
            }
            if (submeshList == null) {
                for (int i = 0; i < entity.SubEntityCount; ++i) {
                    SubEntity sub = entity.GetSubEntity(i);
                    log.InfoFormat("Marking {0} visible", sub.SubMesh.Name);
                    sub.IsVisible = true;
                }
                return;
            }
            entity.CastShadows = false;
            foreach (SubmeshInfo submeshInfo in submeshList) {
                if (submeshInfo.CastShadows) {
                    entity.CastShadows = true;
                    break;
                }
            }
            for (int i = 0; i < entity.SubEntityCount; ++i) {
                SubEntity sub = entity.GetSubEntity(i);
                sub.IsVisible = false;
                log.InfoFormat("Marking {0} invisible", sub.SubMesh.Name);
                foreach (SubmeshInfo submeshInfo in submeshList) {
                    if (sub.SubMesh.Name == submeshInfo.SubmeshName) {
                        sub.IsVisible = true;
                        sub.MaterialName = submeshInfo.MaterialName;
                        log.InfoFormat("Marking {0} visible", sub.SubMesh.Name);
                        break;
                    }
                }
            }
        }

        protected void ApplyModelWithoutPreload(ObjectNode node, string entityName, string meshName, List<SubmeshInfo> submeshList)
        {
            if (node.Entity != null && node.Entity.Mesh.Name != meshName)
            {
                // If we are replacing an existing entity with one from a new model,
                // clear out the old one.
                // we can leave sounds, but attachments and animations have to go
                node.ClearAnimations();
                node.ClearAttachments();
                sceneManager.RemoveEntity(node.Entity);
                node.Entity = null;
                node.EntityName = null;
            }
            if (node.Entity == null)
            {
                // If we are adding a new model
                node.Entity = sceneManager.CreateEntity(entityName, meshName);
                node.EntityName = entityName;
            }
            // Update the submesh visibility and the materials.
            SetupSubEntities(node.Entity, submeshList);
        }

        protected bool ApplyModel(ObjectNode node)
        {
            Monitor.Enter(sceneManager);
            try {
				PreloadEntry entry = null;
				Monitor.Enter(preloadDictionary);
				try {
					if (!preloadDictionary.ContainsKey(node.Oid))
						return false;
					entry = preloadDictionary[node.Oid];
					preloadDictionary.Remove(node.Oid);
				} finally {
					Monitor.Exit(preloadDictionary);
				}
				// Our load of that entry may have been cancelled
				if (entry.cancelled == true) {
					log.ErrorFormat("Application of model to node cancelled for {0}", node.Oid);
					return false;
				}

                ApplyModelWithoutPreload(node, entry.entityName, entry.meshFile, entry.submeshList);
                return true;
			} catch (Exception e) {
				LogUtil.ExceptionLog.WarnFormat("Exception setting model for node oid {0}, name {1}, error message '{2}'",
                               node.Oid, node.Name, e);
            } finally {
                Monitor.Exit(sceneManager);
            }
			return false;
        }

		/// <summary>
		///   Gets the height at a given location, or returns 0 if unable to determine.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		public float GetHeightAt(Vector3 location) {
			if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
				Ray ray = new Ray(location, Vector3.NegativeUnitY);
				ulong flags = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.Height;
				RaySceneQuery query = sceneManager.CreateRayQuery(ray, flags);
                List<RaySceneQueryResultEntry> results = query.Execute();
				foreach (RaySceneQueryResultEntry entry in results)
					return entry.worldFragment.SingleIntersection.y;
			}
			return 0;
		}

        public Vector3 PickTerrain(int x, int y)
        {
            
            Axiom.Core.RaySceneQuery q = sceneManager.CreateRayQuery(client.Camera.GetCameraToViewportRay((float)x / (float)client.Viewport.ActualWidth,
                (float)y / (float)client.Viewport.ActualHeight));

            q.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.FirstTerrain;
            List<RaySceneQueryResultEntry> results = q.Execute();

            if (results.Count > 0)
            {
                RaySceneQueryResultEntry result = results[0];

                return (result.worldFragment.SingleIntersection);
            }
            else
            {
                return Vector3.Zero;
            }
        }

		private void SetupWaterPlane() {
			// water plane setup
			Plane waterPlane = new Plane(Vector3.UnitY, 10f * Client.OneMeter);

			Mesh waterMesh = MeshManager.Instance.CreatePlane(
				"WaterPlane",
				waterPlane,
				60 * 128 * Client.OneMeter, 90 * 128 * Client.OneMeter,
				20, 20,
				true, 1,
				10, 10,
				Vector3.UnitZ);

			Entity waterEntity = sceneManager.CreateEntity("Water", "WaterPlane");

			Debug.Assert(waterEntity != null);
			waterEntity.MaterialName = "Terrain/WaterPlane";

			SceneNode waterNode = sceneManager.RootSceneNode.CreateChildSceneNode("WaterNode");
			Debug.Assert(waterNode != null);
			waterNode.AttachObject(waterEntity);
			waterNode.Translate(new Vector3(0, 0, 0));
		}

        private Axiom.SceneManagers.Multiverse.ITerrainGenerator ParseTerrainXML(XmlReader r)
        {
            Axiom.SceneManagers.Multiverse.ITerrainGenerator terrainGenerator;

            // search attributes to see if Type is specified
            string type = null;
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                if (r.Name == "Type")
                {
                    type = r.Value;
                    break;
                }
            }
            r.MoveToElement();

            if (type == "HeightfieldMosaic")
            {
                terrainGenerator = new Multiverse.Lib.HeightfieldGenerator.HeightfieldTerrainGenerator(r);
            }
            else
            {
                Multiverse.Generator.FractalTerrainGenerator gen = new Multiverse.Generator.FractalTerrainGenerator();

                gen.FromXML(r);

                terrainGenerator = gen;
            }

            return terrainGenerator;
        }

        private Axiom.SceneManagers.Multiverse.ITerrainMaterialConfig ParseTerrainDisplay(XmlReader r)
        {
            Axiom.SceneManagers.Multiverse.ITerrainMaterialConfig terrainMaterialConfig;

            // search attributes to see if Type is specified
            string type = null;
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                if (r.Name == "Type")
                {
                    type = r.Value;
                    break;
                }
            }
            r.MoveToElement();

            if (type == "AlphaSplat")
            {
                terrainMaterialConfig = new Axiom.SceneManagers.Multiverse.AlphaSplatTerrainConfig(r);
            }
            else // assume "AutoSplat" is the default
            {
                terrainMaterialConfig = new Axiom.SceneManagers.Multiverse.AutoSplatConfig(r);
            }

            return terrainMaterialConfig;
        }

        /// <summary>
        /// Configures a generator and terrainMaterialConfig from XML passed from the server.
        /// </summary>
        /// <param name="s">XML string containing terrain and possibly terrainDisplay</param>
        /// <returns>ITerrainGenerator terrain generator object</returns>
        public void ConfigTerrainFromXml(string s)
        {
            string wrappedstring = string.Format("<Root>{0}</Root>", s);
            TextReader tr = new StringReader(wrappedstring);
            XmlReader r = XmlReader.Create(tr);
            Axiom.SceneManagers.Multiverse.ITerrainGenerator gen = null;
            Axiom.SceneManagers.Multiverse.ITerrainMaterialConfig terrainMaterialConfig = null;
            while (r.Read())
            {
                // look for the start of an element
                if (r.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }
                if (r.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }
                if (r.NodeType == XmlNodeType.Element)
                {
                    switch (r.Name)
                    {
                        case "Terrain":
                            gen = ParseTerrainXML(r);
                            break;

                        case "TerrainDisplay":
                            terrainMaterialConfig = ParseTerrainDisplay(r);
                            break;
                    }
                }
            }

            Axiom.SceneManagers.Multiverse.SceneManager mvsm =
                (Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;

            mvsm.SetWorldParams(gen, worldLODSpec);
            mvsm.LoadWorldGeometry(null);

            if (terrainMaterialConfig != null)
            {
                Axiom.SceneManagers.Multiverse.TerrainManager.Instance.TerrainMaterialConfig = terrainMaterialConfig;
            }

            //mvsm.ShadowTechnique = ShadowTechnique.TextureModulative;
            // FIXME: This should be uncommented, and the call down in the 
            //        Axiom scene manager should create the shadow textures.
            //mvsm.ShadowConfig.ShadowTechnique = Axiom.SceneManagers.Multiverse.ShadowTechnique.None;
            //mvsm.ShadowConfig.ShadowTextureSize = 1024;
        }

		private void SetupTerrain(string generatorConfig) {
            SceneNode sceneNode = null;
			Entity entity = null;
			Monitor.Enter(sceneManager);
			//sceneManager.SetFog(Axiom.Graphics.FogMode.Linear, 
			//                    Axiom.Core.ColorEx.White, 0.001f, 50000f, 500000f);
			try {
				if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
                    if (logTerrainConfig)
                    {
                        if (generatorConfig.Length <= 10000)
                            log.InfoFormat(generatorConfig);
                        else
                            log.InfoFormat(generatorConfig.Substring(0, 10000) + " ... </Terrain>");
                    }
                    ConfigTerrainFromXml(generatorConfig);
				}
				else
				{
                    log.Warn("In here - not mvsm");
					sceneNode = sceneManager.RootSceneNode.CreateChildSceneNode("simple_terrain",
													Vector3.Zero, Quaternion.Identity);
					entity = sceneManager.CreateEntity("simple_terrain", "simple_terrain.mesh");
					sceneNode.AttachObject(entity);
				}
				//SetupWaterPlane();
				terrainInitialized = true;
			} finally {
				Monitor.Exit(sceneManager);
			}
		}

		private void SetupBoundary(string regionConfig) {
			TextReader s = new StringReader(regionConfig);
			XmlTextReader r = new XmlTextReader(s);
			Monitor.Enter(sceneManager);
			try {
				if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
					Axiom.SceneManagers.Multiverse.SceneManager mvsm =
						(Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;
					log.Info(regionConfig);
					mvsm.ImportBoundaries(r);
				} else {
                    log.ErrorFormat("Boundaries not supported by scene manager");
				}
			} finally {
				Monitor.Exit(sceneManager);
			}
		}

		public Vector3 ResolveLocation(ObjectNode node, Vector3 loc) {
            Vector3 rv = loc;

			if (node.FollowTerrain) {
                if (rv.y == 0)
    			    rv.y = GetHeightAt(loc);
    			if (!node.IsUpright) {
	    			// if the node is not upright, it may need to have its 
		    		// orientation adjusted to match the terrain
			    	Quaternion q = ResolveOrientation(node, node.Orientation);
				    node.Orientation = q;
			    }
            }
			return rv;
		}

		public Vector3 MoveMobNode(MobNode mobNode, Vector3 desiredDisplacement, long timeInterval)
		{
			Vector3 originalPosition = mobNode.Position;
            Vector3 pos = originalPosition;
            // If we're now loading, don't run collision detection
            // because collision volumes aren't necessarily
            // established.
//             log.DebugFormat("WorldManager.MoveMobNode: oid {0}, name {1}, pos {2}, desiredDisplacement {3}, nowLoading {4}", 
//                 mobNode.Oid, mobNode.Name, pos, desiredDisplacement, nowLoading);
            if (nowLoading)
                pos += desiredDisplacement;
            else
                pos = PrimeMover.MoveMobNode(mobNode, desiredDisplacement, client);
			// Tell the tile manager about the new center-of-view
			if (mobNode is Multiverse.Base.Player &&
				sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
				Axiom.SceneManagers.Multiverse.SceneManager mvsm =
					(Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;
				// Tell mvsm to the new center of the collision
				// domain.  50 meters is the collision horizon.
				mvsm.SetCollisionArea(pos, CollisionRange);
                log.DebugFormat("Player MoveMobNode newLoc {0} oldLoc {1}", pos, mobNode.Position);
			}
            if (voiceMgr != null && !voiceMgr.Disposed) {
                if (mobNode.Oid == playerId)
                    voiceMgr.SetPlayerVoiceProperties(originalPosition, pos, timeInterval, 
                        client.Camera.DerivedDirection, client.Camera.DerivedUp);
                else if (mobNode.PositionalSoundEmitter)
                    voiceMgr.MovePositionalSound(mobNode.Oid, originalPosition, pos, timeInterval);
            }
			return pos;
		}
		
		public class OidAndRegion 
        {
            public OidAndRegion(long oid, string region) {
                this.oid = oid;
                this.region = region;
            }
            
            public long Oid {
                get { return oid; }
                set { oid = value; }
            }

            public string Region {
                get { return region; }
                set { region = value; }
            }

            private long oid;
            private string region;
        }

        // Return true if changed since last time; false otherwise.
        // This is the external API called by Python code
        public List<OidAndRegion> RegionsContaingPoint(Vector3 p) 
        {
            // Find the location of the center of the moving object
            List<RegionVolume> regions = RegionVolumes.Instance.RegionsContainingPoint(p);
            List<OidAndRegion> oidAndRegionList = new List<OidAndRegion>();
            foreach (RegionVolume region in regions)
                oidAndRegionList.Add(new OidAndRegion(region.ObjectOid, region.RegionName));
            return oidAndRegionList;
        }
        
        public Quaternion ResolveOrientation(ObjectNode node, Quaternion q) {
			if (node.Entity == null || !node.FollowTerrain ||
				node.ObjectType == ObjectNodeType.Prop)
				return q;
			float yaw = Multiverse.MathLib.MathUtil.GetFullYaw(q);
            if (log.IsDebugEnabled)
                log.DebugFormat("oid {0}, name {1}, yaw: {2}; quaternion: {3}; new quat: {4}",
                                node.Oid, node.Name, yaw, q,
                                Quaternion.FromAngleAxis(yaw, Vector3.UnitY));
			Quaternion yawRot = Quaternion.FromAngleAxis(yaw, Vector3.UnitY);;
			if (node.IsUpright)
				return yawRot;
#if MATCH_TERRAIN
			Vector3 xVec = yawRot * (2 * Client.OneMeter * Vector3.UnitX);
			Vector3 zVec = yawRot * (2 * Client.OneMeter * Vector3.UnitZ);
			float y0, y1;
			y0 = GetHeightAt(node.Position - xVec);
			y1 = GetHeightAt(node.Position + xVec);
			float roll = (float)Math.Asin((y1 - y0) / (2 * xVec.Length));
			y0 = GetHeightAt(node.Position - zVec);
			y1 = GetHeightAt(node.Position + zVec);
			float pitch = (float)Math.Asin((y1 - y0) / (2 * zVec.Length));
			Logger.Log(0, "yaw: {0}; pitch: {1}; roll: {2}; new quat: {2}", 
					   yaw, pitch, roll,
					   Multiverse.MathLib.MathUtil.FromEulerAngles(yaw, pitch, roll));
			return Multiverse.MathLib.MathUtil.FromEulerAngles(yaw, pitch, roll);
#else
			return yawRot;
#endif
		}

        /// <summary>
        /// Helper function used by the client api to add a client only object
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="name"></param>
        /// <param name="meshName"></param>
        /// <param name="submeshInfo"></param>
        /// <param name="location"></param>
        /// <param name="orientation"></param>
        /// <param name="scale"></param>
        /// <param name="objType"></param>
        /// <param name="followTerrain"></param>
        /// <returns></returns>
        public ObjectNode AddLocalObject(long oid, string name, 
                                         string meshName, List<SubmeshInfo> submeshInfo, 
                                         Vector3 location, Quaternion orientation, Vector3 scale,
                                         ObjectNodeType objType, bool followTerrain)
        {
            log.InfoFormat("In AddLocalObject - oid: {0}; name: {1}; meshName: {2}; objType: {3}, followTerrain: {4}",
					       oid, name, meshName, objType, followTerrain);

            ObjectNode node;

            try
            {
                SceneNode sceneNode = ObjectNode.CreateSceneNode(sceneManager, oid, location, orientation);
                sceneNode.ScaleFactor = scale;

                lock (nodeDictionary)
                {
                    if (nodeDictionary.ContainsKey(oid))
                    {
                        log.ErrorFormat("Got duplicate AddLocalObject for oid {0}", oid);
                        // XXX - might want to throw an exception here
                        return null;
                    }
                }

                node = NewObjectNode(oid, objType, name, sceneNode, followTerrain);

                string entityName = string.Format("entity.{0}.{1}", oid, entityCounter++);
                ApplyModelWithoutPreload(node, entityName, meshName, submeshInfo);

                // Pretend this message was created at time 0.  Since the 
                // direction vector is zero, this will not mess up the 
                // interpolation, but will allow any of the server's queued 
                // dirloc messages about this object to be processed
                node.SetDirLoc(0, Vector3.Zero, sceneNode.Position);
                node.SetOrientation(sceneNode.Orientation);

                if (node is Player && node.Oid == playerId)
                    SetPlayer(node);

                lock (nodeDictionary)
                {
                    nodeDictionary[oid] = node;
                    if (ObjectAdded != null)
                        ObjectAdded(this, node);
                }
            }
            catch (Exception ex)
            {
                // try logging the error here first, before Root is disposed of
                LogUtil.ExceptionLog.Error(ex.Message);
                LogUtil.ExceptionLog.Error(ex.StackTrace);
                throw;
            }
            return node;
        }

		// At some point, I would like to expose the scene node tree, so that
		// I can have objects move with other objects.
		private void AddObjectStub(long oid, string name, Vector3 location,
                                   Vector3 direction, long lastInterp,
                                   Quaternion orientation, Vector3 scale,
								   ObjectNodeType objType, bool followTerrain)
		{
			log.InfoFormat("In AddObjectStub - oid: {0}; name: {1}; objType: {2}, followTerrain: {3}",
					       oid, name, objType, followTerrain);

			SceneNode sceneNode = ObjectNode.CreateSceneNode(sceneManager, oid, location, orientation);
			sceneNode.ScaleFactor = scale;
			ObjectStub stub = new ObjectStub();
			stub.name = name;
			stub.followTerrain = followTerrain;
			stub.objectType = objType;
			stub.sceneNode = sceneNode;
			stub.direction = direction;
            stub.lastInterp = lastInterp;
            objectStubs[oid] = stub;
            if (oid == playerId)
                playerStubInitialized = true;
		}

        private ObjectNode NewObjectNode(long oid, ObjectNodeType objectType, string name, SceneNode sceneNode, bool followTerrain)
        {
            ObjectNode node;

            log.DebugFormat("NewObjectNode oid {0}, objectType {1}, name {2}",  oid, objectType, name);
            switch (objectType)
            {
                case ObjectNodeType.User:
                case ObjectNodeType.Npc:
                    if (player == null && oid == playerId)
                        node = new Player(oid, name, sceneNode, this);
                    else
                        node = new MobNode(oid, name, sceneNode, this);
                    break;
                case ObjectNodeType.Item:
                    node = new ObjectNode(oid, name, sceneNode, objectType, this);
                    break;
                case ObjectNodeType.Prop:
                    node = new ObjectNode(oid, name, sceneNode, objectType, this);
                    staticGeometryHelper.NodeAdded(node);
                    break;
                default:
                    node = null;
                    break;
            }
            node.FollowTerrain = followTerrain;

            return node;
        }

		// At some point, I would like to expose the scene node tree, so that
		// I can have objects move with other objects.
		// Also at some point I should expose multiple mesh files
		private void AddObject(long oid, string meshFile) {
            log.InfoFormat("In AddObject - oid: {0}; model: {1}", oid, meshFile);
			ObjectNode node;

            lock (nodeDictionary) {
                if (nodeDictionary.ContainsKey(oid)) {
                    log.ErrorFormat("Got duplicate AddObject for oid {0}", oid);
					if (!(nodeDictionary[oid] is ObjectNode)) {
                        log.ErrorFormat("AddObject for exisisting non-object entity");
						return;
					}
                    node = nodeDictionary[oid] as ObjectNode;
					ApplyModel(node);
                    return;
			    }
            }

			ObjectStub objStub = GetObjectStub(oid);
            node = NewObjectNode(oid, objStub.objectType, objStub.name, objStub.sceneNode, objStub.followTerrain);

			bool status = ApplyModel(node);
			if (!status)
				return;

            Vector3 p = objStub.sceneNode.Position;
            if (objStub.followTerrain && p.y == 0)
                p.y = GetHeightAt(p);
            
            if (objStub.direction.Length == 0f || !(node is MobNode)) {
                // Pretend this message was created at time 0.  Since the 
                // direction vector is zero, this will not mess up the 
                // interpolation, but will allow any of the server's queued 
                // dirloc messages about this object to be processed
                node.SetDirLoc(0, Vector3.Zero, p);
            }
            else {
                // We have a direction and a last interp time from the
                // ObjectStub, and this is a mobnode.  So adjust the
                // timestamp, and compute the new position.
                long now = WorldManager.CurrentTime;
                node.SetDirLoc(now, objStub.direction, p);
            }

            node.SetOrientation(objStub.sceneNode.Orientation);

            //
            // Add the node to the dictionary first, so that OID lookups in the
            // PlayerInitialized and ObjectAdded event handlers will find it.
            // 
            lock (nodeDictionary)
            {
                nodeDictionary[oid] = node;
            }
            
            //
            // Set the player first, so that PlayerInitialized
            //  triggers before ObjectAdded, and so that the Player
            //  property is initialized for both event handlers.
            //
            if (node is Player && node.Oid == playerId)
            {
                SetPlayer(node);
            }

            if (ObjectAdded != null)
            {
                ObjectAdded(this, node);
            }
            
		}
        
        public void RemoveObject(long oid) {
			log.InfoFormat("Got RemoveObject for oid {0}", oid);
			Debug.Assert(oid != playerId);
			if (oid == playerId) {
				log.ErrorFormat("Attempted to remove the player from the world");
				return;
			}
			// Clear the entry from the preload dictionary
			lock (preloadDictionary) {
				if (preloadDictionary.ContainsKey(oid)) {
					PreloadEntry entry = preloadDictionary[oid];
					entry.cancelled = true;
				}
			}
			// Clear the entry from the world dictionary
			lock (nodeDictionary) {
                if (!nodeDictionary.ContainsKey(oid)) {
                    log.ErrorFormat("Got RemoveObject for invalid oid {0}", oid);
                    return;
                }
				RemoveNode(oid);
			}
            log.InfoFormat("Removed object for oid {0}", oid);
        }

        // Fetch a list of all objects in the world.
        public List<long> GetObjectOidList() {
            List<long> rv = new List<long>();
            lock (nodeDictionary) {
				foreach (long val in nodeDictionary.Keys)
                    rv.Add(val);
            }
            return rv;
        }

		protected ObjectStub GetObjectStub(long oid) {
			lock (objectStubs) {
				return objectStubs[oid];
			}
		}

		public ObjectNode GetObjectNode(long oid) {
			lock (nodeDictionary) {
				WorldEntity node;
                if (nodeDictionary.TryGetValue(oid, out node) && node is ObjectNode)
					return node as ObjectNode;
                return null;
            }
        }

		public ObjectNode GetObjectNode(string name) {
            lock (nodeDictionary) {
				foreach (WorldEntity node in nodeDictionary.Values)
					if (node is ObjectNode) {
						ObjectNode objNode = node as ObjectNode;
						if (objNode.Name == name)
							return objNode;
					}
                return null;
            }
        }

        /// <summary>
        /// Returns a list of all ObjectNode names.  This is used by scripting.
        /// </summary>
        /// <returns></returns>
        public List<string> GetObjectNodeNames()
        {
            List<string> retList = new List<string>();

            lock (nodeDictionary)
            {
                foreach (WorldEntity node in nodeDictionary.Values)
                {
                    ObjectNode objNode = node as ObjectNode;
                    if (objNode != null)
                    {
                        retList.Add(objNode.Name);
                    }
                }
            }

            return retList;
        }

		// The timer event
        public void SendPlayerData(object sender, System.Timers.ElapsedEventArgs e) {
            Monitor.Enter(this);
            try {
                if (player == null || networkHelper == null)
                    return;
                long now = WorldManager.CurrentTime;
      			if (dirLocOrientSupported)
                    SendDirLocOrientMessage(player, now);
                else {
                    SendDirectionMessage(player, now);
                    SendOrientationMessage(player, now);
                }
            } finally {
                Monitor.Exit(this);
            }
        }

		protected void SendOrientationMessage(Player node, long now) {
			if (node.orientUpdate.dirty) {
				Quaternion orient = node.orientUpdate.orientation;
				node.orientUpdate.dirty = false;
				networkHelper.SendOrientationMessage(orient);
			} else {
				// normal periodic send
				Quaternion orient = node.Orientation;
				networkHelper.SendOrientationMessage(orient);
			}
            node.lastOrientSent = now;
		}

		protected void SendDirectionMessage(Player node, long now) {
			if (node.dirUpdate.dirty) {
				long timestamp = node.dirUpdate.timestamp;
				Vector3 dir = node.dirUpdate.direction;
				Vector3 pos = node.dirUpdate.position;
				networkHelper.SendDirectionMessage(timestamp, dir, pos);
				node.dirUpdate.dirty = false;
            } else {
				long timestamp = node.dirUpdate.timestamp;
				Vector3 dir = node.Direction;
				Vector3 pos = node.Position;
				networkHelper.SendDirectionMessage(timestamp, dir, pos);
			}
            node.lastDirSent = now;
		}

		protected void SendDirLocOrientMessage(Player node, long now) {
            if (node.dirUpdate.dirty || node.orientUpdate.dirty) {
				long timestamp = node.dirUpdate.timestamp;
				Vector3 dir = node.dirUpdate.direction;
				Vector3 pos = node.dirUpdate.position;
				Quaternion orient = node.orientUpdate.orientation;
                networkHelper.SendDirLocOrientMessage(timestamp, dir, pos, orient);
				node.dirUpdate.dirty = false;
				node.orientUpdate.dirty = false;
            } else {
                // normal periodic send
				long timestamp = node.dirUpdate.timestamp;
				Vector3 dir = node.Direction;
				Vector3 pos = node.Position;
				Quaternion orient = node.Orientation;
				networkHelper.SendDirLocOrientMessage(timestamp, dir, pos, orient);
            }
            node.lastDirSent = now;
            node.lastOrientSent = now;
        }
        
        protected static TimingMeter tickMeter = MeterManager.GetMeter("Tick Nodes", "WorldManager");
		protected static TimingMeter directionMeter = MeterManager.GetMeter("Direction Message", "WorldManager");
		protected static TimingMeter orientationMeter = MeterManager.GetMeter("Orientation Message", "WorldManager");


        public bool LoadPreloadEntry() {
            PreloadEntry entry = GetUnloadedPreloadEntry();
            if (entry == null) {
                return false;
            }
            try {
                log.InfoFormat("Preparing entity for {0}", entry.entityName);
                long before = TimeTool.CurrentTime;
                if (entry.submeshList != null) {
                    foreach (SubmeshInfo submeshInfo in entry.submeshList) {
                        Material material = MaterialManager.Instance.GetByName(submeshInfo.MaterialName);
                        if (material == null)
                            continue;
                        // ensure the material is loaded.  It will skip it if it already is
                        // This must happen in the render thread (directx is no good multithreaded)
                        material.Load();
                    }
                }
                Debug.Assert(entry.meshFile != null && entry.meshFile != "");
                Mesh mesh = MeshManager.Instance.Load(entry.meshFile);
                entry.loaded = true;
                long ms = TimeTool.CurrentTime - before;
                log.InfoFormat("Prepared entity {0} for {1}, took {2}ms", entry.meshFile, entry.entityName, ms);

            } catch (Exception ex) {
                LogUtil.ExceptionLog.ErrorFormat("Unable to create entity; Exception: {0}", ex);
                entry.loaded = true;
            }
            return true;
        }

		/// <summary>
        ///   Called on each frame to update the nodes
        /// </summary>
        /// <param name="timeSinceLastFrame">time since the last frame in seconds</param>
		public void OnFrameStarted(float timeSinceLastFrame, long now) {
			Monitor.Enter(sceneManager);
			try {
                // TODO: This shouldn't be here.  It should be in another thread.
                // Load at most one new object per frame
                // LoadPreloadEntry();

				if (player == null)
					return;
				tickMeter.Enter();
                long freq = System.Diagnostics.Stopwatch.Frequency;
				lock (nodeDictionary) {
                    foreach (WorldEntity node in nodeDictionary.Values)
                    {
                        if (node is ObjectNode)
                        {
                            ((ObjectNode)node).Tick(timeSinceLastFrame);
                        }
                    }
				}

				tickMeter.Exit();

				bool sendDir = (player.dirUpdate.dirty && (player.lastDirSent + DirUpdateInterval < now));
                bool sendOrient = (player.orientUpdate.dirty && (player.lastOrientSent + OrientUpdateInterval < now));
                // If the server supports the DirLocOrientMessage, use
                // that because it minimizes network traffic
                if (dirLocOrientSupported) {
                    if (sendDir || sendOrient) {
                        log.DebugFormat("In OnFrameStarted, sending DirLocOrient message dir = {0}, pos = {1}, orient = {2}", 
                                        player.Direction, player.Position, player.Orientation);
                        SendDirLocOrientMessage(player, now);
                    }
                }
                else {
                    if (sendDir) {
                        log.DebugFormat("In OnFrameStarted, sending direction message dir = {0}, pos = {1}", 
                                        player.Direction, player.Position);
                        directionMeter.Enter();
                        SendDirectionMessage(player, now);
                        directionMeter.Exit();
                    }
                    if (sendOrient) {
                        log.DebugFormat("In OnFrameStarted, sending orientation message orientation = {0}", 
                                        player.Orientation);
                        orientationMeter.Enter();
                        SendOrientationMessage(player, now);
                        orientationMeter.Exit();
                    }
                }
                //Logger.Log(1, "About to run {0} TimeInterpolators", timeInterpolators.Count);
				List<TimeInterpolator> expiredInterpolators = new List<TimeInterpolator>();
				foreach(TimeInterpolator interpolator in timeInterpolators) {
					// Update the elapsed fraction of time
					float elapsedFraction = interpolator.UpdateElapsedFraction();
					//Logger.Log(1, "Running timing interpolator {0}, elapsedFraction is {1}",
					//		   interpolator, elapsedFraction);
					// If the elapsed time is less than 1.0, run the
					// Update method; if it is 1.0 or greater, make
					// the interpolation go away
					if (elapsedFraction < 1.0f)
						interpolator.UpdateTimeFraction(this, elapsedFraction);
					else
						expiredInterpolators.Add(interpolator);
				}
				foreach(TimeInterpolator timeInterpolator in expiredInterpolators) {
					timeInterpolators.Remove(timeInterpolator);
					timeInterpolator.ExpireInterpolator(this);
				}
				
                if (voiceMgr != null)
                    voiceMgr.Tick();
                
                // Also update animation states for objects that are not
				// on the server.

			} finally {
				Monitor.Exit(sceneManager);
			}
		}

        /// <summary>
        ///   Sets up the data needed for interpolated movement of an object.
        ///   In this case, it is the direction, location, and timestamp when 
        ///   that location was the current location.
        /// </summary>
        /// <param name="oid">object id of the object for which we are setting the dir and loc</param>
        /// <param name="timestamp">time that the message was created (in client time)</param>
        /// <param name="dir">direction of motion of the object</param>
        /// <param name="loc">initial location of the object (at the time the message was created)</param>
		private void SetDirLoc(long oid, long timestamp, Vector3 dir, Vector3 loc) {
			ObjectNode node = GetObjectNode(oid);
            if (node != null)
                node.SetDirLoc(timestamp, dir, loc);
            else
                log.WarnFormat("No node match for: {0}", oid);
        }

        private void SetOrientation(long oid, Quaternion orient) {
            ObjectNode node = GetObjectNode(oid);

			if (node != null && node == player) {
				log.InfoFormat("Server set player orientation to {0} instead of {1}",
							   orient, player.Orientation);
                Multiverse.Input.DefaultInputHandler inputHandler = Client.Instance.InputHandler as Multiverse.Input.DefaultInputHandler;
                if (inputHandler != null) {
                    float pitch, yaw, roll;
                    orient.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                    float yawDifference = yaw - inputHandler.PlayerYaw;
                    inputHandler.PlayerYaw = yaw;
                    inputHandler.CameraYaw += yawDifference;
                }
            }
			if (node != null)
				node.SetOrientation(orient);
			else
				log.WarnFormat("No node match for: {0}", oid);
		}

        public List<MobNode> GetMobNodes() {
			List<MobNode> rv = new List<MobNode>();
			lock (nodeDictionary) {
				foreach (WorldEntity node in nodeDictionary.Values)
					if (node is MobNode)
						rv.Add((MobNode)node);
				return rv;
			}
		}

		public List<LightEntity> GetLightNodes() {
			List<LightEntity> rv = new List<LightEntity>();
			lock (nodeDictionary) {
				foreach (WorldEntity node in nodeDictionary.Values)
					if (node is LightEntity)
						rv.Add((LightEntity)node);
				return rv;
			}
		}

        private void OnPlayerInitialized()
        {
            PlayerInitializedHandler handler = PlayerInitializedEvent;
            if (handler != null)
            {
                handler(null, null);
            }
        }

        private void SetPlayer(ObjectNode node) {
            player = (Player)node;
            if (player != null)
            {
                player.Disposed += new ObjectNodeDisposed(PlayerDisposed);
            }
            OnPlayerInitialized();
        }

        void PlayerDisposed(ObjectNode objNode)
        {
            if ((player != null) || (objNode == player))
            {
                SetPlayer(null);
            }
        }

		public string TranslateHostname(string hostname) {
            if (networkHelper != null && networkHelper.LoginPluginHost != "" && hostname == ":same")
                return networkHelper.LoginPluginHost;
            else
                return hostname;
        }
        
        public long PlayerId {
            get {
				Monitor.Enter(this);
				long rv = playerId;
				Monitor.Exit(this);
				return rv;
			}
			set {
				Monitor.Enter(this);
				playerId = value;
				playerInitialized = true;
                SetPlayer(null);
				Monitor.Exit(this);
			}
        }

		public bool PlayerInitialized {
			get {
				Monitor.Enter(this);
				bool rv = playerInitialized;
				Monitor.Exit(this);
				return rv;
			}
		}

        public bool TerrainInitialized {
            get {
                return terrainInitialized;
            }
        }

        public bool PlayerStubInitialized {
            get {
                return playerStubInitialized;
            }
        }
        
        public bool LoadCompleted {
            get {
                return loadCompleted;
            }
        }

        public NetworkHelper NetworkHelper
        {
            set
            {
                networkHelper = value;
            }
            get
            {
                return networkHelper;
            }
        }

        public CollisionAPI CollisionHelper
        {
            set
            {
                collisionManager = value;
            }
            get
            {
                return collisionManager;
            }
        }

        public SceneManager SceneManager {
            get {
                return sceneManager;
            }
            set {
                try {
                    Monitor.Enter(this);
                    sceneManager = value;
                } finally {
                    Monitor.Exit(this);
                }
            }
        }

        public Axiom.SceneManagers.Multiverse.TerrainDecalManager DecalManager
        {
            get
            {
                if (SceneManager is Axiom.SceneManagers.Multiverse.SceneManager)
                {
                    return Axiom.SceneManagers.Multiverse.TerrainManager.Instance.TerrainDecalManager;
                }
                else
                {
                    return null;
                }
            }
        }

        public Player Player {
            get {
                Monitor.Enter(this);
                Player rv = player;
                Monitor.Exit(this);
                return rv;
            }
        }

		public static long CurrentTime {
            
			get {
                return Multiverse.Utility.TimeTool.CurrentTime;
			}
		}

        // Kludge: This is referenced by interface IWorldManager, and
        // interfaces can't refer to static properties, so define this
        // non-static property
        public long CurrentTimeValue {
            get {
                return WorldManager.CurrentTime;
            }
        }
        
        public string ServerCapabilities
        {
            set {
                string [] capabilities = value.Split(',');
                // For now, we're only interested in DirLocEvent
                foreach (string s in capabilities) {
                    string ts = s.Trim();
                    if (ts == "DirLocEvent")
                        dirLocOrientSupported = true;
                }
            }
        }

        public VoiceManager VoiceMgr 
        {
            get {
                return voiceMgr;
            }
            set {
                voiceMgr = value;
                if (voiceMgr != null) {
                    voiceMgr.onVoiceAllocation += OnVoiceAllocation;
                    voiceMgr.onVoiceDeallocation += OnVoiceDeallocation;
                }
            }
        }

        public Axiom.SceneManagers.Multiverse.ILODSpec WorldLODSpec 
        {
            get { return worldLODSpec; }
            set { worldLODSpec = value; }
        }

        public Dictionary<long, WorldEntity> NodeDictionary
		{
			get { return nodeDictionary; }
        }

        public void RegisterObjectPropertyChangeHandler(string propName, ObjectPropertyChangeEventHandler handler)
        {
            if (!objectPropertyChangeHandlers.ContainsKey(propName))
            {
                objectPropertyChangeHandlers[propName] = new List<ObjectPropertyChangeEventHandler>();
            }

            List<ObjectPropertyChangeEventHandler> handlers = objectPropertyChangeHandlers[propName];

            handlers.Add(handler);
        }

        public void RemoveObjectPropertyChangeHandler(string propName, ObjectPropertyChangeEventHandler handler)
        {
            List<ObjectPropertyChangeEventHandler> handlers = objectPropertyChangeHandlers[propName];

            handlers.Remove(handler);

            // remove the list if there are no more handlers for this property
            if (handlers.Count == 0)
            {
                objectPropertyChangeHandlers.Remove(propName);
            }
        }
        
        /// <summary>
        /// Invoke object property change events
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="objectNode"></param>
        public void OnObjectPropertyChange(string propName, ObjectNode objectNode)
        {
            if (objectPropertyChangeHandlers.ContainsKey(propName))
            {
                foreach (ObjectPropertyChangeEventHandler handler in objectPropertyChangeHandlers[propName])
                {
                    handler(objectNode, new ObjectPropertyChangeEventArgs(objectNode.Oid, propName));
                }
            }
        }
	}

	public class DistanceComparer : IComparer<ObjectNode>, IComparer<MobNode>
	{
		Vector3 point;
		public DistanceComparer(ObjectNode node) {
			this.point = node.Position;
		}
		public int Compare(ObjectNode x, ObjectNode y) {
			float deltaX = (x.Position - point).LengthSquared;
			float deltaY = (y.Position - point).LengthSquared;
			return deltaX.CompareTo(deltaY);
		}
		public bool Equals(ObjectNode x, ObjectNode y) {
			return x.Position.Equals(y.Position);
		}
		public int GetHashCode(ObjectNode x) {
			return x.Position.GetHashCode();
		}
		public int Compare(MobNode x, MobNode y) {
			return Compare((ObjectNode)x, (ObjectNode)y);
		}
		public bool Equals(MobNode x, MobNode y) {
			return Equals((ObjectNode)x, (ObjectNode)y);
		}
		public int GetHashCode(MobNode x) {
			return GetHashCode((ObjectNode)x);
		}

	}

	public abstract class TimeInterpolator
	{
        public TimeInterpolator(WorldManager worldManager, long timestamp, int timeInterval)
        {
			// This call to AdjustTimestamp remains here, because it
			// won't be done during message parsing, because the
			// timestamp is a property.  Fortunately, all this
			// TimeInterpolator is going away anyway.
            this.timestamp = worldManager.NetworkHelper.AdjustTimestamp(timestamp, WorldManager.CurrentTime);
			this.timeInterval = timeInterval;
			this.elapsedFraction = 0f;
		}
		
        public TimeInterpolator(WorldManager worldManager, int timeInterval)
        {
			this.timestamp = WorldManager.CurrentTime;
			this.timeInterval = timeInterval;
			this.elapsedFraction = 0f;
		}
		
		public float UpdateElapsedFraction() {
			// Calculate the fraction of the time that has gone by
			elapsedFraction = ((float)(WorldManager.CurrentTime - timestamp)) / (float)timeInterval;
			return elapsedFraction;
		}

		// The timeFraction is a float greater than or equal to 0.0,
		// and less than 1.0.  This method updates the state of the
		// interpolator based on the fraction.
		public abstract void UpdateTimeFraction(WorldManager worldManager, float timeFraction);
		
		// This method is called when the time fraction gets to 1.0 or
		// greater; the interpolator should do away with itself
		public abstract void ExpireInterpolator(WorldManager worldManager);
		
		protected float elapsedFraction;  // As a fraction of the time
									  // between timestamp and the
									  // timeToImpact
		protected int timeInterval;  // Milliseconds
		protected long timestamp;

        public int TimeInterval {
            get {
                return timeInterval;
            }
            set {
                timeInterval = value;
            }
        }

        public long Timestamp {
            get {
                return timestamp;
            }
            set {
                timestamp = value;
            }
        }

	}

	public class ProjectileTracker : TimeInterpolator
	{
        // If targetSocket is the null string, then the this is a
        // location tracker

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ProjectileTracker));

		public ProjectileTracker(WorldManager worldManager, long parentOid, long projectileOid,
								 int timeToImpact, long timestamp, float height) 
                : base(worldManager, timestamp, timeToImpact) {
			this.parentOid = parentOid;
			this.projectileOid = projectileOid;
			this.height = height;
			this.targetOid = 0;
		}

		public void InitializeObjectTarget(WorldManager worldManager)
		{
			targetNode = worldManager.GetObjectNode(targetOid);
			if (targetNode == null) {
                log.WarnFormat("Tracking interpolation projectile from nonexistent target: {0}",
							   targetOid);
				return;
			}
			AttachmentPoint socket = null;
			string slotName = targetSocket;
			if (targetNode.AttachmentPoints.ContainsKey(slotName))
				socket = targetNode.AttachmentPoints[slotName];
			else {
				socket = new AttachmentPoint(slotName, null, Quaternion.Identity, Vector3.Zero);
				newAttachmentPoint = socket;
			}
			newNodeOid = worldManager.GetLocalOid();
			newNode = targetNode.AttachNode(socket, newNodeOid);
			newNode.NeedUpdate();
            log.InfoFormat("In ProjectileTracker.InitializeObjectTarget, targetNode {0}, socket {1}, newTagPOintOid {2}, newNode {3}",
					       targetNode, socket, newNodeOid, newNode);
		}
			
		public void InitializeProjectile(WorldManager worldManager)
		{
			// Remove the projectile from its parent node
			ObjectNode parentNode = worldManager.GetObjectNode(parentOid);
			if (parentNode == null) {
                log.WarnFormat("Tracking interpolation projectile from nonexistent parent: {0}",
							parentOid);
				return;
			}
            log.InfoFormat("Tracking interpolation projectile oid {0}, parentOid {1}, targetOid {2}, targetSocket {3}",
					       projectileOid, parentOid, targetOid, targetSocket);
			Axiom.Core.Node node = parentNode.Attachments[projectileOid];
			Vector3 p = node.DerivedPosition;
			Quaternion q = node.DerivedOrientation;
			if (node == null) {
                log.WarnFormat("Tracking interpolation projectile for nonexistent particle system: {0}",
							   projectileOid);
				return;
			}
			ParticleSystem particleSystem = null;
			string entityName = "_particle_" + projectileOid;
			if (node is TagPoint) {
				TagPoint tagPointNode = (TagPoint)node;
                log.InfoFormat("Found particle system in TagPoint {0}", tagPointNode);
				particleSystem = (ParticleSystem)tagPointNode.ChildObject;
			}
			else if (node is SceneNode) {
				SceneNode sceneNode = (SceneNode)node;
				foreach (MovableObject obj in sceneNode.Objects) {
					if (obj.Name == entityName) {
						particleSystem = (ParticleSystem)obj;
                        log.InfoFormat("Found particle system in SceneNode {0}", sceneNode);
						break;
					}
				}
			}	
			if (particleSystem == null) {
				log.WarnFormat("Could not find particle system for tracking projectile {0}",
							   projectileOid);
				return;
			}
			parentNode.DetachObject(projectileOid);
			projectile = (SceneNode)worldManager.SceneManager.RootSceneNode.CreateChild();
			projectile.AttachObject(particleSystem);
			projectile.Position = p;
			projectile.Orientation = q;
			startingLocation = p;
			log.InfoFormat("Adding projectile time interpolator {0}, projectileNode Visible {1}, position {2}, orientation {3}, target {4}", 
					       this, projectile.Visible, p, q, Target(worldManager));
		}

		public Vector3 Target(WorldManager worldManager) {
            if (targetOid == 0)
                return targetLocation;
            else
				return newNode.DerivedPosition;
		}
		
		public override void UpdateTimeFraction(WorldManager worldManager, float timeFraction) {
			Vector3 previous = projectile.Position;
			Vector3 start = StartingLocation;
			Vector3 target = Target(worldManager);
			Vector3 pos = start + (target - start) * timeFraction;
			if (height != 0f) {
				float x = (timeFraction - .5f);
				pos.y += height * (-4f * x * x + 1);
			}
			projectile.Position = pos;
			//Logger.Log(1, "In UpdateTimeFraction, start {0}, previous position {1}, new position {2}, target {3}, timeFraction {4}",
			//           start, previous, projectile.Position, target, timeFraction);
		}
		
		public override void ExpireInterpolator(WorldManager worldManager) {
			projectile.DetachAllObjects();
			projectile.RemoveFromParent();
            string entityName = "_particle_" + projectileOid;
			ParticleSystemManager.Instance.RemoveSystem(entityName);
			if (targetOid != 0) {
				targetNode.DetachNode(newNodeOid, true);
				if (newAttachmentPoint != null)
					targetNode.AttachmentPoints.Remove(targetSocket);
			}
			worldManager.SceneManager.RootSceneNode.RemoveChild(projectile);
		}
		
		public override string ToString() {
			return string.Format("ProjectileTracker oid {0}, targetSocket {1}, targetOid {2}",
								 projectileOid, targetSocket, targetOid);
		}
		
		protected long parentOid;
		protected long projectileOid;
		protected Axiom.Core.SceneNode projectile;
		protected Vector3 startingLocation;
		protected Vector3 targetLocation;
		protected float height;

        protected string targetSocket = "";
        protected long targetOid;
        protected ObjectNode targetNode;
		protected Node newNode = null;
		protected long newNodeOid;
		protected AttachmentPoint newAttachmentPoint = null;
		
		public long ParentOid {
			get { return parentOid; }
			set { parentOid = value; }
		}

		public long ProjectileOid {
			get { return projectileOid; }
			set { projectileOid = value; }
		}

		public Axiom.Core.SceneNode Projectile {
			get { return projectile; }
			set { projectile = value; }
		}

		public float ElapsedFraction {
            get { return elapsedFraction; }
            set { elapsedFraction = value; }
        }
		
        public string TargetSocket {
            get { return targetSocket; }
            set { targetSocket = value; }
        }

		public Vector3 TargetLocation {
			get { return targetLocation; }
			set { targetLocation = value; }
		}

        public long TargetOid
        {
            get { return targetOid; }
            set { targetOid = value; }
        }

		public Vector3 StartingLocation 
		{
			get { return startingLocation; }
			set { startingLocation = value; }
		}

    }

}
