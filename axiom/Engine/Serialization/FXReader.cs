using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Axiom.Controllers;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Scripting;

namespace Axiom.Serialization
{
    /// <summary>
    /// Summary description for FXReader.
    /// </summary>
    public class FXReader
    {

        public class NameValuePair
        {
            public string name;
            public string value;
        }

        public class AssignmentValue
        {
            protected AssignmentValue()
            {
            }
            public int index;
        }

        public class AssignmentValue<T> : AssignmentValue
        {
            public AssignmentValue(int index, T value) {
                this.index = index;
                this.value = value;
            }
            public T value;
        }

        public class PassContext
        {
            public string passID;
            public string name;
            public Dictionary<string, AssignmentValue> assignments;
        }

        public class TechniqueContext
        {
            public string name;
            public List<PassContext> passes;
        }

        public class GlobalContext
        {
            public string type;
            public List<string> semantics;
        }

        public class FieldContext : GlobalContext
        {
        }

        public class StructContext
        {
            public List<string> semantics;
            public Dictionary<string, FieldContext> fields;
        }

        public class GlobalContext2 : GlobalContext
        {
        }

        public interface IInitType
        {
        }

        public class ListInit : List<object>, IInitType
        {
        }

        public class DictInit : Dictionary<string, AssignmentValue>, IInitType
        {
        }

        public class DeclarationContext : GlobalContext2
        {
            public List<string> usages;
            public Dictionary<string, Dictionary<string, string>> annotations;
            // public Dictionary<string, object> properties;
            public IInitType initializer;
        }

        public class FunctionParameterContext : GlobalContext2
        {
            public bool uniform;
            public bool isIn;
            public bool isOut;
        }

        public class FunctionContext : GlobalContext
        {
            public Dictionary<string, FunctionParameterContext> parameters;
        }

        #region Parsing Context

        public class FXScriptContext
        {
            // generated material
            public Material material;
            // components
            public Dictionary<string, FunctionContext> functions;
            public Dictionary<string, GlobalContext2> globals; // declarationcontext or functionparametercontext
            public Dictionary<string, StructContext> structs;
            public Dictionary<string, DeclarationContext> textures;
            public List<DeclarationContext> samplers;
            public List<TechniqueContext> techniques;
            public bool hasVertexProgram;
            public bool hasPixelProgram;
            // lexical state
            public string script;
            public string fileName;
            public string language;
            //public ScriptToken savedToken;
            public ScriptToken token;
            public string tokenValue;
            public int lineNo;
            public int passNo;
            public Match m;

            public void Reset()
            {
                material = null;
                functions = new Dictionary<string, FunctionContext>();
                structs = new Dictionary<string, StructContext>();
                globals = new Dictionary<string, GlobalContext2>();
                techniques = new List<TechniqueContext>();
                textures = new Dictionary<string, DeclarationContext>();
                samplers = new List<DeclarationContext>();
                hasVertexProgram = false;
                hasPixelProgram = false;
                script = null;
                tokenValue = null;
                lineNo = passNo = 0;
                m = null;
            }
        }

        protected FXScriptContext context = new FXScriptContext();

        #endregion Parsing Context

        #region Fields

        #endregion Fields

        #region Constructor

        /// <summary>
        ///		Default constructor.
        /// </summary>
        public FXReader()
        {
        }

        #endregion Constructor

        #region Exceptions

        class FXParseError : Exception
        {
            public FXParseError(string message)
                : base(message)
            {
            }
        }
        #endregion Exceptions

        #region Lexical Analyzer

        // regexp patterns for FX source lexical analyzer
        static protected string fxLexPat = @"\G\s*(" +
            "(?'lineComment'//[^\n]*)" +
            @"|(?'comment'/\*(.|\n)*?\*/)" +
            @"|(""(?'string'(.|\n)*?(?<![\\]))"")" +
            @"|(?'preprocessor'\#[^\n]*)" +
            @"|(?'usage'uniform|static|extern|volatile|inline|shared|packed)(?=\W)" +
            @"|(?'in'in)(?=\W)" +
            @"|(?'out'out)(?=\W)" +
            @"|(?'sampler_state'sampler_state)(?=\W)" +
            @"|(?'type'void|bool|string|(int|float|half|fixed)(?'dimension'\d(x\d)*)?)(?=\W)" +
            @"|(?'textureType'texture)(?=\W)" + 
            @"|(?'samplerType'sampler(?'dimension'1D|2D|3D|CUBE|RECT)?)(?=\W)" +
            @"|(?'struct'struct)(?=\W)" +
            @"|(?'technique'technique)(?=\W)" +
            @"|(?'pass'pass)(?=\W)" +
            @"|(?'interface'interface)(?=\W)" +
            @"|(?'symbol'[a-zA-Z_][a-zA-Z_0-9]*)(?=\W)" +
            @"|(?'number'[+-]?(([0-9]+)((\.[0-9]+)([eE][-+]?[0-9]+)?)?))(?'suffix'[fdhx]?)" +
            @"|(?'numberNZ'[+-]?((\.[0-9]+)([eE][-+]?[0-9]+)?))(?'suffix'[fdhx]?)" + 
            @"|(?'ops'[+\-*/?\.!])" +
            @"|(?'ops2'==|!=|<=|>=)" +
            @"|(?'colon':)" +
            @"|(?'semicolon';)" +
            @"|(?'comma',)" +
            @"|(?'equal'=)" +
            @"|(?'openParen'\()" +
            @"|(?'closeParen'\))" +
            @"|(?'openCurly'\{)" +
            @"|(?'closeCurly'\})" +
            @"|(?'openAngle'\<)" +
            @"|(?'closeAngle'\>)" +
            @"|(?'openSquare'\[)" +
            @"|(?'closeSquare'\])" +
            @"|(?'eof'$))\s*";

        protected Regex fxLexRegEx = new Regex(fxLexPat, RegexOptions.Compiled);

