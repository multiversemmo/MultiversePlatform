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
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *

class Tele1Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        # this binds the object
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

        tele1Dest = InstanceClient.getMarkerPoint(instanceOid, "swamp_tele_dest")
        # add behavior
        behav = TeleporterBehavior()
        behav.setRadius(4000)
        behav.setDestination(tele1Dest)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("Tele1Factory", Tele1Factory("Teleporter"))


class Tele2Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        # this binds the object
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

        tele2Dest = InstanceClient.getMarkerPoint(instanceOid, "village_tele_dest")
        # add behavior
        behav = TeleporterBehavior()
        behav.setRadius(4000)
        behav.setDestination(tele2Dest)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("Tele2Factory", Tele2Factory("Teleporter"))



class WolfFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        # this binds the object
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

        wolfLoc = InstanceClient.getMarkerPoint(instanceOid, "village_mob_1")
        wolfLoc.setY(0)
        # add behavior
        behav = RadiusRoamBehavior()
        behav.setCenterLoc(wolfLoc)
        behav.setRadius(80000)
        obj.addBehavior(BaseBehavior())
        obj.addBehavior(behav)
        behav = CombatBehavior()
        behav.setAggressive(0)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("WolfFactory", WolfFactory("Wolf"))



class ZombieFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        # this binds the object
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

        zombieLoc = InstanceClient.getMarkerPoint(instanceOid, "village_mob_2")
        zombieLoc.setY(0)
        # add behavior
        behav = RadiusRoamBehavior()
        behav.setCenterLoc(zombieLoc)
        behav.setRadius(10000)
        obj.addBehavior(BaseBehavior())
        obj.addBehavior(behav)
        behav = CombatBehavior()
        behav.setAggressive(1)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("ZombieFactory", ZombieFactory("Zombie"))



class CrocFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        # this binds the object
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

        crocLoc = InstanceClient.getMarkerPoint(instanceOid, "swamp_mob_2")
        crocLoc.setY(0)
        # add behavior
        behav = RadiusRoamBehavior()
        behav.setCenterLoc(crocLoc)
        behav.setRadius(20000)
        behav.setMovementSpeed(1000)
        obj.addBehavior(BaseBehavior())
        obj.addBehavior(behav)
        behav = CombatBehavior()
        behav.setAggressive(0)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("CrocFactory", CrocFactory("Crocodile"))


class BraxFactory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        # this binds the object
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)

        braxLoc = InstanceClient.getMarkerPoint(instanceOid, "swamp_mob_1")
        braxLoc.setY(0)
        # add behavior
        behav = RadiusRoamBehavior()
        behav.setCenterLoc(braxLoc)
        behav.setRadius(30000)
        behav.setMovementSpeed(1000)
        obj.addBehavior(BaseBehavior())
        obj.addBehavior(behav)
        behav = CombatBehavior()
        behav.setAggressive(0)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("BraxFactory", BraxFactory("Brax"))




welcomeQuest = MarsCollectionQuest()
welcomeQuest.setName("Welcome to Town")
welcomeQuest.setDesc("You're new here, aren't you? If you're looking for work, you should go see Constable Dillon. She'll have something for you to do.")
welcomeQuest.setObjective("Speak to Constable Dillon")

killWolfQuest = MarsCollectionQuest()
killWolfQuest.setName("Kill a Wolf")
killWolfQuest.setDesc("You don't look too impressive, but maybe I can put you to use. Here, take this sword and go kill a wolf. If you can bring me the pelt, I'll know you're not completely hopeless.")
killWolfQuest.setObjective("Bring back one Wolf Skin")
killWolfQuest.addQuestPrereq("Welcome to Town")
killWolfQuest.addDeliveryItem("sword4")
killWolfQuest.addCollectionGoal(MarsCollectionQuest.CollectionGoal("Wolf Skin", 1))

fireballQuest = MarsCollectionQuest()
fireballQuest.setName("A Little Magic")
fireballQuest.setDesc("There's a zombie haunting the ruins down the ridge. If you kill it and bring me any Zombie Dust you find, I'll let you have this book I found.")
fireballQuest.setObjective("Bring a Zombie Dust to Cyrus Blackfire")
fireballQuest.addReward("Tome of Fireball")
fireballQuest.addCollectionGoal(MarsCollectionQuest.CollectionGoal("Zombie Dust", 1))

class npc1Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
        behav = QuestBehavior()
        behav.startsQuest(welcomeQuest)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("npc1Factory", npc1Factory("Hilldale Scout"))



class npc2Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
        behav = QuestBehavior()
        behav.endsQuest(welcomeQuest)
        behav.startsQuest(killWolfQuest)
        behav.endsQuest(killWolfQuest)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("npc2Factory", npc2Factory("Constable Dillon"))



class npc3Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, loc)
        behav = QuestBehavior()
        behav.startsQuest(fireballQuest)
        behav.endsQuest(fireballQuest)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("npc3Factory", npc3Factory("Cyrus Blackfire"))



Log.debug("done with mobserver.py")

