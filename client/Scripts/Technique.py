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

class Technique:
    #
    # Constructor
    #
    def __init__(self):
        assert False

    #
    # Methods
    #
    def GetPass(self, nameOrIndex):
        return ClientAPI.Pass._ExistingPass(self._technique.GetPass(nameOrIndex))
    
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._technique.Name
        
    def _get_NumPasses(self):
        return self._technique.NumPasses
    
    def _get_Parent(self):
        return ClientAPI.Material._ExistingMaterial(self._technique.Parent)
    
    def _get_IsLoaded(self):
        return self._technique.IsLoaded
        
    def _get_IsSupported(self):
        return self._technique.IsSupported
        
    def _get_Scheme(self):
        return self._technique.SchemeName
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
    
    #
    # Property Setters
    #
    def _set_Scheme(self, value):
        self._technique.SchemeName = value
         
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Name': _get_Name, 'NumPasses': _get_NumPasses, 'Parent': _get_Parent, 'IsLoaded': _get_IsLoaded, 'IsSupported': _get_IsSupported, 'Scheme': _get_Scheme }
    _setters = { 'Scheme': _set_Scheme }
    
#
# This class is just another way of making a Technique, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
# The way to get a Technique is to call Material.GetTechnique() or Material.GetBestTechnique()
#
class _ExistingTechnique(Technique):
    #
    # Constructor
    #
    def __init__(self, technique):
        self.__dict__['_technique'] = technique

    def __setattr__(self, attrname, value):
        Technique.__setattr__(self, attrname, value)
