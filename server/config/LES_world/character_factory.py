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

avatarProps = [ "AppearanceOverride",
                "SkinColor",
                "HeadShape",
                "HeadDetail",
                "HairStyle",
                "HairColor",
                "ClothesTorso",
                "ClothesLegs",
                "Tattoo" ]

for prop in avatarProps:
    MarsLoginPlugin.registerCharacterProperty(WorldManagerClient.NAMESPACE, prop)

# default player template
player = Template("DefaultPlayer")

player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)
player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.player)
player.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_RUN_THRESHOLD, Float(5000))
player.put(WorldManagerClient.NAMESPACE, "money", Integer(1250))

player.put(CombatClient.NAMESPACE, "combat.userflag", Boolean(True))
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_DEADSTATE, Boolean(False))
player.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", 100))
player.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
player.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", 100))
player.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
player.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "player attack ability")

ObjectManagerClient.registerTemplate(player)

# character factory
class LESFactory (CharacterFactory):
    def createCharacter(self, worldName, uid, properties):
        ot = Template()

        name = properties.get("characterName");
        # check to see that the name is valid
        # we may also want to check for uniqueness and reject bad words here
        if not name or name == "":
            properties.put("errorMessage", "Invalid name")
            return 0
        
        # set the spawn location
        # loc = Point(-135343, 0, -202945)
        # loc = Point(5134, 166475, -1180304)
        loc = WorldEditorReader.getWaypoint("club_entrance")

        gender = properties.get("sex")

        meshName = "LES_avatar.mesh"
        displayContext = DisplayContext(meshName, True)
        ot.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)

        # copy dynamic avatar properties into template
        for prop in avatarProps:
            if properties.containsKey(prop):
                ot.put(WorldManagerClient.NAMESPACE, prop, properties.get(prop))

        # override template
        ot.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, name)
        ot.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_INSTANCE, Long(0))
        ot.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_LOC, loc)
        ot.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_ORIENT, Quaternion.fromAngleAxis(3.14159, MVVector(0, 1, 0)))

        ot.put(Namespace.OBJECT_MANAGER,
            ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True));

        # generate the object
        objOid = ObjectManagerClient.generateObject("DefaultPlayer", ot)
        Log.debug("LESFactory: generated obj oid=" + str(objOid))
        return objOid

lesFactory = LESFactory()
LoginPlugin.getCharacterGenerator().setCharacterFactory(lesFactory);