        public enum ScriptToken
        {
            Usage = 1, Type, Struct, Interface, Symbol, Number, String, Ops, Ops2, OpenParen, CloseParen, OpenCurly, CloseCurly,
            OpenAngle, CloseAngle, Technique, Pass, Colon, Semicolon, OpenSquare, CloseSquare, In, Out, Comma, Equal, TextureType, 
            SamplerType, Sampler_State,
            EOF, Error
        };
        static protected ScriptToken[] tokenCodes = {
            ScriptToken.Usage, ScriptToken.Type, ScriptToken.Struct, ScriptToken.Interface, ScriptToken.Symbol, ScriptToken.Number, ScriptToken.String, 
            ScriptToken.Ops, ScriptToken.Ops2, ScriptToken.OpenParen, ScriptToken.CloseParen, ScriptToken.OpenCurly, ScriptToken.CloseCurly,
            ScriptToken.OpenAngle, ScriptToken.CloseAngle, ScriptToken.Technique, ScriptToken.Pass, ScriptToken.Colon, ScriptToken.Semicolon, 
            ScriptToken.OpenSquare, ScriptToken.CloseSquare, ScriptToken.In, ScriptToken.Out, ScriptToken.Comma, ScriptToken.Equal,
            ScriptToken.TextureType, ScriptToken.SamplerType, ScriptToken.Sampler_State,
            ScriptToken.EOF
        };
        static protected string[] tokenNames = {
            "usage", "type", "struct", "interface", "symbol", "number", "string", 
            "ops", "ops2", "openParen", "closeParen", "openCurly", "closeCurly",
            "openAngle", "closeAngle", "technique", "pass", "colon", "semicolon", 
            "openSquare", "closeSquare", "in", "out", "comma", "equal", "textureType", "samplerType",
            "sampler_state", 
            "eof"
        };

        protected ScriptToken NextToken()
        {
            if (context.m == null)
                context.m = fxLexRegEx.Match(context.script);
            else
                context.m = context.m.NextMatch();

            Match m = context.m;
            do {
                if (m.Success) {
                    GroupCollection gc = m.Groups;

                    // first handle any lexical chaf, comments, preprocessor defs, etc.
                    if (gc["lineComment"].Success || gc["comment"].Success || gc["preprocessor"].Success) {
                        m = context.m = context.m.NextMatch();
                        continue;
                    }
                    // fix up ill-formed numbers (missing '0' before '.')
                    if (gc["numberNZ"].Success) {
                        context.token = ScriptToken.Number;
                        context.tokenValue = gc["numberNZ"].Value.Replace(".", "0.");
                        return context.token;
                    }
                    for (int i = 0; i < tokenNames.Length; i++)
                        if (gc[tokenNames[i]].Success) {
                            context.tokenValue = gc[tokenNames[i]].Value;
                            context.token = tokenCodes[i];
                            // Console.WriteLine("tok = {0}, val = {1}", context.token, context.tokenValue);
                            return context.token;
                        }
                }
                else {
                    throw new FXParseError("lexical analysis error");
                }
            } while (true);
        }

        protected void TokenMustBe(ScriptToken tok)
        {
            if (context.token != tok)
                throw new FXParseError(string.Format("Parse error, expected {0}, got {1}", tok, context.token));
        }

        protected void NextTokenMustBe(ScriptToken tok)
        {
            NextToken();
            TokenMustBe(tok);
        }

        protected bool IsTypeToken()
        {
            return (context.token == ScriptToken.Type ||
                context.token == ScriptToken.SamplerType ||
                context.token == ScriptToken.TextureType ||
                context.token == ScriptToken.Symbol);
        }

        protected void TokenMustBeType()
        {
            if (!IsTypeToken())
                throw new FXParseError(string.Format("Parse error, expected type, got {0}", context.token));
        }

       protected void NextTokenMustBeType()
        {
            NextToken();
            TokenMustBeType();
        }

        #endregion Lexical Analyzer

        #region Recursive-Descent Parser

        protected void FlushBlock()
        {
            // flush through a curly-bracketed block of code
            TokenMustBe(ScriptToken.OpenCurly);
            int openCount = 1;
            while (true) {
                if (NextToken() == ScriptToken.OpenCurly)
                    openCount++;
                else if (context.token == ScriptToken.CloseCurly) {
                    if (--openCount == 0)
                        return;
                }
                else if (context.token == ScriptToken.EOF)
                    throw new FXParseError("Unexpected end-of-file");
            }
        }

        protected void FlushAnnotation()
        {
            while (NextToken() != ScriptToken.CloseAngle)
                if (context.token == ScriptToken.EOF)
                    throw new FXParseError("Unexpected end-of-file");
        }

        protected string FlushStatement()
        {
            StringBuilder statement = new StringBuilder();
            while (NextToken() != ScriptToken.Semicolon) {
                if (context.token == ScriptToken.EOF)
                    throw new FXParseError("Unexpected end-of-file");
                statement.Append(context.tokenValue + " ");   // HEY!! tokenValue probably isn't right (eg, strings have their quotes stripped)
            }
            return statement.ToString();
        }

        protected Dictionary<string, object> AsmStatement(string shaderName)
        {
            NextTokenMustBe(ScriptToken.OpenCurly);
            FlushBlock();
            throw new FXParseError("inline asm shader code not yet supported");
        }

        protected Dictionary<string, string> CompileStatement(string shaderName)
        {
            Dictionary<string, string> shaderSpec = new Dictionary<string, string>();
            NextTokenMustBe(ScriptToken.Symbol); // target
            shaderSpec["target"] = context.tokenValue;
            NextTokenMustBe(ScriptToken.Symbol); // entry point
            shaderSpec["entry_point"] = context.tokenValue;
            FlushStatement();
            return shaderSpec;
        }

        protected void StateAssignment(Dictionary<string, AssignmentValue> assignments)
        {
            string stateName = context.tokenValue;
            if (context.token == ScriptToken.TextureType)
                stateName = "Texture";  // map "texture" reserved word to "Texture" state name in state assignments (!!)
            else
                TokenMustBe(ScriptToken.Symbol);

            // optional index
            int index = 0;
            if (NextToken() == ScriptToken.OpenSquare) {
                NextTokenMustBe(ScriptToken.Number);
                index = int.Parse(context.tokenValue.Trim());
                NextTokenMustBe(ScriptToken.CloseSquare);
                NextToken();
            }
            TokenMustBe(ScriptToken.Equal);

            AssignmentValue stateValue = null;
            if (stateName == "VertexShader" || stateName == "PixelShader") {
                NextTokenMustBe(ScriptToken.Symbol);
                if (context.tokenValue == "asm")
                    stateValue = new AssignmentValue<Dictionary<string, object>>(index, AsmStatement(stateName));
                else if (context.tokenValue == "compile") {
                    stateValue = new AssignmentValue<Dictionary<string, string>>(index, CompileStatement(stateName));
                    if (stateName == "VertexShader") context.hasVertexProgram = true;
                    if (stateName == "PixelShader") context.hasPixelProgram = true;
                }
                else if (context.tokenValue.ToLower().Trim() == "null") {
                    FlushStatement();
                    return;
                }
                else
                    throw new FXParseError("Shader state assignment expected \"compile\", \"null\" or \"asm\"");
            }
            else
                stateValue = new AssignmentValue<string>(index, FlushStatement());

            assignments[stateName] = stateValue;
        }

