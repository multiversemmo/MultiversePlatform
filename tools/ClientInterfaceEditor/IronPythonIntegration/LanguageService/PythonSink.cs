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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using IronPython.Compiler;
using Hosting = IronPython.Hosting;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {
    public class PythonSink : IronPython.Hosting.CompilerSink {
        AuthoringSink authoringSink;

        public PythonSink(AuthoringSink authoringSink) {
            this.authoringSink = authoringSink;
        }

        private static TextSpan CodeToText(Hosting.CodeSpan code) {
            TextSpan span = new TextSpan();
            if (code.StartLine > 0) {
                span.iStartLine = code.StartLine - 1;
            }
            span.iStartIndex = code.StartColumn;
            if (code.EndLine > 0) {
                span.iEndLine = code.EndLine - 1;
            }
            span.iEndIndex = code.EndColumn;
            return span;
        }

        public override void AddError(string path, string message, string lineText, Hosting.CodeSpan location, int errorCode, Hosting.Severity severity) {
            TextSpan span = new TextSpan();
            if (location.StartLine > 0) {
                span.iStartLine = location.StartLine - 1;
            }
            span.iStartIndex = location.StartColumn;
            if (location.EndLine > 0) {
                span.iEndLine = location.EndLine - 1;
            }
            span.iEndIndex = location.EndColumn;
            authoringSink.AddError(path, message, span, Severity.Error);
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        public override void MatchPair(Hosting.CodeSpan span, Hosting.CodeSpan endContext, int priority) {
            authoringSink.MatchPair(CodeToText(span), CodeToText(endContext), priority);
        }

        public override void EndParameters(Hosting.CodeSpan span) {
            authoringSink.EndParameters(CodeToText(span));
        }

        public override void NextParameter(Hosting.CodeSpan span) {
            authoringSink.NextParameter(CodeToText(span));
        }

        public override void QualifyName(Hosting.CodeSpan selector, Hosting.CodeSpan span, string name) {
            authoringSink.QualifyName(CodeToText(selector), CodeToText(span), name);
        }

        public override void StartName(Hosting.CodeSpan span, string name) {
            authoringSink.StartName(CodeToText(span), name);
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        public override void StartParameters(Hosting.CodeSpan span) {
            authoringSink.StartParameters(CodeToText(span));
        }
    }
}
