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

import Multiverse.Config
import Multiverse.Base.ClientAPI
import Multiverse.Base
import Multiverse.Gui
import Multiverse.Interface
import Multiverse.Voice
import Axiom.Core
import Axiom.MathLib
from Axiom.MathLib import Quaternion, Vector3, Vector4
from Axiom.Core import ColorEx
import SceneNode
import AnimationState
import WorldObject
import Model
import ParticleSystem
import Decal
import SoundSource
import Animation
import NodeAnimationTrack
import NodeKeyFrame
import PropertyAnimationTrack
import PropertyKeyFrame
import MorphAnimationTrack
import MorphKeyFrame
import Light
import Camera
import Compositor
import Input
import Interface
import SystemStatus
import Voice
import Multiverse.Lib.TextureFetcher

from log4net import LogManager

from Axiom.Graphics import SceneBlendFactor as SceneBlendFactor
from Axiom.Graphics import CompareFunction as CompareFunction
from Axiom.Graphics import CullingMode as CullingMode
from Axiom.Graphics import ManualCullingMode as SoftwareCullingMode
from Axiom.Graphics import SceneDetailLevel as SceneDetailLevel
from Axiom.Graphics import Shading as Shading
from Axiom.Graphics import LayerBlendOperation as LayerBlendOperation
from Axiom.Graphics import LayerBlendOperationEx as LayerBlendOperationEx
from Axiom.Graphics import LayerBlendSource as LayerBlendSource
from Axiom.Graphics import FilterOptions as FilterOptions
from Axiom.Graphics import TextureAddressing as TextureAddressing
from Axiom.Graphics import TextureType as TextureType
from Axiom.Media import PixelFormat as PixelFormat

import GPUProgramType
import Material
import Technique
import Pass
import TextureUnit
import EditableImage
from Axiom.SceneManagers.Multiverse import ShadowTechnique as ShadowTechnique
from Axiom.Graphics import MaterialManager as _MaterialManager
from Axiom.SceneManagers.Multiverse import DefaultLODSpec
from Multiverse.Base import ScriptableLODSpec

from HardwareCaps import HardwareCaps as _HardwareCaps
from World import World as _World
from Network import Network as _Network

import System.Diagnostics

_client = Multiverse.Base.Client.Instance
_worldManager = _client.WorldManager
_sceneManager = _worldManager.SceneManager
_debug = _client.doTraceConsole
_log = Multiverse.Base.ClientAPI.Log

UnitsPerMeter = Multiverse.Base.Client.OneMeter
ScreenshotPath = Multiverse.Base.Client.ScreenshotPath

def NumExistingScreenshots():
    return Multiverse.Base.Client.NumExistingScreenshots

def GetNextScreenshotFilename():
    return Multiverse.Base.Client.GetNextScreenshotFilename(ScreenshotPath)

def GetNextScreenshotFilename(screenshotPath, extension):
    return Multiverse.Base.Client.GetNextScreenshotFilename(screenshotPath, extension)

def Log(msg):
    LogInfo(msg)

def DebugLog(msg):
    LogDebug(msg)

def LogDebug(msg):
    _log.Debug(msg)
            
def LogInfo(msg):
    _log.Info(msg)
            
def LogWarn(msg):
    _log.Warn(msg)
            
def LogError(msg):
    _log.Error(msg)
            
def Write(msg):
    _log.Debug(msg)
    Interface.DispatchEvent("CHAT_MSG_SYSTEM", [msg, ""])
    
def DebugWrite(msg):
    if _debug:
        Write(msg)

try:
  from Multiverse.Movie import IMovie as Movie
  from Multiverse.Movie import ICodec as MovieCodec
  from Multiverse.Movie import Manager as MovieManager
  from Multiverse.Movie import IMovieTexture as MovieTexture
  from Multiverse.Movie import MovieTextureSource as MovieTextureSource
