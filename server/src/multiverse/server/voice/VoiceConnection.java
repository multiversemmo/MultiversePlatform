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

package multiverse.server.voice;

import java.io.*;
import multiverse.server.network.*;
import multiverse.server.util.SecureToken;

/**
 * This class holds voice-specific information about the
 * connection, such as the oid of the speaker, the speaking state,
 * etc.
 */
public class VoiceConnection {

    /**
     * Constructor.
     * @param con The ClientCconnection object representing the 
     * network connection to the player associated with this 
     * VoiceConnection.
     */
    public VoiceConnection(ClientConnection con) {
        this.con = con;
    }

    /**
     * The networking connection to the client with the given oid.
     */
    public ClientConnection con = null;

    /**
     * The oid of the player with which this connection is associated.
     */
    public long playerOid;
    
    /**
     * The GroupMember of the player with which this connection is
     * associated.  This is null until an auth packet is successfully
     * processed.
     */
    public GroupMember groupMember;

    /**
     * The oid of the group to which this player belongs.
     */
    public long groupOid;
    
    /**
     * The VoiceGroup instance to which this player belongs.
     */
    public VoiceGroup group;

    /**
     * The device number of the microphone that this connection will
     * use when it acts as a speaker.
     */
    public byte micVoiceNumber;

    /**
     * The authentication token for this connection.  Note that it
     * must be initialized to null, because that's how the VoicePlugin
     * can tell that we have never received an auth packet for this
     * connection.
     */
    public SecureToken authToken = null;

    /**
     * The most recent sequence number received in a message from this voice connection.
     */
    public short seqNum = 0;

    /**
     * True if the speaker is allowed to listen to himself.  Only useful for
     * testing and debugging.
     */
    public boolean listenToYourself = false;

    /**
     * The output stream being used to write speex frames to disk.  Only useful for
     * testing and debugging.
     */
    public BufferedOutputStream recordSpeexStream = null;

    /**
     * A descriptive string for the VoiceConnection object.
     */
    public String toString() {
        return "oid " + playerOid + " " + con.IPAndPort();
    }
}


