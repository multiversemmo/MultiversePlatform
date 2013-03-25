using System;
using Axiom.Graphics;

namespace Axiom.RenderSystems.OpenGL {
	/// <summary>
	///     Interface that can be implemented by a class that is intended to
	///     handle creation of low level gpu program in OpenGL for a particular
	///     syntax code.
	/// </summary>
	public interface IOpenGLGpuProgramFactory {
        /// <summary>
        ///     Creates a gpu program for the specified syntax code (i.e. arbfp1, fp30, etc).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="syntaxCode"></param>
        /// <returns></returns>
        GLGpuProgram Create(string name, GpuProgramType type, string syntaxCode);
	}
}
