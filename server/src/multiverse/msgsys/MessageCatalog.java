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

import java.util.*;
import multiverse.server.util.*;

/**
 * A mapping from message type string to integer message number.
 * 
 */
public class MessageCatalog {
    
    /**
     * Look up a message string in the list of catalogs.  A given
     * string is not supposed to be defined in more than one catalog.
     * @return Message number, or null if not found.
     */
    public static Integer getMessageNumber(String msgTypeString) {
        for (MessageCatalog catalog : catalogList) {
            Integer num = catalog.getMsgNumberFromString(msgTypeString);
            if (num != null)
                return num;
        }
        return null;
    }

    /** Get MessageType by message number.
    */
    public static MessageType getMessageType(Integer msgTypeNumber) {
        if (msgTypeNumber == null) 
            return null;
        else {
//             Log.info("MessageCatalog.getMessageType: numberToMsgTypeMap size " + numberToMsgTypeMap.size() + 
//                 ", numberToMsgTypeMap.get(" + msgTypeNumber + ") " + numberToMsgTypeMap.get(msgTypeNumber));
            return numberToMsgTypeMap.get(msgTypeNumber);
        }
    }

    /** Get MessageType by string.
    */
    public static MessageType getMessageType(String msgTypeString) {
        return getMessageType(getMessageNumber(msgTypeString));
    }
    
    /**
     * Create a catalog with the given name and message number range,
     * and add to the catalog set.
     * 
     */
    public static MessageCatalog addMsgCatalog(String name, int firstMsgNumber, int msgNumberCount) {
        MessageCatalog catalog = new MessageCatalog(name, firstMsgNumber, msgNumberCount);
        addMsgCatalog(catalog);
        return catalog;
    }
    
    /**
     * Add a catalog instance to the catalog set.
     * 
     */
    public static MessageCatalog addMsgCatalog(MessageCatalog catalog) {
        assert catalog != null;
        // Check to ensure that there is no overlap in the message
        // numbers handled
        int first = catalog.firstMsgNumber;
        int last = catalog.lastMsgNumber;
        for (MessageCatalog existingCatalog : catalogList) {
            if ((first >= existingCatalog.firstMsgNumber &&
                first <= existingCatalog.lastMsgNumber) ||
                (last <= existingCatalog.lastMsgNumber &&
                last >= existingCatalog.firstMsgNumber)) {
                    throw new RuntimeException(
                        "MessageCatalog.addMsgCatalog: Numbers for catalog '" +
                        catalog.name + "' overlap with number for catalog '" +
                        existingCatalog.name + "'");
            }
        }
        catalogList.add(catalog);
        return catalog;
    }
    
    /*
     * A list of catalogs
     *
     */
    private static List<MessageCatalog> catalogList = new LinkedList<MessageCatalog>();
    

    /**
     * Create message catalog instance.  Applications should use
     * {@link #addMsgCatalog(String,int,int)}.
     *
     */
    public MessageCatalog(String name, int firstMsgNumber, int msgNumberCount) {
        this.name = name;
        this.firstMsgNumber = firstMsgNumber;
        this.nextMsgNumber = firstMsgNumber;
        this.lastMsgNumber = firstMsgNumber + msgNumberCount - 1;
    }

    /**
     * Get message number by string.
     * Applications should use {@link #getMessageNumber}.
     */
    public Integer getMsgNumberFromString(String msgTypeString) {
        return stringToMsgNumberMap.get(msgTypeString);
    }
    
    /**
     * Add a translation for the given msgTypeString, using the next
     * available message number.
     *
     */
    public void addMsgTypeTranslation(String msgTypeString) {
        MessageType msgType = MessageType.intern(msgTypeString);
        addMsgTypeTranslation(msgType);
    }

    /**
     * Add a translation for the given msgTypeString, using the next
     * available message number.
     *
     */
    public void addMsgTypeTranslation(MessageType msgType) {
        addMsgTypeTranslation(msgType, nextMsgNumber++);
    }
    
    /**
     * A seldom-used overloading that adds a translation for the given
     * msgTypeString, using a specific message number.
     *
     */
    public void addMsgTypeTranslation(MessageType msgType, int msgNumber) {
        if (msgNumber < firstMsgNumber || msgNumber > lastMsgNumber)
            throw new RuntimeException("MessageCatalog.addMsgTypeTranslation: the message number " + 
                msgNumber + " is outside the range of numbers handled by catalog '" + name + "'");
        String msgTypeString = msgType.getMsgTypeString();
        if (stringToMsgNumberMap.get(msgTypeString) != null)
            throw new RuntimeException("MessageCatalog.addMsgTypeTranslation: a translation for msg type '" +
                msgTypeString + "' already exists in catalog '" + name + "'");
        else {
            msgType.setMsgTypeNumber(msgNumber);
            stringToMsgNumberMap.put(msgTypeString, msgNumber);
            numberToMsgTypeMap.put(msgNumber, msgType);
            Log.debug("Adding msg type '" + msgTypeString + "', msgNumber " + msgNumber + "/0x" + Integer.toHexString(msgNumber));
        }
    }
    
    public String toString() {
        return "MessageCatalog [name=" + name + "; firstMsgNumber=" + firstMsgNumber + "; lastMsgNumber=" + lastMsgNumber + "]";
    }

    /** Get the first number in the message range. */
    public int getFirstMsgNumber() {
        return firstMsgNumber;
    }

    /** Get the last number in the message range. */
    public int getLastMsgNumber() {
        return lastMsgNumber;
    }

    /*
     * The name of the catalog
     * 
     */
    private String name;
    
    /*
     * The first and last message numbers for the catalog; recorded so
     * we can display the range of message numbers owned by the
     * catalog.
     *
     */
    private int firstMsgNumber;
    private int lastMsgNumber;
    
    /*
     * The next number to be used in creating translations; recorded
     * so we can display the range of message numbers owned by the
     * catalog.
     *
     */
    private int nextMsgNumber;
    

    /*
     * The mapping from msg type string to msg type number
     *
     */
    private static HashMap<String, Integer> stringToMsgNumberMap = new HashMap<String, Integer>();

    /*
     * The mapping from msg type number to msg type
     *
     */
    private static HashMap<Integer, MessageType> numberToMsgTypeMap = new HashMap<Integer, MessageType>();

}
