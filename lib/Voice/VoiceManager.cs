#region Using directives

using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using log4net;
using Multiverse.Lib.LogUtil;
using Axiom.MathLib;

using SpeexWrapper;

#endregion


namespace Multiverse.Voice
{
    public class VMLogger {
        
        // The logger used throughout the VoiceManager
        private static readonly log4net.ILog log4NetLogger = log4net.LogManager.GetLogger(typeof(VoiceManager));

        private string FormatVM(VoiceManager vm) {
            if (vm == null || !vm.runningVoiceBot)
                return "";
            else if (vm.playerOid == 0)
                return PadTo("", 12);
            else
                return PadTo("oid " + vm.playerOid + " ", 12);
        }
        
        private static string blanks = "                                        ";
        
        private string PadTo(string s, int len) {
            int add = len - s.Length;
            if (add <= 0)
                return s;
            else
                return s + blanks.Substring(0, Math.Min(add, blanks.Length));
        }
        
        public void Info(VoiceManager vm, string what) {
            log4NetLogger.Info(FormatVM(vm) + what);
        }
        
        public void InfoFormat(VoiceManager vm, string format, params Object[] rest) {
            Info(vm, string.Format(format, rest));
        }

        public void Debug(VoiceManager vm, string what) {
            log4NetLogger.Debug(FormatVM(vm) + what);
        }
        
        public void DebugFormat(VoiceManager vm, string format, params Object[] rest) {
            Debug(vm, string.Format(format, rest));
        }

        public void Warn(VoiceManager vm, string what) {
            log4NetLogger.Warn(FormatVM(vm) + what);
        }
        
        public void WarnFormat(VoiceManager vm, string format, params Object[] rest) {
            Warn(vm, string.Format(format, rest));
        }

        public void Error(VoiceManager vm, string what) {
            log4NetLogger.Error(FormatVM(vm) + what);
        }
        
        public void ErrorFormat(VoiceManager vm, string format, params Object[] rest) {
            Error(vm, string.Format(format, rest));
        }
    }
    
    public class VoiceConstants {
        
        public static VMLogger log = new VMLogger();
        
        // This gives the number of bytes in the message after the
        // length, except for the data cases, where it gives the
        // number of bytes in the header, but not including the data
        // itself.  Auth packet is size is 2 + 1 + 1 + 8 + 8 + 1 + 4 =
        // 25 bytes plus number of bytes in the authToken string.
        public static readonly byte[] voiceMsgSize = new byte[] { 0, 25, 12, 12, 12, 4, 4, 5 };
        
        public static readonly int receiveBufferSize = 1024;

        // This array is indexed by Speex narrow-band mode, and gives
        // the narrow-band frame size for that mode.
        public static readonly byte[] speexNarrowBandFrameSize = new byte[] { 1, 6, 15, 20, 28, 38, 46, 62, 10 };
        
        // When sending wide-band data, for each frame Speex first
        // sends the narrow-band encoded frame, and then sends the
        // wide-band "supplement", whose size for each wide-band mode
        // is given below.
        //
        // ??? TBD: When transmitting wide-band frames, is the mode in
        // both the wide and narrow chunks of the frame the same?
        // That would mean that the narrow-band mode is constrained to
        // be 0 - 4
        //
        // ??? TBD: When sending wide-band frames, does the wide-band
        // portion come first or the narrow-band version?  I would
        // hope it's the wide-band version, because otherwise there is
        // no way to tell if a wide-band portion follows the
        // narrow-band one.
        public static readonly int[] speexWideBandFrameSize = new int[] { 1, 5, 14, 24, 44 };

        public static readonly int headerSeqNumOffset = 0;
        public static readonly int headerOpcodeOffset = 2;
        public static readonly int headerVoiceNumberOffset = 3;

        public static readonly int defaultSamplesPerFrame = 160;

        public static readonly int maxSamplesPerFrame = defaultSamplesPerFrame * 8;
        
        ///<summary>
        ///    The maximum size in bytes of an encoded frame
        ///</summary>
        public static readonly int maxBytesPerEncodedFrame = 1024;

        ///<summary>
        ///    The number of incoming voice streams the VoiceManager
        ///    will accept, which is the same as the number of FMOD
        ///    voice channels reserved.
        ///</summary>
        public static readonly int voiceChannelCount = 4;

        ///<summary>
        ///    We need 2 bytes for voice message lengths, because
        ///    all messages are less than 255 bytes in length.
        ///</summary>
        public static readonly int messageLengthByteCount = 2;

        ///<summary>
        ///    The number of bytes per sample: single-channel 16-bit
        ///    PCM.
        ///</summary>
        public static readonly int sampleSize = 2;
        
        ///<summary>
        ///    Maximum number of samples in a voice packet
        ///</summary>
        public static readonly int maxVoicePacketSize = 500;
        
        ///<summary>
        ///    Maximum number of frames in the VoiceChannel queue: 4
        ///</summary>
        public static readonly int playbackQueueFrames = 4;

        ///<summary>
        ///    Maximum number of samples in the VoiceChannel queue
        ///</summary>
        public static readonly int playbackQueueSize = defaultSamplesPerFrame * playbackQueueFrames;

        ///<summary>
        ///    1000 millimeters in a meter
        ///</summary>
        public static readonly float oneMeter = 1000f;

		public static List<VoiceCounter> allCounters = new List<VoiceCounter>();

        public class VoiceCounter {
            public string name;
            public int count = 0;

            public VoiceCounter(string name) {
                this.name = name;
                allCounters.Add(this);
            }

            public void Inc() {
                count++;
            }
            
            public void Inc(int addend) {
                count += addend;
            }
        }
        
        public static VoiceCounter packetsSentCounter = null;
		public static VoiceCounter packetsReceivedCounter = null;
		public static VoiceCounter bytesSentCounter = null;
		public static VoiceCounter bytesReceivedCounter = null;
        public static VoiceCounter silentFrameCounter = null;
        public static VoiceCounter audibleFrameCounter = null;
        public static VoiceCounter aggregatedDataReceivedCounter = null;
        public static VoiceCounter aggregatedDataSentCounter = null;
        public static VoiceCounter allocsSentCounter = null;
        public static VoiceCounter allocsReceivedCounter = null;
        public static VoiceCounter deallocsSentCounter = null;
        public static VoiceCounter deallocsReceivedCounter = null;
        public static VoiceCounter jitterAdjustCounter = null;
        public static VoiceCounter micWAVBytesRecordedCounter = null;
        public static VoiceCounter micSpeexFramesRecordedCounter = null;
        public static VoiceCounter voicesSpeexFramesRecordedCounter = null;

        public static VoiceCounter overrunFrameCounter = null;

        ///<summary>
        ///    Count of times when we want to queue PCM samples but
        ///    the queue is completely full.
        ///</summary>
        public static VoiceCounter queuedFrameCounter = null;

        ///<summary>
        ///    Count of times when we want to queue PCM samples but
        ///    the queue is completely full.
        ///</summary>
        public static VoiceCounter noBytesQueuedFrameCounter = null;

        ///<summary>
        ///    Count of frames encoded
        ///</summary>
        public static VoiceCounter encodedFrameCounter = null;

        ///<summary>
        ///    Count of frames decoded
        ///</summary>
        public static VoiceCounter decodedFrameCounter = null;

        ///<summary>
        ///    Count of frames decoded
        ///</summary>
        public static VoiceCounter voicePlaybackFrameCounter = null;

        ///<summary>
        ///    The timer that logs counters
        ///</summary>
        private static System.Timers.Timer counterLoggingTimer = null;

        public VoiceConstants() {
            if (allCounters.Count == 0) {
                InitializeCounters();
                counterLoggingTimer = new System.Timers.Timer();
                counterLoggingTimer.Elapsed += CounterLoggingTimerTick;
                counterLoggingTimer.Interval = 5000; // ms
                counterLoggingTimer.Enabled = true;
            }
        }
        
        private void CounterLoggingTimerTick(object sender, System.EventArgs e) {
            string s = "";
            int i = 1;
            foreach (VoiceCounter ctr in allCounters) {
                if (ctr.count == 0)
                    continue;
                if (s != "")
                    s += ", ";
                s += ctr.name + " " + ctr.count;
                if (s.Length > 80) {
                    log.Debug(null, "Channel counters" + i + ": " + s);
                    i++;
                    s = "";
                }
            }
            if (s != "")
                log.Debug(null, "Channel counters" + i + ": " + s);
        }


        private void InitializeCounters () {
            packetsSentCounter = new VoiceCounter("pkts sent");
            bytesSentCounter = new VoiceCounter("bytes sent");
            encodedFrameCounter = new VoiceCounter("encoded");
            packetsReceivedCounter = new VoiceCounter("pkts rcved");
            bytesReceivedCounter = new VoiceCounter("bytes rcved");
            decodedFrameCounter = new VoiceCounter("decoded");
            silentFrameCounter = new VoiceCounter("silent frames");
            audibleFrameCounter = new VoiceCounter("audible frames");
            aggregatedDataReceivedCounter = new VoiceCounter("agg data pkts rcved");
            aggregatedDataSentCounter = new VoiceCounter("agg data pkts sent");
            allocsSentCounter = new VoiceCounter("allocs sent");
            deallocsSentCounter = new VoiceCounter("deallocs sent");
            allocsReceivedCounter = new VoiceCounter("allocs received");
            deallocsReceivedCounter = new VoiceCounter("deallocs received");
            jitterAdjustCounter = new VoiceCounter("jitter adjust");

            overrunFrameCounter = new VoiceCounter("overruns");
            queuedFrameCounter = new VoiceCounter("queued");
            noBytesQueuedFrameCounter = new VoiceCounter("no-bytes queued");
            voicePlaybackFrameCounter = new VoiceCounter("voice playback");
            micWAVBytesRecordedCounter = new VoiceCounter("mic WAV bytes rec");
            micSpeexFramesRecordedCounter = new VoiceCounter("mic Speex bytes rec");
            voicesSpeexFramesRecordedCounter = new VoiceCounter("voice Speex bytes rec");
        }
        
    }

    ///<summary>
    ///    This class takes PCM samples from the microphone(s), encodes
    ///    them using the Speex codec, and sends them to the voice
    ///    server.  Simultaneously, it accepts multiple stream of
    ///    Speex-encoded frames over the single connection to the
    ///    voice server, converts them to PCM by invoking the Speex
    ///    decoder, and hands them off to FMOD.
    ///
    ///    PCM frames are represented as sequences of ushorts; Speex
    ///    frames are represented as sequences of bytes.
    ///</summary>
    public class VoiceManager : VoiceConstants, IDisposable {
        
        private TcpClient tcpClient = null;
        private List<int> tcpMonitoredObject = new List<int>();
        private UdpClient udpClient = null;

        ///<summary>
        ///    If true, open a TCP connection to the voice server; if
        ///    false, open a UDP connection.
        ///</summary>
        protected bool useTcp = true;

        ///<summary>
        ///    How long we wait before cancelling the connect attempt.
        ///</summary>
        protected int tcpConnectTimeout = 5000;

        // The count of valid bytes in the buffer.  To get the offset
        // where we add bytes, add this to receiveBufferOffset
        private int bytesInReceiveBuffer = 0;
        // The offset at which we will _read_ the next bytes
        private int receiveBufferOffset = 0;
        private byte[] receiveBuffer;
        private byte[] currentMsgBuf;
        private int currentMsgOffset = 0;

        ///<summary>
        ///    Only set to true for extreme module testing
        ///</summary>
        public bool loggingMicTick = false;
        public bool loggingMessages = true;
        public bool loggingDecode = false;

        // ??? TBD: Must get out of the login response message in the
        // real client.  For now, just a plug value
        private string authToken = "-4130";

        private bool micRecordWAV = false;
        private bool micRecordSpeex = false;
        private bool voicesRecordSpeex = false;

        ///<summary>
        ///    The device number of the "microphone" device
        ///</summary>
        public int micDeviceNumber = 0;

        ///<summary>
        ///    The device number of the playback device
        ///</summary>
        public int playbackDeviceNumber = 0;

        ///<summary>
        ///    The default playback volume for newly-created voice
        ///    channels
        ///</summary>
        public float defaultPlaybackVolume = 1.0f;

        protected bool voiceManagerShutdown = false;
        
        private List<long> recordedOids = new List<long>();
        
        ///<summary>
        ///    A list of the speakers who have been "blacklisted" by
        ///    the client, which means that we won't play their voice
        ///    frames.
        ///</summary>
        private List<long> blacklist = new List<long>();
        
        ///<summary>
        ///    The FMOD instance
        ///</summary>
        public FMOD.System fmod = null;

        ///<summary>
        ///    Lock this to access the fmod object, or any codec.
        ///</summary>
        public Object usingFmod = new List<int>();
        
        ///<summary>
        //    By default, 50 frames == one second of silence before we
        //    deallocate a voice.  Changed using silence_dealloc_time
        //    parm.
        ///</summary>
        public int silentFrameCountDeallocationThreshold = 50;

        ///<summary>
        ///    The Oid of the player
        ///</summary>
        public long playerOid = 0;
        
        ///<summary>
        ///    The listener properties for the player: position,
        ///    velocity, direction, and up.
        ///</summary>
        public ListenerProperties playerListenerProperties = null;
        
        ///<summary>
        ///    The group Oid of the group the player is participating in
        ///</summary>
        public long groupOid = 1;
        
        ///<summary>
        ///    Is this VoiceManager connected to the voice server?
        ///</summary>
        protected bool connectedToServer = false;
        
        ///<summary>
        ///    The host name of the voice server
        ///</summary>
        protected string voiceServerHost;

        ///<summary>
        ///    The port number of the voice server
        ///</summary>
        protected int voiceServerPort;

        ///<summary>
        ///    Should we connect to the server?
        ///</summary>
        protected bool connectToServer = true;
        
        ///<summary>
        ///    A PCM-coded frame of silence
        ///</summary>
        public short[] noSoundShorts = null;
        
        ///<summary>
        ///    The array of VoiceChannel objects
        ///</summary>
        protected Dictionary<Byte, VoiceChannel> voiceChannels = null;
        
        ///<summary>
        ///    The server is supposed to run things so we never have
        ///    more than this number of voice channels.
        ///</summary>
        protected int maxVoiceChannels = 4;
        
        ///<summary>
        ///    Provides a way to look up a voice channel by oid,
        ///    needed in order to deal with positional sounds.
        ///</summary>
        protected Dictionary<long, VoiceChannel> oidToVoiceChannel = new Dictionary<long, VoiceChannel>();
        
        ///<summary>
        ///    The array of MicChannel objects
        ///</summary>
        protected MicrophoneChannel[] micChannels = null;

        ///<summary>
        ///    True if the voice stream from your microphone should be
        ///    sent back to you - - used only for testing.
        ///</summary>
        protected bool listenToYourself = false;

        ///<summary>
        ///    The parameter instance that holds current values of
        ///    parameters
        ///</summary>
        protected VoiceParmSet currentVoiceParms;

        ///<summary>
        ///    Needed to tell where to put the recorded WAV and Speex files
        ///</summary>
        protected string logFolder;
        
        ///<summary>
        ///    A sorted list, ordered by how recently they have
        ///    spoken; i.e., the most recent one is at the front of
        ///    the list.
        ///</summary>
        public List<VoiceChannel> recentSpeakers;
        
        ///<summary>
        ///    The maximum number of elts in the sorted list.
        ///</summary>
        public int maxRecentSpeakers;

        ///<summary>
        ///    A callback delegate to let the caller know that an
        ///    attempt to contact the voice server has completed;
        ///    success is true if the connect succeeded; false if failed.
        ///</summary>
        public delegate void ConnectToServerEvent(long oid, bool success);
        
        ///<summary>
        ///    An event for connecting to the server
        ///</summary>
        public event ConnectToServerEvent onConnectedToServer = null;
        
        ///<summary>
        ///    A bool saying whether we're attempting a connection,
        ///    examined in Tick().  If true, we call the connection
        ///    callback. 
        ///</summary>
        protected bool connectionAttempted = false;

        ///<summary>
        ///    A bool saying whether the attempted connect was successful.
        ///</summary>
        protected bool connectionSuccessful;
        
        ///<summary>
        ///    A callback delegate for allocation of voice streams.
        ///</summary>
        public delegate void VoiceAllocationEvent(long oid, byte voiceNumber, bool positional);

        ///<summary>
        ///    A callback delegate for deallocation of voice streams.
        ///    
        ///    Currently unused
        ///</summary>
        public delegate void VoiceDeallocationEvent(long oid);

        ///<summary>
        ///    An event for allocation of voice streams.
        ///</summary>
        public event VoiceAllocationEvent onVoiceAllocation = null;

        ///<summary>
        ///    An event for deallocation of voice streams.
        ///</summary>
        public event VoiceDeallocationEvent onVoiceDeallocation = null;

        ///<summary>
        ///    A callback delegate for login status events
        ///</summary>
        public delegate void LoginStatusEvent(long oid, bool login);

        ///<summary>
        ///    An event for reception of login status messages
        ///</summary>
        public event LoginStatusEvent onLoginStatusReceived = null;
        
        ///<summary>
        ///    True if this voice manager is running as a voice bot.
        ///</summary>
        public bool runningVoiceBot = false;
        
        ///<summary>
        ///    If we're in playback mode, this is an array of
        ///   .speex files, and we toss a random number to pick
        ///   from them.
        ///</summary>
        public string[] playbackFiles = null;

        ///<summary>
        ///    The min range of a positional voice.
        ///</summary>
        public float minAttenuation = 20.0f;
        
        ///<summary>
        ///    The max range of a positional voice.
        ///</summary>
        public float maxAttenuation = 1000.0f;
        
        ///<summary>
        ///    The attenuation rolloff of a positional voice.
        ///</summary>
        public float attenuationRolloff = 20.0f;
        
        
        ///<summary>
        ///    If we're acting as a bot, this array records whether we
        ///    believe a particular voice number has been alloced
        ///</summary>
        protected bool[] botVoiceChannelInUse = new bool[voiceChannelCount];

