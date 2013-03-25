using System;
using System.Runtime.InteropServices;
using System.Text;
using Axiom.Graphics;
using Axiom.RenderSystems.OpenGL;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.ARB {
	/// <summary>
	/// Summary description for ARBGpuProgram.
	/// </summary>
	public class ARBGpuProgram : GLGpuProgram {
        #region Constructor

        public ARBGpuProgram(string name, GpuProgramType type, string syntaxCode)
            : base(name, type, syntaxCode) {

            // set the type of program for ARB
            programType = (type == GpuProgramType.Vertex) ? Gl.GL_VERTEX_PROGRAM_ARB : Gl.GL_FRAGMENT_PROGRAM_ARB;

            // generate a new program
            Gl.glGenProgramsARB(1, out programId);
        }

        #endregion Constructor

        #region Implementation of GpuProgram

        /// <summary>
        ///     Load Assembler gpu program source.
        /// </summary>
        protected override void LoadFromSource() {
            Gl.glBindProgramARB(programType, programId);

            // MONO: Cannot compile programs when passing in the string as is for whatever reason.
            // would get "Invalid vertex program header", which I assume means the source got mangled along the way
            byte[] bytes = Encoding.ASCII.GetBytes(source);
            IntPtr sourcePtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            Gl.glProgramStringARB(programType, Gl.GL_PROGRAM_FORMAT_ASCII_ARB, source.Length, sourcePtr);

            // check for any errors
            if(Gl.glGetError() == Gl.GL_INVALID_OPERATION) {
                int pos;
                string error;

                Gl.glGetIntegerv(Gl.GL_PROGRAM_ERROR_POSITION_ARB, out pos);
                error = Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_PROGRAM_ERROR_STRING_ARB));

                throw new Exception(string.Format("Error on line {0} in program '{1}'\nError: {2}", pos, name, error));
            }
        }

        /// <summary>
        ///     Unload GL gpu programs.
        /// </summary>
        public override void Unload() {
            base.Unload();

            if (isLoaded) {
                Gl.glDeleteProgramsARB(1, ref programId);

                isLoaded = false;
            }
        }

        #endregion Implementation of GpuProgram

        #region Implementation of GLGpuProgram

        public override void Bind() {
            Gl.glEnable(programType);
            Gl.glBindProgramARB(programType, programId);
        }

        public override void Unbind() {
            Gl.glBindProgramARB(programType, 0);
            Gl.glDisable(programType);
        }

        public override void BindParameters(GpuProgramParameters parms) {
            if(parms.HasFloatConstants) {
                for(int index = 0; index < parms.FloatConstantCount; index++) {
                    GpuProgramParameters.FloatConstantEntry entry = parms.GetFloatConstant(index);

					if(entry.isSet) {
						// MONO: the 4fv version does not work
                        float[] vals = entry.val;
                        Gl.glProgramLocalParameter4fARB(programType, index, vals[0], vals[1], vals[2], vals[3]);
                    }
                }
            }            
        }

        #endregion Implementation of GLGpuProgram
	}

    /// <summary>
    ///     Creates a new ARB gpu program.
    /// </summary>
    public class ARBGpuProgramFactory : IOpenGLGpuProgramFactory {
        #region IOpenGLGpuProgramFactory Implementation

        public GLGpuProgram Create(string name, GpuProgramType type, string syntaxCode) {
            return new ARBGpuProgram(name, type, syntaxCode);
        }

        #endregion IOpenGLGpuProgramFactory Implementation
    }
}
