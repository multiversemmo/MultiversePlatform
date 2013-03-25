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

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Collections;
using Axiom.Utility;

using Multiverse.Network;
using Multiverse.Config;

#endregion

namespace Multiverse.Test
{
	public struct DirUpdate 
	{
		public bool dirty;
		public long timestamp; // in local time
		public Vector3 direction;
		public Vector3 position;
	}

	public struct OrientationUpdate
	{
		public bool dirty;
		public Quaternion orientation;
	}

	public class TestObject {
		public long Oid;
		public DirUpdate dirUpdate;
		public OrientationUpdate orientUpdate;
		public Vector3 position;
		public Vector3 direction;
		public Vector3 scale;
		public Quaternion orientation;
		public bool followTerrain;
		public string name;
		public ObjectNodeType objectType;

		protected long lastDirTimestamp = -1;
		protected long lastLocTimestamp = -1;
		protected Vector3 lastPosition;
		protected Vector3 lastDirection;
		public long lastDirSent;
		public long lastOrientSent;

		public TestObject(long Oid, string name)
		{
			this.Oid = Oid;
			this.name = name;
		}
		
		public void SetDirection(Vector3 dir, Vector3 pos) {
			position = pos;
			if (dir != direction) {
				dirUpdate.direction = dir;
				dirUpdate.position = pos;
				dirUpdate.dirty = true;
				direction = dir;
			}
		}

		public TestObject()
		{
		}
		
		public ObjectNodeType ObjectType
		{
			get {
				return objectType;
			}
			set {
				objectType = value;
			}
		}

		public Vector3 Position
		{
			get {
				return position;
			}
			set {
				position = value;
			}
		}

		public Vector3 Direction
		{
			get {
				return direction;
			}
			set {
				direction = value;
			}
		}

		public string Name
		{
			get {
				return name;
			}
			set {
				name = value;
			}
		}

		public Quaternion Orientation
		{
			get {
				return orientation;
			}
			set {
				if (value != orientation) {
					orientUpdate.orientation = value;
					orientUpdate.dirty = true;
				}
			}
		}

		public bool FollowTerrain
		{
			get {
				return followTerrain;
			}
			set {
				followTerrain = value;
			}
		}

		public void SetDirLoc(long timestamp, Vector3 dir, Vector3 pos) {
			Logger.Log(0, "dir for node {0} = {1} timestamp = {2}/{3}", this, dir, timestamp, System.Environment.TickCount);
			if (timestamp <= lastDirTimestamp) {
				Logger.Log(0, string.Format("ignoring dirloc,since timestamps are too close {0}, {1}", timestamp, lastDirTimestamp));
				return;
			}
			// timestamp is newer.
			lastDirTimestamp = timestamp;
			lastPosition = pos;
			lastDirection = dir;
			SetLoc(timestamp, pos);
		}

		protected void SetLoc(long timestamp, Vector3 loc) {
			if (timestamp <= lastLocTimestamp)
				return;
			// timestamp is newer.
			lastLocTimestamp = timestamp;
			this.Position = loc;
			Logger.Log(0, "loc for node {0} = {1}/{2}", this, loc, this.Position);
		}

		public virtual void SetOrientation(Quaternion orient) {
			orientation = orient;
		}
	}

	public class WorldManager : IWorldManager
    {
		//public WorldManager worldManager = null;
	
		public delegate void ObjectEventHandler(object sender, TestObject objNode);

		public class PreloadEntry {
			public bool loaded;
			public bool cancelled;
			public string entityName;
			public long oid;
			public string meshFile;
			public string[] submeshNames;
			public string[] materialNames;
		}

		long playerId = 0;
		bool playerInitialized = false;
        bool playerStubInitialized = false;
        bool terrainInitialized = false;
        bool loadCompleted = false;
        long nextLocalOid = int.MaxValue;

        // The network helper object
        NetworkHelper networkHelper = null;
		// The player object
        TestObject player = null;

		Thread asyncLoadThread = null;
		Thread asyncBehaviorThread = null;
		Thread verifyServerTimeoutThread = null;
		bool shuttingDown = false;

		// Timer for updating the server with the player's position and other information
		System.Timers.Timer playerUpdateTimer;

		// Complete objects
		Dictionary<long, TestObject> nodeDictionary;

