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
using System.Text.RegularExpressions;

namespace Microsoft.MultiverseInterfaceStudioShell.BuildUtilities
{
    internal class TocFile
    {
        private List<ValueLine> attributes;
        private List<string> files;

        public TocFile()
        {
            this.attributes = new List<ValueLine>();
            this.files = new List<string>();
        }

        public void Read(string path)
        {
            using (TextReader reader = new StreamReader(path, Encoding.UTF8, true))
            {
                this.Read(reader);
            }
        }

        public void Write(string path)
        {
            using (TextWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                this.Write(writer);
            }
        }

        public void Write(TextWriter writer)
        {
            this.attributes.ForEach(delegate(ValueLine vl)
            {
                vl.Write(writer);
            });

            this.files.ForEach(delegate(string s)
            {
                writer.WriteLine(s);
            });
        }

        public void Read(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("##", StringComparison.OrdinalIgnoreCase))
                {
                    Match m = Regex.Match(line, @"^##\s+(?'NAME'[^:]+)\s*:\s*(?'VALUE'.*)$", RegexOptions.Compiled);
                    if (m.Success)
                    {
                        ValueLine vl = new ValueLine(m.Groups["NAME"].Value, m.Groups["VALUE"].Value);
                        this.attributes.Add(vl);

                        continue;
                    }
                }

                this.files.Add(line);
            }
        }

        public List<string> Files
        {
            get { return this.files; }
        }

        public string this[string name]
        {
            get
            {
                ValueLine vl = this.attributes.Find(delegate(ValueLine l) { return l.Name == name; });
                if (vl == null)
                    return null;

                return vl.Value;
            }
            set
            {
                ValueLine vl = this.attributes.Find(delegate(ValueLine l) { return l.Name == name; });
                if (vl == null)
                {
                    vl = new ValueLine(name, value);

                    this.attributes.Add(vl);
                }

                vl.Value = value;
            }
        }

        abstract class Line
        {
            public abstract void Write(TextWriter writer);
        }

        class SimpleLine : Line
        {
            private string value;

            public SimpleLine(string value)
            {
                this.value = value;
            }

            public override void Write(TextWriter writer)
            {
                writer.Write(this.value);
            }
        }

        class ValueLine : Line
        {
            private string name;
            private string value;

            public ValueLine(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return this.name; }
            }

            public string Value
            {
                get { return this.value; }
                set { this.value = value; }
            }

            public override void Write(TextWriter writer)
            {
                writer.Write("## ");
                writer.Write(this.name);
                writer.Write(": ");
                writer.WriteLine(this.value);
            }
        }
    }
}
