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

import java.util.concurrent.locks.*;
import multiverse.server.engine.Namespace;
import multiverse.server.util.*;

/**
 * a handle to an MVObject. when serialized this object stores just the object's
 * OID, so that you do not duplicates of the main object
 */
public class EntityHandle implements java.io.Serializable {
    public EntityHandle() {
        setupTransient();
    }
    public EntityHandle(Long oid) {
        this.oid = oid;
        setupTransient();
    }

    public EntityHandle(Entity entity) {
        this.oid = entity.getOid();
        this.entity = entity;
        setupTransient();
    }

    public boolean equals(Object other) {
        if (other instanceof EntityHandle) {
            return (((EntityHandle) other).getOid().equals(this.getOid()));
        }
        return false;
    }

    public String toString() {
        return "[EntityHandle: objOid=" + getOid() + "]";
    }

    public Entity getEntity(Namespace namespace) {
        lock.lock();
        try {
            if (entity == null) {
                entity = EntityManager.getEntityByNamespace(oid, namespace);
                return entity;
            } else {
                return entity;
            }
        } finally {
            lock.unlock();
        }
    }

    // xml serialization
    public void setOid(Long oid) {
        this.oid = oid;
    }

    public Long getOid() {
        return this.oid;
    }

    private void readObject(java.io.ObjectInputStream in)
            throws java.io.IOException, ClassNotFoundException {
        in.defaultReadObject();
        setupTransient();
    }

    void setupTransient() {
        lock = LockFactory.makeLock("EntityHandle");
    }

    private Long oid = null;

    private transient Entity entity = null;

    private transient Lock lock = null;

    private static final long serialVersionUID = 1L;
}
