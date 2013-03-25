using System;

namespace Axiom.RenderSystems.OpenGL.GLSL {
	/// <summary>
	/// Summary description for GLSLHelper.
	/// </summary>
	public class GLSLHelper {
		/// <summary>
		///		Check for GL errors and report them in the Axiom Log.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="handle"></param>
		public static void CheckForGLSLError(string error, int handle) {
			CheckForGLSLError(error, handle, false, false);
		}

		/// <summary>
		///		Check for GL errors and report them in the Axiom Log.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="handle"></param>
		/// <param name="forceException"></param>
		/// <param name="forceInfoLog"></param>
		public static void CheckForGLSLError(string error, int handle, bool forceInfoLog, bool forceException) {
			// TODO: Implement
		}

		/// <summary>
		///		If there is a message in GL info log then post it in the Ogre Log
		/// </summary>
		/// <param name="message">The info log message string is appended to this string.</param>
		/// <param name="handle">The GL object handle that is used to retrieve the info log</param>
		/// <returns></returns>
		public static string LogObjectInfo(string message, int handle) {
			// TODO: Implement
			return string.Empty;
		}
	}
}