        protected PassContext Pass()
        {
            PassContext p = new PassContext();
            p.passID = "__P" + context.passNo.ToString();

            // optional pass name
            if (NextToken() == ScriptToken.Symbol) {
                p.name = context.tokenValue;
                NextToken();
            }
            // optional annotation
            if (context.token == ScriptToken.OpenAngle) {
                FlushAnnotation();
                NextToken();
            }
            // pass body
            TokenMustBe(ScriptToken.OpenCurly);
            p.assignments = new Dictionary<string, AssignmentValue>();
            while (NextToken() != ScriptToken.CloseCurly) {
                StateAssignment(p.assignments);
            }

            context.passNo += 1;
            return p;
        }

        protected void Technique()
        {
            TechniqueContext t = new TechniqueContext();
            context.techniques.Add(t);
            // optional technique name
            if (NextToken() == ScriptToken.Symbol) {
                t.name = context.tokenValue;
                NextToken();
            }
            // optional annotation
            if (context.token == ScriptToken.OpenAngle) {
                FlushAnnotation();
                NextToken();
            }
            // technique body
            TokenMustBe(ScriptToken.OpenCurly);
            t.passes = new List<PassContext>();
            while (NextToken() == ScriptToken.Pass)
                t.passes.Add(Pass());
            TokenMustBe(ScriptToken.CloseCurly);
        }

        protected List<string> Semantics()
        {
            List<string> s = new List<string>();
            do {
                NextTokenMustBe(ScriptToken.Symbol);
                s.Add(context.tokenValue);
                if (context.tokenValue == "register") {
                    NextTokenMustBe(ScriptToken.OpenParen);
                    NextTokenMustBe(ScriptToken.Symbol);
                    NextTokenMustBe(ScriptToken.CloseParen);
                }
            } while (NextToken() == ScriptToken.Colon);
            return s;
        }

        protected void FunctionDef(string type, string name)
        {
            FunctionContext f = new FunctionContext(); context.functions[name] = f;
            f.type = type;
            // function parameter list
            f.parameters = new Dictionary<string, FunctionParameterContext>();
            do {
                FunctionParameterContext p = new FunctionParameterContext();
                if (NextToken() == ScriptToken.Usage && context.tokenValue == "uniform") {
                    p.uniform = true;
                    NextToken();
                }
                if (context.token == ScriptToken.In) {
                    p.isIn = true;
                    NextToken();
                }
                else if (context.token == ScriptToken.Out) {
                    p.isOut = true;
                    NextToken();
                }
                if (IsTypeToken()) {
                    p.type = context.tokenValue;
                    NextTokenMustBe(ScriptToken.Symbol);
                    f.parameters[context.tokenValue] = p;
                    if (p.uniform)
                        context.globals[context.tokenValue] = p;
                    if (NextToken() == ScriptToken.Colon)
                        p.semantics = Semantics();
                }
                else if (context.token == ScriptToken.CloseParen)
                    break;
                else
                    throw new FXParseError("Error in function argument declaration");
            } while (context.token == ScriptToken.Comma);

            TokenMustBe(ScriptToken.CloseParen);

            // optional semantic
            if (NextToken() == ScriptToken.Colon)
                f.semantics = Semantics();

            FlushBlock();
        }

        // This method returns a list of values (for example, a bunch of floats and a string)
        protected ListInit InitializerTerm(DeclarationContext decl)
        {
            ListInit initialVal = new ListInit();
            if (context.token == ScriptToken.Number)
                initialVal.Add(StringConverter.ParseFloat(context.tokenValue));
            else if (context.token == ScriptToken.String)
                initialVal.Add(context.tokenValue);
            else if (context.token == ScriptToken.Type) {
                NextTokenMustBe(ScriptToken.OpenParen);
                do {
                    NextTokenMustBe(ScriptToken.Number);
                    initialVal.Add(StringConverter.ParseFloat(context.tokenValue));
                }
                while (NextToken() == ScriptToken.Comma);
                TokenMustBe(ScriptToken.CloseParen);
            }
            else if (decl.type == "bool")
            {
                TokenMustBe(ScriptToken.Symbol);
                // flip bool global to int & map true/false initializers to 1/0
                decl.type = "int";
                initialVal.Add(context.tokenValue == "true" ? 1 : 0);
            }
            else
                throw new FXParseError("Unrecognized variable initializer");
            return initialVal;
        }

        protected void Initializer(DeclarationContext decl)
        {
            // record initializer for given global
            if (NextToken() == ScriptToken.Sampler_State) {
                // parse sampler_state block
                NextTokenMustBe(ScriptToken.OpenCurly);
                DictInit sample_initializer = new DictInit();
                decl.initializer = sample_initializer;
                while (NextToken() != ScriptToken.CloseCurly) {
                    StateAssignment(sample_initializer);
                }
            }
            else if (context.token == ScriptToken.OpenCurly) {
                ListInit vecInit = new ListInit(); decl.initializer = vecInit;
                do {
                    if (context.token == ScriptToken.CloseCurly)
                        break;
                    NextToken();
                    vecInit.Add(InitializerTerm(decl));
                }
                while (NextToken() == ScriptToken.Comma);
                TokenMustBe(ScriptToken.CloseCurly);
            }
            else
                decl.initializer = InitializerTerm(decl);
            
            NextTokenMustBe(ScriptToken.Semicolon);
        }

        protected void Annotation(DeclarationContext global)
        {
            // record annotations for given global
            global.annotations = new Dictionary<string, Dictionary<string, string>>();
            while (NextToken() != ScriptToken.CloseAngle) {
                Dictionary<string, string> annotation = new Dictionary<string, string>();
                TokenMustBeType();
                annotation["type"] = context.tokenValue;
                NextTokenMustBe(ScriptToken.Symbol);
                global.annotations[context.tokenValue] = annotation;
                NextTokenMustBe(ScriptToken.Equal);
                annotation["value"] = FlushStatement().Trim();
            }
        }

        protected void GlobalDecl(List<string> usages)
        {
            ScriptToken typeToken = context.token;
            string type = context.tokenValue;
            NextTokenMustBe(ScriptToken.Symbol);
            string name = context.tokenValue;
            // optional array decl
            while (NextToken() == ScriptToken.OpenSquare) {
                NextTokenMustBe(ScriptToken.Number);
                NextTokenMustBe(ScriptToken.CloseSquare);
            }
            // function decl ?
            if (context.token == ScriptToken.OpenParen)
                FunctionDef(type, name);
            else {
                // no, variable decl
                DeclarationContext g = new DeclarationContext(); context.globals[name] = g;
                g.type = type;
                if (typeToken == ScriptToken.TextureType)
                    context.textures[name] = g;
                else if (typeToken == ScriptToken.SamplerType)
                    context.samplers.Add(g);
                g.usages = usages;
                // optional semantic
                if (context.token == ScriptToken.Colon)
                    g.semantics = Semantics();
                // optional annotation
                if (context.token == ScriptToken.OpenAngle) {
                    Annotation(g);
                    NextToken();
                }
                // optional initializer
                if (context.token == ScriptToken.Equal)
                    Initializer(g);

                TokenMustBe(ScriptToken.Semicolon);
            }
        }

