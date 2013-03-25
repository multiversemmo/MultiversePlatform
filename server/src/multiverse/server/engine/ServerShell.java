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

package multiverse.server.engine;

import java.util.*;
import multiverse.server.util.Log;
import multiverse.server.objects.ObjectTypes;
import multiverse.msgsys.*;
import gnu.getopt.Getopt;
import multiverse.server.messages.PerceptionMessage;
import multiverse.server.messages.PerceptionFilter;
import multiverse.server.messages.PerceptionTrigger;



public class ServerShell implements MessageCallback
{
    public static ServerShell shell = null;

    public ServerShell() {
    }

    public static MessageType MSG_TYPE_TEST0 = MessageType.intern("test0");
    public static MessageType MSG_TYPE_TEST1 = MessageType.intern("test1");
    public static MessageType MSG_TYPE_TEST2 = MessageType.intern("test2");
    public static MessageType MSG_TYPE_TEST3 = MessageType.intern("test3");
    public static MessageType MSG_TYPE_TEST4 = MessageType.intern("test4");
    public static MessageType MSG_TYPE_TEST5 = MessageType.intern("test5");

    public static void main(String args[])
    {
        Log.init();
        ServerShell shell = new ServerShell();
//        int sendMessages = 1;

        ServerShell.shell = shell;

        configureMessageCatalog();

        String agentName = "mtest"+args[0];
        Getopt g = new Getopt("ServerShell", args, "t:m:");

        int c;
        List<String> tests = new LinkedList<String>();
        while ((c = g.getopt()) != -1) {
            switch(c) {
            case 't':
                String testName = g.getOptarg();
                tests.add(testName);
                break;
            // Ignore the marshalling file, it's already been read by Trampoline
            case 'm':
                break;
            case '?':
                Log.info("Exiting ServerShell because of unrecognized option '" + c + "'");
                System.exit(1);
            }
        }

        MessageAgent agent = new MessageAgent(agentName);
        shell.agent = agent;
        shell.lastMessageCount = 0;
        try {
            agent.openListener();
            List<MessageType> adverts = new LinkedList<MessageType>();
            adverts.add(MSG_TYPE_TEST0);
            adverts.add(MSG_TYPE_TEST1);
            adverts.add(MSG_TYPE_TEST2);
            adverts.add(MSG_TYPE_TEST3);
            adverts.add(MSG_TYPE_TEST4);
            adverts.add(MSG_TYPE_TEST5);
            agent.setAdvertisements(adverts);
            //MessageTypeFilter f1 = new MessageTypeFilter();
            //f1.addMessageType(MSG_TYPE_TEST1);
            //agent.createSubscription(f1, shell);
            agent.connectToDomain("localhost",DomainServer.DEFAULT_PORT);
        }
        catch (Exception e) {
            System.err.println("connectToDomain: "+e);
            e.printStackTrace();
            System.exit(1);
        }
 
        shell.runTests(tests);
    }

    static void configureMessageCatalog()
    {
        MessageCatalog messageCatalog =
                MessageCatalog.addMsgCatalog("test",10000,100);
        messageCatalog.addMsgTypeTranslation(MSG_TYPE_TEST0);
        messageCatalog.addMsgTypeTranslation(MSG_TYPE_TEST1);
        messageCatalog.addMsgTypeTranslation(MSG_TYPE_TEST2);
        messageCatalog.addMsgTypeTranslation(MSG_TYPE_TEST3);
        messageCatalog.addMsgTypeTranslation(MSG_TYPE_TEST4);
        messageCatalog.addMsgTypeTranslation(MSG_TYPE_TEST5);

    }

    public void handleMessage(Message message, int flags)
    {
        System.out.println("** Got message id "+message.getMsgId());
    }

    public void handleMessage2(Message message, int flags)
    {
        if (startTime == 0)
            startTime = System.currentTimeMillis();
        if (System.currentTimeMillis() - startTime >= 1000) {
            System.out.println("** Got message id "+message.getMsgId());
            System.out.println("Message count "+(agent.getAppMessageCount() - lastMessageCount));
            lastMessageCount = agent.getAppMessageCount();
            startTime = System.currentTimeMillis();
        }
    }

