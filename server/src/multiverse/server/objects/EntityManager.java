/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

package multiverse.server.objects;

import java.util.Map;
import java.util.HashMap;

import multiverse.server.util.Log;
import multiverse.server.engine.*;

public class EntityManager
{
    public EntityManager()
    {
    }

    /**
     * Get the Entity object for the given OID and namespace
     * @return The Entity corresponding to the OID and namespace, or null
     */
    public static Entity getEntityByNamespace(Long oid, Namespace namespace) {
        synchronized (entitiesByNamespace) {
            Map<Long, Entity> namespaceEntities = entitiesByNamespace.get(namespace);
            if (namespaceEntities == null)
                return null;
            else
                return namespaceEntities.get(oid);
        }
    }
    
    /**
     * Register the entity by its OID and the Namespace passed in.
     * This method silently replaces any existing entity of the same
     * oid and namespace, because that's what the old code did.
     */
    public static void registerEntityByNamespace(Entity entity, Namespace namespace) {
        entity.setNamespace(namespace);
        synchronized (entitiesByNamespace) {
            Map<Long, Entity> namespaceEntities = entitiesByNamespace.get(namespace);
            if (namespaceEntities == null) {
                namespaceEntities = new HashMap<Long, Entity>();
                entitiesByNamespace.put(namespace, namespaceEntities);
            }
            Long oid = entity.getOid();
            if (oid == null)
                Log.error("Entity.registerEntityByNamespace: entity "
                    + entity + ", namespace " + namespace + " oid is null");
            else {
//                 Entity previousEntity = namespaceEntities.get(oid);
//                 if (previousEntity != null)
//                     Log.error("Entity.registerEntityByNamespace: entity "
//                         + oid + ", namespace " + namespace + " is already registered");
//                 else
                    namespaceEntities.put(oid, entity);
            }
        }
    }
    
    /**
     * Unregister the entity, using its OID and the Namespace 
     */
    public static boolean removeEntityByNamespace(Entity entity, Namespace namespace) {
        Long oid = entity.getOid();
        return removeEntityByNamespace(oid, entity, namespace);
    }
    
    /**
     * Look up the Entity by OID and Namespace, and unregister it
     */
    public static boolean removeEntityByNamespace(Long oid, Namespace namespace) {
        return removeEntityByNamespace(oid, null, namespace);
    }
    
    /**
     * Given the entity with the specified OID, belonging to the
     * specified Namespace, remove it from the map of registered
     * entities for the Namespace.
     */
    private static boolean removeEntityByNamespace(Long oid, Entity entity, Namespace namespace) {
        synchronized (entitiesByNamespace) {
            Map<Long, Entity> namespaceEntities = entitiesByNamespace.get(namespace);
            if (namespaceEntities == null) {
                Log.error("Entity.removeEntityByNamespace: there are no entities for namespace " +
                    namespace + ", entity oid is " + oid);
                return false;
            }
            else if (oid == null) {
                Log.error("Entity.removeEntityByNamespace: entity "
                    + entity + ", namespace " + namespace + " oid is null");
                return false;
            }
            else {
                Entity previousEntity = namespaceEntities.get(oid);
                if (previousEntity == null) {
                    Log.error("Entity.removeEntityByNamespace: entity "
                        + oid + ", namespace " + namespace + " is not registered");
                    return false;
                }
                else {
                    if (entity != null && previousEntity != entity) {
                        Log.error("Entity.removeEntityByNamespace: entity "
                            + oid + ", namespace " + namespace + " is not the same as the registered entity");
                        return false;
                    }
                    else {
                        namespaceEntities.remove(oid);
                        return true;
                    }
                }
            }
        }
    }
    
    /**
     * Return an array containing all the Entity objects registered in
     * the Namespace.
     */
    public static Entity[] getAllEntitiesByNamespace(Namespace namespace) {
        synchronized (entitiesByNamespace) {
            Map<Long, Entity> namespaceEntities = entitiesByNamespace.get(namespace);
            if (namespaceEntities == null) {
//                 Log.error("Entity.getAllEntitiesByNamespace: there are no entities for namespace " + namespace);
                return new Entity[0];
            }
            Entity[] entities = new Entity[namespaceEntities.size()];
            int i=0;
            for (Entity entity : namespaceEntities.values())
                entities[i++] = entity;
            return entities;
        }
    }

    public static int getEntityCount()
    {
        synchronized (entitiesByNamespace) {
            int size = 0;
            for (Map<Long, Entity> namespaceEntities :
                        entitiesByNamespace.values())
                size += namespaceEntities.size();
            return size;
        }
    }

    static Map<Namespace, Map<Long, Entity>> entitiesByNamespace =
        new HashMap<Namespace, Map<Long, Entity>>();


}

