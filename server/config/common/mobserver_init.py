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
from multiverse.mars.core import *
from multiverse.mars.objects import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.mars.behaviors import *
from multiverse.msgsys import *
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from java.lang import *

# this file gets run before the mobserver manager plugin gets
# registered.  these are the default multiverse behaviors
# if you want to add your own, add it to config/<world>/mobserver_init.py

#//////////////////////////////////////////////////////////////////
#//
#// standard npc behavior
#//
#//////////////////////////////////////////////////////////////////
WEObjFactory.registerBehaviorClass("BaseBehavior", "multiverse.server.engine.BaseBehavior")
WEObjFactory.registerBehaviorClass("CombatBehavior", "multiverse.mars.behaviors.CombatBehavior")
WEObjFactory.registerBehaviorClass("RadiusRoamBehavior", "multiverse.mars.behaviors.RadiusRoamBehavior")
WEObjFactory.registerBehaviorClass("PatrolBehavior", "multiverse.mars.behaviors.PatrolBehavior")