    public void runTests(List<String> tests)
    {
        for (String testName : tests) {
            try {
                if (testName.equals("test1")) {
                    new test1();
                }
                else if (testName.startsWith("pub")) {
                    HashMap<String,String> args = parseArgs(testName);
                    new pub(args);
                }
                else if (testName.startsWith("respond")) {
                    HashMap<String,String> args = parseArgs(testName);
                    new respond(args);
                }
                else if (testName.startsWith("rpc")) {
                    HashMap<String,String> args = parseArgs(testName);
                    new rpc(args);
                }
                else if (testName.startsWith("sub")) {
                    HashMap<String,String> args = parseArgs(testName);
                    new sub(args);
                }
                else if (testName.startsWith("sleep")) {
                    HashMap<String,String> args = parseArgs(testName);
                    int seconds = 1;
                    String str = args.get("s");
                    if (str != null)  {
                        try { seconds = Integer.parseInt(str);
                        }
                        catch (Exception ex) {} 
                    }
                    System.out.println("Sleeping "+seconds+" seconds");
                    try { Thread.sleep(seconds * 1000); }
                    catch (Exception ex) {} 
                }
                else
                    System.err.println("Unknown test "+testName);
            }
            catch (Exception ex) {
                System.out.println("ERROR "+ex);
            }
        }
    }

    public class test1 implements Runnable
    {
        public test1() {
            shell = ServerShell.shell;
            agent = shell.agent;
            (new Thread(this)).start();
        }

        public void run() {
            List<MessageType> messageTypes = new LinkedList<MessageType>();
            messageTypes.add(MSG_TYPE_TEST2);
            messageTypes.add(MSG_TYPE_TEST3);
            PerceptionFilter filter = new PerceptionFilter(messageTypes);
            long subId = agent.createSubscription(filter, shell);
            try { Thread.sleep(2000); }
            catch (InterruptedException ex) { }
            if (filter.addTarget(1001)) {
                FilterUpdate update = new FilterUpdate(1);
                update.addFieldValue(PerceptionFilter.FIELD_TARGETS, new Long(1001));
                agent.applyFilterUpdate(subId, update);
            }
        }

        ServerShell shell;
        MessageAgent agent;
    }

    public class pub implements Runnable
    {
        public pub(HashMap<String,String> args) {
            this.args=args;
            shell = ServerShell.shell;
            agent = shell.agent;
            (new Thread(this)).start();
        }

        public void run() {
            MessageType msgType=MSG_TYPE_TEST0;
            Class msgClass = Message.class;
            long oid = 0;
            long noid = 0;
            int interval = 0;
            int count = 1;
            try {
                String arg= args.get("t");
                if (arg != null)
                    msgType = MessageCatalog.getMessageType(arg);
                arg= args.get("class");
                if (arg != null) {
                    if (arg.equals("subj"))
                        msgClass = SubjectMessage.class;
                    else if (arg.equals("targ"))
                        msgClass = TargetMessage.class;
                    else if (arg.equals("multi"))
                        msgClass = PerceptionMessage.class;
                }
                arg= args.get("oid");
                if (arg != null) {
                    oid = Long.parseLong(arg);
                }
                arg= args.get("noid");
                if (arg != null) {
                    noid = Long.parseLong(arg);
                }
                arg= args.get("interval");
                if (arg != null) {
                    interval = Integer.parseInt(arg) * 1000;
                }
                arg= args.get("intervalms");
                if (arg != null) {
                    interval = Integer.parseInt(arg);
                }
                arg= args.get("count");
                if (arg != null) {
                    count = Integer.parseInt(arg);
                }
            }
            catch (Exception ex) { }

            for ( ; count > 0; count--) {
                Message message=null;
                try {
                    message = (Message) msgClass.newInstance();
                }
                catch (Exception ex) {
                    System.out.println("msgClass "+ex);
                }
                message.setMsgType(msgType);
                if (msgClass == SubjectMessage.class) {
                    ((SubjectMessage)message).setSubject(oid);
                }
                if (msgClass == TargetMessage.class) {
                    ((TargetMessage)message).setTarget(noid);
                }
                if (msgClass == PerceptionMessage.class) {
                    ((PerceptionMessage)message).gainObject(noid,oid,ObjectTypes.unknown);
                }

                agent.sendBroadcast(message);

                try { Thread.sleep(interval); }
                catch (Exception ex) {} 
            }
        }

