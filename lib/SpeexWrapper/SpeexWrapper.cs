#region Using directives

using System.Runtime.InteropServices;
using System;

// using System;
// using System.IO;
// using System.Collections.Generic;
// using System.Text;
// using System.Runtime.InteropServices;
// using System.Diagnostics;

#endregion Using directives

namespace SpeexWrapper
{

    public enum SpeexCtlCode {
        // Set enhancement on/off (decoder only)
        SPEEX_SET_ENH=0,
        // Get enhancement state (decoder only)
        SPEEX_GET_ENH=1,
        // Obtain frame size used by encoder/decoder
        SPEEX_GET_FRAME_SIZE=3,
        // Set quality value
        SPEEX_SET_QUALITY=4,
        // Get current quality setting
        // SPEEX_GET_QUALITY=5 -- Doesn't make much sense, does it? */,
        // Set sub-mode to use
        SPEEX_SET_MODE=6,
        // Get current sub-mode in use
        SPEEX_GET_MODE=7,

        // Set low-band sub-mode to use (wideband only
        SPEEX_SET_LOW_MODE=8,
        // Get current low-band mode in use (wideband only
        SPEEX_GET_LOW_MODE=9,

        // Set high-band sub-mode to use (wideband only
        SPEEX_SET_HIGH_MODE=10,
        // Get current high-band mode in use (wideband only
        SPEEX_GET_HIGH_MODE=11,

        // Set VBR on (1) or off (0)
        SPEEX_SET_VBR=12,
        // Get VBR status (1 for on, 0 for off)
        SPEEX_GET_VBR=13,

        // Set quality value for VBR encoding (0-10)
        SPEEX_SET_VBR_QUALITY=14,
        // Get current quality value for VBR encoding (0-10)
        SPEEX_GET_VBR_QUALITY=15,

        // Set complexity of the encoder (0-10)
        SPEEX_SET_COMPLEXITY=16,
        // Get current complexity of the encoder (0-10)
        SPEEX_GET_COMPLEXITY=17,

        // Set bit-rate used by the encoder (or lower)
        SPEEX_SET_BITRATE=18,
        // Get current bit-rate used by the encoder or decoder
        SPEEX_GET_BITRATE=19,

        // Define a handler function for in-band Speex reques
        SPEEX_SET_HANDLER=20,

        // Define a handler function for in-band user-defined reques
        SPEEX_SET_USER_HANDLER=22,

        // Set sampling rate used in bit-rate computation
        SPEEX_SET_SAMPLING_RATE=24,
        // Get sampling rate used in bit-rate computation
        SPEEX_GET_SAMPLING_RATE=25,

        // Reset the encoder/decoder memories to zer
        SPEEX_RESET_STATE=26,

        // Get VBR info (mostly used internally)
        SPEEX_GET_RELATIVE_QUALITY=29,

        // Set VAD status (1 for on, 0 for off)
        SPEEX_SET_VAD=30,

        // Get VAD status (1 for on, 0 for off)
        SPEEX_GET_VAD=31,

        // Set Average Bit-Rate (ABR) to n bits per seconds
        SPEEX_SET_ABR=32,
        // Get Average Bit-Rate (ABR) setting (in bps)
        SPEEX_GET_ABR=33,

        // Set DTX status (1 for on, 0 for off)
        SPEEX_SET_DTX=34,
        // Get DTX status (1 for on, 0 for off)
        SPEEX_GET_DTX=35,

        // Set submode encoding in each frame (1 for yes, 0 for no, setting to no breaks the standard)
        SPEEX_SET_SUBMODE_ENCODING=36,
        // Get submode encoding in each frame
        SPEEX_GET_SUBMODE_ENCODING=37,

        // SPEEX_SET_LOOKAHEAD=38,
        // Returns the lookahead used by Speex
        SPEEX_GET_LOOKAHEAD=39,

        // Sets tuning for packet-loss concealment (expected loss rate)
        SPEEX_SET_PLC_TUNING=40,
        // Gets tuning for PLC
        SPEEX_GET_PLC_TUNING=41,

        // Sets the max bit-rate allowed in VBR mode
        SPEEX_SET_VBR_MAX_BITRATE=42,
        // Gets the max bit-rate allowed in VBR mode
        SPEEX_GET_VBR_MAX_BITRATE=43,

