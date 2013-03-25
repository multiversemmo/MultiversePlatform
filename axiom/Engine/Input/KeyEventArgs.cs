using System;

namespace Axiom.Input {
	/// <summary>
	///		Events args for keyboard input events.
	/// </summary>
	public class KeyEventArgs : InputEventArgs {
		#region Fields

		/// <summary>
		///		Which key was pressed.
		/// </summary>
		protected KeyCodes key;

		#endregion Fields

		#region Constructor

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="key">Key that was pressed.</param>
		/// <param name="modifiers">Modifier keys pressed at the time of the event.</param>
		public KeyEventArgs(KeyCodes key, ModifierKeys modifiers) : base(modifiers) {
			this.key = key;
		}

		#endregion Constructor

		#region Properties

		/// <summary>
		///		The key that was pressed.
		/// </summary>
		public KeyCodes Key {
			get {
				return key;
			}
		}

		/// <summary>
		///		Character for the key that was pressed.
		/// </summary>
		public char KeyChar {
			get {
				return InputReader.GetKeyChar(key, modifiers);
			}
		}

		#endregion Properties
	}
}
