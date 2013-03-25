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

namespace Microsoft.Samples.VisualStudio.IronPythonLanguageService {

    internal static class PythonConstants {
        public const string pythonFileExtension = ".py";
        public const string pythonCodeDomProviderName = "IronPython";
        public const string packageGuidString = "1b05e2b4-7c21-4f63-910e-29fe55eb5f8b";
        public const string languageServiceGuidString = "ae8ce01a-b3ff-4c19-8c80-54669c197f2c";
        public const string libraryManagerGuidString = "56ad1b05-f296-49b3-ace0-0d150e8c1116";
        public const string libraryManagerServiceGuidString = "923b4811-26e4-4347-ac8a-244762798e1c";
        public const string intellisenseProviderGuidString = "9c1807ea-d222-4775-afa8-c092c580e451";
        public const string PLKMinEdition = "standard";
        public const string PLKCompanyName = "Microsoft Corporation";
        public const string PLKProductName = "Visual Studio Integration of IronPython Language Service";
        public const string PLKProductVersion = "1.0";
        public const int PLKResourceID = 1;
    }
}
