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

# Derived stats do not have a min/max since they are based off of other values
class HealthMaxStat (MarsStatDef):
    def update(self, stat, info):
        strength = info.statGetCurrentValue("strength")
        stat.base = strength * 2
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)    

class HealthStat (MarsStatDef):
    
    def update(self, stat, info):
        # Health is derived from Strength at a rate of 2 X the value. 
        healthMax = info.statGetCurrentValue("health-max")
        stat.max = healthMax
        stat.min = 0
        if(info.dead()):
            stat.base = 0
        stat.setDirty(True)
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
            
            EnginePlugin.setObjectPropertyNoResponse(info.getOwnerOid(), Namespace.WORLD_MANAGER, 
                                                 WorldManagerClient.WORLD_PROP_NOMOVE, Boolean(True))
            EnginePlugin.setObjectPropertyNoResponse(info.getOwnerOid(), Namespace.WORLD_MANAGER, 
                                                 WorldManagerClient.WORLD_PROP_NOTURN, Boolean(True))

            
            if (info.isMob()):
                WorldManagerClient.setObjectProperty(info.getOwnerOid(),
                                                     "lootable", Boolean(True))

class StaminaMaxStat (MarsStatDef):
    def update(self, stat, info):
        intel = info.statGetCurrentValue("strength")
        stat.base = intel * 1.5
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)

class StaminaStat (MarsStatDef):
    def update(self, stat, info):
        # Stamina is derived from Strength at a rate of 2 X the value. 
        StaminaMax = info.statGetCurrentValue("stamina-max")          
        stat.max = int(StaminaMax)
        stat.min = 0
        if(info.dead()):
            stat.base = 0
        stat.setDirty(True)
        MarsStatDef.update(self, stat, info)
        
    def notifyFlags(self, stat, info, oldFlags, newFlags):
        if (info.dead()):
            CombatPlugin.stopRegen(info, stat.getName())
            return

        if ((oldFlags ^ newFlags) & MarsStatDef.MARS_STAT_FLAG_MAX):
            if (newFlags & MarsStatDef.MARS_STAT_FLAG_MAX):
                CombatPlugin.stopRegen(info, stat.getName())
            else:
                regenEffect = Mars.EffectManager.get("stamina regen effect")
                CombatPlugin.startRegen(info, stat.getName(), regenEffect)

class ManaMaxStat (MarsStatDef):
    def update(self, stat, info):
        intel = info.statGetCurrentValue("intelligence")
        stat.base = intel * 2
        stat.setDirty(True);
        MarsStatDef.update(self, stat, info)

class ManaStat (MarsStatDef):
    def update(self, stat, info):
        # Mana is derived from Intelligence at a rate of 2 X the value. 

        ManaMax = info.statGetCurrentValue("mana-max")           
        stat.max = int(ManaMax)
        stat.min = 0
        if(info.dead()):
            stat.base = 0
        stat.setDirty(True)
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

class LevelStat (MarsStatDef):
    def update(self, stat, info):
        expstat = info.getProperty("experience");
        
        if expstat.base >= expstat.max and stat.max != stat.base: 
            if stat.base == None:
                stat.base = stat.current = 0
            
            # they have gained a level
            stat.base = stat.current = stat.base + 1
            
            # NOTE: This line determines what the next level should be. Put your level incrementation
            #       logic here. Right now it just takes the current level and multiplies by 10 and sets
            #       that as the value for the max experience to be gained to level again.
            #
            #       ie: if current level = 1, then it will take 1 * 100, or 100 experience points to level
            #           if the current level is 2, then 200, etc.
            expstat.max = stat.base * 100
                        
            expstat.setDirty(True)
            stat.setDirty(True)
            
            ClassAbilityClient.sendXPUpdate(info.getOid(), stat.getName(), stat.current)
            ClassAbilityPlugin.handleLevelingPlayer(info, stat.base)
            
        
        MarsStatDef.update(self, stat, info)
        

