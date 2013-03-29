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
from multiverse.mars.util import *
from multiverse.server.math import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.util import *
from multiverse.msgsys import *
from multiverse.simpleclient import *
from java.lang import *

# PlayerClient instance

Log.debug("main_block_test_client.py starting PlayerThread");

#[09/14/07 15:10:47.640] Player: (60525.71, 166587.3, -1172475)
#[09/14/07 15:12:04.375] Player: (58734.59, 166614.8, -1189238)
#[09/14/07 15:14:14.953] Player: (-43065.96, 166585.1, -1189102)
#[09/14/07 15:14:45.109] Player: (-43422.13, 166586, -1173661)

playerClient = PlayerClient("--position (8913,166499,-1182518) --polygon 60525,-1172475,58734,-1189238,-43065,-1189102,-43422,-1173661")

Log.debug("completed main_block_test_client.py")
