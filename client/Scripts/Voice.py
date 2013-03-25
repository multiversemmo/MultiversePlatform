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

from Multiverse.Voice import VoiceManager
from Multiverse.Voice import VoiceParmSet
from Multiverse.Base import SoundManager
from Multiverse.Base import VoiceChatConfig
from Multiverse.Utility import ConfigManager
import Multiverse.Base
import ClientAPI

_client = Multiverse.Base.Client.Instance
_worldManager = _client.WorldManager

def MakeHelpString(common):
    return VoiceParmSet.defaultVoiceParms.MakeHelpString(common)

def VoiceManagerRunning():
    return _worldManager.VoiceMgr != None

def StartVoiceManager(args, connectedToVoiceServer):
    _worldManager.VoiceMgr = VoiceManager.Configure(args, connectedToVoiceServer)

def StopVoiceManager():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't be stopped")
    else:
        _worldManager.DisposeVoiceManager()

def ConfigureVoiceManager(args, connectedToVoiceServer):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't be configured")
    else:
        _worldManager.VoiceMgr = VoiceManager.Reconfigure(_worldManager.VoiceMgr, args, connectedToVoiceServer)

def StartTestPlayback(args):
    if _worldManager.VoiceMgr != None:
        StopVoiceManager()
    StartVoiceManager(args, None)
    _worldManager.VoiceMgr.StartTestPlayback("RecordMic.speex")

def PushToTalk(nowTalking):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call PushToTalk")
    else:
        _worldManager.VoiceMgr.PushToTalk(nowTalking)

def BlacklistSpeaker(speakerOid, doit):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call BlacklistSpeaker")
    else:
        _worldManager.VoiceMgr.BlacklistSpeaker(speakerOid, doit)

def SpeakerBlacklisted(speakerOid):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call SpeakerBlacklisted")
        return False
    else:
        return _worldManager.VoiceMgr.SpeakerBlacklisted(speakerOid)

def GetBlacklistedSpeakers():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call GetBlacklistedSpeakers")
        return None
    else:
        return _worldManager.VoiceMgr.GetBlacklistedSpeakers()


def GetMicNumber():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call GetMicNumber")
        return None
    else:
        return _worldManager.VoiceMgr.GetMicNumber()

def GetMicLevel():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call SetMicLevel")
        return None
    else:
        return _worldManager.VoiceMgr.GetMicLevel()
    
def SetMicLevel(micNumber, level):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call SetMicLevel")
    else:
        _worldManager.VoiceMgr.SetMicLevel(micNumber, level)
    
# This will return a C# Dictionary.  What should it really return?
def RecentSpeakers():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call SetMicLevel")
        return None
    else:
        return _worldManager.VoiceMgr.RecentSpeakers()

# Is the player with oid speakerOid audible to this client?
def NowSpeaking(speakerOid):
    if _worldManager.VoiceMgr == None:
        return False
    else:
        return _worldManager.VoiceMgr.NowSpeaking(speakerOid)

# Is the client currently transmitting voice frames?
def MicSpeaking():
    if _worldManager.VoiceMgr == None:
        return False
    else:
        return _worldManager.VoiceMgr.GetMicrophoneChannel(0).MicSpeaking

def GetAllMicrophoneDevices():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call GetAllMicrophoneDevices")
        return None
    else:
        return _worldManager.VoiceMgr.GetAllMicrophoneDevices()
    
def GetAllPlaybackDevices():
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call GetAllPlaybackDevices")
        return None
    else:
        return _worldManager.VoiceMgr.GetAllPlaybackDevices()
    
def SetPlaybackVolume(speakerOid, level):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call SetPlaybackVolume")
    else:
        _worldManager.VoiceMgr.SetPlaybackVolume(speakerOid, level)

def SetPlaybackVolumeForAllSpeakers(level):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call SetPlaybackVolumeForAllSpeakers")
    else:
        _worldManager.VoiceMgr.SetPlaybackVolumeForAllSpeakers(level)

def GetPlaybackVolume(speakerOid):
    if _worldManager.VoiceMgr == None:
        ClientAPI.Write("WorldManager.VoiceMgr is null, so can't call GetPlaybackVolume")
        return 0.0
    else:
        return _worldManager.VoiceMgr.GetPlaybackVolume(speakerOid)

def TranslateHostname(hostname):
    return _worldManager.TranslateHostname(hostname)

def ConnectedToVoiceServer():
    if _worldManager.VoiceMgr == None:
        return False
    else:
        return _worldManager.VoiceMgr.ConnectedToVoiceServer()

def ConfigRequiresRestart(args):
    return VoiceManager.ConfigRequiresRestart(_worldManager.VoiceMgr, args)
