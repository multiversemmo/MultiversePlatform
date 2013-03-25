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

class MessageTypes
{
    public static MessageType MSG_TYPE_AGENT_HELLO = MessageType.intern("msgsys.AGENT_HELLO");
    public static MessageType MSG_TYPE_HELLO_RESPONSE = MessageType.intern("msgsys.HELLO_RESPONSE");
    public static MessageType MSG_TYPE_ALLOC_NAME = MessageType.intern("msgsys.ALLOC_NAME");
    public static MessageType MSG_TYPE_ALLOC_NAME_RESPONSE = MessageType.intern("msgsys.ALLOC_NAME_RESPONSE");
    public static MessageType MSG_TYPE_NEW_AGENT = MessageType.intern("msgsys.NEW_AGENT");
    public static MessageType MSG_TYPE_AGENT_STATE = MessageType.intern("msgsys.AGENT_STATE");
    public static MessageType MSG_TYPE_ADVERTISE = MessageType.intern("msgsys.ADVERTISE");
    public static MessageType MSG_TYPE_SUBSCRIBE = MessageType.intern("msgsys.SUBSCRIBE");
    public static MessageType MSG_TYPE_UNSUBSCRIBE = MessageType.intern("msgsys.UNSUBSCRIBE");
    public static MessageType MSG_TYPE_FILTER_UPDATE = MessageType.intern("msgsys.FILTER_UPDATE");
    public static MessageType MSG_TYPE_AWAIT_PLUGIN_DEPENDENTS = MessageType.intern("msgsys.AWAIT_PLUGIN_DEPENDENTS");
    public static MessageType MSG_TYPE_PLUGIN_AVAILABLE = MessageType.intern("msgsys.PLUGIN_AVAILABLE");
    public static MessageType MSG_TYPE_RESPONSE = MessageType.intern("msgsys.RESPONSE");
    public static MessageType MSG_TYPE_BOOLEAN_RESPONSE = MessageType.intern("msgsys.BOOLEAN_RESPONSE");
    public static MessageType MSG_TYPE_LONG_RESPONSE = MessageType.intern("msgsys.LONG_RESPONSE");
    public static MessageType MSG_TYPE_INT_RESPONSE = MessageType.intern("msgsys.INT_RESPONSE");
    public static MessageType MSG_TYPE_STRING_RESPONSE = MessageType.intern("msgsys.STRING_RESPONSE");

    // Use in MessageTypeFilter to match all message types.
    // Not a valid message type.
    public static MessageType MSG_TYPE_ALL_TYPES = MessageType.intern("msgsys.all");

    public static MessageCatalog catalog;

    public static void initializeCatalog()
    {
        if (catalog != null)
            return;
        catalog = MessageCatalog.addMsgCatalog("msgsysCatalog",5000,100);
        catalog.addMsgTypeTranslation(MSG_TYPE_AGENT_HELLO);
        catalog.addMsgTypeTranslation(MSG_TYPE_HELLO_RESPONSE);
        catalog.addMsgTypeTranslation(MSG_TYPE_ALLOC_NAME);
        catalog.addMsgTypeTranslation(MSG_TYPE_NEW_AGENT);
        catalog.addMsgTypeTranslation(MSG_TYPE_AGENT_STATE);
        catalog.addMsgTypeTranslation(MSG_TYPE_ADVERTISE);
        catalog.addMsgTypeTranslation(MSG_TYPE_SUBSCRIBE);
        catalog.addMsgTypeTranslation(MSG_TYPE_UNSUBSCRIBE);
        catalog.addMsgTypeTranslation(MSG_TYPE_FILTER_UPDATE);
        catalog.addMsgTypeTranslation(MSG_TYPE_AWAIT_PLUGIN_DEPENDENTS);
        catalog.addMsgTypeTranslation(MSG_TYPE_PLUGIN_AVAILABLE);

        firstMsgType = MSG_TYPE_AGENT_HELLO;
        lastMsgType = MSG_TYPE_FILTER_UPDATE;

        catalog.addMsgTypeTranslation(MSG_TYPE_RESPONSE);
        catalog.addMsgTypeTranslation(MSG_TYPE_BOOLEAN_RESPONSE);
        catalog.addMsgTypeTranslation(MSG_TYPE_LONG_RESPONSE);
        catalog.addMsgTypeTranslation(MSG_TYPE_INT_RESPONSE);
        catalog.addMsgTypeTranslation(MSG_TYPE_STRING_RESPONSE);
        catalog.addMsgTypeTranslation(MSG_TYPE_ALLOC_NAME_RESPONSE);
    }

    public static boolean isInternal(MessageType type)
    {
        Integer num = type.getMsgTypeNumber();
        return (num >= firstMsgType.getMsgTypeNumber() &&
                num <= lastMsgType.getMsgTypeNumber());
    }

    private static MessageType firstMsgType;
    private static MessageType lastMsgType;

}

