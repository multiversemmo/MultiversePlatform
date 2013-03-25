# Copyright (c) 2006 Seo Sanghyeon

# This code is derived from PyPy's implementation of array module.
# The main change is the use of .NET array as storage instead of Python list.

# 2006-11-11 sanxiyn Bare minimum to run pycrypto's randpool
# 2006-11-26 sanxiyn Added fromlist, fromstring, tolist, tostring
#                    Added more typecodes

from System import (
    Array,
    SByte, Byte, Int16, UInt16, Int32, UInt32, Single, Double)

from struct import pack, unpack

_type_mapping = {
    'b': (SByte, 1),
    'B': (Byte, 1),
    'h': (Int16, 2),
    'H': (UInt16, 2),
    'i': (Int32, 4),
    'I': (UInt32, 4),
    'f': (Single, 4),
    'd': (Double, 8),
}

class array(object):

    def __new__(cls, typecode, initializer=[]):
        self = object.__new__(cls)
        self.typecode = typecode
        self.type, self.itemsize = _type_mapping[typecode]
        self.array = Array[self.type]([])
        if isinstance(initializer, list):
            self.fromlist(initializer)
        elif isinstance(initializer, str):
            self.fromstring(initializer)
        else:
            raise NotImplementedError("array")
        return self

    def fromlist(self, l):
        array = Array[self.type](l)
        self.array += array

    def fromstring(self, s):
        n = len(s) / self.itemsize
        l = unpack(self.typecode * n, s)
        self.fromlist(l)

    def tolist(self):
        return list(self.array)

    def tostring(self):
        n = len(self.array)
        a = self.array
        return pack(self.typecode * n, *a)

    def __repr__(self):
        if not self.array:
            return "array('%s')" % self.typecode
        else:
            return "array('%s', %s)" % (self.typecode, list(self.array))

    def __getitem__(self, index):
        if isinstance(index, slice):
            sliced = array(self.typecode)
            sliced.array = self.array[index]
            return sliced
        return self.array[index]

    def __setitem__(self, index, value):
        self.array[index] = value
