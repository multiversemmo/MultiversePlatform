#
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
from java.util import Random
from java.util.concurrent import TimeUnit


#jukeboxLoc = WorldEditorReader.getWaypoint("JukeLocation")
#jukeboxOid = ObjectManagerClient.generateObject("jukebox", jukeboxLoc)
#WorldManagerClient.setObjectProperty(jukeboxOid, 'DisplayGroup', 'ClubInterior')
#WorldManagerClient.spawn(jukeboxOid)


class RocketboxFactory(ObjectFactory):
    def __init__(self, template):
        ObjectFactory.__init__(self, template)
        self.name = "npc"
        self.displayContext = None
        self.yRot = None
        self.idle = "idle_01"
        self.displayGroup = None
    def setName(self, name):
        self.name = name
    def getName(self):
        return self.name
    def setDisplayContext(self, dc):
        self.displayContext = dc
    def getDisplayContext(self):
        return self.displayContext
    def setYRot(self, yRot):
        self.yRot = yRot
    def getYRot(self):
        return self.yRot
    def setIdle(self, idle):
        self.idle = idle
    def getIdle(self):
        return self.idle
    def setDisplayGroup(self, dg):
        self.displayGroup = dg
    def getDisplayGroup(self):
        return self.displayGroup
    def makeObject(self, spawnData, instanceOid, loc):
        override = Template()
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_INSTANCE, instanceOid)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_LOC, loc)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, self.getName())
        if (self.getDisplayContext() is not None):
            override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, self.getDisplayContext())
        if (self.getYRot() is not None):
            override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_ORIENT, Quaternion.fromAngleAxis(self.getYRot(), MVVector(0, 1, 0)))
        obj = ObjectFactory.makeObject(self, instanceOid, override)
        if (self.getIdle() != "idle_01"):
            WorldManagerClient.setObjectProperty(obj.getOid(), "dancestate", self.getIdle())
        if (self.getDisplayGroup() is not None):
            WorldManagerClient.setObjectProperty(obj.getOid(), 'DisplayGroup', self.getDisplayGroup())
        obj.addBehavior(BaseBehavior())
        return obj


bartender01Pt = WorldEditorReader.getWaypoint("bartender01")
bartender01Factory  = RocketboxFactory("rocketbox npc")
bartender01DC = DisplayContext("casual13_f_mediumpoly.mesh", True)
bartender01DC.addSubmesh(DisplayContext.Submesh("casual13_f_mediumpoly-mesh.0", "casual13_f_mediumpoly.body"))
bartender01DC.addSubmesh(DisplayContext.Submesh("casual13_f_mediumpoly-mesh.1", "casual13_f_mediumpoly.hair_transparent"))
bartender01Factory.setDisplayContext(bartender01DC)
bartender01Factory.setName("bartender")
bartender01Factory.setYRot(0.78539)
bartender01Factory.setIdle("work_assembly")
bartender01Factory.setDisplayGroup("ClubInterior")
bartender01SpawnGen = SpawnGenerator("Bartender01 generator")
bartender01SpawnGen.setObjectFactory(bartender01Factory)
bartender01SpawnGen.setLoc(bartender01Pt)
bartender01SpawnGen.setNumSpawns(1)
bartender01SpawnGen.setSpawnRadius(1)
bartender01SpawnGen.setRespawnTime(1)
bartender01SpawnGen.activate()


bartender02Pt = WorldEditorReader.getWaypoint("bartender02")
bartender02Factory  = RocketboxFactory("rocketbox npc")
bartender02DC = DisplayContext("casual13_f_mediumpoly.mesh", True)
bartender02DC.addSubmesh(DisplayContext.Submesh("casual13_f_mediumpoly-mesh.0", "casual13_f_mediumpoly.body"))
bartender02DC.addSubmesh(DisplayContext.Submesh("casual13_f_mediumpoly-mesh.1", "casual13_f_mediumpoly.hair_transparent"))
bartender02Factory.setDisplayContext(bartender02DC)
bartender02Factory.setName("bartender")
bartender02Factory.setYRot(1.57079)
bartender02Factory.setIdle("work_assembly")
bartender02Factory.setDisplayGroup("CakeClub")
bartender02SpawnGen = SpawnGenerator("Bartender02 generator")
bartender02SpawnGen.setObjectFactory(bartender02Factory)
bartender02SpawnGen.setLoc(bartender02Pt)
bartender02SpawnGen.setNumSpawns(1)
bartender02SpawnGen.setSpawnRadius(1)
bartender02SpawnGen.setRespawnTime(1)
bartender02SpawnGen.activate()


