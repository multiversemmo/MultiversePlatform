using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using log4net;
using Axiom.Core;
using Axiom.Input;
using Axiom.Graphics;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using DInput = Microsoft.DirectX.DirectInput;

namespace Axiom.Platforms.Win32
{
	/// <summary>
	///		Win32 input implementation using Managed DirectInput (tm).
	/// </summary>
	public class Win32InputReader : InputReader {
		#region Fields
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Win32InputReader));

		/// <summary>
		///		Holds a snapshot of DirectInput keyboard state.
		/// </summary>
		protected KeyboardState keyboardState;
		/// <summary>
		///		Holds a snapshot of DirectInput mouse state.
		/// </summary>
		protected MouseState mouseState;
		/// <summary>
		///		DirectInput keyboard device.
		/// </summary>
		protected DInput.Device keyboardDevice;
		/// <summary>
		///		DirectInput mouse device.
		/// </summary>
		protected DInput.Device mouseDevice;
		protected int mouseRelX, mouseRelY, mouseRelZ;
		protected int mouseAbsX, mouseAbsY, mouseAbsZ;
		protected bool isInitialized;
		protected bool useMouse, useKeyboard, useGamepad;
		protected int mouseButtons;

		/// <summary>
		///		Active host control that reserves control over the input.
		/// </summary>
		protected System.Windows.Forms.Control control;
        /// <summary>
        ///	   Do we want exclusive use of the mouse?
        /// </summary>
        protected bool ownMouse;
        /// <summary>
        ///   Do we want exclusive use of the keyboard?
        /// </summary>
        protected bool ownKeyboard;
        /// <summary>
        ///	   Do we want to collect mouse input when we are in the background?
        /// </summary>
        /// <remarks>
        ///   For security reasons, if ownMouse is set, we will only collect
        ///   mouse input when we are in the foreground.
        /// </remarks>
        protected bool backgroundMouse = true;
        /// <summary>
        ///   Do we want to collect keyboard input when we are in the background?
        /// </summary>
        /// <remarks>
        ///   For security reasons, if ownKeyboard is set, we will only collect
        ///   keyboard input when we are in the foreground.
        /// </remarks>
        protected bool backgroundKeyboard = false;
		/// <summary>
		///		Reference to the render window that is the target of the input.
		/// </summary>
		protected RenderWindow window;
		/// <summary>
		///		Flag used to remember the state of the render window the last time input was captured.
		/// </summary>
		protected bool lastWindowActive;

        protected bool cursorEnabled = true;
        protected Point cursorDisabledPosition = new Point(0, 0);
        protected bool hardwareCursor = true;

        // Is the mouse over our control?
        protected bool mouseInControl;

		#endregion Fields
		
		#region Constants

		/// <summary>
		///		Size to use for DirectInput's input buffer.
		/// </summary>
		const int BufferSize = 1000;

		#endregion Constants

		#region InputReader Members

		#region Properties

        public override bool CursorEnabled {
            get {
                return cursorEnabled;
            }
            set {
                log.DebugFormat("Got Win32InputReader.CursorEnabled set to {0} (was {1})", value, cursorEnabled);                
                if (cursorEnabled == value)
                    return;
                if (!value)
                {
                    // If we just turned off the cursor, stash the position of the mouse.
                    // Each time we check the input, we will update the Cursor.Position
                    Point pt = new Point(mouseAbsX, mouseAbsY);
                    // if our point is no longer in our control, don't disable 
                    // the cursor.  If we don't have focus, return
                    if (!control.Bounds.Contains(pt) || !HasFocus())
                        return;
                    cursorDisabledPosition = pt;
                    // Also, make sure we get the mouse data
                    control.Capture = true;
                    log.DebugFormat("After capture; capture = {0}", control.Capture);
                }
                cursorEnabled = value;
            }
        }

		/// <summary>
		///		Retrieves the relative (compared to the last input poll) mouse movement
		///		on the X (horizontal) axis.
		/// </summary>
		public override int RelativeMouseX {
			get { 
				return mouseRelX; 
			}
		}

		/// <summary>
		///		Retrieves the relative (compared to the last input poll) mouse movement
		///		on the Y (vertical) axis.
		/// </summary>
		public override int RelativeMouseY {
			get { 
				return mouseRelY; 
			}
		}

		/// <summary>
		///		Retrieves the relative (compared to the last input poll) mouse movement
		///		on the Z (mouse wheel) axis.
		/// </summary>
		public override int RelativeMouseZ {
			get { 
				return mouseRelZ; 
			}
		}

		/// <summary>
		///		Retrieves the absolute mouse position on the X (horizontal) axis.
		/// </summary>
		public override int AbsoluteMouseX {
			get { 
				return mouseAbsX; 
			}
		}

		/// <summary>
		///		Retrieves the absolute mouse position on the Y (vertical) axis.
		/// </summary>
		public override int AbsoluteMouseY {
			get { 
				return mouseAbsY; 
			}
		}

		/// <summary>
		///		Retrieves the absolute mouse position on the Z (mouse wheel) axis.
		/// </summary>
		public override int AbsoluteMouseZ {
			get { 
				return mouseAbsZ; 
			}
		}

		/// <summary>
		///		Get/Set whether or not to use event based keyboard input notification.
		/// </summary>
		/// <value>
		///		When true, events will be fired when keyboard input occurs on a call to <see cref="Capture"/>.
		///		When false, the current keyboard state will be available via <see cref="IsKeyPressed"/> .
		/// </value>
		public override bool UseKeyboardEvents {
			get {
				return useKeyboardEvents;
			}
			set {
				if(useKeyboardEvents != value) {
					useKeyboardEvents = value;

					// dump the current keyboard device (if any)
					if(keyboardDevice != null) {
						keyboardDevice.Unacquire();
						keyboardDevice.Dispose();
					}

					// re-init the keyboard
					InitializeKeyboard();
				}
			}
		}

		/// <summary>
		///		Get/Set whether or not to use event based mouse input notification.
		/// </summary>
		/// <value>
		///		When true, events will be fired when mouse input occurs on a call to <see cref="Capture"/>.
		///		When false, the current mouse state will be available via <see cref="IsMousePressed"/> .
		/// </value>
		public override bool UseMouseEvents {
			get {
				return useMouseEvents;
			}
			set {
				if(useMouseEvents != value) {
					useMouseEvents = value;

					// dump the current mouse device (if any)
					if(mouseDevice != null) {
						mouseDevice.Unacquire();
						mouseDevice.Dispose();
					}

					// re-init the mouse
					InitializeMouse();
				}
			}
		}

        public override bool OwnMouse {
            get {
                return ownMouse;
            }
            set {
                ownMouse = value;
                // re-init the mouse
                InitializeMouse();
            }
        }

        public override bool OwnKeyboard {
            get {
                return ownKeyboard;
            }
            set {
                ownKeyboard = value;
                // re-init the keyboard
                InitializeKeyboard();
            }
        }

        public bool BackgroundMouse {
            get {
                return backgroundMouse;
            }
            set {
                backgroundMouse = value;
                // re-init the mouse
                InitializeMouse();
            }
        }

        public bool BackgroundKeyboard {
            get {
                return backgroundKeyboard;
            }
            set {
                backgroundKeyboard = value;
                // re-init the keyboard
                InitializeKeyboard();
            }
        }

		#endregion Properties

		#region Methods

        public override bool HasFocus() {
            if (control is Form)
                return control.Focused;
            return mouseInControl;
        }
		/// <summary>
		///		Captures the state of all active input controllers.
		/// </summary>
		public override void Capture() {
            try {
                if (!window.IsActive) 
                {
                    // if we were active, but are no longer active, trigger the
                    // mouse lost event
                    if (lastWindowActive)
                    {
                        OnMouseLost(new EventArgs());
                        lastWindowActive = false;
                        CursorEnabled = true;
                    }
                }
                else 
                {
                    // Grab the input, acquiring if we weren't active.
                    if (VerifyInputAcquired())
                    {
                        if (useKeyboard)
                        {
                            if (useKeyboardEvents)
                            {
                                ReadBufferedKeyboardData();
                            }
                            else
                            {
                                // TODO: Grab keyboard modifiers
                                CaptureKeyboard();
                            }
                        }

                        if (useMouse)
                        {
                            if (useMouseEvents)
                            {
                                ReadBufferedMouseData();
                            }
                            else
                            {
                                CaptureMouse();
                            }
                        }
                        if (!control.ContainsFocus)
                            control.Focus();

                    }
                    if (hardwareCursor && control != null)
                    {
                        // Usually, we would not capture if we are not active, but
                        // I do still want the app to track the cursor position.
                        HandleMouseMoved(control.PointToClient(Cursor.Position));
                    }
                }
            } catch (InputLostException) {
                if (lastWindowActive)
                {
                    OnMouseLost(new EventArgs());
                    lastWindowActive = false;
                    CursorEnabled = true;
                }
                throw;
            } catch (NotAcquiredException) {
                lastWindowActive = false;
                CursorEnabled = true;
                throw;
            }
        }

		/// <summary>
		///		Intializes DirectInput for use on Win32 platforms.
		/// </summary>
		/// <param name="window"></param>
		/// <param name="useKeyboard"></param>
		/// <param name="useMouse"></param>
		/// <param name="useGamepad"></param>
		public override void Initialize(RenderWindow window, bool useKeyboard, bool useMouse, bool useGamepad, bool ownMouse, bool ownKeyboard) {
			this.useKeyboard = useKeyboard;
            this.useMouse = useMouse;
			this.useGamepad = useGamepad;
			this.ownMouse = ownMouse;
            this.ownKeyboard = ownKeyboard;
			this.window = window;
            this.useMouseEvents = true;
            this.useKeyboardEvents = true;

            log.InfoFormat("Initialized input with parameters {0}:{1}:{2}:{3}:{4}:{5}", window, useKeyboard, useMouse, useGamepad, ownMouse, ownKeyboard);
			// for Windows, this should be a S.W.F.Control
			control = window.Handle as System.Windows.Forms.Control;
            if (control == null)
                throw new AxiomException("Win32InputReader requires the RenderWindow to have an associated handle of either a PictureBox or a Form.");

            control.MouseEnter += new System.EventHandler(this.control_MouseEnter);
            control.MouseLeave += new System.EventHandler(this.control_MouseLeave);

            // initialize keyboard if needed
			if(useKeyboard) {
				InitializeKeyboard();
			}

			// initialize the mouse if needed
			if(useMouse) {
				InitializeMouse();
			}
            Point pt = control.PointToClient(Cursor.Position);
            if (control.Bounds.Contains(pt))
                mouseInControl = true;
			// we are initialized
			isInitialized = true;
	
			mouseAbsX = pt.X;
			mouseAbsY = pt.Y;
		}

        public bool ContainsMouse
        {
            get
            {
                return mouseInControl;
            }
        }

        private void control_MouseEnter(object sender, EventArgs e)
        {
            mouseInControl = true;
        }

        private void control_MouseLeave(object sender, EventArgs e)
        {
            mouseInControl = false;
        }
		/// <summary>
		///		
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public override bool IsKeyPressed(KeyCodes key) {
			if(keyboardState != null) {
				// get the DInput.Key enum from the System.Windows.Forms.Keys enum passed in
				DInput.Key daKey = ConvertKeyEnum(key);

				if(keyboardState[daKey]) {
					return true;
				}
			}
            
			return false;
		}

		/// <summary>
		///    Returns true if the specified mouse button is currently down.
		/// </summary>
		/// <param name="button">Mouse button to query.</param>
		/// <returns>True if the mouse button is down, false otherwise.</returns>
        public override bool IsMousePressed(Axiom.Input.MouseButtons button) {
            if (button == Axiom.Input.MouseButtons.Left)
                return (modifiers & ModifierKeys.MouseButton0) != 0;
            else if (button == Axiom.Input.MouseButtons.Right)
                return (modifiers & ModifierKeys.MouseButton1) != 0;
            else if (button == Axiom.Input.MouseButtons.Middle)
                return (modifiers & ModifierKeys.MouseButton2) != 0;
            return false;
		}

        /// <summary>
        ///     Called when the platform manager is shutting down.
        /// </summary>
        public override void Dispose() {
            if (keyboardDevice != null) {
                keyboardDevice.Unacquire();
                keyboardDevice.Dispose();
                keyboardDevice = null;
            }

            if (mouseDevice != null) {
                mouseDevice.Unacquire();
                mouseDevice.Dispose();
                mouseDevice = null;
            }

            control.MouseEnter -= new System.EventHandler(this.control_MouseEnter);
            control.MouseLeave -= new System.EventHandler(this.control_MouseLeave);
        }


		#endregion Methods

		#endregion InputReader implementation

		#region Helper Methods

		/// <summary>
		///		Initializes the keyboard using either immediate mode or event based input.
		/// </summary>
		private void InitializeKeyboard() {
			if(useKeyboardEvents) {
				InitializeBufferedKeyboard();
			}
			else {
				InitializeImmediateKeyboard();
			}
		}

		/// <summary>
		///		Initializes the mouse using either immediate mode or event based input.
		/// </summary>
		private void InitializeMouse() {
			if(useMouseEvents) {
				InitializeBufferedMouse();
			}
			else {
				InitializeImmediateMouse();
			}
		}

		/// <summary>
		///		Initializes DirectInput for immediate input.
		/// </summary>
		private void InitializeImmediateKeyboard() {
			// Create the device.
			keyboardDevice = new DInput.Device(SystemGuid.Keyboard);

			// grab the keyboard
            CooperativeLevelFlags excl = ownKeyboard ? CooperativeLevelFlags.Exclusive : CooperativeLevelFlags.NonExclusive;
            CooperativeLevelFlags background = (backgroundKeyboard && !ownKeyboard) ? CooperativeLevelFlags.Background : CooperativeLevelFlags.Foreground;
           
            keyboardDevice.SetCooperativeLevel(control.FindForm(), excl | background);
            
			// Set the data format to the keyboard pre-defined format.
			keyboardDevice.SetDataFormat(DeviceDataFormat.Keyboard);

			try {
				keyboardDevice.Acquire();
			}
			catch {
				throw new Exception("Unable to acquire a keyboard using DirectInput.");
			}
		}

		/// <summary>
		///		Prepares DirectInput for non-immediate input capturing.
		/// </summary>
		private void InitializeBufferedKeyboard() {
			// create the device
			keyboardDevice = new DInput.Device(SystemGuid.Keyboard);

			// Set the data format to the keyboard pre-defined format.
			keyboardDevice.SetDataFormat(DeviceDataFormat.Keyboard);

			// grab the keyboard
            // For debugging, use the background flag so we don't lose input when we are in the debugger.
            // For release, use the foreground flag, so input to other apps doesn't show up here
            CooperativeLevelFlags excl = ownKeyboard ? CooperativeLevelFlags.Exclusive : CooperativeLevelFlags.NonExclusive;
            CooperativeLevelFlags background = (backgroundKeyboard && !ownKeyboard) ? CooperativeLevelFlags.Background : CooperativeLevelFlags.Foreground;

            keyboardDevice.SetCooperativeLevel(control.FindForm(), excl | background);

            // set the buffer size to use for input
			keyboardDevice.Properties.BufferSize = BufferSize;

            // note: dont acquire yet, wait till capture
            //try {
            //    keyboardDevice.Acquire();
            //}
            //catch {
            //    throw new Exception("Unable to acquire a keyboard using DirectInput.");
            //}
		}

		/// <summary>
		///		Prepares DirectInput for immediate mouse input.
		/// </summary>
		private void InitializeImmediateMouse() {
			// create the device
			mouseDevice = new DInput.Device(SystemGuid.Mouse);

			mouseDevice.Properties.AxisModeAbsolute = true;

			// set the device format so DInput knows this device is a mouse
			mouseDevice.SetDataFormat(DeviceDataFormat.Mouse);

            CooperativeLevelFlags excl = ownMouse ? CooperativeLevelFlags.Exclusive : CooperativeLevelFlags.NonExclusive;
            CooperativeLevelFlags background = (backgroundMouse && !ownMouse) ? CooperativeLevelFlags.Background : CooperativeLevelFlags.Foreground;

            // set cooperation level
            mouseDevice.SetCooperativeLevel(control.FindForm(), excl | background);

			// note: dont acquire yet, wait till capture
		}

		/// <summary>
		/// 
		/// </summary>
		private void InitializeBufferedMouse() {
			// create the device
			mouseDevice = new DInput.Device(SystemGuid.Mouse);

			mouseDevice.Properties.AxisModeAbsolute = true;

			// set the device format so DInput knows this device is a mouse
			mouseDevice.SetDataFormat(DeviceDataFormat.Mouse);

			// set the buffer size to use for input
			mouseDevice.Properties.BufferSize = BufferSize;

            CooperativeLevelFlags excl = ownMouse ? CooperativeLevelFlags.Exclusive : CooperativeLevelFlags.NonExclusive;
            CooperativeLevelFlags background = (backgroundMouse && !ownMouse) ? CooperativeLevelFlags.Background : CooperativeLevelFlags.Foreground;

            // set cooperation level
            mouseDevice.SetCooperativeLevel(control.FindForm(), excl | background);

			// note: dont acquire yet, wait till capture?
			//try {
			//    mouseDevice.Acquire();
			//} catch {
			//    throw new Exception("Unable to acquire a mouse using DirectInput.");
			//}
		}

		/// <summary>
		///		Reads buffered input data when in buffered mode.
		/// </summary>
		private void ReadBufferedKeyboardData() {
			// grab the collection of buffered data
			BufferedDataCollection bufferedData = keyboardDevice.GetBufferedData();

			// please tell me why this would ever come back null, rather than an empty collection...
			if (bufferedData == null)
				return;
            if (bufferedData.Count >= BufferSize - 1)
                log.Warn("Exceeded keyboard buffer.  Input data lost");

			for (int i = 0; i < bufferedData.Count; i++) {
				BufferedData data = bufferedData[i];

				KeyCodes key = ConvertKeyEnum((DInput.Key)data.Offset);

				// is the key being pressed down, or released?
				bool down = (data.ButtonPressedData == 1);

				KeyChanged(key, down);
			}
		}


		/// <summary>
		///		Reads buffered input data when in buffered mode.
		/// </summary>
		private void ReadMouseEvent(List<BufferedData> bufferedDataList,
                                    MouseData mouseData) {
			foreach (BufferedData data in bufferedDataList) {
                mouseData.timeStamp = data.TimeStamp;
				switch ((DInput.MouseOffset)data.Offset) {
					case MouseOffset.X:
						mouseData.relativeX = data.Data;
						break;
					case MouseOffset.Y:
						mouseData.relativeY = data.Data;
                        break;
					case MouseOffset.Z:
						mouseData.relativeZ = data.Data;
						break;
					case MouseOffset.Button0:
						mouseData.button = Axiom.Input.MouseButtons.Left;
						mouseData.down = (data.ButtonPressedData == 1);
						break;
					case MouseOffset.Button1:
						mouseData.button = Axiom.Input.MouseButtons.Right;
						mouseData.down = (data.ButtonPressedData == 1);
						break;
					case MouseOffset.Button2:
						mouseData.button = Axiom.Input.MouseButtons.Middle;
						mouseData.down = (data.ButtonPressedData == 1);
						break;
				}
			}
			return;
		}
		/// <summary>
		///		Reads buffered input data when in buffered mode.
		/// </summary>
		private void ReadBufferedMouseData() {
			// grab the collection of buffered data
			BufferedDataCollection bufferedData = mouseDevice.GetBufferedData();
			if (bufferedData == null)
				return;
            if (bufferedData.Count >= BufferSize - 1)
                log.Warn("Exceeded mouse buffer.  Input data lost");
            bufferedData.SortBySequence();
            Dictionary<int, List<BufferedData>> bufferedDataDict = new Dictionary<int, List<BufferedData>>();
            List<int> sequenceNumbers = new List<int>();
            int currentSequence = -1;
            List<BufferedData> currentData = null;
            foreach (BufferedData data in bufferedData) {
                if (currentSequence != data.Sequence) {
                    currentData = new List<BufferedData>();
                    currentSequence = data.Sequence;
                    bufferedDataDict[currentSequence] = currentData;
                    sequenceNumbers.Add(currentSequence);
                }
                currentData.Add(data);
            }
            foreach (int eventSequence in sequenceNumbers) {
				MouseData mouseData = new MouseData();
				// Grab all the events with that sequence number, and use them to
				// populate the mouseData and down structures.
				ReadMouseEvent(bufferedDataDict[eventSequence], mouseData);

                //if (control != null && !control.Capture)
                //    return;
                Point mousePoint = new Point(0, 0);
                if (!hardwareCursor) {
                    // If we are using a software cursor, use the relative
                    // data from the event and our previous position to 
                    // compute the current position.
                    mousePoint.X = mouseAbsX + (int)mouseData.relativeX;
                    mousePoint.Y = mouseAbsY + (int)mouseData.relativeY;
                    HandleMouseMoved(mousePoint);
                }
				mouseAbsZ += (int)mouseData.relativeZ;
				mouseData.x = mouseAbsX;
				mouseData.y = mouseAbsY;
				mouseData.z = mouseAbsZ;

				MouseChanged(mouseData);
			}
		}

        /// <summary>
        ///   If the cursor is enabled, set our mouseAbs variables based
        ///   on the data in mousePoint.  If the cursor is disabled and
        ///   we are using the hardware cursor, move the cursor back to the
        ///   point where the cursor was disabled.
        /// </summary>
        /// <param name="mousePoint"></param>
        public void HandleMouseMoved(Point mousePoint) {
            // If we are using a hardware cursor, ignore the relative
            // data from the event, and regenerate the relative data 
            // based on the current position and our previous position.
            if (this.CursorEnabled) {
                if (mouseAbsX != mousePoint.X || mouseAbsY != mousePoint.Y) {
                    // Inject the mouse event that will go beyond our buffered data
                    // to adjust the cursor to its current position.
                    MouseData mouseData = new MouseData();
                    mouseData.x = mousePoint.X;
                    mouseData.y = mousePoint.Y;
                    mouseData.relativeX = mousePoint.X - mouseAbsX;
                    mouseData.relativeY = mousePoint.Y - mouseAbsY;
                    Axiom.Input.MouseEventArgs e =
                        new Axiom.Input.MouseEventArgs(mouseData, modifiers);
                    OnMouseMoved(e);
                }
                mouseAbsX = mousePoint.X;
                mouseAbsY = mousePoint.Y;
                // Now that the cursor size has been scaled to 32x32 in full 
                // screen mode, I don't ever expect to not be able to rely on 
                // the operating system to update the cursor.  Nonetheless, 
                // it's possible that there are still cases out there.  Until 
                // we encounter one of those cases though, I'm leaving this 
                // ifdeffed out because I hate to waste the work updating the
                // cursor if I don't need to.
#if UPDATE_CURSOR_MANUALLY
                // If we can't use the operating system to update our cursor, we will
                // need to do it ourselves.
                if (hardwareCursor) {
                    Point screenPoint = control.PointToScreen(mousePoint);
                    Root.Instance.RenderSystem.SetCursorPosition(screenPoint.X, screenPoint.Y);
                }
#endif
            } else {
                if (HasFocus())
                {
                    if (hardwareCursor && control != null && control.Capture)
                    {
                        log.DebugFormat("Restoring cursor to: {0}", cursorDisabledPosition);
                        Cursor.Position = control.PointToScreen(cursorDisabledPosition);
                    }
                    else if (hardwareCursor && control != null)
                    {
                        // Our app has focus, but doesn't have mouse capture (and should).. grab it.
                        control.Capture = true;
                        log.DebugFormat("Restoring cursor (and recapturing) to: {0}", cursorDisabledPosition);
                        Cursor.Position = control.PointToScreen(cursorDisabledPosition);
                    }
                    else
                    {
                        log.DebugFormat("Cannot restore cursor; control {0}; {1}", control, (control == null) ? false : control.Capture);
                    }
                }
            }
        }

		/// <summary>
		///		Captures an immediate keyboard state snapshot (for non-buffered data).
		/// </summary>
		private void CaptureKeyboard() {
            if (useKeyboardEvents)
                // throw away the collection of buffered data
                keyboardDevice.GetBufferedData();

			keyboardState = keyboardDevice.GetCurrentKeyboardState();
            // Set Alt/Ctrl/Shift
            modifiers &= ~ModifierKeys.Alt;
            modifiers &= ~ModifierKeys.Control;
            modifiers &= ~ModifierKeys.Shift;
            if (keyboardState[Key.LeftAlt] || keyboardState[Key.RightAlt])
                modifiers |= ModifierKeys.Alt;
            if (keyboardState[Key.LeftControl] || keyboardState[Key.RightControl])
                modifiers |= ModifierKeys.Control;
            if (keyboardState[Key.LeftShift] || keyboardState[Key.RightShift])
                modifiers |= ModifierKeys.Shift;
        }

		/// <summary>
		///		Captures the immediate mouse state (for non-buffered data).
		/// </summary>
		private void CaptureMouse() {
			CaptureImmediateMouse();
#if NOT
            // This code should be in place if we really want to track mouse
            // buttons in the modifiers structure, but our base class doesn't
            // ever put these in the modifiers (despite their presence in the 
            // enumeration).

            // Set MouseButton flags
            modifiers &= ~ModifierKeys.MouseButton0;
            modifiers &= ~ModifierKeys.MouseButton1;
            modifiers &= ~ModifierKeys.MouseButton2;
            if (IsMousePressed(MouseButtons.Left))
                modifiers |= ModifierKeys.MouseButton0;
            if (IsMousePressed(MouseButtons.Right))
                modifiers |= ModifierKeys.MouseButton1;
            if (IsMousePressed(MouseButtons.Middle))
                modifiers |= ModifierKeys.MouseButton2;
#endif

		}

		/// <summary>
		///		Takes a snapshot of the mouse state for immediate input checking.
		/// </summary>
		private void CaptureImmediateMouse() {
            // throw away the collection of buffered data
            if (useMouseEvents)
                mouseDevice.GetBufferedData();

			// capture the current mouse state
			mouseState = mouseDevice.CurrentMouseState;

			// store the updated absolute values
			mouseAbsX += mouseState.X;
			mouseAbsY += mouseState.Y;
			mouseAbsZ += mouseState.Z;

			// calc relative deviance from center
			mouseRelX = mouseState.X;
			mouseRelY = mouseState.Y; 
			mouseRelZ = mouseState.Z; 

			byte[] buttons = mouseState.GetMouseButtons();

			// clear the flags
			mouseButtons = 0;

			for(int i = 0; i < buttons.Length; i++) {
				if((buttons[i] & 0x80) != 0) {
					mouseButtons |= (1 << i);
				}
			}
		}

		/// <summary>
		///		Verifies the state of the host window and reacquires input if the window was
		///		previously minimized and has been brought back into focus.
		/// </summary>
		/// <returns>True if the input devices are acquired and input capturing can proceed, false otherwise.</returns>
		protected bool VerifyInputAcquired() {
			// if the window is coming back from being deactivated, lets grab input again
			if (window.IsActive && !lastWindowActive) {
                try {
                    // acquire and capture keyboard input
                    if (useKeyboard) {
                        keyboardDevice.Acquire();
                        CaptureKeyboard();
                    }

                    // acquire and capture mouse input
                    if (useMouse) {
                        mouseDevice.Acquire();
                        CaptureMouse();
                    }

                    lastWindowActive = true;
                } catch (Microsoft.DirectX.DirectInput.OtherApplicationHasPriorityException) {
                    // we probably weren't able to capture the input - 
                    // this is ok since we will try again later, but
                    // don't mark us as the last window active so that
                    // we will try to acquire again later
                    ;
                }
            }

            return lastWindowActive;
		}

		#region Keycode Conversions

		/// <summary>
		///		Used to convert an Axiom.Input.KeyCodes enum val to a DirectInput.Key enum val.
		/// </summary>
		/// <param name="key">Axiom keyboard code to query.</param>
		/// <returns>The equivalent enum value in the DInput.Key enum.</returns>
		private DInput.Key ConvertKeyEnum(KeyCodes key) {
			// TODO: Quotes
			DInput.Key dinputKey = 0;

			switch(key) {
				case KeyCodes.A:
					dinputKey = DInput.Key.A;
					break;
				case KeyCodes.B:
					dinputKey = DInput.Key.B;
					break;
				case KeyCodes.C:
					dinputKey = DInput.Key.C;
					break;
				case KeyCodes.D:
					dinputKey = DInput.Key.D;
					break;
				case KeyCodes.E:
					dinputKey = DInput.Key.E;
					break;
				case KeyCodes.F:
					dinputKey = DInput.Key.F;
					break;
				case KeyCodes.G:
					dinputKey = DInput.Key.G;
					break;
				case KeyCodes.H:
					dinputKey = DInput.Key.H;
					break;
				case KeyCodes.I:
					dinputKey = DInput.Key.I;
					break;
				case KeyCodes.J:
					dinputKey = DInput.Key.J;
					break;
				case KeyCodes.K:
					dinputKey = DInput.Key.K;
					break;
				case KeyCodes.L:
					dinputKey = DInput.Key.L;
					break;
				case KeyCodes.M:
					dinputKey = DInput.Key.M;
					break;
				case KeyCodes.N:
					dinputKey = DInput.Key.N;
					break;
				case KeyCodes.O:
					dinputKey = DInput.Key.O;
					break;
				case KeyCodes.P:
					dinputKey = DInput.Key.P;
					break;
				case KeyCodes.Q:
					dinputKey = DInput.Key.Q;
					break;
				case KeyCodes.R:
					dinputKey = DInput.Key.R;
					break;
				case KeyCodes.S:
					dinputKey = DInput.Key.S;
					break;
				case KeyCodes.T:
					dinputKey = DInput.Key.T;
					break;
				case KeyCodes.U:
					dinputKey = DInput.Key.U;
					break;
				case KeyCodes.V:
					dinputKey = DInput.Key.V;
					break;
				case KeyCodes.W:
					dinputKey = DInput.Key.W;
					break;
				case KeyCodes.X:
					dinputKey = DInput.Key.X;
					break;
				case KeyCodes.Y:
					dinputKey = DInput.Key.Y;
					break;
				case KeyCodes.Z:
					dinputKey = DInput.Key.Z;
					break;
				case KeyCodes.Left :
					dinputKey = DInput.Key.LeftArrow;
					break;
				case KeyCodes.Right:
					dinputKey = DInput.Key.RightArrow;
					break;
				case KeyCodes.Up:
					dinputKey = DInput.Key.UpArrow;
					break;
				case KeyCodes.Down:
					dinputKey = DInput.Key.DownArrow;
					break;
				case KeyCodes.Escape:
					dinputKey = DInput.Key.Escape;
					break;
				case KeyCodes.F1:
					dinputKey = DInput.Key.F1;
					break;
				case KeyCodes.F2:
					dinputKey = DInput.Key.F2;
					break;
				case KeyCodes.F3:
					dinputKey = DInput.Key.F3;
					break;
				case KeyCodes.F4:
					dinputKey = DInput.Key.F4;
					break;
				case KeyCodes.F5:
					dinputKey = DInput.Key.F5;
					break;
				case KeyCodes.F6:
					dinputKey = DInput.Key.F6;
					break;
				case KeyCodes.F7:
					dinputKey = DInput.Key.F7;
					break;
				case KeyCodes.F8:
					dinputKey = DInput.Key.F8;
					break;
				case KeyCodes.F9:
					dinputKey = DInput.Key.F9;
					break;
				case KeyCodes.F10:
					dinputKey = DInput.Key.F10;
					break;
				case KeyCodes.D0:
					dinputKey = DInput.Key.D0;
                    break;
                case KeyCodes.D1:
                    dinputKey = DInput.Key.D1;
                    break;
                case KeyCodes.D2:
                    dinputKey = DInput.Key.D2;
                    break;
                case KeyCodes.D3:
                    dinputKey = DInput.Key.D3;
                    break;
                case KeyCodes.D4:
                    dinputKey = DInput.Key.D4;
                    break;
                case KeyCodes.D5:
                    dinputKey = DInput.Key.D5;
                    break;
                case KeyCodes.D6:
                    dinputKey = DInput.Key.D6;
                    break;
                case KeyCodes.D7:
                    dinputKey = DInput.Key.D7;
                    break;
                case KeyCodes.D8:
                    dinputKey = DInput.Key.D8;
                    break;
                case KeyCodes.D9:
                    dinputKey = DInput.Key.D9;
                    break;
				case KeyCodes.F11:
					dinputKey = DInput.Key.F11;
					break;
				case KeyCodes.F12:
					dinputKey = DInput.Key.F12;
					break;
				case KeyCodes.Enter:
					dinputKey = DInput.Key.Return;
					break;
				case KeyCodes.Tab:
					dinputKey = DInput.Key.Tab;
					break;
				case KeyCodes.LeftShift:
					dinputKey = DInput.Key.LeftShift;
					break;
				case KeyCodes.RightShift:
					dinputKey = DInput.Key.RightShift;
					break;
				case KeyCodes.LeftControl:
					dinputKey = DInput.Key.LeftControl;
					break;
				case KeyCodes.RightControl:
					dinputKey = DInput.Key.RightControl;
					break;
				case KeyCodes.Period:
					dinputKey = DInput.Key.Period;
					break;
				case KeyCodes.Comma:
					dinputKey = DInput.Key.Comma;
					break;
				case KeyCodes.Home:
					dinputKey = DInput.Key.Home;
					break;
				case KeyCodes.PageUp:
					dinputKey = DInput.Key.PageUp;
					break;
				case KeyCodes.PageDown:
					dinputKey = DInput.Key.PageDown;
					break;
				case KeyCodes.End:
					dinputKey = DInput.Key.End;
					break;
				case KeyCodes.Semicolon:
					dinputKey = DInput.Key.SemiColon;
					break;
				case KeyCodes.Subtract:
					dinputKey = DInput.Key.Subtract;
					break;
				case KeyCodes.Add:
					dinputKey = DInput.Key.Add;
					break;
				case KeyCodes.Backspace:
					dinputKey = DInput.Key.BackSpace;
					break;
				case KeyCodes.Delete:
					dinputKey = DInput.Key.Delete;
					break;
				case KeyCodes.Insert:
					dinputKey = DInput.Key.Insert;
					break;
				case KeyCodes.LeftAlt:
					dinputKey = DInput.Key.LeftAlt;
					break;
				case KeyCodes.RightAlt:
					dinputKey = DInput.Key.RightAlt;
					break;
				case KeyCodes.Space:
					dinputKey = DInput.Key.Space;
					break;
				case KeyCodes.Tilde:
					dinputKey = DInput.Key.Grave;
					break;
				case KeyCodes.OpenBracket:
					dinputKey = DInput.Key.LeftBracket;
					break;
				case KeyCodes.CloseBracket:
					dinputKey = DInput.Key.RightBracket;
					break;
				case KeyCodes.Plus:
					dinputKey = DInput.Key.Equals;
					break;
				case KeyCodes.QuestionMark:
					dinputKey = DInput.Key.Slash;
					break;
				case KeyCodes.Quotes:
					dinputKey = DInput.Key.Apostrophe;
					break;
				case KeyCodes.Backslash:
					dinputKey = DInput.Key.BackSlash;
					break;
				case KeyCodes.NumPad0:
					dinputKey = DInput.Key.NumPad0;
					break;
				case KeyCodes.NumPad1:
					dinputKey = DInput.Key.NumPad1;
					break;
				case KeyCodes.NumPad2:
					dinputKey = DInput.Key.NumPad2;
					break;
				case KeyCodes.NumPad3:
					dinputKey = DInput.Key.NumPad3;
					break;
				case KeyCodes.NumPad4:
					dinputKey = DInput.Key.NumPad4;
					break;
				case KeyCodes.NumPad5:
					dinputKey = DInput.Key.NumPad5;
					break;
				case KeyCodes.NumPad6:
					dinputKey = DInput.Key.NumPad6;
					break;
				case KeyCodes.NumPad7:
					dinputKey = DInput.Key.NumPad7;
					break;
				case KeyCodes.NumPad8:
					dinputKey = DInput.Key.NumPad8;
					break;
				case KeyCodes.NumPad9:
					dinputKey = DInput.Key.NumPad9;
					break;
				case KeyCodes.NumLock:
					dinputKey = DInput.Key.Numlock;
					break;
				case KeyCodes.PrintScreen:
					dinputKey = DInput.Key.SysRq;
					break;
			}

			return dinputKey;
		}

		/// <summary>
		///		Used to convert a DirectInput.Key enum val to a Axiom.Input.KeyCodes enum val.
		/// </summary>
		/// <param name="key">DirectInput.Key code to query.</param>
		/// <returns>The equivalent enum value in the Axiom.KeyCodes enum.</returns>
		private Axiom.Input.KeyCodes ConvertKeyEnum(DInput.Key key) {
			// TODO: Quotes
			Axiom.Input.KeyCodes axiomKey = 0;

			switch(key) {
                case DInput.Key.SysRq:
                    axiomKey = Axiom.Input.KeyCodes.PrintScreen;
                    break;
				case DInput.Key.A:
					axiomKey = Axiom.Input.KeyCodes.A;
					break;
				case DInput.Key.B:
					axiomKey = Axiom.Input.KeyCodes.B;
					break;
				case DInput.Key.C:
					axiomKey = Axiom.Input.KeyCodes.C;
					break;
				case DInput.Key.D:
					axiomKey = Axiom.Input.KeyCodes.D;
					break;
				case DInput.Key.E:
					axiomKey = Axiom.Input.KeyCodes.E;
					break;
				case DInput.Key.F:
					axiomKey = Axiom.Input.KeyCodes.F;
					break;
				case DInput.Key.G:
					axiomKey = Axiom.Input.KeyCodes.G;
					break;
				case DInput.Key.H:
					axiomKey = Axiom.Input.KeyCodes.H;
					break;
				case DInput.Key.I:
					axiomKey = Axiom.Input.KeyCodes.I;
					break;
				case DInput.Key.J:
					axiomKey = Axiom.Input.KeyCodes.J;
					break;
				case DInput.Key.K:
					axiomKey = Axiom.Input.KeyCodes.K;
					break;
				case DInput.Key.L:
					axiomKey = Axiom.Input.KeyCodes.L;
					break;
				case DInput.Key.M:
					axiomKey = Axiom.Input.KeyCodes.M;
					break;
				case DInput.Key.N:
					axiomKey = Axiom.Input.KeyCodes.N;
					break;
				case DInput.Key.O:
					axiomKey = Axiom.Input.KeyCodes.O;
					break;
				case DInput.Key.P:
					axiomKey = Axiom.Input.KeyCodes.P;
					break;
				case DInput.Key.Q:
					axiomKey = Axiom.Input.KeyCodes.Q;
					break;
				case DInput.Key.R:
					axiomKey = Axiom.Input.KeyCodes.R;
					break;
				case DInput.Key.S:
					axiomKey = Axiom.Input.KeyCodes.S;
					break;
				case DInput.Key.T:
					axiomKey = Axiom.Input.KeyCodes.T;
					break;
				case DInput.Key.U:
					axiomKey = Axiom.Input.KeyCodes.U;
					break;
				case DInput.Key.V:
					axiomKey = Axiom.Input.KeyCodes.V;
					break;
				case DInput.Key.W:
					axiomKey = Axiom.Input.KeyCodes.W;
					break;
				case DInput.Key.X:
					axiomKey = Axiom.Input.KeyCodes.X;
					break;
				case DInput.Key.Y:
					axiomKey = Axiom.Input.KeyCodes.Y;
					break;
				case DInput.Key.Z:
					axiomKey = Axiom.Input.KeyCodes.Z;
					break;
				case DInput.Key.LeftArrow :
					axiomKey = Axiom.Input.KeyCodes.Left;
					break;
				case DInput.Key.RightArrow:
					axiomKey = Axiom.Input.KeyCodes.Right;
					break;
				case DInput.Key.UpArrow:
					axiomKey = Axiom.Input.KeyCodes.Up;
					break;
				case DInput.Key.DownArrow:
					axiomKey = Axiom.Input.KeyCodes.Down;
					break;
				case DInput.Key.Escape:
					axiomKey = Axiom.Input.KeyCodes.Escape;
					break;
				case DInput.Key.F1:
					axiomKey = Axiom.Input.KeyCodes.F1;
					break;
				case DInput.Key.F2:
					axiomKey = Axiom.Input.KeyCodes.F2;
					break;
				case DInput.Key.F3:
					axiomKey = Axiom.Input.KeyCodes.F3;
					break;
				case DInput.Key.F4:
					axiomKey = Axiom.Input.KeyCodes.F4;
					break;
				case DInput.Key.F5:
					axiomKey = Axiom.Input.KeyCodes.F5;
					break;
				case DInput.Key.F6:
					axiomKey = Axiom.Input.KeyCodes.F6;
					break;
				case DInput.Key.F7:
					axiomKey = Axiom.Input.KeyCodes.F7;
					break;
				case DInput.Key.F8:
					axiomKey = Axiom.Input.KeyCodes.F8;
					break;
				case DInput.Key.F9:
					axiomKey = Axiom.Input.KeyCodes.F9;
					break;
				case DInput.Key.F10:
					axiomKey = Axiom.Input.KeyCodes.F10;
					break;
				case DInput.Key.D0:
					axiomKey = Axiom.Input.KeyCodes.D0;
					break;
				case DInput.Key.D1:
					axiomKey = Axiom.Input.KeyCodes.D1;
					break;
				case DInput.Key.D2:
					axiomKey = Axiom.Input.KeyCodes.D2;
					break;
				case DInput.Key.D3:
					axiomKey = Axiom.Input.KeyCodes.D3;
					break;
				case DInput.Key.D4:
					axiomKey = Axiom.Input.KeyCodes.D4;
					break;
				case DInput.Key.D5:
					axiomKey = Axiom.Input.KeyCodes.D5;
					break;
				case DInput.Key.D6:
					axiomKey = Axiom.Input.KeyCodes.D6;
					break;
				case DInput.Key.D7:
					axiomKey = Axiom.Input.KeyCodes.D7;
					break;
				case DInput.Key.D8:
					axiomKey = Axiom.Input.KeyCodes.D8;
					break;
				case DInput.Key.D9:
					axiomKey = Axiom.Input.KeyCodes.D9;
					break;
				case DInput.Key.F11:
					axiomKey = Axiom.Input.KeyCodes.F11;
					break;
				case DInput.Key.F12:
					axiomKey = Axiom.Input.KeyCodes.F12;
					break;
				case DInput.Key.Return:
					axiomKey = Axiom.Input.KeyCodes.Enter;
					break;
				case DInput.Key.Tab:
					axiomKey = Axiom.Input.KeyCodes.Tab;
					break;
				case DInput.Key.LeftShift:
					axiomKey = Axiom.Input.KeyCodes.LeftShift;
					break;
				case DInput.Key.RightShift:
					axiomKey = Axiom.Input.KeyCodes.RightShift;
					break;
				case DInput.Key.LeftControl:
					axiomKey = Axiom.Input.KeyCodes.LeftControl;
					break;
				case DInput.Key.RightControl:
					axiomKey = Axiom.Input.KeyCodes.RightControl;
					break;
				case DInput.Key.Period:
					axiomKey = Axiom.Input.KeyCodes.Period;
					break;
				case DInput.Key.Comma:
					axiomKey = Axiom.Input.KeyCodes.Comma;
					break;
				case DInput.Key.Home:
					axiomKey = Axiom.Input.KeyCodes.Home;
					break;
				case DInput.Key.PageUp:
					axiomKey = Axiom.Input.KeyCodes.PageUp;
					break;
				case DInput.Key.PageDown:
					axiomKey = Axiom.Input.KeyCodes.PageDown;
					break;
				case DInput.Key.End:
					axiomKey = Axiom.Input.KeyCodes.End;
					break;
				case DInput.Key.SemiColon:
					axiomKey = Axiom.Input.KeyCodes.Semicolon;
					break;
				case DInput.Key.Subtract:
					axiomKey = Axiom.Input.KeyCodes.Subtract;
					break;
				case DInput.Key.Add:
					axiomKey = Axiom.Input.KeyCodes.Add;
					break;
				case DInput.Key.BackSpace:
					axiomKey = Axiom.Input.KeyCodes.Backspace;
					break;
				case DInput.Key.Delete:
					axiomKey = Axiom.Input.KeyCodes.Delete;
					break;
				case DInput.Key.Insert:
					axiomKey = Axiom.Input.KeyCodes.Insert;
					break;
				case DInput.Key.LeftAlt:
					axiomKey = Axiom.Input.KeyCodes.LeftAlt;
					break;
				case DInput.Key.RightAlt:
					axiomKey = Axiom.Input.KeyCodes.RightAlt;
					break;
				case DInput.Key.Space:
					axiomKey = Axiom.Input.KeyCodes.Space;
					break;
				case DInput.Key.Grave:
					axiomKey = Axiom.Input.KeyCodes.Tilde;
					break;
				case DInput.Key.LeftBracket:
					axiomKey = Axiom.Input.KeyCodes.OpenBracket;
					break;
				case DInput.Key.RightBracket:
					axiomKey = Axiom.Input.KeyCodes.CloseBracket;
					break;
				case DInput.Key.Equals:
					axiomKey = KeyCodes.Plus;
					break;
				case DInput.Key.Minus:
					axiomKey = KeyCodes.Subtract;
					break;
				case DInput.Key.Slash:
					axiomKey = KeyCodes.QuestionMark;
					break;
				case DInput.Key.Apostrophe:
					axiomKey = KeyCodes.Quotes;
					break;
				case DInput.Key.BackSlash:
					axiomKey = KeyCodes.Backslash;
					break;
				case DInput.Key.NumPad0:
					axiomKey = Axiom.Input.KeyCodes.NumPad0;
					break;
				case DInput.Key.NumPad1:
					axiomKey = Axiom.Input.KeyCodes.NumPad1;
					break;
				case DInput.Key.NumPad2:
					axiomKey = Axiom.Input.KeyCodes.NumPad2;
					break;
				case DInput.Key.NumPad3:
					axiomKey = Axiom.Input.KeyCodes.NumPad3;
					break;
				case DInput.Key.NumPad4:
					axiomKey = Axiom.Input.KeyCodes.NumPad4;
					break;
				case DInput.Key.NumPad5:
					axiomKey = Axiom.Input.KeyCodes.NumPad5;
					break;
				case DInput.Key.NumPad6:
					axiomKey = Axiom.Input.KeyCodes.NumPad6;
					break;
				case DInput.Key.NumPad7:
					axiomKey = Axiom.Input.KeyCodes.NumPad7;
					break;
				case DInput.Key.NumPad8:
					axiomKey = Axiom.Input.KeyCodes.NumPad8;
					break;
				case DInput.Key.NumPad9:
					axiomKey = Axiom.Input.KeyCodes.NumPad9;
					break;
				case DInput.Key.Numlock:
					axiomKey = Axiom.Input.KeyCodes.NumLock;
					break;
			}

			return axiomKey;
		}

		#endregion Keycode Conversions

		#endregion Helper Methods
	}
}