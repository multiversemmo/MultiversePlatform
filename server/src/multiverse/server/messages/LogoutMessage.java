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

package multiverse.server.messages;

import multiverse.msgsys.*;

/** Sent when player logs out of the world.  The message is sent as a
broadcast RPC, so all subscribers must respond.  The message is
sent after the player has been despawned, but before it has
been unloaded.
*/
public class LogoutMessage extends SubjectMessage
{
    public LogoutMessage()
    {
    }

    public LogoutMessage(long playerOid, String playerName)
    {
        super(MSG_TYPE_LOGOUT,playerOid);
        setPlayerName(playerName);
    }

    /** Get the player name.
    */
    public String getPlayerName()
    {
        return playerName;
    }

    /** Set the player name.
    */
    public void setPlayerName(String name)
    {
        this.playerName = name;
    }

    private String playerName;

    /** LogoutMessage message type.
    */
    public static final MessageType MSG_TYPE_LOGOUT = MessageType.intern("mv.LOGOUT");

    private static final long serialVersionUID = 1L;
}

