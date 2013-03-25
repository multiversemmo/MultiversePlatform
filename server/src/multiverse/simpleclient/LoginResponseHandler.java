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

package multiverse.simpleclient;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.network.*;

public class LoginResponseHandler implements MessageHandler {
    public LoginResponseHandler() {
    }

    public String getName() {
	return "simpleclient.LoginResponseHandler";
    }

    public Event handleMessage(ClientConnection con, MVByteBuffer buf) {
        // playerOid
        Long oid = buf.getLong();
        
        // msgID
        buf.getInt();
        
        // timestamp
        buf.getLong();
        
        int respStatus = buf.getInt();
        
        String respReason = buf.getString();

        if (Log.loggingDebug)
            Log.debug("LoginResponseHandler: playerOid=" + oid +
                      ", success code=" + respStatus +
                      ", reason=" + respReason);

        if (EXIT_ON_MSG) {
            if (respStatus == 1) {
                System.exit(0);
            }
            else {
                System.exit(1);
            }
        }
        return null;
    }

    /**
     * set this to true if you want the handler to exit the
     * process.  this is useful for running a testclient within
     * the context of a bash shell
     */
    public static boolean EXIT_ON_MSG = false;
    
    static final Logger log = new Logger("LoginResponseHandler");
}
