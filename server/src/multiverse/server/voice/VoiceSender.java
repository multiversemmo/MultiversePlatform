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

import multiverse.server.network.*;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;

/**
 * This is the interface that the voice groups use to send messages to
 * listeners.  The VoicePlugin is such a VoiceSender, and likely the
 * only VoiceSender in most virtual worlds.
 */
public interface VoiceSender {

    /**
     * Send a message which tells the client that the server has
     * established a voice channel to the client, identified by
     * the given voiceNumber, using an overloading of sendAllocateVoice
     * in which whether the voice channel will be treated positionally
     * is determined by the boolean positional arg.
     * @param speaker The VoiceConnection object that represents
     * the speaker that will be the source of voice data for the
     * new voice channel.
     * @param listener The VoiceConnection object representing 
     * the listener player, whose client will be sent an allocate 
     * message followed by voice data messages.
     * @param voiceNumber The number, from 0 - maxVoiceChannels,
     * of the voice channel to be established.
     * @param positional True if the voice should behave as a positional 
     * voice.
     */
    public void sendAllocateVoice(VoiceConnection speaker, VoiceConnection listener, byte voiceNumber, boolean positional);
    
    /**
     * Send a message which tells the client that the server has
     * established a voice channel to the client, identified by
     * the given voiceNumber, using an overloading of sendAllocateVoice
     * in which the positional boolean is replaced by the appropriate voice 
     * allocate opcode.
     * @param speaker The VoiceConnection object that represents
     * the speaker that will be the source of voice data for the
     * new voice channel.
     * @param listener The VoiceConnection object representing 
     * the listener player, whose client will be sent an allocate 
     * message followed by voice data messages.
     * @param voiceNumber The number, from 0 - maxVoiceChannels,
     * of the voice channel to be established.
     * @param opcode Either VoicePlugin.opcodeAllocatePositional or 
     * VoicePlugin.opcodeAllocateNonpositional.
     */
    public void sendAllocateVoice(VoiceConnection speaker, VoiceConnection listener, byte voiceNumber, byte opcode);

    /**
     * Send a message which tells the client that the server has
     * deallocated the voice channel whose number is voiceNumber.
     * @param speaker The VoiceConnection object that represents
     * the speaker that was the source of voice data for the
     * voice channel.
     * @param listener The VoiceConnection object representing 
     * the listener player, whose client will be sent the deallocate
     * message.
     * @param voiceNumber The number, from 0 - maxVoiceChannels,
     * of the voice channel to be established.
     */
    public void sendDeallocateVoice(VoiceConnection speaker, VoiceConnection listener, byte voiceNumber);
        
    /**
     * Send a voice data message to the client on the voice channel 
     * identitified by the voiceNumber.
     * @param speaker The VoiceConnection object that represents
     * the speaker that was the source of voice data for the
     * voice channel.
     * @param listener The VoiceConnection object representing 
     * the listener player, whose client will be sent the voice data
     * message.
     * @param opcode  Either VoicePlugin.opcodeAggregateData or 
     * VoicePlugin.opcodeData.
     * @param voiceNumber The number, from 0 - maxVoiceChannels,
     * of the voice channel to be established.
     * @param sourceBuf A byte buffer holding the entire voice message as received
     * from the speaker.  The first byte of the message is at
     * index 0 in buf.
     * @param pktLength The number of bytes of message in the buf.
     */
    public void sendVoiceFrame(VoiceConnection speaker, VoiceConnection listener, byte opcode, byte voiceNumber, MVByteBuffer sourceBuf, short pktLength);
    
    /**
     * Send a broadcast ExtensionMessage to anyone who cares.  Used to
     * send "event" messages from the voice groups.
     * @param msg The ExtensionMessage to send.
     */
    public void sendExtensionMessage(ExtensionMessage msg);

}
