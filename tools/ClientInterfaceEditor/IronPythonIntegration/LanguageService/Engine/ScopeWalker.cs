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

using SystemState = IronPython.Runtime.SystemState;
using IronPython.Hosting;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Runtime;

namespace Microsoft.Samples.VisualStudio.IronPythonInference {
    public class ScopeNode {
        private List<ScopeNode> nested;

		public IList<ScopeNode> NestedScopes {
			get { return nested; }
		}

        public virtual string Name {
            get { return ""; }
        }

        public virtual string Doc {
            get { return ""; }
        }

        public virtual Location Start {
            get {
                return Location.None;
            }
        }
        public virtual Location End {
            get {
                return Location.None;
            }
        }

        public void Add(ScopeNode node) {
            if (nested == null) nested = new List<ScopeNode>();
            nested.Add(node);
        }
    }

    public class ClassNode : ScopeNode {
        private IronPython.Compiler.Ast.ClassDefinition cls;
        public ClassNode(IronPython.Compiler.Ast.ClassDefinition cls) {
            this.cls = cls;
        }

        public override string Name {
            get {
                if (cls.Name == SymbolTable.Empty) {
                    return "";
                }
                return cls.Name.GetString();
            }
        }
        public override string Doc {
            get {
                return cls.Documentation;
            }
        }
        public override Location Start {
            get {
                return cls.Start;
            }
        }
        public override Location End {
            get {
                return cls.End;
            }
        }
    }

    public class FunctionNode : ScopeNode {
        private IronPython.Compiler.Ast.FunctionDefinition func;
        public FunctionNode(IronPython.Compiler.Ast.FunctionDefinition functionDefinition) {
			this.func = functionDefinition;
        }
        public override string Name {
            get {
                if (func.Name == SymbolTable.Empty) {
                    return "";
                }
                return func.Name.GetString();
            }
        }
        public override string Doc {
            get {
                return func.Documentation;
            }
        }
        public override Location Start {
            get {
                return func.Start;
            }
        }
        public override Location End {
            get {
                return func.End;
            }
        }
    }

    public class ScopeWalker : AstWalker {
        private static SystemState state = new SystemState();

        public static ScopeNode GetScopesFromFile(string file) {
            CompilerContext context = new CompilerContext(file, new QuietCompilerSink());
            Parser parser = Parser.FromFile(state, context);
            Statement Statement = parser.ParseFileInput();
            ScopeWalker walker = new ScopeWalker();
            return walker.WalkScopes(Statement);
        }
        public static ScopeNode GetScopesFromText(string text) {
            CompilerContext context = new CompilerContext("<input>", new QuietCompilerSink());
            Parser parser = Parser.FromString(state, context, text);
            Statement Statement = parser.ParseFileInput();
            ScopeWalker walker = new ScopeWalker();
            return walker.WalkScopes(Statement);
        }

        private ScopeNode root = new ScopeNode();
        private Stack<ScopeNode> scopes = new Stack<ScopeNode>();

        private ScopeNode WalkScopes(Statement Statement) {
            Statement.Walk(this);
            return root;
        }

        private void AddNode(ScopeNode node) {
            if (scopes.Count > 0) {
                ScopeNode current = scopes.Peek();
                current.Add(node);
            } else {
                root.Add(node);
            }

            scopes.Push(node);
        }

        #region IAstWalker Members
        public override void PostWalk(IronPython.Compiler.Ast.FunctionDefinition node) {
            scopes.Pop();
        }

        public override void PostWalk(IronPython.Compiler.Ast.ClassDefinition node) {
            scopes.Pop();
        }

        public override bool Walk(IronPython.Compiler.Ast.FunctionDefinition node) {
            FunctionNode functionNode = new FunctionNode(node);
            AddNode(functionNode);
            return true;
        }

        public override bool Walk(IronPython.Compiler.Ast.ClassDefinition node) {
            ClassNode classNode = new ClassNode(node);
            AddNode(classNode);
            return true;
        }
        #endregion
    }
}