dancer01Pt = WorldEditorReader.getWaypoint("dancer01")
dancer01Factory  = RocketboxFactory("rocketbox npc")
dancer01DC = DisplayContext("casual07_f_mediumpoly.mesh", True)
dancer01DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.0", "casual07_f_mediumpoly.body"))
dancer01DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.1", "casual07_f_mediumpoly.hair_transparent"))
dancer01Factory.setDisplayContext(dancer01DC)
dancer01Factory.setName("fan")
dancer01Factory.setYRot(0.98174)
dancer01Factory.setIdle("cheer")
dancer01Factory.setDisplayGroup("ClubInterior")
dancer01SpawnGen = SpawnGenerator("Dancer01 generator")
dancer01SpawnGen.setObjectFactory(dancer01Factory)
dancer01SpawnGen.setLoc(dancer01Pt)
dancer01SpawnGen.setNumSpawns(1)
dancer01SpawnGen.setSpawnRadius(1)
dancer01SpawnGen.setRespawnTime(1)
dancer01SpawnGen.activate()


dancer02Pt = WorldEditorReader.getWaypoint("dancer02")
dancer02Factory  = RocketboxFactory("rocketbox npc")
dancer02DC = DisplayContext("casual15_f_mediumpoly.mesh", True)
dancer02DC.addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.0", "casual15_f_mediumpoly.body"))
dancer02DC.addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.1", "casual15_f_mediumpoly.hair_transparent"))
dancer02Factory.setDisplayContext(dancer02DC)
dancer02Factory.setName("fan")
dancer02Factory.setYRot(0.34906)
dancer02Factory.setIdle("dance")
dancer02Factory.setDisplayGroup("ClubInterior")
dancer02SpawnGen = SpawnGenerator("Dancer02 generator")
dancer02SpawnGen.setObjectFactory(dancer02Factory)
dancer02SpawnGen.setLoc(dancer02Pt)
dancer02SpawnGen.setNumSpawns(1)
dancer02SpawnGen.setSpawnRadius(1)
dancer02SpawnGen.setRespawnTime(1)
dancer02SpawnGen.activate()


dancer03Pt = WorldEditorReader.getWaypoint("dancer03")
dancer03Factory  = RocketboxFactory("rocketbox npc")
dancer03DC = DisplayContext("casual07_m_mediumpoly.mesh", True)
dancer03DC.addSubmesh(DisplayContext.Submesh("casual07_m_mediumpoly-mesh.0", "casual07_m_mediumpoly.body"))
dancer03Factory.setDisplayContext(dancer03DC)
dancer03Factory.setName("fan")
dancer03Factory.setYRot(0.32725)
dancer03Factory.setIdle("dance")
dancer03Factory.setDisplayGroup("ClubInterior")
dancer03SpawnGen = SpawnGenerator("Dancer03 generator")
dancer03SpawnGen.setObjectFactory(dancer03Factory)
dancer03SpawnGen.setLoc(dancer03Pt)
dancer03SpawnGen.setNumSpawns(1)
dancer03SpawnGen.setSpawnRadius(1)
dancer03SpawnGen.setRespawnTime(1)
dancer03SpawnGen.activate()


dancer04Pt = WorldEditorReader.getWaypoint("dancer04")
dancer04Factory  = RocketboxFactory("rocketbox npc")
dancer04DC = DisplayContext("casual07_f_mediumpoly.mesh", True)
dancer04DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.0", "casual07_f_mediumpoly.body"))
dancer04DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.1", "casual07_f_mediumpoly.hair_transparent"))
dancer04Factory.setDisplayContext(dancer04DC)
dancer04Factory.setName("fan")
dancer04Factory.setYRot(-1.00356)
dancer04Factory.setIdle("dance") #("cheer")
dancer04Factory.setDisplayGroup("CakeClub")
dancer04SpawnGen = SpawnGenerator("Dancer04 generator")
dancer04SpawnGen.setObjectFactory(dancer04Factory)
dancer04SpawnGen.setLoc(dancer04Pt)
dancer04SpawnGen.setNumSpawns(1)
dancer04SpawnGen.setSpawnRadius(1)
dancer04SpawnGen.setRespawnTime(1)
dancer04SpawnGen.activate()


