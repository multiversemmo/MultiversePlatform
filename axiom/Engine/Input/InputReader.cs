#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Collections;
using Axiom.Core;
using Axiom.Graphics;

namespace Axiom.Input {
	/// <summary>
	///		Abstract class which allows input to be read from various
	///		controllers.
	///	 </summary>
	///	 <remarks>
	///		Temporary implementation only. This class is likely to be
	///		refactored into a better design when I get time to look at it
	///		properly. For now it's a quick-and-dirty way to get what I need.
	/// </remarks>
	public abstract class InputReader : IDisposable {
		#region Fields

		/// <summary>
		///		Flag for whether or not to fire keyboard events.
		/// </summary>
		protected bool useKeyboardEvents;
		/// <summary>
		///		Flag for whether or not to fire mouse events.
		/// </summary>
		protected bool useMouseEvents;

		/// <summary>
		///		Active modifier keys.
		/// </summary>
		protected ModifierKeys modifiers;

		#endregion Fields

		#region Abstract Members

		#region Methods

		/// <summary>
		///		Subclasses should initialize the underlying input subsystem using this
		///		method.
		/// </summary>
		/// <param name="parent">Parent window that the input belongs to.</param>
		/// <param name="eventQueue">Used for buffering input.  Events will be added to the queue by the input reader.</param>
		/// <param name="useKeyboard"></param>
		/// <param name="useMouse"></param>
		/// <param name="useGamepad"></param>
		/// <param name="ownMouse">
		///		If true, input will be taken over from the OS and exclusive to the window.
		///		If false, input will still be shared with other apps.
		///	</param>
		public abstract void Initialize(RenderWindow parent, bool useKeyboard, bool useMouse, bool useGamepad, bool ownMouse, bool ownKeyboard);

		/// <summary>
		///		Captures the state of all the input devices.
		///	</summary>
		///	 <remarks>
		///		This method captures the state of all input devices and
		///		stores it internally for use when the enquiry methods are
		///		next called. This is done to ensure that all input is
		///		captured at once and therefore combinations of input are not
		///		subject to time differences when methods are called.
		/// </remarks>
		public abstract void Capture();

        public abstract bool HasFocus();

		/// <summary>
		///		Used to check if a particular key was pressed during the last call to Capture.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public abstract bool IsKeyPressed(Axiom.Input.KeyCodes key);

		/// <summary>
		///    Returns true if the specified mouse button is currently down.
		/// </summary>
		/// <param name="button">Mouse button to query.</param>
		/// <returns>True if the mouse button is down, false otherwise.</returns>
		public abstract bool IsMousePressed(Axiom.Input.MouseButtons button);

		#endregion Methods

		#region Properties

        public abstract bool CursorEnabled { get; set; }

		/// <summary>
		///		Retrieves the relative (compared to the last input poll) mouse movement
		///		on the X (horizontal) axis.
		/// </summary>
		public abstract int RelativeMouseX { get; }

		/// <summary>
		///		Retrieves the relative (compared to the last input poll) mouse movement
		///		on the Y (vertical) axis.
		/// </summary>
		public abstract int RelativeMouseY { get; }

		/// <summary>
		///		Retrieves the relative (compared to the last input poll) mouse movement
		///		on the Z (mouse wheel) axis.
		/// </summary>
		public abstract int RelativeMouseZ { get; }

		/// <summary>
		///		Retrieves the absolute mouse position on the X (horizontal) axis.
		/// </summary>
		public abstract int AbsoluteMouseX { get; }

		/// <summary>
		///		Retrieves the absolute mouse position on the Y (vertical) axis.
		/// </summary>
		public abstract int AbsoluteMouseY { get; }

		/// <summary>
		///		Retrieves the absolute mouse position on the Z (mouse wheel) axis.
		/// </summary>
		public abstract int AbsoluteMouseZ { get; }

