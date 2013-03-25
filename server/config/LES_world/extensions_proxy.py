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

from java.util import *
from java.lang import *
from java.util.concurrent import TimeUnit;
from multiverse.mars import *
from multiverse.mars.core import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.msgsys import *
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from multiverse.server.messages import *

True=1
False=0

Log.debug("extensions_proxy.py: Loading...")

class WaveCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/wave: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "wave")
    
class BowCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/bow: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "bow")
    
class ClapCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/clap: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "clap")
    
class CryCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/cry: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "cry")
 
class LaughCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/laugh: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "laugh")
    
class CheerCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/cheer: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "cheer")
    
class NoCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/no: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "disagree")
    
class PointCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/point: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "point")
    
class ShrugCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/shrug: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "shrug")

class JukeboxGetTracksCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        splitCmd = cmdEvent.getCommand().split(" ")
        jukeboxId = int(splitCmd[1])
        Log.debug("/jukeboxGetTracks: oid=" + str(playerOid) + " jukeboxId=" + str(jukeboxId))
        msg = GenericMessage(MessageType.intern("jukeboxSetId"))
        msg.setProperty("id", jukeboxId)
        Engine.getAgent().sendRPC(msg)
        msg = GenericMessage(MessageType.intern("jukeboxGetTracks"))
        respMsg = Engine.getAgent().sendRPC(msg)
        trackData = respMsg.getData()
        extMsg = WorldManagerClient.TargetedExtensionMessage(playerOid)
        extMsg.setExtensionType("jukebox_get_tracks_resp")
        extMsg.put("len", len(trackData))
        for i in range(0, len(trackData)):
            extMsg.put("name_" + str(i), trackData[i]["name"])
            extMsg.put("type_" + str(i), trackData[i]["type"])
            extMsg.put("url_" + str(i), trackData[i]["url"])
            extMsg.put("cost_" + str(i), trackData[i]["cost"])
            extMsg.put("description_" + str(i), trackData[i]["description"])
        Engine.getAgent().sendBroadcast(extMsg)

class JukeboxPlayCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        splitCmd = cmdEvent.getCommand().split(" ")
        id = int(splitCmd[1])
        type = splitCmd[2]
        url = splitCmd[3]
        msg = GenericMessage(MessageType.intern("jukeboxPlay"))
        msg.setProperty("id", id)
        msg.setProperty("url", url)
        msg.setProperty("type", type)
        msg.setProperty("poid", str(playerOid))
        Engine.getAgent().sendBroadcast(msg)

class SpotlightCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        splitCmd = cmdEvent.getCommand().split(" ")
        duration = int(float(splitCmd[1]) * 1000)
        effect = CoordinatedEffect("LES_Spotlight")
        effect.sendTargetOid(True)
        effect.putArgument("duration", duration)
        if len(splitCmd) >= 5:
            effect.putArgument("red", Float(splitCmd[2]))
            effect.putArgument("green", Float(splitCmd[3]))
            effect.putArgument("blue", Float(splitCmd[4]))
        effect.invoke(playerOid, targetOid)

proxyPlugin.registerCommand("/wave", WaveCommand())      
proxyPlugin.registerCommand("/bow", BowCommand())
proxyPlugin.registerCommand("/clap", ClapCommand())
proxyPlugin.registerCommand("/cry", CryCommand())
proxyPlugin.registerCommand("/laugh", LaughCommand())
proxyPlugin.registerCommand("/cheer", CheerCommand())
proxyPlugin.registerCommand("/no", NoCommand())
proxyPlugin.registerCommand("/point", PointCommand())
proxyPlugin.registerCommand("/shrug", ShrugCommand())
proxyPlugin.registerCommand("/jukeboxGetTracks", JukeboxGetTracksCommand())
proxyPlugin.registerCommand("/jukeboxPlay", JukeboxPlayCommand())
proxyPlugin.registerCommand("/spot", SpotlightCommand())



danceDict = { "1"     : "dance1",
              "2"     : "dance2",
              "3"     : "dance3",
              "4"     : "dance4",
              "5"     : "dance5",
              "booty" : "dance1",
              "fist"  : "dance2",
              "hip"   : "dance3",
              "spin"  : "dance4",
              "stomp" : "dance5",
              }

def setDance(oid, dance):
    WorldManagerClient.setObjectProperty(oid, "OnSkateboard", Boolean(False))
    WorldManagerClient.setObjectProperty(oid, "dancestate", dance)

class DanceCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        splitCmd = cmdEvent.getCommand().split(" ")
        Log.debug("/dance: oid=" + str(playerOid))
        if (len(splitCmd)==1):
            roll = rand.nextInt(len(danceDict))
            dance = danceDict.keys()[roll]
        else:
            dance = splitCmd[1]
        setDance(playerOid, danceDict[dance])

