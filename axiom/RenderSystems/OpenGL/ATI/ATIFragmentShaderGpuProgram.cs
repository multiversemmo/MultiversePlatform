using System;
using Axiom.Graphics;
using Axiom.RenderSystems.OpenGL;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.ATI {
	/// <summary>
	/// Summary description for ATIFragmentShaderGpuProgram.
	/// </summary>
	public class ATIFragmentShaderGpuProgram : GLGpuProgram {
		public ATIFragmentShaderGpuProgram(string name, GpuProgramType type, string syntaxCode)
            : base(name, type, syntaxCode) {

            programType = Gl.GL_FRAGMENT_SHADER_ATI;
            programId = Gl.glGenFragmentShadersATI(1);
		}

        #region Implementation of GpuProgram

        protected override void LoadFromSource() {
            PixelShader assembler = new PixelShader();

            //bool testError = assembler.RunTests();

            bool error = !assembler.Compile(source);

            if(!error) {
                Gl.glBindFragmentShaderATI(programId);
                Gl.glBeginFragmentShaderATI();

                // Compile and issue shader commands
                error = !assembler.BindAllMachineInstToFragmentShader();

                Gl.glEndFragmentShaderATI();
            }
            else {
            }
        }

        public override void Unload() {
            base.Unload ();

            // delete the fragment shader for good
            Gl.glDeleteFragmentShaderATI(programId);
        }


        #endregion Implementation of GpuProgram

        #region Implementation of GLGpuProgram

        public override void Bind() {
            Gl.glEnable(programType);
            Gl.glBindFragmentShaderATI(programId);
        }

        public override void BindParameters(GpuProgramParameters parms) {
            // program constants done internally by compiler for local
            if(parms.HasFloatConstants) {
				for(int index = 0; index < parms.FloatConstantCount; index++) {
					GpuProgramParameters.FloatConstantEntry entry = parms.GetFloatConstant(index);

					if(entry.isSet) {
						// send the params 4 at a time
						Gl.glSetFragmentShaderConstantATI(Gl.GL_CON_0_ATI + index, entry.val);
					}
				}
            }   
        }

        public override void Unbind() {
            Gl.glDisable(programType);
        }

        #endregion Implementation of GLGpuProgram
	}

    /// <summary>
    /// 
    /// </summary>
    public class ATIFragmentShaderFactory : IOpenGLGpuProgramFactory {
        #region IOpenGLGpuProgramFactory Members

        public GLGpuProgram Create(string name, Axiom.Graphics.GpuProgramType type, string syntaxCode) {
            // creates and returns a new ATI fragment shader implementation
            return new ATIFragmentShaderGpuProgram(name, type, syntaxCode);
        }

        #endregion

    }

}
