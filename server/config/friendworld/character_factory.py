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
from multiverse.mars.objects import *
from multiverse.mars.core import *
from multiverse.mars.events import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.server.plugins import *
from multiverse.server.math import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from java.lang import *

# "CharacterName"
# "Sex"
# "Model"

meshInfo = {
    "female_01" : "hmn_f_01_base.mesh",
    "male_01"   : "hmn_m_01_base.mesh",
    }

avatarProps = [
    "AccountId",
    "CharacterName",
    "Sex",
    "Model",
    "HeadDetail",
    "SkinColor",
    "HeadShape",
    "HairStyle",
    "HairColor",
    "UpperBodyType",
    "ClothesTorso",
    "TorsoTattoo",
    "TorsoTattooSite",
    "LowerBodyType",
    "ClothesLegs",
    "LegTattoo",
    "LegTattooSite",
    "Footwear",
    "AppearanceOverride",
    "RoomStyle",
    ]

for prop in avatarProps:
    MarsLoginPlugin.registerCharacterProperty(Namespace.WORLD_MANAGER, prop)

MarsLoginPlugin.registerCharacterProperty(Namespace.WORLD_MANAGER, "Targetable")
MarsLoginPlugin.registerCharacterProperty(Namespace.WORLD_MANAGER, "ClickHookName")
MarsLoginPlugin.registerCharacterProperty(Namespace.WORLD_MANAGER, "friendlist")

# default player template
player = Template("Player")

# player.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)
player.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.player)

ObjectManagerClient.registerTemplate(player)

# character factory
class friendworldFactory (CharacterFactory):
    def createCharacter(self, worldName, accountId, properties):
        ot = Template()

	properties.put("accountId", accountId)
	properties.put("AccountId", accountId)

        name = properties.get("CharacterName")
	if not name:
	    name = properties.get("characterName")
	    if name:
		properties.put("CharacterName",name)

        if not name or name=="":
            properties.put("errorMessage", "Invalid name")
            return 0

        sex = properties.get("Sex")
        modelName = properties.get("Model")
        meshName = meshInfo[modelName]

        displayContext = DisplayContext(meshName, True)
        emptySubmesh = DisplayContext.Submesh()
        emptySubmesh.setName("emptySubmesh")
        displayContext.addSubmesh(emptySubmesh)
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)

        for prop in avatarProps:
            if properties.containsKey(prop):
                ot.put(Namespace.WORLD_MANAGER, prop, properties.get(prop))
            else:
                ot.put(Namespace.WORLD_MANAGER, prop, "default")

        ot.put(Namespace.WORLD_MANAGER, "Targetable", True)
        ot.put(Namespace.WORLD_MANAGER, "ClickHookName", "avatar_menu")
        ot.put(Namespace.WORLD_MANAGER, "friendlist", None)

        ot.put(Namespace.OBJECT_MANAGER, "AccountId", accountId)

        # override template
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_NAME, name)

        ot.put(Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True));

        #####
        accountId = properties.get("AccountId")
        privateInstanceName = "room-%s" % accountId
        ot.put(Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME, privateInstanceName)

        roomStyle = properties.get("RoomStyle")
        roomTemplateName = ""
        worldFileName = ""
        initScriptName = ""
        if roomStyle == "hiphop":
            roomTemplateName = "hip hop room template"
            worldFileName = "$WORLD_DIR/hiphoproom.mvw"
            initScriptName = "$WORLD_DIR/instance_load_room_hiphop.py"
        else:
            roomTemplateName = "cute room template"
            worldFileName = "$WORLD_DIR/cuteroom.mvw"
            initScriptName = "$WORLD_DIR/instance_load_room_cute.py"
        privateInstanceOid = InstanceClient.getInstanceOid(privateInstanceName)
        if (privateInstanceOid is None):
            # create instance
            piot = Template() # private instance override template
            piot.put(Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_NAME, privateInstanceName)
            piot.put(Namespace.INSTANCE, "AccountId", accountId)
            props = HashMap()
            piot.put(Namespace.INSTANCE, "RoomItemsProps", props)
            piot.put(Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True))
            
            piot.put(Namespace.INSTANCE, "RoomStyle", roomStyle)
                
            privateInstanceOid = InstanceClient.createInstance(roomTemplateName, piot)
        else:
            while True:
                result = InstanceClient.loadInstance(privateInstanceOid)
                if result != InstanceClient.RESULT_ERROR_RETRY:
                    break
                time.sleep(1)
            if result != InstanceClient.RESULT_OK:
                Log.error("Error loading instance "+str(privateInstanceOid)+", result "+str(result))
            # change
            EnginePlugin.setObjectProperty(privateInstanceOid, Namespace.INSTANCE, InstanceClient.TEMPL_INSTANCE_TEMPLATE_NAME, roomTemplateName)
            EnginePlugin.setObjectProperty(privateInstanceOid, Namespace.INSTANCE, InstanceClient.TEMPL_WORLD_FILE_NAME, worldFileName)
            EnginePlugin.setObjectProperty(privateInstanceOid, Namespace.INSTANCE, InstanceClient.TEMPL_INIT_SCRIPT_FILE_NAME, initScriptName)
            EnginePlugin.setObjectProperty(privateInstanceOid, Namespace.INSTANCE, "RoomStyle", roomStyle)
            # unload
            result = InstanceClient.unloadInstance(privateInstanceOid)
            if not result:
                Log.error("Error unloading instance "+str(privateInstanceOid)+", result "+str(result))
            # load
            while True:
                result = InstanceClient.loadInstance(privateInstanceOid)
                if result != InstanceClient.RESULT_ERROR_RETRY:
                    break
                time.sleep(1)
            if result != InstanceClient.RESULT_OK:
                Log.error("Error loading instance "+str(privateInstanceOid)+", result "+str(result))

        # get instanceSpawnMarker
        privateInstanceSpawnMarker = InstanceClient.getMarker(privateInstanceOid, "spawnPt")

        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, Long(privateInstanceOid))
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC, privateInstanceSpawnMarker.getPoint())
        ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_ORIENT, privateInstanceSpawnMarker.getOrientation())

        #####

        # get default instance oid
        instanceOid = InstanceClient.getInstanceOid("default")
        if not instanceOid:
            Log.Error("friendworldFactory: no 'default' instance")
            properties.put("errorMessage", "No default instance")
            return 0

        # set the spawn location
        spawnMarker = InstanceClient.getMarker(instanceOid, "spawnPt")

#         ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_INSTANCE, Long(instanceOid))
#         ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_LOC, spawnMarker.getPoint())
#         ot.put(Namespace.WORLD_MANAGER, WorldManagerClient.TEMPL_ORIENT, spawnMarker.getOrientation())

        restorePoint = InstanceRestorePoint("default", spawnMarker.getPoint())
        restorePoint.setFallbackFlag(True)
        restoreStack = LinkedList()
        restoreStack.add(restorePoint)
        ot.put(Namespace.OBJECT_MANAGER, ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK, restoreStack)

        # generate the object
        objOid = ObjectManagerClient.generateObject("Player", ot)
        Log.debug("friendworldFactory: generated obj oid=" + str(objOid))
        return objOid

friendworldFactoryInst = friendworldFactory()
LoginPlugin.getCharacterGenerator().setCharacterFactory(friendworldFactoryInst)
