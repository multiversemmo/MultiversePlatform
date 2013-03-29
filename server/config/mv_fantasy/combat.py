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

from java.lang import *
from java.util import *
from multiverse.mars import *
from multiverse.mars.core import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
False=0
True=1


class HealthMaxStat (MarsStatDef):
    def update(self, stat, info):
        stam = info.statGetCurrentValue("stamina")
        stat.base = stam * 10
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)

class HealthStat (MarsStatDef):
    def update(self, stat, info):
        healthMax = info.statGetCurrentValue("health-max")
        stat.max = healthMax
        stat.min = 0
        if(info.dead()):
            stat.base = 0
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)

    def notifyFlags(self, stat, info, oldFlags, newFlags):
        if (info.dead()):
            CombatPlugin.stopRegen(info, stat.getName())
            return
        
        if ((oldFlags ^ newFlags) & MarsStatDef.MARS_STAT_FLAG_MAX):
            if (newFlags & MarsStatDef.MARS_STAT_FLAG_MAX):
                CombatPlugin.stopRegen(info, stat.getName())
            else:
                regenEffect = Mars.EffectManager.get("health regen effect")
                CombatPlugin.startRegen(info, stat.getName(), regenEffect)

        if (((oldFlags ^ newFlags) & MarsStatDef.MARS_STAT_FLAG_MIN)
            and (newFlags & MarsStatDef.MARS_STAT_FLAG_MIN)):
            CombatPlugin.stopRegen(info, stat.getName())
            info.setDeadState(True)
            WorldManagerClient.setObjectProperty(info.getOwnerOid(),
                                                 WorldManagerClient.WORLD_PROP_NOMOVE, Boolean(True))
            WorldManagerClient.setObjectProperty(info.getOwnerOid(),
                                                 WorldManagerClient.WORLD_PROP_NOTURN, Boolean(True))
            if (info.isMob()):
                WorldManagerClient.setObjectProperty(info.getOwnerOid(),
                                                     "lootable", Boolean(True))

class ManaMaxStat (MarsStatDef):
    def update(self, stat, info):
        intel = info.statGetCurrentValue("intelligence")
        stat.base = intel * 10
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)

class ManaStat (MarsStatDef):
    def update(self, stat, info):
        manaMax = info.statGetCurrentValue("mana-max")
        stat.max = manaMax
        stat.min = 0
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)

    def notifyFlags(self, stat, info, oldFlags, newFlags):
        if (info.dead()):
            CombatPlugin.stopRegen(info, stat.getName())
            return

        if ((oldFlags ^ newFlags) & MarsStatDef.MARS_STAT_FLAG_MAX):
            if (newFlags & MarsStatDef.MARS_STAT_FLAG_MAX):
                CombatPlugin.stopRegen(info, stat.getName())
            else:
                regenEffect = Mars.EffectManager.get("mana regen effect")
                CombatPlugin.startRegen(info, stat.getName(), regenEffect)

class AccuracyStat (MarsStatDef):
    def update(self, stat, info):
        offense = info.statGetCurrentValue("offense skill")
        stren = info.statGetCurrentValue("strength")
        agil = info.statGetCurrentValue("agility")
        stat.base = offense + stren + agil
        stat.setDirty(True)
        MarsStatDef.update(self, stat, info)

class DefenseStat (MarsStatDef):
    def update(self, stat, info):
        defense = info.statGetCurrentValue("defense skill")
        agil = info.statGetCurrentValue("agility")
        stat.base = defense + agil * 2
        stat.setDirty(True)
        MarsStatDef.update(self, stat, info)

class AttackPowerStat (MarsStatDef):
    def update(self, stat, info):
        stren = info.statGetCurrentValue("strength")
        stat.base = stren * 2
        stat.setDirty(True)
        MarsStatDef.update(self, stat, info)
        
CombatPlugin.registerStat(MarsStatDef("strength"))
CombatPlugin.registerStat(MarsStatDef("intelligence"))
CombatPlugin.registerStat(MarsStatDef("stamina"))
CombatPlugin.registerStat(MarsStatDef("agility"))
CombatPlugin.registerStat(MarsStatDef("offense skill"))
CombatPlugin.registerStat(MarsStatDef("defense skill"))
CombatPlugin.registerStat(MarsStatDef("armor"))
CombatPlugin.registerStat(HealthMaxStat("health-max"), [ "stamina" ])
CombatPlugin.registerStat(HealthStat("health"), [ "health-max" ])
CombatPlugin.registerStat(ManaMaxStat("mana-max"), [ "intelligence" ])
CombatPlugin.registerStat(ManaStat("mana"), [ "mana-max" ])
CombatPlugin.registerStat(AccuracyStat("accuracy"), [ "offense skill", "strength", "agility" ])
CombatPlugin.registerStat(DefenseStat("defense"), [ "defense skill", "agility" ])
CombatPlugin.registerStat(AttackPowerStat("attack power"), [ "strength" ])

Engine.registerPlugin("multiverse.mars.plugins.CombatPlugin");