        // Turn on/off input/output high-pass filtering
        SPEEX_SET_HIGHPASS=44,
        // Get status of input/output high-pass filtering
        SPEEX_GET_HIGHPASS=45,

        // Get "activity level" of the last decoded frame, i.e, 
        // how much damage we cause if we remove the frame
        SPEEX_GET_ACTIVITY=47
    }

    // Preserving compatibility:
    public enum SpeexCompatCode {
        // Equivalent to SPEEX_SET_ENH
        SPEEX_SET_PF=0,
        // Equivalent to SPEEX_GET_ENH
        SPEEX_GET_PF=1
    }

    // Values allowed for mode queries
    public enum SpeexModeQuery {
        // Query the frame size of a mode
        SPEEX_MODE_FRAME_SIZE=0,

        // Query the size of an encoded frame for a particular sub-mode
        SPEEX_SUBMODE_BITS_PER_FRAME=1
    }

    public enum SpeexVersion {
        // Get major Speex version
        SPEEX_LIB_GET_MAJOR_VERSION=1,
        // Get minor Speex version
        SPEEX_LIB_GET_MINOR_VERSION=3,
        // Get micro Speex version
        SPEEX_LIB_GET_MICRO_VERSION=5,
        // Get extra Speex version
        SPEEX_LIB_GET_EXTRA_VERSION=7,
        // Get Speex version string
        SPEEX_LIB_GET_VERSION_STRING=9,

        //     SPEEX_LIB_SET_ALLOC_FUNC=10,
        //     SPEEX_LIB_GET_ALLOC_FUNC=11,
        //     SPEEX_LIB_SET_FREE_FUNC=12,
        //     SPEEX_LIB_GET_FREE_FUNC=13,

        //     SPEEX_LIB_SET_WARNING_FUNC=14,
        //     SPEEX_LIB_GET_WARNING_FUNC=15,
        //     SPEEX_LIB_SET_ERROR_FUNC=16,
        //     SPEEX_LIB_GET_ERROR_FUNC=17,
    }
    
    // Modes supported by Speex
    public enum SpeexBand {
        // modeID for the defined narrowband mode
        SPEEX_MODEID_NB=0,
        // modeID for the defined wideband mode
        SPEEX_MODEID_WB=1,
        // modeID for the defined ultra-wideband mode
        SPEEX_MODEID_UWB=2,
        // Number of defined modes in Speex
        SPEEX_NB_MODES=3
    }

    // Preprocessor control codes
    public enum PreprocessCtlCode {

        // Set preprocessor denoiser state
        SPEEX_PREPROCESS_SET_DENOISE=0,
        // Get preprocessor denoiser state
        SPEEX_PREPROCESS_GET_DENOISE=1,

        // Set preprocessor Automatic Gain Control state
        SPEEX_PREPROCESS_SET_AGC=2,
        // Get preprocessor Automatic Gain Control state
        SPEEX_PREPROCESS_GET_AGC=3,

        // Set preprocessor Voice Activity Detection state
        SPEEX_PREPROCESS_SET_VAD=4,
        // Get preprocessor Voice Activity Detection state
        SPEEX_PREPROCESS_GET_VAD=5,

        // Set preprocessor Automatic Gain Control level
        SPEEX_PREPROCESS_SET_AGC_LEVEL=6,
        // Get preprocessor Automatic Gain Control level
        SPEEX_PREPROCESS_GET_AGC_LEVEL=7,

        // Set preprocessor dereverb state
        SPEEX_PREPROCESS_SET_DEREVERB=8,
        // Get preprocessor dereverb state
        SPEEX_PREPROCESS_GET_DEREVERB=9,

        // Set preprocessor dereverb level
        SPEEX_PREPROCESS_SET_DEREVERB_LEVEL=10,
        // Get preprocessor dereverb level
        SPEEX_PREPROCESS_GET_DEREVERB_LEVEL=11,

        // Set preprocessor dereverb decay
        SPEEX_PREPROCESS_SET_DEREVERB_DECAY=12,
        // Get preprocessor dereverb decay
        SPEEX_PREPROCESS_GET_DEREVERB_DECAY=13,

        // Set probability required for the VAD to go from silence to voice
        SPEEX_PREPROCESS_SET_PROB_START=14,
        // Get probability required for the VAD to go from silence to voice
        SPEEX_PREPROCESS_GET_PROB_START=15,

