# Copyright (c) 2006 Seo Sanghyeon

# 2006-01-26 sanxiyn Created as md5.py
# 2006-03-25 sanxiyn Copied md5.py as sha.py
# 2006-10-11 sanxiyn Reorganized to follow hashlib API
#                    Added digest_size
# 2006-11-11 sanxiyn Accept array as well as string

from System.Security.Cryptography import (
    HashAlgorithm,
    MD5, SHA1, SHA256, SHA384, SHA512)

from System.Text import Encoding
raw = Encoding.GetEncoding('iso-8859-1')
empty = raw.GetBytes('')

from System import Array, Byte
import array

class PythonHash:

    def __init__(self, context):
        self.context = context
        self.digest_size = context.HashSize / 8

    def update(self, string):
        if isinstance(string, array.array):
            string = string.array
        if isinstance(string, str):
            bytes = raw.GetBytes(string)
        elif isinstance(string, Array[Byte]):
            bytes = string
        elif isinstance(string, Array):
            bytes = Array[Byte](list(string))
        else:
            raise TypeError("argument must be string or array")
        self.context.TransformBlock(bytes, 0, len(bytes), bytes, 0)

    def digest(self):
        context = self.context.MemberwiseClone()
        context.TransformFinalBlock(empty, 0, 0)
        return raw.GetString(context.Hash)

    def hexdigest(self):
        context = self.context.MemberwiseClone()
        context.TransformFinalBlock(empty, 0, 0)
        return ''.join(['%02x' % byte for byte in context.Hash])

    def copy(self):
        context = self.context.MemberwiseClone()
        return PythonHash(context)

# As of Mono 1.1.17.1, HashAlgorithm.Create doesn't accept lowercased
# algorithm names unlike .NET. Mono bug #79641.

def new(name, string=''):
    name = name.upper()
    context = HashAlgorithm.Create(name)
    if not context:
        raise ValueError('unsupported hash type')
    hash = PythonHash(context)
    if string:
        hash.update(string)
    return hash

def make_new(algorithm):
    def new(string=''):
        context = algorithm.Create()
        hash = PythonHash(context)
        if string:
            hash.update(string)
        return hash
    return new

md5 = make_new(MD5)
sha1 = make_new(SHA1)
sha256 = make_new(SHA256)
sha384 = make_new(SHA384)
sha512 = make_new(SHA512)

def sha224(string=''):
    raise NotImplementedError(
        ".NET Framework doesn't provide SHA224 hash algorithm")

try:
    import clr
    clr.AddReference('Mono.Security')
except:
    pass
else:
    # Use Mono's SHA224 implementation when available
    from Mono.Security.Cryptography import SHA224
    sha224 = make_new(SHA224)
