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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VSConstants = Microsoft.VisualStudio.VSConstants;
using Microsoft.Samples.VisualStudio.IronPythonInference;
using IronPython.Hosting;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {

    [Guid(PythonConstants.languageServiceGuidString)]
    public partial class PythonLanguage : LanguageService {
        LanguagePreferences preferences;
        PythonScanner scanner;
        Modules modules = new Modules();
        private Dictionary<IVsTextView, PythonSource> specialSources;

        // This array contains the definition of the colorable items provided by this
        // language service.
        // This specific language does not really need to provide colorable items because it
        // does not define any item different from the default ones, but the base class has
        // an empty implementation of IVsProvideColorableItems, so any language service that
        // derives from it must implement the methods of this interface, otherwise there are
        // errors when the shell loads an editor to show a file associated to this language.
        private static PythonColorableItem[] colorableItems = {
            // The first 6 items in this list MUST be these default items.
            new PythonColorableItem("Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK),
            new PythonColorableItem("Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK),
            new PythonColorableItem("Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK),
            new PythonColorableItem("String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK),
            new PythonColorableItem("Number", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK),
            new PythonColorableItem("Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK)
        };

        public PythonLanguage() {
            specialSources = new Dictionary<IVsTextView, PythonSource>();
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public override void Dispose() {
            try {
                // Clear the special sources
                foreach(PythonSource source in specialSources.Values) {
                    source.Dispose();
                }
                specialSources.Clear();

                // Dispose the preferences.
                if (null != preferences) {
                    preferences.Dispose();
                    preferences = null;
                }

                // Dispose the scanner.
                if (null != scanner) {
                    scanner.Dispose();
                    scanner = null;
                }
            }
            finally {
                base.Dispose();
            }
        }

        public void AddSpecialSource(PythonSource source, IVsTextView view) {
            specialSources.Add(view, source);
        }

        public override string Name {
            get {
                return Resources.Python;
            }
        }

        public override Source CreateSource(IVsTextLines buffer) {
            return new PythonSource(this, buffer, new Colorizer(this, buffer, GetScanner(buffer)));
        }

        public override LanguagePreferences GetLanguagePreferences() {
            if (preferences == null) {
                preferences = new LanguagePreferences(
                    this.Site, typeof(PythonLanguage).GUID, this.Name
                    );
                preferences.Init();
            }
            return preferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer) {
            if (scanner == null) {
                scanner = new PythonScanner();
            }
            return scanner;
        }

        public override AuthoringScope ParseSource(ParseRequest req) {
            if (null == req) {
                throw new ArgumentNullException("req");
            }
            Debug.Print("ParseSource at ({0}:{1}), reason {2}", req.Line, req.Col, req.Reason);
            PythonSource source = null;
            if (specialSources.TryGetValue(req.View, out source) && (null != source.ScopeCreator)) {
                return source.ScopeCreator(req);
            }
            PythonSink sink = new PythonSink(req.Sink);
            return new PythonScope(modules.AnalyzeModule(sink, req.FileName, req.Text), this);
        }

        public override string GetFormatFilterList() {
            return Resources.PythonFormatFilter;
        }

        public override System.Windows.Forms.ImageList GetImageList() {
            System.Windows.Forms.ImageList il = base.GetImageList();
            return il;
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan) {
            if (pCodeSpan != null) {
                pCodeSpan[0].iStartLine = line;
                pCodeSpan[0].iStartIndex = col;
                pCodeSpan[0].iEndLine = line;
                pCodeSpan[0].iEndIndex = col;
                if (buffer != null) {
                    int length;
                    buffer.GetLengthOfLine(line, out length);
                    pCodeSpan[0].iStartIndex = 0;
                    pCodeSpan[0].iEndIndex = length;
                }
                return Microsoft.VisualStudio.VSConstants.S_OK;
            } else {
                return Microsoft.VisualStudio.VSConstants.S_FALSE;
            }
        }

        public override void OnIdle(bool periodic) {
            Source src = GetSource(this.LastActiveTextView);
            if (src != null && src.LastParseTime == Int32.MaxValue) {
                src.LastParseTime = 0;
            }
            base.OnIdle(periodic);
        }

        // Implementation of IVsProvideColorableItems

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        public override int GetItemCount(out int count) {
            count = colorableItems.Length;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
        public override int GetColorableItem(int index, out IVsColorableItem item) {
            if (index < 1) {
                throw new ArgumentOutOfRangeException("index");
            }
            item = colorableItems[index - 1];
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        private int classNameCounter = 0;

        public override ExpansionFunction CreateExpansionFunction(ExpansionProvider provider, string functionName) {
            ExpansionFunction function = null;
            if (functionName == "GetName") {
                ++classNameCounter;
                function = new PythonGetNameExpansionFunction(provider, classNameCounter);
            }
            return function;
        }

        private List<VsExpansion> expansionsList;
        private List<VsExpansion> ExpansionsList {
            get {
                if (null != expansionsList) {
                    return expansionsList;
                }
                GetSnippets();
                return expansionsList;
            }
        }

        // Disable the "DoNotPassTypesByReference" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045")]
        public void AddSnippets(ref PythonDeclarations declarations) {
            if (null == this.ExpansionsList) {
                return;
            }
            foreach (VsExpansion expansionInfo in this.ExpansionsList) {
                declarations.AddDeclaration(new Declaration(expansionInfo));
            }
        }

        [System.Security.Permissions.SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void GetSnippets() {
            if (null == this.expansionsList) {
                this.expansionsList = new List<VsExpansion>();
            } else {
                this.expansionsList.Clear();
            }
            IVsTextManager2 textManager = Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager2;
            if (textManager == null) {
                return;
            }
            SnippetsEnumerator enumerator = new SnippetsEnumerator(textManager, GetLanguageServiceGuid());
            foreach (VsExpansion expansion in enumerator) {
                if (!string.IsNullOrEmpty(expansion.shortcut)) {
                    this.expansionsList.Add(expansion);
                }
            }
        }

        public override ViewFilter CreateViewFilter(CodeWindowManager mgr, IVsTextView newView) {
            // This call makes sure debugging events can be received
            // by our view filter.
            base.GetIVsDebugger();
            return new PythonViewFilter(mgr, newView);
        }

        internal class PythonGetNameExpansionFunction : ExpansionFunction {
            private int nameCount;

            public PythonGetNameExpansionFunction(ExpansionProvider provider, int counter)
                : base(provider) {
                nameCount = counter;
            }

            public override string GetCurrentValue() {
                string name = "MyClass";
                name += nameCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return name;
            }
        }

    }
}
