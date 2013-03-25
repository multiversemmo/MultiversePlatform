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

class HardwareCaps:
    #
    # Constructor
    #
    def __init__(self):
        # should raise an exception
        self.__dict__['_caps'] = ClientAPI._sceneManager.TargetRenderSystem.Caps
        
    #
    # Property Getters
    #
    
    def _get_DeviceName(self):
        return self._caps.DeviceName
        
    def _get_DriverVersion(self):
        return self._caps.DriverVersion
        
    def _get_VideoMemorySize(self):
        try:
            return self._caps.VideoMemorySize
        except:
            return 0
            
    def _get_SystemMemorySize(self):
        return self._caps.SystemMemorySize
    
    def _get_MaxFragmentProgramVersion(self):
        return self._caps.MaxFragmentProgramVersion
        
    def _get_MaxVertexProgramVersion(self):
        return self._caps.MaxVertexProgramVersion
        
    def _get_MaxLights(self):
        return self._caps.MaxLights
        
    def _get_TextureUnitCount(self):
        return self._caps.TextureUnitCount
        
    def _get_WindowSize(self):
        vp = ClientAPI._sceneManager.CurrentViewport
        if vp is None:
            # sometimes we call this before the viewport is established
            return None
        return vp.ActualWidth, vp.ActualHeight
        
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

            
    _getters = { 'DeviceName': _get_DeviceName, 'DriverVersion': _get_DriverVersion, 'VideoMemorySize' : _get_VideoMemorySize,
            'MaxFragmentProgramVersion': _get_MaxFragmentProgramVersion, 'MaxVertexProgramVersion': _get_MaxVertexProgramVersion,
            'MaxLights': _get_MaxLights, 'TextureUnitCount': _get_TextureUnitCount, 'SystemMemorySize': _get_SystemMemorySize,
            'WindowSize': _get_WindowSize }
    _setters = {  }
    
    #
    # Methods
    #
    
    
