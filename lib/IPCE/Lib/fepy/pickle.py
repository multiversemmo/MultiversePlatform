# Copyright (c) 2007 Seo Sanghyeon

# 2007-02-02 sanxiyn Created

from System.IO import MemoryStream
from System.Runtime.Serialization.Formatters.Binary import BinaryFormatter

from System.Text import Encoding
raw = Encoding.GetEncoding('iso-8859-1')

import copy_reg

_formatter = BinaryFormatter()

def _serialization_reconstructor(string):
    bytes = raw.GetBytes(string)
    stream = MemoryStream(bytes)
    return _formatter.Deserialize(stream)

def _serialization_reduce(obj):
    stream = MemoryStream()
    _formatter.Serialize(stream, obj)
    bytes = stream.ToArray()
    string = raw.GetString(bytes)
    return _serialization_reconstructor, (string,)

def register(type):
    copy_reg.pickle(type, _serialization_reduce)
