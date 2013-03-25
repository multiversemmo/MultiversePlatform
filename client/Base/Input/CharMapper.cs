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

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

using Axiom.Input;

#endregion

namespace Multiverse.Input
{
    public class CharMapper
    {
        public static char GetChar(KeyCodes key, bool isShifted) {
			switch(key) {
				case KeyCodes.A:
					return isShifted ? 'A' : 'a';
				case KeyCodes.B:
					return isShifted ? 'B' : 'b';
				case KeyCodes.C:
					return isShifted ? 'C' : 'c';
				case KeyCodes.D:
					return isShifted ? 'D' : 'd';
				case KeyCodes.E:
					return isShifted ? 'E' : 'e';
				case KeyCodes.F:
					return isShifted ? 'F' : 'f';
				case KeyCodes.G:
					return isShifted ? 'G' : 'g';
				case KeyCodes.H:
					return isShifted ? 'H' : 'h';
				case KeyCodes.I:
					return isShifted ? 'I' : 'i';
				case KeyCodes.J:
					return isShifted ? 'J' : 'j';
				case KeyCodes.K:
					return isShifted ? 'K' : 'k';
				case KeyCodes.L:
					return isShifted ? 'L' : 'l';
				case KeyCodes.M:
					return isShifted ? 'M' : 'm';
				case KeyCodes.N:
					return isShifted ? 'N' : 'n';
				case KeyCodes.O:
					return isShifted ? 'O' : 'o';
				case KeyCodes.P:
					return isShifted ? 'P' : 'p';
				case KeyCodes.Q:
					return isShifted ? 'Q' : 'q';
				case KeyCodes.R:
					return isShifted ? 'R' : 'r';
				case KeyCodes.S:
					return isShifted ? 'S' : 's';
				case KeyCodes.T:
					return isShifted ? 'T' : 't';
				case KeyCodes.U:
					return isShifted ? 'U' : 'u';
				case KeyCodes.V:
					return isShifted ? 'V' : 'v';
				case KeyCodes.W:
					return isShifted ? 'W' : 'w';
				case KeyCodes.X:
					return isShifted ? 'X' : 'x';
				case KeyCodes.Y:
					return isShifted ? 'Y' : 'y';
                case KeyCodes.Z:
                    return isShifted ? 'Z' : 'z';

                case KeyCodes.D1:
                    return isShifted ? '!' : '1';
                case KeyCodes.D2:
                    return isShifted ? '@' : '2';
                case KeyCodes.D3:
                    return isShifted ? '#' : '3';
                case KeyCodes.D4:
                    return isShifted ? '$' : '4';
                case KeyCodes.D5:
                    return isShifted ? '%' : '5';
                case KeyCodes.D6:
                    return isShifted ? '^' : '6';
                case KeyCodes.D7:
                    return isShifted ? '&' : '7';
                case KeyCodes.D8:
                    return isShifted ? '*' : '8';
                case KeyCodes.D9:
                    return isShifted ? '(' : '9';
                case KeyCodes.D0:
                    return isShifted ? ')' : '0';

                case KeyCodes.Equals:
                    return isShifted ? '+' : '=';
                case KeyCodes.Minus:
                    return isShifted ? '_' : '-';

                case KeyCodes.Divide:
                    return '/';
                case KeyCodes.Multiply:
                    return '*';
                case KeyCodes.Subtract:
                    return '-';
                case KeyCodes.Add:
                    return '+';
                    
                case KeyCodes.Period:
                    return isShifted ? '>' : '.';
				case KeyCodes.Comma:
					return isShifted ? '<' : ',';
				case KeyCodes.Semicolon:
					return isShifted ? ':' : ';';

                case KeyCodes.Space:
					return ' ';
                case KeyCodes.Tilde:
                    return isShifted ? '~' : '`';
                case KeyCodes.Quotes:
                    return isShifted ? '"' : '\'';
                case KeyCodes.QuestionMark:
                    return isShifted ? '?' : '/';
                case KeyCodes.OpenBracket:
                    return isShifted ? '{' : '[';
                case KeyCodes.CloseBracket:
                    return isShifted ? '}' : ']';
                case KeyCodes.Backslash:
                    return isShifted ? '|' : '\\';
            }

            return '\0';
        }
    }
}
