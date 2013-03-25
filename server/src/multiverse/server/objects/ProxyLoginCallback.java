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

package multiverse.server.objects;

import multiverse.server.network.ClientConnection;

/** Methods called during player proxy login.  Implementations are
registered with {@link multiverse.server.plugins.ProxyPlugin#setProxyLoginCallback(multiverse.server.objects.ProxyLoginCallback) ProxyPlugin.setProxyLoginCallback()}.
<p>
The method call order is:<ol>
<li>preLoad()
<li>postLoad()
<li>postSpawn()
</ol>
*/
public interface ProxyLoginCallback
{
    /** Called before player object is loaded.
        @return null if the login should proceed.  Otherwise, an error
        message string if the login should fail.  The
        error message is returned to the client.
    */
    public String preLoad(Player player, ClientConnection con);

    /** Called after player object is loaded, before any communication
        back to the client.
        @return null if the login should proceed.  Otherwise, an error
        message string if the login should fail.  The
        error message is returned to the client.
    */
    public String postLoad(Player player, ClientConnection con);

    /** Called after the initial player spawn.
    */
    public void postSpawn(Player player, ClientConnection con);
}