        protected void UsageDecl()
        {
            // record type usages in list
            List<string> usages = new List<string>();
            do
                usages.Add(context.tokenValue);
            while (NextToken() == ScriptToken.Usage);
            TokenMustBeType();
            GlobalDecl(usages);
        }

        protected void StructFieldDecl(Dictionary<string, FieldContext> fields)
        {
            if (IsTypeToken()) {
                string type = context.tokenValue;
                NextTokenMustBe(ScriptToken.Symbol);
                FieldContext f = new FieldContext(); fields[context.tokenValue] = f;
                f.type = type;
                // optional array decl
                while (NextToken() == ScriptToken.OpenSquare) {
                    NextTokenMustBe(ScriptToken.Number);
                    NextTokenMustBe(ScriptToken.CloseSquare);
                }
                // optional semantic
                if (context.token == ScriptToken.Colon)
                    f.semantics = Semantics();
                // optional annotation
                if (context.token == ScriptToken.OpenAngle) {
                    FlushAnnotation();
                    NextToken();
                }
                // optional initializer
                if (context.token == ScriptToken.Equal)
                    FlushStatement();

                TokenMustBe(ScriptToken.Semicolon);
            }
            TokenMustBe(ScriptToken.Semicolon);
        }

        protected void Struct()
        {
            NextTokenMustBe(ScriptToken.Symbol);
            StructContext s = new StructContext(); context.structs[context.tokenValue] = s;
            if (NextToken() == ScriptToken.Colon)
                s.semantics = Semantics();
            TokenMustBe(ScriptToken.OpenCurly);
            Dictionary<string, FieldContext> fields = new Dictionary<string, FieldContext>(); s.fields = fields;
            while (NextToken() != ScriptToken.CloseCurly)
                StructFieldDecl(fields);
            NextTokenMustBe(ScriptToken.Semicolon);
        }

        protected void Interface()
        {
            NextTokenMustBe(ScriptToken.Symbol);
            NextToken(); FlushBlock();
            NextTokenMustBe(ScriptToken.Semicolon);
        }

        protected void TopLevelStatement()
        {
            switch (context.token) {
                case ScriptToken.Struct:    Struct(); break;
                case ScriptToken.Usage: UsageDecl(); break;
                case ScriptToken.Type:
                case ScriptToken.TextureType:
                case ScriptToken.SamplerType:
                case ScriptToken.Symbol:    GlobalDecl(null); break;  // assume symbol may be struct name
                case ScriptToken.Interface: Interface(); break;
                case ScriptToken.Technique: Technique(); break;
                default:
                    throw new FXParseError(string.Format("Parse error, unexpected {0}", context.token));
            }
        }

        #endregion Recursive-Descent Parser

        #region Material Builder

        #region Semantic-to-ParamType maps

