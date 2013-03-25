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
using System.Windows.Forms;
using System.Diagnostics;

namespace Multiverse.Patcher
{
    /// <summary>
    ///   This class has a number of utility methods for use with a web 
    ///   browser control, so that we can invoke methods.
    /// </summary>
    public class WebScriptingForm : Form
    {
        internal void AsyncAbort(UpdaterStatus status)
        {
            try
            {
                this.BeginInvoke(new UpdateHandler(this.Abort), status);
            }
            catch (InvalidOperationException)
            {
                // Guess the dialog wasn't ready -- just ignore
            }
        }

        // Called from the thread that created this control
        public void Abort(object sender)
        {
            string message = sender as string;
            if (message != null)
            {
                Dialog d = new Dialog();
                d.Message = message;
                d.ShowDialog();
            }
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        // Called from the worker thread
        internal void AsyncInvokeScript(ScriptCall call)
        {
            // If we have a dialog result other than none, we have already returned from
            // the dialog, and have no guarrantee that the dialog is still around.
            try
            {
                this.BeginInvoke(new InvokeHandler(this.InvokeScript), call, null);
            }
            catch (InvalidOperationException)
            {
                // Guess the dialog wasn't ready -- just ignore
            }
        }

        // Called from the worker thread
        internal void AsyncInvokeScript(ScriptCall scriptCall, MethodCall methodCall)
        {
            // If we have a dialog result other than none, we have already returned from
            // the dialog, and have no guarrantee that the dialog is still around.
            try
            {
                this.BeginInvoke(new InvokeHandler(this.InvokeScript), scriptCall, methodCall);
            }
            catch (InvalidOperationException)
            {
                // Guess the dialog wasn't ready -- just ignore
            }
        }

        // Called from the thread that created this control
        public void InvokeScript(object call1, object call2)
        {
            lock (this)
            {
                ScriptCall scriptCall = call1 as ScriptCall;
                MethodCall methodCall = call2 as MethodCall;
                try
                {
                    object rv = WebBrowser.Document.InvokeScript(scriptCall.method, scriptCall.GetScriptingArgs());
                    if (methodCall != null && methodCall.method != null)
                    {
                        // We may want to invoke a native method as well.  Check to see if the event was handled.
                        bool doInvokeMethod = false;
                        if (rv == null)
                            doInvokeMethod = true;
                        else
                            doInvokeMethod = (bool)rv;
                        if (doInvokeMethod)
                        {
                            methodCall.method.DynamicInvoke(methodCall.GetMethodArgs());
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Assert(false, string.Format("Failed to invoke script: {0} -- {1}", scriptCall, e));
                }
            }
        }

        /// <summary>
        ///   This method needs to be overridden in derived classes to 
        ///   actually return the web browser object.
        /// </summary>
        public virtual WebBrowser WebBrowser
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
