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

from Axiom.MathLib import Vector3, Ray
import Multiverse.Base
import ClientAPI
import WorldObject

class RaySceneQueryResult:
    def __init__(self, distance, obj, loc):
        self.distance = distance
        self.worldObject = obj
        self.fragmentLocation = loc

class RaySceneQuery:
    #
    # Constructor
    #
    def __init__(self, origin, dir):
        ray = Ray(origin, dir)
        self.__dict__['_rayQuery'] = ClientAPI._sceneManager.CreateRayQuery(ray)
        
    #
    # Methods
    #
    def Execute(self):
        results = self._rayQuery.Execute()
        rv = []
        for entry in results:
            if entry.SceneObject is not None:
                if isinstance(entry.SceneObject.UserData, Multiverse.Base.ObjectNode):
                    existingObject = WorldObject._GetExistingWorldObject(entry.SceneObject.UserData)
                    rv.append(RaySceneQueryResult(entry.Distance, existingObject, None))
                else:
                    ClientAPI.Write("Skipping non-multiverse object: %s" % entry.SceneObject.UserData)
                    # ignore this object
                    pass
            elif entry.worldFragment is not None:
                rv.append(RaySceneQueryResult(entry.Distance, None, entry.worldFragment.SingleIntersection))
        return rv
        
    # def Dispose(self):
    #    ClientAPI._sceneManager.RemoveLight(self._light)
    
