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

package multiverse.server.util;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import org.xml.sax.SAXException;
import org.xml.sax.SAXParseException;
import java.io.*;
import org.w3c.dom.Document;
import org.w3c.dom.*;
import java.util.*;

public class XMLHelper {

    public static DocumentBuilder makeDocBuilder() {
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        DocumentBuilder builder = null;
        try {
            builder = factory.newDocumentBuilder();
            builder.setErrorHandler(new org.xml.sax.ErrorHandler() {
                // ignore fatal errors (an exception is guaranteed)
                public void fatalError(SAXParseException exception)
                        throws SAXException {
                }

                // treat validation errors as fatal
                public void error(SAXParseException e) throws SAXParseException {
                    throw e;
                }

                // dump warnings too
                public void warning(SAXParseException err)
                        throws SAXParseException {
                    System.out.println("** Warning" + ", line "
                            + err.getLineNumber() + ", uri "
                            + err.getSystemId());
                    System.out.println("   " + err.getMessage());
                }
            });
        }
//        catch (SAXException sxe) {
//            Exception x = sxe;
//            if (sxe.getException() != null) {
//                x = sxe.getException();
//            }
//            Log.exception("XMLHelper.makeDocBuilder caught SAXException", x);
//        } 
        catch (ParserConfigurationException pce) {
            Log.exception("XMLHelper.makeDocBuilder caught ParserConfigurationException", pce);
        }
        return builder;
    }

    // if the node has a single child and it has the value
    public static String getNodeValue(org.w3c.dom.Node node) {
        return node.getFirstChild().getNodeValue();
    }

    // return a list of all matching children node
    public static List<org.w3c.dom.Node> getMatchingChildren(
            org.w3c.dom.Node node, String name) {
        if (node == null) {
            return null;
        }

        LinkedList<org.w3c.dom.Node> returnList = new LinkedList<org.w3c.dom.Node>();

        org.w3c.dom.NodeList childList = node.getChildNodes();
        int len = childList.getLength();
        for (int i = 0; i < len; i++) {
            org.w3c.dom.Node curNode = childList.item(i);
            if (name.equals(curNode.getNodeName())) {
                returnList.add(curNode);
            }
        }
        return returnList;
    }

    // find the child with the matching name
    public static org.w3c.dom.Node getMatchingChild(org.w3c.dom.Node node,
            String name) {
        if (node == null) {
            return null;
        }

        org.w3c.dom.NodeList childList = node.getChildNodes();
        int len = childList.getLength();
        for (int i = 0; i < len; i++) {
            org.w3c.dom.Node curNode = childList.item(i);
            if (name.equals(curNode.getNodeName())) {
                return curNode;
            }
        }
        return null;
    }

    public static String getMatchingChildValue(org.w3c.dom.Node node,
            String name) {
        org.w3c.dom.Node childNode = getMatchingChild(node, name);
        return getNodeValue(childNode);
    }

    public static String getAttribute(org.w3c.dom.Node node, String attrName) {
        org.w3c.dom.NamedNodeMap attrMap = node.getAttributes();
        if (attrMap == null) {
            Log.debug("getAttribute: attr map is null");
            return null;
        }
        org.w3c.dom.Node attrNode = attrMap.getNamedItem(attrName);
        if (attrNode == null) {
            Log.debug("getAttribute: attr node is null");
            return null;
        }
        return getNodeValue(attrNode);
    }

