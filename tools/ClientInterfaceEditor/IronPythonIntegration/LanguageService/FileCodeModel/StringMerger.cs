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

/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using IronPython.CodeDom;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    internal class StringMerger : IMergeDestination {
        private bool hasMerged = false;
        private List<string> buffer;

        public StringMerger(string initialText) {
            if (string.IsNullOrEmpty(initialText)) {
                buffer = new List<string>();
            } else {
                string text = initialText.Replace(Environment.NewLine, "\r");
                buffer = new List<string>(text.Split('\r'));
            }
        }

        /// <summary>
        /// Returns the text in the buffer starting from a specific line.
        /// </summary>
        internal string GetTextFromLine(int line) {
            if (line < 0) {
                throw new System.ArgumentOutOfRangeException();
            }
            StringBuilder returnText = new StringBuilder();
            for (int i = line; i < buffer.Count; ++i) {
                returnText.AppendLine(buffer[i]);
            }
            return returnText.ToString();
        }

        #region IMergeDestination members
        public void InsertRange(int start, IList<string> lines) {
            if ((null == lines) || (lines.Count == 0)) {
                hasMerged = true;
                return;
            }

            // Check the parameters.
            if (start < 0) {
                throw new System.ArgumentOutOfRangeException();
            }

            int startLine = start;
            if (startLine > buffer.Count) {
                startLine = buffer.Count;
            }
            buffer.InsertRange(startLine, lines);
            hasMerged = true;
        }

        public void RemoveRange(int start, int count) {
            // Check the parameters.
            if (start < 0) {
                throw new System.ArgumentOutOfRangeException();
            }

            for (int i=0; (i < count) && (start < buffer.Count); ++i) {
                buffer.RemoveAt(start);
            }
            hasMerged = true;
        }

        public int LineCount {
            get { return buffer.Count; }
        }

        public bool HasMerged {
            get { return hasMerged; }
        }

        public string FinalText {
            get { 
                // return back modified text
                hasMerged = false;
                StringBuilder builder = new StringBuilder();
                foreach (string line in buffer) {
                    builder.AppendLine(line);
                }
                return builder.ToString();
            }
        }
        #endregion
    }
}
