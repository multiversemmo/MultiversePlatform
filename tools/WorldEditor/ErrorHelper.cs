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
using System.Media;
using System.Windows.Forms;
using System.Timers;
using System.Drawing;

namespace Multiverse.Tools.WorldEditor
{
	class ErrorHelper
	{
		static object obj;
		static WorldEditor app;
		static ToolStripItem toolStripErrorMessageItem;
        static ToolStripLabel toolStripErrorMessage;
        static System.Timers.Timer errorTimeOut;
        static ElapsedEventHandler eventHandler;
        static string link;
        static bool showing = false;

		static public void SendUserError(string text, string anchor, int displayTime, bool includeBeep, object objin, WorldEditor appin)
		{
            app = appin;
            link = anchor;
			if (includeBeep)
			{
				SystemSounds.Beep.Play();
			}
			obj = objin;

            if (showing == false)
            {
                showing = true;
                toolStripErrorMessage = new ToolStripLabel(text, null, false);
                toolStripErrorMessage.ForeColor = Color.Red;
                toolStripErrorMessage.Click += toolStripErrorMessage_clicked;
                toolStripErrorMessage.IsLink = true;
                toolStripErrorMessage.ActiveLinkColor = Color.Red;
                toolStripErrorMessage.LinkBehavior = LinkBehavior.AlwaysUnderline;
                toolStripErrorMessage.LinkColor = Color.Red;
                toolStripErrorMessage.VisitedLinkColor = Color.Red;
                toolStripErrorMessageItem = toolStripErrorMessage as ToolStripItem;
                app.StatusBarAddItem(toolStripErrorMessageItem);
                timeOutErrorMessage(toolStripErrorMessageItem, displayTime);
            }
		}

		static public void timeOutErrorMessage(ToolStripItem item, int displayTime)
		{
			errorTimeOut = new System.Timers.Timer();
            eventHandler = new ElapsedEventHandler(timeOutErrorMessageEvent);
            errorTimeOut.AutoReset = false;
            errorTimeOut.Elapsed += eventHandler; 
			errorTimeOut.Interval = displayTime;
			errorTimeOut.Enabled = true;
		}

        static private void toolStripErrorMessage_clicked(object source, EventArgs e)
        {
            WorldEditor.LaunchDoc(link);
        }

		static private void timeOutErrorMessageEvent(object source, ElapsedEventArgs e)
		{
            app.StatusBarRemoveItem(toolStripErrorMessageItem);
            showing = false;
		}
	}
}