dancer05Pt = WorldEditorReader.getWaypoint("dancer05")
dancer05Factory  = RocketboxFactory("rocketbox npc")
dancer05DC = DisplayContext("casual15_f_mediumpoly.mesh", True)
dancer05DC.addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.0", "casual15_f_mediumpoly.body"))
dancer05DC.addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.1", "casual15_f_mediumpoly.hair_transparent"))
dancer05Factory.setDisplayContext(dancer05DC)
dancer05Factory.setName("fan")
dancer05Factory.setYRot(3.14159)
dancer05Factory.setIdle("cheer") #("dance")
dancer05Factory.setDisplayGroup("CakeClub")
dancer05SpawnGen = SpawnGenerator("Dancer05 generator")
dancer05SpawnGen.setObjectFactory(dancer05Factory)
dancer05SpawnGen.setLoc(dancer05Pt)
dancer05SpawnGen.setNumSpawns(1)
dancer05SpawnGen.setSpawnRadius(1)
dancer05SpawnGen.setRespawnTime(1)
dancer05SpawnGen.activate()


dancer06Pt = WorldEditorReader.getWaypoint("dancer06")
dancer06Factory  = RocketboxFactory("rocketbox npc")
dancer06DC = DisplayContext("casual07_m_mediumpoly.mesh", True)
dancer06DC.addSubmesh(DisplayContext.Submesh("casual07_m_mediumpoly-mesh.0", "casual07_m_mediumpoly.body"))
dancer06Factory.setDisplayContext(dancer06DC)
dancer06Factory.setName("fan")
dancer06Factory.setYRot(2.13803)
dancer06Factory.setIdle("dance")
dancer06Factory.setDisplayGroup("CakeClub")
dancer06SpawnGen = SpawnGenerator("Dancer06 generator")
dancer06SpawnGen.setObjectFactory(dancer06Factory)
dancer06SpawnGen.setLoc(dancer06Pt)
dancer06SpawnGen.setNumSpawns(1)
dancer06SpawnGen.setSpawnRadius(1)
dancer06SpawnGen.setRespawnTime(1)
dancer06SpawnGen.activate()


patron01Pt = WorldEditorReader.getWaypoint("patron01")
patron01Factory  = RocketboxFactory("rocketbox npc")
patron01DC = DisplayContext("casual16_m_mediumpoly.mesh", True)
patron01DC.addSubmesh(DisplayContext.Submesh("casual16_m_mediumpoly-mesh.0", "casual16_m_mediumpoly.body"))
patron01Factory.setDisplayContext(patron01DC)
patron01Factory.setName("patron")
patron01Factory.setYRot(0.34906)
patron01Factory.setDisplayGroup("ClubInterior")
patron01Factory.setIdle("cheer")
patron01SpawnGen = SpawnGenerator("patron01 generator")
patron01SpawnGen.setObjectFactory(patron01Factory)
patron01SpawnGen.setLoc(patron01Pt)
patron01SpawnGen.setNumSpawns(1)
patron01SpawnGen.setSpawnRadius(1)
patron01SpawnGen.setRespawnTime(1)
patron01SpawnGen.activate()

patron02Pt = WorldEditorReader.getWaypoint("patron02")
patron02Factory  = RocketboxFactory("rocketbox npc")
patron02DC = DisplayContext("casual16_m_mediumpoly.mesh", True)
patron02DC.addSubmesh(DisplayContext.Submesh("casual16_m_mediumpoly-mesh.0", "casual16_m_mediumpoly.body"))
patron02Factory.setDisplayContext(patron02DC)
patron02Factory.setName("patron")
patron02Factory.setYRot(2.53073)
patron02Factory.setDisplayGroup("CakeClub")
patron02Factory.setIdle("cheer")
patron02SpawnGen = SpawnGenerator("patron02 generator")
patron02SpawnGen.setObjectFactory(patron02Factory)
patron02SpawnGen.setLoc(patron02Pt)
patron02SpawnGen.setNumSpawns(1)
patron02SpawnGen.setSpawnRadius(1)
patron02SpawnGen.setRespawnTime(1)
patron02SpawnGen.activate()