		// Intermediate entities - there are three states for entries here.
		// 1) No entry - we haven't requested a load of the entry yet
		// 2) Entry exists, loaded is false - we haven't loaded the mesh and materials
		// 3) Entry exists, loaded is true  - we have loaded the mesh and materials
		Dictionary<long, PreloadEntry> preloadDictionary;

		public event ObjectEventHandler ObjectAdded;
		public event ObjectEventHandler ObjectRemoved;

		int entityCounter = 0;
		private bool verifyServer = false;

		protected BehaviorParms behaviorParms = null;
		
		public WorldManager(bool verifyServer, BehaviorParms behaviorParms) {
			this.verifyServer = verifyServer;
			this.behaviorParms = behaviorParms;
		}

		public void Dispose() {
			shuttingDown = true;
			if (asyncLoadThread != null) {
				asyncLoadThread.Interrupt();
				asyncLoadThread.Join();
				asyncLoadThread = null;
			}
			if (asyncBehaviorThread != null) {
				asyncBehaviorThread.Interrupt();
				asyncBehaviorThread.Join();
				asyncBehaviorThread = null;
			}
			if (networkHelper != null) {
				networkHelper.Dispose();
				networkHelper = null;
			}
			if (verifyServerTimeoutThread != null) {
				verifyServerTimeoutThread.Interrupt();
				verifyServerTimeoutThread.Join();
				verifyServerTimeoutThread = null;
			}
		}

