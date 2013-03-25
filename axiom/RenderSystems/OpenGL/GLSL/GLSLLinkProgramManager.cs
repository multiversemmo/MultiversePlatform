using System;
using System.Collections;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.GLSL {
	/// <summary>
	///		Axiom assumes that there are seperate vertex and fragment programs to deal with but
	///		GLSL has one program object that represents the active vertex and fragment shader objects
	///		during a rendering state.  GLSL Vertex and fragment 
	///		shader objects are compiled seperately and then attached to a program object and then the
	///		program object is linked.  Since Ogre can only handle one vertex program and one fragment
	///		program being active in a pass, the GLSL Link Program Manager does the same.  The GLSL Link
	///		program manager acts as a state machine and activates a program object based on the active
	///		vertex and fragment program.  Previously created program objects are stored along with a unique
	///		key in a hash_map for quick retrieval the next time the program object is required.
	/// </summary>
	public sealed class GLSLLinkProgramManager : IDisposable {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static GLSLLinkProgramManager instance;

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal GLSLLinkProgramManager() {
            if (instance == null) {
                instance = this;
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static GLSLLinkProgramManager Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

		#region Fields

		/// <summary>
		///		List holding previously created program objects.
		/// </summary>
		private Hashtable linkPrograms = new Hashtable();
		/// <summary>
		///		Currently active vertex GPU program.
		/// </summary>
        private GLSLGpuProgram activeVertexProgram;
        /// <summary>
		///		Currently active fragment GPU program.
		/// </summary>
        private GLSLGpuProgram activeFragmentProgram;
        /// <summary>
		///		Currently active link program.
		/// </summary>
        private GLSLLinkProgram activeLinkProgram;

		#endregion Fields

		#region Properties

		/// <summary>
		///		Get the program object that links the two active shader objects together
		///		if a program object was not already created and linked a new one is created and linked.
		/// </summary>
		public GLSLLinkProgram ActiveLinkProgram {
			get {
				// if there is an active link program then return it
				if(activeLinkProgram != null) {
					return activeLinkProgram;
				}

				// no active link program so find one or make a new one
				// is there an active key?
				int activeKey = 0;

				if(activeVertexProgram != null) {
					activeKey = activeVertexProgram.ProgramID << 8;
				}

				if(activeFragmentProgram != null) {
					activeKey += activeFragmentProgram.ProgramID;
				}

				// only return a link program object if a vertex or fragment program exist
				if(activeKey > 0) {
					if(linkPrograms[activeKey] == null) {
						activeLinkProgram = new GLSLLinkProgram();
						linkPrograms[activeKey] = activeLinkProgram;

						// tell shaders to attach themselves to the LinkProgram
						// let the shaders do the attaching since they may have several children to attach
						if(activeVertexProgram != null) {
							activeVertexProgram.GLSLProgram.AttachToProgramObject(activeLinkProgram.GLHandle);
						}

						if(activeFragmentProgram != null) {
							activeFragmentProgram.GLSLProgram.AttachToProgramObject(activeLinkProgram.GLHandle);
						}
					}
					else {
						// found a link program in map container so make it active
						activeLinkProgram = (GLSLLinkProgram)linkPrograms[activeKey];
					}
				}

				// make the program object active
				if(activeLinkProgram != null) {
					activeLinkProgram.Activate();
				}

				return activeLinkProgram;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		///		Set the active fragment shader for the next rendering state.
		/// </summary>
		/// <remarks>
		///		The active program object will be cleared.
		///		Normally called from the GLSLGpuProgram.BindProgram and UnbindProgram methods
		/// </remarks>
		/// <param name="fragmentProgram"></param>
		public void SetActiveFragmentShader(GLSLGpuProgram fragmentProgram) {
			if(fragmentProgram != activeFragmentProgram) {
				activeFragmentProgram = fragmentProgram;

				// active link program is no longer valid
				activeLinkProgram = null;

				// change back to fixed pipeline
				Gl.glUseProgramObjectARB(0);
			}
		}

		/// <summary>
		///		Set the active vertex shader for the next rendering state.
		/// </summary>
		/// <remarks>
		///		The active program object will be cleared.
		///		Normally called from the GLSLGpuProgram.BindProgram and UnbindProgram methods
		/// </remarks>
		/// <param name="vertexProgram"></param>
		public void SetActiveVertexShader(GLSLGpuProgram vertexProgram) {
			if(vertexProgram != activeVertexProgram) {
				activeVertexProgram = vertexProgram;

				// active link program is no longer valid
				activeLinkProgram = null;

				// change back to fixed pipeline
				Gl.glUseProgramObjectARB(0);
			}
		}

		#endregion Methods

        #region IDisposable Members

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public void Dispose() {
            foreach (GLSLLinkProgram program in linkPrograms.Values) {
                program.Dispose();
            }

            linkPrograms.Clear();
        }

        #endregion IDisposable Members
    }
}