        ///<summary>
        ///    Set when we dispose the voice manager; clients test it
        ///    before accessing the voice manager.
        ///</summary>
        protected bool disposed = false;
        
        ///<summary>
        ///    Records the parmArray passed into the constructor, so
        ///    we can keep old values when reconfiguration requires
        ///    creation of a new VoiceManager instance.
        ///</summary>
        protected Object[] parameters = null;
        
        //
        // The external API used by the client consists of these
        // methods:
        //
        // - Configure(), used to initially set up a connection or
        //   change it after it has been established.
        //
        // - Reconfigure(), used to initially set up a connection or
        //   change it after it has been established.
        //
        // - Instance, a property that returns the VoiceManger
        //   instance. 
        //
        // - PushToTalk(), which switches on or off transmission of
        //   voice from the client to the voice server.
        //
        // - BlacklistSpeaker(), which adds or removes a speaker from
        //   the blacklist of speakers the client doesn't want to hear.
        //
        // - SpeakerBlacklisted(), return true if the speaker is
        //   blacklisted, false otherwise. 
        //
        // - ChangeBlacklistedSpeakers(), which takes a list of
        //   speakers to be added to blacklist and a list of speakers to
        //   be removes from the blacklist.
        //
        // - GetBlacklistedSpeakers(), taking no args and returning
        //   a list of the oids of blacklisted speakers.
        //
        // - RecentSpeakers(), returning a list of speakers who have
        //   spoken recently, together with when they last spoke.
        //
        // - NowSpeaking(), returning true if voice frames from the
        //   user represented by the speakerOid arg are currently
        //   being sent to the client.  Used to animate a graphic to
        //   show that someone is  speaking and you should hear him.
        //
        // - GetAllMicrophoneDevices(), returning an ordered list of
        //   sound sources.  The caller figures out which he wants and
        //   passes the index to InitMicrophone().
        //
        // - GetMicNumber(), returning the device number of the
        //   microphone.
        //
        // - GetMicVolume(), returning the mic volume level for the selected
        //   microphone.
        //
        // - GetAllPlayerDevices(), returning an ordered list of
        //   sound players.  The caller figures out which he wants and
        //   passes the index to InitMicrophone().
        //
        // - GetPlaybackNumber(), return the number of the playback
        //   device 
        //
        // - GetDefaultVolume(), returning the default playback volume
        //   for newly-created voice channels.
        //
        // - GetPlaybackVolume(), returning the current volume setting
        //   for the incoming voice channel with the given oid.
        //
        // - SetPlaybackVolume(), setting the playback volume 
        //   for the incoming voice channel with the given oid.
        //
        // - SetPlaybackVolumeForAllSpeakers(), setting the playback volume 
        //   for all voice channels, and also the default playback
        //   volume
        // - ConnectedToVoiceServer(), returning true if the client is
        //   connected to the voice server and false otherwise.

        ///<summary>
        ///    This is the external entrypoint to reconfigure the 
        ///    VoiceManager instance, doing so by disposing of the 
        ///    previous version, if any, and then creating the new
        ///    version.  This will disconnect from the voice server
        ///    while disposing the old one, and reconnect when 
        ///    creating the new one.
        ///</summary>
        public static VoiceManager Configure(FMOD.System fmod, ConnectToServerEvent connectEvent,
                                     params Object[] parmArray) {
            return new VoiceManager(fmod, parmArray, connectEvent, null, false, null);
        }
        
        public static VoiceManager Configure(string args, ConnectToServerEvent connectEvent) {
            String[] splitArgs = args.Trim().Split(new char[] { ' ' } );
            return Configure(null, connectEvent, splitArgs);
        }
        
        ///<summary>
        ///    Create a "bot", which createa a voice server
        ///    connection, 
        ///</summary>
        public static VoiceManager CreateBot(string[] playbackFiles, ConnectToServerEvent connectEvent, params Object[] parmArray) {
            VoiceManager vm = new VoiceManager(null, parmArray, connectEvent, null, true, playbackFiles);
            return vm;
        }
        
        ///<summary>
        ///    Create a "bot", which createa a voice server
        ///    connection, 
        ///</summary>
        public static VoiceManager CreateLoginStatusListener(ConnectToServerEvent connectEvent, LoginStatusEvent loginStatusEvent, params Object[] parmArray) {
            VoiceManager vm = new VoiceManager(null, parmArray, connectEvent, loginStatusEvent, true, null);
            return vm;
        }
        
        ///<summary>
        ///    Returns true if the parms represented by the args string
        ///    requires recreating the VoiceManager instance.
        ///</summary>
        public static bool ConfigRequiresRestart(VoiceManager voiceMgr, string args) {
            String[] splitArgs = args.Trim().Split(new char[] { ' ' } );
            Object[] parameters = GetNewParameters(voiceMgr, splitArgs);
            return ConfigRequiresRestart(voiceMgr, parameters);
        }

        public static bool ConfigRequiresRestart(VoiceManager voiceMgr, params Object[] parameters) {
            VoiceParmSet newVoiceParms = new VoiceParmSet(parameters);
            List<VoiceParm> constructorParms = newVoiceParms.GetParmsOfKindOrDefault(VoiceParmKind.Constructor, true);
            return constructorParms.Count > 0;
        }
        
        public static VoiceManager Reconfigure(VoiceManager voiceMgr, string args, ConnectToServerEvent connectEvent) {
            String[] splitArgs = args.Trim().Split(new char[] { ' ' } );
            Object[] parameters = GetNewParameters(voiceMgr, splitArgs);
            if (voiceMgr != null && !ConfigRequiresRestart(voiceMgr, parameters)) {
                voiceMgr.ApplyReconfiguration(new VoiceParmSet(parameters));
                return voiceMgr;
            }
            else {
                Object[] mergedParameters = MergeParameters(voiceMgr, parameters);
                bool runningVoiceBot = false;
                if (voiceMgr != null) {
                    runningVoiceBot = voiceMgr.runningVoiceBot;
                    voiceMgr.Dispose();
                }
                return new VoiceManager(null, mergedParameters, connectEvent, null, runningVoiceBot, null);
            }
        }
        
        protected void ApplyReconfiguration(VoiceParmSet newVoiceParms) {
            bool setGroupOid = ApplyGroupParms(newVoiceParms, true);
            micChannels[0].ApplyMicrophoneSettings(newVoiceParms, true);
            ApplyPlaybackSettings(newVoiceParms, true);
            foreach (VoiceChannel voiceChannel in voiceChannels.Values)
                voiceChannel.ApplyVoiceChannelParms(newVoiceParms, true);
            currentVoiceParms = newVoiceParms;
            if (setGroupOid)
                SendAuthenticateMessage(micChannels[0].sequenceNumber, 0, playerOid, groupOid, authToken, listenToYourself);
        }
    
        /**
         * Returns true if the groupOid was set to a different value
         */
        protected bool ApplyGroupParms(VoiceParmSet voiceParms, bool reconfigure) {
            bool setGroupOid = false;
            List<VoiceParm> groupParms = voiceParms.GetParmsOfKindOrDefault(VoiceParmKind.Group, reconfigure);
            if (groupParms != null) {
                foreach (VoiceParm parm in groupParms) {
                    switch ((GroupParm)(parm.ctlIndex)) {
                    case GroupParm.GroupOid:
                        setGroupOid = groupOid != parm.ret.lValue;
                        groupOid = parm.ret.lValue;
                        break;
                    default:
                        log.Error(this, "VoiceManager.ApplyGroupParms: unknown GroupParm " + parm.ctlIndex);
                        break;
                    }
                }
            }
            return setGroupOid;
        }
        
        ///<summary>
        ///    Set the microphone status - - is it allowed to transmit
        ///    right now?
        ///</summary>
        public void PushToTalk(bool talk) {
            MicrophoneChannel mic = GetMicrophoneChannel(0);
            if (mic != null) {
                log.DebugFormat(this, "VoiceManager.PushToTalk: Setting talk to " + talk);
                mic.Transmitting = talk;
            }
            else
                log.ErrorFormat(this, "VoiceManager.PushToTalk: When setting talk to {0}, mic is null!", talk);
        }

        public string[] GetAllMicrophoneDevices() {
            return playerListenerProperties.GetAllMicrophoneDevices(fmod);
        }
        
        public string[] GetAllPlaybackDevices() {
            return playerListenerProperties.GetAllPlaybackDevices(fmod);
        }

        ///<summary>
        ///    Used to play back recorded sounds
        ///</summary>
        public VoiceChannel AddVoiceChannel(long oid, byte voiceNumber, bool positional, bool recordSpeex) {
            return AddVoiceChannelInternal(oid, voiceNumber, positional, recordSpeex);
        }
        
        ///<summary>
        ///    Return the microphone given by the micNumber - - most
        ///   of the time, there is only one.
        ///</summary>
        public MicrophoneChannel GetMicrophoneChannel(int micNumber) {
            return GetMicrophoneChannelInternal(micNumber);
        }

        ///<summary>
        ///    Return the device number of the microphone
        ///</summary>
        public int GetMicNumber() {
            return micDeviceNumber;
        }

        ///<summary>
        ///    Return the device number of the playback device
        ///</summary>
        public int GetPlaybackNumber() {
            return playbackDeviceNumber;
        }

        ///<summary>
        ///    Return the default playback volume
        ///</summary>
        public float GetDefaultVolume() {
            return defaultPlaybackVolume;
        }

        ///<summary>
        ///    Return the device number of the microphone.
        ///</summary>
        public int GetMicVolume() {
            MicrophoneChannel mic = GetMicrophoneChannelInternal(micDeviceNumber);
            if (mic == null) {
                log.ErrorFormat(this, "VoiceManager.GetMicVolume: There is no mic for micDeviceNumber {0}", micDeviceNumber);
                return 0;
            }
            else
                return mic.MicLevel;
        }

        ///<summary>
        ///    Blacklist a speaker, or remove the speaker from the
        ///    blacklist.  
        ///</summary>
        public void BlacklistSpeaker(long speakerOid, bool blacklistHim) {
            BlacklistSpeakerInternal(speakerOid, blacklistHim);
            SendSingleBlacklistChange(speakerOid, blacklistHim);
        }

        ///<summary>
        ///    Add and/or remove from the set of blacklisted speakers
        ///</summary>
        public void ChangeBlacklistedSpeakers(List<long> addToBlacklist, List<long> removeFromBlacklist) {
            foreach (long speakerOid in addToBlacklist)
                BlacklistSpeakerInternal(speakerOid, true);
            foreach (long speakerOid in removeFromBlacklist)
                BlacklistSpeakerInternal(speakerOid, false);
            if (connectedToServer) {
                micChannels[0].IncSeqNum();
                SendChangeBlacklistMessage(micChannels[0].sequenceNumber, addToBlacklist, removeFromBlacklist);
            }
        }

        ///<summary>
        ///   Return true if the speaker is blacklisted, false
        ///   otherwise.
        ///</summary>
        public bool SpeakerBlacklisted(long speakerOid) {
            return blacklist.Contains(speakerOid);
        }
        
        ///<summary>
        ///   Return a list of the oids of blacklisted speakers
        ///</summary>
        public List<long> GetBlacklistedSpeakers() {
            return new List<long>(blacklist);
        }
        
        ///<summary>
        ///   Sets the microphone level.  Returns the null string if
        ///   everything was good; an error message otherwise.
        ///</summary>
        public string SetMicLevel(int micNumber, int level) {
            MicrophoneChannel mic = GetMicrophoneChannelInternal(micNumber);
            log.DebugFormat(this, "VoiceManager.SetMicLevel: micNubmer {0}, level {1}", micNumber, level);
            if (mic == null) {
                string s = "VoiceManager.SetMicLevel: There is no microphone for micNumber " + micNumber;
                log.Error(this, s);
                return s;
            }
            mic.SetMicLevel(level);
            return "";
        }

        ///<summary>
        ///   Sets the default playback level, and also the playback
        ///   level for all active voice channels.
        ///</summary>
        public void SetPlaybackVolumeForAllSpeakers(float level) {
            defaultPlaybackVolume = level;
            foreach (VoiceChannel channel in voiceChannels.Values)
                channel.SetPlaybackVolume(level);
        }
        
        ///<summary>
        ///    Return the mapping from speaker oid to the time since
        ///    that speaker last spoke, in milliseconds.  Used to support
        ///    the client display of speakers
        ///</summary>
        public Dictionary<long, long> RecentSpeakers() {
            Dictionary<long, long> speakers = new Dictionary<long, long>(recentSpeakers.Count);
            long now = System.Environment.TickCount; 
            foreach (VoiceChannel channel in recentSpeakers)
                speakers[channel.oid] = now - channel.lastSpoke;
            return speakers;
        }

        ///<summary>
        ///    Return true if voice frames from the user represented
        ///    by speakerOid are currently being sent to the client.
        ///    Used to animate a graphic to show that someone is
        ///    speaking and you should hear him.
        ///</summary>
        public bool NowSpeaking(long speakerOid) {
            return oidToVoiceChannel.ContainsKey(speakerOid);
        }
        
        ///<summary>
        ///    Return the current volume setting for the incoming
        ///    voice channel with the given oid, returning a float
        ///    from 0.0 (silent) to 1.0 (full volume). 
        ///</summary>
        public float GetPlaybackVolume(long speakerOid) {
            VoiceChannel channel;
            lock(usingFmod) {
                if (oidToVoiceChannel.TryGetValue(speakerOid, out channel) && channel != null)
                    return channel.GetPlaybackVolume();
                else {
                    log.ErrorFormat(this, "VoiceManager.GetPlaybackVolume: Could not find channel with oid }0}", speakerOid);
                    return 1.0f;
                }
            }
        }

        ///<summary>
        ///    Set the playback volume for the incoming voice channel
        ///    with the given oid to the float value, from 0.0
        ///    (silent) to 1.0 (full volume).  
        ///</summary>
        public void SetPlaybackVolume(long speakerOid, float level) {
            VoiceChannel channel;
            log.DebugFormat(this, "VoiceManager.SetPlaybackVolume: speakerOid {0}, level {1}", speakerOid, level);
            lock(usingFmod) {
                if (oidToVoiceChannel.TryGetValue(speakerOid, out channel) && channel != null)
                    if (channel != null)
                        channel.SetPlaybackVolume(level);
                else
                    log.ErrorFormat(this, "VoiceManager.SetPlaybackVolume: Could not find channel with oid }0}", speakerOid);
            }
        }

        ///<summary>
        ///    Returns true if the client is connected to the voice
        ///    server and false otherwise. 
        ///</summary>
        public bool ConnectedToVoiceServer() {
            return connectedToServer;
        }

        ///<summary>
        ///    The constructor creates the channels, and the null
        ///    sound, but doesn't make any of the channels active.
        ///
        ///    It should take name/value pair command-line args in an
        ///    string[] container, like main args
        ///</summary>
        private VoiceManager(FMOD.System fmod, Object[] parmArray, ConnectToServerEvent connectEvent, LoginStatusEvent loginStatusEvent, bool runningVoiceBot, string[] playbackFiles) {
            log.Info(null, "VoiceManager constructor: " + StringifyParms(parmArray));
            if (connectEvent != null)
                onConnectedToServer += connectEvent;
            if (loginStatusEvent != null)
                onLoginStatusReceived += loginStatusEvent;
            this.parameters = parmArray;
            this.runningVoiceBot = runningVoiceBot;
            this.playbackFiles = playbackFiles;
            currentVoiceParms = new VoiceParmSet(parmArray);
            string MyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string ClientAppDataFolder = Path.Combine(MyDocumentsFolder, "Multiverse World Browser");
            logFolder = Path.Combine(ClientAppDataFolder, "Logs");
            log.Debug(null, "VoiceManager constructor: Applying parms");
            playerListenerProperties = new ListenerProperties();
            playerListenerProperties.Init();
            ApplyConstructorParms(currentVoiceParms);
            ApplyPlaybackSettings(currentVoiceParms, false);
            log.Debug(this, "VoiceManager constructor: Encaching sound sources");
            voiceChannels = new Dictionary<Byte, VoiceChannel>();
            micChannels = new MicrophoneChannel[1];
            if (!runningVoiceBot) {
                if (fmod != null) {
                    log.Debug(this, "VoiceManager constructor: Using fmod instance passed in: " + fmod);
                    this.fmod = fmod;
                }
                else {
                    log.Debug(this, "VoiceManager constructor: Creating new fmod instance");
                    InitFmod();
                }
            }
            log.Debug(this, "VoiceManager constructor: Creating mic channel");
            micChannels[0] = new MicrophoneChannel(this, playerOid, micDeviceNumber, micRecordWAV, micRecordSpeex);
            recentSpeakers = new List<VoiceChannel>();
            noSoundShorts = new short[defaultSamplesPerFrame * 8];
            MaybeDeleteRecordedFile(micRecordWAV, LogFolderPath("RecordMic.wav"));
            MaybeDeleteRecordedFile(micRecordSpeex, LogFolderPath("RecordMic.speex"));
            ApplyGroupParms(currentVoiceParms, false);
            if (runningVoiceBot)
                log.DebugFormat(this, "VoiceManager constructor: Running voice bot oid {0}", playerOid);
            if (connectToServer) {
                log.Debug(this, "VoiceManager constructor: Initializing connection to server at " +
                    voiceServerHost + ", port " + voiceServerPort);
                receiveBuffer = new byte[receiveBufferSize];
                connectedToServer = true;
                InitializeVoiceServerConnection();
            }
            else
                micChannels[0].InitMicrophone(currentVoiceParms, false);
        }
        
        ///<summar>
        ///    This is called every frame
        ///</summary>
        public void Tick() {
            if (disposed)
                return;
            if (connectionAttempted) {
                connectionAttempted = false;
                AfterConnectToVoiceServer(connectionSuccessful);
            }
            if (!runningVoiceBot) {
                lock (usingFmod)
                    playerListenerProperties.Update(fmod);
            }
        }
                