        // Set probability required for the VAD to stay in the voice state (integer percent)
        SPEEX_PREPROCESS_SET_PROB_CONTINUE=16,
        // Get probability required for the VAD to stay in the voice state (integer percent)
        SPEEX_PREPROCESS_GET_PROB_CONTINUE=17,

        // Set maximum attenuation of the noise in dB (negative number)
        SPEEX_PREPROCESS_SET_NOISE_SUPPRESS=18,
        // Get maximum attenuation of the noise in dB (negative number)
        SPEEX_PREPROCESS_GET_NOISE_SUPPRESS=19,

        // Set maximum attenuation of the residual echo in dB (negative number)
        SPEEX_PREPROCESS_SET_ECHO_SUPPRESS=20,
        // Get maximum attenuation of the residual echo in dB (negative number)
        SPEEX_PREPROCESS_GET_ECHO_SUPPRESS=21,

        // Set maximum attenuation of the residual echo in dB when near end is active (negative number)
        SPEEX_PREPROCESS_SET_ECHO_SUPPRESS_ACTIVE=22,
        // Get maximum attenuation of the residual echo in dB when near end is active (negative number)
        SPEEX_PREPROCESS_GET_ECHO_SUPPRESS_ACTIVE=23,

        // Set the corresponding echo canceller state so that residual echo suppression can be performed (NULL for no residual echo suppression)
        SPEEX_PREPROCESS_SET_ECHO_STATE=24,
        // Get the corresponding echo canceller state
        SPEEX_PREPROCESS_GET_ECHO_STATE=25,

        // Set maximal gain increase in dB/second (int32)
        SPEEX_PREPROCESS_SET_AGC_INCREMENT=26,

        // Get maximal gain increase in dB/second (int32)
        SPEEX_PREPROCESS_GET_AGC_INCREMENT=27,

        // Set maximal gain decrease in dB/second (int32)
        SPEEX_PREPROCESS_SET_AGC_DECREMENT=28,

        // Get maximal gain decrease in dB/second (int32)
        SPEEX_PREPROCESS_GET_AGC_DECREMENT=29,

        // Set maximal gain in dB (int32)
        SPEEX_PREPROCESS_SET_AGC_MAX_GAIN=30,

        // Get maximal gain in dB (int32)
        SPEEX_PREPROCESS_GET_AGC_MAX_GAIN=31,

        //  Can't set loudness
        // Get loudness
        SPEEX_PREPROCESS_GET_AGC_LOUDNESS=33
    }

    public enum JitterBufferRetCode {
        // Packet has been retrieved
        JITTER_BUFFER_OK = 0,
        // Packet is lost or is late
        JITTER_BUFFER_MISSING = 1,
        // A "fake" packet is meant to be inserted here to increase buffering
        JITTER_BUFFER_INSERTION = 2,
        // There was an error in the jitter buffer
        JITTER_BUFFER_INTERNAL_ERROR = -1,
        // Invalid argument
        JITTER_BUFFER_BAD_ARGUMENT = -2,
    }

    // Jitter Buffer Control Codes
    public enum JitterBufferCtlCode {
        // Set minimum amount of extra buffering required (margin)
        JITTER_BUFFER_SET_MARGIN = 0,
        // Get minimum amount of extra buffering required (margin)
        JITTER_BUFFER_GET_MARGIN = 1,
        /* JITTER_BUFFER_SET_AVAILABLE_COUNT wouldn't make sense */

        // Get the amount of available packets currently buffered
        JITTER_BUFFER_GET_AVAILABLE_COUNT = 3,
        // Included because of an early misspelling (will remove in next release)
        JITTER_BUFFER_GET_AVALIABLE_COUNT = 3,

        // Assign a function to destroy unused packet. When setting
        // that, the jitter buffer no longer copies packet data.
        JITTER_BUFFER_SET_DESTROY_CALLBACK = 4,
        JITTER_BUFFER_GET_DESTROY_CALLBACK = 5,

        // Tell the jitter buffer to only adjust the delay in
        // multiples of the step parameter provided
        JITTER_BUFFER_SET_DELAY_STEP = 6,
        JITTER_BUFFER_GET_DELAY_STEP = 7,