except:
  Log("Exception on movie import, plugin not loaded")
  
from Multiverse.Web import Browser as WebBrowser

def RegisterEffect(effectName, effectFunc):
    _deprecated("1.1", "ClientAPI.RegisterEffect()", "ClientAPI.World.RegisterEffect()")
    World.RegisterEffect(effectName, effectFunc)
    
def InvokeEffect(effectName, oid, args):
    _deprecated("1.1", "ClientAPI.InvokeEffect()", "ClientAPI.World.InvokeEffect()")
    World.InvokeEffect(effectName, oid, args)

def GetLocalOID():
    return _worldManager.GetLocalOid()

def GetPlayerObject():
    if _worldManager.Player != None:
        return WorldObject._GetExistingWorldObject(_worldManager.Player)
    else:
        return None
    
def GetCurrentTime():
    return Multiverse.Base.WorldManager.CurrentTime
    
def GetObjectByOID(oid):
    _deprecated("1.1", "ClientAPI.GetObjectByOID()", "ClientAPI.World.GetObjectByOID()")
    return World.GetObjectByOID(oid)
    
def GetObjectByName(name):
    _deprecated("1.1", "ClientAPI.GetObjectByName()", "ClientAPI.World.GetObjectByName()")
    return World.GetObjectByName(name)
    
def GetObjectNodeNames():
    _deprecated("1.1", "ClientAPI.GetObjectNodeNames()", "ClientAPI.World.WorldObjectNames")
    return World.WorldObjectNames
    
def GetSoundSource(soundFile, position, looping=False, gain=1.0, ambient=False, local=True):
    realSoundSource = Multiverse.Base.SoundManager.Instance.GetSoundSource(soundFile, ambient, local)
    soundSource = SoundSource._ExistingSoundSource(realSoundSource)
    soundSource.Looping = looping
    soundSource.Gain = gain
    if ambient == False:
        soundSource.Position = position
        
    return soundSource
    

def CreatePoseAnimation(meshName, name, totalTime, frameDescList):
    # name is the name of the new animation
    # totalTime is the length of the animation in seconds
    # frameDescList is a list of frame descriptions.
    #
    # Each frame description describes a key frame, and consists of a tuple of 2 elements.
    # The first element is the time offset for the key frame.
    # The second element is a list of 2 element tuples, with the first element being the name of pose,
    # and the second element being the weight for that pose.  All poses not named are assumed to be weight 0.
        
    mesh = Axiom.Core.MeshManager.Instance.Load("facial.mesh");
    anim = mesh.CreateAnimation(name, totalTime)
    track = anim.CreateVertexTrack(4, Axiom.Animating.VertexAnimationType.Pose);
        
    for time, poses in frameDescList:
        keyframe = track.CreateVertexPoseKeyFrame(time)
            
        for poseName, weight in poses:
            keyframe.AddPoseReference(mesh.GetPoseIndex(poseName), weight)
                
    # force update of animation states
    #self._Entity.Mesh = mesh


def CreateMorphAnimation(model, subMeshName, name, totalTime, frameDescList):
    # name is the name of the new animation
    # totalTime is the length of the animation in seconds
    # frameDescList is a list of frame descriptions.
    #
    # Each frame description describes a key frame, and consists of a tuple of 2 elements.
    # The first element is the time offset for the key frame.
    # The second element is a list of 2 element tuples, with the first element being the name of pose,
    # and the second element being the weight for that pose.  All poses not named are assumed to be weight 0.
        
    anim = model.CreateAnimation(name, totalTime)
    track = anim.CreateMorphTrack(subMeshName)
        
    for time, poses in frameDescList:
        keyframe = track.CreateKeyFrame(time)
            
        for poseName, weight in poses:
            keyframe.AddMorphTarget(poseName, weight)
                
    # force update of animation states
    #self._Entity.Mesh = mesh

