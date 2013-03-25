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

using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Threading;

namespace Multiverse.Utility {

    /// <summary>
    ///    This is the base class for any client facility, including
    ///    facilities defined in Python, that need to persist their
    ///    settings.  In order create a derived class, you must
    ///    implement the three abstract methods
    /// </summary>
    public abstract class ConfigCategory {

        /// <summary>
        ///    GetName() returns the name that should be used in the
        ///    XmlElement containing setting for this category.  This
        ///    is a method an not a property because defining
        ///    properties in Python is a pain. 
        /// </summary>
        public abstract string GetName();

        /// <summary>
        ///    ReadCategoryData() encaches the data in the element.
        ///    In the usual case, each node in the element is itself
        ///    an XmlElement containing a string; you can get that
        ///    string by calling helper method 
        ///    GetElementText(element, nameOfSubElement), and if the
        ///    element value is, e.g., an int rather than a string,
        ///    you can call int.Parse() on it.
        /// </summary>
        public abstract void ReadCategoryData(XmlElement element);

        /// <summary>
        ///    WriteCategoryData() writes the encached category data
        ///    into the element argument.  In the usual case that each
        ///    node in the element has a string value, you can call
        ///    the helper method 
        ///    AddElement(element, subElementName, subElementText) 
        /// </summary>
        public abstract void WriteCategoryData(XmlElement element);

        /// <summary>
        ///    Helper function to add a subelement to an XmlElement.
        /// </summary>
        public void AddElement(XmlElement element, string subElementName, string subElementText) {
            XmlElement subElement = ConfigManager.Instance.xmlDom.CreateElement("", subElementName, "");
            SetElementText(subElement, subElementText);
            element.AppendChild(subElement);
        }

        /// <summary>
        ///    Helper function to get the string value of a subelement
        ///    of an XmlElement.
        /// </summary>
        public string GetElementText(XmlElement element, string subElementName) {
            XmlNodeList list = element.GetElementsByTagName(subElementName);
            if (list.Count == 1)
                return ElementText((XmlElement)list[0]);
            else
                return "";
        }

        /// <summary>
        ///    Helper function to get the string value of an XmlElement.
        /// </summary>
        public string ElementText(XmlElement element) {
            return element.InnerText;
        }
        
        /// <summary>
        ///    Helper function to set the string value of an XmlElement.
        /// </summary>
        public void SetElementText(XmlElement element, string text) {
            element.InnerText = text;
        }
    }
    
    /// <summary>
    ///    A base class for StringCategory, IntCategory and
    ///    FloatCategory.
    /// </summary>
    public abstract class ScalarCategory : ConfigCategory {
        string name;
        
        public ScalarCategory(string name) {
            this.name = name;
        }
        
        public override string GetName() {
            return name;
        }
    }
    
    /// <summary>
    ///    StringCategory is a built-in class for categories that
    ///    want to persist a single string.
    /// </summary>
    public class StringCategory : ScalarCategory {
        private string val = "";

        public StringCategory(string name) : base(name) {
        }
        
        public override void ReadCategoryData(XmlElement element) {
            val = ElementText(element);
        }

        public override void WriteCategoryData(XmlElement element) {
            SetElementText(element, val);
        }

        public string Value {
            get {
                return val;
            }
            set {
                val = value;
            }
        }
    }

    /// <summary>
    ///    IntCategory is a built-in class for categories that
    ///    want to persist a single int.
    /// </summary>
    public abstract class IntCategory : ScalarCategory {
        public int val = 0;

        public IntCategory(string name) : base(name) {
        }
        
        public override void ReadCategoryData(XmlElement element) {
            val = int.Parse(ElementText(element));
        }

        public override void WriteCategoryData(XmlElement element) {
            SetElementText(element, val.ToString());
        }

        public int Value {
            get {
                return val;
            }
            set {
                val = value;
            }
        }
    }

    /// <summary>
    ///    FloatCategory is a built-in class for categories that
    ///    want to persist a single float.
    /// </summary>
    public abstract class FloatCategory : ScalarCategory {
        public float val = 0;

        public FloatCategory(string name) : base(name) {
        }
        
        public override void ReadCategoryData(XmlElement element) {
            val = float.Parse(ElementText(element));
        }

        public override void WriteCategoryData(XmlElement element) {
            SetElementText(element, val.ToString());
        }

        public float Value {
            get {
                return val;
            }
            set {
                val = value;
            }
        }
    }
        
