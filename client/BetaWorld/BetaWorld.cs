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
using Multiverse.BetaWorld.Gui;


namespace Multiverse.BetaWorld {
    /// <summary>
    ///   The BetaWorld class represents the game specific logic of the Multiverse engine.
    ///   Right now, this class is heavily influenced by the World of Warcraft API, since
    ///   we have used the requirements of that game as a sample use case of the plugin
    ///   architecture.
    /// </summary>
	public class BetaWorld : IGameWorld {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BetaWorld));

		protected Client client;
		protected WorldManager worldManager;

		protected Dictionary<int, ColorEx> chatColors = new Dictionary<int, ColorEx>();
        protected Dictionary<string, string> propertyMap = new Dictionary<string, string>();

        protected string questAvailableMesh = "bang.mesh";
        protected string questConcludableMesh = "bang.mesh";

		public class Container : Dictionary<int, InvItemInfo> {
		}

		public class QuestLogEntry {
			long questId;
			string title;
			string description;
			string objective;
			List<string> subObjectives;
			List<ItemEntry> rewardItems;

			public QuestLogEntry() {
				subObjectives = new List<string>();
				rewardItems = new List<ItemEntry>();
			}
			#region Properties
			public long QuestId {
				get {
					return questId;
				}
				set {
					questId = value;
				}
			}
			public string Title {
				get {
					return title;
				}
				set {
					title = value;
				}
			}
			public string Description {
				get {
					return description;
				}
				set {
					description = value;
				}
			}
			public string Objective {
				get {
					return objective;
				}
				set {
					objective = value;
				}
			}
			public List<string> Objectives {
				get {
					return subObjectives;
				}
				set {
					subObjectives = value;
				}
			}
			public List<ItemEntry> RewardItems {
				get {
					return rewardItems;
				}
				set {
					rewardItems = value;
				}
			}
			#endregion
		}

        public class ActionEntry {
            public InvItemInfo item;
            public AbilityEntry ability;
        }

		public class GroupEntry {
			public long memberId;
			public string memberName;
		}

		// Info about the last quest we were offered
		long lastQuestId;
		long lastObjectId;
		string lastQuestTitleText;
		string lastQuestObjectiveText;
		string lastQuestDescriptionText;
		List<ItemEntry> rewardItems;

		// Info about last set of targets
		ObjectNode lastEnemy;
		ObjectNode lastTarget;
		ObjectNode currentEnemy;
		ObjectNode currentTarget;

		// Info about inventory
		Dictionary<int, Container> inventory;

		// Info for quest log
		int questLogSelectedIndex = 0;
		List<QuestLogEntry> questLogEntries;

		// Info about groups
		long groupLeaderId = -1;
        Dictionary<long, List<GroupEntry>> groups;

        // Info about abilities
        List<AbilityEntry> abilities;

        // Info about action buttons
        Dictionary<int, ActionEntry> actions = new Dictionary<int, ActionEntry>();

		// Reference to the edit frame
		Multiverse.Interface.EditBox editFrame;

        // Selected item
        InvItemInfo cursorItem = null;
        // Selected ability
        AbilityEntry cursorAbility = null;
        // add cursorMoney

        // These handle GUI elements for objects in the world
        NameManager nameManager = null;
        BubbleTextManager bubbleManager = null;

		static BetaWorld instance;

        /// <summary>
        ///   Simple constructor that takes a reference to the game client, 
        ///   and sets up the basic game state.
        ///   In this case, that state contains information about inventory,
        ///   quests, group membership, and the colors to use for chat.
        /// </summary>
        /// <param name="client">the client class</param>
		public BetaWorld(Client client) {
			this.client = client;
            client.UiSystem.Unload += this.OnUiUnload;

			chatColors[1] = new ColorEx(1.0f, 1.0f, 1.0f);
			chatColors[2] = new ColorEx(0.3f, 0.9f, 0.3f);
			chatColors[3] = new ColorEx(0.3f, 0.3f, 0.9f);
			chatColors[4] = new ColorEx(0.9f, 0.3f, 0.3f);
			chatColors[5] = new ColorEx(0.8f, 0.8f, 0.4f);

			inventory = new Dictionary<int, Container>();
			questLogEntries = new List<QuestLogEntry>();
			// groupMemberInfo = new List<GroupEntry>();
            groups = new Dictionary<long, List<GroupEntry>>();

			// Set up the global instance
			instance = this;

        }

        /// <summary>
        ///   Sets up the default cursor, as well as the system for handling 
        ///   display of object names and bubble chat.
        /// </summary>
        public void Initialize() {
            // Set up the default cursor
            UiSystem.SetCursor(3, "Interface\\Cursor\\Point");

            // Set up the name manager
            // nameManager = new NameManager(client.RootWindow, client, client.WorldManager);

            // Set up the bubble chat manager
            // bubbleManager = new BubbleTextManager(client.RootWindow, client, client.WorldManager);

            // We don't want to set gui visibility until we have finished with
            // the load screen
            // SetGuiVisibility(true);
        }

        /// <summary>
        ///   Clean up our game specific information
        /// </summary>
		public void Dispose() {
            ClearWorld();
            this.WorldManager = null;
		}

        /// <summary>
        ///   This should also be called whenever we want to clear the world
        ///   right now, nobody calls this (we call from dispose).  This 
        ///   should also be called from the portal code.
        /// </summary>
        protected void ClearWorld() {
            // Clear out names and bubbles
            if (nameManager != null)
                nameManager.ClearNodes();
            if (bubbleManager != null)
                bubbleManager.ClearNodes();
        }

        /// <summary>
        ///   Enable or disable the display of name widgets and bubble chat 
        ///   widgets.  This should also set the visibility of any other game
        ///   specific UI elements that are not part of the general UI system.
        /// </summary>
        /// <param name="visible">whether or not the ui should be visible</param>
        public void SetGuiVisibility(bool visible) {
            if (nameManager != null)
                nameManager.Visible = visible;
            if (bubbleManager != null)
                bubbleManager.Visible = visible;
        }

        /// <summary>
        ///   Expose the instance of the betaworld plugin.
        /// </summary>
		public static BetaWorld Instance {
			get {
				return instance;
			}
		}

        /// <summary>
        ///   This method is called at the start of each frame.  It updates
        ///   the position and state of name bars and bubble chat windows.
        /// </summary>
        /// <param name="timesincelastframe">number of ticks since the last frame</param>
        /// <param name="now">the current tick count</param>
        public void OnFrameStarted(long timesincelastframe, long now) {
#if NO_ASYNC_LOAD
            worldManager.LoadPreloadEntry();
#endif
            if (nameManager != null)
                nameManager.Tick(timesincelastframe, now);
            if (bubbleManager != null)
                bubbleManager.Tick(timesincelastframe, now);
        }

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
            if (objNode.ObjectType == ObjectNodeType.Npc ||
                objNode.ObjectType == ObjectNodeType.User) {
                if (nameManager != null) {
                    nameManager.RemoveNode(objNode.Oid);
                    nameManager.AddNode(objNode.Oid, objNode);
                }
                if (bubbleManager != null) {
                    bubbleManager.RemoveNode(objNode.Oid);
                    bubbleManager.AddNode(objNode.Oid, objNode);
                }
            }
            if (objNode is Player) {
                PlayerEnteringWorld();
                return;
            }
			objNode.MouseClicked += new Axiom.Input.MouseEventHandler(this.OnMouseClicked);
			objNode.MouseEnter += new Axiom.Input.MouseEventHandler(this.OnMouseEnter);
			objNode.MouseExit += new Axiom.Input.MouseEventHandler(this.OnMouseExit);
			log.InfoFormat("Setup mouse callbacks for {0}", objNode.Name);
		}

        /// <summary>
        ///   Add the collision data for an object.  This involves looking 
        ///   for a physics file that matches the mesh file's name, and 
        ///   loading the information from that file to build collision 
        ///   volumes.
        /// </summary>
        /// <param name="objNode">the object for which we are adding the collision data</param>
        private void AddCollisionObject(ObjectNode objNode) {
            if (worldManager.CollisionHelper == null)
                return;
            // Create a set of collision shapes for the object
            List<CollisionShape> shapes = new List<CollisionShape>();
            string meshName = objNode.Entity.Mesh.Name;
            PhysicsData pd = new PhysicsData();
            PhysicsSerializer ps = new PhysicsSerializer();
            bool static_object = true;
            if ((objNode.ObjectType == ObjectNodeType.Npc) ||
                (objNode.ObjectType == ObjectNodeType.User))
            {
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
        private void RemoveCollisionObject(ObjectNode objNode) {
            if (worldManager.CollisionHelper == null)
                return;
            bool static_object = true;
            if ((objNode.ObjectType == ObjectNodeType.Npc) ||
                (objNode.ObjectType == ObjectNodeType.User))
            {
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
            if (objNode is Player)
                return;
			objNode.MouseClicked -= new Axiom.Input.MouseEventHandler(this.OnMouseClicked);
			objNode.MouseEnter -= new Axiom.Input.MouseEventHandler(this.OnMouseEnter);
			objNode.MouseExit -= new Axiom.Input.MouseEventHandler(this.OnMouseExit);
			log.InfoFormat("Removed mouse callbacks for {0}", objNode.Name);
            // Remove the name and bubble nodes
            if (nameManager != null)
                nameManager.RemoveNode(objNode.Oid);
            if (bubbleManager != null)
                bubbleManager.RemoveNode(objNode.Oid);
		}

		#region Message handlers
		public void HandleComm(BaseWorldMessage message) {
			CommMessage commMessage = (CommMessage)message;
			ObjectNode node = worldManager.GetObjectNode(commMessage.Oid);

            // First use the old path.  This will work with interfaces that use
            // the MvChatFrameOutputScrollingMessageFrame0.
			if (node != null && commMessage.ChannelId == (int)CommChannel.Say)
				Write("[" + node.Name + "]: " + commMessage.Message, commMessage.ChannelId);
			else
				Write(commMessage.Message, commMessage.ChannelId);
            // Now the new path.  This will work with any widget that uses the event
            // registration system.  In the future, more cases should be handled here.
            // We could do the CHAT_MSG_GUILD, CHAT_MSG_PARTY, etc..
            // A better mechanism for this is to actually use server logic
            // instead to send these events.
            if (commMessage.ChannelId == (int)CommChannel.Say) {
                GenericEventArgs eventArgs = new GenericEventArgs();
                eventArgs.eventType = "CHAT_MSG_SAY";
                eventArgs.eventArgs = new string[3];
                eventArgs.eventArgs[0] = commMessage.Message;
                eventArgs.eventArgs[1] = node.Name;
                eventArgs.eventArgs[2] = string.Empty; // language
                UiSystem.DispatchEvent(eventArgs);
            } else if (commMessage.ChannelId == (int)CommChannel.ServerInfo) {
                GenericEventArgs eventArgs = new GenericEventArgs();
                eventArgs.eventType = "CHAT_MSG_SYSTEM";
                eventArgs.eventArgs = new string[2];
                eventArgs.eventArgs[0] = commMessage.Message;
                eventArgs.eventArgs[1] = string.Empty;
                UiSystem.DispatchEvent(eventArgs);
            } else if (commMessage.ChannelId == (int)CommChannel.CombatInfo) {
                GenericEventArgs eventArgs = new GenericEventArgs();
                eventArgs.eventType = "CHAT_MSG_COMBAT_MISC_INFO";
                eventArgs.eventArgs = new string[1];
                eventArgs.eventArgs[0] = commMessage.Message;
                UiSystem.DispatchEvent(eventArgs);
            } else {
                GenericEventArgs eventArgs = new GenericEventArgs();
                string channelName = "unknown";
                eventArgs.eventType = "CHAT_MSG_CHANNEL";
                eventArgs.eventArgs = new string[9];
                eventArgs.eventArgs[0] = commMessage.Message;
                eventArgs.eventArgs[1] = node.Name;
                eventArgs.eventArgs[2] = string.Empty; // language
                eventArgs.eventArgs[3] = string.Format("{0}. {1}", commMessage.ChannelId, channelName);
                eventArgs.eventArgs[4] = string.Empty; // ??
                eventArgs.eventArgs[5] = string.Empty; // ??
                eventArgs.eventArgs[6] = commMessage.ChannelId.ToString();
                eventArgs.eventArgs[7] = commMessage.ChannelId.ToString();
                eventArgs.eventArgs[8] = channelName;
                UiSystem.DispatchEvent(eventArgs);
            }
            if (bubbleManager != null)
                bubbleManager.SetBubbleText(commMessage.Oid,
                                            commMessage.Message,
                                            System.Environment.TickCount);
		}
		public void HandleAcquireResponse(BaseWorldMessage message) {
			AcquireResponseMessage acquireMessage = (AcquireResponseMessage)message;
			if (acquireMessage.Status)
                log.InfoFormat("Acquired object: {0}", acquireMessage.ObjectId);
			else
                log.InfoFormat("Failed to acquire object: {0}", acquireMessage.ObjectId);
		}
		public void HandleEquipResponse(BaseWorldMessage message) {
			EquipResponseMessage equipMessage = (EquipResponseMessage)message;
			if (equipMessage.Status)
                log.InfoFormat("Equipped object {0} to slot {1}", equipMessage.ObjectId, equipMessage.SlotName);
			else
                log.InfoFormat("Failed to equip object {0} to slot {1}", equipMessage.ObjectId, equipMessage.SlotName);
		}
		public void HandleUnequipResponse(BaseWorldMessage message) {
			UnequipResponseMessage unequipMessage = (UnequipResponseMessage)message;
			if (unequipMessage.Status)
                log.InfoFormat("Unequipped object {0} from slot {1}", unequipMessage.ObjectId, unequipMessage.SlotName);
			else
                log.InfoFormat("Failed to unequip object {0} from slot {1}", unequipMessage.ObjectId, unequipMessage.SlotName);
		}
		public void HandleAttach(BaseWorldMessage message) {
			AttachMessage attachMessage = (AttachMessage)message;
            log.InfoFormat("Attached object {0} to socket: {1}", attachMessage.ObjectId, attachMessage.SlotName);
		}
		public void HandleDetach(BaseWorldMessage message) {
			DetachMessage detachMessage = (DetachMessage)message;
            log.InfoFormat("Detached object from socket: {0}", detachMessage.SlotName);
		}
		public void HandleDamage(BaseWorldMessage message) {
			DamageMessage damageMessage = (DamageMessage)message;
			ObjectNode damager = worldManager.GetObjectNode(damageMessage.Oid);
			ObjectNode damagee = worldManager.GetObjectNode(damageMessage.ObjectId);
            string damagerName = "Unknown entity";
            string damageeName = "Unknown entity";
            string action = "strikes";
            if (damager != null)
                damagerName = (damager.Oid == client.Player.Oid) ? "You" : damager.Name;
            if (damagee != null)
                damageeName = (damagee.Oid == client.Player.Oid) ? "you" : damagee.Name;
            if (damager != null)
                action = (damager.Oid == client.Player.Oid) ? "strike" : "strikes";
            string msg =
				string.Format("{0} {1} {2} for {3} points of {4} damage.",
							  damagerName, action, damageeName,
							  damageMessage.DamageAmount, damageMessage.DamageType);
            if (damagee == client.Player) {
                // Old style
                Write(msg, 4);
                // New style
                GenericEventArgs eventArgs = new GenericEventArgs();
                eventArgs.eventType = "CHAT_MSG_COMBAT_CREATURE_VS_SELF_HITS";
                eventArgs.eventArgs = new string[1];
                eventArgs.eventArgs[0] = msg;
                UiSystem.DispatchEvent(eventArgs);
            } else {
                // Old style
                Write(msg, 5);
                // New style
                GenericEventArgs eventArgs = new GenericEventArgs();
                eventArgs.eventType = "CHAT_MSG_COMBAT_CREATURE_VS_CREATURE_HITS";
                eventArgs.eventArgs = new string[1];
                eventArgs.eventArgs[0] = msg;
                UiSystem.DispatchEvent(eventArgs);
            }
		}
        public void HandleStatUpdate(BaseWorldMessage message) {
            log.Error("Got deprecated StatUpdate message");
        }

		public void HandleState(BaseWorldMessage message) {
            log.Error("Got deprecated State message");
		}

        public void HandleAbilityUpdate(BaseWorldMessage message) {
            AbilityUpdateMessage abilityMessage = (AbilityUpdateMessage)message;

            abilities = abilityMessage.Entries;

            log.InfoFormat("Got AbilityUpdateMessage with {0} entries", abilityMessage.Entries.Count); 
            
            GenericEventArgs eventArgs = new GenericEventArgs();
            eventArgs.eventType = "ABILITY_UPDATE";
            UiSystem.DispatchEvent(eventArgs);
        }

		public void HandleQuestInfoResponse(BaseWorldMessage message) {
			QuestInfoResponseMessage respMessage = (QuestInfoResponseMessage)message;

			lastQuestTitleText = respMessage.Title;
			lastQuestObjectiveText = respMessage.Objective;
			lastQuestDescriptionText = respMessage.Description;
			lastObjectId = respMessage.ObjectId;
			lastQuestId = respMessage.QuestId;
			rewardItems = new List<ItemEntry>(respMessage.RewardItems);

			GenericEventArgs eventArgs = new GenericEventArgs();
			eventArgs.eventType = "QUEST_DETAIL";
			UiSystem.DispatchEvent(eventArgs);
		}

		public void HandleInventoryUpdate(BaseWorldMessage message) {
			foreach (Container container in inventory.Values)
				container.Clear();
			InventoryUpdateMessage invUpdate = (InventoryUpdateMessage)message;
            log.InfoFormat("Got InventoryUpdateMessage with {0} entries", invUpdate.Inventory.Count);
			foreach (InventoryUpdateEntry entry in invUpdate.Inventory) {
                log.DebugFormat("InventoryUpdateEntry fields: {0}, {1}, {2}, {3}", 
					entry.itemId, entry.containerId, entry.slotId, entry.name);
				if (!inventory.ContainsKey(entry.containerId))
					inventory[entry.containerId] = new Container();
				InvItemInfo invInfo = new InvItemInfo();
				invInfo.icon = entry.icon;
				invInfo.count = entry.count;
				invInfo.itemId = entry.itemId;
                invInfo.name = entry.name;
                inventory[entry.containerId][entry.slotId] = invInfo;
			}
			GenericEventArgs eventArgs = new GenericEventArgs();
			eventArgs.eventType = "UNIT_INVENTORY_UPDATE";
			UiSystem.DispatchEvent(eventArgs);
		}
		public void HandleQuestLogInfo(BaseWorldMessage message) {
			QuestLogInfoMessage questLogInfo = (QuestLogInfoMessage)message;
			QuestLogEntry logEntry = null;
			foreach (QuestLogEntry entry in questLogEntries) {
				if (entry.QuestId == questLogInfo.QuestId) {
					logEntry = entry;
					break;
				}
			}
			if (logEntry == null) {
				logEntry = new QuestLogEntry();
				questLogEntries.Add(logEntry);
			}
			logEntry.QuestId = questLogInfo.QuestId;
			logEntry.Title = questLogInfo.Title;
			logEntry.Description = questLogInfo.Description;
			logEntry.Objective = questLogInfo.Objective;
			logEntry.RewardItems = new List<ItemEntry>(questLogInfo.RewardItems);

			// Update the quest log frame
			GenericEventArgs eventArgs = new GenericEventArgs();
			eventArgs.eventType = "QUEST_LOG_UPDATE";
			UiSystem.DispatchEvent(eventArgs);
		}
		public void HandleQuestStateInfo(BaseWorldMessage message) {
			QuestStateInfoMessage questStateInfo = (QuestStateInfoMessage)message;
			foreach (QuestLogEntry entry in questLogEntries) {
				if (entry.QuestId != questStateInfo.QuestId)
					continue;
				entry.Objectives.Clear();
				foreach (string objective in questStateInfo.Objectives)
					entry.Objectives.Add(objective);
			}

			// Update the quest log frame
			GenericEventArgs eventArgs = new GenericEventArgs();
			eventArgs.eventType = "QUEST_LOG_UPDATE";
			UiSystem.DispatchEvent(eventArgs);
		}

		public void HandleGroupInfo(BaseWorldMessage message) {
			GroupInfoMessage groupInfo = (GroupInfoMessage)message;
            if (groupInfo.GroupInfoEntries.Count == 1) {
                if (groups.ContainsKey(groupInfo.LeaderId))
                    groups.Remove(groupInfo.LeaderId);
                if (groupLeaderId == groupInfo.LeaderId ||
                    groupInfo.LeaderId == client.Player.Oid)
                    groupLeaderId = -1;
            } else {
                List<GroupEntry> groupMemberInfo = new List<GroupEntry>();
                foreach (GroupInfoEntry entry in groupInfo.GroupInfoEntries) {
                    GroupEntry memberInfo = new GroupEntry();
                    memberInfo.memberId = entry.memberId;
                    memberInfo.memberName = entry.memberName;
                    groupMemberInfo.Add(memberInfo);
                    // If the player is a member of the group, set the player's
                    // group leader id.
                    if (memberInfo.memberId == client.Player.Oid)
                        groupLeaderId = groupInfo.LeaderId;
                }
            }
			// Update the party frame
			GenericEventArgs eventArgs = new GenericEventArgs();
			eventArgs.eventType = "PARTY_MEMBERS_CHANGED";
			UiSystem.DispatchEvent(eventArgs);
		}

        /// <summary>
        ///   Special handling for RemoveQuestResponse messages.
        ///   This removes the entry from the internal quest structures.
        /// </summary>
        /// <param name="message">the message object from the server</param>
        public void HandleRemoveQuestResponse(BaseWorldMessage message) {
            RemoveQuestResponseMessage removeQuestInfo = (RemoveQuestResponseMessage)message;
            int index = 1; // questLogSelectedIndex is 1 based.
            foreach (QuestLogEntry entry in questLogEntries) {
                if (entry.QuestId == removeQuestInfo.QuestId) {
                    questLogEntries.Remove(entry);
                    break;
                }
                index++;
            }
            if (index == questLogSelectedIndex)
                // we removed the selected entry. reset selection
                questLogSelectedIndex = 0;
            else if (index < questLogSelectedIndex)
                // removed an entry before our selection - decrement our selection
                questLogSelectedIndex--;
            // if we removed an entry after our selection, we don't need to do anything
            // Update the quest log frame
            GenericEventArgs eventArgs = new GenericEventArgs();
            eventArgs.eventType = "QUEST_LOG_UPDATE";
            UiSystem.DispatchEvent(eventArgs);
        }
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

        /// <summary>
        ///   Union of the stats and states
        /// </summary>
        /// <param name="message"></param>
        public void HandleOldObjectProperty(BaseWorldMessage message) {
            log.Error("Got deprecated OldObjectProperty message");
        }
        public void HandleObjectProperty(BaseWorldMessage message) {
            ObjectPropertyMessage propMessage = (ObjectPropertyMessage)message;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            bool forPlayer = client.Player != null && client.Player.Oid == message.Oid;
			foreach (string key in propMessage.Properties.Keys) {
                object val = propMessage.Properties[key];
				log.DebugFormat("HandleObjectProperty for OID {0}, setting prop {1} = {2}",
				    		   message.Oid, key, val);
				properties[key] = val;
            }
            if (properties.Count > 0)
                HandleObjectPropertyHelper(message.Oid, properties);
        }
        private void HandleObjectPropertyHelper(long oid, Dictionary<string, object> props)
        {
            ObjectNode node = worldManager.GetObjectNode(oid);
            if (node == null)
            {
                log.WarnFormat("Got stat update message for nonexistent object: {0}", oid);
                return;
            }
            node.UpdateProperties(props);
            foreach (string prop in props.Keys) {
                if (client.Target != null && oid == client.Target.Oid) {
                    string unit = "target";
                    // Always post some sort of stat event
                    GenericEventArgs eventArgs = new GenericEventArgs();
                    eventArgs.eventType = "PROPERTY_" + prop;
                    eventArgs.eventArgs = new string[1];
                    eventArgs.eventArgs[0] = unit;
                    UiSystem.DispatchEvent(eventArgs);
                }
                if (client.Player != null && oid == client.Player.Oid) {
                    string unit = "player";
                    // Always post some sort of stat event
                    GenericEventArgs eventArgs = new GenericEventArgs();
                    eventArgs.eventType = "PROPERTY_" + prop;
                    eventArgs.eventArgs = new string[1];
                    eventArgs.eventArgs[0] = unit;
                    UiSystem.DispatchEvent(eventArgs);
                }
                {
                    // Always post an "any" unit event.
                    GenericEventArgs eventArgs = new GenericEventArgs();
                    eventArgs.eventType = "PROPERTY_" + prop;
                    eventArgs.eventArgs = new string[2];
                    eventArgs.eventArgs[0] = "any";
                    eventArgs.eventArgs[1] = oid.ToString();
                    UiSystem.DispatchEvent(eventArgs);
                    // DEBUG: - print out the property messages until we are done
                    // Trace.TraceInformation("Injecting PROPERTY message for {0} with key '{1}' and value '{2}'", oid, prop, props[prop]);
                }
            }
        }
        #endregion
		private void AttachQuestIcon(long oid, bool conclude) {
			ObjectNode parentObj = worldManager.GetObjectNode(oid);
			if (parentObj == null) {
				log.WarnFormat("Got attach message for nonexistent object: {0}", oid);
				return;
			}
			string entityName =	oid.ToString() + "_attach_questavailable";
			Entity entity;
			Monitor.Enter(worldManager.SceneManager);
			try {
                string attachmentMesh;
                if (conclude) {
                    attachmentMesh = questConcludableMesh;
                }
                else {
                    attachmentMesh = questAvailableMesh;
                }

                // remove any old attachment first
                MovableObject sceneObj = parentObj.DetachLocalObject("questavailable");
                if (sceneObj is Entity) {
                    log.InfoFormat("Removing entity: {0}", sceneObj);
                    worldManager.SceneManager.RemoveEntity((Entity)sceneObj);
                }

                entity = worldManager.SceneManager.CreateEntity(entityName, attachmentMesh);
                parentObj.AttachLocalObject("questavailable", entity);
			} catch (AxiomException e) {
				log.ErrorFormat("ignoring attach, due to AxiomException: {0}", e);
			} finally {
				Monitor.Exit(worldManager.SceneManager);
			}
		}
		private void DetachQuestIcon(long oid) {
			ObjectNode parentObj = worldManager.GetObjectNode(oid);
			if (parentObj == null) {
				log.WarnFormat("Got detach message for nonexistent object: {0}", oid);
				return;
			}
			Monitor.Enter(worldManager.SceneManager);
            try {
                MovableObject sceneObj = parentObj.DetachLocalObject("questavailable");
				if (sceneObj is Entity) {
					log.InfoFormat("Removing entity: {0}", sceneObj);
					worldManager.SceneManager.RemoveEntity((Entity)sceneObj);
				}
			} finally {
				Monitor.Exit(worldManager.SceneManager);
			}
		}

        /// <summary>
        ///   Write a message to the chat window and also to the log file.
        /// </summary>
        /// <param name="message">the message to write</param>
		public void Write(string message) {
			Write(message, 0);
			log.DebugFormat(message);
		}

        /// <summary>
        ///   Register our message handlers with the message callback system
        ///   so that we can have custom logic associated with incoming 
        ///   messages.
        /// </summary>
		public void SetupMessageHandlers() {
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Comm,
													   new WorldMessageHandler(this.HandleComm));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AcquireResponse,
													   new WorldMessageHandler(this.HandleAcquireResponse));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.EquipResponse,
													   new WorldMessageHandler(this.HandleEquipResponse));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.UnequipResponse,
													   new WorldMessageHandler(this.HandleUnequipResponse));

			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Attach,
													   new WorldMessageHandler(this.HandleAttach));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Detach,
													   new WorldMessageHandler(this.HandleDetach));

			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Damage,
													   new WorldMessageHandler(this.HandleDamage));
            // These two exist for use with older servers
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.StatUpdate,
													   new WorldMessageHandler(this.HandleStatUpdate));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.StateMessage,
													   new WorldMessageHandler(this.HandleState));

			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.QuestInfoResponse,
													   new WorldMessageHandler(this.HandleQuestInfoResponse));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.InventoryUpdate,
													   new WorldMessageHandler(this.HandleInventoryUpdate));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.QuestLogInfo,
													   new WorldMessageHandler(this.HandleQuestLogInfo));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.QuestStateInfo,
													   new WorldMessageHandler(this.HandleQuestStateInfo));
			MessageDispatcher.Instance.RegisterHandler(WorldMessageType.GroupInfo,
													   new WorldMessageHandler(this.HandleGroupInfo));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.RemoveQuestResponse,
                                                       new WorldMessageHandler(this.HandleRemoveQuestResponse));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.AbilityUpdate,
                                                       new WorldMessageHandler(this.HandleAbilityUpdate));

            // Register for notification about object movement, so we can update the collision data
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Direction,
                                                       new WorldMessageHandler(this.HandleDirection));

            
            // Replacement for the StatUpdate and StateMessage handlers
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.OldObjectProperty,
                                                       new WorldMessageHandler(this.HandleOldObjectProperty));
            MessageDispatcher.Instance.RegisterHandler(WorldMessageType.ObjectProperty,
                                                       new WorldMessageHandler(this.HandleObjectProperty));
        }

        /// <summary>
        ///   This is for supporting users that have not transitioned their
        ///   interface code to the new system.
        /// </summary>
        /// <param name="frame"></param>
        public void PrepareEditBox(Frame frame) {
            if (editFrame != null) {
                // remove the existing edit frame's callbacks
                FontString fontString = (FontString)editFrame.FontString;
                LayeredEditBox leb = (LayeredEditBox)fontString.window;
                leb.TextAccepted -= new EventHandler(this.PostTextAccepted);
                leb.Enabled -= new EventHandler(this.EditBoxEnabled);
                leb.Disabled -= new EventHandler(this.EditBoxDisabled);
            }
            editFrame = (Multiverse.Interface.EditBox)frame;
            if (editFrame != null) {
                FontString fontString = (FontString)editFrame.FontString;
                // TODO: Re-enable
                LayeredEditBox leb = (LayeredEditBox)fontString.window;
                leb.TextAccepted += new EventHandler(this.PostTextAccepted);
                leb.Enabled += new EventHandler(this.EditBoxEnabled);
                leb.Disabled += new EventHandler(this.EditBoxDisabled);
                //leb.PostCharacter += new EventHandler(this.PostCharacter);
            }
        }

        /// <summary>
        ///   Called when the UI is unloaded so we can clear our UI related 
        ///   state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUiUnload(object sender, EventArgs e) {
            editFrame = null;
        }

        /// <summary>
        ///   Set the edit mode to change whether our input events are going
        ///   to the chat frame input box or not.
        /// </summary>
        /// <param name="mode">
        ///   the value of the edit mode (true to send input events to the 
        ///   chat frame input box)
        /// </param>
        public void SetEditMode(bool mode) {
            if (!UiSystem.FrameMap.ContainsKey("MvChatFrameInputFrameEditBox"))
                return;
            Multiverse.Interface.EditBox editFrame =
                (Multiverse.Interface.EditBox)UiSystem.FrameMap["MvChatFrameInputFrameEditBox"];
            if (mode) {
                editFrame.Show();
                editFrame.SetFocus();
            } else {
                editFrame.ClearFocus();
                editFrame.Hide();
            }
        }

		/// <summary>
		///		Handle the line submission for the edit box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PostTextAccepted(object sender, EventArgs e) {
			string text = editFrame.GetText();
            client.HandleCommand(text);
			
			// since we may have reloaded the ui, grab the frame again
            if (text.Length > 0)
			    editFrame.AddHistoryLine(text);
			editFrame.SetText(string.Empty);
			
			editFrame.ClearFocus();
    		editFrame.Hide();
		}
		private void EditBoxEnabled(object sender, EventArgs e) {
			editFrame.Show();
		}
		private void EditBoxDisabled(object sender, EventArgs e) {
			editFrame.Hide();
		}

        /// <summary>
        ///   Register any custom command handlers for this world.  This sets
        ///   up callbacks for slash commands such as '/script' or '/say'
        /// </summary>
        /// <param name="commandHandlers">
        ///   the dictionary mapping commands to a delegate that will be 
        ///   invoked to handle that command
        /// </param>
		public void SetupCommandHandlers(Dictionary<string, GameWorldCommandHandler> commandHandlers) {
			commandHandlers["help"] = delegate(Client client, ObjectNode target, string args)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Commands:");
				foreach (string key in commandHandlers.Keys) {
					sb.Append(" ");
					sb.Append(key);
				}
				Write(sb.ToString());
			};
			commandHandlers["say"] = delegate(Client client, ObjectNode target, string args)
			{
				if (args != null && args.Length > 0)
					client.NetworkHelper.SendCommMessage(args);
			};
			commandHandlers["loc"] = delegate(Client client, ObjectNode target, string args)
			{
				Write("Player: " + client.Player.Position);
			};
			commandHandlers["script"] = delegate(Client client, ObjectNode target, string args)
			{
                try {
                    Multiverse.Interface.UiScripting.RunScript(args);
                } catch (Exception e) {
                    Write("Script Error: " + e.Message);
                    // throw;
                }
			};
			commandHandlers["acquire"] = delegate(Client client, ObjectNode target, string args)
			{
				client.NetworkHelper.SendAcquireMessage(target.Oid);
			};
			commandHandlers["pickup"] = delegate(Client client, ObjectNode target, string args)
			{
				client.NetworkHelper.SendAcquireMessage(target.Oid);
				client.NetworkHelper.SendEquipMessage(target.Oid, "primaryWeapon");
			};
			commandHandlers["reloadui"] = delegate(Client client, ObjectNode target, string args)
			{
				// Multiverse.Interface.UiSystem.DebugDump(0, client.XmlUiWindow);
				client.ReloadUiElements();
				// this.PrepareEditBox(); -- this is now called by ChatFrame.boo
				// Multiverse.Interface.UiSystem.DebugDump(0, client.XmlUiWindow);
			};
			commandHandlers["clientloc"] = delegate(Client client, ObjectNode target, string args)
			{
				string[] tokens = args.Split('\t', '\n', ' ');
				if (tokens.Length > 2) {
					Vector3 loc = new Vector3();
					loc.x = float.Parse(tokens[0]);
					loc.y = float.Parse(tokens[1]);
					loc.z = float.Parse(tokens[2]);
					target.SetDirLoc(WorldManager.CurrentTime, Vector3.Zero, loc);
				} else {
					Write("Usage: /clientloc <x> <y> <z>");
				}
			};
			commandHandlers["setorient"] = delegate(Client client, ObjectNode target, string args)
			{
				string[] tokens = args.Split('\t', '\n', ' ');
				if (tokens.Length > 3) {
					Quaternion rot = new Quaternion();
					rot.x = float.Parse(tokens[0]);
					rot.y = float.Parse(tokens[1]);
					rot.z = float.Parse(tokens[2]);
					rot.w = float.Parse(tokens[3]);
					target.Orientation = rot;
				} else {
					Write("Usage: /setorient <x> <y> <z> <w>");
				}
			};
			commandHandlers["setscale"] = delegate(Client client, ObjectNode target, string args)
			{
				string[] tokens = args.Split('\t', '\n', ' ');
				if (tokens.Length > 2) {
					Vector3 scale = new Vector3();
					scale.x = float.Parse(tokens[0]);
					scale.y = float.Parse(tokens[1]);
					scale.z = float.Parse(tokens[2]);
					target.SceneNode.ScaleFactor = scale;
				} else {
					Write("Usage: /setscale <x> <y> <z>");
				}
			};
			commandHandlers["fps"] = delegate(Client client, ObjectNode target, string args)
			{
				Write(string.Format("Current FPS: {0}", Root.Instance.CurrentFPS));
				Write(string.Format("Best FPS: {0}", Root.Instance.BestFPS));
				Write(string.Format("Worst FPS: {0}", Root.Instance.WorstFPS));
				Write(string.Format("Average FPS: {0}", Root.Instance.AverageFPS));
				Write(string.Format("Triangle Count: {0}", Root.Instance.SceneManager.TargetRenderSystem.FacesRendered));
                Write(string.Format("Message Queue: {0}", MessageDispatcher.Instance.MessageCount));
                Write(string.Format("Available Texture Memory: {0}", TextureManager.Instance.AvailableTextureMemory));
			};

			commandHandlers["animstates"] = delegate(Client client, ObjectNode target, string args)
			{
				Write(target.GetAnimationStates());
			};
			commandHandlers["target"] = delegate(Client client, ObjectNode target, string args)
			{
				string tmp = null;
				string[] tokens = args.Split('\t', '\n', ' ');
				if (tokens.Length > 0)
					tmp = tokens[0];
				if (tmp != null)
					client.Target = worldManager.GetObjectNode(tmp);
			};
			commandHandlers["tar"] = commandHandlers["target"];
            commandHandlers["properties"] = delegate(Client client, ObjectNode target, string args) {
                Write(string.Format("Properties for {0}:", target.Name));
                foreach (string key in target.PropertyNames)
                    Write(string.Format(" {0}: {1}", key, EncodedObjectIO.FormatEncodedObject(target.GetProperty(key))));
            };
            commandHandlers["props"] = commandHandlers["properties"];
			commandHandlers["modules"] = delegate(Client client, ObjectNode target, string args)
			{
				// client.PrintModules();
			};
			commandHandlers["info"] = delegate(Client client, ObjectNode target, string args)
			{
				Write("Target Id: " + target.Oid);
				Write("Target Position: " + target.Position);
				Write("Target Orientation: " + target.Orientation);
				Write("Target SceneNode Orientation: " + target.SceneNode.Orientation);
				Write("Camera Position: " + client.Camera.Position);
				Write("Camera Orientation: " + client.Camera.Orientation);
			};
			commandHandlers["logout"] = delegate(Client client, ObjectNode target, string args)
			{
				client.NetworkHelper.SendLogoutMessage();
			};
			commandHandlers["attack"] = delegate(Client client, ObjectNode target, string args)
			{
				client.NetworkHelper.SendAttackMessage(target.Oid, "strike", true);
			};
			commandHandlers["server"] = delegate(Client client, ObjectNode target, string args)
			{
				client.NetworkHelper.SendCommMessage("/" + args);
			};
			commandHandlers["windowDump"] = delegate(Client client, ObjectNode target, string args)
			{
                Multiverse.Interface.UiSystem.DebugDump(0, client.XmlUiWindow);
                Multiverse.Interface.UiSystem.DebugDump();
            };
			commandHandlers["lights"] = delegate(Client client, ObjectNode target, string args)
			{
				List<LightEntity> lights = worldManager.GetLightNodes();
				foreach (LightEntity light in lights) 
					Write(light.ToString());
			};
			commandHandlers["glow"] = delegate(Client client, ObjectNode target, string args)
			{
				bool glow = bool.Parse(args);
				target.Glow(glow);
			};
            commandHandlers["playerregions"] = delegate(Client client, ObjectNode target, string args)
            {
                List<WorldManager.OidAndRegion> list = worldManager.RegionsContaingPoint(client.Player.Collider.Center());
                foreach (WorldManager.OidAndRegion oidAndRegion in list)
                    Write(string.Format(" oid {0}: region {1}", oidAndRegion.Oid, oidAndRegion.Region));
                Write(string.Format("{0} regions containing player", list.Count));
            };
            
		}

        /// <summary>
        ///   Attack the target (if the target is a legitimate attack target)
        /// </summary>
		public void AttackTargetHelper() {
            if (client.Target == null || client.Target == client.Player ||
                !client.Target.CheckBooleanProperty("attackable"))
                worldManager.NetworkHelper.SendAttackMessage(client.Player.Oid, "strike", false);
            else {
				if (client.Player.CheckBooleanProperty("combatstate"))
					worldManager.NetworkHelper.SendAttackMessage(client.Target.Oid, "strike", false);
				else
					worldManager.NetworkHelper.SendAttackMessage(client.Target.Oid, "strike", true);
			}
		}

		// F8 => target nearest mob (last = null)
		// Tab => target closest - or last


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

        /// <summary>
        ///   Called to update our target.
        ///   This injects a 'PLAYER_TARGET_CHANGED' UI event so that 
        ///   any interested widgets can update their data.
        /// </summary>
        /// <param name="target">the current target</param>
		public void UpdateTarget(ObjectNode target) {
            if (target == currentTarget)
                return;
			lastTarget = currentTarget;
			if (lastTarget != null && IsAttackable(lastTarget))
				lastEnemy = lastTarget;
			currentTarget = target;
			if (currentTarget != null && IsAttackable(currentTarget))
				currentEnemy = currentTarget;
            GenericEventArgs eventArgs = new GenericEventArgs();
            eventArgs.eventType = "PLAYER_TARGET_CHANGED";
            UiSystem.DispatchEvent(eventArgs);
            OnTargetChanged(target);
        }

        /// <summary>
        /// TargetChanged event for scripting API
        /// </summary>
        /// <param name="target">the new target</param>
        public event TargetChangedHandler TargetChanged;

        public void OnTargetChanged(ObjectNode target)
        {
            TargetChangedHandler handler = TargetChanged;
            if (handler != null)
            {
                handler(target);
            }
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
        ///   Writes a message using the specified channel.  The channel is 
        ///   used to determine what color of text to use, and the message
        ///   is then passed to the 'MvChatFrameOutputScrollingMessageFrame0'
        ///   widget.
        /// </summary>
        /// <param name="message">the message to write</param>
        /// <param name="channelId">the channel to use</param>
		public void Write(string message, int channelId) {
            // If the channel is something other than 0, it should already have been
            // handled by another chunk of code that generates the chat_msg event.
            if (channelId == 0) {
                // New system that dispatches events using the ui event system
                GenericEventArgs eventArgs = new GenericEventArgs();
                eventArgs.eventType = "CHAT_MSG_SYSTEM";
                eventArgs.eventArgs = new string[2];
                eventArgs.eventArgs[0] = message;
                eventArgs.eventArgs[1] = string.Empty;
                UiSystem.DispatchEvent(eventArgs);
            }
			if (!UiSystem.FrameMap.ContainsKey("MvChatFrameOutputScrollingMessageFrame0"))
				return;
			IScrollingMessageFrame chatFrame =
				(IScrollingMessageFrame)UiSystem.FrameMap["MvChatFrameOutputScrollingMessageFrame0"];
            ColorEx color;
            if (chatColors.ContainsKey(channelId))
                color = chatColors[channelId];
            else
                color = ColorEx.White;
			chatFrame.AddMessage(message, color.r, color.g, color.b, channelId);
		}

        /// <summary>
        ///   Determine whether the given object has 'pvpstate' set to 1
        /// </summary>
        /// <param name="obj">the object whose status we are checking</param>
        /// <returns>true if the object has the 'pvpstate' set to 1</returns>
		private bool IsPVP(ObjectNode obj) {
            return obj.CheckBooleanProperty("pvpstate");
		}
		private bool IsDead(ObjectNode obj) {
            return obj.CheckBooleanProperty("deadstate");
		}
		private bool IsAttackable(ObjectNode obj) {
            return obj.CheckBooleanProperty("attackable");
		}
		private bool InCombat(ObjectNode obj) {
            return obj.CheckBooleanProperty("combatstate");
		}

        // Mouse click handler that we set up for new objects
		protected void OnMouseClicked(object sender, Axiom.Input.MouseEventArgs args) {
			if (!(sender is ObjectNode))
				return;
			ObjectNode objNode = (ObjectNode)sender;
			if (args.Button == Axiom.Input.MouseButtons.Left) {
				client.Target = objNode;
			} else if (args.Button == Axiom.Input.MouseButtons.Right) {
				Vector3 delta = objNode.Position - client.Player.Position;
				// On a right click, do the context sensitive action
				if (objNode.CheckBooleanProperty("lootable")) {
					client.NetworkHelper.SendTargettedCommand(objNode.Oid, "/lootall");
				} else if (objNode.CheckBooleanProperty("questconcludable")) {
					log.Info("questconcludable");
					if (delta.Length < 6 * Client.OneMeter)
						client.NetworkHelper.SendQuestConcludeRequestMessage(objNode.Oid);
					else
                        Write("That object is too far away");
                } else if (objNode.CheckBooleanProperty("questavailable")) {
                    log.Info("questavailable");
					if (delta.Length < 6 * Client.OneMeter)
						client.NetworkHelper.SendQuestInfoRequestMessage(objNode.Oid);
					else
                        Write("That object is too far away");
                } else if (objNode.CheckBooleanProperty("attackable")) {
					if (delta.Length < 4 * Client.OneMeter)
						client.NetworkHelper.SendAttackMessage(objNode.Oid, "attack", true);
					else
                        Write("That object is too far away");
                } else if (objNode.ObjectType == ObjectNodeType.Item) {
					if (delta.Length < 4 * Client.OneMeter)
						client.NetworkHelper.SendAcquireMessage(objNode.Oid);
					else
						Write("That object is too far away");
				}
			}
		}

        // Update the context cursor
		protected void UpdateCursor(ObjectNode objNode) {
			Vector3 delta = objNode.Position - client.Player.Position;
			if (objNode.CheckBooleanProperty("lootable")) {
				if (delta.Length < 4 * Client.OneMeter)
					SetCursor("LOOT_CURSOR");
				else
					SetCursor("LOOT_ERROR_CURSOR");
			} else if (objNode.CheckBooleanProperty("questconcludable")) {
                log.InfoFormat("questconcludable");
				if (delta.Length < 6 * Client.OneMeter)
					SetCursor("SPEAK_CURSOR");
				else
					SetCursor("SPEAK_ERROR_CURSOR");
			} else if (objNode.CheckBooleanProperty("questavailable")) {
                log.InfoFormat("questavailable");
				if (delta.Length < 6 * Client.OneMeter)
					SetCursor("SPEAK_CURSOR");
				else
					SetCursor("SPEAK_ERROR_CURSOR");
			} else if (objNode.CheckBooleanProperty("attackable")) {
				if (delta.Length < 4 * Client.OneMeter)
					SetCursor("ATTACK_CURSOR");
				else
					SetCursor("ATTACK_ERROR_CURSOR");
			} else if (objNode.ObjectType == ObjectNodeType.Item) {
				if (delta.Length < 6 * Client.OneMeter)
					SetCursor("LOOT_CURSOR");
				else
					SetCursor("LOOT_ERROR_CURSOR");
			}
		}

        // Mouse enter handler that we set up for new objects
		protected void OnMouseEnter(object sender, Axiom.Input.MouseEventArgs args) {
            ObjectNode objNode = sender as ObjectNode;
            if (objNode == null)
                return;
            objNode.Glow(true);

            // Update the context cursor
			UpdateCursor(objNode);
		}

        // Mouse exit handler that we set up for new objects
		protected void OnMouseExit(object sender, Axiom.Input.MouseEventArgs args) {
			ObjectNode objNode = sender as ObjectNode;
            if (objNode == null)
                return;
			objNode.Glow(false);

            // Reset the context cursor
            UiSystem.SetCursor(1, null);
		}



		#region Global API methods

		// Quest methods
		public string[] GetQuestLogQuestText() {
			int questIndex = questLogSelectedIndex;
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				return null;
			string[] rv = new string[2];
			foreach (QuestLogEntry entry in questLogEntries)
				log.InfoFormat("Desc: {0}; Obj: {1}", entry.Description, entry.Objective);
			rv[0] = questLogEntries[questIndex - 1].Description;
			rv[1] = questLogEntries[questIndex - 1].Objective;
			return rv;
		}
		public int[] GetNumQuestLogEntries() {
			int[] rv = new int[2];
			rv[0] = questLogEntries.Count;
			rv[1] = questLogEntries.Count;
			return rv;
		}
		public string GetQuestLogTitle(int questIndex) {
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				return null;
			return questLogEntries[questIndex - 1].Title;
		}
		public int GetQuestLogSelection() {
			return questLogSelectedIndex;
		}
		public int GetQuestMoneyToGet() {
			return 0;
		}
		public void SelectQuestLogEntry(int questIndex) {
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				questLogSelectedIndex = 0;
			else
				questLogSelectedIndex = questIndex;
		}
		public bool GetQuestLogPushable() {
			return false;
		}
		public string GetTitleText() {
			return lastQuestTitleText;
		}
		public string GetObjectiveText() {
			return lastQuestObjectiveText;
		}
		public string GetQuestText() {
			return lastQuestDescriptionText;
		}
		protected List<object> GetQuestRewardInfo(List<ItemEntry> entries, int itemIndex) {
			if (itemIndex <= 0 || itemIndex > entries.Count)
				return null;
			ItemEntry entry = entries[itemIndex - 1];
			List<object> rv = new List<object>();
			rv.Add(entry.name);
			rv.Add(entry.icon);
			rv.Add(entry.count);
			rv.Add(1);
			rv.Add(1);
			return rv;
		}
		public List<object> GetQuestItemInfo(string itemType, int itemIndex) {
			// if (itemType == "reward")
			List<ItemEntry> entries = rewardItems;
			return GetQuestRewardInfo(entries, itemIndex);
		}

		public List<object> GetQuestLogRewardInfo(int itemIndex) {
			int questIndex = questLogSelectedIndex;
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				return null;
			List<ItemEntry> entries = questLogEntries[questIndex - 1].RewardItems;
			return GetQuestRewardInfo(entries, itemIndex);
		}
		public int GetNumQuestRewards() {
			return rewardItems.Count;
		}
		public int GetNumQuestLogRewards() {
			int questIndex = questLogSelectedIndex;
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				return 0;
			return questLogEntries[questIndex - 1].RewardItems.Count;
		}
		public int GetNumQuestLeaderBoards(int questIndex) {
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				return 0;
			return questLogEntries[questIndex - 1].Objectives.Count;
		}
		public object[] GetQuestLogLeaderBoard(int objIndex, int questIndex) {
			if (questIndex <= 0 || questIndex > questLogEntries.Count)
				return null;
			if (objIndex <= 0 || objIndex > questLogEntries[questIndex - 1].Objectives.Count)
				return null;
			object[] rv = new object[3];
			rv[0] = questLogEntries[questIndex - 1].Objectives[objIndex - 1];
			rv[1] = "monster";
			rv[2] = false;
			return rv;
		}
		public void AcceptQuest() {
			SendQuestResponse(true);
		}
		public void DeclineQuest() {
			SendQuestResponse(false);
		}
		public void AbandonQuest() {
		}
		public void CompleteQuest() {
		}
		private void SendQuestResponse(bool accepted) {
			client.NetworkHelper.SendQuestResponseMessage(lastObjectId, lastQuestId, accepted);
		}

		// Grouping methods
		public void AcceptGroup() {
			// SendQuestResponse(true);
		}
		public void DeclineGroup() {
			// SendQuestResponse(false);
		}
		public int GetNumPartyMembers() {
            if (groupLeaderId < 0 || !groups.ContainsKey(groupLeaderId))
                return 0;
            return groups[groupLeaderId].Count - 1;
		}
		public long GetPartyMember(int groupIndex) {
			int curIndex = 0;
            if (groupLeaderId < 0 || !groups.ContainsKey(groupLeaderId))
                return -1;
            List<GroupEntry> groupMemberInfo = groups[groupLeaderId];
            foreach (GroupEntry entry in groupMemberInfo) {
				if (entry.memberId == client.Player.Oid)
					continue;
				else
					curIndex++;
				if (curIndex == groupIndex)
					return entry.memberId;
			}
			return -1;
		}
        public bool IsPartyLeader() {
            if (client.Player == null)
                return false;
            return (groupLeaderId == client.Player.Oid);
        }
		public void InviteByName(string name) {
			ObjectNode objNode = worldManager.GetObjectNode(name);
			if (objNode != null)
				client.NetworkHelper.SendTargettedCommand(objNode.Oid, "/invite");
		}

		public void LeaveParty() {
			client.NetworkHelper.SendTargettedCommand(0, "/disband");
		}

        // Ability methods
        public int GetNumAbilities() {
            return abilities.Count;
        }

        public AbilityEntry GetAbility(int index) {
            return abilities[index-1];
        }

        public void PickupAbility(AbilityEntry ability) {
            cursorAbility = ability;
            cursorItem = null;
            if (ability != null) {
                log.InfoFormat("Pickup ability: {0}, {1}", ability.name, ability.icon);
                // Abilities are priority 0 (before context cursors such as attack)
                UiSystem.SetCursor(0, ability.icon);
            } else {
                log.InfoFormat("Drop ability");
                UiSystem.SetCursor(0, null);
            }
        }

        public AbilityEntry GetCursorAbility() {
            return cursorAbility;
        }

		// Inventory methods
		public int GetContainerNumSlots(int containerId) {
			return 16;
		}

		private InvItemInfo GetContainerItem(int containerId, int slotId) {
			// From the scripting interface, the ids are 1-5 and 1-16
			// Adjust this to be zero based;
			slotId = slotId - 1;
			if (!inventory.ContainsKey(containerId))
				return null;
			Container container = inventory[containerId];
			if (!container.ContainsKey(slotId))
				return null;
			return container[slotId];
		}

		public List<object> GetContainerItemInfo(int containerId, int slotId) {
			InvItemInfo item = GetContainerItem(containerId, slotId);
			if (item == null)
				return null;
			List<object> rv = new List<object>();
			rv.Add(item.icon);
			rv.Add(1);     // count
			rv.Add(false); // locked
			rv.Add(1);     // quality
			rv.Add(false); // readable
			return rv;
		}
        /// <summary>
        ///   Get information about an equipped item.
        ///   Not Implemented
        /// </summary>
        /// <param name="slotName">the equipment slot (e.g. 'HeadSlot')</param>
        /// <returns>
        ///   a list containing the slot id (e.g. 1) and the icon to use 
        ///   (e.g. 'Interface\\Icons\\INV_cloth_hat01')
        /// </returns>
		public List<object> GetInventorySlotInfo(string slotName) {
			List<object> rv = new List<object>();
			int slotId = 0;
			switch (slotName) {
				case "HeadSlot":
					slotId = 1;
					break;
				case "ChestSlot":
					slotId = 2;
					break;
				case "MainHandSlot":
					slotId = 3;
					break;
				default:
					break;
			}
			rv.Add(slotId);
			rv.Add("Interface\\Icons\\" + "foobar");
			return rv;
		}

        // not implemented
		public string GetInventoryItemTexture(string selection, int slotId) {
			return "Interface\\Icons\\" + "foobar";
		}

        // not implemented
        public int GetInventoryItemCount(string selection, int slotId) {
			return 0;
		}

        /// <summary>
        ///   Pick up (or drop) an item from a given container and slot.
        ///   This will set the cursor to the item.
        /// </summary>
        /// <param name="containerId">the id of the container</param>
        /// <param name="slotId">the slot within the container</param>
		public void PickupContainerItem(int containerId, int slotId) {
			InvItemInfo item = GetContainerItem(containerId, slotId);
            if (item != null) {
                cursorItem = item;
                cursorAbility = null;
                log.InfoFormat("Pickup item: {0}, {1}", item, item.icon);
                // Items are priority 0 (before context cursors such as attack)
                UiSystem.SetCursor(0, item.icon);
            } else {
                cursorItem = null;
                log.InfoFormat("Drop item to: {0}, {1}", containerId, slotId);
                UiSystem.SetCursor(0, null);
            }
		}

        public void PickupItem(InvItemInfo item) {
            cursorItem = item;
            cursorAbility = null;
            UiSystem.SetCursor(0, item.icon);
        }

        public InvItemInfo GetCursorItem() {
            return cursorItem;
        }
        
        // not implemented
        public void DeleteCursorItem() {
            cursorItem = null;
            log.InfoFormat("Called delete cursor item");
        }

        /// <summary>
        ///   Activate an item from a given container and slot.
        ///   This sends the '/activate' message to the server.
        /// </summary>
        /// <param name="containerId">the id of the container</param>
        /// <param name="slotId">the slot within the container</param>
        public void UseContainerItem(int containerId, int slotId, long objectId) {
			InvItemInfo item = GetContainerItem(containerId, slotId);
			if (item != null)
				client.NetworkHelper.SendActivateItemMessage(item.itemId, objectId);
		}

        // not implemented
		public bool HasAction(int slotId) {
            return actions.ContainsKey(slotId);
		}

        // not implemented
		public string GetActionTexture(int slotId) {
            ActionEntry action = actions[slotId];
            if (action != null) {
                if (action.ability != null)
                    return action.ability.icon;
                else if (action.item != null)
                    return action.item.icon;
            }
            return "";
		}

        // not implemented
		public void UseAction(int slotId, long objectId) {
            log.InfoFormat("Using action: " + slotId);
            ActionEntry action = actions[slotId];
            if (action.ability != null)
                client.NetworkHelper.SendTargettedCommand(objectId, "/ability " + action.ability.name);
                // client.NetworkHelper.SendStartAbilityMessage(action.ability.name, objectId);
            else if (action.item != null)
                client.NetworkHelper.SendActivateItemMessage(action.item.itemId, objectId);
		}

        public void SetAction(int slotId, ActionEntry action) {
            if (action == null) {
                actions.Remove(slotId);
                return;
            }
            actions[slotId] = action;
        }

        public ActionEntry GetAction(int slotId) {
            return actions[slotId];
        }
        
        /// <summary>
		///   Helper method to resolve a name for an object (such as "target")
        ///   to an actual ObjectNode
		/// </summary>
		/// <param name="selection">the selection (e.g. "target")</param>
		/// <returns>the ObjectNode that corresponds to the selection, or null</returns>
		private ObjectNode GetUnit(string selection) {
			long oid = GetUnitOid(selection);
			if (oid < 0)
				return null;
			return worldManager.GetObjectNode(oid);
		}

		public long GetUnitOid(string selection) {
			switch (selection) {
				case "player":
					return client.PlayerId;
				case "party1":
					return GetPartyMember(1);
				case "party2":
					return GetPartyMember(2);
				case "party3":
					return GetPartyMember(3);
				case "party4":
					return GetPartyMember(4);
				case "target":
					if (client.Target == null)
						return -1;
					return client.Target.Oid;
				case "npc":
					return lastObjectId;
				default:
					return -1;
			}
		}

        /// <summary>
        ///   Get a property from the selected object
        /// </summary>
        /// <param name="selection">string representing the selection (e.g. "player")</param>
        /// <param name="prop">property to fetch</param>
        /// <param name="def">
        ///   default property value to return if the unit cannot be found, 
        ///   or if the unit does not have the given property
        /// </param>
        /// <returns>the attribute value from the selected object (or the default)</returns>
        public object GetUnitProperty(string selection, string prop, int def) {
            ObjectNode unit = GetUnit(selection);
            if (unit != null && unit.PropertyExists(prop))
                return unit.GetProperty(prop);
            return def;
        }
        private ObjectNode GetPartyMemberNode(int memberIndex)
        {
			long memberId = GetPartyMember(memberIndex);
			if (memberId < 0)
				return null;
			return worldManager.GetObjectNode(memberId);
		}

        /// <summary>
        ///   Gets the name of the object referenced by <i>selection</i>
        /// </summary>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>the name of the object referenced by <i>selection</i> or null if there is no matching object</returns>
        public string UnitName(string selection) {
			ObjectNode node = GetUnit(selection);
			return (node == null) ? null : node.Name;
		}
        /// <summary>
        ///   Determine whether the object referenced by <i>selection</i> is in the same party as the player.
        /// </summary>
        /// <param name="selection">the selection string (e.g. "target")</param>
        /// <returns>true if the object referenced by <i>selection</i> exists and is in the same party as the player</returns>
        public bool UnitInParty(string selection) {
			long oid = GetUnitOid(selection);
			if (oid < 0)
				return false;
            if (oid == client.Player.Oid)
                return true;
            if (groupLeaderId < 0)
                return false;
            if (!groups.ContainsKey(groupLeaderId))
                return false;
            List<GroupEntry> groupMemberInfo = groups[groupLeaderId];
			foreach (GroupEntry entry in groupMemberInfo) {
				if (entry.memberId == oid)
					return true;
			}
			return false;
		}
        /// <summary>
        ///   Determine whether the object referenced by <i>selection</i> is a party leader.
        /// </summary>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>true if the object referenced by <i>selection</i> exists and is a party leader</returns>
		public bool UnitIsPartyLeader(string selection) {
			long oid = GetUnitOid(selection);
			if (oid < 0)
				return false;
            return groups.ContainsKey(oid);
		}
        /// <summary>
        ///   Determine whether the object referenced by <i>selection</i> has its
        ///   pvp flag set.
        /// </summary>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>true if the object referenced by <i>selection</i> exists and has its pvp flag set</returns>
        public bool UnitIsPVP(string selection) {
            ObjectNode node = GetUnit(selection);
            if (node == null)
                return false;
            return IsPVP(node);
        }
        /// <summary>
        ///   Determine whether the object referenced by <i>selection</i> exists.
        /// </summary>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>true if the object referenced by <i>selection</i> exists</returns>
        public bool UnitExists(string selection) {
            ObjectNode node = GetUnit(selection);
            return node != null;
        }
        /// <summary>
        ///   Determine the level of the object referenced by <i>selection</i>
        /// </summary>
        /// <remarks>currently this is a stub and always returns 10</remarks>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>the level of the object referenced by <i>selection</i></returns>
        public int UnitLevel(string selection) {
            return 10;
        }
        /// <summary>
        ///   Determine whether the object referenced by <i>selection</i> is
        ///   controlled by the player.
        /// </summary>
        /// <remarks>currently this will only return true if the object is the player</remarks>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>true if the object referenced by <i>selection</i> is controlled by the player</returns>
        public bool UnitPlayerControlled(string selection) {
            return GetUnitOid(selection) == client.PlayerId;
        }
        /// <summary>
        ///   Determine whether the object referenced by <i>selection</i> is a corpse (dead).
        /// </summary>
        /// <param name="selection">the selection string (e.g. "player")</param>
        /// <returns>true if the object referenced by <i>selection</i>is dead</returns>
        public bool UnitIsCorpse(string selection) {
            ObjectNode node = GetUnit(selection);
            if (node == null)
                return false;
            return IsDead(node);
        }

        // Helper methods for tooltips
        public void SetBagItem(string tooltipName, int containerId, int slotId)
        {
            InvItemInfo info = GetContainerItem(containerId, slotId);
            if (info == null)
                return;
            Multiverse.Interface.Region frame = UiSystem.FrameMap[tooltipName + "TextLeft1"];
            FontString textFrame = (FontString)frame;
            textFrame.SetText(info.name);
        }

        // Cursor methods
		public void ResetCursor() {
            log.InfoFormat("Setting cursor to point");
            
            cursorItem = null;
            cursorAbility = null;
            // cursorMoney = 0;

            // Clear the inventory cursor
            UiSystem.SetCursor(0, null);
            // Clear the context cursor
            UiSystem.SetCursor(1, null);
            // Make sure we initialize the least relative cursor
            //  "Interface\\Cursor\\Point"
		}

        // Sets the cursor (currently sets the context cursor)
		public void SetCursor(string cursor) {
			switch (cursor) {
				case "CAST_CURSOR":
					UiSystem.SetCursor(1, "Interface\\Cursor\\Cast");
					break;
				case "CAST_ERROR_CURSOR":
                    UiSystem.SetCursor(1, "Interface\\Cursor\\UnableCast");
					break;
				case "SPEAK_CURSOR":
                    log.Info("Setting cursor to speak");
                    UiSystem.SetCursor(1, "Interface\\Cursor\\Speak");
					break;
				case "SPEAK_ERROR_CURSOR":
                    log.Info("Setting cursor to speak_error");
                    UiSystem.SetCursor(1, "Interface\\Cursor\\UnableSpeak");
					break;
				case "ATTACK_CURSOR":
                    UiSystem.SetCursor(1, "Interface\\Cursor\\Attack");
					break;
				case "ATTACK_ERROR_CURSOR":
                    UiSystem.SetCursor(1, "Interface\\Cursor\\UnableAttack");
					break;
				case "LOOT_CURSOR":
                    UiSystem.SetCursor(1, "Interface\\Cursor\\Pickup");
					break;
				case "LOOT_ERROR_CURSOR":
                    UiSystem.SetCursor(1, "Interface\\Cursor\\UnablePickup");
					break;
				default:
                    log.Info("Setting cursor to unknown: " + cursor);
					ResetCursor();
					break;
			}
		}

		public void AttackTarget() {
			AttackTargetHelper();
		}
		public void ClearTarget() {
			client.Target = null;
		}
        /// <summary>
        ///   Toggle the UI visibility
        /// </summary>
        public void ToggleUi() {
            client.ToggleUiVisibility();
        }
        /// <summary>
        ///   Target an object by its name.
        /// </summary>
        /// <param name="name">the name of the object to target</param>
		public void TargetByName(string name) {
			ObjectNode node = worldManager.GetObjectNode(name);
			// TODO: Should I check to make sure objectnode is a mob?
			client.Target = node;
		}
        /// <summary>
        ///   Target an object by a selection string (e.g. "player")
        /// </summary>
        /// <param name="selection">the selection string</param>
		public void TargetUnit(string selection) {
			ObjectNode node = GetUnit(selection);
			// TODO: Should I check to make sure objectnode is a mob?
			client.Target = node;
		}
        /// <summary>
        ///   Target the last enemy
        /// </summary>
		public void TargetLastEnemy() {
			client.Target = lastEnemy;
		}
        /// <summary>
        ///  Target the last target
        /// </summary>
		public void TargetLastTarget() {
			client.Target = lastTarget;
		}
        /// <summary>
        ///   Target the nearest enemy
        /// </summary>
        /// <param name="reverse"></param>
		public void TargetNearestEnemy(bool reverse) {
			TargetMobHelper(currentEnemy, reverse, true);
		}
		//TargetNearestFriend() - Selects the nearest member of your party. 
		//TargetNearestPartyMember() - Added Patch 1500 Efil 11:20, 6 Jun 2005 
		//TargetNearestRaidMember - ?. 

        // Status methods

        /// <summary>
        ///   Get the current frames per second count.
        /// </summary>
        /// <returns>the current fps</returns>
        public int GetFPS() {
            return Root.Instance.CurrentFPS;
        }

        /// <summary>
        ///   Get the number of messages in the message queue that have not
        ///   yet been processed.
        /// </summary>
        /// <returns>the number of messages in the queue</returns>
        public int GetMessageQueue() {
            return MessageDispatcher.Instance.MessageCount;
        }

        /// <summary>
        ///   Get the number of messages that were handled in the last second
        /// </summary>
        /// <returns>the number of mesages that were handled in the last second</returns>
        public int GetMessagesPerSecond() {
            return MessageDispatcher.Instance.MessagesPerSecond;
        }

        /// <summary>
        ///   Get a string describing the kilobytes per second of
        ///   message input and output.
        /// </summary>
        public string GetKBytesPerSecondString() {
            return string.Format("In {0}/Out {1}",
                                 FormatKBytesPerSecond(MessageDispatcher.Instance.BytesReceivedPerSecond),
                                 FormatKBytesPerSecond(MessageDispatcher.Instance.BytesSentPerSecond));
        }
        
        /// <summary>
        ///   Get the number of render calls we made on the last frame
        /// </summary>
        public int LastFrameRenderCalls() {
            return client.LastFrameRenderCalls;
        }
        
        /// <summary>
        ///   Helper function to format kilobytes per second in a
        ///   compact way.
        /// </summary>
        protected string FormatKBytesPerSecond(int bps) {
            if (bps < 100)
                return "0";
            else {
                float fbps = (float)bps/1000f;
                if (fbps < 10.0f)
                    return string.Format("{0:#.0}", fbps);
                else
                    return string.Format("{0}", (int)fbps);
            }
        }
            
        /// <summary>
        ///   Play a sound file (ambient)
        /// </summary>
        /// <param name="soundFile">name of the file to play</param>
        public void PlaySoundFile(string soundFile) {
            SoundSource soundSource = SoundManager.Instance.GetSoundSource(soundFile, true);
            soundSource.Looping = false;
            soundSource.Gain = 1.0f;
            soundSource.Play();
        }

        public string GetProperty(string property, string def) {
            string rv;
            if (propertyMap.TryGetValue(property, out rv))
                return rv;
            return def;
        }
        public string GetProperty(string property) {
            return GetProperty(property, "");
        }
        public void SetProperty(string property, string val) {
            propertyMap[property] = val;
        }

		#endregion

        /// <summary>
        ///   Inject a mouse click.  This will return true if we handled the click,
        ///   and do not need to continue letting lower level objects process it.
        ///   Otherwise, we will first check frames, then world objects.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>true if we handled the click</returns>
        public bool InjectClick(Axiom.Input.MouseEventArgs e) {
            switch (e.Button) {
                case Axiom.Input.MouseButtons.Right: {
                        // Check to see if we have an item in the cursor, and if so, clear it
                        if (CursorHasItem() || CursorHasMoney() || CursorHasAbility()) {
                            ResetCursor();
                            //cursorMoney = 0;
                            return true;
                        }
                    }
                    break;
                case Axiom.Input.MouseButtons.Left:
                    break;
            }
            return false;
        }

        /// <summary>
        ///   Determine if the cursor has an item
        /// </summary>
        /// <returns>true if the cursor is an item (meaning we have something picked up)</returns>
        public bool CursorHasItem() {
            return cursorItem != null;
        }
        /// <summary>
        ///   Determine if the cursor has an ability
        /// </summary>
        /// <returns>true if the cursor is an ability (meaning we have something picked up)</returns>
        public bool CursorHasAbility()
        {
            return cursorAbility != null;
        }
        /// <summary>
        ///   Determine if the cursor has money
        /// </summary>
        /// <remarks>currently this always returns false</remarks>
        /// <returns>true if the cursor is holding money (we have money picked up)</returns>
        public bool CursorHasMoney() {
            return false;
        }

        /// <summary>
        /// The currently targeted object.
        /// </summary>
        public ObjectNode CurrentTarget
        {
            get
            {
                return currentTarget;
            }
        }
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
        ///   Get the client object representing the more windows specific 
        ///   attributes of our process.
        /// </summary>
        public Client Client
        {
            get { return client; }
        }

        public string ScriptingName {
            get { return "BetaWorld"; }
        }

        public string QuestAvailableMesh {
            get { return questAvailableMesh; }
            set { questAvailableMesh = value; }
        }
        public string QuestConcludableMesh {
            get { return questConcludableMesh; }
            set { questConcludableMesh = value; }
        }
	}
}
