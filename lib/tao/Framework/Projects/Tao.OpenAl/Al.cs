#region License
/*
MIT License
Copyright ©2003-2005 Tao Framework Team
http://www.taoframework.com
All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Tao.OpenAl {
    #region Class Documentation
    /// <summary>
    ///     OpenAL binding for .NET, implementing AL 1.0.
    /// </summary>
    /// <remarks>
    ///     Binds functions and definitions in MVOpenAL32.dll or libAL.so.
    /// </remarks>
    #endregion Class Documentation
    public sealed class Al {
        // --- Fields ---
        #region Private Constants
        #region CallingConvention CALLING_CONVENTION
        /// <summary>
        ///     Specifies the calling convention.
        /// </summary>
        /// <remarks>
        ///     Specifies <see cref="CallingConvention.Cdecl" />.
        /// </remarks>
        private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;
        #endregion CallingConvention CALLING_CONVENTION
        #endregion Private Constants

        #region Public OpenAL 1.0 Constants
        #region AL_INVALID
        /// <summary>
        ///     Bad value.
        /// </summary>
        // #define AL_INVALID -1
        public const int AL_INVALID = -1;
        #endregion AL_INVALID

        #region AL_NONE
        /// <summary>
        ///     Disable value.
        /// </summary>
        // #define AL_NONE 0
        public const int AL_NONE = 0;
        #endregion AL_NONE

        #region AL_FALSE
        /// <summary>
        ///     bool false.
        /// </summary>
        // #define AL_FALSE 0
        public const int AL_FALSE = 0;
        #endregion AL_FALSE

        #region AL_TRUE
        /// <summary>
        ///     bool true.
        /// </summary>
        // #define AL_TRUE 1
        public const int AL_TRUE = 1;
        #endregion AL_TRUE

        #region AL_SOURCE_TYPE
        /// <summary>
        ///     Indicates the type of AL_SOURCE.  Sources can be spatialized.
        /// </summary>
        // #define AL_SOURCE_TYPE 0x200
        public const int AL_SOURCE_TYPE = 0x200;
        #endregion AL_SOURCE_TYPE

        #region AL_SOURCE_ABSOLUTE
        /// <summary>
        ///     Indicates source has absolute coordinates.
        /// </summary>
        // #define AL_SOURCE_ABSOLUTE 0x201
        public const int AL_SOURCE_ABSOLUTE = 0x201;
        #endregion AL_SOURCE_ABSOLUTE

        #region AL_SOURCE_RELATIVE
        /// <summary>
        ///     Indicates source has listener-relative coordinates.
        /// </summary>
        // #define AL_SOURCE_RELATIVE 0x202
        public const int AL_SOURCE_RELATIVE = 0x202;
        #endregion AL_SOURCE_RELATIVE

        #region AL_CONE_INNER_ANGLE
        /// <summary>
        ///     Directional source, inner cone angle, in degrees.  The accepted range is 0 to
        ///     360, the default value is 360.
        /// </summary>
        // #define AL_CONE_INNER_ANGLE 0x1001
        public const int AL_CONE_INNER_ANGLE = 0x1001;
        #endregion AL_CONE_INNER_ANGLE

        #region AL_CONE_OUTER_ANGLE
        /// <summary>
        ///     Directional source, outer cone angle, in degrees.  The accepted range is 0 to
        ///     360, the default value is 360.
        /// </summary>
        // #define AL_CONE_OUTER_ANGLE 0x1002
        public const int AL_CONE_OUTER_ANGLE = 0x1002;
        #endregion AL_CONE_OUTER_ANGLE

        #region AL_PITCH
        /// <summary>
        ///     Specifies the pitch to be applied, either at source, or on mixer results, at
        ///     listener.  The accepted range is 0.5 to 2.0, the default value is 1.0.
        /// </summary>
        // #define AL_PITCH 0x1003
        public const int AL_PITCH = 0x1003;
        #endregion AL_PITCH

        #region AL_POSITION
        /// <summary>
        ///     Specifies the current location in three dimensional space.  OpenAL, like OpenGL,
        ///     uses a right-handed coordinate system, where in a frontal default view X (thumb)
        ///     points right, Y points up (index finger), and Z points towards the viewer/camera
        ///     (middle finger).  To switch to a left-handed coordinate system, flip the sign on
        ///     the Z coordinate.  Listener position is always in the world coordinate system.
        /// </summary>
        // #define AL_POSITION 0x1004
        public const int AL_POSITION = 0x1004;
        #endregion AL_POSITION

        #region AL_DIRECTION
        /// <summary>
        ///     Specifies the current direction as forward vector.
        /// </summary>
        // #define AL_DIRECTION 0x1005
        public const int AL_DIRECTION = 0x1005;
        #endregion AL_DIRECTION

        #region AL_VELOCITY
        /// <summary>
        ///     Specifies the current velocity in three dimensional space.
        /// </summary>
        // #define AL_VELOCITY 0x1006
        public const int AL_VELOCITY = 0x1006;
        #endregion AL_VELOCITY

        #region AL_LOOPING
        /// <summary>
        ///     Indicates whether source has to loop infinitely.  The accepted values are
        ///     <see cref="AL_TRUE" /> or <see cref="AL_FALSE" />, the default value is
        ///     <see cref="AL_FALSE" />.
        /// </summary>
        // #define AL_LOOPING 0x1007
        public const int AL_LOOPING = 0x1007;
        #endregion AL_LOOPING

        #region AL_STREAMING
        /// <summary>
        ///     Indicates whether source is meant to be streaming.  The accepted values are
        ///     <see cref="AL_TRUE" /> or <see cref="AL_FALSE" />, the default value is
        ///     <see cref="AL_FALSE" />.
        /// </summary>
        // #define AL_STREAMING 0x1008
        public const int AL_STREAMING = 0x1008;
        #endregion AL_STREAMING

        #region AL_BUFFER
        /// <summary>
        ///     Indicates the buffer to provide sound samples.  The accepted range is any valid
        ///     buffer ID.
        /// </summary>
        // #define AL_BUFFER 0x1009
        public const int AL_BUFFER = 0x1009;
        #endregion AL_BUFFER

        #region AL_GAIN
        /// <summary>
        ///     Indicates the gain (volume amplification) applied.  The accepted range is 0.0
        ///     or above.  A value of 1.0 means unattenuated/unchanged.  Each division by 2 equals
        ///     an attenuation of -6dB.  Each multiplication by 2 equals an amplification of +6dB.
        ///     A value of 0.0 is meaningless with respect to a logarithmic scale; it is
        ///     interpreted as zero volume, the channel is effectively disabled.
        /// </summary>
        // #define AL_GAIN 0x100A
        public const int AL_GAIN = 0x100A;
        #endregion AL_GAIN

        #region AL_BYTE_LOKI
        /// <summary>
        ///     byte offset into source (in canon format).  -1 if source is not playing.  Do not
        ///     set this, get this.  The accepted range is -1 or above.
        /// </summary>
        // #define AL_BYTE_LOKI 0x100C
        public const int AL_BYTE_LOKI = 0x100C;
        #endregion AL_BYTE_LOKI

        #region AL_MIN_GAIN
        /// <summary>
        ///     Indicates minimum source attenuation.  The accepted range is 0.0 to 1.0.
        /// </summary>
        // #define AL_MIN_GAIN 0x100D
        public const int AL_MIN_GAIN = 0x100D;
        #endregion AL_MIN_GAIN

        #region AL_MAX_GAIN
        /// <summary>
        ///     Indicates maximum source attenuation.  The accepted range is 0.0 to 1.0.
        /// </summary>
        /// #define AL_MAX_GAIN 0x100E
        public const int AL_MAX_GAIN = 0x100E;
        #endregion AL_MAX_GAIN

        #region AL_ORIENTATION
        /// <summary>
        ///     Specifies the current orientation.
        /// </summary>
        // #define AL_ORIENTATION 0x100F
        public const int AL_ORIENTATION = 0x100F;
        #endregion AL_ORIENTATION

        #region AL_REFERENCE_DISTANCE
        /// <summary>
        ///     byte offset into source (in canon format).  -1 if source is not playing.  Do not
        ///     set this, only get this value.  The accepted range is 0.0 or above.  The default
        ///     value is 1.0.
        /// </summary>
        // #define AL_REFERENCE_DISTANCE 0x1020
        public const int AL_REFERENCE_DISTANCE = 0x1020;
        #endregion AL_REFERENCE_DISTANCE

        #region AL_ROLLOFF_FACTOR
        /// <summary>
        ///     Indicates the rolloff factor for the source.  The accepted range is 0.0 or
        ///     above.  The default value is 1.0.
        /// </summary>
        // #define AL_ROLLOFF_FACTOR 0x1021
        public const int AL_ROLLOFF_FACTOR = 0x1021;
        #endregion AL_ROLLOFF_FACTOR

        #region AL_CONE_OUTER_GAIN
        /// <summary>
        ///     Indicates the gain (volume amplification) applied.  The accepted range is 0.0 or
        ///     above.  A value of 1.0 means unattenuated/unchanged.  Each division by 2 equals an
        ///     attenuation of -6dB.  Each multiplication by 2 equals an amplification of +6dB.
        ///     A value of 0.0 is meaningless with respect to a logarithmic scale; it is
        ///     interpreted as zero volume, the channel is effectively disabled.
        /// </summary>
        // #define AL_CONE_OUTER_GAIN 0x1022
        public const int AL_CONE_OUTER_GAIN = 0x1022;
        #endregion AL_CONE_OUTER_GAIN

        #region AL_MAX_DISTANCE
        /// <summary>
        ///     Specifies the maximum distance.  The accepted range is 0.0 or above.
        /// </summary>
        // #define AL_MAX_DISTANCE 0x1023
        public const int AL_MAX_DISTANCE = 0x1023;
        #endregion AL_MAX_DISTANCE

        #region AL_CHANNEL_MASK
        /// <summary>
        ///     Specifies the channel mask.  The accepted range is 0 to 255.
        /// </summary>
        // #define AL_CHANNEL_MASK 0x3000
        public const int AL_CHANNEL_MASK = 0x3000;
        #endregion AL_CHANNEL_MASK

        #region AL_SOURCE_STATE
        /// <summary>
        ///     Source state information.
        /// </summary>
        // #define AL_SOURCE_STATE 0x1010
        public const int AL_SOURCE_STATE = 0x1010;
        #endregion AL_SOURCE_STATE

        #region AL_INITIAL
        /// <summary>
        ///     Source initialized.
        /// </summary>
        // #define AL_INITIAL 0x1011
        public const int AL_INITIAL = 0x1011;
        #endregion AL_INITIAL

        #region AL_PLAYING
        /// <summary>
        ///     Source playing.
        /// </summary>
        // #define AL_PLAYING 0x1012
        public const int AL_PLAYING = 0x1012;
        #endregion AL_PLAYING

        #region AL_PAUSED
        /// <summary>
        ///     Source paused.
        /// </summary>
        // #define AL_PAUSED 0x1013
        public const int AL_PAUSED = 0x1013;
        #endregion AL_PAUSED

        #region AL_STOPPED
        /// <summary>
        ///     Source stopped.
        /// </summary>
        // #define AL_STOPPED 0x1014
        public const int AL_STOPPED = 0x1014;
        #endregion AL_STOPPED

        #region AL_BUFFERS_QUEUED
        /// <summary>
        ///     Buffers are queued.
        /// </summary>
        // #define AL_BUFFERS_QUEUED 0x1015
        public const int AL_BUFFERS_QUEUED = 0x1015;
        #endregion AL_BUFFERS_QUEUED

        #region AL_BUFFERS_PROCESSED
        /// <summary>
        ///     Buffers are processed.
        /// </summary>
        // #define AL_BUFFERS_PROCESSED 0x1016
        public const int AL_BUFFERS_PROCESSED = 0x1016;
        #endregion AL_BUFFERS_PROCESSED

        #region AL_FORMAT_MONO8
        /// <summary>
        ///     8-bit mono buffer.
        /// </summary>
        // #define AL_FORMAT_MONO8 0x1100
        public const int AL_FORMAT_MONO8 = 0x1100;
        #endregion AL_FORMAT_MONO8

        #region AL_FORMAT_MONO16
        /// <summary>
        ///     16-bit mono buffer.
        /// </summary>
        // #define AL_FORMAT_MONO16 0x1101
        public const int AL_FORMAT_MONO16 = 0x1101;
        #endregion AL_FORMAT_MONO16

        #region AL_FORMAT_STEREO8
        /// <summary>
        ///     8-bit stereo buffer.
        /// </summary>
        // #define AL_FORMAT_STEREO8 0x1102
        public const int AL_FORMAT_STEREO8 = 0x1102;
        #endregion AL_FORMAT_STEREO8

        #region AL_FORMAT_STEREO16
        /// <summary>
        ///     16-bit stereo buffer.
        /// </summary>
        // #define AL_FORMAT_STEREO16 0x1103
        public const int AL_FORMAT_STEREO16 = 0x1103;
        #endregion AL_FORMAT_STEREO16

        #region AL_FREQUENCY
        /// <summary>
        ///     Buffer frequency, in units of Hertz (Hz).  This is the number of samples per
        ///     second.  Half of the sample frequency marks the maximum significant frequency
        ///     component.
        /// </summary>
        // #define AL_FREQUENCY 0x2001
        public const int AL_FREQUENCY = 0x2001;
        #endregion AL_FREQUENCY

        #region AL_BITS
        /// <summary>
        ///     Buffer bit depth.
        /// </summary>
        // #define AL_BITS 0x2002
        public const int AL_BITS = 0x2002;
        #endregion AL_BITS

        #region AL_CHANNELS
        /// <summary>
        ///     Buffer channels.
        /// </summary>
        // #define AL_CHANNELS 0x2003
        public const int AL_CHANNELS = 0x2003;
        #endregion AL_CHANNELS

        #region AL_SIZE
        /// <summary>
        ///     Buffer size.
        /// </summary>
        // #define AL_SIZE 0x2004
        public const int AL_SIZE = 0x2004;
        #endregion AL_SIZE

        #region AL_DATA
        /// <summary>
        ///     Buffer data.
        /// </summary>
        // #define AL_DATA 0x2005
        public const int AL_DATA = 0x2005;
        #endregion AL_DATA

        #region AL_UNUSED
        /// <summary>
        ///     Buffer unused.
        /// </summary>
        // #define AL_UNUSED 0x2010
        public const int AL_UNUSED = 0x2010;
        #endregion AL_UNUSED

        #region AL_QUEUED
        /// <summary>
        ///     Buffer queued.
        /// </summary>
        // #define AL_QUEUED 0x2011
        public const int AL_QUEUED = 0x2011;
        #endregion AL_QUEUED

        #region AL_PENDING
        /// <summary>
        ///     Buffer pending.
        /// </summary>
        // #define AL_PENDING 0x2011
        public const int AL_PENDING = 0x2011;
        #endregion AL_PENDING

        #region AL_CURRENT
        /// <summary>
        ///     Buffer current.
        /// </summary>
        // #define AL_CURRENT 0x2012
        public const int AL_CURRENT = 0x2012;
        #endregion AL_CURRENT

        #region AL_PROCESSED
        /// <summary>
        ///     Buffer processed.
        /// </summary>
        // #define AL_PROCESSED 0x2012
        public const int AL_PROCESSED = 0x2012;
        #endregion AL_PROCESSED

        #region AL_NO_ERROR
        /// <summary>
        ///     No error.
        /// </summary>
        // #define AL_NO_ERROR AL_FALSE
        public const int AL_NO_ERROR = AL_FALSE;
        #endregion AL_NO_ERROR

        #region AL_INVALID_NAME
        /// <summary>
        ///     Illegal name passed as an argument to an AL call.
        /// </summary>
        // #define AL_INVALID_NAME 0xA001
        public const int AL_INVALID_NAME = 0xa001;
        #endregion AL_INVALID_NAME

        #region AL_ILLEGAL_ENUM
        /// <summary>
        ///     Illegal enum passed as an argument to an AL call.
        /// </summary>
        // #define AL_ILLEGAL_ENUM 0xA002
        public const int AL_ILLEGAL_ENUM = 0xA002;
        #endregion AL_ILLEGAL_ENUM

        #region AL_INVALID_ENUM
        /// <summary>
        ///     Illegal enum passed as an argument to an AL call.
        /// </summary>
        // #define AL_INVALID_ENUM 0xA002
        public const int AL_INVALID_ENUM = 0xA002;
        #endregion AL_INVALID_ENUM

        #region AL_INVALID_VALUE
        /// <summary>
        ///     Illegal value passed as an argument to an AL call.  Applies to parameter
        ///     values, but not to enumerations.
        /// </summary>
        // #define AL_INVALID_VALUE 0xA003
        public const int AL_INVALID_VALUE = 0xA003;
        #endregion AL_INVALID_VALUE

        #region AL_ILLEGAL_COMMAND
        /// <summary>
        ///     A function was called at an inappropriate time or in an inappropriate way,
        ///     causing an illegal state.  This can be an incompatible value, object ID, and/or
        ///     function.
        /// </summary>
        // #define AL_ILLEGAL_COMMAND 0xA004
        public const int AL_ILLEGAL_COMMAND = 0xA004;
        #endregion AL_ILLEGAL_COMMAND

        #region AL_INVALID_OPERATION
        /// <summary>
        ///     A function was called at an inappropriate time or in an inappropriate way,
        ///     causing an illegal state.  This can be an incompatible value, object ID, and/or
        ///     function.
        /// </summary>
        // #define AL_INVALID_OPERATION 0xA004
        public const int AL_INVALID_OPERATION = 0xA004;
        #endregion AL_INVALID_OPERATION

        #region AL_OUT_OF_MEMORY
        /// <summary>
        ///     A function could not be completed, because there is not enough memory available.
        /// </summary>
        // #define AL_OUT_OF_MEMORY 0xA005
        public const int AL_OUT_OF_MEMORY = 0xA005;
        #endregion AL_OUT_OF_MEMORY

        #region AL_VENDOR
        /// <summary>
        ///     Vendor name.
        /// </summary>
        // #define AL_VENDOR 0xb001
        public const int AL_VENDOR = 0xB001;
        #endregion AL_VENDOR

        #region AL_VERSION
        /// <summary>
        ///     Version.
        /// </summary>
        // #define AL_VERSION 0xB002
        public const int AL_VERSION = 0xB002;
        #endregion AL_VERSION

        #region AL_RENDERER
        /// <summary>
        ///     Renderer.
        /// </summary>
        // #define AL_RENDERER 0xB003
        public const int AL_RENDERER = 0xB003;
        #endregion AL_RENDERER

        #region AL_EXTENSIONS
        /// <summary>
        ///     Extensions.
        /// </summary>
        // #define AL_EXTENSIONS 0xB004
        public const int AL_EXTENSIONS = 0xB004;
        #endregion AL_EXTENSIONS

        #region AL_DOPPLER_FACTOR
        /// <summary>
        ///     Doppler scale.  The default value is 1.0.
        /// </summary>
        // #define AL_DOPPLER_FACTOR 0xC000
        public const int AL_DOPPLER_FACTOR = 0xC000;
        #endregion AL_DOPPLER_FACTOR

        #region AL_DOPPLER_VELOCITY
        /// <summary>
        ///     Doppler velocity.  The default value is 1.0.
        /// </summary>
        // #define AL_DOPPLER_VELOCITY 0xC001
        public const int AL_DOPPLER_VELOCITY = 0xC001;
        #endregion AL_DOPPLER_VELOCITY

        #region AL_DISTANCE_SCALE
        /// <summary>
        ///     Distance scaling.
        /// </summary>
        // #define AL_DISTANCE_SCALE 0xC002
        public const int AL_DISTANCE_SCALE = 0xC002;
        #endregion AL_DISTANCE_SCALE

        #region AL_DISTANCE_MODEL
        /// <summary>
        ///     Distance model.  The default value is <see cref="AL_INVERSE_DISTANCE_CLAMPED" />.
        /// </summary>
        // #define AL_DISTANCE_MODEL 0xD000
        public const int AL_DISTANCE_MODEL = 0xD000;
        #endregion AL_DISTANCE_MODEL

        #region AL_INVERSE_DISTANCE
        /// <summary>
        ///     Inverse distance model.
        /// </summary>
        // #define AL_INVERSE_DISTANCE 0xD001
        public const int AL_INVERSE_DISTANCE = 0xD001;
        #endregion AL_INVERSE_DISTANCE

        #region AL_INVERSE_DISTANCE_CLAMPED
        /// <summary>
        ///     Inverse distance clamped model.
        /// </summary>
        // #define AL_INVERSE_DISTANCE_CLAMPED 0xD002
        public const int AL_INVERSE_DISTANCE_CLAMPED = 0xD002;
        #endregion AL_INVERSE_DISTANCE_CLAMPED

        #region AL_ENV_ROOM_IASIG
        /// <summary>
        ///     Room.  The accepted range is -10000 to 0.  The default value is -10000.
        /// </summary>
        // #define AL_ENV_ROOM_IASIG 0x3001
        public const int AL_ENV_ROOM_IASIG = 0x3001;
        #endregion AL_ENV_ROOM_IASIG

        #region AL_ENV_ROOM_HIGH_FREQUENCY_IASIG
        /// <summary>
        ///     Room high frequency.  The accepted range is -10000 to 0.  The default value is 0.
        /// </summary>
        // #define AL_ENV_ROOM_HIGH_FREQUENCY_IASIG 0x3002
        public const int AL_ENV_ROOM_HIGH_FREQUENCY_IASIG = 0x3002;
        #endregion AL_ENV_ROOM_HIGH_FREQUENCY_IASIG

        #region AL_ENV_ROOM_ROLLOFF_FACTOR
        /// <summary>
        ///     Room rolloff factor.  The accepted range is 0.1 to 20.0.  The default value is
        ///     0.0.
        /// </summary>
        // #define AL_ENV_ROOM_ROLLOFF_FACTOR_IASIG 0x3003
        public const int AL_ENV_ROOM_ROLLOFF_FACTOR = 0x3003;
        #endregion AL_ENV_ROOM_ROLLOFF_FACTOR

        #region AL_ENV_DECAY_TIME_IASIG
        /// <summary>
        ///     Decay time.  The accepted range is 0.1 to 20.0.  The default value is 1.0.
        /// </summary>
        // #define AL_ENV_DECAY_TIME_IASIG 0x3004
        public const int AL_ENV_DECAY_TIME_IASIG = 0x3004;
        #endregion AL_ENV_DECAY_TIME_IASIG

        #region AL_ENV_DECAY_HIGH_FREQUENCY_RATIO_IASIG
        /// <summary>
        ///     Decay high frequency ratio.  The accepted range is 0.1 to 2.0.  The default value
        ///     is 0.5.
        /// </summary>
        // #define AL_ENV_DECAY_HIGH_FREQUENCY_RATIO_IASIG 0x3005
        public const int AL_ENV_DECAY_HIGH_FREQUENCY_RATIO_IASIG = 0x3005;
        #endregion AL_ENV_DECAY_HIGH_FREQUENCY_RATIO_IASIG

        #region AL_ENV_REFLECTIONS_IASIG
        /// <summary>
        ///     Reflections.  The accepted range is -10000 to 1000.  The default value is -10000.
        /// </summary>
        // #define AL_ENV_REFLECTIONS_IASIG 0x3006
        public const int AL_ENV_REFLECTIONS_IASIG = 0x3006;
        #endregion AL_ENV_REFLECTIONS_IASIG

        #region AL_ENV_REFLECTIONS_DELAY_IASIG
        /// <summary>
        ///     Reflections delay.  The accepted range is 0.0 to 0.3.  The default value is 0.02.
        /// </summary>
        // #define AL_ENV_REFLECTIONS_DELAY_IASIG 0x3006
        public const int AL_ENV_REFLECTIONS_DELAY_IASIG = 0x3006;
        #endregion AL_ENV_REFLECTIONS_DELAY_IASIG

        #region AL_ENV_REVERB_IASIG
        /// <summary>
        ///     Reverb.  The accepted range is -10000 to 2000.  The default value is -10000.
        /// </summary>
        // #define AL_ENV_REVERB_IASIG 0x3007
        public const int AL_ENV_REVERB_IASIG = 0x3007;
        #endregion AL_ENV_REVERB_IASIG

        #region AL_ENV_REVERB_DELAY_IASIG
        /// <summary>
        ///     Reverb delay.  The accepted range is 0.0 to 0.1.  The default value is 0.04.
        /// </summary>
        // #define AL_ENV_REVERB_DELAY_IASIG 0x3008
        public const int AL_ENV_REVERB_DELAY_IASIG = 0x3008;
        #endregion AL_ENV_REVERB_DELAY_IASIG

        #region AL_ENV_DIFFUSION_IASIG
        /// <summary>
        ///     Diffusion.  The accepted range is 0.0 to 100.0.  The default value is 100.0.
        /// </summary>
        // #define AL_ENV_DIFFUSION_IASIG 0x3009
        public const int AL_ENV_DIFFUSION_IASIG = 0x3009;
        #endregion AL_ENV_DIFFUSION_IASIG

        #region AL_ENV_DENSITY_IASIG
        /// <summary>
        ///     Density.  The accepted range is 0.0 to 100.0.  The default value is 100.0.
        /// </summary>
        // #define AL_ENV_DENSITY_IASIG 0x300A
        public const int AL_ENV_DENSITY_IASIG = 0x300A;
        #endregion AL_ENV_DENSITY_IASIG

        #region AL_ENV_HIGH_FREQUENCY_REFERENCE_IASIG
        /// <summary>
        ///     High frequency reference.  The accepted range is 20.0 to 20000.0.  The default
        ///     value is 5000.0.
        /// </summary>
        // #define AL_ENV_HIGH_FREQUENCY_REFERENCE_IASIG 0x300B
        public const int AL_ENV_HIGH_FREQUENCY_REFERENCE_IASIG = 0x300B;
        #endregion AL_ENV_HIGH_FREQUENCY_REFERENCE_IASIG
        #endregion Public OpenAL 1.0 Constants

        // --- Constructors & Destructors ---
        #region Al()
        /// <summary>
        ///     Prevents instantiation.
        /// </summary>
        private Al() {
        }
        #endregion Al()

        // --- Public Externs ---
        #region Public OpenAL 1.0 Methods
        #region alBufferData(int buffer, int format, [In] byte[] data, int size, int frequency)
        /// <summary>
        ///     Fills a buffer with audio data.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name to be filled with data.
        /// </param>
        /// <param name="format">
        ///     <para>
        ///         Format type from among the following:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FORMAT_MONO8" /></item>
        ///             <item><see cref="AL_FORMAT_MONO16" /></item>
        ///             <item><see cref="AL_FORMAT_STEREO8" /></item>
        ///             <item><see cref="AL_FORMAT_STEREO16" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="data">
        ///     Pointer to the audio data.
        /// </param>
        /// <param name="size">
        ///     The size of the audio data in bytes.
        /// </param>
        /// <param name="frequency">
        ///     The frequency of the audio data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alBufferData(ALuint buffer, ALenum format, ALvoid* data, ALsizei size, ALsizei freq);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alBufferData(int buffer, int format, [In] byte[] data, int size, int frequency);
        #endregion alBufferData(int buffer, int format, [In] byte[] data, int size, int frequency)

        #region alBufferData(int buffer, int format, [In] IntPtr data, int size, int frequency)
        /// <summary>
        ///     Fills a buffer with audio data.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name to be filled with data.
        /// </param>
        /// <param name="format">
        ///     <para>
        ///         Format type from among the following:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FORMAT_MONO8" /></item>
        ///             <item><see cref="AL_FORMAT_MONO16" /></item>
        ///             <item><see cref="AL_FORMAT_STEREO8" /></item>
        ///             <item><see cref="AL_FORMAT_STEREO16" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="data">
        ///     Pointer to the audio data.
        /// </param>
        /// <param name="size">
        ///     The size of the audio data in bytes.
        /// </param>
        /// <param name="frequency">
        ///     The frequency of the audio data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alBufferData(ALuint buffer, ALenum format, ALvoid* data, ALsizei size, ALsizei freq);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alBufferData(int buffer, int format, [In] IntPtr data, int size, int frequency);
        #endregion alBufferData(int buffer, int format, [In] IntPtr data, int size, int frequency)

        #region alBufferData(int buffer, int format, [In] void *data, int size, int frequency)
        /// <summary>
        ///     Fills a buffer with audio data.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name to be filled with data.
        /// </param>
        /// <param name="format">
        ///     <para>
        ///         Format type from among the following:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FORMAT_MONO8" /></item>
        ///             <item><see cref="AL_FORMAT_MONO16" /></item>
        ///             <item><see cref="AL_FORMAT_STEREO8" /></item>
        ///             <item><see cref="AL_FORMAT_STEREO16" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="data">
        ///     Pointer to the audio data.
        /// </param>
        /// <param name="size">
        ///     The size of the audio data in bytes.
        /// </param>
        /// <param name="frequency">
        ///     The frequency of the audio data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alBufferData(ALuint buffer, ALenum format, ALvoid* data, ALsizei size, ALsizei freq);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alBufferData(int buffer, int format, [In] void *data, int size, int frequency);
        #endregion alBufferData(int buffer, int format, [In] void *data, int size, int frequency)

        #region alDeleteBuffers(int number, [In] ref int buffer)
        /// <summary>
        ///     Deletes one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be deleted.
        /// </param>
        /// <param name="buffer">
        ///     Pointer to an array of buffer names identifying the buffers to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be deleted, an error will be
        ///     generated which can be detected with <see cref="alGetError" />.  If an error
        ///     occurs, no buffers will be deleted.  If <i>number</i> equals zero,
        ///     <b>alDeleteBuffers</b> does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteBuffers(int number, [In] ref int buffer);
        #endregion alDeleteBuffers(int number, [In] ref int buffer)

        #region alDeleteBuffers(int number, [In] int[] buffers)
        /// <summary>
        ///     Deletes one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be deleted.
        /// </param>
        /// <param name="buffers">
        ///     Pointer to an array of buffer names identifying the buffers to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be deleted, an error will be
        ///     generated which can be detected with <see cref="alGetError" />.  If an error
        ///     occurs, no buffers will be deleted.  If <i>number</i> equals zero,
        ///     <b>alDeleteBuffers</b> does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteBuffers(int number, [In] int[] buffers);
        #endregion alDeleteBuffers(int number, [In] int[] buffers)

        #region alDeleteBuffers(int number, [In] IntPtr buffers)
        /// <summary>
        ///     Deletes one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be deleted.
        /// </param>
        /// <param name="buffers">
        ///     Pointer to an array of buffer names identifying the buffers to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be deleted, an error will be
        ///     generated which can be detected with <see cref="alGetError" />.  If an error
        ///     occurs, no buffers will be deleted.  If <i>number</i> equals zero,
        ///     <b>alDeleteBuffers</b> does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteBuffers(int number, [In] IntPtr buffers);
        #endregion alDeleteBuffers(int number, [In] IntPtr buffers)

        #region alDeleteBuffers(int number, [In] int *buffers)
        /// <summary>
        ///     Deletes one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be deleted.
        /// </param>
        /// <param name="buffers">
        ///     Pointer to an array of buffer names identifying the buffers to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be deleted, an error will be
        ///     generated which can be detected with <see cref="alGetError" />.  If an error
        ///     occurs, no buffers will be deleted.  If <i>number</i> equals zero,
        ///     <b>alDeleteBuffers</b> does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alDeleteBuffers(int number, [In] int *buffers);
        #endregion alDeleteBuffers(int number, [In] int *buffers)

        #region alDeleteSources(int number, [In] ref int sources)
        /// <summary>
        ///     Deletes one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be deleted.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of source names identifying the sources to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be deleted, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be deleted.  If <i>number</i> equals zero, <b>alDeleteSources</b>
        ///     does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteSources(int number, [In] ref int sources);
        #endregion alDeleteSources(int number, [In] ref int sources)

        #region alDeleteSources(int number, [In] int[] sources)
        /// <summary>
        ///     Deletes one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be deleted.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of source names identifying the sources to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be deleted, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be deleted.  If <i>number</i> equals zero, <b>alDeleteSources</b>
        ///     does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteSources(int number, [In] int[] sources);
        #endregion alDeleteSources(int number, [In] int[] sources)

        #region alDeleteSources(int number, [In] IntPtr sources)
        /// <summary>
        ///     Deletes one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be deleted.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of source names identifying the sources to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be deleted, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be deleted.  If <i>number</i> equals zero, <b>alDeleteSources</b>
        ///     does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteSources(int number, [In] IntPtr sources);
        #endregion alDeleteSources(int number, [In] IntPtr sources)

        #region alDeleteSources(int number, [In] int *sources)
        /// <summary>
        ///     Deletes one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be deleted.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of source names identifying the sources to be deleted.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be deleted, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be deleted.  If <i>number</i> equals zero, <b>alDeleteSources</b>
        ///     does nothing and will not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDeleteSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alDeleteSources(int number, [In] int *sources);
        #endregion alDeleteSources(int number, [In] int *sources)

        #region alDisable(int capability)
        /// <summary>
        ///     Disables a feature of the OpenAL driver.
        /// </summary>
        /// <param name="capability">
        ///     The capability to disable.
        /// </param>
        /// <remarks>
        ///     At the time of this writing, there are no features to be disabled using this
        ///     function, so if it is called the error <see cref="AL_INVALID_ENUM" /> will be
        ///     generated.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDisable(ALenum capability);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDisable(int capability);
        #endregion alDisable(int capability)

        #region alDistanceModel(int val)
        /// <summary>
        ///     Selects the OpenAL distance model.
        /// </summary>
        /// <param name="val">
        ///     <para>
        ///         The distance model to be set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_NONE" /></item>
        ///             <item><see cref="AL_INVERSE_DISTANCE" /></item>
        ///             <item><see cref="AL_INVERSE_DISTANCE_CLAMPED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <remarks>
        ///     <para>
        ///         The default distance model in OpenAL is <see cref="AL_INVERSE_DISTANCE" />.
        ///     </para>
        ///     <para>
        ///         The <see cref="AL_INVERSE_DISTANCE" /> model works according to the following
        ///         formula:
        ///     </para>
        ///     <para>
        ///         <c>
        ///             G_dB = AL_GAIN – 20log10(1 + AL_ROLLOFF_FACTOR * (distance – AL_REFERENCE_DISTANCE) / AL_REFERENCE_DISTANCE));
        ///             G_dB = min(G_dB, AL_MAX_GAIN);
        ///             G_dB = max(G_dB, AL_MIN_GAIN);
        ///         </c>
        ///     </para>
        ///     <para>
        ///         The <see cref="AL_INVERSE_DISTANCE_CLAMPED" /> model works according to the
        ///         following formula:
        ///     </para>
        ///     <para>
        ///         <c>
        ///             distance = max(distance, AL_REFERENCE_DISTANCE);
        ///             distance = min(distance, AL_MAX_DISTANCE);
        ///             G_dB = AL_GAIN – 20log10(1 + AL_ROLLOFF_FACTOR * (distance – AL_REFERENCE_DISTANCE) / AL_REFERENCE_DISTANCE));
        ///             G_dB = min(G_dB, AL_MAX_GAIN);
        ///             G_dB = max(G_dB, AL_MIN_GAIN);
        ///         </c>
        ///     </para>
        ///     <para>
        ///         The <see cref="AL_NONE" /> model works according to the following formula:
        ///     </para>
        ///     <para>
        ///         <c>
        ///             G_db = AL_GAIN;
        ///         </c>
        ///     </para>
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDistanceModel(ALenum value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDistanceModel(int val);
        #endregion alDistanceModel(int val)

        #region alDopplerFactor(float val)
        /// <summary>
        ///     Selects the OpenAL Doppler factor value.
        /// </summary>
        /// <param name="val">
        ///     The Doppler scale value to set.
        /// </param>
        /// <remarks>
        ///     The default Doppler factor value is 1.0.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDopplerFactor(ALfloat value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDopplerFactor(float val);
        #endregion alDopplerFactor(float val)

        #region alDopplerVelocity(float val)
        /// <summary>
        ///     Selects the OpenAL Doppler velocity value.
        /// </summary>
        /// <param name="val">
        ///     The Doppler velocity value to set.
        /// </param>
        /// <remarks>
        ///     The default Doppler velocity value is 343.3.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alDopplerVelocity(ALfloat value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDopplerVelocity(float val);
        #endregion alDopplerVelocity(float val)

        #region alEnable(int capability)
        /// <summary>
        ///     Enables a feature of the OpenAL driver.
        /// </summary>
        /// <param name="capability">
        ///     The capability to enable.
        /// </param>
        /// <remarks>
        ///     At the time of this writing, there are no features to be enabled using this
        ///     function, so if it is called the error <see cref="AL_INVALID_ENUM" /> will be
        ///     generated.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alEnable(ALenum capability);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alEnable(int capability);
        #endregion alEnable(int capability)

        #region alGenBuffers(int number, out int buffer)
        /// <summary>
        ///     Generates one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be generated.
        /// </param>
        /// <param name="buffer">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     buffers.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     buffers will be generated.  If <i>number</i> equals zero, <b>alGenBuffers</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGenBuffers(int number, out int buffer);
        #endregion alGenBuffers(int number, out int buffer)

        #region alGenBuffers(int number, [Out] int[] buffers)
        /// <summary>
        ///     Generates one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be generated.
        /// </param>
        /// <param name="buffers">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     buffers.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     buffers will be generated.  If <i>number</i> equals zero, <b>alGenBuffers</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGenBuffers(int number, [Out] int[] buffers);
        #endregion alGenBuffers(int number, [Out] int[] buffers)

        #region alGenBuffers(int number, [Out] IntPtr buffers)
        /// <summary>
        ///     Generates one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be generated.
        /// </param>
        /// <param name="buffers">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     buffers.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     buffers will be generated.  If <i>number</i> equals zero, <b>alGenBuffers</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGenBuffers(int number, [Out] IntPtr buffers);
        #endregion alGenBuffers(int number, [Out] IntPtr buffers)

        #region alGenBuffers(int number, [Out] int *buffers)
        /// <summary>
        ///     Generates one or more buffers.
        /// </summary>
        /// <param name="number">
        ///     The number of buffers to be generated.
        /// </param>
        /// <param name="buffers">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     buffers.
        /// </param>
        /// <remarks>
        ///     If the requested number of buffers cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     buffers will be generated.  If <i>number</i> equals zero, <b>alGenBuffers</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenBuffers(ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGenBuffers(int number, [Out] int *buffers);
        #endregion alGenBuffers(int number, [Out] int *buffers)

        #region alGenSources(int number, out int source)
        /// <summary>
        ///     Generates one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be generated.
        /// </param>
        /// <param name="source">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     sources.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be generated.  If <i>number</i> equals zero, <b>alGenSources</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGenSources(int number, out int source);
        #endregion alGenSources(int number, out int source)

        #region alGenSources(int number, [Out] int[] sources)
        /// <summary>
        ///     Generates one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be generated.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     sources.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be generated.  If <i>number</i> equals zero, <b>alGenSources</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGenSources(int number, [Out] int[] sources);
        #endregion alGenSources(int number, [Out] int[] sources)

        #region alGenSources(int number, [Out] IntPtr sources)
        /// <summary>
        ///     Generates one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be generated.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     sources.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be generated.  If <i>number</i> equals zero, <b>alGenSources</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGenSources(int number, [Out] IntPtr sources);
        #endregion alGenSources(int number, [Out] IntPtr sources)

        #region alGenSources(int number, [Out] int *sources)
        /// <summary>
        ///     Generates one or more sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be generated.
        /// </param>
        /// <param name="sources">
        ///     Pointer to an array of integer values which will store the names of the new
        ///     sources.
        /// </param>
        /// <remarks>
        ///     If the requested number of sources cannot be created, an error will be generated
        ///     which can be detected with <see cref="alGetError" />.  If an error occurs, no
        ///     sources will be generated.  If <i>number</i> equals zero, <b>alGenSources</b>
        ///     does nothing and does not return an error.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGenSources(ALsizei n, ALuint* sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGenSources(int number, [Out] int *sources);
        #endregion alGenSources(int number, [Out] int *sources)

        #region int alGetBoolean(int state)
        /// <summary>
        ///     Returns a boolean OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <returns>
        ///     The boolean value (<see cref="AL_TRUE" /> or <see cref="AL_FALSE" />) described
        ///     by <i>state</i> will be returned.
        /// </returns>
        /// <remarks>
        ///     There aren’t any boolean states defined at the time of this writing, so this
        ///     function will always generate the error <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALboolean ALAPIENTRY alGetBoolean(ALenum param);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGetBoolean(int state);
        #endregion int alGetBoolean(int state)

        #region alGetBooleanv(int state, out int output)
        /// <summary>
        ///     Retrieves a boolean OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any boolean states defined at the time of this writing, so this
        ///     function will always generate the error <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBooleanv(ALenum param, ALboolean* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBooleanv(int state, out int output);
        #endregion alGetBooleanv(int state, out int output)

        #region alGetBooleanv(int state, [Out] int[] output)
        /// <summary>
        ///     Retrieves a boolean OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any boolean states defined at the time of this writing, so this
        ///     function will always generate the error <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBooleanv(ALenum param, ALboolean* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBooleanv(int state, [Out] int[] output);
        #endregion alGetBooleanv(int state, [Out] int[] output)

        #region alGetBooleanv(int state, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves a boolean OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any boolean states defined at the time of this writing, so this
        ///     function will always generate the error <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBooleanv(ALenum param, ALboolean* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBooleanv(int state, [Out] IntPtr output);
        #endregion alGetBooleanv(int state, [Out] IntPtr output)

        #region alGetBooleanv(int state, [Out] int *output)
        /// <summary>
        ///     Retrieves a boolean OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any boolean states defined at the time of this writing, so this
        ///     function will always generate the error <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBooleanv(ALenum param, ALboolean* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetBooleanv(int state, [Out] int *output);
        #endregion alGetBooleanv(int state, [Out] int *output)

        #region alGetBufferf(int buffer, int attribute, out int val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferf(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferf(int buffer, int attribute, out int val);
        #endregion alGetBufferf(int buffer, int attribute, out int val)

        #region alGetBufferf(int buffer, int attribute, [Out] int[] val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferf(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferf(int buffer, int attribute, [Out] int[] val);
        #endregion alGetBufferf(int buffer, int attribute, [Out] int[] val)

        #region alGetBufferf(int buffer, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferf(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferf(int buffer, int attribute, [Out] IntPtr val);
        #endregion alGetBufferf(int buffer, int attribute, [Out] IntPtr val)

        #region alGetBufferf(int buffer, int attribute, [Out] float *val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferf(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetBufferf(int buffer, int attribute, [Out] float *val);
        #endregion alGetBufferf(int buffer, int attribute, [Out] float *val)

        #region alGetBufferfv(int buffer, int attribute, out float val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferfv(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferfv(int buffer, int attribute, out float val);
        #endregion alGetBufferfv(int buffer, int attribute, out float val)

        #region alGetBufferfv(int buffer, int attribute, [Out] float[] val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferfv(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferfv(int buffer, int attribute, [Out] float[] val);
        #endregion alGetBufferfv(int buffer, int attribute, [Out] float[] val)

        #region alGetBufferfv(int buffer, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferfv(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferfv(int buffer, int attribute, [Out] IntPtr val);
        #endregion alGetBufferfv(int buffer, int attribute, [Out] IntPtr val)

        #region alGetBufferfv(int buffer, int attribute, [Out] float *val)
        /// <summary>
        ///     Retrieves a floating point property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     The name of the attribute to be retrieved.
        /// </param>
        /// <param name="val">
        ///     A pointer to an float to hold the retrieved data.
        /// </param>
        /// <remarks>
        ///     There are no float attributes for buffers at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetBufferfv(ALuint buffer, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetBufferfv(int buffer, int attribute, [Out] float *val);
        #endregion alGetBufferfv(int buffer, int attribute, [Out] float *val)

        #region alGetBufferi(int buffer, int attribute, out int val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferi(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferi(int buffer, int attribute, out int val);
        #endregion alGetBufferi(int buffer, int attribute, out int val)

        #region alGetBufferi(int buffer, int attribute, [Out] int[] val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferi(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferi(int buffer, int attribute, [Out] int[] val);
        #endregion alGetBufferi(int buffer, int attribute, [Out] int[] val)

        #region alGetBufferi(int buffer, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferi(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferi(int buffer, int attribute, [Out] IntPtr val);
        #endregion alGetBufferi(int buffer, int attribute, [Out] IntPtr val)

        #region alGetBufferi(int buffer, int attribute, [Out] int *val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferi(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetBufferi(int buffer, int attribute, [Out] int *val);
        #endregion alGetBufferi(int buffer, int attribute, [Out] int *val)

        #region alGetBufferiv(int buffer, int attribute, out int val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferiv(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferiv(int buffer, int attribute, out int val);
        #endregion alGetBufferiv(int buffer, int attribute, out int val)

        #region alGetBufferiv(int buffer, int attribute, [Out] int[] val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferiv(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferiv(int buffer, int attribute, [Out] int[] val);
        #endregion alGetBufferiv(int buffer, int attribute, [Out] int[] val)

        #region alGetBufferiv(int buffer, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferiv(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetBufferiv(int buffer, int attribute, [Out] IntPtr val);
        #endregion alGetBufferiv(int buffer, int attribute, [Out] IntPtr val)

        #region alGetBufferiv(int buffer, int attribute, [Out] int *val)
        /// <summary>
        ///     Retrieves an integer property of a buffer.
        /// </summary>
        /// <param name="buffer">
        ///     Buffer name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_FREQUENCY" /></item>
        ///             <item><see cref="AL_BITS" /></item>
        ///             <item><see cref="AL_CHANNELS" /></item>
        ///             <item><see cref="AL_SIZE" /></item>
        ///             <item><see cref="AL_DATA" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to an integer to hold the retrieved data.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetBufferiv(ALuint buffer, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetBufferiv(int buffer, int attribute, [Out] int *val);
        #endregion alGetBufferiv(int buffer, int attribute, [Out] int *val)

        #region double alGetdouble(int state)
        /// <summary>
        ///     Returns a double precision floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <returns>
        ///     The double value described by <i>state</i> will be returned.
        /// </returns>
        /// <remarks>
        ///     There aren’t any double precision floating point states defined at the time of
        ///     this writing, so this function will always generate the error
        ///     <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALdouble ALAPIENTRY alGetdouble(ALenum param);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern double alGetdouble(int state);
        #endregion double alGetdouble(int state)

        #region alGetdoublev(int state, out double output)
        /// <summary>
        ///     Retrieves a double precision floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any double precision floating point states defined at the time of
        ///     this writing, so this function will always generate the error
        ///     <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetdoublev(ALenum param, ALdouble* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetdoublev(int state, out double output);
        #endregion alGetdoublev(int state, out double output)

        #region alGetdoublev(int state, [Out] double[] output)
        /// <summary>
        ///     Retrieves a double precision floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any double precision floating point states defined at the time of
        ///     this writing, so this function will always generate the error
        ///     <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetdoublev(ALenum param, ALdouble* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetdoublev(int state, [Out] double[] output);
        #endregion alGetdoublev(int state, [Out] double[] output)

        #region alGetdoublev(int state, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves a double precision floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any double precision floating point states defined at the time of
        ///     this writing, so this function will always generate the error
        ///     <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetdoublev(ALenum param, ALdouble* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetdoublev(int state, [Out] IntPtr output);
        #endregion alGetdoublev(int state, [Out] IntPtr output)

        #region alGetdoublev(int state, [Out] double *output)
        /// <summary>
        ///     Retrieves a double precision floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     The state to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        /// <remarks>
        ///     There aren’t any double precision floating point states defined at the time of
        ///     this writing, so this function will always generate the error
        ///     <see cref="AL_INVALID_ENUM" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetdoublev(ALenum param, ALdouble* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetdoublev(int state, [Out] double *output);
        #endregion alGetdoublev(int state, [Out] double *output)

        #region int alGetEnumValue(string enumName)
        /// <summary>
        ///     Returns the enumeration value of an OpenAL enum described by a string.
        /// </summary>
        /// <param name="enumName">
        ///     A string describing an OpenAL enum.
        /// </param>
        /// <returns>
        ///     The actual value for the described enum is returned.
        /// </returns>
        // ALAPI ALenum ALAPIENTRY alGetEnumValue(ALubyte* ename);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGetEnumValue(string enumName);
        #endregion int alGetEnumValue(string enumName)

        #region int alGetError()
        /// <summary>
        ///     Returns the current error state and then clears the error state.
        /// </summary>
        /// <returns>
        ///     The error state.
        /// </returns>
        /// <remarks>
        ///     When an OpenAL error occurs, the error state is set and will not be changed until
        ///     the error state is retrieved using <b>alGetError</b>.  Whenever <b>alGetError</b>
        ///     is called, the error state is cleared and the last state (the current state when
        ///     the call was made) is returned.  To isolate error detection to a specific portion
        ///     of code, <b>alGetError</b> should be called before the isolated section to clear
        ///     the current error state.
        /// </remarks>
        // ALAPI ALenum ALAPIENTRY alGetError(ALvoid);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGetError();
        #endregion int alGetError()

        #region float alGetFloat(int state)
        /// <summary>
        ///     Returns a floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DOPPLER_FACTOR" /></item>
        ///             <item><see cref="AL_DOPPLER_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     The floating point value described by <i>state</i> will be returned.
        /// </returns>
        // ALAPI ALfloat ALAPIENTRY alGetFloat(ALenum param);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern float alGetFloat(int state);
        #endregion float alGetFloat(int state)

        #region alGetFloatv(int state, out float output)
        /// <summary>
        ///     Retrieves a floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DOPPLER_FACTOR" /></item>
        ///             <item><see cref="AL_DOPPLER_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetFloatv(ALenum param, ALfloat* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetFloatv(int state, out float output);
        #endregion alGetFloatv(int state, out float output)

        #region alGetFloatv(int state, [Out] float[] output)
        /// <summary>
        ///     Retrieves a floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DOPPLER_FACTOR" /></item>
        ///             <item><see cref="AL_DOPPLER_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetFloatv(ALenum param, ALfloat* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetFloatv(int state, [Out] float[] output);
        #endregion alGetFloatv(int state, [Out] float[] output)

        #region alGetFloatv(int state, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves a floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DOPPLER_FACTOR" /></item>
        ///             <item><see cref="AL_DOPPLER_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetFloatv(ALenum param, ALfloat* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetFloatv(int state, [Out] IntPtr output);
        #endregion alGetFloatv(int state, [Out] IntPtr output)

        #region alGetFloatv(int state, [Out] float *output)
        /// <summary>
        ///     Retrieves a floating point OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DOPPLER_FACTOR" /></item>
        ///             <item><see cref="AL_DOPPLER_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetFloatv(ALenum param, ALfloat* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetFloatv(int state, [Out] float *output);
        #endregion alGetFloatv(int state, [Out] float *output)

        #region int alGetInteger(int state)
        /// <summary>
        ///     Returns an integer OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DISTANCE_MODEL" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     The integer value described by <i>state</i> will be returned.
        /// </returns>
        // ALAPI ALint ALAPIENTRY alGetInteger(ALenum param);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGetInteger(int state);
        #endregion int alGetInteger(int state)

        #region alGetIntegerv(int state, out int output)
        /// <summary>
        ///     Retrieves an integer OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DISTANCE_MODEL" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetIntegerv(ALenum param, ALint* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetIntegerv(int state, out int output);
        #endregion alGetIntegerv(int state, out int output)

        #region alGetIntegerv(int state, [Out] int[] output)
        /// <summary>
        ///     Retrieves an integer OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DISTANCE_MODEL" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetIntegerv(ALenum param, ALint* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetIntegerv(int state, [Out] int[] output);
        #endregion alGetIntegerv(int state, [Out] int[] output)

        #region alGetIntegerv(int state, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves an integer OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DISTANCE_MODEL" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetIntegerv(ALenum param, ALint* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetIntegerv(int state, [Out] IntPtr output);
        #endregion alGetIntegerv(int state, [Out] IntPtr output)

        #region alGetIntegerv(int state, [Out] int *output)
        /// <summary>
        ///     Retrieves an integer OpenAL state.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The state to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_DISTANCE_MODEL" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the location where the state will be stored.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetIntegerv(ALenum param, ALint* data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetIntegerv(int state, [Out] int *output);
        #endregion alGetIntegerv(int state, [Out] int *output)

        #region alGetListener3f(int attribute, out float output1, out float output2, out float output3)
        /// <summary>
        ///     Retrieves a set of three floating point values from a property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output1">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output2">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output3">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListener3f(ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListener3f(int attribute, out float output1, out float output2, out float output3);
        #endregion alGetListener3f(int attribute, out float output1, out float output2, out float output3)

        #region alGetListener3f(int attribute, [Out] float[] output1, [Out] float[] output2, [Out] float[] output3)
        /// <summary>
        ///     Retrieves a set of three floating point values from a property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output1">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output2">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output3">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListener3f(ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListener3f(int attribute, [Out] float[] output1, [Out] float[] output2, [Out] float[] output3);
        #endregion alGetListener3f(int attribute, [Out] float[] output1, [Out] float[] output2, [Out] float[] output3)

        #region alGetListener3f(int attribute, [Out] IntPtr output1, [Out] IntPtr output2, [Out] IntPtr output3)
        /// <summary>
        ///     Retrieves a set of three floating point values from a property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output1">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output2">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output3">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListener3f(ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListener3f(int attribute, [Out] IntPtr output1, [Out] IntPtr output2, [Out] IntPtr output3);
        #endregion alGetListener3f(int attribute, [Out] IntPtr output1, [Out] IntPtr output2, [Out] IntPtr output3)

        #region alGetListener3f(int attribute, [Out] float *output1, [Out] float *output2, [Out] float *output3)
        /// <summary>
        ///     Retrieves a set of three floating point values from a property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output1">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output2">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        /// <param name="output3">
        ///     Pointer to the the floating point being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListener3f(ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetListener3f(int attribute, [Out] float *output1, [Out] float *output2, [Out] float *output3);
        #endregion alGetListener3f(int attribute, [Out] float *output1, [Out] float *output2, [Out] float *output3)

        #region alGetListenerf(int attribute, out float output)
        /// <summary>
        ///     Retrieves a floating point property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_GAIN" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerf(ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListenerf(int attribute, out float output);
        #endregion alGetListenerf(int attribute, out float output)

        #region alGetListenerf(int attribute, [Out] float[] output)
        /// <summary>
        ///     Retrieves a floating point property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_GAIN" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerf(ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListenerf(int attribute, [Out] float[] output);
        #endregion alGetListenerf(int attribute, [Out] float[] output)

        #region alGetListenerf(int attribute, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves a floating point property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_GAIN" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerf(ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListenerf(int attribute, [Out] IntPtr output);
        #endregion alGetListenerf(int attribute, [Out] IntPtr output)

        #region alGetListenerf(int attribute, [Out] float *output)
        /// <summary>
        ///     Retrieves a floating point property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_GAIN" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerf(ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetListenerf(int attribute, [Out] float *output);
        #endregion alGetListenerf(int attribute, [Out] float *output)

        #region alGetListenerfv(int attribute, out float output)
        /// <summary>
        ///     Retrieves a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point-vector value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListenerfv(int attribute, out float output);
        #endregion alGetListenerfv(int attribute, out float output)

        #region alGetListenerfv(int attribute, [Out] float[] output)
        /// <summary>
        ///     Retrieves a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point-vector value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListenerfv(int attribute, [Out] float[] output);
        #endregion alGetListenerfv(int attribute, [Out] float[] output)

        #region alGetListenerfv(int attribute, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point-vector value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListenerfv(int attribute, [Out] IntPtr output);
        #endregion alGetListenerfv(int attribute, [Out] IntPtr output)

        #region alGetListenerfv(int attribute, [Out] float *output)
        /// <summary>
        ///     Retrieves a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="output">
        ///     A pointer to the floating point-vector value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetListenerfv(int attribute, [Out] float *output);
        #endregion alGetListenerfv(int attribute, [Out] float *output)

        #region alGetListeneri(int attribute, out int output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneri(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListeneri(int attribute, out int output);
        #endregion alGetListeneri(int attribute, out int output)

        #region alGetListeneri(int attribute, [Out] int[] output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneri(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListeneri(int attribute, [Out] int[] output);
        #endregion alGetListeneri(int attribute, [Out] int[] output)

        #region alGetListeneri(int attribute, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneri(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListeneri(int attribute, [Out] IntPtr output);
        #endregion alGetListeneri(int attribute, [Out] IntPtr output)

        #region alGetListeneri(int attribute, [Out] int *output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneri(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetListeneri(int attribute, [Out] int *output);
        #endregion alGetListeneri(int attribute, [Out] int *output)

        #region alGetListeneriv(int attribute, out int output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneriv(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListeneriv(int attribute, out int output);
        #endregion alGetListeneriv(int attribute, out int output)

        #region alGetListeneriv(int attribute, [Out] int[] output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneriv(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListeneriv(int attribute, [Out] int[] output);
        #endregion alGetListeneriv(int attribute, [Out] int[] output)

        #region alGetListeneriv(int attribute, [Out] IntPtr output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneriv(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetListeneriv(int attribute, [Out] IntPtr output);
        #endregion alGetListeneriv(int attribute, [Out] IntPtr output)

        #region alGetListeneriv(int attribute, [Out] int *output)
        /// <summary>
        ///     Retrieves an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be queried.
        /// </param>
        /// <param name="output">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alGetListeneriv(ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetListeneriv(int attribute, [Out] int *output);
        #endregion alGetListeneriv(int attribute, [Out] int *output)

        #region IntPtr alGetProcAddress(string functionName)
        /// <summary>
        ///     Returns the address of an OpenAL extension function.
        /// </summary>
        /// <param name="functionName">
        ///     A string containing the function name.
        /// </param>
        /// <returns>
        ///     A pointer to the desired function is returned.
        /// </returns>
        /// <remarks>
        ///     The return value will be IntPtr.Zero if the function is not found.
        /// </remarks>
        // ALAPI ALvoid* ALAPIENTRY alGetProcAddress(ALubyte* fname);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alGetProcAddress(string functionName);
        #endregion IntPtr alGetProcAddress(string functionName)

        #region alGetSource3f(int source, int attribute, out float value1, out float value2, out float value3)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="value1">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value2">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value3">
        ///     The float values which the attribute will be set to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSource3f(ALuint source,  ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSource3f(int source, int attribute, out float value1, out float value2, out float value3);
        #endregion alGetSource3f(int source, int attribute, out float value1, out float value2, out float value3)

        #region alGetSource3f(int source, int attribute, [Out] float[] value1, [Out] float[] value2, [Out] float[] value3)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="value1">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value2">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value3">
        ///     The float values which the attribute will be set to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSource3f(ALuint source,  ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSource3f(int source, int attribute, [Out] float[] value1, [Out] float[] value2, [Out] float[] value3);
        #endregion alGetSource3f(int source, int attribute, [Out] float[] value1, [Out] float[] value2, [Out] float[] value3)

        #region alGetSource3f(int source, int attribute, [Out] IntPtr value1, [Out] IntPtr value2, [Out] IntPtr value3)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="value1">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value2">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value3">
        ///     The float values which the attribute will be set to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSource3f(ALuint source,  ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSource3f(int source, int attribute, [Out] IntPtr value1, [Out] IntPtr value2, [Out] IntPtr value3);
        #endregion alGetSource3f(int source, int attribute, [Out] IntPtr value1, [Out] IntPtr value2, [Out] IntPtr value3)

        #region alGetSource3f(int source, int attribute, [Out] float *value1, [Out] float *value2, [Out] float *value3)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="value1">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value2">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value3">
        ///     The float values which the attribute will be set to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSource3f(ALuint source,  ALenum param, ALfloat* v1, ALfloat* v2, ALfloat* v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetSource3f(int source, int attribute, [Out] float *value1, [Out] float *value2, [Out] float *value3);
        #endregion alGetSource3f(int source, int attribute, [Out] float *value1, [Out] float *value2, [Out] float *value3)

        #region alGetSourcef(int source, int attribute, out float val)
        /// <summary>
        ///     Retrieves a floating point property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_PITCH" /></item>
        ///             <item><see cref="AL_GAIN" /></item>
        ///             <item><see cref="AL_MIN_GAIN" /></item>
        ///             <item><see cref="AL_MAX_GAIN" /></item>
        ///             <item><see cref="AL_MAX_DISTANCE" /></item>
        ///             <item><see cref="AL_ROLLOFF_FACTOR" /></item>
        ///             <item><see cref="AL_CONE_OUTER_GAIN" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_REFERENCE_DISTANCE" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcef(ALuint source, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcef(int source, int attribute, out float val);
        #endregion alGetSourcef(int source, int attribute, out float val)

        #region alGetSourcef(int source, int attribute, [Out] float[] val)
        /// <summary>
        ///     Retrieves a floating point property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_PITCH" /></item>
        ///             <item><see cref="AL_GAIN" /></item>
        ///             <item><see cref="AL_MIN_GAIN" /></item>
        ///             <item><see cref="AL_MAX_GAIN" /></item>
        ///             <item><see cref="AL_MAX_DISTANCE" /></item>
        ///             <item><see cref="AL_ROLLOFF_FACTOR" /></item>
        ///             <item><see cref="AL_CONE_OUTER_GAIN" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_REFERENCE_DISTANCE" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcef(ALuint source, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcef(int source, int attribute, [Out] float[] val);
        #endregion alGetSourcef(int source, int attribute, [Out] float[] val)

        #region alGetSourcef(int source, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves a floating point property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_PITCH" /></item>
        ///             <item><see cref="AL_GAIN" /></item>
        ///             <item><see cref="AL_MIN_GAIN" /></item>
        ///             <item><see cref="AL_MAX_GAIN" /></item>
        ///             <item><see cref="AL_MAX_DISTANCE" /></item>
        ///             <item><see cref="AL_ROLLOFF_FACTOR" /></item>
        ///             <item><see cref="AL_CONE_OUTER_GAIN" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_REFERENCE_DISTANCE" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcef(ALuint source, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcef(int source, int attribute, [Out] IntPtr val);
        #endregion alGetSourcef(int source, int attribute, [Out] IntPtr val)

        #region alGetSourcef(int source, int attribute, [Out] float *val)
        /// <summary>
        ///     Retrieves a floating point property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_PITCH" /></item>
        ///             <item><see cref="AL_GAIN" /></item>
        ///             <item><see cref="AL_MIN_GAIN" /></item>
        ///             <item><see cref="AL_MAX_GAIN" /></item>
        ///             <item><see cref="AL_MAX_DISTANCE" /></item>
        ///             <item><see cref="AL_ROLLOFF_FACTOR" /></item>
        ///             <item><see cref="AL_CONE_OUTER_GAIN" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_REFERENCE_DISTANCE" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the floating point value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcef(ALuint source, ALenum param, ALfloat* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetSourcef(int source, int attribute, [Out] float *val);
        #endregion alGetSourcef(int source, int attribute, [Out] float *val)

        #region alGetSourcefv(int source, int attribute, out float val)
        /// <summary>
        ///     Retrieves a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the vector to retrieve.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcefv(int source, int attribute, out float val);
        #endregion alGetSourcefv(int source, int attribute, out float val)

        #region alGetSourcefv(int source, int attribute, [Out] float[] values)
        /// <summary>
        ///     Retrieves a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to retrieve.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcefv(int source, int attribute, [Out] float[] values);
        #endregion alGetSourcefv(int source, int attribute, [Out] float[] values)

        #region alGetSourcefv(int source, int attribute, [Out] IntPtr values)
        /// <summary>
        ///     Retrieves a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to retrieve.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcefv(int source, int attribute, [Out] IntPtr values);
        #endregion alGetSourcefv(int source, int attribute, [Out] IntPtr values)

        #region alGetSourcefv(int source, int attribute, [Out] float *values)
        /// <summary>
        ///     Retrieves a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to retrieve.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetSourcefv(int source, int attribute, [Out] float *values);
        #endregion alGetSourcefv(int source, int attribute, [Out] float *values)

        #region alGetSourcei(int source, int attribute, out int val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_LOOPING" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcei(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcei(int source, int attribute, out int val);
        #endregion alGetSourcei(int source, int attribute, out int val)

        #region alGetSourcei(int source, int attribute, [Out] int[] val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcei(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcei(int source, int attribute, [Out] int[] val);
        #endregion alGetSourcei(int source, int attribute, [Out] int[] val)

        #region alGetSourcei(int source, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcei(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourcei(int source, int attribute, [Out] IntPtr val);
        #endregion alGetSourcei(int source, int attribute, [Out] IntPtr val)

        #region alGetSourcei(int source, int attribute, [Out] int *val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourcei(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetSourcei(int source, int attribute, [Out] int *val);
        #endregion alGetSourcei(int source, int attribute, [Out] int *val)

        #region alGetSourceiv(int source, int attribute, out int val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_LOOPING" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourceiv(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourceiv(int source, int attribute, out int val);
        #endregion alGetSourceiv(int source, int attribute, out int val)

        #region alGetSourceiv(int source, int attribute, [Out] int[] val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourceiv(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourceiv(int source, int attribute, [Out] int[] val);
        #endregion alGetSourceiv(int source, int attribute, [Out] int[] val)

        #region alGetSourceiv(int source, int attribute, [Out] IntPtr val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourceiv(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alGetSourceiv(int source, int attribute, [Out] IntPtr val);
        #endregion alGetSourceiv(int source, int attribute, [Out] IntPtr val)

        #region alGetSourceiv(int source, int attribute, [Out] int *val)
        /// <summary>
        ///     Retrieves an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being retrieved.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to retrieve:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///             <item><see cref="AL_BUFFERS_QUEUED" /></item>
        ///             <item><see cref="AL_BUFFERS_PROCESSED" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     A pointer to the integer value being retrieved.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alGetSourceiv(ALuint source, ALenum param, ALint* value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alGetSourceiv(int source, int attribute, [Out] int *val);
        #endregion alGetSourceiv(int source, int attribute, [Out] int *val)

        #region string alGetString(int state)
        /// <summary>
        ///     Retrieves an OpenAL string property.
        /// </summary>
        /// <param name="state">
        ///     <para>
        ///         The property to be queried:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_VENDOR" /></item>
        ///             <item><see cref="AL_VERSION" /></item>
        ///             <item><see cref="AL_RENDERER" /></item>
        ///             <item><see cref="AL_EXTENSIONS" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     A pointer to a null-terminated string.
        /// </returns>
        // ALAPI ALubyte* ALAPIENTRY alGetString(ALenum param);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern string alGetString(int state);
        #endregion string alGetString(int state)

        #region alHint(int target, int mode)
        /// <summary>
        ///     Sets application preferences for driver performance choices.
        /// </summary>
        /// <param name="target">
        ///     Unknown.
        /// </param>
        /// <param name="mode">
        ///     Unknown.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alHint(ALenum target, ALenum mode);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alHint(int target, int mode);
        #endregion alHint(int target, int mode)

        #region int alIsBuffer(int buffer)
        /// <summary>
        ///     Tests if a buffer name is valid.
        /// </summary>
        /// <param name="buffer">
        ///     A buffer name to be tested for validity.
        /// </param>
        /// <returns>
        ///     bool value <see cref="AL_TRUE" /> if the buffer name is valid or
        ///     <see cref="AL_FALSE" /> if the buffer name is not valid.
        /// </returns>
        // ALAPI ALboolean ALAPIENTRY alIsBuffer(ALuint buffer);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alIsBuffer(int buffer);
        #endregion int alIsBuffer(int buffer)

        #region int alIsEnabled(int capability)
        /// <summary>
        ///     Returns a value indicating if a specific feature is enabled in the OpenAL driver.
        /// </summary>
        /// <param name="capability">
        ///     The capability to query.
        /// </param>
        /// <returns>
        ///     <see cref="AL_TRUE" /> if the capability is enabled, <see cref="AL_FALSE" /> if
        ///     the capability is disabled.
        /// </returns>
        /// <remarks>
        ///     At the time of this writing, this function always returns <see cref="AL_FALSE" />,
        ///     and since there are no capabilities defined yet, the error
        ///     <see cref="AL_INVALID_ENUM" /> will also be set.
        /// </remarks>
        // ALAPI ALboolean ALAPIENTRY alIsEnabled(ALenum capability);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alIsEnabled(int capability);
        #endregion int alIsEnabled(int capability)

        #region int alIsExtensionPresent(string extensionName)
        /// <summary>
        ///     Tests if a specific extension is available for the OpenAL driver.
        /// </summary>
        /// <param name="extensionName">
        ///     A string describing the desired extension.
        /// </param>
        /// <returns>
        ///     <see cref="AL_TRUE" /> if the extension is available, <see cref="AL_FALSE" /> if
        ///     the extension is not available.
        /// </returns>
        // ALAPI ALboolean ALAPIENTRY alIsExtensionPresent(ALubyte* fname);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alIsExtensionPresent(string extensionName);
        #endregion int alIsExtensionPresent(String extensionName)

        #region int alIsSource(int id)
        /// <summary>
        ///     Tests if a source name is valid.
        /// </summary>
        /// <param name="id">
        ///     A source name to be tested for validity.
        /// </param>
        /// <returns>
        ///     bool value <see cref="AL_TRUE" /> if the source name is valid or
        ///     <see cref="AL_FALSE" /> if the source name is not valid.
        /// </returns>
        // ALAPI ALboolean ALAPIENTRY alIsSource(ALuint id);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alIsSource(int id);
        #endregion int alIsSource(int id)

        #region alListener3f(int attribute, float value1, float value2, float value3)
        /// <summary>
        ///     Sets a floating point property for the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="value1">
        ///     The value to set the attribute to.
        /// </param>
        /// <param name="value2">
        ///     The value to set the attribute to.
        /// </param>
        /// <param name="value3">
        ///     The value to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alListener3f(ALenum param, ALfloat v1, ALfloat v2, ALfloat v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alListener3f(int attribute, float value1, float value2, float value3);
        #endregion alListener3f(int attribute, float value1, float value2, float value3)

        #region alListenerf(int attribute, float val)
        /// <summary>
        ///     Sets a floating point property for the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be set.
        /// </param>
        /// <param name="val">
        ///     The float value to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alListenerf(ALenum param, ALfloat value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alListenerf(int attribute, float val);
        #endregion alListenerf(int attribute, float val)

        #region alListenerfv(int attribute, [In] ref float values)
        /// <summary>
        ///     Sets a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     Pointer to floating point-vector values.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alListenerfv(int attribute, [In] ref float values);
        #endregion alListenerfv(int attribute, [In] ref float values)

        #region alListenerfv(int attribute, [In] float[] values)
        /// <summary>
        ///     Sets a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     Pointer to floating point-vector values.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alListenerfv(int attribute, [In] float[] values);
        #endregion alListenerfv(int attribute, [In] float[] values)

        #region alListenerfv(int attribute, [In] IntPtr values)
        /// <summary>
        ///     Sets a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     Pointer to floating point-vector values.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alListenerfv(int attribute, [In] IntPtr values);
        #endregion alListenerfv(int attribute, [In] IntPtr values)

        #region alListenerfv(int attribute, [In] float *values)
        /// <summary>
        ///     Sets a floating point-vector property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to be set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_ORIENTATION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     Pointer to floating point-vector values.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alListenerfv(ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alListenerfv(int attribute, [In] float *values);
        #endregion alListenerfv(int attribute, [In] float *values)

        #region alListeneri(int attribute, int val)
        /// <summary>
        ///     Sets an integer property of the listener.
        /// </summary>
        /// <param name="attribute">
        ///     The name of the attribute to be set.
        /// </param>
        /// <param name="val">
        ///     The integer value to set the attribute to.
        /// </param>
        /// <remarks>
        ///     There are no integer listener attributes at this time.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alListeneri(ALenum param, ALint value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alListeneri(int attribute, int val);
        #endregion alListeneri(int attribute, int val)

        #region alQueuei(int source, int attribute, int val)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="source">
        ///     Unknown.
        /// </param>
        /// <param name="attribute">
        ///     Unknown.
        /// </param>
        /// <param name="val">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alQueuei(ALuint sid, ALenum param, ALint value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alQueuei(int source, int attribute, int val);
        #endregion alQueuei(int source, int attribute, int val)

        #region alSource3f(int source, int attribute, float value1, float value2, float value3)
        /// <summary>
        ///     Sets a source property requiring three floating point values.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="value1">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value2">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <param name="value3">
        ///     The float values which the attribute will be set to.
        /// </param>
        /// <remarks>
        ///     This function is an alternative to <see cref="alSourcefv" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSource3f(ALuint source, ALenum param, ALfloat v1, ALfloat v2, ALfloat v3);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSource3f(int source, int attribute, float value1, float value2, float value3);
        #endregion alSource3f(int source, int attribute, float value1, float value2, float value3)

        #region alSourcef(int source, int attribute, float val)
        /// <summary>
        ///     Sets a floating point property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_PITCH" /></item>
        ///             <item><see cref="AL_GAIN" /></item>
        ///             <item><see cref="AL_MAX_DISTANCE" /></item>
        ///             <item><see cref="AL_ROLLOFF_FACTOR" /></item>
        ///             <item><see cref="AL_REFERENCE_DISTANCE" /></item>
        ///             <item><see cref="AL_MIN_GAIN" /></item>
        ///             <item><see cref="AL_MAX_GAIN" /></item>
        ///             <item><see cref="AL_CONE_OUTER_GAIN" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     The value to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourcef(ALuint source, ALenum param, ALfloat value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcef(int source, int attribute, float val);
        #endregion alSourcef(int source, int attribute, float val)

        #region alSourcefv(int source, int attribute, [In] ref float values)
        /// <summary>
        ///     Sets a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcefv(int source, int attribute, [In] ref float values);
        #endregion alSourcefv(int source, int attribute, float[] values)

        #region alSourcefv(int source, int attribute, [In] float[] values)
        /// <summary>
        ///     Sets a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcefv(int source, int attribute, [In] float[] values);
        #endregion alSourcefv(int source, int attribute, [In] float[] values)

        #region alSourcefv(int source, int attribute, [In] IntPtr values)
        /// <summary>
        ///     Sets a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcefv(int source, int attribute, [In] IntPtr values);
        #endregion alSourcefv(int source, int attribute, [In] IntPtr values)

        #region alSourcefv(int source, int attribute, [In] float *values)
        /// <summary>
        ///     Sets a floating point-vector property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute being set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_POSITION" /></item>
        ///             <item><see cref="AL_VELOCITY" /></item>
        ///             <item><see cref="AL_DIRECTION" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="values">
        ///     A pointer to the vector to set the attribute to.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourcefv(ALuint source, ALenum param, ALfloat* values);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourcefv(int source, int attribute, [In] float *values);
        #endregion alSourcefv(int source, int attribute, [In] float *values)

        #region alSourcei(int source, int attribute, int val)
        /// <summary>
        ///     Sets an integer property of a source.
        /// </summary>
        /// <param name="source">
        ///     Source name whose attribute is being set.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         The name of the attribute to set:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="AL_SOURCE_RELATIVE" /></item>
        ///             <item><see cref="AL_CONE_INNER_ANGLE" /></item>
        ///             <item><see cref="AL_CONE_OUTER_ANGLE" /></item>
        ///             <item><see cref="AL_LOOPING" /></item>
        ///             <item><see cref="AL_BUFFER" /></item>
        ///             <item><see cref="AL_SOURCE_STATE" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="val">
        ///     The value to set the attribute to.
        /// </param>
        /// <remarks>
        ///     The buffer name zero is reserved as a “Null Buffer" and is accepted by
        ///     <b>alSourcei(…, Al.AL_BUFFER, …)</b> as a valid buffer of zero length.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcei(ALuint source, ALenum param, ALint value);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcei(int source, int attribute, int val);
        #endregion alSourcei(int source, int attribute, int val)

        #region alSourcePause(int source)
        /// <summary>
        ///     Pauses a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to be paused.
        /// </param>
        /// <remarks>
        ///     The paused source will have its state changed to <see cref="AL_PAUSED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePause(ALuint source);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePause(int source);
        #endregion alSourcePause(int source)

        #region alSourcePausev(int number, [In] ref int source)
        /// <summary>
        ///     Pauses a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be paused.
        /// </param>
        /// <param name="source">
        ///     A pointer to an array of sources to be paused.
        /// </param>
        /// <remarks>
        ///     The paused sources will have their state changed to <see cref="AL_PAUSED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePausev(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePausev(int number, [In] ref int source);
        #endregion alSourcePausev(int number, [In] ref int source)

        #region alSourcePausev(int number, [In] int[] sources)
        /// <summary>
        ///     Pauses a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be paused.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be paused.
        /// </param>
        /// <remarks>
        ///     The paused sources will have their state changed to <see cref="AL_PAUSED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePausev(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePausev(int number, [In] int[] sources);
        #endregion alSourcePausev(int number, [In] int[] sources)

        #region alSourcePausev(int number, [In] IntPtr sources)
        /// <summary>
        ///     Pauses a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be paused.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be paused.
        /// </param>
        /// <remarks>
        ///     The paused sources will have their state changed to <see cref="AL_PAUSED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePausev(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePausev(int number, [In] IntPtr sources);
        #endregion alSourcePausev(int number, [In] IntPtr sources)

        #region alSourcePausev(int number, [In] int *sources)
        /// <summary>
        ///     Pauses a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be paused.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be paused.
        /// </param>
        /// <remarks>
        ///     The paused sources will have their state changed to <see cref="AL_PAUSED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePausev(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourcePausev(int number, [In] int *sources);
        #endregion alSourcePausev(int number, [In] int *sources)

        #region alSourcePlay(int source)
        /// <summary>
        ///     Plays a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to be played.
        /// </param>
        /// <remarks>
        ///     The playing source will have its state changed to <see cref="AL_PLAYING" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePlay(ALuint source);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePlay(int source);
        #endregion alSourcePlay(int source)

        #region alSourcePlayv(int number, [In] ref int source)
        /// <summary>
        ///     Plays a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be played.
        /// </param>
        /// <param name="source">
        ///     A pointer to an array of sources to be played.
        /// </param>
        /// <remarks>
        ///     The playing sources will have their state changed to <see cref="AL_PLAYING" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePlayv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePlayv(int number, [In] ref int source);
        #endregion alSourcePlayv(int number, [In] ref int source)

        #region alSourcePlayv(int number, [In] int[] sources)
        /// <summary>
        ///     Plays a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be played.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be played.
        /// </param>
        /// <remarks>
        ///     The playing sources will have their state changed to <see cref="AL_PLAYING" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePlayv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePlayv(int number, [In] int[] sources);
        #endregion alSourcePlayv(int number, [In] int[] sources)

        #region alSourcePlayv(int number, [In] IntPtr sources)
        /// <summary>
        ///     Plays a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be played.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be played.
        /// </param>
        /// <remarks>
        ///     The playing sources will have their state changed to <see cref="AL_PLAYING" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePlayv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourcePlayv(int number, [In] IntPtr sources);
        #endregion alSourcePlayv(int number, [In] IntPtr sources)

        #region alSourcePlayv(int number, [In] int *sources)
        /// <summary>
        ///     Plays a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be played.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be played.
        /// </param>
        /// <remarks>
        ///     The playing sources will have their state changed to <see cref="AL_PLAYING" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourcePlayv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourcePlayv(int number, [In] int *sources);
        #endregion alSourcePlayv(int number, [In] int *sources)

        #region alSourceQueueBuffers(int source, int number, [In] ref int buffer)
        /// <summary>
        ///     Queues a set of buffers on a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to queue buffers onto.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be queued.
        /// </param>
        /// <param name="buffer">
        ///     A pointer to an array of buffer names to be queued.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceQueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceQueueBuffers(int source, int number, [In] ref int buffer);
        #endregion alSourceQueueBuffers(int source, int number, [In] ref int buffer)

        #region alSourceQueueBuffers(int source, int number, [In] int[] buffers)
        /// <summary>
        ///     Queues a set of buffers on a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to queue buffers onto.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be queued.
        /// </param>
        /// <param name="buffers">
        ///     A pointer to an array of buffer names to be queued.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceQueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceQueueBuffers(int source, int number, [In] int[] buffers);
        #endregion alSourceQueueBuffers(int source, int number, [In] int[] buffers)

        #region alSourceQueueBuffers(int source, int number, [In] IntPtr buffers)
        /// <summary>
        ///     Queues a set of buffers on a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to queue buffers onto.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be queued.
        /// </param>
        /// <param name="buffers">
        ///     A pointer to an array of buffer names to be queued.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceQueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceQueueBuffers(int source, int number, [In] IntPtr buffers);
        #endregion alSourceQueueBuffers(int source, int number, [In] IntPtr buffers)

        #region alSourceQueueBuffers(int source, int number, [In] int *buffers)
        /// <summary>
        ///     Queues a set of buffers on a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to queue buffers onto.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be queued.
        /// </param>
        /// <param name="buffers">
        ///     A pointer to an array of buffer names to be queued.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceQueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourceQueueBuffers(int source, int number, [In] int *buffers);
        #endregion alSourceQueueBuffers(int source, int number, [In] int *buffers)

        #region alSourceRewind(int source)
        /// <summary>
        ///     Stops the source and sets its state to <see cref="AL_INITIAL" />.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to be rewound.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceRewind(ALuint source);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceRewind(int source);
        #endregion alSourceRewind(int source)

        #region alSourceRewindv(int number, [In] ref int source)
        /// <summary>
        ///     Stops a set of sources and sets all their states to <see cref="AL_INITIAL" />.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be rewound.
        /// </param>
        /// <param name="source">
        ///     A pointer to an array of sources to be rewound.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceRewindv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceRewindv(int number, [In] ref int source);
        #endregion alSourceRewindv(int number, [In] ref int source)

        #region alSourceRewindv(int number, [In] int[] sources)
        /// <summary>
        ///     Stops a set of sources and sets all their states to <see cref="AL_INITIAL" />.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be rewound.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be rewound.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceRewindv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceRewindv(int number, [In] int[] sources);
        #endregion alSourceRewindv(int number, [In] int[] sources)

        #region alSourceRewindv(int number, [In] IntPtr sources)
        /// <summary>
        ///     Stops a set of sources and sets all their states to <see cref="AL_INITIAL" />.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be rewound.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be rewound.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceRewindv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceRewindv(int number, [In] IntPtr sources);
        #endregion alSourceRewindv(int number, [In] IntPtr sources)

        #region alSourceRewindv(int number, [In] int *sources)
        /// <summary>
        ///     Stops a set of sources and sets all their states to <see cref="AL_INITIAL" />.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to be rewound.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be rewound.
        /// </param>
        // ALAPI ALvoid ALAPIENTRY alSourceRewindv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourceRewindv(int number, [In] int *sources);
        #endregion alSourceRewindv(int number, [In] int *sources)

        #region alSourceStop(int source)
        /// <summary>
        ///     Stops a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to be stopped.
        /// </param>
        /// <remarks>
        ///     The stopped source will have its state changed to <see cref="AL_STOPPED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceStop(ALuint source);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceStop(int source);
        #endregion alSourceStop(int source)

        #region alSourceStopv(int number, [In] ref int source)
        /// <summary>
        ///     Stops a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to stop.
        /// </param>
        /// <param name="source">
        ///     A pointer to an array of sources to be stopped.
        /// </param>
        /// <remarks>
        ///     The stopped sources will have their state changed to <see cref="AL_STOPPED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceStopv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceStopv(int number, [In] ref int source);
        #endregion alSourceStopv(int number, [In] ref int source)

        #region alSourceStopv(int number, [In] int[] sources)
        /// <summary>
        ///     Stops a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to stop.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be stopped.
        /// </param>
        /// <remarks>
        ///     The stopped sources will have their state changed to <see cref="AL_STOPPED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceStopv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceStopv(int number, [In] int[] sources);
        #endregion alSourceStopv(int number, [In] int[] sources)

        #region alSourceStopv(int number, [In] IntPtr sources)
        /// <summary>
        ///     Stops a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to stop.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be stopped.
        /// </param>
        /// <remarks>
        ///     The stopped sources will have their state changed to <see cref="AL_STOPPED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceStopv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceStopv(int number, [In] IntPtr sources);
        #endregion alSourceStopv(int number, [In] IntPtr sources)

        #region alSourceStopv(int number, [In] int *sources)
        /// <summary>
        ///     Stops a set of sources.
        /// </summary>
        /// <param name="number">
        ///     The number of sources to stop.
        /// </param>
        /// <param name="sources">
        ///     A pointer to an array of sources to be stopped.
        /// </param>
        /// <remarks>
        ///     The stopped sources will have their state changed to <see cref="AL_STOPPED" />.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceStopv(ALsizei n, ALuint *sources);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourceStopv(int number, [In] int *sources);
        #endregion alSourceStopv(int number, [In] int *sources)

        #region alSourceUnqueueBuffers(int source, int number, [In] ref int buffer)
        /// <summary>
        ///     Unqueues a set of buffers attached to a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to unqueue buffers from.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be unqueued.
        /// </param>
        /// <param name="buffer">
        ///     A pointer to an array of buffer names that were removed.
        /// </param>
        /// <remarks>
        ///     The unqueue operation will only take place if all <i>number</i> buffers can be
        ///     removed from the queue.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceUnqueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceUnqueueBuffers(int source, int number, [In] ref int buffer);
        #endregion alSourceUnqueueBuffers(int source, int number, [In] ref int buffer)

        #region alSourceUnqueueBuffers(int source, int number, [In] int[] buffers)
        /// <summary>
        ///     Unqueues a set of buffers attached to a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to unqueue buffers from.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be unqueued.
        /// </param>
        /// <param name="buffers">
        ///     A pointer to an array of buffer names that were removed.
        /// </param>
        /// <remarks>
        ///     The unqueue operation will only take place if all <i>number</i> buffers can be
        ///     removed from the queue.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceUnqueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceUnqueueBuffers(int source, int number, [In] int[] buffers);
        #endregion alSourceUnqueueBuffers(int source, int number, [In] int[] buffers)

        #region alSourceUnqueueBuffers(int source, int number, [In] IntPtr buffers)
        /// <summary>
        ///     Unqueues a set of buffers attached to a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to unqueue buffers from.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be unqueued.
        /// </param>
        /// <param name="buffers">
        ///     A pointer to an array of buffer names that were removed.
        /// </param>
        /// <remarks>
        ///     The unqueue operation will only take place if all <i>number</i> buffers can be
        ///     removed from the queue.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceUnqueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alSourceUnqueueBuffers(int source, int number, [In] IntPtr buffers);
        #endregion alSourceUnqueueBuffers(int source, int number, [In] IntPtr buffers)

        #region alSourceUnqueueBuffers(int source, int number, [In] int *buffers)
        /// <summary>
        ///     Unqueues a set of buffers attached to a source.
        /// </summary>
        /// <param name="source">
        ///     The name of the source to unqueue buffers from.
        /// </param>
        /// <param name="number">
        ///     The number of buffers to be unqueued.
        /// </param>
        /// <param name="buffers">
        ///     A pointer to an array of buffer names that were removed.
        /// </param>
        /// <remarks>
        ///     The unqueue operation will only take place if all <i>number</i> buffers can be
        ///     removed from the queue.
        /// </remarks>
        // ALAPI ALvoid ALAPIENTRY alSourceUnqueueBuffers(ALuint source, ALsizei n, ALuint* buffers);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alSourceUnqueueBuffers(int source, int number, [In] int *buffers);
        #endregion alSourceUnqueueBuffers(int source, int number, [In] int *buffers)
        #endregion Public OpenAL 1.0 Methods

        #region Public IASIG Methods
        #region int alGenEnvironmentIASIG(int number, out int environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        /// <returns>
        ///     Unknown.
        /// </returns>
        // ALAPI ALsizei ALAPIENTRY alGenEnvironmentIASIG(ALsizei n, ALuint* environs);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGenEnvironmentIASIG(int number, out int environments);
        #endregion int alGenEnvironmentIASIG(int number, out int environments)

        #region int alGenEnvironmentIASIG(int number, [Out] int[] environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        /// <returns>
        ///     Unknown.
        /// </returns>
        // ALAPI ALsizei ALAPIENTRY alGenEnvironmentIASIG(ALsizei n, ALuint* environs);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGenEnvironmentIASIG(int number, [Out] int[] environments);
        #endregion int alGenEnvironmentIASIG(int number, [Out] int[] environments)

        #region int alGenEnvironmentIASIG(int number, [Out] IntPtr environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        /// <returns>
        ///     Unknown.
        /// </returns>
        // ALAPI ALsizei ALAPIENTRY alGenEnvironmentIASIG(ALsizei n, ALuint* environs);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alGenEnvironmentIASIG(int number, [Out] IntPtr environments);
        #endregion int alGenEnvironmentIASIG(int number, [Out] IntPtr environments)

        #region int alGenEnvironmentIASIG(int number, [Out] int *environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        /// <returns>
        ///     Unknown.
        /// </returns>
        // ALAPI ALsizei ALAPIENTRY alGenEnvironmentIASIG(ALsizei n, ALuint* environs);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern int alGenEnvironmentIASIG(int number, [Out] int *environments);
        #endregion int alGenEnvironmentIASIG(int number, [Out] int *environments)

        #region alDeleteEnvironmentIASIG(int number, [In] ref int environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alDeleteEnvironmentIASIG( ALsizei n, ALuint* environs );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteEnvironmentIASIG(int number, [In] ref int environments);
        #endregion alDeleteEnvironmentIASIG(int number, [In] ref int environments)

        #region alDeleteEnvironmentIASIG(int number, [In] int[] environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alDeleteEnvironmentIASIG( ALsizei n, ALuint* environs );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteEnvironmentIASIG(int number, [In] int[] environments);
        #endregion alDeleteEnvironmentIASIG(int number, [In] int[] environments)

        #region alDeleteEnvironmentIASIG(int number, [In] IntPtr environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alDeleteEnvironmentIASIG( ALsizei n, ALuint* environs );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alDeleteEnvironmentIASIG(int number, [In] IntPtr environments);
        #endregion alDeleteEnvironmentIASIG(int number, [In] IntPtr environments)

        #region alDeleteEnvironmentIASIG(int number, [In] int *environments)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="number">
        ///     Unknown.
        /// </param>
        /// <param name="environments">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alDeleteEnvironmentIASIG( ALsizei n, ALuint* environs );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alDeleteEnvironmentIASIG(int number, [In] int *environments);
        #endregion alDeleteEnvironmentIASIG(int number, [In] int *environments)

        #region int alIsEnvironmentIASIG(int environment)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="environment">
        ///     Unknown.
        /// </param>
        /// <returns>
        ///     Unknown.
        /// </returns>
        // ALAPI ALboolean ALAPIENTRY alIsEnvironmentIASIG( ALuint environ );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alIsEnvironmentIASIG(int environment);
        #endregion int alIsEnvironmentIASIG(int environment)

        #region alEnvironmentiIASIG(int environmentId, int attribute, int val)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="environmentId">
        ///     Unknown.
        /// </param>
        /// <param name="attribute">
        ///     Unknown.
        /// </param>
        /// <param name="val">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alEnvironmentiIASIG( ALuint eid, ALenum param, ALint value );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alEnvironmentiIASIG(int environmentId, int attribute, int val);
        #endregion alEnvironmentiIASIG(int environmentId, int attribute, int val)

        #region alEnvironmentfIASIG(int environmentId, int attribute, int val)
        /// <summary>
        ///     Unknown.
        /// </summary>
        /// <param name="environmentId">
        ///     Unknown.
        /// </param>
        /// <param name="attribute">
        ///     Unknown.
        /// </param>
        /// <param name="val">
        ///     Unknown.
        /// </param>
        // ALAPI void ALAPIENTRY alEnvironmentfIASIG( ALuint eid, ALenum param, ALuint value );
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alEnvironmentfIASIG(int environmentId, int attribute, int val);
        #endregion alEnvironmentfIASIG(int environmentId, int attribute, int val)
        #endregion Public IASIG Methods
    }
}
