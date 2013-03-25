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
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *


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
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/dance: oid=" + str(playerOid))
        WorldManagerClient.setObjectProperty(playerOid, "dancestate", Boolean(not WorldManagerClient.getObjectProperty(playerOid, "dancestate")))

class GestureCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/gesture: oid=" + str(playerOid))
        WorldManagerClient.setObjectProperty(playerOid, "gesturestate", Boolean(not WorldManagerClient.getObjectProperty(playerOid, "gesturestate")))

class SitCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/Sit: oid=" + str(playerOid))
        WorldManagerClient.setObjectProperty(playerOid, "sitstate", Boolean(not WorldManagerClient.getObjectProperty(playerOid, "sitstate")))
    
class WaveCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/wave: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "wave")

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
        AnimationClient.playSingleAnimation(playerOid, "noway")
    
class LookCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/look: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "lookaround")
    
class FightCommand (ProxyPlugin.CommandParser):
    def parse(self, cmdEvent):
        playerOid = cmdEvent.getObjectOid()
        Log.debug("/fight: oid=" + str(playerOid))
        AnimationClient.playSingleAnimation(playerOid, "fight")

proxyPlugin.registerCommand("/wave", WaveCommand())      
proxyPlugin.registerCommand("/clap", ClapCommand())
proxyPlugin.registerCommand("/cry", CryCommand())
proxyPlugin.registerCommand("/laugh", LaughCommand())
proxyPlugin.registerCommand("/cheer", CheerCommand())
proxyPlugin.registerCommand("/no", NoCommand())
proxyPlugin.registerCommand("/look", LookCommand())
proxyPlugin.registerCommand("/fight", FightCommand())
proxyPlugin.registerCommand("/setmesh", SetMeshCommand())
proxyPlugin.registerCommand("/playanimation", PlayAnimationCommand())
proxyPlugin.registerCommand("/dance", DanceCommand())
proxyPlugin.registerCommand("/gesture", GestureCommand())
proxyPlugin.registerCommand("/sit", SitCommand())

# proxyPlugin.registerCommand("/sitcrossed", DanceCommand())
# proxyPlugin.registerCommand("/lounge", DanceCommand())
