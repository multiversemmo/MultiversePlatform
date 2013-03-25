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

Log.debug("extensions_anim.py: Loading...")

MSG_TYPE_JUKEBOX_SET_ID = MessageType.intern("jukeboxSetId")
MSG_TYPE_JUKEBOX_GET_ID = MessageType.intern("jukeboxGetId")
MSG_TYPE_JUKEBOX_GET_TRACKS = MessageType.intern("jukeboxGetTracks")
MSG_TYPE_JUKEBOX_ADD_TRACK = MessageType.intern("jukeboxAddTrack")
MSG_TYPE_JUKEBOX_DELETE_TRACK = MessageType.intern("jukeboxDeleteTrack")
MSG_TYPE_JUKEBOX_GET_FUNDS = MessageType.intern("jukeboxGetFunds")
MSG_TYPE_JUKEBOX_ADD_FUNDS = MessageType.intern("jukeboxAddFunds")
MSG_TYPE_JUKEBOX_PLAY = MessageType.intern("jukeboxPlay")
MSG_TYPE_GET_ALL_USERS = MessageType.intern("GET_ALL_USERS")

trackData = [
    { "name" : "Video: Satisfaction",
      "type" : "movie",
      "url" : "http://video.multiverse.net/movies/satisfaction.asf",
      "cost" : "0",
      "description" : "Temporary movie for demo only"
      },
    { "name" : "Video: Bush N Blair",
      "type" : "movie",
      "url" : "http://video.multiverse.net/movies/bushnblair.asf",
      "cost" : "0",
      "description" : "Temporary movie for demo only"
      },
    { "name" : "Vocal Trance",
      "type" : "stream",
      "url" : "http://scfire-chi-aa03.stream.aol.com:80/stream/1065",
      "cost" : "0",
      "description" : "DIGITALLY IMPORTED - Vocal Trance - a fusion of trance, dance, and chilling vocals"
      },
    { "name" : "Groove Salad",
      "type" : "stream",
      "url" : "http://scfire-nyk-aa02.stream.aol.com:80/stream/1018",
      "cost" : "0",
      "description" : "Groove Salad: a nicely chilled plate of ambient beats and grooves. [Soma FM]"
      },
    { "name" : "The 80s Channel",
      "type" : "stream",
      "url" : "http://scfire-ntc-aa01.stream.aol.com:80/stream/1040",
      "cost" : "0",
      "description" : ".997 The 80s Channel"
      },
    { "name" : "Radio Paradise",
      "type" : "stream",
      "url" : "http://scfire-chi-aa03.stream.aol.com:80/stream/1048",
      "cost" : "0",
      "description" : "Radio Paradise - DJ-mixed modern & classic rock, world, electronica & more - info: radioparadise.com"
      },
    { "name" : "The Hitz Channel",
      "type" : "stream",
      "url" : "http://scfire-chi-aa01.stream.aol.com:80/stream/1074",
      "cost" : "0",
      "description" : ".997 The Hitz Channel"
      },
    { "name" : "SKY.FM",
      "type" : "stream",
      "url" : "http://scfire-chi-aa03.stream.aol.com:80/stream/1010",
      "cost" : "0",
      "description" : "SKY.FM - Absolute Smooth Jazz - the world's smoothest jazz 24 hours a day"
      },
    { "name" : "-=[:: HOT 108 JAMZ ::]=-",
      "type" : "stream",
      "url" : "http://scfire-nyk-aa04.stream.aol.com:80/stream/1071",
      "cost" : "0",
      "description" : "-=[:: HOT 108 JAMZ ::]=- #1 FOR HIP HOP -128K HD) * CONNECT FROM OUR WEBSITE www.hot108.com"
      },
    { "name" : "Video: Life Goes On",
      "type" : "movie",
      "url" : "http://video.multiverse.net/movies/lagunabeach.asf",
      "cost" : "0",
      "description" : "Artist: Mr. Kane"
      },
    { "name" : "DJ Stevie",
      "type" : "stream",
      "url" : "http://ct5.fast-serv.com:8804/",
      "cost" : "0",
      "description" : "For Demo purposes - Live DJ stream"
      },
    ]

jukeboxes = {}

