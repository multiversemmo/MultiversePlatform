using System;
using System.Collections;
using Axiom.Core;
using Axiom.Graphics;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.GLSL {
	/// <summary>
	///		Encapsulation of GLSL Program Object.
	/// </summary>
	public class GLSLLinkProgram : IDisposable {
		#region Structs

		/// <summary>
		///		 Structure used to keep track of named uniforms in the linked program object.
		/// </summary>
		public class UniformReference {
			public string name;
			public int type;
			public int location;
			public bool isFloat;
			public int elementCount;
		}
			
		#endregion Structs

		#region Inner Classes
		
		public class UniformReferenceList : ArrayList {
		}

		#endregion Inner Classes

		#region Fields

		/// <summary>
		///		Container of uniform references that are active in the program object.
		/// </summary>
		protected UniformReferenceList uniformReferences = new UniformReferenceList();
		/// <summary>
		///		Flag to indicate that uniform references have already been built.
		/// </summary>
		protected bool uniformRefsBuilt;
		/// <summary>
		///		GL handle for the program object.
		/// </summary>
		protected int glHandle;
		/// <summary>
		///		Flag indicating that the program object has been successfully linked
		/// </summary>
		protected bool linked;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Default constructor.
		/// </summary>
		public GLSLLinkProgram() {
			GLSLHelper.CheckForGLSLError("Error prior to creating GLSL program object.", 0);

			// create the shader program object
			glHandle = Gl.glCreateProgramObjectARB();

			GLSLHelper.CheckForGLSLError("Error Creating GLSL Program Object", 0);
		}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets the GL Handle for the program object.
		/// </summary>
		public int GLHandle {
			get {
				return glHandle;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		///		Makes a program object active by making sure it is linked and then putting it in use.
		/// </summary>
		public void Activate() {
			if(!linked) {
				int linkStatus;

				Gl.glLinkProgramARB(glHandle);
				Gl.glGetObjectParameterivARB(glHandle, Gl.GL_OBJECT_LINK_STATUS_ARB, out linkStatus);

				linked = (linkStatus != 0);

				// force logging and raise exception if not linked
				GLSLHelper.CheckForGLSLError("Error linking GLSL Program Object", glHandle, !linked, !linked);

				if(linked) {
					GLSLHelper.LogObjectInfo("GLSL link result : ", glHandle);
					BuildUniformReferences();
				}
			}
			
			if(linked) {
				Gl.glUseProgramObjectARB(glHandle);
			}
		}

		/// <summary>
		///		Build uniform references from active named uniforms.
		/// </summary>
		private void BuildUniformReferences() {
			if(!uniformRefsBuilt) {
				// scane through the active uniforms and add them to the reference list
				int uniformCount;
				int size = 0;
				const int BufferSize = 100;
				string uniformName;

				// get the count of active uniforms
				Gl.glGetObjectParameterivARB(glHandle, Gl.GL_OBJECT_ACTIVE_UNIFORMS_ARB, out uniformCount);

				// Loop over each of the active uniforms, and add them to the reference container
				// only do this for user defined uniforms, ignore built in gl state uniforms
				for(int i = 0; i < uniformCount; i++) {
					UniformReference newUniformReference = new UniformReference();

					// get the info for the current uniform
					System.Text.StringBuilder uniformNameBuilder = new System.Text.StringBuilder(BufferSize);
					int test;
					Gl.glGetActiveUniformARB(glHandle, i, BufferSize, out test, out size, out newUniformReference.type, uniformNameBuilder);

					uniformName = uniformNameBuilder.ToString();

					// don't add built in uniforms
					newUniformReference.location = Gl.glGetUniformLocationARB(glHandle, uniformName);

					if(newUniformReference.location >= 0) {
						// user defined uniform found, add it to the reference list
						newUniformReference.name = uniformName;

						// decode uniform size and type
						switch(newUniformReference.type) {
							case Gl.GL_FLOAT:
								newUniformReference.isFloat = true;
								newUniformReference.elementCount = 1;
								break;

							case Gl.GL_FLOAT_VEC2_ARB:
								newUniformReference.isFloat = true;
								newUniformReference.elementCount = 2;
								break;

							case Gl.GL_FLOAT_VEC3_ARB:
								newUniformReference.isFloat = true;
								newUniformReference.elementCount = 3;
								break;

							case Gl.GL_FLOAT_VEC4_ARB:
								newUniformReference.isFloat = true;
								newUniformReference.elementCount = 4;
								break;

							case Gl.GL_INT:
							case Gl.GL_SAMPLER_1D_ARB:
							case Gl.GL_SAMPLER_2D_ARB:
							case Gl.GL_SAMPLER_3D_ARB:
							case Gl.GL_SAMPLER_CUBE_ARB:
								newUniformReference.isFloat = false;
								newUniformReference.elementCount = 1;
								break;

							case Gl.GL_INT_VEC2_ARB:
								newUniformReference.isFloat = false;
								newUniformReference.elementCount = 2;
								break;

							case Gl.GL_INT_VEC3_ARB:
								newUniformReference.isFloat = false;
								newUniformReference.elementCount = 3;
								break;

							case Gl.GL_INT_VEC4_ARB:
								newUniformReference.isFloat = false;
								newUniformReference.elementCount = 4;
								break;
						} // end switch

						uniformReferences.Add(newUniformReference);
					} // end if
				} // end for

				uniformRefsBuilt = true;
			}
		}

		/// <summary>
		///		Updates program object uniforms using data from GpuProgramParameters.
		///		normally called by GLSLGpuProgram.BindParameters() just before rendering occurs.
		/// </summary>
		/// <param name="parameters">GPU Parameters to use to update the uniforms params.</param>
		public void UpdateUniforms(GpuProgramParameters parameters) {
			for(int i = 0; i < uniformReferences.Count; i++) {
				UniformReference uniformRef = (UniformReference)uniformReferences[i];

				GpuProgramParameters.FloatConstantEntry currentFloatEntry = null;
				GpuProgramParameters.IntConstantEntry currentIntEntry = null;

				if(uniformRef.isFloat) {
					currentFloatEntry = parameters.GetNamedFloatConstant(uniformRef.name);

					if(currentFloatEntry != null) {
						if(currentFloatEntry.isSet) {
							switch(uniformRef.elementCount) {
								case 1:
									Gl.glUniform1fvARB(uniformRef.location, 1, currentFloatEntry.val);
									break;

								case 2:
									Gl.glUniform2fvARB(uniformRef.location, 1, currentFloatEntry.val);
									break;

								case 3:
									Gl.glUniform3fvARB(uniformRef.location, 1, currentFloatEntry.val);
									break;

								case 4:
									Gl.glUniform4fvARB(uniformRef.location, 1, currentFloatEntry.val);
									break;
							} // end switch
						}
					}
				}
				else {
					currentIntEntry = parameters.GetNamedIntConstant(uniformRef.name);

					if(currentIntEntry != null) {
						if(currentIntEntry.isSet) {
							switch(uniformRef.elementCount) {
								case 1:
									Gl.glUniform1ivARB(uniformRef.location, 1, currentIntEntry.val);
									break;

								case 2:
									Gl.glUniform2ivARB(uniformRef.location, 1, currentIntEntry.val);
									break;

								case 3:
									Gl.glUniform3ivARB(uniformRef.location, 1, currentIntEntry.val);
									break;

								case 4:
									Gl.glUniform4ivARB(uniformRef.location, 1, currentIntEntry.val);
									break;
							} // end switch
						}
					}
				}
			}
		}

		#endregion Methods

        #region IDisposable Members

        /// <summary>
        ///     Called to destroy the program used by this link program.
        /// </summary>
        public void Dispose() {
            Gl.glDeleteObjectARB(glHandle);
        }

        #endregion
    }
}
