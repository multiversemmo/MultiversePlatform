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

import Axiom.Input
import Multiverse.Base
import Multiverse.Input

import ClientAPI

class BaseInputHandler:
    #
    # Methods
    #
    def IsMousePressed(self):
        return self._inputHandler.IsMousePressed()
    
    def IsMousePressed(self, button):
        if button == 1:
            return self._inputHandler.IsMousePressed(Axiom.Input.MouseButtons.Left)
        elif button == 2:
            return self._inputHandler.IsMousePressed(Axiom.Input.MouseButtons.Right)
        elif button == 3:
            return self._inputHandler.IsMousePressed(Axiom.Input.MouseButtons.Middle)
        else:
            return False

class GuiInputHandler(BaseInputHandler):
    #
    # Constructor
    #
    def __init__(self):
        self.__dict__['_inputHandler'] = Multiverse.Input.GuiInputHandler(Multiverse.Base.Client.Instance)

    #
    # Operator overload
    #
    def __eq__(self, other):
        if other is None or not isinstance(other, GuiInputHandler):
            return False
        return self._inputHandler == other._inputHandler

    #
    # Property Getters
    #
    def _get_IsMouseLook(self):
        return self._inputHandler.IsMouseLook()
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    _getters = { 'IsMouseLook': _get_IsMouseLook }

    #
    # Methods
    #
    def IsMousePressed(self):
        return BaseInputHandler.IsMousePressed(self)
    
    def IsMousePressed(self, button):
        return BaseInputHandler.IsMousePressed(self, button)
    
