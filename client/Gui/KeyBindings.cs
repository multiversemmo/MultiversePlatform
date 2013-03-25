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
using System.IO;
using System.Diagnostics;

using Axiom.Input;

namespace Multiverse.Gui {
    /// <summary>
    ///   This class name has become something of a misnomer, since it is now 
    ///   responsible for mouse bindings as well.
    /// </summary>
    public class KeyBindings {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(KeyBindings));

        // TODO: Clean this up - string<->string sucks
        Dictionary<string, string> keyBindings = new Dictionary<string, string>();

        public void Load(Stream file) {
            StreamReader reader = new StreamReader(file);
            while (true) {
                string str = reader.ReadLine();
                if (str == null)
                    break;
                string[] fields = str.Split();
                if (fields.Length != 2)
                    continue;
                keyBindings[fields[0]] = fields[1];
            }
        }

        public string GetBinding(KeyCodes k, ModifierKeys m) {
            // TODO: support actions for simple modifier keys?
            string keyStr = KeyBindings.GetKeyString(k);
            if (keyStr == null)
                return null;
            return GetBinding(keyStr, m);
        }

        public string GetBinding(MouseButtons buttons, ModifierKeys m) {
            string keyStr;
            switch (buttons) {
                case MouseButtons.Left:
                    keyStr = "BUTTON1";
                    break;
                case MouseButtons.Right:
                    keyStr = "BUTTON2";
                    break;
                case MouseButtons.Middle:
                    keyStr = "BUTTON3";
                    break;
                default:
                    keyStr = null;
                    break;
            }
            if (keyStr == null)
                return null;
            return GetBinding(keyStr, m);
        }

        protected string GetBinding(string keyStr, ModifierKeys m) {
            StringBuilder keyName = new StringBuilder();
            if ((m & ModifierKeys.Alt) > 0) {
                if (keyName.Length > 0)
                    keyName.Append("-");
                keyName.Append("ALT");
            }
            if ((m & ModifierKeys.Control) > 0) {
                if (keyName.Length > 0)
                    keyName.Append("-");
                keyName.Append("CTRL");
            }
            if ((m & ModifierKeys.Shift) > 0) {
                if (keyName.Length > 0)
                    keyName.Append("-");
                keyName.Append("SHIFT");
            }

            if (keyName.Length > 0)
                keyName.Append("-");
            keyName.Append(keyStr);

            string keyNameStr = keyName.ToString();
            if (keyBindings.ContainsKey(keyNameStr))
                return keyBindings[keyNameStr];
            else
                log.DebugFormat("No binding for {0}", keyNameStr);
            return null;
        }

        protected static string GetKeyString(KeyCodes k) {
            switch (k) {
                case KeyCodes.QuestionMark:
                    return "/";
                case KeyCodes.D1:
                    return "1";
                case KeyCodes.D2:
                    return "2";
                case KeyCodes.D3:
                    return "3";
                case KeyCodes.D4:
                    return "4";
                case KeyCodes.D5:
                    return "5";
                case KeyCodes.D6:
                    return "6";
                case KeyCodes.D7:
                    return "7";
                case KeyCodes.D8:
                    return "8";
                case KeyCodes.D9:
                    return "9";
                case KeyCodes.D0:
                    return "0";
                case KeyCodes.Subtract:
                    return "-";
                case KeyCodes.Plus:
                    return "=";
            }
            return k.ToString().ToUpper();
        }
    }
}