def LaunchBrowser(url):
    if url.startswith("http:") or url.startswith("https:"):
        System.Diagnostics.Process.Start(url)
    else:
        Write("browser URLs must use http: or https:")

_newWorldEvents = ['WorldInitialized', 'ObjectAdded', 'ObjectRemoved', 'AmbientLightChanged', 'LightAdded', 'LightRemoved']

def RegisterEventHandler(eventName, eventHandler):
    if eventName in _newWorldEvents:
        _deprecated("1.1", "ClientAPI.RegisterEventHandler('" + eventName + "')", "ClientAPI.World.RegisterEventHandler('" + eventName + "')")
        World.RegisterEventHandler(eventName, eventHandler)
    elif eventName == 'WorldConnect':
        Multiverse.Base.ClientAPI.WorldConnect += eventHandler
    elif eventName == 'WorldDisconnect':
        Multiverse.Base.ClientAPI.WorldDisconnect += eventHandler
    elif eventName == 'FrameStarted':
        Multiverse.Base.ClientAPI.FrameStarted += eventHandler
    elif eventName == 'FrameEnded':
        Multiverse.Base.ClientAPI.FrameEnded += eventHandler
    elif eventName == 'TargetChanged':
        LogError("ClientAPI.RegisterEventHandler('TargetChanged') is no longer available")
        _deprecated("1.1", "ClientAPI.RegisterEventHandler('" + eventName + "')", "a subscription to the 'PLAYER_TARGET_CHANGED' UI Event")
    elif eventName == 'PlayerInitialized':
        _worldManager.PlayerInitializedEvent += eventHandler
    else:
        ClientAPI.LogError("Invalid event name '%s' passed to ClientAPI.RegisterEventHandler" % str(eventName))

        
def RemoveEventHandler(eventName, eventHandler):
    if eventName in _newWorldEvents:
        _deprecated("1.1", "ClientAPI.RemoveEventHandler('" + eventName + "')", "ClientAPI.World.RemoveEventHandler('" + eventName + "')")
        World.RemoveEventHandler(eventName, eventHandler)
    elif eventName == 'WorldConnect':
        Multiverse.Base.ClientAPI.WorldConnect -= eventHandler
    elif eventName == 'WorldDisconnect':
        Multiverse.Base.ClientAPI.WorldDisconnect -= eventHandler
    elif eventName == 'FrameStarted':
        Multiverse.Base.ClientAPI.FrameStarted -= eventHandler
    elif eventName == 'FrameEnded':
        Multiverse.Base.ClientAPI.FrameEnded -= eventHandler
    elif eventName == 'TargetChanged':
        LogError("ClientAPI.RemoveEventHandler('TargetChanged') is no longer available")
        _deprecated("1.1", "ClientAPI.RemoveEventHandler('" + eventName + "')", "a subscription to the 'PLAYER_TARGET_CHANGED' UI Event")
    elif eventName == 'PlayerInitialized':
        _worldManager.PlayerInitializedEvent -= eventHandler
    else:
        ClientAPI.LogError("Invalid event name '%s' passed to ClientAPI.RemoveEventHandler" % str(eventName))
        
def _WorldInitHandler(sender, args):
    global ShadowConfig
    global OceanConfig
    global FogConfig
    global AmbientLight
    global PlayerCamera
    ShadowConfig = _sceneManager.ShadowConfig
    OceanConfig = _sceneManager.OceanConfig
    FogConfig = _sceneManager.FogConfig
    AmbientLight = _sceneManager.AmbientLightConfig

def SetSkyBox(materialName, enable = True):
    _deprecated("1.1", "ClientAPI.SetSkyBox()", "ClientAPI.World.SetSkyBox()")
    World.SetSkyBox(materialName, enable)
    
def GetTerrainHeight(x, z):
    _deprecated("1.1", "ClientAPI.GetTerrainHeight()", "ClientAPI.World.GetTerrainHeight()")
    return World.GetTerrainHeight(x,z)
    