        // Tell the jitter buffer to only do concealment in multiples of the size parameter provided
        JITTER_BUFFER_SET_CONCEALMENT_SIZE = 8,
        JITTER_BUFFER_GET_CONCEALMENT_SIZE = 9,

        // Absolute max amount of loss that can be tolerated
        // regardless of the delay. Typical loss should be half of
        // that or less.
        JITTER_BUFFER_SET_MAX_LATE_RATE = 10,
        JITTER_BUFFER_GET_MAX_LATE_RATE = 11,

        // Equivalent cost of one percent late packet in timestamp units
        JITTER_BUFFER_SET_LATE_COST = 12,
        JITTER_BUFFER_GET_LATE_COST = 13
    }
    
    public unsafe class SpeexCodec {

        public struct SpeexBits
        {
            char* chars;        /* "raw" data */
            int nbBits;         /* Total number of bits stored in the stream*/
            int charPtr;        /* Position of the byte "cursor" */
            int bitPtr;         /* Position of the bit "cursor" within the current char */
            int owner;          /* Does the struct "own" the "raw" buffer (member "chars") */
            int overflow;       /* Set to one if we try to read past the valid data */
            int buf_size;       /* Allocated size for buffer */
            int reserved1;      /* Reserved for future use */
            void* reserved2;    /* Reserved for future use */
        }

        public struct JitterBuffer {
        }
        
        public struct JitterBufferPacket {
            public byte *data;         /* Data bytes contained in the packet */
            public uint len;           /* Length of the packet in bytes */
            public uint timestamp;     /* Timestamp for the packet */
            public uint span;          /* Time covered by the packet (timestamp units) */
        }

        public struct SpeexPreprocessState {
        }
        
        // EXPORTED ENCODER METHODS

        [DllImport("libspeex.dll")]
        public static extern void *speex_encoder_init_new(int modeID);

        [DllImport("libspeex.dll")]
        public static extern int speex_encoder_ctl(void *state, int request, void *ptr);

        [DllImport("libspeex.dll")]
        public static extern int speex_encode_int(void *state, short *input, SpeexBits *bits);	// IntPtr

        [DllImport("libspeex.dll")]
        public static extern int speex_encoder_destroy(void* state);

        // EXPORTED ENCODER BIT-OPERATION METHODS

        [DllImport("libspeex.dll")]
        public static extern int speex_bits_write(SpeexBits *bits, byte *bytes, int max_len);	// char *

        [DllImport("libspeex.dll")]
        public static extern int speex_bits_write_whole_bytes(SpeexBits *bits, byte *bytes, int max_len);	// char *

        // EXPORTED DECODER METHODS

        [DllImport("libspeex.dll")]
        public static extern void* speex_decoder_init_new(int modeID);

        [DllImport("libspeex.dll")]
        public static extern int speex_decoder_ctl(void *state, int request, void *ptr);

        [DllImport("libspeex.dll")]
        public static extern int speex_bits_read_from(SpeexBits *bits, byte *inputBuffer, int inputByteCount);

        [DllImport("libspeex.dll")]
        public static extern int speex_decode_int(void* state, SpeexBits *bits, short* output);	// IntPtr

        [DllImport("libspeex.dll")]
        public static extern int speex_decoder_destroy(void* state);

        // Preprocessor API

        [DllImport("libspeexdsp.dll")]
        public static extern SpeexPreprocessState *speex_preprocess_state_init(int frame_size, int sampling_rate);
        
        [DllImport("libspeexdsp.dll")]
        public static extern void speex_preprocess_state_destroy(SpeexPreprocessState *st);
        
        [DllImport("libspeexdsp.dll")]
        public static extern int speex_preprocess_run(SpeexPreprocessState *st, short *x);
        
        [DllImport("libspeexdsp.dll")]
        public static extern void speex_preprocess_estimate_update(SpeexPreprocessState *st, short *x);

        [DllImport("libspeexdsp.dll")]
        public static extern int speex_preprocess_ctl(SpeexPreprocessState *st, int request, void *ptr);        
        
        // Jitter Buffer API
        [DllImport("libspeexdsp.dll")]
        public static extern JitterBuffer *jitter_buffer_init(int step_size);
        
        [DllImport("libspeexdsp.dll")]
        public static extern void jitter_buffer_reset(JitterBuffer *jitter);
        
