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

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;

using Axiom.MathLib;
using Axiom.Core;
using Axiom.Input;

using log4net;

using Multiverse.Gui;
using Multiverse.Interface;
using Multiverse.Network;
using Multiverse.Base;
using Multiverse.Config;
using Multiverse.CollisionLib;

using Multiverse.Serialization;


namespace Multiverse.Base {
    /// <summary>
    ///   The MarsWorld class represents some basic logic for the Multiverse 
    ///   engine that should be applicable to most worlds.  The implementation 
    ///   of the collision system is established in this library.
    /// 
    ///   This class still has some methods that are not truly appropriate,
    ///   but these will be culled in later releases.  Specifically, the
    ///   object properties are hardcoded in this class.
    /// </summary>
	public class MarsWorld : IGameWorld {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MarsWorld));

        static MarsWorld instance;

        protected Client client;
		protected WorldManager worldManager;

        /// <summary>
        ///   Simple constructor that takes a reference to the game client, 
        ///   and sets up the basic game state.
        ///   In this case, that state contains information about inventory,
        ///   quests, group membership, and the colors to use for chat.
        /// </summary>
        /// <param name="client">the client class</param>
		public MarsWorld(Client client) {
			this.client = client;

            // Set up the global instance
			instance = this;
        }

        /// <summary>
        ///   Sets up the default cursor.
        /// </summary>
        public void Initialize() {
            // Set up the default cursor
            UiSystem.SetCursor(3, "Interface\\Cursor\\Point");
        }

        /// <summary>
        ///   Clean up our game specific information
        /// </summary>
		public void Dispose() {
            this.WorldManager = null;
		}

        #region Collision Methods
        /// <summary>
        ///   Add the collision data for an object.  This involves looking 
        ///   for a physics file that matches the mesh file's name, and 
        ///   loading the information from that file to build collision 
        ///   volumes.
        /// </summary>
        /// <param name="objNode">the object for which we are adding the collision data</param>
        public void AddCollisionObject(ObjectNode objNode) {
            if (worldManager.CollisionHelper == null)
                return;
            if (!objNode.UseCollisionObject)
                return;
            // Create a set of collision shapes for the object
            List<CollisionShape> shapes = new List<CollisionShape>();
            string meshName = objNode.Entity.Mesh.Name;
            PhysicsData pd = new PhysicsData();
            PhysicsSerializer ps = new PhysicsSerializer();
            bool static_object = true;
            if ((objNode.ObjectType == ObjectNodeType.Npc) ||
                (objNode.ObjectType == ObjectNodeType.User)) {
                static_object = false;
            }

            if (meshName.EndsWith(".mesh")) {
                string physicsName = meshName.Substring(0, meshName.Length - 5) + ".physics";
                try {
                    Stream stream = ResourceManager.FindCommonResourceData(physicsName);
                    ps.ImportPhysics(pd, stream);
                    foreach (SubEntity subEntity in objNode.Entity.SubEntities) {
                        if (subEntity.IsVisible) {
                            string submeshName = subEntity.SubMesh.Name;
                            List<CollisionShape> subEntityShapes = pd.GetCollisionShapes(submeshName);
                            foreach (CollisionShape subShape in subEntityShapes) {
                                // static objects will be transformed here, but movable objects
                                // are transformed on the fly
                                if (static_object)
                                    subShape.Transform(objNode.SceneNode.DerivedScale,
                                                       objNode.SceneNode.DerivedOrientation,
                                                       objNode.SceneNode.DerivedPosition);
                                shapes.Add(subShape);
                                log.DebugFormat("Added collision shape for oid {0}, subShape {1}, subMesh {2}",
                                                objNode.Oid, subShape, submeshName);
                            }
                        }
                    }
                    // Now populate the region volumes
                    foreach (KeyValuePair<string, List<CollisionShape>> entry in pd.CollisionShapes) {
                        string regionName = RegionVolumes.ExtractRegionName(entry.Key);
                        if (regionName != "") {
                            // We must record this region - - must be
                            // a static object
                            Debug.Assert(static_object);
                            List<CollisionShape> subShapes = new List<CollisionShape>();
                            foreach (CollisionShape subShape in entry.Value) {
                                subShape.Transform(objNode.SceneNode.DerivedScale,
                                                   objNode.SceneNode.DerivedOrientation,
                                                   objNode.SceneNode.DerivedPosition);
                                subShapes.Add(subShape);
                            }
                            RegionVolumes.Instance.AddRegionShapes(objNode.Oid, regionName, subShapes);
                        }
                    }
                } catch (Exception) {
                    // Unable to load physics data -- use a sphere or no collision data?
                    log.InfoFormat("Unable to load physics data: {0}", physicsName);
                    //// For now, I'm going to put in spheres.  Later I should do something real.
                    //CollisionShape shape = new CollisionSphere(Vector3.Zero, Client.OneMeter);
                    //if (static_object)
                    //    // static objects will be transformed here, but movable objects
                    //    // are transformed on the fly
                    //    shape.Transform(objNode.SceneNode.DerivedScale,
                    //                    objNode.SceneNode.DerivedOrientation,
                    //                    objNode.SceneNode.DerivedPosition);
                    //shapes.Add(shape);
                }
            }


            if (static_object) {
                foreach (CollisionShape shape in shapes)
                    worldManager.CollisionHelper.AddCollisionShape(shape, objNode.Oid);
                objNode.CollisionShapes = shapes;
            } else {
                MovingObject mo = new MovingObject(worldManager.CollisionHelper);
                foreach (CollisionShape shape in shapes)
                    worldManager.CollisionHelper.AddPartToMovingObject(mo, shape);
                objNode.Collider = mo;
                objNode.Collider.Transform(objNode.SceneNode.DerivedScale,
                                           objNode.SceneNode.DerivedOrientation,
                                           objNode.SceneNode.DerivedPosition);
            }
        }

        /// <summary>
        ///   Remove the associated collision data from an object.
        /// </summary>
        /// <param name="objNode">the object for which we are removing the collision data</param>
        public void RemoveCollisionObject(ObjectNode objNode) {
            if (worldManager.CollisionHelper == null)
                return;
            bool static_object = true;
            if ((objNode.ObjectType == ObjectNodeType.Npc) ||
                (objNode.ObjectType == ObjectNodeType.User)) {
                static_object = false;
            }
            if (static_object) {
                worldManager.CollisionHelper.RemoveCollisionShapesWithHandle(objNode.Oid);
                RegionVolumes.Instance.RemoveCollisionShapesWithHandle(objNode.Oid);
            } else {
                if (objNode.Collider != null)
                    objNode.Collider.Dispose();
                objNode.Collider = null;
            }
        }
        #endregion

        /// <summary>
        ///   This method is called when we add an object from the world.
        ///   This method will register the mouse events, set up the
        ///   name and bubble widgets, and add the collision volumes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objNode"></param>
		private void OnObjectAdded(object sender, ObjectNode objNode) {
			if (objNode == null)
				return;
            AddCollisionObject(objNode);
            if (objNode is Player) {
                PlayerEnteringWorld();
                return;
            }
		}

        /// <summary>
        ///   This method is called when we remove an object from the world.
        ///   This method will unregister the mouse events, clear the
        ///   name and bubble widgets, and remove the collision volumes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objNode"></param>
		private void OnObjectRemoved(object sender, ObjectNode objNode) {
			if (objNode == null)
				return;
            RemoveCollisionObject(objNode);
        }

		#region Message handlers
        /// <summary>
        ///   Special handling for DirectionMessage objects.
        ///   This updates the collision volumes if needed.
        /// </summary>
        /// <param name="message">the message object from the server</param>
        public void HandleDirection(BaseWorldMessage message) {
            DirectionMessage dirMessage = message as DirectionMessage;
            ObjectNode objNode = worldManager.GetObjectNode(dirMessage.Oid);
            if (objNode != null) {
                bool static_object = true;
                if ((objNode.ObjectType == ObjectNodeType.Npc) ||
                    (objNode.ObjectType == ObjectNodeType.User))
                {
                    static_object = false;
                }
                if (static_object) {
                    RemoveCollisionObject(objNode);
                    AddCollisionObject(objNode);
                } else {
                    if (objNode.Collider != null)
                        objNode.Collider.Transform(objNode.SceneNode.DerivedScale,
                                                   objNode.SceneNode.DerivedOrientation,
                                                   objNode.SceneNode.DerivedPosition);
                }
            }
        }

        public void HandleObjectProperty(BaseWorldMessage message) {
            ObjectPropertyMessage propMessage = (ObjectPropertyMessage)message;
            // Debugging
			foreach (string key in propMessage.Properties.Keys) {
                object val = propMessage.Properties[key];
				log.DebugFormat("HandleObjectProperty for OID {0}, setting prop {1} = {2}",
				    		   message.Oid, key, val);
            }
            HandleObjectPropertyHelper(message.Oid, propMessage.Properties);
        }
        private void HandleObjectPropertyHelper(long oid, Dictionary<string, object> props)
        {
            if (props.Count <= 0)
                return;
            ObjectNode node = worldManager.GetObjectNode(oid);
            if (node == null)
            {
                log.WarnFormat("Got stat update message for nonexistent object: {0}", oid);
                return;
            }
            node.UpdateProperties(props);
        }
        #endregion

        /// <summary>
        ///   Register our message handlers with the message callback system
        ///   so that we can have custom logic associated with incoming 
        ///   messages.
        /// </summary>
		public void SetupMessageHandlers() {
            // Register for notification about object movement, so we can update the collision data
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Direction,
                                                       new WorldMessageHandler(this.HandleDirection));
            
            // Replacement for the StatUpdate and StateMessage handlers
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.ObjectProperty,
                                                       new WorldMessageHandler(this.HandleObjectProperty));
        }

        /// <summary>
        ///   This method is called when the player enters the world.
        ///   Generally this only happens when the player logs in.
        ///   This injects 'PLAYER_ENTERING_WORLD' and 'UNIT_NAME_UPDATE' 
        ///   UI events so that any interested widgets can update their data.
        /// </summary>
        public void PlayerEnteringWorld() {
            GenericEventArgs eventArgs;
            eventArgs = new GenericEventArgs();
            eventArgs.eventType = "PLAYER_ENTERING_WORLD";
            UiSystem.DispatchEvent(eventArgs);
            
            eventArgs = new GenericEventArgs();
            eventArgs.eventType = "UNIT_NAME_UPDATE";
            eventArgs.eventArgs = new string[1];
            eventArgs.eventArgs[0] = "player";
            UiSystem.DispatchEvent(eventArgs);
        }

        /// <summary>
        ///   Writes a message using the specified channel.
        /// </summary>
        /// <param name="message">the message to write</param>
		public void Write(string message) {
            log.Debug(message);
            // New system that dispatches events using the ui event system
            GenericEventArgs eventArgs = new GenericEventArgs();
            eventArgs.eventType = "CHAT_MSG_SYSTEM";
            eventArgs.eventArgs = new string[2];
            eventArgs.eventArgs[0] = message;
            eventArgs.eventArgs[1] = string.Empty;
            UiSystem.DispatchEvent(eventArgs);
		}


