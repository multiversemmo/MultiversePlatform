using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using log4net;
using Axiom.Media;
using Axiom.Core;
using Multiverse.Lib.LogUtil;
using System.Threading;
using Tao.DevIl;

namespace Multiverse.Lib.TextureFetcher
{
    public delegate void TextureFetchDone(string textureName, int initialWidth, int initialHeight, bool loaded);

    public class Request
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Request));

        protected string url;
        protected string textureName;
        protected static Dictionary<string, int> contentTypeMap;
        protected bool done = false;
        protected TextureFetchDone doneHandler;
        protected Thread workThread;

        // the buffer for the downloaded source image
        protected byte[] sourceBuffer = null;
        protected int ilImageType;

        protected int destWidth;
        protected int destHeight;
        protected bool keepAspect;
        protected ColorEx fillColor;

        protected string authUser = null;
        protected string authPW = null;
        protected string authDomain = null;

        static Request()
        {
            // initialize mapping of content types to IL image type
            contentTypeMap = new Dictionary<string, int>();
            contentTypeMap["image/jpeg"] = Il.IL_JPG;
            contentTypeMap["image/png"] = Il.IL_PNG;
            // gif support doesn't currently exist in devil
            //contentTypeMap["image/gif"] = Il.IL_GIF;
        }

        public Request(string url, string textureName, TextureFetchDone doneHandler, int destWidth, int destHeight, bool keepAspect, ColorEx fillColor, string authUser, string authPW, string authDomain)
        {
            this.url = url;
            if (!url.StartsWith("http:"))
            {
                log.ErrorFormat("TextureFetch: only http urls supported: {0}", url);
                throw new Exception("TextureFetch: only http urls supported: " + url);
            }
            this.textureName = textureName;
            this.doneHandler = doneHandler;
            this.destWidth = destWidth;
            this.destHeight = destHeight;
            this.keepAspect = keepAspect;
            this.fillColor = fillColor;

            this.authUser = authUser;
            this.authPW = authPW;
            this.authDomain = authDomain;
        }

        // start the loading thread
        public void Load()
        {
            workThread = new Thread(new ThreadStart(LoadImpl));
            workThread.Start();
        }

        // clean up work thread
        public void Finish()
        {
            Debug.Assert(Done);

            workThread.Join();
            workThread = null;
        }

        /// <summary>
        /// We use a separate method for this because calling iluScale directly inline
        /// was failing on the release build, with the stack getting trashed.  
        /// My(jsw) theory is that there is some problem with the jit compiler, pinvoke,
        /// or the combination of the two that was causing this problem.
        /// </summary>
        /// <param name="scaleX">destination size of the scale operation</param>
        /// <param name="scaleY">destination size of the scale operation</param>
        private void DoScale(int scaleX, int scaleY)
        {
            // set the scale function filter & scale
            Ilu.iluImageParameter(Ilu.ILU_FILTER, Ilu.ILU_BILINEAR); // .ILU_SCALE_BSPLINE);
            Ilu.iluScale(scaleX, scaleY, 1);
        }

        // create the texture and call the callback
        public void Notify()
        {
            Debug.Assert(Done);

            bool loaded = false;

            // need these in the callback for scaling images into picture frames (friendworld)
            int initialWidth = 0;
            int initialHeight = 0;

            try
            {
                if (sourceBuffer != null)
                {
                    int imageID;

                    // create and bind a new image
                    Il.ilGenImages(1, out imageID);
                    Il.ilBindImage(imageID);

                    // Put it right side up
                    Il.ilEnable(Il.IL_ORIGIN_SET);
                    Il.ilSetInteger(Il.IL_ORIGIN_MODE, Il.IL_ORIGIN_UPPER_LEFT);

                    // Keep DXTC(compressed) data if present
                    Il.ilSetInteger(Il.IL_KEEP_DXTC_DATA, Il.IL_TRUE);

                    // load the data into DevIL
                    Il.ilLoadL(this.ilImageType, sourceBuffer, sourceBuffer.Length);

                    // check for an error
                    int ilError = Il.ilGetError();

                    if (ilError != Il.IL_NO_ERROR)
                    {
                        log.ErrorFormat("TextureFetcher: Error while decoding image data: '{0}'", Ilu.iluErrorString(ilError));
                        throw new Exception("TextureFetcher: Error while decoding image data: " + Ilu.iluErrorString(ilError));
                    }

                    int ilFormat = Il.ilGetInteger(Il.IL_IMAGE_FORMAT);
                    int imageType = Il.ilGetInteger(Il.IL_IMAGE_TYPE);

                    // force conversion to 24-bit RGB image
                    if ((imageType != Il.IL_BYTE && imageType != Il.IL_UNSIGNED_BYTE) || ilFormat != Il.IL_BGR)
                    {
                        ilFormat = Il.IL_BGR;
                        imageType = Il.IL_UNSIGNED_BYTE;

                        Il.ilConvertImage(ilFormat, imageType);
                    }

                    PixelFormat format = PixelFormat.R8G8B8;
                    int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                    int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
                    int depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);
                    int numMipMaps = Il.ilGetInteger(Il.IL_NUM_MIPMAPS) + 1;
                    int numFaces = Il.ilGetInteger(Il.IL_NUM_IMAGES) + 1;

                    initialWidth = width;
                    initialHeight = height;

                    if (numFaces > 1 || numMipMaps > 1)
                    {
                        log.WarnFormat("TextureFetcher: ignoring extra faces or mipmaps: faces={0}: mipmaps={1}", numFaces, numMipMaps);
                    }

                    if (destHeight == 0)
                    {
                        destHeight = height;
                    }
                    if (destWidth == 0)
                    {
                        destWidth = width;
                    }

                    int scaleX;
                    int scaleY;
                    int topFill = 0;
                    int bottomFill = 0;
                    int leftFill = 0;
                    int rightFill = 0;

                    if (keepAspect)
                    {
                        float scaleWidthFactor = (float)width / (float)destWidth;
                        float scaleHeightFactor = (float)height / (float)destHeight;
                        if (scaleWidthFactor > scaleHeightFactor)
                        {
                            scaleX = destWidth;
                            scaleY = (int)(height / scaleWidthFactor);
                            topFill = (destHeight - scaleY) / 2;
                            bottomFill = destHeight - scaleY - topFill;
                        }
                        else
                        {
                            scaleX = (int)(width / scaleHeightFactor);
                            scaleY = destHeight;
                            leftFill = (destWidth - scaleX) / 2;
                            rightFill = destWidth - scaleX - leftFill;
                        }
                    }
                    else
                    {
                        scaleX = destWidth;
                        scaleY = destWidth;
                    }

                    // scale the image
                    if ((scaleX != width) || (scaleY != height))
                    {
                        DoScale(scaleX, scaleY);

                        width = scaleX;
                        height = scaleY;
                    }

                    int imageSize = PixelUtil.GetMemorySize(destWidth, destHeight, depth, format);

                    // set up buffer for the decoded data
                    byte[] buffer = new byte[imageSize];

                    byte fillRed = (byte)(fillColor.r * 255.0);
                    byte fillGreen = (byte)(fillColor.g * 255.0);
                    byte fillBlue = (byte)(fillColor.b * 255.0);

                    if (keepAspect)
                    {
                        // fill top
                        for (int y = 0; y < topFill; y++)
                        {
                            int offset = y * destWidth * 3;
                            for (int x = 0; x < destWidth; x++)
                            {
                                buffer[offset++] = fillBlue;
                                buffer[offset++] = fillGreen;
                                buffer[offset++] = fillRed;
                            }
                        }

                        // copy the data
                        IntPtr srcPtr = Il.ilGetData();
                        int srcOffset = 0;
                        unsafe
                        {
                            byte* srcBytes = (byte*)srcPtr.ToPointer();

                            for (int y = topFill; y < topFill + height; y++)
                            {
                                int offset = y * destWidth * 3;
                                for (int x = 0; x < leftFill; x++)
                                {
                                    buffer[offset++] = fillBlue;
                                    buffer[offset++] = fillGreen;
                                    buffer[offset++] = fillRed;
                                }

                                for (int x = 0; x < width; x++)
                                {
                                    buffer[offset++] = srcBytes[srcOffset++];
                                    buffer[offset++] = srcBytes[srcOffset++];
                                    buffer[offset++] = srcBytes[srcOffset++];
                                }

                                for (int x = 0; x < rightFill; x++)
                                {
                                    buffer[offset++] = fillBlue;
                                    buffer[offset++] = fillGreen;
                                    buffer[offset++] = fillRed;
                                }
                            }
                        }

                        // fill bottom
                        for (int y = topFill + height; y < destHeight; y++)
                        {
                            int offset = y * destWidth * 3;
                            for (int x = 0; x < destWidth; x++)
                            {
                                buffer[offset++] = fillBlue;
                                buffer[offset++] = fillGreen;
                                buffer[offset++] = fillRed;
                            }
                        }
                    }
                    else
                    {
                        // copy the data
                        IntPtr srcPtr = Il.ilGetData();
                        unsafe
                        {
                            byte* srcBytes = (byte*)srcPtr.ToPointer();
                            for (int i = 0; i < imageSize; i++)
                            {
                                buffer[i] = srcBytes[i];
                            }
                        }
                    }

                    // Restore IL state
                    Il.ilDisable(Il.IL_ORIGIN_SET);
                    Il.ilDisable(Il.IL_FORMAT_SET);

                    // we won't be needing this anymore
                    Il.ilDeleteImages(1, ref imageID);

                    Image resultImage = Image.FromDynamicImage(buffer, destWidth, destHeight, depth, format);

                    TextureManager.Instance.LoadImage(textureName, resultImage);
                    loaded = true;
                    resultImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogUtil.ExceptionLog.ErrorFormat("TextureFetcher: exception while decoding image: {0}", ex);
            }
            finally
            {
                doneHandler(textureName, initialWidth, initialHeight, loaded);
            }
            return;
        }

        public void LoadImpl()
        {
            HttpWebResponse response = null;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                if (authUser != null)
                {
                    webRequest.Credentials = new NetworkCredential(authUser, authPW, authDomain);
                }
                response = webRequest.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (contentTypeMap.ContainsKey(response.ContentType))
                    {
                        log.InfoFormat("TextureFetcher: content-type: {0}, content-length: {1}", response.ContentType, response.ContentLength);
                        Stream stream = response.GetResponseStream();
                        BinaryReader binReader = new BinaryReader(stream);
                        sourceBuffer = binReader.ReadBytes((int)response.ContentLength);
                        ilImageType = contentTypeMap[response.ContentType];
                        binReader.Close();
                        
                        log.InfoFormat("TextureFetcher: bytes read: {0}", sourceBuffer.Length);
                    }
                    else
                    {
                        log.ErrorFormat("TextureFetcher: invalid content type: {0} : {1}", response.ContentType, url);
                    }
                }
                else
                {
                    log.ErrorFormat("TextureFetcher: url not found: {0}", url);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogUtil.ExceptionLog.ErrorFormat("TextureFetcher: exception while loading image: {0}", ex);

            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                Done = true;
            }
        }

        public void Dispose()
        {
        }

        #region Properties
        //
        // the Done property is the only class member that is accessed by both threads,
        // so it is the only one that has a lock.
        //
        public bool Done
        {
            get
            {
                bool ret;
                lock (this)
                {
                    ret = done;
                }
                return ret;
            }
            set
            {
                lock (this)
                {
                    done = value;
                }
            }
        }

        public string URL
        {
            get
            {
                return url;
            }
        }

        public string TextureName
        {
            get
            {
                return textureName;
            }
        }

        public TextureFetchDone DoneHandler
        {
            get
            {
                return doneHandler;
            }
        }
        #endregion Properties
    }

    public class TextureFetcher
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(TextureFetcher));
        protected int maxWorkThreads = 4;
        protected Queue<Request> queue = new Queue<Request>();
        protected List<Request> running = new List<Request>();
        protected List<Request> done = new List<Request>();

        public TextureFetcher()
        {
        }

        #region singleton implementation
        private static TextureFetcher instance = null;

        public static TextureFetcher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TextureFetcher();
                }
                return instance;
            }
        }
        #endregion

        public void FetchTexture(string url, string textureName, TextureFetchDone doneHandler, int width, int height, bool keepAspect, ColorEx fillColor, string authUser, string authPW, string authDomain)
        {
            Request request = new Request(url, textureName, doneHandler, width, height, keepAspect, fillColor, authUser, authPW, authDomain);
            queue.Enqueue(request);
            Process();
        }

        public void FetchTexture(string url, string textureName, TextureFetchDone doneHandler, int width, int height, bool keepAspect, ColorEx fillColor)
        {
            FetchTexture(url, textureName, doneHandler, width, height, keepAspect, fillColor);
        }

        public void FetchTexture(string url, string textureName, TextureFetchDone doneHandler)
        {
            FetchTexture(url, textureName, doneHandler, 0, 0, false, ColorEx.Black);
        }

        public void Process()
        {
            // check for requests that have finished fetching
            if (running.Count > 0)
            {
                foreach (Request req in running)
                {
                    if (req.Done)
                    {
                        done.Add(req);
                    }
                }

                // handle any requests that are complete
                if (done.Count > 0)
                {
                    foreach (Request req in done)
                    {
                        req.Finish();
                        req.Notify();
                        req.Dispose();

                        running.Remove(req);
                    }
                    done.Clear();
                }
            }

            // run requests until the running list if full or the queue is empty
            while ((queue.Count > 0) && (running.Count <= maxWorkThreads))
            {
                Request req = queue.Dequeue();
                running.Add(req);
                req.Load();
            }

            // done list should always be empty
            Debug.Assert(done.Count == 0);
        }

        public int MaxWorkThreads
        {
            get
            {
                return maxWorkThreads;
            }
            set
            {
                maxWorkThreads = value;
            }
        }
    }
}