def GetTerrainHeightVector(v):
    _deprecated("1.1", "ClientAPI.GetTerrainHeightVector()", "ClientAPI.World.GetTerrainHeightVector()")
    return World.GetTerrainHeightVector(v)

def GetPlayerCamera():
    return Camera._ExistingCamera(_client.Camera)
    
def GrabPlayerCamera():
    InputHandler.CameraGrabbed = True
    
def ReleasePlayerCamera():
    InputHandler.CameraGrabbed = False
    
def GetMousePosition():
    mousePos = Multiverse.Gui.GuiSystem.Instance.MousePosition
    return mousePos.X, mousePos.Y
    
def GetScreenSize():
    _deprecated("1.1", "ClientAPI.GetScreenSize()", "ClientAPI.HardwareCaps.WindowSize")
    return HardwareCaps.WindowSize
    
def PickTerrain(x, y):
    _deprecated("1.1", "ClientAPI.PickTerrain()", "ClientAPI.World.PickTerrain()")
    return World.PickTerrain(x, y)

def SetTerrainDisplay(terrainDisplay):
    _deprecated("1.1", "ClientAPI.SetTerrainDisplay()", "ClientAPI.World.DisplayTerrain")
    World.DisplayTerrain = terrainDisplay    
    
def GetCharacterEntries():
    """Get the list of characters that were provided by the remote server.
       This is a list of objects of type CharacterEntry, each of which
       contains the properties of the character."""
    return _client.CharacterEntries

def Exit():
    """Notify the client that it should shut down.  This sets an internal
       flag so that the client can shut down gracefully as soon as it is
       possible to do so."""
    return _client.RequestShutdown()

def IsWorldLocal():
    """Expose the client flag that indicates whether we are using a locally
       created world.  This allows scripts to tell if we are communicating
       with the world server, or just showing a local scene."""
    _deprecated("1.1", "ClientAPI.IsWorldLocal()", "ClientAPI.World.IsWorldLocal")
    return World.IsWorldLocal
    
def GetMaterial(name):
    mat = _MaterialManager.Instance.GetByName(name)
    if mat is not None:
        return Material._ExistingMaterial(mat)
    return None

def SetTerrainLODSpec(tilesPerPageDelegate, metersPerSampleDelegate, pageSize, visPageRadius):
    """Create a ScriptableLODSpec instance, passing the four arguments to the
       constructor, and set the WorldManager's WorldLODSpec property to the instance.
       Note well: This method must be called in Startup.py so that it takes
       effect before the world geometry is created."""
    scriptableLODSpec = ScriptableLODSpec(tilesPerPageDelegate, metersPerSampleDelegate, pageSize, visPageRadius)
    _worldManager.WorldLODSpec = scriptableLODSpec

def SetTerrainLODSpecPageSize(pageSize):
    """Create a DefaultLODSpec instance, passing the two arguments to the
       constructor, and set the WorldManager's WorldLODSpec property to the instance.
       Note well: This method must be called in Startup.py so that it takes
       effect before the world geometry is created."""
    defaultLODSpec = DefaultLODSpec(pageSize)
    _worldManager.WorldLODSpec = defaultLODSpec

def RegisterObjectPropertyChangeHandler(propName, handler):
    _deprecated("1.1", "ClientAPI.RegisterObjectPropertyChangeHandler()", "ClientAPI.World.RegisterObjectPropertyChangeHandler()")
    World.RegisterObjectPropertyChangeHandler(propName, handler)

def RemoveObjectPropertyChangeHandler(propName, handler):
    _deprecated("1.1", "ClientAPI.RemoveObjectPropertyChangeHandler()", "ClientAPI.World.RemoveObjectPropertyChangeHandler()")
    World.RemoveObjectPropertyChangeHandler(propName, handler)
    
