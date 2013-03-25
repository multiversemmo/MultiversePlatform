#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Diagnostics;
using System.Reflection;
using System.Management;
using Axiom.Core;

namespace Axiom.Graphics {
    /// <summary>
    /// 	This serves as a way to query information about the capabilies of a 3D API and the
    /// 	users hardware configuration.  A RenderSystem should create and initialize an instance
    /// 	of this class during startup so that it will be available for use ASAP for checking caps.
    /// </summary>
    public class HardwareCaps {
        #region Member variables
		
        /// <summary>
        ///    Flag enum holding the bits that identify each supported feature.
        /// </summary>
        private Capabilities caps;
        /// <summary>
        ///    Max number of texture units available on the current hardware.
        /// </summary>
        private int numTextureUnits;
        /// <summary>
        ///    Max number of world matrices supported.
        /// </summary>
        private int numWorldMatrices;
        /// <summary>
        ///    The best vertex program version supported by the hardware.
        /// </summary>
        private string maxVertexProgramVersion;
        /// <summary>
        ///    The best fragment program version supported by the hardware.
        /// </summary>
        private string maxFragmentProgramVersion;
        /// <summary>
        ///    The number of floating point constants the current hardware supports for vertex programs.
        /// </summary>
        private int vertexProgramConstantFloatCount;
        /// <summary>
        ///    The number of integer constants the current hardware supports for vertex programs.
        /// </summary>
        private int vertexProgramConstantIntCount;
        /// <summary>
        ///    The number of boolean constants the current hardware supports for vertex programs.
        /// </summary>
        //private int vertexProgramConstantBoolCount; (unused)
        /// <summary>
        ///    The number of floating point constants the current hardware supports for fragment programs.
        /// </summary>
        private int fragmentProgramConstantFloatCount;
        /// <summary>
        ///    The number of integer constants the current hardware supports for fragment programs.
        /// </summary>
        private int fragmentProgramConstantIntCount;
        /// <summary>
        ///    The number of boolean constants the current hardware supports for fragment programs.
        /// </summary>
        // private int fragmentProgramConstantBoolCount; (unused)
        /// <summary>
        ///    Stencil buffer bits available.
        /// </summary>
        private int stencilBufferBits;
        /// <summary>
        ///    The number of simultaneous render targets supported
        /// </summary>
        private int numMultiRenderTargets;
        /// <summary>
        ///    The maximum point size
        /// </summary>
        private float maxPointSize;
        /// <summary>
        ///    Are non-POW2 textures feature-limited?
        /// </summary>
        private bool nonPOW2TexturesLimited;
        /// <summary>
        ///    The number of vertex texture units supported
        /// </summary>
        private int numVertexTextureUnits;
        /// <summary>
        ///    Are vertex texture units shared with fragment processor?
        /// </summary>
        private bool vertexTextureUnitsShared;
        /// <summary>
        ///    Maximum number of lights that can be active in the scene at any given time.
        /// </summary>
        private int maxLights;
        /// <summary>
        /// name of the adapter
        /// </summary>
        private string deviceName = "";
        /// <summary>
        /// version number of the driver
        /// </summary>
        private string driverVersion = "";

        /// <summary>
        /// estimate of amount of available texture memory on startup
        /// </summary>
        private int videoMemorySize = 0;

        /// <summary>
        /// estimate of amount of available system memory in the machine
        /// </summary>
        private uint systemMemorySize = 0;

        #endregion
		
        #region Constructors
		
        /// <summary>
        ///    Default constructor.
        /// </summary>
        public HardwareCaps() {
            caps = 0;

            systemMemorySize = GetSystemMemorySize();
        }
		
        #endregion
		
        #region Properties

        /// <summary>
        /// Name of the display adapter
        /// </summary>
        public string DeviceName
        {
            get
            {
                return deviceName;
            }
            set
            {
                deviceName = value;
            }
        }
        /// <summary>
        /// The driver version string
        /// </summary>
        public string DriverVersion
        {
            get
            {
                return driverVersion;
            }
            set
            {
                driverVersion = value;
            }
        }

