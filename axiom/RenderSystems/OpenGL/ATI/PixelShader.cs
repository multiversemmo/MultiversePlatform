/**
 * DX8.1 Pixel Shader to ATI Fragment Shader compiler
 * Original Author: NFZ
 * 
	A number of invaluable references were used to put together this ps.1.x compiler for ATI_fragment_shader execution

	References:
		1. MSDN: DirectX 8.1 Reference
		2. Wolfgang F. Engel "Fundamentals of Pixel Shaders - Introduction to Shader Programming Part III" on gamedev.net
		3. Martin Ecker - XEngine
		4. Shawn Kirst - ps14toATIfs
		5. Jason L. Mitchell "Real-Time 3D Graphics With Pixel Shaders" 
		6. Jason L. Mitchell "1.4 Pixel Shaders"
		7. Jason L. Mitchell and Evan Hart "Hardware Shading with EXT_vertex_shader and ATI_fragment_shader"
		6. ATI 8500 SDK
		7. GL_ATI_fragment_shader extension reference
*/

using System;
using Axiom.Core;
using Tao.OpenGl;

namespace Axiom.RenderSystems.OpenGL.ATI {
	/// <summary>
    ///     Subclasses Compiler2Pass to provide a ps_1_x compiler that takes DirectX pixel shader assembly
    ///     and converts it to a form that can be used by ATI_fragment_shader OpenGL API.
	/// </summary>
	/// <remarks>
	///     All ps_1_1, ps_1_2, ps_1_3, ps_1_4 assembly instructions are recognized but not all are passed
	///     on to ATI_fragment_shader.	ATI_fragment_shader does not have an equivelant directive for
	///     texkill or texdepth instructions.
	///     <p/>
	///     The user must provide the GL binding interfaces.
	///     <p/>
	///     A Test method is provided to verify the basic operation of the compiler which outputs the test
	///     results to a file.
	/// </remarks>
	public class PixelShader : Compiler2Pass {
        #region Static Fields

        static bool libInitialized = false;

        const int RGB_BITS = 0x07;
        const int ALPHA_BIT = 0x08;

