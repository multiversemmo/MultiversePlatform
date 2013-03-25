using System;

namespace Axiom.RenderSystems.OpenGL.ATI {
    /// <summary>
    ///     Structure used to build rule paths.
    /// </summary>
    public struct TokenRule {
        public OperationType operation;
        public Symbol tokenID;
        public string symbol;
        public int errorID;

        public TokenRule(OperationType op) {
            this.operation = op;
            tokenID = 0;
            symbol = "";
            errorID = 0;
        }

        public TokenRule(OperationType op, Symbol tokenID) {
            this.operation = op;
            this.tokenID = tokenID;
            symbol = "";
            errorID = 0;
        }

        public TokenRule(OperationType op, Symbol tokenID, string symbol) {
            this.operation = op;
            this.tokenID = tokenID;
            this.symbol = symbol;
            errorID = 0;
        }
    }

    /// <summary>
    ///     Structure used to build Symbol Type library.
    /// </summary>
    public struct SymbolDef {
        /// <summary>
        ///     Token ID which is the index into the Token Type library.
        /// </summary>
        public Symbol ID;
        /// <summary>
        ///     Data used by pass 2 to build native instructions.
        /// </summary>
        public int pass2Data;
        /// <summary>
        ///     Context key to fit the Active Context.
        /// </summary>
        public uint contextKey;
        /// <summary>
        ///     New pattern to set for Active Context bits.
        /// </summary>
        public uint contextPatternSet;
        /// <summary>
        ///     Contexts bits to clear Active Context bits.
        /// </summary>
        public uint contextPatternClear;
        /// <summary>
        ///     Index into text table for default name : set at runtime.
        /// </summary>
        public int defTextID;
        /// <summary>
        ///     Index into Rule database for non-terminal toke rulepath.
        ///     Note: If RuleID is zero the token is terminal.
        /// </summary>
        public int ruleID;

        public SymbolDef(Symbol symbol, int glEnum, ContextKeyPattern ckp) {
            this.ID = symbol;
            this.pass2Data = glEnum;
            this.contextKey = (uint)ckp;
            contextPatternSet = 0;
            contextPatternClear = 0;
            defTextID = 0;
            ruleID = 0;
        }

        public SymbolDef(Symbol symbol, int glEnum, ContextKeyPattern ckp, uint cps) {
            this.ID = symbol;
            this.pass2Data = glEnum;
            this.contextKey = (uint)ckp;
            this.contextPatternSet = cps;
            contextPatternClear = 0;
            defTextID = 0;
            ruleID = 0;
        }

        public SymbolDef(Symbol symbol, int glEnum, ContextKeyPattern ckp, ContextKeyPattern cps) {
            this.ID = symbol;
            this.pass2Data = glEnum;
            this.contextKey = (uint)ckp;
            this.contextPatternSet = (uint)cps;
            contextPatternClear = 0;
            defTextID = 0;
            ruleID = 0;
        }
    }

    /// <summary>
    ///     Structure for Token instructions.
    /// </summary>
    public struct TokenInstruction {
        /// <summary>
        ///     Non-Terminal Token Rule ID that generated Token.
        /// </summary>
        public Symbol NTTRuleID;
        /// <summary>
        ///     Token ID.
        /// </summary>
        public Symbol ID;
        /// <summary>
        ///     Line number in source code where Token was found
        /// </summary>
        public int line;
        /// <summary>
        ///     Character position in source where Token was found
        /// </summary>
        public int pos;

        public TokenInstruction(Symbol symbol, Symbol ID) {
            this.NTTRuleID = symbol;
            this.ID = ID;
            line = 0;
            pos = 0;
        }
    }

    public struct TokenInstType{
        public string Name;
        public int ID;

    }

    public struct RegisterUsage {
        public bool Phase1Write;
        public bool Phase2Write;
    }

    /// <summary>
    ///     Structure used to keep track of arguments and instruction parameters.
    /// </summary>
    struct OpParam {
        public int Arg;		// type of argument
        public bool Filled;		// has it been filled yet
        public uint MaskRep;	// Mask/Replicator flags
        public int Mod;		// argument modifier
    }

    struct RegModOffset {
        public int MacroOffset;
        public int RegisterBase;
        public int OpParamsIndex;

        public RegModOffset(int offset, Symbol regBase, int index) {
            this.MacroOffset = offset;
            this.RegisterBase = (int)regBase;
            this.OpParamsIndex = index;
        }
    }

    struct MacroRegModify {
        public TokenInstruction[] Macro;
        public int MacroSize;
        public RegModOffset[] RegMods;
        public int RegModSize;

        public MacroRegModify(TokenInstruction[] tokens, RegModOffset[] offsets) {
            this.Macro = tokens;
            this.MacroSize = tokens.Length;
            this.RegMods = offsets;
            this.RegModSize = offsets.Length;
        }
    }
}
