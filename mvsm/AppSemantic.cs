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

namespace Axiom.SceneManagers.Multiverse
{
    public class AppSemantic : IBoundarySemantic
    {
        public delegate void XMLWriter(System.Xml.XmlTextWriter w);

        private string type;
        private XMLWriter appXMLWriter;

        public AppSemantic(string type, XMLWriter appXMLWriter)
        {
            this.type = type;
            this.appXMLWriter = appXMLWriter;
        }

        #region IBoundarySemantic Members

        public string Type
        {
            get
            {
                return type;
            }
        }

        public void ToXML(System.Xml.XmlTextWriter w)
        {
            w.WriteStartElement("boundarySemantic");
            w.WriteAttributeString("type", type);

            // call app to insert XML here
            appXMLWriter(w);

            w.WriteEndElement();
        }

        public void PerFrameProcessing(float time, Axiom.Core.Camera camera)
        {
        }

        public void PageShift()
        {
        }

        public void BoundaryChange()
        {
        }

        public void AddToBoundary(Boundary boundary)
        {
        }

        public void RemoveFromBoundary()
        {
        }

        public void FogNotify()
        {
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
