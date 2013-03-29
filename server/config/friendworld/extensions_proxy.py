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

from java.util.concurrent import *
from java.util import *
from java.lang import *
from java.net import *
from java.sql import *
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
import time
import sys

driverName = "com.mysql.jdbc.Driver"
Class.forName(driverName)

# photo storage
places_url = "http://places.multiverse.net/"

# host running web database
webdb_host = "webdb.mv-places.com"
# for testing
#webdb_host = "localhost"

ProxyPlugin.MaxConcurrentUsers = 400

ROOM_PLAYER_LIMIT = 50

maxUsersProp = Engine.getProperty("places.max_concurrent_users")
if maxUsersProp != None:
    ProxyPlugin.MaxConcurrentUsers = int(maxUsersProp)

roomLimitProp = Engine.getProperty("places.room_player_limit")
if roomLimitProp != None:
    ROOM_PLAYER_LIMIT = int(roomLimitProp)

AGENT_NAME = Engine.getAgent().getName()
TOKEN_LIFE = 30000 # 30 seconds after which the token expires

def getDomainHost():
    hostName = Engine.getMessageServerHostname()
    if hostName == 'localhost':
        try:
            localMachine = InetAddress.getLocalHost()
            hostName = localMachine.getHostName()
        except UnknownHostException:
            Log.error("getDomainHost: couldn't get host name from local IP address %s" % str(localMachine))
    Log.debug("getDomainHost: hostname = %s" % hostName)
    return hostName

domainHostName = getDomainHost()


class SetMeshCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand();
        playerOid = cmdEvent.getObjectOid()
        meshstring = cmd[cmd.index(' ')+1:]
        submeshes = LinkedList()
        meshlist = meshstring.split()
        basemesh = meshlist[0]
        for i in range(1, len(meshlist)-1, 2):
            submesh = DisplayContext.Submesh(meshlist[i], meshlist[i+1])
            submeshes.add(submesh)
        Log.debug("/setmesh: oid=" + str(playerOid) + " to: " + meshstring)
        WorldManagerClient.modifyDisplayContext(playerOid, WorldManagerClient.ModifyDisplayContextAction.REPLACE, basemesh, submeshes)

class PlayAnimationCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand();
        playerOid = cmdEvent.getObjectOid()
        animation = cmd[cmd.index(' ')+1:]
        Log.debug("/playanimation: oid=" + str(playerOid) + " with: " + animation);
        AnimationClient.playSingleAnimation(playerOid, animation)

class DanceCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/dance: oid=" + str(playerOid))
        if len(args) == 1:
            currentDanceState = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "dancestate")
            newDanceState = 0
            if currentDanceState == 0:
                rand = Random()
                newDanceState = int(rand.nextInt(6)) + 1
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "dancestate", newDanceState)
        elif len(args) == 2:
            if args[1] == "on":
                newDanceState = int(rand.nextInt(6)) + 1
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "dancestate", newDanceState)
            elif args[1] == "off" or args[1] == "0":
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "dancestate", 0)
            else:
                try:
                    newDanceState = int(args[1])
                    if newDanceState >= 1 and newDanceState <= 6:
                        EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "dancestate", newDanceState)
                except:
                    pass

class GestureCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/gesture: oid=" + str(playerOid))
        if len(args) == 1:
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "gesturestate", Boolean(not EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "gesturestate")))
        elif len(args) == 2:
            if args[1] == "on":
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "gesturestate", Boolean(True))
            if args[1] == "off":
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "gesturestate", Boolean(False))

sitList = {
    'low'  : 'ntrl_sit_50cm',
    'med'  : 'ntrl_sit_75cm',
    'high' : 'ntrl_sit_85cm',
    '1'    : 'ntrl_sit_50cm_attd_01_idle_01',
    '2'    : 'ntrl_sit_50cm_attd_02_idle_01',
    '3'    : 'ntrl_sit_50cm_attd_03_idle_01',
    }

class SitCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/sit: oid=" + str(playerOid))
        if len(args) == 1:
            Log.debug("/sit: oid=" + str(playerOid))
            if (not WorldManagerClient.getObjectProperty(playerOid, "sitstate")):
                AnimationClient.playSingleAnimation(playerOid, "sit") # stand to sit
            else:
                # AnimationClient.playSingleAnimation(playerOid, "stand") # sit to stand
                pass
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate", Boolean(not EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate")))
        elif len(args) == 2:
            sitStyle = args[1]
            Log.debug("/sit: oid=" + str(playerOid) + ", sit style=" + sitStyle)

            if sitStyle == "on":
                AnimationClient.playSingleAnimation(playerOid, "sit") # stand to sit
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate", Boolean(True))
                return
            elif sitStyle == "off":
                # AnimationClient.playSingleAnimation(playerOid, "stand") # sit to stand
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate", Boolean(False))
                return
                
            animName = 'sit'
            if sitStyle in sitList.keys():
                animName = sitList[sitStyle]
            if (not WorldManagerClient.getObjectProperty(playerOid, "sitstate")):
                AnimationClient.playSingleAnimation(playerOid, animName) # stand to sit
            else:
                # AnimationClient.playSingleAnimation(playerOid, "stand") # sit to stand
                pass
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate", Boolean(not EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate")))

class GMCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        playerOid = cmdEvent.getObjectOid()
        accountId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "AccountId")
        if isAdmin(accountId):
            Log.debug("/gmmode: oid=" + str(playerOid))
            gmMode = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "GMMode")
            if gmMode == None:
                gmMode = False
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "GMMode", Boolean(not gmMode))

class PropertyCommand(ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        if len(args) == 3:
            playerOid = cmdEvent.getObjectOid()
            Log.debug("/property: oid=" + str(playerOid) + " " + args[1] + " " + args[2])
            propName  = args[1]
            propValue = args[2]
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, propName, propValue)
        if len(args) == 2:
            playerOid = cmdEvent.getObjectOid()
            Log.debug("/property: oid=" + str(playerOid) + " " + args[1])
            propName  = args[1]
            propValue = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, propName)
            WorldManagerClient.sendObjChatMsg(playerOid, 0, str(propValue))

class IgnoreCommand(ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        player = proxyPlugin.getPlayer(playerOid)
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        Log.debug("/ignore: oid=%s; cmd=%s; args=%s" % (str(playerOid), cmd, args))
        # Rest for 2+ but only ignore the first.
        # Additional args may be first name, last name, etc.,
        # for greater ignore granularity in the future.
        if len(args) >= 2:
            result = proxyPlugin.matchingPlayers(player, args[1], True)
            if result is not None:
                oids = result[0]
                if oids is not None and oids.size() > 0:
                    if playerOid in oids: # can't ignore self
                        # This is ugly, but remove(playerOid) doesn't
			# work (playerOid is treated as an index), and
			# indexOf(playerOid) returns -1.
                        for i in range(len(oids)):
                            if playerOid == oids[i]:
                                oids.remove(i)
                                break;
                    # Make sure removing playerOid didn't empty the list.
                    if oids.size() > 0:
                        proxyPlugin.updateIgnoredOids(player, oids, None)
                        WorldManagerClient.sendObjChatMsg(playerOid, 0, "You are now ignoring all characters named %s." % args[1])
                else:
                    WorldManagerClient.sendObjChatMsg(playerOid, 0, "No matches found for %s." % args[1])
            else:
                WorldManagerClient.sendObjChatMsg(playerOid, 0, "No matches found for %s." % args[1])
        else:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "Usage: /ignore playername")

#
# places specific /sys command
# determine admin status of caller, than calls into common/proxy.py
#
class FRW_SysCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        accountId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "AccountId")
        if isAdmin(accountId):
            handleSysCommand(cmdEvent)


proxyPlugin.registerCommand("/setmesh", SetMeshCommand())
proxyPlugin.registerCommand("/playanimation", PlayAnimationCommand())
proxyPlugin.registerCommand("/dance", DanceCommand())
proxyPlugin.registerCommand("/gesture", GestureCommand())
proxyPlugin.registerCommand("/sit", SitCommand())
proxyPlugin.registerCommand("/gmmode", GMCommand())
proxyPlugin.registerCommand("/property", PropertyCommand())
proxyPlugin.registerCommand("/ignore", IgnoreCommand())
proxyPlugin.registerCommand("/sys", FRW_SysCommand())


class YesCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/yes: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_nod")
    
class NoCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/no: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_headshake")
    
class ShrugCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/shrug: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_shrug")
    
class LaughCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/laugh: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_laugh")
    
class WaveCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/wave: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_wave")
    
class BowCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/bow: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_bow")
    
class PointCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/point: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_point")
    
class ClapCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/clap: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_clap")

class CheerCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/cheer: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_cheer")

class AttitudeCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        cmd = cmdEvent.getCommand()
        args = cmd.split()
        animNum = None
        if len(args) > 1:
            try:
                animNum = int(args[1])
            except:
                animNum = 1
        else:
            animNum = 1
        if animNum > 3:
            animNum = 1
        Log.debug("/attitude: oid= %s; cmd=%s" % (str(playerOid), cmd))
        AnimationClient.playSingleAnimation(playerOid, "ntrl_attd_%02d_idle_01" % animNum)

class SetTVUrlCommand(ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        tvOid = cmdEvent.getTarget()
        cmd = cmdEvent.getCommand()
        splitCmd = cmd.split(" ")
        url = splitCmd[1]
        if url != None and (url.startswith("http://") or url.startswith("mms://")):
            WorldManagerClient.setObjectProperty(tvOid,"tv_url", url)
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "TV set to: " + url)
        else:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "Please include http:// or mms:// in the address")
           

proxyPlugin.registerCommand("/yes", YesCommand())
proxyPlugin.registerCommand("/no", NoCommand())
proxyPlugin.registerCommand("/shrug", ShrugCommand())
proxyPlugin.registerCommand("/laugh", LaughCommand())
proxyPlugin.registerCommand("/wave", WaveCommand())
proxyPlugin.registerCommand("/bow", BowCommand())
proxyPlugin.registerCommand("/point", PointCommand())
proxyPlugin.registerCommand("/clap", ClapCommand())
proxyPlugin.registerCommand("/cheer", CheerCommand())
proxyPlugin.registerCommand("/attitude", AttitudeCommand())
proxyPlugin.registerCommand("/attd", AttitudeCommand())
proxyPlugin.registerCommand("/settvurl", SetTVUrlCommand())