        ServerShell shell;
        MessageAgent agent;
        HashMap<String,String> args;
    }

    public class respond implements Runnable, MessageCallback
    {
        public respond(HashMap<String,String> args) {
            this.args=args;
            shell = ServerShell.shell;
            agent = shell.agent;
            (new Thread(this)).start();
        }

        public void run() {
//            MessageType msgType = MessageTypes.MSG_TYPE_RESPONSE;
//            Class msgClass = ResponseMessage.class;
            MessageType requestType=MSG_TYPE_TEST4;
            try {
                String arg= args.get("t");
//                if (arg != null)
//                    msgType = MessageCatalog.getMessageType(arg);
                arg= args.get("class");
                if (arg != null) {
                }
            }
            catch (Exception ex) { }

            List<MessageType> messageTypes = new LinkedList<MessageType>();
            messageTypes.add(requestType);
            MessageTypeFilter filter = new MessageTypeFilter(messageTypes);
            agent.createSubscription(filter,this,MessageAgent.RESPONDER);
        }

        public void handleMessage(Message message, int flags)
        {
            //System.out.println("** Responder got message id "+
            //    message.getMsgId()+" "+flags);
            ResponseMessage response = new ResponseMessage(message);
            agent.sendResponse(response);
        }

        ServerShell shell;
        MessageAgent agent;
        HashMap<String,String> args;
    }

    public class rpc implements Runnable, ResponseCallback
    {
        public rpc(HashMap<String,String> args) {
            this.args=args;
            shell = ServerShell.shell;
            agent = shell.agent;
            (new Thread(this)).start();
        }

        public void run() {
            MessageType msgType=MSG_TYPE_TEST4;
            Class msgClass = Message.class;
            long oid = 0;
            long noid = 0;
            boolean broadcast = false;
            int count = 1;
            try {
                String arg= args.get("t");
                if (arg != null)
                    msgType = MessageCatalog.getMessageType(arg);
                arg= args.get("class");
                if (arg != null) {
                    if (arg.equals("subj"))
                        msgClass = SubjectMessage.class;
                    else if (arg.equals("targ"))
                        msgClass = TargetMessage.class;
                }
                arg= args.get("oid");
                if (arg != null) {
                    oid = Long.parseLong(arg);
                }
                arg= args.get("noid");
                if (arg != null) {
                    noid = Long.parseLong(arg);
                }
                arg= args.get("broadcast");
                if (arg != null) {
                    broadcast= true;
                }
                arg= args.get("count");
                if (arg != null) {
                    count= Integer.parseInt(arg);
                }
            }
            catch (Exception ex) { }

            long startTime = System.currentTimeMillis();
            for ( int rpcCount = count; rpcCount > 0; rpcCount--) {
                Message message=null;
                try {
                    message = (Message) msgClass.newInstance();
                }
                catch (Exception ex) {
                    System.out.println("msgClass "+ex);
                }
                message.setMsgType(msgType);
                if (msgClass == SubjectMessage.class) {
                    ((SubjectMessage)message).setSubject(oid);
                }
                if (msgClass == TargetMessage.class) {
                    ((TargetMessage)message).setTarget(noid);
                }

                if (broadcast) {
                    /* int recipients = */ agent.sendBroadcastRPC(message, this);
                    //System.out.println("RPC: send to "+recipients+" recipients");
                }
                else {
                    /* Message response = */ agent.sendRPC(message);
                    //System.out.println("RPC: got response msgid="+response.getMsgId()+
                    //      " type="+response.getMessageType()+
                    //      " "+response.getClass().getName());
                }
            }
            System.out.println(count + " rpc in " +
                    (System.currentTimeMillis() - startTime) + " ms");
        }

