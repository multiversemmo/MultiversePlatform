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

Log.debug("club_test_client.py starting PlayerThread");

#[09/14/07 18:16:17.468] Player: (12771.99, 162669.7, -1144619)
#[09/14/07 18:16:47.750] Player: (11141.07, 162669.7, -1144768)
#[09/14/07 18:17:20.890] Player: (11715.57, 162669.7, -1150730)
#[09/14/07 18:18:07.718] Player: (14206.52, 162669.7, -1151483)
#[09/14/07 18:18:18.484] Player: (17681.47, 162669.7, -1159884)
#[09/14/07 18:18:38.156] Player: (19912.65, 162669.7, -1159932)
#[09/14/07 18:18:49.062] Player: (20060.82, 162669.7, -1152379)

playerClient = PlayerClient("--position (17319,162670,-1154832) --polygon 12771,-1144619,11141,-1144768,11715,-1150730,14206,-1151483,17681,-1159884,19912,-1159932,20060,-1152379")

Log.debug("completed club_test_client.py")