def instanceSetObjectProperty(instanceOid, oid, namespace, key, value):
    props = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps")
    objInfo = WorldManagerClient.getObjectInfo(oid)
    objName = objInfo.name # objInfo.getProperty("name")
    objProps = None
    if props.containsKey(objName):
        objProps = props[objName]
    else:
        objProps = HashMap()
    objProps[key] = value
    props[objName] = objProps
    EnginePlugin.setObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps", props)


######################
# Dynamic Instancing #
######################
class DynInstProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        props = event.getPropertyMap()
        DynamicInstancing().handleRequest(props, player, proxy)



def setProfilePhotos(instanceOid):
    roomItemsProps = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps")
    roomOwnerId    = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "AccountId")
    roomStyle      = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomStyle")

    # get photo for room owner
    photoURL = getDBProperty(roomOwnerId, "PhotoURL")
    # get oid for profile_main
    profileMain = roomStyle + "_profile_main"
    profileMainOid = ObjectManagerClient.getNamedObject(instanceOid, profileMain, None)
    Log.debug("[CYC] '%s' oid is %s" % (profileMain, profileMainOid))
    if profileMainOid is None:
        return
    # set pic_url for profile
    roomItemsProps = setObjectProperty(profileMainOid, Namespace.WORLD_MANAGER, "pic_url", photoURL, roomItemsProps)

    # get friendlist
    friendlist = getFriendlist(roomOwnerId)
    i = 0
    for friendId in friendlist:
        # get photo
        photoURL = getDBProperty(friendId, "PhotoURL")
        # set pic_url for friendlist
        i = i + 1
        profileName = roomStyle + "_profile_%02d" % i
        profileOid = ObjectManagerClient.getNamedObject(instanceOid, profileName, None)
        Log.debug("[CYC] '%s' oid is %s" % (profileName, profileOid))
        if profileOid is None:
            return
        roomItemsProps = setObjectProperty(profileOid, Namespace.WORLD_MANAGER, "pic_url", photoURL, roomItemsProps)
        roomItemsProps = setObjectProperty(profileOid, Namespace.WORLD_MANAGER, "AccountId", friendId, roomItemsProps)

    EnginePlugin.setObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps", roomItemsProps)



#
# Separate class allows instancing to be called outside the hook
# (i.e. kicking a player to the default instance).
#
class DynamicInstancing:
    def handleRequest(self, props, player, proxy):
        cmd = None
        if props.containsKey("command"):
            cmd = props["command"]

        if cmd == "collectible":
            self.addCollectible(props, player, proxy)

        if (cmd == "instance") or (cmd == "load"):
            Log.debug("processExtensionEvent (dyninst): cmd =" + cmd)

            markerName = ""
            if props.containsKey("markerName"):
                markerName = props["markerName"]
            else:
                markerName = "spawnPt"
            instanceName = ""
            if props.containsKey("instanceName"):
                instanceName = props["instanceName"]

            owner = None
            if props.containsKey("owner"):
                owner = props["owner"]
                db = Engine.getDatabase()
                try:
                    accountId = int(owner)
                except:
                    ownerOid = db.getOidByName(owner, Namespace.WORLD_MANAGER)
                    accountId = EnginePlugin.getObjectProperty(ownerOid, Namespace.WORLD_MANAGER, "AccountId")
                instanceName = "room-" + str(accountId)

	    instanceOid = self.loadInstance(props, player, proxy, instanceName)
	    if instanceOid == None:
		WorldManagerClient.sendObjChatMsg(player.getOid(), 0, "Player does not have a room.")
		return

            if (cmd == "instance"):
                success = self.enterInstance(props, player, proxy, instanceName, markerName)
                if success:
                    playerOid = player.getOid()
                    roomOwnerId = None    # default instance
                    if owner is not None: # room instance
                        roomOwnerId = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "AccountId")
                    EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "roomOwnerId", roomOwnerId)

    def loadInstance(self, props, player,  proxy, instanceName):
        instanceOid = InstanceClient.getInstanceOid(instanceName)
        if instanceOid is None:
            Log.error("Error loading instance "+instanceName)
            return None
        while True:
            result = InstanceClient.loadInstance(instanceOid)
            if result != InstanceClient.RESULT_ERROR_RETRY:
                break
            time.sleep(1)
        if result != InstanceClient.RESULT_OK:
            Log.error("Error loading instance "+str(instanceOid)+", result "+str(result))

        if instanceName.find("room-") == 0:
            setProfilePhotos(instanceOid)

	return instanceOid

    def enterInstance(self, props, player, proxy, instanceName, markerName):
        instanceOid = proxyPlugin.getInstanceEntryCallback().selectInstance(player,instanceName)
	if instanceOid == None:
	    return False

        if instanceName.find("room-") == 0:
            setProfilePhotos(instanceOid)

        if (instanceOid is not None):
            loc = InstanceClient.getMarkerPoint(instanceOid, markerName)
            wnode = BasicWorldNode()
            wnode.setInstanceOid(instanceOid)
            rand = Random()
            newloc = Point((loc.getX() + (int(rand.nextFloat() * 4000.0) - 2000)),
                           (loc.getY()),
                           (loc.getZ() + (int(rand.nextFloat() * 4000.0) - 2000)))
            wnode.setLoc(newloc)
            wnode.setDir(MVVector(0,0,0))
	    return InstanceClient.objectInstanceEntry(player.getOid(),wnode,0)
	return False

    def addCollectible(self, props, player, proxy):
        Log.debug("makeCollectible (dyninst): loveseat")
        playerOid = player.getOid()
        loc      = Point(props["loc"])
        dir      = props["dir"]
        meshname = props["mesh_name"]
        itemname = props["item_name"]
        pWNode = WorldManagerClient.getWorldNode(playerOid)
        instanceOid = pWNode.getInstanceOid()
        iInfo = InstanceClient.getInstanceInfo(instanceOid, InstanceClient.FLAG_NAME)
        dc = DisplayContext(meshname, True)
        ot = Template("furniture") # template name
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, dc)
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_NAME, itemname)
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, Long(instanceOid)) # -- instance OID
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC, loc) # player location + 2m in the Z-axis
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_ORIENT, dir) # player orientation
        # ot.put(Namespace.WORLD_MANAGER, "Targetable", Boolean(True))
        # ot.put(Namespace.WORLD_MANAGER, "ClickHookName", "furniture_menu")
        ot.put(Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True))
        objectOid = ObjectManagerClient.generateObject("furniture", ot) # template name
        rv = WorldManagerClient.spawn(objectOid)
        Log.debug("dynamic instance: generated obj oid = " + str(objectOid))
        return objectOid

