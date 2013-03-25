/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;

using log4net;

using Axiom.MathLib;
using Axiom.Core;
using Axiom.Input;
using Axiom.Graphics;
using Axiom.Utility;

using Multiverse.Base;
using Multiverse.Config;
using Multiverse.CollisionLib;
using Multiverse.Gui;
using Multiverse.Interface;

using ObjectNodeType = Multiverse.Network.ObjectNodeType;

using TimeTool = Multiverse.Utility.TimeTool;

#endregion

namespace Multiverse.Input
{
    public interface IInputHandler {
        // Grab and apply the mouse and keyboard input
        void OnFrameStarted(object source, FrameEventArgs e, long now);
        void Activate();
        void Deactivate();
        void Detach();

        InputReader Reader { get; }
    }
#if NOT
        // These are used by the bindings file
        void MoveForward(bool status);
        void MoveBackward(bool status);
        void MoveUp(bool status);
        void MoveDown(bool status);
        void TurnLeft(bool status);
        void TurnRight(bool status);
        void StrafeLeft(bool status);
        void StrafeRight(bool status);
        void ToggleAutorun();

        // Used by client API
        bool CameraGrabbed { get; set; }

        // Used by MarsDecal
        float PlayerYaw { get; }
#endif

    public abstract class BaseInputHandler : IInputHandler {
        #region Fields

        protected InputReader input;
        protected Client client;

        protected static TimingMeter captureMeter = MeterManager.GetMeter("Capture Input", "Input");

        #endregion


        public BaseInputHandler(Client client) {
            this.client = client;
            client.InputHandler = this;

            // retrieve and initialize the input system
            InputReader reader = PlatformManager.Instance.CreateInputReader();
            reader.Initialize(client.Window, true, true, false, 
                              !client.UseCooperativeInput, !client.UseCooperativeInput);

            reader.UseKeyboardEvents = true;
            reader.UseMouseEvents = true;
            
            Attach(reader);
        }

        public virtual void Activate() {
        }

        public virtual void Deactivate() {
        }
        
        public virtual void Attach(InputReader reader) {
            Detach();
            input = reader;
        }

        public virtual void Detach() {
            if (input != null) {
                input.Dispose();
                input = null;
            }
        }

        public virtual void OnFrameStarted(object source, FrameEventArgs e, long now) {
            captureMeter.Enter();
            // capture the current input state if we are the active window
            try {
                // This will call all the OnMouse and OnKey handlers.
                input.Capture();
            } catch (Exception) {
                // just ignore this, and skip our input handling
                // until we can get the input again
                captureMeter.Exit();
                return;
            }
            captureMeter.Exit();
        }

        /// <summary>
        ///   Check the state of the mouse buttons as of the last capture
        /// </summary>
        /// <returns>true if any of the three main mouse buttons are down</returns>
        public virtual bool IsMousePressed() {
            return input.IsMousePressed(MouseButtons.Left) ||
                   input.IsMousePressed(MouseButtons.Right) ||
                   input.IsMousePressed(MouseButtons.Middle);
        }

        /// <summary>
        ///   Check the state of the mouse buttons as of the last capture
        /// </summary>
        /// <returns>true if the given mouse button is down</returns>
        public virtual bool IsMousePressed(MouseButtons button) {
            return input.IsMousePressed(button);
        }

        public InputReader Reader {
            get {
                return input;
            }
        }
    }

    /// <summary>
    ///   This InputHandler extends BaseInputHandler to handle Gui events
    ///   and send these off.  This also will dispatch to objects with
    ///   mouse event handlers.
    /// </summary>
    public class GuiInputHandler : BaseInputHandler, IInputHandler {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(GuiInputHandler));

        protected static int ClickToleranceTime = 3000; // 3 seconds
        protected static int ClickToleranceSpace = 10; // 10 pixels
        protected static float TickInterval = 1.0f / 200.0f; // To set off a tick event every 1/5 of a second

        /// <summary>
        ///   Maintain a list of buttons that we think are depressed.
        /// </summary>
        protected List<MouseButtons> mouseButtonsDown = new List<MouseButtons>();
        // protected List<KeyCodes> keysDown = new List<KeyCodes>();

        protected MouseEventArgs mouseDownEvent;

        protected bool cursorEnabled = true;

        protected ObjectNode mouseoverObject;
        protected ObjectNode mouseDownObject;

        protected float mouseFrameRelX = 0.0f;
        protected float mouseFrameRelY = 0.0f;
        protected float mouseFrameRelZ = 0.0f;

        long timerTick = 0;

        public GuiInputHandler(Client client) : base(client) {
        }


        /// <summary>
        ///   Attach this input handler to an input reader
        /// </summary>
        /// <param name="reader"></param>
        public override void Attach(InputReader reader) {
            base.Attach(reader);
            if (input != null) {
                // add the KeyUp and KeyDown callbacks
                input.KeyDown += input_KeyDown;
                input.KeyUp += input_KeyUp;
                // handle keyboard lost as well

                // add the MouseUp, MouseDown and MouseMoved callbacks
                input.MouseDown += input_MouseDown;
                input.MouseUp += input_MouseUp;
                input.MouseMoved += input_MouseMoved;
                input.MouseLost += input_MouseLost;
            }
        }

        /// <summary>
        ///   Detach this input handler from the input reader
        /// </summary>
        public override void Detach() {
            if (input != null) {
                // remove the KeyUp and KeyDown callbacks
                input.KeyDown -= input_KeyDown;
                input.KeyUp -= input_KeyUp;
                // handle keyboard lost as well

                // remove the MouseUp, MouseDown and MouseMoved callbacks
                input.MouseDown -= input_MouseDown;
                input.MouseUp -= input_MouseUp;
                input.MouseMoved -= input_MouseMoved;
                input.MouseLost -= input_MouseLost;
            }
            base.Detach();
        }

        public override void OnFrameStarted(object source, FrameEventArgs e, long now) {
            long tick = TimeTool.CurrentTime;
            long tmp = (int)(tick * TickInterval);
            if (timerTick != tmp) {
                timerTick = tmp;
                this.OnTick(source, TimeTool.CurrentTime);
            }
            base.OnFrameStarted(source, e, now);
            
            if (UiSystem.MouseDirty)
                UiSystem.UpdateMouseOver();

            // Update any GuiSystem timers (for key repeat)
            GuiSystem.Instance.OnUpdate(TimeTool.CurrentTime);
        }
        
