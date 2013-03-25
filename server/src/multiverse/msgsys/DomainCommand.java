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

import java.util.Properties;
import gnu.getopt.Getopt;

import multiverse.server.util.InitLogAndPid;


public class DomainCommand
{
    public static void main(String argv[])
    {
        Getopt g = new Getopt("DomainCommand", argv, "n:m:t:P:");

        String[] allocName = null;

        int c;
        while ((c = g.getopt()) != -1) {
            switch(c) {
            // alloc name
            case 'n':
                allocName = g.getOptarg().split(",",2);
                break;
            case 't':
            case 'm':
                /* ignore */
                break;
            case 'P':
                /* ignore */
                break;
            }
        }

        Properties properties = InitLogAndPid.initLogAndPid(argv);

        MessageAgent agent = new MessageAgent();
        String domainHost = properties.getProperty("multiverse.msgsvr_hostname",
            System.getProperty("multiverse.msgsvr_hostname"));
        String portString = properties.getProperty("multiverse.msgsvr_port",
            System.getProperty("multiverse.msgsvr_port"));
        int domainPort = DomainServer.DEFAULT_PORT;
        if (portString != null)
            domainPort = Integer.parseInt(portString);

        try {
            agent.connectToDomain(domainHost, domainPort);
            if (allocName != null) {
                String agentName = agent.getDomainClient().allocName(
                    allocName[0], allocName[1]);
                System.out.println(agentName);
            }
        }
        catch (Exception ex) {
            System.err.println("DomainCommand: "+ex);
            throw new RuntimeException("failed", ex);
        }

    }

}

