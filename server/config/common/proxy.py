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

False=0
True=1

proxyPlugin = MarsProxyPlugin();

# register messages that are to be forwarded from the client to the server
proxyPlugin.registerExtensionSubtype("proxy.DYNAMIC_INSTANCE", MessageType.intern("proxy.DYNAMIC_INSTANCE"))
proxyPlugin.registerExtensionSubtype("mv.TRADE_OFFER_REQ", MarsInventoryClient.MSG_TYPE_TRADE_OFFER_REQ)
proxyPlugin.registerExtensionSubtype("mv.SWAP_ITEM", MarsInventoryClient.MSG_TYPE_SWAP_ITEM)
proxyPlugin.registerExtensionSubtype("mv.DESTROY_ITEM", InventoryClient.MSG_TYPE_DESTROY_ITEM)
proxyPlugin.registerExtensionSubtype("mv.REQ_TRAINER_INFO", TrainerClient.MSG_TYPE_REQ_TRAINER_INFO)
proxyPlugin.registerExtensionSubtype("mv.REQ_SKILL_TRAINING", TrainerClient.MSG_TYPE_REQ_SKILL_TRAINING)
proxyPlugin.registerExtensionSubtype("mv.GROUP_INVITE", GroupClient.MSG_TYPE_GROUP_INVITE)
proxyPlugin.registerExtensionSubtype("mv.GROUP_REMOVE_MEMBER", GroupClient.MSG_TYPE_GROUP_REMOVE_MEMBER)
proxyPlugin.registerExtensionSubtype("mv.GROUP_CHAT", GroupClient.MSG_TYPE_GROUP_CHAT)
proxyPlugin.registerExtensionSubtype("mv.GROUP_INVITE_RESPONSE", GroupClient.MSG_TYPE_GROUP_INVITE_RESPONSE)
proxyPlugin.registerExtensionSubtype("mv.GROUP_SET_ALLOWED_SPEAKER", GroupClient.MSG_TYPE_GROUP_SET_ALLOWED_SPEAKER)
proxyPlugin.registerExtensionSubtype("mv.GROUP_MUTE_VOICE_CHAT", GroupClient.MSG_TYPE_GROUP_MUTE_VOICE_CHAT)
proxyPlugin.registerExtensionSubtype("mv.GROUP_VOICE_CHAT_STATUS", GroupClient.MSG_TYPE_GROUP_VOICE_CHAT_STATUS)

# register messages that are to be forwarded from the server to the client
proxyPlugin.addExtraPlayerExtensionMessageType(MarsInventoryClient.MSG_TYPE_TRADE_START)
proxyPlugin.addExtraPlayerExtensionMessageType(MarsInventoryClient.MSG_TYPE_TRADE_COMPLETE)
proxyPlugin.addExtraPlayerExtensionMessageType(MarsInventoryClient.MSG_TYPE_TRADE_OFFER_UPDATE)
proxyPlugin.addExtraPlayerExtensionMessageType(QuestClient.MSG_TYPE_QUEST_LOG_INFO)
proxyPlugin.addExtraPlayerExtensionMessageType(QuestClient.MSG_TYPE_QUEST_INFO)
proxyPlugin.addExtraPlayerExtensionMessageType(QuestClient.MSG_TYPE_REMOVE_QUEST_RESP)
proxyPlugin.addExtraPlayerExtensionMessageType(TrainerClient.MSG_TYPE_TRAINING_INFO)
proxyPlugin.addExtraPlayerExtensionMessageType(CombatClient.MSG_TYPE_TRAINING_FAILED)
proxyPlugin.addExtraPlayerExtensionMessageType(CombatClient.MSG_TYPE_COMBAT_ABILITY_MISSED)
proxyPlugin.addExtraPlayerExtensionMessageType(ClassAbilityClient.MSG_TYPE_STAT_XP_UPDATE)

Engine.registerPlugin(proxyPlugin);
    
class PersistCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()

        cmd = cmdEvent.getCommand()
        splitCmd = cmd.split(" ")
        objOid = int(splitCmd[1])

        Log.debug("PersistCommand: playerOid=" + str(playerOid) + ", objOid=" + str(objOid))

        # send out a set persist message
        ObjectManagerClient.setPersistenceFlag(objOid,
                                               Boolean.TRUE)
        
        WorldManagerClient.sendObjChatMsg(playerOid, 1, "persisted obj " + str(objOid))

class GenerateCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        # The args are template name
        cmd = cmdEvent.getCommand()
        splitCmd = cmd.split(" ")
        templateName = splitCmd[1]
        Log.debug("GenerateCommand: player=" + str(playerOid) + ", template=" + templateName)
        # get the player's loc
        objInfo = WorldManagerClient.getObjectInfo(playerOid)
        loc = objInfo.loc
        loc.add(1000,0,0)
        Log.debug("GenerateCommand: player=" + str(playerOid) + ", loc=" + loc.toString())
        
        # generate the object
        objOid = ObjectManagerClient.generateObject(templateName, loc)
        Log.debug("GenerateCommand: player=" + str(playerOid) + ", generated obj oid=" + str(objOid))

        # spawn object
        rv = WorldManagerClient.spawn(objOid)
        Log.debug("GenerateCommand: player=" + str(playerOid) + ", spawned obj rv=" + str(rv))
        WorldManagerClient.sendObjChatMsg(playerOid, 1, "generated oid=" + str(objOid))

class ThreadStatsCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
	global proxyPlugin;
        cmd = cmdEvent.getCommand()
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        Log.debug("ThreadStatsCommand: playerOid=" + str(playerOid) + ", " + proxyPlugin.getCurrentThreads().toString())
        InventoryClient.activateObject(targetOid,
                                       playerOid)
        WorldManagerClient.sendObjChatMsg(playerOid, 1, "threadstats=" + proxyPlugin.getCurrentThreads().toString())

class ConcludeCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()

        Log.debug("ConcludeCommand: cmd=" + cmd)

        rv = QuestClient.requestConclude(targetOid,
                                         playerOid)
        
class LootCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()

        Log.debug("LootCommand: cmd=" + cmd)

        rv = InventoryClient.lootAll(playerOid,
                                     targetOid)
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "proxy.py:lootAll=" + str(rv))

class SetLocCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        Log.debug("SetLocCommand: cmd=" + cmd)
        splitCmd = cmd.split(" ")
        x = int(splitCmd[1])
        y = int(splitCmd[2])
        z = int(splitCmd[3])
        wnode = BasicWorldNode()
        wnode.setLoc(Point(x,y,z))

        # tell the worldmanager we've moved
        # this should update everyone near me
        WorldManagerClient.updateWorldNode(cmdEvent.getObjectOid(), wnode, True)
    
class GotoCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        playerOid = cmdEvent.getObjectOid()
        Log.debug("GotoCommand: cmd=" + cmd)
        splitCmd = cmd.split(" ")
        markerName = splitCmd[1]
        Log.debug("GotoCommand: got goto command, to marker=" + markerName)
	current = WorldManagerClient.getWorldNode(playerOid)
	instanceOid = current.getInstanceOid()
        marker = InstanceClient.getMarker(instanceOid, markerName)
	if (marker == None):
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "Unknown marker")
	    return

        Log.debug("GotoCommand: marker " + markerName + "=" + marker.toString())
        wnode = BasicWorldNode()
        wnode.setLoc(marker.getPoint())
        wnode.setOrientation(marker.getOrientation())

        # tell the worldmanager we've moved
        # this should update everyone near me
        WorldManagerClient.updateWorldNode(playerOid, wnode, True)


