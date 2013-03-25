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

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Multiverse.Gui {
    public class TextUtil {
        /// <summary>
        ///   Get up to the next word.  This will include any leading whitespace
        ///   that precede the word.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string GetNextWord(string text, int offset) {
            // start will be the index of the first character that is included
            int start = -1; 
            for (int i = offset; i < text.Length; ++i) {
                if (!Char.IsWhiteSpace(text, i)) {
                    // we found a non-whitespace character
                    start = i;
                    break;
                }
            }
            if (start == -1) // no non-whitespace characters
                start = offset;
            // Now scan from start to see how large of a chunk we can get
            // until we run into another whitespace
            // end will be the index of the first character that is not included
            int end = -1;
            for (int i = start; i < text.Length; ++i) {
                if (Char.IsWhiteSpace(text, i)) {
                    end = i;
                    break;
                }
            }
            if (end == -1) // no whitespace characters after start
                end = text.Length;
            return text.Substring(offset, end - offset);
        }

        public static bool HasNextWord(string text, int offset) {
            if (offset >= text.Length)
                return false;
            for (int i = offset; i < text.Length; ++i)
                if (!Char.IsWhiteSpace(text, i))
                    return true;
            return false;
        }

        /// <summary>
        ///   Get the start of the word who owns the character at index.
        ///   In this system, words own their trailing whitespace.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetWordStartIndex(string text, int index) {
            if (index >= text.Length) {
                if (text.Length == 0)
                    return 0;
                // pretend we are talking about the last character in the text
                index = text.Length - 1;
            }
            // if index points to a whitespace character, grab the word before 
            // that whitespace.
            while (index > 0 && Char.IsWhiteSpace(text, index))
                --index;
            if (index == 0)
                return 0;
            // Ok, index points to the last non-whitespace character of
            // our word
            while (index > 0 && !Char.IsWhiteSpace(text, index))
                --index;
            if (index == 0)
                return 0;
            return index + 1;
        }

        /// <summary>
        ///   Get the index of the last non-whitespace character of 
        ///   the word that owns the character at index.  This character
        ///   may precede the index if the character at index is a 
        ///   whitespace character.
        ///   This can return -1 if the first character is whitespace
        ///   or if text length is 0.
        ///   In this system, words own their trailing whitespace.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetWordEndIndex(string text, int index) {
            if (index >= text.Length) {
                if (text.Length == 0)
                    return -1;
                // pretend we are talking about the last character in the text
                index = text.Length - 1;
            }
            // if index points to a whitespace character, grab the word before 
            // that whitespace.
            while (index > -1 && Char.IsWhiteSpace(text, index))
                --index;
            // Ok, index points to the last non-whitespace character of
            // our word (this may be -1)
            return index;
        }

        /// <summary>
        ///   Get the start of the word that follows the word which owns the 
        ///   character at index.
        ///   In this system, words own their trailing whitespace.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetNextWordStartIndex(string text, int index) {
            // advance past any non-whitespace what we might be in
            while (index < text.Length && !Char.IsWhiteSpace(text, index))
                ++index;
            // advance past any whitespace that we might be in
            while (index < text.Length && Char.IsWhiteSpace(text, index))
                ++index;
            // this also handles the case where index started out past the
            // end of our string
            if (index >= text.Length)
                // return the character after the last character in the text
                return text.Length;
            return index;
        }

        /// <summary>
        ///   Grab the bounds of the next word.  The initial index may be the
        ///   first character of the line (in which case it may be whitespace),
        ///   or it may be the start of a word.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="wordEndIndex">the index of the character after the last character of the word (or 0)</param>
        /// <param name="chunkEndIndex">the index of the character after the last character owned by the word (includes trailing whitespace)</param>
        /// <returns></returns>
        public static void GetWordBounds(string text, int start, int end,
                                         out int wordEndIndex,
                                         out int chunkEndIndex) {
            int index = start;
            // advance past any whitespace that we might be in
            while (index < end && Char.IsWhiteSpace(text, index))
                ++index;
            // advance past any non-whitespace what we might be in
            while (index < end && !Char.IsWhiteSpace(text, index))
                ++index;
            wordEndIndex = index;
            // advance past any whitespace that we might be in
            while (index < end && Char.IsWhiteSpace(text, index))
                ++index;
            chunkEndIndex = index;
        }
    }

    /// <summary>
    ///   Width computations are complex.
    ///   When text is aligned, the leading whitespace is included, but trailing whitespace
    ///   will be ignored.
    /// </summary>
    public class TextWrapHelper {
        /// <summary>
        ///   Given a text range for a line with possible trailing whitespace, 
        ///   return a text range where the trailing whitespace has been trimmed.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static TextRange Trim(TextRange range, string text) {
            int end = range.end;
            while (end > range.start && Char.IsWhiteSpace(text, end - 1))
                end--;
            range.end = end;
            return range;
        }

        /// <summary>
        ///   Break the text up based on hard line breaks.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<TextRange> GetLines(string text) {
            List<TextRange> lines = new List<TextRange>();

            int cur = 0;
            while (cur < text.Length) {
                TextRange range = new TextRange();
                range.start = cur;
                int eol = text.IndexOf('\n', range.start);
                if (eol == -1) {
                    range.end = text.Length;
                    cur = text.Length;
                } else {
                    range.end = eol;
                    cur = eol + 1;
                }
                lines.Add(range);
            }
            return lines;
        }

        public static List<TextRange> GetWrappedLines(SimpleFont font, string text, float width, bool nonSpaceWrap) {
            List<TextRange> hardWrappedLines = GetLines(text);
            return GetWrappedLines(font, hardWrappedLines, text, width, nonSpaceWrap);
        }

        public static List<TextRange> GetWrappedLines(SimpleFont font, List<TextRange> hardWrappedLines, string text, float width, bool nonSpaceWrap) {
            List<TextRange> lines = new List<TextRange>();
            foreach (TextRange range in hardWrappedLines) {
                // Does the whole line fit?
                if (font.GetTextExtent(text, range.start, range.Length) < width) {
                    lines.Add(range);
                } else {
                    int start = range.start;
                    // loop for adding multiple lines for the one hard-break line
                    while (start < range.end) {
                        float curWidth = 0;
                        TextRange curLine = new TextRange();
                        curLine.start = start;
                        // loop for adding words to the line
                        while (start < range.end) {
                            int wordEndIndex = 0;
                            int chunkEndIndex = 0;
                            // this method should get words.
                            // bounds.start will be the start of the word
                            // bounds.end will be after the end of the word with whitespace trimmed.
                            // chunkEndIndex is after the end of the word including trailing whitespace.
                            TextUtil.GetWordBounds(text, start, range.end, out wordEndIndex, out chunkEndIndex);
                            float wordWidth = font.GetTextExtent(text, start, wordEndIndex - start);
                            if (curWidth + wordWidth < width) {
                                // include the word
                                curLine.end = wordEndIndex;
                                curWidth += wordWidth;
                                // include the trailing space
                                curWidth += font.GetTextExtent(text, wordEndIndex, chunkEndIndex - wordEndIndex);
                                start = chunkEndIndex;
                            } else if (nonSpaceWrap && start == curLine.start) {
                                // FIXME - the one word didn't fit on this line -- deal with non-space wrap
                                Debug.Assert(false, "Still need to handle lines that don't break");
                            } else if (start == curLine.start) {
                                // We don't do non-space wrap, but the first word doesn't fit..
                                // We will have to pretend it does, so that we can make progress.
                                // include the word
                                curLine.end = wordEndIndex;
                                curWidth += wordWidth;
                                // include the trailing space
                                curWidth += font.GetTextExtent(text, wordEndIndex, chunkEndIndex - wordEndIndex);
                                start = chunkEndIndex;
                            } else {
                                // the word doesn't fit on this line, so I'm through with this line
                                curLine.end = start;
                                break;
                            }
                        }
                        lines.Add(curLine);
                    }
                }
            }
            //Trace.TraceInformation("GetWrappedLines():");
            //foreach (TextRange range in lines)
            //    Trace.TraceInformation("  '{0}'", text.Substring(range.start, range.end - range.start));
            return lines;
        }
    }
}
