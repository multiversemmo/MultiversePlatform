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

package multiverse.msgsys;

import java.io.*;
import java.util.*;
import multiverse.server.network.*;
import multiverse.server.marshalling.*;
import multiverse.server.util.Log;

/** Message type object.  Message types have a name and a number.  The
message number is assigned when the MessageType is added to a
{@link MessageCatalog}.
<p>
Applications should declare static MessageType
instances using {@link #intern} and reuse that object.  For example:
<pre>
public static MessageType MSG_TYPE_SKY_FALLING = MessageType.intern("me.SKY_FALLING");
</pre>
It is suggested that the MessageType string name be derived from the
program identifier.  Multiverse reserves all names beginning with "mv.".

*/
public class MessageType implements Serializable, Marshallable {

    /**
     * No argument constructor is needed for marshalling.
     */
    public MessageType() { 
    }
    
    protected MessageType(String msgTypeString) {
        this.msgTypeString = msgTypeString;
        this.msgTypeNumber = -1;
    }
    
    /** Get message type number. */
    public int getMsgTypeNumber() {
        if (msgTypeNumber == -1) {
            Integer number = MessageCatalog.getMessageNumber(msgTypeString);
            if (number != null)
                msgTypeNumber = number;
            else
                msgTypeNumber = 0;
        }
        return msgTypeNumber;
    }

    /** Set message type number.  Applications should add the MessageType
        to a {@link MessageCatalog} to assign a number.
    */
    public void setMsgTypeNumber(int msgTypeNumber) {
        this.msgTypeNumber = msgTypeNumber;
    }

    /** Get message type name.
    */
    public String getMsgTypeString() {
        return msgTypeString;
    }
    
    protected static Map<String, MessageType> internedMsgTypes = new HashMap<String, MessageType>();

    /**
     * Get singleton MessageType instance for a message type name.
     */
    public static MessageType intern(String typeName) {
        MessageType type = internedMsgTypes.get(typeName);
        if (type == null) {
            type = new MessageType(typeName);
            internedMsgTypes.put(typeName, type);
        }
        return type;
    }

    public String toString() {
        return "MessageType['" + msgTypeString + "', " + msgTypeNumber + "]";
    }

    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        boolean b = in.readBoolean();
        if (b) {
            msgTypeNumber = in.readInt();
            MessageType type = MessageCatalog.getMessageType(msgTypeNumber);
            msgTypeString = type.getMsgTypeString();  // Is there any reason to do this?
//             if (Log.loggingNet) {
//                 Log.net("MessageType.readObject: read message type number " + msgTypeNumber + " for " + type);
//            }
        }
        else {
            msgTypeNumber = -1;
            msgTypeString = in.readUTF();
//             if (Log.loggingNet) {
//                 MessageType type = MessageType.intern(msgTypeString);
//                 Log.net("MessageType.readObject: read message type string '" + msgTypeString + "' for " + type);
//             }
        }
    }

    /** Internal use only. */
    public static MessageType readObjectUtility(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        boolean b = in.readBoolean();
        if (b) {
            int msgTypeNumber = in.readInt();
            return MessageCatalog.getMessageType(msgTypeNumber);
        }
        else {
            String msgTypeString = in.readUTF();
            return MessageType.intern(msgTypeString);
        }
    }

    private Object readResolve() 
	throws ObjectStreamException {
        if (msgTypeNumber > 0) {
            
            MessageType type = MessageCatalog.getMessageType(msgTypeNumber);
//             if (Log.loggingNet)
//                 Log.net("MessageType.readResolve: read message type number " + msgTypeNumber + " for " + type);
            return type;
        }
        else {
            MessageType type = MessageType.intern(msgTypeString);
//             if (Log.loggingNet)
//                 Log.net("MessageType.readResolve: read message type string '" + msgTypeString + "' for " + type);
            return type;
        }
    }

    private void writeObject(ObjectOutputStream out)
	throws IOException, ClassNotFoundException {
	if (msgTypeNumber > 0) {
//             if (Log.loggingNet)
//                 Log.net("MessageType.writeObject: writing message type number " + msgTypeNumber + " for " + this);
            out.writeBoolean(true);
            out.writeInt(msgTypeNumber);
        }
        else {
//             if (Log.loggingNet)
//                 Log.net("MessageType.writeObject: writing message type string '" + msgTypeString + "' for " + this);
            out.writeBoolean(false);
            out.writeUTF(msgTypeString);
        }
    }

    /** Internal use only. */
    public static void writeObjectUtility(ObjectOutputStream out, MessageType type)
	throws IOException, ClassNotFoundException {
	if (type.msgTypeNumber > 0) {
            out.writeBoolean(true);
            out.writeInt(type.msgTypeNumber);
        }
        else {
            out.writeBoolean(false);
            out.writeUTF(type.msgTypeString);
        }
    }

    /** Internal use only. */
    public void marshalObject(MVByteBuffer buf) {
	if (msgTypeNumber > 0) {
            buf.putByte((byte)1);
            buf.putShort((short)((int)msgTypeNumber));
        }
        else {
            buf.putByte((byte)0);
            buf.putString(msgTypeString);
        }
    }

    /** Internal use only. */
    public Object unmarshalObject(MVByteBuffer buf) {
        MessageType msgType = null;
        byte b = buf.getByte();
        if (b != 0) {
            int typeNum = buf.getShort();
            msgType = MessageCatalog.getMessageType(typeNum);
            if (msgType == null) {
                Log.error("No MessageType number "+typeNum+" in MessageCatalog");
            }
        }
        else
            msgType = MessageType.intern((String)buf.getString());
        return msgType;
    }

    transient protected String msgTypeString;
    transient protected Integer msgTypeNumber;

    private static final long serialVersionUID = 1L;
}

