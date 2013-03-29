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


# places_url = "http://places.multiverse.net/"


# def setObjectProperty(oid, namespace, key, value, props):
#     objInfo = WorldManagerClient.getObjectInfo(oid)
#     objName = objInfo.name # objInfo.getProperty("name")
#     objProps = None
#     if props.containsKey(objName):
#         objProps = props[objName]
#     else:
#         objProps = HashMap()
#     objProps[key] = value
#     props[objName] = objProps
#     EnginePlugin.setObjectProperty(oid, namespace, key, value)
#     return props


# def getDBProperty(accountId, property):
#     value = None
#     try:
#         url = "jdbc:mysql://places.multiverse.net/friendworld?user=root&password=mv123"
#         sql = "SELECT value FROM profile WHERE account_id = %d AND property = '%s'" % ( accountId, property)
#         con = DriverManager.getConnection(url)
#         stm = con.createStatement()
#         srs = stm.executeQuery(sql)
#         # _types = {Types.INTEGER:srs.getInt, Types.FLOAT:srs.getFloat}
#         while (srs.next()):
#             value = srs.getString(1)
#         srs.close()
#         stm.close()
#         con.close()
#     except:
#         Log.debug("getDBProperty(): Exception")
#         pass
#     if value is None:
#         if property == "PhotoURL":
#             # value = places_url + "images/missing.jpg"
#             value = places_url + "photos/%08d.jpg" % accountId
#         else:
#             value = "Unknown"
#     Log.debug("getDBProperty(): accountId=%d, property=%s, value=%s" % (accountId, property, value))
#     return value


instance = Instance.current()
instanceOid = Instance.currentOid()


# roomOwnerId    = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "AccountId")
# roomItemsProps = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps")
# roomOwnerId    = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "AccountId")
# roomStyle      = EnginePlugin.getObjectProperty(instanceOid, Namespace.INSTANCE, "RoomStyle")

# # get photo for room owner
# photoURL = getDBProperty(roomOwnerId, "PhotoURL")
# # get oid for profile_main
# profileMain = roomStyle + "_profile_main"
# profileMainOid = ObjectManagerClient.getNamedObject(instanceOid, profileMain, None)
# if profileMainOid is not None:
#     # set pic_url for profile
#     roomItemsProps = setObjectProperty(profileMainOid, Namespace.WORLD_MANAGER, "pic_url", photoURL, roomItemsProps)
    
# # get friendlist
# friendlist = getFriendlist(roomOwnerId)
# i = 0
# for friendId in friendlist:
#     # get photo
#     photoURL = getDBProperty(friendId, "PhotoURL")
#             # set pic_url for friendlist
#     i = i + 1
#     profileName = roomStyle + "_profile_%02d" % i
#     profileOid = ObjectManagerClient.getNamedObject(instanceOid, profileName, None)
#     if profileOid is not None:
#         roomItemsProps = setObjectProperty(profileOid, Namespace.WORLD_MANAGER, "pic_url", photoURL, roomItemsProps)
#         roomItemsProps = setObjectProperty(profileOid, Namespace.WORLD_MANAGER, "AccountId", friendId, roomItemsProps)

# EnginePlugin.setObjectProperty(instanceOid, Namespace.INSTANCE, "RoomItemsProps", roomItemsProps)


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


Log.debug("done with instance_load_room_hiphop.py")