        static protected Dictionary<string, AutoConstants> semanticToParamType = new Dictionary<string, AutoConstants>() {
        		    { "DIRECTION",        AutoConstants.LightDirection },
                    { "POSITION",         AutoConstants.LightPosition },
                    { "LIGHTDIRECTION",   AutoConstants.LightDirection },
		            { "LIGHTPOSITION",    AutoConstants.LightPosition },
        		    { "LIGHTPOSITIONOBJECTSPACE", AutoConstants.LightPositionObjectSpace },
		            { "LIGHTPOSITIONVIEWSPACE", AutoConstants.LightPositionViewSpace },
        		    { "LIGHTDIFFUSE",     AutoConstants.LightDiffuseColor },
        		    { "AMBIENT",     AutoConstants.AmbientLightColor },
		            { "LIGHTSPECULAR",    AutoConstants.LightSpecularColor },
        		    { "EYEPOSITION",      AutoConstants.CameraPosition },
		            { "EYEPOSITIONOBJECTSPACE", AutoConstants.CameraPositionObjectSpace },
		            { "UPDIRECTION",      AutoConstants.ViewUpVector },
		            { "PROJECTION",       AutoConstants.ProjectionMatrix },
		            { "VIEW",             AutoConstants.ViewMatrix },
		            { "VIEWINVERSE",      AutoConstants.InverseViewMatrix },
		            { "VIEWPROJECTION",   AutoConstants.ViewProjMatrix },
		            { "WORLD",            AutoConstants.WorldMatrix },
		            { "WORLDINVERSE",     AutoConstants.InverseWorldMatrix },
		            { "WORLDINVERSETRANSPOSE",     AutoConstants.InverseTransposeWorldMatrix },
		            { "WORLDVIEW",        AutoConstants.WorldViewMatrix },
		            { "WORLDVIEWINVERSE", AutoConstants.InverseWorldViewMatrix },
                    { "WORLDVIEWPROJECTION", AutoConstants.WorldViewProjMatrix },
        #region Potential Constant Semantics
/*
 *    potential constant semantics...
		    "objectxform", AutoConstants.,
		    "Defines the object transformation matrix", AutoConstants.,
		    "matrix", AutoConstants.,
		    "worldviewprojectioninverse", AutoConstants.,
		    "Defines the inverse world view projection matrix", AutoConstants.,
		    "matrix", AutoConstants.,
		    "worldviewprojectioninversetranspose", AutoConstants.,
		    "Defines the transpose of the inverse world view projection matrix", AutoConstants.,
		    "matrix", AutoConstants.,
		    "worldviewinversetranspose", AutoConstants.,
		    "Defines the world-view-inverse-transpose matrix.", AutoConstants.,
    ", AutoConstants.,		"matrix", AutoConstants.,
		    "worldinversetranspose", AutoConstants.,
		    "Defines the world-inverse-transpose matrix.", AutoConstants.,
		    "matrix", AutoConstants.,
		    "worldtranspose", AutoConstants.,
		    "Defines the world-transpose matrix.", AutoConstants.,
		    "matrix", AutoConstants.,
		    "viewinversetranspose", AutoConstants.,
		    "Defines the view-inverse-transpose matrix.", AutoConstants.,
		    "matrix", AutoConstants.,
		    "lookatposition", AutoConstants.,
		    "Defines the look at position in world space.", AutoConstants.,
		    "float4", AutoConstants.,
		    "lightambient", AutoConstants.,
		    "Defines the light's ambient color.", AutoConstants.,
		    "float4", AutoConstants.,
            "lightattenuation0", AutoConstants.,
		    "Defines the constant light attenuation. Light attenuation is a vector of attenuation factors.", AutoConstants.,
		    "float", AutoConstants.,
		    "lightattenuation1", AutoConstants.,
		    "Defines the linear light attenuation. Light attenuation is a vector of attenuation factors.", AutoConstants.,
		    "float", AutoConstants.,
		    "lightattenuation2", AutoConstants.,
		    "Defines the quadratic light attenuation. Light attenuation is a vector of attenuation factors.", AutoConstants.,
		    "float", AutoConstants.,
		    "lightindex", AutoConstants.,
		    "The light that is associated with the current rendering pass.", AutoConstants.,
		    "float", AutoConstants.,
		    "lightfalloff", AutoConstants.,
		    "Defines the intensity that attenuates between the bright inner cone and the outer cone of the light that emits from a spotlight.", AutoConstants.,
		    "float", AutoConstants.,
		    "lightphi", AutoConstants.,
		    "Defines the angle for the outer cone of the light that emits from a spotlight. This is calculated as cos(outer cone angle).", AutoConstants.,
		    "float", AutoConstants.,
		    "lightrange", AutoConstants.,
		    "Defines the distance, in world space, at which meshes in a scene no longer receive light emitted by that object.", AutoConstants.,
		    "float", AutoConstants.,
		    "lighttheta", AutoConstants.,
		    "Defines the radian angle for the inner cone of the light that emits from a spotlight. This is calculated as cos(inner cone angle).", AutoConstants.,
		    "float", AutoConstants.,
		    "lighttype", AutoConstants.,
		    "Defines that light type; 0=Point, 1=Spot, 2=Directional.", AutoConstants.,
		    "float", AutoConstants.,
		    "materialpower, materialspecularpower", AutoConstants.,
		    "Defines the sharpness of the specular highlights.", AutoConstants.,
    ", AutoConstants.,		"float", AutoConstants.,
		    "meshradius", AutoConstants.,
		    "Defines the bounding radius of an object, in scene units.", AutoConstants.,
		    "float", AutoConstants.,
		    "time", AutoConstants.,
		    "Defines the time measured in seconds.", AutoConstants.,
		    "float", AutoConstants.,
		    "materialopacity", AutoConstants.,
		    "Defines the material opacity", AutoConstants.,
		    "float", AutoConstants.,
		    "TargetWidth", AutoConstants.,
		    "Defines the active render target width", AutoConstants.,
		    "float", AutoConstants.,
		    "TargetHeight", AutoConstants.,
		    "Defines the active render target height", AutoConstants.,
		    "float", AutoConstants.,
		    "ViewportWidth", AutoConstants.,
		    "Defines the current viewport width", AutoConstants.,
		    "float", AutoConstants.,
		    "ViewportHeight", AutoConstants.,
		    "Defines the current viewport height", AutoConstants.,
		    "float", AutoConstants.,
		    "ViewportX", AutoConstants.,
		    "Defines the current viewport x position (in pixels)", AutoConstants.,
		    "float", AutoConstants.,
		    "ViewportY", AutoConstants.,
		    "Defines the current viewport y position (in pixels)", AutoConstants.,
		    "float", AutoConstants.,
		    "FOV", AutoConstants.,
		    "Defines the camera field of view", AutoConstants.,
    ", AutoConstants.,		"float", AutoConstants.,
		    "NearClipPlane", AutoConstants.,
		    "Defines the camera near clip plane", AutoConstants.,
		    "float", AutoConstants.,
		    "FarClipPlane", AutoConstants.,
		    "Defines the camera far clip plane", AutoConstants.,
		    "float", AutoConstants.,
		    "direction", AutoConstants.,
		    "Defines the direction of the light.", AutoConstants.,
		    "float4", AutoConstants.,
		    "materialambient", AutoConstants.,
		    "Defines the color of the ambient light.", AutoConstants.,
		    "float4", AutoConstants.,
		    "materialdiffuse", AutoConstants.,
		    "Defines the diffuse light color.", AutoConstants.,
		    "float4", AutoConstants.,
		    "materialemissive", AutoConstants.,
		    "Defines the 'glow' light color.", AutoConstants.,
		    "float4", AutoConstants.,
		    "materialspecular", AutoConstants.,
		    "Defines the specular light (reflection) color.", AutoConstants.,
		    "float4", AutoConstants.,
    ", AutoConstants.,		"materiallightambient", AutoConstants.,
		    "Defines the material's ambient reflection characteristics. This is calculated as materialambient * lightambient.", AutoConstants.,
		    "float4", AutoConstants.,
		    "materiallightdiffuse", AutoConstants.,
		    "Defines the material's diffuse reflection characteristics. This is calculated as materialdiffuse * lightdiffuse.", AutoConstants.,
		    "float4", AutoConstants.,
		    "materiallightspecular", AutoConstants.,
		    "Defines the material's specular highlight characteristics. This is calculated as materialspecular * lightspecular.", AutoConstants.,
		    "float4", AutoConstants.,
		    "objectxforminverse", AutoConstants.,
		    "Defines the inverse of the object transformation matrix", AutoConstants.,
		    "matrix", AutoConstants.,
		    "AxisRotation", AutoConstants.,
		    "Defines the rotation component of the view transfomration matrix", AutoConstants.,
		    "matrix", AutoConstants.,
		    "Shader Annotations", AutoConstants.,
		    "The following are the supported shader parameter annotations. This list is organized by data type:", AutoConstants.,
		    "Note: Annotations are not case-sensitive.", AutoConstants.,
		    "Annotation", AutoConstants.,
    ", AutoConstants.,		"Description", AutoConstants.,
		    "Data Type", AutoConstants.,
		    "Height", AutoConstants.,
		    "Sets the height of a renderable / procedural texture.", AutoConstants.,
		    "integer", AutoConstants.,
		    "Width", AutoConstants.,
    ", AutoConstants.,		"Sets the width of a renderable / procedural texture.", AutoConstants.,
		    "integer", AutoConstants.,
		    "Target", AutoConstants.,
		    "Sets the compile target for a procedurally generated texture.", AutoConstants.,
		    "integer", AutoConstants.,
		    "Depth", AutoConstants.,
		    "Sets the depth of a procedural texture.", AutoConstants.,
		    "integer", AutoConstants.,
		    "MipMap", AutoConstants.,
		    "Sets the depth of a procedural texture", AutoConstants.,
		    "bool", AutoConstants.,
		    "BumpMap", AutoConstants.,
		    "Sets the depth of a procedural texture", AutoConstants.,
		    "bool", AutoConstants.,
		    "UIMax", AutoConstants.,
		    "The maximum value to which the slider variable is set.", AutoConstants.,
		    "float", AutoConstants.,
		    "UIMin", AutoConstants.,
		    "The minimum value to which the slider variable is set.", AutoConstants.,
		    "float", AutoConstants.,
		    "Function", AutoConstants.,
		    "The entry point to a procedurally generated texture.", AutoConstants.,
		    "string", AutoConstants.,
		    "Name", AutoConstants.,
		    "The filename for a texture resource.", AutoConstants.,
		    "string", AutoConstants.,
		    "Type", AutoConstants.,
		    "The type of texture resource: 2D, cube, or volume.", AutoConstants.,
		    "string", AutoConstants.,
		    "UIName", AutoConstants.,
		    "The name that is displayed in the user interface.", AutoConstants.,
    ", AutoConstants.,		"string", AutoConstants.,
		    "UIType", AutoConstants.,
		    "The object to use in the user interface: color, slider, or image.", AutoConstants.,
		    "string", AutoConstants.,
		    "UIWidget", AutoConstants.,
		    "The object to use in the user interface: color, slider, or image.", AutoConstants.,
		    "string", AutoConstants.,
		    "The following are the supported shader pass annotations. This list is organized by data type:", AutoConstants.,
		    "Note: Annotations are not case-sensitive.", AutoConstants.,
		    "Annotation", AutoConstants.,
		    "Description", AutoConstants.,
		    "Data Type", AutoConstants.,
		    "CopyColorBuffer", AutoConstants.,
		    "Flag whilch can be used to copy the backbuffer contents into a render target before rendering.", AutoConstants.,
		    "bool", AutoConstants.,
		    "UseExistingDepthBuffer", AutoConstants.,
		    "Flag to indicate use of the existing backbuffer depth buffer, instead of a local depth buffer attached to each render target.", AutoConstants.,
		    "bool", AutoConstants.,
		    "UseExistingViewport", AutoConstants.,
		    "Flag to indicate use of the existing viewport settings (scaled if necessary), instead of full size of a render target.", AutoConstants.,
		    "bool", AutoConstants.,
		    "RenderPassGeometry", AutoConstants.,
		    "Flag to optionally skip any geometry rendering for this pass (normally used to copy backbuffer contents).", AutoConstants.,
		    "bool", AutoConstants.,
		    "UseReverseCullMode", AutoConstants.,
		    "Flag to use the reverse cull render state for pass geometry. Required to maintain behavior when rendering mirror objects, etc.", AutoConstants.,
		    "bool", AutoConstants.,
		    "ClearDepth", AutoConstants.,
		    "Sets the depth clear value tp optionally apply before rendering.", AutoConstants.,
		    "float", AutoConstants.,
		    "ClearColor", AutoConstants.,
		    "Sets the clear color to optionally apply before rendering.", AutoConstants.,
		    "float4", AutoConstants.,
		    "RenderTarget", AutoConstants.,
		    "Sets the name of the local render target to use.", AutoConstants.,
		    "string", AutoConstants.,
		    "RenderObject", AutoConstants.,
		    "Name of an Object type resource (normally a simple .x file) to be used as the rendered geometry for this pass.", AutoConstants.,
		    "string", AutoConstants.,
		    "", AutoConstants.,
		    "
*/
        #endregion
        };		    

