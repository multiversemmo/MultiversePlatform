using System;
using System.Diagnostics;
using Axiom.Core;
using Axiom.Scripting;

/**
 * DX8.1 Pixel Shader to ATI Fragment Shader compiler
 * Original Author: NFZ (nfuzz@hotmail.com)
 */

namespace Axiom.RenderSystems.OpenGL.ATI {
	/// <summary>
	///     Compiler2Pass is a generic compiler/assembler.
	/// </summary>
	/// <remarks>
	///     Provides a tokenizer in pass 1 and relies on the subclass to provide the virtual method for pass 2 
	///     
	///     PASS 1 - tokenize source: this is a simple brute force lexical scanner/analyzer that also parses
	///     the formed token for proper semantics and context in one pass
	///     it uses Look Ahead Left-Right (LALR) ruling based on Backus - Naur From notation for semantic
	///     checking and also performs context checking allowing for language dialects.
	///     
	///     PASS 2 - generate application specific instructions ie native instructions
	///     <p/>
	///     This class must be subclassed with the subclass providing implementation for Pass 2.  The subclass
	///     is responsible for setting up the token libraries along with defining the language syntax.
	/// </remarks>
	public abstract class Compiler2Pass {
        #region Fields

        /// <summary>
        ///     Container for tokens extracted from source.
        /// </summary>
        protected TokenInstructionList tokenInstructions = new TokenInstructionList();
        /// <summary>
        ///     Source to be compiled.
        /// </summary>
        protected string source;
        /// <summary>
        ///     Reference to the Text and Token type libraries set up by subclass.
        /// </summary>
        protected SymbolDef[] symbolTypeLib;
        /// <summary>
        ///     Reference to the root rule path - has to be set by subclass constructor.
        /// </summary>
        protected TokenRule[] rootRulePath;
        /// <summary>
        ///     Needs to be initialized by the subclass before compiling occurs
        ///     it defines the token ID used in the symbol type library.
        /// </summary>
        protected Symbol valueID;
        /// <summary>
        ///     Storage container for constants defined in source.
        /// </summary>
        protected FloatList constants = new FloatList();
        /// <summary>
        ///     Active Contexts pattern used in pass 1 to determine which tokens are valid for a certain context.
        /// </summary>
        protected uint activeContexts;
        /// <summary>
        ///     Current line in the source string.
        /// </summary>
        protected int currentLine;
        /// <summary>
        ///     Current position in the source string.
        /// </summary>
        protected int charPos;
        protected int rulePathLibCount;
        protected int symbolTypeLibCount;
        protected int endOfSource;

        #endregion Fields

        #region Constructor

        /// <summary>
        ///     Default constructor.
        /// </summary>
		public Compiler2Pass() {
            //tokenInstructions.Capacity = 100;
            //constants.Capacity = 80;

            activeContexts = 0xffffffff;
		}

        #endregion Constructor

        #region Methods

        /// <summary>
        ///     perform pass 1 of compile process
        /// </summary>
        /// <remarks>
        ///     Scans source for symbols that can be tokenized and then
		///     performs general semantic and context verification on each symbol before it is tokenized.
		///     A tokenized instruction list is built to be used by Pass 2.
        /// </remarks>
        /// <returns></returns>
        protected bool DoPass1() {
            // scan through Source string and build a token list using TokenInstructions
            // this is a simple brute force lexical scanner/analyzer that also parses the formed
            // token for proper semantics and context in one pass
            currentLine = 1;
            charPos = 0;

            // reset position in Constants container
            constants.Clear();
            endOfSource = source.Length;

            // start with a clean slate
            tokenInstructions.Clear();

            // tokenize and check semantics untill an error occurs or end of source is reached
            // assume RootRulePath has pointer to rules so start at index + 1 for first rule path
            // first rule token would be a rule definition so skip over it
            bool passed = ProcessRulePath(0);

            // if a symbol in source still exists then the end of source was not reached and there was a problem some where
            if(PositionToNextSymbol()) {
                passed = false;
            }

            return passed;
        }

        /// <summary>
        ///     Abstract method that must be set up by subclass to perform Pass 2 of compile process
        /// </summary>
        /// <returns></returns>
        protected abstract bool DoPass2();

