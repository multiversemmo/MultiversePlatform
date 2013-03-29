#
#  The Multiverse Platform is made available under the MIT License.
#
#  Copyright (c) 2012 The Multiverse Foundation
#
#  Permission is hereby granted, free of charge, to any person 
#  obtaining a copy of this software and associated documentation 
#  files (the "Software"), to deal in the Software without restriction, 
#  including without limitation the rights to use, copy, modify, 
#  merge, publish, distribute, sublicense, and/or sell copies 
#  of the Software, and to permit persons to whom the Software 
#  is furnished to do so, subject to the following conditions:
#
#  The above copyright notice and this permission notice shall be 
#  included in all copies or substantial portions of the Software.
# 
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
#  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
#  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
#  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
#  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
#  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
#  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
#  OR OTHER DEALINGS IN THE SOFTWARE.
#
#

from multiverse.mars import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.server.math import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *

evtSvr = Engine.getEventServer()

# register the id numbers to event mapping
evtSvr.registerEventId(1, "multiverse.server.events.LoginEvent")
#evtSvr.registerEventId(2, "multiverse.server.events.DirLocEvent")
evtSvr.registerEventId(3, "multiverse.server.events.ComEvent")
evtSvr.registerEventId(4, "multiverse.server.events.LoginResponseEvent")
evtSvr.registerEventId(5, "multiverse.server.events.LogoutEvent")
evtSvr.registerEventId(6, "multiverse.server.events.TerrainEvent")
evtSvr.registerEventId(7, "multiverse.server.events.SkyboxEvent")
evtSvr.registerEventId(8, "multiverse.server.events.NewObjectEvent")
#evtSvr.registerEventId(9, "multiverse.server.events.OrientEvent")
evtSvr.registerEventId(10, "multiverse.server.events.NotifyFreeObjectEvent")
evtSvr.registerEventId(11, "multiverse.server.events.AcquireEvent")
evtSvr.registerEventId(12, "multiverse.server.events.AcquireResponseEvent")
evtSvr.registerEventId(13, "multiverse.server.events.CommandEvent")
evtSvr.registerEventId(14, "multiverse.mars.events.EquipEvent")
evtSvr.registerEventId(15, "multiverse.mars.events.MarsEquipResponseEvent")
evtSvr.registerEventId(16, "multiverse.mars.events.MarsUnequipEvent")
#evtSvr.registerEventId(17, "multiverse.server.events.UnequipResponseEvent")
evtSvr.registerEventId(18, "multiverse.server.events.AttachEvent")
evtSvr.registerEventId(19, "multiverse.server.events.DetachEvent")
evtSvr.registerEventId(20, "multiverse.mars.events.CombatEvent")
evtSvr.registerEventId(21, "multiverse.server.events.AutoAttackEvent")
evtSvr.registerEventId(22, "multiverse.mars.events.StatusUpdateEvent")
evtSvr.registerEventId(23, "multiverse.mars.events.MarsDamageEvent")
evtSvr.registerEventId(24, "multiverse.server.events.DropEvent")
evtSvr.registerEventId(25, "multiverse.mars.events.DropResponseEvent")
evtSvr.registerEventId(26, "multiverse.server.events.NotifyPlayAnimationEvent")
#evtSvr.registerEventId(27, "multiverse.server.events.NotifyPlaySoundEvent")
#evtSvr.registerEventId(28, "multiverse.server.events.AmbientSoundEvent")
#evtSvr.registerEventId(29, "multiverse.server.events.FollowTerrainEvent")
evtSvr.registerEventId(30, "multiverse.server.events.PortalEvent")
evtSvr.registerEventId(31, "multiverse.server.events.AmbientLightEvent")
evtSvr.registerEventId(32, "multiverse.server.events.NewLightEvent")
#evtSvr.registerEventId(33, "multiverse.mars.events.TradeStartReqEvent")
#evtSvr.registerEventId(34, "multiverse.mars.events.TradeStartEvent")
#evtSvr.registerEventId(35, "multiverse.mars.events.TradeOfferReqEvent")
#evtSvr.registerEventId(36, "multiverse.mars.events.TradeCompleteEvent")
#evtSvr.registerEventId(37, "multiverse.mars.events.TradeOfferUpdateEvent")
evtSvr.registerEventId(38, "multiverse.mars.events.MarsStateEvent")
evtSvr.registerEventId(39, "multiverse.mars.events.RequestQuestInfo")
evtSvr.registerEventId(40, "multiverse.mars.events.QuestInfo")
evtSvr.registerEventId(41, "multiverse.mars.events.QuestResponse")
evtSvr.registerEventId(42, "multiverse.server.events.RegionConfiguration")
#evtSvr.registerEventId(43, "multiverse.mars.events.InventoryUpdate")
evtSvr.registerEventId(44, "multiverse.mars.events.QuestLogInfo")
#evtSvr.registerEventId(45, "multiverse.mars.events.QuestStateInfo")
evtSvr.registerEventId(47, "multiverse.mars.events.RemoveQuestResponse")
evtSvr.registerEventId(49, "multiverse.mars.events.ConcludeQuest")
evtSvr.registerEventId(50, "multiverse.server.events.UITheme")
#evtSvr.registerEventId(52, "multiverse.server.events.ModelInfoEvent")
evtSvr.registerEventId(53, "multiverse.server.events.FragmentedMessage")
evtSvr.registerEventId(54, "multiverse.server.events.RoadEvent")
#evtSvr.registerEventId(55, "multiverse.server.events.FogEvent")
evtSvr.registerEventId(55, "multiverse.server.plugins.WorldManagerClient$FogMessage")
evtSvr.registerEventId(56, "multiverse.mars.events.AbilityUpdateEvent")
evtSvr.registerEventId(57, "multiverse.mars.events.AbilityInfoEvent")
evtSvr.registerEventId(58, "multiverse.mars.events.CooldownEvent")
evtSvr.registerEventId(59, "multiverse.mars.events.AbilityActivateEvent")
evtSvr.registerEventId(60, "multiverse.mars.events.AbilityProgressEvent")
evtSvr.registerEventId(72, "multiverse.server.events.ActivateItemEvent")
evtSvr.registerEventId(75, "multiverse.server.events.NewTerrainDecalEvent")
evtSvr.registerEventId(76, "multiverse.server.events.FreeTerrainDecalEvent")
evtSvr.registerEventId(77, "multiverse.server.events.ModelInfoEvent")
evtSvr.registerEventId(79, "multiverse.server.events.DirLocOrientEvent")
evtSvr.registerEventId(80, "multiverse.server.events.AuthorizedLoginEvent")
evtSvr.registerEventId(81, "multiverse.server.events.AuthorizedLoginResponseEvent")
evtSvr.registerEventId(82, "multiverse.server.events.LoadingStateEvent")
evtSvr.registerEventId(83, "multiverse.server.events.ExtensionMessageEvent")