        #endregion Semantic-to-ParamType maps

        protected string BuildGPUProgram(PassContext p, GpuProgramType type, string stateKey)
        {
            // build GPUPrograms for given technique, pass & shader type
            GpuProgram gp = null;
            Dictionary<string, AssignmentValue> assignments = p.assignments;
            if (!assignments.ContainsKey(stateKey)) {
                LogManager.Instance.Write("Missing assignment for stateKey: {0}", stateKey);
                return null;
            }
            Dictionary<string, string> s = ((AssignmentValue<Dictionary<string, string>>)assignments[stateKey]).value;
            string name = null;
            if (s != null) {
                try {
                    name = p.passID + "_" + stateKey;
                    HighLevelGpuProgram hgp = HighLevelGpuProgramManager.Instance.CreateProgram(name, context.language, type);
                    gp = hgp;
                    // set source file, target & entry point
                    hgp.SourceFile = context.fileName;
                    switch (context.language) { 
                        case "cg":
                            hgp.SetParam("profiles", s["target"]);
                            hgp.SetParam("entry_point", s["entry_point"]);
                            break;
                        case "hlsl":
                            hgp.SetParam("target", s["target"]);
                            hgp.SetParam("entry_point", s["entry_point"]);
                            hgp.SetParam("sdk_mul_compat", "true");
                            break;
                    }

                    bool supt = hgp.IsSupported;

                    // set skeletal animation option
                    gp.IsSkeletalAnimationIncluded = false;  // HEY!! how do we specify these in an FX script?
                    gp.IsMorphAnimationIncluded = false;
                    gp.PoseAnimationCount = 0;
                }
                catch (Exception ex) {
                    name = null;
                    LogManager.Instance.Write("Could not create {0} GPU program for entry_point '{1}'. error reported was: {2}.", stateKey, s["entry_point"], ex.Message);
                }
            }
            return name;
        }

        protected bool FindParamTypeForSemantic(List<string> semantics, ref AutoConstants paramType) 
        {
            // map first-recognized uniform semantic to Ogre auto-param type
            if (semantics != null)
            {
                foreach (string s in semantics)
                {
                    string us = s.ToUpper();
                    if (semanticToParamType.ContainsKey(us))
                    {
                        paramType = semanticToParamType[us];
                        return true;
                    }
                }
                StringBuilder tmp = new StringBuilder();
                foreach (string s in semantics)
                {
                    if (tmp.Length != 0)
                        tmp.Append(", ");
                    tmp.Append(s);
                }
                LogManager.Instance.Write("Unknown semantics: {0}", tmp.ToString());
            }
            return false;
        }

