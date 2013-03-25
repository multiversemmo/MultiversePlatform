using TUVienna.CS_CUP.Runtime;
%%
%line
%char
%cup
%state COMMENT BODY

D=			[0-9]
L=			[a-zA-Z_-]

%%
<YYINITIAL> [\r\n]		{ System.Console.WriteLine("Skipped header line"); yybegin(BODY); break; }
<YYINITIAL> [^\r\n]		{ break; }

<BODY> "//"			{ yybegin(COMMENT); break; }
<BODY> "#"			{ yybegin(COMMENT); break; }

<BODY> "array"			{ return new Symbol(sym.ARRAY); }
<BODY> "template"		{ return new Symbol(sym.TEMPLATE); }

<BODY> "binary"			{ return new Symbol(sym.BINARY_TYPE); }
<BODY> "binary_resource"	{ return new Symbol(sym.BINARY_RESOURCE_TYPE); }
<BODY> "char"			{ return new Symbol(sym.CHAR_TYPE); }
<BODY> "cstring"		{ return new Symbol(sym.CSTRING_TYPE); }
<BODY> "double"			{ return new Symbol(sym.DOUBLE_TYPE); }
<BODY> "dword"			{ return new Symbol(sym.DWORD_TYPE); }
<BODY> "float"			{ return new Symbol(sym.FLOAT_TYPE); }
<BODY> "sdword"			{ return new Symbol(sym.SDWORD_TYPE); }
<BODY> "string"			{ return new Symbol(sym.STRING_TYPE); }
<BODY> "sword"			{ return new Symbol(sym.SWORD_TYPE); }
<BODY> "uchar"			{ return new Symbol(sym.UCHAR_TYPE); }
<BODY> "ulonglong"		{ return new Symbol(sym.ULONGLONG_TYPE); }
<BODY> "unicode"		{ return new Symbol(sym.UNICODE_TYPE); }
<BODY> "word"			{ return new Symbol(sym.WORD_TYPE); }

<BODY> "Header" 		{ return new Symbol(sym.HEADER_TYPE); }
<BODY> "Vector" 		{ return new Symbol(sym.VECTOR_TYPE); }
<BODY> "ColorRGB" 		{ return new Symbol(sym.COLOR_RGB_TYPE); }
<BODY> "ColorRGBA" 		{ return new Symbol(sym.COLOR_RGBA_TYPE); }
<BODY> "Coords2d" 		{ return new Symbol(sym.COORDS2D_TYPE); }
<BODY> "Matrix4x4" 		{ return new Symbol(sym.MATRIX4X4_TYPE); }
<BODY> "FrameTransformMatrix"	{ return new Symbol(sym.FRAME_TRANSFORM_MATRIX_TYPE); }
<BODY> "Frame"			{ return new Symbol(sym.FRAME_TYPE); }
<BODY> "MeshFace"		{ return new Symbol(sym.MESH_FACE_TYPE); }
<BODY> "Mesh"			{ return new Symbol(sym.MESH_TYPE); }
<BODY> "TextureFilename"	{ return new Symbol(sym.TEXTURE_FILENAME_TYPE); }
<BODY> "Material"		{ return new Symbol(sym.MATERIAL_TYPE); }
<BODY> "MeshTextureCoords"	{ return new Symbol(sym.MESH_TEXTURE_COORDS_TYPE); }
<BODY> "MeshNormals"		{ return new Symbol(sym.MESH_NORMALS_TYPE); }
<BODY> "MeshMaterialList"	{ return new Symbol(sym.MESH_MATERIAL_LIST_TYPE); }

<BODY> {L}({L}|{D})*		{ return new Symbol(sym.IDENTIFIER, yytext()); }

<BODY> {D}+			{ return new Symbol(sym.INTEGER, (object)int.Parse(yytext())); }
<BODY> -?{D}*"."{D}+		{ return new Symbol(sym.FLOAT, float.Parse(yytext())); }
<BODY> -?{D}+"."{D}*		{ return new Symbol(sym.FLOAT, float.Parse(yytext())); }
<BODY> \"(\.|[^\"])*\"		{ return new Symbol(sym.STRING, yytext()); }

<BODY> "..."			{ return new Symbol(sym.ELLIPSIS); }
<BODY> ";"			{ return new Symbol(sym.SEMI); }
<BODY> "{"			{ return new Symbol(sym.LCURLY); }
<BODY> "}"			{ return new Symbol(sym.RCURLY); }
<BODY> ","			{ return new Symbol(sym.COMMA); }
<BODY> ":"			{ return new Symbol(sym.COLON); }
<BODY> "["			{ return new Symbol(sym.LSQUARE); }
<BODY> "]"			{ return new Symbol(sym.RSQUARE); }
<BODY> "<"			{ return new Symbol(sym.LANGLE); }
<BODY>" >"			{ return new Symbol(sym.RANGLE); }
<BODY> "|"			{ return new Symbol(sym.PIPE); }

<BODY> [ \t\v\r\n\f]		{ }
<BODY> . 			{ System.Console.Error.WriteLine("Illegal body character: "+yytext()); break; }

<COMMENT> [\r\n]		{ yybegin(BODY); break; }
<COMMENT> [^\r\n]		{ break; }
