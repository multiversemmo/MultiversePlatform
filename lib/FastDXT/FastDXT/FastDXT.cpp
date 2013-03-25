// FastDXT.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "FastDXT.h"
#include <intrin.h>

FASTDXT_API BOOL HasMMX = FALSE;
FASTDXT_API BOOL HasSSE2 = FALSE;

void initialize(void)
{
    int ret[4];
    __cpuid(ret, 1);
    if ( ret[3] & ( 1 << 23 ) ) {
        HasMMX = TRUE;
    }
    if ( ret[3] & ( 1 << 26 ) ) {
        HasSSE2 = TRUE;
    }
}

FASTDXT_API BOOL GetHasMMX(void)
{
    return HasMMX;
}

FASTDXT_API BOOL GetHasSSE2(void)
{
    return HasSSE2;
}

byte *globalOutData;

#define ALIGN16( x ) __declspec(align(16)) x

word ColorTo565( const byte *color ) {
    return ( ( color[ 0 ] >> 3 ) << 11 ) | ( ( color[ 1 ] >> 2 ) << 5 ) | ( color[ 2 ] >> 3 );
}

void EmitByte( byte b ) {
    globalOutData[0] = b;
    globalOutData += 1;
}
void EmitWord( word s ) {
    globalOutData[0] = ( s >> 0 ) & 255;
    globalOutData[1] = ( s >> 8 ) & 255;
    globalOutData += 2;
}
void EmitDoubleWord( dword i ) {
    globalOutData[0] = ( i >> 0 ) & 255;
    globalOutData[1] = ( i >> 8 ) & 255;
    globalOutData[2] = ( i >> 16 ) & 255;
    globalOutData[3] = ( i >> 24 ) & 255;
    globalOutData += 4;
}

/*
SIMD Optimized Extraction of a Texture Block
Copyright (C) 2006 Id Software, Inc.
Written by J.M.P. van Waveren
This code is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.
This code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.
*/
void ExtractBlock_MMX( const byte *inPtr, int width, byte *colorBlock ) {
    __asm {
        mov esi, inPtr
        mov edi, colorBlock
        mov eax, width
        shl eax, 2
        movq mm0, [esi+0]
        movq [edi+ 0], mm0
        movq mm1, [esi+8]
        movq [edi+ 8], mm1
        movq mm2, [esi+eax+0] // + 4 * width
        movq [edi+16], mm2
        movq mm3, [esi+eax+8] // + 4 * width
        movq [edi+24], mm3
        movq mm4, [esi+eax*2+0] // + 8 * width
        movq [edi+32], mm4
        movq mm5, [esi+eax*2+8] // + 8 * width
        add esi, eax
        movq [edi+40], mm5
        movq mm6, [esi+eax*2+0] // + 12 * width
        movq [edi+48], mm6
        movq mm7, [esi+eax*2+8] // + 12 * width
        movq [edi+56], mm7
        emms
    }
}
void ExtractBlock_SSE2( const byte *inPtr, int width, byte *colorBlock ) {
    __asm {
        mov esi, inPtr
        mov edi, colorBlock
        mov eax, width
        shl eax, 2
        movdqa xmm0, [esi]
        movdqa [edi+ 0], xmm0
        movdqa xmm1, [esi+eax] // + 4 * width
        movdqa [edi+16], xmm1
        movdqa xmm2, [esi+eax*2] // + 8 * width
        add esi, eax
        movdqa [edi+32], xmm2
        movdqa xmm3, [esi+eax*2] // + 12 * width
        movdqa [edi+48], xmm3
    }
}

