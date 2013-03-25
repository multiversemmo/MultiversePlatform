using System;
using System.Runtime.InteropServices;
using Axiom.Core;
using Axiom.Input;
using Tao.Sdl;

namespace Axiom.Platforms.SDL {
	/// <summary>
	///		Platform management specialization for Microsoft Windows (r) platform.
	/// </summary>
	public class SdlInputReader : InputReader {
		#region Fields
		
		/// <summary>
		///		Is the SDL window currently visible? 
		/// </summary>
		protected bool isVisible;

		protected int mouseX, mouseY;
		protected int relMouseX, relMouseY, relMouseZ;
		protected byte mouseKeys;
		protected MouseButtons mouseButtons;
		protected byte[] keyboardState;

		/// <summary>
		///		Array to use for holding buffered event data.
		/// </summary>
		protected Sdl.SDL_Event[] events = new Sdl.SDL_Event[BufferSize];
		
		/// <summary>
		/// 
		/// </summary>
		protected const int WheelStep = 60;

		/// <summary>
		///  Size of the arrays used to hold buffered input data.
		/// </summary>
		protected const int BufferSize = 16;
		
		#endregion Fields
		
		#region Constructor
		
		/// <summary>
		///		Constructor.
		/// </summary>
		public SdlInputReader() {
			// start off assuming we are visible
			isVisible = true;
		}

		/// <summary>
		///		Destructor.
		/// </summary>
		~SdlInputReader() {
			// release input
			Sdl.SDL_WM_GrabInput(Sdl.SDL_GRAB_OFF);
			Sdl.SDL_ShowCursor(1);
		}
		
		#endregion Constructor
		
		#region InputReader Members

		#region Properties

		public override int AbsoluteMouseX {
			get {
				return mouseX;
			}
		}

		public override int AbsoluteMouseY {
			get {
				return mouseY;
			}
		}

		public override int AbsoluteMouseZ {
			get {
				return 0;
			}
		}

		public override int RelativeMouseX {
			get {
				return relMouseX;
			}
		}

		public override int RelativeMouseY {
			get {
				return relMouseY;
			}
		}

		public override int RelativeMouseZ {
			get {
				return relMouseZ;
			}
		}

		#endregion Properties
		
		#region Methods