# Class for handling ClassAbility stat management
class ClassAbilityRankStat (MarsStatDef):
    def update(self, stat, info):    
        statname, statext = stat.getName().split("_")
        
        if statext is 'rank':
          MarsStatDef.update(self, stat, info)
        
        rankstat = info.getProperty(statname + "_rank")
        
        # this will check to see if the stat is >= to the max, if so, increment the rank
        if rankstat.max > rankstat.base and stat.current >= stat.max:
            #reset this stat and update the rank
            stat.base = stat.current = (0 + stat.current - stat.max)       
            
            if rankstat.max > rankstat.base:
               # bring the stat down to the base
               if rankstat.base > 0: 
                   stat.max = stat.max / rankstat.base
               
               #increase the rank of the skill/ability
               rankstat.base = rankstat.current = rankstat.base + 1
               rankstat.setDirty(True)
               
               # for now send a message to the client showing the rank has increased
               ClassAbilityClient.sendXPUpdate(info.getOid(), rankstat.getName(), rankstat.base)
               ClassAbilityPlugin.sendSkillUpdate(CombatPlugin.getCombatInfo(info.getOid()))
               
               # If this is a skill check to see if the new skill level has unlocked any new abilities
               if Mars.SkillManager.get(statname) != None:
                   ClassAbilityClient.CheckSkillAbilities(info.getOid(), statname, rankstat.base)
               
               # finally increase the max for the stat
               ClassAbilityPlugin.handleSkillAbilityRanking(info, statname, rankstat.base)
               
        MarsStatDef.update(self, stat, info)
        
# grab the death property message

# Register                Class               Stat                Dependencies
CombatPlugin.registerStat(MarsStatDef(        "strength"))
CombatPlugin.registerStat(MarsStatDef(        "dexterity"))
CombatPlugin.registerStat(MarsStatDef(        "wisdom"))
CombatPlugin.registerStat(MarsStatDef(        "intelligence"))
CombatPlugin.registerStat(HealthMaxStat(      "health-max"),      ["strength"])
CombatPlugin.registerStat(MarsStatDef(        "stamina-max"),     ["strength"])
CombatPlugin.registerStat(ManaMaxStat(        "mana-max"),        ["intelligence"])
CombatPlugin.registerStat(StaminaStat(        "stamina"),         ["stamina-max"])
CombatPlugin.registerStat(HealthStat("health"), [ "health-max" ])
CombatPlugin.registerStat(ManaStat("mana"), [ "mana-max" ])
CombatPlugin.registerStat(MarsStatDef(        "experience"))
CombatPlugin.registerStat(LevelStat(          "level"),           ["experience"])

# Skills and Abilities also can gain experience, so we need to setup the catch for their Ranks

# SKILLS
ClassAbilityPlugin.registerStat(MarsStatDef("Sword_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Axe_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Dagger_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Thrown Weapons_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("First Aid_exp"))
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Sword_rank"), [ "Sword_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Axe_rank"), [ "Axe_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Dagger_rank"), [ "Dagger_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Thrown Weapons_rank"), [ "Thrown Weapons_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("First Aid_rank"), [ "First Aid_exp" ])

# ABILITIES
ClassAbilityPlugin.registerStat(MarsStatDef("Wounding Thrust_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Whirling Dervish_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Cleave_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Pierce_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Flying Dagger_exp"))
ClassAbilityPlugin.registerStat(MarsStatDef("Lesser Bandages_exp"))
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Wounding Thrust_rank"), [ "Wounding Thrust_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Whirling Dervish_rank"), [ "Whirling Dervish_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Cleave_rank"), [ "Cleave_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Pierce_rank"), [ "Pierce_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Flying Dagger_rank"), [ "Flying Dagger_exp" ])
ClassAbilityPlugin.registerStat(ClassAbilityRankStat("Lesser Bandages_rank"), [ "Lesser Bandages_exp" ])

Engine.registerPlugin("multiverse.mars.plugins.CombatPlugin");
