# Copyright (c) 2006 Seo Sanghyeon

# Based on Mathieu Fenniak's work dated 2006-06-04
# http://mathieu.fenniak.net/import-zlib-vs-net-framework/

# 2006-10-08 sanxiyn Created

from System.IO import MemoryStream
from System.IO.Compression import CompressionMode, DeflateStream

# --------------------------------------------------------------------
# Utilities

from System import Array, Byte

from System.Text import Encoding
raw = Encoding.GetEncoding('iso-8859-1')

def _make_buffer(size):
    return Array.CreateInstance(Byte, size)

def _read_to_end(stream, bufsize=4096):
    buffer = _make_buffer(bufsize)
    memory = MemoryStream()
    while True:
        count = stream.Read(buffer, 0, bufsize)
        if not count:
            break
        memory.Write(buffer, 0, count)
    bytes = memory.ToArray()
    memory.Close()
    return bytes

# --------------------------------------------------------------------
# ZLIB data format (RFC 1950)

DEFLATED = 8
MAX_WBITS = 15

def adler32(string, value=1):
    mod = 65521
    s2, s1 = divmod(value, 65536)
    for char in string:
        s1 += ord(char)
        s2 += s1
        s1 %= mod
        s2 %= mod
    return s2 * 65536 + s1

def _zlib_header(wbits=MAX_WBITS):
    CM = DEFLATED
    CINFO = wbits - 8
    CMF = CM + (CINFO << 4)
    FDICT = 0
    FLEVEL = 0
    temp = (FDICT << 5) + (FLEVEL << 6) + (CMF << 8)
    FCHECK = -temp % 31
    FLG = FCHECK + (FDICT << 5) + (FLEVEL << 6)
    return chr(CMF) + chr(FLG)

def _zlib_footer(string):
    sum = adler32(string)
    # unsigned long network byte order
    import struct
    return struct.pack('!L', sum)

# --------------------------------------------------------------------
# Compression and decompression

def compress(string, level=6):
    bytes = raw.GetBytes(string)
    stream = MemoryStream()
    zstream = DeflateStream(stream, CompressionMode.Compress, True)
    zstream.Write(bytes, 0, len(bytes))
    zstream.Close()
    compressed = raw.GetString(stream.ToArray())
    stream.Close()

    header = _zlib_header()
    footer = _zlib_footer(string)
    return header + compressed + footer

def decompress(string, wbits=MAX_WBITS):
    # Python Library Reference states:
    # When wbits is negative, the header is suppressed
    if wbits < 0:
        pass
    else:
        string = string[2:-4] # strip header and footer

    bytes = raw.GetBytes(string)
    stream = MemoryStream(bytes)
    zstream = DeflateStream(stream, CompressionMode.Decompress)
    decompressed = raw.GetString(_read_to_end(zstream))
    zstream.Close()
    return decompressed
