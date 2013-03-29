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

True=1
False=0


class TemplateHook(EnginePlugin.PluginActivateHook):
    def activate(self):
        tmpl = Template("jukebox")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, DisplayContext("LES_jukebox.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_ORIENT, Quaternion.fromAngleAxis( 0.774, MVVector(0, 1, 0)))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, "jukebox", 1)
        tmpl.put("jukebox", "jukebox", 1)
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", 100))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        ObjectManagerClient.registerTemplate(tmpl)
        
        avatar_base_DC = DisplayContext("LES_avatar.mesh", True)
        tmpl = Template("avatar")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, avatar_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_RUN_THRESHOLD, Float(5000))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_FOLLOWS_TERRAIN, Boolean(True))
        tmpl.put(WorldManagerClient.NAMESPACE, "female pedestrian", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", 100))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        ObjectManagerClient.registerTemplate(tmpl)

        tmpl = Template("rocketbox npc")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, "bartender", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", 100))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        ObjectManagerClient.registerTemplate(tmpl)
