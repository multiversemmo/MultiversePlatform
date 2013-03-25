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

class AnimationState:
    #
    # Constructor
    #
    def __init__(self):
        assert False
        
    #
    # Property Getters
    #
    
    def _get_Time(self):
        return self._state.State.Time
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname

            
    _getters = { 'Time' : _get_Time }
    _setters = {  }
    
    #
    # Methods
    #
    def AddTime(self, t):
        return self._state.AddTime(t)
        
    def RegisterTimeEventHandler(self, time, handler):
        AnimationStateEventWrapper(self, handler, time)
    
#
# This class is just another way of making an AnimationState, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
class _ExistingAnimationState(AnimationState):
    #
    # Constructor
    #
    def __init__(self, state):
        self.__dict__['_state'] = state

    def __setattr__(self, attrname, value):
        AnimationState.__setattr__(self, attrname, value)
        
class AnimationStateEventWrapper:

    def __init__(self, state, handler, triggerTime):
        self.animState = state 
        self.realHandler = handler
        state._state.RegisterTimeEventHandler(triggerTime, self.Handler)
        
    def Handler(self, axiomState, triggerTime):
        self.realHandler(self.animState, triggerTime)
