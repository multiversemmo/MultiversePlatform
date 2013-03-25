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
import java.beans.Encoder;
import java.beans.Expression;
import java.io.*;

import multiverse.server.marshalling.Marshallable;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.util.Log;
import multiverse.server.util.MVRuntimeException;


/** Multiverse object type.  Classifies {@link Entity} and
{@link MVObject} objects by their basic type.  ObjectTypes have
a string name and a short id.  The name and id must be unique.
All plugins must use the same names and ids.
<p>
Object types may have one or more "base types".  The base types
are {@link #BASE_STRUCTURE}, {@link #BASE_MOB}, and {@link #BASE_PLAYER}.
Methods {@link #isStructure}, {@link #isMob}, and {@link #isPlayer}
reflect the base type.
<p>
Applications should use the ObjectTypes defined in {@link ObjectTypes}
rather than defining their own.
<p>
Applications create ObjectTypes using {@link #intern intern()}.
Multiple calls to intern() in the same process with the same
parameters will return the same instance.
*/
public class ObjectType
        implements java.io.Serializable, Marshallable
{
    /** No-arg constructor required for marshalling. */
    public ObjectType() {
    }

    /** Structure base type. */
    public static final int BASE_STRUCTURE = 1;

    /** Mob base type. */
    public static final int BASE_MOB = 2;

    /** Player base type. */
    public static final int BASE_PLAYER = 4;

    ObjectType(short type, String typeName, int baseType) {
        this.typeId = type;
        this.typeName = typeName;
        this.baseType = baseType;
    }

    /** Get or create an ObjectType.
        Multiple calls to intern() in the same process with the same
        parameters will return the same instance.
        @param typeId Unique type number.
        @param typeName Unique type name.
    */
    public static ObjectType intern(short typeId, String typeName) {
        return intern(typeId,typeName,0);
    }

    /** Get or create an ObjectType.
        Multiple calls to intern() in the same process with the same
        parameters will return the same instance.
        @param typeId Unique type number.
        @param typeName Unique type name.
        @param baseType Bitwise OR of {@link #BASE_STRUCTURE}, {@link #BASE_MOB}, and {@link #BASE_PLAYER}.
    */
    public static ObjectType intern(short typeId, String typeName,
                int baseType)
    {
        ObjectType objectType = internedTypes.get(typeName);
        if (objectType == null) {
            objectType = new ObjectType(typeId,typeName,baseType);
            internedTypes.put(typeName,objectType);
            internedTypeIds.put(typeId,objectType);
        }
        else if (objectType.getTypeId() != typeId) {
            Log.error("ObjectType.intern: typeId mismatch for \""+typeName+
                "\": existing="+objectType.getTypeId()+" new="+typeId);
        }
        return objectType;
    }

    /** Get object type.
        @return null if {@code typeId} does not exist (use intern() to
        create the object type.)
    */
    public static ObjectType getObjectType(short typeId)
    {
        return internedTypeIds.get(typeId);
    }

    /** Get object type.
        @return null if {@code typeName} does not exist (use intern() to
        create the object type.)
    */
    public static ObjectType getObjectType(String typeName)
    {
        return internedTypes.get(typeName);
    }

    public String toString() {
        return "["+typeName+","+typeId+"]";
    }

    /** Get the object type number. */
    public short getTypeId() {
        return typeId;
    }

    /** Get the object type name. */
    public String getTypeName() {
        return typeName;
    }

    /** True if object base type is a structure. */
    public boolean isStructure() {
        return (baseType & BASE_STRUCTURE) > 0;
    }

    /** True if object base type is a mob. */
    public boolean isMob() {
        return (baseType & BASE_MOB) > 0;
    }

    /** True if object base type is a player. */
    public boolean isPlayer() {
        return (baseType & BASE_PLAYER) > 0;
    }

    /** Get the object base type. */
    public int getBaseType() {
        return baseType;
    }

    /** Internal use only. */
    public void marshalObject(MVByteBuffer buf) {
        buf.putShort(typeId);
    }

    /** Internal use only. */
    public Object unmarshalObject(MVByteBuffer buf) {
        typeId = buf.getShort();
        ObjectType type = getObjectType(typeId);
        if (type == null)
            throw new MVRuntimeException("ObjectType.unmarshalObject: no interned ObjectType for typeId="+typeId);
        return type;
    }

    transient short typeId;
    transient String typeName;
    transient int baseType;

    static Map<String,ObjectType> internedTypes =
        new HashMap<String,ObjectType>();
    static Map<Short,ObjectType> internedTypeIds =
        new HashMap<Short,ObjectType>();

    private static final long serialVersionUID = 1L;

    /** Internal use only. */
    public static class PersistenceDelegate
        extends java.beans.DefaultPersistenceDelegate
    {
        protected Expression instantiate(Object oldInstance, Encoder out)
        {
            ObjectType objectType = (ObjectType) oldInstance;
            return (new Expression(ObjectType.class, "getObjectType",
                    new Object[]{objectType.getTypeId()}));
        }

        protected boolean mutatesTo(Object oldInstance, Object newInstance) {
            return oldInstance == newInstance;
        }
    }

    private void readObject(ObjectInputStream in)
        throws IOException, ClassNotFoundException
    {
        typeId = in.readShort();
    }

    private Object readResolve()
        throws ObjectStreamException
    {
        return getObjectType(typeId);
    }

    private void writeObject(ObjectOutputStream out)
        throws IOException, ClassNotFoundException
    {
        out.writeShort(typeId);
    }
}