proxyPlugin.addProxyExtensionHook("proxy.DYNAMIC_INSTANCE", DynInstProxyExtHook())

class PlacesInstanceEntryCallback (InstanceEntryCallback):
    def instanceEntryAllowed(self, playerOid, instanceOid, location):
	Log.info("PlacesInstanceEntryCallback: playerOid="+str(playerOid)+" "+
		"instanceOid="+str(instanceOid)+" loc="+str(location))
	info = None
	# Get the instance name.  In the case of a room, we can extract
	# the owner's account id.
	instanceName = Engine.getDatabase().getObjectName(instanceOid, InstanceClient.NAMESPACE)
	if instanceName == None:
	    info = InstanceClient.getInstanceInfo(instanceOid,
		InstanceClient.FLAG_PLAYER_POPULATION | InstanceClient.FLAG_NAME)
	    if info == None or info.name == None:
		Log.debug("PlacesInstanceEntryCallback: Could not get instance information for instanceOid="+str(instanceOid))
		return False
	    instanceName = info.name

	if instanceName.find("room-") != 0:
	    return True
	ownerAccountId = instanceName[5:]

	# Get the player's account id
	playerAccountId = EnginePlugin.getObjectProperty(playerOid, Namespace.OBJECT_MANAGER, "AccountId")
	# HACK for backward compatibility: if no AccountId, then allow
	if playerAccountId == None:
	    return True
	# Player can always enter their own room
	if playerAccountId == int(ownerAccountId):
	    return True
	if not self.playerAllowedEntry(ownerAccountId, playerAccountId):
	    Log.debug("PlacesInstanceEntryCallback: playerAllowed returned false for accountId " + str(playerAccountId))
	    WorldManagerClient.sendObjChatMsg(playerOid, 0, "Privacy settings for room '" + instanceName + "' don't allow you to enter")
	    return False

	# Get instance population and check limit
	if info == None:
	    info = InstanceClient.getInstanceInfo(instanceOid, InstanceClient.FLAG_PLAYER_POPULATION)
	limit = EnginePlugin.getObjectProperty(instanceOid, InstanceClient.NAMESPACE, "populationLimit")
	if limit == None:
	    limit = ROOM_PLAYER_LIMIT
	if info.playerPopulation >= limit:
	    WorldManagerClient.sendObjChatMsg(playerOid, 0, "Room is full, try again later.")
	    Log.info("ProxyPlugin: INSTANCE_FULL playerOid=" + str(playerOid) +
		" instanceOid=" + str(instanceOid) +
		" ownerAccountId=" + str(ownerAccountId) +
		" limit=" + str(limit))
	    return False
	else:
	    return True
	return True

    def playerAllowedEntry(self, ownerAccountId, friendAccountId):
	privacy_setting = "Anyone"
	is_friend = 0
	logPrefix = "playerAllowedEntry: For ownerAccountId " + str(ownerAccountId) + " and friendAccountId " + str(friendAccountId)
	sql = "SELECT p.value, IF (EXISTS (SELECT 1 FROM friends AS f WHERE f.my_id = %d AND f.friend_id = %d) ,1,0) AS is_friend FROM profile AS p WHERE p.account_id = %d AND p.property = 'Privacy'" % (ownerAccountId, friendAccountId, ownerAccountId)
	try:
	    url = "jdbc:mysql://"+webdb_host+"/friendworld?user=root&password=test"
	    # Get a row with two columns: the value of the 'Privacy' property for the profile table, and whether friendIs is a friend
	    con = DriverManager.getConnection(url)
	    stm = con.createStatement()
	    srs = stm.executeQuery(sql)
	    if (srs.next()):
		privacy_setting = srs.getString("value")
		is_friend = srs.getInt("is_friend")
		#Log.debug(logPrefix + privacy_setting + " and is_friend = " + str(is_friend))
	    else:
		# If there were no rows returned, that means we should use the default value of "Anyone"
		#Log.debug(logPrefix + ", didn't find a 'Privacy' row in the properties table")
		privacy_setting = "Anyone"
	    srs.close()
	    stm.close()
	    con.close()
	except:
	    Log.debug("playerAllowedEntry: Got exception running database query to retrieve privacy permission for account " + 
                   str(ownerAccountId) + ", sql is " + sql + ", exception " + str(sys.exc_info()[0]))
	if privacy_setting == "Anyone":
	    Log.debug(logPrefix + ", allowing entry because the privacy setting is 'Anyone'")
	    return True
	if (privacy_setting == "Friends"):
	    if is_friend == 1:
		Log.debug(logPrefix + ", allowing entry because the privacy setting is 'Friends' and he is a friend")
		return True
	    else:
		Log.debug(logPrefix + ", not allowing entry because the privacy setting is 'Friends' and he is not a friend")
		return False
	else:
	    Log.debug(logPrefix + ", not allowing entry because the privacy setting is '" + privacy_setting + "'")
	    return False

    def selectInstance(self,player,instanceName):
	infos = InstanceClient.getInstanceInfoByName(instanceName,
	    InstanceClient.FLAG_PLAYER_POPULATION)
	if infos.size() == 0:
	    Log.error("PlacesInstanceEntryCallback: unknown instance name " +
		instanceName)
	    return None
	if infos.size() == 1:
	    return infos.get(0).oid
	selected = None
	for info in infos:
	    if selected == None or info.playerPopulation > selected.playerPopulation:
		limit = EnginePlugin.getObjectProperty(info.oid,
		    InstanceClient.NAMESPACE, "populationLimit")
		if limit == None:
		    limit = ROOM_PLAYER_LIMIT
		if info.playerPopulation < limit:
		    selected = info
	if selected != None:
	    return selected.oid
	else:
	    Log.error("PlacesInstanceEntryCallback: all instances full name=" +
		instanceName)
	    return None



