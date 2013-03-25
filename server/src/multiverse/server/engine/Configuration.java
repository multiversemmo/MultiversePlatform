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

package multiverse.server.engine;

import javax.xml.parsers.DocumentBuilder; 
import javax.xml.parsers.DocumentBuilderFactory;   
import javax.xml.parsers.ParserConfigurationException;
import org.xml.sax.SAXException;  
import org.xml.sax.SAXParseException;
import java.io.File;
import java.io.IOException;
import org.w3c.dom.Document;
import java.util.*;
import multiverse.server.util.MVRuntimeException;

// checked for locking - this is not thread safe - but we are phasing it
// out, and currently its used only in one thread

public class Configuration {
    public Configuration(String fileName) {
	DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
	try {
	    DocumentBuilder builder = factory.newDocumentBuilder();
	    builder.setErrorHandler(
		new org.xml.sax.ErrorHandler() {
		    // ignore fatal errors (an exception is guaranteed)
		    public void fatalError(SAXParseException exception)
			throws SAXException {
		    }
		    
		    // treat validation errors as fatal
		    public void error(SAXParseException e)
			throws SAXParseException {
			throw e;
		    }
		    
		    // dump warnings too
		    public void warning(SAXParseException err)
			throws SAXParseException {
			System.out.println("** Warning"
					   + ", line " + err.getLineNumber()
					   + ", uri " + err.getSystemId());
			System.out.println("   " + err.getMessage());
		    }
		}
		);
	    document = builder.parse(new File(fileName));
	    
	} 
	catch(SAXException sxe) {
	    Exception x = sxe;
	    if (sxe.getException() != null) {
		x = sxe.getException();
	    }
	    x.printStackTrace();
	}
	catch(ParserConfigurationException pce) {
	    pce.printStackTrace();
	}
	catch(IOException ioe) {
	    ioe.printStackTrace();
	}
    }
    
    public org.w3c.dom.Node getRoot() {
	return document;
    }

    public static String getValueFromChild(String childName, org.w3c.dom.Node node) {
	if (node == null) {
	    throw new MVRuntimeException("node is null");
	}
	org.w3c.dom.Node childNode = 
	    Configuration.findChild(node, childName);
	if (childNode == null)
	    throw new MVRuntimeException("could not find child node with name: " +
				  childName);
	return Configuration.getNodeValue(childNode);
    }
					 
    // if the node has a single child and it has the value
    public static String getNodeValue(org.w3c.dom.Node node) {
	return node.getFirstChild().getNodeValue();
    }

    // return a list of all matching children node
    public static List getMatchingChildren(org.w3c.dom.Node node,
					   String name) {
	if (node == null) {
	    return null;
	}
	
	LinkedList<org.w3c.dom.Node> returnList = new LinkedList<org.w3c.dom.Node>();
	org.w3c.dom.NodeList childList = node.getChildNodes();
	int len = childList.getLength();
	for (int i=0; i<len; i++) {
	    org.w3c.dom.Node curNode = childList.item(i);
	    if (name.equals(curNode.getNodeName())) {
		returnList.add(curNode);
	    }
	}
	return returnList;
    }

    // find the child with the matching name
    public static org.w3c.dom.Node findChild(org.w3c.dom.Node node, 
					     String name) {
	if (node == null) {
	    return null;
	}
	
	org.w3c.dom.NodeList childList = node.getChildNodes();
	int len = childList.getLength();
	for (int i=0; i<len; i++) {
	    org.w3c.dom.Node curNode = childList.item(i);
	    if (name.equals(curNode.getNodeName())) {
		return curNode;
	    }
	}
	return null;
    }
    
    public static void printAllChildren(org.w3c.dom.Node node) {
	if (node == null) {
	    return;
	}

	org.w3c.dom.NodeList childList = node.getChildNodes();
	int len = childList.getLength();
	for (int i=0; i<len; i++) {
	    org.w3c.dom.Node curNode = childList.item(i);
	    System.out.println("node: " + toStringNode(curNode));
	}
    }

    public static String toStringNode(org.w3c.dom.Node domNode) {
	if (domNode == null) {
	    return "";
	}
	String s = typeName[domNode.getNodeType()];
	String nodeName = domNode.getNodeName();
	if (! nodeName.startsWith("#")) {
	    s += ": " + nodeName;
	}
	if (domNode.getNodeValue() != null) {
	    if (s.startsWith("ProcInstr")) 
		s += ", "; 
	    else 
		s += ": ";
	    // Trim the value to get rid of NL's at the front
	    String t = domNode.getNodeValue().trim();
	    int x = t.indexOf("\n");
	    if (x >= 0) t = t.substring(0, x);
	    s += t;
	}
	return s;
    }
			  

    // An array of names for DOM node-types
    // (Array indexes = nodeType() values.)
    static final String[] typeName = {
        "none",
        "Element",
        "Attr",
        "Text",
        "CDATA",
        "EntityRef",
        "Entity",
        "ProcInstr",
        "Comment",
        "Document",
        "DocType",
        "DocFragment",
        "Notation",
    };

    private Document document;

    public static void main(String[] args) {
	// read config
	if (args.length != 1) {
	    System.err.println("specify config file");
	    System.exit(1);
	}
	Configuration config = new Configuration(args[0]);
	org.w3c.dom.Node portNode = 
	    findChild(config.getRoot().getFirstChild(), "port");
	if (portNode == null) {
	    System.out.println("could not find port node");
	    System.exit(1);
	}
	System.out.println("found port node");

	System.out.println("printing all port node children");
	printAllChildren(portNode);
    }
}