        protected void AfterConnectToVoiceServer(bool success) {
            if (success) {
                if (useTcp)
                    WaitForTcpData();
                // Now send the authentication message
                MicrophoneChannel micChannel = GetMicrophoneChannelInternal(0);
                micChannel.IncSeqNum();
                SendAuthenticateMessage(micChannel.sequenceNumber, 0, playerOid, groupOid, authToken, listenToYourself);
                if (blacklist.Count > 0) {
                    micChannel.IncSeqNum();
                    SendChangeBlacklistMessage(micChannel.sequenceNumber, blacklist, new List<long>());
                }
                micChannels[0].InitMicrophone(currentVoiceParms, false);
            }
            if (onConnectedToServer != null) {
                log.Debug(this, "VoiceManager.AfterConnectToVoiceServer: invoking the onConnectedToServer event with success = " + success);
                onConnectedToServer(playerOid, success);
            }
        }
        
        protected static bool FindParmNamed(string name, Object[] parameters) {
            for(int i=0; i<parameters.Length; i+=2) {
                string parmName = (string)parameters[i];
                if (parmName == name)
                    return true;
            }
            return false;
        }
        
        ///<summary>
        ///    Return true if the value associated with the name in
        ///    the parameters is different from the value arg, or if
        ///    the parameters doesn't contain the name parameter.
        ///</summary>
        protected static bool NewParmValue(string name, string value, Object[] parameters) {
            for(int i=0; i<parameters.Length; i+=2) {
                string parmName = (string)parameters[i];
                string parmValue = (string)parameters[i + 1];
                if (parmName == name)
                    return parmValue != value;
            }
            return true;
        }
        
        ///<summary>
        ///    Return the subset of the newParms parameters whose
        ///    values are different than those in the voiceMgr
        ///    instance. 
        ///</summary>
        protected static Object[] GetNewParameters(VoiceManager voiceMgr, Object[] newParms) {
            if (voiceMgr == null)
                return newParms;
            List<Object> parmsList = new List<Object>();
            Object [] oldParms = voiceMgr.parameters;
            for(int i=0; i<newParms.Length; i+=2) {
                string parmName = (string)newParms[i];
                string parmValue = (string)newParms[i + 1];
                if (NewParmValue(parmName, parmValue, oldParms)) {
                    parmsList.Add(parmName);
                    parmsList.Add(parmValue);
                }
            }
            Object[] returnedParms = new Object[parmsList.Count];
            for (int i = 0; i < parmsList.Count; i++)
                returnedParms[i] = parmsList[i];
            return returnedParms;
        }
        
        protected static Object[] MergeParameters(VoiceManager voiceMgr, Object[] newParms) {
            if (voiceMgr == null)
                return newParms;
            List<Object> parmsList = new List<Object>();
            Object [] oldParms = voiceMgr.parameters;
            for(int i=0; i<oldParms.Length; i+=2) {
                string parmName = (string)oldParms[i];
                if (!FindParmNamed(parmName, newParms)) {
                    parmsList.Add(parmName);
                    parmsList.Add(oldParms[i+1]);
                }
            }
            for(int i=0; i<newParms.Length; i+=2) {
                parmsList.Add(newParms[i]);
                parmsList.Add(newParms[i+1]);
            }
            Object[] returnedParms = new Object[parmsList.Count];
            for(int i=0; i<parmsList.Count; i++)
                returnedParms[i] = parmsList[i];
            return returnedParms;
        }
        
        protected string StringifyParms(Object[] args) {
            int len = args.Length;
            if ((len & 1) != 0) {
                log.Error(this, "VoiceManager.StringifyParms: Odd number of parms in " + args);
                len--;
            }
            string s = "";
            for (int i=0; i<len; i+=2) {
                if (s != "")
                    s += ", ";
                s += args[i].ToString() + " " + args[i+1];
            }
            return s;
        }
        
        protected void ApplyConstructorParms(VoiceParmSet parmSet) {
            foreach (VoiceParm parm in parmSet.GetParmsOfKindOrDefault(VoiceParmKind.Constructor, false)) {
                log.DebugFormat(this, "VoiceManager.ApplyConstructorParms: setting constructor parm {0} to {1}", parm.name, parm.value);
                switch ((ConstructorParm)(parm.ctlIndex)) {
                case ConstructorParm.MicDeviceNumber:
                    micDeviceNumber = parm.ret.iValue;
                    break;
                case ConstructorParm.UseTcp:
                    useTcp = parm.ret.bValue;
                    break;
                case ConstructorParm.MicRecordWAV:
                    micRecordWAV = parm.ret.bValue;
                    break;
                case ConstructorParm.MicRecordSpeex:
                    micRecordSpeex = parm.ret.bValue;
                    break;
                case ConstructorParm.VoicesRecordSpeex:
                    voicesRecordSpeex = parm.ret.bValue;
                    break;
                case ConstructorParm.ListenToYourself:
                    listenToYourself = parm.ret.bValue;
                    break;
                case ConstructorParm.VoiceServerHost:
                    voiceServerHost = parm.ret.sValue;
                    break;
                case ConstructorParm.VoiceServerPort:
                    voiceServerPort = parm.ret.iValue;
                    break;
                case ConstructorParm.PlayerOid:
                    playerOid = parm.ret.lValue;
                    break;
                case ConstructorParm.AuthenticationToken:
                    authToken = parm.ret.sValue;
                    break;
                case ConstructorParm.ConnectToServer:
                    connectToServer = parm.ret.bValue;
                    break;
                case ConstructorParm.MaxRecentSpeakers:
                    maxRecentSpeakers = parm.ret.iValue;
                    break;
                case ConstructorParm.TcpConnectTimeout:
                    tcpConnectTimeout = parm.ret.iValue;
                    break;
                case ConstructorParm.PlaybackDeviceNumber:
                    playbackDeviceNumber = parm.ret.iValue;
                    break;
                case ConstructorParm.MinAttenuation:
                    minAttenuation = parm.ret.fValue;
                    break;
                case ConstructorParm.MaxAttenuation:
                    maxAttenuation = parm.ret.fValue;
                    break;
                case ConstructorParm.AttenuationRolloff:
                    attenuationRolloff = parm.ret.fValue;
                    break;
                default:
                    log.ErrorFormat(this, "VoiceManager.ApplyConstructorParms: Unknown voice parm '{0}', index {1}", parm.name, parm.ctlIndex);
                    break;
                }
            }
        }

        protected void ApplyPlaybackSettings(VoiceParmSet parmSet, bool reconfigure) {
            List<VoiceParm> parms = parmSet.GetParmsOfKindOrDefault(VoiceParmKind.Playback, reconfigure);
            if (parms != null) {
                foreach (VoiceParm parm in parms) {
                    switch ((PlaybackParm)parm.ctlIndex) {
                    case PlaybackParm.DefaultVolume:
                        defaultPlaybackVolume = parm.ret.fValue;
                        break;
                    default:
                        log.ErrorFormat(this, "VoiceManager.ApplyPlaybackSettings: Unknown PlaybackParm of value {0}!", parm.ctlIndex);
                        break;
                    }
                }
            }
        }

        private void MaybeDeleteRecordedFile(bool recording, string filename) {
            if (recording && File.Exists(filename)) {
                log.Info(this, "Deleting file " + filename + " prior to recording it");
                File.Delete(filename);       
            }
        }
    
        ///<summary>
        ///    Create our own private copy of fmod
        ///</summary>
        private void InitFmod() {
            lock(usingFmod) {
                try {
                    FMOD.RESULT result = FMOD.Factory.System_Create(ref fmod);
                    ERRCHECK(result);
                    // Select the playback driver
                    result = fmod.setDriver(playbackDeviceNumber);
                    ERRCHECK(result);
                    // Initialize it to suppose 3D sound
                    result = fmod.init(32, FMOD.INITFLAG.NORMAL | FMOD.INITFLAG._3D_RIGHTHANDED, (IntPtr)null);
                    ERRCHECK(result);
                    // A meter is 1000 distance units.
                    result = fmod.set3DSettings(1.0f, 1000.0f, attenuationRolloff);
                    ERRCHECK(result);
                }
                catch (Exception e) {
                    log.ErrorFormat(this, "VoiceManager.InitFmod: exception raised {0}", e.Message);
                }
            }
        }

        public void StartTestPlayback(string filename) {
            if (Path.GetDirectoryName(filename) == "")
                filename = Path.Combine(logFolder, filename);
            VoiceManager.VoiceChannel voiceChannel = AddVoiceChannel(-1L, 1, false, true);
            voiceChannel.StartPlayback(filename);
        }
        
        ///<summary>
        ///    Get the microphone with the given index
        ///</summary>
        protected MicrophoneChannel GetMicrophoneChannelInternal(int index) {
            return micChannels[index];
        }
        
        ///<summary>
        ///    Get the voice with the given index
        ///</summary>
        public VoiceChannel GetVoiceChannel(byte voiceNumber) {
            return voiceChannels[voiceNumber];
        }

        public VoiceChannel GetVoiceChannelOrNull(byte voiceNumber) {
            VoiceChannel vc;
            if (voiceChannels.TryGetValue(voiceNumber, out vc))
                return vc;
            else
                return null;
        }

        ///<summary>
        ///    Used for playback of recorded sounds
        ///</summary>
        public VoiceChannel AddVoiceChannelInternal(long oid, byte voiceNumber, bool positional, bool recordSpeex) {
            VoiceChannel vc = GetVoiceChannelOrNull(voiceNumber);
            if (vc != null)
                return vc;
            else
                return AllocateVoice(oid, voiceNumber, VoiceOpcode.AllocateCodec, positional, false);
        }
        
        ///<summary>
        ///    Make sure we dispose of the channels when we're
        ///    disposed of
        ///</summary>
        public void Dispose() {
            if (disposed)
                return;
            disposed = true;
            voiceManagerShutdown = true;
            foreach (MicrophoneChannel mic in micChannels) {
                if (mic != null)
                    mic.Dispose();
            }
            foreach (VoiceChannel vc in voiceChannels.Values) {
                if (vc != null)
                    vc.Dispose();
            }
            CloseVoiceServerConnection();
            if (!runningVoiceBot && fmod != null) {
                FMOD.RESULT result = fmod.release();
                if (result != 0)
                    log.ErrorFormat(this, "VoiceManager.Dispose: Releasing the FMOD instance, error code is {0}", result);
            }
            log.Debug(this, "VoiceManager.Dispose: VoiceManager is disposal complete");
        }

        public string LogFolderPath(string file) {
            return logFolder + "/" + file;
        }
        
        public void InitializeVoiceServerConnection() {
            log.DebugFormat(this, "VoiceManager.InitializeVoiceServerConnection: useTcp {0}", useTcp);
            if (useTcp) {
                Thread connectThread = new Thread(InitializeTcpConnection);
                connectThread.Start();
            }
            else {
                InitializeUdpConnection();
                AfterConnectToVoiceServer(true);
            }
        }
        
        public void CloseVoiceServerConnection() {
            if (useTcp) {
                if (tcpClient != null) {
                    tcpClient.Close();
                    tcpClient = null;
                }
            }
            else if (udpClient != null) {
                udpClient.Close();
                udpClient = null;
            }
        }
        
        public void InitializeTcpConnection() {
            // This constructor arbitrarily assigns the local port number.
            tcpClient = new TcpClient();
            connectionSuccessful = false;
            Monitor.Enter(tcpMonitoredObject);
            try {
                tcpClient.BeginConnect(voiceServerHost, voiceServerPort, FinishTcpConnect, this);
                // Wait for the connection timeout time to connect
                Monitor.Wait(tcpMonitoredObject, tcpConnectTimeout);
            } catch (Exception e) {
                log.ErrorFormat(this, "VoiceManager.InitializeTcpConnection connection to '{0}' at port {1} got exception: {2}",
                    voiceServerHost, voiceServerPort, e.ToString());
                tcpClient.Close();
                connectionAttempted = true;
                connectionSuccessful = false;
            }
            finally {
                Monitor.Exit(tcpMonitoredObject);
            }
            if (!connectionSuccessful) {
                if (tcpClient != null) {
                    tcpClient.Close();
                    tcpClient = null;
                }
                log.ErrorFormat(this, "VoiceManager.InitializeTcpConnection connection to '{0} at port {1}' failed",
                    voiceServerHost, voiceServerPort);
            }
            connectionAttempted = true;
        }

        public void FinishTcpConnect(IAsyncResult ar) {
            Monitor.Enter(tcpMonitoredObject);
            try {
                if (tcpClient != null && tcpClient.Client != null) {
                    if (tcpClient.Connected) {
                        log.DebugFormat(this, "VoiceManager.FinishTcpConnect: Connected to voice server at {0}, port {1}",
                            voiceServerHost, voiceServerPort);
                        tcpClient.Client.Blocking = false;
                        tcpClient.Client.NoDelay = true;
                        connectionSuccessful = true;
                    }
                }
                Monitor.PulseAll(tcpMonitoredObject);
            }
            finally {
                Monitor.Exit(tcpMonitoredObject);
            }
        }
        
        public void InitializeUdpConnection() {
            udpClient = new UdpClient();
            udpClient.Connect(voiceServerHost, voiceServerPort);
        }
        
        public bool NetworkClientActive() {
            if (useTcp)
                return (tcpClient != null || tcpClient.Client != null);
            else
                return (udpClient != null || udpClient.Client != null);
        }
        
        public static string ByteArrayToHexString(byte[] inBuf, int startingByte, int length) {
            byte ch = 0x00;
            string[] pseudo = {"0", "1", "2",
                               "3", "4", "5", "6", "7", "8",
                               "9", "A", "B", "C", "D", "E",
                               "F"};
            StringBuilder outBuf = new StringBuilder(length * 2);
            StringBuilder charBuf = new StringBuilder(length);
            for (int i=0; i<length; i++) {
                byte b = inBuf[i + startingByte];
                ch = (byte) (b & 0xF0);  // Strip off high nibble
                ch = (byte) (ch >> 4);                         // shift the bits down
                ch = (byte) (ch & 0x0F);                       // must do this if high order bit is on!
                outBuf.Append(pseudo[(int)ch]);                // convert the nibble to a String Character
                ch = (byte) (b & 0x0F);  // Strip off low nibble 
                outBuf.Append(pseudo[ (int) ch]);              // convert the nibble to a String Character
                if (b >= 32 && b <= 126)
                    charBuf.Append((char)b);
                else
                    charBuf.Append("*");
            }
            return outBuf + " == " + charBuf;
        } 

        private void SendMessageToServer(byte[] buf) {
            int size = buf.Length;
            if (!connectedToServer) {
                log.Error(this, "VoiceManager.SendMessageToServer: Not connected to voice server: " + new StackTrace(true).ToString());
                return;
            }
            log.Debug(this, "VoiceManager.SendMessageToServer: len " + (size - 2) + ", opcode " + buf[4] + ", msg " + ByteArrayToHexString(buf, 2, buf.Length - 2));
            if (useTcp)
                SendTcpMessage(buf, size);
            else
                SendUdpMessage(buf, size);
        }
    
        public void SendAuthenticateMessage(short seqNum, byte voiceNumber, long oid, long groupOid, string authToken, bool listenToYourself) {
            int msgSize = (int)voiceMsgSize[(int)VoiceOpcode.Authenticate] + authToken.Length;
            VoiceMessage msg = new VoiceMessage(msgSize, seqNum, VoiceOpcode.Authenticate, (byte)voiceNumber, 
                oid, groupOid, authToken, listenToYourself);
            SendMessageToServer(msg.Bytes);
        }

        protected void BlacklistSpeakerInternal(long speakerOid, bool blacklistHim) {
            bool currentlyBlacklisted = blacklist.Contains(speakerOid);
            if (currentlyBlacklisted == blacklistHim)
                log.ErrorFormat(this, "VoiceManager.BlacklistSpeaker: blacklistHim {0}, currentlyBlacklisted {1}; speaker is already in requested state!",
                    currentlyBlacklisted, blacklistHim);
            else if (blacklistHim) {
                log.DebugFormat(this, "VoiceManger.BlacklistSpeaker: blacklisting speaker {0}", speakerOid);
                blacklist.Add(speakerOid);
            }
            else {
                log.DebugFormat(this, "VoiceManger.BlacklistSpeaker: removing speaker {0} from the blacklist", speakerOid);
                blacklist.Remove(speakerOid);
            }
        }
        
        protected void SendSingleBlacklistChange(long speakerOid, bool blacklistHim) {
            if (connectedToServer) {
                List<long> bl = new List<long>();
                bl.Add(speakerOid);
                micChannels[0].IncSeqNum();
                if (blacklistHim)
                    SendChangeBlacklistMessage(micChannels[0].sequenceNumber, bl, new List<long>());
                else
                    SendChangeBlacklistMessage(micChannels[0].sequenceNumber, new List<long>(), bl);
            }
        }

        public void SendChangeBlacklistMessage(short seqNum, List<long> addToBlacklist, List<long> removeFromBlacklist) {
            VoiceMessage msg = new VoiceMessage(seqNum, addToBlacklist, removeFromBlacklist);
            SendMessageToServer(msg.Bytes);
        }
        
        private void SendTcpMessage(byte[] buf, int size) {
            try {
                if (tcpClient != null && tcpClient.Client != null)
                    tcpClient.Client.Send(buf, size, 0);
			} catch (Exception e) {
				log.ErrorFormat(this, "VoiceManager.SendTcpMessage, exception: {0}", e);
                if (tcpClient != null)
                    tcpClient.Client = null;
                return;
            }
            bytesSentCounter.Inc(size);
            packetsSentCounter.Inc();
        }

        private void SendUdpMessage(byte[] buf, int size) {
            try {
                udpClient.Send(buf, size);
			} catch (Exception e) {
				log.ErrorFormat(this, "VoiceManager.SendUdpMessage, exception: {0}", e);
                if (udpClient != null)
                    udpClient.Client = null;
                return;
            }
            bytesSentCounter.Inc(size);
            packetsSentCounter.Inc();
        }