proxyPlugin.setInstanceEntryCallback(PlacesInstanceEntryCallback())


#####
#
#####
def setObjectProperty(oid, namespace, key, value, props):
    objInfo = WorldManagerClient.getObjectInfo(oid)
    objName = objInfo.name # objInfo.getProperty("name")
    objProps = None
    if props.containsKey(objName):
        objProps = props[objName]
    else:
        objProps = HashMap()
    objProps[key] = value
    props[objName] = objProps
    EnginePlugin.setObjectProperty(oid, namespace, key, value)
    return props

class SetPropertyProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        playerOid = player.getOid()
        pWNode = WorldManagerClient.getWorldNode(playerOid)
        instanceOid = pWNode.getInstanceOid()

        # security check -- check if player is instance owner
        isOwner = False
        accountId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "AccountId")
        instanceName = InstanceClient.getInstanceInfo(instanceOid, InstanceClient.FLAG_NAME).name
        instanceOwnerStr = instanceName[instanceName.index('-')+1:]
        instanceOwner = Integer.parseInt(instanceOwnerStr)
        if instanceOwner == accountId:
            isOwner = True
        
        props = event.getPropertyMap()
        roomItemsProps = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps")
        if 'tv_url' in props.keySet():
            oid = props['oid']
            url = props['tv_url']
            roomItemsProps = setObjectProperty(oid, Namespace.WORLD_MANAGER, "tv_url", url, roomItemsProps)
        if 'radio_url' in props.keySet():
            oid = props['oid']
            url = props['radio_url']
            roomItemsProps = setObjectProperty(oid, Namespace.WORLD_MANAGER, "radio_url", url, roomItemsProps)
        if 'pic_url' in props.keySet() and isOwner:
            oid = props['oid']
            url = props['pic_url']
            roomItemsProps = setObjectProperty(oid, Namespace.WORLD_MANAGER, "pic_url", url, roomItemsProps)
        if 'cd_url' in props.keySet() and isOwner:
            oid = props['oid']
            url = props['cd_url']
            name = props['tooltip']
            roomItemsProps = setObjectProperty(oid, Namespace.WORLD_MANAGER, "cd_url", url, roomItemsProps)
            roomItemsProps = setObjectProperty(oid, Namespace.WORLD_MANAGER, "tooltip", name, roomItemsProps)
        if 'subsurface' in props.keySet() and isOwner:
            objOid         = props['oid']
            subsurfaceName = props['subsurface']
            subsurface     = props['value']
            roomItemsProps = setObjectProperty(objOid, Namespace.WORLD_MANAGER, subsurfaceName, subsurface, roomItemsProps)
            roomItemsProps = setObjectProperty(objOid, Namespace.WORLD_MANAGER, 'AppearanceOverride', 'coloredfurniture', roomItemsProps)
        ######
#         if 'hide' in props.keySet():
#             for pair in props['hide']:
#                 roomItemsProps = self.setObjectProperty(pair[0], Namespace.WORLD_MANAGER, 'Hide', Boolean(pair[1]), roomItemsProps)
#         if 'style' in props.keySet():
#             objOid = props['oid']
#             style  = props['style']
#             roomItemsProps = self.setObjectProperty(objOid, Namespace.WORLD_MANAGER, 'RoomStyle', style, roomItemsProps)
        ######
        EnginePlugin.setObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps", roomItemsProps)
            

proxyPlugin.addProxyExtensionHook("mv.SET_PROPERTY", SetPropertyProxyExtHook())