        static SymbolDef[] PS_1_4_SymbolTypeLib = {
            new SymbolDef( Symbol.PS_1_4 , Gl.GL_NONE , ContextKeyPattern.PS_BASE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.PS_1_1 , Gl.GL_NONE , ContextKeyPattern.PS_BASE , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.PS_1_2 , Gl.GL_NONE , ContextKeyPattern.PS_BASE , (uint)ContextKeyPattern.PS_1_2 + (uint)ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.PS_1_3 , Gl.GL_NONE , ContextKeyPattern.PS_BASE , (uint)ContextKeyPattern.PS_1_3 + (uint)ContextKeyPattern.PS_1_2 + (uint)ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.C0 , Gl.GL_CON_0_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C1 , Gl.GL_CON_1_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C2 , Gl.GL_CON_2_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C3 , Gl.GL_CON_3_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C4 , Gl.GL_CON_4_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C5 , Gl.GL_CON_5_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C6 , Gl.GL_CON_6_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.C7 , Gl.GL_CON_7_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.V0 , Gl.GL_PRIMARY_COLOR_ARB  , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.V1 , Gl.GL_SECONDARY_INTERPOLATOR_ATI  , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.ADD , Gl.GL_ADD_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.SUB , Gl.GL_SUB_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.MUL , Gl.GL_MUL_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.MAD , Gl.GL_MAD_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.LRP , Gl.GL_LERP_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.MOV , Gl.GL_MOV_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.CMP , Gl.GL_CND0_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.CND , Gl.GL_CND_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DP3 , Gl.GL_DOT3_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DP4 , Gl.GL_DOT4_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DEF , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.R , Gl.GL_RED_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.RA , Gl.GL_RED_BIT_ATI | ALPHA_BIT , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.G , Gl.GL_GREEN_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.GA , Gl.GL_GREEN_BIT_ATI | ALPHA_BIT , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.B , Gl.GL_BLUE_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.BA , Gl.GL_BLUE_BIT_ATI | ALPHA_BIT , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.A, ALPHA_BIT , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.RGBA, RGB_BITS | ALPHA_BIT , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.RGB, RGB_BITS  , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.RG , Gl.GL_RED_BIT_ATI | Gl.GL_GREEN_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.RGA , Gl.GL_RED_BIT_ATI | Gl.GL_GREEN_BIT_ATI | ALPHA_BIT , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.RB , Gl.GL_RED_BIT_ATI | Gl.GL_BLUE_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.RBA , Gl.GL_RED_BIT_ATI | Gl.GL_BLUE_BIT_ATI | ALPHA_BIT , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.GB , Gl.GL_GREEN_BIT_ATI | Gl.GL_BLUE_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.GBA , Gl.GL_GREEN_BIT_ATI | Gl.GL_BLUE_BIT_ATI | ALPHA_BIT , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.RRRR , Gl.GL_RED , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.GGGG , Gl.GL_GREEN , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.BBBB , Gl.GL_BLUE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.AAAA , Gl.GL_ALPHA , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.X2 , Gl.GL_2X_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef(Symbol.X4 , Gl.GL_4X_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.D2 , Gl.GL_HALF_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef(Symbol.SAT , Gl.GL_SATURATE_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.BIAS , Gl.GL_BIAS_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef(Symbol.INVERT , Gl.GL_COMP_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.NEGATE , Gl.GL_NEGATE_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef(Symbol.BX2 , Gl.GL_2X_BIT_ATI | Gl.GL_BIAS_BIT_ATI , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.COMMA , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef(Symbol.VALUE , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.R0 , Gl.GL_REG_0_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.R1 , Gl.GL_REG_1_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.R2 , Gl.GL_REG_2_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.R3 , Gl.GL_REG_3_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.R4 , Gl.GL_REG_4_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.R5 , Gl.GL_REG_5_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.T0 , Gl.GL_TEXTURE0_ARB , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.T1 , Gl.GL_TEXTURE1_ARB , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.T2 , Gl.GL_TEXTURE2_ARB , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.T3 , Gl.GL_TEXTURE3_ARB , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.T4 , Gl.GL_TEXTURE4_ARB , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.T5 , Gl.GL_TEXTURE5_ARB , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.DP2ADD , Gl.GL_DOT2_ADD_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.X8 , Gl.GL_8X_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.D8 , Gl.GL_EIGHTH_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.D4 , Gl.GL_QUARTER_BIT_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.TEXCRD , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef(Symbol.TEXLD , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.STR , Gl.GL_SWIZZLE_STR_ATI - Gl.GL_SWIZZLE_STR_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.STQ , Gl.GL_SWIZZLE_STQ_ATI - Gl.GL_SWIZZLE_STR_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.STRDR , Gl.GL_SWIZZLE_STR_DR_ATI - Gl.GL_SWIZZLE_STR_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.STQDQ , Gl.GL_SWIZZLE_STQ_DQ_ATI - Gl.GL_SWIZZLE_STR_ATI , ContextKeyPattern.PS_1_4),
            new SymbolDef(  Symbol.BEM , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.PHASE , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.R0_1 , Gl.GL_REG_4_ATI , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.R1_1 , Gl.GL_REG_5_ATI , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.T0_1 , Gl.GL_REG_0_ATI , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.T1_1 , Gl.GL_REG_1_ATI , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.T2_1, Gl.GL_REG_2_ATI , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.T3_1 , Gl.GL_REG_3_ATI , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.TEX , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXCOORD , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXM3X2PAD , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.TEXM3X2TEX , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXM3X3PAD , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXM3X3TEX , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXM3X3SPEC , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXM3X3VSPEC , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(Symbol.TEXREG2AR , Gl.GL_NONE , ContextKeyPattern.PS_1_2),
            new SymbolDef( Symbol.TEXREG2GB , Gl.GL_NONE , ContextKeyPattern.PS_1_2),
            new SymbolDef( Symbol.TEXREG2RGB , Gl.GL_NONE , ContextKeyPattern.PS_1_2),
            new SymbolDef(Symbol.TEXDP3 , Gl.GL_NONE , ContextKeyPattern.PS_1_2),
            new SymbolDef(Symbol.TEXDP3TEX , Gl.GL_NONE , ContextKeyPattern.PS_1_2),
            new SymbolDef( Symbol.SKIP , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef(Symbol.PLUS , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.PROGRAM , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.PROGRAMTYPE , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DECLCONSTS , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DEFCONST , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.CONSTANT , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.COLOR , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef(  Symbol.TEXSWIZZLE , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.UNARYOP , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.NUMVAL , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.SEPERATOR , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.ALUOPS , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.TEXMASK , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.TEXOP_PS1_1_3 , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef(  Symbol.TEXOP_PS1_4 , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.ALU_STATEMENT , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DSTMODSAT , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.UNARYOP_ARGS , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.REG_PS1_4 , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.TEX_PS1_4 , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.REG_PS1_1_3 , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.TEX_PS1_1_3 , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.DSTINFO , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.SRCINFO , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.BINARYOP_ARGS , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.TERNARYOP_ARGS , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.TEMPREG , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DSTMASK , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.PRESRCMOD , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.SRCNAME , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.SRCREP , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.POSTSRCMOD , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DSTMOD , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.DSTSAT , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.BINARYOP , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.TERNARYOP , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.TEXOPS_PHASE1 , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.COISSUE , Gl.GL_NONE , ContextKeyPattern.PS_BASE),
            new SymbolDef( Symbol.PHASEMARKER , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.TEXOPS_PHASE2 , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.TEXREG_PS1_4 , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.TEXOPS_PS1_4 , Gl.GL_NONE , ContextKeyPattern.PS_1_4),
            new SymbolDef( Symbol.TEXOPS_PS1_1_3 , Gl.GL_NONE , ContextKeyPattern.PS_1_1),
            new SymbolDef( Symbol.TEXCISCOP_PS1_1_3 , Gl.GL_NONE , ContextKeyPattern.PS_1_1)                                     
        };

        static TokenRule[] PS_1_x_RulePath = {
            new TokenRule(OperationType.Rule,  Symbol.PROGRAM, "Program"),
            new TokenRule(OperationType.And,  Symbol.PROGRAMTYPE),
            new TokenRule(OperationType.Optional,  Symbol.DECLCONSTS),
            new TokenRule(OperationType.Optional,  Symbol.TEXOPS_PHASE1),
            new TokenRule(OperationType.Optional,  Symbol.ALUOPS ),
            new TokenRule(OperationType.Optional,  Symbol.PHASEMARKER),
            new TokenRule(OperationType.Optional,  Symbol.TEXOPS_PHASE2),
            new TokenRule(OperationType.Optional,  Symbol.ALUOPS),
            new TokenRule(OperationType.End ),
            new TokenRule(OperationType.Rule,  Symbol.PROGRAMTYPE, "<ProgramType>"),
            new TokenRule(OperationType.And,  Symbol.PS_1_4, "ps.1.4"),
            new TokenRule(OperationType.Or,  Symbol.PS_1_1, "ps.1.1"),
            new TokenRule(OperationType.Or,  Symbol.PS_1_2, "ps.1.2"),
            new TokenRule(OperationType.Or,  Symbol.PS_1_3, "ps.1.3"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.PHASEMARKER, "<PhaseMarker>"),
            new TokenRule(OperationType.And,  Symbol.PHASE, "phase"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DECLCONSTS, "<DeclareConstants>"),
            new TokenRule(OperationType.Repeat,  Symbol.DEFCONST),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXOPS_PHASE1, "<TexOps_Phase1>"),
            new TokenRule(OperationType.And,  Symbol.TEXOPS_PS1_1_3),
            new TokenRule(OperationType.Or,  Symbol.TEXOPS_PS1_4),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXOPS_PHASE2, "<TexOps_Phase2>"),
            new TokenRule(OperationType.And,  Symbol.TEXOPS_PS1_4),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.NUMVAL, "<NumVal>"),
            new TokenRule(OperationType.And,  Symbol.VALUE, "Float Value"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXOPS_PS1_1_3, "<TexOps_PS1_1_3>"),
            new TokenRule(OperationType.Repeat,  Symbol.TEXOP_PS1_1_3),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXOPS_PS1_4, "<TexOps_PS1_4>"),
            new TokenRule(OperationType.Repeat,  Symbol.TEXOP_PS1_4),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXOP_PS1_1_3, "<TexOp_PS1_1_3>"),
            new TokenRule(OperationType.And,   Symbol.TEXCISCOP_PS1_1_3),
            new TokenRule(OperationType.And,  Symbol.TEX_PS1_1_3),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.TEX_PS1_1_3),
            new TokenRule(OperationType.Or,   Symbol.TEXCOORD, "texcoord"),
            new TokenRule(OperationType.And,  Symbol.TEX_PS1_1_3),
            new TokenRule(OperationType.Or,   Symbol.TEX, "tex"),
            new TokenRule(OperationType.And,  Symbol.TEX_PS1_1_3),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXOP_PS1_4, "<TexOp_PS1_4>"),
            new TokenRule(OperationType.And,   Symbol.TEXCRD, "texcrd"),
            new TokenRule(OperationType.And,  Symbol.REG_PS1_4),
            new TokenRule(OperationType.Optional,  Symbol.TEXMASK),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.TEXREG_PS1_4),
            new TokenRule(OperationType.Or,   Symbol.TEXLD, "texld"),
            new TokenRule(OperationType.And,  Symbol.REG_PS1_4),
            new TokenRule(OperationType.Optional,  Symbol.TEXMASK),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.TEXREG_PS1_4 ),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.ALUOPS, "<ALUOps>"),
            new TokenRule(OperationType.Repeat,  Symbol.ALU_STATEMENT),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.ALU_STATEMENT, "<ALUStatement>"),
            new TokenRule(OperationType.And,  Symbol.COISSUE),
            new TokenRule(OperationType.And,  Symbol.UNARYOP),
            new TokenRule(OperationType.Optional,  Symbol.DSTMODSAT),
            new TokenRule(OperationType.And,  Symbol.UNARYOP_ARGS ),
            new TokenRule(OperationType.Or,  Symbol.COISSUE),
            new TokenRule(OperationType.And,  Symbol.BINARYOP),
            new TokenRule(OperationType.Optional,  Symbol.DSTMODSAT),
            new TokenRule(OperationType.And,  Symbol.BINARYOP_ARGS),
            new TokenRule(OperationType.Or,  Symbol.COISSUE),
            new TokenRule(OperationType.And,  Symbol.TERNARYOP),
            new TokenRule(OperationType.Optional,  Symbol.DSTMODSAT),
            new TokenRule(OperationType.And,  Symbol.TERNARYOP_ARGS ),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXREG_PS1_4, "<TexReg_PS1_4>"),
            new TokenRule(OperationType.And,  Symbol.TEX_PS1_4  ),
            new TokenRule(OperationType.Optional,  Symbol.TEXSWIZZLE),
            new TokenRule(OperationType.Or,  Symbol.REG_PS1_4  ),
            new TokenRule(OperationType.Optional,  Symbol.TEXSWIZZLE),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.UNARYOP_ARGS, "<UnaryOpArgs>"),
            new TokenRule(OperationType.And,   Symbol.DSTINFO),
            new TokenRule(OperationType.And,  Symbol.SRCINFO),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.BINARYOP_ARGS, "<BinaryOpArgs>"),
            new TokenRule(OperationType.And,   Symbol.DSTINFO),
            new TokenRule(OperationType.And,  Symbol.SRCINFO),
            new TokenRule(OperationType.And,  Symbol.SRCINFO),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TERNARYOP_ARGS, "<TernaryOpArgs>"),
            new TokenRule(OperationType.And,   Symbol.DSTINFO),
            new TokenRule(OperationType.And,  Symbol.SRCINFO),
            new TokenRule(OperationType.And,  Symbol.SRCINFO),
            new TokenRule(OperationType.And,  Symbol.SRCINFO),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DSTINFO, "<DstInfo>"),
            new TokenRule(OperationType.And,  Symbol.TEMPREG),
            new TokenRule(OperationType.Optional,  Symbol.DSTMASK),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.SRCINFO, "<SrcInfo>"),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.Optional,  Symbol.PRESRCMOD),
            new TokenRule(OperationType.And,  Symbol.SRCNAME),
            new TokenRule(OperationType.Optional,  Symbol.POSTSRCMOD),
            new TokenRule(OperationType.Optional,  Symbol.SRCREP),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.SRCNAME, "<SrcName>"),
            new TokenRule(OperationType.And,  Symbol.TEMPREG),
            new TokenRule(OperationType.Or,  Symbol.CONSTANT),
            new TokenRule(OperationType.Or,  Symbol.COLOR),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DEFCONST, "<DefineConstant>"),
            new TokenRule(OperationType.And,  Symbol.DEF, "def"),
            new TokenRule(OperationType.And,  Symbol.CONSTANT),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.NUMVAL),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.NUMVAL),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.NUMVAL),
            new TokenRule(OperationType.And,  Symbol.SEPERATOR),
            new TokenRule(OperationType.And,  Symbol.NUMVAL),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.CONSTANT, "<Constant>"),
            new TokenRule(OperationType.And,  Symbol.C0, "c0"),
            new TokenRule(OperationType.Or,  Symbol.C1, "c1"),
            new TokenRule(OperationType.Or,  Symbol.C2, "c2"),
            new TokenRule(OperationType.Or,  Symbol.C3, "c3"),
            new TokenRule(OperationType.Or,  Symbol.C4, "c4"),
            new TokenRule(OperationType.Or,  Symbol.C5, "c5"),
            new TokenRule(OperationType.Or,  Symbol.C6, "c6"),
            new TokenRule(OperationType.Or,  Symbol.C7, "c7"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXCISCOP_PS1_1_3, "<TexCISCOp_PS1_1_3>"),
            new TokenRule(OperationType.And,  Symbol.TEXDP3TEX,"texdp3tex"),
            new TokenRule(OperationType.Or,  Symbol.TEXDP3,"texdp3"),
            new TokenRule(OperationType.Or,  Symbol.TEXM3X2PAD,"texm3x2pad"),
            new TokenRule(OperationType.Or,  Symbol.TEXM3X2TEX,"texm3x2tex"),
            new TokenRule(OperationType.Or,  Symbol.TEXM3X3PAD,"texm3x3pad"),
            new TokenRule(OperationType.Or,  Symbol.TEXM3X3TEX,"texm3x3tex"),
            new TokenRule(OperationType.Or,  Symbol.TEXM3X3SPEC,"texm3x3spec"),
            new TokenRule(OperationType.Or,  Symbol.TEXM3X3VSPEC,"texm3x3vspec"),
            new TokenRule(OperationType.Or,  Symbol.TEXREG2RGB,"texreg2rgb"),
            new TokenRule(OperationType.Or,  Symbol.TEXREG2AR,"texreg2ar"),
            new TokenRule(OperationType.Or,  Symbol.TEXREG2GB,"texreg2gb"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEXSWIZZLE, "<TexSwizzle>"),
            new TokenRule(OperationType.And,  Symbol.STQDQ,"_dw.xyw"),
            new TokenRule(OperationType.Or,  Symbol.STQDQ,"_dw"),
            new TokenRule(OperationType.Or,  Symbol.STQDQ,"_da.rga"),
            new TokenRule(OperationType.Or,  Symbol.STQDQ,"_da"),
            new TokenRule(OperationType.Or,  Symbol.STRDR,"_dz.xyz"),
            new TokenRule(OperationType.Or,  Symbol.STRDR,"_dz"),
            new TokenRule(OperationType.Or,  Symbol.STRDR,"_db.rgb"),
            new TokenRule(OperationType.Or,  Symbol.STRDR,"_db"),
            new TokenRule(OperationType.Or,  Symbol.STR,".xyz"),
            new TokenRule(OperationType.Or,  Symbol.STR,".rgb"),
            new TokenRule(OperationType.Or,  Symbol.STQ,".xyw"),
            new TokenRule(OperationType.Or,  Symbol.STQ,".rga"),
            new TokenRule(OperationType.End ),
            new TokenRule(OperationType.Rule,  Symbol.TEXMASK, "<TexMask>"),
            new TokenRule(OperationType.And,  Symbol.RGB,".rgb"),
            new TokenRule(OperationType.Or,  Symbol.RGB,".xyz"),
            new TokenRule(OperationType.Or,  Symbol.RG,".rg"),
            new TokenRule(OperationType.Or,  Symbol.RG,".xy"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.SEPERATOR, "<Seperator>"),
            new TokenRule(OperationType.And,  Symbol.COMMA, ","),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.REG_PS1_4, "<Reg_PS1_4>"),
            new TokenRule(OperationType.And,  Symbol.R0, "r0"),
            new TokenRule(OperationType.Or,  Symbol.R1, "r1"),
            new TokenRule(OperationType.Or,  Symbol.R2, "r2"),
            new TokenRule(OperationType.Or,  Symbol.R3, "r3"),
            new TokenRule(OperationType.Or,  Symbol.R4, "r4"),
            new TokenRule(OperationType.Or,  Symbol.R5, "r5"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEX_PS1_4, "<Tex_PS1_4>"),
            new TokenRule(OperationType.And,  Symbol.T0, "t0"),
            new TokenRule(OperationType.Or,  Symbol.T1, "t1"),
            new TokenRule(OperationType.Or,  Symbol.T2, "t2"),
            new TokenRule(OperationType.Or,  Symbol.T3, "t3"),
            new TokenRule(OperationType.Or,  Symbol.T4, "t4"),
            new TokenRule(OperationType.Or,  Symbol.T5, "t5"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.REG_PS1_1_3, "<Reg_PS1_1_3>"),
            new TokenRule(OperationType.And,  Symbol.R0_1, "r0"),
            new TokenRule(OperationType.Or,  Symbol.R1_1, "r1"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEX_PS1_1_3, "<Tex_PS1_1_3>"),
            new TokenRule(OperationType.And,  Symbol.T0_1, "t0"),
            new TokenRule(OperationType.Or,  Symbol.T1_1, "t1"),
            new TokenRule(OperationType.Or,  Symbol.T2_1, "t2"),
            new TokenRule(OperationType.Or,  Symbol.T3_1, "t3"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.COLOR, "<Color>"),
            new TokenRule(OperationType.And,  Symbol.V0, "v0"),
            new TokenRule(OperationType.Or,  Symbol.V1, "v1"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TEMPREG, "<TempReg>"),
            new TokenRule(OperationType.And,  Symbol.REG_PS1_4),
            new TokenRule(OperationType.Or,  Symbol.REG_PS1_1_3),
            new TokenRule(OperationType.Or,  Symbol.TEX_PS1_1_3),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DSTMODSAT, "<DstModSat>"),
            new TokenRule(OperationType.Optional,  Symbol.DSTMOD),
            new TokenRule(OperationType.Optional,  Symbol.DSTSAT),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,   Symbol.UNARYOP, "<UnaryOp>"),
            new TokenRule(OperationType.And,  Symbol.MOV, "mov"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.BINARYOP, "<BinaryOP>"),
            new TokenRule(OperationType.And,  Symbol.ADD, "add"),
            new TokenRule(OperationType.Or,  Symbol.MUL, "mul"),
            new TokenRule(OperationType.Or,  Symbol.SUB, "sub"),
            new TokenRule(OperationType.Or,  Symbol.DP3, "dp3"),
            new TokenRule(OperationType.Or,  Symbol.DP4, "dp4"),
            new TokenRule(OperationType.Or,  Symbol.BEM, "bem"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.TERNARYOP, "<TernaryOp>"),
            new TokenRule(OperationType.And,  Symbol.MAD, "mad"),
            new TokenRule(OperationType.Or,  Symbol.LRP, "lrp"),
            new TokenRule(OperationType.Or,  Symbol.CND, "cnd"),
            new TokenRule(OperationType.Or,  Symbol.CMP, "cmp"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DSTMASK, "<DstMask>"),
            new TokenRule(OperationType.And,  Symbol.RGBA,".rgba"),
            new TokenRule(OperationType.Or,  Symbol.RGBA,".xyzw"),
            new TokenRule(OperationType.Or,  Symbol.RGB,".rgb"),
            new TokenRule(OperationType.Or,  Symbol.RGB,".xyz"),
            new TokenRule(OperationType.Or,  Symbol.RGA,".xyw"),
            new TokenRule(OperationType.Or,  Symbol.RGA,".rga"),
            new TokenRule(OperationType.Or,  Symbol.RBA,".rba"),
            new TokenRule(OperationType.Or,  Symbol.RBA,".xzw"),
            new TokenRule(OperationType.Or,  Symbol.GBA,".gba"),
            new TokenRule(OperationType.Or,  Symbol.GBA,".yzw"),
            new TokenRule(OperationType.Or,  Symbol.RG,".rg"),
            new TokenRule(OperationType.Or,  Symbol.RG,".xy"),
            new TokenRule(OperationType.Or,  Symbol.RB,".xz"),
            new TokenRule(OperationType.Or,  Symbol.RB,".rb"),
            new TokenRule(OperationType.Or,  Symbol.RA,".xw"),
            new TokenRule(OperationType.Or,  Symbol.RA,".ra"),
            new TokenRule(OperationType.Or,  Symbol.GB,".gb"),
            new TokenRule(OperationType.Or,  Symbol.GB,".yz"),
            new TokenRule(OperationType.Or,  Symbol.GA,".yw"),
            new TokenRule(OperationType.Or,  Symbol.GA,".ga"),
            new TokenRule(OperationType.Or,  Symbol.BA,".zw"),
            new TokenRule(OperationType.Or,  Symbol.BA,".ba"),
            new TokenRule(OperationType.Or,  Symbol.R,".r"),
            new TokenRule(OperationType.Or,  Symbol.R,".x"),
            new TokenRule(OperationType.Or,  Symbol.G,".g"),
            new TokenRule(OperationType.Or,  Symbol.G,".y"),
            new TokenRule(OperationType.Or,  Symbol.B,".b"),
            new TokenRule(OperationType.Or,  Symbol.B,".z"),
            new TokenRule(OperationType.Or,  Symbol.A,".a"),
            new TokenRule(OperationType.Or,  Symbol.A,".w"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.SRCREP, "<SrcRep>"),
            new TokenRule(OperationType.And,  Symbol.RRRR, ".r"),
            new TokenRule(OperationType.Or,  Symbol.RRRR, ".x"),
            new TokenRule(OperationType.Or,  Symbol.GGGG, ".g"),
            new TokenRule(OperationType.Or,  Symbol.GGGG, ".y"),
            new TokenRule(OperationType.Or,  Symbol.BBBB, ".b"),
            new TokenRule(OperationType.Or,  Symbol.BBBB, ".z"),
            new TokenRule(OperationType.Or,  Symbol.AAAA, ".a"),
            new TokenRule(OperationType.Or,  Symbol.AAAA, ".w"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.PRESRCMOD, "<PreSrcMod>"),
            new TokenRule(OperationType.And,  Symbol.INVERT, "1-"),
            new TokenRule(OperationType.Or,  Symbol.INVERT, "1 -"),
            new TokenRule(OperationType.Or,  Symbol.NEGATE, "-"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.POSTSRCMOD, "<PostSrcMod>"),
            new TokenRule(OperationType.And,  Symbol.BX2, "_bx2"),
            new TokenRule(OperationType.Or,  Symbol.X2, "_x2"),
            new TokenRule(OperationType.Or,  Symbol.BIAS, "_bias"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DSTMOD, "<DstMod>"),
            new TokenRule(OperationType.And,  Symbol.X2, "_x2"),
            new TokenRule(OperationType.Or,  Symbol.X4, "_x4"),
            new TokenRule(OperationType.Or,  Symbol.D2, "_d2"),
            new TokenRule(OperationType.Or,  Symbol.X8, "_x8"),
            new TokenRule(OperationType.Or,  Symbol.D4, "_d4"),
            new TokenRule(OperationType.Or,  Symbol.D8, "_d8"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.DSTSAT, "<DstSat>"),
            new TokenRule(OperationType.And,  Symbol.SAT, "_sat"),
            new TokenRule(OperationType.End),
            new TokenRule(OperationType.Rule,  Symbol.COISSUE, "<CoIssue>"),
            new TokenRule(OperationType.Optional,  Symbol.PLUS, "+"),
            new TokenRule(OperationType.End)
        };

        //***************************** MACROs for PS1_1 , PS1_2, PS1_3 CISC instructions **************************************

        /// <summary>
        ///     Macro token expansion for ps_1_2 instruction: texreg2ar
        /// </summary>
        static TokenInstruction[] texreg2ar = {
            // mov r(x).r, r(y).a
            new TokenInstruction(Symbol.UNARYOP, Symbol.MOV),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.DSTMASK, Symbol.R),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0),
            new TokenInstruction(Symbol.SRCREP, Symbol.AAAA),
            // mov r(x).g, r(y).r
            new TokenInstruction(Symbol.UNARYOP, Symbol.MOV),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.DSTMASK, Symbol.G),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0),
            new TokenInstruction(Symbol.SRCREP, Symbol.RRRR),
            // texld r(x), r(x)
            new TokenInstruction(Symbol.TEXOP_PS1_4, Symbol.TEXLD),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1)
        };

        static RegModOffset[] texreg2xx_RegMods = {
            new RegModOffset(1, Symbol.R_BASE, 0),
            new RegModOffset(7, Symbol.R_BASE, 0),
            new RegModOffset(13, Symbol.R_BASE, 0),
            new RegModOffset(15, Symbol.R_BASE, 0),
            new RegModOffset(4, Symbol.R_BASE, 1),
            new RegModOffset(10, Symbol.R_BASE, 1),
        };

        static MacroRegModify texreg2ar_MacroMods = 
            new MacroRegModify(texreg2ar, texreg2xx_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_2 instruction: texreg2gb
        /// </summary>
        static TokenInstruction[] texreg2gb = {
            new TokenInstruction(Symbol.UNARYOP,Symbol.MOV),
            new TokenInstruction(Symbol.REG_PS1_4,Symbol.R1),
            new TokenInstruction(Symbol.DSTMASK,Symbol.R),
            new TokenInstruction(Symbol.SEPERATOR,Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,Symbol.R0),
            new TokenInstruction(Symbol.SRCREP,Symbol.GGGG),
            // mov r(x).g, r(y).b
            new TokenInstruction(Symbol.UNARYOP,Symbol.MOV),
            new TokenInstruction(Symbol.REG_PS1_4,Symbol.R1),
            new TokenInstruction(Symbol.DSTMASK,Symbol.G),
            new TokenInstruction(Symbol.SEPERATOR,Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,Symbol.R0),
            new TokenInstruction(Symbol.SRCREP,Symbol.BBBB),
            // texld r(x), r(x)
            new TokenInstruction(Symbol.TEXOP_PS1_4, Symbol.TEXLD),
            new TokenInstruction(Symbol.REG_PS1_4,Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR,Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,Symbol.R1)
        };

        static MacroRegModify texreg2gb_MacroMods = 
            new MacroRegModify(texreg2gb, texreg2xx_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_1 instruction: texdp3
        /// </summary>
        static TokenInstruction[] texdp3 = {
            // texcoord t(x)
            new TokenInstruction(Symbol.TEXOP_PS1_1_3,  Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3,  Symbol.T1_1),
            // dp3 r(x), r(x), r(y)
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0)
        };

        static RegModOffset[] texdp3_RegMods = {
            new RegModOffset(1, Symbol.T_BASE, 0),
            new RegModOffset(3, Symbol.R_BASE, 0),
            new RegModOffset(5, Symbol.R_BASE, 0),
            new RegModOffset(7, Symbol.R_BASE, 1)
        };

        static MacroRegModify texdp3_MacroMods = 
            new MacroRegModify(texdp3, texdp3_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_1 instruction: texdp3
        /// </summary>
        static TokenInstruction[] texdp3tex = {
            // texcoord t(x)
            new TokenInstruction(Symbol.TEXOP_PS1_1_3,  Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3,  Symbol.T1_1),
	        // dp3 r1, r(x), r(y)
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0),
            // texld r(x), r(x)
            new TokenInstruction(Symbol.TEXOP_PS1_4, Symbol.TEXLD),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1)
        };

        static RegModOffset[] texdp3tex_RegMods = {
            new RegModOffset(1, Symbol.T_BASE, 0),
            new RegModOffset(3, Symbol.R_BASE, 0),
            new RegModOffset(5, Symbol.R_BASE, 0),
            new RegModOffset(7, Symbol.R_BASE, 1),
            new RegModOffset(9, Symbol.R_BASE, 1),
            new RegModOffset(11, Symbol.R_BASE, 1)
        };

        static MacroRegModify texdp3tex_MacroMods =
            new MacroRegModify(texdp3tex, texdp3tex_RegMods);

        static TokenInstruction[] texm3x2pad = {
            // texcoord t(x)
            new TokenInstruction(Symbol.TEXOP_PS1_1_3,  Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3,  Symbol.T0_1),
            // dp3 r4.r,  r(x),  r(y)
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK, Symbol.R),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0)
        };

        static RegModOffset[] texm3xxpad_RegMods = {
            new RegModOffset(1, Symbol.T_BASE, 0),
            new RegModOffset(6, Symbol.R_BASE, 0),
            new RegModOffset(8, Symbol.R_BASE, 1)
        };

        static MacroRegModify texm3x2pad_MacroMods =
            new MacroRegModify(texm3x2pad, texm3xxpad_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_1 instruction: texm3x2tex
        /// </summary>
        static TokenInstruction[] texm3x2tex = {
            // texcoord t(x)
            new TokenInstruction(Symbol.TEXOP_PS1_1_3, Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3, Symbol.T1_1),
            // dp3 r4.g, r(x), r(y)
            new TokenInstruction(Symbol.BINARYOP,	Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK,	Symbol.G),
            new TokenInstruction(Symbol.SEPERATOR,	Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR,	Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R0),
            // texld r(x), r4
            new TokenInstruction(Symbol.TEXOP_PS1_4, Symbol.TEXLD),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR,	Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R4)
        };

        static RegModOffset[] texm3xxtex_RegMods = {
            new RegModOffset(1, Symbol.T_BASE, 0),
            new RegModOffset(6, Symbol.R_BASE, 0),
            new RegModOffset(8, Symbol.R_BASE, 1),
            new RegModOffset(10, Symbol.R_BASE, 0)
        };

        static MacroRegModify texm3x2tex_MacroMods = 
            new MacroRegModify(texm3x2tex, texm3xxtex_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_1 instruction: texm3x3tex
        /// </summary>
        static TokenInstruction[] texm3x3pad = {
            // texcoord t(x)
            new TokenInstruction(Symbol.TEXOP_PS1_1_3,  Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3,  Symbol.T0_1),
            // dp3 r4.b, r(x), r(y)
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK, Symbol.B),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0)
        };

        static MacroRegModify texm3x3pad_MacroMods =
            new MacroRegModify(texm3x3pad, texm3xxpad_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_1 instruction: texm3x3pad
        /// </summary>
        static TokenInstruction[] texm3x3tex = {
            // texcoord t(x)
            new TokenInstruction(Symbol.TEXOP_PS1_1_3, Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3, Symbol.T1_1),
            // dp3 r4.b, r(x), r(y)
            new TokenInstruction(Symbol.BINARYOP,	Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK,	Symbol.B),
            new TokenInstruction(Symbol.SEPERATOR,	Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR,	Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R0),
            // texld r1, r4
            new TokenInstruction(Symbol.TEXOP_PS1_4, Symbol.TEXLD),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R1),
            new TokenInstruction(Symbol.SEPERATOR,	Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4,	Symbol.R4)
        };

        static MacroRegModify texm3x3tex_MacroMods =
            new MacroRegModify(texm3x3tex, texm3xxtex_RegMods);

        /// <summary>
        ///     Macro token expansion for ps_1_1 instruction: texm3x3spec
        /// </summary>
        static TokenInstruction[] texm3x3spec = {
            new TokenInstruction(Symbol.TEXOP_PS1_1_3, Symbol.TEXCOORD),
            new TokenInstruction(Symbol.TEX_PS1_1_3, Symbol.T3_1),
            // dp3 r4.b, r3, r(x)
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK, Symbol.B),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R3),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R0),
            // dp3_x2 r3, r4, c(x)
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.DSTMOD, 	Symbol.X2),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R3),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.CONSTANT, Symbol.C0),
            // mul r3, r3, c(x)
            new TokenInstruction(Symbol.UNARYOP, Symbol.MUL),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R3),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R3),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.CONSTANT, Symbol.C0),
            // dp3 r2, r4, r4
            new TokenInstruction(Symbol.BINARYOP, Symbol.DP3),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R2),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            // mad r4.rgb, 1-c(x), r2, r3
            new TokenInstruction(Symbol.TERNARYOP, Symbol.MAD),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK, Symbol.RGB),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.PRESRCMOD, Symbol.INVERT),
            new TokenInstruction(Symbol.CONSTANT, Symbol.C0),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R2),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R3),
            // + mov r4.a, r2.r
            new TokenInstruction(Symbol.UNARYOP, Symbol.MOV),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.DSTMASK, Symbol.A),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R2),
            new TokenInstruction(Symbol.SRCREP, 	Symbol.RRRR),
            // texld r3, r4.xyz_dz
            new TokenInstruction(Symbol.TEXOP_PS1_4, Symbol.TEXLD),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R3),
            new TokenInstruction(Symbol.SEPERATOR, Symbol.COMMA),
            new TokenInstruction(Symbol.REG_PS1_4, Symbol.R4),
            new TokenInstruction(Symbol.TEXSWIZZLE, Symbol.STRDR)
        };

        static RegModOffset[] texm3x3spec_RegMods = {
            new RegModOffset(8, Symbol.R_BASE, 1),
            new RegModOffset(15, Symbol.R_BASE, 2),
            new RegModOffset(21, Symbol.C_BASE, 2),
            new RegModOffset(33, Symbol.C_BASE, 2)
        };

        static MacroRegModify texm3x3spec_MacroMods =
            new MacroRegModify(texm3x3spec, texm3x3spec_RegMods);

        #endregion Static Fields

        #region Fields

        /// <summary>
        ///     Machine instructions for phase one texture section.
        /// </summary>
        IntList phase1TEX_mi = new IntList();
        /// <summary>
        ///     Machine instructions for phase one ALU section.
        /// </summary>
        IntList phase1ALU_mi = new IntList();
        /// <summary>
        ///     Machine instructions for phase two texture section.
        /// </summary>
        IntList phase2TEX_mi = new IntList();
        /// <summary>
        ///     Machine instructions for phase two ALU section.
        /// </summary>
        IntList phase2ALU_mi = new IntList();

        // vars used during pass 2
        MachineInstruction opType;
        Symbol opInst;
        bool do_Alpha;
        PhaseType instructionPhase;
        int argCnt;
        int constantsPos;

        const int MAXOPPARRAMS = 5; // max number of parrams bound to an instruction
	
        OpParam[] opParams = new OpParam[MAXOPPARRAMS];

        /// keeps track of which registers are written to in each phase
        /// if a register is read from but has not been written to in phase 2
        /// then if it was written to in phase 1 perform a register pass function
        /// at the begining of phase2 so that the register has something worthwhile in it
        /// NB: check ALU and TEX section of phase 1 and phase 2
        /// there are 6 temp registers r0 to r5 to keep track off
        /// checks are performed in pass 2 when building machine instructions
        RegisterUsage[] Phase_RegisterUsage = new RegisterUsage[6];

        bool macroOn; // if true then put all ALU instructions in phase 1

        int texm3x3padCount; // keep track of how many texm3x3pad instructions are used so know which mask to use

        int lastInstructionPos; // keep track of last phase 2 ALU instruction to check for R0 setting
        int secondLastInstructionPos;

        // keep track if phase marker found: determines which phase the ALU instructions go into
        bool phaseMarkerFound; 

        #endregion Fields

        #region Constructor

		public PixelShader() {
            symbolTypeLib = PS_1_4_SymbolTypeLib;
            symbolTypeLibCount = PS_1_4_SymbolTypeLib.Length;

            rootRulePath = PS_1_x_RulePath;
            rulePathLibCount = PS_1_x_RulePath.Length;

            // tell compiler what the symbol id is for a numeric value
            valueID = Symbol.VALUE;

            // only need to initialize the rule database once
            if(!libInitialized) {
                InitSymbolTypeLib();
                libInitialized = true;
            }

            // set initial context to recognize PS base instructions
            activeContexts = (uint)ContextKeyPattern.PS_BASE;
		}

        #endregion Constructor

        #region Members

        /** attempt to build a machine instruction using current tokens
            determines what phase machine insturction should be in and if an Alpha Op is required
            calls expandMachineInstruction() to expand the token into machine instructions
        */
        bool BuildMachineInst() {
            // check the states to see if a machine instruction can be assembled

            // // assume everything will go okay untill proven otherwise
            bool passed = true;

            // start with machine NOP instuction
            // this is used after the switch to see if an instruction was set up
            // determine which MachineInstID is required based on the op instruction
            opType = MachineInstruction.Nop;

            switch((Symbol)opInst) {
                    // ALU operations
                case Symbol.ADD:
                case Symbol.SUB:
                case Symbol.MUL:
                case Symbol.MAD:
                case Symbol.LRP:
                case Symbol.MOV:
                case Symbol.CMP:
                case Symbol.CND:
                case Symbol.DP2ADD:
                case Symbol.DP3:
                case Symbol.DP4:
                    opType = (MachineInstruction)((int)MachineInstruction.ColorOp1 + argCnt - 1);

                    // if context is ps.1.x and Macro not on or a phase marker was found then put all ALU ops in phase 2 ALU container
                    if ((((activeContexts & (uint)ContextKeyPattern.PS_1_1) > 0) && !macroOn) || phaseMarkerFound) {
                        instructionPhase = PhaseType.PHASE2ALU;
                    }
                    else {
                        instructionPhase = PhaseType.PHASE1ALU;
                    }

                    // check for alpha op in destination register which is OpParrams[0]
                    // if no Mask for destination then make it .rgba
                    if(opParams[0].MaskRep == 0) {
                        opParams[0].MaskRep = Gl.GL_RED_BIT_ATI | Gl.GL_GREEN_BIT_ATI | Gl.GL_BLUE_BIT_ATI | ALPHA_BIT;
                    }

                    if ((opParams[0].MaskRep & ALPHA_BIT) > 0) {
                        do_Alpha = true;
                        opParams[0].MaskRep -= ALPHA_BIT;
                        if(opParams[0].MaskRep == 0) {
                            opType = MachineInstruction.Nop; // only do alpha op
                        }
                    }
                    break;

                case Symbol.TEXCRD:
                    opType = MachineInstruction.PassTexCoord;
                    if (phaseMarkerFound) {
                        instructionPhase = PhaseType.PHASE2TEX;
                    }
                    else {
                        instructionPhase = PhaseType.PHASE1TEX;
                    }
                    break;

                case Symbol.TEXLD:
                    opType = MachineInstruction.SampleMap;
                    if (phaseMarkerFound) {
                        instructionPhase = PhaseType.PHASE2TEX;
                    }
                    else {
                        instructionPhase = PhaseType.PHASE1TEX;
                    }
                    break;

                case Symbol.TEX: // PS_1_1 emulation
                    opType = MachineInstruction.Tex;
                    instructionPhase = PhaseType.PHASE1TEX;
                    break;

                case Symbol.TEXCOORD: // PS_1_1 emulation
                    opType = MachineInstruction.TexCoord;
                    instructionPhase = PhaseType.PHASE1TEX;
                    break;

                case Symbol.TEXREG2AR:
                    passed = ExpandMacro(texreg2ar_MacroMods);
                    break;

                case Symbol.TEXREG2GB:
                    passed = ExpandMacro(texreg2gb_MacroMods);
                    break;

                case Symbol.TEXDP3:
                    passed = ExpandMacro(texdp3_MacroMods);
                    break;

                case Symbol.TEXDP3TEX:
                    passed = ExpandMacro(texdp3tex_MacroMods);
                    break;

                case Symbol.TEXM3X2PAD:
                    passed = ExpandMacro(texm3x2pad_MacroMods);
                    break;

                case Symbol.TEXM3X2TEX:
                    passed = ExpandMacro(texm3x2tex_MacroMods);
                    break;

                case Symbol.TEXM3X3PAD:
                    // only 2 texm3x3pad instructions allowed
                    // use count to modify macro to select which mask to use
                    if(texm3x3padCount < 2) {
                        texm3x3pad[4].ID = (Symbol)((int)Symbol.R + texm3x3padCount);
                        texm3x3padCount++;
                        passed = ExpandMacro(texm3x3pad_MacroMods);
                    }
                    else {
                        passed = false;
                    }

                    break;

                case Symbol.TEXM3X3TEX:
                    passed = ExpandMacro(texm3x3tex_MacroMods);
                    break;

                case Symbol.DEF:
                    opType = MachineInstruction.SetConstants;
                    instructionPhase = PhaseType.PHASE1TEX;
                    break;

                case Symbol.PHASE: // PS_1_4 only
                    phaseMarkerFound = true;
                    break;

            } // end of switch

            if(passed) {
                passed = ExpandMachineInstruction();
            }

            return passed;
        }
	
        void ClearMachineInstState() {
            // set current Machine Instruction State to baseline
            opType = MachineInstruction.Nop;
            opInst = Symbol.Invalid;
            do_Alpha = false;
            argCnt = 0;

            for(int i = 0; i < MAXOPPARRAMS; i++) {
                opParams[i].Arg = Gl.GL_NONE;
                opParams[i].Filled = false;
                opParams[i].MaskRep = Gl.GL_NONE;
                opParams[i].Mod = Gl.GL_NONE;
            }
        }

        bool SetOpParam(SymbolDef symboldef) {
            bool success = true;

            if(argCnt < MAXOPPARRAMS) {
                if(opParams[argCnt].Filled) {
                    argCnt++;
                }
            }

            if (argCnt < MAXOPPARRAMS) {
                opParams[argCnt].Filled = true;
                opParams[argCnt].Arg = symboldef.pass2Data;
            }
            else {
                success = false;
            }

            return success;
        }

        /** optimizes machine instructions depending on pixel shader context
            only applies to ps.1.1 ps.1.2 and ps.1.3 since they use CISC instructions
            that must be transformed into RISC instructions
        */
        void Optimize() {
            // perform some optimizations on ps.1.1 machine instructions
            if ((activeContexts & (int)ContextKeyPattern.PS_1_1) > 0) {
                // need to check last few instructions to make sure r0 is set
                // ps.1.1 emulation uses r4 for r0 so last couple of instructions will probably require
                // changine destination register back to r0
                if (lastInstructionPos < phase2ALU_mi.Count) {
                    // first argument at mLastInstructionPos + 2 is destination register for all ps.1.1 ALU instructions
                    phase2ALU_mi[lastInstructionPos + 2] = Gl.GL_REG_0_ATI; 
                    // if was an alpha op only then modify second last instruction destination register
                    if (((MachineInstruction)phase2ALU_mi[lastInstructionPos] == MachineInstruction.AlphaOp1) ||
                        ((MachineInstruction)phase2ALU_mi[lastInstructionPos] == MachineInstruction.AlphaOp2) ||
                        ((MachineInstruction)phase2ALU_mi[lastInstructionPos] == MachineInstruction.AlphaOp3)) {

                        phase2ALU_mi[secondLastInstructionPos + 2] = Gl.GL_REG_0_ATI; 
                    }
                }// end if (mLastInstructionPos < mMachineInstructions.size())
            }// end if (mActiveContexts & ckp_PS_1_1)
        }

        // the method is expected to be recursive to allow for inline expansion of instructions if required
        bool Pass2scan(TokenInstruction[] Tokens, int size) {
            // execute TokenInstructions to build MachineInstructions
            bool passed = true;
            SymbolDef cursymboldef;
            Symbol ActiveNTTRuleID;

            ClearMachineInstState();

            // iterate through all the tokens and build machine instruction
            // for each machine instruction need: optype, opinst, and up to 5 parameters
            for(int i = 0; i < size; i++) {
                // lookup instruction type in library
                cursymboldef = symbolTypeLib[(int)Tokens[i].ID];
                ActiveNTTRuleID = (Symbol)Tokens[i].NTTRuleID;
                currentLine = Tokens[i].line;
                charPos = Tokens[i].pos;

                switch(ActiveNTTRuleID) {
                    case Symbol.CONSTANT:
                    case Symbol.COLOR:
                    case Symbol.REG_PS1_4:
                    case Symbol.TEX_PS1_4:
                    case Symbol.REG_PS1_1_3:
                    case Symbol.TEX_PS1_1_3:
                        // registars can be used for read and write so they can be used for dst and arg
                        passed = SetOpParam(cursymboldef);
                        break;

                    case Symbol.DEFCONST:
                    case Symbol.UNARYOP:
                    case Symbol.BINARYOP:
                    case Symbol.TERNARYOP:
                    case Symbol.TEXOP_PS1_1_3:
                    case Symbol.TEXOP_PS1_4:
                    case Symbol.PHASEMARKER:
                    case Symbol.TEXCISCOP_PS1_1_3:
                        // if the last instruction has not been passed on then do it now
                        // make sure the pipe is clear for a new instruction
                        BuildMachineInst();
                        if(opInst == Symbol.Invalid) {
                            opInst = cursymboldef.ID;
                        }
                        else {
                            passed = false;
                        }
                        break;

                    case Symbol.DSTMASK:
                    case Symbol.SRCREP:
                    case Symbol.TEXSWIZZLE:
                        // could be a dst mask or a arg replicator
                        // if dst mask and alpha included then make up a alpha instruction: maybe best to wait until instruction args completed
                        opParams[argCnt].MaskRep = (uint)cursymboldef.pass2Data;
                        break;

                    case Symbol.DSTMOD:
                    case Symbol.DSTSAT:
                    case Symbol.PRESRCMOD:
                    case Symbol.POSTSRCMOD:
                        opParams[argCnt].Mod |= cursymboldef.pass2Data;
                        break;

                    case Symbol.NUMVAL:
                        passed = SetOpParam(cursymboldef);
                        // keep track of how many values are used
                        // update Constants array position
                        constantsPos++;
                        break;

                    case Symbol.SEPERATOR:
                        argCnt++;
                        break;
                } // end of switch

                if(!passed) {
                    break;
                }
            }// end of for: i<TokenInstCnt

            // check to see if there is still an instruction left in the pipe
            if(passed) {
                BuildMachineInst();
                // if there are no more instructions in the pipe than OpInst should be invalid
                if(opInst != Symbol.Invalid) {
                    passed = false;
                }
            }

            return passed;
        }

        /// 
        /** Build a machine instruction from token and ready it for expansion
            will expand CISC tokens using macro database

        */
        bool BindMachineInstInPassToFragmentShader(IntList PassMachineInstructions) {
            int instIDX = 0;
            int instCount = PassMachineInstructions.Count;
            bool error = false;

            while ((instIDX < instCount) && !error) {
                switch((MachineInstruction)PassMachineInstructions[instIDX]) {
                    case MachineInstruction.ColorOp1:
                        if((instIDX+7) < instCount)
                            Gl.glColorFragmentOp1ATI(
                                (int)PassMachineInstructions[instIDX+1], // op
                                (int)PassMachineInstructions[instIDX+2], // dst
                                (int)PassMachineInstructions[instIDX+3], // dstMask
                                (int)PassMachineInstructions[instIDX+4], // dstMod
                                (int)PassMachineInstructions[instIDX+5], // arg1
                                (int)PassMachineInstructions[instIDX+6], // arg1Rep
                                (int)PassMachineInstructions[instIDX+7]);// arg1Mod
                        instIDX += 8;
                        break;

                    case MachineInstruction.ColorOp2:
                        if((instIDX+10) < instCount)
                            Gl.glColorFragmentOp2ATI(
                                (int)PassMachineInstructions[instIDX+1], // op
                                (int)PassMachineInstructions[instIDX+2], // dst
                                (int)PassMachineInstructions[instIDX+3], // dstMask
                                (int)PassMachineInstructions[instIDX+4], // dstMod
                                (int)PassMachineInstructions[instIDX+5], // arg1
                                (int)PassMachineInstructions[instIDX+6], // arg1Rep
                                (int)PassMachineInstructions[instIDX+7], // arg1Mod
                                (int)PassMachineInstructions[instIDX+8], // arg2
                                (int)PassMachineInstructions[instIDX+9], // arg2Rep
                                (int)PassMachineInstructions[instIDX+10]);// arg2Mod
                        instIDX += 11;
                        break;

                    case MachineInstruction.ColorOp3:
                        if((instIDX+13) < instCount)
                            Gl.glColorFragmentOp3ATI(
                                (int)PassMachineInstructions[instIDX+1], // op
                                (int)PassMachineInstructions[instIDX+2],  // dst
                                (int)PassMachineInstructions[instIDX+3],  // dstMask
                                (int)PassMachineInstructions[instIDX+4],  // dstMod
                                (int)PassMachineInstructions[instIDX+5],  // arg1
                                (int)PassMachineInstructions[instIDX+6],  // arg1Rep
                                (int)PassMachineInstructions[instIDX+7],  // arg1Mod
                                (int)PassMachineInstructions[instIDX+8],  // arg2
                                (int)PassMachineInstructions[instIDX+9],  // arg2Rep
                                (int)PassMachineInstructions[instIDX+10], // arg2Mod
                                (int)PassMachineInstructions[instIDX+11], // arg2
                                (int)PassMachineInstructions[instIDX+12], // arg2Rep
                                (int)PassMachineInstructions[instIDX+13]);// arg2Mod
                        instIDX += 14;
                        break;

                    case MachineInstruction.AlphaOp1:
                        if((instIDX+6) < instCount)
                            Gl.glAlphaFragmentOp1ATI(
                                (int)PassMachineInstructions[instIDX+1], // op
                                (int)PassMachineInstructions[instIDX+2],   // dst
                                (int)PassMachineInstructions[instIDX+3],   // dstMod
                                (int)PassMachineInstructions[instIDX+4],   // arg1
                                (int)PassMachineInstructions[instIDX+5],   // arg1Rep
                                (int)PassMachineInstructions[instIDX+6]);  // arg1Mod
                        instIDX += 7;
                        break;

                    case MachineInstruction.AlphaOp2:
                        if((instIDX+9) < instCount)
                            Gl.glAlphaFragmentOp2ATI(
                                (int)PassMachineInstructions[instIDX+1], // op
                                (int)PassMachineInstructions[instIDX+2],   // dst
                                (int)PassMachineInstructions[instIDX+3],   // dstMod
                                (int)PassMachineInstructions[instIDX+4],   // arg1
                                (int)PassMachineInstructions[instIDX+5],   // arg1Rep
                                (int)PassMachineInstructions[instIDX+6],   // arg1Mod
                                (int)PassMachineInstructions[instIDX+7],   // arg2
                                (int)PassMachineInstructions[instIDX+8],   // arg2Rep
                                (int)PassMachineInstructions[instIDX+9]);  // arg2Mod
                        instIDX += 10;
                        break;

                    case MachineInstruction.AlphaOp3:
                        if((instIDX+12) < instCount)
                            Gl.glAlphaFragmentOp3ATI(
                                (int)PassMachineInstructions[instIDX+1], // op
                                (int)PassMachineInstructions[instIDX+2],   // dst
                                (int)PassMachineInstructions[instIDX+3],   // dstMod
                                (int)PassMachineInstructions[instIDX+4],   // arg1
                                (int)PassMachineInstructions[instIDX+5],   // arg1Rep
                                (int)PassMachineInstructions[instIDX+6],   // arg1Mod
                                (int)PassMachineInstructions[instIDX+7],   // arg2
                                (int)PassMachineInstructions[instIDX+8],   // arg2Rep
                                (int)PassMachineInstructions[instIDX+9],   // arg2Mod
                                (int)PassMachineInstructions[instIDX+10],  // arg2
                                (int)PassMachineInstructions[instIDX+11],  // arg2Rep
                                (int)PassMachineInstructions[instIDX+12]); // arg2Mod
                        instIDX += 13;
                        break;

                    case MachineInstruction.SetConstants:

                        if((instIDX+2) < instCount) {
                            int start = (int)PassMachineInstructions[instIDX+2];
                            float[] vals = new float[4];
                            vals[0] = (float)constants[start++];
                            vals[1] = (float)constants[start++];
                            vals[2] = (float)constants[start++];
                            vals[3] = (float)constants[start];

                            Gl.glSetFragmentShaderConstantATI(
                                (int)PassMachineInstructions[instIDX+1], // dst
                                vals);
                        }
                        instIDX += 3;
                        break;

                    case MachineInstruction.PassTexCoord:
                        if((instIDX+3) < instCount)
                            Gl.glPassTexCoordATI(
                                (int)PassMachineInstructions[instIDX+1], // dst
                                (int)PassMachineInstructions[instIDX+2], // coord
                                (int)PassMachineInstructions[instIDX+3]); // swizzle
                        instIDX += 4;
                        break;

                    case MachineInstruction.SampleMap:
                        if((instIDX+3) < instCount)
                            Gl.glSampleMapATI(
                                (int)PassMachineInstructions[instIDX+1], // dst
                                (int)PassMachineInstructions[instIDX+2], // interp
                                (int)PassMachineInstructions[instIDX+3]); // swizzle
                        instIDX += 4;
                        break;

                    default:
                        instIDX = instCount;
                        break;
                        // should generate an error since an unknown instruction was found
                        // instead for now the bind process is terminated and the fragment program may still function
                        // but its output may not be what was programmed

                } // end of switch

                error = (Gl.glGetError() != Gl.GL_NO_ERROR);
            }// end of while

            return !error;
        }

        /** Expand CISC tokens into PS1_4 token equivalents

        */
        bool ExpandMacro(MacroRegModify MacroMod) {
            RegModOffset regmod;

            // set source and destination registers in macro expansion
            for (int i = 0; i < MacroMod.RegModSize; i++) {
                regmod = MacroMod.RegMods[i];
                MacroMod.Macro[regmod.MacroOffset].ID = (Symbol)(regmod.RegisterBase + opParams[regmod.OpParamsIndex].Arg);
            }

            // turn macro support on so that ps.1.4 ALU instructions get put in phase 1 alu instruction sequence container
            macroOn = true;

            // pass macro tokens on to be turned into machine instructions
            // expand macro to ps.1.4 by doing recursive call to doPass2
            bool passed = Pass2scan(MacroMod.Macro, MacroMod.MacroSize);

            macroOn = false;

            return passed;
        }

        /** Expand Machine instruction into operation type and arguments and put into proper machine
            instruction container
            also expands scaler alpha machine instructions if required

        */
        bool ExpandMachineInstruction() {
            // now push instructions onto MachineInstructions container
            // assume that an instruction will be expanded
            bool passed = true;

            if (opType != MachineInstruction.Nop) {
                // a machine instruction will be built
                // this is currently the last one being built so keep track of it
                if (instructionPhase == PhaseType.PHASE2ALU) { 
                    secondLastInstructionPos = lastInstructionPos;
                    lastInstructionPos = phase2ALU_mi.Count;
                }

                switch (opType) {
                    case MachineInstruction.ColorOp1:
                    case MachineInstruction.ColorOp2:
                    case MachineInstruction.ColorOp3: {
                        AddMachineInst(instructionPhase, (int)opType);
                        AddMachineInst(instructionPhase, symbolTypeLib[(int)opInst].pass2Data);
                        // send all parameters to machine inst container
                        for(int i=0; i <= argCnt; i++) {
                            AddMachineInst(instructionPhase, opParams[i].Arg);
                            AddMachineInst(instructionPhase, (int)opParams[i].MaskRep);
                            AddMachineInst(instructionPhase, opParams[i].Mod);
                            // check if source register read is valid in this phase
                            passed &= IsRegisterReadValid(instructionPhase, i);
                        }

                        // record which registers were written to and in which phase
                        // opParams[0].Arg is always the destination register r0 -> r5
                        UpdateRegisterWriteState(instructionPhase);

                    }
                        break;

                    case MachineInstruction.SetConstants:
                        AddMachineInst(instructionPhase, (int)opType);
                        AddMachineInst(instructionPhase, opParams[0].Arg); // dst
                        AddMachineInst(instructionPhase, constantsPos); // index into constants array
                        break;

                    case MachineInstruction.PassTexCoord:
                    case MachineInstruction.SampleMap:
                        // if source is a temp register than place instruction in phase 2 Texture ops
                        if ((opParams[1].Arg >= Gl.GL_REG_0_ATI) && (opParams[1].Arg <= Gl.GL_REG_5_ATI)) {
                            instructionPhase = PhaseType.PHASE2TEX;
                        }

                        AddMachineInst(instructionPhase, (int)opType);
                        AddMachineInst(instructionPhase, opParams[0].Arg); // dst
                        AddMachineInst(instructionPhase, opParams[1].Arg); // coord
                        AddMachineInst(instructionPhase, (int)opParams[1].MaskRep + Gl.GL_SWIZZLE_STR_ATI); // swizzle
                        // record which registers were written to and in which phase
                        // opParams[0].Arg is always the destination register r0 -> r5
                        UpdateRegisterWriteState(instructionPhase);
                        break;

                    case MachineInstruction.Tex: // PS_1_1 emulation - turn CISC into RISC - phase 1
                        AddMachineInst(instructionPhase, (int)MachineInstruction.SampleMap);
                        AddMachineInst(instructionPhase, opParams[0].Arg); // dst
                        // tex tx becomes texld rx, tx with x: 0 - 3
                        AddMachineInst(instructionPhase, opParams[0].Arg - Gl.GL_REG_0_ATI + Gl.GL_TEXTURE0_ARB); // interp
                        // default to str which fills rgb of destination register
                        AddMachineInst(instructionPhase, Gl.GL_SWIZZLE_STR_ATI); // swizzle
                        // record which registers were written to and in which phase
                        // opParams[0].Arg is always the destination register r0 -> r5
                        UpdateRegisterWriteState(instructionPhase);
                        break;

                    case MachineInstruction.TexCoord: // PS_1_1 emulation - turn CISC into RISC - phase 1
                        AddMachineInst(instructionPhase, (int)MachineInstruction.PassTexCoord);
                        AddMachineInst(instructionPhase, opParams[0].Arg); // dst
                        // texcoord tx becomes texcrd rx, tx with x: 0 - 3
                        AddMachineInst(instructionPhase, opParams[0].Arg - Gl.GL_REG_0_ATI + Gl.GL_TEXTURE0_ARB); // interp
                        // default to str which fills rgb of destination register
                        AddMachineInst(instructionPhase, Gl.GL_SWIZZLE_STR_ATI); // swizzle
                        // record which registers were written to and in which phase
                        // opParams[0].Arg is always the destination register r0 -> r5
                        UpdateRegisterWriteState(instructionPhase);
                        break;

                } // end of switch (opType)
            } // end of if (opType != mi_NOP)

            if(do_Alpha) {
                // process alpha channel
                //
                // a scaler machine instruction will be built
                // this is currently the last one being built so keep track of it
                if (instructionPhase == PhaseType.PHASE2ALU) { 
                    secondLastInstructionPos = lastInstructionPos;
                    lastInstructionPos = phase2ALU_mi.Count;
                }

                MachineInstruction alphaoptype = (MachineInstruction)(MachineInstruction.AlphaOp1 + argCnt - 1);
                AddMachineInst(instructionPhase, (int)alphaoptype);
                AddMachineInst(instructionPhase, symbolTypeLib[(int)opInst].pass2Data);

                // put all parameters in instruction que
                for(int i = 0; i <= argCnt; i++) {
                    AddMachineInst(instructionPhase, opParams[i].Arg);
                    // destination parameter has no mask since it is the alpha channel
                    // don't push mask for parrameter 0 (dst)
                    if(i > 0) {
                        AddMachineInst(instructionPhase, (int)opParams[i].MaskRep);
                    }

                    AddMachineInst(instructionPhase, opParams[i].Mod);
                    // check if source register read is valid in this phase
                    passed &= IsRegisterReadValid(instructionPhase, i);
                }

                UpdateRegisterWriteState(instructionPhase);
            }

            // instruction passed on to machine instruction so clear the pipe
            ClearMachineInstState();

            return passed;
        }

        // mainly used by tests - too slow for use in binding
        int GetMachineInst(int Idx) {
            if (Idx < phase1TEX_mi.Count) {
                return (int)phase1TEX_mi[Idx];
            }
            else {
                Idx -= phase1TEX_mi.Count;
                if (Idx < phase1ALU_mi.Count) {
                    return (int)phase1ALU_mi[Idx];
                }
                else {
                    Idx -= phase1ALU_mi.Count;
                    if (Idx < phase2TEX_mi.Count) {
                        return (int)phase2TEX_mi[Idx];
                    }
                    else {
                        Idx -= phase2TEX_mi.Count;
                        if (Idx < phase2ALU_mi.Count) {
                            return (int)phase2ALU_mi[Idx];
                        }
                    }
                }
            }

            return 0;
        }

        int GetMachineInstCount() {
            return (phase1TEX_mi.Count + phase1ALU_mi.Count + phase2TEX_mi.Count + phase2ALU_mi.Count);
        }

        void AddMachineInst(PhaseType phase, int inst) {
            switch(phase) {
                case PhaseType.PHASE1TEX:
                    phase1TEX_mi.Add(inst);
                    break;

                case PhaseType.PHASE1ALU:
                    phase1ALU_mi.Add(inst);
                    break;

                case PhaseType.PHASE2TEX:
                    phase2TEX_mi.Add(inst);

                    break;

                case PhaseType.PHASE2ALU:
                    phase2ALU_mi.Add(inst);
                    break;
            } // end switch(phase)
        }

        void ClearAllMachineInst() {
            phase1TEX_mi.Clear();
            phase1ALU_mi.Clear();
            phase2TEX_mi.Clear();
            phase2ALU_mi.Clear();

            // reset write state for all registers
            for(int i = 0; i < 6; i++) {
                Phase_RegisterUsage[i].Phase1Write = false;
                Phase_RegisterUsage[i].Phase2Write = false;
            }

            phaseMarkerFound = false;
            constantsPos = -4;
            // keep track of the last instruction built
            // this info is used at the end of pass 2 to optimize the machine code
            lastInstructionPos = 0;
            secondLastInstructionPos = 0;

            macroOn = false;  // macro's off at the beginning
            texm3x3padCount = 0;
        }

        void UpdateRegisterWriteState(PhaseType phase) {
            int reg_offset = opParams[0].Arg - Gl.GL_REG_0_ATI;

            switch(phase) {

                case PhaseType.PHASE1TEX:
                case PhaseType.PHASE1ALU:
                    Phase_RegisterUsage[reg_offset].Phase1Write = true;
                    break;

                case PhaseType.PHASE2TEX:
                case PhaseType.PHASE2ALU:
                    Phase_RegisterUsage[reg_offset].Phase2Write = true;
                    break;

            } // end switch(phase)
        }

        bool IsRegisterReadValid(PhaseType phase, int param) {
            bool passed = true; // assume everything will go alright

            // if in phase 2 ALU and argument is a source
            if((phase == PhaseType.PHASE2ALU) && (param > 0)) {
                // is source argument a temp register r0 - r5?
                if((opParams[param].Arg >= Gl.GL_REG_0_ATI) && (opParams[param].Arg <= Gl.GL_REG_5_ATI)) {
                    int reg_offset = opParams[param].Arg - Gl.GL_REG_0_ATI;
                    // if register was not written to in phase 2 but was in phase 1
                    if((Phase_RegisterUsage[reg_offset].Phase2Write == false) && Phase_RegisterUsage[reg_offset].Phase1Write) {
                        // only perform register pass if there are ALU instructions in phase 1
                        if(phase1ALU_mi.Count > 0) {
                            // build machine instructions for passing a register from phase 1 to phase 2
                            // NB: only rgb components of register will get passed

                            AddMachineInst(PhaseType.PHASE2TEX, (int)MachineInstruction.PassTexCoord);
                            AddMachineInst(PhaseType.PHASE2TEX, opParams[param].Arg); // dst
                            AddMachineInst(PhaseType.PHASE2TEX, opParams[param].Arg); // coord
                            AddMachineInst(PhaseType.PHASE2TEX, Gl.GL_SWIZZLE_STR_ATI); // swizzle

                            // mark register as being written to
                            Phase_RegisterUsage[reg_offset].Phase2Write = true;
                        }
                    }
                        // register can not be used because it has not been written to previously
                    else {
                        passed = false;
                    }
                }

            }

            return passed;
        }

        /// <summary>
        ///     Binds machine instructions generated in Pass 2 to the ATI GL fragment shader.
        /// </summary>
        /// <returns></returns>
        public bool BindAllMachineInstToFragmentShader() {
            bool passed;

            // there are 4 machine instruction queues to pass to the ATI fragment shader
            passed = BindMachineInstInPassToFragmentShader(phase1TEX_mi);
            passed &= BindMachineInstInPassToFragmentShader(phase1ALU_mi);
            passed &= BindMachineInstInPassToFragmentShader(phase2TEX_mi);
            passed &= BindMachineInstInPassToFragmentShader(phase2ALU_mi);

            return passed;
        }

        #endregion Members

        #region Compiler2Pass Members

        /// <summary>
        ///     Pass 1 is completed so now take tokens generated and build machine instructions.
        /// </summary>
        /// <returns></returns>
        protected override bool DoPass2() {
            ClearAllMachineInst();

            // if pass 2 was successful, optimize the machine instructions
            bool passed = Pass2scan((TokenInstruction[])tokenInstructions.ToArray((typeof(TokenInstruction))), tokenInstructions.Count);

            if (passed) {
                Optimize();  
            }

            return passed;
        }

        #endregion Compiler2Pass Members

        #region Test Cases

        struct Test1Result {
            public char character;
            public int line;

            public Test1Result(char c, int line) {
                this.character = c;
                this.line = line;
            }
        }

        struct TestFloatResult{
            public string testString;
            public float val;
            public int charSize;

            public TestFloatResult(string test, float val, int charSize) {
                this.testString = test;
                this.val = val;
                this.charSize = charSize;
            }
        }

        string testString1 = "   \n\r  //c  \n\r// test\n\r  \t  c   - \n\r ,  e";
        string testString3 = "mov r0,c1";
        string testSymbols = "mov";

        Test1Result[] test1results = new Test1Result[] {
            new Test1Result('c', 4),
            new Test1Result('-', 4),
            new Test1Result(',', 5),
            new Test1Result('e', 5)
        };

        TestFloatResult[] testFloatResults = {
            new TestFloatResult("1 test", 1.0f, 1),
            new TestFloatResult("2.3f test", 2.3f, 3),
            new TestFloatResult("-0.5 test", -0.5f, 4),
            new TestFloatResult(" 23.6 test", 23.6f, 5),
            new TestFloatResult("  -0.021 test", -0.021f, 8),
            new TestFloatResult("12 test", 12.0f, 2),
            new TestFloatResult("3test", 3.0f, 1)
        };

        int resultID = 0;

        public bool RunTests() {

            // ***TEST 1***
            // See if PositionToNextSymbol can find a valid symbol
            Console.WriteLine("**Testing: PositionToNextSymbol\n");

            source = testString1;
            charPos = 0;
            currentLine = 1;
            endOfSource = source.Length;
            while (PositionToNextSymbol()) {
                Console.WriteLine("  Character found: {0}   Line:{1}  : " , source[charPos], currentLine);
                if((source[charPos] == test1results[resultID].character) && 
                    (currentLine == test1results[resultID].line)) {
                    Console.WriteLine("Passed!");
                }
                else {
                    Console.WriteLine("FAILED!");
                }

                resultID++;
                charPos++;
            }

            Console.WriteLine("**Finished testing: PositionToNextSymbol\n");

            // ***TEST 2***
            // Did the type lib get initialized properly with a default name index
            Console.WriteLine("**Testing: GetTypeDefText\n");
            string resultStr = GetTypeDefText(Symbol.MOV);
            Console.WriteLine(" Default name of mov is {0}", resultStr);
            Console.WriteLine("**Finished testing: GetTypeDefText\n");

            // ***TEST 3***
            // Does IsSymbol work properly?
            Console.WriteLine("**Testing: IsSymbol\n");

            source = testString3;
            charPos = 0;
            Console.WriteLine("  Before: {0}", source);
            Console.WriteLine("  Symbol to find: {0}", testSymbols);

            if(IsSymbol(testSymbols, out resultID)) {
                Console.WriteLine("  After: {0} : {1}", source.Substring(resultID + 1), (source[resultID + 1] == 'r')? "Passed." : "Failed!");
            }
            else {
                Console.WriteLine("Failed!");
            }

            Console.WriteLine("  Symbol size: {0}", resultID);

            Console.WriteLine("**Finished testing: IsSymbol\n");

            // ***TEST 4***
            // Does IsFloatValue work properly?
            Console.WriteLine("**Testing: IsFloatValue\n");

            float val = 0;
            int charSize = 0;
            charPos = 0;

            for(int i = 0; i < testFloatResults.Length; i++) {
                source = testFloatResults[i].testString;
                Console.WriteLine("     Test string {0}", source);
                IsFloatValue(out val, out charSize);
                Console.WriteLine("     Value is: {0}, should be {1}: {2}", val, testFloatResults[i].val, (val == testFloatResults[i].val) ? "Passed" : "Failed");
                Console.WriteLine("     Char size is: {0}, should be {1}: {2}", charSize, testFloatResults[i].charSize, (charSize == testFloatResults[i].charSize) ? "Passed" : "Failed");
            }

            Console.WriteLine("**Finished testing: IsFloatValue\n");

            // ***TEST 5***
            // Simple compile test for ps.1.4
            string CompileTest1src = "ps.1.4\n";
            Symbol[] CompileTest1result = {Symbol.PS_1_4};
            TestCompile("Basic PS_1_4", CompileTest1src, CompileTest1result);

            // ***TEST 6***
            // Simple compile test for ps1.1
            string CompileTest2src = "ps.1.1\n";
            Symbol[] CompileTest2result = {Symbol.PS_1_1};
            TestCompile("Basic PS_1_1", CompileTest2src, CompileTest2result);

            // ***TEST 7***
            // Simple compile test, ps.1.4 with defines
            string CompileTest3src = "ps.1.4\ndef c0, 1.0, 2.0, 3.0, 4.0\n";
            Symbol[] CompileTest3result = {Symbol.PS_1_4, Symbol.DEF, Symbol.C0, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA,
		        Symbol.VALUE, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA, Symbol.VALUE};

            TestCompile("PS_1_4 with defines", CompileTest3src, CompileTest3result);

            // ***TEST 8***
            // Simple compile test, ps.1.4 with 2 defines
            string CompileTest4src = "ps.1.4\n//test kkl \ndef c0, 1.0, 2.0, 3.0, 4.0\ndef c3, 1.0, 2.0, 3.0, 4.0\n";
            Symbol[] CompileTest4result = {Symbol.PS_1_4, Symbol.DEF, Symbol.C0, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA,
		        Symbol.VALUE, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA, Symbol.VALUE,Symbol.DEF, Symbol.C3, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA,
		        Symbol.VALUE, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA, Symbol.VALUE};

            TestCompile("PS_1_4 with 2 defines", CompileTest4src, CompileTest4result);

            // ***TEST 9***
            // Simple compile test, checking machine instructions
            int[] CompileTest5MachinInstResults = {(int)MachineInstruction.SetConstants, Gl.GL_CON_0_ATI, 0};

            TestCompile("PS_1_4 with defines", CompileTest3src, CompileTest3result, CompileTest5MachinInstResults);

            // ***TEST 10***
            // Simple compile test, checking ALU instructions
            string CompileTest6Src = "ps.1.4\nmov r0.xzw, c1 \nmul r3, r2, c3";
            Symbol[] CompileTest6result = {Symbol.PS_1_4, Symbol.MOV, Symbol.R0, Symbol.RBA, Symbol.COMMA, Symbol.C1,
	            Symbol.MUL, Symbol.R3, Symbol.COMMA, Symbol.R2, Symbol.COMMA, Symbol.C3};
              
            TestCompile("PS_1_4 ALU simple", CompileTest6Src, CompileTest6result);


            // test to see if PS_1_4 compile pass 2 generates the proper machine instructions
	        string CompileTest7Src = "ps.1.4\ndef c0,1.0,2.0,3.0,4.0\nmov_x8 r1,v0\nmov r0,r1.g";

	        Symbol[] CompileTest7result = {
		        Symbol.PS_1_4, Symbol.DEF, Symbol.C0, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA,
		        Symbol.VALUE, Symbol.COMMA, Symbol.VALUE, Symbol.COMMA, Symbol.VALUE, Symbol.MOV, Symbol.X8, Symbol.R1, Symbol.COMMA,
		        Symbol.V0, Symbol.MOV, Symbol.R0, Symbol.COMMA, Symbol.R1, Symbol.GGGG
	        };

	        int[] CompileTest7MachinInstResults = {
		        (int)MachineInstruction.SetConstants, Gl.GL_CON_0_ATI, 0,
		        (int)MachineInstruction.ColorOp1, Gl.GL_MOV_ATI, Gl.GL_REG_1_ATI, RGB_BITS, Gl.GL_8X_BIT_ATI,	Gl.GL_PRIMARY_COLOR_ARB, Gl.GL_NONE, Gl.GL_NONE,
		        (int)MachineInstruction.AlphaOp1, Gl.GL_MOV_ATI, Gl.GL_REG_1_ATI, Gl.GL_8X_BIT_ATI, Gl.GL_PRIMARY_COLOR_ARB, Gl.GL_NONE, Gl.GL_NONE,
		        (int)MachineInstruction.ColorOp1, Gl.GL_MOV_ATI, Gl.GL_REG_0_ATI, RGB_BITS, Gl.GL_NONE,Gl.GL_REG_1_ATI, Gl.GL_GREEN, Gl.GL_NONE,
		        (int)MachineInstruction.AlphaOp1, Gl.GL_MOV_ATI, Gl.GL_REG_0_ATI, Gl.GL_NONE, Gl.GL_REG_1_ATI, Gl.GL_GREEN, Gl.GL_NONE,
	        };

	        TestCompile("PS_1_4 ALU simple modifier", CompileTest7Src, CompileTest7result, CompileTest7MachinInstResults);

            return true;
        }

        private void TestCompile(string testName, string snippet, Symbol[] expectedResults) {
            TestCompile(testName, snippet, expectedResults, null);
        }

        private void TestCompile(string testName, string snippet, Symbol[] expectedResults, int[] machineInstResults) {
            string passed = "PASSED";
            string failed = "***** FAILED ****";

            SetActiveContexts((uint)ContextKeyPattern.PS_BASE);

            Console.WriteLine("*** TESTING: {0} Compile: Check Pass 1 and 2\n", testName);
            Console.WriteLine("  Source to compile:\n[\n{0}\n]", snippet);

            bool compiled = Compile(snippet);

            Console.WriteLine("  Pass 1 Lines scanned: {0}, Tokens produced: {1} out of {2}: {3}",
                currentLine, tokenInstructions.Count, expectedResults.Length,
                (tokenInstructions.Count == expectedResults.Length) ? passed : failed);

            Console.WriteLine("    Validating Pass 1:");

            Console.WriteLine("\n  Tokens:");
            for(int i = 0; i < tokenInstructions.Count; i++) {
                Console.WriteLine("    Token[{0}] [{1}] {2}: [{3}] {4}: {5}", 
                    i, 
                    GetTypeDefText(((TokenInstruction)tokenInstructions[i]).ID),
                    ((TokenInstruction)tokenInstructions[i]).ID, 
                    GetTypeDefText(expectedResults[i]), 
                    expectedResults[i],
                    (((TokenInstruction)tokenInstructions[i]).ID == expectedResults[i]) ? passed : failed);
            }

            if(machineInstResults != null) {
                Console.WriteLine("\n  Machine Instructions:");

                int MIcount = GetMachineInstCount();

                Console.WriteLine("  Pass 2 Machine Instructions generated: {0} out of {1}: {2}", 
                    MIcount,
                    machineInstResults.Length, 
                    (MIcount == machineInstResults.Length) ? passed : failed);

                Console.WriteLine("    Validating Pass 2:");

                for(int i = 0; i < MIcount; i++) {
                    Console.WriteLine("    instruction[{0}] = {1} : {2} : {3}", i, 
                        GetMachineInst(i), 
                        machineInstResults[i], 
                        (GetMachineInst(i) == machineInstResults[i]) ? passed : failed);
                }

                Console.WriteLine("    Constants:");

                for(int i=0; i < 4; i++) {
                    Console.WriteLine("    Constants[{0}] = {1} : {2}", 
                        i, 
                        constants[i], 
                        ((float)constants[i] == (1.0f + i)) ? passed : failed);
                }
            }

            if(!compiled) {
                Console.WriteLine(failed);
            }

            Console.WriteLine("\nFinished testing: {0} Compile: Check Pass 2\n\n", testName);

            SetActiveContexts((uint)ContextKeyPattern.PS_BASE);
        }

        #endregion Test Cases
	}
}
