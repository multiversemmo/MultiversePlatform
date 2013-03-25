using System;
using System.Runtime.InteropServices;
using Axiom.Core;
using Axiom.Input;

namespace Axiom.Platforms.SDL
{
	/// <summary>
	///		Platform management specialization for Microsoft Windows (r) platform.
	/// </summary>
	// TODO: Disposal of object create here.
	public class SdlPlatformManager : IPlatformManager {
		#region Fields

		/// <summary>
		///		Reference to the current input reader.
		/// </summary>
		private InputReader inputReader;
		/// <summary>
		///		Reference to the current active timer.
		/// </summary>
        private ITimer timer;

		#endregion Fields

		#region IPlatformManager Members

		public void Dispose() {
		}

		/// <summary>
		///		Creates an InputReader implemented using Microsoft DirectInput (tm).
		/// </summary>
		/// <returns></returns>
		public Axiom.Input.InputReader CreateInputReader() {
			inputReader = new SdlInputReader();
			return inputReader;
		}

		/// <summary>
		///		Creates a high precision Windows timer.
		/// </summary>
		/// <returns></returns>
		public ITimer CreateTimer() {
            timer = new SdlTimer();
			return timer;
		}

		/// <summary>
		///		Implements the Microsoft Windows (r) message pump for allowing the OS to process
		///		pending events.
		/// </summary>
		public void DoEvents() {
			// not required
		}

		#endregion
	}
}