patron03Pt = WorldEditorReader.getWaypoint("patron03")
patron03Factory  = RocketboxFactory("rocketbox npc")
patron03DC = DisplayContext("casual07_m_mediumpoly.mesh", True)
patron03DC.addSubmesh(DisplayContext.Submesh("casual07_m_mediumpoly-mesh.0", "casual07_m_mediumpoly.body"))
patron03Factory.setDisplayContext(patron03DC)
patron03Factory.setName("patron")
patron03Factory.setYRot(0) #(3.14159)
patron03Factory.setDisplayGroup("Cakeshop")
patron03Factory.setIdle("work_assembly")
patron03SpawnGen = SpawnGenerator("patron03 generator")
patron03SpawnGen.setObjectFactory(patron03Factory)
patron03SpawnGen.setLoc(patron03Pt)
patron03SpawnGen.setNumSpawns(1)
patron03SpawnGen.setSpawnRadius(1)
patron03SpawnGen.setRespawnTime(1)
patron03SpawnGen.activate()

patron04Pt = WorldEditorReader.getWaypoint("patron04")
patron04Factory  = RocketboxFactory("rocketbox npc")
patron04DC = DisplayContext("casual15_f_mediumpoly.mesh", True)
patron04DC.addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.0", "casual15_f_mediumpoly.body"))
patron04DC.addSubmesh(DisplayContext.Submesh("casual15_f_mediumpoly-mesh.1", "casual15_f_mediumpoly.hair_transparent"))
patron04Factory.setDisplayContext(patron04DC)
patron04Factory.setName("patron")
patron04Factory.setYRot(-2.96706)
patron04Factory.setDisplayGroup("Cakeshop")
patron04Factory.setIdle("talk_01")
patron04SpawnGen = SpawnGenerator("patron04 generator")
patron04SpawnGen.setObjectFactory(patron04Factory)
patron04SpawnGen.setLoc(patron04Pt)
patron04SpawnGen.setNumSpawns(1)
patron04SpawnGen.setSpawnRadius(1)
patron04SpawnGen.setRespawnTime(1)
patron04SpawnGen.activate()

patron05Pt = WorldEditorReader.getWaypoint("patron05")
patron05Factory  = RocketboxFactory("rocketbox npc")
patron05DC = DisplayContext("casual16_m_mediumpoly.mesh", True)
patron05DC.addSubmesh(DisplayContext.Submesh("casual16_m_mediumpoly-mesh.0", "casual16_m_mediumpoly.body"))
patron05Factory.setDisplayContext(patron05DC)
patron05Factory.setName("patron")
patron05Factory.setYRot(0.17453)
patron05Factory.setDisplayGroup("Cakeshop")
patron05Factory.setIdle("listen")
patron05SpawnGen = SpawnGenerator("patron05 generator")
patron05SpawnGen.setObjectFactory(patron05Factory)
patron05SpawnGen.setLoc(patron05Pt)
patron05SpawnGen.setNumSpawns(1)
patron05SpawnGen.setSpawnRadius(1)
patron05SpawnGen.setRespawnTime(1)
patron05SpawnGen.activate()

clerk01Pt = WorldEditorReader.getWaypoint("clerk01")
clerk01Factory  = RocketboxFactory("rocketbox npc")
clerk01DC = DisplayContext("casual07_f_mediumpoly.mesh", True)
clerk01DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.0", "casual07_f_mediumpoly.body"))
clerk01DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.1", "casual07_f_mediumpoly.hair_transparent"))
clerk01Factory.setDisplayContext(clerk01DC)
clerk01Factory.setName("clerk")
clerk01Factory.setYRot(1.57079)
clerk01Factory.setDisplayGroup("Cakeshop")
clerk01Factory.setIdle("idle_02")
clerk01SpawnGen = SpawnGenerator("clerk01 generator")
clerk01SpawnGen.setObjectFactory(clerk01Factory)
clerk01SpawnGen.setLoc(clerk01Pt)
clerk01SpawnGen.setNumSpawns(1)
clerk01SpawnGen.setSpawnRadius(1)
clerk01SpawnGen.setRespawnTime(1)
clerk01SpawnGen.activate()

