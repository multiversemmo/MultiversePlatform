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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Multiverse.Gui
{
    public class TextBuffer
    {
        private Axiom.Gui.Elements.TextArea textArea;
            
        public class TextLine
        {
            private int typeId;
            private string text;

            public TextLine(int typeId, string text) {
                this.typeId = typeId;
                this.text = text;
            }
        }

        private int maxLines = 100;
        private List<TextLine> lines = new List<TextLine>();
        // private List lines = new List();

        public TextBuffer(Axiom.Gui.Elements.TextArea textArea) {
            this.textArea = textArea;
        }

        public TextBuffer(Axiom.Gui.Elements.TextArea textArea, int size) {
            this.textArea = textArea;
            maxLines = size;
        }

        void Update() {
        }

        void PutText(int typeId, string text) {
            TextLine tl = new TextLine(typeId, text);
            lines.Insert(0, tl);
            if (maxLines > 0 && lines.Count > maxLines)
                lines.RemoveRange(maxLines, lines.Count - maxLines);
        }

        // Fetches in the line with 0 being the most recent line
        public TextLine GetLine(int i) {
            if (i >= lines.Count)
                return null;
            return lines[i];
        }

        public void ClearBuffer() {
            lines.Clear();
        }
    }
}
