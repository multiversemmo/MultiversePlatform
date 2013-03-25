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

// System references
using System;
using System.Collections.Generic;
using System.Reflection;

// Visual Studio references.
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

// Unit test framework.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.UnitTestLibrary;

// Namespaces to test.
using Microsoft.Samples.VisualStudio.IronPythonInterfaces;
using Microsoft.Samples.VisualStudio.IronPythonConsole;

namespace Microsoft.Samples.VisualStudio.IronPythonConsole.UnitTest
{
    [TestClass]
    public class ConsoleAuthoringScopeTest
    {
        internal class TestLanguage : LanguageService
        {
            private IScanner mockScanner;
            internal IScanner MockScanner
            {
                get { return mockScanner; }
                set { mockScanner = value; }
            }

            public override string GetFormatFilterList()
            {
                return string.Empty;
            }
            public override LanguagePreferences GetLanguagePreferences()
            {
                return null;
            }
            public override IScanner GetScanner(IVsTextLines buffer)
            {
                return MockScanner;
            }
            public override AuthoringScope ParseSource(ParseRequest req)
            {
                return null;
            }
            public override string Name
            {
                get { return "TestLanguage"; }
            }
        }

        private static FieldInfo consoleEngine;
        private static void ResetScopeState()
        {
            if (null == consoleEngine)
            {
                consoleEngine = typeof(ConsoleAuthoringScope).GetField("engine", BindingFlags.NonPublic | BindingFlags.Static);
            }
            consoleEngine.SetValue(null, null);
        }

        [TestMethod]
        public void CreateScopeTest()
        {
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(null));

            ParseRequest request = new ParseRequest(false);

