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

from Axiom.Animating import *

import ClientAPI
import Axiom.Core

class Model:
    #
    # Constructor
    #
    def __init__(self, name, meshName):
        self.__dict__['_Entity'] = ClientAPI._sceneManager.CreateEntity(name, meshName)
        self.__dict__['_movableObject'] = self._Entity
        
    def Dispose(self):
        ClientAPI._sceneManager.RemoveEntity(self._Entity)
        
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._Entity.Name
        
    def _get_ParentNode(self):
        return ClientAPI.SceneNode.SceneNode(self._Entity.ParentNode)

    def _get_MeshName(self):
        return self._Entity.Mesh.Name

    def _get_SubMeshNames(self):
        subEntities = self._Entity.SubEntities
        nameList = []
        for subEntity in subEntities:
            nameList.append(subEntity.SubMesh.Name)
        return nameList
        
    def _get_AnimationNames(self):
        animStates = self._Entity.GetAllAnimationStates().Values
        nameList = []
        for animState in animStates:
            nameList.append(animState.Name)
        return nameList
        
    def _get_PoseNames(self):
        nameList = []
        poseList = self._Entity.Mesh.PoseList
        for pose in poseList:
            nameList.append(pose.Name)
        return nameList
        
    def _get_IsVisible(self):
        return self._Entity.IsVisible
    
    def _get_Bounds(self):
        box = self._Entity.BoundingBox
        return (box.Minimum, box.Maximum)
    
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
    #
    # Property Setters
    #
    def _set_Name(self, name):
        self._Entity.Name = name

    def _set_IsVisible(self, val):
        self._Entity.IsVisible = val
                 
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Name': _get_Name, 'ParentNode': _get_ParentNode, 'MeshName': _get_MeshName,
                 'SubMeshNames': _get_SubMeshNames, 'AnimationNames': _get_AnimationNames,
                 'PoseNames': _get_PoseNames, 'IsVisible': _get_IsVisible, 'Bounds': _get_Bounds }
    _setters = { 'Name': _set_Name, 'IsVisible': _set_IsVisible }

    #
    # Methods
    #
    def ShowSubMesh(self, name):
        self._Entity.GetSubEntity(name).IsVisible = True
        
    def HideSubMesh(self, name):
        self._Entity.GetSubEntity(name).IsVisible = False

    def SetSubMeshMaterial(self, submeshName, materialName):
        self._Entity.GetSubEntity(submeshName).MaterialName = materialName

    def GetSubMeshMaterial(self, submeshName):
        return self._Entity.GetSubEntity(submeshName).MaterialName
                
    def CreateAnimation(self, name, totalTime):
        realanim = self._Entity.Mesh.CreateAnimation(name, totalTime)
        # set the mesh back to itself to reset the animation states
        self._Entity.Mesh = self._Entity.Mesh
        anim = ClientAPI.Animation._ExistingAnimation(realanim, self)
        return anim

    def GetAnimation(self, name):
        """Retrieve the animation with the specified name from the list
           of animations associated with this entity."""
        realanim = None
        if self._Entity.Skeleton is not None and self._Entity.Skeleton.ContainsAnimation(name):
            realanim = self._Entity.Skeleton.GetAnimation(name)
        elif self._Entity.Mesh is not None and self._Entity.Mesh.ContainsAnimation(name):
            realanim = self._Entity.Mesh.GetAnimation(name)
        else:
            return None
        anim = ClientAPI.Animation._ExistingAnimation(realanim, self)
        return anim
        
    def AnimationLength(self, name):
        return self._Entity.GetAnimationState(name).Length

    def SetAnimationBlendMode(self, animType):
        if not self._Entity.Skeleton is None:
            if (animType == "average"):
                self._Entity.Skeleton.BlendMode = SkeletalAnimBlendMode.Average
            elif (animType == "cumulative"):
                self._Entity.Skeleton.BlendMode = SkeletalAnimBlendMode.Cumulative
        

#
# This class is just another way of making a Model, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
class _ExistingModel(Model):
    #
    # Constructor
    #
    def __init__(self, entity):
        self.__dict__['_Entity'] = entity
        self.__dict__['_movableObject'] = self._Entity
        
    
    def __setattr__(self, attrname, value):
        Model.__setattr__(self, attrname, value)