        public void OnTick(object source, long time) {
            PointF mousePos = GuiSystem.Instance.MousePosition;

            UiSystem.InjectTick();
            UiSystem.UpdateMouseOver();

            if (!cursorEnabled)
                return;

            // If the mouse is over a UI widget, don't do the mouseover or mouse click
            // events for the world objects.
            if (client.GuiHasFocus)
                return;

            // At some point, I need this cast ray logic to have 
            // a concept of only hostile or only friendly or not 
            // player or stuff like that.  For now, I just set this
            // up to not include the player object.
            ObjectNode target = client.CastRay(mousePos.X / client.Viewport.ActualWidth,
                                               mousePos.Y / client.Viewport.ActualHeight);

            // MouseExit/MouseEnter events
            if (mouseoverObject != target) {
                if (mouseoverObject != null)
                    mouseoverObject.OnMouseExit(null);
                mouseoverObject = target;
                if (mouseoverObject != null)
                    mouseoverObject.OnMouseEnter(null);
            }
        }

        /// <summary>
        ///   Check the state of the mouse buttons as of the last capture
        /// </summary>
        /// <returns>true if any of the three main mouse buttons are down</returns>
        public override bool IsMousePressed()
        {
            return mouseButtonsDown.Contains(MouseButtons.Left) ||
                   mouseButtonsDown.Contains(MouseButtons.Right) ||
                   mouseButtonsDown.Contains(MouseButtons.Middle);
        }

        /// <summary>
        ///   Check the state of the mouse buttons as of the last capture
        /// </summary>
        /// <returns>true if the given mouse button is down</returns>
        public override bool IsMousePressed(MouseButtons button)
        {
            return mouseButtonsDown.Contains(button);
        }

        /// <summary>
        ///   Hand the mouse moved events off to widgets and update the 
        ///   cursor position if appropriate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void input_MouseMoved(object sender, Axiom.Input.MouseEventArgs e) {
            bool moveHandled = false;
            bool wheelHandled = false;
            input_MouseMovedHelper(out moveHandled, out wheelHandled, sender, e);
        }

        protected void input_MouseMovedHelper(out bool moveHandled, out bool wheelHandled, 
                                              object sender, Axiom.Input.MouseEventArgs e) {
            // store the updated absolute values
            mouseFrameRelX += e.RelativeX;
            mouseFrameRelY += e.RelativeY;
            mouseFrameRelZ += e.RelativeZ;

            moveHandled = false;
            wheelHandled = false;

            // If we are dragging an item, the gui will have focus, and we 
            // want to send these mouse move events to the gui system.  Also, 
            // if the mouse button is not down, we want to send the mouse
            // move events to the gui system.  If the mouse is down, and is
            // not over a gui window, we don't want to send the mouse events 
            // to the gui system, because we aren't moving the cursor.
            if (!this.IsMouseLook() || UiSystem.CaptureFrame != null) {
                // If none of the mouse buttons are down, they are moving the cursor
                // If the gui has focus, we want the gui to get mouse events.

                // Don't pass mouse movement through to the GuiSystem if we are
                // holding down a mouse button, and the gui doesn't have focus, 
                // since they are probably just adjusting the camera.

                // grab the relative mouse movement
                int rotX = (int)e.RelativeX;
                int rotY = (int)e.RelativeY;

                // inject mouse movement into the GuiSystem
                if (rotX != 0 || rotY != 0) {
                    Multiverse.Interface.UiSystem.InjectMouseMove(e);
                    if (!e.Handled)
                        GuiSystem.Instance.InjectMouseMove(e.X, e.Y, rotX, rotY);
                }
                moveHandled = true; // mouse movement should not be passed to the camera code

                // Mouse wheel test
                if (e.RelativeZ != 0) {
                    float wheelStep = (e.RelativeZ > 0) ? 1 : -1;
                    wheelHandled = Multiverse.Interface.UiSystem.InjectMouseWheel(wheelStep);
                }
                // End mouse wheel test
            }
        }

        protected void input_MouseDown(object sender, Axiom.Input.MouseEventArgs e) {
            mouseDownEvent = e;
            mouseDownObject = mouseoverObject;

            if (e.Button != MouseButtons.None)
            {
                // Keep track of which mouse buttons we think are down, and
                // send those to the gui system.
                if (!mouseButtonsDown.Contains(e.Button))
                {
                    mouseButtonsDown.Add(e.Button);
                    log.InfoFormat("Added mouse button {0} to list of down buttons", e.Button);
                    GuiSystem.Instance.OnMouseDown(e);
                }
            }
        }

        protected void input_MouseUp(object sender, Axiom.Input.MouseEventArgs e) {
            bool clickEvent = false;
            if (mouseDownEvent != null &&
                (e.TimeStamp - mouseDownEvent.TimeStamp < ClickToleranceTime) &&
                (e.Button == mouseDownEvent.Button) &&
                (Math.Abs(e.X - mouseDownEvent.X) < ClickToleranceSpace) &&
                (Math.Abs(e.Y - mouseDownEvent.Y) < ClickToleranceSpace)) {
                // Was the mouse button released the only mouse button down?
                // If so, it will be a click event
                switch (e.Button) {
                    case MouseButtons.Left:
                        if (!IsMousePressed(MouseButtons.Right) &&
                            !IsMousePressed(MouseButtons.Middle))
                            clickEvent = true;
                        break;
                    case MouseButtons.Right:
                        if (!IsMousePressed(MouseButtons.Left) &&
                            !IsMousePressed(MouseButtons.Middle))
                            clickEvent = true;
                        break;
                    case MouseButtons.Middle:
                        if (!IsMousePressed(MouseButtons.Left) &&
                            !IsMousePressed(MouseButtons.Right))
                            clickEvent = true;
                        break;
                }
            }

            if (e.Button != MouseButtons.None) {
                if (mouseButtonsDown.Contains(e.Button))
                {
                    mouseButtonsDown.Remove(e.Button);
                    GuiSystem.Instance.OnMouseUp(e);
                    if (clickEvent)
                        Multiverse.Interface.UiSystem.InjectClick(e.Button);
                }
            }

            // If the mouse down was over an object, and the mouse up is over 
            // the same object, this is a click.
            if (clickEvent && mouseoverObject != null && 
                mouseoverObject == mouseDownObject) {
                mouseoverObject.OnMouseClicked(mouseDownEvent);
            }

            // Restore the GuiSystem's cursor if no buttons are down
            if (!this.IsMouseLook() && input.HasFocus()) {
                Multiverse.Interface.UiSystem.RestoreCursor();
                cursorEnabled = true;
                input.CursorEnabled = cursorEnabled;
            }
        }

        protected void input_MouseLost(object sender, EventArgs e)
        {
            foreach (MouseButtons button in mouseButtonsDown)
            {
                MouseEventArgs mouseEvent = new MouseEventArgs(button, ModifierKeys.None, 0, 0, 0);
                GuiSystem.Instance.OnMouseUp(mouseEvent);
            }
            mouseButtonsDown.Clear();
        }

        protected void input_KeyDown(object sender, Axiom.Input.KeyEventArgs e) {
            // fire the keydown and character events
            GuiSystem.Instance.OnKeyDown(e);
        }

        protected void input_KeyUp(object sender, Axiom.Input.KeyEventArgs e) {
            GuiSystem.Instance.OnKeyUp(e);
        }


        /// <summary>
        ///   Should we be disabling the movement of the cursor?
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMouseLook() {
            bool currentMouseDown = IsMousePressed();
            // If we are dragging an item, the gui will have focus, and we 
            // want to send these mouse move events to the gui system.  Also, 
            // if the mouse button is not down, we want to send the mouse
            // move events to the gui system.  If the mouse is down, and is
            // not over a gui window, we don't want to send the mouse events 
            // to the gui system, because we aren't moving the cursor.
            return !client.GuiHasFocus && currentMouseDown;
        }
    }

  
	public class DefaultInputHandler : GuiInputHandler
	{
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DefaultInputHandler));

        protected Vector3 playerDir = Vector3.Zero;
		protected Vector3 playerAccel = Vector3.Zero;

		// These should be documented, since we're making them
		// externally accessible
		protected float playerSpeed = 7.0f * Client.OneMeter; // 7 m/s
		public float RotateSpeed = 90.0f; // Rotate speed with cursor keys
		protected float RotateFixSpeed = 180.0f; // Rotate speed for adjusting
		public float MouseVelocity = .13f;
		public float MouseWheelVelocity = -.001f;
        /// <summary>
        ///   This variable allows me to lock the interface to mouse look mode.
        ///   Essentially, this treats the interface as though the right mouse
        ///   button is down all the time for purposes of the camera updates.
        ///   The movement still follows 
        /// </summary>
        protected bool mouseLookLocked = false;

