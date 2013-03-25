
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
			    "multiverse.mars.eventhandlers.MarsTimerHandler");

evtSvr.registerEventHandler("multiverse.server.events.PortalEvent",
			    "multiverse.server.eventhandlers.PortalHandler");

evtSvr.registerEventHandler("multiverse.server.events.ComEvent",
			    "multiverse.server.eventhandlers.ComHandler");

evtSvr.registerEventHandler("multiverse.server.events.DirLocEvent",
			    "multiverse.mars.eventhandlers.MarsDirLocHandler");

// see proxy
// evtSvr.registerEventHandler("multiverse.server.events.LoginEvent",
// 			    "multiverse.server.eventhandlers.LoginHandler");

evtSvr.registerEventHandler("multiverse.server.events.LogoutEvent",
			    "multiverse.server.eventhandlers.LogoutHandler");

// this one queues the newobjectevent plus animation and sound stuff
evtSvr.registerEventHandler("multiverse.server.events.NotifyNewObjectEvent",
			    "multiverse.mars.eventhandlers.MarsNewObjectHandler");

// this one sends details about a new object
evtSvr.registerEventHandler("multiverse.server.events.NewObjectEvent",
			    "multiverse.server.eventhandlers.NewObjectHandler");

evtSvr.registerEventHandler("multiverse.server.events.OrientEvent",
			    "multiverse.server.eventhandlers.OrientHandler");

// this just sends the freeobject event to the notify subj
evtSvr.registerEventHandler("multiverse.server.events.NotifyFreeObjectEvent",
			    "multiverse.server.eventhandlers.NotifyFreeObjectHandler");

evtSvr.registerEventHandler("multiverse.server.events.AcquireEvent",
			    "multiverse.mars.eventhandlers.MarsAcquireHandler");

evtSvr.registerEventHandler("multiverse.server.events.CommandEvent",
			    "multiverse.mars.eventhandlers.MarsCommandHandler");

evtSvr.registerEventHandler("multiverse.mars.events.EquipEvent",
			    "multiverse.mars.eventhandlers.MarsEquipHandler");

// evtSvr.registerEventHandler("multiverse.mars.events.MarsEquipResponseEvent",
// 			    "multiverse.server.eventhandlers.EquipResponseHandler");

evtSvr.registerEventHandler("multiverse.server.events.AttachEvent",
			    "multiverse.server.eventhandlers.AttachHandler");

evtSvr.registerEventHandler("multiverse.server.events.AutoAttackEvent",
			    "multiverse.mars.eventhandlers.MarsAutoAttackHandler");

evtSvr.registerEventHandler("multiverse.mars.events.CombatEvent",
			    "multiverse.mars.eventhandlers.CombatHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsDamageEvent",
			    "multiverse.mars.eventhandlers.MarsDamageHandler");

evtSvr.registerEventHandler("multiverse.server.events.DropEvent",
			    "multiverse.mars.eventhandlers.DropHandler");

evtSvr.registerEventHandler("multiverse.server.events.DetachEvent",
			    "multiverse.server.eventhandlers.DetachHandler");

evtSvr.registerEventHandler("multiverse.mars.events.MarsUnequipEvent",
			    "multiverse.mars.eventhandlers.MarsUnequipHandler");

evtSvr.registerEventHandler("multiverse.mars.events.DropResponseEvent",
			    "multiverse.mars.eventhandlers.DropResponseHandler");

evtSvr.registerEventHandler("multiverse.server.events.NotifyPlayAnimationEvent",
			    "multiverse.server.eventhandlers.NotifyPlayAnimationHandler");

evtSvr.registerEventHandler("multiverse.server.events.NotifyPlaySoundEvent",
			    "multiverse.server.eventhandlers.NotifyPlaySoundHandler");

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

evtSvr.registerEventHandler("multiverse.mars.events.TradeRequestEvent",
			    "multiverse.mars.eventhandlers.TradeRequestHandler");

evtSvr.registerEventHandler("multiverse.mars.events.TradeAcceptedEvent",
			    "multiverse.mars.eventhandlers.TradeAcceptedHandler");

evtSvr.registerEventHandler("multiverse.server.events.JScriptEvent",
			    "multiverse.server.eventhandlers.JScriptHandler");

evtSvr.registerEventHandler("multiverse.mars.events.StatusUpdateEvent",
			    "multiverse.mars.eventhandlers.StatusUpdateHandler");

evtSvr.registerEventHandler("multiverse.server.events.SaveEvent",
			    "multiverse.server.eventhandlers.SaveHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestAvailableEvent",
			    "multiverse.mars.eventhandlers.QuestAvailableHandler");

evtSvr.registerEventHandler("multiverse.mars.events.RequestQuestInfo",
			    "multiverse.mars.eventhandlers.RequestQuestInfoHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestInfo",
			    "multiverse.mars.eventhandlers.QuestInfoHandler");

evtSvr.registerEventHandler("multiverse.mars.events.QuestResponse",
			    "multiverse.mars.eventhandlers.QuestResponseHandler");

evtSvr.registerEventHandler("multiverse.mars.events.NewQuestStateEvent",
			    "multiverse.mars.eventhandlers.NewQuestStateHandler");

evtSvr.registerEventHandler("multiverse.mars.events.ConcludeQuest",
			    "multiverse.mars.eventhandlers.ConcludeQuestHandler");

Engine.registerPlugin("multiverse.mars.plugins.MarsWorldManagerPlugin");
