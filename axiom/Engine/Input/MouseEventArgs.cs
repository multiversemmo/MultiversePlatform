using System;

namespace Axiom.Input {

	public class MouseData {
		/// <summary>
		///		X coordinate of the mouse.
		/// </summary>
		public float x;

		/// <summary>
		///		Y coordinate of the mouse.
		/// </summary>
		public float y;

		/// <summary>
		///		Z coordinate of the mouse.
		/// </summary>
		public float z;
		
		/// <summary>
		///		Relative X coordinate of the mouse.
		/// </summary>
		public float relativeX;

		/// <summary>
		///		Relative Y coordinate of the mouse.
		/// </summary>
		public float relativeY;

		/// <summary>
		///		Relative Z coordinate of the mouse.
		/// </summary>
		public float relativeZ;

		/// <summary>
		///		Mouse button pressed during this event.
		/// </summary>
		public MouseButtons button;

		/// <summary>
		///   Indicates whether this was a mouse down event.  
		///   If the button is not set (MouseButtons.None), then the value 
		///   stored here is not meaningful.
		/// </summary>
		public bool down;

        /// <summary>
        ///   The timestamp of the event.
        /// </summary>
        public int timeStamp;

		public MouseData() {
			button = MouseButtons.None;
			down = false;
            timeStamp = 0;
			x = 0;
			y = 0;
			z = 0;
			relativeX = 0;
			relativeY = 0;
			relativeZ = 0;
		}
	}

	/// <summary>
	///		Events args for mouse input events.
	/// </summary>
	public class MouseEventArgs : InputEventArgs {
		#region Fields

        /// <summary>
        ///   Mouse data that was captured
        /// </summary>
        protected MouseData mouseData;

		#endregion Fields

		#region Constructors

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="button">Mouse button pressed.</param>
		/// <param name="modifiers">Any modifier keys that are down.</param>
		/// <param name="x">Mouse X position.</param>
		/// <param name="y">Mouse Y position.</param>
		/// <param name="z">Mouse Z position.</param>
		public MouseEventArgs(MouseButtons button, ModifierKeys modifiers, float x, float y, float z) 
			: this(button, modifiers, x, y, z, 0, 0, 0) {}

		/// <summary>
		///		Constructor.
		/// </summary>
		/// <param name="button">Mouse button pressed.</param>
		/// <param name="modifiers">Any modifier keys that are down.</param>
		/// <param name="x">Mouse X position.</param>
		/// <param name="y">Mouse Y position.</param>
		/// <param name="z">Mouse Z position.</param>
		/// <param name="relX">Relative mouse X position.</param>
		/// <param name="relY">Relative mouse Y position.</param>
		/// <param name="relZ">Relative mouse Z position.</param>
		public MouseEventArgs(MouseButtons button, ModifierKeys modifiers, float x, float y, float z, float relX, float relY, float relZ) 
            : base(modifiers) {
            this.mouseData = new MouseData();
			mouseData.button = button;
            mouseData.x = x;
            mouseData.y = y;
            mouseData.z = z;
            mouseData.relativeX = relX;
            mouseData.relativeY = relY;
            mouseData.relativeZ = relZ;
		}

        public MouseEventArgs(MouseData mouseData, ModifierKeys modifiers)
            : base(modifiers) {
            this.mouseData = mouseData;
        }

		#endregion Constructors

		#region Properties

		/// <summary>
		///		Mouse button pressed during this event.
		/// </summary>
		public MouseButtons Button {
			get {
				return mouseData.button;
			}
		}

		/// <summary>
		///		Mouse X coordinate.
		/// </summary>
		public float X {
			get {
                return mouseData.x;
			}
		}

		/// <summary>
		///		Mouse Y coordinate.
		/// </summary>
		public float Y {
			get {
                return mouseData.y;
			}
		}

		/// <summary>
		///		Mouse Z coordinate.
		/// </summary>
		public float Z {
			get {
                return mouseData.z;
			}
		}

		/// <summary>
		///		Relative mouse X coordinate.
		/// </summary>
		public float RelativeX {
			get {
                return mouseData.relativeX;
			}
		}

		/// <summary>
		///		Relative mouse Y coordinate.
		/// </summary>
		public float RelativeY {
			get {
                return mouseData.relativeY;
			}
		}

        /// <summary>
        ///		Relative mouse Z coordinate.
        /// </summary>
        public float RelativeZ {
            get {
                return mouseData.relativeZ;
            }
        }

        /// <summary>
        ///		Timestamp of the event.
        /// </summary>
        public int TimeStamp {
            get {
                return mouseData.timeStamp;
            }
        }

		#endregion Properties
	}
}
