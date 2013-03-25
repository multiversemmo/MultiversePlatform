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

import java.util.List;
import java.util.Map;
import java.util.LinkedList;
import java.util.LinkedHashMap;
import java.io.Serializable;
import gnu.getopt.Getopt;
import gnu.getopt.LongOpt;

import multiverse.server.util.Log;
import multiverse.server.util.InitLogAndPid;
import multiverse.msgsys.*;

public class PluginStatusCheck extends TransientAgent
    implements ResponseCallback
{
    public static final int TIMEOUT = 5 * 1000;

    public PluginStatusCheck(String agentName, String domainServer,
        int domainPort)
    {
        super(agentName,domainServer,domainPort);
        agent().addAdvertisement(Management.MSG_TYPE_GET_PLUGIN_STATUS);
    }

    public void connect()
        throws java.io.IOException
    {
        super.connect();
        agent().getDomainClient().awaitPluginDependents("Domain",
            "PluginStatusCheck");
    }

    public List<String> getPluginStatus()
    {
        Message request = new Message(Management.MSG_TYPE_GET_PLUGIN_STATUS);
        expectedResponders = agent().sendBroadcastRPC(request,this);
        synchronized (this) {
            responders += expectedResponders;
            long startTime = System.currentTimeMillis();
            while (responders != 0) {
                try {
                    this.wait(TIMEOUT);
                    if (System.currentTimeMillis() - startTime > TIMEOUT) {
                        break;
                    }
                } catch (InterruptedException e) {
                }
            }
        }
        return pluginStatus;
    }

    public synchronized void handleResponse(ResponseMessage rr)
    {
        responders --;

        GenericResponseMessage response = (GenericResponseMessage) rr;
        LinkedHashMap<String,Serializable> status =
            (LinkedHashMap<String,Serializable>) response.getData();

        String statusString = "";
        String pluginName = (String) status.get("plugin");
        if (pluginName == null)
            pluginName = response.getSenderName();

        for (Map.Entry<String,Serializable> ss : status.entrySet()) {
            if (ss.getKey().equals("plugin"))
                continue;
            statusString += pluginName + "." + ss.getKey() + "=" + ss.getValue() + " ";
            rollupValue(ss.getKey(), ss.getValue());
        }
        pluginStatus.add(statusString);

        if (responders == 0)
            this.notify();
    }

    private void rollupValue(String key, Serializable value)
    {
        Serializable current = rollup.get(key);
        if (value instanceof Integer) {
            if (current == null)
                current = new Integer(0);
            current = ((Integer)current) + ((Integer)value);
            rollup.put(key,current);
        }
        else if (value instanceof Long) {
            if (current == null)
                current = new Long(0);
            current = ((Long)current) + ((Long)value);
            rollup.put(key,current);
        }
        else if (value instanceof Float) {
            if (current == null)
                current = new Float(0);
            current = ((Float)current) + ((Float)value);
            rollup.put(key,current);
        }
        else if (value instanceof Double) {
            if (current == null)
                current = new Double(0);
            current = ((Double)current) + ((Double)value);
            rollup.put(key,current);
        }
    }

    public int getMissingResponders()
    {
        return responders;
    }

    public int getExpectedResponders()
    {
        return expectedResponders;
    }

    public Map<String,Serializable> getRollup()
    {
        return rollup;
    }

    int responders = 0;
    int expectedResponders;
    List<String> pluginStatus = new LinkedList<String>();
    LinkedHashMap<String,Serializable> rollup = 
        new LinkedHashMap<String,Serializable>();

    public static void main(String[] args)
        throws java.io.IOException
    {
        InitLogAndPid.initLogAndPid(args);

        LongOpt[] longopts = new LongOpt[3];
        longopts[0] = new LongOpt("port", LongOpt.REQUIRED_ARGUMENT, null, 2);
        longopts[1] = new LongOpt("keys", LongOpt.REQUIRED_ARGUMENT, null, 3);
        longopts[2] = new LongOpt("host", LongOpt.REQUIRED_ARGUMENT, null, 4);
        Getopt g = new Getopt("PluginStatusCheck", args, "s:a:m:t:", longopts);

        String agentName = "PluginStatusCheck";
        String domainServer = "localhost";
        int domainPort = DomainServer.DEFAULT_PORT;
        List<String> scripts = new LinkedList<String>();

        int c;
        String[] keys = null;
        while ((c = g.getopt()) != -1) {
            switch (c) {
            case 's':
                scripts.add(g.getOptarg());
                break;
            case 'a':
                agentName = g.getOptarg();
                break;
            case 't':
            case 'm':
                break;
            // domain server port
            case 2:
                domainPort = Integer.parseInt(g.getOptarg());
                break;
            case 3:
                keys = g.getOptarg().split(",");
                break;
            case 4:
                domainServer = g.getOptarg();
                break;
            }
        }

        Log.init();

        PluginStatusCheck tagent = new PluginStatusCheck(agentName,
            domainServer, domainPort);
        for (String scriptFileName : scripts)
            tagent.runScript(scriptFileName);

        tagent.agent().setDomainConnectRetries(0);
        tagent.connect();
        List<String> status = tagent.getPluginStatus();
        String statusText;
        int exitCode;
        if (tagent.getMissingResponders() == 0) {
            statusText = "OK " +
                tagent.getExpectedResponders() + " plugins responding";
            exitCode = 0;
        }
        else {
            statusText = "WARN " +
                "missing "+tagent.getMissingResponders()+" out of "+
                tagent.getExpectedResponders() + " plugins";
            exitCode = 1;
        }

        String perfData = "|";
        if (keys != null) {
            for (String key : keys) {
                Object value = tagent.getRollup().get(key);
                if (value != null)
                    perfData += key + "=" + value + " ";
                else
                    perfData += key + "=U ";
            }
        }
        else {
            for (Map.Entry<String,Serializable> ss :
                        tagent.getRollup().entrySet()) {
                perfData += ss.getKey() + "=" + ss.getValue() + " ";
            }
        }

        if (keys == null) {
            for (String ss : status) {
                perfData += ss;
            }
        }

        System.out.println(statusText + perfData);

        System.exit(exitCode);
    }

}

