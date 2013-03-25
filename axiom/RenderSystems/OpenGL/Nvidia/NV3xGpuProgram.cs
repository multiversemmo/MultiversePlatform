using System;
using System.Runtime.InteropServices;
using Axiom.Graphics;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.Nvidia {
    /// <summary>
    ///     Base class for handling nVidia specific extensions for supporting
    ///     GeForceFX level gpu programs
    /// </summary>
    /// <remarks>
    ///     Subclasses must implement BindParameters since there are differences
    ///     in how parameters are passed to NV vertex and fragment programs.
    /// </remarks>
    public abstract class NV3xGpuProgram : GLGpuProgram {
        #region Constructor

        public NV3xGpuProgram(string name, GpuProgramType type, string syntaxCode) 
            : base(name, type, syntaxCode){

            // generate the program and store the unique name
            Gl.glGenProgramsNV(1, out programId);

            // find the GL enum for the type of program this is
            programType = (type == GpuProgramType.Vertex) ? 
                Gl.GL_VERTEX_PROGRAM_NV : Gl.GL_FRAGMENT_PROGRAM_NV;
        }

        #endregion Constructor

        #region GpuProgram Members

        /// <summary>
        ///     Loads NV3x level assembler programs into the hardware.
        /// </summary>
        protected override void LoadFromSource() {
            // bind this program before loading
            Gl.glBindProgramNV(programType, programId);

            // load the ASM source into an NV program
            Gl.glLoadProgramNV(programType, programId, source.Length, source);

            // get the error string from the NV program loader
            string error = Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_PROGRAM_ERROR_STRING_NV));

            // if there was an error, report it
            if(error != null && error.Length > 0) {
                int pos;

                // get the position of the error
                Gl.glGetIntegerv(Gl.GL_PROGRAM_ERROR_POSITION_ARB, out pos);
                
                throw new Exception(string.Format("Error on line {0} in program '{1}'\nError: {2}", pos, name, error));
            }
        }

        /// <summary>
        ///     Overridden to delete the NV program.
        /// </summary>
        public override void Unload() {
            base.Unload();

            // delete this NV program
            Gl.glDeleteProgramsNV(1, ref programId);
        }


        #endregion GpuProgram Members

        #region GLGpuProgram Members

        /// <summary>
        ///     Binds an NV program.
        /// </summary>
        public override void Bind() {
            // enable this program type
            Gl.glEnable(programType);

            // bind the program to the context
            Gl.glBindProgramNV(programType, programId);
        }

        /// <summary>
        ///     Unbinds an NV program.
        /// </summary>
        public override void Unbind() {
            // disable this program type
            Gl.glDisable(programType);
        }

        #endregion GLGpuProgram Members
    }

    /// <summary>
    ///     GeForceFX class vertex program.
    /// </summary>
    public class VP30GpuProgram : NV3xGpuProgram {
        #region Constructor

        public VP30GpuProgram(string name, GpuProgramType type, string syntaxCode) 
            : base(name, type, syntaxCode) {}

        #endregion Constructor

        #region GpuProgram Members

        /// <summary>
        ///     Binds params by index to the vp30 program.
        /// </summary>
        /// <param name="parms"></param>
        public override void BindParameters(GpuProgramParameters parms) {
            if(parms.HasFloatConstants) {
				for(int index = 0; index < parms.FloatConstantCount; index++) {
					GpuProgramParameters.FloatConstantEntry entry = parms.GetFloatConstant(index);

					if(entry.isSet) {
						// send the params 4 at a time
						Gl.glProgramParameter4fvNV(programType, index, entry.val);
					}
                }
            }  
        }

        /// <summary>
        ///     Overriden to return parms set to transpose matrices.
        /// </summary>
        /// <returns></returns>
        public override GpuProgramParameters CreateParameters() {
            GpuProgramParameters parms = base.CreateParameters();

            parms.TransposeMatrices = true;

            return parms;
        }

        #endregion GpuProgram Members
    }

    /// <summary>
    ///     GeForceFX class fragment program.
    /// </summary>
    public class FP30GpuProgram : NV3xGpuProgram {
        #region Constructor

        public FP30GpuProgram(string name, GpuProgramType type, string syntaxCode) 
            : base(name, type, syntaxCode) {}

        #endregion Constructor

        #region GpuProgram members

        /// <summary>
        ///     Binds named parameters to fp30 programs.
        /// </summary>
        /// <param name="parms"></param>
        public override void BindParameters(GpuProgramParameters parms) {
            if(parms.HasFloatConstants) {
                for(int index = 0; index < parms.FloatConstantCount; index++) {
                    string name = parms.GetNameByIndex(index);

					if(name != null) {
						GpuProgramParameters.FloatConstantEntry entry = parms.GetFloatConstant(index);

						// send the params 4 at a time
						Gl.glProgramNamedParameter4fvNV(programId, name.Length, name, entry.val);
					}
                }
            }  
        }
        #endregion GpuProgram members
    }

    /// <summary>
    ///     Factory class that handles requested for GeForceFX program implementations.
    /// </summary>
    public class NV3xGpuProgramFactory : IOpenGLGpuProgramFactory {
        #region IOpenGLGpuProgramFactory Members

        public GLGpuProgram Create(string name, Axiom.Graphics.GpuProgramType type, string syntaxCode) {
            if(type == GpuProgramType.Vertex) {
                return new VP30GpuProgram(name, type, syntaxCode);
            }
            else {
                return new FP30GpuProgram(name, type, syntaxCode);
            }
        }

        #endregion
    }
}
