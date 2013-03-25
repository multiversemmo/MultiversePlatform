#region Using directives

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using SpeexWrapper;

#endregion


namespace Multiverse.Voice
{

    // All voice packets start with 4 bytes:
    //    o 16-bit sequence number, increased by one for each
    //      successive transmission for this voice.
    //    o 8-bit opcode byte
    //    o 8-bit voice number
    //
    // Three of the message types, AllocateCodec, ReallocateCodec
    // and ReconfirmCodec, allocate a voice channel for sounds
    // from a game object.  They contain the 8-byte OID of the
    // game object producing the sound, for a total size of 12
    // bytes.
    //
    // There are really only two state transitions for listener
    // voices: from Unallocated to Allocated, and back again.
    //
    // When a voice is unallocated data packets are ignored until
    // an AllocateCodec, ReallocateCodec, or ReallocateCodec is
    // seen.  All three result in allocation of the channel, which
    // moves it to the Allocated state.
    //
    // When in the Allocated state, opcodeDeallocate returns the
    // listener voice to the unallocated state.
    //
    // The only remaining message is opcodeData, whose representation
    // is the standard 4-byte header followed by the Speex data frame.
    //
    // The client uses the same format opcodeAllocateCodec message to
    // inform the server about a new microphone, and opcodeData to
    // send frames of Speex data, and opcodeDeallocate message to
    // disassociate the microphone.  We may need some additional
    // opcodes to handle voice activity changes, but for now the
    // assumption is that the microphone will either deallocate the
    // channel when the user isn't speaking, or just not send data
    // packets.
    //
    // In the case of the microphone, the "voice number" is the
    // microphone device number.  When allocate and data messages
    // are sent to listeners by the voice server, the voice number
    // is the number that the client should use to represent a 
    // single stream of voice data from one sound source.  The
    // number ranges from 0 .. maxVoiceChannels - 1.
    public enum VoiceOpcode {

        // All voices start out unallocated
        VoiceUnallocated = 0,

        // The authenticate packet must be the first one received
        // by the voice plugin on any new connection from a
        // client.  The voice plugin maintains a map of the
        // IP/port number to the oid of the client, used to
        // validate traffic.  The packet contains the string
        // authentication token, the oid of the player; the oid of
        // the group the player is signing up to; and a bool
        // saying whether voice packets from this connection
        // should be sent back to this connection.  Total size is
        // 25 bytes.
        Authenticate = 1,

        // Allocate a voice.  Apart from the header, the payload
        // is the 8-byte OID of the object emitting the sound.
        // Size is 12 bytes.
        AllocateCodec = 2,

        // This has exactly the same payload as AllocateCodec, but
        // tells the client that it should treat the sound as
        // positional.
        AllocatePositionalCodec = 3,

        // This has exactly the same payload as AllocateCodec, but
        // is used with lossy transports like UDP to send the
        // opcode parameters every second or so.  Size is 12
        // bytes.
        ReconfirmCodec = 4,

        // Deallocate the voice number.  A voice number must be
        // deallocated before it can be reused.  There is no
        // additional data.  This is used both when a client signs
        // off, and when the microphone goes quiet.  A total of 4
        // bytes.
        Deallocate = 5,

        // A data packet, consisting of a 4-byte header followed by the
        // bytes of the data frame.  All data messages _from_ the client
        // supply the microphone number as the voice number.  Since for
        // the time being we support exactly one microphone, the voice
        // numbers in messages from the client are always zero.  Size is 4
        // bytes plus the codec frame playload, typically 28 bytes for
        // 11000bps.
        Data = 6,

        // An aggregated data packet contains a number of data packets.
        // It starts with a standard header, and has an additional byte
        // arg which is the number of data packets contained therein.
        // Each data packet instead an aggregated data packet starts with
        // a 1-byte length.
        AggregatedData = 7,

        // An opcode sent exclusively from the server to the client, and
        // used only to support synchronization between voice bots and
        // test clients.  It contains the standard header plus the oid of
        // the player whose login status has changed.  The voiceNumber is
        // 1 if it's a login, and 0 if it's a logout.
        LoginStatus = 8,