/*
SIMD Optimized Calculation of Line Through Color Space
Copyright (C) 2006 Id Software, Inc.
Written by J.M.P. van Waveren
This code is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.
This code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.
*/
#define R_SHUFFLE_D( x, y, z, w ) (( (w) & 3 ) << 6 | ( (z) & 3 ) << 4 | ( (y) & 3 ) << 2 | ( (x) & 3 ))
ALIGN16( static byte SIMD_MMX_byte_0[8] ) = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
#define INSET_SHIFT 4 // inset the bounding box with ( range >> shift )
void GetMinMaxColors_MMX( const byte *colorBlock, byte *minColor, byte *maxColor ) {
    __asm {
        mov eax, colorBlock
        mov esi, minColor
        mov edi, maxColor
        // get bounding box
        pshufw mm0, qword ptr [eax+ 0], R_SHUFFLE_D( 0, 1, 2, 3 )
        pshufw mm1, qword ptr [eax+ 0], R_SHUFFLE_D( 0, 1, 2, 3 )
        pminub mm0, qword ptr [eax+ 8]
        pmaxub mm1, qword ptr [eax+ 8]
        pminub mm0, qword ptr [eax+16]
        pmaxub mm1, qword ptr [eax+16]
        pminub mm0, qword ptr [eax+24]
        pmaxub mm1, qword ptr [eax+24]
        pminub mm0, qword ptr [eax+32]
        pmaxub mm1, qword ptr [eax+32]
        pminub mm0, qword ptr [eax+40]
        pmaxub mm1, qword ptr [eax+40]
        pminub mm0, qword ptr [eax+48]
        pmaxub mm1, qword ptr [eax+48]
        pminub mm0, qword ptr [eax+56]
        pmaxub mm1, qword ptr [eax+56]
        pshufw mm6, mm0, R_SHUFFLE_D( 2, 3, 2, 3 )
        pshufw mm7, mm1, R_SHUFFLE_D( 2, 3, 2, 3 )
        pminub mm0, mm6
        pmaxub mm1, mm7
        // inset the bounding box
        punpcklbw mm0, SIMD_MMX_byte_0
        punpcklbw mm1, SIMD_MMX_byte_0
        movq mm2, mm1
        psubw mm2, mm0
        psrlw mm2, INSET_SHIFT
        paddw mm0, mm2
        psubw mm1, mm2
        packuswb mm0, mm0
        packuswb mm1, mm1
        // store bounding box extents
        movd dword ptr [esi], mm0
        movd dword ptr [edi], mm1
        emms
    }
}

ALIGN16( static byte SIMD_SSE2_byte_0[16] ) = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

void GetMinMaxColors_SSE2( const byte *colorBlock, byte *minColor, byte *maxColor ) {
    __asm {
        mov eax, colorBlock
        mov esi, minColor
        mov edi, maxColor
        // get bounding box
        movdqa xmm0, qword ptr [eax+ 0]
        movdqa xmm1, qword ptr [eax+ 0]
        pminub xmm0, qword ptr [eax+16]
        pmaxub xmm1, qword ptr [eax+16]
        pminub xmm0, qword ptr [eax+32]
        pmaxub xmm1, qword ptr [eax+32]
        pminub xmm0, qword ptr [eax+48]
        pmaxub xmm1, qword ptr [eax+48]
        pshufd xmm3, xmm0, R_SHUFFLE_D( 2, 3, 2, 3 )
        pshufd xmm4, xmm1, R_SHUFFLE_D( 2, 3, 2, 3 )
        pminub xmm0, xmm3
        pmaxub xmm1, xmm4
        pshuflw xmm6, xmm0, R_SHUFFLE_D( 2, 3, 2, 3 )
        pshuflw xmm7, xmm1, R_SHUFFLE_D( 2, 3, 2, 3 )
        pminub xmm0, xmm6
        pmaxub xmm1, xmm7
        // inset the bounding box
        punpcklbw xmm0, SIMD_SSE2_byte_0
        punpcklbw xmm1, SIMD_SSE2_byte_0
        movdqa xmm2, xmm1
        psubw xmm2, xmm0
        psrlw xmm2, INSET_SHIFT
        paddw xmm0, xmm2
        psubw xmm1, xmm2
        packuswb xmm0, xmm0
        packuswb xmm1, xmm1
        // store bounding box extents
        movd dword ptr [esi], xmm0
        movd dword ptr [edi], xmm1
    }
}

/*
SIMD Optimized Calculation of Color Indices
Copyright (C) 2006 Id Software, Inc.
Written by J.M.P. van Waveren
This code is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.
This code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.
*/

#define C565_5_MASK 0xF8 // 0xFF minus last three bits
#define C565_6_MASK 0xFC // 0xFF minus last two bits

