
//////////////////////////////////////////////////////////////
//
// event id mapping and event handlers mapping
//
//////////////////////////////////////////////////////////////

var evtSvr = Engine.getEventServer();

//
// register the event to event handler mapping
//

evtSvr.registerEventHandler("multiverse.server.events.DirectedEvent",
			    "multiverse.entitymgr.eventhandlers.DirectedHandler");

// behaviors send timer events to 'wake' itself up later
evtSvr.registerEventHandler("multiverse.server.events.TimerEvent",
			    "multiverse.entitymgr.eventhandlers.TimerHandler");

// behaviors start up when their object spawns
evtSvr.registerEventHandler("multiverse.server.events.ObjectSpawnedEvent",
			    "multiverse.entitymgr.eventhandlers.ObjectSpawnedHandler");

evtSvr.registerEventHandler("multiverse.server.events.PortalEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.ComEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

// evtSvr.registerEventHandler("multiverse.server.events.DirLocEvent",
// 			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

// evtSvr.registerEventHandler("multiverse.server.events.DirLocEvent",
// 			    "multiverse.entitymgr.eventhandlers.DirLocHandler");

evtSvr.registerEventHandler("multiverse.server.events.DirLocEvent",
			    "multiverse.server.eventhandlers.NullHandler");

// evtSvr.registerEventHandler("multiverse.server.events.NotifyNewObjectEvent",
// 			    "multiverse.server.eventhandlers.DummyHandler");

// this sends the event to the appropriate mob's behavior
// evtSvr.registerEventHandler("multiverse.server.events.NewObjectEvent",
// 			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

// this creates the entity on the server and also sets up aware list
evtSvr.registerEventHandler("multiverse.server.events.NewObjectEvent",
			    "multiverse.mars.eventhandlers.MarsSpawnNewObject");

evtSvr.registerEventHandler("multiverse.server.events.NotifyFreeObjectEvent",
			    "multiverse.entitymgr.eventhandlers.NotifyFreeObjectHandler");

evtSvr.registerEventHandler("multiverse.server.events.OrientEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.AcquireEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.AcquireResponseEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.EquipEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsEquipResponseEvent",
			    "multiverse.server.eventhandlers.NullHandler");

evtSvr.registerEventHandler("multiverse.server.events.AttachEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.AutoAttackEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.CombatEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsDamageEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.DropEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.DetachEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsUnequipEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.DropResponseEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.NotifyPlayAnimationEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.NotifyPlaySoundEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");


evtSvr.registerEventHandler("multiverse.mars.events.TradeRequestEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.mars.events.TradeAcceptedEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.JScriptEvent",
			    "multiverse.entitymgr.eventhandlers.DefaultHandler");

evtSvr.registerEventHandler("multiverse.server.events.RegisterEntityEvent",
			    "multiverse.server.eventhandlers.RegisterEntityHandler");

evtSvr.registerEventHandler("multiverse.server.events.RegisterEntityResponseEvent",
			    "multiverse.server.eventhandlers.RegisterEntityResponse");

evtSvr.registerEventHandler("multiverse.server.events.UnregisterEntityEvent",
			    "multiverse.server.eventhandlers.UnregisterEntityHandler");

evtSvr.registerEventHandler("multiverse.server.events.UnregisterEntityResponseEvent",
			    "multiverse.server.eventhandlers.UnregisterEntityResponseHandler");

evtSvr.registerEventHandler("multiverse.server.events.ConResetEvent",
			    "multiverse.server.eventhandlers.ConResetHandler");

evtSvr.registerEventHandler("multiverse.server.events.ScriptEvent",
			    "multiverse.entitymgr.eventhandlers.ScriptHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestResponse",
			    "multiverse.mars.eventhandlers.QuestResponseHandler");

evtSvr.registerEventHandler("multiverse.mars.events.ServerRequestQuestInfo",
			    "multiverse.mars.eventhandlers.ServerRequestQuestInfoHandler");


Engine.registerPlugin("multiverse.mars.plugins.MobManagerPlugin");