class DanceNumCommand (ProxyPlugin.CommandParser):
    def __init__(self, dance):
        self.dance = dance

    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        splitCmd = cmdEvent.getCommand().split(" ")
        Log.debug(splitCmd[0] + ": oid=" + str(playerOid))
        setDance(playerOid, self.dance)

proxyPlugin.registerCommand("/dance",  DanceCommand())
proxyPlugin.registerCommand("/dance1", DanceNumCommand("dance1"))
proxyPlugin.registerCommand("/dance2", DanceNumCommand("dance2"))
proxyPlugin.registerCommand("/dance3", DanceNumCommand("dance3"))
proxyPlugin.registerCommand("/dance4", DanceNumCommand("dance4"))
proxyPlugin.registerCommand("/dance5", DanceNumCommand("dance5"))



class FlirtCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        maybePassTag(playerOid, targetOid)
        splitCmd = cmdEvent.getCommand().split(" ")
        Log.debug("/flirt: oid=" + str(playerOid) + ", cmd=" + cmdEvent.getCommand())
        if (len(splitCmd)==1):
            roll = rand.nextInt(4)+1
            if (roll == 1):
                AnimationClient.playSingleAnimation(playerOid, "flirt_giggle")
            if (roll == 2):
                AnimationClient.playSingleAnimation(playerOid, "flirt_kiss")
            if (roll == 3):
                AnimationClient.playSingleAnimation(playerOid, "flirt_cute")
            if (roll == 4):
                AnimationClient.playSingleAnimation(playerOid, "flirt_wave")
        else: # (len(splitCmd)>1):
            if ((splitCmd[1]=="1") or (splitCmd[1]=="giggle")):
                AnimationClient.playSingleAnimation(playerOid, "flirt_giggle")
            if ((splitCmd[1]=="2") or (splitCmd[1]=="kiss")):
                AnimationClient.playSingleAnimation(playerOid, "flirt_kiss")
            if ((splitCmd[1]=="3") or (splitCmd[1]=="cute") or (splitCmd[1]=="blush")):
                AnimationClient.playSingleAnimation(playerOid, "flirt_cute")
            if ((splitCmd[1]=="4") or (splitCmd[1]=="wave")):
                AnimationClient.playSingleAnimation(playerOid, "flirt_wave")