#region Parameter Code
		private bool trueString (string value) 
		{
			return value.ToLower() == "true";
		}
		
		private bool setCameraParameterHandler(string parameterName, string parameterValue)
		{
			switch (parameterName) {
			case "PlayerVisible":
				playerVisible = trueString(parameterValue);
				break;
			case "CameraFirstPerson":
				cameraFirstPerson = trueString(parameterValue);
				break;
			case "CameraFree":
				cameraFree = trueString(parameterValue);
				break;
			case "CameraPosition":
				cameraPosition = Vector3.Parse(parameterValue);
				break;
			case "CameraMotionYaw":
				cameraMotionYaw = trueString(parameterValue);
				break;
			default:
				float value = 0f;
				try {
					value = float.Parse(parameterValue);
				}
				catch(Exception) {
					return false;
				}
				switch (parameterName) {
				case "CameraDistance":
					CameraDistance = value;
					break;
				case "CameraMaxDistance":
					CameraMaxDistance = value;
					break;
				case "CameraPitch":
					CameraPitch = value;
					break;
				case "CameraMaxPitch":
					MaxPitch = value;
					break;
				case "CameraMinPitch":
					MinPitch = value;
					break;
				case "CameraYaw":
					CameraYaw = value;
					break;
				case "MinPlayerVisibleDistance":
					MinPlayerVisibleDistance = value;
					break;
				case "MinThirdPersonDistance":
					MinThirdPersonDistance = value;
					break;
				case "HeadHeightAbovePlayerOrigin":
					headHeightAbovePlayerOrigin = value;
                    cameraTargetOffset = headHeightAbovePlayerOrigin * Vector3.UnitY;
					break;
				case "MoveCameraBy":
					Vector3 delta = client.Camera.DerivedDirection.ToNormalized() * value;
					cameraPosition += delta;
					break;
				default:
					return false;
				}
				return false;
			}
			return true;
		}
		
		private bool getCameraParameterHandler(string parameterName, out string parameterValue)
		{
			parameterValue = "";
			switch (parameterName) {
			case "Help":
				parameterValue = CameraParameterHelp();
				break;
			case "PlayerVisible":
				parameterValue = (playerVisible ? "true" : "false");
				break;
			case "CameraFirstPerson":
				parameterValue = (cameraFirstPerson ? "true" : "false");
				break;
			case "CameraFree":
				parameterValue = (cameraFree ? "true" : "false");
				break;
			case "CameraPosition":
				parameterValue = cameraPosition.ToString();
				break;
			case "CameraMotionYaw":
				parameterValue = (cameraMotionYaw ? "true" : "false");
                break;
			default:
                float value = 0f;
				switch (parameterName) {
				case "CameraDistance":
					value = cameraDist;
					break;
				case "CameraMaxDistance":
					value = cameraMaxDist;
					break;
				case "CameraPitch":
					value = CameraPitch;
					break;
				case "CameraMinPitch":
					value = MinPitch;
					break;
				case "CameraMaxPitch":
					value = MaxPitch;
					break;
				case "CameraYaw":
					value = CameraYaw;
					break;
				case "MinPlayerVisibleDistance":
					value = minPlayerVisibleDistance;
					break;
				case "MinThirdPersonDistance":
					value = minThirdPersonDistance;
					break;
				case "HeadHeightAbovePlayerOrigin":
					value = headHeightAbovePlayerOrigin;
					break;
				default:
					return false;
				}
				parameterValue = value.ToString();
				return true;
			}
			return true;
		}

		private string CameraParameterHelp()
		{
			return
				"bool PlayerVisible: A boolean determining if the player will be rendered" +
				"\n" +
				"bool CameraFirstPerson: A boolean determining if the camera is in first-person mode" +
				"\n" +
				"bool CameraFree: A boolean determining whether motion of the player causes motion of the camera; " +
				"the default is false.  CameraPosition is ignored unless CameraFree is true; if CameraFree is true, " +
				"then CameraDistance and CameraMaxDistance are ignored" +
				"\n" +
				"Vector3 CameraPosition: If CameraFree is true, this is the position of the camera, and changing " +
				"CameraPosition changes the position of the camera.  If CameraFree is false, CameraPosition is " +
				"ignored" +
				"\n" +
				"float MoveCameraBy: If CameraFree is true, setting this signed parameter moves the camera the " +
				"specified number of millimeters in the direction the camera is pointing.  Getting this parameter " +
				"returns the null string\n" +

				"float CameraDistance: The current distance between the camera and the head of the player, " +
				"measured in millimeters; the default is 5000 millimeters" +
				"\n" +
				"float CameraMaxDistance: The maximum distance allowed between the camera and the head of the player, " +
				"measured in millimeters; the default is 20000 millimeters" +
				"\n" +
				"float CameraPitch: The angle in degrees from the horizontal toward which the camera is pointed" +
				"\n" +
				"float CameraMaxPitch: The maximum pitch of the camera, in degrees; default is 85 degrees" +
				"\n" +
				"float CameraMinPitch: The minimum pitch of the camera, in degrees; default is -85 degrees" +
				"\n" +
				"float CameraYaw: The horizontal rotation angle of the camera, in degrees measured from + Z" +
				"\n" +
				"bool CameraMotionYaw: If true, when the player moves, set the camera yaw to the player yaw; " +
				"the default is true" +
				"\n" +
				"float MinPlayerVisibleDistance: The minimum distance from the camera to the player, in millimeters, " +
				"at which the player is still visible; default is 2000 millimeters" +
				"\n" +
				"float MinThirdPersonDistance: The minimum distance from the camera to the player, in millimeters, " +
				"at which the camera is not yet in first-person mode; default is 1000 millimeter" +
				"\n" +
				"float HeadHeightAbovePlayerOrigin: The distance, in meters, from the player origin to the eyes in " +
				"the player head, measured in millimeters; the default is 1800 millimeters" +
				"\n";
		}