        [DllImport("libspeexdsp.dll")]
        public static extern void jitter_buffer_destroy(JitterBuffer *jitter);
        
        [DllImport("libspeexdsp.dll")]
        public static extern void jitter_buffer_put(JitterBuffer *jitter, JitterBufferPacket *packet);
        
        [DllImport("libspeexdsp.dll")]
        public static extern int jitter_buffer_get(JitterBuffer *jitter, JitterBufferPacket *packet, int desired_span, int *start_offset);

        [DllImport("libspeexdsp.dll")]
        public static extern int jitter_buffer_get_another(JitterBuffer *jitter, JitterBufferPacket *packet);

        [DllImport("libspeexdsp.dll")]
        public static extern int jitter_buffer_get_pointer_timestamp(JitterBuffer *jitter);

        [DllImport("libspeexdsp.dll")]
        public static extern void jitter_buffer_tick(JitterBuffer *jitter);

        [DllImport("libspeexdsp.dll")]
        public static extern void jitter_buffer_remaining_span(JitterBuffer *jitter, uint rem);

        [DllImport("libspeexdsp.dll")]
        public static extern int jitter_buffer_ctl(JitterBuffer *jitter, int request, void *ptr);

        [DllImport("libspeexdsp.dll")]
        public static extern int jitter_buffer_update_delay(JitterBuffer *jitter, JitterBufferPacket *packet, int *start_offset);

        // Utility methods

        [DllImport("libspeex.dll")]
        public static extern void speex_bits_init(SpeexBits* bits);

        [DllImport("libspeex.dll")]
        public static extern int speex_bits_reset(SpeexBits* bits);

        [DllImport("libspeex.dll")]
        public static extern int speex_bits_destroy(SpeexBits* bits);

        // SpeexCodec data members

        private int frameSize;	
        private int maxFrameSize;

        private void *encoderState;
        private SpeexBits encodedBits;

        private SpeexPreprocessState *preprocessState;
        
        private bool validJitterBits = false;
        private JitterBuffer *jitterBuffer = null;
        private byte[] encodedJitterFrame = new byte[2048];
        private int encodedJitterFrameLength;
        private int encodedJitterFrameErrorCode;
        
        // These are just used for logging
        public byte[] EncodedJitterFrame {
            get {
                return encodedJitterFrame;
            }
        }
        
        public int EncodedJitterFrameLength {
            get {
                return encodedJitterFrameLength;
            }
        }
        
        public int EncodedJitterFrameErrorCode {
            get {
                return encodedJitterFrameErrorCode;
            }
        }
        
        // Provide something to lock
        private class JitterBufferLockable {
        }
        private JitterBufferLockable jitterBufferLockable = new JitterBufferLockable();
        
        void *decoderState;
        SpeexBits decodedBits;

        public int SetOneCodecSetting(bool encoder, SpeexCtlCode setting, int value) {
            int retcode = 0;
            unsafe {
                int *intPtr = &value;
                if (encoder)
                    retcode = speex_encoder_ctl(encoderState, (int)setting, (void *)intPtr);
                else
                    retcode = speex_decoder_ctl(decoderState, (int)setting, (void *)intPtr);
            }
            return retcode;
        }
        
        public int GetOneCodecSetting(bool encoder, SpeexCtlCode setting, ref int value) {
            int retcode = 0;
            unsafe {
                fixed (int *intPtr = &value) {
                    if (encoder)
                        retcode = speex_encoder_ctl(encoderState, (int)setting, (void *)intPtr);
                    else
                        retcode = speex_decoder_ctl(decoderState, (int)setting, (void *)intPtr);
                }
            }
            return retcode;
        }
        
        public int SetOnePreprocessorSetting(PreprocessCtlCode setting, int value) {
            int retcode = 0;
            unsafe {
                int *intPtr = &value;
                retcode = speex_preprocess_ctl(preprocessState, (int)setting, (void *)intPtr);
            }
            return retcode;
        }
        
        public int SetOnePreprocessorSetting(PreprocessCtlCode setting, float value) {
            int retcode = 0;
            unsafe {
                float *floatPtr = &value;
                retcode = speex_preprocess_ctl(preprocessState, (int)setting, (void *)floatPtr);
            }
            return retcode;
        }
        
