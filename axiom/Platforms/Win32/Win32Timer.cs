#region BSD License
/*
 BSD License
Copyright (c) 2002, The CsGL Development Team
http://csgl.sourceforge.net/authors.html
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright notice,
   this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of The CsGL Development Team nor the names of its
   contributors may be used to endorse or promote products derived from this
   software without specific prior written permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
   COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
   INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
   BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
   LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
   CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
   LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
   ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
   POSSIBILITY OF SUCH DAMAGE.
 */
#endregion BSD License

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Axiom.Core;

namespace Axiom.Platforms.Win32
{
	/// <summary>
	///		Encapsulates the functionality of the platform's highest resolution timer available.
	/// </summary>
	/// <remarks>
	///		On Windows this will be a QueryPerformanceCounter (if available), otherwise it will be TimeGetTime().  
	/// </remarks>
	public class Win32Timer : ITimer
	{
		#region Private Fields
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Win32Timer));

		/// <summary>
		///		The Type Of Timer Supported On This System
		/// </summary>
		private TimerType timerType = TimerType.None;			
		/// <summary>
		///		The Frequency Of The Timer
		/// </summary>
		private ulong timerFrequency = 0;
		/// <summary>
		///		Is This Timer Running?
		/// </summary>
		private bool timerIsRunning = false;
		/// <summary>
		///		The Timer Start Count.
		/// </summary>
		private ulong timerStartCount = 0;

		#endregion Private Fields

		#region Enums

		/// <summary>
		/// The type of timer supported by this platform.
		/// </summary>
		public enum TimerType {
			/// <summary>
			/// No timer available.
			/// </summary>
			None,
			/// <summary>
			/// The timer is a Query Performance Counter.
			/// </summary>
			QueryPerformanceCounter,
			/// <summary>
			/// The timer will use TimeGetTime.
			/// </summary>
			TimeGetTime
		}

		#endregion Enums

		#region Constructor

		/// <summary>
		/// This static constructor determines which platform timer to use
		/// and populates the timer's <see cref="Frequency" />
		/// and <see cref="TimerType" />.
		/// </summary>
		internal Win32Timer() {
			bool test = false;
			ulong testTime = 0;

			// Try The Windows QueryPerformanceCounter.
			try {
				test = QueryPerformanceFrequency(ref timerFrequency);
			}
			catch(DllNotFoundException e) {
                log.Warn(e.ToString());
				test = false;
			}
			catch(EntryPointNotFoundException e) {
                log.Warn(e.ToString());
				test = false;
			}

			// If The QueryPerformanceCounter Is Supported
			if(test && timerFrequency != 0) {			
				// Let's Use It				
				timerType = TimerType.QueryPerformanceCounter;							
			}
			else {			
				// Otherwise, lets try TimeGetTime()											
				try {
					test = true;
					testTime = timeGetTime();
				}
				catch(DllNotFoundException e) {
                    log.Warn(e.ToString());
					test = false;
				}
				catch(EntryPointNotFoundException e) {
                    log.Warn(e.ToString());
					test = false;
				}

				// If TimeGetTime Is Supported
				if(test && testTime != 0) {
					// Let's Use It
					timerType = TimerType.TimeGetTime;
					timerFrequency = 1000;
				}
			}
		}

		#endregion Constructor

		#region Methods

		/// <summary>
		/// Gets the current tick count.
		/// </summary>
		/// <returns>
		/// Number Of Ticks (ulong).
		/// </returns>
		private ulong GetCurrentCount() {
			ulong tmp = 0;

			if(timerType == TimerType.QueryPerformanceCounter) {
				QueryPerformanceCounter(ref tmp);
				return tmp;
			}
			else if(timerType == TimerType.TimeGetTime) {
				tmp = timeGetTime();
				return tmp;	
			}
			else {
				return 0;
			}
		}

		/// <summary>
		/// Start this instance's timer.
		/// </summary>
		public void Start() {
			// get new start count
			timerStartCount = GetCurrentCount();	
			// mark that the timer is running
			timerIsRunning = true;
		}

		#endregion Methods

		#region Public Properties
		/// <summary>
		/// Gets a <see cref="System.UInt64" /> representing the 
		/// current tick count of the timer.
		/// </summary>
		public ulong Count {
			get { 
				return GetCurrentCount();
			}
		}

		/// <summary>
		/// Gets a <see cref="System.UInt64" /> representing the 
		/// frequency of the counter in ticks-per-second.
		/// </summary>
		public ulong Frequency {
			get {
				return timerFrequency;
			}
		}

		/// <summary>
		/// Gets a <see cref="System.Boolean" /> representing whether the 
		/// timer has been started and is currently running.
		/// </summary>
		public bool IsRunning {
			get {
				return timerIsRunning;
			}
		}

		/// <summary>
		/// Gets a <see cref="System.Double" /> representing the 
		/// resolution of the timer in seconds.
		/// </summary>
		public float Resolution {
			get {
				return ((float) 1.0 / (float) timerFrequency);
			}
		}

		/// <summary>
		/// Gets a <see cref="System.UInt64" /> representing the 
		/// tick count at the start of the timer's run.
		/// </summary>
		public ulong StartCount {
			get {
				return timerStartCount;
			}
		}

		#endregion Public Properties

		#region Externs

		[DllImport("kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(ref ulong frequencyCount);

		[DllImport("kernel32.dll")]
		private static extern bool QueryPerformanceCounter(ref ulong performanceCount);

		[DllImport("winmm.dll")]
		private static extern ulong timeGetTime();

		#endregion Externs

        #region ITimer Members

        /// <summary>
        ///		Reset this instance's timer.
        /// </summary>
        public void Reset() {
            // reset by restarting the timer
            Start();																	
        }

        public long Microseconds {
            get {
                // TODO:  Add Win32Timer.Microseconds getter implementation
                return 0;
            }
        }

        public long Milliseconds {
            get {
                long ticks = (long)(Count - StartCount);
                ticks *= 1000;
                ticks /= (long)Frequency;

                return ticks;
            }
        }

        #endregion
	}
}
