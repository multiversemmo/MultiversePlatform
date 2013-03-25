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
from multiverse.server.worldmgr import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.server.math import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from multiverse.msgsys import *

# Uncomment if you want to set a log level for this process
# that is different from the server's default log level
#Log.setLogLevel(1)

#
# set us up as a mob server
#
#Engine.setServerID(10)
#Engine.isEntityManager(true)
#Engine.setWorldID(3)
#Engine.setPort(5200)

#Engine.msgSvrHostname = "localhost"
#Engine.msgSvrPort = 20374

Engine.setBasicInterpolatorInterval(5000)

World.setGeometry(Geometry.maxGeometry())

#
# add world servers to the world server manager
#
# Log.debug("Connnecting to world servers")
# wsMgr = Engine.getWSManager()
# wsMgr.addWorldServer(WorldServer("localhost", 5090, 0))
# wsMgr.addWorldServer(WorldServer("localhost", 5091, 1))

# Log.setLogFilename("fantasy.log")
Log.debug("mobserver_local: done with local config")
