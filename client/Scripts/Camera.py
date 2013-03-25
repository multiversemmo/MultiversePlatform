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

from Axiom.Graphics import Projection as Projection

class Camera:
    #
    # Constructor
    #
    def __init__(self, name):
        self.__dict__['_camera'] = ClientAPI._sceneManager.CreateCamera(name)
        self.__dict__['_movableObject'] = self._camera
        
    #
    # Property Getters
    #
    def _get_Position(self):
        return self._camera.Position
        
    def _get_Direction(self):
        return self._camera.Direction
        
    def _get_Orientation(self):
        return self._camera.Orientation
        
    def _get_Up(self):
        return self._camera.Up
        
    def _get_Right(self):
        return self._camera.Right

    def _get_DerivedPosition(self):
        return self._camera.DerivedPosition
        
    def _get_DerivedDirection(self):
        return self._camera.DerivedDirection
        
    def _get_DerivedOrientation(self):
        return self._camera.DerivedOrientation
                
    def _get_DerivedUp(self):
        return self._camera.DerivedUp
        
    def _get_DerivedRight(self):
        return self._camera.DerivedRight
    
    def _get_AutoTrackingTarget(self):
        return self._camera.AutoTrackingTarget
        
    def _get_AutoTrackingOffset(self):
        return self._camera.AutoTrackingOffset
        
    def _get_Far(self):
        return self._camera.Far
        
    def _get_Near(self):
        return self._camera.Near
        
    def _get_AspectRatio(self):
        return self._camera.AspectRatio
        
    def _get_FieldOfView(self):
        return self._camera.FOVy
       
    def _get_ProjectionType(self):
        return self._camera.ProjectionType
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_Position(self, pos):
        self._camera.Position = pos
        
    def _set_Direction(self, dir):
        self._camera.Direction = dir
        
    def _set_Orientation(self, quaternion):
        self._camera.Orientation = quaternion
  
    def _set_AutoTrackingTarget(self, sceneNode):
        if sceneNode is None:
            self._camera.AutoTrackingTarget = None
        else:
            self._camera.AutoTrackingTarget = sceneNode._realSceneNode
        
    def _set_AutoTrackingOffset(self, offset):
        self._camera.AutoTrackingOffset = offset
        
    def _set_Far(self, far):
        self._camera.Far = far
        
    def _set_Near(self, near):
        if self._camera is None:
            ClientAPI.Log("_set_Near: camera is None")
        self._camera.Near = near
        
    def _set_AspectRatio(self, ratio):
        self._camera.AspectRatio = ratio
        
    def _set_FieldOfView(self, fov):
        self._camera.FOVy = fov
        
    def _set_ProjectionType(self, proj):
        self._camera.ProjectionType = proj
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname

            
    _getters = { 'Position': _get_Position, 'Direction': _get_Direction, 'Orientation': _get_Orientation,
                 'Up': _get_Up, 'Right': _get_Right, 'DerivedPosition': _get_DerivedPosition,
                 'DerivedDirection': _get_DerivedDirection, 'DerivedOrientation': _get_DerivedOrientation,
                 'DerivedUp': _get_DerivedUp, 'DerivedRight': _get_DerivedRight, 
                 'AutoTrackingTarget': _get_AutoTrackingTarget, 'AutoTrackingOffset': _get_AutoTrackingOffset,
                 'Far': _get_Far, 'Near': _get_Near, 'AspectRatio': _get_AspectRatio, 'FieldOfView': _get_FieldOfView,
                 'ProjectionType': _get_ProjectionType }
    _setters = { 'Position': _set_Position, 'Direction': _set_Direction, 'Orientation': _set_Orientation,
                 'AutoTrackingTarget': _set_AutoTrackingTarget, 'AutoTrackingOffset': _set_AutoTrackingOffset,
                 'Far': _set_Far, 'Near': _set_Near, 'AspectRatio': _set_AspectRatio, 'FieldOfView': _set_FieldOfView,
                 'ProjectionType': _set_ProjectionType }
    
    #
    # Methods
    #
    
    def LookAt(self, loc):
        self._camera.LookAt(loc)
        
    def Yaw(self, degrees):
        self._camera.Yaw(degrees)
        
    def Pitch(self, degrees):
        self._camera.Pitch(degrees)
        
    def Roll(self, degrees):
        self._camera.Roll(degrees)
        
    def Dispose(self):
        ClientAPI._sceneManager.RemoveCamera(self._camera)
    
#
# This class is just another way of making a Camera, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
class _ExistingCamera(Camera):
    #
    # Constructor
    #
    def __init__(self, camera):
        self.__dict__['_camera'] = camera
        self.__dict__['_movableObject'] = self._camera
        
    def __setattr__(self, attrname, value):
        Camera.__setattr__(self, attrname, value)
        