        // An opcode sent exclusively from the client to the server that
        // marks a collection of oids as blacklisted or not blacklisted.
        // The format is a short count of speakers whose blacklist status
        // should change, and for each speaker, a byte which is non-zero
        // if it should be blacklisted or zero if it should no longer be
        // blacklisted.
        ChangeBlacklistStatus = 9,
    }


    ///<summary>
    ///    The various constructors of this class represent the
    ///    message formats that can be sent by the client.
    ///</summary>
    public class VoiceMessage {

        // The logger used throughout the VoiceManager
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(VoiceMessage));

        private BinaryWriter writer;
        private MemoryStream memStream;

        public VoiceMessage(int msgSize, short sequenceNumber, VoiceOpcode opcode, byte voiceNumber) {
            memStream = new MemoryStream();
            writer = new BinaryWriter(memStream);
            writeHeader(msgSize, sequenceNumber, opcode, voiceNumber);
        }

        private void writeHeader(int msgSize, short sequenceNumber, VoiceOpcode opcode, byte voiceNumber) {
            writer.Write(IPAddress.HostToNetworkOrder((short)msgSize));
            writer.Write(IPAddress.HostToNetworkOrder(sequenceNumber));
            writer.Write((byte)opcode);
            writer.Write(voiceNumber);
        }

        public VoiceMessage(int msgSize, short sequenceNumber, VoiceOpcode opcode, byte voiceNumber, long oid) {
            memStream = new MemoryStream();
            writer = new BinaryWriter(memStream);
            writeHeader(msgSize, sequenceNumber, opcode, voiceNumber);
            writer.Write(IPAddress.HostToNetworkOrder(oid));
        }

        public VoiceMessage(int msgSize, short sequenceNumber, VoiceOpcode opcode, byte voiceNumber, 
                            long oid, long groupOid, string authToken, bool listenToYourself) {
            memStream = new MemoryStream();
            writer = new BinaryWriter(memStream);
            writeHeader(msgSize, sequenceNumber, opcode, voiceNumber);
            writer.Write(IPAddress.HostToNetworkOrder(oid));
            writer.Write(IPAddress.HostToNetworkOrder(groupOid));
            byte[] authBytes = Encoding.UTF8.GetBytes(authToken);
			writer.Write(IPAddress.HostToNetworkOrder((int)(authBytes.Length)));
            writer.Write(Encoding.UTF8.GetBytes(authToken));
            writer.Write((byte)(listenToYourself ? 1 : 0));
        }

        public VoiceMessage(short sequenceNumber, List<long> blacklist, List<long> removeFromBlacklist) {
            int count = blacklist.Count + removeFromBlacklist.Count;
            int msgSize = 4 + sizeof(short) + count * (sizeof(byte) + sizeof(long));
            memStream = new MemoryStream();
            writer = new BinaryWriter(memStream);
            writeHeader(msgSize, sequenceNumber, VoiceOpcode.ChangeBlacklistStatus, (byte)0);
            writer.Write((short)count);
            foreach (long oid in blacklist) {
                writer.Write((byte)1);
                writer.Write(oid);
            }
            foreach (long oid in removeFromBlacklist) {
                writer.Write((byte)0);
                writer.Write(oid);
            }
        }

        ///<summary>
        ///    Used by the packet aggregator machinery
        ///</summary>
        public void WriteSubDataFrame(byte[] dataFrame) {
            int len = dataFrame.Length;
            if (len > 255)
                log.ErrorFormat("VoiceMessage.WriteSubDataFrame: dataFrame len is {0}, for frame {1}",
                    len, VoiceManager.ByteArrayToHexString(dataFrame, 0, len));
            writer.Write((byte)len);
            writer.Write(dataFrame, 0, len);
        }

        public void WriteBytes(byte[] buf, int length) {
            writer.Write(buf, 0, length);
        }

        public void WriteByte(byte b) {
            writer.Write((byte)b);
        }

        public byte[] Bytes {
            get {
                return memStream.ToArray();
            }
        }

        public byte BytesLength {
            get {
                return (byte)memStream.Length;
            }
        }

    }

}
