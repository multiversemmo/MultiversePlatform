#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using log4net;
using Multiverse.Lib.LogUtil;
using Axiom.MathLib;

using SpeexWrapper;

#endregion


namespace Multiverse.Voice
{
    public enum ConstructorParm {
        // byte value: default first mic
        MicDeviceNumber = 1,
        // bool; default true
        UseTcp = 2,
        // bool; default false
        MicRecordWAV = 3,
        // bool; default false
        MicRecordSpeex = 4,
        // bool; default false
        VoicesRecordSpeex = 5,
        // bool; default false
        ListenToYourself = 6,
        // string; default localhost
        VoiceServerHost = 7,
        // int; default 5051
        VoiceServerPort = 8,
        // long: player oid; default 0
        PlayerOid = 9,
        // string: default "-4130"
        AuthenticationToken = 10,
        // bool; default true
        ConnectToServer = 11,
        // int; default 8
        MaxRecentSpeakers = 12,
        // int; default 5000
        TcpConnectTimeout = 13,
        // byte value: default first playback device
        PlaybackDeviceNumber = 14,
        // float; default 20
        MinAttenuation = 15,
        // float; default 1000
        MaxAttenuation = 16,
        // float; default 10
        AttenuationRolloff = 17,
    }

    ///<summary>
    ///    The one playback parameter
    ///</summary>
    public enum PlaybackParm {
        // float value: 0.0 - 1.0
        DefaultVolume = 1
    }
    
    ///<summary>
    ///    The one group parameter
    ///</summary>
    public enum GroupParm {
        // long value
        GroupOid = 1
    }
    
    public enum VoiceBotParm {
        // int; default 60
        MaxWaitTilNextPlayback = 1,
    }
    
    public enum TransmissionParm {
        // int; default 5
        MaxSavedFrames = 1,
        UsePowerThreshold = 2,
        FramePowerThreshold = 3,
    }

    public enum VoiceParmKind {
        Constructor = 1,
        Encodec = 2,
        Decodec = 3,
        Preprocessor = 4,
        JitterBuffer = 5,
        Playback = 6,
        Group = 7,
        VoiceBot = 8,
        Transmission = 9,
    }

    public enum ValueKind {
        Int = 1,
        Long = 2,
        Bool = 3,
        Float = 4,
        String = 5
    }
        
    public class ParmStatus {
        // The empty string if the parm is valid, and an error
        // string if it is not
        public string errorMessage = "";
        public int iValue = 0;
        public long lValue = 0;
        public bool bValue = false;
        public float fValue = 0f;
        public string sValue;
        public VoiceParm parm = null;
    }
        
    // The data that allows us to decode parameters
    public class VoiceParm {
        public VoiceParmKind kind;
        public string name;
        public string description;
        public ValueKind valueKind;
        public Object value;
        // True if it's a commonly-used parm
        public bool common;
        // Min and max apply only to int and float parameters
        public int imin;
        public int imax;
        public float fmin;
        public float fmax;

        public int ctlIndex;
        public ParmStatus ret;

        public VoiceParm(VoiceParmKind kind, string name, string description, ValueKind valueKind, Object value, int index, bool common, int min, int max) {
            this.kind = kind;
            this.name = name;
            this.description = description;
            this.valueKind = valueKind;
            this.value = value;
            this.common = common;
            this.imin = min;
            this.imax = max;
            this.ctlIndex = index;
        }

        public VoiceParm(VoiceParmKind kind, string name, string description, ValueKind valueKind, Object value, int index, bool common, float min, float max) {
            this.kind = kind;
            this.name = name;
            this.description = description;
            this.valueKind = valueKind;
            this.value = value;
            this.common = common;
            this.fmin = min;
            this.fmax = max;
            this.ctlIndex = index;
        }

        public VoiceParm(VoiceParm other) {
            this.kind = other.kind;
            this.name = other.name;
            this.description = other.description;
            this.valueKind = other.valueKind;
            this.value = other.value;
            this.imin = other.imin;
            this.imax = other.imax;
            this.fmin = other.fmin;
            this.fmax = other.fmax;
            this.ctlIndex = other.ctlIndex;
            this.ret = other.ret;
        }

    }
        
