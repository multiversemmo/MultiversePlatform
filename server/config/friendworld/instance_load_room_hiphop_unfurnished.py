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

from multiverse.server.plugins import *
from multiverse.server.objects import *
from multiverse.server.util import Log
from java.lang import Long


instance = Instance.current()
instanceOid = Instance.currentOid()


#npc1Marker = instance.getMarker("npc1_marker").clone()
#npc1Marker.getPoint().setY(0)

#spawnData = SpawnData()
#spawnData.setFactoryName("npc1Factory")
#spawnData.setInstanceOid(Long(instanceOid))
#spawnData.setLoc(npc1Marker.getPoint())
#spawnData.setNumSpawns(1)
#spawnData.setSpawnRadius(1)
#spawnData.setRespawnTime(1000)
#spawnData.setCorpseDespawnTime(0)
#MobManagerClient.createSpawnGenerator(spawnData)


Log.debug("done with instance_load_room_hiphop_unfurnished.py")
