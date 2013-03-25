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
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    public class CodeDomTextPoint : TextPoint {
        int x, y;
        TextDocument parent;

        public CodeDomTextPoint(TextDocument parent, int column, int row) {
            x = column;
            y = row;
            this.parent = parent;
        }

        #region TextPoint Members

        public int AbsoluteCharOffset {
            get { throw new NotImplementedException(); }
        }

        public bool AtEndOfDocument {
            get { throw new NotImplementedException(); }
        }

        public bool AtEndOfLine {
            get { throw new NotImplementedException(); }
        }

        public bool AtStartOfDocument {
            get { return x == 1 && y == 1; }
        }

        public bool AtStartOfLine {
            get { return x == 1; }
        }

        public EditPoint CreateEditPoint() {
            return parent.CreateEditPoint(this);
            //return new CodeDomEditPoint(parent, this);
        }

        public DTE DTE {
            get { return parent.DTE; }
        }

        public int DisplayColumn {
            get { return x; }
        }

        public bool EqualTo(TextPoint Point) {
            CodeDomTextPoint tp = Point as CodeDomTextPoint;
            if (tp == null) return false;

            return tp.x == x && tp.y == y;
        }

        public bool GreaterThan(TextPoint Point) {
            CodeDomTextPoint tp = Point as CodeDomTextPoint;
            if (tp == null) return false;

            return tp.y < y || (tp.y == y && tp.x < x);
        }

        public bool LessThan(TextPoint Point) {
            CodeDomTextPoint tp = Point as CodeDomTextPoint;
            if (tp == null) return false;

            return tp.y > y || (tp.y == y && tp.x > x);
        }

        public int Line {
            get { return y; }
        }

        public int LineCharOffset {
            get { return x; }
        }

        public int LineLength {
            get { throw new NotImplementedException(); }
        }

        public TextDocument Parent {
            get { return parent; }
        }

        public bool TryToShow(vsPaneShowHow How, object PointOrCount) {
            throw new NotImplementedException();
        }

        public CodeElement get_CodeElement(vsCMElement Scope) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
