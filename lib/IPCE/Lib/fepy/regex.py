import sys
import fepy

def install():
    fepy.override_builtin('_sre')
    import sre
    sys.modules['re'] = sre
