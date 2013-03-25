# Copyright (c) 2006 Seo Sanghyeon

# 2006-04-19 sanxiyn Created

def _null_validation(*args):
    return True

def _make_ssl_stream_standard(stream):
    from System.Net.Security import SslStream
    ssl = SslStream(stream, True, _null_validation)
    ssl.AuthenticateAsClient('ignore')
    return ssl

def _make_ssl_stream_mono(stream):
    from Mono.Security.Protocol.Tls import SslClientStream
    ssl = SslClientStream(stream, 'ignore', False)
    ssl.ServerCertValidationDelegate = _null_validation
    return ssl

try:
    import clr
    clr.AddReference('Mono.Security')
except:
    _make_ssl_stream = _make_ssl_stream_standard
else:
    _make_ssl_stream = _make_ssl_stream_mono

from System import Array, Byte
from System.Text import Encoding
raw = Encoding.GetEncoding('iso-8859-1')

class PythonSSL:

    def __init__(self, stream):
        self.stream = stream

    def write(self, string):
        bytes = raw.GetBytes(string)
        self.stream.Write(bytes)
        return len(bytes)

    def _read_once(self, bufsize):
        buffer = Array.CreateInstance(Byte, bufsize)
        count = self.stream.Read(buffer, 0, bufsize)
        return raw.GetString(buffer[:count])

    def _read_to_end(self):
        bufsize = 4096
        all = []
        while True:
            string = self._read_once(bufsize)
            if not string:
                break
            all.append(string)
        return ''.join(all)

    def read(self, bufsize=None):
        if bufsize is None:
            return self._read_to_end()
        else:
            return self._read_once(bufsize)

from System.Net.Sockets import NetworkStream

def ssl(socket, keyfile=None, certfile=None):
    stream = NetworkStream(socket.socket)
    ssl = _make_ssl_stream(stream)
    return PythonSSL(ssl)
