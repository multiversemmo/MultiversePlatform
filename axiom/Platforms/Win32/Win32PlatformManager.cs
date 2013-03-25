using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Axiom.Core;
using Axiom.Input;

namespace Axiom.Platforms.Win32
{
	/// <summary>
	///		Platform management specialization for Microsoft Windows (r) platform.
	/// </summary>
	public class Win32PlatformManager : IPlatformManager {
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

		/// <summary>
		///		Creates an InputReader implemented using Microsoft DirectInput (tm).
		/// </summary>
		/// <returns></returns>
		public Axiom.Input.InputReader CreateInputReader() {
			inputReader = new Win32InputReader();
			return inputReader;
		}

		/// <summary>
		///		Creates a high precision Windows timer.
		/// </summary>
		/// <returns></returns>
		public ITimer CreateTimer() {
            timer = new Win32Timer();
			return timer;
		}

		/// <summary>
		///		Implements the Microsoft Windows (r) message pump for allowing the OS to process
		///		pending events.
		/// </summary>
		public void DoEvents() {
            Application.DoEvents();
		}

        /// <summary>
        ///     Called when the engine is being shutdown.
        /// </summary>
        public void Dispose() {
            if (inputReader != null) {
                inputReader.Dispose();
            }
        }

		#endregion
	}
}
