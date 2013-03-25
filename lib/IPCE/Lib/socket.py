# Copyright (c) 2006 Seo Sanghyeon

# 2006-01-31 sanxiyn Created
# 2006-02-03 sanxiyn Added DNS functions and getsockname/getpeername
# 2006-03-15 sanxiyn Added socket.error, convert number to constant
# 2006-03-17 sanxiyn Added sendall, makefile
# 2006-03-18 sanxiyn Added recvfrom, use array slicing
#                    Added getsockopt/setsockopt, getfqdn
# 2006-04-13 sanxiyn Added error handling with _handle_error decorator
# 2006-04-19 sanxiyn Added SSL support
# 2006-04-26 sanxiyn Added sendto, fixed close
# 2006-05-01 sanxiyn Added inet_aton/inet_ntoa
# 2006-11-11 baus    Added _fileobject class
# 2006-11-20 sanxiyn Splitted _fileobject to the separate file
# 2006-11-29 sanxiyn Added timeout, SocketType

from System import Array, Byte, Enum

from System.Net import IPAddress, IPEndPoint
from System.Net.Sockets import Socket, SocketException, NetworkStream
from System.Net.Sockets import AddressFamily, ProtocolType, SocketType
from System.Net.Sockets import SocketOptionLevel, SocketOptionName

# Name collision with the Python interface
ClrSocketType = SocketType

from System.Text import Encoding
raw = Encoding.GetEncoding('iso-8859-1')

def _make_buffer(size):
    return Array.CreateInstance(Byte, size)

AF_INET = AddressFamily.InterNetwork
AF_UNSPEC = AddressFamily.Unspecified
SOCK_STREAM = ClrSocketType.Stream
SOCK_DGRAM = ClrSocketType.Dgram
IPPROTO_IP = ProtocolType.IP

SOL_SOCKET = SocketOptionLevel.Socket
SO_REUSEADDR = SocketOptionName.ReuseAddress

AI_PASSIVE = None

class error(Exception):
    pass

class timeout(error):
    pass

def _handle_error(function):
    def wrapper(*args, **kwargs):
        try:
            return function(*args, **kwargs)
        except SocketException, e:
            code = e.SocketErrorCode
            message = e.Message
            raise error(code, message)
    return wrapper

def _address_to_endpoint(address):
    host, port = address
    ip = gethostbyname(host)
    return IPEndPoint(IPAddress.Parse(ip), port)

def _endpoint_to_address(endpoint):
    return str(endpoint.Address), endpoint.Port

def _make_endpoint():
    return IPEndPoint(IPAddress.Any, 0)

class PythonSocket:

    def __init__(self, socket):
        self.socket = socket

    @_handle_error
    def bind(self, address):
        endpoint = _address_to_endpoint(address)
        self.socket.Bind(endpoint)

    def listen(self, backlog):
        self.socket.Listen(backlog)

    def accept(self):
        conn = self.socket.Accept()
        address = _endpoint_to_address(conn.RemoteEndPoint)
        return PythonSocket(conn), address

    @_handle_error
    def connect(self, address):
        endpoint = _address_to_endpoint(address)
        self.socket.Connect(endpoint)

    def send(self, string):
        bytes = raw.GetBytes(string)
        return self.socket.Send(bytes)

    sendall = send

    def sendto(self, string, address):
        bytes = raw.GetBytes(string)
        endpoint = _address_to_endpoint(address)
        return self.socket.SendTo(bytes, endpoint)

    def recv(self, bufsize):
        buffer = _make_buffer(bufsize)
        received = self.socket.Receive(buffer)
        string = raw.GetString(buffer[:received])
        return string

    def recvfrom(self, bufsize):
        buffer = _make_buffer(bufsize)
        endpoint = _make_endpoint()
        received, endpoint = self.socket.ReceiveFrom(buffer, endpoint)
        string = raw.GetString(buffer[:received])
        address = _endpoint_to_address(endpoint)
        return string, address

    def close(self):
        # There may be NetworkStream's still open
        #self.socket.Close()
        pass

    def getsockopt(self, level, name):
        return self.socket.GetSocketOption(level, name)

    def setsockopt(self, level, name, value):
        self.socket.SetSocketOption(level, name, value)

    def gettimeout(self):
        return None

    def settimeout(self, value):
        pass

    def setblocking(self, flag):
        pass

    def getsockname(self):
        endpoint = self.socket.LocalEndPoint
        return _endpoint_to_address(endpoint)

    def getpeername(self):
        endpoint = self.socket.RemoteEndPoint
        return _endpoint_to_address(endpoint)

    def makefile(self, mode='r', bufsize=-1):
        stream = NetworkStream(self.socket)
        return file(stream, mode)

def socket(family=AF_INET, type=SOCK_STREAM, proto=IPPROTO_IP):
    family = Enum.ToObject(AddressFamily, family)
    type = Enum.ToObject(ClrSocketType, type)
    proto = Enum.ToObject(ProtocolType, proto)
    socket = Socket(family, type, proto)
    return PythonSocket(socket)

SocketType = PythonSocket

# --------------------------------------------------------------------
# IP address functions

def inet_aton(string):
    ip = IPAddress.Parse(string)
    packed = raw.GetString(ip.GetAddressBytes())
    return packed

def inet_ntoa(packed):
    bytes = raw.GetBytes(packed)
    ip = IPAddress(bytes)
    return str(ip)

# --------------------------------------------------------------------
# DNS functions

from System.Net import Dns

def _safe_gethostbyname(hostname):
    if not hostname:
        hostname = str(IPAddress.Any)
    return Dns.GetHostByName(hostname)

def _entry_to_triple(entry):
    hostname = entry.HostName
    aliaslist = list(entry.Aliases)
    ipaddrlist = map(str, entry.AddressList)
    return hostname, aliaslist, ipaddrlist

def gethostname():
    return Dns.GetHostName()

def gethostbyname(hostname):
    entry = _safe_gethostbyname(hostname)
    return str(entry.AddressList[0])

def gethostbyname_ex(hostname):
    entry = _safe_gethostbyname(hostname)
    return _entry_to_triple(entry)

def gethostbyaddr(address):
    entry = Dns.GetHostByAddress(address)
    return _entry_to_triple(entry)

def getfqdn(hostname=''):
    if not hostname:
        hostname = Dns.GetHostName()
    entry = Dns.GetHostByName(hostname)
    return entry.HostName

def getaddrinfo(host, port, family=AF_INET, socktype=SOCK_STREAM,
                proto=IPPROTO_IP, flags=None):
    entry = _safe_gethostbyname(host)
    family = Enum.ToObject(AddressFamily, family)
    socktype = Enum.ToObject(ClrSocketType, socktype)
    proto = Enum.ToObject(ProtocolType, proto)
    if family == AF_UNSPEC:
        family = AF_INET
    return [ (family, socktype, proto, '', (str(ip), port))
             for ip in entry.AddressList ]

try:
    from ssl import ssl
except ImportError:
    pass

try:
    from _fileobject import _fileobject
except ImportError:
    pass