    // Used to represent parameters, and default sets of parameters
    public class VoiceParmSet {

        public Dictionary<VoiceParmKind, Dictionary<int, VoiceParm>> voiceKindParms = new Dictionary<VoiceParmKind, Dictionary<int, VoiceParm>>();
        public Dictionary<string, VoiceParm> voiceParmsByName = new Dictionary<string, VoiceParm>();

        ///<summary>
        ///    The parameter instance that holds the default values of
        ///    parameters
        ///</summary>
        public static VoiceParmSet defaultVoiceParms = MakeDefaultParms();

        public static VoiceParmSet MakeDefaultParms() {
            VoiceParmSet parmSet = new VoiceParmSet();
            parmSet.AddDefaultParameters();
            return parmSet;
        }

        public VoiceParmSet() {
        }

        public VoiceParmSet(Object[] parmArray) {
            int len = parmArray.Length;
            if ((len & 1) != 0) {
                log.Error("VoiceParmSet: Odd number of parms " + parmArray);
                len--;
            }
            for (int i=0; i<len; i+=2) {
                string name = parmArray[i].ToString();
                string value = parmArray[i+1].ToString();
                VoiceParm defaultParm = defaultVoiceParms.GetNamedParm(name);
                if (defaultParm == null) {
                    log.Error("VoiceParmSet: no default parm for '" + name + "'");
                    continue;
                }
                VoiceParm parmCopy = new VoiceParm(defaultParm);
                CheckValidParm(parmCopy, value);
                if (parmCopy.ret.errorMessage != "")
                    log.Error(parmCopy.ret.errorMessage);
                else {
                    parmCopy.value = value;
                    Add(parmCopy);
                }
            }
        }        

        public VoiceParm GetNamedParm(string name) {
            VoiceParm parm;
            if (voiceParmsByName.TryGetValue(name, out parm))
                return parm;
            else
                return null;
        }

        public List<VoiceParm> GetParmsOfKind(VoiceParmKind kind) {
            Dictionary<int, VoiceParm> parms;
            if (voiceKindParms.TryGetValue(kind, out parms))
                return new List<VoiceParm>(parms.Values);
            else
                return null;
        }
        
        public List<VoiceParm> GetParmsOfKindOrDefault(VoiceParmKind kind, bool reconfigure) {
            List<VoiceParm> parms = GetParmsOfKind(kind);
            List<VoiceParm> defaultOfKind = defaultVoiceParms.GetParmsOfKind(kind);
            if (parms == null) {
                if (reconfigure)
                    return new List<VoiceParm>();
                else
                    return new List<VoiceParm>(defaultOfKind);
            }
            List<VoiceParm> returnedParms = new List<VoiceParm>();
            foreach (VoiceParm defParm in defaultOfKind) {
                VoiceParm parm = GetNamedParm(defParm.name);
                if (parm != null) {
//                     log.InfoFormat("VoiceParmSet.GetParmsOfKindOrDefault: defParm.name {0} parm.name {1} parm.value {2}, parm.ret.lValue {3}",
//                             defParm.name, parm.name, parm.value, parm.ret.lValue);
                    returnedParms.Add(parm);
                }
                else if (!reconfigure)
                    returnedParms.Add(defParm);
            }
            return returnedParms;
        }
        
        public void Add(VoiceParmKind kind, string name, string description, ValueKind valueKind, Object value, int index, bool common) {
            switch (valueKind) {
            case ValueKind.Int:
                Add(kind, name, description, valueKind, value, index, common, Int32.MinValue, Int32.MaxValue);
                break;
            case ValueKind.Float:
                Add(kind, name, description, valueKind, value, index, common, Single.MinValue, Single.MaxValue);
                break;
            default:
                Add(kind, name, description, valueKind, value, index, common, 0, 0);
                break;
            }
        }
            
        public void Add(VoiceParmKind kind, string name, string description, ValueKind valueKind, Object value, int index, bool common, int min, int max) {
            VoiceParm newParm = new VoiceParm(kind, name, description, valueKind, value, index, common, min, max);
            Add(newParm);
        }
        
