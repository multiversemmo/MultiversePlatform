using System;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	/// Summary description for Plugin.
	/// </summary>
	public sealed class Plugin : IPlugin {
		#region Implementation of IPlugin

        /// <summary>
        ///     Reference to a GLSL program factory.
        /// </summary>
		private GLSL.GLSLProgramFactory factory = new GLSL.GLSLProgramFactory();
        /// <summary>
        ///     Reference to the render system instance.
        /// </summary>
        private GLRenderSystem renderSystem = new GLRenderSystem();

        public void Start() {
			// add an instance of this plugin to the list of available RenderSystems
            Root.Instance.RenderSystems.Add("OpenGL", renderSystem);

            HighLevelGpuProgramManager.Instance.AddFactory(factory);
		}

		public void Stop() {
            factory.Dispose();
            renderSystem.Shutdown();
        }

		#endregion Implementation of IPlugin
	}
}