        protected void BuildParams(GpuProgramParameters parameters)                   
        {
            // construct auto params for all the globals with known semantics
			foreach(KeyValuePair<string, GlobalContext2> entry in context.globals) {
				string global = entry.Key;
                DeclarationContext decl = entry.Value as DeclarationContext;
                if (decl == null)
                    continue;
                if (decl.semantics != null)
                {
                    List<string> semantics = decl.semantics;
                    // global with semantics, map to Ogre paramType & construct auto-param
                    AutoConstants paramType = 0;
                    if (FindParamTypeForSemantic(decl.semantics, ref paramType))
                    {
                        Dictionary<string, Dictionary<string, string>> annotations = decl.annotations;
                        if (annotations != null && annotations.ContainsKey("Space"))
                        {
                            // found a semantic->auto param type match, set it up
                            // look for per-semantic annotation qualifiers
                            if (paramType == AutoConstants.LightPosition)
                            {
                                switch (annotations["Space"]["value"])
                                {
                                    case "View": paramType = AutoConstants.LightPositionViewSpace; break;
                                    case "Object": paramType = AutoConstants.LightPositionObjectSpace; break;
                                }
                            }
                            else if (paramType == AutoConstants.LightDirection) 
                            {
                                switch (annotations["Space"]["value"])
                                {
                                    case "View": paramType = AutoConstants.LightDirectionViewSpace; break;
                                    case "Object": paramType = AutoConstants.LightDirectionObjectSpace; break;
                                }
                            }
                        }
                    }
                                        
                    int intExtras = 0; float floatExtras = 0;
                    bool extras = false;
                    bool isFloat = false;
                    // these types require extra data
                    if (paramType == AutoConstants.LightDiffuseColor ||
                        paramType == AutoConstants.LightSpecularColor ||
                        paramType == AutoConstants.LightAttenuation ||
                        paramType == AutoConstants.LightPosition ||
                        paramType == AutoConstants.LightDirection ||
                        paramType == AutoConstants.LightPositionObjectSpace ||
                        paramType == AutoConstants.LightDirectionObjectSpace) {
                        intExtras = 0;  // HEY!! hack, defaulting light extraParams to 0
                        extras = true;
                        isFloat = false;
                    }
                    else if (paramType == AutoConstants.Time_0_X ||
                               paramType == AutoConstants.SinTime_0_X) {
                        floatExtras = 120.0F;  // HEY!! hack, default time extraParams to 120sec
                        extras = true;
                        isFloat = true;
                    }
                    // get global param index & set the param
                    try {
                        int index = parameters.GetParamIndex(global);  // may throw if the global is not actually used in the shader, ignore if so
                        if (isFloat && extras)
                            parameters.SetAutoConstant(index, paramType, floatExtras);
                        else if (extras)
                            parameters.SetAutoConstant(index, paramType, intExtras);
                        else
                            parameters.SetAutoConstant(index, paramType, 0);
                    }
                    catch {
                        // global unused in shader, ignore
                    }
                }
                else {
                    // no semantic, if it has initializers and is referrenced in shader, add a param_named for it
                    if (decl.initializer != null)
                    {                        
                        IInitType initializer = decl.initializer;
                        try {
                            int index = parameters.GetParamIndex(global);  // may throw if the global is not actually used in the shader, ignore if so
                            if (initializer is ListInit) {
                                // arraylist => scalar or vector numbers
                                List<object> vec = (List<object>)initializer;
                                int dim = vec.Count;
                                if (dim % 4 != 0)
                                    dim = dim + 4 - (dim % 4);
                                if (decl.type.IndexOf("int") != -1) {
                                    int[] buffer = new int[dim];
                                    int i;
                                    for (i = 0; i < vec.Count; i++)
                                        buffer[i] = (int)vec[i];
                                    for (; i < dim; i++)
                                        buffer[i] = 0;
                                    parameters.SetConstant(index, buffer);
                                }
                                else { // float
                                    float[] buffer = new float[dim];
                                    int i;
                                    for (i = 0; i < vec.Count; i++)
                                        buffer[i] = (float)vec[i];
                                    for (; i < dim; i++)
                                        buffer[i] = 0;
                                    parameters.SetConstant(index, buffer);
                                }
                            }
                            else if (initializer is DictInit)
                            {
                                // sampler_state
                            }
                        }
                        catch {
                            // global unused in shader, ignore
                        }
                    }
                }
            }
        }

        protected static FilterOptions filterOptionStateVal(string val)
        {
            switch (val.Trim().ToUpper()) {
                case "NONE": return FilterOptions.None;
                case "POINT": return FilterOptions.Point;
                case "LINEAR": return FilterOptions.Linear;
                case "ANISOTROPIC": return FilterOptions.Anisotropic;
            }
            return FilterOptions.None;
        }

        protected static TextureAddressing addressingModeStateVal(string val)
        {
            switch (val.Trim().ToUpper()) {
                case "WRAP": return TextureAddressing.Wrap;
                case "MIRROR": return TextureAddressing.Mirror;
                case "CLAMP": return TextureAddressing.Clamp;
                case "BORDER": return TextureAddressing.Border;
            }
            return TextureAddressing.Wrap;
        }

        protected static ColorEx colorStateVal(string val)
        {
            ColorEx color = new ColorEx();
            // !! need to parse the float4(r,g,b,a)
            /*
            string[] vals = val.Split(' ');

            color.r = ParseFloat(vals[0]);
            color.g = ParseFloat(vals[1]);
            color.b = ParseFloat(vals[2]);
            color.a = (vals.Length == 4) ? ParseFloat(vals[3]) : 1.0f;
            */

            return color;
        }

        protected void BindSamplers(Pass pass)
        {
            foreach (DeclarationContext sampler in context.samplers)
            {
                TextureUnitState tu = pass.CreateTextureUnitState();
                
                // process all sampler_state initializers, assign to sampler's TextureUnitState
                if (sampler.initializer != null)
                {
                    DictInit sample_initializer = (DictInit)sampler.initializer;
                    foreach (string texState in sample_initializer.Keys) {
                        int index = sample_initializer[texState].index;
                        string tsValue = ((AssignmentValue<string>)sample_initializer[texState]).value;
                        switch (texState.ToUpper()) {
                            case "TEXTURE": {
                                // of the form "< texture_var_name >", look for named global
                                string key = tsValue.Trim(" <>".ToCharArray());
                                if (context.globals.ContainsKey(key)) {
                                    DeclarationContext tex = (DeclarationContext)context.globals[key];
                                    // now look for texture file annotation
                                    if (tex.annotations != null)
                                    {
                                        Dictionary<string, Dictionary<string, string>> annotations = tex.annotations;
                                        string texfile = null;
                                        if (annotations.ContainsKey("name"))
                                            texfile = annotations["name"]["value"];
                                        else if (annotations.ContainsKey("ResourceName"))
                                            texfile = annotations["ResourceName"]["value"];
                                        if (texfile != null) {
                                            TextureType texType = TextureType.TwoD;
                                            switch (sampler.type) {
                                                case "sampler1D": texType = TextureType.OneD; break;
                                                case "sampler2D": texType = TextureType.TwoD; break;
                                                case "sampler3D": texType = TextureType.ThreeD; break;
                                                case "samplerCUBE": texType = TextureType.CubeMap; break;
                                            }
                                            tu.SetTextureName(texfile.Trim(), texType);
                                        }
                                    }
                                }
                                break;
                            }
                            case "MINFILTER":
                                tu.SetTextureFiltering(FilterType.Min, filterOptionStateVal(tsValue)); break;
                            case "MAGFILTER":
                                tu.SetTextureFiltering(FilterType.Mag, filterOptionStateVal(tsValue)); break;
                            case "MIPFILTER":
                                tu.SetTextureFiltering(FilterType.Mip, filterOptionStateVal(tsValue)); break;
                            case "ADDRESSU":
                                tu.SetTextureAddressingMode(addressingModeStateVal(tsValue), tu.GetTextureAddressingMode().v, tu.GetTextureAddressingMode().w); break;
                            case "ADDRESSV":
                                tu.SetTextureAddressingMode(tu.GetTextureAddressingMode().u, addressingModeStateVal(tsValue), tu.GetTextureAddressingMode().w); break;
                            case "ADDRESSW":
                                tu.SetTextureAddressingMode(tu.GetTextureAddressingMode().u, tu.GetTextureAddressingMode().v, addressingModeStateVal(tsValue)); break;
                            case "MAXANISOTROPY":
                                tu.TextureAnisotropy = int.Parse(tsValue); break;
                            case "BORDERCOLOR":
                                tu.TextureBorderColor = colorStateVal(tsValue); break;

                            // hey!! map rest of sample_state assignments to appropriate settings on TextureUnitState
                        }
                    }
                }
            }
        }

