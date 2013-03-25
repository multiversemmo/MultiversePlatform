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
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Windows.Forms.Design;
using System.Threading;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	[ToolboxItem(false)]
	public partial class EventEditorUserControl : UserControl
	{
        private FrameType Frame { get; set; }
        private string EventName { get; set; }
        
		private FrameXmlDesignerLoader DesignerLoader { get; set; }
        
		private IWindowsFormsEditorService edSvc;
        //private LuaInterface luaInterface;

        public EventEditorUserControl(FrameType frame, string eventName, FrameXmlDesignerLoader designerLoader, IWindowsFormsEditorService edSvc)
            : this()
        {
            Frame = frame;
            EventName = eventName;
            DesignerLoader = designerLoader;
            this.edSvc = edSvc;
            //luaInterface = new LuaInterface(this.DesignerLoader);
        }

		private EventEditorUserControl()
		{
            InitializeComponent();
        }

        public string Script
        {
            get { return textBoxScript.Text; }
            set 
            { 
                textBoxScript.Text = value;
                SetButtonProperties();
            }
        }

        private void buttonEditScript_Click(object sender, EventArgs e)
		{
            using (EventEditorForm form = new EventEditorForm())
            {
                form.Script = this.Script;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    this.Script = form.Script;
                }
            }
		}

        private void buttonCreateJump_Click(object sender, EventArgs e)
        {
            string eventHandlerName;

            if (this.Script.Trim().Length == 0)
            {
                eventHandlerName = String.Format("{0}_{1}", this.Frame.name, this.EventName);
            }
            else
            {
                Match match = regex.Match(this.Script);
                if (!match.Groups[1].Success)
                    return;
                    
                eventHandlerName = match.Groups[1].Value;
            }

            // adds the code to the textbox
            this.Script = eventHandlerName + "();";
            this.edSvc.CloseDropDown();
            
            // ugly, but required :)
            // opening of the LUA editor dismisses the event property editing.
            // DoEvents assures that property editing is finished while the lua script editing commences on a new thread.
            Application.DoEvents();
            ThreadPool.QueueUserWorkItem(new WaitCallback(EditScript), eventHandlerName);
        }

        private void EditScript(object eventHandlerNameObject)
        {
            string eventHandlerName = (string)eventHandlerNameObject;

            //luaInterface.CreateShowFunction(eventHandlerName);
        }

        private void textBoxScript_TextChanged(object sender, EventArgs e)
        {
            SetButtonProperties();
        }

        static Regex regex = new Regex(@"(?i)^\s*([A-Za-z_][A-Za-z0-9_]*)?\s*(\(\s*\))?\s*(;)?\s*$");

        private void SetButtonProperties()
        {
            Match match = regex.Match(this.Script);

            //buttonCreateJump.Enabled = luaInterface.IsValid && match.Success;
            if (buttonCreateJump.Enabled)
            {
                buttonCreateJump.Text = match.Groups[1].Success ?
                    "Jump" :
                    "Create";
            }
            else
            {
                //buttonCreateJump.Text = luaInterface.IsValid ?
                //    "Wrong name" :
                //    "No script file";
            }

            //string toolTipText = !luaInterface.IsValid ?
            //    "There is no .lua file associated with this frame xml." :
            //    !match.Success ? "This is not an event handler name." : "";

            //this.toolTip.SetToolTip(this.buttonCreateJump, toolTipText);
        }
	}
}
