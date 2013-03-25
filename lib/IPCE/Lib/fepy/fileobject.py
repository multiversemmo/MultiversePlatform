def install():
    import socket
    from _fileobject import _fileobject
    socket._fileobject = _fileobject