class CreateItemCommand(ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()

        # the template name is whatever is after the space
        template = cmd[cmd.index(' ')+1:]
        Log.debug("CreateItemSubObjCommand: template=" + template)
        playerOid = cmdEvent.getObjectOid()

        bagOid = WorldManagerClient.getObjectProperty(playerOid, InventoryPlugin.INVENTORY_PROP_BAG_KEY)
        Log.debug("CreateItemSubObjCommand: bagOid=" + str(bagOid))

        # generate the object
        Log.debug("CreateItemSubObjCommand: templ=" + template + ", generating object")
        itemOid = ObjectManagerClient.generateObject(template, Template())

        # add to inventory
        Log.debug("CreateItemSubObjCommand: createitem: oid=" + str(itemOid) + ", bagOid=" + str(bagOid) + ", adding to inventory")
        rv = InventoryClient.addItem(playerOid, playerOid, playerOid, itemOid)
        Log.debug("CommandPlugin: createitem: oid=" + str(itemOid) + ", added, rv=" + str(rv))
        WorldManagerClient.sendObjChatMsg(playerOid, 1, "added item" + str(rv))
        
class SetMeshCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        playerOid = cmdEvent.getObjectOid()
        mesh = cmd[cmd.index(' ')+1:]
        submeshes = LinkedList()
        Log.debug("/setmesh: oid=" + str(playerOid) + " to: " + mesh)
        WorldManagerClient.modifyDisplayContext(playerOid,
                                                WorldManagerClient.modifyDisplayContextActionReplace,
                                                mesh,
                                                submeshes)

class ReleaseCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/release: oid=" + str(playerOid))
        CombatClient.releaseObject(playerOid)

class WhoCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        global proxyPlugin;
        playerOid = cmdEvent.getObjectOid()
        playerNames = proxyPlugin.getPlayerNames()
        Log.debug("/who: oid=" + str(playerOid) + ": returning "+str(playerNames.size())+" players")
        response = "players\n------------\n"
        for name in playerNames:
            response = response + name + "\n"
        response = response + str(playerNames.size()) + " players\n"
        WorldManagerClient.sendObjChatMsg(playerOid, 0, response)

class PlayerCountCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        global proxyPlugin;
        playerOid = cmdEvent.getObjectOid()
        playerNames = proxyPlugin.getPlayerNames()
        Log.debug("/playercount: oid=" + str(playerOid) + ": returning "+str(playerNames.size())+" players")
        response = str(playerNames.size()) + " players\n"
        WorldManagerClient.sendObjChatMsg(playerOid, 0, response)

class ParmCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        cmd = cmdEvent.getCommand()
        splitCmd = cmd.split(" ")
        count = (len(splitCmd) - 1) / 2
        i = -1
        msg = WorldManagerClient.TargetedExtensionMessage(playerOid)
        msg.setProperty("ext_msg_subtype", "ClientParameter")
        for s in splitCmd:
            if (i != -1):
                if ((i & 1) == 0):
                    n = s
                else:
                    msg.setProperty(n, s)
            i = i + 1
        Engine.getAgent().sendBroadcast(msg)


#
# separated this out of SysCommand so other worlds can call in after
# world-specific processing (i.e. testing for admin status in Places)
#
def handleSysCommand(cmdEvent):
        cmd = cmdEvent.getCommand()
        msg = cmd[cmd.index(' ')+1:]
        Log.debug("SysCommand: msg=" + msg)

        WorldManagerClient.sendSysChatMsg("System: " + msg)

class SysCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        handleSysCommand(cmdEvent)


class DirLightCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        global proxyPlugin;
        playerOid = cmdEvent.getObjectOid()
        cmd = cmdEvent.getCommand()
        splitCmd = cmd.split(" ")
        x = Float.valueOf(splitCmd[1])
        y = Float.valueOf(splitCmd[2])
        z = Float.valueOf(splitCmd[3])
        Log.debug("DirLightCommand: playerOid=" + str(playerOid) +
                  ", x=" + str(x) +
                  ", y=" + str(y) +
                  ", z=" + str(z))
        grey = Color(140, 128, 128)
        dirlight = DirectionalLight("dirlight")
        dirlight.setDiffuse(grey)
        dirlight.setSpecular(grey)
        dirlight.setAttenuationRange(1000000)
        dirlight.setAttenuationConstant(1)
        dirlight.setAttenuationLinear(0)
        dirlight.setAttenuationQuadradic(0)
        vector = MVVector(x,y,z)
        dirlight.setLightOrientation(Quaternion.fromAngleAxis(-1, vector))
        lightEvent = NewLightEvent()
        lightEvent.setObjectOid(playerOid)
        lightEvent.setLight(dirlight)
        playerOids = proxyPlugin.getPlayerOids()
        for oid in playerOids:
            conMap = proxyPlugin.getConnectionMap()
            con = conMap.getConnection(oid)
            Log.debug("DirLightCommand: sending light " +
                      dirlight.toString() +
                      " to oid " + str(oid))
            con.send(lightEvent.toBytes())

class FindCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        cmd = cmdEvent.getCommand()
        itemTemplate = cmd[cmd.index(' ')+1:]
        itemOid = InventoryClient.findItem(playerOid, itemTemplate)
        if (itemOid == None):
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "item not found")
        else:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, itemTemplate + ":" + str(itemOid))

class FindWeaponCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        itemOid = MarsInventoryClient.findItem(playerOid, MarsEquipSlot.PRIMARYWEAPON)
        if (itemOid == None):
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "item not found")
        else:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "weapon:" + str(itemOid))

class RemoveCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        cmd = cmdEvent.getCommand()
        itemTemplate = cmd[cmd.index(' ')+1:]
        itemOid = InventoryClient.removeItem(playerOid, itemTemplate)
        if (itemOid == None):
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "item not found")
        else:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, itemTemplate + ":" + str(itemOid))

class AbilityCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        cmd = cmdEvent.getCommand()
        abilityName = cmd[cmd.index(' ')+1:]
        CombatClient.startAbility(abilityName, playerOid, targetOid, None)

class NoMoveCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        cmd = cmdEvent.getCommand()
        nomove = cmd[cmd.index(' ')+1:]
        if nomove == "on":
            WorldManagerClient.setObjectProperty(targetOid, "world.nomove", Boolean(True))
        else:
            WorldManagerClient.setObjectProperty(targetOid, "world.nomove", Boolean(False))

class ResetQuestCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        QuestClient.resetQuests(playerOid)

class DumpStacksCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        msg = Message(EnginePlugin.MSG_TYPE_DUMP_ALL_THREAD_STACKS)
        Engine.getAgent().sendBroadcast(msg)

class TradeStartCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        MarsInventoryClient.tradeStart(playerOid, targetOid)

class StuckCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        wnode = BasicWorldNode()
        playerOid = cmdEvent.getObjectOid()
	current = WorldManagerClient.getWorldNode(playerOid)
	instanceOid = current.getInstanceOid()
        marker = InstanceClient.getMarker(instanceOid, "stuck")
        if (marker != None):
            wnode.setLoc(marker.getPoint())
            wnode.setOrientation(marker.getOrientation())
            Log.debug("StuckCommand: marker stuck = " + marker.toString())
        else:
            wnode.setLoc(Point(0, 0, 0))
            Log.debug("StuckCommand: no marker stuck, so going to (0, 0, 0)")
        # tell the worldmanager we've moved
        WorldManagerClient.updateWorldNode(cmdEvent.getObjectOid(), wnode, True)


# tests should send a chat message back with "test success" or "test failure"
class TestCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        playerOid = cmdEvent.getObjectOid()

        cmd = cmdEvent.getCommand()
        splitCmd = cmd.split(" ")
        testName = splitCmd[1]
        if (testName == "persistKey") :
            # example /test persistKey testKey
            key = splitCmd[2]
            self.testKeywordPersistence(playerOid, key)
        else :
            Log.error("TestCommand: unknown test " + testName)

    def testKeywordPersistence(self, playerOid, key) :
        testNamespace = Namespace.intern("testNamespace")
        saveEntity = Entity("saveEntity")
        saveEntity.setProperty("sample_property", "sample_value")
        if (not ObjectManagerClient.saveObjectData(key,
                                                   saveEntity,
                                                   testNamespace)) :
            Log.error("testKeywordPersistence failed on saving")
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "test failure")
            return
        loadEntity = ObjectManagerClient.loadObjectData(key)
        if (loadEntity == None) :
            Log.error("testKeywordPersistence failed on loading, got null")
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "test failure")
            return
        val = loadEntity.getProperty("sample_property")
        if (val != "sample_value") :
            Log.error("testKeywordPersistence failed, value=" + val)
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "test failure")
            return
        Log.error("testKeywordPersistence: test passed")
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "test success")
        
# Display the server's notion of the orientation of a targeted entity as a chat message
class OrientationCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        targetOid = cmdEvent.getTarget()
        targetObj = WorldManagerClient.getObjectInfo(targetOid)
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "Orientation: " + str(targetObj.orient))

class DebugOidCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        cmd = cmdEvent.getCommand().split(" ");
        if len(cmd) > 1:
            cmd = cmd[1]
        else:
            cmd = ""
        objOid = 0
        try:
            if cmd != "":
                objOid = int(cmd)
        except ValueError:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "Invalid OID");
            return
        except TypeError:
            pass
        if objOid == 0:
            World.DEBUG_OID = cmdEvent.getTarget()
        else:
            World.DEBUG_OID = objOid
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "DEBUG_OID = "+str(World.DEBUG_OID))

class TemplatesCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        templateNames = ObjectManagerClient.getTemplateNames()
        Log.debug("/templates: oid=" + str(playerOid) + ": returning "+str(templateNames.size())+" templates")
        response = "Templates\n------------\n"
        for name in templateNames:
            response = response + name + "\n"
        response = response + str(templateNames.size()) + " templates\n"
        WorldManagerClient.sendObjChatMsg(playerOid, 0, response)

