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
from Axiom.Graphics import LightType

class Light:
    #
    # Constructor
    #
    def __init__(self, name):
        self.__dict__['_light'] = ClientAPI._sceneManager.CreateLight(name)
        self.__dict__['_movableObject'] = self._light
        
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._light.Name
        
    def _get_Type(self):
        return self._light.Type
        
    def _get_Position(self):
        return self._light.Position
        
    def _get_DerivedPosition(self):
        return self._light.DerivedPosition
        
    def _get_Direction(self):
        return self._light.Direction
        
    def _get_DerivedDirection(self):
        return self._light.DerivedDirection
        
    def _get_Diffuse(self):
        return self._light.Diffuse
        
    def _get_Specular(self):
        return self._light.Specular
        
    def _get_AttenuationRange(self):
        return self._light.AttenuationRange
        
    def _get_AttenuationConstant(self):
        return self._light.AttenuationConstant
        
    def _get_AttenuationLinear(self):
        return self._light.AttenuationLinear
        
    def _get_AttenuationQuadratic(self):
        return self._light.AttenuationQuadratic
    
    def _get_IsVisible(self):
        return self._light.IsVisible
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_Type(self, type):
        self._light.Type = type
        
    def _set_Position(self, pos):
        self._light.Position = pos
        
    def _set_DerivedPosition(self, pos):
        self._light.DerivedPosition = pos
        
    def _set_Direction(self, dir):
        self._light.Direction = dir
        
    def _set_DerivedDirection(self, dir):
        self._light.DerivedDirection = dir
        
    def _set_Diffuse(self, color):
        self._light.Diffuse = color
        
    def _set_Specular(self, color):
        self._light.Specular = color
        
    def _set_AttenuationRange(self, range):
        self._light.AttenuationRange = range
        
    def _set_AttenuationConstant(self, constant):
        self._light.AttenuationConstant = constant
        
    def _set_AttenuationLinear(self, linear):
        self._light.AttenuationLinear = linear
        
    def _set_AttenuationQuadratic(self, quad):
        self._light.AttenuationQuadratic = quad
        
    def _set_IsVisible(self, value):
        self._light.IsVisible = value
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Name': _get_Name, 'Type': _get_Type, 'Position': _get_Position, 'DerivedPosition': _get_DerivedPosition,
                 'Direction': _get_Direction, 'DerivedDirection': _get_DerivedDirection, 'Diffuse': _get_Diffuse,
                 'Specular': _get_Specular, 'AttenuationRange': _get_AttenuationRange,
                 'AttenuationConstant': _get_AttenuationConstant, 'AttenuationLinear': _get_AttenuationLinear,
                 'AttenuationQuadratic': _get_AttenuationQuadratic, 'IsVisible' : _get_IsVisible }
    _setters = { 'Type': _set_Type, 'Position': _set_Position, 'DerivedPosition': _set_DerivedPosition,
                 'Direction': _set_Direction, 'DerivedDirection': _set_DerivedDirection, 'Diffuse': _set_Diffuse,
                 'Specular': _set_Specular, 'AttenuationRange': _set_AttenuationRange,
                 'AttenuationConstant': _set_AttenuationConstant, 'AttenuationLinear': _set_AttenuationLinear,
                 'AttenuationQuadratic': _set_AttenuationQuadratic, 'IsVisible' : _set_IsVisible }
    
    #
    # Methods
    #
    def CreateAnimableValue(self, valueName):
        return self._light.CreateAnimableValue(valueName)
        
    def Dispose(self):
        ClientAPI._sceneManager.RemoveLight(self._light)
    
#
# This class is just another way of making a Light, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
class _ExistingLight(Light):
    #
    # Constructor
    #
    def __init__(self, light):
        self.__dict__['_light'] = light
        self.__dict__['_movableObject'] = self._light
        
    
    def __setattr__(self, attrname, value):
        Light.__setattr__(self, attrname, value)
        
