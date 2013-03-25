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

package multiverse.example;

import java.util.List;
import java.util.ArrayList;
import java.util.Collection;
import multiverse.msgsys.*;

public class ChatFilter extends Filter {
   public ChatFilter() { }
   public ChatFilter(int channelId) {
       this.channelId = channelId;
   }
   public Collection<MessageType> getMessageTypes() {
       return filterTypes;
   }
   public boolean matchMessageType(Collection<MessageType> messageTypes) {
       return messageTypes.contains(ChatMessage.MSG_TYPE_CHAT);
   }
   public boolean matchRemaining(Message message) {
       if (message instanceof ChatMessage)
           return (((ChatMessage)message).getChannelId() == channelId);
       else
           return false;
   }
   public int getChannelId() { return channelId; }

   public FilterTable getSendFilterTable() { return sendFilterTable; }
   public FilterTable getReceiveFilterTable() { return receiveFilterTable; }
   public FilterTable getResponderSendFilterTable() { return responderReceiveFilterTable; }
   public FilterTable getResponderReceiveFilterTable() { return responderSendFilterTable; }
 
   int channelId;

   static List<MessageType> filterTypes = new ArrayList<MessageType>(1);
   static {
       filterTypes.add(ChatMessage.MSG_TYPE_CHAT);
   }

   static FilterTable sendFilterTable = new ChatFilterTable();
   static FilterTable receiveFilterTable = new ChatFilterTable();
   static FilterTable responderReceiveFilterTable = new ChatFilterTable();
   static FilterTable responderSendFilterTable = new ChatFilterTable();
}

