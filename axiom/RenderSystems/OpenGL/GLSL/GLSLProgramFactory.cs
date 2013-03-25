using System;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.RenderSystems.OpenGL.GLSL {
	/// <summary>
	///		Factory class for GLSL programs.
	/// </summary>
	public sealed class GLSLProgramFactory : IHighLevelGpuProgramFactory, IDisposable {
        #region Fields

        /// <summary>
        ///     Language string.
        /// </summary>
		private static string languageName = "glsl";
        /// <summary>
        ///     Reference to the link program manager we create.
        /// </summary>
        private GLSLLinkProgramManager glslLinkProgramMgr;

        #endregion Fields

        #region Constructor

        /// <summary>
        ///     Default constructor.
        /// </summary>
        internal GLSLProgramFactory() {
            // instantiate the singleton
            glslLinkProgramMgr = new GLSLLinkProgramManager();
        }

        #endregion Constructor

        #region IHighLevelGpuProgramFactory Implementation

		/// <summary>
		///		Creates and returns a new GLSL program object.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="type">Type of the object.</param>
		/// <returns>A newly created GLSL program object.</returns>
		public HighLevelGpuProgram Create(string name, Axiom.Graphics.GpuProgramType type) {
			return new GLSLProgram(name, type, languageName);
		}

		/// <summary>
		///		Returns the language code for this high level program manager.
		/// </summary>
		public string Language {
			get {
				return languageName;
			}
        }

        #endregion IHighLevelGpuProgramFactory Implementation

        #region IDisposable Implementation

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public void Dispose() {
            if (glslLinkProgramMgr != null) {
                glslLinkProgramMgr.Dispose();
            }
        }

        #endregion IDisposable Implementation
    }
}