        protected static bool boolStateVal(string val)
        {
            return val.Trim().ToLower() == "true";
        }

        protected static CullingMode cullModeStateVal(string val)
        {
            switch (val.Trim().ToUpper()) {
                case "NONE": return CullingMode.None;
                case "CW":   return CullingMode.Clockwise;
                case "CCW":  return CullingMode.CounterClockwise;
            }
            return CullingMode.None;
        }

        protected static SceneBlendFactor blendStateVal(string val)
        {
            switch (val.Trim().ToUpper()) {
                case "ZERO":            return SceneBlendFactor.Zero;
                case "ONE":             return SceneBlendFactor.One;
                case "SRCCOLOR":        return SceneBlendFactor.SourceColor;
                case "INVSRCCOLOR":     return SceneBlendFactor.OneMinusSourceColor;
                case "SRCALPHA":        return SceneBlendFactor.SourceAlpha;
                case "INVSRCALPHA":     return SceneBlendFactor.OneMinusSourceAlpha;
                case "DESTALPHA":       return SceneBlendFactor.DestAlpha;
                case "INVDESTALPHA":    return SceneBlendFactor.OneMinusDestAlpha;
                case "DESTCOLOR":       return SceneBlendFactor.DestColor;
                case "INVDESTCOLOR":    return SceneBlendFactor.OneMinusDestColor;
           }
           return SceneBlendFactor.Zero;
        }
        protected void SetPassAttributes(PassContext p, Pass pass)
        {
            // set the Pass attribute by mapping state-assignments in assignment table in p
            if (p.assignments != null) {
                foreach (KeyValuePair<string, AssignmentValue> kvp in p.assignments) {
                    int index = kvp.Value.index;
                    if (!(kvp.Value is AssignmentValue<string>))
                        continue;
                    string val = ((AssignmentValue<string>)kvp.Value).value;
                    switch (kvp.Key) {  // !! bit of a mess, where is setattr() in C#??
                        // D3D9 states:
                        case "ZEnable":         pass.DepthCheck = boolStateVal(val); break;
                        case "ZWriteEnable":    pass.DepthWrite = boolStateVal(val); break;
                        case "CullMode":        pass.CullMode = cullModeStateVal(val); break;
                        case "AlphaBlendEnable":
                            if (boolStateVal(val))
                                pass.SetSceneBlending(SceneBlendFactor.SourceAlpha, SceneBlendFactor.OneMinusSourceAlpha);
                            else
                                pass.SetSceneBlending(SceneBlendFactor.One, SceneBlendFactor.Zero);
                            break;
                        case "SrcBlend": pass.SetSceneBlending(blendStateVal(val), pass.DestBlendFactor); break;
                        case "DestBlend": pass.SetSceneBlending(pass.SourceBlendFactor, blendStateVal(val)); break;
                    }
                }
            }
        }

        protected void BuildMaterial(string FXFileName)
        {
            // build material the from parse-tree
            string materialName = Path.GetFileNameWithoutExtension(FXFileName);   // for now, material name is derived from FX filename
            context.material = (Material)MaterialManager.Instance.Create(materialName);
            context.material.RemoveAllTechniques();

            // run over techniques
            foreach (TechniqueContext t in context.techniques)
            {

                // create new technique
                Technique technique = context.material.CreateTechnique();
                if (t.name != null)
                    technique.Name = t.name;

                // run over technique's passes
                foreach (PassContext p in t.passes)
                {
                    // create a new pass
                    Pass pass = technique.CreatePass();
                    if (p.name != null)
                        pass.Name = p.name;
                    SetPassAttributes(p, pass);

                    // build & ref GPU programs, vertex first
                    if (context.hasVertexProgram) {
                        string name = BuildGPUProgram(p, GpuProgramType.Vertex, "VertexShader");
                        if (name != null)
                            pass.SetVertexProgram(name);
                        BuildParams(pass.VertexProgramParameters);
                    }
                    // next for fragment/pixel
                    if (context.hasPixelProgram) {
                        string name = BuildGPUProgram(p, GpuProgramType.Fragment, "PixelShader");
                        if (name != null)
                            pass.SetFragmentProgram(name);
                        BuildParams(pass.FragmentProgramParameters);
                        BindSamplers(pass);
                    }
                }
             }
        }

        #endregion Material Builder

        #region Public Methods

        public static void ParseAllFX()
        {
            foreach (string fileName in ResourceManager.GetAllCommonNamesLike("", ".fx")) {
                Stream stream = ResourceManager.FindCommonResourceData(fileName);
                if (stream != null)
                    new FXReader().ParseScript(stream, fileName);
            }
        }

        public static void ParseAllSources(string ext)
        {
            LogManager.Instance.Write("In FXReader.ParseAllSources");

            foreach (string fileName in ResourceManager.GetAllCommonNamesLike("", ext))
            {
                LogManager.Instance.Write("In FXReader.ParseAllSources; fileName = " + fileName);
                Stream stream = ResourceManager.FindCommonResourceData(fileName);
                if (stream != null)
                    new FXReader().ParseScript(stream, fileName);
            }
        }

        public void ParseScript(Stream stream, string FXFileName)
        {
            using (StreamReader sr = new StreamReader(stream)) {
                // establish new parsing context
                context.Reset();
                context.fileName = FXFileName;
                switch (Path.GetExtension(FXFileName).ToLower()) {
                    case ".fx": context.language = "hlsl"; break;
                    case ".cgfx": context.language = "cg"; break;
                    default:
                        throw new FXParseError("Unrecognized file type (must be .fx or .cgfx)");
                }
                context.script = sr.ReadToEnd();
                
                // parse the effect file, gathering parse-tree in context
                while (NextToken() != ScriptToken.EOF)
                    TopLevelStatement();
            }

            // build the material from the parse-tree
            LogManager.Instance.Write("Building FX based material for: " + FXFileName);
            BuildMaterial(FXFileName);
        }

        #endregion
    }
}