#endregion
		
		// Target player yaw info - so we approach an orientation instead of
		// just jumping there.
//		protected bool useTargetPlayerYaw;
//		protected float targetPlayerYaw;

        protected Quaternion cameraOrientation;
        protected Quaternion playerOrientation;

		// Camera parameters - - exposed via the ParameterRegistry
        protected bool playerVisible = true;
		protected bool cameraFirstPerson = false;
		protected bool cameraFree = false;
		protected bool cameraMotionYaw = true;

		protected float minPlayerVisibleDistance = 2f * Client.OneMeter;
		protected float minThirdPersonDistance = 1f * Client.OneMeter;
		protected float headHeightAbovePlayerOrigin = 1.8f * Client.OneMeter;
        protected Vector3 cameraTargetOffset;
		// cameraPosition is only used if cameraFree is true
		protected Vector3 cameraPosition = Vector3.Zero;
		
		// Camera pitch (in degrees)
		protected float maxPitch = 85.0f;
		protected float minPitch = -85.0f;
		protected float cameraDist = 5 * Client.OneMeter;
		protected float cameraMaxDist = 20 * Client.OneMeter;


        // is the camera grabbed by a script
        protected bool cameraGrabbed = false;

		protected SceneManager sceneManager;
        protected bool playerOrientationInitialized = false;

		protected bool consoleMode = false;

        protected Multiverse.Interface.Region mouseoverFrame;

        public DefaultInputHandler(Client client) : base(client) {
			this.sceneManager = client.WorldManager.SceneManager;

			ParameterRegistry.RegisterSubsystemHandlers("Camera", setCameraParameterHandler,
														getCameraParameterHandler);

            cameraTargetOffset = headHeightAbovePlayerOrigin * Vector3.UnitY;
 		    cameraOrientation = Quaternion.FromAngleAxis(MathUtil.DegreesToRadians(-20.0f), Vector3.UnitX);
		}

		/// <summary>
		///   Move the camera based on the new position of the camera target (player)
		/// </summary>
		/// <param name="playerPos"></param>
		/// <param name="playerOrient"></param>
		protected void UpdateViewpoint(bool playerMoved, Vector3 playerPos, Quaternion playerOrient) {
			UpdateCamera(playerMoved, playerPos, playerOrient);
			SoundManager.Instance.ListenerPosition = client.Camera.DerivedPosition;
			SoundManager.Instance.ListenerForward = client.Camera.DerivedDirection;
            SoundManager.Instance.ListenerUp = client.Camera.DerivedUp;
		}


        // Keep the code below, commented out, because it is useful
        // for debugging near plane issues