        public void Add(VoiceParmKind kind, string name, string description, ValueKind valueKind, Object value, int index, bool common, float min, float max) {
            VoiceParm newParm = new VoiceParm(kind, name, description, valueKind, value, index, common, min, max);
            Add(newParm);
        }
        
        public void Add(VoiceParm newParm) {
            Dictionary<int, VoiceParm> parms;
            if (!voiceKindParms.TryGetValue(newParm.kind, out parms)) {
                parms = new Dictionary<int, VoiceParm>();
                voiceKindParms[newParm.kind] = parms;
            }
            VoiceParm parm;
            if (parms.TryGetValue(newParm.ctlIndex, out parm))
                log.ErrorFormat("VoiceParmCollection.Add: for parameter {0} of kind {1} and index {2}, the index is already in the kind collection",
                    newParm.name, newParm.kind, newParm.ctlIndex);
            else if (voiceParmsByName.TryGetValue(newParm.name, out parm))
                log.ErrorFormat("VoiceParmCollection.Add: parameter {0} of kind {1} is already the name index",
                    newParm.name, newParm.kind);
            else {
                parm = newParm;
                parms[newParm.ctlIndex] = parm;
                // Also, add it to the name dictionary
                voiceParmsByName[newParm.name] = parm;
                CheckValidParm(parm, parm.value.ToString());
                if (parm.ret.errorMessage != "")
                    log.Error("VoiceParmSet.Add: " + parm.ret.errorMessage);
            }
        }

        public ParmStatus CheckValidParm(VoiceParm parm, string value) {
            //log.InfoFormat("VoiceParmCollection.CheckValidParm: for parameter {0}, value is {1}", parm.name, value);
            ParmStatus ret = new ParmStatus();
            parm.ret = ret;
            if (parm == null) {
                ret.errorMessage = "There is no parameter named '" + parm.name + "'";
                return ret;
            }
            switch (parm.valueKind) {

            case ValueKind.Int:
                try {
                    ret.iValue = Int32.Parse(value);
                }
                catch (Exception) {
                    ret.errorMessage = string.Format("Int parameter '{0}' value '{1}' can't be parsed",
                        parm.name, value);
                    return ret;
                }
                if (ret.iValue < parm.imin || ret.iValue > parm.imax) {
                    ret.errorMessage = string.Format("Int parameter '{0}' value '{1}' is outside the range from {2} to {3}",
                        parm.name, ret.iValue, parm.imin, parm.imax);
                    return ret;
                }
                break;

            case ValueKind.Long:
                try {
                    ret.lValue = Int64.Parse(value);
                }
                catch (Exception) {
                    ret.errorMessage = string.Format("Int parameter '{0}' value '{1}' can't be parsed",
                        parm.name, value);
                    return ret;
                }
                break;

            case ValueKind.Float:
                try {
                    ret.fValue = Single.Parse(value);
                }
                catch (Exception) {
                    ret.errorMessage = string.Format("Float parameter '{0}' value '{1}' can't be parsed",
                        parm.name, value);
                    return ret;
                }
                if (ret.fValue < parm.fmin || ret.fValue > parm.fmax) {
                    ret.errorMessage = string.Format("Float parameter '{0}' value '{1}' is outside the range from {2} to {3}",
                        parm.name, ret.fValue, parm.fmin, parm.fmax);
                    return ret;
                }
                break;
                
            case ValueKind.Bool:
                try {
                    ret.bValue = Boolean.Parse(value);
                }
                catch (Exception) {
                    ret.errorMessage = string.Format("Boolean parameter '{0}' value '{1}' can't be parsed",
                        parm.name, value);
                    return ret;
                }
                break;

            case ValueKind.String:
                ret.sValue = value;
                break;

            }
            return ret;
        }
        
