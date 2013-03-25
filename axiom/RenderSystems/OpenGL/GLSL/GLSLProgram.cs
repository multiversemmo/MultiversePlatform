using System;
using System.Collections;
using Axiom.Graphics;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.GLSL {
	/// <summary>
	///		Specialisation of HighLevelGpuProgram to provide support for OpenGL 
	///		Shader Language (GLSL).
	///	</summary>
	///	<remarks>
	///		GLSL has no target assembler or entry point specification like DirectX 9 HLSL.
	///		Vertex and Fragment shaders only have one entry point called "main".  
	///		When a shader is compiled, microcode is generated but can not be accessed by
	///		the application.
	///		GLSL also does not provide assembler low level output after compiling.  The GL Render
	///		system assumes that the Gpu program is a GL Gpu program so GLSLProgram will create a 
	///		GLSLGpuProgram that is subclassed from GLGpuProgram for the low level implementation.
	///		The GLSLProgram class will create a shader object and compile the source but will
	///		not create a program object.  It's up to GLSLGpuProgram class to request a program object
	///		to link the shader object to.
	///		<p/>
	///		GLSL supports multiple modular shader objects that can be attached to one program
	///		object to form a single shader.  This is supported through the "attach" material script
	///		command.  All the modules to be attached are listed on the same line as the attach command
	///		seperated by white space.
	///	</remarks>
	public class GLSLProgram : HighLevelGpuProgram {
		#region Fields

		/// <summary>
		///		The GL id for the program object.
		/// </summary>
		protected int glHandle;
		/// <summary>
		///		Flag indicating if shader object successfully compiled.
		/// </summary>
		protected bool isCompiled;
		/// <summary>
		///		Names of shaders attached to this program.
		/// </summary>
		protected string attachedShaderNames;
		/// <summary>
		///		Holds programs attached to this object.
		/// </summary>
		protected ArrayList attachedGLSLPrograms = new ArrayList();

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		public GLSLProgram(string name, GpuProgramType type, string language) : base(name, type, language) {
			// want scenemanager to pass on surface and light states to the rendersystem
			// these can be accessed in GLSL
			passSurfaceAndLightStates = true;

			// only create a shader object if glsl is supported
			if(IsSupported) {
				GLSLHelper.CheckForGLSLError("GL Errors before creating shader object.", 0);

				// create shader object
				glHandle = Gl.glCreateShaderObjectARB(type == GpuProgramType.Vertex ? Gl.GL_VERTEX_SHADER_ARB : Gl.GL_FRAGMENT_SHADER_ARB);

				GLSLHelper.CheckForGLSLError("GL Errors creating shader object.", 0);
			}
		}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets the GL id for the program object.
		/// </summary>
		public int GLHandle {
			get {
				return glHandle;
			}
		}

        public override int SamplerCount
        {
            get
            {
                // XXX - hack for now.  If we ever use GL, someone will have to fix this.
                return 8;
            }
        }

		#endregion Properties

		#region Methods

		/// <summary>
		///		Attach another GLSL Shader to this one.
		/// </summary>
		/// <param name="name"></param>
		public void AttachChildShader(string name) {
			// is the name valid and already loaded?
			// check with the high level program manager to see if it was loaded
			HighLevelGpuProgram hlProgram = HighLevelGpuProgramManager.Instance.GetByName(name);

			if(hlProgram != null) {
				if(hlProgram.SyntaxCode == "glsl") {
					// make sure attached program source gets loaded and compiled
					// don't need a low level implementation for attached shader objects
					// loadHighLevelImpl will only load the source and compile once
					// so don't worry about calling it several times
					GLSLProgram childShader = (GLSLProgram)hlProgram;

					// load the source and attach the child shader only if supported
					if(IsSupported) {
						childShader.LoadHighLevelImpl();
						// add to the constainer
						attachedGLSLPrograms.Add(childShader);
						attachedShaderNames += name + " ";
					}
				}
			}
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="programObject"></param>
		public void AttachToProgramObject(int programObject) {
			Gl.glAttachObjectARB(programObject, glHandle);
			GLSLHelper.CheckForGLSLError("Error attaching " + this.name + " shader object to GLSL Program Object.", programObject);

			// atach child objects
			for(int i = 0; i < attachedGLSLPrograms.Count; i++) {
				GLSLProgram childShader = (GLSLProgram)attachedGLSLPrograms[i];

				// bug in ATI GLSL linker : modules without main function must be recompiled each time 
				// they are linked to a different program object
				// don't check for compile errors since there won't be any
				// *** minor inconvenience until ATI fixes there driver
				childShader.Compile(false);
				childShader.AttachToProgramObject(programObject);
			}
		}

		/// <summary>
		///		
		/// </summary>
		protected bool Compile() {
			return Compile(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="checkErrors"></param>
		protected bool Compile(bool checkErrors) {
			Gl.glCompileShaderARB(glHandle);

			int compiled;

			// check for compile errors
			Gl.glGetObjectParameterivARB(glHandle, Gl.GL_OBJECT_COMPILE_STATUS_ARB, out compiled);

			isCompiled = (compiled != 0);

			// force exception if not compiled
			if(checkErrors) {
				GLSLHelper.CheckForGLSLError("Cannot compile GLSL high-level shader: " + name + " ", glHandle, !isCompiled, !isCompiled);

				if(isCompiled) {
					GLSLHelper.LogObjectInfo(name + " : GLGL compiled ", glHandle);
				}
			}

			return isCompiled;
		}

		#endregion Methods

		#region HighLevelGpuProgram Implementation

		protected override void CreateLowLevelImpl() {
			assemblerProgram = new GLSLGpuProgram(this);
		}

		/// <summary>
		///		
		/// </summary>
		protected override void LoadFromSource() {
			Gl.glShaderSourceARB(glHandle, 1, ref source, null);

			// check for load errors
			GLSLHelper.CheckForGLSLError("Cannot load GLGL high-level shader source " + name, 0);

			Compile();
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="parms"></param>
		protected override void PopulateParameterNames(GpuProgramParameters parms) {
			// can't populate parameter names in GLSL until link time
			// allow for names read from a material script to be added automatically to the list
			parms.AutoAddParamName = true;
		}

		/// <summary>
		///		Set a custom param for this high level gpu program.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		// TODO: Refactor to command pattern
		public override bool SetParam(string name, string val) {
			if(name == "attach") {
				//get all the shader program names: there could be more than one
				string[] shaderNames = val.Split(new char[] {' ', '\t'});

				// attach the specified shaders to this program
				for(int i = 0; i < shaderNames.Length; i++) {
					AttachChildShader(shaderNames[i]);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void UnloadImpl() {
			if(IsSupported) {
				// only delete it if it was supported to being with, else it won't exist
				Gl.glDeleteObjectARB(glHandle);
			}

			// just clearing the reference here
			assemblerProgram = null;
		}

		#endregion HighLevelGpuProgram Implementation
	}
}