        /// <summary>
        ///     Get the text symbol for this token.
        /// </summary>
        /// <remarks>
        ///     Mainly used for debugging and in test routines.
        /// </remarks>
        /// <param name="symbol">Token ID.</param>
        /// <returns>String text.</returns>
        protected string GetTypeDefText(Symbol symbol) {
            return rootRulePath[symbolTypeLib[(int)symbol].defTextID].symbol;
        }

        /// <summary>
        ///     Check to see if the text at the present position in the source is a numerical constant.
        /// </summary>
        /// <param name="val">Receives the float value that is in the source.</param>
        /// <param name="length">Receives number of characters that make of the value in the source.</param>
        /// <returns>True if the characters form a valid float, false otherwise.</returns>
        protected bool IsFloatValue(out float val, out int length) {
            bool valueFound = false;

            int currPos = charPos;
            string floatString = "";
            bool firstNonSpace = false;

            char c = source[currPos];

            // have the out param at least set to 0
            val = 0.0f;
            length = 0;

            while(Char.IsNumber(c) || c == '.' || c == '-' || c == ' ') {
                if(c != ' ' && !firstNonSpace) {
                    firstNonSpace = true;
                }

                if(c == ' ' && firstNonSpace) {
                    break;
                }
                else {
                    length++;
                }

                floatString += c;
                c = source[++currPos];
            }

            if(charPos != currPos) {
                val = StringConverter.ParseFloat(floatString);
                valueFound = true;
            }

            return valueFound;
        }

        /// <summary>
        ///     Check to see if the text is in the symbol text library.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="symbolSize"></param>
        /// <returns></returns>
        protected bool IsSymbol(string symbol, out int symbolSize) {
            // compare text at source+charpos with the symbol : limit testing to symbolsize

            // assume failure
            bool symbolFound = false;

            symbolSize = symbol.Length;

            if(charPos + symbolSize <= endOfSource) {
                if(string.Compare(source.Substring(charPos, symbolSize), symbol) == 0) {
                    symbolFound = true;
                }   
            }

            return symbolFound;
        }

        /// <summary>
        ///     Position to the next possible valid symbol.
        /// </summary>
        /// <returns></returns>
        protected bool PositionToNextSymbol() {
            bool validSymbolFound = false;
            bool eos = false;

            while(!validSymbolFound && !eos) {
                SkipWhitespace();
                SkipEndOfLine();
                SkipComments();

                // have we reached the end of the string?
                if (charPos == endOfSource) {
                    eos = true;
                }
                else {
                    // if ASCII > space then assume valid character is found
                    if (source[charPos] > ' ') {
                        validSymbolFound = true;
                    }
                }
            }// end of while

            return validSymbolFound;
        }