        public static string StringValue(VoiceParm parm) {
            if (parm.ret != null) {
                switch (parm.valueKind) {
                case ValueKind.Int:
                    return parm.ret.iValue.ToString();
                case ValueKind.Long:
                    return parm.ret.lValue.ToString();
                case ValueKind.Float:
                    return parm.ret.fValue.ToString();
                case ValueKind.Bool:
                    return (parm.ret.bValue ? "true" : "false");
                case ValueKind.String:
                    return parm.ret.sValue;
                default:
                    log.ErrorFormat("VoiceParm.StringValue: For parm '{0}', unknown valueKind {1}",
                        parm.name, parm.valueKind);
                    return "None";
                }
            }
            log.ErrorFormat("VoiceParm.StringValue: For parm '{0}', ret is null!", parm.name);
            return "None";
        }

        public void AddDefaultParameters() {
            // Constructor parameters
            Add(VoiceParmKind.Constructor, "mic_device_number", "device number of the microphone device",
                ValueKind.Int, 0, (int)ConstructorParm.MicDeviceNumber, false, 0, 255);
            Add(VoiceParmKind.Constructor, "use_tcp", "true if using TCP; false otherwise",
                ValueKind.Bool, true, (int)ConstructorParm.UseTcp, false);
            Add(VoiceParmKind.Constructor, "mic_record_wav", "true if recording mic output in WAV format",
                ValueKind.Bool, false, (int)ConstructorParm.MicRecordWAV, false);
            Add(VoiceParmKind.Constructor, "mic_record_speex", "true if recording mic output in Speex format",
                ValueKind.Bool, false, (int)ConstructorParm.MicRecordSpeex, false);
            Add(VoiceParmKind.Constructor, "voices_record_speex", "true if recording incoming voices in Speex format", 
                ValueKind.Bool, false, (int)ConstructorParm.VoicesRecordSpeex, false);
            Add(VoiceParmKind.Constructor, "listen_to_yourself", "true if your speech should be sent back to you",
                ValueKind.Bool, false, (int)ConstructorParm.ListenToYourself, true);
            Add(VoiceParmKind.Constructor, "voice_server_host", "the host name of the voice server",
                ValueKind.String, "localhost", (int)ConstructorParm.VoiceServerHost, true);
            Add(VoiceParmKind.Constructor, "voice_server_port", "the port name of the voice server",
                ValueKind.Int, 5051, (int)ConstructorParm.VoiceServerPort, true);
            Add(VoiceParmKind.Constructor, "player_oid", "oid of the player",
                ValueKind.Long, 0, (int)ConstructorParm.PlayerOid, true);
            Add(VoiceParmKind.Constructor, "authentication_token", "the authentication token to be sent to the voice server",
                ValueKind.String, "-4130", (int)ConstructorParm.AuthenticationToken, false);
            Add(VoiceParmKind.Constructor, "connect_to_server", "false if you're just playing back recorded files",
                ValueKind.Bool, true, (int)ConstructorParm.ConnectToServer, false);
            Add(VoiceParmKind.Constructor, "max_recent_speakers", "the number of recent speakers to keep track of",
                ValueKind.Int, 8, (int)ConstructorParm.MaxRecentSpeakers, false);
            Add(VoiceParmKind.Constructor, "tcp_connect_timeout", "the number milliseconds before giving up on a TCP connect",
                ValueKind.Int, 5000, (int)ConstructorParm.TcpConnectTimeout, false, 0, Int32.MaxValue);
            Add(VoiceParmKind.Constructor, "playback_device_number", "device number of the playback device",
                ValueKind.Int, 0, (int)ConstructorParm.PlaybackDeviceNumber, false, 0, 255);
            Add(VoiceParmKind.Constructor, "min_attenuation", "The minimum range of positional voice, in meters",
                ValueKind.Float, 15.0f, (int)ConstructorParm.MinAttenuation, false, 0.0f, 1000.0f);
            Add(VoiceParmKind.Constructor, "max_attenuation", "The maximum range of positional voice, in meters",
                ValueKind.Float, 1000.0f, (int)ConstructorParm.MaxAttenuation, false, 0.0f, 1000.0f);
            Add(VoiceParmKind.Constructor, "attenuation_rolloff", "The rolloff rate of positional voice",
                ValueKind.Float, 10.0f, (int)ConstructorParm.AttenuationRolloff, false, 0.0f, 1000.0f);

            // Group parameters
            Add(VoiceParmKind.Group, "group_oid", "oid of the group",
                ValueKind.Long, 1, (int)GroupParm.GroupOid, true);
            
            // Encodec parameters
            Add(VoiceParmKind.Encodec, "complexity", "how much CPU time will be put into encoding; higher is better",
                ValueKind.Int, 3, (int)SpeexCtlCode.SPEEX_SET_COMPLEXITY, false, 1, 10);
            Add(VoiceParmKind.Encodec, "quality", "with sampling_rate, determines codec bit rate; higher is better",
                ValueKind.Int, 4, (int)SpeexCtlCode.SPEEX_SET_QUALITY, false, 1, 10);
            Add(VoiceParmKind.Encodec, "sampling_rate", "the number of samples per second", 
                ValueKind.Int, 8000, (int)SpeexCtlCode.SPEEX_SET_SAMPLING_RATE, false);
            Add(VoiceParmKind.Encodec, "silence_dealloc_time", "the number of seconds of vad silence before the channel is deallocated",
                ValueKind.Float, 1.0f, 0, true);

            // Decodec parameters
            Add(VoiceParmKind.Decodec, "perceptual_enhancement", "1 if the decodec does 'perceptual enhancement'; 0 if not",
                ValueKind.Int, 1, (int)SpeexCtlCode.SPEEX_SET_ENH, false, 0, 1);

            // Playback parms
            Add(VoiceParmKind.Playback, "default_volume", "the default volume at which voice channels are played back",
                ValueKind.Float, 0.5f, (int)PlaybackParm.DefaultVolume, true, 0.0f, 10.0f);

            // Preprocessor parameters
            Add(VoiceParmKind.Preprocessor, "denoise_enable", "true if the preprocessor should filter out noise",
                ValueKind.Bool, true, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_DENOISE, false);
            Add(VoiceParmKind.Preprocessor, "noise_suppress", "maximum attenuation of the noise in dB (negative number)",
                ValueKind.Int, -15, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_NOISE_SUPPRESS, true, -100, 0);

            Add(VoiceParmKind.Preprocessor, "agc_enable", "true if preprocessor Automatic Gain Control is enabled",
                ValueKind.Bool, true, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC, false);
            Add(VoiceParmKind.Preprocessor, "agc_level", "preprocessor Automatic Gain Control level",
                ValueKind.Int, 5, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC_LEVEL, true, 1, 20);
//             Add(VoiceParmKind.Preprocessor, "agc_increment", "maximal gain increase in dB/second (int32)",
//                 ValueKind.Int, 12, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC_INCREMENT, false, 0, 100);
//             Add(VoiceParmKind.Preprocessor, "agc_decrement", "maximal gain decrease in dB/second (int32)",
//                 ValueKind.Int, 40, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC_DECREMENT, false, 0, 100);
//             Add(VoiceParmKind.Preprocessor, "agc_max_gain", "maximal gain in dB (int32)",
//                 ValueKind.Int, 30, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_AGC_MAX_GAIN, true, 0, 100);

            Add(VoiceParmKind.Preprocessor, "vad_enable", "true if preprocessor voice activity detection is enabled", 
                ValueKind.Bool, true, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_VAD, false);
            Add(VoiceParmKind.Preprocessor, "vad_prob_start", "probability for VAD to go from silence to voice",
                ValueKind.Int, 35, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_PROB_START, true, 0, 100);
            Add(VoiceParmKind.Preprocessor, "vad_prob_continue", "probability for VAD to stay in the voice state",
                ValueKind.Int, 20, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_PROB_CONTINUE, true, 0, 100);

            Add(VoiceParmKind.Preprocessor, "echo_suppress", "maximum attenuation of the residual echo in dB",
                ValueKind.Int, -40, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_ECHO_SUPPRESS, false, -100, 0);
            Add(VoiceParmKind.Preprocessor, "echo_suppress_active", "maximum attenuation of the residual echo in dB when near end is active",
                ValueKind.Int, -15, (int)PreprocessCtlCode.SPEEX_PREPROCESS_SET_ECHO_SUPPRESS_ACTIVE, false, -100, 0);
                
            Add(VoiceParmKind.Transmission, "max_saved_frames", "the number of recent frames saved and sent when mic becomes audible",
                ValueKind.Int, 5, (int)TransmissionParm.MaxSavedFrames, false, 0, 500);
            Add(VoiceParmKind.Transmission, "use_power_threshold", "if true, use the frame_power_threshold rather than the preprocessor VAD probabilities; defaulting is true",
                ValueKind.Bool, true, (int)TransmissionParm.UsePowerThreshold, false);
            Add(VoiceParmKind.Transmission, "frame_power_threshold", "the value of the squared power of the mic frame above which a player is speaking; 0.5 to 3.0, defaulting to 1.5",
                ValueKind.Float, 1.25f, (int)TransmissionParm.FramePowerThreshold, false, 0.5f, 3.0f);
                
            Add(VoiceParmKind.JitterBuffer, "jitter_margin", "how many frames to keep in the buffer (lower bound)",
                ValueKind.Int, 0, (int)JitterBufferCtlCode.JITTER_BUFFER_SET_MARGIN, false, 0, 10);
            Add(VoiceParmKind.JitterBuffer, "jitter_delay_step", "size of the steps when adjusting buffering (timestamp units)",
                ValueKind.Int, 160, (int)JitterBufferCtlCode.JITTER_BUFFER_SET_DELAY_STEP, false, 0, 10000);
            Add(VoiceParmKind.JitterBuffer, "jitter_concealment_size", "size of the packet loss concealment 'units'",
                ValueKind.Int, 160, (int)JitterBufferCtlCode.JITTER_BUFFER_SET_CONCEALMENT_SIZE, false, 0, 10000);
            Add(VoiceParmKind.JitterBuffer, "jitter_max_late_rate", "absolute max amount of loss; typical loss should be halfor less",
                ValueKind.Int, 4, (int)JitterBufferCtlCode.JITTER_BUFFER_SET_MAX_LATE_RATE, false, 0, 100);
            Add(VoiceParmKind.JitterBuffer, "jitter_late_cost", "equivalent cost of one percent late packet in timestamp units",
                ValueKind.Int, 0, (int)JitterBufferCtlCode.JITTER_BUFFER_SET_LATE_COST, false,  0, 100);

            Add(VoiceParmKind.VoiceBot, "max_wait_til_next_playback", "seconds waiting between playing voice bot sounds", 
                ValueKind.Int, 60, (int)VoiceBotParm.MaxWaitTilNextPlayback, false,  0, 1000);

        }

