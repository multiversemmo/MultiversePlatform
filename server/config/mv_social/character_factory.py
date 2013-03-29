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

meshInfo = { "casual07_f_mediumpoly.mesh" : [[ "casual07_f_mediumpoly-mesh.0", "casual07_f_mediumpoly.body" ],
                                             [ "casual07_f_mediumpoly-mesh.1", "casual07_f_mediumpoly.hair_transparent" ]],
             "casual06_f_mediumpoly.mesh" : [[ "casual06_f_mediumpoly-mesh.0", "casual06_f_mediumpoly.body" ],
                                             [ "casual06_f_mediumpoly-mesh.1", "casual06_f_mediumpoly.hair_transparent" ]],
             "casual15_f_mediumpoly.mesh" : [[ "casual15_f_mediumpoly-mesh.0", "casual15_f_mediumpoly.body" ],
                                             [ "casual15_f_mediumpoly-mesh.1", "casual15_f_mediumpoly.hair_transparent" ]],
             "casual19_f_mediumpoly.mesh" : [[ "casual19_f_mediumpoly-mesh.0", "casual19_f_mediumpoly.body" ],
                                             [ "casual19_f_mediumpoly-mesh.1", "casual19_f_mediumpoly.hair_transparent" ]],
             "casual21_f_mediumpoly.mesh" : [[ "casual21_f_mediumpoly-mesh.0", "casual21_f_mediumpoly.body" ],
                                             [ "casual21_f_mediumpoly-mesh.1", "casual21_f_mediumpoly.hair_transparent" ]],
             "business04_f_mediumpoly.mesh" : [[ "business04_mediumpoly-mesh.0", "business04_f_mediumpoly.body" ],
                                               [ "business04_mediumpoly-mesh.1", "business04_f_mediumpoly.hair_transparent" ]],
             "sportive01_f_mediumpoly.mesh" : [[ "sportive01_f_mediumpoly-mesh.0", "sportive01_f_mediumpoly.body" ],
                                               [ "sportive01_f_mediumpoly-mesh.1", "sportive01_f_mediumpoly.hair_transparent" ]],
             "sportive02_f_mediumpoly.mesh" : [[ "sportive02_f_mediumpoly-mesh.0", "sportive02_f_mediumpoly.body" ],
                                               [ "sportive02_f_mediumpoly-mesh.1", "sportive02_f_mediumpoly.hair_transparent" ]],
             "sportive05_f_mediumpoly.mesh" : [[ "sportive05_f_mediumpoly-mesh.0", "sportive05_f_mediumpoly.body" ],
                                               [ "sportive05_f_mediumpoly-mesh.1", "sportive05_f_mediumpoly.hair_transparent" ]],
             "sportive07_f_mediumpoly.mesh" : [[ "sportive07_f_mediumpoly-mesh.0", "sportive07_f_mediumpoly.body" ]],
             "casual03_m_mediumpoly.mesh" : [[ "casual03_m_medium-mesh.0", "casual03_m_mediumpoly.body" ]],
             "casual04_m_mediumpoly.mesh" : [[ "casual04_m_mediumpoly-mesh.0", "casual04_m_mediumpoly.body" ]],
             "casual07_m_mediumpoly.mesh" : [[ "casual07_m_mediumpoly-mesh.0", "casual07_m_mediumpoly.body" ]],
             "casual10_m_mediumpoly.mesh" : [[ "casual10_m_mediumpoly-mesh.0", "casual10_m_mediumpoly.body" ]],
             "casual16_m_mediumpoly.mesh" : [[ "casual16_m_mediumpoly-mesh.0", "casual16_m_mediumpoly.body" ]],
             "casual21_m_mediumpoly.mesh" : [[ "casual21_m_mediumpoly-mesh.0", "casual21_m_mediumpoly.body" ]],
             "business03_m_mediumpoly.mesh" : [[ "business03_m_medium-mesh.0", "business03_m_mediumpoly.body" ]],
             "business05_m_mediumpoly.mesh" : [[ "business05_m_mediumpoly-mesh.0", "business05_m_mediumpoly.body" ]],
             "sportive01_m_mediumpoly.mesh" : [[ "sportive01_m_mediumpoly-mesh.0", "sportive01_m_mediumpoly.body" ]],
             "sportive09_m_mediumpoly.mesh" : [[ "sportive09_m_mediumpoly-mesh.0", "sportive09_m_mediumpoly.body" ]] }

displayContext = DisplayContext("casual07_f_mediumpoly.mesh", True)

# default player template
player = Template("MVSocialPlayer")

player.put(WorldManagerClient.NAMESPACE,
           WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
           displayContext)

player.put(WorldManagerClient.NAMESPACE,
           WorldManagerClient.TEMPL_OBJECT_TYPE,
           ObjectTypes.player)

ObjectManagerClient.registerTemplate(player)

# character factory
class MVSocialFactory (CharacterFactory):
    def createCharacter(self, worldName, uid, properties):
        ot = Template()

        name = properties.get("characterName");

        # get the account name for this player
        if not name:
            db = Engine.getDatabase()
            name = db.getUserName(uid)
            if not name:
                name = "default"

        # set the spawn location
        loc = Point(368917, 71000, 294579)

        meshName = properties.get("model")
        gender = properties.get("sex")

        if meshName:
            displayContext = DisplayContext(meshName, True)
            submeshInfo = meshInfo[meshName]
            for entry in submeshInfo:
                displayContext.addSubmesh(DisplayContext.Submesh(entry[0],
                                                                 entry[1]))
            ot.put(WorldManagerClient.NAMESPACE,
                   WorldManagerClient.TEMPL_DISPLAY_CONTEXT, displayContext)

        # get default instance oid
        instanceOid = InstanceClient.getInstanceOid("default")
        if not instanceOid:
            Log.error("MVSocialFactory: no 'default' instance")
            properties.put("errorMessage", "No default instance")
            return 0

        # override template
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_NAME, name)
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_INSTANCE, Long(instanceOid))
        ot.put(WorldManagerClient.NAMESPACE,
               WorldManagerClient.TEMPL_LOC, loc)
        ot.put(Namespace.OBJECT_MANAGER,
            ObjectManagerClient.TEMPL_PERSISTENT, Boolean(True));

        restorePoint = InstanceRestorePoint("default", loc);
        restorePoint.setFallbackFlag(True);
        restoreStack = LinkedList();
        restoreStack.add(restorePoint);
        ot.put(Namespace.OBJECT_MANAGER,
               ObjectManagerClient.TEMPL_INSTANCE_RESTORE_STACK, restoreStack)
        ot.put(Namespace.OBJECT_MANAGER,
               ObjectManagerClient.TEMPL_CURRENT_INSTANCE_NAME, "default")

        # generate the object
        objOid = ObjectManagerClient.generateObject("MVSocialPlayer", ot)
        Log.debug("MVSocialFactory: generated obj oid=" + str(objOid))
        return objOid

mvSocialFactory = MVSocialFactory()
LoginPlugin.getCharacterGenerator().setCharacterFactory(mvSocialFactory)