class JukeboxSetIdHook(Hook):
    def processMessage(self, msg, flags):
        jukeboxId = msg.getProperty("id")
        respMsg = ResponseMessage(msg)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxGetIdHook(Hook):
    def processMessage(self, msg, flags):
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(jukeboxId)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxGetTracksHook(Hook):
    def processMessage(self, msg, flags):
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(trackData)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxAddTrackHook(Hook):
    def processMessage(self, msg, flags):
        Log.debug("JukeboxAddTrackHook: got an add request, msg type is " + msg.getMsgTypeString())
        name = msg.getProperty("name")
        inList = False
        for i in range(0, len(trackData)):
            trackName = trackData[i]["name"]
            if (trackName == name):
                inList = True
        Log.debug("JukeboxAddTrackHook: inList? " + str(inList))
        if (not inList):
            type = msg.getProperty("type")
            Log.debug("JukeboxAddTrackHook: type=" + type)
            url = msg.getProperty("url")
            Log.debug("JukeboxAddTrackHook: url=" + url)
            cost = msg.getProperty("cost")
            Log.debug("JukeboxAddTrackHook: cost=" + cost)
            description = msg.getProperty("description")
            Log.debug("JukeboxAddTrackHook: description=" + description)
            trackInfo = {"name" : name, "type" : type, "url" : url, "cost" : cost, "description" : description}
            trackData.append(trackInfo)
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(not inList)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxDeleteTrackHook(Hook):
    def processMessage(self, msg, flags):
        name = msg.getProperty("name")
        delTrack = -1
        for i in range(0, len(trackData)):
            trackName = trackData[i]["name"]
            if (trackName == name):
                delTrack = i
        if (delTrack != -1):
            trackData.pop(delTrack)
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(delTrack != -1)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxGetFundsHook(Hook):
    def processMessage(self, msg, flags):
        playerOid = long(msg.getProperty("poid"))
        money = EnginePlugin.getObjectProperty(playerOid, WorldManagerClient.NAMESPACE, "money")
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(money)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxAddFundsHook(Hook):
    def processMessage(self, msg, flags):
        playerOid = long(msg.getProperty("poid"))
        deposit = int(msg.getProperty("money"))
        funds = EnginePlugin.getObjectProperty(playerOid, WorldManagerClient.NAMESPACE, "money")
        if (funds == None):
            funds = 0
        funds = funds + deposit
        EnginePlugin.setObjectProperty(playerOid, WorldManagerClient.NAMESPACE, "money", funds)
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(funds)
        Engine.getAgent().sendResponse(respMsg)
        return True

class JukeboxPlayHook(Hook):
    def processMessage(self, msg, flags):
        url = msg.getProperty("url")
        type = msg.getProperty("type")
        jukeboxId = msg.getProperty("id")
        playerOid = long(msg.getProperty("poid"))
        oid = jukeboxes[jukeboxId]
        money = EnginePlugin.getObjectProperty(playerOid, WorldManagerClient.NAMESPACE, "money")
        cost = None
        for i in range(0, len(trackData)):
            if (trackData[i]["url"] == url):
                cost = trackData[i]["cost"]
        if (cost == None):
            return True
        cost = int(cost)
        if (money < cost):
            money = 2000
        else:
            money = money - cost
        EnginePlugin.setObjectProperty(playerOid, WorldManagerClient.NAMESPACE, "money", money)
        EnginePlugin.setObjectProperty(oid, WorldManagerClient.NAMESPACE, "jukeboxUrl", type + "#" + url)
        return True

class JukeboxDependencyHook(Hook):
    def processMessage(self, msg, flags):
        return True

class JukeboxGenerateSubObjectHook(EnginePlugin.GenerateSubObjectHook):
    def generateSubObject(self, template, namespace, masterOid):
	global nameSpace
        jukeboxId = template.get(nameSpace, "jukebox")
        jukeboxes[jukeboxId] = masterOid
        subObj = Entity(masterOid)
        Entity.registerEntityByNamespace(subObj, nameSpace)
        return EnginePlugin.SubObjData(None, JukeboxDependencyHook())

class WhoHook (Hook):
    def processMessage(self, sub, msg):
        Log.debug("WhoHook: got a who request, msg type is " + msg.getMsgTypeString())
        userList = LinkedList()

        # make a list of names
        userList.add("Fred")
        userList.add("Barney")

        Log.debug("WhoHook: returning user list size=" + str(userList.size()))

        # send the response message
        respMsg = GenericResponseMessage(msg)
        respMsg.setData(userList)
        Engine.getAgent().sendResponse(respMsg)
        return Boolean(True)

class JukeboxPlugin (EnginePlugin):
    def __init__(self,myName):
        EnginePlugin.__init__(self,myName);

    def onActivate(self):
        Log.debug("JukeboxPlugin.onActivate()")
	global nameSpace
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_SET_ID, JukeboxSetIdHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_GET_ID, JukeboxGetIdHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_GET_TRACKS, JukeboxGetTracksHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_ADD_TRACK, JukeboxAddTrackHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_DELETE_TRACK, JukeboxDeleteTrackHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_GET_FUNDS, JukeboxGetFundsHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_ADD_FUNDS, JukeboxAddFundsHook())
        self.getHookManager().addHook(MSG_TYPE_JUKEBOX_PLAY, JukeboxPlayHook())
        self.getHookManager().addHook(MSG_TYPE_GET_ALL_USERS, WhoHook())
        filter = MessageTypeFilter()
        filter.addType(MSG_TYPE_JUKEBOX_SET_ID)
        filter.addType(MSG_TYPE_JUKEBOX_GET_ID)
        filter.addType(MSG_TYPE_JUKEBOX_GET_TRACKS)
        filter.addType(MSG_TYPE_JUKEBOX_ADD_TRACK)
        filter.addType(MSG_TYPE_JUKEBOX_DELETE_TRACK)
        filter.addType(MSG_TYPE_JUKEBOX_GET_FUNDS)
        filter.addType(MSG_TYPE_JUKEBOX_ADD_FUNDS)
        filter.addType(MSG_TYPE_GET_ALL_USERS)
        Engine.getAgent().createSubscription(filter, self, MessageAgent.RESPONDER)
        filter2 = MessageTypeFilter()
        filter2.addType(MSG_TYPE_JUKEBOX_PLAY)
        Engine.getAgent().createSubscription(filter2, self)
        self.registerPluginNamespace(nameSpace, JukeboxGenerateSubObjectHook(self))

nameSpace = Namespace.intern("jukebox")
Engine.registerPlugin(JukeboxPlugin())

Log.debug("extensions_anim.py: LOADED")
