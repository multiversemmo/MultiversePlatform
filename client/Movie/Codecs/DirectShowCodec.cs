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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using DirectShowLib;

using Microsoft.DirectX;
using Microsoft.DirectX.PrivateImplementationDetails;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;
using Microsoft.Win32;

using Multiverse.Base;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.RenderSystems.DirectX9;
using Multiverse.Lib.LogUtil;

namespace Multiverse.Movie.Codecs
{
    public class DirectShowCodec : Codec, IWindowTarget
    {
        public const string PARAM_LOOPING = "looping";
        public const string PARAM_BALANCE = "balance";
        public const string PARAM_VOLUME = "volume";

        public const int WM_GRAPHNOTIFY  = 0x4000 + 0x123;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DirectShowCodec));

        #region ICodec methods
        public const string CodecName = "DirectShow";
        public override string Name() { return CodecName; }

        public static IntPtr wndHandle = IntPtr.Zero;

        public override IMovieTexture CreateMovieTexture(string name)
        {
            return new D3DMovieTexture(name);
        }
        public override void Start()
        {
            log.InfoFormat("DirectShowCodec Start");
            base.Start();
            Root.Instance.FrameStarted += new FrameEvent(OnFrameStarted);
        }

        public override void Stop()
        {
            Root.Instance.FrameStarted -= new FrameEvent(OnFrameStarted);
            base.Stop();
            DefaultForm.RemoveWndOverride(WM_GRAPHNOTIFY);
            log.Info("DirectShowCodec Stop");
        }

        public override bool ValidateParameter(string name, string value)
        {
            switch(name)
            {
                case PARAM_LOOPING:
                case PARAM_BALANCE:
                case PARAM_VOLUME:
                    return true;
                default:
                    return base.ValidateParameter(name, value);
            }
        }

        public override IMovie LoadFile(string name, string file, string textureName)
        {
            string path = Manager.ResolveMovieFile(file);
            if (path == null)
            {
                if (ShouldStream(file))
                {
                    return LoadStream(name, file, textureName);
                }
                else
                {
                    log.ErrorFormat("Failed to find movie named '{0}' ('{1}'), make sure it's in the Movies directory",
                                    name, file);
                    return null;
                }
            }
            return DoMovieLoad(name, path, textureName);
        }

        public override IMovie LoadStream(string name, string url, string textureName)
        {
            return DoMovieLoad(name, url, textureName);
        }

        private IMovie DoMovieLoad(string name, string path, string textureName)
        {
            DirectShowMovie movie;
            movie = new DirectShowMovie();
            try
            {
                movie.Initialize(name, path, textureName);

                // Add a notification handler (see WndProc)
                if (!DefaultForm.HasWndOverride(WM_GRAPHNOTIFY))
                {
                    DefaultForm.AddWndOverride(WM_GRAPHNOTIFY, this);
                }
                int hr = movie.AttachToWindow(wndHandle, WM_GRAPHNOTIFY);
                DsError.ThrowExceptionForHR(hr);
            } 
            catch(Exception e)
            {
                LogUtil.ExceptionLog.ErrorFormat("Failed to play movie '{0}'", name);
                LogUtil.ExceptionLog.Error(e.ToString());
                return null;
            }
            AddMovie(movie);
            return movie;
        }
        #endregion

        public static bool ShouldStream(string url)
        {
            return
                (url.StartsWith("http://") ||
                 url.StartsWith("https://")) &&
                ((System.IO.Path.GetExtension(url) == ".wmv") ||
                 (System.IO.Path.GetExtension(url) == ".wma") ||
                 (System.IO.Path.GetExtension(url) == ".asf") ||
                 (System.IO.Path.GetExtension(url) == ".mp3"));
        }

        #region Frame Events
        protected void OnFrameStarted(object source, FrameEventArgs evargs)
        {
            try {
                if (wndHandle == IntPtr.Zero)
                {
                    RenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;
                    if (rw != null)
                    {
                        Control c = (Control)rw.GetCustomAttribute("HWND");
                        if (!c.InvokeRequired)
                        {
                            wndHandle = c.Handle;
                        }
                        c.Resize += new EventHandler(c_Resize);
                    }
                }
                if (MovieList.Count != 0) 
                {
                    foreach (DictionaryEntry e in MovieList)
                    {
                        DirectShowMovie movie = (e.Value as DirectShowMovie);
                        movie.CheckProgress();
                    }
                } else
                if (SavedMovieList != null)
                {
                    if (SavedMovieFrameCount-- == 0)
                    {
                        int status;
                        RenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;
                        Device device = (Device)rw.GetCustomAttribute("D3DDEVICE");
                        device.CheckCooperativeLevel(out status);
                        if (status == (int)D3D.ResultCode.Success)
                        {
                            RestoreMovies();
                            SavedMovieFrameCount = SavedMovieFramePause;
                        }
                    }
                }
            }
            catch (Exception e) {
                log.ErrorFormat("DirectShowCodec.OnFrameStarted: Exception {0}, stack trace: {1}", e.Message, new StackTrace(true));
            }
        }

        private class SavedMovieData
        {
            public string Name;
            public string Path;
            public string Texture;
            public int Volume;
            public int Balance;
            public bool Looping;
            public SavedMovieData(
                string name, string path, string texture, 
                int volume, int balance, bool looping)
            {
                Name = name;
                Path = path;
                Texture = texture;
                Volume = volume;
                Balance = balance;
                Looping = looping;
            }
        }
        private IDictionary SavedMovieList = null;
        private const int SavedMovieFramePause = 1;
        private int SavedMovieFrameCount = SavedMovieFramePause;

        void c_Resize(object sender, EventArgs evt)
        {
            if (MovieList.Count != 0)
            {
                SavedMovieList = new Hashtable();
                foreach (DictionaryEntry e in MovieList)
                {
                    DirectShowMovie movie = (e.Value as DirectShowMovie);
                    SavedMovieData md = new SavedMovieData(
                        movie.Name(), movie.Path(), movie.TextureName(), 
                        movie.GetVolume(), movie.GetBalance(), movie.GetLooping());
                    SavedMovieList.Add(movie.ID(), md);
                }
            }
            UnloadAll();
        }

        void RestoreMovies()
        {
            if (SavedMovieList != null)
            {
                foreach (DictionaryEntry e in SavedMovieList)
                {
                    SavedMovieData md = (e.Value as SavedMovieData);
                    DirectShowMovie m = DoMovieLoad(md.Name, md.Path, md.Texture) as DirectShowMovie;
                    if (m != null)
                    {
                        m.SetVolume(md.Volume);
                        m.SetBalance(md.Balance);
                        m.SetLooping(md.Looping);
                    }
                }
                SavedMovieList = null;
            }
        }
        #endregion

        #region IWindowTarget methods
        public void OnHandleChange(System.IntPtr p)
        {
        }
        public void OnMessage(ref Message m)
        {
            if (m.Msg == WM_GRAPHNOTIFY)
            {
                int movieID = (int)m.LParam;
                if (MovieList.Contains(movieID) && MovieList[movieID] is DirectShowMovie)
                    (MovieList[movieID] as DirectShowMovie).GetEvents();
            }
        }
        #endregion
    }

    public class DirectShowMovie : Movie
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DirectShowMovie));

        // DirectShow objects
        private IFilterGraph2 graphBuilder;
        private DsROTEntry rot;
        private IBaseFilter vmr9;
        public DirectShowAllocator allocator;
        private DirectShowStreamer streamer;

        // Microsoft.DirectX.Direct3D.PresentFlag.Video?

        private bool looping;

        public DirectShowMovie() : base()
        {
            looping = false;
            allocator = null;
            streamer = null;
        }

        #region IMovie methods
        public override string CodecName()
        {
            return DirectShowCodec.CodecName;
        }
        public override Size VideoSize()
        {
            if (allocator != null)
            {
                return allocator.VideoSize;
            }
            throw new Exception("DirectShowCodec asked for video size before initialization");
        }
        public override Size TextureSize()
        {
            if (allocator != null)
            {
                return allocator.TextureSize;
            }
            throw new Exception("DirectShowCodec asked for texture size before initialization");
        }
        public override Axiom.Core.Texture Texture()
        {
            if (allocator != null)
            {
                return allocator.Texture;
            }
            else
            {
                return null;
            }
        }

        public override bool Play()
        {
            lock (this)
            {
                try
                {
                    log.InfoFormat("DirectShowCodec[{0}]: start play", ID());
                    int hr = (graphBuilder as IMediaControl).Run();
                    DsError.ThrowExceptionForHR(hr);
                }
                catch (Exception e)
                {
                    LogUtil.ExceptionLog.ErrorFormat("DirectShowCodec[{0}]: Error on play", ID()); 
                    LogUtil.ExceptionLog.Error(e.ToString());
                    return false;
                }
                log.InfoFormat("DirectShowCodec[{0}]: play started", ID()); 
                return base.Play();
            }
        }

        public override bool Pause()
        {
            lock (this)
            {
                try
                {
                    log.InfoFormat("DirectShowCodec[{0}]: pause", ID());
                    int hr = (graphBuilder as IMediaControl).Pause();
                    DsError.ThrowExceptionForHR(hr);
                }
                catch (Exception e)
                {
                    LogUtil.ExceptionLog.ErrorFormat("DirectShowCodec[{0}]: Error on pause", ID());
                    LogUtil.ExceptionLog.Error(e.ToString());
                    return false;
                }
                return base.Pause();
            }
        }

        public override bool Stop()
        {
            lock (this)
            {
                try
                {
                    log.InfoFormat("DirectShowCodec[{0}]: stop", ID());
                    int hr = (graphBuilder as IMediaControl).StopWhenReady();
                    DsError.ThrowExceptionForHR(hr);
                }
                catch (Exception e)
                {
                    LogUtil.ExceptionLog.ErrorFormat("DirectShowCodec[{0}]: Error on stop", ID());
                    LogUtil.ExceptionLog.Error(e.ToString());
                    return false;
                }
                return base.Stop();
            }
        }

        public override void Unload()
        {
            lock (this)
            {
                int hr = 0;

                log.InfoFormat("DirectShowCodec[{0}]: unload", ID());

                // Remove the ROT entry
                if (rot != null)
                {
                    rot.Dispose();
                }

                if (graphBuilder != null)
                {
                    // Stop DirectShow notifications
                    hr = (graphBuilder as IMediaEventEx).SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);

                    // Stop the graph
                    hr = (graphBuilder as IMediaControl).StopWhenReady();
                    hr = (graphBuilder as IMediaControl).Stop();

                    // Dispose the allocator
                    allocator.Dispose();
                    allocator = null;

                    // Release DirectShow objects
                    Marshal.ReleaseComObject(vmr9);
                    Marshal.ReleaseComObject(graphBuilder);
                    graphBuilder = null;
                }

                if (streamer != null)
                {
                    streamer.Shutdown();
                    streamer = null;
                }
                base.Unload();
            }
        }

        public override bool SetParameter(string name, string val)
        {
            switch(name)
            {
                case DirectShowCodec.PARAM_LOOPING:
                    SetLooping(val.ToLower() == "true");
                    return true;
                case DirectShowCodec.PARAM_VOLUME:
                    try
                    {
                        int vol = Int32.Parse(val);
                        SetVolume(vol);
                        return true;
                    }
                    catch (Exception e)
                    {
                        LogUtil.ExceptionLog.ErrorFormat("Failed to set volume on movie '{0}' (tried to set '{1}')", Name(), val);
                        LogUtil.ExceptionLog.Error(e.ToString());
                        return false;
                    }
                case DirectShowCodec.PARAM_BALANCE:
                    try
                    {
                        int bal = Int32.Parse(val);
                        SetBalance(bal);
                        return true;
                    }
                    catch (Exception e)
                    {
                        LogUtil.ExceptionLog.ErrorFormat("Failed to set balance on movie '{0}' (tried to set '{1}')", Name(), val);
                        LogUtil.ExceptionLog.Error(e.ToString());
                        return false;
                    }
            }
            return base.SetParameter(name, val);
        }
        #endregion

        private int Volume = 100;
        internal int GetVolume() { return Volume; }
        public bool SetVolume(int volume)
        {
            if ((volume < 0) || (volume > 100))
            {
                log.ErrorFormat("DirectShowCodec[{0}]: Cannot set volume to {1} (must be 0-100%)", ID(), volume);
                return false;
            }
            lock (this)
            {
                try
                {
                    double val = -10000f;
                    if (volume != 0)
                    {
                        // er, this is slightly off at the 1 value, but seems otherwise fine
                        val = (Math.Log10(volume / 100f) * 5000f);
                    }
                    log.InfoFormat("DirectShowCodec[{0}]: Set volume to {1}", ID(), (int)val);
                    (graphBuilder as IBasicAudio).put_Volume((int)val);
                }
                catch (Exception e)
                {
                    LogUtil.ExceptionLog.ErrorFormat("DirectShowCodec[{0}]: Caught exception on volume change: {1}", ID(), e.ToString());
                    return false;
                }
            }
            return true;
        }

        private int Balance = 0;
        internal int GetBalance() { return Balance; }
        public bool SetBalance(int balance)
        {
            if ((balance < -100) || (balance > 100))
            {
                log.ErrorFormat("DirectShowCodec[{0}]: Cannot set balance to {1} (must be -100-100%)", ID(), balance);
                return false;
            }
            lock (this)
            {
                try
                {
                    double val = 0f;
                    if (balance != 0)
                    {
                        // er, this is slightly off at the 1 value, but seems otherwise fine
                        val = (Math.Log10(Math.Abs(balance) / 100f) * 5000f);
                        if (balance > 0)
                        {
                            val *= -1.0f;
                        }
                    }
                    log.InfoFormat("DirectShowCodec[{0}]: Set balance to {1}", ID(), (int)val);
                    (graphBuilder as IBasicAudio).put_Balance((int)val);
                    Balance = balance;
                }
                catch (Exception e)
                {
                    LogUtil.ExceptionLog.ErrorFormat("DirectShowCodec[{0}]: Caught exception on balance change: {1}", ID(), e.ToString());
                    return false;
                }
            }
            return true;
        }
        internal bool GetLooping() { return looping; }
        public bool SetLooping(bool loop)
        {
            looping = loop;
            log.InfoFormat("DirectShowCodec[{0}]: Set looping to {1}", ID(), looping);
            return true;
        }

        public int AttachToWindow(IntPtr handle, int id)
        {
            return (graphBuilder as IMediaEventEx).SetNotifyWindow(
                handle,
                id,
                new IntPtr(ID()));
        }

        public bool Initialize(string n, string p, string t)
        {
            int hr = 0;

            path = p;
            name = n;
            textureName = t;
            if (textureName == null)
            {
                textureName = Manager.TextureName(this);
            }
            log.InfoFormat("DirectShowCodec[{0}] Initialize: {1} ({2}), texture '{3}'", ID(), name, path, textureName);

            // check for DirectX-ness
            if (!(Root.Instance.RenderSystem is Axiom.RenderSystems.DirectX9.D3D9RenderSystem))
            {
                throw new Exception("DirectShow movie codec is DirectX only");
            }
            if ((Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow == null)
            {
                throw new Exception("Initializing before primary window has been created");
            }
            D3DRenderWindow rw = (Root.Instance.RenderSystem as D3D9RenderSystem).PrimaryWindow;

            // Create a DirectShow FilterGraph
            graphBuilder = (IFilterGraph2)new FilterGraph();

            // Add it in ROT for debug purpose
            rot = new DsROTEntry(graphBuilder);

            // Create a VMR9 object
            vmr9 = (IBaseFilter)new VideoMixingRenderer9();

            IVMRFilterConfig9 filterConfig = (IVMRFilterConfig9)vmr9;

            // We want the Renderless mode!
            hr = filterConfig.SetRenderingMode(VMR9Mode.Renderless);
            DsError.ThrowExceptionForHR(hr);

            // One stream is enough for this sample
            hr = filterConfig.SetNumberOfStreams(1);
            DsError.ThrowExceptionForHR(hr);

            // Create the Allocator / Presenter object
            Device device = (Device)rw.GetCustomAttribute("D3DDEVICE");
            allocator = new DirectShowAllocator(device, ID(), TextureName());

            IVMRSurfaceAllocatorNotify9 vmrSurfAllocNotify = (IVMRSurfaceAllocatorNotify9)vmr9;

            // Notify the VMR9 filter about our allocator
            hr = vmrSurfAllocNotify.AdviseSurfaceAllocator(IntPtr.Zero, allocator);
            DsError.ThrowExceptionForHR(hr);

            // Notify our allocator about the VMR9 filter
            hr = allocator.AdviseNotify(vmrSurfAllocNotify);
            DsError.ThrowExceptionForHR(hr);

            IVMRMixerControl9 mixerControl = (IVMRMixerControl9)vmr9;

            // Select the mixer mode : YUV or RGB
            hr = mixerControl.SetMixingPrefs(VMR9MixerPrefs.RenderTargetYUV | VMR9MixerPrefs.NoDecimation | VMR9MixerPrefs.ARAdjustXorY | VMR9MixerPrefs.BiLinearFiltering);
            DsError.ThrowExceptionForHR(hr);

            // Add the VMR-9 filter to the graph
            hr = graphBuilder.AddFilter(vmr9, "Video Mixing Renderer 9");
            DsError.ThrowExceptionForHR(hr);

            bool shouldStream = false;
            string url = path;
#if false
            int maxRedirects = DirectShowWebReader.MAX_REDIRECTS;
            DirectShowWebReader reader = null;
            while (maxRedirects != 0)
            {
                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    try
                    {
                        // Start reading the data
                        reader = new DirectShowWebReader();
                        hr = reader.Load(url);
                        DsError.ThrowExceptionForHR(hr);
                        shouldStream = true;
                        break;
                    }
                    catch (DirectShowWebReader.RedirectException e)
                    {
                        log.Info("Redirect to (" + e.url + ")");
                        url = e.url;
                        maxRedirects--;
                        reader = null;
                    }
                }
                else
                {
                    shouldStream = false;
                    reader = null;
                    break;
                }
            }
            if (maxRedirects == 0)
            {
                log.Warn("Stopped redirection after " + DirectShowWebReader.MAX_REDIRECTS + " attempts.");
            }
#else
            shouldStream = DirectShowCodec.ShouldStream(url);
#endif
            if(shouldStream)
            {
#if false
                // Add our source filter to the graph
                DirectShowStreamer strm = new DirectShowStreamer(reader);
                hr = graphBuilder.AddFilter(strm, DirectShowStreamer.FILTER_NAME);
                DsError.ThrowExceptionForHR(hr);
                streamer = strm;
                streamer.SetType(reader.MajorType, reader.MinorType);

                // Render the file
                hr = graphBuilder.Render(strm.GetOutputPin());
                DsError.ThrowExceptionForHR(hr);
#else
                WMAsfReader asf = new WMAsfReader();
                IBaseFilter ibf = (asf as IBaseFilter);
                IFileSourceFilter ifs = (asf as IFileSourceFilter);
                hr = ifs.Load(url, IntPtr.Zero);
                DsError.ThrowExceptionForHR(hr);

                // XXXMLM - how to set buffer time?
                // IWMStreamConfig2 on the output pins?
                
                hr = graphBuilder.AddFilter(ibf, "WM ASF Reader");
                DsError.ThrowExceptionForHR(hr);

                IEnumPins ie;
                IPin[] pins = new IPin[1];
                hr = ibf.EnumPins(out ie);
                int fetched = 0;
                while (hr == 0)
                {
                    hr = ie.Next(1, pins, out fetched);
                    if (fetched != 0)
                    {
                        hr = graphBuilder.Render(pins[0]);
                        DsError.ThrowExceptionForHR(hr);
                    }
                }
#endif
            }
            else
            {
                // Render the file
                hr = graphBuilder.RenderFile(url, null);
                DsError.ThrowExceptionForHR(hr);
            }

#if true
            // Run the graph
            hr = (graphBuilder as IMediaControl).Run();
            DsError.ThrowExceptionForHR(hr);

            // Wait for ready
            hr = (graphBuilder as IMediaControl).Pause();
            DsError.ThrowExceptionForHR(hr);

            ps = PLAY_STATE.BUFFERING;
#endif
            return true;
        }

        public void GetEvents()
        {
            lock (this)
            {
                IMediaEventEx eventEx = (IMediaEventEx)graphBuilder;

                EventCode evCode;
                int param1, param2;

                while (eventEx.GetEvent(out evCode, out param1, out param2, 0) == 0)
                {
                    if (evCode == EventCode.StErrStPlaying)
                    {
                        long cur, stop, early, late, length;
                        (graphBuilder as IMediaSeeking).GetPositions(out cur, out stop);
                        (graphBuilder as IMediaSeeking).GetAvailable(out early, out late);
                        (graphBuilder as IMediaSeeking).GetDuration(out length);
                        log.WarnFormat("DirectShowCodec[{0}] Buffering: {1}, {2}, {3}, {4}, {5} ((6})",
                                       ID(), cur, stop, early, late, length, DsError.GetErrorText(param1));
                        if ((cur != stop) && (late != length))
                        {
                            Pause();
                            ps = PLAY_STATE.BUFFERING;
                        }
                    }
                    else
                    {
                        if (evCode == EventCode.Complete)
                        {
                            if (looping)
                            {
                                OutputPosition();

                                long cur, stop;
                                long early, late;
                                (graphBuilder as IMediaControl).StopWhenReady();
                                (graphBuilder as IMediaSeeking).GetAvailable(out early, out late);
                                (graphBuilder as IMediaSeeking).GetPositions(out cur, out stop);
                                (graphBuilder as IMediaSeeking).SetPositions(
                                    early, AMSeekingSeekingFlags.AbsolutePositioning,
                                    stop, AMSeekingSeekingFlags.AbsolutePositioning);
                                (graphBuilder as IMediaControl).Run();
                            }
                            else
                            {
                                Stop();
                                ShowAltImage();
                            }
                            log.InfoFormat("DirectShowCodec[{0}] Event: {1}, {2}, {3}, {4}",
                                           ID(), evCode, param1, param2, evCode.ToString());
                        }
                        else
                        {
                            log.InfoFormat("DirectShowCodec[{0}] Event: {1}, {2}, {3}, {4}",
                                           ID(), evCode, param1, param2, evCode.ToString());
                        }
                    }
                    eventEx.FreeEventParams(evCode, param1, param2);
                }
            }
        }

        public void CheckProgress()
        {
            lock (this)
            {
                if (ps == PLAY_STATE.BUFFERING)
                {
                    long length;
                    (graphBuilder as IMediaSeeking).GetDuration(out length);
                    log.InfoFormat("DirectShowCodec[{0}] Loaded: {1}", ID(), length);
                    Play();
                }
            }
        }

        public void OutputPosition()
        {
            lock (this)
            {
                long cur, stop;
                (graphBuilder as IMediaSeeking).GetPositions(out cur, out stop);
                log.InfoFormat("DirectShowCodec[{0}] Positions: {1}, {2}", ID(), cur, stop);
            }
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    unsafe public class DirectShowAllocator : IVMRSurfaceAllocator9, IVMRImagePresenter9, IDisposable
    {
        // Constants
        private const int S_OK = unchecked((int)0x00000000);
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int D3DERR_INVALIDCALL = unchecked((int)0x8876086c);

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DirectShowAllocator));

        private Device device = null;
        private AdapterInformation adapterInfo = null;
        private DeviceCreationParameters creationParameters;

        private Surface videoSurface = null;
        private IDirect3DSurface9 *unmanagedSurface = (IDirect3DSurface9 *)IntPtr.Zero;

        private Axiom.Core.Texture axiomTexture = null;
        private D3D.Texture privateTexture = null;
        private Surface privateSurface = null;

        private bool needCopy;

        private Size textureSize;
        private Size videoSize;
        private Rectangle videoRectangle;

        private IVMRSurfaceAllocatorNotify9 vmrSurfaceAllocatorNotify = null;

        private bool disposed = false;

        private int ID;
        private string textureName;
        private IntPtr notifiedDevice = IntPtr.Zero;

        public DirectShowAllocator(Device dev, int movieID, string tname)
        {
            ID = movieID;
            textureName = tname;
            adapterInfo = D3D.Manager.Adapters.Default;

            device = dev;

            creationParameters = device.CreationParameters;
        }

        ~DirectShowAllocator()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                DeleteSurfaces();
                disposed = true;
            }
        }

        #endregion

        // Handy Properties to retrieve the texture, its size and the video size
        public Axiom.Core.Texture Texture
        {
            get
            {
                return axiomTexture;
            }
        }

        public D3D.Texture D3DTexture
        {
            get
            {
                return privateTexture;
            }
        }

        public Size TextureSize
        {
            get
            {
                return textureSize;
            }
        }

        public Size VideoSize
        {
            get
            {
                return videoSize;
            }
        }

        // Delete surfaces...
        private void DeleteSurfaces()
        {
            lock (this)
            {
                if (privateTexture != null)
                {
                    privateTexture.Dispose();
                    privateTexture = null;
                }

                if (privateSurface != null)
                {
                    privateSurface.Dispose();
                    privateSurface = null;
                }

                if (videoSurface != null)
                {
                    videoSurface.Dispose();
                    videoSurface = null;
                }
                axiomTexture = null;
            }
        }

        // Helper method to convert integer FourCC driver 
        // surface identifier into a readable string
        private string FourCCToStr(int fcc)
        {
            byte[] chars = new byte[4];

            if (fcc < 100)
                return fcc.ToString();

            for (int i = 0; i < 4; i++)
            {
                chars[i] = (byte)(fcc & 0xff);
                fcc = fcc >> 8;
            }

            return System.Text.Encoding.ASCII.GetString(chars);
        }

        // Helper method to convert a FourCC string 
        // into its integer surface identifier
        private int StrToFourCC(string fourcc)
        {
            int ans = 0;
            for (int i = 3; i >= 0; i--)
            {
                ans <<= 8;
                ans |= (fourcc[i] & 0xff);
            }
            return ans;
        }

        #region IVMRSurfaceAllocator9 Members

        public int InitializeDevice(IntPtr dwUserID, ref VMR9AllocationInfo lpAllocInfo, ref int lpNumBuffers)
        {
            lock (this)
            {
                log.InfoFormat("DirectShowCodec[{0}]: {1}x{2} : {3} / {4} / {5} / {6} / 0x{7:x}", ID,
                               lpAllocInfo.dwWidth, lpAllocInfo.dwHeight,
                               FourCCToStr(lpAllocInfo.Format),
                               adapterInfo.CurrentDisplayMode.Format,
                               lpAllocInfo.MinBuffers,
                               (Pool)lpAllocInfo.Pool,
                               lpAllocInfo.dwFlags);

                Axiom.Media.PixelFormat texFormat = D3DHelper.ConvertEnum(adapterInfo.CurrentDisplayMode.Format);
                Format format = (Format)lpAllocInfo.Format;

                // if format is YUV ? (note : 0x30303030 = "    ")
                if (lpAllocInfo.Format > 0x30303030)
                {
                    // NV12 textures appear not to play correctly; when using them with an 
                    // offscreen surface, they only show the first frame or so.  Doing this
                    // should cause it to renegotiate to RGB.
                    //
                    // New and improved: YV12 seems to have the same problem.
                    if (
                        (lpAllocInfo.Format == StrToFourCC("NV12")) || 
                        (lpAllocInfo.Format == StrToFourCC("YV12")) 
                        )
                    {
                        // XXXMLM - this may cause us to pop an external window
                        log.WarnFormat("DirectShowCodec[{0}]: Rejecting {1} format", ID, FourCCToStr(lpAllocInfo.Format));
                        return D3DERR_INVALIDCALL;
                    }

                    // Check if the hardware support format conversion from this YUV format to the RGB desktop format
                    if (!D3D.Manager.CheckDeviceFormatConversion(creationParameters.AdapterOrdinal, creationParameters.DeviceType,
                        (Format)lpAllocInfo.Format, adapterInfo.CurrentDisplayMode.Format))
                    {
                        // If not, refuse this format!
                        // The VMR9 will propose other formats supported by the downstream filter output pin.
                        log.WarnFormat("DirectShowCodec[{0}]: Cannot convert between formats", ID);
                        return D3DERR_INVALIDCALL;
                    }
                }
                try
                {
                    IDirect3DDevice9* unmanagedDevice = device.UnmanagedComPointer;
                    IntPtr hMonitor = D3D.Manager.GetAdapterMonitor(adapterInfo.Adapter);

                    // Give our Direct3D device to the VMR9 filter
                    try
                    {
                        IVMRSurfaceAllocatorNotify9 notify9 = vmrSurfaceAllocatorNotify;
                        vmrSurfaceAllocatorNotify.SetD3DDevice((IntPtr)unmanagedDevice, hMonitor);
                    }
                    catch (InvalidCastException e)
                    {
                        // It turns out that if this function is called from the 
                        // decoder thread, the hr return value of the SetD3DDevice
                        // call will be E_INVALIDCAST.  However, if we've already
                        // notified the allocator notify interface of the device,
                        // the rest of this function will happen correctly.  So 
                        // only throw the exception if we haven't notified the
                        // device yet.
                        if ((IntPtr)unmanagedDevice != notifiedDevice)
                        {
                            throw e;
                        }
                    }
                    notifiedDevice = (IntPtr)unmanagedDevice;

                    videoSize = new Size(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);
                    videoRectangle = new Rectangle(Point.Empty, videoSize);

                    // Always do power of two sized textures
                    lpAllocInfo.dwWidth = Manager.NextPowerOfTwo(lpAllocInfo.dwWidth);
                    lpAllocInfo.dwHeight = Manager.NextPowerOfTwo(lpAllocInfo.dwHeight);

                    textureSize = new Size(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);

                    // Just in case
                    DeleteSurfaces();

                    // if format is YUV ?
                    if (lpAllocInfo.Format > 0x30303030)
                    {
                        log.InfoFormat("DirectShowCodec[{0}]: Creating offscreen surface ({1}x{2})",
                                       ID, lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);

                        // An offscreen surface must be created
                        lpAllocInfo.dwFlags |= VMR9SurfaceAllocationFlags.OffscreenSurface;

                        // Create it
                        try
                        {
                            // ATI and nVidia both fail this call when created with YUV, so ask for
                            // an RGB texture first if we can get away with it.
                            if ((lpAllocInfo.dwFlags & VMR9SurfaceAllocationFlags.RGBDynamicSwitch) != 0)
                            {
                                videoSurface = device.CreateOffscreenPlainSurface(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight,
                                    adapterInfo.CurrentDisplayMode.Format, (Pool)lpAllocInfo.Pool);
                            }
                            else
                            {
                                videoSurface = device.CreateOffscreenPlainSurface(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight,
                                    (Format)lpAllocInfo.Format, (Pool)lpAllocInfo.Pool);
                            }
                        }
                        catch
                        {
                            log.WarnFormat("Failed to create {0} surface", (Format)lpAllocInfo.Format);
                            return D3DERR_INVALIDCALL;
                        }
                        // And get it unmanaged pointer
                        unmanagedSurface = videoSurface.UnmanagedComPointer;

                        axiomTexture = TextureManager.Instance.GetByName(textureName);
                        if (axiomTexture != null)
                        {
                            log.InfoFormat("DirectShowCodec[{0}]: Removing old texture \"{1}\"",
                                ID, textureName);
                            axiomTexture.Unload();

                            axiomTexture.TextureType = TextureType.TwoD;
                            axiomTexture.Width = lpAllocInfo.dwWidth;
                            axiomTexture.Height = lpAllocInfo.dwHeight;
                            axiomTexture.NumMipMaps = 0;
                            axiomTexture.Format = texFormat;
                            axiomTexture.Usage = TextureUsage.RenderTarget;
                            axiomTexture.CreateInternalResources();
                        }
                        else
                        {
                            axiomTexture = TextureManager.Instance.CreateManual(
                                textureName,
                                TextureType.TwoD,
                                lpAllocInfo.dwWidth, lpAllocInfo.dwHeight,
                                0, // no mip maps
                                texFormat, // from the display
                                TextureUsage.RenderTarget);
                        }
                        if (axiomTexture is D3DTexture)
                        {
                            D3DTexture d3t = (D3DTexture)axiomTexture;
                            if (d3t.DXTexture is D3D.Texture)
                            {
                                privateTexture = (D3D.Texture)d3t.DXTexture;
                            }
                            else
                            {
                                throw new Exception("D3D texture could not get DX texture");
                            }
                        }
                        else
                        {
                            throw new Exception("D3D Texture failed to create");
                        }

                        // Get the MipMap surface 0 for the copy (see PresentImage)
                        privateSurface = privateTexture.GetSurfaceLevel(0);
                        device.ColorFill(privateSurface, new Rectangle(0, 0, lpAllocInfo.dwWidth, lpAllocInfo.dwHeight), Color.Black);

                        // This code path need a surface copy
                        needCopy = true;
                    }
                    else
                    {
                        log.InfoFormat("DirectShowCodec[{0}]: Creating texture surface ({1}x{2})",
                                       ID, lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);

                        // in RGB pixel format
                        //lpAllocInfo.dwFlags |= VMR9SurfaceAllocationFlags.TextureSurface;

                        //Surface s = device.CreateRenderTarget();

                        axiomTexture = TextureManager.Instance.GetByName(textureName);
                        if (axiomTexture != null)
                        {
                            log.InfoFormat("DirectShowCodec[{0}]: Removing old texture \"{1}\"",
                                ID, textureName);
                            axiomTexture.Unload();

                            axiomTexture.TextureType = TextureType.TwoD;
                            axiomTexture.Width = lpAllocInfo.dwWidth;
                            axiomTexture.Height = lpAllocInfo.dwHeight;
                            axiomTexture.NumMipMaps = 0;
                            axiomTexture.Format = texFormat;
                            axiomTexture.Usage = TextureUsage.RenderTarget;
                            axiomTexture.CreateInternalResources();
                        }
                        else
                        {
                            axiomTexture = TextureManager.Instance.CreateManual(
                                textureName,
                                TextureType.TwoD,
                                lpAllocInfo.dwWidth, lpAllocInfo.dwHeight,
                                0, // no mip maps
                                texFormat, // from the display
                                TextureUsage.RenderTarget);
                        }
                        if (axiomTexture is D3DTexture)
                        {
                            D3DTexture d3t = (D3DTexture)axiomTexture;
                            if (d3t.DXTexture is D3D.Texture)
                            {
                                privateTexture = (D3D.Texture)d3t.DXTexture;
                            }
                        }
                        else
                        {
                            throw new Exception("D3D Texture failed to create");
                        }

                        // And get the MipMap surface 0 for the VMR9 filter
                        privateSurface = privateTexture.GetSurfaceLevel(0);
                        unmanagedSurface = privateSurface.UnmanagedComPointer;
                        device.ColorFill(privateSurface, new Rectangle(0, 0, lpAllocInfo.dwWidth, lpAllocInfo.dwHeight), Color.Black);

                        // This code path don't need a surface copy.
                        // The client appllication use the same texture the VMR9 filter use.
                        needCopy = false;
                    }

                    // This allocator only support 1 buffer.
                    // Notify the VMR9 filter
                    lpNumBuffers = 1;
                }

                catch (DirectXException e)
                {
                    // A Direct3D error can occure : Notify it to the VMR9 filter
                    LogUtil.ExceptionLog.ErrorFormat("Caught DirectX Exception: {0}", e.ToString());
                    return e.ErrorCode;
                }
                catch (Exception e)
                {
                    // Or else, notify a more general error
                    LogUtil.ExceptionLog.ErrorFormat("Caught Exception: {0}", e.ToString());
                    return E_FAIL;
                }

                // This allocation is a success
                return 0;
            }
        }

        public int TerminateDevice(IntPtr dwID)
        {
            DeleteSurfaces();
            return 0;
        }

        public int GetSurface(IntPtr dwUserID, int SurfaceIndex, int SurfaceFlags, out IntPtr lplpSurface)
        {
            lplpSurface = IntPtr.Zero;

            // If the filter ask for an invalid buffer index, return an error.
            if (SurfaceIndex >= 1)
                return E_FAIL;

            lock (this)
            {
                // IVMRSurfaceAllocator9.GetSurface documentation state that the caller release the returned 
                // interface so we must increment its reference count.
                lplpSurface = (IntPtr)unmanagedSurface;
                Marshal.AddRef(lplpSurface);
                return 0;
            }
        }

        public int AdviseNotify(IVMRSurfaceAllocatorNotify9 lpIVMRSurfAllocNotify)
        {
            lock (this)
            {
                vmrSurfaceAllocatorNotify = lpIVMRSurfAllocNotify;

                // Give our Direct3D device to the VMR9 filter
                IDirect3DDevice9 *unmanagedDevice = device.UnmanagedComPointer;
                IntPtr hMonitor = D3D.Manager.GetAdapterMonitor(D3D.Manager.Adapters.Default.Adapter);

                return vmrSurfaceAllocatorNotify.SetD3DDevice((IntPtr)unmanagedDevice, hMonitor);
            }
        }

        #endregion

        #region IVMRImagePresenter9 Members

        public int StartPresenting(IntPtr dwUserID)
        {
            lock (this)
            {
                if (device == null)
                {
                    return E_FAIL;
                }

                return 0;
            }
        }

        public int StopPresenting(IntPtr dwUserID)
        {
            return 0;
        }

        public int PresentImage(IntPtr dwUserID, ref VMR9PresentationInfo lpPresInfo)
        {
            lock (this)
            {
                try
                {
                    // If YUV mixing is activated, a surface copy is needed
                    if (needCopy)
                    {
                        FrameRender();
                    }
                }
                catch (DirectXException e)
                {
                    // A Direct3D error can occure : Notify it to the VMR9 filter
                    LogUtil.ExceptionLog.ErrorFormat("Caught DirectX Exception: {0}", e.ToString());
                    return e.ErrorCode;
                }
                catch (Exception e)
                {
                    // Or else, notify a more general error
                    LogUtil.ExceptionLog.ErrorFormat("Caught Exception: {0}", e.ToString());
                    return E_FAIL;
                }

                // This presentation is a success
                return 0;
            }
        }
        #endregion

        #region Frame Events
        public void FrameRender()
        {
            lock (this)
            {
                try
                {
                    // If YUV mixing is activated, a surface copy is needed
                    if (needCopy)
                    {
#if false
                        log.Info("DirectShowCodec[" + ID + "]: Render from thread " +
                            System.Threading.Thread.CurrentThread.ManagedThreadId);
#endif
#if true
                        // Use StretchRectangle to do the Pixel Format conversion
                        // XXXMLM - need to call videoSurface.LockRectangle?  returns
                        // D3DERR_INVALIDCALL
                        //videoSurface.LockRectangle(LockFlags.ReadOnly | LockFlags.NoSystemLock);
                        //privateSurface.LockRectangle(LockFlags.NoSystemLock);
                        device.StretchRectangle(
                          videoSurface,
                          videoRectangle,
                          privateSurface,
                          videoRectangle,
                          TextureFilter.None
                          );
                        //privateSurface.UnlockRectangle();
                        //videoSurface.UnlockRectangle();
#elif true
                        device.UpdateSurface(videoSurface, privateSurface);
#endif
                    }
                }
                catch (DirectXException e)
                {
                    //videoSurface.UnlockRectangle();
                    LogUtil.ExceptionLog.WarnFormat("Caught DirectX Exception {0} {1} while rendering.", e.ErrorCode, e.ToString());
                }
            }
        }
        #endregion
    }

    unsafe public class DirectShowWebReader
    {
        public const int S_OK = unchecked((int)0x00000000);
        public const int S_FALSE = unchecked((int)0x00000001);

        public const int MAX_REDIRECTS = 10;
        public const int READ_SIZE = 100000;
        public const int FILE_ID_SIZE = 32768;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DirectShowWebReader));

        private Thread bufferThread = null;
        private string url = null;
        private byte[] buffer = null;
        private long bufferSize = 0;
        private long bufferEnd = 0;
        private bool bufferThreadExit = false;
        private string redirect = null;
        private Guid majorType = Guid.Empty;
        private Guid minorType = Guid.Empty;
        private Guid recommendedFilter = Guid.Empty;

        public string URL { get { return url; } }
        public Guid MajorType { get { return majorType; } }
        public Guid MinorType { get { return minorType; } }
        public Guid RecommendedFilter { get { return recommendedFilter; } }

        public class RedirectException : Exception
        {
            public string url;
            public RedirectException(string u) { url = u; }
        }

        public DirectShowWebReader()
        {
        }

        ~DirectShowWebReader()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            if (bufferThread != null)
            {
                Monitor.Enter(this);
                bufferThreadExit = true;
                Monitor.PulseAll(this);
                Monitor.Exit(this);
                bufferThread.Join();
                bufferThread = null;
            }
        }

        public long BufferSize { get { long ans; Monitor.Enter(this); ans = bufferSize; Monitor.Exit(this); return ans; } }
        public long BufferEnd  { get { long ans; Monitor.Enter(this); ans = bufferEnd;  Monitor.Exit(this); return ans; } }

        public byte[] GetBuffer()
        {
            Monitor.Enter(this);
            return buffer;
        }

        public void ReleaseBuffer()
        {
            Monitor.Exit(this);
        }

        public int Load(string u)
        {
            log.InfoFormat("Loading URL ({0})", u);
            int ans = S_OK;
            bufferSize = 0;
            url = u;
            bufferThreadExit = false;
            Monitor.Enter(this);
            bufferThread = new Thread(new ThreadStart(BufferProc));
            bufferThread.Name = "DirectShow streaming thread (" + url + ")";
            bufferThread.Start();
            Monitor.Wait(this);
            if (redirect != null)
            {
                bufferThread = null;
                RedirectException e = new RedirectException(redirect);
                Monitor.Exit(this);
                throw e;
            }
            if (bufferSize == 0)
            {
                bufferThread = null;
                ans = S_FALSE;
            }
            else
            {
                GuessMediaType(url, out majorType, out minorType, out recommendedFilter);
            }
            Monitor.Exit(this);
            return ans;
        }

        [STAThread]
        private void BufferProc()
        {
            Monitor.Enter(this);
#if true
            WebRequest w;
            WebResponse r;
            Stream stream = null;
            long count = 0;
            long total = 0;
            long length;

            w = WebRequest.Create(url);
            r = w.GetResponse();
            if (r != null)
            {
                log.InfoFormat("Request success, content length {0}", r.ContentLength);
                buffer = new byte[r.ContentLength];
                bufferSize = r.ContentLength;
                bufferEnd = 0;
                stream = r.GetResponseStream();
                count = 0;
                total = 0;
                length = r.ContentLength;
            }
            else
            {
                Monitor.PulseAll(this);
                Monitor.Exit(this);
                return;
            }
#else
            // fakezors
            int idx = url.LastIndexOf('/');
            string filename = url.Substring(idx + 1);
            filename = filename.Insert(0, "c:\\cygwin\\home\\mlm\\tmp\\");
            Stream stream = File.OpenRead(filename);
            log.Info("File open success, content length " + stream.Length);
            buffer = new byte[stream.Length];
            long count = 0;
            long total = 0;
            long length;
            bufferSize = stream.Length;
            bufferEnd = 0;
            length = stream.Length;
#endif

            // hold the lock until we have enough data for file ID
            bool pulsed = false;
            while (true)
            {
                if (pulsed)
                {
                    Monitor.Enter(this);
                } // else we have the lock
                if (bufferThreadExit)
                {
                    break;
                }
                try
                {
                    count = stream.Read(buffer, (int)total, (length > READ_SIZE) ? READ_SIZE : (int)length);
                    if (count != 0)
                    {
#if false
                        log.Info("Read " + count + " bytes (" + total + ")");
#endif
                        total += count;
                        length -= count;
                        bufferEnd = total;
                        if (length == 0)
                        {
                            log.InfoFormat("End of stream ({0}, {1})", total, bufferSize);
#if false
                            Stream f = File.Open("c:\\cygwin\\home\\mlm\\tmp\\TEST_FILE", FileMode.Create);
                            f.Write(buffer, 0, (int)total);
                            f.Close();
#endif
                            CheckForRedirect();
                            Monitor.PulseAll(this);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogUtil.ExceptionLog.ErrorFormat("DirectShowStreamingPin read caught exception: {0}", e);
                    Monitor.PulseAll(this);
                    break;
                }
                if (!pulsed)
                {
#if true
                    if (total > FILE_ID_SIZE)
                    {
                        log.InfoFormat("File header read ({0})", total);
                        Monitor.PulseAll(this);
                        Monitor.Exit(this);
                        pulsed = true;
                    } // else hold the lock
#endif
                }
                else
                {
                    Monitor.PulseAll(this);
                    Monitor.Exit(this);
                }
            }
            // we hold the lock right now
            bufferThread = null;
            Monitor.Exit(this);
        }

        public void CheckForRedirect()
        {
            // hurr - there's no asx file parser outside of wmp, this bites
            System.Text.ASCIIEncoding e = new System.Text.ASCIIEncoding();
            string header = e.GetString(buffer, 0, 4);
            if (header.ToLower() == "<asx")
            {
                // hurrgh, it's not even valid XML - & in urls needs escaping
                string buf = e.GetString(buffer);
                buf = Regex.Replace(buf, "&", "&amp;");
                byte[] bytes = e.GetBytes(buf);
                MemoryStream ms = new MemoryStream(bytes);
                XmlDocument doc = new XmlDocument();
                doc.Load(ms);
                // also, they use case insensitivity even though xml is sensitive
                foreach (XmlNode node in doc.ChildNodes)
                {
                    if (node.Name.ToLower() == "asx")
                    {
                        foreach(XmlNode entries in node.ChildNodes)
                        {
                            if (entries.Name.ToLower() == "entry")
                            {
                                foreach (XmlNode reference in entries.ChildNodes)
                                {
                                    if (reference.Name.ToLower() == "ref")
                                    {
                                        foreach (XmlAttribute att in reference.Attributes)
                                        {
                                            if (att.Name.ToLower() == "href")
                                            {
                                                redirect = att.Value;
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public bool GuessMediaType(string path, out Guid majorType, out Guid minorType, out Guid recommendedFilter)
        {
            // check the file bytes for major and minor media type
            // XXXMLM - we don't look at the protocol section, should we?
            Monitor.Enter(this);
            byte[] buffer = GetBuffer();
            RegistryKey root = Registry.ClassesRoot.OpenSubKey("Media Type");
            foreach (string majorName in root.GetSubKeyNames())
            {
                if (majorName != "Extensions")
                {
                    RegistryKey major = root.OpenSubKey(majorName);
                    foreach (string minorName in major.GetSubKeyNames())
                    {
                        RegistryKey minor = major.OpenSubKey(minorName);
                        foreach (string indexName in minor.GetValueNames())
                        {
                            if (indexName != "Source Filter")
                            {
                                string id = (minor.GetValue(indexName) as string);
                                if (id != null)
                                {
                                    string[] values = id.Split(new char[] { ',' });
                                    if ((values.Length % 4) == 0)
                                    {
                                        bool match = false;

                                        // offset, size, mask, value
                                        for (int i = 0; i < values.Length; i += 4)
                                        {
                                            int offset, size;
                                            offset = int.Parse(values[i + 0].Trim());
                                            size = int.Parse(values[i + 1].Trim());
                                            // negative offset is from end of file, we don't want to wait
                                            if (offset >= 0)
                                            {
                                                string mask = values[i + 2].Trim();
                                                string value = values[i + 3].Trim();
                                                if (mask == "")
                                                {
                                                    mask = new string('f', (size * 2));
                                                }
                                                if (mask.Length != (size * 2))
                                                {
                                                    mask.PadLeft((size * 2) - mask.Length, '0');
                                                }
#if false
                                                string bufstr = "";
                                                for(int z=0; z < size; z++)
                                                {
                                                    byte v = (byte)(buffer[offset + z] & 0xff);
                                                    v &= byte.Parse(mask.Substring((z*2), 2), NumberStyles.AllowHexSpecifier);
                                                    bufstr += string.Format("{0:x2}", v);
                                                }
                                                log.Info(majorName + ", " + minorName + ": " +
                                                    values[i + 0] + ", " + values[i + 1] + ", " + values[i + 2] + ", " + values[i + 3] +
                                                    " (" + offset + ", " + size + ", " + mask + ", " + value + ") - " +
                                                    bufstr);
#endif
                                                for (int j = 0; j < size; j++)
                                                {
                                                    byte v = (byte)(buffer[offset + j] & 0xff);
                                                    v &= byte.Parse(mask.Substring((j * 2), 2), NumberStyles.AllowHexSpecifier);
                                                    if (v == (byte.Parse(value.Substring((j * 2), 2), NumberStyles.AllowHexSpecifier)))
                                                    {
                                                        match = true;
                                                    }
                                                    else
                                                    {
                                                        match = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!match)
                                            {
                                                break;
                                            }
                                        }
                                        if (match)
                                        {
                                            string sourceFilter = (minor.GetValue("Source Filter") as string);
                                            log.InfoFormat("GuessMediaType: {0} - {1}", majorName, minorName);
                                            log.InfoFormat("Source filter: {0}", sourceFilter);
                                            majorType = new Guid(majorName);
                                            minorType = new Guid(minorName);
                                            if (sourceFilter != null)
                                            {
                                                recommendedFilter = new Guid(sourceFilter);
                                            }
                                            else
                                            {
                                                recommendedFilter = Guid.Empty;
                                            }
                                            ReleaseBuffer();
                                            Monitor.Exit(this);
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // no file bytes match? guess we'll try the extension
            string fileext = Path.GetExtension(path);
            if (fileext != null)
            {
                RegistryKey exts = root.OpenSubKey("Extensions");
                foreach (string ext in exts.GetSubKeyNames())
                {
                    if (ext == fileext)
                    {
                        RegistryKey filetype = exts.OpenSubKey(ext);
                        string majorName = filetype.GetValue("Media Type") as string;
                        string minorName = filetype.GetValue("Subtype") as string;
                        string sourceFilter = filetype.GetValue("Source Filter") as string;
                        if ((majorName != null) && (minorName != null))
                        {
                            log.InfoFormat("GuessMediaType: {0} - {1} (by ext)", majorName, minorName);
                            log.InfoFormat("Source filter: {0}", sourceFilter);
                            majorType = new Guid(majorName);
                            minorType = new Guid(minorName);
                            if (sourceFilter != null)
                            {
                                recommendedFilter = new Guid(sourceFilter);
                            }
                            else
                            {
                                recommendedFilter = Guid.Empty;
                            }
                            ReleaseBuffer();
                            Monitor.Exit(this);
                            return true;
                        }
                    }
                }
            }

            // i give up
            log.Info("GuessMediaType did not find a media type.");
            majorType = Guid.Empty;
            minorType = Guid.Empty;
            recommendedFilter = Guid.Empty;
            ReleaseBuffer();
            Monitor.Exit(this);
            return false;
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("5260957C-D808-463b-9EB4-7DF4F9E6170B")]
    unsafe public class DirectShowStreamingPin : IPin, IAsyncReader
    {
        public const string PIN_ID = "Streaming Output";
        public const int DS_RESCALE_FACTOR = 10000000;
        public const int S_OK = unchecked((int)0x00000000);
        public const int S_FALSE = unchecked((int)0x00000001);
        public const int E_NOTIMPL = unchecked((int)0x80004001);
        public const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
        public const int VFW_E_NOT_CONNECTED = DsResults.E_NotConnected;
        public const int VFW_E_NO_TRANSPORT = DsResults.E_NoTransport;
        public const int VFW_E_NO_ACCEPTABLE_TYPES = DsResults.E_NoAcceptableTypes;
        public const int VFW_E_TYPE_NOT_ACCEPTED = DsResults.E_TypeNotAccepted;
        public const int VFW_E_WRONG_STATE = DsResults.E_WrongState;
        public const int VFW_E_TIMEOUT = DsResults.E_Timeout;
        public const int BUFFER_SIZE = 1000000;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DirectShowStreamingPin));

        private DirectShowStreamer parent = null;
        private DirectShowWebReader reader = null;
        private List<AMMediaType> media = null;
        private IPin pin = null;
        private IMemAllocator allocator = null;
        private AMMediaType pintype = null;
        private AMMediaType mediatype = null;
        private long startTime = 0L;
        private List<IMediaSample> sampleList = new List<IMediaSample>();
        private List<IntPtr> samplePtrList = new List<IntPtr>();
        private bool flushing = false;

        public DirectShowStreamingPin(DirectShowStreamer p, DirectShowWebReader r)
        {
            parent = p;
            reader = r;
            media = new List<AMMediaType>();
            media.Add(new AMMediaType());
            media[0].majorType = MediaType.Stream;
            media[0].subType = MediaType.Null;
            mediatype = new AMMediaType();
            mediatype.majorType = media[0].majorType;
            mediatype.subType = media[0].subType;
        }

        ~DirectShowStreamingPin()
        {
            Monitor.Enter(this);
            Shutdown();
            for(int i=0; i < media.Count; i++)
            {
                AMMediaType am = media[i];
                DsUtils.FreeAMMediaType(am);
            }
            media.Clear();
            DsUtils.FreeAMMediaType(mediatype);
            Monitor.Exit(this);
        }

        public void SetType(Guid majorType, Guid minorType)
        {
            mediatype.majorType = majorType;
            mediatype.subType = minorType;
        }

        public void Shutdown()
        {
            Monitor.Enter(this);
            if (pintype != null)
            {
                DsUtils.FreeAMMediaType(pintype);
                pintype = null;
            }
            if (reader != null)
            {
                reader.Shutdown();
                reader = null;
            }
            Monitor.Exit(this);
        }

        #region IPin methods
        [PreserveSig]
        public int Connect(IPin pReceivePin, AMMediaType pmt)
        {
            Monitor.Enter(this);
            int hr = S_OK;
            pin = null;
            pintype = null;
            allocator = null;
            string id = "Unnamed pin";
            pReceivePin.QueryId(out id);
            PinInfo pi = new PinInfo();
            hr = pReceivePin.QueryPinInfo(out pi);
            if (hr == S_OK)
            {
                FilterInfo fi = new FilterInfo();
                hr = pi.filter.QueryFilterInfo(out fi);
                if (hr == S_OK)
                {
                    id += (" (" + fi.achName);
                }
                Guid guid;
                hr = pi.filter.GetClassID(out guid);
                if (hr == S_OK)
                {
                    id += (", " + guid.ToString());
                }
                id += ")";
            }
            try
            {
                AMMediaType amt = null;
                if (pmt != null)
                {
                    amt = pmt;
                }
                else
#if false
                {
                    IEnumMediaTypes ie;
                    hr = pReceivePin.EnumMediaTypes(out ie);
                    int fetched;
                    int alloc = Marshal.SizeOf(typeof(AMMediaType));
                    IntPtr mtypePtr = Marshal.AllocCoTaskMem(alloc);
                    while (ie.Next(1, mtypePtr, out fetched) == S_OK)
                    {
                        amt = new AMMediaType();
                        Marshal.PtrToStructure(mtypePtr, amt);
                        hr = pReceivePin.QueryAccept(amt);
                        if (hr == S_OK)
                        {
                            break;
                        }
                        DsUtils.FreeAMMediaType(amt);
                        amt = null;
                    }
                    if (fetched == 0)
                    {
                        amt = null;
                    }
                    Marshal.FreeCoTaskMem(mtypePtr);
                }
                if (amt == null)
#endif
                {
                    amt = mediatype;
                }
                hr = pReceivePin.QueryAccept(amt);
                if (hr == S_FALSE)
                {
                    log.InfoFormat("No media type for pin '{0}'", id);
                    Monitor.Exit(this);
                    return VFW_E_NO_ACCEPTABLE_TYPES;
                }

                hr = pReceivePin.ReceiveConnection(this, amt);
                if (hr == VFW_E_TYPE_NOT_ACCEPTED)
                {
                    log.InfoFormat("No connection to pin '{0}'", id);
                    Monitor.Exit(this);
                    return VFW_E_NO_ACCEPTABLE_TYPES;
                }
                DsError.ThrowExceptionForHR(hr);

                pin = pReceivePin;
                pintype = amt;
            }
            catch (Exception e)
            {
                LogUtil.ExceptionLog.ErrorFormat("Caught exception in connect ({0}): {1}{2}", id, e.Message, e.StackTrace);
                pin = null;
                pintype = null;
                allocator = null;
                Monitor.Exit(this);
                return VFW_E_NO_TRANSPORT;
            }
            Monitor.Exit(this);
            log.InfoFormat("Connected to pin '{0}'", id);
            return S_OK;
        }

        [PreserveSig]
        public int ReceiveConnection(IPin pReceivePin, AMMediaType pmt)
        {
            // this is for input pins only
            return E_UNEXPECTED;
        }

        [PreserveSig]
        public int Disconnect()
        {
            int ans = S_FALSE;
            Monitor.Enter(this);
            if (pin != null)
            {
                if (allocator != null)
                {
                    allocator.Decommit();
                }
                string id = "Unnamed pin";
                pin.QueryId(out id);
                log.InfoFormat("Disconnect from pin '{0}'", id);
                pin = null;
                pintype = null;
                allocator = null;
                ans = S_OK;
            }
            Monitor.Exit(this);
            return ans;
        }

        [PreserveSig]
        public int ConnectedTo(out IPin ppPin)
        {
            int ans = S_OK;
            Monitor.Enter(this);
            if (pin != null)
            {
                ppPin = pin;
                ans = S_OK;
            }
            else
            {
                ppPin = null;
                ans = VFW_E_NOT_CONNECTED;
            }
            Monitor.Exit(this);
            return ans;
        }

        [PreserveSig]
        public int ConnectionMediaType(AMMediaType pmt)
        {
            int ans = S_OK;
            Monitor.Enter(this);
            if (pintype != null)
            {
                pmt = pintype;
                ans = S_OK;
            }
            else
            {
                pmt = null;
                ans = VFW_E_NOT_CONNECTED;
            }
            Monitor.Exit(this);
            return ans;
        }

        [PreserveSig]
        public int QueryPinInfo(out PinInfo pInfo)
        {
            Monitor.Enter(this);
            pInfo.name = PIN_ID;
            pInfo.dir = PinDirection.Output;
            pInfo.filter = parent;
            Monitor.Exit(this);
            return S_OK;
        }

        [PreserveSig]
        public int QueryDirection(out PinDirection pPinDir)
        {
            pPinDir = PinDirection.Output;
            return S_OK;
        }

        [PreserveSig]
        public int QueryId(out string Id)
        {
            Id = PIN_ID;
            return S_OK;
        }

        [PreserveSig]
        public int QueryAccept(AMMediaType pmt)
        {
            // output pins like ourselves dictate the connection type
            return E_NOTIMPL;
        }

        class MediaTypeEnumerator : IEnumMediaTypes
        {
            DirectShowStreamingPin p;
            IEnumerator e = null;
            int c;
            public MediaTypeEnumerator(DirectShowStreamingPin parent)
            {
                p = parent;
                e = parent.media.GetEnumerator();
                c = 0;
            }

            [PreserveSig]
            public int Next(int cMediaTypes, IntPtr ppMediaTypes, out int pcFetched)
            {
                int ans = S_OK;
                pcFetched = 0;
                for (int i = 0; i < cMediaTypes; i++)
                {
                    if (e.MoveNext())
                    {
                        AMMediaType am = (e.Current as AMMediaType);
                        IntPtr amPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf(am));
                        Marshal.StructureToPtr(am, amPointer, true);
                        Marshal.WriteIntPtr(ppMediaTypes, i * IntPtr.Size, amPointer);
                        pcFetched++;
                        c++;
                    }
                    else
                    {
                        ans = S_FALSE;
                        break;
                    }
                }
                return ans;
            }

            [PreserveSig]
            public int Skip(int cMediaTypes)
            {
                int ans = S_OK;
                for (int i = 0; i < cMediaTypes; i++)
                {
                    if (!e.MoveNext())
                    {
                        ans = S_FALSE;
                        break;
                    }
                    else
                    {
                        c++;
                    }
                }
                return ans;
            }

            [PreserveSig]
            public int Reset()
            {
                e.Reset();
                c = 0;
                return S_OK;
            }

            [PreserveSig]
            public int Clone(out IEnumMediaTypes ppEnum)
            {
                MediaTypeEnumerator ans = new MediaTypeEnumerator(p);
                if(c != 0)
                {
                    ans.Skip(c);
                }
                ppEnum = ans;
                return S_OK;
            }

        }
        [PreserveSig]
        public int EnumMediaTypes(out IEnumMediaTypes ppEnum)
        {
            ppEnum = new MediaTypeEnumerator(this);
            return S_OK;
        }

        [PreserveSig]
        public int QueryInternalConnections(IPin[] ppPins, ref int nPin)
        {
            // we have no internal connections, docs say this is correct return
            return E_NOTIMPL;
        }

        [PreserveSig]
        public int EndOfStream()
        {
            // i should be telling you that the stream is over, not the other
            // way around
            return E_UNEXPECTED;
        }

        [PreserveSig]
        public int BeginFlush()
        {
            log.Info("Begin flush");
            Monitor.Enter(this);
            flushing = true;
            Monitor.Exit(this);
            return S_OK;
        }

        [PreserveSig]
        public int EndFlush()
        {
            log.Info("End flush");
            Monitor.Enter(this);
            flushing = false;
            Monitor.Exit(this);
            return S_OK;
        }

        [PreserveSig]
        public int NewSegment(long tStart, long tStop, double dRate)
        {
            log.InfoFormat("New segment ({0}, {1}, {2})", tStart, tStop, dRate);
            return S_OK;
        }
        #endregion

        #region Connection stuff
        public void BufferData(int pos)
        {
            int toBuffer = pos + BUFFER_SIZE;
            int endOfBuffer = (int)(reader.BufferSize - pos);
            if (endOfBuffer < toBuffer)
            {
                toBuffer = endOfBuffer;
            }
            WaitForData(toBuffer);
        }

        public void WaitForData(int pos)
        {
            int count = 0;
            Monitor.Enter(reader);
            while (pos > reader.BufferEnd)
            {
                Monitor.PulseAll(reader);
                Monitor.Wait(reader);
                count++;
                if ((count % 10) == 0)
                {
                    log.InfoFormat("Still waiting ({0}, {1}, {2})", count, pos, reader.BufferEnd);
                }
            }
            Monitor.Exit(reader);
        }

        #endregion

        #region Flow control
        public int Play(long tStart)
        {
            log.InfoFormat("Input pin play ({0})", tStart);
            Monitor.Enter(this);
            startTime = tStart;
            Monitor.Exit(this);
            return S_OK;
        }

        public int Pause()
        {
            log.Info("Input pin pause");
            return S_OK;
        }

        public int Stop()
        {
            log.Info("Input pin stop");
            return S_OK;
        }
        #endregion

        #region IAsyncReader methods
        [PreserveSig]
        public int RequestAllocator(IMemAllocator pPreferred, AllocatorProperties pProps, out IMemAllocator ppActual)
        {
            // always use their allocator
            if (pPreferred != null)
            {
                ppActual = pPreferred;
                allocator = pPreferred;
                return S_OK;
            }
            else
            {
                ppActual = null;
                return E_NOTIMPL;
            }
        }

        [PreserveSig]
        public int Request(IMediaSample pSample, IntPtr dwUser)
        {
            // queues up a request for a new sample
            Monitor.Enter(this);
            long start, end;
            pSample.GetTime(out start, out end);
            log.InfoFormat("Request ({0}, {1} - {2})", start, end, pSample.GetSize());
            sampleList.Add(pSample);
            samplePtrList.Add(dwUser);
            Monitor.Exit(this);
            return S_OK;
        }

        [PreserveSig]
        public int WaitForNext(int dwTimeout, out IMediaSample ppSample, out IntPtr pdwUser)
        {
            Monitor.Enter(this);
            int timeout = (dwTimeout == -1) ? 1000 : dwTimeout;
            int timesleep = 1;
            if ((sampleList.Count == 0) && (!flushing))
            {
                log.InfoFormat("WaitForNext waiting for next ({0}), {1}, {2}", sampleList.Count, flushing, dwTimeout);
            }
            while ((sampleList.Count == 0) && (!flushing))
            {
                // yeah, i know - problem is, for Request to pulse the lock, i have to 
                // know what thread it's coming from, so it can be holding the lock, and
                // i don't know what thread the request will come in from - so, spin the
                // hard way
                Monitor.Exit(this);
                Thread.Sleep(timesleep);
                Monitor.Enter(this);
                timeout -= timesleep;
                if (timeout <= 0)
                {
                    ppSample = null;
                    pdwUser = IntPtr.Zero;
                    Monitor.Exit(this);
                    return VFW_E_TIMEOUT;
                }
            }
            log.InfoFormat("WaitForNext ({0}), {1}, {2}", sampleList.Count, flushing, dwTimeout);
            if (sampleList.Count != 0)
            {
                ppSample = sampleList[0];
                pdwUser = samplePtrList[0];
                int hr = SyncReadAligned(ppSample);
                sampleList.RemoveAt(0);
                samplePtrList.RemoveAt(0);
                Monitor.Exit(this);
                return hr;
            }
            else
            {
                ppSample = null;
                pdwUser = IntPtr.Zero;
                int ans;
                if (flushing)
                {
                    ans = VFW_E_WRONG_STATE;
                }
                else
                {
                    ans = VFW_E_TIMEOUT;
                }
                Monitor.Exit(this);
                return ans;
            }
        }

        [PreserveSig]
        public int SyncReadAligned(IMediaSample pSample)
        {
            Monitor.Enter(this);
            long start, end;
            long mediaStart, mediaEnd;
            int size;
            int offset;
            int hr;
            int ans = S_OK;
            IntPtr ptr = new IntPtr();
            hr = pSample.GetPointer(out ptr);
            hr = pSample.GetTime(out start, out end);
            hr = pSample.GetMediaTime(out mediaStart, out mediaEnd);
            size = pSample.GetSize();
            int rescale = DS_RESCALE_FACTOR; // (int)((end - start) / size);
            //start += startTime;
            offset = (int)(start / rescale);
            if ((offset + size) > reader.BufferSize)
            {
                Memory.Set(ptr, offset, size);
                log.InfoFormat("SyncReadAligned went off the end ({0}, {1}, {2})", offset, (offset + size), reader.BufferSize);
                size = (int)(reader.BufferSize - offset);
                ans = S_FALSE;
            }
            if ((offset + size) > reader.BufferEnd)
            {
                log.InfoFormat("SyncReadAligned wait for buffer ({0}, {1}, {2})", offset, (offset + size), reader.BufferEnd);
                BufferData(offset + size);
            }
            log.InfoFormat("SyncReadAligned ({0} / {1} ({2}), {3} / {4}) - {5}, {6}", start, mediaStart, offset, end, mediaEnd, size, rescale);
            byte[] buffer = reader.GetBuffer();
            Marshal.Copy(buffer, offset, ptr, size);
            reader.ReleaseBuffer();
            Monitor.Exit(this);
            return ans;
        }

        [PreserveSig]
        public int SyncRead(long llPosition, int lLength, IntPtr pBuffer) // BYTE *
        {
            Monitor.Enter(this);
            int ans = S_OK;
            long toRead = lLength;
            if (llPosition > reader.BufferSize)
            {
                toRead = 0;
                Memory.Set(pBuffer, 0, lLength);
                log.InfoFormat("SyncRead position off end of buffer - {0}, {1}", llPosition, reader.BufferSize);
                return S_FALSE;
            }
            if ((llPosition + toRead) > reader.BufferSize)
            {
                Memory.Set(pBuffer, 0, lLength);
                toRead = reader.BufferSize - llPosition;
                log.InfoFormat("SyncRead shortened buffer - {0}, {1}", lLength, toRead);
                ans = S_FALSE;
            }
            if ((llPosition + toRead) > reader.BufferEnd)
            {
                log.InfoFormat("SyncRead wait for buffer ({0}, {1}, {2})", llPosition, (llPosition + lLength), reader.BufferEnd);
                BufferData((int)(llPosition + lLength));
            }
            byte[] buffer = reader.GetBuffer();
            Marshal.Copy(buffer, (int)llPosition, pBuffer, (int)toRead);
            reader.ReleaseBuffer();
            log.InfoFormat("SyncRead ({0}, {1}) + ({2})", llPosition, lLength, toRead);
            Monitor.Exit(this);
            return ans;
        }

        [PreserveSig]
        public int Length(out long pTotal, out long pAvailable)
        {
            Monitor.Enter(this);
            // bug in MPEG-1 splitter, always report full availability
            pTotal = (reader.BufferSize);// +(startTime / DS_RESCALE_FACTOR);
            pAvailable = (reader.BufferEnd);//(pTotal);// +(startTime / DS_RESCALE_FACTOR);
            log.InfoFormat("Length {0}, {1}", pTotal, pAvailable);
            Monitor.Exit(this);
            return S_OK;
        }
        #endregion
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("AA36A2AD-62FF-474e-8F2B-908F627A18AB")]
    unsafe public class DirectShowStreamer : IBaseFilter
    {
        public const string FILTER_NAME = "Multiverse DirectShow Streamer";
        public const string VENDOR_NAME = "Multiverse";
        public const int S_OK = unchecked((int)0x00000000);
        public const int S_FALSE = unchecked((int)0x00000001);
        public const int E_NOTIMPL = unchecked((int)0x80004001);
        public const int VFW_E_NOT_FOUND = DsResults.E_NotFound;

        private FilterState curState = FilterState.Stopped;
        private IReferenceClock clock = null;
        private IFilterGraph graph = null;
        private List<IPin> pins = null;

        public DirectShowStreamer(DirectShowWebReader r)
        {
            pins = new List<IPin>();
            pins.Add(new DirectShowStreamingPin(this, r));
        }

        ~DirectShowStreamer()
        {
            Shutdown();
        }

        public void SetType(Guid majorType, Guid minorType)
        {
            foreach (IPin pin in pins)
            {
                DirectShowStreamingPin p = (pin as DirectShowStreamingPin);
                if (p != null)
                {
                    p.SetType(majorType, minorType);
                }
            }
        }

        public void Shutdown()
        {
            foreach (IPin pin in pins)
            {
                DirectShowStreamingPin p = (pin as DirectShowStreamingPin);
                if (p != null)
                {
                    p.Shutdown();
                }
            }
        }

        public IPin GetOutputPin()
        {
            return pins[0];
        }

        #region IPersist Methods
        [PreserveSig]
        public int GetClassID(out Guid pClassID)
        {
            Attribute att = Attribute.GetCustomAttribute(typeof(DirectShowStreamer), typeof(GuidAttribute));
            pClassID = new Guid(((GuidAttribute)att).Value);
            return S_OK;
        }
        #endregion

        #region IMediaFilter Methods
        [PreserveSig]
        public int Stop()
        {
            curState = FilterState.Stopped;
            int ans = S_OK;
            foreach (IPin pin in pins)
            {
                DirectShowStreamingPin p = (pin as DirectShowStreamingPin);
                if (p != null)
                {
                    ans = p.Stop();
                    DsError.ThrowExceptionForHR(ans);
                }
            }
            return ans;
        }
        [PreserveSig]
        public int Pause()
        {
            curState = FilterState.Paused;
            int ans = S_OK;
            foreach (IPin pin in pins)
            {
                DirectShowStreamingPin p = (pin as DirectShowStreamingPin);
                if (p != null)
                {
                    ans = p.Pause();
                    DsError.ThrowExceptionForHR(ans);
                }
            }
            return ans;
        }
        [PreserveSig]
        public int Run(long tStart)
        {
            curState = FilterState.Running;
            int ans = S_OK;
            foreach (IPin pin in pins)
            {
                DirectShowStreamingPin p = (pin as DirectShowStreamingPin);
                if (p != null)
                {
                    ans = p.Play(tStart);
                    DsError.ThrowExceptionForHR(ans);
                }
            }
            return ans;
        }
        [PreserveSig]
        public int GetState(int dwMilliSecsTimeout, out FilterState filtState)
        {
            filtState = curState;
            return S_OK;
        }
        [PreserveSig]
        public int SetSyncSource(IReferenceClock pClock)
        {
            clock = pClock;
            return S_OK;
        }
        [PreserveSig]
        public int GetSyncSource(out IReferenceClock pClock)
        {
            pClock = clock;
            return S_OK;
        }
        #endregion

        #region IBaseFilter methods
        class PinEnumerator : IEnumPins
        {
            DirectShowStreamer p;
            IEnumerator e = null;
            int c;
            public PinEnumerator(DirectShowStreamer parent)
            {
                p = parent;
                e = parent.pins.GetEnumerator();
                c = 0;
            }

            public int Next(int cPins, IPin[] ppPins, out int pcFetched)
            {
                int ans = S_OK;
                pcFetched = 0;
                for (int i = 0; i < cPins; i++)
                {
                    if (e.MoveNext())
                    {
                        ppPins[pcFetched] = (e.Current as IPin);
                        pcFetched++;
                        c++;
                    }
                    else
                    {
                        ans = S_FALSE;
                        break;
                    }
                }
                return ans;
            }

            public int Skip(int cPins)
            {
                int ans = S_OK;
                for (int i = 0; i < cPins; i++)
                {
                    if (!e.MoveNext())
                    {
                        ans = S_FALSE;
                        break;
                    }
                    else
                    {
                        c++;
                    }
                }
                return ans;
            }

            public int Reset()
            {
                e.Reset();
                c = 0;
                return S_OK;
            }

            public int Clone(out IEnumPins ppEnum)
            {
                PinEnumerator ans = new PinEnumerator(p);
                if(c != 0)
                {
                    ans.Skip(c);
                }
                ppEnum = ans;
                return S_OK;
            }
        }
        [PreserveSig]
        public int EnumPins(out IEnumPins ppEnum)
        {
            ppEnum = new PinEnumerator(this);
            return S_OK;
        }
        [PreserveSig]
        public int FindPin(string Id, out IPin ppPin)
        {
            IEnumerator ie = pins.GetEnumerator();
            while (ie.MoveNext())
            {
                IPin pin = (ie.Current as IPin);
                string pid;
                if (pin.QueryId(out pid) == S_OK)
                {
                    if (pid == Id)
                    {
                        ppPin = pin;
                        return S_OK;
                    }
                }
            }
            ppPin = null;
            return VFW_E_NOT_FOUND;
        }
        [PreserveSig]
        public int QueryFilterInfo(out FilterInfo pInfo)
        {
            pInfo.achName = FILTER_NAME;
            pInfo.pGraph = graph;
            return S_OK;
        }
        [PreserveSig]
        public int JoinFilterGraph(IFilterGraph pGraph, string pName)
        {
            graph = pGraph;
            return S_OK;
        }
        [PreserveSig]
        public int QueryVendorInfo(out string pVendorInfo)
        {
            pVendorInfo = VENDOR_NAME;
            return S_OK;
        }
        #endregion
    }
}
