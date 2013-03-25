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
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Multiverse.Patcher
{
#if NOT
    // right now, i don't think i will need this class, but keep it around for a bit, just in case (robin@multiverse.net)

    /// <summary>
    ///   Variant of UpdateManager that is designed for updating a world.
    ///   The UpdateManager base class is used for both world updates and
    ///   client updates.
    /// </summary>
    public class WorldUpdateManager : UpdateManager
    {
        string worldId;

        // This is designed so that I can update any appropriate parameters 
        // when the world changes.  Right now, this is used by the LoginForm 
        // so that when the world changes, the client updates the various urls 
        // and directories that we use.
        public event EventHandler WorldChanged;

        protected void OnWorldChanged(EventArgs args)
        {
            if (WorldChanged != null)
                WorldChanged(this, args);
        }

        public string WorldId {
            get {
                return worldId;
            }
            set {
                if (worldId != value)
                {
                    worldId = value;
                    OnWorldChanged(new EventArgs());
                }
            }
        }
    }
#endif

    public class UpdateManager : WebScriptingForm
    {
        protected UpdateHelper updateHelper;
        /// <summary>
        ///   This flag indicates whether we should automatically start the 
        ///   update when the OnLoaded method is called.
        /// </summary>
        bool autoStart = false;
        Updater updater;
        Thread updaterThread;

        public UpdateManager()
        {
            updateHelper = new UpdateHelper(this);
            this.Updater = new Updater();
        }

        /// <summary>
        ///   Initialize the updater
        /// </summary>
        /// <param name="url">This is the url that will be loaded in the web browser control</param>
        /// <param name="autoStart">If this flag is set, we will start updating as soon as the browser page OnLoaded hook is called</param>
        public void Initialize(string url, bool autoStart)
        {
            this.autoStart = autoStart;
            WebBrowser.ObjectForScripting = updateHelper;
            WebBrowser.Navigate(url);
        }

        public void StartUpdate()
        {
            // Just in case, abort any existing update
            AbortUpdate();
            // Start the thread
            updaterThread = new Thread(new ThreadStart(updater.Update));
            updaterThread.Name = "Resource Loader";
            updaterThread.Start();
        }

        public void AbortUpdate()
        {
            if (updaterThread != null)
            {
                bool done = updaterThread.Join(0);
                if (!done)
                    updaterThread.Abort();
                updaterThread = null;
            }
        }

        public bool NeedUpdate()
        {
            return updater.CheckVersion();
        }

        public void OnLoaded()
        {
            // The page has been loaded, time to start the updater thread.
            if (autoStart)
                StartUpdate();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AbortUpdate();
            updater.CloseLog();
            base.OnFormClosed(e);
        }

        public UpdaterStatus UpdaterStatus
        {
            get
            {
                return updater.UpdaterStatus;
            }
        }

        public Updater Updater
        {
            get
            {
                return updater;
            }
            set
            {
                if (updater == value)
                    return;
                if (updater != null)
                {
                    updater.UpdateAborted -= updateHelper.HandleUpdateAborted;
                    updater.UpdateStarted -= updateHelper.HandleUpdateStarted;
                    updater.UpdateCompleted -= updateHelper.HandleUpdateCompleted;
                    updater.FileFetchStarted -= updateHelper.HandleFileFetchStarted;
                    updater.FileFetchEnded -= updateHelper.HandleFileFetchEnded;
                    updater.FileAdded -= updateHelper.HandleFileAdded;
                    updater.FileRemoved -= updateHelper.HandleFileRemoved;
                    updater.FileModified -= updateHelper.HandleFileModified;
                    updater.FileIgnored -= updateHelper.HandleFileIgnored;
                    updater.StateChanged -= updateHelper.HandleStateChanged;
                    updater.UpdateProgress -= updateHelper.HandleUpdateProgress;
                }
                updater = value;
                if (updater != null)
                {
                    updater.UpdateAborted += updateHelper.HandleUpdateAborted;
                    updater.UpdateStarted += updateHelper.HandleUpdateStarted;
                    updater.UpdateCompleted += updateHelper.HandleUpdateCompleted;
                    updater.FileFetchStarted += updateHelper.HandleFileFetchStarted;
                    updater.FileFetchEnded += updateHelper.HandleFileFetchEnded;
                    updater.FileAdded += updateHelper.HandleFileAdded;
                    updater.FileRemoved += updateHelper.HandleFileRemoved;
                    updater.FileModified += updateHelper.HandleFileModified;
                    updater.FileIgnored += updateHelper.HandleFileIgnored;
                    updater.StateChanged += updateHelper.HandleStateChanged;
                    updater.UpdateProgress += updateHelper.HandleUpdateProgress;
                }
            }
        }

        public Thread UpdaterThread
        {
            get
            {
                return updaterThread;
            }
        }

    }
}
