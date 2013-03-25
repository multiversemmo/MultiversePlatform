import fepy

def install():
    fepy.override_builtin('socket')
    fepy.override_builtin('select')
