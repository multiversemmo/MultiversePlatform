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

package multiverse.server.plugins;

import multiverse.server.engine.Hook;
import multiverse.server.objects.Player;
import multiverse.msgsys.Message;

/**
 * Interface used by the proxy for processing player-related messages.
 *
 * @see multiverse.server.engine.EnginePlugin#handleMessageImpl
 * @see multiverse.server.engine.HookManager
 */
public interface ProxyHook extends Hook
{
    /**
     * Handles processing of the message, called by EnginePlugin.
     *
     * @param msg The message to process.
     * @param flags The message flags.
     * @param player The player on whose behalf the message is being processed.
     */
    public void processMessage(Message msg, int flags, Player player);
}

