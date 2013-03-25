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

package multiverse.management;

import java.io.IOException;
import java.util.ArrayList;

import multiverse.msgsys.*;
import multiverse.server.util.Log;
import multiverse.server.engine.ScriptManager;


public class TransientAgent
{
    public TransientAgent()
    {
    }

    public TransientAgent(String agentName, String domainServer,
        int domainPort)
    {
        this.agentName = agentName;
        this.domainServer = domainServer;
        this.domainPort = domainPort;
        agent = new MessageAgent(agentName);
        agent.setDomainFlags(MessageAgent.DOMAIN_FLAG_TRANSIENT);
        agent.setAdvertisements(new ArrayList<MessageType>());
    }

    public MessageAgent agent()
    {
        return agent;
    }

    public void connect()
        throws IOException
    {
        agent.openListener();
        agent.connectToDomain(domainServer, domainPort);
        agent.waitForRemoteAgents();
    }

    public boolean runScript(String fileName)
    {
        if (scriptManager == null) {
            scriptManager = new ScriptManager();
            org.python.core.Options.verbose = org.python.core.Py.WARNING;
            scriptManager.init();
        }

        try {
            scriptManager.runFileWithThrow(fileName);
        }
        catch (Exception e) {
            Log.exception(fileName,e);
            return false;
        }
        return true;
    }

    private String agentName;
    private String domainServer;
    private int domainPort;
    private MessageAgent agent;
    private ScriptManager scriptManager;

    public static void main(String[] args)
        throws IOException
    {
        String agentName = args[2];
        String domainServer = args[3];
        int domainPort = Integer.parseInt(args[4]);

        Log.init();
        TransientAgent tagent = new TransientAgent(agentName, domainServer,
            domainPort);
        tagent.connect();
    }

}