        protected void HandleMessage(byte[] buf) {
            if (voiceManagerShutdown)
                return;
            VoiceChannel channel;
            try {
                if (buf.Length == 0) {
                    log.Warn(this, "VoiceManager.HandleMessage: Got message of length 0");
                    // probably a nul packet
                    return;
                }
                if (buf.Length < 4) {
                    log.ErrorFormat(this, "VoiceManager.Invalid message length: {0}", buf.Length);
                    return;
                }
                packetsReceivedCounter.Inc();
                MemoryStream memStream = new MemoryStream();
                memStream.Write(buf, 0, buf.Length);
                memStream.Flush();
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                BinaryReader reader = new BinaryReader(memStream);
                short seqNum = IPAddress.NetworkToHostOrder(reader.ReadInt16());
                byte opcodeAndFlags = reader.ReadByte();
                VoiceOpcode opcode = (VoiceOpcode)(opcodeAndFlags & 0xf);
                byte voiceNumber = reader.ReadByte();
                long oid;
                if (loggingMessages && opcode != VoiceOpcode.AggregatedData)
                    log.Debug(this, "VoiceManager.HandleMessage: Msg length " + buf.Length + 
                        ", opcode " + (int)opcode + ", seqNum " + seqNum + ": " + ByteArrayToHexString(buf, 0, buf.Length));
                switch (opcode) {
                case VoiceOpcode.VoiceUnallocated:
                    log.ErrorFormat(this, "VoiceManager.HandleMessage: Handling message for host {0}, port {1}, opcode = VoiceUnallocated",
                        voiceServerHost, voiceServerPort);
                    break;
                case VoiceOpcode.AllocateCodec:
                    allocsReceivedCounter.Inc();
                    oid = IPAddress.NetworkToHostOrder(reader.ReadInt64());
                    if (runningVoiceBot)
                        SetVoiceBotChannelInUse(voiceNumber, true);
                    else
                        AllocateVoice(oid, voiceNumber, opcode, false, false);
                    break;
                case VoiceOpcode.AllocatePositionalCodec:
                    allocsReceivedCounter.Inc();
                    oid = IPAddress.NetworkToHostOrder(reader.ReadInt64());
                    if (runningVoiceBot)
                        SetVoiceBotChannelInUse(voiceNumber, true);
                    else
                        AllocateVoice(oid, voiceNumber, opcode, true, false);
                    break;
                case VoiceOpcode.Deallocate:
                    deallocsReceivedCounter.Inc();
                    if (runningVoiceBot)
                        SetVoiceBotChannelInUse(voiceNumber, false);
                    else
                        DeallocateVoice(voiceNumber);
                    break;
                case VoiceOpcode.Data:
                    if (runningVoiceBot)
                        AssureVoiceBotChannelInUse(voiceNumber, "received a Data message but ");
                    else {
                        channel = GetVoiceChannelOrNull(voiceNumber);
                        if (channel == null)
                            log.ErrorFormat(this, "VoiceManager.HandleMessage: Data message frame of length {0} for voiceNumber {1}: channel doesn't exist!",
                                buf.Length, voiceNumber);
                        else {
                            if (!blacklist.Contains(channel.oid))
                                channel.ProcessIncomingFrame(buf, 4, buf.Length - 4);
                            SetLastSpoke(channel);
                        }
                    }
                    break;
                case VoiceOpcode.AggregatedData:
                    if (runningVoiceBot)
                        AssureVoiceBotChannelInUse(voiceNumber, "received an AggregatedData message but ");
                    else {
                        channel = GetVoiceChannelOrNull(voiceNumber);
                        if (channel == null)
                            log.ErrorFormat(this, "VoiceManager.HandleMessage: Data message frame of length {0} for voiceNumber {1}: channel doesn't exist!",
                                buf.Length, voiceNumber);
                        else if (!blacklist.Contains(channel.oid)) {
                            channel.ProcessAggregatedData(reader.ReadByte(), buf, 5, buf.Length - 5);
                            SetLastSpoke(channel);
                        }
                    }
                    break;
                case VoiceOpcode.LoginStatus:
                    oid = IPAddress.NetworkToHostOrder(reader.ReadInt64());
                    if (onLoginStatusReceived != null) {
                        log.DebugFormat(this, "VoiceManager.HandleMessage: Running event onLoginStatus for oid {0}, {1}",
                            oid, (voiceNumber == 1 ? "login" : "logout"));
                        onLoginStatusReceived(oid, voiceNumber == 1);
                    }
                    else
                        log.ErrorFormat(this, "VoiceManager.HandleMessage: Got LoginStatus msg for oid {0}, {1}, but there is no event handler",
                            oid, (voiceNumber == 1 ? "login" : "logout"));
                    break;
                default:
                    log.Error(this, "VoiceManager.HandleMessage: Illegal opcode " + opcode + " in message.  Msg length " + buf.Length + 
                        ", opcode " + (int)opcode + ", seqNum " + seqNum + ": " + ByteArrayToHexString(buf, 0, buf.Length));
                    break;
                }
            }
            catch (Exception e) {
                log.Error(this, "VoiceManager.HandleMessage: Exception " + e.Message + "; buf " + 
                    ByteArrayToHexString(buf, 0, buf.Length) + "; stack trace\n" + e.StackTrace);
            }
        }

        ///<summary>
        ///    Removes the channel from the recentSpeakers sorted list,
        ///    sets the time when the speaker spoke to be right now,
        ///    and then reinserts it.
        ///</summary>
        private void SetLastSpoke(VoiceChannel channel) {
            channel.lastSpoke = System.Environment.TickCount;
            if (!recentSpeakers.Contains(channel)) {
                if (recentSpeakers.Count == maxRecentSpeakers)
                    RemoveOldestEntry(recentSpeakers);
                recentSpeakers.Add(channel);
            }
        }
        
        private void RemoveOldestEntry(List<VoiceChannel> channels) {
            int oldestIndex = 0;
            long oldestSpoke = Int64.MaxValue;
            int i = 0;
            foreach (VoiceChannel vc in channels) {
                if (oldestSpoke > vc.lastSpoke) {
                    oldestSpoke = vc.lastSpoke;
                    oldestIndex = i;
                }
            }
            channels.RemoveAt(oldestIndex);
        }
        
        private VoiceChannel AllocateVoice(long oid, byte voiceNumber, VoiceOpcode opcode, bool positional, bool shouldExist) {
            VoiceChannel channel = GetVoiceChannelOrNull(voiceNumber);
            if (shouldExist != (channel != null))
                log.ErrorFormat(this, "VoiceManager.AllocateVoice: opcode {0} voice number {1} {2}",
                    opcode.ToString(), voiceNumber, (shouldExist ? "does not exist!" : "already exists!"));
            if (channel != null) {
                log.ErrorFormat(this, "VoiceManager.AllocateVoice: voice number {0} already allocated!", voiceNumber);
                voiceChannels.Remove(voiceNumber);
                channel.Dispose();
            }
            if (voiceChannels.Count == maxVoiceChannels)
                log.WarnFormat(this, "VoiceManager.AllocateVoice: Allocating voiceNumber {0}, but the channel count is already {1}, the maximum allowed!",
                    voiceNumber, maxVoiceChannels);
            log.Debug(this, "VoiceManager.AllocateVoice: Creating VoiceChannel object for voiceNumber " + voiceNumber + ", oid " + oid);
            VoiceChannel vc = new VoiceChannel(this, oid, voiceNumber, positional, voicesRecordSpeex, currentVoiceParms);
            voiceChannels[voiceNumber] = vc;
            oidToVoiceChannel[oid] = vc;
            if (onVoiceAllocation != null)
                onVoiceAllocation(oid, voiceNumber, positional);
            return vc;
        }
        
        private void DeallocateVoice(byte voiceNumber) {
            VoiceChannel channel = voiceChannels[voiceNumber];
            log.Debug(this, "VoiceManager.DeallocateVoice: deallocating voiceNumber " + voiceNumber + ", channel " + channel);
            if (channel == null)
                log.ErrorFormat(this, "VoiceManager.DeallocateVoice: voice number {0} doesn't exist!", voiceNumber);
            else {
                voiceChannels.Remove(voiceNumber);
                oidToVoiceChannel.Remove(channel.oid);
                if (onVoiceDeallocation != null)
                    onVoiceDeallocation(channel.oid);
                channel.Dispose();
            }
        }

        private void SetVoiceBotChannelInUse(byte voiceNumber, bool setInUse) {
            if (setInUse == botVoiceChannelInUse[voiceNumber]) {
                log.ErrorFormat(this, "VoiceManager.SetVoiceBotChannelInUse: For voiceNumber {0}, in-use is {1} but set-in-use is also {2}",
                    voiceNumber, botVoiceChannelInUse[voiceNumber], setInUse);
            }
            botVoiceChannelInUse[voiceNumber] = setInUse;
        }
        
        private void AssureVoiceBotChannelInUse(byte voiceNumber, string what){
            if (!botVoiceChannelInUse[voiceNumber])
                log.ErrorFormat(this, "VoiceManager.AssureVoiceBotChannelInUse: For voiceNumber {0}, {1} channel is not in use!", 
                    voiceNumber, what);
        }
        
        /// <summary>
		///      Set the position and velocity of the player
		/// </summary>
        public void SetPlayerVoiceProperties(Vector3 originalPosition, Vector3 newPosition, long timeInterval, 
                                             Vector3 listenerForward, Vector3 listenerUp) {
            log.DebugFormat(this, "VoiceManager.SetPlayerVoiceProperties: newPosition {0}, FMOD's old position {1}", 
                newPosition, playerListenerProperties.ListenerPosition);
            Vector3 v = CalculateVelocity(originalPosition, newPosition, timeInterval);
            playerListenerProperties.ListenerPosition = newPosition;
            playerListenerProperties.ListenerVelocity = v;
            playerListenerProperties.ListenerForward = listenerForward;
            playerListenerProperties.ListenerUp = listenerUp;
        }

        /// <summary>
		///      Call into fmod to give a sound source a different
		///      position and velocity
		/// </summary>
        public void MovePositionalSound(long oid, Vector3 originalPosition, Vector3 newPosition, long timeInterval) {
            Vector3 v = CalculateVelocity(originalPosition, newPosition, timeInterval);
            Vector3 p = newPosition;
            lock(usingFmod) {
                VoiceChannel vc;
                if (oidToVoiceChannel.TryGetValue(oid, out vc) && vc != null) {
                    if (vc.Active) {
                        log.DebugFormat(this, "VoiceManager.MovePositionalSound: newPosition {0}, FMOD's previous position {1}, playback volume {2}",
                            newPosition, vc.Position, vc.GetPlaybackVolume());
                        vc.SetPositionAndVelocity(p, v);
                    }
                }
            }
        }
        
        protected Vector3 CalculateVelocity(Vector3 originalPosition, Vector3 newPosition, long timeInterval) {
            Vector3 v;
            if (timeInterval == 0)
                v = Vector3.Zero;
            else
                v = (newPosition - originalPosition) * ((float)(timeInterval) / 1000f);
            return v;
        }

        /// <summary>
		///      Starts a receive operation
		/// </summary>
        private void WaitForTcpData() {
            // Begin receiving the data from the remote device.
            try {
                if (tcpClient == null || tcpClient.Client == null)
                    return;
                tcpClient.Client.BeginReceive(receiveBuffer, bytesInReceiveBuffer,
                    receiveBufferSize - bytesInReceiveBuffer, 0,
                    new AsyncCallback(OnTcpDataReceived), null);
			} catch (Exception e) {
				if (!voiceManagerShutdown)
                    log.ErrorFormat(this, "VoiceManager.WaitForTcpData, exception: {0}", e);
                if (tcpClient != null)
                    tcpClient.Client = null;
                return;
            }
        }

        private void AdvanceReceiveBuffer(int amount) {
            bytesInReceiveBuffer -= amount;
            receiveBufferOffset += amount;
        }
        
        /// <summary>
		///      Callback for the TCP data
		/// </summary>
		/// <param name="asyn"></param>
		private void OnTcpDataReceived(IAsyncResult asyn) {
            int bytesReceived;
            try {
                if (tcpClient == null || tcpClient.Client == null)
                    return;
                bytesReceived = tcpClient.Client.EndReceive(asyn);
			} catch (Exception e) {
				if (!voiceManagerShutdown)
                    log.ErrorFormat(this, "VoiceManager.OnTcpDataReceived, exception: {0}", e);
                if (tcpClient != null)
                    tcpClient.Client = null;
                return;
			}
// 			if (loggingMessages)
//              log.Debug(this, "VoiceManager.OnTcpDataReceived: Received raw data of length " + bytesReceived +
//                    ": " + ByteArrayToHexString(receiveBuffer, receiveBufferOffset, bytesReceived));
            try {
                bytesReceivedCounter.Inc(bytesReceived);
                bytesInReceiveBuffer += bytesReceived;
                // If we are still building a message from the last read
                if (currentMsgBuf != null) {
                    int bufLeft = currentMsgBuf.Length - currentMsgOffset;
                    int amountToCopy = Math.Min(bytesInReceiveBuffer, bufLeft);
                    Array.Copy(receiveBuffer, receiveBufferOffset, currentMsgBuf, currentMsgOffset, amountToCopy);
                    AdvanceReceiveBuffer(amountToCopy);
                    currentMsgOffset += amountToCopy;
                    if (amountToCopy == bufLeft) {
                        // We have a complete message
                        HandleMessage(currentMsgBuf);
                        currentMsgBuf = null;
                    }
                }
                while (bytesInReceiveBuffer >= messageLengthByteCount) {
                    // Get the message length - of size 2.
                    short length = (short)((receiveBuffer[receiveBufferOffset] << 8) | receiveBuffer[receiveBufferOffset + 1]);
                    AdvanceReceiveBuffer(messageLengthByteCount);
//                     if (loggingMessages)
//                         log.DebugFormat(this, "OnTcpDataReceived: In while loop, length {0}, receiveBufferOffset {1}, currentMsgLength {2}, currentMsgOffset {3}",
//                             length, receiveBufferOffset, currentMsgLength, currentMsgOffset);
                    byte[] buf = new byte[length];
                    if (bytesInReceiveBuffer >= length) {
                        Array.Copy(receiveBuffer, receiveBufferOffset, buf, 0, length);
                        HandleMessage(buf);
                        AdvanceReceiveBuffer(length);
                    }
                    else {
                        // We didn't get all the bytes required for this message
                        currentMsgBuf = buf;
                        currentMsgOffset = bytesInReceiveBuffer;
                        Array.Copy(receiveBuffer, receiveBufferOffset, buf, 0, bytesInReceiveBuffer);
                        AdvanceReceiveBuffer(bytesInReceiveBuffer);
                    }
                }
                if (bytesInReceiveBuffer == 0)
                    receiveBufferOffset = 0;
                else {
                    int left = bytesReceived - receiveBufferOffset;
                    if (bytesInReceiveBuffer < 0 || bytesInReceiveBuffer > (messageLengthByteCount - 1))
                        log.ErrorFormat(this, "VoiceManager.OnTCPDataReceived: Bytes left illegal!  left {0} bytesReceived {1} receiveBufferOffset {2} currentMsgBuf {3}",
                            bytesInReceiveBuffer, bytesInReceiveBuffer, receiveBufferOffset, currentMsgBuf);
                    else {
                        Array.Copy(receiveBuffer, receiveBufferOffset, receiveBuffer, 0, bytesInReceiveBuffer);
                        receiveBufferOffset = 0;
                    }
                }
            }
            catch (Exception e) {
                log.Error(this, "VoiceManager.OnTcpDataReceived: Exception " + e.Message + "; stack trace\n" + e.StackTrace);
            }
            
            WaitForTcpData();
        }

        ///<summary>
        ///    The static utility method to check fmod API results
        ///</summary>
        public static void ERRCHECK(FMOD.RESULT result) {
            if (result != FMOD.RESULT.OK) {
                log.Error(null, "VoiceManager.ERRCHECK: error code " + FMOD.Error.String(result));
                ThrowError("FMOD error! " + result + " - " + FMOD.Error.String(result));
            }
        }

        ///<summary>
        ///    This records the message and the stack trace, and then
        ///    throws the exception.
        ///</summary>
        public static void ThrowError(string msg) {
            log.Error(null, msg + "; stack trace:\n" + new StackTrace(true).ToString());
            throw new Exception(msg);
        }
        
        public int PacketsSentCounter {
            get {
                return packetsSentCounter.count;
            }
        }
        
        public int PacketsReceivedCounter {
            get {
                return packetsReceivedCounter.count;
            }
        }
        
        public int BytesSentCounter {
            get {
                return bytesSentCounter.count;
            }
        }
        
        public int BytesReceivedCounter {
            get {
                return bytesReceivedCounter.count;
            }
        }
        
        public int VoiceCount {
            get {
                return voiceChannels.Count;
            }
        }
            
        public bool MicActive {
            get {
                return (micChannels[0] != null) && micChannels[0].Active;
            }
        }
            
        public bool MicSpeaking {
            get {
                return (micChannels[0] != null) && micChannels[0].MicSpeaking;
            }
        }
            
        public int MicWAVBytesRecordedCounter {
            get {
                return micWAVBytesRecordedCounter.count;
            }
        }
        
        public int MicSpeexFramesRecordedCounter {
            get {
                return micSpeexFramesRecordedCounter.count;
            }
        }
        
        public int VoicesSpeexFramesRecordedCounter {
            get {
                return voicesSpeexFramesRecordedCounter.count;
            }
        }
        
        public VoiceAllocationEvent OnVoiceAllocation {
            get {
                return onVoiceAllocation;
            }
            set {
                onVoiceAllocation = value;
            }
        }
        
        public VoiceDeallocationEvent OnVoiceDeallocation {
            get {
                return onVoiceDeallocation;
            }
            set {
                onVoiceDeallocation = value;
            }
        }
        
        public bool Disposed {
            get {
                return disposed;
            }
        }
        
        ///<summary>
        ///    This base class contains the machinery shared by both
        ///    MicrophoneChannels, which create sound streams but
        ///    don't play them, and VoiceChannels play sound streams
        ///    but don't create them.
        ///</summary>
        public class BasicChannel : SoundProperties {
            
