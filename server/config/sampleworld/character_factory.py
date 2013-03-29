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
from java.util import LinkedList

meshInfo = { "human_female.mesh" : [[ "bodyShape-lib.0", "human_female.skin_material" ],
                                    [ "head_aShape-lib.0", "human_female.head_a_material" ],
                                    [ "hair_bShape-lib.0", "human_female.hair_b_material" ]],
             "human_male.mesh" : [[ "bodyShape-lib.0", "human_male.skin_material" ],
                                  [ "head_aShape-lib.0", "human_male.head_a_material" ]] }

# Add clothing meshes.  These should really be done as inventory items,
# but as a quick fix, we put it in the base display context of the object.
meshInfo["human_male.mesh"].extend([[ "cloth_a_bootsShape-lib.0", "human_male.cloth_a_material" ],
                                    [ "cloth_a_shirtShape-lib.0", "human_male.cloth_a_material" ],
                                    [ "cloth_a_pantsShape-lib.0", "human_male.cloth_a_material" ]])
meshInfo["human_female.mesh"].extend([[ "leather_a_beltShape-lib.0", "human_female.leather_a_material" ],
                                      [ "leather_a_pantsShape-lib.0", "human_female.leather_a_material" ],
                                      [ "leather_a_bootsShape-lib.0", "human_female.leather_a_material" ],
                                      [ "leather_a_tunicShape-lib.0", "human_female.leather_a_material" ]])

# set up the default display context
displayContext = DisplayContext("human_female.mesh", True)
for entry in meshInfo["human_female.mesh"]:
    displayContext.addSubmesh(DisplayContext.Submesh(entry[0], entry[1]))

# default player template
player = Template("DefaultPlayer")

player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)
player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.player)
player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_RUN_THRESHOLD, Float(5000))

player.put(CombatClient.NAMESPACE, "combat.userflag", Boolean(True))
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_DEADSTATE, Boolean(False))
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
player.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "Leather Tunic; Leather Pants")
player.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", 100))
player.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
player.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", 100))
player.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "player attack ability")

ObjectManagerClient.registerTemplate(player)

# character factory
class SampleFactory (CharacterFactory):
    def createCharacter(self, worldName, uid, properties):
        ot = Template()

        name = properties.get("characterName")
        # check to see that the name is valid
        # we may also want to check for uniqueness and reject bad words here
        if not name or name == "":
            properties.put("errorMessage", "Invalid name")
            return 0
        
        meshName = None
        gender = properties.get("sex")
        
        if gender == "female":
            meshName = "human_female.mesh"
        elif gender == "male":
            meshName = "human_male.mesh"

        if meshName:
            displayContext = DisplayContext(meshName, True)
            submeshInfo = meshInfo[meshName]
            for entry in submeshInfo:
                displayContext.addSubmesh(DisplayContext.Submesh(entry[0],
                                                                 entry[1]))
            ot.put(WorldManagerClient.NAMESPACE,
                   WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)

	statProperties = ["strength","dexterity","wisdom","intelligence",
		"class"]
	for statProp in statProperties:
	    if (not properties.get(statProp)):
		properties.put("errorMessage", "Missing property "+statProp)
		return 0

        # get combat settings
        strength = int(properties.get("strength"))
        dexterity = int(properties.get("dexterity"))
        wisdom = int(properties.get("wisdom"))
        intelligence = int(properties.get("intelligence"))
        player_class = str(properties.get("class"))

	# get default instance oid
	instanceOid = InstanceClient.getInstanceOid("default")
	if not instanceOid:
	    Log.error("SampleFactory: no 'default' instance")
            properties.put("errorMessage", "No default instance")
	    return 0

        # set the spawn location
        spawnMarker = InstanceClient.getMarker(instanceOid, "spawn")
        spawnMarker.getPoint().setY(0)

        # override template
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_NAME, name)
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_INSTANCE, Long(instanceOid))
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_LOC, spawnMarker.getPoint())
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_ORIENT, spawnMarker.getOrientation())

	restorePoint = InstanceRestorePoint("default", spawnMarker.getPoint())
	restorePoint.setFallbackFlag(True)
	restoreStack = LinkedList()
	restoreStack.add(restorePoint)
        ot.put(Namespace.OBJECT_MANAGER,
               ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK, restoreStack)
        ot.put(Namespace.OBJECT_MANAGER,
               ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME, "default")

        ot.put(Namespace.OBJECT_MANAGER,
                ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True))

        ot.put(ClassAbilityClient.NAMESPACE, "class", player_class)        
        ot.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", strength))
        ot.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", dexterity))
        ot.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", wisdom))
        ot.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", intelligence))
        ot.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(strength)*1.5)))
        ot.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(strength)*1.5)))
        ot.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(intelligence)*2))
        ot.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(intelligence)* 2))
        ot.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(strength) * 2))
        ot.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(strength)*2))
        ot.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0, 100))
        ot.put(CombatClient.NAMESPACE, "level", MarsStat("level", 1, 100))

        # generate the object
        objOid = ObjectManagerClient.generateObject("DefaultPlayer", ot)
        Log.debug("SampleFactory: generated obj oid=" + str(objOid))
        return objOid

sampleFactory = SampleFactory()
LoginPlugin.getCharacterGenerator().setCharacterFactory(sampleFactory)
