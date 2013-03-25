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
    ///     OpenAL binding for .NET, implementing ALC 1.0.
    /// </summary>
    /// <remarks>
    ///     Binds functions and definitions in MVOpenAL32.dll or libAL.so.
    /// </remarks>
    #endregion Class Documentation
    public sealed class Alc {
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
        #region ALC_INVALID
        /// <summary>
        ///     Bad value.
        /// </summary>
        // #define ALC_INVALID (-1)
        public const int ALC_INVALID = (-1);
        #endregion ALC_INVALID

        #region ALC_FALSE
        /// <summary>
        ///     bool false.
        /// </summary>
        // #define ALC_FALSE 0
        public const int ALC_FALSE = 0;
        #endregion ALC_FALSE

        #region ALC_TRUE
        /// <summary>
        ///     bool true.
        /// </summary>
        // #define ALC_TRUE 1
        public const int ALC_TRUE = 1;
        #endregion ALC_TRUE

        #region ALC_NO_ERROR
        /// <summary>
        ///     No error.
        /// </summary>
        // #define ALC_NO_ERROR ALC_FALSE
        public const int ALC_NO_ERROR = ALC_FALSE;
        #endregion ALC_NO_ERROR

        #region ALC_MAJOR_VERSION
        /// <summary>
        ///     Major version.
        /// </summary>
        // #define ALC_MAJOR_VERSION 0x1000
        public const int ALC_MAJOR_VERSION = 0x1000;
        #endregion ALC_MAJOR_VERSION

        #region ALC_MINOR_VERSION
        /// <summary>
        ///     Minor version.
        /// </summary>
        // #define ALC_MINOR_VERSION 0x1001
        public const int ALC_MINOR_VERSION = 0x1001;
        #endregion ALC_MINOR_VERSION

        #region ALC_ATTRIBUTES_SIZE
        /// <summary>
        ///     Attributes size.
        /// </summary>
        // #define ALC_ATTRIBUTES_SIZE 0x1002
        public const int ALC_ATTRIBUTES_SIZE = 0x1002;
        #endregion ALC_ATTRIBUTES_SIZE

        #region ALC_ALL_ATTRIBUTES
        /// <summary>
        ///     All attributes.
        /// </summary>
        // #define ALC_ALL_ATTRIBUTES 0x1003
        public const int ALC_ALL_ATTRIBUTES = 0x1003;
        #endregion ALC_ALL_ATTRIBUTES

        #region ALC_DEFAULT_DEVICE_SPECIFIER
        /// <summary>
        ///     Default device specifier.
        /// </summary>
        // #define ALC_DEFAULT_DEVICE_SPECIFIER 0x1004
        public const int ALC_DEFAULT_DEVICE_SPECIFIER = 0x1004;
        #endregion ALC_DEFAULT_DEVICE_SPECIFIER

        #region ALC_DEVICE_SPECIFIER
        /// <summary>
        ///     Device specifier.
        /// </summary>
        // #define ALC_DEVICE_SPECIFIER 0x1005
        public const int ALC_DEVICE_SPECIFIER = 0x1005;
        #endregion ALC_DEVICE_SPECIFIER

        #region ALC_EXTENSIONS
        /// <summary>
        ///     Extensions.
        /// </summary>
        // #define ALC_EXTENSIONS 0x1006
        public const int ALC_EXTENSIONS = 0x1006;
        #endregion ALC_EXTENSIONS

        #region ALC_FREQUENCY
        /// <summary>
        ///     Frequency.
        /// </summary>
        // #define ALC_FREQUENCY 0x1007
        public const int ALC_FREQUENCY = 0x1007;
        #endregion ALC_FREQUENCY

        #region ALC_REFRESH
        /// <summary>
        ///     Refresh.
        /// </summary>
        // #define ALC_REFRESH 0x1008
        public const int ALC_REFRESH = 0x1008;
        #endregion ALC_REFRESH

        #region ALC_SYNC
        /// <summary>
        ///     Sync.
        /// </summary>
        // #define ALC_SYNC 0x1009
        public const int ALC_SYNC = 0x1009;
        #endregion ALC_SYNC

        #region ALC_INVALID_DEVICE
        /// <summary>
        ///     The device argument does not name a valid device.
        /// </summary>
        // #define ALC_INVALID_DEVICE 0xA001
        public const int ALC_INVALID_DEVICE = 0xA001;
        #endregion ALC_INVALID_DEVICE

        #region ALC_INVALID_CONTEXT
        /// <summary>
        ///     The context argument does not name a valid context.
        /// </summary>
        // #define ALC_INVALID_CONTEXT 0xA002
        public const int ALC_INVALID_CONTEXT = 0xA002;
        #endregion ALC_INVALID_CONTEXT

        #region ALC_INVALID_ENUM
        /// <summary>
        ///     A function was called at inappropriate time, or in an inappropriate way, causing
        ///     an illegal state.  This can be an incompatible value, object ID, and/or function.
        /// </summary>
        // #define ALC_INVALID_ENUM 0xA003
        public const int ALC_INVALID_ENUM = 0xA003;
        #endregion ALC_INVALID_ENUM

        #region ALC_INVALID_VALUE
        /// <summary>
        ///     Illegal value passed as an argument to an AL call.  Applies to parameter values,
        ///     but not to enumerations.
        /// </summary>
        // #define ALC_INVALID_VALUE 0xA004
        public const int ALC_INVALID_VALUE = 0xA004;
        #endregion ALC_INVALID_VALUE

        #region ALC_OUT_OF_MEMORY
        /// <summary>
        ///     A function could not be completed, because there is not enough memory available.
        /// </summary>
        // #define ALC_OUT_OF_MEMORY 0xA005
        public const int ALC_OUT_OF_MEMORY = 0xA005;
        #endregion ALC_OUT_OF_MEMORY
        #endregion Public OpenAL 1.0 Constants

        // --- Constructors & Destructors ---
        #region Alc()
        /// <summary>
        ///     Prevents instantiation.
        /// </summary>
        private Alc() {
        }
        #endregion Alc()

        // --- Public Externs ---
        #region Public OpenAL 1.0 Methods
        #region alcCloseDevice([In] IntPtr device)
        /// <summary>
        ///     Closes a device.
        /// </summary>
        /// <param name="device">
        ///     A pointer to an opened device.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcCloseDevice(ALCdevice *device);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcCloseDevice([In] IntPtr device);
        #endregion alcCloseDevice([In] IntPtr device)

        #region IntPtr alcCreateContext([In] IntPtr device, [In] ref int attribute)
        /// <summary>
        ///     Creates a context using a specified device.
        /// </summary>
        /// <param name="device">
        ///     A pointer to a device.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         A pointer to a set of attributes:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_FREQUENCY" /></item>
        ///             <item><see cref="ALC_REFRESH" /></item>
        ///             <item><see cref="ALC_SYNC" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a pointer to the new context (IntPtr.Zero on failure).
        /// </returns>
        // ALCAPI ALCcontext* ALCAPIENTRY alcCreateContext(ALCdevice *device, ALCint *attrList);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcCreateContext([In] IntPtr device, [In] ref int attribute);
        #endregion IntPtr alcCreateContext([In] IntPtr device, [In] ref int attribute)

        #region IntPtr alcCreateContext([In] IntPtr device, [In] int[] attribute)
        /// <summary>
        ///     Creates a context using a specified device.
        /// </summary>
        /// <param name="device">
        ///     A pointer to a device.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         A pointer to a set of attributes:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_FREQUENCY" /></item>
        ///             <item><see cref="ALC_REFRESH" /></item>
        ///             <item><see cref="ALC_SYNC" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a pointer to the new context (IntPtr.Zero on failure).
        /// </returns>
        // ALCAPI ALCcontext* ALCAPIENTRY alcCreateContext(ALCdevice *device, ALCint *attrList);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcCreateContext([In] IntPtr device, [In] int[] attribute);
        #endregion IntPtr alcCreateContext([In] IntPtr device, [In] int[] attribute)

        #region IntPtr alcCreateContext([In] IntPtr device, [In] IntPtr attribute)
        /// <summary>
        ///     Creates a context using a specified device.
        /// </summary>
        /// <param name="device">
        ///     A pointer to a device.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         A pointer to a set of attributes:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_FREQUENCY" /></item>
        ///             <item><see cref="ALC_REFRESH" /></item>
        ///             <item><see cref="ALC_SYNC" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a pointer to the new context (IntPtr.Zero on failure).
        /// </returns>
        // ALCAPI ALCcontext* ALCAPIENTRY alcCreateContext(ALCdevice *device, ALCint *attrList);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcCreateContext([In] IntPtr device, [In] IntPtr attribute);
        #endregion IntPtr alcCreateContext([In] IntPtr device, [In] IntPtr attribute)

        #region IntPtr alcCreateContext([In] IntPtr device, [In] int *attribute)
        /// <summary>
        ///     Creates a context using a specified device.
        /// </summary>
        /// <param name="device">
        ///     A pointer to a device.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         A pointer to a set of attributes:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_FREQUENCY" /></item>
        ///             <item><see cref="ALC_REFRESH" /></item>
        ///             <item><see cref="ALC_SYNC" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a pointer to the new context (IntPtr.Zero on failure).
        /// </returns>
        // ALCAPI ALCcontext* ALCAPIENTRY alcCreateContext(ALCdevice *device, ALCint *attrList);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern IntPtr alcCreateContext([In] IntPtr device, [In] int *attribute);
        #endregion IntPtr alcCreateContext([In] IntPtr device, [In] int *attribute)

        #region alcDestroyContext([In] IntPtr context)
        /// <summary>
        ///     Destroys a context.
        /// </summary>
        /// <param name="context">
        ///     Pointer to the context to be destroyed.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcDestroyContext(ALCcontext *context);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcDestroyContext([In] IntPtr context);
        #endregion alcDestroyContext([In] IntPtr context)

        #region IntPtr alcGetContextsDevice([In] IntPtr context)
        /// <summary>
        ///     Gets the device for a context.
        /// </summary>
        /// <param name="context">
        ///     The context to query.
        /// </param>
        /// <returns>
        ///     A pointer to a device or IntPtr.Zero on failue.
        /// </returns>
        // ALCAPI ALCdevice* ALCAPIENTRY alcGetContextsDevice(ALCcontext *context);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcGetContextsDevice([In] IntPtr context);
        #endregion IntPtr alcGetContextsDevice([In] IntPtr context)

        #region IntPtr alcGetCurrentContext()
        /// <summary>
        ///     Retrieves the current context.
        /// </summary>
        /// <returns>
        ///     Returns a pointer to the current context or IntPtr.Zero on failure.
        /// </returns>
        // ALCAPI ALCcontext* ALCAPIENTRY alcGetCurrentContext(ALCvoid);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcGetCurrentContext();
        #endregion IntPtr alcGetCurrentContext()

        #region int alcGetEnumValue([In] IntPtr device, string enumName)
        /// <summary>
        ///     Retrieves the enum value for a specified enumeration name.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried.
        /// </param>
        /// <param name="enumName">
        ///     A null terminated string describing the enum value.
        /// </param>
        /// <returns>
        ///     Returns the enum value described by the <i>enumName</i> string.
        /// </returns>
        // ALCAPI ALCenum ALCAPIENTRY alcGetEnumValue(ALCdevice *device, ALCubyte *enumName);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, CharSet=CharSet.Ansi, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alcGetEnumValue([In] IntPtr device, string enumName);
        #endregion int alcGetEnumValue([In] IntPtr device, string enumName)

        #region int alcGetError([In] IntPtr device)
        /// <summary>
        ///     Retrieves the current context error state.
        /// </summary>
        /// <param name="device">
        ///     The device to query.
        /// </param>
        /// <returns>
        ///     The current context error state will be returned.
        /// </returns>
        // ALCAPI ALCenum ALCAPIENTRY alcGetError(ALCdevice *device);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alcGetError([In] IntPtr device);
        #endregion int alcGetError([In] IntPtr device)

        #region alcGetIntegerv([In] IntPtr device, int attribute, int size, out int data)
        /// <summary>
        ///     Returns integers related to the context.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         An attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_MAJOR_VERSION" /></item>
        ///             <item><see cref="ALC_MINOR_VERSION" /></item>
        ///             <item><see cref="ALC_ATTRIBUTES_SIZE" /></item>
        ///             <item><see cref="ALC_ALL_ATTRIBUTES" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="size">
        ///     The size of the destination buffer provided.
        /// </param>
        /// <param name="data">
        ///     A pointer to the data to be returned.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcGetIntegerv(ALCdevice *device, ALCenum param, ALCsizei size, ALCint *data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcGetIntegerv([In] IntPtr device, int attribute, int size, out int data);
        #endregion alcGetIntegerv([In] IntPtr device, int attribute, int size, out int data)

        #region alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] int[] data)
        /// <summary>
        ///     Returns integers related to the context.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         An attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_MAJOR_VERSION" /></item>
        ///             <item><see cref="ALC_MINOR_VERSION" /></item>
        ///             <item><see cref="ALC_ATTRIBUTES_SIZE" /></item>
        ///             <item><see cref="ALC_ALL_ATTRIBUTES" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="size">
        ///     The size of the destination buffer provided.
        /// </param>
        /// <param name="data">
        ///     A pointer to the data to be returned.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcGetIntegerv(ALCdevice *device, ALCenum param, ALCsizei size, ALCint *data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] int[] data);
        #endregion alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] int[] data)

        #region alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] IntPtr data)
        /// <summary>
        ///     Returns integers related to the context.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         An attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_MAJOR_VERSION" /></item>
        ///             <item><see cref="ALC_MINOR_VERSION" /></item>
        ///             <item><see cref="ALC_ATTRIBUTES_SIZE" /></item>
        ///             <item><see cref="ALC_ALL_ATTRIBUTES" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="size">
        ///     The size of the destination buffer provided.
        /// </param>
        /// <param name="data">
        ///     A pointer to the data to be returned.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcGetIntegerv(ALCdevice *device, ALCenum param, ALCsizei size, ALCint *data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] IntPtr data);
        #endregion alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] IntPtr data)

        #region alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] int *data)
        /// <summary>
        ///     Returns integers related to the context.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         An attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_MAJOR_VERSION" /></item>
        ///             <item><see cref="ALC_MINOR_VERSION" /></item>
        ///             <item><see cref="ALC_ATTRIBUTES_SIZE" /></item>
        ///             <item><see cref="ALC_ALL_ATTRIBUTES" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <param name="size">
        ///     The size of the destination buffer provided.
        /// </param>
        /// <param name="data">
        ///     A pointer to the data to be returned.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcGetIntegerv(ALCdevice *device, ALCenum param, ALCsizei size, ALCint *data);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), CLSCompliant(false), SuppressUnmanagedCodeSecurity]
        public unsafe static extern void alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] int *data);
        #endregion alcGetIntegerv([In] IntPtr device, int attribute, int size, [Out] int *data)

        #region IntPtr alcGetProcAddress([In] IntPtr device, string functionName)
        /// <summary>
        ///     Retrieves the address of a specified context extension function.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried for the function.
        /// </param>
        /// <param name="functionName">
        ///     A null terminated string describing the function.
        /// </param>
        /// <returns>
        ///     Returns the address of the function, or IntPtr.Zero if it is not found.
        /// </returns>
        // ALCAPI ALCvoid* ALCAPIENTRY alcGetProcAddress(ALCdevice *device, ALCubyte *funcName);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, CharSet=CharSet.Ansi, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcGetProcAddress([In] IntPtr device, string functionName);
        #endregion IntPtr alcGetProcAddress([In] IntPtr device, string functionName)

        #region string alcGetString([In] IntPtr device, int attribute)
        /// <summary>
        ///     Returns strings related to the context.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried.
        /// </param>
        /// <param name="attribute">
        ///     <para>
        ///         An attribute to be retrieved:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item><see cref="ALC_DEFAULT_DEVICE_SPECIFIER" /></item>
        ///             <item><see cref="ALC_DEVICE_SPECIFIER" /></item>
        ///             <item><see cref="ALC_EXTENSIONS" /></item>
        ///         </list>
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns a pointer to a string.
        /// </returns>
        // ALCAPI ALCubyte* ALCAPIENTRY alcGetString(ALCdevice *device, ALCenum param);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, CharSet=CharSet.Ansi, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern string alcGetString([In] IntPtr device, int attribute);
        #endregion string alcGetString([In] IntPtr device, int attribute)

        #region int alcIsExtensionPresent([In] IntPtr device, string extensionName)
        /// <summary>
        ///     Queries if a specified context extension is available.
        /// </summary>
        /// <param name="device">
        ///     The device to be queried for an extension.
        /// </param>
        /// <param name="extensionName">
        ///     A null terminated string describing the extension.
        /// </param>
        /// <returns>
        ///     Returns <see cref="ALC_TRUE" /> if the extension is available,
        ///     <see cref="ALC_FALSE" /> if the extension is not available.
        /// </returns>
        // ALCAPI ALCboolean ALCAPIENTRY alcIsExtensionPresent(ALCdevice *device, ALCubyte *extName);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, CharSet=CharSet.Ansi, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alcIsExtensionPresent([In] IntPtr device, string extensionName);
        #endregion int alcIsExtensionPresent([In] IntPtr device, string extensionName)

        #region int alcMakeContextCurrent([In] IntPtr context)
        /// <summary>
        ///     Makes a specified context the current context.
        /// </summary>
        /// <param name="context">
        ///     Pointer to the new context.
        /// </param>
        /// <returns>
        ///     Returns an error code on failure.
        /// </returns>
        // ALCAPI ALCboolean ALCAPIENTRY alcMakeContextCurrent(ALCcontext *context);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern int alcMakeContextCurrent([In] IntPtr context);
        #endregion int alcMakeContextCurrent([In] IntPtr context)

        #region IntPtr alcOpenDevice(string deviceName)
        /// <summary>
        ///     Opens a device by name.
        /// </summary>
        /// <param name="deviceName">
        ///     A null-terminated string describing a device.
        /// </param>
        /// <returns>
        ///     Returns a pointer to the opened device.
        /// </returns>
        // ALCAPI ALCdevice* ALCAPIENTRY alcOpenDevice(ALCubyte *deviceName);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, CharSet=CharSet.Ansi, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern IntPtr alcOpenDevice(string deviceName);
        #endregion IntPtr alcOpenDevice(string deviceName)

        #region alcProcessContext([In] IntPtr context)
        /// <summary>
        ///     Tells a context to begin processing.
        /// </summary>
        /// <param name="context">
        ///     Pointer to the new context.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcProcessContext(ALCcontext *context);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcProcessContext([In] IntPtr context);
        #endregion alcProcessContext([In] IntPtr context)

        #region alcSuspendContext([In] IntPtr context)
        /// <summary>
        ///     Suspends processing on a specified context.
        /// </summary>
        /// <param name="context">
        ///     A pointer to the context to be suspended.
        /// </param>
        // ALCAPI ALCvoid ALCAPIENTRY alcSuspendContext(ALCcontext *context);
        [DllImport("MVOpenAL32.dll", CallingConvention=CALLING_CONVENTION, ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        public static extern void alcSuspendContext([In] IntPtr context);
        #endregion alcSuspendContext([In] IntPtr context)
        #endregion Public OpenAL 1.0 Methods
    }
}
