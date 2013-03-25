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
from Axiom.Animating import InterpolationMode, RotationInterpolationMode, VertexAnimationType
import Multiverse.Base

class Animation:
    #
    # Constructor
    #
    def __init__(self, name, length):
        self.__dict__['_animation'] = ClientAPI._sceneManager.CreateAnimation(name, length)
        self.__dict__['_animationState'] = ClientAPI._sceneManager.CreateAnimationState(name)
        self.__dict__['_trackNum'] = 0
        
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._animation.Name
        
    def _get_Length(self):
        return self._animation.Length
        
    def _get_Enabled(self):
        return self._animationState.IsEnabled
        
    def _get_InterpolationMode(self):
        return self._animation.InterpolationMode
        
    def _get_RotationInterpolationMode(self):
        return self._animation.RotationInterpolationMode
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_Enabled(self, enabled):
        self._animationState.IsEnabled = enabled
        
                
    def _set_InterpolationMode(self, mode):
        self._animation.InterpolationMode = mode
        
    def _set_RotationInterpolationMode(self, mode):
        self._animation.RotationInterpolationMode = mode
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname

            
    _getters = { 'Name': _get_Name, 'Length': _get_Length, 'Enabled': _get_Enabled, 'InterpolationMode': _get_InterpolationMode, 'RotationInterpolationMode': _get_RotationInterpolationMode }
    _setters = { 'Enabled': _set_Enabled, 'InterpolationMode': _set_InterpolationMode, 'RotationInterpolationMode': _set_RotationInterpolationMode }
    
    #
    # Methods
    #
    
    #
    # Generate a new track number to use as a handle
    #
    def _NextTrackNum(self):
        newTrackNum = self._trackNum
        self.__dict__['_trackNum'] = newTrackNum + 1
        return newTrackNum
        
    def CreateNodeTrack(self, sceneNode):
        # create the real animation track
        realTrack = self._animation.CreateNodeTrack(self._NextTrackNum(), sceneNode._realSceneNode)
        
        # wrap it in our api object and return it
        return ClientAPI.NodeAnimationTrack._ExistingNodeAnimationTrack(realTrack)

    def CreatePropertyTrack(self, animableValue):
        # create the real animation track
        realTrack = self._animation.CreateNumericTrack(self._NextTrackNum(), animableValue)
        
        # wrap it in our api object and return it
        return ClientAPI.PropertyAnimationTrack._ExistingPropertyAnimationTrack(realTrack)

    def CreateMorphTrack(self, subMeshName):
        # create the real animation track
        realTrack = self._animation.CreateVertexTrack(self._mesh.GetTrackHandle(subMeshName), VertexAnimationType.Pose);
        
        # wrap it in our api object and return it
        return ClientAPI.MorphAnimationTrack._ExistingMorphAnimationTrack(realTrack, self._mesh)
                
    def AddTime(self, time):
        self._animationState.AddTime(time)

    def SetTime(self, time):
        self._animationState.Time = time
        
    def Dispose(self):
        ClientAPI._sceneManager.DestroyAnimation(self._animation.Name)
        
    def Play(self, speed=1.0, looping=False):
        Multiverse.Base.ClientAPI.PlaySceneAnimation(self._animationState, speed, looping)
        
    def Stop(self):
        Multiverse.Base.ClientAPI.StopSceneAnimation(self._animationState)
        
#
# This class is just another way of making a Animation, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
class _ExistingAnimation(Animation):
    #
    # Constructor
    #
    def __init__(self, anim, entity):
        self.__dict__['_animation'] = anim
        self.__dict__['_animationState'] = entity._Entity.GetAnimationState(anim.Name)
        self.__dict__['_trackNum'] = 0
        self.__dict__['_mesh'] = entity._Entity.Mesh
    
    def __setattr__(self, attrname, value):
        Animation.__setattr__(self, attrname, value)
        