#if NOT
        /// <summary>
        ///   Helper method to target mobs.  This is used to handle targetting.
        ///   Typically F8 will invoke this to target nearest mob (last = null)
        ///   Typically Tab will invoke this to target closest mob (or last)
        /// </summary>
        /// <param name="last">last mob targetted</param>
        /// <param name="reverse">whether we are spiraling in instead of out</param>
        /// <param name="onlyAttackable">
        ///   whether we should exclude objects that are not attackable
        /// </param>
		public void TargetMobHelper(ObjectNode last, bool reverse, bool onlyAttackable) {
			// Get a copy of the list of mobs
			List<MobNode> mobNodes = new List<MobNode>(worldManager.GetMobNodes());
			mobNodes.Remove(client.Player);
			IComparer<MobNode> comparer = new DistanceComparer(client.Player);
			mobNodes.Sort(comparer);
			if (reverse)
				mobNodes.Reverse();
			// Ideally, I would set lastMob to be the last mob within 
			// targetting range, but for now, I'll just consider all.
			bool lastFound = (last == null) ? true : false;
			ObjectNode target = null;
			int lastIndex = -1;
			if (last != null) {
				for (int i = 0; i < mobNodes.Count; ++i)
					if (mobNodes[i] == last) {
						lastIndex = i;
						break;
					}
			}
			// Now, starting at lastIndex + 1, loop around the list
			for (int i = 1; i < mobNodes.Count; ++i) {
				int index = (i + lastIndex) % mobNodes.Count;
				if (!onlyAttackable || IsAttackable(mobNodes[index])) {
					target = mobNodes[index];
					break;
				}
			}
			// If we found the 'last' object, and found another object 
			if (target != null)
				client.Target = target;
		}
#endif

        /// <summary>
        ///   Get or set the WorldManager object that keeps track of objects in the world.
        /// </summary>
		public WorldManager WorldManager {
			get {
				return worldManager;
			}
			set {
				if (worldManager != null) {
					worldManager.ObjectAdded -= this.OnObjectAdded;
					worldManager.ObjectRemoved -= this.OnObjectRemoved;
				}
				worldManager = value;
				if (worldManager != null) {
					worldManager.ObjectAdded += this.OnObjectAdded;
					worldManager.ObjectRemoved += this.OnObjectRemoved;
				}
			}
		}

        /// <summary>
        ///   Get the singleton
        /// </summary>
        public static MarsWorld Instance {
            get {
                return instance;
            }
        }

	}
}
