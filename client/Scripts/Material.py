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
import Axiom.Graphics

class Material:
    #
    # Constructor
    #
    def __init__(self):
        assert False
        
    #
    # Methods
    #
    def Dispose(self):
        ClientAPI._MaterialManager.Instance.Unload(self._material)
        self._material.Dispose()
        
    def Load(self):
        self._material.Load()
        
    def Clone(self, newName):
        return _ExistingMaterial(self._material.Clone(newName))
        
    def GetTechnique(self, nameOrIndex):
        return ClientAPI.Technique._ExistingTechnique(self._material.GetTechnique(nameOrIndex))
        
    def GetBestTechnique(self):
        return ClientAPI.Technique._ExistingTechnique(self._material.GetBestTechnique())
        
    def ApplyTextureAlias(self, alias, textureName):
        return self._material.ApplyTextureAlias(alias, textureName)
    
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._material.Name
        
    def _get_NumTechniques(self):
        return self._material.NumTechniques
    
    def _get_ReceiveShadows(self):
        return self._material.ReceiveShadows
                
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
    
    #
    # Property Setters
    #
    def _set_ReceiveShadows(self, value):
        self._material.ReceiveShadows = value
         
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Name': _get_Name, 'NumTechniques': _get_NumTechniques, 'ReceiveShadows': _get_ReceiveShadows }
    _setters = { 'ReceiveShadows': _set_ReceiveShadows }
    
#
# This class is just another way of making a Material, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
# The way to get a Material is to call ClientAPI.GetMaterial() or Material.Clone()
#
class _ExistingMaterial(Material):
    #
    # Constructor
    #
    def __init__(self, material):
        self.__dict__['_material'] = material

    def __setattr__(self, attrname, value):
        Material.__setattr__(self, attrname, value)