#evtSvr.registerEventId(1024, "multiverse.server.events.ServerEvent")
evtSvr.registerEventId(1025, "multiverse.server.events.RegisterEntityEvent")
evtSvr.registerEventId(1026, "multiverse.server.events.RegisterEntityResponseEvent")
evtSvr.registerEventId(1027, "multiverse.server.events.ConResetEvent")
evtSvr.registerEventId(1028, "multiverse.server.events.NotifyNewObjectEvent")
evtSvr.registerEventId(1029, "multiverse.server.events.ScriptEvent")
evtSvr.registerEventId(1030, "multiverse.server.events.DirectedEvent")
evtSvr.registerEventId(1031, "multiverse.server.events.SaveEvent")
evtSvr.registerEventId(1032, "multiverse.mars.events.QuestAvailableEvent")
evtSvr.registerEventId(1033, "multiverse.mars.events.NewQuestStateEvent")
#evtSvr.registerEventId(1034, "multiverse.mars.events.ServerRequestQuestInfo")
evtSvr.registerEventId(1035, "multiverse.mars.events.QuestCompleted")
evtSvr.registerEventId(1036, "multiverse.server.events.UnregisterEntityEvent")
evtSvr.registerEventId(1037, "multiverse.server.events.UnregisterEntityResponseEvent")

