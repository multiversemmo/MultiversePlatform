using System;

namespace Axiom.Core
{
	/// <summary>
	///		Describes the interface for a platform independent timer.
	/// </summary>
	public interface ITimer {

		#region Methods 

		/// <summary>
		///		Resets this timer.
		/// </summary>
		/// <remarks>
		///		This must be called first before using the timer.
		/// </remarks>
		void Reset();

		#endregion Methods

		#region Properties

		/// <summary>
		///		Returns microseconds since initialization or last reset.
		/// </summary>
		long Microseconds {
			get;
		}

		/// <summary>
		///		Returns milliseconds since initialization or last reset.
		/// </summary>
		long Milliseconds {
			get;
		}

		#endregion Properties
	}
}