    public static String toXML(org.w3c.dom.Node node) {
        String xml = "";
        String name = node.getNodeName();
        xml += "<" + name;

        // get the attributes
        NamedNodeMap attrMap = node.getAttributes();
        if (attrMap != null) {
            int len = attrMap.getLength();
            for (int i = 0; i < len; i++) {
                Node attrNode = attrMap.item(i);
                String attrName = attrNode.getNodeName();
                String attrVal = XMLHelper.getNodeValue(attrNode);
                xml += " " + attrName + "=\"" + attrVal + "\"";
            }
        }
        xml += ">";

        // get children
        NodeList children = node.getChildNodes();
        if (children != null) {
            int len = children.getLength();
            for (int i = 0; i < len; i++) {
                Node childNode = children.item(i);
                short nodeType = childNode.getNodeType();
                if (nodeType == Node.TEXT_NODE) {
                    // Log.debug("childTEXTNODE: " +
                    // ", name=" + childNode.getNodeName() +
                    // ", type=" + typeName[childNode.getNodeType()] +
                    // ", val=" + childNode.getNodeValue());
                    xml += childNode.getNodeValue();
                } else if (nodeType == Node.ELEMENT_NODE) {
                    // Log.debug("childELEMENT: " +
                    // ", name=" + childNode.getNodeName() +
                    // ", type=" + typeName[childNode.getNodeType()] +
                    // ", val=" + childNode.getNodeValue());
                    xml += toXML(childNode);
                } else {
                    if (Log.loggingDebug)
                        Log.debug("XMLHelper: unknown child node: " + ", name="
                                  + childNode.getNodeName() + ", type="
                                  + typeName[childNode.getNodeType()] + ", val="
                                  + childNode.getNodeValue());
                }
            }
        }
        xml += "</" + name + ">";
        return xml;
    }

    public static void printAllChildren(org.w3c.dom.Node node) {
        if (node == null) {
            return;
        }

        org.w3c.dom.NodeList childList = node.getChildNodes();
        int len = childList.getLength();
        for (int i = 0; i < len; i++) {
            org.w3c.dom.Node curNode = childList.item(i);
            if (Log.loggingDebug)
                Log.debug("XMLHelper.printAllChildren: childnode= "
                          + nodeToString(curNode));
        }
    }

    public static String nodeToString(org.w3c.dom.Node domNode) {
        if (domNode == null) {
            return "";
        }
        String s = typeName[domNode.getNodeType()];
        String nodeName = domNode.getNodeName();
        if (!nodeName.startsWith("#")) {
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
            if (x >= 0)
                t = t.substring(0, x);
            s += t;
        }
        return s;
    }

    public static Map<String, Serializable> nameValuePairsHelper(org.w3c.dom.Node node) {
	Map<String, Serializable> resultMap = new HashMap<String, Serializable>();

	List<Node> nameValueNodes = XMLHelper.getMatchingChildren(node, "NameValuePair");
	for (Node pairNode : nameValueNodes) {
	    String key = XMLHelper.getAttribute(pairNode, "Name");
	    String valueString = XMLHelper.getAttribute(pairNode, "Value");
            String type = XMLHelper.getAttribute(pairNode, "Type");
            Serializable value = null;

            if (type.equalsIgnoreCase("string") || type.equalsIgnoreCase("enum"))
                value = valueString;
            else if (type.equalsIgnoreCase("boolean"))
                value = Boolean.parseBoolean(valueString);
            else if (type.equalsIgnoreCase("int") || type.equalsIgnoreCase("uint"))
                value = Integer.parseInt(valueString);
            else if (type.equalsIgnoreCase("float"))
                value = Float.parseFloat(valueString);

	    resultMap.put(key, value);
	}

	return resultMap;
    }

    // An array of names for DOM node-types
    // (Array indexes = nodeType() values.)
    static final String[] typeName = { "none", "Element", "Attr", "Text",
            "CDATA", "EntityRef", "Entity", "ProcInstr", "Comment", "Document",
            "DocType", "DocFragment", "Notation", };

    public static void main(String[] args) {
        // read config
        if (args.length != 1) {
            System.err.println("specify config file");
            System.exit(1);
        }
        DocumentBuilder builder = XMLHelper.makeDocBuilder();
        Document doc;
        try {
            doc = builder.parse(new File(args[0]));
        } catch (Exception e) {
            Log.error(e.toString());
            return;
        }

        Node worldDescNode = XMLHelper
                .getMatchingChild(doc, "WorldDescription");

        Node terrainNode = XMLHelper.getMatchingChild(worldDescNode, "Terrain");
        if (Log.loggingDebug)
            Log.debug("toXML: " + XMLHelper.toXML(terrainNode));
    }
}
    