		/// <summary>
		///		Get/Set whether or not to use event based keyboard input notification.
		/// </summary>
		/// <value>
		///		When true, events will be fired when keyboard input occurs on a call to <see cref="Capture"/>.
		///		When false, the current keyboard state will be available via <see cref="IsKeyPressed"/> .
		/// </value>
		public abstract bool UseKeyboardEvents { get; set; }

		/// <summary>
		///		Get/Set whether or not to use event based mouse input notification.
		/// </summary>
		/// <value>
		///		When true, events will be fired when mouse input occurs on a call to <see cref="Capture"/>.
		///		When false, the current mouse state will be available via <see cref="IsMousePressed"/> .
		/// </value>
		public abstract bool UseMouseEvents { get; set; }

        public abstract bool OwnMouse { get; set; }
        public abstract bool OwnKeyboard { get; set; }

		#endregion Properties

		#endregion Abstract Members

		#region Base Members

		#region Methods

		/// <summary>
		///		Given a key code enum value, the corresponding character is returned.
		/// </summary>
		/// <param name="keyCode"></param>
		/// <returns></returns>
		public static char GetKeyChar(KeyCodes keyCode, ModifierKeys modifiers) {
			bool isShiftDown = (modifiers & ModifierKeys.Shift) > 0;

			switch(keyCode) {
				case KeyCodes.A:
					return isShiftDown ? 'A' : 'a';
				case KeyCodes.B:
					return isShiftDown ? 'B' : 'b';
				case KeyCodes.C:
					return isShiftDown ? 'C' : 'c';
				case KeyCodes.D:
					return isShiftDown ? 'D' : 'd';
				case KeyCodes.E:
					return isShiftDown ? 'E' : 'e';
				case KeyCodes.F:
					return isShiftDown ? 'F' : 'f';
				case KeyCodes.G:
					return isShiftDown ? 'G' : 'g';
				case KeyCodes.H:
					return isShiftDown ? 'H' : 'h';
				case KeyCodes.I:
					return isShiftDown ? 'I' : 'i';
				case KeyCodes.J:
					return isShiftDown ? 'J' : 'j';
				case KeyCodes.K:
					return isShiftDown ? 'K' : 'k';
				case KeyCodes.L:
					return isShiftDown ? 'L' : 'l';
				case KeyCodes.M:
					return isShiftDown ? 'M' : 'm';
				case KeyCodes.N:
					return isShiftDown ? 'N' : 'n';
				case KeyCodes.O:
					return isShiftDown ? 'O' : 'o';
				case KeyCodes.P:
					return isShiftDown ? 'P' : 'p';
				case KeyCodes.Q:
					return isShiftDown ? 'Q' : 'q';
				case KeyCodes.R:
					return isShiftDown ? 'R' : 'r';
				case KeyCodes.S:
					return isShiftDown ? 'S' : 's';
				case KeyCodes.T:
					return isShiftDown ? 'T' : 't';
				case KeyCodes.U:
					return isShiftDown ? 'U' : 'u';
				case KeyCodes.V:
					return isShiftDown ? 'V' : 'v';
				case KeyCodes.W:
					return isShiftDown ? 'W' : 'w';
				case KeyCodes.X:
					return isShiftDown ? 'X' : 'x';
				case KeyCodes.Y:
					return isShiftDown ? 'Y' : 'y';
				case KeyCodes.Z:
					return isShiftDown ? 'Z' : 'z';
				case KeyCodes.Space:
					return ' ';
				case KeyCodes.QuestionMark:
					return isShiftDown ? '?' : '/';
				case KeyCodes.Comma:
					return isShiftDown ? '<' : ',';
				case KeyCodes.Period:
					return isShiftDown ? '>' : '.';
				case KeyCodes.D0:
					return isShiftDown ? ')' : '0';
				case KeyCodes.D1:
					return isShiftDown ? '!' : '1';
				case KeyCodes.D2:
					return isShiftDown ? '@' : '2';
				case KeyCodes.D3:
					return isShiftDown ? '#' : '3';
				case KeyCodes.D4:
					return isShiftDown ? '$' : '4';
				case KeyCodes.D5:
					return isShiftDown ? '%' : '5';
				case KeyCodes.D6:
					return isShiftDown ? '^' : '6';
				case KeyCodes.D7:
					return isShiftDown ? '&' : '7';
				case KeyCodes.D8:
					return isShiftDown ? '*' : '8';
				case KeyCodes.D9:
					return isShiftDown ? '(' : '9';
				case KeyCodes.Semicolon:
					return isShiftDown ? ':' : ';';
				case KeyCodes.Quotes:
					return isShiftDown ? '"' : '\'';
				case KeyCodes.OpenBracket:
					return isShiftDown ? '{' : '[';
				case KeyCodes.CloseBracket:
					return isShiftDown ? '}' : ']';
				case KeyCodes.Backslash:
					return isShiftDown ? '|' : '\\';
				case KeyCodes.Plus:
					return isShiftDown ? '+' : '=';
				case KeyCodes.Subtract:
					return isShiftDown ? '_' : '-';
				case KeyCodes.Tilde:
					return isShiftDown ? '~' : '`';

				default:
					return char.MinValue;//0
			}
		}

