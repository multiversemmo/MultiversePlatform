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
using System.IO;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.Graphics
{
	/// <summary>
    /// 	Abstract base class representing a high-level program (a vertex or
    /// 	fragment program).
	/// </summary>
	/// <remarks>
	///    High-level programs are vertex and fragment programs written in a high-level
	///    language such as Cg or HLSL, and as such do not require you to write assembler code
	///    like GpuProgram does. However, the high-level program does eventually 
	///    get converted (compiled) into assembler and then eventually microcode which is
	///    what runs on the GPU. As well as the convenience, some high-level languages like Cg allow
	///    you to write a program which will operate under both Direct3D and OpenGL, something
	///    which you cannot do with just GpuProgram (which requires you to write 2 programs and
	///    use each in a Technique to provide cross-API compatibility). The engine will be creating
	///    a GpuProgram for you based on the high-level program, which is compiled specifically 
	///    for the API being used at the time, but this process is transparent.
	///    <p/>
	///    You cannot create high-level programs direct - use HighLevelGpuProgramManager instead.
	///    Plugins can register new implementations of HighLevelGpuProgramFactory in order to add
	///    support for new languages without requiring changes to the core engine API. To allow 
	///    custom parameters to be set, this class implement IConfigurable - the application
	///    can query on the available custom parameters and get/set them without having to 
	///    link specifically with it.
	/// </remarks>
	public abstract class HighLevelGpuProgram : GpuProgram, IConfigurable {
		#region Fields

        /// <summary>
        ///    Whether the high-level program (and it's parameter defs) is loaded.
        /// </summary>
        protected bool isHighLevelLoaded;
        /// <summary>
        ///    The underlying assembler program.
        /// </summary>
        protected GpuProgram assemblerProgram;
		
		#endregion Fields
		
		#region Constructors
		
        /// <summary>
        ///    Default constructor.
        /// </summary>
        /// <param name="name">Name of the high level program.</param>
        /// <param name="type">Type of program, vertex or fragment.</param>
        /// <param name="language">HLSL language this program is written in.</param>
		public HighLevelGpuProgram(string name, GpuProgramType type, string language) : base(name, type, language) {
		}
		
		#endregion
		
		#region Methods

        public override void Preload() {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        ///    Implementation of Resource.Load.
        /// </summary>
        protected override void LoadImpl() {
            if (IsSupported) {
                // polymorphic load 
                LoadHighLevelImpl();

                // polymorphic creation of the low level program
                CreateLowLevelImpl();

                Debug.Assert(assemblerProgram != null, "Subclasses of HighLevelGpuProgram MUST initialize the low level assembler program.");

                // load the low level assembler program
                if (assemblerProgram != null) {
                    assemblerProgram.Load();
                    isLoaded = true;
                }
            }
        }

        /// <summary>
        ///    Internal load implementation, loads just the high-level portion, enough to 
        ///    get parameters.
        /// </summary>
        protected virtual void LoadHighLevel() {
            if (!isHighLevelLoaded) {
                try {
                    LoadHighLevelImpl();
                    isHighLevelLoaded = true;
                } catch (Exception e) {
                    LogManager.Instance.WriteException("High-level program '{0}' encountered an error " +
                                              "during loading and is thus not supported.\n{1}\n{2}", name, e.Message, e.StackTrace );
                    // this.compileError = true;
                }
            }
        }

        /// <summary>
        ///    Internal load implementation, loads just the high-level portion, enough to 
        ///    get parameters.
        /// </summary>
        protected virtual void LoadHighLevelImpl() {
            if (loadFromFile) {
                Stream stream = GpuProgramManager.Instance.FindResourceData(fileName);
                StreamReader reader = new StreamReader(stream, System.Text.Encoding.ASCII);
                source = reader.ReadToEnd();
                stream.Dispose();
            }
            LoadFromSource();
        }

        /// <summary>
        ///    Internal method for creating an appropriate low-level program from this
        ///    high-level program, must be implemented by subclasses.
        /// </summary>
        protected abstract void CreateLowLevelImpl();

        protected abstract void UnloadHighLevelImpl();

        /// <summary>
        ///    Implementation of Resource.Unload.
        /// </summary>
        protected override void UnloadImpl() {
            if (assemblerProgram != null) {
                assemblerProgram.Unload();
                assemblerProgram = null;
            }

            // polymorphic unload
            UnloadHighLevel();
        }

        public void UnloadHighLevel() {
            if (isHighLevelLoaded) {
                UnloadHighLevelImpl();
                isHighLevelLoaded = false;
            }
        }

        /// <summary>
        ///    Populate the passed parameters with name->index map, must be overridden.
        /// </summary>
        /// <param name="parms"></param>
        protected abstract void PopulateParameterNames(GpuProgramParameters parms);

        /// <summary>
        ///    Creates a new parameters object compatible with this program definition.
        /// </summary>
        /// <remarks>
        ///    Unlike low-level assembly programs, parameters objects are specific to the
        ///    program and therefore must be created from it rather than by the 
        ///    HighLevelGpuProgramManager. This method creates a new instance of a parameters
        ///    object containing the definition of the parameters this program understands.
        /// </remarks>
        /// <returns>A new set of program parameters.</returns>
        public override GpuProgramParameters CreateParameters() {
            // create and load named parameters
            GpuProgramParameters newParams = GpuProgramManager.Instance.CreateParameters();

            // Only populate named parameters if we can support this program
            if (IsSupported) {
                LoadHighLevel();

                // Errors during load may have prevented compile
                if (IsSupported)
                    PopulateParameterNames(newParams);
            }

			// copy in default parameters if present
			if(defaultParams != null) {
				newParams.CopyConstantsFrom(defaultParams);
			}

            return newParams;
        }

		#endregion
		
		#region Properties

        /// <summary>
        ///    Gets the lowlevel assembler program based on this HighLevel program.
        /// </summary>
        public override GpuProgram BindingDelegate {
            get {
                return assemblerProgram;
            }
        }
		
		#endregion

        #region IConfigurable Members

        /// <summary>
        ///    Must be implemented by subclasses.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public abstract bool SetParam(string name, string val);

        #endregion
    }
}
