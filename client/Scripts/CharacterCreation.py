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

from System.Collections.Generic import *
from Axiom.MathLib import Quaternion, Vector3
from Axiom.Core import ColorEx
from Multiverse.Network import *
from Multiverse.Base import Client

import ClientAPI
import WorldObject

class CharacterCreationContext:
    #
    # Constructor
    #
    def __init__(self):
        self._attrs = {}

    #
    # Methods
    #
    def GetAttribute(self, attr):
        """Get the value of a current attribute."""
        return self._attrs[attr]

    def GetAttributes(self):
        """Get a copy of the current attributes dictionary."""
        return dict(self._attrs)

    def GetValidAttributeValues(self, attr):
        """Get a list of legal values for the given attribute, based on the
           set of attribute values that have been set on our character."""
        return None

    def SetAttribute(self, attr, val):
        """This method sets a single attribute.  Other attributes may end
           up being altered by this change."""
        attrs = self.GetAttributes()
        attrs[attr] = val
        return self.SetAttributes(attr, attrs)

    def SetAttributes(self, primaryAttr, attrs):
        """Set the attributes of the character being created.  In some
           cases the selection of attributes may be invalid, so the
           internal dictionary may not end up matching the passed attributes.
           This method requires primaryAttr to be a valid attribute name,
           and requires the entry for primaryAttr in the attrs dictionary to
           be valid.
           There must be some legitimate value for each other attribute.
           There must be an entry for each attribute in the attrs dictionary.
           If the attributes were modified, this method returns False."""
        rv = self._setAttributes(primaryAttr, attrs)
        self.OnAttributesUpdated()
        return rv

    def _setAttributes(self, primaryAttr, attrs):
        """This needs to be overriden in a derived class."""
        return False

    def OnAttributesUpdated():
        """This should be overriden in a derived class to implement whatever
           behavior is appropriate when the attributes change.  Typically,
           will update the displayed avatar."""
        pass
    
    def CreateCharacter(self):
        """This call actually causes the character to be created on the
           server.  This will return 0 if everything worked, or an error
           code if something failed."""
        attrs = Dictionary[str, object]()
        for k in self._attrs.keys():
            attrs[k] = self._attrs[k]
        return ClientAPI.Network.CreateCharacter(attrs)
    
    # helper method
    def _listContains(self, l, entry):
        """Helper method to determine if a list contains an entry"""
        for i in range(0, len(l)):
            if l[i] == entry:
                return True
        return False
        

class CharacterSelectionContext:
    def GetCharacterIds(self):
        """Return a list containing the character ids of each character."""
        rv = []
        for charEntry in ClientAPI.GetCharacterEntries():
            rv.append(charEntry.CharacterId)
        return rv

    def GetCharacterAttribute(self, characterId, attr):
        """Fetch the value of the given attribute for the given characterId."""
        for charEntry in ClientAPI.GetCharacterEntries():
            if charEntry.CharacterId == characterId:
                return charEntry[attr]
        return None

    def GetCharacterAttributes(self, characterId):
        """Fetch a dictionary containing a copy of the attributes sent from
           the server for the character with characterId."""
        for charEntry in ClientAPI.GetCharacterEntries():
            if charEntry.CharacterId == characterId:
                return charEntry
        return None

    def Login(self, characterId):
        """Connect to the world server as the given character.  This method
           returns a status code indicating the result of the connection."""
        return LoginToWorld(characterId)

    def Delete(self, characterId):
        """Delete the character that corresponds to the characterId."""
        return DeleteCharacter(characterId)

    def OnSelectionUpdated(self, characterId):
        """This should be overriden in a derived class to implement whatever
           behavior is appropriate when the selected character changes.
           Typically this will update the displayed avatar."""
        pass

def LoginToWorld(characterId, worldId = Client.Instance.WorldId):
    """Method to connect to the given world as the given character.
       This variant allows you to specify both the characterId and the
       worldId"""
    ClientAPI.World.IsWorldLocal = False
    portalMessage = PortalMessage()
    portalMessage.WorldId = worldId
    portalMessage.CharacterId = characterId
    MessageDispatcher.Instance.QueueMessage(portalMessage)

def DeleteCharacter(characterId):
    """Method to tell the server to delete a character."""
    attrs = Dictionary[str, object]()
    attrs['characterId'] = characterId
    return ClientAPI.Network.DeleteCharacter(attrs)

def InitializeStartupWorld():
    """This method injects the messages that are required to initialize the
       client."""
    # First, the login response
    loginResponse = AuthorizedLoginResponseMessage()
    loginResponse.Oid = 0;
    loginResponse.Success = True
    loginResponse.Message = "standalone"
    loginResponse.Version = "standalone"
    MessageDispatcher.Instance.QueueMessage(loginResponse)
    # Next, set up the world
    terrainInitString = "<Terrain><algorithm>HybridMultifractalWithFloor</algorithm><xOffset>0</xOffset><yOffset>0</yOffset><zOffset>0</zOffset><h>0.25</h><lacunarity>2</lacunarity><octaves>8</octaves><metersPerPerlinUnit>500</metersPerPerlinUnit><heightScale>0</heightScale><heightOffset>0</heightOffset><fractalOffset>0.7</fractalOffset><heightFloor>0</heightFloor></Terrain>"
    terrainConfigMessage = TerrainConfigMessage()
    terrainConfigMessage.ConfigString = terrainInitString
    MessageDispatcher.Instance.QueueMessage(terrainConfigMessage)
    # Now set up the player stub
    newObjMessage = NewObjectMessage()
    newObjMessage.Oid = 0
    newObjMessage.ObjectId = 0
    newObjMessage.Name = "char"
    newObjMessage.Location = Vector3(0, 0, 0)
    newObjMessage.Orientation = Quaternion.Identity
    newObjMessage.ScaleFactor = Vector3.UnitScale
    newObjMessage.ObjectType = ObjectNodeType.User
    newObjMessage.FollowTerrain = False
    MessageDispatcher.Instance.QueueMessage(newObjMessage)
    # Model info for the player stub
    modelInfoMessage = ModelInfoMessage()
    modelInfoMessage.Oid = 0
    meshInfo = MeshInfo()
    meshInfo.MeshFile = "tiny_cube.mesh"
    meshInfo.SubmeshList = List[SubmeshInfo]()
    modelInfoMessage.ModelInfo.Add(meshInfo)
    MessageDispatcher.Instance.QueueMessage(modelInfoMessage)
    # The ui theme for this initial world
    uiThemeMessage = UiThemeMessage()
    uiThemeMessage.UiModules = List[str]()
    uiThemeMessage.UiModules.Add("startup.toc")
    uiThemeMessage.KeyBindingsFile = "startup_bindings.txt"
    MessageDispatcher.Instance.QueueMessage(uiThemeMessage)