        public void handleResponse(ResponseMessage response) {
            System.out.println("** handleResponse got message id "+
                response.getMsgId() + " from " + response.getSenderName());
        }

        ServerShell shell;
        MessageAgent agent;
        HashMap<String,String> args;
    }

    public class sub implements Runnable, MessageCallback
    {
        public sub(HashMap<String,String> args) {
            this.args=args;
            shell = ServerShell.shell;
            agent = shell.agent;
            (new Thread(this)).start();
        }

        public void run() {
            MessageType msgType=MSG_TYPE_TEST5;
            MessageType triggerMsgType=MSG_TYPE_TEST0;
            Filter filter= new MessageTypeFilter();
            MessageTrigger trigger=null;
            long noid=0;
            short flags = MessageAgent.NO_FLAGS;
            try {
                String arg= args.get("t");
                if (arg != null) {
                    msgType = MessageCatalog.getMessageType(arg);
                    if (msgType == null) {
                        throw new RuntimeException("Unknown message type "+arg);
                    }
                }
                arg= args.get("filter");
                if (arg != null) {
                    if (arg.equals("multi")) {
                        filter = new PerceptionFilter();
                    }
                }
                arg= args.get("trigger");
                if (arg != null) {
                    if (arg.equals("multi")) {
                        trigger = new PerceptionTrigger();
                    }
                }
                arg= args.get("trigger-type");
                if (arg != null) {
                    triggerMsgType = MessageCatalog.getMessageType(arg);
                    if (msgType == null) {
                        throw new RuntimeException("Unknown message type "+arg);
                    }
                }
                arg= args.get("noid");
                if (arg != null) {
                    noid = Long.parseLong(arg);
                }
                arg= args.get("blocking");
                if (arg != null) {
                    flags |= MessageAgent.BLOCKING;
                }
            }
            catch (RuntimeException ex) { throw ex; }
            catch (Exception ex) { System.out.println("error "+ex); }

            List<MessageType> messageTypes = new LinkedList<MessageType>();
System.out.println("sub msgType "+msgType);
            messageTypes.add(msgType);
            if (filter instanceof MessageTypeFilter) {
                ((MessageTypeFilter)filter).setTypes(messageTypes);
            }
            if (filter instanceof PerceptionFilter) {
                ((PerceptionFilter)filter).setTypes(messageTypes);
                if (noid != 0)
                    ((PerceptionFilter)filter).addTarget(noid);
            }
            if (trigger instanceof PerceptionTrigger) {
                List<MessageType> types = new LinkedList<MessageType>();
                types.add(triggerMsgType);
                ((PerceptionTrigger)trigger).setTriggeringTypes(types);
            }

            long startTime = System.currentTimeMillis();
            /* long subId = */ agent.createSubscription(filter,this, flags, trigger);
            System.out.println("createSubscription "+
                (System.currentTimeMillis()-startTime) + " ms");
        }

        public void handleMessage(Message message, int flags)
        {
            System.out.println("** Subscriber got message id="+
                message.getMsgId() +
                " class="+message.getClass().getName());
        }

        ServerShell shell;
        MessageAgent agent;
        HashMap<String,String> args;
    }

    public static HashMap<String,String> parseArgs(String arg)
    {
        int colon = arg.indexOf(':');
        HashMap<String,String> args = new HashMap<String,String>();

        if (colon == -1)
            return args;

        arg = arg.substring(colon+1);
        String[] strings = arg.split(",");
        for (int ii=0; ii < strings.length; ii++) {
            String[] kv = strings[ii].split("=",2);
            if (kv.length == 1)
                args.put(kv[0],null);
            else 
                args.put(kv[0],kv[1]);
        }
        return args;
    }

    public MessageAgent agent;
    public long lastMessageCount = 0;
    public long startTime = 0;
}


