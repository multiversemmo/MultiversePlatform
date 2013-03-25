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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Multiverse.Patcher {

    public delegate void UpdateHandler(object scriptCall);
    public delegate void InvokeHandler(object scriptCall, object methodCall);

    /// <summary>
    ///   This class wraps a call to javascript.  We pass in the C# arguments 
    ///   and the name of the javascript method to invoke.
    /// </summary>
    public class ScriptCall {
        public string method;
        public object[] args;

        public ScriptCall(string method, object[] args) {
            this.method = method;
            this.args = args;
        }

        /// <summary>
        ///   Convert the argument array in a form that is usable from javascript
        /// </summary>
        /// <returns></returns>
        public object[] GetScriptingArgs() {
            object[] rv = new object[args.Length];
            for (int i = 0; i < args.Length; ++i) {
                if (args[i] is long) {
                    long val = (long)args[i];
                    rv[i] = (double)val;
                } else {
                    rv[i] = args[i];
                }
            }
            return rv;
        }

        public override string ToString() {
            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("{0}(", this.method);
            for (int i = 0; i < this.args.Length; ++i) {
                if (i == (this.args.Length - 1))
                    msg.AppendFormat("'{0}'", this.args[i]);
                else
                    msg.AppendFormat("'{0}', ", this.args[i]);
            }
            msg.Append(")");
            return msg.ToString();
        }
    }

    /// <summary>
    ///   This class wraps a call to some method.  We pass in the arguments 
    ///   and the name of the method to invoke.
    /// </summary>
    /// <remarks>
    ///   Unlike the ScriptCall class, this class is designed for methods
    ///   that have a CLR implementation.
    /// </remarks>
    public class MethodCall {
        public Delegate method;
        public object[] args;

        public MethodCall(Delegate method, object[] args) {
            this.method = method;
            this.args = args;
        }

        public object[] GetMethodArgs() {
            return args;
        }
    }

    /// <summary>
    ///   This class is made available to the browser object.
    /// </summary>
    public class UpdateHelper {
        UpdateManager parentForm;

        public string FileFetchStartedMethod = "HandleFileFetchStarted";
        public string FileFetchEndedMethod = "HandleFileFetchEnded";
        public string FileAddedMethod = "HandleFileAdded";
        public string FileModifiedMethod = "HandleFileModified";
        public string FileIgnoredMethod = "HandleFileIgnored";
        public string FileRemovedMethod = "HandleFileRemoved";
        public string StateChangedMethod = "HandleStateChanged";
        public string UpdateAbortedMethod = "HandleUpdateAborted";
        public string UpdateStartedMethod = "HandleUpdateStarted";
        public string UpdateCompletedMethod = "HandleUpdateCompleted";
        public string UpdateProgressMethod = "HandleUpdateProgress";

        public UpdateHelper(UpdateManager parent) {
            parentForm = parent;
        }

        public void StartUpdate()
        {
            parentForm.StartUpdate();
        }

        public void AbortUpdate()
        {
            parentForm.AbortUpdate();
        }

        /// <summary>
        ///   Determine whether we need to update our media.
        /// </summary>
        /// <remarks>
        ///   This method is virtual, so that it can be overridden.  The 
        ///   LoginHelper class uses this to return false when we disable
        ///   media updates.
        /// </remarks>
        /// <returns>whether any files need to be updated</returns>
        public virtual bool NeedUpdate()
        {
            return parentForm.NeedUpdate();
        }

        public void OnLoaded() {
            parentForm.OnLoaded();
        }

        public void OK() {
            parentForm.DialogResult = DialogResult.OK;
            parentForm.Close();
        }

        public void Abort() {
            parentForm.DialogResult = DialogResult.Abort;
            parentForm.Close();
        }

        public UpdaterStatus GetUpdaterStatus()
        {
            return parentForm.UpdaterStatus;
        }

        public void HandleFileFetchStarted(object sender, UpdateFileStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(FileFetchStartedMethod, new object[] { status.file, status.length }));
        }
        // Called from the worker thread
        public void HandleFileFetchEnded(object sender, UpdateFileStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(FileFetchEndedMethod, new object[] { status.file, status.length, status.compressedLength }));
        }
        // Called from the worker thread
        public void HandleFileAdded(object sender, UpdateFileStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(FileAddedMethod, new object[] { status.file, status.length }));
        }
        // Called from the worker thread
        public void HandleFileModified(object sender, UpdateFileStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(FileModifiedMethod, new object[] { status.file, status.length }));
        }
        // Called from the worker thread
        public void HandleFileIgnored(object sender, UpdateFileStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(FileIgnoredMethod, new object[] { status.file, status.length }));
        }
        // Called from the worker thread
        public void HandleFileRemoved(object sender, UpdateFileStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(FileRemovedMethod, new object[] { status.file, status.length }));
        }
        // Called from the worker thread
        public void HandleStateChanged(object sender, int state) {
            parentForm.AsyncInvokeScript(new ScriptCall(StateChangedMethod, new object[] { state }));
        }
        // Called from the worker thread
        public void HandleUpdateAborted(object sender, UpdaterStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(UpdateAbortedMethod, new object[] { status.message }),
                                         new MethodCall(new UpdateHandler(parentForm.Abort), new object[] { status.message }));
        }
        // Called from the worker thread
        public void HandleUpdateStarted(object sender, UpdaterStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(UpdateStartedMethod, new object[] { status.bytes, status.files }));
        }
        // Called from the worker thread
        public void HandleUpdateCompleted(object sender, UpdaterStatus status) 
        {
            parentForm.AsyncInvokeScript(new ScriptCall(UpdateCompletedMethod, new object[] { status.bytes, status.files, status.bytesFetched, status.bytesTransferred }));
        }

        // Called from the worker thread
        public void HandleUpdateProgress(object sender, UpdaterStatus status)
        {
            parentForm.AsyncInvokeScript(new ScriptCall(UpdateProgressMethod, new object[] { status.bytesFetched }));
            // System.Diagnostics.Trace.Write(string.Format("HandleUpdateProgress: {0}/{1}", status.bytesFetched, status.bytes));
        }

        public Version Version
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                return assembly.GetName().Version;
            }
        }
    }
}
