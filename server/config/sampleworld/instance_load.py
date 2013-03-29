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


from java.lang import Long
from multiverse.server.plugins import *
from multiverse.server.objects import *


instance = Instance.current()
instanceOid = Instance.currentOid()


wolfMarker = instance.getMarker("wolfmarker").clone()
wolfMarker.getPoint().setY(0)
spawnData = SpawnData()
spawnData.setFactoryName("WolfFactory")
spawnData.setInstanceOid(Long(instanceOid))
spawnData.setLoc(wolfMarker.getPoint())
spawnData.setNumSpawns(3)
spawnData.setSpawnRadius(20000)
spawnData.setRespawnTime(60000)
spawnData.setCorpseDespawnTime(30000)
MobManagerClient.createSpawnGenerator(spawnData)


zombieMarker = instance.getMarker("zombiemarker").clone()
zombieMarker.getPoint().setY(0)
spawnData = SpawnData()
spawnData.setFactoryName("ZombieFactory")
spawnData.setInstanceOid(Long(instanceOid))
spawnData.setLoc(zombieMarker.getPoint())
spawnData.setNumSpawns(2)
spawnData.setSpawnRadius(30000)
spawnData.setRespawnTime(60000)
spawnData.setCorpseDespawnTime(30000)
MobManagerClient.createSpawnGenerator(spawnData)


npc1Marker = instance.getMarker("npcmarker").clone()
npc1Marker.getPoint().setY(0)
spawnData = SpawnData()
spawnData.setFactoryName("Npc1Factory")
spawnData.setInstanceOid(Long(instanceOid))
spawnData.setLoc(npc1Marker.getPoint())
spawnData.setOrientation(npc1Marker.getOrientation())
spawnData.setNumSpawns(1)
spawnData.setSpawnRadius(1)
spawnData.setRespawnTime(5000)
spawnData.setCorpseDespawnTime(0)
MobManagerClient.createSpawnGenerator(spawnData)


npc2Marker = instance.getMarker("npcmarker").clone()
npc2Marker.getPoint().setY(0)
npc2Marker.getPoint().add(3000, 0, 0)
spawnData = SpawnData()
spawnData.setFactoryName("Npc2Factory")
spawnData.setInstanceOid(Long(instanceOid))
spawnData.setLoc(npc2Marker.getPoint())
spawnData.setOrientation(npc2Marker.getOrientation())
spawnData.setNumSpawns(1)
spawnData.setSpawnRadius(1)
spawnData.setRespawnTime(5000)
spawnData.setCorpseDespawnTime(0)
MobManagerClient.createSpawnGenerator(spawnData)

soldierTrainerMarker = instance.getMarker("npcmarker").clone()
soldierTrainerMarker.getPoint().setY(0)
soldierTrainerMarker.getPoint().add(6000, 0, 0)
spawndata = SpawnData()
spawnData.setFactoryName("SoldierTrainerFactory")
spawnData.setInstanceOid(Long(instanceOid))
spawnData.setLoc(soldierTrainerMarker.getPoint())
spawnData.setOrientation(soldierTrainerMarker.getOrientation())
spawnData.setNumSpawns(1)
spawnData.setSpawnRadius(1)
spawnData.setRespawnTime(5000)
spawnData.setCorpseDespawnTime(0)
MobManagerClient.createSpawnGenerator(spawnData)