class TellCommand(ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split(None, 2)
        playerOid = cmdEvent.getObjectOid()
        player = proxyPlugin.getPlayer(playerOid)
        db = Engine.getDatabase()
        if len(args) < 3:
            WorldManagerClient.sendObjChatMsg(playerOid, 2, args[0] + " <player name> <message>")
            return
        targetName = args[1]
        targetOid = db.getOidByName(targetName, Namespace.WORLD_MANAGER)
        if (targetOid is None):
            WorldManagerClient.sendObjChatMsg(playerOid, 2, targetName + " is not a valid character name.")
        else:
            targetOid = ObjectManagerClient.getNamedObject(None,targetName,ObjectTypes.player)
            if (targetOid != None):
		proxy = Engine.getCurrentPlugin()
		proxy.incrementPrivateChatCount()
		Log.info("ProxyPlugin: CHAT_SENT player=" + player.toString() +
			" to=" + str(targetOid) + " toName=" + targetName +
			" private=true msg=[" + args[2] + "]")
                WorldManagerClient.sendObjChatMsg(playerOid, 6, "[to " + targetName + "]: " + args[2])
                WorldManagerClient.sendObjChatMsg(targetOid, 6, "[from " + player.getName() + "]: " + args[2])
            else:
                WorldManagerClient.sendObjChatMsg(playerOid, 2, targetName + " is not available for chat.")

class GetRegionCommand(ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split(" ")
	format = "xyz"
	ii = 1
	while ii < len(args):
	    arg = args[ii]
	    if arg == "-pxz":
		format = "xz"
	    elif len(arg) > 0 and arg[0] != "-":
		break
	    ii = ii + 1
        playerOid = cmdEvent.getObjectOid()
	current = WorldManagerClient.getWorldNode(playerOid)
	instanceOid = current.getInstanceOid()
	region = InstanceClient.getRegion(instanceOid,args[ii],InstanceClient.REGION_BOUNDARY)
	if region == None:
	    WorldManagerClient.sendObjChatMsg(playerOid, 0, "Unknown region "+args[ii])
	    return
	points = region.getBoundary().getPoints()
	pointString = ""
	for point in points:
	    if pointString != "":
		pointString = pointString + ","
	    if format == "xz":
		pointString = pointString + str(point.getX()) + "," + str(point.getZ())
	    else:
		pointString = pointString + str(point.getX()) + "," + str(point.getY()) + "," + str(point.getZ())
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "POINTS: "+pointString)

class UnknownCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        WorldManagerClient.sendObjChatMsg(playerOid, 0, "Unknown command " + cmdEvent.getCommand())

proxyPlugin.registerCommand("/resetquests", ResetQuestCommand())
proxyPlugin.registerCommand("/nomove", NoMoveCommand())
proxyPlugin.registerCommand("/goto", GotoCommand())
proxyPlugin.registerCommand("/createitem", CreateItemCommand())
proxyPlugin.registerCommand("/setmesh", SetMeshCommand())
proxyPlugin.registerCommand("/setloc", SetLocCommand())
proxyPlugin.registerCommand("/lootall", LootCommand())
proxyPlugin.registerCommand("/release", ReleaseCommand())
proxyPlugin.registerCommand("/conclude", ConcludeCommand())
proxyPlugin.registerCommand("/who", WhoCommand())
proxyPlugin.registerCommand("/playercount", PlayerCountCommand())
proxyPlugin.registerCommand("/parm", ParmCommand())
proxyPlugin.registerCommand("/sys", SysCommand())
proxyPlugin.registerCommand("/dirlight", DirLightCommand())
proxyPlugin.registerCommand("/threadstats", ThreadStatsCommand())
proxyPlugin.registerCommand("/find", FindCommand())
proxyPlugin.registerCommand("/remove", RemoveCommand())
proxyPlugin.registerCommand("/findweapon", FindWeaponCommand())
proxyPlugin.registerCommand("/ability", AbilityCommand())
proxyPlugin.registerCommand("/generate", GenerateCommand())
proxyPlugin.registerCommand("/persist", PersistCommand())
proxyPlugin.registerCommand("/dumpstacks", DumpStacksCommand())
proxyPlugin.registerCommand("/trade", TradeStartCommand())
proxyPlugin.registerCommand("/stuck", StuckCommand())
proxyPlugin.registerCommand("/test", TestCommand())
proxyPlugin.registerCommand("/orientation", OrientationCommand())
proxyPlugin.registerCommand("/debugoid", DebugOidCommand())
proxyPlugin.registerCommand("/templates", TemplatesCommand())
proxyPlugin.registerCommand("/getregion", GetRegionCommand())
proxyPlugin.registerCommand("/tell", TellCommand())
proxyPlugin.registerCommand("/t", TellCommand())
proxyPlugin.registerCommand("/whisper", TellCommand())
proxyPlugin.registerCommand("/w", TellCommand())
proxyPlugin.registerCommand("/pm", TellCommand())
proxyPlugin.registerCommand("/im", TellCommand())
proxyPlugin.registerCommand("/unknowncmd", UnknownCommand())