class Flirt1Command (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        maybePassTag(playerOid, targetOid)
        Log.debug("/flirt1: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "flirt_giggle")

class Flirt2Command (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        maybePassTag(playerOid, targetOid)
        Log.debug("/flirt2: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "flirt_kiss")

class Flirt3Command (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        maybePassTag(playerOid, targetOid)
        Log.debug("/flirt3: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "flirt_cute")

class Flirt4Command (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        maybePassTag(playerOid, targetOid)
        Log.debug("/flirt4: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "flirt_wave")

class SkateboardCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/skateboard: oid=" + str(playerOid))
        WorldManagerClient.setObjectProperty(playerOid, "dancestate", "")
        if (WorldManagerClient.getObjectProperty(playerOid, "OnSkateboard") == True):
            WorldManagerClient.setObjectProperty(playerOid, "OnSkateboard", Boolean(False))
        else:
            WorldManagerClient.setObjectProperty(playerOid, "OnSkateboard", Boolean(True))

class LightCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        targetOid = cmdEvent.getTarget()
        splitCmd = cmdEvent.getCommand().split()
        Log.debug("/light oid=" + str(targetOid))
        EnginePlugin.setObjectProperty(targetOid, WorldManagerClient.NAMESPACE,
                                       "LightControl_State", splitCmd[1])

class ClubLightCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        targetOid = cmdEvent.getTarget()
        splitCmd = cmdEvent.getCommand().split()
        Log.debug("/clubLight oid=" + str(targetOid))
        EnginePlugin.setObjectProperty(targetOid, WorldManagerClient.NAMESPACE,
                                       "club_light", splitCmd[1])

class ClubStrobeCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        targetOid = cmdEvent.getTarget()
        splitCmd = cmdEvent.getCommand().split()
        Log.debug("/clubStrobe oid=" + str(targetOid))
        EnginePlugin.setObjectProperty(targetOid, WorldManagerClient.NAMESPACE,
                                       "club_strobe", splitCmd[1])

class TattooCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        splitCmd = cmdEvent.getCommand().split(" ")
        Log.debug("/tattoo: oid=" + str(playerOid) + ", cmd=" + cmdEvent.getCommand())
        if (splitCmd[1] == "0"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "")
        if (splitCmd[1] == "1"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_01_arm")
        if (splitCmd[1] == "2"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_02_arm")
        if (splitCmd[1] == "3"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_03_arm")
        if (splitCmd[1] == "4"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_01_back")
        if (splitCmd[1] == "5"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_02_back")
        if (splitCmd[1] == "6"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_03_back")
        if (splitCmd[1] == "7"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_01_chest")
        if (splitCmd[1] == "8"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_02_chest")
        if (splitCmd[1] == "9"):
            WorldManagerClient.setObjectProperty(playerOid, "Tattoo", "tattoo_03_chest")

proxyPlugin.registerCommand("/flirt",  FlirtCommand())
proxyPlugin.registerCommand("/flirt1", Flirt1Command())
proxyPlugin.registerCommand("/giggle", Flirt1Command())
proxyPlugin.registerCommand("/flirt2", Flirt2Command())
proxyPlugin.registerCommand("/kiss",   Flirt2Command())
proxyPlugin.registerCommand("/flirt3", Flirt3Command())
proxyPlugin.registerCommand("/cute",   Flirt3Command())
proxyPlugin.registerCommand("/blush",  Flirt3Command())
proxyPlugin.registerCommand("/flirt4", Flirt4Command())
proxyPlugin.registerCommand("/wave",   Flirt4Command())
proxyPlugin.registerCommand("/light",  LightCommand())
proxyPlugin.registerCommand("/clubLight", ClubLightCommand())
proxyPlugin.registerCommand("/clubStrobe", ClubStrobeCommand())
proxyPlugin.registerCommand("/tattoo", TattooCommand())


proxyPlugin.registerCommand("/skate",      SkateboardCommand())
proxyPlugin.registerCommand("/skateboard", SkateboardCommand())

# smoke for club
smokeLoc = Point(16046, 162683, -1150555)
smokeDC = DisplayContext("tiny_cube.mesh")
smokeDC.addSubmesh(DisplayContext.Submesh("foo", "foo"))
smokeOid = WorldManagerClient.spawnStructure("smokeObj", smokeDC, smokeLoc,
                                             Quaternion(0, 0, 0, 1), MVVector(1, 1, 1), 0);
WorldManagerClient.despawn(smokeOid)
# Should be reimplemented using coordinated effect
#AnimationClient.addParticleEffect(smokeOid, "foo", "LES_club_fog",
#                                  Quaternion(0, 0, 0, 1), 1.0, 1.0, 0)
smokeSpawned = False

class SmokeCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        global smokeOid
        global smokeSpawned
        if smokeSpawned:
            Log.debug("despawn smokeObj")
            WorldManagerClient.despawn(smokeOid)
            smokeSpawned = False
        else:
            Log.debug("spawn smokeObj")
            WorldManagerClient.spawn(smokeOid)
            smokeSpawned = True
        

proxyPlugin.registerCommand("/smoke", SmokeCommand())

tagTarget = None
lastTarget = None
tagCount = 0
clubLoc = Point(16046, 162683, -1150555)
clubRadius = 15000
def findRandomTarget():
    playerOids = Engine.getPlugin("Proxy1").getPlayerOids()
    clubPlayers = []
    for oid in playerOids:
        if isInClub(oid):
            clubPlayers.add(oid)
    player = clubPlayers[rand.nextInt(len(clubPlayers))]
    return player

def isInClub(oid):
    global clubLoc
    global clubRadius
    wnode = WorldManagerClient.getWorldNode(oid)
    if wnode is None:
        return False
    else:
        return Points.isClose(wnode.getLoc(), clubLoc, clubRadius)

def setTagTarget(oid):
    global tagTarget
    global lastTarget
    global tagCount
    if oid == tagTarget:
        return
    if oid == lastTarget:
        val = 2
    else:
        val = 1
    if tagTarget != None:
        msg = PropertyMessage(tagTarget)
        msg.put("tagGameTarget", 0)
        Engine.getAgent().sendBroadcast(msg)
    if oid == None:
        lastTarget = None
        tagTarget = None
        tagCount = tagCount + 1
    else:
        lastTarget = tagTarget
        tagTarget = oid
        tagCount = tagCount + 1
    if oid != None:
        msg = PropertyMessage(oid)
        msg.put("tagGameTarget", val)
        Engine.getAgent().sendBroadcast(msg)
        startTimer(tagCount)

def maybePassTag(playerOid, targetOid):
    global tagTarget
    if playerOid != tagTarget:
        return
    if not isInClub(playerOid):
        return
    if not isInClub(targetOid):
        return
    setTagTarget(targetOid)

class StartTagGameCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        global tagTarget
        playerOid = cmdEvent.getObjectOid()
        if tagTarget is not None:
            return
        if isInClub(playerOid):
            setTagTarget(playerOid)

class ResetTagCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        setTagTarget(None)

proxyPlugin.registerCommand("/starttag", StartTagGameCommand())
proxyPlugin.registerCommand("/resettag", ResetTagCommand())

def startTimer(count):
    timer = TagTimer(count)
    Engine.getExecutor().schedule(timer, 15, TimeUnit.SECONDS)

class TagTimer (Runnable):
    def __init__(self, count):
        self.count = count

    def run(self):
        global tagCount
        if self.count == tagCount:
            setTagTarget(None)

rand = Random()

Log.debug("extensions_proxy.py: LOADED")
