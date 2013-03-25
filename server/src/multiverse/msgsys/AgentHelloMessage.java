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


public class AgentHelloMessage extends Message
{
    public AgentHelloMessage() {
        msgType = MessageTypes.MSG_TYPE_AGENT_HELLO;
    }

    AgentHelloMessage(String agentName, String agentIP, int agentPort) {
        msgType = MessageTypes.MSG_TYPE_AGENT_HELLO;
        this.agentName = agentName;
        this.agentIP = agentIP;
        this.agentPort = agentPort;
    }

    public String getAgentName()
    {
        return agentName;
    }

    public String getAgentIP()
    {
        return agentIP;
    }

    public int getAgentPort()
    {
        return agentPort;
    }

    public int getFlags()
    {
        return flags;
    }

    public void setFlags(int flags)
    {
        this.flags = flags;
    }

    private String agentName;
    private String agentIP;
    private int agentPort;
    private int flags;
    
    private static final long serialVersionUID = 1L;
}