clerk02Pt = WorldEditorReader.getWaypoint("clerk02")
clerk02Factory  = RocketboxFactory("rocketbox npc")
clerk02DC = DisplayContext("casual07_f_mediumpoly.mesh", True)
clerk02DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.0", "casual07_f_mediumpoly.body"))
clerk02DC.addSubmesh(DisplayContext.Submesh("casual07_f_mediumpoly-mesh.1", "casual07_f_mediumpoly.hair_transparent"))
clerk02Factory.setDisplayContext(clerk02DC)
clerk02Factory.setName("clerk")
clerk02Factory.setYRot(1.57079)
clerk02Factory.setIdle("idle_02")
clerk02SpawnGen = SpawnGenerator("clerk02 generator")
clerk02SpawnGen.setObjectFactory(clerk02Factory)
clerk02SpawnGen.setLoc(clerk02Pt)
clerk02SpawnGen.setNumSpawns(1)
clerk02SpawnGen.setSpawnRadius(1)
clerk02SpawnGen.setRespawnTime(1)
clerk02SpawnGen.activate()

welcomeQuest = MarsCollectionQuest()
welcomeQuest.setName("Free Song of the Day!")
welcomeQuest.setDesc("The party is going on downstairs.  The DJ is giving out the free Song of the day.  Go talk to him downstairs by the DJ area.")
welcomeQuest.setObjective("Go see the DJ downstairs.")

songQuest = MarsCollectionQuest()
songQuest.setName("Do you know your stuff?")
songQuest.setDesc("So, you want your free song of the day, huh?  Let's see if you know your stuff.")
songQuest.setObjective("Finish the quiz.")
songQuest.addQuestPrereq("Free Song of the Day!")
welcomeQuest.setChainQuest(songQuest)

uploadQuest = MarsCollectionQuest()
uploadQuest.setName("So you're a DJ...")
uploadQuest.setDesc("You're a DJ too huh?  Think your track is good enough to be song of the day?")
uploadQuest.setObjective("Upload your song.")
uploadQuest.addQuestPrereq("Do you know your stuff?")
songQuest.setChainQuest(uploadQuest)

class AdvancedReactionBehavior(TeleporterBehavior):
    def __init__(self):
        TeleporterBehavior.__init__(self)
        self.msg = "Hello."
        self.anim = "None"
        self.delay = 0
        self.players = {}
    def reaction(self, nMsg):
        oid = nMsg.getTarget()
        poid = nMsg.getSubject()
        cms = System.currentTimeMillis()
        if (not self.players.has_key(poid)):
            wave = True
        else:
            wms = self.players[poid]
            wave = ((cms - wms) > self.getAnimDelayMs())
        if (wave):
            self.players[poid] = cms
            AnimationClient.playSingleAnimation(oid, self.getAnim())
            chat = self.getMsg()
            chat = chat.replace("%pn", WorldManagerClient.getObjectInfo(poid).name)
            WorldManagerClient.sendChatMsg(oid, 1, chat)
    def setMsg(self, msg):
        self.msg = msg
    def getMsg(self):
        return self.msg
    def setAnim(self, anim):
        self.anim = anim
    def getAnim(self):
        return self.anim
    def setAnimDelayMs(self, delay):
        self.delay = delay
    def getAnimDelayMs(self):
        return self.delay

class BouncerFactory(RocketboxFactory):
    def __init__(self, template):
        RocketboxFactory.__init__(self, template)
    def makeObject(self, spawnData, instanceOid, loc):
        obj = RocketboxFactory.makeObject(self, spawnData, instanceOid, loc)
        behav = AdvancedReactionBehavior()
        behav.setRadius(3000)
        behav.setMsg("Greetings, %pn.  Right click on me to start your tour.")
        behav.setAnim("wave")
        behav.setAnimDelayMs(60000)
        obj.addBehavior(behav)
        behav = QuestBehavior()
        behav.startsQuest(welcomeQuest)
        obj.addBehavior(behav)
        return obj

bouncer01Pt = WorldEditorReader.getWaypoint("bouncer01")
bouncer01Factory  = BouncerFactory("rocketbox npc")
bouncer01DC = DisplayContext("casual21_m_mediumpoly.mesh", True)
bouncer01DC.addSubmesh(DisplayContext.Submesh("casual21_m_mediumpoly-mesh.0", "casual21_m_mediumpoly.body"))
bouncer01Factory.setDisplayContext(bouncer01DC)
bouncer01Factory.setName("Bouncer Joe")
bouncer01Factory.setYRot(-1.57079)
bouncer01SpawnGen = SpawnGenerator("Bouncer01 generator")
bouncer01SpawnGen.setObjectFactory(bouncer01Factory)
bouncer01SpawnGen.setLoc(bouncer01Pt)
bouncer01SpawnGen.setNumSpawns(1)
bouncer01SpawnGen.setSpawnRadius(1)
bouncer01SpawnGen.setRespawnTime(1)
bouncer01SpawnGen.activate()

