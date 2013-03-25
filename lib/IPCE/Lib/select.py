# Copyright (c) 2006 Seo Sanghyeon

# 2006-05-01 sanxiyn Created

from System.Net.Sockets import Socket
from socket import PythonSocket

def select(rlist, wlist, xlist, timeout=None):
    _rlist = [r.socket for r in rlist]
    _wlist = [w.socket for w in wlist]
    _xlist = [x.socket for x in xlist]
    if timeout is None:
        timeout = -1
    else:
        timeout = timeout * 1000000
    Socket.Select(_rlist, _wlist, _xlist, timeout)
    rlist = [PythonSocket(r) for r in _rlist]
    wlist = [PythonSocket(w) for w in _wlist]
    xlist = [PythonSocket(x) for x in _xlist]
    return rlist, wlist, xlist
