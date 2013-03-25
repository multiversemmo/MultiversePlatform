"""Add .pth and site-packages support to IronPython

Since this code is a modified version of the standard CPython distribution, 
it is licensed with the same license:

http://www.python.org/download/releases/2.4.3/license/

"""
import sys
import os

def makepath(*paths):
    dir = os.path.abspath(os.path.join(*paths))
    return dir, os.path.normcase(dir)

def removeduppaths():
    """ Remove duplicate entries from sys.path along with making them
    absolute"""
    # This ensures that the initial path provided by the interpreter contains
    # only absolute pathnames, even if we're running from the build directory.
    L = []
    known_paths = set()
    for dir in sys.path:
        # Filter out duplicate paths (on case-insensitive file systems also
        # if they only differ in case); turn relative paths into absolute
        # paths.
        dir, dircase = makepath(dir)
        if not dircase in known_paths:
            L.append(dir)
            known_paths.add(dircase)
    sys.path[:] = L
    return known_paths

def _init_pathinfo():
    """Return a set containing all existing directory entries from sys.path"""
    d = set()
    for dir in sys.path:
        try:
            if os.path.isdir(dir):
                dir, dircase = makepath(dir)
                d.add(dircase)
        except TypeError:
            continue
    return d

def addpackage(sitedir, name, known_paths):
    """Add a new path to known_paths by combining sitedir and 'name' or execute
    sitedir if it starts with 'import'"""
    if known_paths is None:
        _init_pathinfo()
        reset = 1
    else:
        reset = 0
    fullname = os.path.join(sitedir, name)
    try:
        f = open(fullname, "rU")
    except IOError:
        return
    try:
        for line in f:
            if line.startswith("#"):
                continue
            if line.startswith("import"):
                exec line
                continue
            line = line.rstrip()
            dir, dircase = makepath(sitedir, line)
            if not dircase in known_paths and os.path.exists(dir):
                sys.path.append(dir)
                known_paths.add(dircase)
    finally:
        f.close()
    if reset:
        known_paths = None
    return known_paths

def addsitedir(sitedir, known_paths=None):
    """Add 'sitedir' argument to sys.path if missing and handle .pth files in
    'sitedir'"""
    if known_paths is None:
        known_paths = _init_pathinfo()
        reset = 1
    else:
        reset = 0
    sitedir, sitedircase = makepath(sitedir)
    if not sitedircase in known_paths:
        sys.path.append(sitedir)        # Add path component
    try:
        names = os.listdir(sitedir)
    except os.error:
        return
    names.sort()
    for name in names:
        if name.endswith(os.extsep + "pth"):
            addpackage(sitedir, name, known_paths)
    if reset:
        known_paths = None
    return known_paths

def addsitepackages(known_paths):
    """Add site-packages (and possibly site-python) to sys.path"""
    prefixes = [os.path.join(sys.prefix, "local"), sys.prefix]
    if sys.exec_prefix != sys.prefix:
        prefixes.append(os.path.join(sys.exec_prefix, "local"))
    for prefix in prefixes:
        if prefix:
            sitedirs = [os.path.join(prefix, "Lib", "site-packages")]
            for sitedir in sitedirs:
                if os.path.isdir(sitedir):
                    addsitedir(sitedir, known_paths)
    return None

def execsitecustomize():
    """Run custom site specific code, if available."""
    try:
        import sitecustomize
    except ImportError:
        pass

def main():
    paths_in_sys = removeduppaths()
    paths_in_sys = addsitepackages(paths_in_sys)
    execsitecustomize()

main()