		/// <summary>
		///		Capture the current state of SDL input.
		/// </summary>
		public override void Capture() {
			// if we aren't active, wait
			if(!isVisible) {
				Sdl.SDL_Event evt;
				
				while(Sdl.SDL_WaitEvent(out evt) != 0) {
					if(evt.type == Sdl.SDL_ACTIVEEVENT && evt.active.gain == 1) {
						break;
					}
				}
			}

			if(useKeyboardEvents) {
				ProcessBufferedKeyboard();
			}

			if(useMouseEvents) {
				ProcessBufferedMouse();
			}

			// gather input from the various devices
			Sdl.SDL_PumpEvents();

			if(!useKeyboardEvents) {
				int numKeys;
				keyboardState = Sdl.SDL_GetKeyState(out numKeys);
			}

			if(!useMouseEvents) {
				// TODO: Look into awkward mouse wheel behavior
				mouseKeys = 0;
				relMouseX = 0;
				relMouseY = 0;
				relMouseZ = 0;

				// get mouse info
				if((Sdl.SDL_GetAppState() & Sdl.SDL_APPMOUSEFOCUS) != 0) {
					mouseKeys = Sdl.SDL_GetMouseState(out mouseX, out mouseY);
					Sdl.SDL_GetRelativeMouseState(out relMouseX, out relMouseY);

					// the value that is added to mMouseRelativeZ when the wheel
					// is moved one step (this value is actually added
					// twice per movement since a wheel movement triggers a
					// MOUSEBUTTONUP and a MOUSEBUTTONDOWN event)

					// fetch all mouse related events
					// TODO: SDL_PeepEvents throwing exceptions, investigate
					int count = 0;
						//Sdl.SDL_PeepEvents(events, BufferSize, Sdl.SDL_GETEVENT, 
						//	Sdl.SDL_MOUSEMOTIONMASK | Sdl.SDL_MOUSEBUTTONDOWNMASK | Sdl.SDL_MOUSEBUTTONUPMASK);

					if(count > 0) {
						for(int i = 0; i < count; i++) {
							if(events[i].type == Sdl.SDL_MOUSEBUTTONDOWN || events[i].type == Sdl.SDL_MOUSEBUTTONUP) {
								if(events[i].button.button == Sdl.SDL_BUTTON_WHEELUP) {
									relMouseZ += WheelStep;
								}
								else if(events[i].button.button == Sdl.SDL_BUTTON_WHEELDOWN) {
									relMouseZ -= WheelStep;
								}
							}
						} // for
					} // if count...
				} // if mouse focus

				mouseButtons	 = (mouseKeys & Sdl.SDL_BUTTON_LMASK) != 0	? MouseButtons.Left : 0; // left
				mouseButtons	|= (mouseKeys & Sdl.SDL_BUTTON_RMASK) != 0	? MouseButtons.Right : 0; // right
				mouseButtons	|= (mouseKeys & Sdl.SDL_BUTTON_MMASK) != 0	? MouseButtons.Middle : 0; // middle
			} // if not using mouse events
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="useKeyboard"></param>
		/// <param name="useMouse"></param>
		/// <param name="useGamepad"></param>
		/// <param name="ownMouse"></param>
		public override void Initialize(Axiom.Graphics.RenderWindow parent, bool useKeyboard, bool useMouse, bool useGamepad, bool ownMouse) {
			if(useMouse && ownMouse) {
				// hide the cursor
				// TODO: Commented out for easier debugging
				//Sdl.SDL_ShowCursor(0);

				//Sdl.SDL_WM_GrabInput(Sdl.SDL_GRAB_ON);
			}

			// mouse starts out in the center of the window
			mouseX = (int)(parent.Width * 0.5f);
			mouseY = (int)(parent.Height * 0.5f);
		}

		/// <summary>
		///		Checks the current keyboard state to see if the specified key is pressed.
		/// </summary>
		/// <param name="key">KeyCode to check.</param>
		/// <returns>true if the key is down, false otherwise.</returns>
		public override bool IsKeyPressed(KeyCodes key) {
			int sdlKey = ConvertKeyEnum(key);

			return keyboardState[sdlKey] != 0;
		}

		public override bool IsMousePressed(MouseButtons button) {
			return (mouseButtons & button) != 0;
		}

		public override bool UseKeyboardEvents {
			get {
				return useKeyboardEvents;
			}
			set {
				useKeyboardEvents = value;
			}
		}

		public override bool UseMouseEvents {
			get {
				return useMouseEvents;
			}
			set {
				useMouseEvents = value;
			}
		}

		public override void Dispose() {

		}


		#endregion Methods
		
		#endregion InputReader Members

		#region Methods

		private void ProcessBufferedKeyboard() {
			int count = Sdl.SDL_PeepEvents(
				events, BufferSize, Sdl.SDL_GETEVENT, 
				(Sdl.SDL_KEYDOWNMASK | Sdl.SDL_KEYUPMASK));

			// no events to process
			if(count == 0) {
				return;
			}

			// fire an event for each key event
			for(int i = 0; i < count; i++) {
				bool down = (events[i].type == Sdl.SDL_KEYDOWN);

				Axiom.Input.KeyCodes keyCode = ConvertKeyEnum(events[i].key.keysym.sym);

				KeyChanged(keyCode, down);
			}
		}

		private void ProcessBufferedMouse() {
		}

		#region Keycode Conversions

		/// <summary>
		///		Used to convert an Axiom.Input.KeyCodes enum val to a Sdl enum val.
		/// </summary>
		/// <param name="key">Axiom keyboard code to query.</param>
		/// <returns>The equivalent enum value in the Sdl enum.</returns>
		private int ConvertKeyEnum(KeyCodes key) {
			// TODO: Quotes
			int sdlKey = 0;

			switch(key) {
				case KeyCodes.A:
					sdlKey = Sdl.SDLK_a;
					break;
				case KeyCodes.B:
					sdlKey = Sdl.SDLK_b;
					break;
				case KeyCodes.C:
					sdlKey = Sdl.SDLK_c;
					break;
				case KeyCodes.D:
					sdlKey = Sdl.SDLK_d;
					break;
				case KeyCodes.E:
					sdlKey = Sdl.SDLK_e;
					break;
				case KeyCodes.F:
					sdlKey = Sdl.SDLK_f;
					break;
				case KeyCodes.G:
					sdlKey = Sdl.SDLK_g;
					break;
				case KeyCodes.H:
					sdlKey = Sdl.SDLK_h;
					break;
				case KeyCodes.I:
					sdlKey = Sdl.SDLK_i;
					break;
				case KeyCodes.J:
					sdlKey = Sdl.SDLK_j;
					break;
				case KeyCodes.K:
					sdlKey = Sdl.SDLK_k;
					break;
				case KeyCodes.L:
					sdlKey = Sdl.SDLK_l;
					break;
				case KeyCodes.M:
					sdlKey = Sdl.SDLK_m;
					break;
				case KeyCodes.N:
					sdlKey = Sdl.SDLK_n;
					break;
				case KeyCodes.O:
					sdlKey = Sdl.SDLK_o;
					break;
				case KeyCodes.P:
					sdlKey = Sdl.SDLK_p;
					break;
				case KeyCodes.Q:
					sdlKey = Sdl.SDLK_q;
					break;
				case KeyCodes.R:
					sdlKey = Sdl.SDLK_r;
					break;
				case KeyCodes.S:
					sdlKey = Sdl.SDLK_s;
					break;
				case KeyCodes.T:
					sdlKey = Sdl.SDLK_t;
					break;
				case KeyCodes.U:
					sdlKey = Sdl.SDLK_u;
					break;
				case KeyCodes.V:
					sdlKey = Sdl.SDLK_v;
					break;
				case KeyCodes.W:
					sdlKey = Sdl.SDLK_w;
					break;
				case KeyCodes.X:
					sdlKey = Sdl.SDLK_x;
					break;
				case KeyCodes.Y:
					sdlKey = Sdl.SDLK_y;
					break;
				case KeyCodes.Z:
					sdlKey = Sdl.SDLK_z;
					break;
				case KeyCodes.Left :
					sdlKey = Sdl.SDLK_LEFT;
					break;
				case KeyCodes.Right:
					sdlKey = Sdl.SDLK_RIGHT;
					break;
				case KeyCodes.Up:
					sdlKey = Sdl.SDLK_UP;
					break;
				case KeyCodes.Down:
					sdlKey = Sdl.SDLK_DOWN;
					break;
				case KeyCodes.Escape:
					sdlKey = Sdl.SDLK_ESCAPE;
					break;
				case KeyCodes.F1:
					sdlKey = Sdl.SDLK_F1;
					break;
				case KeyCodes.F2:
					sdlKey = Sdl.SDLK_F2;
					break;
				case KeyCodes.F3:
					sdlKey = Sdl.SDLK_F3;
					break;
				case KeyCodes.F4:
					sdlKey = Sdl.SDLK_F4;
					break;
				case KeyCodes.F5:
					sdlKey = Sdl.SDLK_F5;
					break;
				case KeyCodes.F6:
					sdlKey = Sdl.SDLK_F6;
					break;
				case KeyCodes.F7:
					sdlKey = Sdl.SDLK_F7;
					break;
				case KeyCodes.F8:
					sdlKey = Sdl.SDLK_F8;
					break;
				case KeyCodes.F9:
					sdlKey = Sdl.SDLK_F9;
					break;
				case KeyCodes.F10:
					sdlKey = Sdl.SDLK_F10;
					break;
				case KeyCodes.D0:
					sdlKey = Sdl.SDLK_0;
					break;
				case KeyCodes.D1:
					sdlKey = Sdl.SDLK_1;
					break;
				case KeyCodes.D2:
					sdlKey = Sdl.SDLK_2;
					break;
				case KeyCodes.D3:
					sdlKey = Sdl.SDLK_3;
					break;
				case KeyCodes.D4:
					sdlKey = Sdl.SDLK_4;
					break;
				case KeyCodes.D5:
					sdlKey = Sdl.SDLK_5;
					break;
				case KeyCodes.D6:
					sdlKey = Sdl.SDLK_6;
					break;
				case KeyCodes.D7:
					sdlKey = Sdl.SDLK_7;
					break;
				case KeyCodes.D8:
					sdlKey = Sdl.SDLK_8;
					break;
				case KeyCodes.D9:
					sdlKey = Sdl.SDLK_9;
					break;
				case KeyCodes.F11:
					sdlKey = Sdl.SDLK_F11;
					break;
				case KeyCodes.F12:
					sdlKey = Sdl.SDLK_F12;
					break;
				case KeyCodes.Enter:
					sdlKey = Sdl.SDLK_RETURN;
					break;
				case KeyCodes.Tab:
					sdlKey = Sdl.SDLK_TAB;
					break;
				case KeyCodes.LeftShift:
					sdlKey = Sdl.SDLK_LSHIFT;
					break;
				case KeyCodes.RightShift:
					sdlKey = Sdl.SDLK_RSHIFT;
					break;
				case KeyCodes.LeftControl:
					sdlKey = Sdl.SDLK_LCTRL;
					break;
				case KeyCodes.RightControl:
					sdlKey = Sdl.SDLK_RCTRL;
					break;
				case KeyCodes.Period:
					sdlKey = Sdl.SDLK_PERIOD;
					break;
				case KeyCodes.Comma:
					sdlKey = Sdl.SDLK_COMMA;
					break;
				case KeyCodes.Home:
					sdlKey = Sdl.SDLK_HOME;
					break;
				case KeyCodes.PageUp:
					sdlKey = Sdl.SDLK_PAGEUP;
					break;
				case KeyCodes.PageDown:
					sdlKey = Sdl.SDLK_PAGEDOWN;
					break;
				case KeyCodes.End:
					sdlKey = Sdl.SDLK_END;
					break;
				case KeyCodes.Semicolon:
					sdlKey = Sdl.SDLK_SEMICOLON;
					break;
				case KeyCodes.Subtract:
					sdlKey = Sdl.SDLK_MINUS;
					break;
				case KeyCodes.Add:
					sdlKey = Sdl.SDLK_PLUS;
					break;
				case KeyCodes.Backspace:
					sdlKey = Sdl.SDLK_BACKSPACE;
					break;
				case KeyCodes.Delete:
					sdlKey = Sdl.SDLK_DELETE;
					break;
				case KeyCodes.Insert:
					sdlKey = Sdl.SDLK_INSERT;
					break;
				case KeyCodes.LeftAlt:
					sdlKey = Sdl.SDLK_LALT;
					break;
				case KeyCodes.RightAlt:
					sdlKey = Sdl.SDLK_RALT;
					break;
				case KeyCodes.Space:
					sdlKey = Sdl.SDLK_SPACE;
					break;
				case KeyCodes.Tilde:
					sdlKey = Sdl.SDLK_BACKQUOTE;
					break;
				case KeyCodes.OpenBracket:
					sdlKey = Sdl.SDLK_LEFTBRACKET;
					break;
				case KeyCodes.CloseBracket:
					sdlKey = Sdl.SDLK_RIGHTBRACKET;
					break;
				case KeyCodes.Plus:
					sdlKey = Sdl.SDLK_EQUALS;
					break;
				case KeyCodes.QuestionMark:
					sdlKey = Sdl.SDLK_SLASH;
					break;
				case KeyCodes.Quotes:
					sdlKey = Sdl.SDLK_QUOTE;
					break;
				case KeyCodes.Backslash:
					sdlKey = Sdl.SDLK_BACKSLASH;
					break;
			}

			return sdlKey;
		}

		/// <summary>
		///		Used to convert a Sdl enum val to a Axiom.Input.KeyCodes enum val.
		/// </summary>
		/// <param name="key">Sdl code to query.</param>
		/// <returns>The equivalent enum value in the Axiom.KeyCodes enum.</returns>
		private Axiom.Input.KeyCodes ConvertKeyEnum(int key) {
			// TODO: Quotes
			Axiom.Input.KeyCodes axiomKey = 0;

			switch(key) {
				case Sdl.SDLK_a:
					axiomKey = Axiom.Input.KeyCodes.A;
					break;
				case Sdl.SDLK_b:
					axiomKey = Axiom.Input.KeyCodes.B;
					break;
				case Sdl.SDLK_c:
					axiomKey = Axiom.Input.KeyCodes.C;
					break;
				case Sdl.SDLK_d:
					axiomKey = Axiom.Input.KeyCodes.D;
					break;
				case Sdl.SDLK_e:
					axiomKey = Axiom.Input.KeyCodes.E;
					break;
				case Sdl.SDLK_f:
					axiomKey = Axiom.Input.KeyCodes.F;
					break;
				case Sdl.SDLK_g:
					axiomKey = Axiom.Input.KeyCodes.G;
					break;
				case Sdl.SDLK_h:
					axiomKey = Axiom.Input.KeyCodes.H;
					break;
				case Sdl.SDLK_i:
					axiomKey = Axiom.Input.KeyCodes.I;
					break;
				case Sdl.SDLK_j:
					axiomKey = Axiom.Input.KeyCodes.J;
					break;
				case Sdl.SDLK_k:
					axiomKey = Axiom.Input.KeyCodes.K;
					break;
				case Sdl.SDLK_l:
					axiomKey = Axiom.Input.KeyCodes.L;
					break;
				case Sdl.SDLK_m:
					axiomKey = Axiom.Input.KeyCodes.M;
					break;
				case Sdl.SDLK_n:
					axiomKey = Axiom.Input.KeyCodes.N;
					break;
				case Sdl.SDLK_o:
					axiomKey = Axiom.Input.KeyCodes.O;
					break;
				case Sdl.SDLK_p:
					axiomKey = Axiom.Input.KeyCodes.P;
					break;
				case Sdl.SDLK_q:
					axiomKey = Axiom.Input.KeyCodes.Q;
					break;
				case Sdl.SDLK_r:
					axiomKey = Axiom.Input.KeyCodes.R;
					break;
				case Sdl.SDLK_s:
					axiomKey = Axiom.Input.KeyCodes.S;
					break;
				case Sdl.SDLK_t:
					axiomKey = Axiom.Input.KeyCodes.T;
					break;
				case Sdl.SDLK_u:
					axiomKey = Axiom.Input.KeyCodes.U;
					break;
				case Sdl.SDLK_v:
					axiomKey = Axiom.Input.KeyCodes.V;
					break;
				case Sdl.SDLK_w:
					axiomKey = Axiom.Input.KeyCodes.W;
					break;
				case Sdl.SDLK_x:
					axiomKey = Axiom.Input.KeyCodes.X;
					break;
				case Sdl.SDLK_y:
					axiomKey = Axiom.Input.KeyCodes.Y;
					break;
				case Sdl.SDLK_z:
					axiomKey = Axiom.Input.KeyCodes.Z;
					break;
				case Sdl.SDLK_LEFT:
					axiomKey = Axiom.Input.KeyCodes.Left;
					break;
				case Sdl.SDLK_RIGHT:
					axiomKey = Axiom.Input.KeyCodes.Right;
					break;
				case Sdl.SDLK_UP:
					axiomKey = Axiom.Input.KeyCodes.Up;
					break;
				case Sdl.SDLK_DOWN:
					axiomKey = Axiom.Input.KeyCodes.Down;
					break;
				case Sdl.SDLK_ESCAPE:
					axiomKey = Axiom.Input.KeyCodes.Escape;
					break;
				case Sdl.SDLK_F1:
					axiomKey = Axiom.Input.KeyCodes.F1;
					break;
				case Sdl.SDLK_F2:
					axiomKey = Axiom.Input.KeyCodes.F2;
					break;
				case Sdl.SDLK_F3:
					axiomKey = Axiom.Input.KeyCodes.F3;
					break;
				case Sdl.SDLK_F4:
					axiomKey = Axiom.Input.KeyCodes.F4;
					break;
				case Sdl.SDLK_F5:
					axiomKey = Axiom.Input.KeyCodes.F5;
					break;
				case Sdl.SDLK_F6:
					axiomKey = Axiom.Input.KeyCodes.F6;
					break;
				case Sdl.SDLK_F7:
					axiomKey = Axiom.Input.KeyCodes.F7;
					break;
				case Sdl.SDLK_F8:
					axiomKey = Axiom.Input.KeyCodes.F8;
					break;
				case Sdl.SDLK_F9:
					axiomKey = Axiom.Input.KeyCodes.F9;
					break;
				case Sdl.SDLK_F10:
					axiomKey = Axiom.Input.KeyCodes.F10;
					break;
				case Sdl.SDLK_0:
					axiomKey = Axiom.Input.KeyCodes.D0;
					break;
				case Sdl.SDLK_1:
					axiomKey = Axiom.Input.KeyCodes.D1;
					break;
				case Sdl.SDLK_2:
					axiomKey = Axiom.Input.KeyCodes.D2;
					break;
				case Sdl.SDLK_3:
					axiomKey = Axiom.Input.KeyCodes.D3;
					break;
				case Sdl.SDLK_4:
					axiomKey = Axiom.Input.KeyCodes.D4;
					break;
				case Sdl.SDLK_5:
					axiomKey = Axiom.Input.KeyCodes.D5;
					break;
				case Sdl.SDLK_6:
					axiomKey = Axiom.Input.KeyCodes.D6;
					break;
				case Sdl.SDLK_7:
					axiomKey = Axiom.Input.KeyCodes.D7;
					break;
				case Sdl.SDLK_8:
					axiomKey = Axiom.Input.KeyCodes.D8;
					break;
				case Sdl.SDLK_9:
					axiomKey = Axiom.Input.KeyCodes.D9;
					break;
				case Sdl.SDLK_F11:
					axiomKey = Axiom.Input.KeyCodes.F11;
					break;
				case Sdl.SDLK_F12:
					axiomKey = Axiom.Input.KeyCodes.F12;
					break;
				case Sdl.SDLK_RETURN:
					axiomKey = Axiom.Input.KeyCodes.Enter;
					break;
				case Sdl.SDLK_TAB:
					axiomKey = Axiom.Input.KeyCodes.Tab;
					break;
				case Sdl.SDLK_LSHIFT:
					axiomKey = Axiom.Input.KeyCodes.LeftShift;
					break;
				case Sdl.SDLK_RSHIFT:
					axiomKey = Axiom.Input.KeyCodes.RightShift;
					break;
				case Sdl.SDLK_LCTRL:
					axiomKey = Axiom.Input.KeyCodes.LeftControl;
					break;
				case Sdl.SDLK_RCTRL:
					axiomKey = Axiom.Input.KeyCodes.RightControl;
					break;
				case Sdl.SDLK_PERIOD:
					axiomKey = Axiom.Input.KeyCodes.Period;
					break;
				case Sdl.SDLK_COMMA:
					axiomKey = Axiom.Input.KeyCodes.Comma;
					break;
				case Sdl.SDLK_HOME:
					axiomKey = Axiom.Input.KeyCodes.Home;
					break;
				case Sdl.SDLK_PAGEUP:
					axiomKey = Axiom.Input.KeyCodes.PageUp;
					break;
				case Sdl.SDLK_PAGEDOWN:
					axiomKey = Axiom.Input.KeyCodes.PageDown;
					break;
				case Sdl.SDLK_END:
					axiomKey = Axiom.Input.KeyCodes.End;
					break;
				case Sdl.SDLK_SEMICOLON:
					axiomKey = Axiom.Input.KeyCodes.Semicolon;
					break;
				case Sdl.SDLK_MINUS:
					axiomKey = Axiom.Input.KeyCodes.Subtract;
					break;
				case Sdl.SDLK_PLUS:
					axiomKey = Axiom.Input.KeyCodes.Add;
					break;
				case Sdl.SDLK_BACKSPACE:
					axiomKey = Axiom.Input.KeyCodes.Backspace;
					break;
				case Sdl.SDLK_DELETE:
					axiomKey = Axiom.Input.KeyCodes.Delete;
					break;
				case Sdl.SDLK_INSERT:
					axiomKey = Axiom.Input.KeyCodes.Insert;
					break;
				case Sdl.SDLK_LALT:
					axiomKey = Axiom.Input.KeyCodes.LeftAlt;
					break;
				case Sdl.SDLK_RALT:
					axiomKey = Axiom.Input.KeyCodes.RightAlt;
					break;
				case Sdl.SDLK_SPACE:
					axiomKey = Axiom.Input.KeyCodes.Space;
					break;
				case Sdl.SDLK_BACKQUOTE:
					axiomKey = Axiom.Input.KeyCodes.Tilde;
					break;
				case Sdl.SDLK_LEFTBRACKET:
					axiomKey = Axiom.Input.KeyCodes.OpenBracket;
					break;
				case Sdl.SDLK_RIGHTBRACKET:
					axiomKey = Axiom.Input.KeyCodes.CloseBracket;
					break;
				case Sdl.SDLK_EQUALS:
					axiomKey = KeyCodes.Plus;
					break;
				case Sdl.SDLK_SLASH:
					axiomKey = KeyCodes.QuestionMark;
					break;
				case Sdl.SDLK_QUOTE:
					axiomKey = KeyCodes.Quotes;
					break;
				case Sdl.SDLK_BACKSLASH:
					axiomKey = KeyCodes.Backslash;
					break;
			}

			return axiomKey;
		}

		#endregion Keycode Conversions

		#endregion Methods
	}
}
