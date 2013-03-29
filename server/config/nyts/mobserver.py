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
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from java.util import Random
from java.util.concurrent import TimeUnit

#//////////////////////////////////////////////////////////////////
#//
#// standard npc behavior
#//
#//////////////////////////////////////////////////////////////////

class DelayedSpawnGenerator(SpawnGenerator):
    def activate(self):
        if (self.numSpawns > 0):
            self.numSpawns = self.numSpawns - 1
            self.spawnObject()
            if (self.numSpawns > 0):
                Engine.getExecutor().schedule(self, self.respawnTime, TimeUnit.MILLISECONDS)
    def run(self):
        self.activate()

MobManagerPlugin.registerSpawnGeneratorClass("DelayedSpawnGenerator",
	DelayedSpawnGenerator)

pedDC = []
pedDC.append(DisplayContext("casual06_f_mediumpoly.mesh", True))
pedDC[0].addSubmesh(DisplayContext.Submesh("casual06_f_mediumpoly-mesh.0", "casual06_f_mediumpoly.body"))
pedDC[0].addSubmesh(DisplayContext.Submesh("casual06_f_mediumpoly-mesh.1", "casual06_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("casual07_f_mediumpoly.mesh", True))
pedDC[1].addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.0", "casual07_f_mediumpoly.body"))
pedDC[1].addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.1", "casual07_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("casual13_f_mediumpoly.mesh", True))
pedDC[2].addSubmesh(DisplayContext.Submesh("casual13_f_mediumpoly-mesh.0", "casual13_f_mediumpoly.body"))
pedDC[2].addSubmesh(DisplayContext.Submesh("casual13_f_mediumpoly-mesh.1", "casual13_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("casual15_f_mediumpoly.mesh", True))
pedDC[3].addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.0", "casual15_f_mediumpoly.body"))
pedDC[3].addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.1", "casual15_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("casual19_f_mediumpoly.mesh", True))
pedDC[4].addSubmesh(DisplayContext.Submesh("casual19_f_mediumpoly-mesh.0", "casual19_f_mediumpoly.body"))
pedDC[4].addSubmesh(DisplayContext.Submesh("casual19_f_mediumpoly-mesh.1", "casual19_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("casual21_f_mediumpoly.mesh", True))
pedDC[5].addSubmesh(DisplayContext.Submesh("casual21_f_mediumpoly-mesh.0", "casual21_f_mediumpoly.body"))
pedDC[5].addSubmesh(DisplayContext.Submesh("casual21_f_mediumpoly-mesh.1", "casual21_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("business04_f_mediumpoly.mesh", True))
pedDC[6].addSubmesh(DisplayContext.Submesh("business04_mediumpoly-mesh.0", "business04_f_mediumpoly.body"))
pedDC[6].addSubmesh(DisplayContext.Submesh("business04_mediumpoly-mesh.1", "business04_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("sportive01_f_mediumpoly.mesh", True))
pedDC[7].addSubmesh(DisplayContext.Submesh("sportive01_f_mediumpoly-mesh.0", "sportive01_f_mediumpoly.body"))
pedDC[7].addSubmesh(DisplayContext.Submesh("sportive01_f_mediumpoly-mesh.1", "sportive01_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("sportive02_f_mediumpoly.mesh", True))
pedDC[8].addSubmesh(DisplayContext.Submesh("sportive02_f_mediumpoly-mesh.0", "sportive02_f_mediumpoly.body"))
pedDC[8].addSubmesh(DisplayContext.Submesh("sportive02_f_mediumpoly-mesh.1", "sportive02_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("sportive05_f_mediumpoly.mesh", True))
pedDC[9].addSubmesh(DisplayContext.Submesh("sportive05_f_mediumpoly-mesh.0", "sportive05_f_mediumpoly.body"))
pedDC[9].addSubmesh(DisplayContext.Submesh("sportive05_f_mediumpoly-mesh.1", "sportive05_f_mediumpoly.hair_transparent"))
pedDC.append(DisplayContext("sportive07_f_mediumpoly.mesh", True))
pedDC[10].addSubmesh(DisplayContext.Submesh("sportive07_f_mediumpoly-mesh.0", "sportive07_f_mediumpoly.body"))
pedDC.append(DisplayContext("casual03_m_mediumpoly.mesh", True))
pedDC[11].addSubmesh(DisplayContext.Submesh("casual03_m_medium-mesh.0", "casual03_m_mediumpoly.body"))
pedDC.append(DisplayContext("casual04_m_mediumpoly.mesh", True))
pedDC[12].addSubmesh(DisplayContext.Submesh("casual04_m_mediumpoly-mesh.0", "casual04_m_mediumpoly.body"))
pedDC.append(DisplayContext("casual07_m_mediumpoly.mesh", True))
pedDC[13].addSubmesh(DisplayContext.Submesh("casual07_m_mediumpoly-mesh.0", "casual07_m_mediumpoly.body"))
pedDC.append(DisplayContext("casual10_m_mediumpoly.mesh", True))
pedDC[14].addSubmesh(DisplayContext.Submesh("casual10_m_mediumpoly-mesh.0", "casual10_m_mediumpoly.body"))
pedDC.append(DisplayContext("casual16_m_mediumpoly.mesh", True))
pedDC[15].addSubmesh(DisplayContext.Submesh("casual16_m_mediumpoly-mesh.0", "casual16_m_mediumpoly.body"))
pedDC.append(DisplayContext("casual21_m_mediumpoly.mesh", True))
pedDC[16].addSubmesh(DisplayContext.Submesh("casual21_m_mediumpoly-mesh.0", "casual21_m_mediumpoly.body"))
pedDC.append(DisplayContext("business03_m_mediumpoly.mesh", True))
pedDC[17].addSubmesh(DisplayContext.Submesh("business03_m_medium-mesh.0", "business03_m_mediumpoly.body"))
pedDC.append(DisplayContext("business05_m_mediumpoly.mesh", True))
pedDC[18].addSubmesh(DisplayContext.Submesh("business05_m_mediumpoly-mesh.0", "business05_m_mediumpoly.body"))
pedDC.append(DisplayContext("sportive01_m_mediumpoly.mesh", True))
pedDC[19].addSubmesh(DisplayContext.Submesh("sportive01_m_mediumpoly-mesh.0", "sportive01_m_mediumpoly.body"))
pedDC.append(DisplayContext("sportive09_m_mediumpoly.mesh", True))
pedDC[20].addSubmesh(DisplayContext.Submesh("sportive09_m_mediumpoly-mesh.0", "sportive09_m_mediumpoly.body"))


class AdvancedPatrolBehavior(PatrolBehavior):
    def __init__(self,mobPt):
        PatrolBehavior.__init__(self)
        self.nextWaypoint = 0
        self.path = []
        self.path.append(mobPt[0])
        self.path.append(mobPt[4])
        self.path.append(mobPt[5])
        self.path.append(mobPt[6])
        self.path.append(mobPt[3])
        self.path.append(mobPt[2])
        self.path.append(mobPt[1])
        self.path.append(mobPt[0])
        self.path.append(mobPt[4])
        self.path.append(mobPt[3])
        self.path.append(mobPt[2])
        self.path.append(mobPt[8])
        self.path.append(mobPt[7])
        self.path.append(mobPt[6])
        self.path.append(mobPt[5])
        self.path.append(mobPt[4])
        self.path.append(mobPt[3])
        self.path.append(mobPt[2])
        self.path.append(mobPt[1])
        self.listSize = len(self.path)
    def startPatrol(self):
        self.nextPatrol()
    def nextPatrol(self):
        self.nextPoint = self.path[self.nextWaypoint]
        self.sendMessage(self.nextPoint, self.getMovementSpeed())
        self.nextWaypoint = (self.nextWaypoint + 1) % self.listSize


class RocketboxFactory(ObjectFactory):
    def __init__(self, template):
        ObjectFactory.__init__(self, template)
        self.name = 'Pedestrian'
        self.displayContext = None
	self.rand = Random()
    def setName(self, name):
        self.name = name
    def getName(self):
        return self.name
    def setDisplayContext(self, dc):
        self.displayContext = dc
    def getDisplayContext(self):
        return self.displayContext
    def makeObject(self, spawnData, instanceOid, loc):
        override = Template()
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_LOC, loc)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, self.getName())
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, pedDC[self.rand.nextInt(21)])
        obj = ObjectFactory.makeObject(self, spawnData, instanceOid, override)
        obj.addBehavior(BaseBehavior())
	mobPt = []
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt0"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt1"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt2"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt3"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt4"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt5"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt6"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt7"))
	mobPt.append(InstanceClient.getMarkerPoint(instanceOid,"MobPt8"))
        behav = AdvancedPatrolBehavior(mobPt)
        behav.setLingerTime(0)
        behav.setMovementSpeed(1500)
        obj.addBehavior(behav)
        return obj

ObjectFactory.register("RocketboxFactory",RocketboxFactory("pedestrian"))

#ped0Factory = RocketboxFactory("pedestrian")
#ped0SpawnGen = DelayedSpawnGenerator("pedestrian generator")
#ped0SpawnGen.setObjectFactory(ped0Factory)
#ped0SpawnGen.setLoc(mobPt[0])
#ped0SpawnGen.setNumSpawns(0)
#ped0SpawnGen.setSpawnRadius(0)
#ped0SpawnGen.setRespawnTime(10000)
#ped0SpawnGen.activate()

Log.debug("done with nyts mobserver.py");