        /// <summary>
        ///    Max number of floating point constants supported by the hardware for fragment programs.
        /// </summary>
        public int FragmentProgramConstantFloatCount {
            get {
                return fragmentProgramConstantFloatCount;
            }
            set {
                fragmentProgramConstantFloatCount = value;
            }
        }

        /// <summary>
        ///    Max number of integer constants supported by the hardware for fragment programs.
        /// </summary>
        public int FragmentProgramConstantIntCount {
            get {
                return fragmentProgramConstantIntCount;
            }
            set {
                fragmentProgramConstantIntCount = value;
            }
        }

        /// <summary>
        ///    Best fragment program version supported by the hardware.
        /// </summary>
        public string MaxFragmentProgramVersion {
            get {
                return maxFragmentProgramVersion;
            }
            set {
                maxFragmentProgramVersion = value;
            }
        }

        /// <summary>
        ///		Maximum number of lights that can be active in the scene at any given time.
        /// </summary>
        public int MaxLights {
            get { 
                return maxLights; 
            }
            set { 
                maxLights = value; 
            }
        }

        /// <summary>
        ///    Best vertex program version supported by the hardware.
        /// </summary>
        public string MaxVertexProgramVersion {
            get {
                return maxVertexProgramVersion;
            }
            set {
                maxVertexProgramVersion = value;
            }
        }

        /// <summary>
        ///		Reports on the number of texture units the graphics hardware has available.
        /// </summary>
        public int TextureUnitCount {
            get { 
                return numTextureUnits; 
            }
            set { 
                numTextureUnits = value; 
            }
        }

        /// <summary>
        ///    Max number of world matrices supported by the hardware.
        /// </summary>
        public int NumWorldMatrices {
            get {
                return numWorldMatrices;
            }
            set {
                numWorldMatrices = value;
            }
        }

        /// <summary>
        ///		Number of stencil buffer bits suppported by the hardware.
        /// </summary>
        public int StencilBufferBits {
            get { 
                return stencilBufferBits; 
            }
            set { 
                stencilBufferBits = value; 
            }
        }

        /// <summary>
        ///    Max number of floating point constants supported by the hardware for vertex programs.
        /// </summary>
        public int VertexProgramConstantFloatCount {
            get {
                return vertexProgramConstantFloatCount;
            }
            set {
                vertexProgramConstantFloatCount = value;
            }
        }

        /// <summary>
        ///    Max number of integer constants supported by the hardware for vertex programs.
        /// </summary>
        public int VertexProgramConstantIntCount {
            get {
                return vertexProgramConstantIntCount;
            }
            set {
                vertexProgramConstantIntCount = value;
            }
        }

        /// <summary>
        ///    The number of simultaneous render targets supported
        /// </summary>
        public int NumMultiRenderTargets {
            get {
                return numMultiRenderTargets;
            }
            set {
                numMultiRenderTargets = value;
            }
        }

        /// <summary>
        ///    The maximum point size
        /// </summary>
        public float MaxPointSize {
            get {
                return maxPointSize;
            }
            set {
                maxPointSize = value;
            }
        }

        /// <summary>
        ///    Are non-POW2 textures feature-limited?
        /// </summary>
        public bool NonPOW2TexturesLimited {
            get {
                return nonPOW2TexturesLimited;
            }
            set {
                nonPOW2TexturesLimited = value;
            }
        }

        /// <summary>
        ///    The number of vertex texture units supported
        /// </summary>
        public int NumVertexTextureUnits {
            get {
                return numVertexTextureUnits;
            }
            set {
                numVertexTextureUnits = value;
            }
        }

        /// <summary>
        ///    Are vertex texture units shared with fragment processor?
        /// </summary>
        public bool VertexTextureUnitsShared {
            get {
                return vertexTextureUnitsShared;
            }
            set {
                vertexTextureUnitsShared = value;
            }
        }