        /// <summary>
        ///     Process input source text using rulepath to determine allowed tokens.
        /// </summary>
        /// <remarks>
        ///     The method is reentrant and recursive.
        ///     if a non-terminal token is encountered in the current rule path then the method is
        ///     called using the new rule path referenced by the non-terminal token
        ///     Tokens can have the following operation states which effects the flow path of the rule
        ///     <list type="">
        ///     <item>Rule: defines a rule path for the non-terminal token.</item>
        ///     <item>And: the token is required for the rule to pass.</item>
        ///     <item>Or: if the previous tokens failed then try these ones.</item>
        ///     <item>Optional: the token is optional and does not cause the rule to fail if the token is not found.</item>
        ///     <item>Repeat: the token is required but there can be more than one in a sequence.</item>
        ///     <item>End: end of the rule path - the method returns the success of the rule.</item>
        ///     </list>
        /// </remarks>
        /// <param name="rulePathIdx">Index into to array of Token Rules that define a rule path to be processed.</param>
        /// <returns>True if rule passed - all required tokens found.  False otherwise.</returns>
        protected bool ProcessRulePath(int rulePathIdx) {
            // rule path determines what tokens and therefore what symbols are acceptable from the source
            // it is assumed that the tokens with the longest similar symbols are arranged first so
            // if a match is found it is accepted and no further searching is done

            // record position of last token in container
            // to be used as the rollback position if a valid token is not found
            int tokenContainerOldSize = tokenInstructions.Count;
            int oldCharPos = charPos;
            int oldLinePos = currentLine;
            int oldConstantsSize = constants.Count;

            // keep track of what non-terminal token activated the rule
            Symbol activeNTTRule = rootRulePath[rulePathIdx].tokenID;

            // start rule path at next position for definition
            rulePathIdx++;

            // assume the rule will pass
            bool passed = true;
            bool endFound = false;

            // keep following rulepath until the end is reached
            while (!endFound) {
                switch (rootRulePath[rulePathIdx].operation) {

                    case OperationType.And:
                        // only validate if the previous rule passed
                        if(passed) {
                            passed = ValidateToken(rulePathIdx, activeNTTRule); 
                        }
                        break;

                    case OperationType.Or:
                        // only validate if the previous rule failed
                        if (!passed) {
                            // clear previous tokens from entry and try again
                            tokenInstructions.Resize(tokenContainerOldSize);
                            passed = ValidateToken(rulePathIdx, activeNTTRule);
                        }
                        else { 
                            // path passed up to this point therefore finished so pretend end marker found
                            endFound = true;
                        }
                        break;

                    case OperationType.Optional:
                        // if previous passed then try this rule but it does not effect succes of rule since its optional
                        if(passed) { 
                            ValidateToken(rulePathIdx, activeNTTRule);
                        }
                        break;

                    case OperationType.Repeat:
                        // repeat until no tokens of this type found 
                        // at least one must be found
                        if(passed) {
                            int tokensPassed = 0;
                            // keep calling until failure
                            while (passed = ValidateToken(rulePathIdx, activeNTTRule)) {
                                // increment count for previous passed token
                                tokensPassed++;
                            }
                            // defaults to Passed = fail
                            // if at least one token found then return passed = true
                            if (tokensPassed > 0) {
                                passed = true;
                            }
                        }
                        break;

                    case OperationType.End:
                        // end of rule found so time to return
                        endFound = true;

                        if(!passed) {
                            // the rule did not validate so get rid of tokens decoded
                            // roll back the token container end position to what it was when rule started
                            // this will get rid of all tokens that had been pushed on the container while
                            // trying to validating this rule
                            tokenInstructions.Resize(tokenContainerOldSize);
                            constants.Resize(oldConstantsSize);
                            charPos = oldCharPos;
                            currentLine = oldLinePos;
                        }
                        break;

                    default:
                        // an exception should be raised since the code should never get here
                        passed = false;
                        endFound = true;
                        break;
                }

                // move on to the next rule in the path
                rulePathIdx++;
            }

            return passed;
        }

        /// <summary>
        ///     Setup ActiveContexts - should be called by subclass to setup initial language contexts.
        /// </summary>
        /// <param name="contexts"></param>
        protected void SetActiveContexts(uint contexts) {
            activeContexts = contexts;
        }

        /// <summary>
        ///     Skips all comment specifiers.
        /// </summary>
        protected void SkipComments() {
            // if current char and next are // then search for EOL
            if(charPos < endOfSource) {
                if( ((source[charPos] == '/') && 
                    (source[charPos + 1] == '/')) ||
                    (source[charPos] == ';') ||
                    (source[charPos] == '#') ) {
                    
                    FindEndOfLine();
                }
            }
        }

        protected void FindEndOfLine() {
            int newPos = source.IndexOf('\n', charPos);

            if(newPos != -1) {
                charPos += newPos - charPos;
            }
            else {
                charPos = endOfSource - 1;
            }
        }

        /// <summary>
        ///     Find the end of line marker and move past it.
        /// </summary>
        protected void SkipEndOfLine() {
            if(charPos == endOfSource) {
                return;
            }

            if ((source[charPos] == '\n') || (source[charPos] == '\r')) {
                currentLine++;
                charPos++;

                if ((charPos != endOfSource) && ((source[charPos] == '\n') || (source[charPos] == '\r'))) {
                    charPos++;
                }
            }
        }

        /// <summary>
        ///     Skip all the whitespace which includes spaces and tabs.
        /// </summary>
        protected void SkipWhitespace() {
            if(charPos == endOfSource) {
                return;
            }

            // FIX - this method kinda slow
            while(charPos != endOfSource && ((source[charPos] == ' ') || (source[charPos] == '\t'))) {
                charPos++; // find first non white space character
            }
        }