// 		   private static int nearPlaneCollisionCount = 0;
//         private static int nearPlaneNodeId = 99999;
//         private static List<RenderedNode>[] nearPlaneRenderedNodes = new List<RenderedNode>[4] {
//             new List<RenderedNode>(), new List<RenderedNode>(), new List<RenderedNode>(), new List<RenderedNode>() };
//         private static CollisionCapsule[] nearPlaneCapsules = new CollisionCapsule[4];
//        
//         protected void CreateNearPlaneCapsules(Camera camera,
//                                                Vector3 cameraPosition,
//                                                Vector3 playerHeadPosition)
//         {
//             // Frame the clip plane with 4 narrow capsules
//             // Start by finding the center of the near plane
//             Vector3 toPlayer = (playerHeadPosition - cameraPosition).ToNormalized();
//             Vector3 center = cameraPosition + (1.02f * camera.Near) * toPlayer;
//             float thetaY = MathUtil.DegreesToRadians(camera.FOVy * 0.5f);
//             float tanThetaY = MathUtil.Tan(thetaY);
//             float tanThetaX = tanThetaY * camera.AspectRatio;
//             Vector3 right = tanThetaX * camera.Near * (camera.Right.ToNormalized());
//             Vector3 up = tanThetaY * camera.Near * (camera.Up.ToNormalized());
//             Vector3 corner1 = center + right - up;
//             Vector3 corner2 = center + right + up;
//             Vector3 corner3 = center - right + up;
//             Vector3 corner4 = center - right - up;
//             nearPlaneCapsules[0] = new CollisionCapsule(corner1, corner2, Client.OneMeter * .001f);
//             nearPlaneCapsules[1] = new CollisionCapsule(corner2, corner3, Client.OneMeter * .001f);
//             nearPlaneCapsules[2] = new CollisionCapsule(corner3, corner4, Client.OneMeter * .001f);
//             nearPlaneCapsules[3] = new CollisionCapsule(corner4, corner1, Client.OneMeter * .001f);
//            
//             RenderedNode.Scene = sceneManager;
//             for (int i=0; i<4; i++) {
//                 RenderedNode.RemoveNodeRendering(nearPlaneNodeId + i, ref nearPlaneRenderedNodes[i]);
//                 RenderedNode.CreateRenderedNodes(nearPlaneCapsules[i], nearPlaneNodeId + i, ref nearPlaneRenderedNodes[i]);
//             }
//         }
		
		protected float FindAcceptableCameraPosition(Camera camera, Vector3 cameraPos, Vector3 cameraTarget) 
        {
            //Logger.Log(0, "FindAcceptableCameraPosition cameraPos {0}, cameraTarget {1}", cameraPos, cameraTarget);

            // If there is no CollisionAPI object, punt
			CollisionAPI api = client.WorldManager.CollisionHelper;
			if (api == null)
				return (cameraPos - cameraTarget).Length;

            // Start by finding the point at which a narrow capsule
            // from the camera to the player intersects.  If it
            // doesn't, then set the point to the cameraPos.
         
            // Make a very narrow capsule between the camera and the player's head
			CollisionCapsule cap = new CollisionCapsule(cameraPos,
														cameraTarget,
														Client.OneMeter * .001f);  // 1 millimeter
			Vector3 newCameraPos = cameraPos;
            Vector3 unitTowardCamera = (newCameraPos - cameraTarget).ToNormalized();
            
            // Test for a collision, setting intersection if there is one.
            Vector3 intersection = Vector3.Zero;
			if (api.FindClosestCollision(cap, cameraTarget, cameraPos, ref intersection) != null) {
                // There is A collision - - set the new camera pos to
                // be the point of intersection
                newCameraPos = intersection - unitTowardCamera * camera.Near;
                //Logger.Log(0, "FindAcceptableCameraPosition: Thin cap collides at {0}", newCameraPos);
            }
                
            // Move a near-plane OBB forward from the point of
            // intersection until we find a place where the near plane
            // doesn't intersect any collision volumes

            Vector3 center = newCameraPos - camera.Near * unitTowardCamera;
            float thetaY = MathUtil.DegreesToRadians(camera.FOVy * 0.5f);
            float tanThetaY = MathUtil.Tan(thetaY);
            float tanThetaX = tanThetaY * camera.AspectRatio;

            // Compute the OBB axes and extents
            Vector3[] axes = new Vector3[3];
            Vector3 extents = Vector3.Zero;
            // The axis perpendicular to the near plane
            float obbDepth = 100f;  // 100 mm thick
            axes[0] = -unitTowardCamera;
            axes[1] = -axes[0].Cross(Vector3.UnitY).ToNormalized();
            axes[2] = -axes[1].Cross(axes[0]).ToNormalized();
            extents[0] = obbDepth;
            extents[1] = camera.Near * tanThetaX;
            extents[2] = camera.Near * tanThetaY;
            float len = (newCameraPos - cameraTarget).Length;
            float startingLen = len;
            CollisionOBB obb = new CollisionOBB(center, axes, extents);
            CollisionParms parms = new CollisionParms();
            while (len >= minThirdPersonDistance) {
                obb.center = cameraTarget + unitTowardCamera * len;
                bool collides = api.ShapeCollides(obb, parms);
                //Logger.Log(0, "FindAcceptableCameraPosition len {0}, obb center {1}, collides {2}", len, obb.center, collides);
                if (!collides)
                    break;
                //client.Write(string.Format("OBB collides; len {0}", len));
                len -= obbDepth;
            }
            // Len is less than the camera min distance, so just
            // return it
            //Logger.Log(0, "FindAcceptableCameraPosition: starting len {0} final len {1}", startingLen, len);
            return len;
        }
        
        /// <summary>
		///   Move the camera based on the new position of the camera target (player)
		/// </summary>
		/// <param name="playerPos"></param>
		/// <param name="playerOrient"></param>
		protected void UpdateCamera(bool playerMoved, Vector3 playerPos, Quaternion playerOrient) {
			// Convenience to avoid calling client.Camera everywhere
			Camera camera = client.Camera;

            // If the camera isn't free and the player is moving, and
			// if the boolean indicating we should point the camera in
			// the direction of player motion is true, do so.
            //if (!cameraFree && playerMoved && cameraMotionYaw)
            //    this.CameraYaw = playerOrient.YawInDegrees + 180f;
            log.DebugFormat("Orientation: {0}", cameraOrientation.EulerString);

            Vector3 cameraDir = cameraOrientation * Vector3.UnitZ;

			// Look at a point that is 1.8m above the player's base - this should be
			// around the character's head.
			Vector3 cameraTarget = playerPos + playerOrient * cameraTargetOffset;

            if (cameraGrabbed)
            {
                // if the camera is grabbed, then dont do anything
            }
            // If the camera is free, just set the position and direction
			else if (cameraFree) {
				camera.Position = cameraPosition;
				camera.Direction = -cameraDir;
                cameraDist = (playerPos - cameraPosition).Length;
				client.Player.SceneNode.Visible = cameraDist > minThirdPersonDistance;
                log.DebugFormat("Camera is free; cameraPosition = {0}, cameraOrientation = {1}",
                                cameraPosition, cameraOrientation.EulerString);
			}
			else {
                // Put the camera cameraDist behind the player
                Vector3 cameraPos = cameraTarget + cameraDir * cameraDist;
                Vector3 targetDir = (cameraPos - cameraTarget).ToNormalized();
                float len = FindAcceptableCameraPosition(camera, cameraPos, cameraTarget);

				// The player is visible if the camera is further
				// than the minimum player visible distance
				playerVisible = len > minPlayerVisibleDistance;
                // Record if we were first person last frame
                bool formerlyFirstPerson = cameraFirstPerson;
                // We shift to first-person mode if the distance
				// is less than the minimum third-person distance
                cameraFirstPerson = len < minThirdPersonDistance;
				// Set the camera position to the target if we're
				// in first person mode, else use the calculated
				// position.
				camera.Position = (cameraFirstPerson ? cameraTarget : cameraTarget + cameraDir * len);

                if (cameraFirstPerson)
                    camera.Direction = -cameraDir;
                else
					camera.LookAt(cameraTarget);

                log.DebugFormat("Camera is not free; camera.Position = {0}, camera.Orientation = {1}, camera.Direction = {2}, cameraTarget = {3}",
                                camera.Position, camera.Orientation.EulerString, camera.Direction, cameraTarget);

				client.Player.SceneNode.Visible = playerVisible;
			
				float maxHeight;

				// build the points of the near clip plane
				Vector3[] points = new Vector3[4];
				for (int i = 0; i < 4; ++i)
					points[i] = camera.WorldSpaceCorners[i];
                SceneManager sceneManager = client.WorldManager.SceneManager;
				if (sceneManager is Axiom.SceneManagers.Multiverse.SceneManager) {
					Axiom.SceneManagers.Multiverse.SceneManager mvsm =
						(Axiom.SceneManagers.Multiverse.SceneManager)sceneManager;
					maxHeight = mvsm.GetAreaHeight(points);
				} else
					maxHeight = 0.0f;

				float delta = 0.0f;
				for (int i = 0; i < 4; ++i)
					if (points[i].y < maxHeight)
						delta = Math.Max(delta, maxHeight - points[i].y);
				// This shows how much we need to raise the corners by.
				if (delta > 0) {
					// Scale the camera movement so that it will move up enough to get that near box up
					float targetToCam = (cameraTarget - camera.WorldPosition).Length;
					float camDelta = delta * (targetToCam / (targetToCam - camera.Near));
					Vector3 camPos = camera.Position;
					camPos.y += camDelta;
					camera.Position = camPos;
					// Re-evaluate the lookat target, since otherwise we will be looking too high.
					// TODO: Corners can dip below terrain here, but this is unlikely
					// Unfortunately, this rotation can push the corners back down below the
					// terrain, but for now, I'm ignoring that.  A bigger issue is the 
					// stair-stepping of the camera due to the maximum height code.
					camera.LookAt(cameraTarget);
					// Try to prevent stairstepping by updating camera pitch
					Vector3 relativePos = cameraTarget - camera.Position;
					float theta = (float)Math.Asin((cameraTarget.y - camera.Position.y) / relativePos.Length);
					// Update the camera's pitch for later
					this.CameraPitch = MathUtil.RadiansToDegrees(theta);
                    log.DebugFormat("Camera is not free; Adjusted camera.Position = {0}, camera.Orientation = {1}",
                                    camera.Position, camera.Orientation.EulerString);
                }
			}
		}

        protected void UpdateCursorVisibility(bool mouseDown) {
            log.DebugFormat("Called UpdateCursorVisibility; MouseDown = {0}; MouseLookLocked = {1}", mouseDown, mouseLookLocked);
            cursorEnabled = true;
            if (input.HasFocus() && (mouseLookLocked || mouseDown)) {
                // Clear the cursor, since we aren't in the right mode for it.
                Multiverse.Interface.UiSystem.ClearCursor();
                cursorEnabled = false;
            } else {
                // Restore the GuiSystem's cursor
                Multiverse.Interface.UiSystem.RestoreCursor();
                cursorEnabled = true;
            }
            input.CursorEnabled = cursorEnabled;
        }

        /// <summary>
        ///   This is called from the MouseMoved handler.  It updates the 
        ///   cameraPitch, cameraYaw and cameraDist variables (which are 
        ///   later used to modify the camera).  It may also update the 
        ///   PlayerYaw.
        /// </summary>
        /// <param name="e"></param>
		protected virtual void UpdateCamera(MouseEventArgs e) {
            UpdateCursorVisibility(IsMousePressed(MouseButtons.Left) || IsMousePressed(MouseButtons.Right));
			if (mouseLookLocked || this.IsMousePressed(MouseButtons.Right)) {
				// If they are holding down the right mouse button, 
				// rotate both the camera and the player
                ApplyMouseToCamera(e.RelativeX, e.RelativeY);

				// Whenever we use second mouse button or movement keys to 
				// rotate, reset the player's orientation to match the camera
				// Since the player model's default orientation is facing the 
				// camera, spin the camera an extra 180 degrees.
                if (e.RelativeX != 0 || e.RelativeY != 0) {
                    Quaternion q = Quaternion.FromAngleAxis((float)Math.PI, Vector3.UnitY);
                    this.PlayerOrientation = this.CameraOrientation * q;
                }
			} else if (this.IsMousePressed(MouseButtons.Left)) {
                // If they are holding down the left mouse button, 
                // rotate the camera around the player.
                ApplyMouseToCamera(e.RelativeX, e.RelativeY);
			}

            // Set the lower bound on the treatment of camera distance so we
            // act as though we are at least 10cm awway.
            float mult = Math.Max(.1f * Client.OneMeter, cameraDist);
            // a non-linear distance transform here for the scroll wheel
            float d = MouseWheelVelocity * mult * e.RelativeZ;
			cameraDist += d;
            if (cameraDist < 0)
                cameraDist = 0;

			// limit the range of camera movement
			cameraDist = Math.Min(cameraMaxDist, cameraDist);

			// Check to see if the player should be visible
			playerVisible = cameraDist > minPlayerVisibleDistance;
		}

        /// <summary>
        ///   Use the information from the mouse movement to update the camera.
        /// </summary>
        /// <param name="x">the relative x offset of the mouse (in mouse units)</param>
        /// <param name="y">the relative y offset of the mouse (in mouse units)</param>
        protected void ApplyMouseToCamera(float x, float y) {
            this.CameraYaw -= x * MouseVelocity;
            float cameraPitch = this.CameraPitch;
            cameraPitch -= y * MouseVelocity;
            if (cameraPitch < MinPitch)
                cameraPitch = MinPitch;
            else if (cameraPitch > MaxPitch)
                cameraPitch = MaxPitch;
            this.CameraPitch = cameraPitch;
        }

        public override bool IsMouseLook() {
            return mouseLookLocked || base.IsMouseLook();
        }

        // keep track of what movement keys are pressed
		// 0,1 = left/right ; 2,3 = forward/back ; 4,5 = strafe
        // 6,7 up/down ; 8 = autorun
		protected enum MoveEnum : int {
            Left = 0,
            Right = 1,
            Forward = 2,
            Back = 3,
            StrafeLeft = 4,
            StrafeRight = 5,
            Up = 6,
            Down = 7,
            AutoRun = 8,
            Count = 9
        }
        
		protected bool[] movement = new bool[(int)MoveEnum.Count];

        public void MoveForward(bool status) {
            if (status) {
                movement[(int)MoveEnum.Forward] = true;
                movement[(int)MoveEnum.Back] = false;
                movement[(int)MoveEnum.AutoRun] = false;
            } else {
                movement[(int)MoveEnum.Forward] = false;
            }
        }

        public void MoveBackward(bool status) {
            if (status) {
           		movement[(int)MoveEnum.Forward] = false;
				movement[(int)MoveEnum.Back] = true;
				movement[(int)MoveEnum.AutoRun] = false;
            } else {
                movement[(int)MoveEnum.Back] = false;
            }
        }

        public void TurnLeft(bool status) {
            if (status) {
            	movement[(int)MoveEnum.Left] = true;
			    movement[(int)MoveEnum.Right] = false;
            } else {
                movement[(int)MoveEnum.Left] = false;
            }
        }

        public void TurnRight(bool status) {
            if (status) {
                movement[(int)MoveEnum.Left] = false;
				movement[(int)MoveEnum.Right] = true;
            } else {
                movement[(int)MoveEnum.Right] = false;
            }
        }

        public void StrafeLeft(bool status) {
            movement[(int)MoveEnum.StrafeLeft] = status;
        }

        public void StrafeRight(bool status) {
            movement[(int)MoveEnum.StrafeRight] = status;
        }

        public void MoveUp(bool status) {
            if (status) {
                movement[(int)MoveEnum.Up] = true;
                movement[(int)MoveEnum.Down] = false;
                movement[(int)MoveEnum.AutoRun] = false;
            } else {
                movement[(int)MoveEnum.Up] = false;
            }
        }

        public void MoveDown(bool status) {
            if (status) {
           		movement[(int)MoveEnum.Up] = false;
				movement[(int)MoveEnum.Down] = true;
				movement[(int)MoveEnum.AutoRun] = false;
            } else {
                movement[(int)MoveEnum.Down] = false;
            }
        }

        public void ToggleAutorun() {
            movement[(int)MoveEnum.AutoRun] = !movement[(int)MoveEnum.AutoRun];
        }

		/// <summary>
		///   Handle the keyboard and mouse input for movement of the player and camera.
		///   This method name says immediate, but really it is acting on a keyboard state
		///   that may have been filled in by buffered or immediate input.
		/// </summary>
		/// <param name="timeSinceLastFrame">This is supposed to be in milliseconds, but seems to be in seconds.</param>
		protected void HandleImmediateKeys(float timeSinceLastFrame, long now) {
			// Now handle movement and stuff

			// Ignore the input if we're in the loading state
            if (Client.Instance.LoadingState)
                return;
            
            // reset acceleration zero
			playerAccel = Vector3.Zero;

			if (movement[(int)MoveEnum.Forward] || movement[(int)MoveEnum.AutoRun])
				playerAccel.z += 1.0f;
			if (movement[(int)MoveEnum.Back])
				playerAccel.z -= 1.0f;
			if (movement[(int)MoveEnum.StrafeLeft])
				playerAccel.x += 0.5f;
			if (movement[(int)MoveEnum.StrafeRight])
				playerAccel.x -= 0.5f;
			if (movement[(int)MoveEnum.Up])
				playerAccel.y += 0.5f;
			if (movement[(int)MoveEnum.Down])
				playerAccel.y -= 0.5f;
            
			log.DebugFormat("HandleImmediateKeys: playerAccel = {0}", playerAccel);
            
            // If mouse2 (button1) is down, left and right are strafe.
			// Otherwise, they are rotation.
			if (mouseLookLocked || this.IsMousePressed(MouseButtons.Right)) {
				// Apply the left and right as strafe
				if (movement[(int)MoveEnum.Left])
					playerAccel.x += 0.5f;
				if (movement[(int)MoveEnum.Right])
					playerAccel.x -= 0.5f;
			} else {
				if (client.Player.CanTurn()) {
					// Apply the left and right as rotate
					if (movement[(int)MoveEnum.Left])
						this.PlayerYaw += RotateSpeed * timeSinceLastFrame;
					if (movement[(int)MoveEnum.Right])
						this.PlayerYaw -= RotateSpeed * timeSinceLastFrame;
				}
				// If the left mouse is not down, rotate the camera, but 
				// otherwise, leave it alone
				if (!this.IsMousePressed(MouseButtons.Left)) {
					if (movement[(int)MoveEnum.Left])
						this.CameraYaw += RotateSpeed * timeSinceLastFrame;
					if (movement[(int)MoveEnum.Right])
						this.CameraYaw -= RotateSpeed * timeSinceLastFrame;
				}
			}
		}

		public void InitViewpoint(Player player) {
			float tmpAngle = Multiverse.MathLib.MathUtil.GetFullYaw(player.Orientation);
			// the default player orientation (identity) has the player facing +Z.
			// the default camera orientation should have the camera facing -Z;
			// To accomplish this, the camera yaw should be opposite the player yaw.
			float tmpDegrees = MathUtil.RadiansToDegrees(tmpAngle);
			this.CameraYaw = tmpDegrees - 180;
			this.PlayerOrientation = player.Orientation;
		}

		protected static TimingMeter applyImmediateMeter = MeterManager.GetMeter("Apply Immediate", "Input");
		
		protected void ApplyImmediateInput(long now) {
			if (client.Player == null)
				return;
            else if (!playerOrientationInitialized) {
                playerOrientation = client.Player.Orientation;
                InitViewpoint(client.Player);
                playerOrientationInitialized = true;
            }
			applyImmediateMeter.Enter();
			SceneNode playerNode = client.Player.SceneNode;

            // If appropriate, clamp the player orientation to the ground
            if (client.Player.FollowTerrain)
                playerOrientation = Quaternion.FromEulerAnglesInDegrees(0, PlayerYaw, 0);

            // Update the player's orientation to match what we have stored as his yaw.
			if (client.Player.CanTurn())
                client.Player.Orientation = playerOrientation;

            if (client.Player.CanMove()) {
                // Set the player's direction vector based on his 
                // target orientation and playerAccel vector.
                playerAccel.Normalize(); // this isn't really right - 
                // moving backward, left and right should be slower than forward.
                playerDir = playerSpeed * (client.Player.Orientation * playerAccel);
                Vector3 playerPos = playerNode.Position;
//                 Logger.Log(0, "ApplyImmediateInput: playerAccel = {0}, playerPos = {1}, playerDir = {2}",
//                            playerAccel, playerPos, playerDir);
                client.Player.SetDirection(playerDir, playerPos, now);
            }

            UpdateViewpoint(playerAccel != Vector3.Zero, playerNode.Position, playerNode.Orientation);
			applyImmediateMeter.Exit();
		}

        protected override void input_MouseMoved(object sender, Axiom.Input.MouseEventArgs e) {
            bool moveHandled = false;
            bool wheelHandled = false;
            input_MouseMovedHelper(out moveHandled, out wheelHandled, sender, e);
            if (!input.HasFocus())
                return;
            if (!moveHandled && (e.RelativeX != 0 || e.RelativeY != 0)) {
                // The movement wasn't handled by the gui (to update cursor or ui), and we had movement
                MouseEventArgs moveArgs = e;
                if (wheelHandled && moveArgs.RelativeZ != 0)
                    // If there was a mouse wheel movement, and that was handled by the ui, strip it out here
                    moveArgs = new MouseEventArgs(e.Button, e.Modifiers, e.X, e.Y, e.Z, e.RelativeX, e.RelativeY, 0);
                UpdateCamera(e);
                wheelHandled = true; // either the wheel was handled earlier, or the UpdateCamera call just handled it.
                moveHandled = true; // we just handled the movement
            }
            if (!wheelHandled && e.RelativeZ != 0) {
                // The wheel wasn't handled by the gui (to update ui), and we had wheel movement
                MouseEventArgs moveArgs = e;
                if (moveHandled && (e.RelativeX != 0 || e.RelativeY != 0))
                    // If there was a mouse move, and that was handled already, strip it out here
                    moveArgs = new MouseEventArgs(e.Button, e.Modifiers, e.X, e.Y, e.Z, 0, 0, e.RelativeZ);
                UpdateCamera(e);
                wheelHandled = true; // we just handled the wheel
                moveHandled = true; // either the movement was handled earlier, or the UpdateCamera call just handled it.
            }
		}

		protected static TimingMeter immediateKeysMeter = MeterManager.GetMeter("Immediate Keys", "Input");

        public override void OnFrameStarted(object source, FrameEventArgs e, long now) {
            base.OnFrameStarted(source, e, now);

			// This looks at the state of the mouse buttons and the keys,
            // and uses this information to determine how to move the camera.
            // Notably, we are not looking at mouse movement (just buttons).
			immediateKeysMeter.Enter();
			HandleImmediateKeys(e.TimeSinceLastFrame, now);
			immediateKeysMeter.Exit();

			// Apply the result of any mouse and keyboard input.
            // This takes the playerYaw, cameraYaw, playerAccel
            // cameraPitch, etc.. and uses them to update the
            // player's desired orientation and direction.  It also
            // updates the camera.
			ApplyImmediateInput(now);
        }

        #region Properties

        public float MinPlayerVisibleDistance
        {
            get
            {
                return minPlayerVisibleDistance;
            }
            set
            {
                minPlayerVisibleDistance = value;
            }
        }

        public float MinThirdPersonDistance
        {
            get
            {
                return minThirdPersonDistance;
            }
            set
            {
                minThirdPersonDistance = value;
            }
        }

        public float CameraDistance
        {
            get
            {
                return cameraDist;
            }
            set
            {
                cameraDist = value;
            }
        }

        public float CameraMaxDistance
        {
            get
            {
                return cameraMaxDist;
            }
            set
            {
                cameraMaxDist = value;
            }
        }

        // These angles are all in degrees.
        public float CameraPitch {
            get {
                float pitch, yaw, roll;
                cameraOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                return pitch;
            }
            set {
                float pitch, yaw, roll;
                cameraOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                pitch = value;
                cameraOrientation = Quaternion.FromEulerAnglesInDegrees(pitch, yaw, roll);
            }
        }

        public float MaxPitch
        {
            get
            {
                return maxPitch;
            }
            set
            {
                maxPitch = value;
            }
        }

        public float MinPitch
        {
            get
            {
                return minPitch;
            }
            set
            {
                minPitch = value;
            }
        }

        public float CameraYaw {
            get {
                float pitch, yaw, roll;
                cameraOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                return yaw;
            }
            set {
                float pitch, yaw, roll;
                cameraOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                yaw = value;
                cameraOrientation = Quaternion.FromEulerAnglesInDegrees(pitch, yaw, roll);
            }
        }

        protected Quaternion CameraOrientation {
            get {
                return cameraOrientation;
            }
            set {
                cameraOrientation = value;
            }
        }

        /// <summary>
        ///   Is the camera grabbed by a script?
        /// </summary>
        public bool CameraGrabbed {
            get {
                return cameraGrabbed;
            }
            set {
                cameraGrabbed = value;
            }
        }

        public float PlayerYaw {
            get {
                float pitch, yaw, roll;
                playerOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                return yaw;
            }
            set {
                float pitch, yaw, roll;
                playerOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                yaw = value;
                playerOrientation = Quaternion.FromEulerAnglesInDegrees(pitch, yaw, roll);
                if (log.IsDebugEnabled) {
                    log.DebugFormat("pre-player angles: [{0}, {1}, {2}]", pitch, yaw, roll);
                    playerOrientation.ToEulerAnglesInDegrees(out pitch, out yaw, out roll);
                    log.DebugFormat("post-player angles: [{0}, {1}, {2}]", pitch, yaw, roll);
                }
            }
        }

        public Quaternion PlayerOrientation {
            get {
                return playerOrientation;
            }
            set {
                playerOrientation = value;
            }
        }

		public float PlayerSpeed {
			get {
				return playerSpeed;
			}
            set {
                playerSpeed = value;
            }
		}

        public bool MouseLookLocked {
            get {
                return mouseLookLocked;
            }
            set {
                mouseLookLocked = value;
                UpdateCursorVisibility(IsMousePressed(MouseButtons.Left) || IsMousePressed(MouseButtons.Right));
            }
        }

        public Vector3 CameraTargetOffset {
            get {
                return cameraTargetOffset;
            }
            set {
                cameraTargetOffset = value;
            }
        }

        #endregion
	}
}
