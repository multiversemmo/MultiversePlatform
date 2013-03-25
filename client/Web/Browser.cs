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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Input;
using Axiom.RenderSystems.DirectX9;

using log4net;

namespace Multiverse.Web
{
    /// <summary>
    /// The Web.Browser lets you create a 2D web browser that overlays
    /// the rendering of the 3D scene like a user interface element.
    /// While there are scripting functions to work with the browser,
    /// most of the browser creation and destruction will happen in 
    /// the FrameXML.
    /// </summary>
    public class Browser : ContainerControl
    {
        #region Public API
        /// <summary>
        /// Create a new, empty, hidden web browser.  Use Open() to 
        /// load a URL.
        /// </summary>
        public Browser()
        {
            b = null;
            url = null;
            BackColor = DEFAULT_BACKGROUND;
            LocationChanged += new EventHandler(Browser_LocationChanged);
            SizeChanged += new EventHandler(Browser_SizeChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (b != null)
            {
                b = null;
            }
            Form f = FindForm();
            if (f != null)
            {
                f.Focus();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the URL of the currently loaded web page.  To go to 
        /// a new URL, use Open().
        /// </summary>
        public string URL { get { return url; } }

        /// <summary>
        /// Sets the location of the browser on the screen relative to the upper 
        /// left corner of the window.  The browser window will be clipped to 
        /// the client window if it goes offscreen.
        /// </summary>
        /// <param name="x">
        /// The X position of the browser, relative to the upper left corner 
        /// of the window.
        /// </param>
        /// <param name="y">
        /// The Y position of the browser, relative to the upper left corner
        /// of the window.  Note that this position starts at 0 and goes down,
        /// as opposed to the FrameXML, which starts from the bottom of the 
        /// window and goes up.
        /// </param>
        public void SetLocation(int x, int y)
        {
            BrowserLocation = new Point(x, y);
        }

        /// <summary>
        /// Sets the size of the browser window in pixels.
        /// </summary>
        /// <param name="w">
        /// The width of the window in pixels.
        /// </param>
        /// <param name="h">
        /// The height of the window in pixels.
        /// </param>
        public void SetSize(int w, int h)
        {
            BrowserSize = new Size(w, h);
        }

        /// <summary>
        /// Sets an empty space around the browser window, in pixels.  The
        /// browser window will position itself offset from the upper left
        /// corner by this border width.
        /// </summary>
        /// <param name="w">
        /// The size of the border in pixels.  The border will be added to 
        /// all four sides (the top, left, bottom, and right.)
        /// </param>
        public void SetBorder(int w)
        {
            BrowserBorder = w;
        }

        /// <summary>
        /// Draws a black border line around the browser window with this width.
        /// This line is applied after the border is applied, and is cumulative.
        /// </summary>
        /// <param name="w">
        /// The width of the line around the browser, in pixels.
        /// </param>
        public void SetLine(int w)
        {
            BrowserLine = w;
        }

        /// <summary>
        /// Whether or not the browser will create scrollbars if the display size
        /// is greater than the browser window size.  Note that plugins and Flash
        /// for example may still create scrollbars.
        /// </summary>
        /// <param name="enabled">
        /// Whether or not scrollbars are enabled.
        /// </param>
        public void EnableScrollbars(bool enabled)
        {
            BrowserScrollbars = enabled;
        }

        /// <summary>
        /// Whether or not the embedded browser will bring up dialog boxes when
        /// script errors occur.  This is useful for debugging but probably 
        /// should be turned off in production.
        /// </summary>
        /// <param name="enabled">
        /// Whether or not errors are enabled.
        /// </param>
        public void EnableErrors(bool enabled)
        {
            BrowserErrors = enabled;
        }

        /// <summary>
        /// Loads a new URL into the browser window.  Returns true if the action
        /// succeeded, or false if it didn't.  Note that embedded objects like
        /// Quicktime movies or loading Flash apps directly may require some 
        /// extra HTML to not generate an IE error.  
        /// </summary>
        /// <param name="u">
        /// The URL to open, for example, "http://www.multiverse.net"
        /// </param>
        /// <returns>
        /// Whether or not the URL was loaded.  Does not wait for the document
        /// to load, so it may display a 404.  (In fact, there aren't very many
        /// ways this can return false right now.)
        /// </returns>
        public bool Open(string u)
        {
            url = u;
            if (b == null)
            {
                SetupBrowser();
            }
            b.Navigate(URL);
            // XXXMLM - loading screen
            // XXXMLM - fullscreen is dodgy
            // XXXMLM - onclose make no warning, close control
            b.ScrollBarsEnabled = BrowserScrollbars;
            b.ScriptErrorsSuppressed = !BrowserErrors;
            b.AllowNavigation = true;
            // b.Focus();
            b.WebBrowserShortcutsEnabled = true;

            return true;
        }
        #endregion

        // Create a logger for use in this class
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Browser));

        public static int DEFAULT_BORDER = 50;
        public static int DEFAULT_LINE = 0;
        public static Color DEFAULT_BACKGROUND = Color.Black;
        private WebBrowser b;
		private Label dummy;  // dummy label used to 'unfocus' the browser
        private string url;
        private Point browserLocation = Point.Empty;
        private Size browserSize = Size.Empty;
        private int browserBorder = DEFAULT_BORDER;
        private int browserLine = DEFAULT_LINE;
        private bool browserScrollbars = true;
        private bool browserErrors = false;

        void Browser_LocationChanged(object sender, EventArgs e)
        {
            if (b != null)
            {
                PositionBrowser();
            }
        }

        void Browser_SizeChanged(object sender, EventArgs e)
        {
            if (b != null)
            {
                PositionBrowser();
            }
        }

        public static bool RegisterForScripting()
        {
            return true;
        }

        public Point BrowserLocation { get { return browserLocation; } set { browserLocation = value; } }
        public Size BrowserSize { get { return browserSize; } set { browserSize = value; } }
        public int BrowserBorder { get { return browserBorder; } set { browserBorder = value; } }
        public int BrowserLine { get { return browserLine; } set { browserLine = value; } }
        public bool BrowserScrollbars { get { return browserScrollbars; } set { browserScrollbars = value; } }
        public bool BrowserErrors { get { return browserErrors; } set { browserErrors = value; } }
        public object ObjectForScripting { get { return b.ObjectForScripting; } set { b.ObjectForScripting = value; } }

        public void SetupBrowser()
        {
            RenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;
            Control c = (Control)rw.GetCustomAttribute("HWND");

			// dummy label used to 'unfocus' the browser
			dummy = new Label();
            dummy.Size = new Size(1, 1);
            dummy.Location = new Point(1, 1);
			Controls.Add(dummy);
			dummy.CreateControl();

            b = new WebBrowser();
            PositionBrowser();
            Controls.Add(b);
            b.CreateControl();
			b.BringToFront();
            c.Controls.Add(this);

            CreateControl();
        }

        public void PositionBrowser()
        {
            int w, h;
            int x, y;
            if (BrowserSize != Size.Empty)
            {
                w = BrowserSize.Width;
                h = BrowserSize.Height;
            }
            else
            {
                RenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;
                Control c = (Control)rw.GetCustomAttribute("HWND");
                w = (c.Width) - (BrowserBorder * 2);
                h = (c.Height) - (BrowserBorder * 2);
            }
            if (BrowserLocation != Point.Empty)
            {
                x = BrowserLocation.X;
                y = BrowserLocation.Y;
            }
            else
            {
                RenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;
                Control c = (Control)rw.GetCustomAttribute("HWND");
                x = (c.Width - w) / 2;
                y = (c.Height - h) / 2;
            }
            Size = new Size(w, h);
            Location = new Point(x, y);
            b.Size = new Size(w - (BrowserLine * 2), h - (BrowserLine * 2));
            b.Location = new Point(BrowserLine, BrowserLine);
        }

		public void GetFocus() {
			b.BringToFront();
			b.Select();
		}
		public void Losefocus() {
			dummy.Select();
		}

        /// <summary>
        ///   Invoke a javascript method, using the browser control.
        /// </summary>
        /// <remarks>
        ///   This should only be called from the thread that created the 
        ///   browser control     
        /// </remarks>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object InvokeScript(string method, IEnumerable<object> args)
        {
            object[] scriptingArgs = GetScriptingArgs(args);
            lock (this)
            {
                try
                {
                    return b.Document.InvokeScript(method, scriptingArgs);
                }
                catch (Exception e)
                {
                    string formattedScriptCall = FormatScriptCall(method, scriptingArgs);
                    log.WarnFormat("Failed to invoke script: {0} -- {1}", formattedScriptCall, e);
                    throw;
                }
            }
        }

        /// <summary>
        ///   Convert the argument array in a form that is usable from javascript
        /// </summary>
        /// <returns></returns>
        protected static object[] GetScriptingArgs(IEnumerable<object> args)
        {
            // We put the arguments in a list
            List<object> argumentList = new List<object>();
            foreach (object obj in args)
            {
                if (obj is long)
                {
                    long val = (long)obj;
                    argumentList.Add((double)val);
                }
                else
                {
                    argumentList.Add(obj);
                }
            }
            return argumentList.ToArray();
        }

        /// <summary>
        ///   This is mostly intended to help with debugging.  If our script 
        ///   fails, we will use this to display the call.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected static string FormatScriptCall(string method, object[] args)
        {
            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("{0}(", method);
            for (int i = 0; i < args.Length; ++i)
            {
                if (i == (args.Length - 1))
                    msg.AppendFormat("'{0}'", args[i]);
                else
                    msg.AppendFormat("'{0}', ", args[i]);
            }
            msg.Append(")");
            return msg.ToString();
        }
    }
}
