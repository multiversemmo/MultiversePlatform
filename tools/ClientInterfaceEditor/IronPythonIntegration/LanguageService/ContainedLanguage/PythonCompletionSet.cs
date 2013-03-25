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
using System.Windows.Forms;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {
    internal sealed class PythonCompletionSet : CompletionSet {
        internal TextViewWrapper view;

        internal PythonCompletionSet(ImageList imageList, Source source) : base(imageList, source) {
        }

        public override void Init(IVsTextView textView, Declarations declarations, bool completeWord) {
            view = textView as TextViewWrapper;
            base.Init(textView, declarations, completeWord);
        }

        public override int GetInitialExtent(out int line, out int startIdx, out int endIdx) {
            int returnCode = base.GetInitialExtent(out line, out startIdx, out endIdx);
            if (ErrorHandler.Failed(returnCode) || (null == view)) {
                return returnCode;
            }

            TextSpan secondary = new TextSpan();
            secondary.iStartLine = secondary.iEndLine = line;
            secondary.iStartIndex = startIdx;
            secondary.iEndIndex = endIdx;

            TextSpan primary = view.GetPrimarySpan(secondary);
            line = primary.iStartLine;
            startIdx = primary.iStartIndex;
            endIdx = primary.iEndIndex;

            return returnCode;
        }
    }
}