    /// <summary>
    ///    ConfigManager manages loading and saving the
    ///    LocalConfig.xml file, which contains the Xml representation
    ///    of a collection of ConfigCategories.  There is one such
    ///    file independent of the number of categories or number of
    ///    worlds.
    /// </summary>
    public class ConfigManager {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ConfigManager));

        private string pathname = "";
        private static ConfigManager instance;
        internal XmlDocument xmlDom = null;
        internal XmlElement xmlRoot = null;

        /// <summary>
        ///    The client calls Initialize() at startup to ensure
        ///    that the LocalConfig.xml file is read.  Subsequently,
        ///    ConfigCategories can call GetCategoryData() to encache
        ///    the settings in the LocalConfig.xml document, and
        ///    WriteCategoryData() to add their data to the
        ///    LocalConfig.xml document, and write the document to the
        ///    file system.
        /// </summary>
        public static void Initialize(string pathname) {
            if (instance != null)
                log.ErrorFormat("ConfigManager.Initialize: ConfigManager is already initialized!  Stack trace:\n{0}", new StackTrace(true));
            else {
                instance = new ConfigManager(pathname);
            }
        }
        
        /// <summary>
        ///    The private constructor is called only by Initialize().
        /// </summary>
        private ConfigManager(string pathname) {
            this.pathname = pathname;
            xmlDom = new XmlDocument();
            if (File.Exists(pathname)) {
                TextReader streamReader = new StreamReader(pathname);
                xmlDom.Load(streamReader);
                xmlRoot = xmlDom.DocumentElement;
            }
            else {
                xmlDom.AppendChild(xmlDom.CreateElement("", "Settings", ""));
                xmlRoot = xmlDom.DocumentElement;
                log.WarnFormat("ConfigManager constructor: Could not find file '{0}'", pathname);
            }
        }
    
        /// <summary>
        ///    ConfigCategories call GetCategoryData() to encache
        ///    their configuration settings.  This overloading is used
        ///    when there is only one top-level XmlElement whose name
        ///    is the name of the category.
        /// </summary>
        public XmlElement GetCategoryData(ConfigCategory category) {
            try {
                Monitor.Enter(this);
                XmlNodeList list = xmlRoot.GetElementsByTagName(category.GetName());
                if (list.Count == 0)
                    return null;
                else if (list.Count > 1)
                    log.ErrorFormat("ConfigManager.Initialize: There are {0} nodes whose element name is '{0}'", category.GetName());
                XmlElement element = (XmlElement)list.Item(0);
                return element;
            }
            finally {
                Monitor.Exit(this);
            }
        }
        
        /// <summary>
        ///    ConfigCategories call GetCategoryData() to encache
        ///    their configuration settings.  This overloading is used
        ///    when there is more than one top-level XmlElement whose
        ///    name is the name of the category.  In that case, the
        ///    right one to return is the one that has a subelement
        ///    whose name is subElementName and whose value is
        ///    subElementValue. 
        /// </summary>
        public XmlElement GetCategoryData(ConfigCategory category, string subElementName, string subElementValue) {
            try {
                Monitor.Enter(this);
                return GetElementWithSubElementOfNameAndValue(category.GetName(), subElementName, subElementValue);
            }
            finally {
                Monitor.Exit(this);
            }
        }
        
        /// <summary>
        ///    ConfigCategories call WriteCategoryData() to persist
        ///    their configuration settings.  This overloading is used
        ///    when there is only one top-level XmlElement whose name
        ///    is the name of the category.
        /// </summary>
        public void WriteCategoryData(ConfigCategory category) {
            try {
                Monitor.Enter(this);
                XmlElement node = xmlDom.CreateElement("", category.GetName(), "");
                category.WriteCategoryData(node);
                XmlNodeList list = xmlRoot.GetElementsByTagName(category.GetName());
                if (list.Count == 0)
                    xmlRoot.AppendChild(node);
                else {
                    if (list.Count > 1) {
                        for (int i=1; i<list.Count; i++)
                            xmlRoot.RemoveChild(list[i]);
                    }
                    xmlRoot.ReplaceChild(node, list[0]);
                }
                WriteConfigFile();
            }
            finally {
                Monitor.Exit(this);
            }
        }
        
        /// <summary>
        ///    ConfigCategories call WriteCategoryData() to persist
        ///    their configuration settings.  This overloading is used
        ///    when there is more than one top-level XmlElement whose
        ///    name is the name of the category.  In that case, the
        ///    one persisted is has a subelement whose name is 
        ///    subElementName and whose value is subElementValue. 
        /// </summary>
        public void WriteCategoryData(ConfigCategory category, string subElementName, string subElementValue) {
            try {
                Monitor.Enter(this);
                XmlElement node = xmlDom.CreateElement("", category.GetName(), "");
                category.WriteCategoryData(node);
                XmlElement oldNode = GetElementWithSubElementOfNameAndValue(category.GetName(), subElementName, subElementValue);
                if (oldNode != null)
                    xmlRoot.ReplaceChild(node, oldNode);
                else
                    xmlRoot.AppendChild(node);
                WriteConfigFile();
            }
            finally {
                Monitor.Exit(this);
            }
        }
        
        /// <summary>
        ///    WriteConfigFile() just writes the LocalConfig.xml file.
        ///    Since WriteConfigFile() is called by both overloadings
        ///    of WriteCategoryData(), there should never be a need to
        ///    call it explicitly.
        /// </summary>
        public void WriteConfigFile() {
            try {
                Monitor.Enter(this);
                TextWriter streamWriter = new StreamWriter(pathname);
                xmlDom.Save(streamWriter);
            }
            finally {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        ///    Get the ConfigManager instance.
        /// </summary>
        public static ConfigManager Instance {
            get {
                return instance;
            }
        }
        
        /// <summary>
        ///    Return the subelement whose name is subElementName and whose value is
        ///    subElementValue, or null if none such exists.
        /// </summary>
        private XmlElement GetElementWithSubElementOfNameAndValue(string categoryName, string subElementName, string subElementValue) {
                XmlNodeList list = xmlRoot.GetElementsByTagName(categoryName);
                foreach (XmlNode node in list) {
                    XmlElement element = (XmlElement)node;
                    XmlNodeList nodeElements = element.GetElementsByTagName(subElementName);
                    if (nodeElements.Count == 1) {
                        XmlElement subElement = (XmlElement)nodeElements[0];
                        if (subElement.InnerText == subElementValue)
                            return element;
                    }
                }
                return null;
        }

        /// <summary>
        ///    Get rid of all elements whose name is the name of the category.
        /// </summary>
        public void RemoveChildrenOfCategory(ConfigCategory category) {
            try {
                Monitor.Enter(this);
                XmlNodeList list = xmlRoot.GetElementsByTagName(category.GetName());
                foreach (XmlNode node in list)
                    xmlRoot.RemoveChild(node);
            }
            finally {
                Monitor.Exit(this);
            }
        }
        
    }
}