#
# convenience function used solely to determine whether the SELECT
# finds a match - note we append "LIMIT 1" to the passed query, to
# return only a single match
#
# returns True (there was a match), or False (there were no matches)
#
def doesQueryMatch(sql):
    result = False
    url = "jdbc:mysql://%s/friendworld?user=root&password=test" % webdb_host
    con = DriverManager.getConnection(url)
    stm = con.createStatement()
    sql = "%s LIMIT 1" % sql
    res = stm.executeQuery(sql)
    if res.next():
        result = True
    stm.close()
    con.close()
    return result

#
# convenience function used to perform an INSERT, UPDATE or DELETE
# on the web database
#
# returns number of rows affected by the update
#
def updateDatabase(sql):
    result = 0
    url = "jdbc:mysql://%s/friendworld?user=root&password=test" % webdb_host
    con = DriverManager.getConnection(url)
    stm = con.createStatement()
    result = stm.executeUpdate(sql)
    stm.close()
    con.close()
    return result


class AddFriendProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        Log.debug("[CYC] add friend proxy hook")
        playerOid = player.getOid()
        pWNode = WorldManagerClient.getWorldNode(playerOid)
        instanceOid = pWNode.getInstanceOid()
        props = event.getPropertyMap()
        friendAccountId = None
        if props.containsKey('friend_id'):
            friendAccountId = props['friend_id']
        friendOid = None
        if props.containsKey('friend_oid'):
            friendOid = props['friend_oid']
        myAccountId = None
        if props.containsKey('account_id'):
            myAccountId = props['account_id']
        Log.debug("[CYC] %s, %s, %s" % (friendAccountId, friendOid, myAccountId))
        if friendAccountId is None or friendOid is None or myAccountId is None:
            return

        #
        # so we can provide the player with useful feedback
        #
        friendName = proxyPlugin.getPlayer(friendOid).name

        #
        # don't add a friend invite if...
        #
        # we're already friends
        if doesQueryMatch("SELECT friend_id FROM friends WHERE my_id = %d AND friend_id = %d" % (myAccountId, friendAccountId)):
            WorldManagerClient.sendObjChatMsg(playerOid, 2, "You're already friends with %s." % friendName)
            return

        # i've already invited this person to become friends
        haveInvited = doesQueryMatch("SELECT to_id, from_id FROM invitations WHERE to_id = %d AND from_id = %d" % (friendAccountId, myAccountId))
        if haveInvited:
            WorldManagerClient.sendObjChatMsg(playerOid, 2, "You've already sent %s a friend request." % friendName)
            return

        #
        # if this person has previously invited me to become friends,
        # treat 'add friend' as a confirmation - add as friend, and
        # remove any mutual invitations
        #
        if doesQueryMatch("SELECT to_id, from_id FROM invitations WHERE to_id = %d AND from_id = %d" % (myAccountId, friendAccountId)):
            result = updateDatabase("INSERT INTO friends (my_id, friend_id, timestamp) VALUES (%d, %d, NOW())" % (myAccountId, friendAccountId))
            result = updateDatabase("INSERT INTO friends (my_id, friend_id, timestamp) VALUES (%d, %d, NOW())" % (friendAccountId, myAccountId))
            result = updateDatabase("DELETE FROM invitations WHERE to_id = %d AND from_id = %d" % (myAccountId, friendAccountId))
            if haveInvited:
                result = updateDatabase("DELETE FROM invitations WHERE to_id = %d AND from_id = %d" % (friendAccountId, myAccountId))
            WorldManagerClient.sendObjChatMsg(playerOid, 2, "You are now friends with %s." % friendName)
            return

        Log.debug("[CYC] adding friend ... db call")
        # Add friend
        message = ""
        url = "jdbc:mysql://"+webdb_host+"/friendworld?user=root&password=test"
        sql = "INSERT INTO invitations (to_id, from_id, message, timestamp) VALUES (%s, %s, '%s', NOW())" % (friendAccountId, myAccountId, message)
        con = DriverManager.getConnection(url)
        stm = con.createStatement()
        res = stm.executeUpdate(sql)
        Log.debug("[CYC] add friend insert result = %d" % res)
        stm.close()
        con.close()
        Log.debug("[CYC] sending friend request message")

        # Send friend message
        WorldManagerClient.sendObjChatMsg(playerOid, 2, "You have sent a friend request to %s." % friendName)
        WorldManagerClient.sendObjChatMsg(friendOid, 2, "You have a new friend request from %s." % player.name)

proxyPlugin.addProxyExtensionHook("mvp.ADD_FRIEND", AddFriendProxyExtHook())


class KickPlayerProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        playerOid = player.getOid()
        # get player's accountId
        accountId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "AccountId")
        Log.debug("KickHook: kick request from playerOid=%d, accountId=%d" % (playerOid, accountId))
        # get room's ownerId
        roomOwnerId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "roomOwnerId")
        # kicking player must own the room or be an admin
        adminState = str(getDBProperty(accountId, "Admin"))
        if accountId != roomOwnerId and adminState != "True":
            WorldManagerClient.sendObjChatMsg(playerOid, 2, "Sorry, you don't have permission to kick that player.")
            return
        # validate kick target
        props = event.getPropertyMap()
        kickOid = None
        if props.containsKey('oid'):
            kickOid = props['oid']
        if kickOid is None:
            return
        # don't let owner be kicked from their own room
        kickAccountId = EnginePlugin.getObjectProperty(kickOid, Namespace.WORLD_MANAGER, "AccountId")
        if kickAccountId == roomOwnerId:
            WorldManagerClient.sendObjChatMsg(playerOid, 2, "Sorry, can't kick a player from their own room.")
            return
        # bad target, go away!
        props = HashMap()
        props.put("command", "instance")
        props.put("instanceName", "default")
        props.put("markerName", "spawnPt")
        kickedPlayer = proxyPlugin.getPlayer(kickOid)
        Log.debug("KickHook: kicking kickOid=%d (%s)" % (kickOid, kickedPlayer.name))
        DynamicInstancing().handleRequest(props, kickedPlayer, proxy)
        WorldManagerClient.sendObjChatMsg(playerOid, 2, "%s has been kicked from the room." % kickedPlayer.name)
        WorldManagerClient.sendObjChatMsg(kickOid, 2, "You have been kicked from the room.")

proxyPlugin.addProxyExtensionHook("mvp.KICK_FROM_ROOM", KickPlayerProxyExtHook())


proxyPlugin.addProxyExtensionHook("proxy.INSTANCE_ENTRY", InstanceEntryProxyHook())


def getDBProperty(accountId, property):
    value = None
    try:
        url = "jdbc:mysql://"+webdb_host+"/friendworld?user=root&password=test"
        sql = "SELECT value FROM profile WHERE account_id = %d AND property = '%s'" % ( accountId, property)
        con = DriverManager.getConnection(url)
        stm = con.createStatement()
        srs = stm.executeQuery(sql)
        # _types = {Types.INTEGER:srs.getInt, Types.FLOAT:srs.getFloat}
        while (srs.next()):
            value = srs.getString(1)
        srs.close()
        stm.close()
        con.close()
    except:
        Log.debug("getDBProperty(): Exception")
        pass
    if value is None:
        if property == "PhotoURL":
            # value = places_url + "images/missing.jpg"
            value = places_url + "photos/%08d.jpg" % accountId
        else:
            value = "Unknown"
    Log.debug("getDBProperty(): accountId=%d, property=%s, value=%s" % (accountId, property, value))
    return value

#
# Simple test to see if the player is an admin.
#
def isAdmin(accountId):
    result = False
    state = getDBProperty(accountId, "Admin")
    if state == "True":
        result = True
    return result


def getFriendlist(accountId):
    friendList = LinkedList()
    try:
        url = "jdbc:mysql://"+webdb_host+"/friendworld?user=root&password=test"
        sql = "SELECT friend_id FROM friends WHERE my_id = %d LIMIT 12" % accountId
        con = DriverManager.getConnection(url)
        stm = con.createStatement()
        srs = stm.executeQuery(sql)
        # _types = {Types.INTEGER:srs.getInt, Types.FLOAT:srs.getFloat}
        while (srs.next()):
            friend_id = srs.getInt(1)
            friendList.add(str(friend_id))
        srs.close()
        stm.close()
        con.close()
    except:
        friendList.add(1)
        # friendList.add(2156)
        # friendList.add(7811)
    return friendList

def getPlaylist(accountId):
    playList = LinkedList()
    try:
        url = "jdbc:mysql://"+webdb_host+"/friendworld?user=root&password=test"
        sql = "SELECT name, URL FROM media WHERE account_id = %d AND media_type=1" % accountId
        con = DriverManager.getConnection(url)
        stm = con.createStatement()
        srs = stm.executeQuery(sql)
        # _types = {Types.INTEGER:srs.getInt, Types.FLOAT:srs.getFloat}
        while (srs.next()):
            name = srs.getString(1)
            url  = srs.getString(2)
            nvpair = LinkedList()
            nvpair.add(name)
            nvpair.add(url)
            playList.add(nvpair)
        srs.close()
        stm.close()
        con.close()
    except:
        nvpair = LinkedList()
        nvpair.add("Slick Rick 1")
        nvpair.add("http://www.tradebit.com/usr/scheme05/pub/8/Chamillionaire-feat.-Slick-Rick---Hip-Hop-Police.mp3")
        playList.add(nvpair)
    return playList

class GetPropertyProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        props = event.getPropertyMap()
        oid = None
        if props.containsKey("oid"):
            oid = props["oid"]
        else:
            oid = player.getOid()
        accountId = None
        if props.containsKey("account_id"):
            accountId = props["account_id"]
        else:
            accountId = EnginePlugin.getObjectProperty(oid, Namespace.WORLD_MANAGER, "AccountId")
            if accountId is None:
                accountId = EnginePlugin.getObjectProperty(oid, Namespace.WORLD_MANAGER, "roomOwnerId")
        propKey = None
        if props.containsKey("property_name"):
            propKey = props["property_name"]
        else:
            propKey = "PhotoURL"
        cmd = None
        if props.containsKey("cmd"):
            cmd = props["cmd"]
        if (accountId is not None) and (oid is not None):
            if (cmd == "property"):
                propValue = getDBProperty(accountId, propKey)
                EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, propKey, propValue)
            if (cmd == "friendlist"):
                friend_list = getFriendlist(accountId)
                EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, "friendlist", friend_list)
            if (cmd == "playlist"):
                play_list = getPlaylist(accountId)
                EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, "playlist", play_list)
            if (cmd == "roomstyle"):
                room_style = EnginePlugin.getObjectProperty(Instance.currentOid(), Namespace.INSTANCE, "RoomStyle")
                EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, "roomstyle", room_style)
            if (cmd == "room_owner_id"):
                roomOwnerId = EnginePlugin.getObjectProperty(Instance.currentOid(), Namespace.INSTANCE, "AccountId")
                EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, "roomOwnerId", roomOwnerId)