class DefaultInputHandler(GuiInputHandler):
    #
    # Constructor
    #
    def __init__(self):
        self.__dict__['_inputHandler'] = Multiverse.Input.DefaultInputHandler(Multiverse.Base.Client.Instance)

    #
    # Operator overload
    #
    def __eq__(self, other):
        if other is None or not isinstance(other, DefaultInputHandler):
            return False
        return self._inputHandler == other._inputHandler

    #
    # Property Getters
    #
    def _get_CameraGrabbed(self):
        return self._inputHandler.CameraGrabbed
    
    def _get_CameraTargetOffset(self):
        return self._inputHandler.CameraTargetOffset
    
    def _get_FollowTerrain(self):
        return ClientAPI.GetPlayerObject()._objectNode.FollowTerrain

    def _get_IsMouseLook(self):
        return GuiInputHandler._get_IsMouseLook(self)

    def _get_MillisecondsStuckBeforeGotoStuck(self):
        return ClientAPI._client.MillisecondsStuckBeforeGotoStuck
    
    def _get_MouseLookLocked(self):
        return self._inputHandler.MouseLookLocked
    
    def _get_MouseVelocity(self):
        return self._inputHandler.MouseVelocity
    
    def _get_MouseWheelVelocity(self):
        return self._inputHandler.MouseWheelVelocity
    
    def _get_PlayerSpeed(self):
        return self._inputHandler.PlayerSpeed

    def _get_RotateSpeed(self):
        return self._inputHandler.RotateSpeed
        
    def _get_CameraDistance(self):
        return self._inputHandler.CameraDistance
        
    def _get_CameraMaxDistance(self):
        return self._inputHandler.CameraMaxDistance

    def _get_CameraPitch(self):
        return self._inputHandler.CameraPitch

    def _get_CameraMaxPitch(self):
        return self._inputHandler.MaxPitch
        
    def _get_CameraMinPitch(self):
        return self._inputHandler.MinPitch
        
    def _get_CameraYaw(self):
        return self._inputHandler.CameraYaw
        
    def _get_MinPlayerVisibleDistance(self):
        return self._inputHandler.MinPlayerVisibleDistance

    def _get_MinThirdPersonDistance(self):
        return self._inputHandler.MinThirdPersonDistance
                            
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_CameraGrabbed(self, value):
        self._inputHandler.CameraGrabbed = value
    
    def _set_CameraTargetOffset(self, value):
        self._inputHandler.CameraTargetOffset = value
    
    def _set_FollowTerrain(self, value):
        ClientAPI.GetPlayerObject()._objectNode.FollowTerrain = value

    def _set_MillisecondsStuckBeforeGotoStuck(self, value):
        ClientAPI._client.MillisecondsStuckBeforeGotoStuck = value
    
    def _set_MouseLookLocked(self, value):
        self._inputHandler.MouseLookLocked = value
    
    def _set_MouseVelocity(self, value):
        self._inputHandler.MouseVelocity = value

    def _set_MouseWheelVelocity(self, value):
        self._inputHandler.MouseWheelVelocity = value

    def _set_PlayerSpeed(self, value):
        self._inputHandler.PlayerSpeed = value

    def _set_RotateSpeed(self, value):
        self._inputHandler.RotateSpeed = value
        
    def _set_CameraDistance(self, value):
        self._inputHandler.CameraDistance = value
        
    def _set_CameraMaxDistance(self, value):
        self._inputHandler.CameraMaxDistance = value

    def _set_CameraPitch(self, value):
        self._inputHandler.CameraPitch = value

    def _set_CameraMaxPitch(self, value):
        self._inputHandler.MaxPitch = value
        
    def _set_CameraMinPitch(self, value):
        self._inputHandler.MinPitch = value
        
    def _set_CameraYaw(self, value):
        self._inputHandler.CameraYaw = value
        
    def _set_MinPlayerVisibleDistance(self, value):
        self._inputHandler.MinPlayerVisibleDistance = value

    def _set_MinThirdPersonDistance(self, value):
        self._inputHandler.MinThirdPersonDistance = value

    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname

            
    _getters = { 'CameraGrabbed': _get_CameraGrabbed, 'CameraTargetOffset': _get_CameraTargetOffset, 'IsMouseLook': _get_IsMouseLook, 'FollowTerrain': _get_FollowTerrain, 'MouseLookLocked': _get_MouseLookLocked, 'MouseVelocity': _get_MouseVelocity, 'MouseWheelVelocity': _get_MouseWheelVelocity, 'PlayerSpeed': _get_PlayerSpeed, 'RotateSpeed': _get_RotateSpeed, 'CameraDistance': _get_CameraDistance, 'CameraMaxDistance': _get_CameraMaxDistance, 'CameraPitch': _get_CameraPitch, 'CameraMaxPitch': _get_CameraMaxPitch, 'CameraMinPitch': _get_CameraMinPitch, 'CameraYaw': _get_CameraYaw, 'MinPlayerVisibleDistance': _get_MinPlayerVisibleDistance, 'MinThirdPersonDistance': _get_MinThirdPersonDistance }
    _setters = { 'CameraGrabbed': _set_CameraGrabbed, 'CameraTargetOffset': _set_CameraTargetOffset, 'FollowTerrain': _set_FollowTerrain, 'MouseLookLocked': _set_MouseLookLocked, 'MouseVelocity': _set_MouseVelocity, 'MouseWheelVelocity': _set_MouseWheelVelocity, 'PlayerSpeed': _set_PlayerSpeed, 'RotateSpeed': _set_RotateSpeed, 'CameraDistance': _set_CameraDistance, 'CameraMaxDistance': _set_CameraMaxDistance, 'CameraPitch': _set_CameraPitch, 'CameraMaxPitch': _set_CameraMaxPitch, 'CameraMinPitch': _set_CameraMinPitch, 'CameraYaw': _set_CameraYaw, 'MinPlayerVisibleDistance': _set_MinPlayerVisibleDistance, 'MinThirdPersonDistance': _set_MinThirdPersonDistance }
    
    #
    # Methods
    #
    def IsMousePressed(self):
        return BaseInputHandler.IsMousePressed(self)
    
    def IsMousePressed(self, button):
        return BaseInputHandler.IsMousePressed(self, button)
    
    def MoveForward(self, state):
        self._inputHandler.MoveForward(state)
        
    def MoveBackward(self, state):
        self._inputHandler.MoveBackward(state)

    def MoveUp(self, state):
        self._inputHandler.MoveUp(state)

    def MoveDown(self, state):
        self._inputHandler.MoveDown(state)

    def TurnLeft(self, state):
        self._inputHandler.TurnLeft(state)

    def TurnRight(self, state):
        self._inputHandler.TurnRight(state)

    def StrafeLeft(self, state):
        self._inputHandler.StrafeLeft(state)

    def StrafeRight(self, state):
        self._inputHandler.StrafeRight(state)

    def ToggleAutorun(self):
        self._inputHandler.ToggleAutorun()