		/// <summary>
		///		Helper method for running logic on a key change.
		/// </summary>
		/// <param name="key">Code of the key being changed</param>
		/// <param name="down">True if the key is being pressed down, false if being released.</param>
		protected void KeyChanged(KeyCodes key, bool down) {
			if(down) {
				switch(key) {
					case KeyCodes.LeftAlt:
					case KeyCodes.RightAlt:
						modifiers |= ModifierKeys.Alt;
						break;

					case KeyCodes.LeftShift:
					case KeyCodes.RightShift:
						modifiers |= ModifierKeys.Shift;
						break;

					case KeyCodes.LeftControl:
					case KeyCodes.RightControl:
						modifiers |= ModifierKeys.Control;
						break;
				}

				Axiom.Input.KeyEventArgs e = new Axiom.Input.KeyEventArgs(key, modifiers);
				OnKeyDown(e);
			}
			else {
				switch(key) {
					case KeyCodes.LeftAlt:
					case KeyCodes.RightAlt:
						modifiers &= ~ModifierKeys.Alt;
						break;

					case KeyCodes.LeftShift:
					case KeyCodes.RightShift:
						modifiers &= ~ModifierKeys.Shift;
						break;

					case KeyCodes.LeftControl:
					case KeyCodes.RightControl:
						modifiers &= ~ModifierKeys.Control;
						break;
				}

				Axiom.Input.KeyEventArgs e = new Axiom.Input.KeyEventArgs(key, modifiers);
				OnKeyUp(e);
			}
		}

		/// <summary>
		///		Helper method for running logic on a mouse change.
		/// </summary>
		/// <param name="mouse">Data for the set of mouse events</param>
		protected void MouseChanged(MouseData mouseData) {
  			if (mouseData.button != MouseButtons.None) {
                if (mouseData.down) {
                    Axiom.Input.MouseEventArgs e =
                        new Axiom.Input.MouseEventArgs(mouseData, modifiers);
                    if ((mouseData.button & MouseButtons.Left) != 0)
                        modifiers |= ModifierKeys.MouseButton0;
                    if ((mouseData.button & MouseButtons.Right) != 0)
                        modifiers |= ModifierKeys.MouseButton1;
                    if ((mouseData.button & MouseButtons.Middle) != 0)
                        modifiers |= ModifierKeys.MouseButton2;
                    OnMouseDown(e);
                } else {
                    if ((mouseData.button & MouseButtons.Left) != 0)
                        modifiers &= ~ModifierKeys.MouseButton0;
                    if ((mouseData.button & MouseButtons.Right) != 0)
                        modifiers &= ~ModifierKeys.MouseButton1;
                    if ((mouseData.button & MouseButtons.Middle) != 0)
                        modifiers &= ~ModifierKeys.MouseButton2;
                    Axiom.Input.MouseEventArgs e =
                        new Axiom.Input.MouseEventArgs(mouseData, modifiers);
                    OnMouseUp(e);
                }
            }
            if (mouseData.relativeX != 0 ||
                mouseData.relativeY != 0 ||
                mouseData.relativeZ != 0) {
                Axiom.Input.MouseEventArgs e =
                    new Axiom.Input.MouseEventArgs(mouseData, modifiers);
                OnMouseMoved(e);
            }
		}


