using System;
using System.Globalization;
using System.IO;
using System.Text;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Scripting {
    /// <summary>
    /// 	Class contining helper methods for parsing text files.
    /// </summary>
    public class ParseHelper {
        #region Methods
		
        /// <summary>
        ///    Helper method for taking a string array and returning a single concatenated
        ///    string composed of the range of specified elements.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static string Combine(string[] items, int start, int end) {
            StringBuilder sb = new StringBuilder();

            for(int i = start; i < end; i++) {
                sb.AppendFormat("{0} ", items[i]);
            }

            return sb.ToString(0, sb.Length - 1);
        }

        /// <summary>
        ///		Helper method to log a formatted error when encountering problems with parsing
        ///		an attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="context"></param>
        /// <param name="expectedParams"></param>
        public static void LogParserError(string attribute, string context, string reason) {
            string error = string.Format("Bad {0} attribute in block '{1}'. Reason: {2}", attribute, context, reason);

            LogManager.Instance.Write(error);
        }

        /// <summary>
        ///		Helper method to nip/tuck the string before parsing it.  This includes trimming spaces from the beginning
        ///		and end of the string, as well as removing excess spaces in between values.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadLine(TextReader reader) {
            string line = reader.ReadLine();

            if(line != null) {
                line = line.Replace("\t", " ");
                line = line.Trim();

                // ignore blank lines, lines without spaces, or comments
                if(line.Length == 0 || line.IndexOf(' ') == -1 || line.StartsWith("//")) {
                    return line;
                }

                StringBuilder sb = new StringBuilder();

                string[] values = line.Split(' ');

                // reduce big space gaps between values down to a single space
                for(int i = 0; i < values.Length; i++) {
                    string val = values[i];

                    if(val.Length != 0) {
                        sb.Append(val + " ");
                    }
                }
				
                line = sb.ToString();
                line = line.TrimEnd();
            } // if
			
            return line;
        }

        /// <summary>
        ///		Helper method to remove the first item from a string array and return a new array 1 element smaller
        ///		starting at the second element of the original array.  This helpe to seperate the params from the command
        ///		in the various script files.
        /// </summary>
        /// <param name="splitLine"></param>
        /// <returns></returns>
        public static string[] GetParams(string[] all) {
            // create a seperate parm list that has the command removed
            string[] parms = new string[all.Length - 1];
            Array.Copy(all, 1, parms, 0, parms.Length);

            return parms;
        }

        /// <summary>
        ///    Advances in the stream until it hits the next {.
        /// </summary>
        public static void SkipToNextOpenBrace(TextReader reader) {
            string line = "";
            while(line != null && line != "{") {
                line = ReadLine(reader);
            }
        }

        /// <summary>
        ///    Advances in the stream until it hits the next }.
        /// </summary>
        /// <param name="reader"></param>
        public static void SkipToNextCloseBrace(TextReader reader) {
            string line = "";
            while(line != null && line != "}") {
                line = ReadLine(reader);
            }
        }

        #endregion
    }
}