        /// <summary>
        ///     Check if current position in source has the symbol text equivalent to the TokenID.
        /// </summary>
        /// <param name="rulePathIdx">Index into rule path database of token to validate.</param>
        /// <param name="activeRuleID">Index of non-terminal rule that generated the token.</param>
        /// <returns>
        ///     True if token was found.
        ///     False if token symbol text does not match the source text.
        ///     If token is non-terminal, then ProcessRulePath is called.
        /// </returns>
        protected bool ValidateToken(int rulePathIdx, Symbol activeRuleID) {
            int tokenlength = 0;
            // assume the test is going to fail
            bool passed = false;
            Symbol tokenID = rootRulePath[rulePathIdx].tokenID;

            // only validate token if context is correct
            if((symbolTypeLib[(int)tokenID].contextKey & activeContexts) > 0) {
	            int ruleID = symbolTypeLib[(int)tokenID].ruleID;
                // if terminal token then compare text of symbol with what is in source
                if (ruleID == 0) {
                    if (PositionToNextSymbol()) {
                        // if Token is supposed to be a number then check if its a numerical constant
                        if (tokenID == valueID) {
                            float constantvalue;
                            if(passed = IsFloatValue(out constantvalue, out tokenlength)) {
                                constants.Add(constantvalue);
                            }
                        }
                            // compare token symbol text with source text
                        else {
                            passed = IsSymbol(rootRulePath[rulePathIdx].symbol, out tokenlength);
                        }
					
                        if(passed) {
                            TokenInstruction newtoken;

                            // push token onto end of container
                            newtoken.ID = tokenID;
                            newtoken.NTTRuleID = activeRuleID;
                            newtoken.line = currentLine;
                            newtoken.pos = charPos;

                            tokenInstructions.Add(newtoken);
                            // update source position
                            charPos += tokenlength;

                            // allow token instruction to change the ActiveContexts
                            // use token contexts pattern to clear ActiveContexts pattern bits
                            activeContexts &= ~symbolTypeLib[(int)tokenID].contextPatternClear;
                            // use token contexts pattern to set ActiveContexts pattern bits
                            activeContexts |= symbolTypeLib[(int)tokenID].contextPatternSet;
                        }
                    }

                }
                    // else a non terminal token was found
                else {

                    // execute rule for non-terminal
                    // get rule_ID for index into  rulepath to be called
                    passed = ProcessRulePath(symbolTypeLib[(int)tokenID].ruleID);
                }
            }

            return passed;
        }

        /// <summary>
        ///     Initialize the type library with matching symbol text found in symbol text library.
        ///     Find a default text for all Symbol Types in library.
        ///     Scan through all the rules and initialize TypeLib with index to text and index to rules for non-terminal tokens.
        ///     Must be called by subclass after libraries and rule database setup.
        /// </summary>
        protected void InitSymbolTypeLib() {
            Symbol tokenID;
            // find a default text for all Symbol Types in library

            // scan through all the rules and initialize TypeLib with index to text and index to rules for non-terminal tokens
            for(int i = 0; i < rulePathLibCount; i++) {
                tokenID = rootRulePath[i].tokenID;

                Debug.Assert(symbolTypeLib[(int)tokenID].ID == tokenID);

                switch(rootRulePath[i].operation) {
                    case OperationType.Rule:
                        // if operation is a rule then update typelib
                        symbolTypeLib[(int)tokenID].ruleID = i;
                        break;

                    case OperationType.And:
                    case OperationType.Or:
                    case OperationType.Optional:
                        if(rootRulePath[i].symbol != null) {
                            symbolTypeLib[(int)tokenID].defTextID = i;
                        }
                        break;
                } // switch
            } // for
        }

        /// <summary>
        ///     Compile the source - performs 2 passes:
        ///     First pass is to tokinize, check semantics and context.
        ///     Second pass is performed by subclass and converts tokens to application specific instructions.
        /// </summary>
        /// <param name="source">Source to be compiled.</param>
        /// <returns>
        ///     True if Pass 1 and Pass 2 are successful.
        ///     False if any errors occur in Pass 1 or Pass 2
        /// </returns>
        public bool Compile(string source) {
            bool passed = false;

            this.source = source;

            // start compiling if there is a rule base to work with
            if(rootRulePath != null) {
                passed = DoPass1();

                if(passed) {
                    passed = DoPass2();
                }
            }

            return passed;
        }

        #endregion Methods

        #region Properties

        #endregion Properties
	}
}