		#endregion Methods

		#endregion Base Members

		#region Events

		// Note: Events are only applicable when UseMouseEvents or UseKeyboardEvents are set to true.

		#region Declarations

		/// <summary>
		///		Occurs when a key is initially pressed down.
		/// </summary>
		public event KeyboardEventHandler KeyDown;

		/// <summary>
		///		Occurs when a key is released from being pressed down.
		/// </summary>
		public event KeyboardEventHandler KeyUp;

        /// <summary>
        ///		Occurs when we lose the keyboard (e.g. lost focus).
        /// </summary>
        public event EventHandler KeyboardLost;

		/// <summary>
		///		Occurs when a mouse button is initially pressed down.
		/// </summary>
		public event MouseEventHandler MouseDown;

		/// <summary>
		///		Occurs when a mouse button is released from being pressed down.
		/// </summary>
		public event MouseEventHandler MouseUp;

		/// <summary>
		///		Occurs when a mouse is moved.
		/// </summary>
		public event MouseEventHandler MouseMoved;

        /// <summary>
        ///		Occurs when we lose the mouse (e.g. lost focus).
        /// </summary>
        public event EventHandler MouseLost;

		#endregion Declaration

		#region Trigger Methods

		/// <summary>
		///		Triggers the <see cref="KeyDown"/> event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnKeyDown(KeyEventArgs e) {
			if(KeyDown != null) {
				KeyDown(this, e);
			}
		}

		/// <summary>
		///		Triggers the <see cref="KeyUp"/> event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnKeyUp(KeyEventArgs e) {
			if(KeyUp != null) {
				KeyUp(this, e);
			}
		}


        /// <summary>
        ///		Triggers the <see cref="KeyboardLost"/> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected void OnKeyboardLost(EventArgs e) {
            if (KeyboardLost != null) {
                KeyboardLost(this, e);
            }
        }

		/// <summary>
		///		Triggers the <see cref="MouseDown"/> event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnMouseDown(MouseEventArgs e) {
			if (MouseDown != null) {
				MouseDown(this, e);
			}
		}

		/// <summary>
		///		Triggers the <see cref="MouseUp"/> event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnMouseUp(MouseEventArgs e) {
			if (MouseUp != null) {
				MouseUp(this, e);
			}
		}

        /// <summary>
        ///		Triggers the <see cref="MouseUp"/> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected void OnMouseMoved(MouseEventArgs e) {
            if (MouseMoved != null) {
                MouseMoved(this, e);
            }
        }

        /// <summary>
        ///		Triggers the <see cref="MouseLost"/> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected void OnMouseLost(EventArgs e) {
            if (MouseLost != null) {
                MouseLost(this, e);
            }
        }
		#endregion Trigger Methods

		#endregion Events

        #region IDisposable Members

        /// <summary>
        ///     Called to destroy this input reader.
        /// </summary>
        public abstract void Dispose();

        #endregion IDisposable Members
    }

	#region Delegates

	/// <summary>
	///		Delegate for mouse related events.
	/// </summary>
	public delegate void MouseEventHandler(object sender, MouseEventArgs e);

	/// <summary>
	///		Delegate for keyboard related events.
	/// </summary>
	public delegate void KeyboardEventHandler(object sender, KeyEventArgs e);

	#endregion Delegates
}
