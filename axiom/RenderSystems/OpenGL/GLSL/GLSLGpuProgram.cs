using System;
using Axiom.Graphics;

namespace Axiom.RenderSystems.OpenGL.GLSL {
	/// <summary>
	///		GLSL low level compiled shader object - this class is used to get at the linked program object 
	///		and provide an interface for GLRenderSystem calls.  GLSL does not provide access to the
	///		low level code of the shader so this class is really just a dummy place holder.
	///		GLSL uses a program object to represent the active vertex and fragment programs used
	///		but Axiom materials maintain seperate instances of the active vertex and fragment programs
	///		which creates a small problem for GLSL integration.  The GLSLGpuProgram class provides the
	///		interface between the GLSLLinkProgramManager , GLRenderSystem, and the active GLSLProgram
	///		instances.
	/// </summary>
	public class GLSLGpuProgram : GLGpuProgram {
		#region Fields

		/// <summary>
		///		GL Handle for the shader object.
		/// </summary>
		protected GLSLProgram glslProgram;

		/// <summary>
		///		Keep track of the number of vertex shaders created.
		/// </summary>
		protected static int vertexShaderCount;
		/// <summary>
		///		Keep track of the number of fragment shaders created.
		/// </summary>
		protected static int fragmentShaderCount;

		#endregion Fields

		#region Constructor

		public GLSLGpuProgram(GLSLProgram parent) : base(parent.Name, parent.Type, "glsl") {
			// store off the reference to the parent program
			glslProgram = parent;

			if(parent.Type == GpuProgramType.Vertex) {
				programId = ++vertexShaderCount;
			}
			else {
				programId = ++fragmentShaderCount;
			}

			// there is nothing to load
			loadFromFile = false;
		}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Gets the GLSLProgram for the shader object.
		/// </summary>
		public GLSLProgram GLSLProgram {
			get {
				return glslProgram;
			}
		}
	
		#endregion Properties

		#region Resource Implementation

		public override void Load() {
			isLoaded = true;
		}

		public override void Unload() {
			// nothing to unload
		}

		#endregion Resource Implementation

		#region GpuProgram Implementation

		public override void Bind() {
			// tell the Link Program Manager what shader is to become active
			if(type == GpuProgramType.Vertex) {
				GLSLLinkProgramManager.Instance.SetActiveVertexShader(this);
			}
			else {
				GLSLLinkProgramManager.Instance.SetActiveFragmentShader(this);
			}
		}

		public override void BindParameters(GpuProgramParameters parameters) {
			// activate the link program object
			GLSLLinkProgram linkProgram = GLSLLinkProgramManager.Instance.ActiveLinkProgram;

			// pass on parameters from params to program object uniforms
			linkProgram.UpdateUniforms(parameters);
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void LoadFromSource() {
			// nothing to load
		}

		public override void Unbind() {
			// tell the Link Program Manager what shader is to become inactive
			if(type == GpuProgramType.Vertex) {
				GLSLLinkProgramManager.Instance.SetActiveVertexShader(null);
			}
			else {
				GLSLLinkProgramManager.Instance.SetActiveFragmentShader(null);
			}
		}

		#endregion GpuProgram Implementation
	}
}
