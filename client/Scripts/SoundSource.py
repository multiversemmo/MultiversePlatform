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

class SoundSource:

    def __init__(self):
        assert False
        
    #
    # Property Getters
    #
    def _get_Position(self):
        return self._soundSource.Position
    
    def _get_Looping(self):
        return self._soundSource.Looping
        
    def _get_Ambient(self):
        return self._soundSouce.Ambient
        
    def _get_Gain(self):
        return self._soundSource.Gain
        
    def _get_SoundFile(self):
        return self._soundSource.SoundFile

    def _get_MaxAttenuationDistance(self):
        return self._soundSource.MaxAttenuationDistance

    def _get_MinAttenuationDistance(self):
        return self._soundSource.MinAttenuationDistance
    
    def _get_LinearAttenuation(self):
        return self._soundSource.LinearAttenuation
                        
    def _get_Name(self):
        return self._soundSource.Name

    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_Position(self, pos):
        self._soundSource.Position = pos
    
    def _set_Looping(self, looping):
        self._soundSource.Looping = looping
        
    def _set_Ambient(self, ambient):
        self._soundSource.Ambient = ambient
        
    def _set_Gain(self, gain):
        self._soundSource.Gain = gain
        
    def _set_SoundFile(self, soundfile):
        self._soundSource.SoundFile = soundfile

    def _set_MinAttenuationDistance(self, min):
        self._soundSource.MinAttenuationDistance = min

    def _set_MaxAttenuationDistance(self, max):
        self._soundSource.MaxAttenuationDistance = max
        
    def _set_LinearAttenuation(self, linear):
        self._soundSource.LinearAttenuation = linear
                        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Position': _get_Position, 'Looping': _get_Looping, 'Ambient': _get_Ambient, 'Gain': _get_Gain, 'SoundFile': _get_SoundFile, 'MinAttenuationDistance': _get_MinAttenuationDistance, 'MaxAttenuationDistance': _get_MaxAttenuationDistance, 'LinearAttenuation' : _get_LinearAttenuation, 'Name' : _get_Name }
    _setters = { 'Position': _set_Position, 'Looping': _set_Looping, 'Ambient': _set_Ambient, 'Gain': _set_Gain, 'SoundFile': _set_SoundFile, 'MinAttenuationDistance': _set_MinAttenuationDistance, 'MaxAttenuationDistance': _set_MaxAttenuationDistance, 'LinearAttenuation' : _set_LinearAttenuation }

    #
    # Methods
    #
    def Stop(self):
        self._soundSource.Stop()
        
    def Play(self):
        self._soundSource.Play()

    def Remove(self, name):
        self._soundSource.Remove(name)

#
# This class is just another way of making a SoundSource, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
# Since SoundSources are all allocated by the SoundManager, this will be the only
#  way to make SoundSource
#
class _ExistingSoundSource(SoundSource):
    #
    # Constructor
    #
    def __init__(self, soundSource):
        self.__dict__['_soundSource'] = soundSource

    def __setattr__(self, attrname, value):
        SoundSource.__setattr__(self, attrname, value)
        
