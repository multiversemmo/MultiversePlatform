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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using log4net;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.Media;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Axiom.RenderSystems.DirectX9 {
	/// <summary>
	/// The Direct3D implementation of the RenderWindow class.
	/// </summary>
	public class D3DRenderWindow : RenderWindow {
        #region Fields
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(D3DRenderWindow));

        protected Driver driver;
        Control windowHandle;
        bool isExternal;

        // private D3D.Device device;
        protected bool isSwapChain;
		protected D3D.SwapChain swapChain;
        protected PresentParameters presentParams = new PresentParameters();
		/// <summary>Used to provide support for multiple RenderWindows per device.</summary>
        protected D3D.Surface renderSurface;
        /// <summary>Stencil buffer (or zbuffer)</summary>
        protected D3D.Surface renderZBuffer;
        protected MultiSampleType fsaaType;
        protected int fsaaQuality;
        protected int displayFrequency;
        protected bool isVSync;
        protected bool useNVPerfHUD;
        protected bool multiThreaded;
        protected string initialLoadBitmap;
        protected DefaultForm form = null;

        #endregion Fields

        public D3D.Surface RenderSurface {
            get {
                return renderSurface;
            }
        }

        public PresentParameters PresentationParameters {
            get {
                return presentParams;
            }
        }

		#region Constructor
	
		public D3DRenderWindow(Driver driver, bool isSwapChain) {
            this.driver = driver;
            this.isSwapChain = isSwapChain;

            fsaaType = MultiSampleType.None;
            fsaaQuality = 0;
            isFullScreen = false;
            isSwapChain = false;
            isExternal = false;
            windowHandle = null;
            isActive = false;
            displayFrequency = 0;
		}
	
		#endregion
	
		#region RenderWindow implementation
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colorDepth"></param>
		/// <param name="isFullScreen"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="depthBuffer"></param>height
		/// <param name="miscParams"></param>
		public override void Create(string name, int width, int height, bool isFullScreen, params object[] miscParams) {
            Control parentWindow = null;
            Control externalWindow = null;
 		    fsaaType = MultiSampleType.None;
            fsaaQuality = 0;
            isVSync = false;
            string title = name;
            int colorDepth = 32;
            int left = -1;
            int top = -1;
            bool depthBuffer = true;
            // Parameters that would have been set in the params list, but are not used
            // border, outerSize
            useNVPerfHUD = false;
            multiThreaded = false;

            Debug.Assert(miscParams.Length % 2 == 0);
            int index = 0;
            while (index < miscParams.Length) {
                string key = (string)miscParams[index++];
                object value = miscParams[index++];
                switch (key) {
                    case "left":
                        left = (int)value;
                        break;
                    case "top":
                        top = (int)value;
                        break;
                    case "title":
                        title = (string)value;
                        break;
                    case "parentWindow":
                        parentWindow = (Control)value;
                        break;
                    case "externalWindow":
                        externalWindow = (Control)value;
                        break;
                    case "vsync":
                        isVSync = (bool)value;
                        break;
                    case "displayFrequency":
                        displayFrequency = (int)value;
                        break;
                    case "colorDepth":
                    case "colourDepth":
                        colorDepth = (int)value;
                        break;
                    case "depthBuffer":
                        depthBuffer = (bool)value;
                        break;
                    case "FSAA":
                        fsaaType = (MultiSampleType)value;
                        break;
                    case "FSAAQuality":
                        fsaaQuality = (int)value;
                        break;
                    case "useNVPerfHUD":
                        useNVPerfHUD = (bool)value;
                        break;
                    case "multiThreaded":
                        multiThreaded = (bool)value;
                        break;
                    case "initialLoadBitmap":
                        initialLoadBitmap = (string)value;
                        break;
                    case "border":
                    case "outerDimensions":
                    default:
                        log.Warn("Option not yet implemented");
                        break;
                }
            }

	       
            if (windowHandle != null)
                Destroy();

            if (externalWindow == null) {
                this.width = width;
                this.height = height;
                this.top = top;
                this.left = left;
                FormBorderStyle borderStyle = FormBorderStyle.None;
                FormWindowState windowState = FormWindowState.Normal;
                if (!isFullScreen) {
                    // If RenderSystem.AllowResize is true, put a
                    // resize border on the window.
                    borderStyle = (Root.Instance.RenderSystem.AllowResize ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle);
                    windowState = FormWindowState.Normal;
                } else {
                    borderStyle = FormBorderStyle.None;
                    windowState = FormWindowState.Maximized;
                    this.top = 0;
                    this.left = 0;
                }
                isExternal = false;
                form = new DefaultForm(!isFullScreen, initialLoadBitmap);
                // Set these two to false, or else windows get created
                // with different dimensions that requesting in Width
                // and Height!
                log.InfoFormat("Initial form settings: AutoSize: {0}; AutoScale: {1}", form.AutoSize, form.AutoScaleMode);
                form.AutoSize = false;
                form.AutoScaleMode = AutoScaleMode.None;
                form.ClientSize = new System.Drawing.Size(width, height);
                // TODO: I should support the maximize box once I get resize working
                // form.MaximizeBox = true;
                form.MaximizeBox = false;
                form.MinimizeBox = true;
                form.Top = this.top;
                form.Left = this.left;
                form.FormBorderStyle = borderStyle;
                form.WindowState = windowState;
                form.Text = title;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.BringToFront();
                if (isFullScreen) {
                    form.TopMost = true;
                    form.TopLevel = true;
                    form.Width = width;
                    form.Height = height;
                }
                // form.Target.Visible = false;
                form.Show();
                // set the default form's renderwindow so it can access it internally
                form.RenderWindow = this;
                form.Activate();
                windowHandle = form.Target;
            } else {
                windowHandle = externalWindow;
                isExternal = true;

                this.top = windowHandle.Top;
                this.left = windowHandle.Left;
                System.Drawing.Rectangle rect = windowHandle.ClientRectangle;
                this.width = rect.Width;
                this.height = rect.Height;
            }
            windowHandle.Resize += this.OnExternalWindowEvent;
            windowHandle.Move += this.OnExternalWindowEvent;

            this.name = name;
            this.isDepthBuffered = depthBuffer;
            this.isFullScreen = isFullScreen;
            this.colorDepth = colorDepth;

            CreateD3DResources();
            isActive = true;
	
            // FIXME: These lines were not in Ogre, but are in Axiom.
            //D3D.Device device = driver.Device;
            // device.DeviceReset += new EventHandler(OnResetDevice);
            //this.OnResetDevice(device, null);
        }

		/// <summary>
		/// Specifies the custom attribute by converting this to a string and passing to GetCustomAttribute()
		/// </summary>
		// public enum CustomAttribute { D3DDEVICE, D3DZBUFFER, D3DBACKBUFFER }

        public void CreateD3DResources() {
            // access device via driver
		    Device device = driver.Device;

            if (isSwapChain && device == null) {
		        throw new Exception("Secondary window has not been given the device from the primary!");
    		}
        
            DeviceType devType = DeviceType.Hardware;

            presentParams = new PresentParameters();
            presentParams.Windowed = !isFullScreen;
			presentParams.SwapEffect = SwapEffect.Discard;
            // triple buffer if VSync is on
            presentParams.BackBufferCount = isVSync ? 2 : 1;
			presentParams.EnableAutoDepthStencil = isDepthBuffered;
			presentParams.DeviceWindow = windowHandle;
			presentParams.BackBufferWidth = width;
			presentParams.BackBufferHeight = height;
            presentParams.FullScreenRefreshRateInHz = isFullScreen ? displayFrequency : 0;
            
            if (isVSync) {
                presentParams.PresentationInterval = PresentInterval.One;
            } else {
                // NB not using vsync in windowed mode in D3D9 can cause jerking at low 
			    // frame rates no matter what buffering modes are used (odd - perhaps a
			    // timer issue in D3D9 since GL doesn't suffer from this) 
			    // low is < 200fps in this context
			    if (!isFullScreen)
                    log.Debug("Disabling VSync in windowed mode can cause timing issues at lower " +
					          "frame rates, turn VSync on if you observe this problem.");
        		presentParams.PresentationInterval = PresentInterval.Immediate;
            }
            
            presentParams.BackBufferFormat = Format.R5G6B5;
            if (colorDepth > 16)
                presentParams.BackBufferFormat = Format.X8R8G8B8;

            if (colorDepth > 16) {
                // Try to create a 32-bit depth, 8-bit stencil
                if (!D3D.Manager.CheckDeviceFormat(driver.AdapterNumber, devType,
                                                   presentParams.BackBufferFormat,
                                                   Usage.DepthStencil,
                                                   ResourceType.Surface, DepthFormat.D24S8)) {
                    // Bugger, no 8-bit hardware stencil, just try 32-bit zbuffer 
                    if (!D3D.Manager.CheckDeviceFormat(driver.AdapterNumber, devType,
                                                      presentParams.BackBufferFormat,
                                                      Usage.DepthStencil,
                                                      ResourceType.Surface, DepthFormat.D32)) {
                        // Jeez, what a naff card. Fall back on 16-bit depth buffering
                        presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                    } else
                        presentParams.AutoDepthStencilFormat = DepthFormat.D32;
                } else {
                    // Woohoo!
                    if (D3D.Manager.CheckDepthStencilMatch(driver.AdapterNumber, devType,
                                                           presentParams.BackBufferFormat,
                                                           presentParams.BackBufferFormat,
                                                           DepthFormat.D24S8)) {
                        presentParams.AutoDepthStencilFormat = DepthFormat.D24S8;
                    } else
                        presentParams.AutoDepthStencilFormat = DepthFormat.D24X8;
                }
            } else {
                // 16-bit depth, software stencil
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
            }

            presentParams.MultiSample = fsaaType;
            presentParams.MultiSampleQuality = fsaaQuality;
                 
            if (isSwapChain) {
			    swapChain = new SwapChain(device, presentParams);
                if (swapChain == null) {
           		    // Try a second time, may fail the first time due to back buffer count,
				    // which will be corrected by the runtime
                    swapChain = new SwapChain(device, presentParams);
                }
                // Store references to buffers for convenience
                renderSurface = swapChain.GetBackBuffer(0, BackBufferType.Mono);
     			// Additional swap chains need their own depth buffer
	    		// to support resizing them
                if (isDepthBuffered) {
                    renderZBuffer =
                        device.CreateDepthStencilSurface(width, height,
					                                     presentParams.AutoDepthStencilFormat,
					                                     presentParams.MultiSample,
					                                     presentParams.MultiSampleQuality,
					                                     false);
                } else {
                    renderZBuffer = null;
                }
                // Ogre releases the mpRenderSurface here (but not the mpRenderZBuffer)
                // release immediately so we don't hog them
			    // mpRenderSurface->Release();
			    // We'll need the depth buffer for rendering the swap chain
			    // //mpRenderZBuffer->Release();
            } else {
                if (device == null) {
#if !USE_D3D_EVENTS
                    // Turn off default event handlers, since Managed DirectX seems confused.
                    Device.IsUsingEventHandlers = false; 
#endif

        			// We haven't created the device yet, this must be the first time

                    // Do we want to preserve the FPU mode? Might be useful for scientific apps
                    CreateFlags extraFlags = 0;
                    if (multiThreaded) {
                        extraFlags |= CreateFlags.MultiThreaded;
                    }
                    // TODO: query and preserve the fpu mode
                    
                    // Set default settings (use the one Ogre discovered as a default)
                    int adapterToUse = driver.AdapterNumber;
                    if (useNVPerfHUD) {
                        // Look for 'NVIDIA NVPerfHUD' adapter
					    // If it is present, override default settings
					    foreach (AdapterInformation identifier in D3D.Manager.Adapters) {
                            log.Info("Device found: " + identifier.Information.Description);
                            if (identifier.Information.Description.Contains("PerfHUD")) {
                                log.Info("Attempting to use PerfHUD");
							    adapterToUse = identifier.Adapter;
							    devType = DeviceType.Reference;
							    break;
						    }
                        }
					}

                    try
                    {
                        device = new D3D.Device(adapterToUse, devType, windowHandle,
                                                CreateFlags.HardwareVertexProcessing | extraFlags,
                                                presentParams);
                    }
                    catch (Exception) {
                        log.Info("First device creation failed");
                        try
                        {
                            // Try a second time, may fail the first time due to back buffer count,
                            // which will be corrected down to 1 by the runtime
                            device = new D3D.Device(adapterToUse, devType, windowHandle,
                                                    CreateFlags.HardwareVertexProcessing | extraFlags,
                                                    presentParams);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                // Looks like we can't use HardwareVertexProcessing, so try Mixed
                                device = new D3D.Device(adapterToUse, devType, windowHandle,
                                                        CreateFlags.MixedVertexProcessing | extraFlags,
                                                        presentParams);
                            }
                            catch (Exception)
                            {
                                // Ok, one last try. Try software.  If this fails, just throw up.
                                device = new D3D.Device(adapterToUse, devType, windowHandle,
                                                        CreateFlags.SoftwareVertexProcessing | extraFlags,
                                                        presentParams);
                            }
                        }
                    }

                    // TODO: For a full screen app, I'm supposed to do this to prevent alt-tab 
                    //       from messing things up.
                    //device.DeviceResizing += new
                    //    System.ComponentModel.CancelEventHandler(this.CancelResize);

				}
                log.InfoFormat("Device constructed with presentation parameters: {0}", presentParams.ToString());
                
                // update device in driver
                driver.Device = device;
		    	// Store references to buffers for convenience
			    renderSurface = device.GetRenderTarget(0);
                renderZBuffer = device.DepthStencilSurface;
                // Ogre releases these here
    			// release immediately so we don't hog them
    			// mpRenderSurface->Release();
	    		// mpRenderZBuffer->Release();
            }
        }


        private void CancelResize(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
        }

		/// <summary>
		/// 
		/// </summary>
        public override void Dispose() {
            base.Dispose();
            Destroy();
        }

        public void DestroyD3DResources() {
            // FIXME: Ogre doesn't do this Dispose call here (just sets to null).
            if (renderSurface != null) {
                renderSurface.Dispose();
                renderSurface = null;
            }
            // renderSurface = null;

            if (isSwapChain) {
                if (renderZBuffer != null) {
                    renderZBuffer.Dispose();
                    renderZBuffer = null;
                }
                if (swapChain != null) {
                    swapChain.Dispose();
                    swapChain = null;
                }
            } else {
                // FIXME: Ogre doesn't do this Dispose call here (just sets to null).
                if (renderZBuffer != null) {
                    renderZBuffer.Dispose();
                    renderZBuffer = null;
                }
                // renderZBuffer = null;
            }
        }
        
        protected void Destroy() {
            DestroyD3DResources();

            if (windowHandle != null && !isExternal) {
                // if the control is a form, then close it
                Form form = windowHandle.FindForm();
                form.Close();
                form.Dispose();
            }
            windowHandle = null;
            // make sure this window is no longer active
            isActive = false;
        }

		public override void Reposition(int top, int left) {
            if (windowHandle != null && !isFullScreen) {
                Form form = windowHandle.FindForm();
                form.SetDesktopLocation(left, top);
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public override void Resize(int width, int height) {
            if (windowHandle != null && !isFullScreen) {
                windowHandle.Size = new System.Drawing.Size(width, height);
            }
        }

        public void OnExternalWindowEvent(object sender, EventArgs e) {
            WindowMovedOrResized();
        }

        public void WindowMovedOrResized() {
            log.Info("D3DRenderWindow.WindowMovedOrResized called.");
            if (windowHandle == null)
                return;
            Form form = windowHandle.FindForm();
            if (form != null) {
                if (form.WindowState == FormWindowState.Minimized)
                    return;

                this.top = form.DesktopLocation.Y;
                this.left = form.DesktopLocation.X;
            }

            if ((width == windowHandle.Size.Width) && (height == windowHandle.Size.Height))
                return;

            if (isSwapChain) {
				PresentParameters pp = new PresentParameters(presentParams);
                width = windowHandle.Size.Width > 0 ? windowHandle.Size.Width : 1;
                height = windowHandle.Size.Height > 0 ? windowHandle.Size.Height : 1;
                if (presentParams.Windowed)
                {
                    pp.BackBufferWidth = width;
                    pp.BackBufferHeight = height;
                }
                if (renderZBuffer != null) {
                    renderZBuffer.Dispose();
                    renderZBuffer = null;
                }
                if (swapChain != null) {
                    swapChain.Dispose();
                    swapChain = null;
                }
                if (renderSurface != null) {
                    renderSurface.Dispose();
                    renderSurface = null;
                }

                swapChain =  new SwapChain(driver.Device, pp);
                presentParams = pp;

                renderSurface = swapChain.GetBackBuffer(0, BackBufferType.Mono);
				renderZBuffer = 
                    driver.Device.CreateDepthStencilSurface(presentParams.BackBufferWidth, 
                                                            presentParams.BackBufferHeight,
                                                            presentParams.AutoDepthStencilFormat,
                                                            presentParams.MultiSample,
                                                            presentParams.MultiSampleQuality,
                                                            false);

                // TODO: Ogre releases here
                // renderSurface.Release();
			} else {
                // primary windows must reset the device
                width = windowHandle.Size.Width > 0 ? windowHandle.Size.Width : 1;
                height = windowHandle.Size.Height > 0 ? windowHandle.Size.Height : 1;
                if (presentParams.Windowed)
                {
                    presentParams.BackBufferWidth = width;
                    presentParams.BackBufferHeight = height;
                }
                D3D9RenderSystem renderSystem = (D3D9RenderSystem)Root.Instance.RenderSystem;
                renderSystem.NotifyDeviceLost();
            }
       		// Notify viewports of resize
            foreach (Axiom.Core.Viewport viewport in viewportList)
                viewport.UpdateDimensions();
		}

        /// <summary>
		/// 
		/// </summary>
		/// <param name="waitForVSync"></param>
		public override void SwapBuffers(bool waitForVSync) {
            D3D.Device device = driver.Device;
            if (device != null) {
                int status;
                // tests coop level to make sure we are ok to render
                device.CheckCooperativeLevel(out status);
                if (status == (int)Microsoft.DirectX.Direct3D.ResultCode.Success) {
                    // swap back buffer to the front
                    if (isSwapChain) {
                        swapChain.Present();
                    } else {
                        device.Present();
                    }
                } else if (status == (int)Microsoft.DirectX.Direct3D.ResultCode.DeviceLost) {
                    // Device is lost, and is not available for reset now
                    log.Warn("Device State: DeviceLost");
                    D3D9RenderSystem renderSystem = (D3D9RenderSystem)Root.Instance.RenderSystem;
                    renderSystem.NotifyDeviceLost();
                } else if (status == (int)Microsoft.DirectX.Direct3D.ResultCode.DeviceNotReset) {
                    // The device needs to be reset, and has not yet been reset.
                    log.Warn("Device State: DeviceNotReset");
                    device.Reset(device.PresentationParameters);
                } else {
                    throw new Exception(string.Format("Unknown status code from CheckCooperativeLevel: {0}", status));
                }
			}
		}

		public override object GetCustomAttribute(string attribute) {
			switch(attribute) {
				case "D3DDEVICE":
					return driver.Device;

				case "HWND":
                    return windowHandle;
			
	            case "isTexture":
                    return false;

                case "D3DZBUFFER":
                    return renderZBuffer;

                case "DDBACKBUFFER":
                    return renderSurface;

                case "DDFRONTBUFFER":
                    return renderSurface;
			}
            log.WarnFormat("There is no D3DRenderWindow custom attribute named {0}", attribute);
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsActive {
			get { 
				return isActive; 
			}
			set { 
				isActive = value;	
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool IsFullScreen {
			get {
				return base.IsFullScreen;
			}
		}

		/// <summary>
		///     Saves the window contents to a stream.
		/// </summary>
		/// <param name="stream">Stream to write the window contents to.</param>
        public override void Save(Stream stream, PixelFormat requestedFormat)
        {
            D3D.Device device = driver.Device;
            DisplayMode mode = device.DisplayMode;

            SurfaceDescription desc = new SurfaceDescription();
            desc.Width = mode.Width;
            desc.Height = mode.Height;
            desc.Format = Format.A8R8G8B8;

			// create a temp surface which will hold the screen image
            Surface surface = device.CreateOffscreenPlainSurface(
                mode.Width, mode.Height, Format.A8R8G8B8, Pool.SystemMemory);

			// get the entire front buffer.  This is SLOW!!
            device.GetFrontBufferData(0, surface);

			// if not fullscreen, the front buffer contains the entire desktop image.  we need to grab only the portion
			// that contains our render window
            if (!IsFullScreen) {
				// whatever our target control is, we need to walk up the chain and find the parent form
                Form form = windowHandle.FindForm();

				// get the actual screen location of the form
                System.Drawing.Rectangle rect = form.RectangleToScreen(form.ClientRectangle);

                desc.Width = width;
                desc.Height = height;
                desc.Format = Format.A8R8G8B8;

				// create a temp surface that is sized the same as our target control
                Surface tmpSurface = device.CreateOffscreenPlainSurface(rect.Width, rect.Height, Format.A8R8G8B8, Pool.Default);

				// copy the data from the front buffer to the window sized surface
                device.UpdateSurface(surface, rect, tmpSurface);

                // dispose of the prior surface
                surface.Dispose();

                surface = tmpSurface;
            }

            int pitch;

            // lock the surface to grab the data
            GraphicsStream graphStream = surface.LockRectangle(LockFlags.ReadOnly | LockFlags.NoSystemLock, out pitch);

            // create an RGB buffer
            byte[] buffer = new byte[width * height * 3];

            int offset = 0, line = 0, count = 0;

			// gotta copy that data manually since it is in another format (sheesh!)
            unsafe {
                byte* data = (byte*)graphStream.InternalData;

                for (int y = 0; y < desc.Height; y++) {
                    line = y * pitch;

                    for (int x = 0; x < desc.Width; x++) {
                        offset = x * 4;

                        int pixel = line + offset;

                        // Actual format is BRGA for some reason
                        buffer[count++] = data[pixel + 2];
                        buffer[count++] = data[pixel + 1];
                        buffer[count++] = data[pixel + 0];
                    }
                }
            }

            surface.UnlockRectangle();

            // dispose of the surface
            surface.Dispose();

			// gotta flip the image real fast
			Image image = Image.FromDynamicImage(buffer, width, height, PixelFormat.R8G8B8);
			image.FlipAroundX();

			// write the data to the stream provided
			stream.Write(image.Data, 0, image.Data.Length);
		}

        public override void Update() {
    		D3D9RenderSystem rs = (D3D9RenderSystem)Root.Instance.RenderSystem;

		    // access device through driver
		    D3D.Device device = driver.Device;

		    if (rs.DeviceLost)
		    {
                log.Info("In D3DRenderWindow.Update, rs.DeviceLost is true");
                // Test the cooperative mode first
                int status;
                device.CheckCooperativeLevel(out status);
                if (status == (int)Microsoft.DirectX.Direct3D.ResultCode.DeviceLost) {
        		    // device lost, and we can't reset
				    // can't do anything about it here, wait until we get 
				    // D3DERR_DEVICENOTRESET; rendering calls will silently fail until 
				    // then (except Present, but we ignore device lost there too)
                    // FIXME: Ogre doesn't do these two Dispose calls here.
#if NOT
                    // This code is what Ogre does for this clause, but since
                    // Ogre gets to immediately call release on the renderSurface
                    // and renderZBuffer, this assign of null will end up leaving 
                    // the reference count at 0 for them, and will cause them to
                    // be freed.  For the Axiom code, I'm just going to leave them
                    // alone, and do the proper dispose calls in the devicenotreset 
                    // clause.
                    
                    renderSurface = null;
				    // need to release if swap chain
                    if (!isSwapChain) {
                        renderZBuffer = null;
                    } else {
                        // Do I need to dispose of the ZBuffer here?
                        // SAFE_RELEASE (mpRenderZBuffer);
                        if (renderZBuffer != null) {
                            renderZBuffer.Dispose();
                            renderZBuffer = null;
                        }
                    }
#endif
                    Thread.Sleep(50);
				    return;
                } else {
                    if (status != (int)Microsoft.DirectX.Direct3D.ResultCode.Success &&
                        status != (int)Microsoft.DirectX.Direct3D.ResultCode.DeviceNotReset) {
                        // I've encountered some unexpected device state
                        // Ogre would just continue, but I want to make sure I notice this.
                        throw new Exception(string.Format("Unknown Device State: {0}", status));
                    }
                    // FIXME: Ogre doesn't do these two Dispose calls here.
                    if (renderSurface != null) {
                        log.Info("Disposing of render surface");
                        renderSurface.Dispose();
                        renderSurface = null;
                    }
                    if (renderZBuffer != null) {
                        log.Info("Disposing of render zbuffer");
                        renderZBuffer.Dispose();
                        renderZBuffer = null;
                    }

                    log.InfoFormat("In D3DRenderWindow.Update, calling rs.RestoreLostDevice(); status = {0}", status);
                    // device lost, and we can reset
                    rs.RestoreLostDevice();

                    // Still lost?
                    if (rs.DeviceLost) {
                        // Wait a while
                        Thread.Sleep(50);
                        return;
                    }

                    if (!isSwapChain) {
                        log.Info("In D3DRenderWindow.Update, re-querying buffers");
                        // re-qeuery buffers
                        renderSurface = device.GetRenderTarget(0);
                        renderZBuffer = device.DepthStencilSurface;
                        // release immediately so we don't hog them
                        // mpRenderSurface->Release();
                        // mpRenderZBuffer->Release();
                    } else {
                        log.Info("In D3DRenderWindow.Update, isSwapChain is true, changing viewport dimensions");
                        // Update dimensions incase changed
                        foreach (Axiom.Core.Viewport vp in viewportList)
                            vp.UpdateDimensions();
                        // Actual restoration of surfaces will happen in 
                        // D3D9RenderSystem.RestoreLostDevice when it calls
                        // CreateD3DResources for each secondary window
                    }
                }
		    }
            base.Update();
        }

		private void OnResetDevice(object sender, EventArgs e) {
			Device resetDevice = (Device)sender;

			// Turn off culling, so we see the front and back of the triangle
			resetDevice.RenderState.CullMode = Cull.None;
			// Turn on the ZBuffer
			resetDevice.RenderState.ZBufferEnable = true;
			resetDevice.RenderState.Lighting = true;    //make sure lighting is enabled
		}

        public override object Handle {
            get {
                return windowHandle;
            }
        }

        public override bool PictureBoxVisible {
            get {
                if (form != null && form.PictureBox != null)
                    return form.PictureBox.Visible;
                else
                    return false;
            }
            set {
                if (form != null && form.PictureBox != null)
                    form.PictureBox.Visible = value;
            }
        }
        
		#endregion
	}
}
