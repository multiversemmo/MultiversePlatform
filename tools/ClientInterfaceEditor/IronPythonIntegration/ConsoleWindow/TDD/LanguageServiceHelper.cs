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
using System.Reflection;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;

using Microsoft.Samples.VisualStudio.IronPythonLanguageService;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    // In order to test some aspect of the language service integration we need a mock package that
    // derives from the Package class exposed by the package framework. This kind of object can not
    // be built from a GenericMockFactory because otherwise it will also derive from BaseMock and
    // it is not possible to have an object that derives from more than one base class.
    internal class MockPackage : Package
    {
        public MockPackage()
        {
        }
    }

    // We have to create a new kind of language service in order to change the type of view filter
    // returned by CreateViewFilter. This is because the implementation of Dispose in the standard
    // filter will call Marshal.ReleaseComObject on the text view, but our mock objects are not
    // COM objects, so the call will fail.
    internal class MockLanguage : PythonLanguage
    {
        // This class implements the view filter used by this language service.
        // The only difference from the default one is in the Dispose method where we set the
        // textView member to null before calling the base's Dispose.
        private class MockFilter : ViewFilter
        {
            public MockFilter(CodeWindowManager windowManager, IVsTextView view)
                : base(windowManager, view)
            { }

            public override void Dispose()
            {
                // Clear the text view so that the base class will not call 
                // Marshal.ReleaseComObject on it.
                FieldInfo viewField = typeof(ViewFilter).GetField("textView", BindingFlags.Instance | BindingFlags.NonPublic);
                viewField.SetValue(this, null);
                base.Dispose();
            }
        }

        // Override the CreateViewFilter method so that it will create an instance of 
        // MockFilter instead of ViewFilter.
        public override ViewFilter CreateViewFilter(CodeWindowManager mgr, IVsTextView newView)
        {
            return new MockFilter(mgr, newView);
        }

    }
}
