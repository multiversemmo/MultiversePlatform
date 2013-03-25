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

import ClientAPI
import Multiverse.Base
import Multiverse.Network

import System.Collections.Generic

from Multiverse.Network import MessageDispatcher, WorldMessageType

class Network:
    #
    # Constructor
    #
    def __init__(self):
        self.__dict__['_client'] = ClientAPI._client
        self.__dict__['_networkHelper'] = ClientAPI._client.NetworkHelper
        self._extensionHandlers = {}
        MessageDispatcher.Instance.RegisterHandler(WorldMessageType.Extension, self._HandleExtensionMessage)

    #
    # Methods to send messages
    #
    def CreateCharacter(self, attrs):
        return self._networkHelper.CreateCharacter(attrs)

    def DeleteCharacter(self, attrs):
        self._networkHelper.DeleteCharacter(attrs)
        
    def SendMessage(self, message):
        self._networkHelper.SendMessage(message)

    def SendQuestResponseMessage(self, objectId, questId, accepted):
        message = Multiverse.Network.QuestResponseMessage()
        message.ObjectId = objectId
        message.QuestId = questId
        message.Accepted = accepted
        self.SendMessage(message)

    def SendCommMessage(self, text):
        message = Multiverse.Network.CommMessage()
        message.ChannelId = 1 # CommChannel.Say
        message.Message = text
        self.SendMessage(message)

    def SendAcquireMessage(self, objectId):
        message = Multiverse.Network.AcquireMessage()
        message.ObjectId = objectId
        self.SendMessage(message)

    def SendEquipMessage(self, objectId, slotName):
        message = Multiverse.Network.EquipMessage()
        message.ObjectId = objectId
        message.SlotName = slotName
        self.SendMessage(message)

    def SendAttackMessage(self, objectId, attackType, attackStatus):
        message = Multiverse.Network.AutoAttackMessage()
        message.ObjectId = objectId
        message.AttackType = attackType
        message.AttackStatus = attackStatus
        self.SendMessage(message)

    def SendLogoutMessage(self):
        message = Multiverse.Network.LogoutMessage()
        self.SendMessage(message)

    def SendTargetedCommand(self, objectId, text):
        message = Multiverse.Network.CommandMessage()
        message.ObjectId = objectId
        message.Command = text
        self.SendMessage(message)

    def SendQuestInfoRequestMessage(self, objectId):
        message = Multiverse.Network.QuestInfoRequestMessage()
        message.ObjectId = objectId
        self.SendMessage(message)

    def SendQuestResponseMessage(self, objectId, questId, accepted):
        message = Multiverse.Network.QuestResponseMessage()
        message.ObjectId = objectId
        message.QuestId = questId
        message.Accepted = accepted
        self.SendMessage(message)

    def SendQuestConcludeRequestMessage(self, objectId):
        message = Multiverse.Network.QuestConcludeRequestMessage()
        message.ObjectId = objectId
        self.SendMessage(message)

    def SendTradeOffer(self, partnerId, itemIds, accepted, cancelled):
        items = System.Collections.Generic.List[System.Int64]()
        for i in itemIds:
            items.Add(i)
        message = Multiverse.Network.TradeOfferRequestMessage()
        message.Oid = ClientAPI.GetPlayerObject().OID
        message.ObjectId = partnerId
        message.Accepted = accepted
        message.Cancelled = cancelled
        message.Offer = items
        self.SendMessage(message)

    def SendActivateItemMessage(self, itemId, objectId):
        message = Multiverse.Network.ActivateItemMessage()
        message.ItemId = itemId
        message.ObjectId = objectId
        self.SendMessage(message)

    def SendExtensionMessage(self, targetOid, clientTargeted, extensionType, properties):
        message = Multiverse.Network.ExtensionMessage(targetOid, clientTargeted)
        nativeDict = Multiverse.Network.PropertyMap.FromPythonDict(properties)
        for key in nativeDict.Keys:
            message.Properties[key] = nativeDict[key]
        message.Properties["ext_msg_subtype"] = extensionType
        self.SendMessage(message)
        
    def RegisterExtensionMessageHandler(self, extensionType, handler):
        handlers = []
        if self._extensionHandlers.has_key(extensionType):
            handlers = self._extensionHandlers[extensionType]
        else:
            self._extensionHandlers[extensionType] = handlers
        handlers.append(handler)
    
    def RemoveExtensionMessageHandler(self, extensionType, handler):
        if self._extensionHandlers.has_key(extensionType):
            handlers = self._extensionHandlers[extensionType]
            handlers.remove(handler)
            if len(handlers) == 0:
                del self._extensionHandlers[extensionType]

    def _HandleExtensionMessage(self, message):
        if message is None or not isinstance(message, Multiverse.Network.ExtensionMessage):
            return
        extensionType = None
        if message.Properties.ContainsKey("ext_msg_subtype"):
            extensionType = message.Properties["ext_msg_subtype"]
        elif message.Properties.ContainsKey("ext_msg_type"):
            ClientAPI._deprecated("1.1", "Extension message with 'ext_msg_type'",
                                  "Extension message with 'ext_msg_subtype'")
            extensionType = message.Properties["ext_msg_type"]
        else:
            ClientAPI.LogWarn("Received extension message without a subtype")
            return
        if self._extensionHandlers.has_key(extensionType):
            message.Properties["ext_msg_subject_oid"] = message.Oid
            message.Properties["ext_msg_target_oid"] = message.TargetOid
            message.Properties["ext_msg_client_targeted"] = message.ClientTargeted
            handlers = self._extensionHandlers[extensionType]
            for handler in handlers:
                handler(Multiverse.Network.PropertyMap.ToPythonDict(message.Properties))
