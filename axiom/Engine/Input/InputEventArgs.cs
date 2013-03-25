using System;

namespace Axiom.Input
{
	/// <summary>
	/// Summary description for InputEvent.
	/// </summary>
	public class InputEventArgs : EventArgs {
		#region Fields

		/// <summary>
		///		Special keys currently pressed during this event.
		/// </summary>
		protected ModifierKeys modifiers;
		/// <summary>
		///		Has this event been handled?
		/// </summary>
		protected bool handled;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="modifiers">Special modifier keys down at the time of this event.</param>
		public InputEventArgs(ModifierKeys modifiers) {
			this.modifiers = modifiers;
		}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		Get/Set whether or not this input event has been handled.
		/// </summary>
		public bool Handled {
			get {
				return handled;
			}
			set {
				handled = value;
			}
		}

		/// <summary>
		///		True if the alt key was down during this event.
		/// </summary>
		public bool IsAltDown {
			get {
				return (modifiers & ModifierKeys.Alt) != 0;
			}
		}

		/// <summary>
		///		True if the shift key was down during this event.
		/// </summary>
		public bool IsShiftDown {
			get {
				return (modifiers & ModifierKeys.Shift) != 0;
			}
		}

		/// <summary>
		///		True if the ctrl key was down during this event.
		/// </summary>
		public bool IsControlDown {
			get {
				return (modifiers & ModifierKeys.Control) != 0;
			}
		}

		/// <summary>
		///		Gets the modifier keys that were down during this event.
		/// </summary>
		/// <remarks>
		///		This is a combination of values from the <see cref="ModifierKeys"/> enum.
		/// </remarks>
		public ModifierKeys Modifiers {
			get {
				return modifiers;
			}
		}

		#endregion Properties
	}
}