        public int VideoMemorySize
        {
            get
            {
                return videoMemorySize;
            }
            set
            {
                videoMemorySize = value;
            }
        }

        public uint SystemMemorySize
        {
            get
            {
                return systemMemorySize;
            }
            set
            {
                systemMemorySize = value;
            }
        }

        #endregion

        #region Methods

        private uint GetSystemMemorySize()
        {
            UInt64 size = 0;
            uint mbSize = 0;

            try
            {
                System.Management.SelectQuery selectQuery
                  = new System.Management.SelectQuery("Win32_ComputerSystem");
                System.Management.ManagementObjectSearcher searcher
                  = new System.Management.ManagementObjectSearcher(selectQuery);
                foreach (System.Management.ManagementObject comp in searcher.Get())
                {
                    size += ((UInt64)(comp["TotalPhysicalMemory"]));
                }

                mbSize = (uint)(size / (1024 * 1024));
            }
            catch (Exception)
            {
                mbSize = 0;
            }

            LogManager.Instance.Write("Total System Memory Size: {0}MB", mbSize);
            return mbSize;
        }

        /// <summary>
        ///    Returns true if the current hardware supports the requested feature.
        /// </summary>
        /// <param name="cap">Feature to query (i.e. Dot3 bump mapping)</param>
        /// <returns></returns>
        public bool CheckCap(Capabilities cap) {
            return (caps & cap) > 0;
        }

        /// <summary>
        ///    Sets a flag stating the specified feature is supported.
        /// </summary>
        /// <param name="cap"></param>
        public void SetCap(Capabilities cap) {
            caps |= cap;
        }