        public string MakeHelpString(bool common) {
            StringBuilder builder = new StringBuilder();
            try {
                foreach (KeyValuePair<string, VoiceParm> entry in voiceParmsByName) {
                    string name = entry.Key;
                    VoiceParm parm = entry.Value;
                    if (common && !parm.common)
                        continue;
                    builder.Append(string.Format("{0}: {1} default {2}",
                            name, valueKindNames[(int)parm.valueKind], parm.value.ToString()));
                    if (parm.valueKind == ValueKind.Int && (parm.imin != Int32.MinValue || parm.imax != Int32.MaxValue))
                        builder.Append(string.Format(" min {0} max {1}", parm.imin, parm.imax));
                    else if (parm.valueKind == ValueKind.Float && (parm.fmin != Int32.MinValue || parm.fmax != Int32.MaxValue))
                        builder.Append(string.Format(" min {0} max {1}", parm.fmin, parm.fmax));
                    builder.Append(string.Format(" ({0})\n", parm.description));
                }
            }
            catch (Exception e) {
                log.Error("VoiceParmSet.MakeHelpString: exception " + e.Message + "; Stack trace\n" + e.StackTrace.ToString());
            }
            return builder.ToString();
        }
        
        public static string[] valueKindNames = new string[] { "", "int", "long", "bool", "float", "string" };
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(VoiceParmSet));
    }

}

