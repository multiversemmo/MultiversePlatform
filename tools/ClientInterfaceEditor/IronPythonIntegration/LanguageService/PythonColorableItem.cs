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

using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService
{
    public class PythonColorableItem : IVsColorableItem {

        private string displayName;
        private COLORINDEX background;
        private COLORINDEX foreground;

        public PythonColorableItem(string displayName, COLORINDEX foreground, COLORINDEX background) {
            this.displayName = displayName;
            this.background = background;
            this.foreground = foreground;
        }

        #region IVsColorableItem Members

        public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground) {
            if (null == piForeground) {
                throw new ArgumentNullException("piForeground");
            }
            if (0 == piForeground.Length) {
                throw new ArgumentOutOfRangeException("piForeground");
            }
            piForeground[0] = foreground;

            if (null == piBackground) {
                throw new ArgumentNullException("piBackground");
            }
            if (0 == piBackground.Length) {
                throw new ArgumentOutOfRangeException("piBackground");
            }
            piBackground[0] = background;

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetDefaultFontFlags(out uint pdwFontFlags) {
            pdwFontFlags = 0;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int GetDisplayName(out string pbstrName) {
            pbstrName = displayName;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        #endregion
    }
}
