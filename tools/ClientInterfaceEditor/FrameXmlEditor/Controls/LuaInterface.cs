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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
    public class LuaInterface
    {
        public enum FindOptions
        {
            RegularExpression = EnvDTE.vsFindOptions.vsFindOptionsRegularExpression,
            Backwards = EnvDTE.vsFindOptions.vsFindOptionsBackwards
        }

		private FrameXmlDesignerLoader DesignerLoader { get; set; }
        
        private EnvDTE.TextSelection selection = null;

        private EnvDTE.TextSelection Selection
        {
            get
            {
                if (selection == null)
                    selection = OpenFile();
                return selection;
            }
        }

        public void CreateShowFunction(string eventHandlerName)
        {
            bool isFunctionDeclared = this.FindText(@"^:b*function:b+" + eventHandlerName,
                LuaInterface.FindOptions.RegularExpression);

            if (isFunctionDeclared)
            {
                // no action is necessary, FindText positions cursor on the function name
            }
            else
            {
                string comment = "--put your event handler logic here";
                string eventHandlerCode1 = String.Format(
                    "\nfunction {0}()\n\t{1}\n",
                    eventHandlerName,
                    comment);

                string eventHandlerCode2 = "end\n";

                this.AppendText(eventHandlerCode1);
                this.Ident(-1);
                this.AppendText(eventHandlerCode2);
                this.FindText(comment, LuaInterface.FindOptions.Backwards);
            }
        }


        #region public interface

        private EnvDTE.ProjectItem firstChildItem;
		public LuaInterface(FrameXmlDesignerLoader designerLoader)
        {
			this.DesignerLoader = designerLoader;
            firstChildItem = LoadFile();
        }

        public bool IsValid
        {
            get { return (firstChildItem != null); }
        }

        public void AppendText(string text)
        {
            if (Selection != null)
            {
                Selection.EndOfDocument(false);
                Selection.Text = text; 
            }
        }

        public bool FindText(string text, FindOptions findOptions)
        {
            bool found = Selection.FindText(text, (int)findOptions);
            return found;
        }

        public void Ident(int count)
        {
            if (Selection != null)
            {
                Selection.Indent(count);
            }
        }

        #endregion

        #region helper methods
        
        private EnvDTE.ProjectItem LoadFile()
        {
			IVsRunningDocumentTable rdt = (IVsRunningDocumentTable)this.DesignerLoader.Host.GetService(typeof(SVsRunningDocumentTable));

            IVsHierarchy projectHierarchy;
            uint xmlItemid;
            IntPtr xmlDocData;
            uint dwCookie;
			int ret = rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, this.DesignerLoader.DocumentMoniker, out projectHierarchy, out xmlItemid, out xmlDocData, out dwCookie);

            object firstChildValue;
            ret = projectHierarchy.GetProperty(xmlItemid, (int)__VSHPROPID.VSHPROPID_FirstChild, out firstChildValue);

            if (firstChildValue == null)
                return null;

            int fileHandle = (int)firstChildValue;
            if (fileHandle < 0)
                return null;

            object firstChildObject;
            ret = projectHierarchy.GetProperty((uint)fileHandle, (int)__VSHPROPID.VSHPROPID_ExtObject, out firstChildObject);

            EnvDTE.ProjectItem firstChildItem = (EnvDTE.ProjectItem)firstChildObject;
            
            return
                firstChildItem;
        }

        private EnvDTE.TextSelection OpenFile()
        {
            if (firstChildItem == null)
                return null;

            if (!firstChildItem.get_IsOpen(null))
            {
                EnvDTE.Window window = firstChildItem.Open(null);
                window.Visible = true;
            }
            else
            {
                firstChildItem.Document.Activate();
            }

            EnvDTE.TextSelection selection = firstChildItem.Document.Selection as EnvDTE.TextSelection;
            selection.StartOfDocument(false);

            return selection;
        }

        #endregion
    }
}
