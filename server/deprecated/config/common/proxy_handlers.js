
//////////////////////////////////////////////////////////////
//
// event id mapping and event handlers mapping
//
//////////////////////////////////////////////////////////////

var evtSvr = Engine.getEventServer();

//
// register the event to event handler mapping
//
evtSvr.registerEventHandler("multiverse.server.events.TimerEvent",
			    "multiverse.server.eventhandlers.ProxyTimerHandler");

evtSvr.registerEventHandler("multiverse.server.events.PortalEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.ComEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.TerrainEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.SkyboxEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.AmbientLightEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.DirLocEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.AmbientSoundEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

// this one sends details about a new object
//evtSvr.registerEventHandler("multiverse.server.events.NewObjectEvent",
//			    "multiverse.mars.eventhandlers.MarsSpawnNewObject");

evtSvr.registerEventHandler("multiverse.server.events.NewObjectEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

// this just sends the freeobject event to the notify subj
evtSvr.registerEventHandler("multiverse.server.events.NotifyFreeObjectEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.OrientEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.AcquireEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.AcquireResponseEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.CommandEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.EquipEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsEquipResponseEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.AttachEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.AutoAttackEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.CombatEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsDamageEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.DropEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.DetachEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsUnequipEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.DropResponseEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.NotifyPlayAnimationEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.NotifyPlaySoundEvent",
			    "multiverse.server.eventhandlers.DummyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.TradeRequestEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.TradeAcceptedEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.JScriptEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.StatusUpdateEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.NewLightEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsStateEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestInfo",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.RequestQuestInfo",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestResponse",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.RegionConfiguration",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.InventoryUpdate",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestLogInfo",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestStateInfo",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.GroupInfo",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.UITheme",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.ConcludeQuest",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsModelInfoEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.FragmentedMessage",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.RoadEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.FogEvent",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.mars.events.RemoveQuestResponse",
			    "multiverse.server.eventhandlers.ProxyHandler");

evtSvr.registerEventHandler("multiverse.server.events.ConResetEvent",
			    "multiverse.server.eventhandlers.ConResetHandler");

// special events

evtSvr.registerEventHandler("multiverse.server.events.LoginEvent",
			    "multiverse.server.eventhandlers.LoginHandler");

evtSvr.registerEventHandler("multiverse.server.events.RegisterEntityResponseEvent",
			    "multiverse.server.eventhandlers.RegisterEntityResponse");

