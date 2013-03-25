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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

using Microsoft.Samples.VisualStudio.IronPythonInference;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {
    class PythonScope : AuthoringScope {
        Module module;
        LanguageService language;

        public PythonScope(Module module, LanguageService language) {
            this.module = module;
            this.language = language;
        }

        public override string GetDataTipText(int line, int col, out TextSpan span) {
            span = new TextSpan();
            return null;
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason) {
            System.Diagnostics.Debug.Print("GetDeclarations line({0}), col({1}), TokenInfo(type {2} at {3}-{4} triggers {5}), reason({6})",
                line, col, info.Type, info.StartIndex, info.EndIndex, info.Trigger, reason);

            IList<Declaration> declarations = module.GetAttributesAt(line + 1, info.StartIndex);
            PythonDeclarations pythonDeclarations = new PythonDeclarations(declarations, language);
            
            //Show snippets according to current language context
            if (IsContextRightForSnippets(line, info)) {
                ((PythonLanguage)language).AddSnippets(ref pythonDeclarations);
            }

            //Sort statement completion items in alphabetical order
            pythonDeclarations.Sort();
            return pythonDeclarations;
        }

        private bool IsContextRightForSnippets(int line, TokenInfo info) {
            IronPython.Compiler.Ast.Node node;
            Scope scope;
            module.Locate(line + 1, info.StartIndex, out node, out scope);
            bool contextOK = true;
            if (null != node) {
                if (node is IronPython.Compiler.Ast.FieldExpression || node is IronPython.Compiler.Ast.ConstantExpression) {
                    contextOK = false;
                }
            }
            return contextOK;
        }

        public override Methods GetMethods(int line, int col, string name) {
            System.Diagnostics.Debug.Print("GetMethods line({0}), col({1}), name({2})", line, col, name);

            IList<FunctionInfo> methods = module.GetMethodsAt(line + 1, col, name);
            return new PythonMethods(methods);
        }

        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span) {
            span = new TextSpan();
            return null;
        }
    }
}
