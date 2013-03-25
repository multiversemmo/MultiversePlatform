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
import Axiom.Core
import Axiom.ParticleSystems

class ParticleSystem:
    #
    # Constructor
    #
    def __init__(self, name, particleSystem):
        self.__dict__['_ParticleSystem'] = Axiom.ParticleSystems.ParticleSystemManager.Instance.CreateSystem(name, particleSystem)
        self.__dict__['_movableObject'] = self._ParticleSystem
        
    def Dispose(self):
        Axiom.ParticleSystems.ParticleSystemManager.Instance.RemoveSystem(self._ParticleSystem.Name)
        
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._ParticleSystem.Name
        
    def _get_ParentNode(self):
        return ClientAPI.SceneNode.SceneNode(self._ParticleSystem.ParentNode)
        
    def _get_DefaultWidth(self):
        return self._ParticleSystem.DefaultWidth
        
    def _get_DefaultHeight(self):
        return self._ParticleSystem.DefaultHeight
        
    def _get_ParticleQuota(self):
        return self._ParticleSystem.ParticleQuota
        
    def _get_ParticleCount(self):
        return self._ParticleSystem.ParticleCount
        
    def _get_Color(self):
        return self._ParticleSystem.Color
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
    
    #
    # Property Setters
    #       
    def _set_Name(self, name):
        self._ParticleSystem.Name = name
         
    def _set_DefaultWidth(self, width):
        self._ParticleSystem.DefaultWidth = width
        
    def _set_DefaultHeight(self, height):
        self._ParticleSystem.DefaultHeight = height
        
    def _set_ParticleQuota(self, quota):
        self._ParticleSystem.ParticleQuota = quota

    def _set_Color(self, color):
        self._ParticleSystem.Color = color
                
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Name': _get_Name, 'ParentNode': _get_ParentNode, 'DefaultWidth': _get_DefaultWidth, 'DefaultHeight': _get_DefaultHeight, 'ParticleQuota': _get_ParticleQuota, 'ParticleCount': _get_ParticleCount, 'Color': _get_Color }
    _setters = { 'Name': _set_Name, 'DefaultWidth': _set_DefaultWidth, 'DefaultHeight': _set_DefaultHeight, 'ParticleQuota': _set_ParticleQuota, 'Color': _set_Color }
    
    #
    # Methods
    #
    def ScaleVelocity(self, scaleFactor):
        self._ParticleSystem.ScaleVelocity(scaleFactor)
        
        
