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
from multiverse.mars.core import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.mars.behaviors import *
from multiverse.msgsys import *
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from java.lang import *


class WolfFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

	wolfLoc = InstanceClient.getMarkerPoint(instanceOid, "wolfmarker")
        # add behavior
        behav = RadiusRoamBehavior()
        behav.setCenterLoc(wolfLoc)
        behav.setRadius(20000)
        obj.addBehavior(BaseBehavior())
        obj.addBehavior(behav)
        obj.addBehavior(CombatBehavior())
        
        return obj

ObjectFactory.register("WolfFactory", WolfFactory("Wolf"))

class ZombieFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
	obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
	zombieLoc = InstanceClient.getMarkerPoint(instanceOid, "zombiemarker")
	behav = PatrolBehavior()
	behav.addWaypoint(zombieLoc)
	behav.addWaypoint(loc)
	obj.addBehavior(BaseBehavior())
	obj.addBehavior(behav)
	obj.addBehavior(CombatBehavior())
	return obj

ObjectFactory.register("ZombieFactory", ZombieFactory("Zombie"))

welcomeQuest = MarsCollectionQuest()
welcomeQuest.setName("Welcome Ashore")
welcomeQuest.setDesc("Welcome ashore! Talk to my twin, she has a job for you.")
welcomeQuest.setObjective("Talk to the other npc")
welcomeQuest.addReward("Bronze Longsword")

collectQuest = MarsCollectionQuest()
collectQuest.setName("Get some skins")
collectQuest.setDesc("Kill a wolf and bring me the skin and bones")
collectQuest.setObjective("Collect 1 Wolf Skin and 1 Wolf Bones")
collectQuest.addQuestPrereq("Welcome Ashore")
collectQuest.addCollectionGoal(MarsCollectionQuest.CollectionGoal("Wolf Skin", 1))
collectQuest.addCollectionGoal(MarsCollectionQuest.CollectionGoal("Wolf Bones", 1))
collectQuest.addReward("Leather Boots")

wolfQuest = MarsKillQuest()
wolfQuest.setName("Kill The Wolves")
wolfQuest.setDesc("Please kill 2 wolves for me.")
wolfQuest.setObjective("Kill 2 Wolves")
wolfQuest.setCashReward(1000)
wolfQuest.setKillGoal(MarsKillQuest.KillGoal("Wolf", 2))
wolfQuest.addReward("Leather Tunic")

class Npc1Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
	obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
	behav = QuestBehavior()
	behav.startsQuest(welcomeQuest)
	obj.addBehavior(behav)
	return obj

ObjectFactory.register("Npc1Factory", Npc1Factory("Human Female Leather"))


class Npc2Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
	obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
	behav = QuestBehavior()
	behav.endsQuest(welcomeQuest)
	behav.startsQuest(collectQuest)
	behav.endsQuest(collectQuest)
	obj.addBehavior(behav)
	return obj

ObjectFactory.register("Npc2Factory", Npc2Factory("Human Female Leather"))

class SoldierTrainerFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
        return obj

ObjectFactory.register("SoldierTrainerFactory", SoldierTrainerFactory("Human Female Trainer"))

Log.debug("done with mobserver.py")