            ///<summary>
            ///    The VoiceManager to which this channel belongs
            ///</summary>
            protected VoiceManager voiceMgr = null;

            ///<summary>
            ///    Is the mic device set up for use?
            ///</summary>
            protected bool sourceReady = false;

            ///<summary>
            ///    Is the device set currently in use?
            ///</summary>
            protected bool sourceActive = false;

            ///<summary>
            ///    The number of frames per second
            ///</summary>
            public int framesPerSecond;

            ///<summary>
            ///    The number of samples per second.  Many customers
            ///    will choose the 8000 samples/second settings.
            ///</summary>
            public int samplesPerSecond;

            ///<summary>
            ///    The number of samples in one frame
            ///</summary>
            public int samplesPerFrame;

            ///<summary>
            ///    The number of samples in one frame
            ///</summary>
            public short sequenceNumber = 0;

            ///<summary>
            ///    The oid of the entity making the sound
            ///</summary>
            public long oid;
            
            ///<summary>
            ///    Default to narrow-band
            ///</summary>
            protected SpeexBand speexBand = SpeexBand.SPEEX_MODEID_NB;

            ///<summary>
            ///    The sound instance associated with the microphone
            ///</summary>
            protected FMOD.Sound channelSound = null;

            ///<summary>
            ///    The codec used to encode microphone PCM samples.
            ///</summary>
            protected SpeexCodec channelCodec = null;

            ///<summary>
            ///    For a MicrophoneChannel, this holds the PCM samples
            ///    prior to encoding.  For a VoiceChannel, this holds
            ///    the decoded PCM samples prior to fmod playing them.
            ///</summary>
            protected CircularBuffer queuedSamples = null;

            ///<summary>
            ///    Is this channel recording now?
            ///</summary>
            protected bool recordingSpeex = false;

            ///<summary>
            ///    Are the encoded frames in this channel being recorded?
            ///</summary>
            protected FileStream recordStreamSpeex = null;

            ///<summary>
            ///    Is this channel recording now?
            ///</summary>
            protected bool recordingWAV = false;

            ///<summary>
            ///    Are the PCM samples in this channel being recorded?
            ///</summary>
            protected FileStream recordStreamWAV = null;

            ///<summary>
            ///    The position of the last frame, measured in PCM
            ///    samples
            ///</summary>
            protected int lastRecordPos = 0;
            
            ///<summary>
            ///    The length of the recorded .wav file, measured in
            ///    PCM samples.
            ///</summary>
            protected int dataLength = 0;

            protected string title;
            
            /// The AGC setting, on a scale from 1 to 20
            protected int micLevel = 5;
            
            ///<summary>
            ///    If we're playing back recorded samples, this stream
            ///    will be non-null.
            ///</summary>
            protected FileStream playbackStream = null;

            ///<summary>
            ///    The file name of the file we're playing back, for logging.
            ///</summary>
            protected string playbackFile = "";

            ///<summary>
            ///    The file name of the file we're playing back, for logging.
            ///</summary>
            protected int playbackFileOffset;
            
            ///<summary>
            ///   If we're in playback mode, and are waiting to start
            ///   reading the next file, this is the max time til we
            ///   open the next Speex file.
            ///</summary>
            protected int maxWaitTilNextPlayback = 60;

            ///<summary>
            ///   The number of saved frames we maintain, to 
            ///   which are sent when the mic transitions from
            ///   inaudible to audible.  By default, the same 
            ///   as the audible frame count threshold.
            ///</summary>
            public int maxSavedFrames = 5;
        
            ///<summary>
            ///   If true, we use a power threshold computation rather
            ///   than Speex VAD to determine voice activity.
            ///</summary>
            public bool usePowerThreshold = true;

            ///<summary>
            ///   The power threshold for a frame at which we consider
            ///   the person to be speaking if usePowerThreshold is true.
            ///</summary>
            public float framePowerThreshold = 1.5f;
            
            ///<summary>
            ///   Set when we're disposing a channel, so we
            ///   immediately return in callbacks and avoid errors.
            ///</summary>
            protected bool disposing = false;
            
            ///<summary>
            ///    Constructor
            ///</summary>
            public BasicChannel(VoiceManager voiceMgr, bool recordingWAV, bool recordingSpeex, bool ambient) : base("", ambient) {
                
                this.voiceMgr = voiceMgr;
                // For VoiceChannels, these values will be overriden
                // by the codec definition.
                framesPerSecond = 50;
                samplesPerSecond = 8000;
                samplesPerFrame = samplesPerSecond / framesPerSecond;
                this.recordingWAV = recordingWAV;
                this.recordingSpeex = recordingSpeex;
            }
            
            ///<summary>
            ///    Make sure to free the sound and the codec
            ///</summary>
            public void Dispose() {
                disposing = true;
                lock(voiceMgr.usingFmod) {
                    sourceReady = false;
                    StopRecording(channelSound);
                    if (playbackStream != null) {
                        playbackStream.Close();
                        playbackStream = null;
                        playbackFile = "";
                    }
                    log.Debug(voiceMgr, "BasicChannel.Dispose called");
                    if (channel != null) {
                        CheckRetCode("Stopping the FMOD channel in BasicChannel.Dispose()", channel.stop());
                        channel = null;
                    }
                    if (channelSound != null) {
                        CheckRetCode("Releasing the FMOD sound in BasicChannel.Dispose()", channelSound.release());
                        channelSound = null;
                    }
                    if (channelCodec != null) {
                        channelCodec.ResetEncoder();
                        channelCodec = null;
                    }
                }
            }
            
            ///<summary>
            ///    Transition this channel from inactive to active,
            ///    and if playChannel is true, call the FMOD playSound
            ///    entrypoint on this channel's sound.
            ///
            ///    You only call with playChannel == true if you want
            ///    to _hear_ the channel on the local sound system.
            ///    So for microphone channels, playChannel = false.
            ///</summary>
            public void StartChannel(bool playChannel) {
                try {
                    if (sourceActive) {
                        log.Error(voiceMgr, "BasicChannel.StartChannel, channel already active");
                    }

                    FMOD.RESULT result = 0;
                    if (playChannel) {
                        lock(voiceMgr.usingFmod) {
                            result = voiceMgr.fmod.playSound(FMOD.CHANNELINDEX.REUSE, channelSound, false, ref channel);
                        }
                    }
                    if (result == 0)
                        sourceActive = true;
                    else
                        CheckRetCode("BasicChannel.StartChannel", result);
                    if (this is VoiceChannel) {
                        lock (voiceMgr.usingFmod) {
                            result = channel.set3DMinMaxDistance(voiceMgr.minAttenuation * 1000.0f, voiceMgr.maxAttenuation * 1000.0f);
                        }
                    }
                        
                }
                catch (Exception e) {
                    log.ErrorFormat(voiceMgr, "BasicChannel.StartChannel: exception raised while stopping channel: {0}", e.Message);
                }

            }

            ///<summary>
            ///    You only call with playChannel == true if you have
            ///    been hearing the channel on the local sound system.
            ///    So it doesn't get called for microphone channels.
            ///</summary>
            public void StopChannel(bool playChannel) {
                if (!sourceActive)
                    log.ErrorFormat(voiceMgr, "BasicChannel.StopChannel, channel {0}/FMOD{1}, channel not active");
                else {
                    try {
                        if (playChannel) {
                            FMOD.RESULT result;
                            lock(voiceMgr.usingFmod) {
                                result = channel.stop();
                            }
                            ERRCHECK(result);
                        }
                        sourceActive = false;
                    }
                    catch (Exception) {
                    }
                }
            }

            ///<summary>
            ///    Return whether the channel is active
            ///</summary>
            public bool Active {
                get {
                    return sourceActive;
                }
            }
                
            ///<summary>
            ///    Return whether the channel is active
            ///</summary>
            public int MicLevel {
                get {
                    return micLevel;
                }
            }
                
            ///<summary>
            ///    Add the buffer of PCM samples to the circular
            ///    buffer for this channel.  Returns the number queued
            ///</summary>
            public int QueuePCMSamples(short[] buffer, int startIndex, int sampleCount) {
                if (queuedSamples == null) {
                    log.Error(voiceMgr, "QueuePCMSamples: queuedSamples is null!");
                    return 0;
                }
                int count = queuedSamples.PutSamples(buffer, startIndex, sampleCount);
                if (count < sampleCount) {
                    overrunFrameCounter.Inc();
                    log.ErrorFormat(voiceMgr, "BasicChannel.QueuePCMSamples overrun: sampleCount {0}, put count {1}, Free {2}",
                        sampleCount, count, queuedSamples.Free);
                    if (count == 0)
                        noBytesQueuedFrameCounter.Inc();
                }
                if (voiceMgr.loggingMicTick)
                    log.DebugFormat(voiceMgr, "BasicChannel.QueuePCMSamples for {0}: sampleCount {1}, put count {2}, Free {3}, WritePos {4}, ReadPos {5}",
                        title, sampleCount, count, queuedSamples.Free, queuedSamples.WritePos, queuedSamples.ReadPos);
                return count;
            }
            
            ///<summary>
            ///    Scale the shorts in the buffer starting with
            ///    startIndex for sampleCount samples by the scale
            ///    factor, being careful to threshold the scaling by
            ///    the range of a short.
            ///</summary>
            public void ScalePCMSamples(float scale, short[] buffer, int startIndex, int sampleCount) {
                for (int i=0; i<sampleCount; i++) {
                    int index = startIndex + i;
                    short sample = buffer[index];
                    float scaledSample = sample * scale;
                    if (scaledSample > short.MaxValue)
                        sample = short.MaxValue;
                    else if (scaledSample < short.MinValue)
                            sample = short.MinValue;
                    else
                        sample = (short)scaledSample;
                    buffer[index] = sample;
                }
            }
            
            ///<summary>
            ///    Create a streams to recording either the raw
            ///    PCM sample stream or the encoded frame stream or
            ///    both.
            ///</summary>
            public void StartRecording(FMOD.Sound sound, string path, bool raw, bool encoded) {
                lastRecordPos = 0;
                dataLength = 0;

                if (raw) {
                    log.InfoFormat(voiceMgr, "BasicChannel.StartRecording: Recording WAV to path '{0}.wav'", path);
                    recordStreamWAV = new FileStream(path + ".wav", FileMode.Append, FileAccess.Write);
                    // Write out the wav header.  As we don't know the length yet it will be 0.
                    WaveWriter.WriteWavHeader(recordStreamWAV, sound, 0, voiceMgr.usingFmod);
                }
                
                if (encoded) {
                    log.InfoFormat(voiceMgr, "BasicChannel.StartRecording: Recording Speex to path '{0}.speex'", path);
                    recordStreamSpeex = new FileStream(path + ".speex", FileMode.Append, FileAccess.Write);
                }
            }

            public void StartFmodMicUpdates(FMOD.Sound sound) {
                try {
                    FMOD.RESULT result;
                    lock(voiceMgr.usingFmod) {
                        result = voiceMgr.fmod.recordStart(sound, true);
                    }
                    ERRCHECK(result);
                }
                catch (Exception e) {
                    log.ErrorFormat(voiceMgr, "BasicChannel.StartFmodMicUpdates: exception raised {0}", e.Message);
                }
            }

            ///<summary>
            ///    For each recording stream, finish writing, close
            ///    the stream and set the stream variable to null
            ///</summary>
            public void StopRecording(FMOD.Sound sound) {
                // Write back the wav header now that we know its length.
                if (recordingWAV) {
                    WaveWriter.WriteWavHeader(recordStreamWAV, sound, dataLength, voiceMgr.usingFmod);
                    log.Info(voiceMgr, "BasicChannel.StopRecording: Closing recording file " + recordStreamWAV.Name);
                    recordStreamWAV.Close();
                    recordStreamWAV = null;
                    recordingWAV = false;
                }
                if (recordingSpeex) {
                    log.Info(voiceMgr, "BasicChannel.StopRecording: Closing recording file " + recordStreamSpeex.Name);
                    recordStreamSpeex.Close();
                    recordStreamSpeex = null;
                    recordingSpeex = false;
                }
            }

            ///<summary>
            ///    Write byteCount bytes starting with from the
            ///    address in data to the stream
            ///</summary>
            public void WriteRecording(FileStream stream, IntPtr data, int byteCount) {
                byte[] buf = new byte[byteCount];
                Marshal.Copy(data, buf, 0, byteCount);
                stream.Write(buf, 0, byteCount);
                dataLength += byteCount;
            }
            
            ///<summary>
            ///    Write byteCount bytes starting with from the
            ///    address for the first element of the short array to
            ///   the stream 
            ///</summary>
            public void WriteRecording(FileStream stream, short[] shorts, int byteCount) {
                // ??? Isn't there some way to avoid this copy?
                byte[] buf = new byte[byteCount];
                unsafe {
                    fixed (short* shortArray = shorts) {
                        byte* shortBytes = (byte*)shortArray;
                        for (int i = 0; i < byteCount; i++)
                            buf[i] = shortBytes[i];
                    }
                }
                stream.Write(buf, 0, byteCount);
                dataLength += byteCount;
            }
            
            ///<summary>
            ///    Write byteCount bytes starting with from the
            ///    address for the first element of the byte array to
            ///   the stream 
            ///</summary>
            public void WriteRecording(FileStream stream, byte[] bytes, int startIndex, int byteCount, bool writeByteCount) {
                if (writeByteCount)
                    stream.WriteByte((byte)byteCount);
                stream.Write(bytes, startIndex, byteCount);
                dataLength += byteCount;
            }
            
            protected void SendMessage(byte voiceNumber, VoiceOpcode opcode) {
                SendMessage(voiceNumber, opcode, null, 0);
            }

            ///<summary>
            ///    Increment the short seqNum by 1 so that it wraps around properly.
            ///</summary
            public void IncSeqNum() {
                if (sequenceNumber == short.MaxValue)
                    sequenceNumber = short.MinValue;
                else
                    sequenceNumber = (short)(sequenceNumber + 1);
            }

            ///<summary>
            ///    Increment the short seqNum by an int so that it wraps around properly.
            ///</summary
            public void IncSeqNum(int byWhat) {
                int sum = byWhat + (int)sequenceNumber;
                if (sum >= short.MaxValue)
                    sequenceNumber = (short)(short.MinValue + (sum - short.MaxValue));
                else
                    sequenceNumber = (short)(sum);
            }

            // For non-data messages, msgBytes is null, and byteCount is zero
            protected void SendMessage(byte voiceNumber, VoiceOpcode opcode, byte[] msgBytes, int byteCount) {
                // Put the header plus the bytes into one array
                int headerSize = voiceMsgSize[(int)opcode];
                byte length = (byte)(headerSize + byteCount);
                IncSeqNum();
                VoiceMessage msg = new VoiceMessage(length, sequenceNumber, opcode, (byte)voiceNumber);
                if (byteCount > 0)
                    msg.WriteBytes(msgBytes, byteCount);
                voiceMgr.SendMessageToServer(msg.Bytes);
            }

            protected void SendAggregatedDataMessage(byte voiceNumber, List<byte[]> dataFrames) {
                int totalLen = voiceMsgSize[(int)VoiceOpcode.AggregatedData];
                foreach (byte[] dataFrame in dataFrames)
                    totalLen += dataFrame.Length + 1;
                IncSeqNum();
                VoiceMessage msg = new VoiceMessage(totalLen, sequenceNumber, VoiceOpcode.AggregatedData, voiceNumber);
                msg.WriteByte((byte)dataFrames.Count);
                foreach (byte[] dataFrame in dataFrames)
                    msg.WriteSubDataFrame(dataFrame);
                voiceMgr.SendMessageToServer(msg.Bytes);
                aggregatedDataSentCounter.Inc();
                // Increment the sequence number such that every
                // aggregated voice frame gets it's own sequence
                // number.
                IncSeqNum(dataFrames.Count - 1);
            }

            // For non-data messages, msgBytes is null, and byteCount is zero
            public void SendAllocMessage(byte voiceNumber, VoiceOpcode opcode, long oid) {
                allocsSentCounter.Inc();
                // Put the header plus the bytes into one array
                byte msgSize = voiceMsgSize[(int)opcode];
                IncSeqNum();
                VoiceMessage msg = new VoiceMessage(msgSize, sequenceNumber, opcode, voiceNumber, oid);
                voiceMgr.SendMessageToServer(msg.Bytes);
            }

            public void ApplyMicrophoneSettings(VoiceParmSet parmSet, bool reconfigure) {
                ApplyTransmissionParms(parmSet, reconfigure);
                ApplyEncodecSettings(parmSet, reconfigure);
                ApplyPreprocessorSettings(parmSet, reconfigure);
                ApplyVoiceBotParms(parmSet, reconfigure);
            }
            
            protected void ApplyEncodecSettings(VoiceParmSet parmSet, bool reconfigure) {
                List<VoiceParm> parms = parmSet.GetParmsOfKindOrDefault(VoiceParmKind.Encodec, reconfigure);
                log.DebugFormat(voiceMgr, "BasicChannel.ApplyEncodecSettings: There are {0} settings", parms != null ? parms.Count : 0);
                if (parms != null) {
                    foreach (VoiceParm parm in parms) {
                        log.DebugFormat(voiceMgr, "BasicChannel.ApplyEncodecSettings: setting {0} to {1}", parm.name, parm.value);
                        if (parm.valueKind == ValueKind.Float) {
                            if (parm.ctlIndex == 0)
                                voiceMgr.silentFrameCountDeallocationThreshold = (int)(framesPerSecond * parm.ret.fValue);
                            else
                                log.ErrorFormat(voiceMgr, "BasicChannel.ApplyEncodecSettings: unknown float parm, ctlIndex {0}", parm.ctlIndex);
                        }
                        // All the rest int parms, and none of them require special treatment
                        else if (parm.valueKind != ValueKind.Int)
                            log.ErrorFormat(voiceMgr, "BasicChannel.ApplyEncodecSettings: codec parm '{0}' is not int-valued; instead value type is '{1}'!",
                                parm.name, parm.valueKind);
                        else {
                            SpeexCtlCode code = (SpeexCtlCode)parm.ctlIndex;
                            if (code == SpeexCtlCode.SPEEX_SET_QUALITY) {
                                int mode = 0;
                                channelCodec.GetOneCodecSetting(true, SpeexCtlCode.SPEEX_GET_MODE, ref mode);
                                log.DebugFormat(voiceMgr, "BasicChannel.ApplyEncodecSettings: Before setting quality to {0}, mode is {1}",
                                    parm.ret.iValue, mode);
                            }
                            CheckRetCode(parm, channelCodec.SetOneCodecSetting(true, code, parm.ret.iValue));
                            if (code == SpeexCtlCode.SPEEX_SET_QUALITY) {
                                int mode = 0;
                                channelCodec.GetOneCodecSetting(true, SpeexCtlCode.SPEEX_GET_MODE, ref mode);
                                log.DebugFormat(voiceMgr, "BasicChannel.ApplyEncodecSettings: After setting quality to {0}, mode is {1}",
                                    parm.ret.iValue, mode);
                            }
                        }
                    }
                }
            }

