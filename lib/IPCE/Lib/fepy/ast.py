# Copyright (c) 2006 Seo Sanghyeon

# 2006-12-01 sanxiyn Created
# 2006-12-20 sanxiyn Added try, def
# 2006-12-22 sanxiyn Added lambda, raise, from import, class
# 2006-12-28 sanxiyn Added try finally, exec
# 2006-12-29 sanxiyn Added del, yield, list comprehension

from IronPython.Compiler import CompilerContext, Parser
from IronPython.Compiler.Ast import (
    CallExpression, FromImportStatement, FunctionAttributes,
    ListComprehensionFor,
    SliceExpression, TupleExpression)

import sys

from compiler import ast
from compiler.visitor import walk
from compiler.consts import OP_APPLY, OP_ASSIGN, OP_DELETE
from compiler.consts import CO_VARARGS, CO_VARKEYWORDS

def parse(buf, mode='exec'):
    context = CompilerContext()
    parser = Parser.FromString(sys, context, buf)
    node = parser.ParseFileInput()
    tree = Transformer().parsefile(node)
    walk(tree, FlattenVisitor())
    return tree

class FlattenVisitor:

    def make_visit(cls):
        def visit(self, node):
            nodes = []
            for child in node.nodes:
                self.visit(child)
                if isinstance(child, cls):
                    nodes.extend(child.nodes)
                else:
                    nodes.append(child)
            node.nodes = nodes
        return visit

    visitStmt = make_visit(ast.Stmt)
    visitAnd = make_visit(ast.And)
    visitOr = make_visit(ast.Or)
    visitBitand = make_visit(ast.Bitand)
    visitBitor = make_visit(ast.Bitor)
    visitBitxor = make_visit(ast.Bitxor)

    def visitCompare(self, node):
        orig = node
        ops = []
        while True:
            rel, node = node.ops[0]
            if not isinstance(node, ast.Compare):
                ops.append((rel, node))
                break
            ops.append((rel, node.expr))
        orig.ops = ops

