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

from java.util import *
from java.lang import *
from multiverse.msgsys import *

#
# This python file creates a world-specific message catalog, and 
# contains definitions for world-specific message  types, if 
# your world makes use of them.  Not all worlds actually
# need to define their own message types, but if if your world does
# need world-specific message types, they must be added to your 
# world-specific message catalog by listing them in this file
#

#
# Create the world message catalog.  Multiverse reserves message numbers
# from 1 through 500; the world-specific catalog defined below allocates
# message type numbers from the range 501-1000.
#
worldMessageCatalog = MessageCatalog.addMsgCatalog("worldMessageCatalog", 501, 500);

#
# Add your world-specific messages here.  Each call to addMsgTypeTranslation
# adds the message type which is the second argument to the world message
# catalog.  Each message type must be defined in YourWorldModule by a call
# to MessageType.intern(message_type_string);
# 
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("GET_ALL_USERS"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxPlay"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxDeleteTrack"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxAddFunds"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxAddTrack"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxGetTracks"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxGetFunds"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxSetId"))
MessageCatalog.addMsgTypeTranslation(worldMessageCatalog, MessageType.intern("jukeboxGetId"))