            protected void ApplyDecodecSettings(VoiceParmSet parmSet, bool reconfigure) {
                List<VoiceParm> parms = parmSet.GetParmsOfKindOrDefault(VoiceParmKind.Decodec, reconfigure);
                log.DebugFormat(voiceMgr, "BasicChannel.ApplyDecodecSettings: There are {0} settings", parms != null ? parms.Count : 0);
                if (parms != null) {
                    foreach (VoiceParm parm in parms) {
                        log.DebugFormat(voiceMgr, "BasicChannel.ApplyDecodecSettings: setting {0} to {1}", parm.name, parm.value);
                        // They are all int parms, and none of them require special treatment
                        if (parm.valueKind != ValueKind.Int)
                            log.ErrorFormat(voiceMgr, "BasicChannel.ApplyDecodecSettings: codec parm '{0}' is not int-valued; instead value type is '{1}'!",
                                parm.name, parm.valueKind);
                        else
                            CheckRetCode(parm, channelCodec.SetOneCodecSetting(false, (SpeexCtlCode)parm.ctlIndex, parm.ret.iValue));
                    }
                }
            }

            protected void ApplyPreprocessorSettings(VoiceParmSet parmSet, bool reconfigure) {
                List<VoiceParm> parms = parmSet.GetParmsOfKindOrDefault(VoiceParmKind.Preprocessor, reconfigure);
                log.DebugFormat(voiceMgr, "BasicChannel.ApplyPreprocessorSettings: There are {0} settings", parms != null ? parms.Count : 0);
                if (parms != null) {
                    foreach (VoiceParm parm in parms) {
                        log.DebugFormat(voiceMgr, "BasicChannel.ApplyPreprocessorSettings: setting {0} to {1}", parm.name, parm.value);
                        // DENOISE, MICLEVEL, FRAME_POWER_THRESHOLD
                        // and SET_VAD require special treatment
                        if (parm.ctlIndex == (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_DENOISE)
                            CheckRetCode(parm, channelCodec.SetOnePreprocessorSetting((PreprocessCtlCode)parm.ctlIndex, (parm.ret.bValue ? 1 : 2)));
                        else if (parm.ctlIndex == (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC_LEVEL)
                            SetMicLevel(parm.ret.iValue);
                        else if (parm.ctlIndex == (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_VAD && usePowerThreshold)
                            // Turn off VAD if we're using the power
                            // threshold instead of preprocess VAD
                            CheckRetCode(parm, channelCodec.SetOnePreprocessorSetting(PreprocessCtlCode.SPEEX_PREPROCESS_SET_VAD, 0));
                        else {
                            switch (parm.valueKind) {
                            case ValueKind.Int:
                                CheckRetCode(parm, channelCodec.SetOnePreprocessorSetting((PreprocessCtlCode)parm.ctlIndex, parm.ret.iValue));
                                break;
                            case ValueKind.Float:
                                CheckRetCode(parm, channelCodec.SetOnePreprocessorSetting((PreprocessCtlCode)parm.ctlIndex, parm.ret.fValue));
                                break;
                            case ValueKind.Bool:
                                CheckRetCode(parm, channelCodec.SetOnePreprocessorSetting((PreprocessCtlCode)parm.ctlIndex, parm.ret.bValue));
                                break;
                            default:
                                log.Error(voiceMgr, "BasicChannel.ApplyPreprocessorSettings: Unknown parm.valueKind " + parm.valueKind);
                                break;
                            }
                        }
                    }
                }
            }

            protected void ApplyJitterBufferSettings(VoiceParmSet parmSet, bool reconfigure) {
                List<VoiceParm> parms = parmSet.GetParmsOfKindOrDefault(VoiceParmKind.JitterBuffer, reconfigure);
                if (parms != null) {
                    foreach (VoiceParm parm in parms) {
                        log.DebugFormat(voiceMgr, "BasicChannel.ApplyJitterBufferSettings: setting {0} to {1}", parm.name, parm.value);
                        CheckRetCode(parm, channelCodec.SetOneJitterBufferSetting((JitterBufferCtlCode)parm.ctlIndex, parm.ret.iValue));
                    }
                }
            }
            
            protected void ApplyVoiceBotParms(VoiceParmSet parmSet, bool reconfigure) {
                foreach (VoiceParm parm in parmSet.GetParmsOfKindOrDefault(VoiceParmKind.VoiceBot, reconfigure)) {
                    log.DebugFormat(voiceMgr, "VoiceManager.ApplyVoiceBotParms: setting voice bot parm {0} to {1}", parm.name, parm.value);
                    switch ((VoiceBotParm)(parm.ctlIndex)) {
                    case VoiceBotParm.MaxWaitTilNextPlayback:
                        maxWaitTilNextPlayback = parm.ret.iValue;
                        break;
                    default:
                        log.ErrorFormat(voiceMgr, "VoiceManager.ApplyVoiceBotParms: Unknown voice parm '{0}', index {1}", parm.name, parm.ctlIndex);
                        break;
                    }
                }
            }

            protected void ApplyTransmissionParms(VoiceParmSet parmSet, bool reconfigure) {
                foreach (VoiceParm parm in parmSet.GetParmsOfKindOrDefault(VoiceParmKind.Transmission, reconfigure)) {
                    log.DebugFormat(voiceMgr, "VoiceManager.ApplyTransmissionParms: setting voice bot parm {0} to {1}", parm.name, parm.value);
                    switch ((TransmissionParm)(parm.ctlIndex)) {
                    case TransmissionParm.MaxSavedFrames:
                        maxSavedFrames = parm.ret.iValue;
                        break;
                    case TransmissionParm.UsePowerThreshold:
                        usePowerThreshold = parm.ret.bValue;
                        break;
                    case TransmissionParm.FramePowerThreshold:
                        framePowerThreshold = parm.ret.fValue;
                        break;
                    default:
                        log.ErrorFormat(voiceMgr, "VoiceManager.ApplyTransmissionParms: Unknown voice parm '{0}', index {1}", parm.name, parm.ctlIndex);
                        break;
                    }
                }
            }

            public void CheckRetCode(VoiceParm parm, int retcode) {
                if (retcode != 0)
                    log.ErrorFormat(voiceMgr, "BasicChannel.CheckRetCode: Setting parm '{0}' to value '{1}' resulted in error code {3}",
                        parm.name, VoiceParmSet.StringValue(parm), retcode);
            }

            public void CheckRetCode(string where, FMOD.RESULT retcode) {
                if (retcode != 0)
                    log.ErrorFormat(voiceMgr, "{0} resulted in error code {1}",
                        where, retcode);
            }

            public void SetMicLevel(int micLevelInput) {
                micLevel = Math.Min(20, Math.Max(1, micLevelInput));
                float agcLevel = micLevel * 32750 / 20;
                log.DebugFormat(voiceMgr, "BasicChannel.SetMicLevel: Setting agcLevel to {0} on a range from 0 to 32765, corresponding to micLevel of {1} on a scale from 1 to 20",
                    agcLevel, micLevel);
                int retcode = channelCodec.SetOnePreprocessorSetting(PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC_LEVEL, agcLevel);
                if (retcode != 0)
                    log.ErrorFormat(voiceMgr, "BasicChannel.SetMicLevel: Setting parm 'agc_level' to value '{0}' resulted in error code {1}",
                        micLevel, retcode);
            }
            
            public bool OpenPlaybackStream(string fileName) {
                playbackFileOffset = 0;
                playbackStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (playbackStream == null) {
                    log.ErrorFormat(voiceMgr, "BasicChannel.OpenPlaybackStream: Could not open '{0}'", fileName);
                    return false;
                }
                else {
                    playbackFile = fileName;
                    log.DebugFormat(voiceMgr, "BasicChannel.OpenPlaybackStream: Opened '{0}' for player {1}", fileName, voiceMgr.playerOid);
                    return true;
                }
            }
            
            // Return the total frame size for narrow or wide-band mode
            // given.  If it doesn't understand the mode, returns -1
            public int EncodedFrameSizeForMode(int mode, bool wideBand) {
                if (wideBand) {
                    if (mode < 0 || mode > 4) {
                        log.Error(voiceMgr, "VoiceManager.EncodedFrameSizeForMode: wide-band mode " + mode + " is outside the range of 0-4");
                        return -1;
                    }
                    else
                        // ??? TBD: The same mode for wide and narrow?
                        // This can't be right, methinks.
                        return speexNarrowBandFrameSize[mode] + speexWideBandFrameSize[mode];
                }
                else {
                    if (mode < 0 || mode > 8) {
                        log.Error(voiceMgr, "VoiceManager.EncodedFrameSizeForMode: narrow-band mode " + mode + " is outside the range of 0-8");
                        return -1;
                    }
                    else
                        return speexNarrowBandFrameSize[mode];
                }
            }

            // Return the frame size for the band/mode specified by the
            // first byte of the Speex frame.  If the byte doesn't
            // represent a legal Speex first byte, it returns 0
            public int EncodedFrameSizeFromFirstByte(byte b) {
                if ((b & 0x80) != 0)
                    return EncodedFrameSizeForMode((b & 0x70) >> 4, true);
                else
                    return EncodedFrameSizeForMode((b & 0x78) >> 3, false);
            }

            ///<summary>
            ///    Get a frame of Speex from a playback file into the
            ///    buf supplied, and return the length
            ///</summary>
            public int ReadPlaybackFrame(byte [] buf, bool infiniteLoop) {
                // Read 1 byte, and use it to find the actual frame
                // size
                try {
                    int lastFileOffset = playbackFileOffset;
                    int bytesRead = playbackStream.Read(buf, 0, 1);
                    if (bytesRead == 0) {
                        if (infiniteLoop) {
                            log.DebugFormat(voiceMgr, "BasicChannel.ReadPlaybackFrame: For file '{0}', end of file encountered; restarting playback on file", playbackFile);
                            // Make the playback loop
                            playbackStream.Seek(0, SeekOrigin.Begin);
                            bytesRead = playbackStream.Read(buf, 0, 1);
                        }
                        else {
                            log.DebugFormat(voiceMgr, "BasicChannel.ReadPlaybackFrame: For file '{0}', end of file encountered; closing file", playbackFile);
                            playbackStream.Close();
                            playbackStream = null;
                            playbackFile = "";
                            playbackFileOffset = 0;
                        }
                    }
                    if (bytesRead > 0) {
                        playbackFileOffset += bytesRead;
                        int expectedLength = buf[0];
                        if (expectedLength > 0) {
                            bytesRead = playbackStream.Read(buf, 0, expectedLength);
                            voicePlaybackFrameCounter.Inc();
                            if (voiceMgr.loggingDecode) {
                                log.DebugFormat(voiceMgr, "BasicChannel.ReadPlaybackFrame: file offset {0}, expectedLength {1}, bytesRead {2}, buf {3}", 
                                    lastFileOffset, expectedLength, bytesRead, ByteArrayToHexString(buf, 0, bytesRead));
                            }
                            if (bytesRead != expectedLength)
                                log.ErrorFormat(voiceMgr, "VoiceChannel.ReadPlaybackFrame: Read {0} bytes; got {1} bytes", expectedLength, bytesRead);
                            playbackFileOffset += bytesRead;
                            return bytesRead;
                        }
                        else
                            log.ErrorFormat(voiceMgr, "BasicChannel.ReadPlaybackFrame: At position {0} of file '{1}', illegal first byte {2}; {3}!", 
                                playbackStream.Position, playbackFile, buf[0], ByteArrayToHexString(buf, 0, bytesRead));
                    }
                }
                catch (Exception e) {
                    log.ErrorFormat(voiceMgr, "BasicChannel.ReadPlaybackFrame: Error reading, seeking or closing file '{0}': {1}",
                        playbackFile, e.Message);
                    playbackStream.Close();
                    playbackStream = null;
                }
                return 0;
            }

        }

        ///<summary>
        ///    This class is a BasicChannel that knows how to find a
        ///    microphone device 
        ///</summary>
        public class MicrophoneChannel : BasicChannel, IDisposable {

            ///<summary>
            ///    The windows device number; ignored for now
            ///</summary>
            protected int deviceNumber = 0;

            ///<summary>
            ///    The last position of the microphone stream
            ///</summary>
            protected uint lastMicPos = 0;

            ///<summary>
            ///    This is where the samples from the microphone are
            ///    copied before being fed to the codec.
            ///</summary>
            private short[] micSamples = null;

            ///<summary>
            ///    Maximum number of PCM samples in the
            ///    MicrophoneChannel queue: 4 times as much as for a
            ///    single frame.
            ///</summary>
            protected int micQueueSize;

            ///<summary>
            ///    This buffer holds the 16-bit PCM samples that are
            ///    input to codec encoder prior to handing it off to
            ///    the nextwork.
            ///</summary>
            private short[] encoderInputBuffer = null;

            ///<summary>
            ///    This buffer holds the output of the codec encoder prior
            ///    to handing it off to the nextwork.
            ///</summary>
            private byte[] encoderOutputBuffer = null;

            ///<summary>
            ///    The timer that allows us to collect microphone samples.
            ///</summary>
            private System.Timers.Timer micHandlingTimer;

            ///<summary>
            ///    A count of successive "silent" frames, used to
            ///    trigger deallocation of the voice from the point of
            ///    view of the server and listeners.
            ///</summary>
            private int silentFrameCount = 0;
        
            ///<summary>
            ///    A count of successive "audible" frames after a
            ///    period of silence, used to trigger allocation of
            ///    the voice and restart of transmission of microphone
            ///    frames.
            ///</summary>
            private int audibleFrameCount = 0;

            ///<summary>
            //    5 frames == .1 seconds of non-silence before we
            //    allocate a voice and start transmitting.
            ///</summary>
            public int audibleFrameCountAllocationThreshold = 5;
        
            ///<summary>
            //    The last audibleFrameCountAllocationThreshold
            //    frames, saved up so that we can send them when the
            //    audible threshold is reached.
            ///</summary>
            public List<byte[]> savedFrames = new List<byte[]>();
            
            ///<summary>
            //    Are we generating non-silent voice frames right now?
            ///</summary>
            protected bool micSpeaking = false;

            ///<summary>
            //    Are we permitting the mic to send voice to the network?
            ///</summary>
            protected bool micTransmitting = true;

            ///<summary>
            //    Is Voice Activity Detection enable?
            ///</summary>
            protected bool enableVAD = true;

            ///<summary>
            //    The probability that voice activity has started
            ///</summary>
            protected int VADProbStart = 45;

            ///<summary>
            ///    The instance that aggregates data frames for
            ///    transmission to the server.
            ///</summary>
            DataFrameAggregator dataFrameAggregator = null;

            ///<summary>
            //    True if we're using aggregated data frame messages
            ///</summary>
            protected bool usingDataFrameAggregation = true;

            ///<summary>
            ///   If we're in playback mode, and are waiting to start
            ///   reading the next file, this is the time at which we
            ///   again start playing a file.
            ///</summary>
            protected long timeOfNextPlayback = 0;

            ///<summary>
            //    The random number generator we use to pick playback times.
            ///</summary>
            protected Random playbackRandomGenerator = new Random((int)(System.Environment.TickCount & 0x7FFFFFFF));

            ///<summary>
            ///    Constructor
            ///</summary>
            public MicrophoneChannel(VoiceManager voiceMgr, long playerOid, int deviceNumber, bool recordingWAV, bool recordingSpeex)
                : base(voiceMgr, recordingWAV, recordingSpeex, false)
            {
                ;
                // Will be used when we init the device
                micQueueSize = samplesPerFrame * 8;
                this.deviceNumber = deviceNumber;
                this.oid = playerOid;
                this.sourceActive = false;
                this.sourceReady = false;
                this.queuedSamples = new CircularBuffer(micQueueSize);
                this.title = "mic";
            }

            ///<summary>
            ///    Initialize a new microphone instance, creating the
            ///    FMOD sound object, optionally opening a recording
            ///    stream, and creating and starting a timer instance
            ///    to copy the PCM samples.
            ///</summary>
            public void InitMicrophone(VoiceParmSet parmSet, bool reconfigure) {
                log.DebugFormat(voiceMgr, "MicrophoneChannel.InitMicrophone called with recording WAV({0}), Speex({1})", recordingWAV, recordingSpeex);
                if (channelCodec != null)
                    return;
                try {
                    log.Debug(voiceMgr, "MicrophoneChannel.InitMicrophone: Creating codec");
                    encoderOutputBuffer = new byte[maxBytesPerEncodedFrame];
                    if (voiceMgr.runningVoiceBot) {
                        ApplyVoiceBotParms(parmSet, reconfigure);
                        SetTimeOfNextPlayback();
                    }
                    else {
                        encoderInputBuffer = new short[maxSamplesPerFrame];
                        channelCodec = new SpeexCodec();
                        int encodecFrameSize = channelCodec.InitEncoder(maxSamplesPerFrame, samplesPerFrame, samplesPerSecond);
                        if (encodecFrameSize != samplesPerFrame)
                            log.ErrorFormat(voiceMgr, "MicrophoneChannel.InitMicrophone: The encoder frame size {0} != samplesPerFrame " + samplesPerFrame);
                        ApplyMicrophoneSettings(parmSet, reconfigure);
                        // create a sound object
                        FMOD.RESULT result;
                        FMOD.MODE mode = FMOD.MODE._2D | FMOD.MODE.OPENUSER | FMOD.MODE.SOFTWARE | FMOD.MODE.LOOP_NORMAL;
                        lock(voiceMgr.usingFmod) {
                            FMOD.CREATESOUNDEXINFO  exinfo = new FMOD.CREATESOUNDEXINFO();
                            exinfo.cbsize = Marshal.SizeOf(exinfo);
                            exinfo.decodebuffersize = 0;
                            exinfo.length = (uint)(micQueueSize * 2);
                            exinfo.numchannels = 1;
                            exinfo.defaultfrequency = samplesPerSecond;
                            exinfo.format = FMOD.SOUND_FORMAT.PCM16;

                            log.Debug(voiceMgr, "MicrophoneChannel.InitMicrophone: Creating sound");
                            result = voiceMgr.fmod.createSound((string)null, mode, ref exinfo, ref channelSound);
                        }
                        try {
                            ERRCHECK(result);
                        }
                        catch (Exception e) {
                            log.ErrorFormat(voiceMgr, "MicrophoneChannel.InitMicrophone: Error creating microphone sound: {0}; stack trace\n {1}",
                                e.Message, e.StackTrace);
                        }
                        SetMicDevice();
                        micSamples = new short[micQueueSize];
                        if (recordingWAV || recordingSpeex) {
                            log.Debug(voiceMgr, "MicrophoneChannel.InitMicrophone: Calling StartRecording");
                            StartRecording(channelSound, voiceMgr.LogFolderPath("RecordMic"), recordingWAV, recordingSpeex);
                        }
                        log.Debug(voiceMgr, "MicrophoneChannel.InitMicrophone: Calling StartFmodMicUpdates");
                        StartFmodMicUpdates(channelSound);
                        log.Debug(voiceMgr, "MicrophoneChannel.InitMicrophone: Calling StartChannel");
                        StartChannel(false);
                    }
                    micHandlingTimer = new System.Timers.Timer();
                    micHandlingTimer.Elapsed += MicHandlingTimerTick;
                    micHandlingTimer.Interval = 10; // ms
                    micHandlingTimer.Enabled = true;
                    if (usingDataFrameAggregation) {
                        dataFrameAggregator = new DataFrameAggregator(voiceMgr, deviceNumber, this);
                        dataFrameAggregator.Start();
                    }
                    sourceReady = true;
                }
                catch (Exception e) {
                    log.Error(voiceMgr, "MicrophoneChannel.InitMicrophone: Exception " + e.Message + ", stack trace\n" + e.StackTrace);
                }
            }

            public new void Dispose() {
                disposing = true;
                if (micHandlingTimer != null)
                    micHandlingTimer.Enabled = false;
                if (dataFrameAggregator != null)
                    dataFrameAggregator.Dispose();
                base.Dispose();
            }
            
            ///<summary>
            ///    A class that aggregates data frames to minimize the
            ///    number of network transmissions to the voice server
            ///    for successive data frames.
            ///</summary>
            public class DataFrameAggregator : IDisposable {
                
                protected VoiceManager voiceMgr = null;
                protected int deviceNumber;
                protected MicrophoneChannel micChannel;
                protected long earliestAddTime = 0L;
                protected List<byte[]> pendingDataFrames;
                protected Thread aggregatedDataThread = null;

                // 125 ms from earliest add til send
                protected static int packetAggregationInterval = 125;
                protected static int dataFrameSendThreshold = 6;

                protected bool shuttingDown = false;

                public DataFrameAggregator(VoiceManager voiceMgr, int deviceNumber, MicrophoneChannel micChannel) {
                    this.voiceMgr = voiceMgr;
                    this.deviceNumber = deviceNumber;
                    this.micChannel = micChannel;
                    this.pendingDataFrames = new List<byte[]>();
                }
                
                public void Start() {
                    aggregatedDataThread = new Thread(AggregatedDataFrames);
                    aggregatedDataThread.Start();
                }
                
                public void Dispose() {
                    Monitor.Enter(pendingDataFrames);
                    try {
                        shuttingDown = true;
                        Monitor.PulseAll(pendingDataFrames);
                    }
                    finally {
                        Monitor.Exit(pendingDataFrames);
                    }
                }
                
                public void AddPendingDataFrame(byte[] dataFrame) {
                    if (shuttingDown)
                        return;
//                     if (voiceMgr.loggingMessages)
//                         log.Debug(voiceMgr, "DataFrameAggregator.AddPendingDataFrame: dataFrame: " + ByteArrayToHexString(dataFrame, 0, dataFrame.Length));
                    Monitor.Enter(pendingDataFrames);
                    try {
                        pendingDataFrames.Add(dataFrame);
                        if (earliestAddTime == 0)
                            earliestAddTime = System.Environment.TickCount;
                        MaybeSendAggregatedPackets(false);
                    }
                    finally {
                        Monitor.Exit(pendingDataFrames);
                    }
                }

                public void ForceSendingDataFrames() {
                    Monitor.Enter(pendingDataFrames);
                    try {
                        MaybeSendAggregatedPackets(true);
                    }
                    finally {
                        Monitor.Exit(pendingDataFrames);
                    }
                }

                private void MaybeSendAggregatedPackets(bool force) {
                    long now = System.Environment.TickCount;
                    int cnt = pendingDataFrames.Count;
                    bool sendPackets = (force || cnt >= dataFrameSendThreshold ||
                        (cnt > 0 && (earliestAddTime != 0 && (now - earliestAddTime) >= packetAggregationInterval)));
                    if (sendPackets && cnt > 0) {
//                         if (voiceMgr.loggingMessages)
//                             log.DebugFormat(voiceMgr, "DataFrameAggregator.AggregatedDataPackets: sending {0} frames, now {1}, earliestAddTime {2}",
//                                 pendingDataFrames.Count, now, earliestAddTime);
                        micChannel.SendAggregatedDataMessage((byte)deviceNumber, pendingDataFrames);
                        pendingDataFrames.Clear();
                        earliestAddTime = 0;
                    }
                }
                
                private void AggregatedDataFrames() {
                    Monitor.Enter(pendingDataFrames);
                    try {
                        while (!shuttingDown) {
                            MaybeSendAggregatedPackets(false);
                            try {
                                if (pendingDataFrames.Count > 0) {
                                    long now = System.Environment.TickCount;
                                    int timeLeft = packetAggregationInterval - Math.Min(packetAggregationInterval, (int)(now - earliestAddTime));
                                    Monitor.Wait(pendingDataFrames, timeLeft);
                                }
                                else
                                    Monitor.Wait(pendingDataFrames);
                            }
                            catch (ThreadInterruptedException) {
                                continue;
                            }
                            catch (ThreadAbortException e) {
                                log.ErrorFormat(voiceMgr, "DataFrameAggregator.AggregatedDataFrames: Aborting AggregatedDataPackets thread due to {0}", e);
                                return;
                            }
                        }
                    }
                    finally {
                        Monitor.Exit(pendingDataFrames);
                    }
                }
            }

            protected void SendOrAggregateDataMessage(byte voiceNumber, byte[] msgBytes, int byteCount) {
                if (voiceMgr.voiceManagerShutdown)
                    return;
                if (usingDataFrameAggregation)
                    dataFrameAggregator.AddPendingDataFrame(BufferSubset(msgBytes, 0, byteCount));
                else
                    SendMessage(voiceNumber, VoiceOpcode.Data, msgBytes, byteCount);
            }
            
            protected byte[] BufferSubset(byte[] buf, int startIndex, int byteCount) {
                byte[] msg = new byte[byteCount];
                Array.Copy(buf, startIndex, msg, 0, byteCount);
                return msg;
            }
            
            public void StopRecording() {
                StopChannel(false);
                StopRecording(channelSound);
            }
            
            ///<summary>
            ///    ??? TBD For now, ignore the deviceNumber, and just
            ///    grab the first microphone..  We'll figure out
            ///    something smarter at a later point.
            ///</summary>
            private void SetMicDevice() {
                try {
                    // get input device
                    int numdrivers = 0;
                    int micDevice = 0;
                    FMOD.RESULT result;
                    lock(voiceMgr.usingFmod) {
                        result = voiceMgr.fmod.getRecordNumDrivers(ref numdrivers);
                        ERRCHECK(result);
                        if (numdrivers <= 0)
                            ThrowError("There are no input devices, such as microphones, available");
                        micDevice = 0;
                        result = voiceMgr.fmod.setRecordDriver(micDevice);
                    }
                    ERRCHECK(result);
                    this.title = "mic" + micDevice;
                }
                catch (Exception e) {
                    log.ErrorFormat(voiceMgr, "MicrophoneChannel.SetMicDevice: Exception while setting mic device: {0}", e.Message);
                }
            }
            
            ///<summary>
            ///    Change the transmission state of the microphone,
            ///    used to implement push-to-talk
            ///</summary>
            protected void SetTransmitting(bool transmitting) {
                lock(voiceMgr.usingFmod) {
                    bool formerlyTransmitting = micTransmitting;
                    micTransmitting = transmitting;
                    // If we were transmitting before this call and
                    // not transmitting now, send the deallocate
                    if (formerlyTransmitting && !transmitting) {
                        if (micSpeaking && voiceMgr.connectedToServer)
                            SendDeallocMessage((byte)deviceNumber);
                    }
                    silentFrameCount = 0;
                    audibleFrameCount = 0;
                    micSpeaking = false;
                }
            }
            
            public void SendDeallocMessage(byte voiceNumber) {
                deallocsSentCounter.Inc();
                if (usingDataFrameAggregation)
                    dataFrameAggregator.ForceSendingDataFrames();
                byte msgSize = voiceMsgSize[(int)VoiceOpcode.Deallocate];
                IncSeqNum();
                VoiceMessage msg = new VoiceMessage(msgSize, sequenceNumber, VoiceOpcode.Deallocate, voiceNumber);
                voiceMgr.SendMessageToServer(msg.Bytes);
            }

            ///<summary>
            ///    The tick routine for MicrophoneChannels
            ///
            ///    ??? Need to put the log.Info calls in this and the
            ///    things it calls under some additional debug flag
            ///</summary>
            private void MicHandlingTimerTick(object sender, System.EventArgs e) {
                FMOD.RESULT result;

                if (voiceMgr.voiceManagerShutdown || disposing)
                    return;
                
                if (voiceMgr.runningVoiceBot)
                    HandleBotMicrophonePlayback();
                else if (sourceReady && sourceActive) {
                    try {
                        lock(voiceMgr.usingFmod) {
                            uint micPos = 0;
                            result = voiceMgr.fmod.getRecordPosition(ref micPos);
                            ERRCHECK(result);
                            if (micPos != lastMicPos) {
                                if (voiceMgr.loggingMicTick)
                                    log.Debug(voiceMgr, "MicrophoneChannel.MicHandlingTimerTick micPos " + micPos + " != lastMicPos " + lastMicPos);
                                IntPtr ptr1 = IntPtr.Zero, ptr2 = IntPtr.Zero;
                                uint byteCount1 = 0;
                                uint byteCount2 = 0;
                            
                                int blocklength = (int)micPos - (int)lastMicPos;
                                if (blocklength < 0)
                                    blocklength += micQueueSize;
                                // Lock the sound to get access to the raw data.
                                // 2 = mon 16bit.  1 sample = 2 bytes.
                                channelSound.@lock(lastMicPos * 2, (uint)blocklength * 2, ref ptr1, ref ptr2, ref byteCount1, ref byteCount2);

                                try {
                                    if (ptr1 != IntPtr.Zero && byteCount1 > 0) {
                                        Marshal.Copy(ptr1, micSamples, 0, (int)(byteCount1 >> 1));
                                    }
                                    if (ptr2 != IntPtr.Zero && byteCount2 > 0) {
                                        Marshal.Copy(ptr2, micSamples, (int)(byteCount1 >> 1), (int)(byteCount2 >> 1));
                                    }

                                    // Unlock the sound to allow FMOD to use it again.
                                    channelSound.unlock(ptr1, ptr2, byteCount1, byteCount2);
                                }
                                catch (Exception exception) {
                                    log.Error(voiceMgr, "MicrophoneChannel.MicHandlingTimerTick: got exception " + exception.Message);
                                }
                                int byteCount = (int)(byteCount1 + byteCount2);
                                if (recordingWAV) {
                                    micWAVBytesRecordedCounter.Inc(byteCount);
                                    WriteRecording(recordStreamWAV, micSamples, byteCount);
                                }
                                // Now we have the sounds in a buffer - - run them
                                // through the codec
                                ProcessMicrophoneSamples(micSamples, byteCount);
                                if (voiceMgr.loggingMicTick)
                                    log.DebugFormat(voiceMgr, "MicrophoneChannel.MicHandlingTimerTick: Processed samples;  micPos {0}, lastMicPos {1}, byteCount1 {2}, byteCount2 {3}, byteCount {4}, blocklength {5}",
                                        micPos, lastMicPos, byteCount1, byteCount2, byteCount, blocklength);
                    
                                lastMicPos = micPos;
                            }
                            if (voiceMgr.fmod != null)
                                voiceMgr.fmod.update();
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat(voiceMgr, "MicrophoneChannel.MicHandlingTimerTick: exception raised {0}, stack trace {1}",
                            ex.Message, ex.StackTrace);
                    }
                }
            }

            private void HandleBotMicrophonePlayback() {
//                 log.DebugFormat(voiceMgr, "MicrophoneChannel.HandleBotMicrophonePlayback: Entering playbackFile '{0}', timeOfNextPlayback {1}, System.Environment.TickCount {2}",
//                     playbackFile, timeOfNextPlayback, System.Environment.TickCount);
                lock(voiceMgr) {
                    if (playbackStream != null) {
                        int encodedByteCount = ReadPlaybackFrame(encoderOutputBuffer, false);
                        if (voiceMgr.loggingMicTick)
                            log.DebugFormat(voiceMgr, "MicrophoneChannel.HandleBotMicrophonePlayback: During playback, encodedByteCount {0}", encodedByteCount);
                        if (encodedByteCount > 0) {
                            if (encodedByteCount > maxVoicePacketSize)
                                log.ErrorFormat(voiceMgr, "MicrophoneChannel.HandleBotMicrophonePlayback: encodedbyteCount {0}, but maxVoicePacketSize is {1}",
                                    encodedByteCount, maxVoicePacketSize);
                            else
                                SendOrAggregateDataMessage((byte)deviceNumber, encoderOutputBuffer, encodedByteCount);
                        }
                        else{
                            SendDeallocMessage((byte)deviceNumber); 
                            SetTimeOfNextPlayback();
                        }
                    }
                    // Is it time to start the next file?
                    else if (voiceMgr.playbackFiles != null && timeOfNextPlayback != 0 && System.Environment.TickCount >= timeOfNextPlayback) {
                        timeOfNextPlayback = 0;
                        int count = voiceMgr.playbackFiles.Length;
                        int chosenFileNumber = (int)(count * playbackRandomGenerator.NextDouble());
                        OpenPlaybackStream(voiceMgr.playbackFiles[chosenFileNumber]);
                        SendAllocMessage((byte)deviceNumber, VoiceOpcode.AllocateCodec, oid);
                    }
                }
            }

            private void SetTimeOfNextPlayback() {
                int waitMs = (int)(maxWaitTilNextPlayback * 1000 * playbackRandomGenerator.NextDouble());
                log.DebugFormat(voiceMgr, "MicrophoneChannel.SetTimeOfNextPlayback: maxWait {0}, waitMs {1}", maxWaitTilNextPlayback, waitMs);
                timeOfNextPlayback = System.Environment.TickCount + waitMs;
            }
            
            /// <summary>
            ///      Add the samples supplied to the queue of outgoing
            ///      samples.  Iterate while we have enough samples
            ///      for a frame, encoding the frame; recording it if
            ///      we're in record mode, and handling it off for
            ///      transmission.
            /// </summary>
            private void ProcessMicrophoneSamples(short[] buf, int byteCount) {
                if (disposing)
                    return;
                // captured new samples? if so then encode and distribute them
                if (voiceMgr.loggingMicTick)
                    log.Debug(voiceMgr, "MicrophoneChannel.ProcessMicrophoneSamples: byteCount " + byteCount);
                if (micTransmitting && byteCount > 0) {
                    try {
                        QueuePCMSamples(buf, 0, byteCount >> 1);
                        while (queuedSamples.Used >= samplesPerFrame) {
                            EncodeOnePCMFrame();
                        }
                    }
                    catch (Exception e) {
                        log.Error(voiceMgr, "MicrophoneChannel.ProcessMicrophoneSamples: got exception " + e.Message + ", stack trace " + e.StackTrace);
                    }
                }
            }

            // Returns the power of a signal as a floating point
            // number.  The typical threshold should be between 1.0
            // and 2.0, with the default at 1.5.
            float ComputeFramePower(short[] samples, int numSamples) {
                float powerSum = 0.0f;
                for (int i = 0; i<numSamples; i++) {
                    float amp = (float)samples[i];
                    powerSum += amp * amp;
                }
                return powerSum * 1000f / (32768.0f * 32768.0f * (float)numSamples); 
            }

            private void EncodeOnePCMFrame() {
                int got = queuedSamples.GetSamples(encoderInputBuffer, 0, samplesPerFrame);
                if (got != samplesPerFrame) {
                    log.Error(voiceMgr, "MicrophoneChannel.EncodeOnePCMFrame: queuedSamples.GetSamples returned " +
                        got + ", samplesPerFrame " + samplesPerFrame);
                    return;
                }
                bool speaking = speaking = 0 != channelCodec.PreprocessFrame(encoderInputBuffer);
                if (voiceMgr.loggingMicTick)
                    log.DebugFormat(voiceMgr, "MicrophoneChannel.EncodeOnePCMFrame: usePowerThreshold {0}", usePowerThreshold);
                if (usePowerThreshold) {
                    float power = ComputeFramePower(encoderInputBuffer, samplesPerFrame);
                    speaking = power > framePowerThreshold;
                    if (voiceMgr.loggingMicTick)
                        log.DebugFormat(voiceMgr, "MicrophoneChannel.EncodeOnePCMFrame: power {0}, framePowerThreshold {1}, speaking {2}", 
                            power, framePowerThreshold, speaking);
                }
                int encodedByteCount = channelCodec.EncodeFrame(encoderInputBuffer, encoderOutputBuffer);
                encodedFrameCounter.Inc();
                if (voiceMgr.loggingMicTick)
                    log.Debug(voiceMgr, "MicrophoneChannel.EncodeOnePCMFrame: encodedByteCount " + encodedByteCount + ", speaking " + speaking);
                if (encodedByteCount > 0) {
                    if (encodedByteCount > maxVoicePacketSize)
                        log.Error(voiceMgr, "MicrophoneChannel.EncodeOnePCMFrame: encodedbyteCount " + encodedByteCount +
                            ", but maxVoicePacketSize " + maxVoicePacketSize);
                    else {
                        byte [] buf = BufferSubset(encoderOutputBuffer, 0, encodedByteCount);
                        SaveFrame(buf);
                        // If we're not switching to silent mode, transmit
                        // and maybe record the frame
                        if (!FrameShouldBeSilent(speaking)) {
                            if (recordingSpeex) {
                                micSpeexFramesRecordedCounter.Inc();
                                WriteRecording(recordStreamSpeex, encoderOutputBuffer, 0, encodedByteCount, true);
                            }
                            if (voiceMgr.connectedToServer && micSpeaking) {
                                // Send them upstream
                                SendOrAggregateDataMessage((byte)deviceNumber, encoderOutputBuffer, encodedByteCount);
                            }
                        }
                    }
                } 
            }

            private void SaveFrame(byte[] buf) {
                if (savedFrames.Count >= maxSavedFrames)
                    savedFrames.RemoveAt(0);
                savedFrames.Add(buf);
            }

            ///<summary>
            ///    Maintains the count of silent frames, and decides when
            ///    we should stop transmitting based on the count.  It
            ///    also switches us back to transmitting when enough
            ///    frames on non-silence are received.
            ///</summary>
            private bool FrameShouldBeSilent(bool speaking) {
                if (voiceMgr.loggingDecode)
                    log.DebugFormat(voiceMgr, "MicrophoneChannel.FrameShouldBeSilent: frame silent; silentFrameCount {0}, audibleFrameCount {1}, seqNum {2}",
                        silentFrameCount, audibleFrameCount, sequenceNumber);
                if (!speaking) {
                    silentFrameCount++;
                    silentFrameCounter.Inc();
                    audibleFrameCount = 0;
                    if (silentFrameCount == voiceMgr.silentFrameCountDeallocationThreshold) {
                        if (voiceMgr.connectedToServer && micSpeaking)
                            SendDeallocMessage((byte)deviceNumber); 
                        micSpeaking = false;
                        return true;
                    }
                    return false;
                }
                else {
                    silentFrameCount = 0;
                    audibleFrameCount++;
                    audibleFrameCounter.Inc();
                    if (!micSpeaking) {
                        // Is this is enough activity to cause us to
                        // start transmitting again?
                        if (audibleFrameCount == audibleFrameCountAllocationThreshold) {
                            if (voiceMgr.connectedToServer)
                                // Allocate the voice again, and send
                                // the last few frames.
                                ResumeAudibility();
                            micSpeaking = true;
                        }
                        else
                            // We're waiting for enough audible
                            // samples to queue up, so make this one
                            // silent.
                            return true;
                    }
                }
                // By default, the frame is transmitted
                return false;
            }
            
            ///<summary>
            ///    This sends the accumulated last few frames, many of
            ///    which must have been audible.
            ///</summary>
            public void ResumeAudibility() {
                SendAllocMessage((byte)deviceNumber, VoiceOpcode.AllocateCodec, oid);
                foreach (byte[] frame in savedFrames)
                    SendOrAggregateDataMessage((byte)deviceNumber, frame, frame.Length);
            }
            
            public bool MicSpeaking {
                get {
                    return micSpeaking;
                }
            }

            public bool Transmitting {
                get {
                    return micTransmitting;
                }
                set {
                    if (micTransmitting != value)
                        SetTransmitting(value);
                }
            }
            
        }

        ///<summary>
        ///    This class is a BasicChannel that knows how to decode
        ///    and play Speex frames
        ///</summary>
        public class VoiceChannel : BasicChannel, IDisposable {
            
            ///<summary>
            ///    The voice number supplied by the AllocateCodec
            ///    packet.  This has nothing to do with the FMOD voice
            ///    number whose value we don't need to know.
            ///</summary>
            private byte voiceNumber;
            
            ///<summary>
            ///    Is this sound positional or ambient?
            ///</summary>
            private bool positionalSound;
            
            ///<summary>
            ///    A buffer to hold the bytes of the encoded frame
            ///</summary>
            private byte[] encodedBytes = null;

            ///<summary>
            ///    A buffer to hold the decoded PCM samples of the frame
            ///</summary>
            private short[] decodeBuffer = null;

            ///<summary>
            ///    The jitter buffer's timestamp counter
            ///</summary>
            private uint jitterPutTimestamp = 0;
            private uint jitterGetTimestamp = 0;

            ///<summary>
            ///    The system time at which the speaker last spoke
            ///</summary>
            public long lastSpoke = 0;
            
            ///<summary>
            ///    Read callback delegate
            ///</summary>
            private FMOD.SOUND_PCMREADCALLBACK voiceReadCallback = null;

            ///<summary>
            ///    The volume at which this channel will be played.
            ///    If the number is greater than 1.0, it will be
            ///    applied as a "software preamp", since FMOD can't
            ///    handle volumes greater than 1.0.
            ///</summary>
            protected float playbackVolume = 1.0f;
            
            ///<summary>
            ///    The constructor creates the VoiceChannel object,
            ///    assigning it the external voiceNumber, and finally
            ///    activates the channel.
            ///</summary>
            public VoiceChannel(VoiceManager voiceMgr, long oid, byte voiceNumber, bool positionalSound, bool voicesRecordSpeex, VoiceParmSet parms) 
                : base(voiceMgr, false, voicesRecordSpeex, !positionalSound)
            {
                this.oid = oid;
                this.positionalSound = positionalSound;
                this.voiceNumber = voiceNumber;
                this.channelCodec = new SpeexCodec();
                this.decodeBuffer = new short[playbackQueueSize];
                this.encodedBytes = new byte[maxBytesPerEncodedFrame];
                this.queuedSamples = new CircularBuffer(playbackQueueSize);
                this.voiceReadCallback = new FMOD.SOUND_PCMREADCALLBACK(VoiceChannelReadPCM);
                this.title = "voice" + voiceNumber;
                this.lastSpoke = 0;
                ActivateChannel();
                ApplyVoiceChannelParms(parms, false);
            }
            
            public void ApplyVoiceChannelParms(VoiceParmSet parms, bool reconfigure) {
                log.DebugFormat(voiceMgr, "VoiceChannel.ApplyVoiceChannelParms: voiceMgr.defaultPlaybackVolume {0}", voiceMgr.defaultPlaybackVolume);
                ApplyDecodecSettings(parms, reconfigure);
                SetPlaybackVolume(voiceMgr.defaultPlaybackVolume);
                ApplyJitterBufferSettings(parms, reconfigure);
            }
            
            ///<summary>
            ///    Begin playing back a recorded stream
            ///</summary>
            public void StartPlayback(string fileName) {
                if (OpenPlaybackStream(fileName)) {
                    // Prime the pump by filling up the playback queue
                    for (int i=0; i<playbackQueueFrames - 1; i++)
                        GetPlaybackFrame();
                }
            }
            
            ///<summary>
            ///    Create the FMOD sound associated with the channel,
            ///    and if this channel will be recorded, start the
            ///    recording. 
            ///</summary>
            public void ActivateChannel() {
                if (sourceActive) {
                    log.ErrorFormat(voiceMgr, "VoiceChannel.ActivateChannel, channel {0}/FMOD{1}, channel already active", voiceNumber);
                    return;
                }
                try {
                    log.Debug(voiceMgr, "VoiceChannel.ActivateChannel: Creating codec");
                    channelCodec.InitDecoder(true, samplesPerFrame, samplesPerFrame);
                    FMOD.RESULT result;
                    lock(voiceMgr.usingFmod) {
                        FMOD.MODE twoOrThree = (positionalSound ? FMOD.MODE._3D : FMOD.MODE._2D);
                        FMOD.MODE mode = (twoOrThree | FMOD.MODE.OPENUSER | FMOD.MODE.LOOP_NORMAL | FMOD.MODE.HARDWARE | FMOD.MODE.CREATESTREAM);
                        FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();
                        exinfo.cbsize = Marshal.SizeOf(exinfo);
                        exinfo.decodebuffersize = (uint)(samplesPerFrame * 2 * 2);
                        exinfo.length = (uint)samplesPerFrame * 2;
                        exinfo.numchannels = 1;
                        exinfo.defaultfrequency = samplesPerSecond;
                        exinfo.format = FMOD.SOUND_FORMAT.PCM16;
                        exinfo.pcmreadcallback = voiceReadCallback;
                        log.Debug(voiceMgr, "VoiceChannel.ActivateChannel: Creating sound");
                        result = voiceMgr.fmod.createSound((string)null, mode, ref exinfo, ref channelSound);
                    }
                    ERRCHECK(result);
                    if (recordingWAV || recordingSpeex) {
                        if (!voiceMgr.recordedOids.Contains(oid)) {
                            File.Delete(voiceMgr.LogFolderPath("RecordVoice-" + oid + ".wav"));
                            File.Delete(voiceMgr.LogFolderPath("RecordVoice-" + oid + ".speex"));
                            voiceMgr.recordedOids.Add(oid);
                        }
                        StartRecording(channelSound, voiceMgr.LogFolderPath("RecordVoice-" + oid), recordingWAV, recordingSpeex);
                    }
                    log.Debug(voiceMgr, "VoiceChannel.ActivateChannel: Calling StartChannel");
                    StartChannel(true);
                }
                catch (Exception e) {
                    log.ErrorFormat(voiceMgr, "VoiceChannel.ActivateChannel: Error activating channel {0}; exception : {1}", voiceNumber, e.Message);
                }
            }

            public float GetPlaybackVolume() {
                return playbackVolume;
            }

            protected void SetPlaybackVolume(VoiceParm parm) {
                playbackVolume = parm.ret.fValue;
                FMOD.RESULT result = SetPlaybackVolumeInternal(Math.Min(1.0f, playbackVolume));
                CheckRetCode(parm, (int)result);
                log.DebugFormat(voiceMgr, "VoiceChannel.SetPlaybackVolume: playbackVolume {0}", playbackVolume);
            }

            public void SetPlaybackVolume(float level) {
                CheckRetCode("VoiceChannel.SetPlaybackVolume", SetPlaybackVolumeInternal(level));
            }

            protected FMOD.RESULT SetPlaybackVolumeInternal(float level) {
                lock(voiceMgr.usingFmod) {
                    return channel.setVolume(level);
                }
            }

            ///<summary>
            ///    Add the incoming frame to the jitter buffer.
            ///</summary>
            public void ProcessIncomingFrame(byte[] encodedFrame, int startIndex, int encodedByteCount) {
                if (recordingSpeex) {
                    voicesSpeexFramesRecordedCounter.Inc();
                    WriteRecording(recordStreamSpeex, encodedFrame, startIndex, encodedByteCount, true);
                }
//                 if (loggingDecode)
//                     log.Debug(voiceMgr, "VoiceChannel.ProcessIncomingFrame: frame len " + encodedByteCount + ", msg " + ByteArrayToHexString(encodedFrame, startIndex, encodedByteCount));
                channelCodec.JitterBufferPut(encodedFrame, startIndex, (uint)encodedByteCount, jitterPutTimestamp);
                jitterPutTimestamp += (uint)samplesPerFrame;
                if ((int)jitterPutTimestamp - (int)jitterGetTimestamp < 2 * samplesPerFrame) {
                    jitterAdjustCounter.Inc();
                    if (voiceMgr.loggingDecode)
                        log.DebugFormat(voiceMgr, "VoiceChannel.ProcessIncomingFrame, adjusting jitterPutTimestamp: voice {0}, jitterPutTimestamp {1}, jitterGetTimestamp {2}, new jitterPutTimestamp {3}",
                            voiceNumber, jitterPutTimestamp, jitterGetTimestamp, jitterGetTimestamp + 2 * (uint)samplesPerFrame);
                    jitterPutTimestamp = jitterGetTimestamp + 4 * (uint)samplesPerFrame;
                }
                if (voiceMgr.loggingDecode)
                    log.DebugFormat(voiceMgr, "VoiceChannel.ProcessIncomingFrame: voice {0}, startIndex {1}, encodedByteCount {2}, jitterPutTimestamp {3}",
                        voiceNumber, startIndex, encodedByteCount, jitterPutTimestamp);
            }
            
            ///<summary>
            ///    Process an aggregated data packet, handing off each
            ///    subbuf to ProcessIncomingFrame.
            ///</summary>
            public void ProcessAggregatedData(byte dataFrameCount, byte[] aggregatedBuf, int startIndex, int bufSize) {
                aggregatedDataReceivedCounter.Inc();
                int currentIndex = startIndex;
                for (int frame=0; frame<dataFrameCount; frame++) {
                    int encodedByteCount = (int)aggregatedBuf[currentIndex];
                    currentIndex++;
//                     if (loggingDecode)
//                         log.DebugFormat(voiceMgr, "VoiceChannel.ProcessAggregatedData: startIndex {0}, bufSize {1}, currentIndex {2}, encodedByteCount {3}, buf {4}",
//                             startIndex, bufSize, currentIndex, encodedByteCount, ByteArrayToHexString(aggregatedBuf, currentIndex, encodedByteCount));
                    ProcessIncomingFrame(aggregatedBuf, currentIndex, encodedByteCount);
                    currentIndex += encodedByteCount;
                }
                if (currentIndex  - voiceMsgSize[(int)VoiceOpcode.AggregatedData] != bufSize)
                    log.ErrorFormat(voiceMgr, "VoiceChannel.ProcessAggregatedData: Error: indices don't match! voice number {0}, processing {1} subbufs, currentIndex {2}, bufSize {3}",
                        voiceNumber, dataFrameCount, currentIndex, bufSize);
                else
                    log.DebugFormat(voiceMgr, "VoiceChannel.ProcessAggregatedData: For voice number {0}, processed {1} subbufs", voiceNumber, dataFrameCount);
            }

            ///<summary>
            ///    Fetch a frame worth of PCM shorts.  Since the card
            ///    doesn't always ask for a multiple of the frame
            ///    size, we need to queue the samples until we have
            ///    enough.
            ///</summary>
            public int GetJitterFrame(IntPtr data, int shortCount) {
                int startOffset = 0;
                while (queuedSamples.Used < shortCount) {
                    channelCodec.JitterBufferGet(decodeBuffer, jitterGetTimestamp, ref startOffset);
                    if (voiceMgr.loggingDecode) {
                        int len = channelCodec.EncodedJitterFrameLength;
                        log.Debug(voiceMgr, "VoiceChannel.GetJitterFrame: frame err code " + channelCodec.EncodedJitterFrameErrorCode +
                            ", len " + len + ", msg " + ByteArrayToHexString(channelCodec.EncodedJitterFrame, 0, len));
                        log.DebugFormat(voiceMgr, "VoiceChannel.GetJitterFrame: voice {0}, jitterPutTimestamp {1} jitterGetTimestamp {2} startOffset {3}",
                            voiceNumber, jitterPutTimestamp, jitterGetTimestamp, startOffset);
                    }
                    jitterGetTimestamp += (uint)samplesPerFrame;
                    decodedFrameCounter.Inc();
                    if (playbackVolume > 1.0f)
                        ScalePCMSamples(playbackVolume, decodeBuffer, 0, samplesPerFrame);
                    QueuePCMSamples(decodeBuffer, 0, samplesPerFrame);
                }
                return queuedSamples.GetSamples(data, shortCount);
            } 
            
            ///<summary>
            ///    This method fills up the sound with samples queued
            ///    up from the network.
            ///</summary>
            public FMOD.RESULT VoiceChannelReadPCM(IntPtr soundraw, IntPtr data, uint byteCount) {
                /* lock(voiceMgr.usingFmod) */ {
                    int wantCount = (int)(byteCount >> 1);
                    int gotCount = GetJitterFrame(data, wantCount);
                    int remaining = wantCount - gotCount;
                    if (voiceMgr.loggingDecode)
                        log.DebugFormat(voiceMgr, "VoiceChannel.VoiceChannelReadPCM: soundraw {0}, data {1}, byteCount {2}, wantCount {3}, gotCount {4}, remaining {5}",
                            soundraw, data, byteCount, wantCount, gotCount, remaining);
                    if (gotCount > 0) {
                        if (recordStreamWAV != null) {
                            int rest = (wantCount - remaining) * sampleSize;
                            WriteRecording(recordStreamWAV, data, rest);
                        }
                        // If there is any remaining, that means we've run out
                        // of samples, so insert some silence
                        if (remaining > 0) {
                            Marshal.Copy(voiceMgr.noSoundShorts, 0, data, remaining);
                            if (recordingWAV)
                                WriteRecording(recordStreamWAV, voiceMgr.noSoundShorts, remaining * sampleSize);
                        }
                        // If we're playing back from a file, read the
                        // number of samples we used from the file
                        //
                        // ??? TBD The file reading should be happening on
                        // a different thread
                        if (playbackStream != null) {
                            GetPlaybackFrame();
                        }
                    }
                    return FMOD.RESULT.OK;
                }
            }

            protected void GetPlaybackFrame() {
                int bytesRead = ReadPlaybackFrame(encodedBytes, true);
                ProcessIncomingFrame(encodedBytes, 0, bytesRead);
            }
            
        }

    }

}
