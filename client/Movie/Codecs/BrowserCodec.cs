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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.RenderSystems.DirectX9;
using Multiverse.Lib.LogUtil;
using Multiverse.Interface;

using D3D = Microsoft.DirectX.Direct3D;

namespace Multiverse.Movie.Codecs
{
    class BrowserCodec : Codec
    {
        public const string PARAM_TEXTURESIZE = "textureSize";
        public const string PARAM_VIDEOSIZE = "videoSize";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BrowserCodec));

        #region Codec methods
        public const string CodecName = "Browser";
        public override string Name()
        {
            return CodecName;
        }
        public override bool ValidateParameter(string name, string val)
        {
            switch (name)
            {
                case PARAM_TEXTURESIZE:
                case PARAM_VIDEOSIZE:
                    return true;
            }
            return base.ValidateParameter(name, val);
        }
        public override IMovieTexture CreateMovieTexture(string name)
        {
            return new D3DMovieTexture(name);
        }
        public override IMovie LoadFile(string name, string file, string textureName)
        {
            // XXXMLM - should this be allowed?
            return DoMovieLoad(name, file, textureName);
        }
        public override IMovie LoadStream(string name, string url, string textureName)
        {
            return DoMovieLoad(name, url, textureName);
        }
        #endregion

        private IMovie DoMovieLoad(string name, string url, string textureName)
        {
            BrowserMovie ans = new BrowserMovie();
            ans.Initialize(name, url, textureName);
            AddMovie(ans);
            return ans;
        }
    }

    class BrowserMovie : Movie
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(BrowserMovie));

        public Size DEFAULT_VIDEO_SIZE = new Size(256, 256);
        public Size DEFAULT_TEXTURE_SIZE = new Size(256, 256);
        
        Browser browser;
        //D3D.Surface d3dsurface;
        // The default scripting object.  It's a little tricky to construct a valid
        // object that we can use, so I have a dictionary object that can be used.
        private ScriptingDictionary defaultScriptingObj = new ScriptingDictionary();
        private object scriptingObj; 

        public BrowserMovie() : base()
        {
            browser = null;
            scriptingObj = defaultScriptingObj;
        }

        public bool Initialize(string n, string p, string t)
        {
            name = n;
            path = p;
            textureName = t;
            if (textureName == null)
            {
                textureName = Manager.TextureName(this);
            }
            log.InfoFormat("BrowserCodec[{0}] Initialize: {1} ({2}), texture '{3}'", ID(), name, path, textureName);

            // check for DirectX-ness
            if ((Root.Instance != null) && !(Root.Instance.RenderSystem is Axiom.RenderSystems.DirectX9.D3D9RenderSystem))
            {
                ps = PLAY_STATE.STOPPED;
                throw new Exception("Browser codec is DirectX only");
            }
            if ((Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow == null)
            {
                ps = PLAY_STATE.STOPPED;
                throw new Exception("Initializing before primary window has been created");
            }
            D3DRenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;
            Control c = (Control)rw.GetCustomAttribute("HWND");

            CreateTexture();

            Root.Instance.FrameStarted += new FrameEvent(FrameStarted);
            ps = PLAY_STATE.BUFFERING;
            return true;
        }

        void CreateTexture()
        {
            Size tsz = TextureSize();

            if (texture == null)
            {
                texture = TextureManager.Instance.GetByName(textureName);
            }
            if (texture != null)
            {
                log.InfoFormat("BrowserCodec[{0}]: Removing old texture \"{1}\"",
                    ID(), textureName);
                TextureManager.Instance.Remove(texture.Name);
                texture.Unload();
                texture = null;
                //d3dsurface = null;
            }
            texture = TextureManager.Instance.CreateManual(
                textureName,
                TextureType.TwoD,
                tsz.Width, tsz.Height,
                0, // no mip maps
                D3DHelper.ConvertEnum(D3D.Format.X8R8G8B8),
                TextureUsage.Dynamic);
            if (texture is D3DTexture)
            {
                D3D.Surface d3dsurface = ((texture as D3DTexture).DXTexture as D3D.Texture).GetSurfaceLevel(0);
                Graphics g = d3dsurface.GetGraphics();
                g.Clear(Color.Black);
                d3dsurface.ReleaseGraphics();
            }
        }

        int count = 0;
        Font loadingFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        PointF loadingPoint = new PointF(10, 10);
        void FrameStarted(object source, FrameEventArgs e)
        {
            try {
                if (texture == null || 
                    !(texture is D3DTexture) || 
                    ((texture as D3DTexture).DXTexture) == null ||
                    ((texture as D3DTexture).DXTexture as D3D.Texture) == null)
                    return;
                D3D.Surface d3dsurface = ((texture as D3DTexture).DXTexture as D3D.Texture).GetSurfaceLevel(0);
                if (ps == PLAY_STATE.BUFFERING)
                {
                    if (browser == null)
                    {
                        browser = new Browser();
                        browser.SetObjectForScripting(scriptingObj);
                        browser.SetSize(VideoSize());
                        CreateTexture();
                        bool ans = browser.Open(path);
                        if (ans == false)
                        {
                            ps = PLAY_STATE.STOPPED;
                            browser = null;
                        }
                    }

                    if (browser.Loaded())
                    {
                        Play();
                    }
                    else
                    {
                        Graphics g = d3dsurface.GetGraphics();
                        string text;
                        int pct = browser.LoadPercent();
                        if ((pct == 100) || (pct == 0))
                        {
                            text = string.Format("Loading ({0}%)", pct);
                        }
                        else
                        {
                            text = string.Format("Loading ({0:d2}%)", pct);
                        }
                        switch ((count++ / 10) % 3)
                        {
                            case 0:
                                text += ".";
                                break;
                            case 1:
                                text += "..";
                                break;
                            case 2:
                                text += "...";
                                break;
                        }
                        g.Clear(Color.Black);
                        g.DrawString(text, loadingFont, Brushes.White, loadingPoint);
                        d3dsurface.ReleaseGraphics();
                    }
                }
                if(ps == PLAY_STATE.RUNNING)
                {
                    Graphics g = d3dsurface.GetGraphics();
                    browser.RenderTo(g);
                    d3dsurface.ReleaseGraphics();
                }
            }
            catch (Exception exc) {
                log.WarnFormat("BrowserMovie.FrameStarted exception: {0}, stack trace {1}",
                    exc.Message, exc.StackTrace);
            }
        }

        #region IMovie methods
        public override string CodecName()
        {
            return BrowserCodec.CodecName;
        }
        public override Size VideoSize() 
        {
            if (videoSize != Size.Empty)
            {
                return videoSize;
            }
            else if (textureSize != Size.Empty)
            {
                return textureSize;
            }
            else
            {
                return DEFAULT_VIDEO_SIZE;
            }
        }
        public override Size TextureSize()
        {
            if (textureSize != Size.Empty)
            {
                return textureSize;
            }
            else if (videoSize != Size.Empty)
            {
                Size ans = new Size(Manager.NextPowerOfTwo(videoSize.Width), Manager.NextPowerOfTwo(videoSize.Height));
                return ans;
            }
            else
            {
                return DEFAULT_TEXTURE_SIZE;
            }
        }
        public override void Unload()
        {
            log.InfoFormat("BrowserCodec[{0}]: unload", ID());
            Stop();
            if (Root.Instance != null)
            {
                Root.Instance.FrameStarted -= new FrameEvent(FrameStarted);
            }
            if (browser != null)
            {
                if (browser.IsHandleCreated)
                {
                    browser.Close();
                }
                browser = null;
            }
            base.Unload();
            return; 
        }
        public override bool SetParameter(string name, string val)
        {
            switch(name)
            {
                case BrowserCodec.PARAM_TEXTURESIZE:
                    try
                    {
                        string[] parsed = val.Split(new char[] { 'x' }, 2);
                        int width = Int32.Parse(parsed[0]);
                        int height = Int32.Parse(parsed[1]);
                        SetTextureSize(new Size(width, height));
                        return true;
                    }
                    catch (Exception e)
                    {
                        LogUtil.ExceptionLog.ErrorFormat("Failed to set texture size on movie '{0}' (tried to set '{1}')", Name(), val);
                        LogUtil.ExceptionLog.Error(e.ToString());
                        LogUtil.ExceptionLog.Error("Format is WIDTHxHEIGHT (for example, 640x480)");
                        return false;
                    }
                case BrowserCodec.PARAM_VIDEOSIZE:
                    try
                    {
                        string[] parsed = val.Split(new char[] { 'x' }, 2);
                        int width = Int32.Parse(parsed[0]);
                        int height = Int32.Parse(parsed[1]);
                        SetVideoSize(new Size(width, height));
                        return true;
                    }
                    catch (Exception e)
                    {
                        LogUtil.ExceptionLog.ErrorFormat("Failed to set video size on movie '{0}' (tried to set '{1}')", Name(), val);
                        LogUtil.ExceptionLog.Error(e.ToString());
                        LogUtil.ExceptionLog.Error("Format is WIDTHxHEIGHT (for example, 640x480)");
                        return false;
                    }
            }
            return base.SetParameter(name, val);
        }
        #endregion

        /// <summary>
        ///   This method lets us invoke a javascript method in the browser control.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object InvokeScript(string method, IEnumerable<object> args)
        {
            return browser.InvokeScript(method, args);
        }

        public void SetObjectForScripting(object obj)
        {
            scriptingObj = obj;
            if (browser != null)
                browser.SetObjectForScripting(obj);
        }

        public object GetObjectForScripting()
        {
            return scriptingObj;
        }

        #region Size methods
        public bool SetVideoSize(Size sz)
        {
            log.InfoFormat("BrowserCodec[{0}]: SetVideoSize {1}x{2}", ID(), sz.Width, sz.Height);
            videoSize = sz;
            if (browser != null)
            {
                browser.SetSize(sz);
            }
            CreateTexture();
            return true;
        }
        public bool SetTextureSize(Size sz)
        {
            log.InfoFormat("BrowserCodec[{0}]: SetTextureSize {1}x{2}", ID(), sz.Width, sz.Height);
            textureSize = sz;
            CreateTexture();
            return true;
        }
        #endregion
    }

    class Browser : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Browser));

        protected override bool ShowWithoutActivation { get { return true; } }

        WebBrowser b;
        string url;
        bool loaded;
        int loadpct;
        public Browser()
        {
            FormWindowState fs = FormWindowState.Normal;
            FormBorderStyle fb = FormBorderStyle.SizableToolWindow;
            FormBorderStyle = fb;
            WindowState = fs;
            ShowInTaskbar = false;
            Enabled = false;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            url = null;
            loaded = false;
            loadpct = 0;
            StartPosition = FormStartPosition.Manual;
            DesktopLocation = new Point(SystemInformation.VirtualScreen.Right, SystemInformation.VirtualScreen.Bottom);
            CreateBrowser();
        }

        public void CreateBrowser()
        {
            b = new WebBrowser();
            b.Location = new Point(0, 0);
            b.ProgressChanged += new WebBrowserProgressChangedEventHandler(OnProgressChanged);
            b.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnDocumentCompleted);
            b.ScrollBarsEnabled = false;
            b.WebBrowserShortcutsEnabled = false;
            b.IsWebBrowserContextMenuEnabled = false;
            b.ScriptErrorsSuppressed = true;
        }

        public void SetSize(Size sz)
        {
            ClientSize = sz;
            b.Size = sz;
            b.Location = new Point(0, 0);
        }

        public bool Open(string u)
        {
            Controls.Add(b);
            b.CreateControl();
            CreateControl();
            Show();
            url = u;
            loaded = false;
            loadpct = 0;
            b.Navigate(url);
            return true;
        }

        public void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            log.InfoFormat("DocumentCompleted: {0}", url);
            loaded = true;
        }

        void OnProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress != -1L)
            {
                if (e.CurrentProgress > e.MaximumProgress)
                {
                    loadpct = 100;
                }
                else
                {
                    loadpct = (int)(((float)e.CurrentProgress / (float)e.MaximumProgress) * 100);
                }
            }
        }

        public bool Loaded()
        {
            return loaded;
        }

        public int LoadPercent()
        {
            return loadpct;
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
        ///   This is currently only invoked by the BrowserMovie code,
        ///   when it initializes a Browser object.
        /// </summary>
        /// <param name="obj"></param>
        public void SetObjectForScripting(object obj)
        {
            b.ObjectForScripting = obj;
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

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);
        const int PW_CLIENTONLY = 0x00000001;

        public void RenderTo(Graphics target)
        {
            Graphics g = target;
            IntPtr h = g.GetHdc();
            if (!PrintWindow(b.Handle, h, 0))
            {
                log.Warn("Print failed");
            }
            g.ReleaseHdc();
            //bm.Save(string.Format("c:\\cygwin\\home\\mlm\\tmp\\ss\\test{0:x4}.bmp", count++));
        }
    }
}