ALIGN16( static word SIMD_MMX_word_0[4] ) = { 0x0000, 0x0000, 0x0000, 0x0000 };
ALIGN16( static word SIMD_MMX_word_1[4] ) = { 0x0001, 0x0001, 0x0001, 0x0001 };
ALIGN16( static word SIMD_MMX_word_2[4] ) = { 0x0002, 0x0002, 0x0002, 0x0002 };
ALIGN16( static word SIMD_MMX_word_div_by_3[4] ) = { (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1 };
ALIGN16( static byte SIMD_MMX_byte_colorMask[8] ) = { C565_5_MASK, C565_6_MASK, C565_5_MASK, 0x00, 0x00, 0x00, 0x00, 0x00 };

void EmitColorIndices_MMX( const byte *colorBlock, const byte *minColor, const byte *maxColor ) {
    ALIGN16( byte color0[8] );
    ALIGN16( byte color1[8] );
    ALIGN16( byte color2[8] );
    ALIGN16( byte color3[8] );
    ALIGN16( byte result[8] );
    __asm {
        mov esi, maxColor
        mov edi, minColor
        pxor mm7, mm7
        movq result, mm7
        movd mm0, [esi]
        pand mm0, SIMD_MMX_byte_colorMask
        punpcklbw mm0, mm7
        pshufw mm4, mm0, R_SHUFFLE_D( 0, 3, 2, 3 )
        pshufw mm5, mm0, R_SHUFFLE_D( 3, 1, 3, 3 )
        psrlw mm4, 5
        psrlw mm5, 6
        por mm0, mm4
        por mm0, mm5
        movq mm2, mm0
        packuswb mm2, mm7
        movq color0, mm2
        movd mm1, [edi]
        pand mm1, SIMD_MMX_byte_colorMask
        punpcklbw mm1, mm7
        pshufw mm4, mm1, R_SHUFFLE_D( 0, 3, 2, 3 )
        pshufw mm5, mm1, R_SHUFFLE_D( 3, 1, 3, 3 )
        psrlw mm4, 5
        psrlw mm5, 6
        por mm1, mm4
        por mm1, mm5
        movq mm3, mm1
        packuswb mm3, mm7
        movq color1, mm3
        movq mm6, mm0
        paddw mm6, mm0
        paddw mm6, mm1
        pmulhw mm6, SIMD_MMX_word_div_by_3 // * ( ( 1 << 16 ) / 3 + 1 ) ) >> 16
        packuswb mm6, mm7
        movq color2, mm6
        paddw mm1, mm1
        paddw mm0, mm1
        pmulhw mm0, SIMD_MMX_word_div_by_3 // * ( ( 1 << 16 ) / 3 + 1 ) ) >> 16
        packuswb mm0, mm7
        movq color3, mm0
        mov eax, 48
        mov esi, colorBlock
    loop1: // iterates 4 times
        movd mm3, dword ptr [esi+eax+0]
        movd mm5, dword ptr [esi+eax+4]
        movq mm0, mm3
        movq mm6, mm5
        psadbw mm0, color0
        psadbw mm6, color0
        packssdw mm0, mm6
        movq mm1, mm3
        movq mm6, mm5
        psadbw mm1, color1
        psadbw mm6, color1
        packssdw mm1, mm6
        movq mm2, mm3
        movq mm6, mm5
        psadbw mm2, color2
        psadbw mm6, color2
        packssdw mm2, mm6
        psadbw mm3, color3
        psadbw mm5, color3
        packssdw mm3, mm5
        movd mm4, dword ptr [esi+eax+8]
        movd mm5, dword ptr [esi+eax+12]
        movq mm6, mm4
        movq mm7, mm5
        psadbw mm6, color0
        psadbw mm7, color0
        packssdw mm6, mm7
        packssdw mm0, mm6 // d0
        movq mm6, mm4
        movq mm7, mm5
        psadbw mm6, color1
        psadbw mm7, color1
        packssdw mm6, mm7
        packssdw mm1, mm6 // d1
        movq mm6, mm4
        movq mm7, mm5
        psadbw mm6, color2
        psadbw mm7, color2
        packssdw mm6, mm7
        packssdw mm2, mm6 // d2
        psadbw mm4, color3
        psadbw mm5, color3
        packssdw mm4, mm5
        packssdw mm3, mm4 // d3
        movq mm7, result
        pslld mm7, 8
        movq mm4, mm0
        movq mm5, mm1
        pcmpgtw mm0, mm3 // b0
        pcmpgtw mm1, mm2 // b1
        pcmpgtw mm4, mm2 // b2
        pcmpgtw mm5, mm3 // b3
        pcmpgtw mm2, mm3 // b4
        pand mm4, mm1 // x0
        pand mm5, mm0 // x1
        pand mm2, mm0 // x2
        por mm4, mm5
        pand mm2, SIMD_MMX_word_1
        pand mm4, SIMD_MMX_word_2
        por mm2, mm4
        pshufw mm5, mm2, R_SHUFFLE_D( 2, 3, 0, 1 )
        punpcklwd mm2, SIMD_MMX_word_0
        punpcklwd mm5, SIMD_MMX_word_0
        pslld mm5, 4
        por mm7, mm5
        por mm7, mm2
        movq result, mm7
        sub eax, 16
        jge loop1
        mov esi, globalOutData
        movq mm6, mm7
        psrlq mm6, 32-2
        por mm7, mm6
        movd dword ptr [esi], mm7
        emms
    }
    globalOutData += 4;
}
ALIGN16( static word SIMD_SSE2_word_0[8] ) = { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 };
ALIGN16( static word SIMD_SSE2_word_1[8] ) = { 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001 };
ALIGN16( static word SIMD_SSE2_word_2[8] ) = { 0x0002, 0x0002, 0x0002, 0x0002, 0x0002, 0x0002, 0x0002, 0x0002 };
ALIGN16( static word SIMD_SSE2_word_div_by_3[8] ) = { (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1, (1<<16)/3+1 };
ALIGN16( static byte SIMD_SSE2_byte_colorMask[16] ) = { C565_5_MASK, C565_6_MASK, C565_5_MASK, 0x00, 0x00, 0x00, 0x00, 0x00, C565_5_MASK, C565_6_MASK, C565_5_MASK, 0x00, 0x00, 0x00, 0x00, 0x00 };
void EmitColorIndices_SSE2( const byte *colorBlock, const byte *minColor, const byte *maxColor ) {
    ALIGN16( byte color0[16] );
    ALIGN16( byte color1[16] );
    ALIGN16( byte color2[16] );
    ALIGN16( byte color3[16] );
    ALIGN16( byte result[16] );
    __asm {
        mov esi, maxColor
        mov edi, minColor
        pxor xmm7, xmm7
        movdqa result, xmm7
        movd xmm0, [esi]
        pand xmm0, SIMD_SSE2_byte_colorMask
        punpcklbw xmm0, xmm7
        pshuflw xmm4, xmm0, R_SHUFFLE_D( 0, 3, 2, 3 )
        pshuflw xmm5, xmm0, R_SHUFFLE_D( 3, 1, 3, 3 )
        psrlw xmm4, 5
        psrlw xmm5, 6
        por xmm0, xmm4
        por xmm0, xmm5
        movd xmm1, [edi]
        pand xmm1, SIMD_SSE2_byte_colorMask
        punpcklbw xmm1, xmm7
        pshuflw xmm4, xmm1, R_SHUFFLE_D( 0, 3, 2, 3 )
        pshuflw xmm5, xmm1, R_SHUFFLE_D( 3, 1, 3, 3 )
        psrlw xmm4, 5
        psrlw xmm5, 6
        por xmm1, xmm4
        por xmm1, xmm5
        movdqa xmm2, xmm0
        packuswb xmm2, xmm7
        pshufd xmm2, xmm2, R_SHUFFLE_D( 0, 1, 0, 1 )
        movdqa color0, xmm2
        movdqa xmm6, xmm0
        paddw xmm6, xmm0
        paddw xmm6, xmm1
        pmulhw xmm6, SIMD_SSE2_word_div_by_3 // * ( ( 1 << 16 ) / 3 + 1 ) ) >> 16
        packuswb xmm6, xmm7
        pshufd xmm6, xmm6, R_SHUFFLE_D( 0, 1, 0, 1 )
        movdqa color2, xmm6
        movdqa xmm3, xmm1
        packuswb xmm3, xmm7
        pshufd xmm3, xmm3, R_SHUFFLE_D( 0, 1, 0, 1 )
        movdqa color1, xmm3
        paddw xmm1, xmm1
        paddw xmm0, xmm1
        pmulhw xmm0, SIMD_SSE2_word_div_by_3 // * ( ( 1 << 16 ) / 3 + 1 ) ) >> 16
        packuswb xmm0, xmm7
        pshufd xmm0, xmm0, R_SHUFFLE_D( 0, 1, 0, 1 )
        movdqa color3, xmm0
        mov eax, 32
        mov esi, colorBlock
    loop1: // iterates 2 times
        movq xmm3, qword ptr [esi+eax+0]
        pshufd xmm3, xmm3, R_SHUFFLE_D( 0, 2, 1, 3 )
        movq xmm5, qword ptr [esi+eax+8]
        pshufd xmm5, xmm5, R_SHUFFLE_D( 0, 2, 1, 3 )
        movdqa xmm0, xmm3
        movdqa xmm6, xmm5
        psadbw xmm0, color0
        psadbw xmm6, color0
        packssdw xmm0, xmm6
        movdqa xmm1, xmm3
        movdqa xmm6, xmm5
        psadbw xmm1, color1
        psadbw xmm6, color1
        packssdw xmm1, xmm6
        movdqa xmm2, xmm3
        movdqa xmm6, xmm5
        psadbw xmm2, color2
        psadbw xmm6, color2
        packssdw xmm2, xmm6
        psadbw xmm3, color3
        psadbw xmm5, color3
        packssdw xmm3, xmm5
        movq xmm4, qword ptr [esi+eax+16]
        pshufd xmm4, xmm4, R_SHUFFLE_D( 0, 2, 1, 3 )
        movq xmm5, qword ptr [esi+eax+24]
        pshufd xmm5, xmm5, R_SHUFFLE_D( 0, 2, 1, 3 )
        movdqa xmm6, xmm4
        movdqa xmm7, xmm5
        psadbw xmm6, color0
        psadbw xmm7, color0
        packssdw xmm6, xmm7
        packssdw xmm0, xmm6 // d0
        movdqa xmm6, xmm4
        movdqa xmm7, xmm5
        psadbw xmm6, color1
        psadbw xmm7, color1
        packssdw xmm6, xmm7
        packssdw xmm1, xmm6 // d1
        movdqa xmm6, xmm4
        movdqa xmm7, xmm5
        psadbw xmm6, color2
        psadbw xmm7, color2
        packssdw xmm6, xmm7
        packssdw xmm2, xmm6 // d2
        psadbw xmm4, color3
        psadbw xmm5, color3
        packssdw xmm4, xmm5
        packssdw xmm3, xmm4 // d3
        movdqa xmm7, result
        pslld xmm7, 16
        movdqa xmm4, xmm0
        movdqa xmm5, xmm1
        pcmpgtw xmm0, xmm3 // b0
        pcmpgtw xmm1, xmm2 // b1
        pcmpgtw xmm4, xmm2 // b2
        pcmpgtw xmm5, xmm3 // b3
        pcmpgtw xmm2, xmm3 // b4
        pand xmm4, xmm1 // x0
        pand xmm5, xmm0 // x1
        pand xmm2, xmm0 // x2
        por xmm4, xmm5
        pand xmm2, SIMD_SSE2_word_1
        pand xmm4, SIMD_SSE2_word_2
        por xmm2, xmm4
        pshufd xmm5, xmm2, R_SHUFFLE_D( 2, 3, 0, 1 )
        punpcklwd xmm2, SIMD_SSE2_word_0
        punpcklwd xmm5, SIMD_SSE2_word_0
        pslld xmm5, 8
        por xmm7, xmm5
        por xmm7, xmm2
        movdqa result, xmm7
        sub eax, 32
        jge loop1
        mov esi, globalOutData
        pshufd xmm4, xmm7, R_SHUFFLE_D( 1, 2, 3, 0 )
        pshufd xmm5, xmm7, R_SHUFFLE_D( 2, 3, 0, 1 )
        pshufd xmm6, xmm7, R_SHUFFLE_D( 3, 0, 1, 2 )
        pslld xmm4, 2
        pslld xmm5, 4
        pslld xmm6, 6
        por xmm7, xmm4
        por xmm7, xmm5
        por xmm7, xmm6
        movd dword ptr [esi], xmm7
    }
    globalOutData += 4;
}

void CompressImageDXT1_SSE2( const byte *inBuf, byte *outBuf, int width, int height ) {
    ALIGN16( byte block[64] );
    ALIGN16( byte minColor[4] );
    ALIGN16( byte maxColor[4] );
    globalOutData = outBuf;
    for ( int j = 0; j < height; j += 4, inBuf += width * 4*4 ) {
        for ( int i = 0; i < width; i += 4 ) {
            //int foo = width / 0;
            ExtractBlock_MMX( inBuf + i * 4, width, block );
            GetMinMaxColors_SSE2( block, minColor, maxColor );
            EmitWord( ColorTo565( maxColor ) );
            EmitWord( ColorTo565( minColor ) );
            EmitColorIndices_SSE2( block, minColor, maxColor );
        }
    }
}

void CompressImageDXT1_MMX( const byte *inBuf, byte *outBuf, int width, int height ) {
    ALIGN16( byte block[64] );
    ALIGN16( byte minColor[4] );
    ALIGN16( byte maxColor[4] );
    globalOutData = outBuf;
    for ( int j = 0; j < height; j += 4, inBuf += width * 4*4 ) {
        for ( int i = 0; i < width; i += 4 ) {
            ExtractBlock_MMX( inBuf + i * 4, width, block );
            GetMinMaxColors_MMX( block, minColor, maxColor );
            EmitWord( ColorTo565( maxColor ) );
            EmitWord( ColorTo565( minColor ) );
            EmitColorIndices_MMX( block, minColor, maxColor );
        }
    }
}

FASTDXT_API void CompressImageDXT1( const byte *inBuf, byte *outBuf, int width, int height ) {
    if ( HasSSE2 ) {
        CompressImageDXT1_SSE2(inBuf, outBuf, width, height);
    }
    else if ( HasMMX ) {
        CompressImageDXT1_MMX(inBuf, outBuf, width, height);
    }
}