bouncer02Pt = WorldEditorReader.getWaypoint("bouncer02")
bouncer02Factory  = BouncerFactory("rocketbox npc")
bouncer02DC = DisplayContext("casual21_m_mediumpoly.mesh", True)
bouncer02DC.addSubmesh(DisplayContext.Submesh("casual21_m_mediumpoly-mesh.0", "casual21_m_mediumpoly.body"))
bouncer02Factory.setDisplayContext(bouncer02DC)
bouncer02Factory.setName("Bouncer Roy")
bouncer02Factory.setYRot(0.0)
bouncer02SpawnGen = SpawnGenerator("Bouncer02 generator")
bouncer02SpawnGen.setObjectFactory(bouncer02Factory)
bouncer02SpawnGen.setLoc(bouncer02Pt)
bouncer02SpawnGen.setNumSpawns(1)
bouncer02SpawnGen.setSpawnRadius(1)
bouncer02SpawnGen.setRespawnTime(1)
bouncer02SpawnGen.activate()


class DjFactory(RocketboxFactory):
    def __init__(self, template):
        RocketboxFactory.__init__(self, template)
    def makeObject(self, spawnData, instanceOid, loc):
        obj = RocketboxFactory.makeObject(self, spawnData, instanceOid, loc)
        behav = QuestBehavior()
        behav.endsQuest(welcomeQuest)

        behav.startsQuest(songQuest)
        behav.endsQuest(songQuest)

        behav.startsQuest(uploadQuest)
        behav.endsQuest(uploadQuest)

        obj.addBehavior(behav)
        return obj

dj01Pt = WorldEditorReader.getWaypoint("dj01")
dj01Factory  = DjFactory("rocketbox npc")
dj01DC = DisplayContext("LES_dj.mesh", True)
dj01DC.addSubmesh(DisplayContext.Submesh("dj-mesh.0", "LES_dj.dj"))
dj01Factory.setDisplayContext(dj01DC)
dj01Factory.setName("DJ Stevie")
dj01Factory.setYRot(-1.57079)
dj01Factory.setDisplayGroup('ClubInterior')
dj01SpawnGen = SpawnGenerator("Dj01 generator")
dj01SpawnGen.setObjectFactory(dj01Factory)
dj01SpawnGen.setLoc(dj01Pt)
dj01SpawnGen.setNumSpawns(1)
dj01SpawnGen.setSpawnRadius(1)
dj01SpawnGen.setRespawnTime(1)
dj01SpawnGen.activate()

dj02Pt = WorldEditorReader.getWaypoint("dj02")
dj02Factory  = DjFactory("rocketbox npc")
dj02DC = DisplayContext("LES_dj.mesh", True)
dj02DC.addSubmesh(DisplayContext.Submesh("dj-mesh.0", "LES_dj.dj"))
dj02Factory.setDisplayContext(dj02DC)
dj02Factory.setName("DJ Stan")
dj02Factory.setYRot(3.14159)
dj02Factory.setDisplayGroup('CakeClub')
dj02SpawnGen = SpawnGenerator("Dj02 generator")
dj02SpawnGen.setObjectFactory(dj02Factory)
dj02SpawnGen.setLoc(dj02Pt)
dj02SpawnGen.setNumSpawns(1)
dj02SpawnGen.setSpawnRadius(1)
dj02SpawnGen.setRespawnTime(1)
dj02SpawnGen.activate()


