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

displayContext = DisplayContext("human_female_ruth.mesh", True)
displayContext.addSubmesh(DisplayContext.Submesh("human_female_body_ruth-mesh.0",
                                                 "human_female_ruth.ruth_body_clothed_mat"))
displayContext.addSubmesh(DisplayContext.Submesh("human_female_head_ruth-mesh.0",
                                                 "human_female_ruth.ruth_head_mat"))
displayContext.addSubmesh(DisplayContext.Submesh("human_female_head_ruth_hair-mesh.0",
                                                 "human_female_ruth.ruth_hair_mat"))

# default player template
player = Template("DefaultPlayer")

player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)
player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.player)
player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_RUN_THRESHOLD, Float(5000))

player.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "*Leather Tunic; *Leather Pants")

player.put(CombatClient.NAMESPACE, "combat.userflag", Boolean(True))
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_DEADSTATE, Boolean(False))
player.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
player.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
player.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 18))
player.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 18))
player.put(CombatClient.NAMESPACE, "health", MarsStat("health", 180))
player.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 180))
player.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
player.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
player.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 5))
player.put(CombatClient.NAMESPACE, "weaponBaseDmg", 15)
player.put(CombatClient.NAMESPACE, "weaponVarDmg", 15)
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")

ObjectManagerClient.registerTemplate(player)

meshInfo = { "female_1" : [ "human_female_fantasy.mesh" ,
                            [ [ "human_female_body-mesh.0", "human_female_fantasy.body_01_clothed_mat" ],
                              [ "human_female_head_01-mesh.0", "human_female_fantasy.head_01_mat" ],
                              [ "human_female_head_01_hair-mesh.0", "human_female_fantasy.head_01_hair_01_mat" ],
                              ]
                            ],
             "female_2" : [ "human_female_fantasy.mesh" ,
                            [ [ "human_female_body-mesh.0", "human_female_fantasy.body_02_clothed_mat" ],
                              [ "human_female_head_02-mesh.0", "human_female_fantasy.head_02_mat" ],
                              [ "human_female_head_02_hair_01-mesh.0", "human_female_fantasy.head_02_hair_01_mat" ],
                              ]
                            ],
             "male_1" : [ "human_male_fantasy.mesh" ,
                          [ [ "human_male_body-mesh.0", "human_male_fantasy.human_male_body_01" ],
                            [ "human_male_head_01-mesh.0", "human_male_fantasy.human_male_head_01" ],
                            [ "male_head_01_hair_01-mesh.0", "human_male_fantasy.human_male_head_01_hair_01" ],
                            ]
                          ],
             "male_2" : [ "human_male_fantasy.mesh" ,
                          [ [ "human_male_body-mesh.0", "human_male_fantasy.human_male_body_02" ],
                            [ "human_male_head_02-mesh.0", "human_male_fantasy.human_male_head_02" ],
                            [ "human_male_02_hair_01-mesh.0", "human_male_fantasy.human_male_head_02_hair_01" ],
                            ]
                          ]
             }


# character factory
class FantasyFactory (CharacterFactory):
    def createCharacter(self, worldName, uid, properties):
        name = properties.get("characterName")

        # check to see that the name is valid
        # we may also want to check for uniqueness and reject bad words here
        if not name or name == "":
            properties.put("errorMessage", "Invalid name")
            return 0

        modelName = properties.get("model")
        meshName = meshInfo[modelName][0]
        submeshes = meshInfo[modelName][1]
        override = Template()
        if meshName:
            displayContext = DisplayContext(meshName, True)
            for entry in submeshes:
                displayContext.addSubmesh(DisplayContext.Submesh(entry[0],
                                                                 entry[1]))
            override.put(WorldManagerClient.NAMESPACE,
                         WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)

        # get default instance oid
        instanceOid = InstanceClient.getInstanceOid("default")
        if not instanceOid:
            Log.error("FantasyFactory: no 'default' instance")
            properties.put("errorMessage", "No default instance")
            return 0

        spawnMarker = InstanceClient.getMarker(instanceOid, "village_spawn")
        spawnMarker.getPoint().setY(0)

        override.put(WorldManagerClient.NAMESPACE,
		WorldManagerClient.TEMPL_NAME, name)
        override.put(WorldManagerClient.NAMESPACE,
		WorldManagerClient.TEMPL_INSTANCE, Long(instanceOid))
        override.put(WorldManagerClient.NAMESPACE,
		WorldManagerClient.TEMPL_LOC, spawnMarker.getPoint())
        override.put(WorldManagerClient.NAMESPACE,
		WorldManagerClient.TEMPL_ORIENT, spawnMarker.getOrientation())
        override.put(Namespace.OBJECT_MANAGER,
		ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True))

        restorePoint = InstanceRestorePoint("default", spawnMarker.getPoint())
        restorePoint.setFallbackFlag(True)
        restoreStack = LinkedList()
        restoreStack.add(restorePoint)
        override.put(Namespace.OBJECT_MANAGER,
               ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK, restoreStack)
        override.put(Namespace.OBJECT_MANAGER,
               ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME, "default")

        objOid = ObjectManagerClient.generateObject("DefaultPlayer", override)
        Log.debug("FantasyFactory: generated obj oid=" + str(objOid))

        return objOid

fantasyFactory = FantasyFactory()
LoginPlugin.getCharacterGenerator().setCharacterFactory(fantasyFactory)