proxyPlugin.addProxyExtensionHook("mv.GET_PROPERTY", GetPropertyProxyExtHook())

class UpdateObjectProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        props = event.getPropertyMap()
        dir = None
        if props.containsKey("dir"):
            dir = props["dir"]
            transition = None
            if props.containsKey("transition"):
                transition = props["transition"]
            idle = None
            if props.containsKey("idle"):
                idle = props["idle"]
            loc_start = None
            if props.containsKey("loc_start"):
                loc_start = props["loc_start"]
            if (transition is not None) and (idle is not None) and (loc_start is not None):
                wnode_start = BasicWorldNode()
                wnode_start.setLoc(Point(loc_start))
                wnode_start.setOrientation(dir)
                playerOid = player.getOid()
                WorldManagerClient.updateWorldNode(playerOid, wnode_start, True)
                AnimationClient.playSingleAnimation(playerOid, transition)
                # wnode_end = BasicWorldNode()
                # wnode_end.setLoc(Point(loc_end))
                # wnode_end.setOrientation(dir)
                # WorldManagerClient.updateWorldNode(playerOid, wnode_end, True)
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitidle", idle)
                EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "sitstate", Boolean(True))

        if props.containsKey("property"):
            oid = None
            if props.containsKey("oid"):
                oid = props["oid"]
            property = props["property"]
            value = None
            if props.containsKey("value"):
                value = props["value"]
            if (oid is not None) and (property is not None) and (value is not None):
                EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, property, value)

proxyPlugin.addProxyExtensionHook("mv.UPDATE_OBJECT", UpdateObjectProxyExtHook())


class PlacesLoginCallback (ProxyLoginCallback):
    def preLoad(self, player, conn):
        pass

    def postLoad(self, player, conn):
        #
        # setting "isAdmin" on the player object will let us appropriately
        # update UI elements on the client where only admins should be able
        # to perform an operation - note that this should only be used for
        # UI, no to determine permission to perform an operation - admin
        # requests should *ALWAYS* be confirmed on the world server
        #
        playerOid = player.getOid()
        accountId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "AccountId")
        EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "isAdmin", isAdmin(accountId))

    def postSpawn(self, player, conn):
        Log.debug("[CYC] postSpawn")
        playerOid = player.getOid()
        pWNode = WorldManagerClient.getWorldNode(playerOid)
        instanceOid = pWNode.getInstanceOid()
        iInfo = InstanceClient.getInstanceInfo(instanceOid, InstanceClient.FLAG_NAME)
        instanceName = iInfo.name
        if instanceName.find("room-") == 0:
            setProfilePhotos(instanceOid)
            roomOwnerId = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "AccountId")
            EnginePlugin.setObjectProperty(playerOid, Namespace.WORLD_MANAGER, "roomOwnerId", roomOwnerId)

proxyPlugin.setProxyLoginCallback(PlacesLoginCallback())


def generateToken(props=None):
    expiry = System.currentTimeMillis() + TOKEN_LIFE
    if props is None:
        tokenSpec = SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_DOMAIN, AGENT_NAME, expiry)
    else:
        tokenSpec = SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_DOMAIN, AGENT_NAME, expiry, props)
    token = SecureTokenManager.getInstance().generateToken(tokenSpec)
    return token


class GenerateTokenProxyExtHook (ProxyExtensionHook):
    def processExtensionEvent(self, event, player, proxy):
        playerOid = player.getOid()
        eventProps = event.getPropertyMap()
        if not 'frameName' in eventProps or not 'jspArgs' in eventProps:
            WorldManagerClient.sendObjChatMsg(playerOid, 0, "GTPExtHook request failed: Bad data passed to server.")
            return

        # get player's accountId
        accountId = EnginePlugin.getObjectProperty(playerOid, Namespace.WORLD_MANAGER, "AccountId")
        Log.debug("GenerateTokenHook: token requested by playerOid=%d, accountId=%d" % (playerOid, accountId))
        props = HashMap()
        props.put("accountId", accountId)
        token = generateToken(props=props)
        token64 = Base64.encodeBytes(token, Base64.URL_SAFE)
        Log.debug("GenerateTokenHook: token64 = %s" % token64)

        msg = WorldManagerClient.TargetedExtensionMessage("mvp.TOKEN_GENERATED", playerOid)
        msgProps = msg.getPropertyMapRef()
        # need to send these back to the client
        jspArgs = eventProps['jspArgs']
        jspArgs = "%s&host=%s&token=%s" % (jspArgs, domainHostName, token64)
        msgProps.put("jspArgs", jspArgs)
        msgProps.put("frameName", eventProps['frameName'])
        Engine.getAgent().sendBroadcast(msg)


proxyPlugin.addProxyExtensionHook("mvp.GENERATE_TOKEN", GenerateTokenProxyExtHook())