        public int SetOnePreprocessorSetting(PreprocessCtlCode setting, bool bValue) {
            int value = (bValue ? 1 : 0);
            int retcode = 0;
            unsafe {
                int *intPtr = &value;
                retcode = speex_preprocess_ctl(preprocessState, (int)setting, (void *)intPtr);
            }
            return retcode;
        }
        
        public int GetOnePreprocessorSetting(PreprocessCtlCode setting, ref int value) {
            int retcode = 0;
            unsafe {
                fixed (int *intPtr = &value) {
                    retcode = speex_preprocess_ctl(preprocessState, (int)setting, (void *)intPtr);
                }
            }
            return retcode;
        }
        
        public int GetOnePreprocessorSetting(PreprocessCtlCode setting, ref float value) {
            int retcode = 0;
            unsafe {
                fixed (float *floatPtr = &value) {
                    retcode = speex_preprocess_ctl(preprocessState, (int)setting, (void *)floatPtr);
                }
            }
            return retcode;
        }
        
        public int GetOnePreprocessorSetting(PreprocessCtlCode setting, ref bool value) {
            int retcode = 0;
            int intValue = 0;
            unsafe {
                retcode = speex_preprocess_ctl(preprocessState, (int)setting, (void *)&intValue);
            }
            value = (intValue == 0 ? false : true);
            return retcode;
        }
        
        public int SetOneJitterBufferSetting(JitterBufferCtlCode setting, int value) {
            int retcode = 0;
            unsafe {
                int *intPtr = &value;
                retcode = jitter_buffer_ctl(jitterBuffer, (int)setting, (void *)intPtr);
            }
            return retcode;
        }
        
        public int InitEncoder(int maxFrameSize, int samplesPerFrame, int samplingRate) {
            this.maxFrameSize = maxFrameSize;
            encodedBits = new SpeexBits();
            encoderState = speex_encoder_init_new(0);
            // Don't set VAD in the codec, because we're setting it in
            // the preprocessor instead
            fixed (int *fSize = &frameSize) {
                speex_encoder_ctl(encoderState, (int)SpeexCtlCode.SPEEX_GET_FRAME_SIZE, fSize);
            }
            fixed (SpeexBits *bitsAdd = &encodedBits) {
                speex_bits_init(bitsAdd);
            }
            preprocessState = speex_preprocess_state_init(samplesPerFrame, samplingRate);
            return frameSize;
        }
    
        public int PreprocessFrame(short[] sampleBuffer) {
            fixed (short *fixedSamples = sampleBuffer) {
                return speex_preprocess_run(preprocessState, fixedSamples);
            }
        }
        
        public int EncodeFrame(short[] inputFrame, byte[] outputFrame) {
            int encodedDataSize = 0;
            fixed (short *inputAdd = inputFrame) {
                fixed (SpeexBits *bitsAdd = &encodedBits) {
                    speex_encode_int(encoderState, inputAdd, bitsAdd);
                    fixed (byte* outputBytes = outputFrame) {
//                         encodedDataSize = speex_bits_write_whole_bytes(bitsAdd, outputBytes, maxFrameSize);
                        encodedDataSize = speex_bits_write(bitsAdd, outputBytes, maxFrameSize);
                    }
                }
            }
            fixed (SpeexBits *bitsAdd = &encodedBits) {
                speex_bits_reset(bitsAdd);
            }
            return encodedDataSize;
        }

        public void ResetEncoder() {
            fixed (SpeexBits *bitsToAdd = &encodedBits) {
                speex_bits_destroy(bitsToAdd);
            }
            if (encoderState != null) {
                speex_encoder_destroy(encoderState);
                encoderState = null;
            }
        }
    
        public void InitDecoder(bool useJitterBuffer, int stepSize, int frameSize) {
            this.frameSize = frameSize;
            decodedBits = new SpeexBits();
            decoderState = speex_decoder_init_new(0);
            fixed (SpeexBits *bitsDecode = &decodedBits)
            {
                speex_bits_init(bitsDecode);
            }
            if (useJitterBuffer) {
                jitterBuffer = jitter_buffer_init(stepSize);
                validJitterBits = false;
            }
        }
    
        public int DecodeFrame(byte[] inputToDecode, int encodedByteCount, short[] decodedFrame) {
            DecoderReadFrom(inputToDecode, encodedByteCount);
            return DecoderDecodeBits(decodedFrame);
        }
        
