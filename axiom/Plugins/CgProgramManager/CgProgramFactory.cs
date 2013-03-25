using System;
using Axiom.Core;
using Axiom.Graphics;
using Tao.Cg;

namespace Axiom.CgPrograms {
	/// <summary>
	/// 	Summary description for CgProgramFactory.
	/// </summary>
	public class CgProgramFactory : IHighLevelGpuProgramFactory, IDisposable {
        #region Fields

        /// <summary>
        ///    ID of the active Cg context.
        /// </summary>
        private IntPtr cgContext;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///    Internal constructor.
        /// </summary>
        internal CgProgramFactory() {
            // create the Cg context
            cgContext = Cg.cgCreateContext();

            CgHelper.CheckCgError("Error creating Cg context.", cgContext);
        }

        #endregion Constructors

        #region IHighLevelGpuProgramFactory Members

        /// <summary>
        ///    Creates and returns a specialized CgProgram instance.
        /// </summary>
        /// <param name="name">Name of the program to create.</param>
        /// <param name="type">Type of program to create, vertex or fragment.</param>
        /// <returns>A new CgProgram instance within the current Cg Context.</returns>
        public HighLevelGpuProgram Create(string name, Axiom.Graphics.GpuProgramType type) {
            return new CgProgram(name, type, this.Language, cgContext);
        }

        /// <summary>
        ///    Returns 'cg' to identify this factory.
        /// </summary>
        public string Language {
            get {
                return "cg";
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///    Destroys the Cg context upon being disposed.
        /// </summary>
        public void Dispose() {
            // destroy the Cg context
            Cg.cgDestroyContext(cgContext);
        }

        #endregion
    }
}
