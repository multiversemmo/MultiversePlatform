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

"""
The WorldObject module contains the WorldObject class in the Client Scripting API,
which represents an object in the 3D view.
"""

import Axiom.Core
import ClientAPI
import Multiverse.Network
import System.Collections.Generic
from Multiverse.Network import ObjectNodeType as WorldObjectType
from Multiverse.Network import EncodedObjectIO

class WorldObject:
    """
    This class is a wrapper object for the underlying object, and exposes
    methods and fields to enable modification of these objects.
    @group Properties: _get_* _set_*
    """
    def __init__(self, oid, name, meshName, location, followTerrain=True, orientation=ClientAPI.Quaternion.Identity, scale=ClientAPI.Vector3.UnitScale, objType=WorldObjectType.Prop, displaySubmeshes=True):
        """
        Creates a new WorldObject object.
        @param oid: 64-bit object ID for this object.  For client-created
            objects, you would typically call ClientAPI.GetLocalOID() to
            generate a unique OID.
        @type oid: long
        @param name: name of the WorldObject
        @param meshName: name of the mesh file to display for this object
        @param location: location of the object
        @type location: Vector3
        @keyword followTerrain: if True, objects are kept at ground level when
            they move
        @keyword orientation: orientation of the object
        @type orientation: Quaternion
        @keyword scale: scale of the object
        @type scale: Vector3
        @keyword objType: type of the object.  Valid values are:
          - WorldObjectType.User - for the player
          - WorldObjectType.Npc - for mobs and other players
          - WorldObjectType.Prop - for static objects in the world
          - WorldObjectType.Item - for items that can be picked up
        """
        submeshInfo = None
        if not displaySubmeshes:
            submeshInfo = System.Collections.Generic.List[Multiverse.Network.SubmeshInfo]()
        self.__dict__['_objectNode'] = ClientAPI._worldManager.AddLocalObject(oid, name, meshName, submeshInfo, location, orientation, scale, objType, followTerrain)
        self.__dict__['_disposedHandlers'] = []
        self.__dict__['_positionHandlers'] = []
        self.__dict__['_directionHandlers'] = []
        self.__dict__['_orientationHandlers'] = []
        self.__dict__['_propertyHandlers'] = []
        self._objectNode.Disposed += self._handleDisposed

    def Dispose(self):
        """
        Dispose of the object, removing it from the scene and cleaning up
        all associated resources.
        """
        ClientAPI._worldManager.RemoveObject(self._objectNode.Oid)

    #
    # Operator overload
    #
    def __eq__(self, other):
        if other is None or not isinstance(other, WorldObject):
            return False
        return self._objectNode == other._objectNode

    #
    # Property Getters
    #
    def _get_SceneNode(self):
        return ClientAPI.SceneNode.SceneNode(self._objectNode.SceneNode)
        
    def _get_Position(self):
        return self._objectNode.Position

    def _get_Name(self):
        return self._objectNode.Name
        
    def _get_Orientation(self):
        return self._objectNode.Orientation
        
    def _get_Model(self):
        return ClientAPI.Model._ExistingModel(self._objectNode.Entity)
        
    def _get_AttachmentPoints(self):
        return self._objectNode.AttachmentPoints.Keys
        
    def _get_OID(self):
        return self._objectNode.Oid
        
    def _get_Targetable(self):
        return self._objectNode.Targetable
        
    def _get_ObjectType(self):
        return self._objectNode.ObjectType
    
    def _get_Direction(self):
        return self._objectNode.Direction
        
    def _get_PropertyNames(self):
        return self._objectNode.PropertyNames

    def _get_EnableCollision(self):
        return self._objectNode.UseCollisionObject
                    
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_SceneNode(self, sceneNode):
        self._objectNode.SceneNode = sceneNode

    def _set_Position(self, v):
        self._objectNode.Position = v

    def _set_Direction(self, v):
        self._objectNode.Direction = v
                
    def _set_Orientation(self, q):
        self._objectNode.Orientation = q
        
    def _set_Targetable(self, val):
        self._objectNode.Targetable = val
        
    def _set_EnableCollision(self, val):
        self._objectNode.UseCollisionObject = val
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Position': _get_Position, 'Name': _get_Name, 'SceneNode': _get_SceneNode, 'Orientation': _get_Orientation, 'Model': _get_Model, 'AttachmentPoints': _get_AttachmentPoints, 'OID': _get_OID, 'Targetable': _get_Targetable, 'ObjectType': _get_ObjectType, 'Direction': _get_Direction, 'PropertyNames': _get_PropertyNames, 'EnableCollision': _get_EnableCollision }
    _setters = { 'Position': _set_Position, 'SceneNode': _set_SceneNode, 'Orientation': _set_Orientation, 'Targetable': _set_Targetable, 'Direction': _set_Direction, 'EnableCollision': _set_EnableCollision }
    
    #
    # Methods
    #
    def AttachModel(self, attachmentPoint, model, orientation=ClientAPI.Quaternion.Identity, offset=ClientAPI.Vector3.Zero):
        self._objectNode.AttachScriptObject(attachmentPoint, model._Entity, orientation, offset)

    def AttachObject(self, attachmentPoint, sceneObject, orientation=ClientAPI.Quaternion.Identity, offset=ClientAPI.Vector3.Zero):
        """
        Attach the specified object to the named attachment point on the
        WorldObject. Currently, the object must be either a ParticleSystem or
        Model object.
        @param attachmentPoint: the name of the attachmentPoint
        @param sceneObject:
        @param orientation: a Quaternion with the orientation of the attached
            object relative to the attachment point
        @param offset: a Vector3 with the offset of the attached object
            relative to the attachment point
        """
        self._objectNode.AttachScriptObject(attachmentPoint, sceneObject._movableObject, orientation, offset)
        
    def AttachSound(self, soundSource):
        """
        Attach the given SoundSource to the WorldObject.
        @param soundSource: the SoundSource that will be attached to the
            WorldObject
        """
        self._objectNode.AttachSound(soundSource._soundSource)

    def AttachmentPointPosition(self, attachmentPoint):
        """
        Returns the position in world space of the named attachment point
        on the WorldObject.
        @param attachmentPoint: the name of the attachmentPoint
        @return: the offset of the attachment point in world space
        @rtype Vector3
        """
        return self._objectNode.AttachmentPointPosition(attachmentPoint)
        
    def AttachmentPointOrientation(self, attachmentPoint):
        """
        Returns the orientation in world space of the named attachment point
        on the WorldObject.
        @param attachmentPoint: the name of the attachmentPoint
        @return: the orientation of the attachment point in world space
        @rtype: Quaternion
        """
        return self._objectNode.AttachmentPointOrientation(attachmentPoint)
        
    def AttachmentPointTransform(self, attachmentPoint):
        """
        Returns the world transform of the named attachment point on the
        WorldObject.
        @param attachmentPoint: the name of the attachmentPoint
        @return: the world transform of the attachment point
        @rtype: Matrix4
        """
        return self._objectNode.AttachmentPointTransform(attachmentPoint)
        
    def AttachNode(self, attachmentPointName):
        """
        Creates a new SceneNode, and attaches it to the given attachment point.
        @return: the SceneNode that has been attached to the attachment point
        @rtype: SceneNode
        """
        node = self._objectNode.AttachScriptNode(attachmentPointName)
        return ClientAPI.SceneNode.SceneNode(node)
        
    def DetachNode(self, node):
        """Detaches the SceneNode."""
        self._objectNode.DetachScriptNode(node._realSceneNode)

    def DetachModel(self, model):
        """
        Detach the given Model from the WorldObject.
        @param model: the Model to detach
        """
        self._objectNode.DetachScriptObject(model._Entity)

    def DetachObject(self, sceneObject):
        """
        Detach the given object from the WorldObject.
        @param sceneObject:
        """
        self._objectNode.DetachScriptObject(sceneObject._movableObject)
        
    def DetachSound(self, soundSource):
        """
        Detach the specified SoundSource from the WorldObject.
        @param soundSource: the SoundSource that will be detached from the
            WorldObject
        """
        self._objectNode.DetachSound(soundSource._soundSource)

    def AddAnimation(self, animation, speed=1.0, looping=False, startOffset=0.0, endOffset=0.0, weight=1.0):
        """
        Add the specified animation to the WorldObject's set of active
        animations.
        @param animation: name of the animation to add
        @param speed: amount by which to multiply the speed of the animation.
            1.0 will play the animation at the normal speed. 2.0 will play at
            double speed, etc.
        @param looping: whether the animation should loop until canceled
        @param startOffset: the time offset at which to start the animation
        @param endOffset: the time offset at which to end the animation. Use
            0.0 for the end time.
        @param weight: the weight to apply to this animation
        """
        self._objectNode.AddAnimation(animation, startOffset, endOffset, speed, weight, looping)

    def AddAnimationExt(self, animation, startOffset=0.0, endOffset=0.0, speed=1.0, weight=1.0, looping=False):
        """
        @deprecated: Use WorldObject.AddAnimation()
        """
        ClientAPI._deprecated("1.1", "WorldObject.AddAnimationExt()", "WorldObject.AddAnimation")
        self.AddAnimation(animation, speed, looping, startOffset, endOffset, weight)

    def QueueAnimation(self, animation, speed=1.0, looping=False, startOffset=0.0, endOffset=0.0, weight=1.0):
        """
        Add the specified animation to the WorldObject's animation queue.
        @param animation: the name of the animation to queue
        @param speed: amount by which to multiply the speed of the animation.
            1.0 will play the animation at the normal speed. 2.0 will play at
            double speed, etc.
        @param looping: whether the animation should loop until canceled
        @param startOffset: the time offset at which to start the animation
        @param endOffset: the time offset at which to end the animation. Use
            0.0 for the end time.
        @param weight: the weight to apply to this animation
        @return: the updated AnimationState object
        @rtype: AnimationState
        """
        return ClientAPI.AnimationState._ExistingAnimationState(self._objectNode.QueueAnimation(animation, startOffset, endOffset, speed, weight, looping))

    def QueueAnimationExt(self, animation, startOffset=0.0, endOffset=0.0, speed=1.0, weight=1.0, looping=False):
        """
        @deprecated: Use WorldObject.QueueAnimation()
        """
        ClientAPI._deprecated("1.1", "WorldObject.QueueAnimationExt()", "WorldObject.QueueAnimation")
        return self.QueueAnimation(animation, speed, looping, startOffset, endOffset, weight)
        
    def RemoveAnimation(self, animation):
        """
        Removes an animation from the object's animation queue.
        @param animation: the name of the animation to remove
        """
        self._objectNode.RemoveAnimation(animation)
        
    def ClearAnimations(self):
        """
        Clear all animations from the object's animation queue.
        """
        self._objectNode.ClearAnimationQueue()
        
    def ClearSounds(self):
        """
        Clear all sounds currently playing on the object.
        """
        self._objectNode.ClearSounds()
        
    def GetProperty(self, propName):
        """
        Get the value of an object property. These properties can come
        from the server, or can be set locally in the client by calling
        WorldObject.SetProperty.
        @param propName: the name of the property to retrieve
        @return: the value of the property
        """
        val = self._objectNode.GetProperty(propName)
        if type(val) is System.Collections.Generic.LinkedList[object]:
            val = Multiverse.Network.PropertyMap.ToPythonList(val)
        elif type(val) is System.Collections.Generic.Dictionary[str, object]:
            val = Multiverse.Network.PropertyMap.ToPythonDict(val)
        return val
        
    def FormatProperty(self, propName):
        # TODO: Add docstring
        val = self._objectNode.GetProperty(propName)
        return EncodedObjectIO.FormatEncodedObject(val)
        
    def PropertyExists(self, propName):
        """
        Check to see if the named object property has been defined for
        this WorldObject.
        @param propName: the name of the property to check
        @return: True if the property exists or False if it does not
        """
        return self._objectNode.PropertyExists(propName)
        
    def SetProperty(self, propName, value):
        """
        Sets the value of an object property.<br>
        Important: The value is only set locally. It is not propagated back to
        the server, or other clients.
        @param propName: the name of the property to set
        @param value: an object with the appropriate value
        """
        self._objectNode.SetProperty(propName, value)
        
    def CheckBooleanProperty(self, propName):
        """
        This is a helper function for checking the value of boolean
        properties. not set at all.
        @param propName: the name of the property to set
        @return: this method will return True if the property is set to True.
          It  will return False if the property is set to False, or if the
          property is not set at all.
        """
        return self._objectNode.CheckBooleanProperty(propName)

    def _handleDisposed(self, objNode):
        for handler in self._disposedHandlers:
            handler(self)
            
    def _handlePositionChange(self, sender, arg):
        for handler in self._positionHandlers:
            handler(self)

    def _handleDirectionChange(self, sender, arg):
        for handler in self._directionHandlers:
            handler(self)

    def _handleOrientationChange(self, sender, arg):
        for handler in self._orientationHandlers:
            handler(self)
    
    def _handlePropertyChange(self, sender, arg):
        for handler in self._propertyHandlers:
            handler(self, arg.PropertyName)
                                                
    def RegisterEventHandler(self, eventName, eventHandler):
        """
        Registers a handler for the named event.
        @param eventName: the name of the event
        @param eventHandler: the method to be invoked when the named event
            occurs.
        """
        if eventName == 'Disposed':
            self._disposedHandlers.append(eventHandler)
        elif eventName == 'PositionChange':
            # hook in to client event handler if we haven't done so already
            if len(self._positionHandlers) == 0:
                self._objectNode.PositionChange += self._handlePositionChange
            self._positionHandlers.append(eventHandler)
        elif eventName == 'DirectionChange':
            # hook in to client event handler if we haven't done so already
            if len(self._directionHandlers) == 0:
                self._objectNode.DirectionChange += self._handleDirectionChange
            self._directionHandlers.append(eventHandler)
        elif eventName == 'OrientationChange':
            # hook in to client event handler if we haven't done so already
            if len(self._orientationHandlers) == 0:
                self._objectNode.OrientationChange += self._handleOrientationChange
            self._orientationHandlers.append(eventHandler)
        elif eventName == 'PropertyChange':
            # hook in to client event handler if we haven't done so already
            if len(self._propertyHandlers) == 0:
                self._objectNode.PropertyChange += self._handlePropertyChange
            self._propertyHandlers.append(eventHandler)
                                            
    def RemoveEventHandler(self, eventName, eventHandler):
        """
        Removes a registered handler for the named event.
        @param eventName: the name of the event
        @param eventHandler: the method to be removed
        """
        if eventName == 'Disposed':
            del self._disposedHandlers[self._disposedHandlers.index(eventHandler)]
        elif eventName == 'PositionChange':
            del self._positionHandlers[self._positionHandlers.index(eventHandler)]
            # unhook the client event handler if we don't have any more handlers of our own
            if len(self._positionHandlers) == 0:
                self._objectNode.PositionChange -= self._handlePositionChange
        elif eventName == 'DirectionChange':
            del self._directionHandlers[self._directionHandlers.index(eventHandler)]
            # unhook the client event handler if we don't have any more handlers of our own
            if len(self._directionHandlers) == 0:
                self._objectNode.DirectionChange -= self._handleDirectionChange
        elif eventName == 'OrientationChange':
            del self._orientationHandlers[self._orientationHandlers.index(eventHandler)]
            # unhook the client event handler if we don't have any more handlers of our own
            if len(self._orientationHandlers) == 0:
                self._objectNode.OrientationChange -= self._handleOrientationChange
        elif eventName == 'PropertyChange':
            del self._propertyHandlers[self._propertyHandlers.index(eventHandler)]
            # unhook the client event handler if we don't have any more handlers of our own
            if len(self._propertyHandlers) == 0:
                self._objectNode.PropertyChange -= self._handlePropertyChange

    def PointCollides(self, point):
        """
        Check whether the given point is inside this object's collision volumes.
        @param point: the position to be checked
        @type point: Vector3
        @return: True if the point collides with a this WorldObject's collision
          volume.
        """
        return ClientAPI._worldManager.CollisionHelper.PointInCollisionVolume(point, self._objectNode.Oid)
        
#
# This class is just another way of making a WorldObject, with a different constructor,
# since we don't have constructor overloading within a single class.  This should only
# be used internally by the API.
#
class _ExistingWorldObject(WorldObject):
    #
    # Create a WorldObject based on an objectNode
    #
    # @param objectNode
    def __init__(self, objectNode):
        self.__dict__['_objectNode'] = objectNode
        self.__dict__['_disposedHandlers'] = []
        self.__dict__['_positionHandlers'] = []
        self.__dict__['_directionHandlers'] = []
        self.__dict__['_orientationHandlers'] = []
        self.__dict__['_propertyHandlers'] = []
        self._objectNode.Disposed += self._handleDisposed
        
    def __setattr__(self, attrname, value):
        WorldObject.__setattr__(self, attrname, value)
    
_objectMap = {}

#
# Get an existing world object.
# This should only be used internally by the API.
#
def _GetExistingWorldObject(objectNode):
    if objectNode is None:
        return None
    if objectNode.Oid in _objectMap:
        ret = _objectMap[objectNode.Oid]
        if ret._objectNode == objectNode:
            return ret

    newObj = _ExistingWorldObject(objectNode)
    _objectMap[objectNode.Oid] = newObj
    return newObj