        /// <summary>
        ///    Write all hardware capability information to registered listeners.
        /// </summary>
        public void Log() {
            log4net.ILog log =  log4net.LogManager.GetLogger("HardwareCaps");

            log.InfoFormat("---RenderSystem capabilities---");
            log.InfoFormat("\t-Adapter Name: {0}", deviceName);
            log.InfoFormat("\t-Driver Version: {0}", driverVersion);
            log.InfoFormat("\t-Total System Memory: {0}Meg", systemMemorySize);
            log.InfoFormat("\t-Video Memory: {0}Meg", videoMemorySize);
            log.InfoFormat("\t-Available texture units: {0}", this.TextureUnitCount);
            log.InfoFormat("\t-Maximum lights available: {0}", this.MaxLights);
            log.InfoFormat("\t-Hardware generation of mip-maps: {0}", ConvertBool(CheckCap(Capabilities.HardwareMipMaps)));
            log.InfoFormat("\t-Texture blending: {0}", ConvertBool(CheckCap(Capabilities.TextureBlending)));
            log.InfoFormat("\t-Anisotropic texture filtering: {0}", ConvertBool(CheckCap(Capabilities.AnisotropicFiltering)));
            log.InfoFormat("\t-Dot product texture operation: {0}", ConvertBool(CheckCap(Capabilities.Dot3)));
            log.InfoFormat("\t-Cube Mapping: {0}", ConvertBool(CheckCap(Capabilities.CubeMapping)));

            log.InfoFormat("\t-Hardware stencil buffer: {0}", ConvertBool(CheckCap(Capabilities.StencilBuffer)));

            if (CheckCap(Capabilities.StencilBuffer)) {
                log.InfoFormat("\t\t-Stencil depth: {0} bits", stencilBufferBits);
                log.InfoFormat("\t\t-Two sided stencil support: {0}", ConvertBool(CheckCap(Capabilities.TwoSidedStencil)));
                log.InfoFormat("\t\t-Wrap stencil values: {0}", ConvertBool(CheckCap(Capabilities.StencilWrap)));
            }

            log.InfoFormat("\t-Hardware vertex/index buffers: {0}", ConvertBool(CheckCap(Capabilities.VertexBuffer)));

            log.InfoFormat("\t-Vertex programs: {0}", ConvertBool(CheckCap(Capabilities.VertexPrograms)));

            if(CheckCap(Capabilities.VertexPrograms)) {
                log.InfoFormat("\t\t-Max vertex program version: {0}", this.MaxVertexProgramVersion);
            }

            log.InfoFormat("\t-Fragment programs: {0}", ConvertBool(CheckCap(Capabilities.FragmentPrograms)));

            if (CheckCap(Capabilities.FragmentPrograms)) {
                log.InfoFormat("\t\t-Max fragment program version: {0}", this.MaxFragmentProgramVersion);
            }

            log.InfoFormat("\t-Texture compression: {0}", ConvertBool(CheckCap(Capabilities.TextureCompression)));

            if (CheckCap(Capabilities.TextureCompression)) {
                log.InfoFormat("\t\t-DXT: {0}", ConvertBool(CheckCap(Capabilities.TextureCompressionDXT)));
                log.InfoFormat("\t\t-VTC: {0}", ConvertBool(CheckCap(Capabilities.TextureCompressionVTC)));
            }

            log.InfoFormat("\t-Scissor rectangle: {0}", ConvertBool(CheckCap(Capabilities.ScissorTest)));
            log.InfoFormat("\t-Hardware Occlusion Query: {0}", ConvertBool(CheckCap(Capabilities.HardwareOcculusion)));
            log.InfoFormat("\t-User clip planes: {0}", ConvertBool(CheckCap(Capabilities.UserClipPlanes)));
            log.InfoFormat("\t-VertexElementType.UBYTE4: {0}", ConvertBool(CheckCap(Capabilities.VertexFormatUByte4)));
            log.InfoFormat("\t-Infinite far plane projection: {0}", ConvertBool(CheckCap(Capabilities.InfiniteFarPlane)));
            log.InfoFormat("\t-Hardware render-to-texture: {0}", ConvertBool(CheckCap(Capabilities.HardwareRenderToTexture)));
            log.InfoFormat("\t-Floating point textures: {0}{1}",
                ConvertBool(CheckCap(Capabilities.TextureFloat)),
                (nonPOW2TexturesLimited ? " (limited)" : ""));
            log.InfoFormat("\t-Volume textures: {0}", ConvertBool(CheckCap(Capabilities.Texture3D)));
            log.InfoFormat("\t-Multiple Render Targets: {0}", numMultiRenderTargets);
            log.InfoFormat("\t-Point Sprites: {0}", ConvertBool(CheckCap(Capabilities.PointSprites)));
            log.InfoFormat("\t-Extended point parameters: {0}", ConvertBool(CheckCap(Capabilities.PointExtendedParameters)));
            log.InfoFormat("\t-Max Point Size: {0}", maxPointSize);
            log.InfoFormat("\t-Vertex texture fetch: {0}", ConvertBool(CheckCap(Capabilities.VertexTextureFetch)));
            if (CheckCap(Capabilities.VertexTextureFetch)) {
                log.InfoFormat("\t\t-Max vertex textures: {0}", numVertexTextureUnits);
                log.InfoFormat("\t\t-Vertex textures shared: {0}", ConvertBool(vertexTextureUnitsShared));
            }
            log.InfoFormat("\t-Max vertex program float constants: {0}", vertexProgramConstantFloatCount);
            log.InfoFormat("\t-Max vertex program int constants: {0}", vertexProgramConstantIntCount);
            log.InfoFormat("\t-Max fragment program float constants: {0}", fragmentProgramConstantFloatCount);
            log.InfoFormat("\t-Max fragment int constants: {0}", fragmentProgramConstantIntCount);
        }

        /// <summary>
        ///     Helper method to convert true/false to yes/no.
        /// </summary>
        /// <param name="val">Bool bal.</param>
        /// <returns>"yes" if true, else "no".</returns>
        private string ConvertBool(bool val) {
            return val ? "yes" : "no";
        }

        #endregion
    }
}