class StarFactory(ObjectFactory):
    def __init__(self, template):
        ObjectFactory.__init__(self, template)
        self.yRot = None
        self.displayGroup = None
        self.name = "Brena Star"
    def setName(self, name):
        self.name = name
    def getName(self):
        return self.name
    def setYRot(self, yRot):
        self.yRot = yRot
    def getYRot(self):
        return self.yRot
    def setDisplayGroup(self, dg):
        self.displayGroup = dg
    def getDisplayGroup(self):
        return self.displayGroup
    def makeObject(self, spawnData, instanceOid, loc):
        override = Template()
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_INSTANCE, instanceOid)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_LOC, loc)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, self.getName())
        if (self.getYRot() is not None):
            override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_ORIENT, Quaternion.fromAngleAxis(self.getYRot(), MVVector(0, 1, 0)))
        obj = ObjectFactory.makeObject(self, instanceOid, override)
        WorldManagerClient.setObjectProperty(obj.getOid(), "Gender", "female")
        WorldManagerClient.setObjectProperty(obj.getOid(), "AppearanceOverride", "avatar")
        WorldManagerClient.setObjectProperty(obj.getOid(), "SkinColor", "caucasian")
        WorldManagerClient.setObjectProperty(obj.getOid(), "HeadShape", "caucasian_01")
        WorldManagerClient.setObjectProperty(obj.getOid(), "HeadDetail", "")
        WorldManagerClient.setObjectProperty(obj.getOid(), "HairStyle", "bob2")
        WorldManagerClient.setObjectProperty(obj.getOid(), "HairColor", "brown")
        WorldManagerClient.setObjectProperty(obj.getOid(), "ClothesTorso", "strapless_red")
        WorldManagerClient.setObjectProperty(obj.getOid(), "ClothesLegs", "short_skirt_leopard")
        WorldManagerClient.setObjectProperty(obj.getOid(), "Tattoo", "tattoo_02_chest")
        WorldManagerClient.setObjectProperty(obj.getOid(), "dancestate", "dance3")
        if (self.getDisplayGroup() is not None):
            WorldManagerClient.setObjectProperty(obj.getOid(), 'DisplayGroup', self.getDisplayGroup())
        obj.addBehavior(BaseBehavior())
        return obj

star01Pt = WorldEditorReader.getWaypoint("star01")
star01Factory  = StarFactory("avatar")
star01Factory.setName("Brena Star")
star01Factory.setYRot(-2.55251)
star01Factory.setDisplayGroup('ClubInterior')
star01SpawnGen = SpawnGenerator("Star01 generator")
star01SpawnGen.setObjectFactory(star01Factory)
star01SpawnGen.setLoc(star01Pt)
star01SpawnGen.setNumSpawns(1)
star01SpawnGen.setSpawnRadius(1)
star01SpawnGen.setRespawnTime(1)
star01SpawnGen.activate()

star02Pt = WorldEditorReader.getWaypoint("star02")
star02Factory  = StarFactory("avatar")
star02Factory.setName("Breda Star")
star02Factory.setYRot(0)
star02Factory.setDisplayGroup('CakeClub')
star02SpawnGen = SpawnGenerator("Star02 generator")
star02SpawnGen.setObjectFactory(star02Factory)
star02SpawnGen.setLoc(star02Pt)
star02SpawnGen.setNumSpawns(1)
star02SpawnGen.setSpawnRadius(1)
star02SpawnGen.setRespawnTime(1)
star02SpawnGen.activate()


rand = Random()
startPt = WorldEditorReader.getWaypoint("path0_pt0")
class AdvancedPatrolBehavior(PatrolBehavior):
    def __init__(self):
        PatrolBehavior.__init__(self)
        self.nextWaypoint=1
        self.path=[[],[]]
        self.listSize=28
        for i in range(self.listSize):
            self.path[0].append(WorldEditorReader.getWaypoint("path0_pt"+str(i)))
            self.path[1].append(WorldEditorReader.getWaypoint("path1_pt"+str(i)))
        self.side=0
        self.cross=0
    def startPatrol(self):
        self.nextPatrol()
    def nextPatrol(self):
        chance = rand.nextInt(1000)
        if (self.nextWaypoint == 0):
            if (chance < 50):
                self.cross = 1
            else:
                self.cross = 0
        if (self.nextWaypoint == 1):
            self.cross = 0
        if (self.nextWaypoint == 2):
            self.cross = 0
        if (self.nextWaypoint == 3):
            if (chance < 500):
                self.cross = 1
            else:
                self.cross = 0
        if ((self.nextWaypoint >= 4) and (self.nextWaypoint <= 17)):
            if (chance < 50):
                self.cross = 1
            else:
                self.cross = 0
        if (self.nextWaypoint == 18):
            self.cross = 0
        if (self.nextWaypoint == 19):
            self.cross = 0
        if (self.nextWaypoint == 20):
            if (chance < 500):
                self.cross = 1
            else:
                self.cross = 0
        if ((self.nextWaypoint >= 21) and (self.nextWaypoint <= 27)):
            if (chance < 50):
                self.cross = 1
            else:
                self.cross = 0
        self.side = (self.side + self.cross) % 2
        self.nextPoint = self.path[self.side][self.nextWaypoint]
        self.sendMessage(self.nextPoint, self.getMovementSpeed())
        self.nextWaypoint = (self.nextWaypoint + 1) % self.listSize
