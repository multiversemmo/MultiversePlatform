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
using Microsoft.Windows.Design.Host;
using Microsoft.Windows.Design.Model;
using System.ComponentModel;
using System.Globalization;
using System.CodeDom.Compiler;

namespace Microsoft.Samples.VisualStudio.IronPythonProject.WPFProviders
{
    class PythonRuntimeNameProvider : RuntimeNameProvider
    {
        public override string CreateValidName(string proposal)
        {
            return proposal;
        }

        public override bool IsExistingName(string name)
        {
            //We will get uniqueness in the XAML file via the matchScope predicate.
            //In a more complete implementation, this method would verify that there isn't
            //a member in the code behind file with the given name.
            return false;
        }

        public override RuntimeNameFactory NameFactory
        {
            get { return new PythonRuntimeNameFactory(); }
        }
    }

    [Serializable]
    class PythonRuntimeNameFactory : RuntimeNameFactory
    {
        public override string CreateUniqueName(Type itemType, string proposedName, Predicate<string> matchScope, bool rootScope, RuntimeNameProvider provider)
        {
            if (null == itemType) throw new ArgumentNullException("itemType");
            if (null == matchScope) throw new ArgumentNullException("matchScope");
            if (null == provider) throw new ArgumentNullException("provider");

            string name = null;
            string baseName = proposedName;

            if (string.IsNullOrEmpty(baseName))
            {
                baseName = TypeDescriptor.GetClassName(itemType);
                int lastDot = baseName.LastIndexOf('.');
                if (lastDot != -1)
                {
                    baseName = baseName.Substring(lastDot + 1);
                }

                // Names should start with a lower-case character
                baseName = char.ToLower(baseName[0], CultureInfo.InvariantCulture) + baseName.Substring(1);
            }

            int idx = 1;
            bool isUnique = false;
            while (!isUnique)
            {
                name = string.Format(CultureInfo.InvariantCulture, "{0}{1}", baseName, idx++);

                // Test for uniqueness
                isUnique = !matchScope(name);

                string tempName = name;
                name = provider.CreateValidName(tempName);

                if (!string.Equals(name, tempName, StringComparison.Ordinal))
                {
                    // RNP has changed the name, test again for uniqueness
                    isUnique = !matchScope(name);
                }

                if (isUnique && rootScope)
                {
                    // Root name scope means we have to let the RNP test for uniqueness too
                    isUnique = !provider.IsExistingName(name);
                }
            }

            return name;
        }
    }

}