def GetVolumeRegions(location):
    _deprecated("1.1", "ClientAPI.GetVolumeRegions()", "ClientAPI.World.GetVolumeRegions()")
    return World.GetVolumeRegions(location)
    
def GetLight(name):
    _deprecated("1.1", "ClientAPI.GetLight()", "ClientAPI.World.GetLight()")
    return World.GetLight(name)

def SetMaterialScheme(scheme):
    vp = _sceneManager.CurrentViewport
    vp.MaterialScheme = scheme

def GetMaterialScheme():
    vp = _sceneManager.CurrentViewport
    return vp.MaterialScheme
    
def SetTextureNameOverride(name):
    Axiom.Core.TextureManager.Instance.OverrideName = name

def GetLightNames():
    _deprecated("1.1", "ClientAPI.GetLightNames()", "ClientAPI.World.LightNames")
    return World.LightNames

def RegisterCommandHandler(command, hander):
    LogError("ClientAPI.RegisterCommandHandler is no longer available")
    _deprecated("1.1", "ClientAPI.RegisterCommandHandler", "MarsCommand.RegisterCommandHandler")
        
def _deprecated(version, oldMethod, newMethod):
    Multiverse.Base.ClientAPI.ScriptDeprecated(version, oldMethod, newMethod)
    
Multiverse.Base.ClientAPI.WorldInitialized += _WorldInitHandler
RootSceneNode = SceneNode.SceneNode(_sceneManager.RootSceneNode)
HardwareCaps = _HardwareCaps()
World = _World()
Network = _Network()
InputHandler = None

Log("ClientAPI.py loaded")

#
# Newly added methods to remove lower level calls.  These may need to
# be moved into appropriate modules.
#
def GetMouseoverTarget():
    mousePos = Multiverse.Gui.GuiSystem.Instance.MousePosition
    windowSize = HardwareCaps.WindowSize
    if windowSize is None:
        # We're not initialized yet
        Log("WindowSize not initialized")
        return None
    width, height = windowSize
    objNode = _client.CastRay(mousePos.X / width, mousePos.Y / height)
    if objNode is None:
        return None
    else:
        return World.GetObjectByOID(objNode.Oid)

def TakeScreenshot():
    _client.TakeScreenshot()

def ToggleRenderMode():
    _client.ToggleRenderMode()

def ToggleTexture():
    _client.ToggleTexture()

def ToggleBoundingBoxes():
    _client.ToggleBoundingBoxes()

def ToggleRenderCollisionVolumes():
    _client.ToggleRenderCollisionVolumes()

def GetScreenPosition(pos):
    return _client.GetScreenPosition(pos)

def SetClientParameter(name, val):
    return Multiverse.Config.ParameterRegistry.SetParameter(name, val)

def GetClientParameter(name):
    return Multiverse.Config.ParameterRegistry.GetParameter(name)

_rand = System.Random()

def Random(arg):
    return _rand.Next(arg)
    
def RandomFloat(arg):
    return arg * float(_rand.NextDouble())
    
def FetchRemoteTexture(url, textureName, handler, destWidth=0, destHeight=0, keepAspect=False, fillColor=ColorEx.Black, authUser=None, authPW=None, authDomain=None):
    Multiverse.Lib.TextureFetcher.TextureFetcher.Instance.FetchTexture(url, textureName, handler, destWidth, destHeight, keepAspect, fillColor, authUser, authPW, authDomain)

def SetProfileMarker(message, color=ColorEx.Red):
    _sceneManager.SetProfileMarker(color, message)
    
def GetAssetPath(assetName):
    return Axiom.Core.ResourceManager.ResolveCommonResourceData(assetName)

def GetParameter(paramName):
    """This exposes parameters that may have been set on the command line."""
    return _client.GetParameter(paramName)

def HasParameter(paramName):
    return _client.HasParameter(paramName)
    
def SetParameter(paramName, paramValue):
    _client.SetParameter(paramName, paramValue)
