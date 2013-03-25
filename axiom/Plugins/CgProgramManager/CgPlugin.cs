using System;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.CgPrograms {
	/// <summary>
	///    Main plugin class.
	/// </summary>
	public class CgPlugin : IPlugin  {
        private CgProgramFactory factory;

        /// <summary>
        ///    Called when the plugin is started.
        /// </summary>
        public void Start() {
            // register our Cg Program Factory
            factory = new CgProgramFactory(); 

            HighLevelGpuProgramManager.Instance.AddFactory(factory);
        }

        /// <summary>
        ///    Called when the plugin is stopped.
        /// </summary>
        public void Stop() {
            //factory.Dispose();
        }
	}
}
