def install():
    import os
    if 'FEPY_OPTIONS' not in os.environ:
        return
    names = os.environ['FEPY_OPTIONS'].split(',')
    for name in names:
        install_option(name)

def install_option(name):
    modname = 'fepy.' + name
    module = getattr(__import__(modname), name)
    module.install()

def override_builtin(name):
    import imp, sys, os
    sys.modules[name] = module = imp.new_module(name)
    path = os.path.join(sys.prefix, 'Lib', name + '.py')
    execfile(path, module.__dict__)