class Transformer:

    context_targets = [
        'FieldExpression',
        'IndexExpression',
        'NameExpression',
        'TupleExpression',
        'ListExpression',
    ]

    def parsefile(self, node):
        doc = node.Documentation
        body = self.transform(node)
        if doc:
            del body.nodes[0]
        return ast.Module(doc, body)

    def transform(self, node, context=OP_APPLY):
        if node is None:
            return None
        #import __main__; __main__.x = node
        typename = node.__class__.__name__
        handler = getattr(self, '_do_' + typename, None)
        if not handler:
            raise NotImplementedError('AST transform: ' + typename)
        if typename in self.context_targets:
            tree = handler(node, context)
        else:
            tree = handler(node)
        if isinstance(tree, ast.Node):
            tree.lineno = node.Start.Line
        return tree

    def transform_assign(self, node):
        return self.transform(node, OP_ASSIGN)

    def transform_delete(self, node):
        return self.transform(node, OP_DELETE)

    def transform_suite(self, node):
        if node is None:
            return None
        node = self.transform(node)
        if not isinstance(node, ast.Stmt):
            node = ast.Stmt([node])
        return node

    def transform_param(self, param):
        if isinstance(param, TupleExpression):
            params = map(self.transform_param, param.Items)
            return tuple(params)
        return str(param.Name)

    def function_info(self, function):
        if function.Parameters:
            argnames = map(self.transform_param, function.Parameters)
        else:
            argnames = ()
        if function.Defaults:
            defaults = map(self.transform, function.Defaults)
        else:
            defaults = ()
        flags = 0
        if function.Flags & FunctionAttributes.ArgumentList:
            flags |= CO_VARARGS
        if function.Flags & FunctionAttributes.KeywordDictionary:
            flags |= CO_VARKEYWORDS
        return argnames, defaults, flags

    def transform_decorator(self, node):
        if node is None:
            return None
        nodes = []
        while isinstance(node, CallExpression):
            decorator = self.transform(node.Target)
            nodes.append(decorator)
            node = node.Args[0].Expression
        return ast.Decorators(nodes)

    def _do_SuiteStatement(self, node):
        nodes = map(self.transform, node.Statements)
        return ast.Stmt(nodes)

    def _do_IfStatement(self, node):
        tests = map(self.transform, node.Tests)
        else_ = self.transform_suite(node.ElseStatement)
        return ast.If(tests, else_)

    def _do_IfStatementTest(self, node):
        test = self.transform(node.Test)
        body = self.transform_suite(node.Body)
        return test, body

    def _do_WhileStatement(self, node):
        test = self.transform(node.Test)
        body = self.transform_suite(node.Body)
        else_ = self.transform_suite(node.ElseStatement)
        return ast.While(test, body, else_)

    def _do_ForStatement(self, node):
        assign = self.transform_assign(node.Left)
        list = self.transform(node.List)
        body = self.transform_suite(node.Body)
        else_ = self.transform_suite(node.ElseStatement)
        return ast.For(assign, list, body, else_)

    def _do_TryStatement(self, node):
        body = self.transform_suite(node.Body)
        if node.Handlers:
            handlers = map(self.transform, node.Handlers)
            else_ = self.transform_suite(node.ElseStatement)
            return ast.TryExcept(body, handlers, else_)
        else:
            final = self.transform_suite(node.FinallyStatement)
            return ast.TryFinally(body, final)

    def _do_TryStatementHandler(self, node):
        test = self.transform(node.Test)
        target = self.transform_assign(node.Target)
        body = self.transform_suite(node.Body)
        return test, target, body

    def _do_FunctionDefinition(self, node):
        decorators = self.transform_decorator(node.Decorators)
        name = str(node.Name)
        argnames, defaults, flags = self.function_info(node)
        doc = node.Documentation
        code = self.transform_suite(node.Body)
        if doc:
            del code.nodes[0]
        return ast.Function(decorators, name, argnames, defaults,
            flags, doc, code)

    def _do_ClassDefinition(self, node):
        name = str(node.Name)
        bases = map(self.transform, node.Bases)
        doc = node.Documentation
        code = self.transform_suite(node.Body)
        if doc:
            del code.nodes[0]
        return ast.Class(name, bases, doc, code)

    def _do_ExpressionStatement(self, node):
        expr = self.transform(node.Expression)
        return ast.Discard(expr)

    def _do_AssertStatement(self, node):
        test = self.transform(node.Test)
        fail = self.transform(node.Message)
        return ast.Assert(test, fail)

    def _do_AssignStatement(self, node):
        nodes = map(self.transform_assign, node.Left)
        expr = self.transform(node.Right)
        return ast.Assign(nodes, expr)

    def _do_AugAssignStatement(self, node):
        target = self.transform(node.Left)
        op = node.Operator.Symbol + '='
        expr = self.transform(node.Right)
        return ast.AugAssign(target, op, expr)

    def _do_PassStatement(self, node):
        return ast.Pass()

    def _do_DelStatement(self, node):
        if len(node.Expressions) == 1:
            return self.transform_delete(node.Expressions[0])
        nodes = map(self.transform_delete, node.Expressions)
        return ast.AssTuple(nodes)

    def _do_PrintStatement(self, node):
        nodes = map(self.transform, node.Expressions)
        dest = self.transform(node.Destination)
        if node.TrailingComma:
            return ast.Print(nodes, dest)
        else:
            return ast.Printnl(nodes, dest)

    def _do_ReturnStatement(self, node):
        value = self.transform(node.Expression)
        if value is None:
            value = ast.Const(None)
        return ast.Return(value)

    def _do_YieldStatement(self, node):
        value = self.transform(node.Expression)
        return ast.Yield(value)

    def _do_RaiseStatement(self, node):
        expr1 = self.transform(node.ExceptionType)
        expr2 = self.transform(node.Value)
        expr3 = self.transform(node.TraceBack)
        return ast.Raise(expr1, expr2, expr3)

    def _do_BreakStatement(self, node):
        return ast.Break()

    def _do_ContinueStatement(self, node):
        return ast.Continue()

    def _do_ImportStatement(self, node):
        names = [name.MakeString() for name in node.Names]
        as_names = map(str, node.AsNames)
        names = zip(names, as_names)
        return ast.Import(names)

    def _do_FromImportStatement(self, node):
        modname = node.Root.MakeString()
        if node.Names is FromImportStatement.Star:
            return ast.From(modname, [('*', None)])
        names = map(str, node.Names)
        as_names = map(str, node.AsNames)
        names = zip(names, as_names)
        return ast.From(modname, names)

    def _do_GlobalStatement(self, node):
        names = map(str, node.Names)
        return ast.Global(names)

    def _do_ExecStatement(self, node):
        expr = self.transform(node.Code)
        locals = self.transform(node.Locals)
        globals = self.transform(node.Globals)
        return ast.Exec(expr, globals, locals)

    def _do_UnaryExpression(self, node):
        operator = node.Operator.Symbol
        cls = _unary_mapping[operator]
        expr = self.transform(node.Expression)
        return cls(expr)

    def _do_BinaryExpression(self, node):
        operator = node.Operator.Symbol
        left = self.transform(node.Left)
        right = self.transform(node.Right)
        if node.IsComparison():
            return ast.Compare(left, [(operator, right)])
        else:
            cls = _binary_mapping[operator]
            return cls((left, right))

    def _do_AndExpression(self, node):
        left = self.transform(node.Left)
        right = self.transform(node.Right)
        return ast.And([left, right])

    def _do_OrExpression(self, node):
        left = self.transform(node.Left)
        right = self.transform(node.Right)
        return ast.Or([left, right])

    def _do_LambdaExpression(self, node):
        function = node.Function
        argnames, defaults, flags = self.function_info(function)
        code = self.transform(function.Body.Expression)
        return ast.Lambda(argnames, defaults, flags, code)

    def _do_FieldExpression(self, node, context):
        expr = self.transform(node.Target)
        attrname = str(node.Name)
        if context == OP_APPLY:
            return ast.Getattr(expr, attrname)
        else:
            return ast.AssAttr(expr, attrname, context)

    def _do_IndexExpression(self, node, context):
        expr = self.transform(node.Target)
        index = node.Index
        if isinstance(index, SliceExpression) and index.SliceStep is None:
            lower = self.transform(index.SliceStart)
            upper = self.transform(index.SliceStop)
            return ast.Slice(expr, context, lower, upper)
        sub = self.transform(index)
        return ast.Subscript(expr, context, [sub])

    def _do_SliceExpression(self, node):
        def transform(node):
            if node is None:
                return ast.Const(None)
            return self.transform(node)
        start = transform(node.SliceStart)
        stop = transform(node.SliceStop)
        step = transform(node.SliceStep)
        return ast.Sliceobj([start, stop, step])

    def _do_CallExpression(self, node):
        target = self.transform(node.Target)
        args = []
        star_args = dstar_args = None
        for arg in node.Args:
            name = str(arg.Name)
            expr = self.transform(arg.Expression)
            if name == '*':
                star_args = expr
            elif name == '**':
                dstar_args = expr
            elif name is not None:
                args.append(ast.Keyword(name, expr))
            else:
                args.append(expr)
        return ast.CallFunc(target, args, star_args, dstar_args)

    def _do_ParenthesisExpression(self, node):
        return self.transform(node.Expression)

    def _do_TupleExpression(self, node, context):
        if context == OP_APPLY:
            nodes = map(self.transform, node.Items)
            return ast.Tuple(nodes)
        else:
            transform = lambda node: self.transform(node, context)
            nodes = map(transform, node.Items)
            return ast.AssTuple(nodes)

    def _do_ListExpression(self, node, context):
        if context == OP_APPLY:
            nodes = map(self.transform, node.Items)
            return ast.List(nodes)
        else:
            transform = lambda node: self.transform(node, context)
            nodes = map(transform, node.Items)
            return ast.AssList(nodes)

    def _do_ListComprehension(self, node):
        expr = self.transform(node.Item)
        quals = []
        for iterator in node.Iterators:
            if isinstance(iterator, ListComprehensionFor):
                _for = self.transform(iterator)
                quals.append(_for)
            else:
                _if = self.transform(iterator)
                _for.ifs.append(_if)
        return ast.ListComp(expr, quals)

    def _do_ListComprehensionFor(self, node):
        assign = self.transform_assign(node.Left)
        list = self.transform(node.List)
        return ast.ListCompFor(assign, list, [])

    def _do_ListComprehensionIf(self, node):
        test = self.transform(node.Test)
        return ast.ListCompIf(test)

    def _do_DictionaryExpression(self, node):
        items = []
        for item in node.Items:
            key = self.transform(item.SliceStart)
            value = self.transform(item.SliceStop)
            items.append((key, value))
        return ast.Dict(items)

    def _do_BackQuoteExpression(self, node):
        expr = self.transform(node.Expression)
        return ast.Backquote(expr)

    def _do_ConstantExpression(self, node):
        value = node.Value
        if value is None:
            return ast.Name('None')
        return ast.Const(value)

    def _do_NameExpression(self, node, context):
        name = str(node.Name)
        if context == OP_APPLY:
            return ast.Name(name)
        else:
            return ast.AssName(name, context)

_unary_mapping = {
    'not': ast.Not,
    '+': ast.UnaryAdd,
    '-': ast.UnarySub,
    '~': ast.Invert,
}

_binary_mapping = {
    '**': ast.Power,
    '+': ast.Add,
    '-': ast.Sub,
    '*': ast.Mul,
    '/': ast.Div,
    '//': ast.FloorDiv,
    '%': ast.Mod,
    '<<': ast.LeftShift,
    '>>': ast.RightShift,
    '&': ast.Bitand,
    '|': ast.Bitor,
    '^': ast.Bitxor,
}

def install():
    import compiler
    compiler.parse = parse
    compiler.walk = walk
