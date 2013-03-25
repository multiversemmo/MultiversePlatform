# Copyright (c) 2005, 2006 Seo Sanghyeon

# A chapter from Dan Wahlin's "XML for ASP.NET Developers" is useful
# for understanding this code. Posted to informit.com, 2002-02-22.
# http://www.informit.com/articles/article.asp?p=25485 

# 2005-11-16 sanxiyn Created
# 2006-08-18 sanxiyn Merged changes from Mark Rees
#  * Adapted to the new way to load .NET libraries
#  * Handle empty elements
# 2006-08-29 sanxiyn Added support for XML namespaces
#                    Simplified code a lot
# 2006-10-21 sanxiyn Minimal support for xml.sax
# 2006-10-24 sanxiyn Implemented ordered_attributes, namespace_prefixes
# 2006-10-27 sanxiyn Added expat.error
# 2006-10-29 sanxiyn Implemented Start/End NamespaceDeclHandler
# 2006-11-20 sanxiyn Merged changes from Fredrik Lundh
#  * Handle multiple calls to Parse()
# 2007-02-19 sylvain Added missing EndDoctypeDeclHandler declaration
# 2007-10-11 shozoa  Set ProhibitDtd to false

import clr
clr.AddReference("System.Xml")

from System import Enum
from System.IO import StringReader
from System.Xml import XmlReader, XmlReaderSettings, XmlNodeType

# xml.sax passes an undocumented keyword argument "intern" to ParserCreate.
# Let's ignore it.

def ParserCreate(encoding=None, namespace_separator=None, **kw):
    return xmlparser(namespace_separator)

# Used by xml.sax
XML_PARAM_ENTITY_PARSING_UNLESS_STANDALONE = 1
# Used by Kid
XML_PARAM_ENTITY_PARSING_ALWAYS = 2

class error(Exception):
    pass

class xmlparser(object):

    __slots__ = [
        # Internal
        "_data",
        "_separator",
        "_reader",
        "_ns_stack",

        # Attributes
        # Implemented
        "ordered_attributes",
        "namespace_prefixes",
        # Stub for xml.dom
        "buffer_text",
        "specified_attributes",

        # Handlers
        # Implemented
        "StartElementHandler",
        "EndElementHandler",
        "CharacterDataHandler",
        "StartNamespaceDeclHandler",
        "EndNamespaceDeclHandler",
        # Stub for ElementTree
        "DefaultHandlerExpand",
        # Stub for xml.sax
        "ProcessingInstructionHandler",
        "UnparsedEntityDeclHandler",
        "NotationDeclHandler",
        "ExternalEntityRefHandler",
        "EndDoctypeDeclHandler",
        # Stub for xml.dom
        "StartDoctypeDeclHandler",
        "EntityDeclHandler",
        "CommentHandler",
        "StartCdataSectionHandler",
        "EndCdataSectionHandler",
        "XmlDeclHandler",
        "ElementDeclHandler",
        "AttlistDeclHandler",
        # Stub for Kid
        "DefaultHandler",
        ]

    returns_unicode = True
    intern = {}

    def __init__(self, separator):
        self._data = []
        self._separator = separator
        self._ns_stack = []
        self.ordered_attributes = False
        self.namespace_prefixes = False

    def Parse(self, data, isfinal=False):
        self._data.append(data)
        if isfinal:
            data = "".join(self._data)
            self._data = None
            self._parse(data)

    def _qname(self):
        separator = self._separator
        reader = self._reader
        if separator is None:
            return reader.Name
        if reader.NamespaceURI:
            temp = reader.NamespaceURI + separator + reader.LocalName
            if self.namespace_prefixes:
                if reader.Prefix:
                    return temp + separator + reader.Prefix
            else:
                return temp
        else:
            return reader.LocalName

    def _parse(self, data):
        settings = XmlReaderSettings(ProhibitDtd=False)
        reader = XmlReader.Create(StringReader(data), settings)
        self._reader = reader
        while reader.Read():
            nodetype = reader.NodeType
            typename = Enum.GetName(XmlNodeType, nodetype)
            handler = getattr(self, '_handle_' + typename, None)
            if handler is not None:
                handler()

    def _handle_Element(self):
        reader = self._reader
        name = self._qname()
        ns_stack = self._ns_stack
        ns_stack.append(None)
        if self.ordered_attributes:
            attributes = []
        else:
            attributes = {}
        while reader.MoveToNextAttribute():
            if reader.Prefix == 'xmlns':
                prefix = reader.LocalName
                uri = reader.Value
                ns_stack.append(prefix)
                if hasattr(self, "StartNamespaceDeclHandler"):
                    self.StartNamespaceDeclHandler(prefix, uri)
                continue
            key = self._qname()
            value = reader.Value
            if self.ordered_attributes:
                attributes.append(key)
                attributes.append(value)
            else:
                attributes[key] = value
        reader.MoveToElement()
        if hasattr(self, "StartElementHandler"):
            self.StartElementHandler(name, attributes)
        # EndElement node is not generated for empty elements.
        # Call its handler here.
        if reader.IsEmptyElement:
            self._handle_EndElement()

    def _handle_EndElement(self):
        name = self._qname()
        if hasattr(self, "EndElementHandler"):
            self.EndElementHandler(name)
        ns_stack = self._ns_stack
        while True:
            prefix = ns_stack.pop()
            if prefix is None:
                break
            if hasattr(self, "EndNamespaceDeclHandler"):
                self.EndNamespaceDeclHandler(prefix)

    def _handle_Text(self):
        reader = self._reader
        data = reader.Value
        if hasattr(self, "CharacterDataHandler"):
            self.CharacterDataHandler(data)

    # Stub for xml.sax
    def SetBase(self, base):
        pass
    def SetParamEntityParsing(self, flag):
        return True
    # Stub for Kid
    def UseForeignDTD(self):
        pass