        public void DecoderReadFrom(byte[] inputToDecode, int encodedByteCount) {
            fixed (SpeexBits *bitsDecoder = &decodedBits) {
                fixed (byte *inputFrame = inputToDecode) {
                    speex_bits_read_from(bitsDecoder, inputFrame, encodedByteCount);
                }
            }
        }
            
        public int DecoderDecodeBits(short[] decodedFrame) {
            fixed (SpeexBits *bitsDecoder = &decodedBits) {
                fixed (short *outputFrame = decodedFrame) {
                    return speex_decode_int(decoderState, bitsDecoder, outputFrame);
                }
            }
        }
        
        public int DecoderDecodeNullBits(short[] decodedFrame) {
            fixed (short *outputFrame = decodedFrame) {
                return speex_decode_int(decoderState, null, outputFrame);
            }
        }
        
        public void ResetDecoder() {
            fixed (SpeexBits *bitsDecoder = &decodedBits) {
                speex_bits_destroy(bitsDecoder);
            }
            if (decoderState != null) {
                speex_decoder_destroy(decoderState);
                decoderState = null;
            }
            if (jitterBuffer != null) {
                jitter_buffer_destroy(jitterBuffer);
                jitterBuffer = null;
            }
        }

        // Jitter buffer wrapper API

        // Locking must be done at the application level to ensure
        // that two threads can't be in jitter buffer methods.  
        // timestamp is a counter incremented once per "tick"
        public void JitterBufferPut(byte[] frame, int startIndex, uint byteCount, uint timestamp) {
            if (jitterBuffer == null)
                throw new Exception("JitterBufferPut: jitterBuffer is null!");
            lock(jitterBufferLockable) {
                JitterBufferPacket p = new JitterBufferPacket();
                unsafe {
                    fixed (byte* frameBytes = &frame[startIndex]) {
                        p.data = frameBytes;
                        p.len = byteCount;
                        p.timestamp = timestamp;
                        p.span = (uint)frameSize;
                        jitter_buffer_put(jitterBuffer, &p);
                    }
                }
            }
        }

        // Returns the length of the _encoded_ frame in bytes
        public void JitterBufferGet(short[] decodedFrame, uint timestamp, ref int startOffset) {
            int i;
            int ret;
            int activity = 0;

            if (jitterBuffer == null)
                throw new Exception("JitterBufferPut: jitterBuffer is null!");

            lock(jitterBufferLockable) {
                if (validJitterBits) {
                    // Try decoding last received packet
                    ret = DecoderDecodeBits(decodedFrame);
                    if (ret == 0) {
                        jitter_buffer_tick(jitterBuffer);
                        return;
                    } else
                        validJitterBits = false;
                }

                JitterBufferPacket packet = new JitterBufferPacket();
                packet.span = (uint)frameSize;
                packet.timestamp = timestamp;
                // The encoded buffer must be fixed, because
                // jitter_buffer_get refers to it through packet
                unsafe {
                    fixed (byte* pData = &encodedJitterFrame[0]) {
                        fixed (int* pStartOffset = &startOffset) {
                            packet.data = pData;
                            packet.span = (uint)frameSize;
                            packet.len = 2048;
                            ret = jitter_buffer_get(jitterBuffer, &packet, frameSize, pStartOffset);
                        }
                    }
                }
                encodedJitterFrameErrorCode = ret;
                if (ret != (int)JitterBufferRetCode.JITTER_BUFFER_OK) {
                    // No packet found: Packet is late or lost
                    DecoderDecodeNullBits(decodedFrame);
                } else {
                    encodedJitterFrameLength = (int)packet.len;
                    DecoderReadFrom(encodedJitterFrame, encodedJitterFrameLength);
                    /* Decode packet */
                    ret = DecoderDecodeBits(decodedFrame);
                    if (ret == 0)
                        validJitterBits = true;
                    else {
                        /* Error while decoding */
                        for (i=0; i<frameSize; i++)
                            decodedFrame[i] = 0;
                    }
                }
            
                GetOneCodecSetting(false, SpeexCtlCode.SPEEX_GET_ACTIVITY, ref activity);
                if (activity < 30)
                    jitter_buffer_update_delay(jitterBuffer, &packet, null);
                jitter_buffer_tick(jitterBuffer);
            }
        }

    }

}

