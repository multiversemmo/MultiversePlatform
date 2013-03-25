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

import Axiom.Core
import ClientAPI

class Decal:
    #
    # Constructor
    #
    def __init__(self, imageName, posX, posZ, sizeX, sizeZ, priority=50, rot=0, lifetime=0, deleteRadius=0):
        self.__dict__['_decalElement'] = ClientAPI._worldManager.DecalManager.CreateDecalElement(imageName, posX, posZ, sizeX, sizeZ, rot, lifetime, deleteRadius, priority)
        
    def Dispose(self):
        ClientAPI._worldManager.DecalManager.RemoveDecalElement(self._decalElement)
        
    #
    # Property Getters
    #
    def _get_ImageName(self):
        return self._decalElement.ImageName
        
    def _get_PosX(self):
        return self._decalElement.PosX
        
    def _get_PosZ(self):
        return self._decalElement.PosZ
        
    def _get_SizeX(self):
        return self._decalElement.SizeX
        
    def _get_SizeZ(self):
        return self._decalElement.SizeZ
        
    def _get_Rotation(self):
        return self._decalElement.Rot
        
    def _get_Priority(self):
        return self._decalElement.Priority
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
            
    #
    # Property Setters
    #
    def _set_PosX(self, pos):
        self._decalElement.PosX = pos
        
    def _set_PosZ(self, pos):
        self._decalElement.PosZ = pos
        
    def _set_SizeX(self, size):
        self._decalElement.SizeX = size
        
    def _set_SizeZ(self, size):
        self._decalElement.SizeZ = size
        
    def _set_Rotation(self, rot):
        self._decalElement.Rot = rot
        
    def _set_Priority(self, priority):
        self._decalElement.Priority = priority
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'ImageName': _get_ImageName, 'PosX': _get_PosX, 'PosZ': _get_PosZ, 'SizeX': _get_SizeX, 'SizeZ': _get_SizeZ, 'Rotation': _get_Rotation, 'Priority': _get_Priority }
    _setters = { 'PosX': _set_PosX, 'PosZ': _set_PosZ, 'SizeX': _set_SizeX, 'SizeZ': _set_SizeZ, 'Rotation': _set_Rotation, 'Priority': _set_Priority }
    
    #
    # Methods
    #
    def CreateAnimableValue(self, propertyName):
        return self._decalElement.CreateAnimableValue(propertyName)
        
