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

package multiverse.server.network.rdp;

import java.net.*;
import java.io.*;

/**
 * the connection info, localhost, localport, remotehost, remote port
 */
public class ConnectionInfo implements Serializable {
    ConnectionInfo(InetAddress remoteAddress,
                   int remotePort,
                   int localPort) {
        this.remoteAddress = remoteAddress;
        this.remotePort = remotePort;
        this.localPort = localPort;
    }

    public boolean equals(Object o) {
        ConnectionInfo other = (ConnectionInfo) o;
        return (remoteAddress.equals(other.remoteAddress) &&
                (remotePort == other.remotePort) &&
                (localPort == other.localPort));
    }

    public int hashCode() {
        return (remoteAddress.hashCode() ^ remotePort ^ localPort);
    }

    public String toString() {
        return "[ConnectionInfo: remoteAddress=" + remoteAddress +
            ", remotePort=" + remotePort +
            ", localPort=" + localPort + "]";
    }
    public InetAddress remoteAddress;
    public int remotePort;
    public int localPort;
    private static final long serialVersionUID = 1L;
}
    
