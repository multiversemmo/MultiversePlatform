#
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

import sys
from java.util import *
from java.lang import *
from multiverse.mars.plugins import *
from multiverse.msgsys import *
from multiverse.server.math import *
from multiverse.server.plugins import *
from multiverse.server.util import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.messages import PropertyMessage
from multiverse.server.messages import LoginMessage
from multiverse.server.messages import LogoutMessage

# This config file defines the catalog of message types used by the
# multiverse system.  All server plugins load this file during startup
# before _any_ message is sent, and thus can agree on the generated
# message numbers.

# Game developers can extend the list of cataloged messages by adding
# to the file config/world_name/worldmessages.py.  The startup script
# multiverse.sh ensures that both mvmessages.py and
# config/world_name/worldmessages.py are read by every plugin before
# any messages are generated

# Create the Multiverse message catalog

MessageCatalog.addMsgTypeTranslation(mvMessageCatalog, MessageType.intern("proxy.DYNAMIC_INSTANCE"))
MessageCatalog.addMsgTypeTranslation(mvMessageCatalog, MessageType.intern("mvp.GET_FRIENDLIST"))
MessageCatalog.addMsgTypeTranslation(mvMessageCatalog, MessageType.intern("mvp.ADD_FRIEND"))
