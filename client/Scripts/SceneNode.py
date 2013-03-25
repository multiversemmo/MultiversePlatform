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

class SceneNode:

    #
    # Constructor
    #
    def __init__(self, arg):
        if type(arg) is Axiom.Core.SceneNode:
            # build from an existing scene node
            #self._realSceneNode = arg
            self.__dict__['_realSceneNode'] = arg
        elif type(arg) is Axiom.Core.Node:
            self.__dict__['_realSceneNode'] = arg
        elif type(arg) is Axiom.Animating.TagPoint:
            self.__dict__['_realSceneNode'] = arg
        else:
            # create a new scene node
            self.__dict__['_realSceneNode'] = ClientAPI._sceneManager.CreateSceneNode(arg)

    #
    # Property getters
    #
    def _get_Position(self):
        return self._realSceneNode.Position

    def _get_Orientation(self):
        return self._realSceneNode.Orientation

    def _get_DerivedPosition(self):
        return self._realSceneNode.DerivedPosition

    def _get_DerivedOrientation(self):
        return self._realSceneNode.DerivedOrientation
                
    def _get_Name(self):
        return self._realSceneNode.Name
        
    def _get_Parent(self):
        return SceneNode(self._realSceneNode.Parent)
        
    def _get_Visible(self):
        return self._realSceneNode.Visible

    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property setters
    #
    def _set_Position(self, v):
        self._realSceneNode.Position = v

    def _set_Orientation(self, q):
        self._realSceneNode.Orientation = q
                
    def _set_Name(self, name):
        self._realSceneNode.Name = name
        
    def _set_Parent(self, parent):
        self._realSceneNode.Parent = parent._realSceneNode
        
    def _set_Visible(self, value):
        self._realSceneNode.Visible = value
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
        
    _getters = { 'Position': _get_Position, 'Name': _get_Name, 'Parent': _get_Parent, 'Orientation': _get_Orientation, 'DerivedPosition': _get_DerivedPosition, 'DerivedOrientation': _get_DerivedOrientation, 'Visible': _get_Visible }
    _setters = { 'Position': _set_Position, 'Name': _set_Name, 'Parent': _set_Parent, 'Orientation': _set_Orientation, 'Visible': _set_Visible }
    
    #
    # Methods
    #
    def Scale(self, v):
        self._realSceneNode.Scale(v)
        
    def Translate(self, v):
        self._realSceneNode.Translate(v)
        
    def Pitch(self, degrees):
        self._realSceneNode.Pitch(degrees)
        
    def Yaw(self, degrees):
        self._realSceneNode.Yaw(degrees)
    
    def Roll(self, degrees):
        self._realSceneNode.Roll(degrees)
        
    def AttachObject(self, movableObject):
        self._realSceneNode.AttachObject(movableObject._movableObject)
        
    def DetachObject(self, movableObject):
        self._realSceneNode.DetachObject(movableObject._movableObject)
        
    def Dispose(self):
        ClientAPI._sceneManager.DestroySceneNode(self._realSceneNode.Name)
        
    def RegisterEventHandler(self, eventName, eventHandler):
        if eventName == 'Updated':
            self._realSceneNode.UpdatedFromParent += eventHandler

    def RemoveEventHandler(self, eventName, eventHandler):
        if eventName == 'Updated':
            self._realSceneNode.UpdatedFromParent -= eventHandler
            
