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
import Multiverse.Base
import Multiverse.Base.ClientAPI

class World:
    #
    # Constructor
    #
    def __init__(self):
        self.__dict__['_sceneManager'] = ClientAPI._sceneManager
        self.__dict__['_worldManager'] = ClientAPI._worldManager
        
        self.__dict__['_objectAddedHandlers'] = []
        self.__dict__['_objectRemovedHandlers'] = []
        self.__dict__['_effects'] = {}

        self._worldManager.ObjectAdded += self._handleObjectAdded
        self._worldManager.ObjectRemoved += self._handleObjectRemoved

    #
    # Property Getters
    #

    def _get_LightNames(self):
        lightMap = self._sceneManager.GetMovableObjectMap("Light")
        outList = []
        if not lightMap is None:
            for name in lightMap.Keys:
                outList.append(name)
        return outList
        
    def _get_WorldObjectNames(self):
        return self._worldManager.GetObjectNodeNames()
        
    def _get_WorldObjectOIDs(self):
        return self._worldManager.GetObjectOidList()
        
    def _get_DisplayTerrain(self):
        return self._sceneManager.DisplayTerrain
        
    def _get_IsWorldLocal(self):
        return ClientAPI._client.UseLocalWorld
        
    def _get_WorldName(self):
        return ClientAPI._client.WorldId
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_DisplayTerrain(self, value):
        self._sceneManager.DisplayTerrain = value
        
    def _set_IsWorldLocal(self, value):
        ClientAPI._client.UseLocalWorld = value
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname

            
    _getters = { 'LightNames': _get_LightNames, 'WorldObjectNames': _get_WorldObjectNames, 'WorldObjectOIDs': _get_WorldObjectOIDs, 'DisplayTerrain': _get_DisplayTerrain, 'IsWorldLocal': _get_IsWorldLocal, 'WorldName': _get_WorldName }
    _setters = { 'DisplayTerrain': _set_DisplayTerrain, 'IsWorldLocal': _set_IsWorldLocal }
    
    #
    # Methods
    #
    def GetObjectByOID(self, oid):
        objNode = self._worldManager.GetObjectNode(oid)
        if objNode:
            return ClientAPI.WorldObject._GetExistingWorldObject(objNode)
        return None
        
    def GetObjectByName(self, name):
        objNode = self._worldManager.GetObjectNode(name)
        if objNode:
            return ClientAPI.WorldObject._GetExistingWorldObject(objNode)
        return None
        
    def SetSkyBox(self, materialName, enable = True, distance = Multiverse.Base.Client.HorizonDistance):
        self._sceneManager.SetSkyBox(enable, materialName, distance)

    def GetTerrainHeight(self, x, z):
        return self._worldManager.GetHeightAt(ClientAPI.Vector3(x, 0, z))
    
    def GetTerrainHeightVector(self, v):
        return ClientAPI.Vector3(v.x, self._worldManager.GetHeightAt(v), v.z)    

    def PickTerrain(self, x, y):
        return self._worldManager.PickTerrain(x, y)
        
    def GetVolumeRegions(self, location):
        vols = self._worldManager.RegionsContaingPoint(location)
        ret = []
        for vol in vols:
            ret.append((vol.Oid, vol.Region))
        return ret
        
    def GetLight(self, name):
        return ClientAPI.Light._ExistingLight(self._sceneManager.GetLight(name))
        
    def RegisterObjectPropertyChangeHandler(self, propName, handler):
        self._worldManager.RegisterObjectPropertyChangeHandler(propName, handler)

    def RemoveObjectPropertyChangeHandler(self, propName, handler):
        self._worldManager.RemoveObjectPropertyChangeHandler(propName, handler)
        
    def _handleObjectAdded(self, sender, objNode):
        worldObj = ClientAPI.WorldObject._GetExistingWorldObject(objNode)
        for handler in self._objectAddedHandlers:
            handler(worldObj)

    def _handleObjectRemoved(self, sender, objNode):
        worldObj = ClientAPI.WorldObject._GetExistingWorldObject(objNode)
        for handler in self._objectRemovedHandlers:
            handler(worldObj)
                            
    def RegisterEventHandler(self, eventName, eventHandler):
        if eventName == 'WorldInitialized':
            Multiverse.Base.ClientAPI.WorldInitialized += eventHandler
        elif eventName == 'ObjectAdded':
            self._objectAddedHandlers.append(eventHandler)
        elif eventName == 'ObjectRemoved':
            self._objectRemovedHandlers.append(eventHandler)
        elif eventName == 'AmbientLightChanged':
            self._sceneManager.AmbientLightChanged += eventHandler
        elif eventName == 'LightAdded':
            self._sceneManager.LightAdded += eventHandler
        elif eventName == 'LightRemoved':
            self._sceneManager.LightRemoved += eventHandler
        elif eventName == 'LoadingStateChanged':
            self._worldManager.OnLoadingStateChange += eventHandler
        else:
            ClientAPI.LogError("Invalid event name '%s' passed to World.RegisterEventHandler" % str(eventName))
            
    def RemoveEventHandler(self, eventName, eventHandler):
        if eventName == 'WorldInitialized':
            Multiverse.Base.ClientAPI.WorldInitialized -= eventHandler
        elif eventName == 'ObjectAdded':
            del self._objectAddedHandlers[_objectAddedHandlers.index(eventHandler)]
        elif eventName == 'ObjectRemoved':
            del self._objectRemovedHandlers[_objectRemovedHandlers.index(eventHandler)]
        elif eventName == 'AmbientLightChanged':
            self._sceneManager.AmbientLightChanged -= eventHandler
        elif eventName == 'LightAdded':
            self._sceneManager.LightAdded -= eventHandler
        elif eventName == 'LightRemoved':
            self._sceneManager.LightRemoved -= eventHandler
        elif eventName == 'LoadingStateChanged':
            self._worldManager.OnLoadingStateChange -= eventHandler
        else:
            ClientAPI.LogError("Invalid event name '%s' passed to World.RemoveEventHandler" % str(eventName))

    def RegisterEffect(self, effectName, effectFunc):
        ClientAPI.DebugLog("Registering Effect " + effectName)
        self._effects[effectName] = effectFunc
    
    def InvokeEffect(self, effectName, oid, args):
        ClientAPI.DebugLog("Invoking Effect " + effectName)
        instance = self._effects[effectName](oid)
        ret = instance.ExecuteEffect(**args)
        if ret != None:
            Multiverse.Base.ClientAPI.QueueYieldEffect(ret, 0)            

    # Set the loading screen either visible or invisible
    def SetLoadingScreenVisible(self, visible):
        ClientAPI.LogDebug("World.py: Setting loading screen visible '%s'" % str(visible))
        ClientAPI._client.LoadWindowVisible = visible

    # Determine if we're adding new scene nodes to the set of rendered
    # nodes.  You could set it to false during scene changes
    def SetUpdateRenderTargets(self, update):
        ClientAPI._client.UpdateRenderTargets = update

    # Rebuild static geometry; called when the LoadingState turns to false.
    def RebuildStaticGeometry(self, msg, startingLoad):
        #ClientAPI.LogDebug("World.py.RebuildStaticGeometry(): startingLoad %s" % str(startingLoad))
        ClientAPI._worldManager.RebuildStaticGeometryAfterLoading(msg, startingLoad)