#     def gotoSetup(self, dest, speed):
#         self.destLoc = dest
#         self.mobSpeed = speed
#         myLoc = self.obj.getWorldNode().getLoc()
#         oid = self.obj.getOid()
#         self.scheduleMe(self.setupPathInterpolator(oid, myLoc, dest, Boolean(False), Boolean(False)))

skinColor = ['caucasian', 'asian', 'african_american']
headMesh  = ['caucasian_01', 'asian_01', 'african_american_01']
hairMesh  = ['pony', 'bob', 'layers', 'bob2']
hairColor = ['blonde', 'red', 'brown', 'black']
torso     = ['sleeveless_white', 'sleeveless_purple', 'sleeveless_blue',
             'strapless_brown', 'strapless_purple', 'strapless_red',
             'leotard_blue', 'leotard_red', 'leotard_skull']
legs      = ['capris_black', 'capris_brown', 'capris_blue',
             'short_skirt_leopard', 'short_skirt_red']
tattoo    = ['', '', '', '', '', '', '', '', '', '', '', '',
             'tattoo_01_arm', 'tattoo_01_back', 'tattoo_01_chest',
             'tattoo_02_arm', 'tattoo_02_back', 'tattoo_02_chest',
             'tattoo_03_arm', 'tattoo_03_back', 'tattoo_03_chest',]
name      = ['Betty', 'Maria', 'Lisa', 'Penny', 'Debby']

class DelayedSpawnGenerator(SpawnGenerator):
    def activate(self):
        if (self.numSpawns > 0):
            self.numSpawns = self.numSpawns - 1
            self.spawnObject()
            if (self.numSpawns > 0):
                Engine.getExecutor().schedule(self, self.respawnTime, TimeUnit.MILLISECONDS)
    def run(self):
        self.activate()

class Female_Pedestrian_Factory (ObjectFactory):
    def makeObject(self, spawnData, instanceOid, loc):
        override = Template()
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_INSTANCE, instanceOid)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_LOC, loc)
        override.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, name[rand.nextInt(5)])
        override.put(WorldManagerClient.NAMESPACE, 'Gender', 'female')
        override.put(WorldManagerClient.NAMESPACE, 'AppearanceOverride', 'avatar')
        override.put(WorldManagerClient.NAMESPACE, 'SkinColor', skinColor[rand.nextInt(3)])
        override.put(WorldManagerClient.NAMESPACE, 'HeadShape', headMesh[rand.nextInt(3)])
        override.put(WorldManagerClient.NAMESPACE, 'HeadDetail', '')
        override.put(WorldManagerClient.NAMESPACE, 'HairStyle', hairMesh[rand.nextInt(4)])
        override.put(WorldManagerClient.NAMESPACE, 'HairColor', hairColor[rand.nextInt(4)])
        override.put(WorldManagerClient.NAMESPACE, 'ClothesTorso', torso[rand.nextInt(9)])
        override.put(WorldManagerClient.NAMESPACE, 'ClothesLegs', legs[rand.nextInt(5)])
        override.put(WorldManagerClient.NAMESPACE, 'Tattoo', tattoo[rand.nextInt(21)])
        override.put(WorldManagerClient.NAMESPACE, 'DisplayGroup', 'Exterior')
        obj = ObjectFactory.makeObject(self, instanceOid, override)
        obj.addBehavior(BaseBehavior())
        behav = AdvancedPatrolBehavior()
        behav.setLingerTime(0)
        behav.setMovementSpeed(1750)
        obj.addBehavior(behav)
        return obj

female_pedestrian_Factory  = Female_Pedestrian_Factory("avatar")
female_pedestrian_SpawnGen = DelayedSpawnGenerator("female pedestrian generator")
female_pedestrian_SpawnGen.setObjectFactory(female_pedestrian_Factory)
female_pedestrian_SpawnGen.setLoc(startPt)
female_pedestrian_SpawnGen.setNumSpawns(0)
female_pedestrian_SpawnGen.setSpawnRadius(0)
female_pedestrian_SpawnGen.setRespawnTime(8000)
female_pedestrian_SpawnGen.activate()


Log.debug("done with mobserver.py")