		public void Init(Client client) {
			nodeDictionary = new Dictionary<long, TestObject>();

			preloadDictionary = new Dictionary<long, PreloadEntry>();
			
			playerUpdateTimer = new System.Timers.Timer();
            playerUpdateTimer.Enabled = true;
            playerUpdateTimer.Interval = behaviorParms.playerUpdateInterval;
            playerUpdateTimer.Elapsed +=
               new System.Timers.ElapsedEventHandler(this.SendPlayerData);

			asyncLoadThread = new Thread(new ThreadStart(this.AsyncLoadEntities));
			asyncLoadThread.Name = "Async Resource Loader";
			asyncLoadThread.Start();

			asyncBehaviorThread = new Thread(new ThreadStart(this.AsyncBehavior));
			asyncBehaviorThread.Name = "Async Player Behavior";
			asyncBehaviorThread.Start();
			
			if (verifyServer) {
				verifyServerTimeoutThread = new Thread(new ThreadStart(this.VerifyServerTimeout));
				verifyServerTimeoutThread.Name = "Verify Server Timeout";
				verifyServerTimeoutThread.Start();
			}
			
			MessageDispatcher.Instance.RegisterPreHandler(WorldMessageType.ModelInfo,
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
            //MessageDispatcher.Instance.RegisterHandler(MessageType.StatUpdate,
            //                                           new MessageHandler(this.HandleStatUpdate));
            //MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Animation,
            //                                           new WorldMessageHandler(this.HandleAnimation));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Attach,
													   new WorldMessageHandler(this.HandleAttach));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Detach,
													   new WorldMessageHandler(this.HandleDetach));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Comm,
													   new WorldMessageHandler(this.HandleComm));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Sound,
													   new WorldMessageHandler(this.HandleSound));
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
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.RoadInfo,
													   new WorldMessageHandler(this.HandleRoadInfo));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Fog,
													   new WorldMessageHandler(this.HandleFogMessage));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AddParticleEffect,
                                                       new WorldMessageHandler(this.HandleAddParticleEffect));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.RemoveParticleEffect,
                                                       new WorldMessageHandler(this.HandleRemoveParticleEffect));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.ObjectProperty,
                                                       new WorldMessageHandler(this.HandleObjectProperty));


			// Initialize the network helper
			networkHelper.Init();

		}

        public long GetLocalOid() {
            return nextLocalOid++;
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

		public void VerifyServerTimeout() 
		{
			// Sleep for 5 minutes.  If we haven't terminated yet, log
			// the fact, and exit with error code -1
			Thread.Sleep(5 * 60 * 1000);
			// We're still alive, so the verification failed
			Logger.Log(4, "In --verify_server mode, 5 minutes has elapsed and we haven't gotten the --verify_server Comm message, so exiting with exit code -1");
			System.Environment.Exit(-1);
		}
		
		/// <summary>
		///   This method can be called from another thread and will preload the entity.
		///   TODO: Lock the underlying EntityManager.entityList
		/// </summary>
		public void AsyncLoadEntities() {
			while (!shuttingDown) {
				try {
					PreloadEntry entry = GetUnloadedPreloadEntry();
					if (entry == null) {
						Thread.Sleep(100);
						continue;
					}
					try {
						Logger.Log(3, "Preparing entity for {0}", entry.entityName);
// 						if (entry.materialNames != null) {
// 							foreach (string materialName in entry.materialNames) {
// 								Material material = MaterialManager.Instance.GetByName(materialName);
// 								if (material == null)
// 									continue;
// 								// ensure the material is loaded.  It will skip it if it already is
// 								material.Load();
// 							}
// 						}
// 						Debug.Assert(entry.meshFile != null && entry.meshFile != "");
// 						Mesh mesh = MeshManager.Instance.Load(entry.meshFile);

						// Put the entry in our node dictionary
						lock(nodeDictionary) {
							nodeDictionary[entry.oid] = new TestObject(entry.oid, entry.entityName);
						}
						entry.loaded = true;
						Logger.Log(3, "Prepared entity {0} for {1}", entry.meshFile, entry.entityName);
					} catch (AxiomException ex) {
						Trace.TraceError("Unable to create entity; AxiomException: " + ex);
						entry.loaded = true;
					}
				} catch (ThreadInterruptedException) {
					continue;
				} catch (ThreadAbortException e) {
					Logger.Log(4, "Aborting thread due to " + e);
					return;
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
		public void SetModel(TestObject node, string meshFile, List<string> subMeshNames, List<string> materialNames) {
			for (int tries = 0; tries < 100; ++tries) {
				if (PrepareModel(node.Oid, meshFile, subMeshNames, materialNames)) {
					ApplyModel(node);
					return;
				}
				Thread.Sleep(100);
			}
			Logger.Log(4, "Timed out trying to change model");
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
		protected bool PrepareModel(long oid, string meshFile, List<string> submeshNames, List<string> materialNames) {
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
						//Logger.Log(1, "still preparing model for " + oid);
					}
				} else {
					// start loading the model
					PreloadEntry entry = new PreloadEntry();
					entry.loaded = false;
					entry.cancelled = false;
					entry.meshFile = meshFile;
					entry.submeshNames = (submeshNames == null ? null : submeshNames.ToArray());
					entry.materialNames = (materialNames == null ? null : materialNames.ToArray());
					entry.entityName = string.Format("entity.{0}.{1}", oid, entityCounter++);
					entry.oid = oid;
					// entry.entity = null;
					preloadDictionary[oid] = entry;
					//Logger.Log(0, "preparing model for " + oid);
				}
			} finally {
				Monitor.Exit(preloadDictionary);
			}
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
            // TODO: I changed the way these submeshes are handled, and don't want to
            // bother maintaining test.  Just skip this for now.
			// bool rv = PrepareModel(modelInfo.Oid, meshInfo.MeshFile, meshInfo.SubmeshList);
            // message.DelayHandling = !rv;
            message.DelayHandling = false;
		}
		#endregion

		#region Message handlers
		public void HandleNewObject(BaseWorldMessage message) {
			if (!this.TerrainInitialized) {
				Trace.TraceError("Ignoring new object message, since terrain is not initialized");
				return;
			}
			NewObjectMessage newObj = (NewObjectMessage)message;
			AddObjectStub(newObj.ObjectId, newObj.Name, newObj.Location, 
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
			Logger.Log(0, "Got dir message at {0}:{1}", now, dirMessage.Timestamp);
			if (dirMessage.Oid == playerId)
				Trace.TraceWarning("Got DirLoc for Player");
			SetDirLoc(dirMessage.Oid, dirMessage.Timestamp, dirMessage.Direction,
					  dirMessage.Location);
		}
		public void HandleOrientation(BaseWorldMessage message) {
			OrientationMessage orientMessage = (OrientationMessage)message;
			SetOrientation(orientMessage.Oid, orientMessage.Orientation);
		}
        //public void HandleStatUpdate(BaseMessage message) {
        //    StatUpdateMessage statUpdateMessage = (StatUpdateMessage)message;
        //    TestObject objNode = GetObjectNode(statUpdateMessage.Oid);
        //    if (objNode == null) {
        //        Trace.TraceWarning("Got stat update message for nonexistent object: " + message.Oid);
        //        return;
        //    }
        //    objNode.UpdateStats(statUpdateMessage.StatValues);
        //}
        //public void HandleState(BaseMessage message) {
        //    StateMessage stateMessage = (StateMessage)message;
        //    TestObject objNode = GetObjectNode(stateMessage.Oid);
        //    if (objNode == null) {
        //        Trace.TraceWarning("Got state message for nonexistent object: " + message.Oid);
        //        return;
        //    }
        //    objNode.UpdateStates(stateMessage.States);
        //}
        //public void HandleAnimation(BaseWorldMessage message) {
        //    AnimationMessage animMessage = (AnimationMessage)message;
        //    TestObject objNode = GetObjectNode(animMessage.Oid);
        //    if (objNode == null) {
        //        Trace.TraceWarning("Got animation message for nonexistent object: " + message.Oid);
        //        return;
        //    }
        //    if (animMessage.Clear) {
        //        Logger.Log(1, "Cleared animation queue for: " + objNode.Oid);
        //    }
        //    List<AnimationEntry> animations = animMessage.Animations;
        //    foreach (AnimationEntry animEntry in animations) {
        //        Logger.Log(1, "Queued animation " +
        //            animEntry.animationName + " for " + objNode.Oid + 
        //            " with loop set to " + animEntry.loop);
        //    }
        //}
		public void HandleAttach(BaseWorldMessage message) {
			AttachMessage attachMessage = (AttachMessage)message;
            TestObject parentObj = GetObjectNode(attachMessage.Oid);
			if (parentObj == null) {
				Trace.TraceWarning("Got attach message for nonexistent object: " + message.Oid);
				return;
			}
		}
		public void HandleDetach(BaseWorldMessage message) {
			DetachMessage detachMessage = (DetachMessage)message;
            TestObject parentObj = GetObjectNode(detachMessage.Oid);
			if (parentObj == null) {
                Trace.TraceWarning("Got detach message for nonexistent object: " + message.Oid);
				return;
			}
		}
		public void HandleComm(BaseWorldMessage message) {
			CommMessage commMessage = (CommMessage)message;
			Logger.Log(2, "Received comm message with text '" + commMessage.Message + "'");
			if (verifyServer && 
				commMessage.Message.Substring(0, 28) == "--verify_server Comm Message") {
				// We need to exit with a code of 0, indicating success
				Logger.Log(4, "Called with --verify_server; got Comm message we sent, exiting with exit code 0");
				Trace.Flush();
				System.Environment.Exit(0);
			}
			if (commMessage.ChannelId != (int)CommChannel.Say)
				return;
			if (!this.PlayerInitialized || this.Player == null)
				return;
// 			bubbleManager.SetBubbleText(commMessage.Oid,
// 										commMessage.Message,
// 										System.Environment.TickCount);
		}

		public void HandleSound(BaseWorldMessage message) {
			SoundMessage soundMessage = (SoundMessage)message;
			TestObject node = GetObjectNode(soundMessage.Oid);
			if (node == null) {
				Trace.TraceWarning("Got sound message for invalid object: " + soundMessage.Oid);
				return;
			}
			Logger.Log(0, "Sound entries for " + soundMessage.Oid + (soundMessage.Clear ? " (cleared)" : ""));
		}
		public void HandleAmbientLight(BaseWorldMessage message) {
			AmbientLightMessage ambientLightMessage = (AmbientLightMessage)message;
			Trace.TraceWarning("Got ambient light message color " + ambientLightMessage.Color);
		}
		public void HandleNewLight(BaseWorldMessage message) {
			NewLightMessage newLightMessage = (NewLightMessage)message;
			Trace.TraceWarning("Got new light message with oid " + newLightMessage.ObjectId);
		}

		public void HandleTerrainConfig(BaseWorldMessage message) {
			TerrainConfigMessage terrainConfigMessage = (TerrainConfigMessage)message;
			Trace.TraceWarning("Got terrain config message");
			string s = terrainConfigMessage.ConfigString;
			terrainInitialized = true;
// 			if (terrainConfigMessage.ConfigKind == "file") {
// 				Stream fileStream = ResourceManager.FindCommonResourceData(s);
// 				StreamReader reader = new StreamReader(fileStream);
// 				s = reader.ReadToEnd();
// 				reader.Close();
// 			}
// 			lock (nodeDictionary) {
// 				foreach (TestObject node in nodeDictionary.Values) {
// 					if (node.FollowTerrain) {
// 						Vector3 loc = node.Position;
// 						loc.y = GetHeightAt(loc);
// 						node.Position = loc;
// 					}
// 				}
// 			}
		}

		public void HandleRegionConfig(BaseWorldMessage message) {
			RegionConfigMessage regionConfigMessage = (RegionConfigMessage)message;
			Trace.TraceWarning("Got region config message");
		}

		public void HandleSkyboxMaterial(BaseWorldMessage message) {
			SkyboxMaterialMessage skyboxMaterialMessage = (SkyboxMaterialMessage)message;
			Trace.TraceWarning("Got skybox material message");
		}
		public void HandleRoadInfo(BaseWorldMessage message) {
			RoadInfoMessage roadInfo = (RoadInfoMessage)message;
			Trace.TraceWarning("Got road info message");
		}
		public void HandleFogMessage(BaseWorldMessage message) {
			FogMessage fogMessage = (FogMessage)message;
			Trace.TraceWarning("Got fog message");
		}
        public void HandleAddParticleEffect(BaseWorldMessage message) {
            AddParticleEffectMessage particleMessage = (AddParticleEffectMessage)message;
			Trace.TraceWarning("Got add particle effect message for object " + particleMessage.Oid);
			TestObject parentObj = GetObjectNode(particleMessage.Oid);
            if (parentObj == null) {
                Trace.TraceWarning("Got particle effects message for nonexistent object: " + message.Oid);
                return;
            }
        }

        public void HandleRemoveParticleEffect(BaseWorldMessage message) {
            RemoveParticleEffectMessage particleMessage = (RemoveParticleEffectMessage)message;
			Trace.TraceWarning("Got remove particle effect message for object " + particleMessage.Oid);
            TestObject parentObj = GetObjectNode(particleMessage.Oid);
            if (parentObj == null) {
                Trace.TraceWarning("Got particle effects message for nonexistent object: " + message.Oid);
                return;
            }
        }

        public void HandleObjectProperty(BaseWorldMessage message) {
            ObjectPropertyMessage propMessage = (ObjectPropertyMessage)message;
			foreach (string key in propMessage.Properties.Keys) {
                object val = propMessage.Properties[key];
				Logger.Log(1, "HandleObjectProperty for OID {0}, setting prop {1} = {2}",
						   message.Oid, key, val);
			}
		}

		#endregion


		/// <summary>
		///   Update the time offset.
		/// </summary>
		/// <param name="timestamp">timestamp received from the server</param>
		public void AdjustTimestamp(long timestamp)
		{
			networkHelper.AdjustTimestamp(timestamp, System.Environment.TickCount);
		}
		
		/// <summary>
		///   We already hold the lock on the sceneManager and the nodeDictionary.
		///   This doesn't remove the node from the dictionary, but it does remove
		///   it from the scene, and cleans up.
		/// </summary>
		/// <param name="oid"></param>
		private void RemoveNode(TestObject node) {
// 			nameManager.RemoveNode(node.Oid);
// 			bubbleManager.RemoveNode(node.Oid);
			if (ObjectRemoved != null)
				ObjectRemoved(this, node);
		}

		// Removes a node from the nodeDictionary and also does whatever
		// cleanup needs to be done on that node.
		private void RemoveNode(long oid) {
				lock (nodeDictionary) {
					TestObject node = nodeDictionary[oid];
					RemoveNode(node);
					nodeDictionary.Remove(oid);
				}
		}

		public void ClearWorld() {
			// Remove all objects from the scene
			lock (nodeDictionary) {
				foreach (TestObject node in nodeDictionary.Values) {
					RemoveNode(node);
				}
				nodeDictionary.Clear();
			}

			SetPlayer(null);
			playerId = 0;
			playerInitialized = false;
			terrainInitialized = false;
			loadCompleted = false;
		}

		protected void SetupSubEntities(Entity entity, string[] submeshNames, string[] materialNames) {
			Logger.Log(2, "Submesh names for {0}: {1}", entity, ArrayToString(submeshNames));
			Logger.Log(2, "Material names for {0}: {1}", entity, ArrayToString(materialNames));
		}

		protected bool ApplyModel(TestObject node)
        {
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
				Logger.Log(2, "Application of model to node cancelled for " + node.Oid);
				return false;
			}
			return true;
		}

		public float GetHeightAt(Vector3 location) {
			return 0;
		}

		public Vector3 MoveMobNode(TestObject mobNode, Vector3 desiredDisplacement)
		{
			Vector3 pos = mobNode.position + desiredDisplacement;
			return pos;
		}
		
		// At some point, I would like to expose the scene node tree, so that
		// I can have objects move with other objects.
		private void AddObjectStub(long oid, string name, Vector3 location,
								   Quaternion orientation, Vector3 scale,
								   ObjectNodeType objType, bool followTerrain)
		{ 
			Logger.Log(2, "In AddObjectStub - oid: {0}; name: {1}; objType {2}; followTerrain: {3}",
					   oid, name, objType, followTerrain);

			TestObject stub = new TestObject();
			stub.Oid = oid;
			stub.name = name;
			stub.followTerrain = followTerrain;
			stub.objectType = objType;
			nodeDictionary[oid] = stub;
            if (oid == playerId) {
                playerStubInitialized = true;
				playerInitialized = true;
				player = stub;
			}
		}
		// At some point, I would like to expose the scene node tree, so that
		// I can have objects move with other objects.
		// Also at some point I should expose multiple mesh files
		private void AddObject(long oid, string meshFile) {
            Logger.Log(2, "In AddObject - oid: {0}; model: {1}", oid, meshFile);
			TestObject node;

            lock (nodeDictionary) {
                if (nodeDictionary.ContainsKey(oid)) {
                    Logger.Log(3, "Got duplicate AddObject for oid {0}", oid);
					if (!(nodeDictionary[oid] is TestObject)) {
						Logger.Log(3, "AddObject for exisisting non-object entity");
						return;
					}
                    node = nodeDictionary[oid];
					ApplyModel(node);
                    if (player == null && oid == playerId)
                        player = node;
                    return;
			    }
            }

			TestObject objStub = GetObjectNode(oid);
			switch (objStub.objectType) {
				case ObjectNodeType.User:
				case ObjectNodeType.Npc:
					node = objStub;
					if (player == null && oid == playerId)
						player = node;
					break;
				case ObjectNodeType.Item:
				case ObjectNodeType.Prop:
                    node = objStub;
					break;
				default:
					node = null;
					break;
			}
			node.FollowTerrain = objStub.followTerrain;
			bool status = ApplyModel(node);
			if (!status)
				return;

            // Pretend this message was created at time 0.  Since the 
            // direction vector is zero, this will not mess up the 
            // interpolation, but will allow any of the server's queued 
            // dirloc messages about this object to be processed
			node.SetDirLoc(0, Vector3.Zero, objStub.Position);
			node.SetOrientation(objStub.Orientation);

            if (node.Oid == playerId)
                player = node;

            lock (nodeDictionary) {
                nodeDictionary[oid] = node;
                if (ObjectAdded != null)
                    ObjectAdded(this, node);
            }
            
// 			if (objStub.objectType == ObjectNodeType.Npc || 
// 				objStub.objectType == ObjectNodeType.User) {
// 				nameManager.AddNode(oid, node);
// 				bubbleManager.AddNode(oid, node);
// 			}
		}

		private void RemoveObject(long oid) {
			Logger.Log(2, "Got RemoveObject for oid {0}", oid);
			Debug.Assert(oid != playerId);
			if (oid == playerId) {
				Trace.TraceError("Attempted to remove the player from the world");
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
                    Trace.TraceWarning("Got RemoveObject for invalid oid {0}", oid);
                    return;
                }
				RemoveNode(oid);
			}
            Logger.Log(2, "Removed object for oid {0}", oid);
        }

		protected TestObject GetObjectStub(long oid) {
			lock (nodeDictionary) {
				return nodeDictionary[oid];
			}
		}

		public TestObject GetObjectNode(long oid) {
			lock (nodeDictionary) {
				TestObject node;
                if (nodeDictionary.TryGetValue(oid, out node))
					return node;
                return null;
            }
        }

		public TestObject GetObjectNode(string name) {
            lock (nodeDictionary) {
				foreach (TestObject node in nodeDictionary.Values)
					if (node.Name == name)
						return node;
                return null;
            }
        }

		// The timer event
        public void SendPlayerData(object sender, System.Timers.ElapsedEventArgs e) {
            if (player == null || networkHelper == null)
                return;
			long now = WorldManager.CurrentTime;
			SendDirectionMessage(player, now);
            SendOrientationMessage(player, now);
        }

		protected void SendOrientationMessage(TestObject node, long now) {
			if (node.orientUpdate.dirty) {
				Quaternion orient = node.orientUpdate.orientation;
				node.orientUpdate.dirty = false;
				networkHelper.SendOrientationMessage(orient);
			} else {
				// normal periodic send
				Quaternion orient = node.orientUpdate.orientation;
				networkHelper.SendOrientationMessage(orient);
			}
		}

		protected void SendDirectionMessage(TestObject node, long now) {
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
		}

		protected static TimingMeter tickMeter = MeterManager.GetMeter("Tick Nodes", "WorldManager");
		protected static TimingMeter directionMeter = MeterManager.GetMeter("Direction Message", "WorldManager");
		protected static TimingMeter orientationMeter = MeterManager.GetMeter("Orientation Message", "WorldManager");

		/// <summary>
        ///   Called on each frame to update the nodes
        /// </summary>
        /// <param name="timeSinceLastFrame">time since the last frame in seconds</param>
		public void OnFrameStarted(long now) {
				if (player == null)
					return;
				tickMeter.Enter();
// 				lock (nodeDictionary) {
// 					foreach (TestObject node in nodeDictionary.Values)
// 						node.Tick(timeSinceLastFrame);
// 				}
// 				nameManager.Tick(timeSinceLastFrame, now);
// 				bubbleManager.Tick(timeSinceLastFrame, now);
				tickMeter.Exit();

				MessageDispatcher.Instance.HandleMessageQueue(behaviorParms.MaxMessagesPerFrame);
				
				if (player.dirUpdate.dirty &&
					(player.lastDirSent + behaviorParms.DirUpdateInterval < now)) {
					directionMeter.Enter();
					SendDirectionMessage(player, now);
					directionMeter.Exit();
				}
				if (player.orientUpdate.dirty &&
					(player.lastOrientSent + behaviorParms.OrientUpdateInterval < now)) {
					orientationMeter.Enter();
					SendOrientationMessage(player, now);
					orientationMeter.Exit();
				}

				// Also update animation states for objects that are not
				// on the server.
		}

		protected float playerYaw = 180;
		
		public void AsyncBehavior() 
		{
			// We start out active if we're ever active
			bool playerActive = behaviorParms.maxActiveTime > 0;
			DateTime changeActivityTime = DateTime.Now;
			// Stall until we have our player
			while (player == null)
				Thread.Sleep(1000);
			// Now life starts.  Every moveInterval milliseconds, we
			// move.  If there is anyone nearby to chat with, we do
			// so.
			Random moveRand = new Random();
			// Start out moving in a randon direction
			playerYaw = 360f * (float)moveRand.NextDouble();
			// Remember where we started
			Vector3 startingPosition = Player.Position;
			// A counter of steps since we last changed direction,
			// used to ensure that we don't continuously reverse
			int changeCounter = 100;
			// Every 30 seconds we send a Comm message
			int commCounter = 30 * 1000 / behaviorParms.moveInterval;
			int messageNumber = 2;
			// Loop forever
			while (true) {
				Thread.Sleep(behaviorParms.moveInterval);
				// Count down the comm counter; send a message every 30 seconds
				commCounter--;
				if (commCounter < 0) {
					string s = (verifyServer ? "--verify_server Comm Message" :
								"Comm Message #" + messageNumber.ToString());
					Logger.Log(1, "Sending Comm Message: " + s);
					networkHelper.SendCommMessage(s);
					commCounter = 30 * 1000 / behaviorParms.moveInterval;
					messageNumber = (messageNumber == 2 ? 1 : 2);
				}
				// Process any pending messages
				OnFrameStarted(CurrentTime);
				// See if we are reversing activity level
				DateTime n = DateTime.Now;
				if (changeActivityTime <= n) {
					float m = (float)moveRand.NextDouble();
					int nextPeriodMS = (int)(m * (playerActive ? behaviorParms.maxIdleTime : behaviorParms.maxActiveTime));
					if (nextPeriodMS > 0) {
						playerActive = !playerActive;
						changeActivityTime = n.AddMilliseconds(nextPeriodMS);
					}
					else {
						int currentPeriodMS = (int)(m * (playerActive ? behaviorParms.maxActiveTime : behaviorParms.maxIdleTime));
						changeActivityTime = n.AddMilliseconds(currentPeriodMS);
					}
				}
				if (playerActive) {
					// If the last move left us outside the moveMaximum
					// limit, reverse directions
					changeCounter--;
					if (changeCounter < 0 &&
						(Player.Position - startingPosition).Length > behaviorParms.moveMaximum * Client.OneMeter) {
						Logger.Log(1, "Reversing direction because the character is more than {0} meters from the starting position",
								   behaviorParms.moveMaximum);
						PlayerYaw += 180;
						changeCounter = 100;
						continue;
					}
					float r = (float)moveRand.NextDouble();
					float zMult = 0f;
					float xMult = 0f;
					float rotateMult = 0f;
					if (r < behaviorParms.forwardFraction)
						zMult = 1f;
					else {
						r -= behaviorParms.forwardFraction;
						if (r < behaviorParms.backFraction)
							zMult = -1f;
						else {
							r -= behaviorParms.backFraction;
							if (r < behaviorParms.rotateFraction)
								rotateMult = (r >= behaviorParms.rotateFraction * .5f ? 1f : -1f);
							else {
								r -= behaviorParms.rotateFraction;
								xMult = (r >= behaviorParms.sideFraction * .5f ? 1f : -1f);
							}
						}
					}
					MovePlayer(xMult, zMult, rotateMult);
					// Now that we're moved, is there anyone nearby to chat with?
				}
				else if (playerDir != Vector3.Zero) {
					playerDir = Vector3.Zero;
					Player.SetDirection(playerDir, Player.Position);
				}
			}
		}
		
        protected Vector3 playerDir = Vector3.Zero;
		protected Vector3 playerAccel = Vector3.Zero;

		protected void MovePlayer(float xMult, float zMult, float rotateMult) {
			// Now handle movement and stuff

			// reset acceleration zero
			playerAccel = Vector3.Zero;

			playerAccel.x += xMult * 0.5f;
			playerAccel.z += zMult * 1.0f;
			PlayerYaw += rotateMult * behaviorParms.rotateSpeed * behaviorParms.moveInterval / 1000f;
			
			Quaternion playerOrientation =
				Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(PlayerYaw), Vector3.UnitY);
			Player.Orientation = playerOrientation;
			Matrix3 yawMatrix = Player.Orientation.ToRotationMatrix();
			playerAccel.Normalize();
			playerDir = (yawMatrix * behaviorParms.playerSpeed * playerAccel);
			Vector3 playerPos = Player.Position + (playerDir.ToNormalized() * behaviorParms.playerSpeed * behaviorParms.moveInterval / 1000f);
			Logger.Log(1, string.Format("Moving player in direction {0}, new position {1}", 
										playerDir, playerPos));
			Player.SetDirection(playerDir, playerPos);
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
			TestObject node = GetObjectNode(oid);
            if (node != null)
                node.SetDirLoc(timestamp, dir, loc);
            else
                Trace.TraceWarning("No node match for: " + oid);
        }

        private void SetOrientation(long oid, Quaternion orient) {
            TestObject node = GetObjectNode(oid);

			if (node != null && node == player)
				Trace.TraceWarning("Ignoring orientation for player.");
			else if (node != null)
				node.SetOrientation(orient);
			else
				Trace.TraceWarning("No node match for: " + oid);

			// DEBUG
			if (node != null && node == player)
				Trace.TraceWarning("Server set player orientation to " +
								   orient + " instead of " + player.Orientation);
		}

        private void SetPlayer(TestObject node) {
            player = node;
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

        public TestObject Player {
            get {
                Monitor.Enter(this);
                TestObject rv = player;
                Monitor.Exit(this);
                return rv;
            }
        }

		public static long CurrentTime {
			get {
				long timestamp = System.Environment.TickCount;
				while (timestamp < 0) {
					timestamp += int.MaxValue;
					timestamp -= int.MinValue;
				}
				return timestamp;
			}
		}

		public float PlayerYaw {
			get {
				return playerYaw;
			}
			set {
				while (value < 0)
					value += 360;
				playerYaw = value % 360;
			}
		}
	
		public bool VerifyServer 
		{
			get {
				return verifyServer;
			}
		}
		
	}

}