            request.Reason = ParseReason.None;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.MemberSelect;
            Assert.IsNotNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.HighlightBraces;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.MemberSelectAndHighlightBraces;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.MatchBraces;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.Check;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.CompleteWord;
            Assert.IsNotNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.DisplayMemberList;
            Assert.IsNotNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.QuickInfo;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.MethodTip;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.Autos;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.CodeSpan;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));

            request.Reason = ParseReason.Goto;
            Assert.IsNull(ConsoleAuthoringScope.CreateScope(request));
        }

        [TestMethod]
        public void GetDataTipTextTest()
        {
            // This method is not implemented, so the only check is that it does not
            // crashes and that returns null.
            ParseRequest request = new ParseRequest(false);
            request.Reason = ParseReason.DisplayMemberList;
            AuthoringScope scope = ConsoleAuthoringScope.CreateScope(request);
            Assert.IsNotNull(scope);
            TextSpan span;
            Assert.IsNull(scope.GetDataTipText(0, 0, out span));
        }

        [TestMethod]
        public void GetMethodsTest()
        {
            // This method is not implemented, so the only check is that it does not
            // crashes and that returns null.
            ParseRequest request = new ParseRequest(false);
            request.Reason = ParseReason.DisplayMemberList;
            AuthoringScope scope = ConsoleAuthoringScope.CreateScope(request);
            Assert.IsNotNull(scope);
            Assert.IsNull(scope.GetMethods(0, 0, ""));
        }

        [TestMethod]
        public void GotoTest()
        {
            // This method is not implemented, so the only check is that it does not
            // crashes and that returns null.
            ParseRequest request = new ParseRequest(false);
            request.Reason = ParseReason.DisplayMemberList;
            AuthoringScope scope = ConsoleAuthoringScope.CreateScope(request);
            Assert.IsNotNull(scope);
            TextSpan span;
            Assert.IsNull(scope.Goto(
                    Microsoft.VisualStudio.VSConstants.VSStd97CmdID.ClearPane,
                    null, 0, 0, out span));
        }

        [TestMethod]
        public void GetDeclarationsNullText()
        {
            // Create a mock IConsoleText that will return null on TextOfLine.
            IConsoleText consoleText = MockFactories.ConsoleTextFactory.GetInstance() as IConsoleText;
            ConsoleAuthoringScope.PythonConsole = consoleText;

            // Create the authoring scope.
            ParseRequest request = new ParseRequest(false);
            request.Reason = ParseReason.DisplayMemberList;
            AuthoringScope scope = ConsoleAuthoringScope.CreateScope(request);
            Assert.IsNotNull(scope);

            // Create object with not null value for the parameters.
            IVsTextView view = MockFactories.TextViewFactory.GetInstance() as IVsTextView;
            TokenInfo tokenInfo = new TokenInfo();

            // Call GetDeclarations.
            Declarations declarations = scope.GetDeclarations(view, 0, 0, tokenInfo, ParseReason.DisplayMemberList);
            Assert.IsTrue(0 == declarations.GetCount());
        }

        [TestMethod]
        public void GetDeclarationsNullView()
        {
            // Create the authoring scope.
            ParseRequest request = new ParseRequest(false);
            request.Reason = ParseReason.DisplayMemberList;
            AuthoringScope scope = ConsoleAuthoringScope.CreateScope(request);
            Assert.IsNotNull(scope);

            // Create a mock IConsoleText
            BaseMock mockConsoleText = MockFactories.ConsoleTextFactory.GetInstance();
            mockConsoleText.AddMethodReturnValues(
                string.Format("{0}.{1}", typeof(IConsoleText).FullName, "TextOfLine"),
                new object[] { "dte." });
            ConsoleAuthoringScope.PythonConsole = mockConsoleText as IConsoleText;

            // Creeate a TokenInfo
            TokenInfo tokenInfo = new TokenInfo();

            // Call GetDeclarations.
            bool exceptionThrown = false;
            try
            {
                scope.GetDeclarations(null, 0, 0, tokenInfo, ParseReason.DisplayMemberList);
            }
            catch (ArgumentNullException)
            {
                exceptionThrown = true;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                ArgumentNullException inner = e.InnerException as ArgumentNullException;
                if (null != inner)
                    exceptionThrown = true;
            }
            Assert.IsTrue(exceptionThrown);
        }

        private static void StandardScannerCallback(object sender, CallbackArgs args)
        {
            BaseMock mock = (BaseMock)sender;
            int iteration = (int)mock["Iteration"];
            TokenInfo[] tokens = (TokenInfo[])mock["Tokens"];
            if (tokens.Length <= iteration)
            {
                args.ReturnValue = false;
                return;
            }
            TokenInfo token = (TokenInfo)args.GetParameter(0);
            token.StartIndex = tokens[iteration].StartIndex;
            token.EndIndex = tokens[iteration].EndIndex;
            token.Trigger = tokens[iteration].Trigger;
            iteration += 1;
            mock["Iteration"] = iteration;
            args.ReturnValue = true;
        }

        private static Declarations ExecuteGetDeclarations(string lineText, IScanner scanner, IServiceProvider site)
        {
            // Create the authoring scope.
            ParseRequest request = new ParseRequest(false);
            request.Reason = ParseReason.DisplayMemberList;
            AuthoringScope scope = ConsoleAuthoringScope.CreateScope(request);
            Assert.IsNotNull(scope);

            // Create a mock IConsoleText
            BaseMock mockConsoleText = MockFactories.ConsoleTextFactory.GetInstance();
            mockConsoleText.AddMethodReturnValues(
                string.Format("{0}.{1}", typeof(IConsoleText).FullName, "TextOfLine"),
                new object[] { lineText });
            ConsoleAuthoringScope.PythonConsole = mockConsoleText as IConsoleText;

            // Create a language service.
            TestLanguage language = new TestLanguage();
            // Set the scanner for this language.
            language.MockScanner = scanner;
            ConsoleAuthoringScope.Language = language;

            // Set the site for the scope.
            ConsoleAuthoringScope.Site = site;

            // Create the view and token info to call the scope.
            IVsTextView view = MockFactories.TextViewFactory.GetInstance() as IVsTextView;
            TokenInfo tokenInfo = new TokenInfo();

            return scope.GetDeclarations(view, 0, 0, tokenInfo, ParseReason.DisplayMemberList);
        }

        [TestMethod]
        public void GetDeclarationsNoTokens()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                ResetScopeState();

                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the scanner for this test.
                BaseMock scannerMock = MockFactories.ScannerFactory.GetInstance();
                scannerMock["Iteration"] = 0;
                scannerMock["Tokens"] = new TokenInfo[0];
                scannerMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IScanner).FullName, "ScanTokenAndProvideInfoAboutIt"),
                    new EventHandler<CallbackArgs>(StandardScannerCallback));

                Declarations declarations = ExecuteGetDeclarations("dte.", scannerMock as IScanner, provider);
                Assert.IsTrue(0 == declarations.GetCount());
                Assert.IsTrue(0 == mockEngine.TotalCallsAllFunctions());
            }
        }

        [TestMethod]
        public void OneTokenNoTrigger()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                ResetScopeState();

                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the scanner for this test.
                BaseMock scannerMock = MockFactories.ScannerFactory.GetInstance();
                scannerMock["Iteration"] = 0;
                TokenInfo token = new TokenInfo();
                token.StartIndex = 0;
                token.EndIndex = 3;
                token.Trigger = TokenTriggers.None;
                scannerMock["Tokens"] = new TokenInfo[] { token };
                scannerMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IScanner).FullName, "ScanTokenAndProvideInfoAboutIt"),
                    new EventHandler<CallbackArgs>(StandardScannerCallback));

                Declarations declarations = ExecuteGetDeclarations("dte.", scannerMock as IScanner, provider);
                Assert.IsTrue(0 == declarations.GetCount());
                Assert.IsTrue(0 == mockEngine.TotalCallsAllFunctions());
            }
        }


        [TestMethod]
        public void OneTriggerNoText()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                ResetScopeState();

                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the scanner for this test.
                BaseMock scannerMock = MockFactories.ScannerFactory.GetInstance();
                scannerMock["Iteration"] = 0;
                TokenInfo token = new TokenInfo();
                token.StartIndex = 0;
                token.EndIndex = 1;
                token.Trigger = TokenTriggers.MemberSelect;
                scannerMock["Tokens"] = new TokenInfo[] { token };
                scannerMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IScanner).FullName, "ScanTokenAndProvideInfoAboutIt"),
                    new EventHandler<CallbackArgs>(StandardScannerCallback));

                Declarations declarations = ExecuteGetDeclarations(".", scannerMock as IScanner, provider);
                Assert.IsTrue(0 == mockEngine.TotalCallsAllFunctions());
            }
        }

        [TestMethod]
        public void TokensWithSpace()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                ResetScopeState();
                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the scanner for this test.
                BaseMock scannerMock = MockFactories.ScannerFactory.GetInstance();
                scannerMock["Iteration"] = 0;
                TokenInfo[] tokens = new TokenInfo[2];
                tokens[0] = new TokenInfo();
                tokens[0].StartIndex = 0;
                tokens[0].EndIndex = 2;
                tokens[0].Trigger = TokenTriggers.None;

                tokens[1] = new TokenInfo();
                tokens[1].StartIndex = 4;
                tokens[1].EndIndex = 4;
                tokens[1].Trigger = TokenTriggers.MemberSelect;

                scannerMock["Tokens"] = tokens;
                scannerMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IScanner).FullName, "ScanTokenAndProvideInfoAboutIt"),
                    new EventHandler<CallbackArgs>(StandardScannerCallback));

                Declarations declarations = ExecuteGetDeclarations("dte .", scannerMock as IScanner, provider);
                Assert.IsTrue(0 == declarations.GetCount());
                Assert.IsTrue(0 == mockEngine.TotalCallsAllFunctions());
            }
        }

        private static void EvaluateNullResultCallback(object sender, CallbackArgs args)
        {
            string param = (string)args.GetParameter(0);
            Assert.IsTrue("dir(dte)" == param);
            args.ReturnValue = null;
        }
        [TestMethod]
        public void GetDeclarationsNullResult()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                ResetScopeState();
                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
                string evaluateMethodName = string.Format("{0}.{1}", typeof(IEngine).FullName, "Evaluate");
                mockEngine.AddMethodCallback(
                    evaluateMethodName,
                    new EventHandler<CallbackArgs>(EvaluateNullResultCallback));
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the scanner for this test.
                BaseMock scannerMock = MockFactories.ScannerFactory.GetInstance();
                scannerMock["Iteration"] = 0;
                TokenInfo[] tokens = new TokenInfo[2];
                tokens[0] = new TokenInfo();
                tokens[0].StartIndex = 0;
                tokens[0].EndIndex = 2;
                tokens[0].Trigger = TokenTriggers.None;

                tokens[1] = new TokenInfo();
                tokens[1].StartIndex = 3;
                tokens[1].EndIndex = 3;
                tokens[1].Trigger = TokenTriggers.MemberSelect;

                scannerMock["Tokens"] = tokens;
                scannerMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IScanner).FullName, "ScanTokenAndProvideInfoAboutIt"),
                    new EventHandler<CallbackArgs>(StandardScannerCallback));

                Declarations declarations = ExecuteGetDeclarations("dte.", scannerMock as IScanner, provider);
                Assert.IsTrue(0 == declarations.GetCount());
                Assert.IsTrue(1 == mockEngine.FunctionCalls(evaluateMethodName));
            }
        }

        private void EvaluateCallback(object sender, CallbackArgs args)
        {
            string param = (string)args.GetParameter(0);
            Assert.IsTrue("dir(variable)" == param);
            List<string> list = new List<string>();
            list.Add("Method 1");
            list.Add("Method 2");
            args.ReturnValue = list;
        }
        [TestMethod]
        public void GetDeclarationsTwoResults()
        {
            using (OleServiceProvider provider = new OleServiceProvider())
            {
                ResetScopeState();
                // Create a mock engine provider.
                BaseMock mockEngineProvider = MockFactories.EngineProviderFactory.GetInstance();
                // Create a mock engine.
                BaseMock mockEngine = MockFactories.EngineFactory.GetInstance();
                string evaluateMethodName = string.Format("{0}.{1}", typeof(IEngine).FullName, "Evaluate");
                mockEngine.AddMethodCallback(
                    evaluateMethodName,
                    new EventHandler<CallbackArgs>(EvaluateCallback));
                // Set this engine as the one returned from the GetSharedEngine of the engine provider.
                mockEngineProvider.AddMethodReturnValues(
                    string.Format("{0}.{1}", typeof(IPythonEngineProvider), "GetSharedEngine"),
                    new object[] { (IEngine)mockEngine });
                // Add the engine provider to the list of the services.
                provider.AddService(typeof(IPythonEngineProvider), mockEngineProvider, false);

                // Create the scanner for this test.
                BaseMock scannerMock = MockFactories.ScannerFactory.GetInstance();
                scannerMock["Iteration"] = 0;
                TokenInfo[] tokens = new TokenInfo[2];
                tokens[0] = new TokenInfo();
                tokens[0].StartIndex = 0;
                tokens[0].EndIndex = 7;
                tokens[0].Trigger = TokenTriggers.None;

                tokens[1] = new TokenInfo();
                tokens[1].StartIndex = 8;
                tokens[1].EndIndex = 8;
                tokens[1].Trigger = TokenTriggers.MemberSelect;

                scannerMock["Tokens"] = tokens;
                scannerMock.AddMethodCallback(
                    string.Format("{0}.{1}", typeof(IScanner).FullName, "ScanTokenAndProvideInfoAboutIt"),
                    new EventHandler<CallbackArgs>(StandardScannerCallback));

                Declarations declarations = ExecuteGetDeclarations("variable.", scannerMock as IScanner, provider);
                Assert.IsTrue(2 == declarations.GetCount());
                Assert.IsTrue(1 == mockEngine.FunctionCalls(evaluateMethodName));
                Assert.IsTrue("Method 1" == declarations.GetDisplayText(0));
                Assert.IsTrue("Method 2" == declarations.GetDisplayText(1));
            }
        }
    }
}
