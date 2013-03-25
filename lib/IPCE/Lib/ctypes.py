# Copyright (c) 2006 Seo Sanghyeon

# 2006-06-08 sanxiyn Created
# 2006-06-11 sanxiyn Implemented .value on primitive types
# 2006-11-02 sanxiyn Support for multiple signatures

__all__ = [
    'c_int', 'c_float', 'c_double', 'c_char_p', 'c_void_p',
    'LibraryLoader', 'CDLL', 'cdll',
    'byref', 'sizeof'
    ]

# --------------------------------------------------------------------
# Dynamic module definition

from System import AppDomain
from System.Reflection import AssemblyName
from System.Reflection.Emit import AssemblyBuilderAccess

def pinvoke_module():
    domain = AppDomain.CurrentDomain
    name = AssemblyName('pinvoke')
    flag = AssemblyBuilderAccess.Run
    assembly = domain.DefineDynamicAssembly(name, flag)
    module = assembly.DefineDynamicModule('pinvoke')
    return module

# --------------------------------------------------------------------
# General interface

class pinvoke_value:
    type = None
    value = None

def get_type(obj):
    if isinstance(obj, pinvoke_value):
        return obj.type
    else:
        return type(obj)

def get_value(obj):
    if isinstance(obj, pinvoke_value):
        return obj.value
    else:
        return obj

# --------------------------------------------------------------------
# Primitive types

from System import Single, Double, IntPtr

class pinvoke_primitive(pinvoke_value):

    def __init__(self, value=None):
        if value is None:
            value = self.type()
        if not isinstance(value, self.type):
            expected = self.type.__name__
            given = value.__class__.__name__
            msg = "%s expected instead of %s" % (expected, given)
            raise TypeError(msg)
        self.value = value

    def __repr__(self):
        clsname = self.__class__.__name__
        return "%s(%r)" % (clsname, self.value)

class c_int(pinvoke_primitive):
    type = int

class c_float(pinvoke_primitive):
    type = Single

class c_double(pinvoke_primitive):
    type = Double

class c_char_p(pinvoke_primitive):
    type = str

class c_void_p(pinvoke_primitive):
    type = IntPtr

# --------------------------------------------------------------------
# Reference

from System import Type

class pinvoke_reference(pinvoke_value):

    def __init__(self, obj):
        self.obj = obj
        self.type = Type.MakeByRefType(obj.type)
        self.value = obj.value

    def __repr__(self):
        return "byref(%r)" % (self.obj,)

def byref(obj):
    if not isinstance(obj, pinvoke_value):
        raise TypeError("byref() argument must be a ctypes instance")
    ref = pinvoke_reference(obj)
    return ref

# --------------------------------------------------------------------
# Utility

from System.Runtime.InteropServices import Marshal

def sizeof(obj):
    return Marshal.SizeOf(obj.type)

# --------------------------------------------------------------------
# Dynamic P/Invoke

from System import Array
from System.Reflection import CallingConventions, MethodAttributes
from System.Runtime.InteropServices import CallingConvention, CharSet
from IronPython.Runtime.Calls import BuiltinFunction, FunctionType

class pinvoke_method:

    pinvoke_attributes = (
        MethodAttributes.Public |
        MethodAttributes.Static |
        MethodAttributes.PinvokeImpl
        )

    calling_convention = None
    return_type = None

    def __init__(self, dll, entry):
        self.dll = dll
        self.entry = entry
        self.restype = None
        self.argtypes = None
        self.func = None
        self.signatures = set()

    def create(self, restype, argtypes):
        dll = self.dll
        entry = self.entry
        attributes = self.pinvoke_attributes
        cc = self.calling_convention
        clr_argtypes = Array[Type](argtypes)

        module = pinvoke_module()
        module.DefinePInvokeMethod(
            entry, dll, attributes, CallingConventions.Standard,
            restype, clr_argtypes, cc, CharSet.Ansi)
        module.CreateGlobalFunctions()

        method = module.GetMethod(entry)
        self.func = BuiltinFunction.MakeOrAdd(
            self.func, entry, method, FunctionType.Function)
        self.signatures.add((restype, argtypes))

    def __call__(self, *args):
        if self.restype:
            restype = self.restype.type
        else:
            restype = self.return_type.type

        if self.argtypes:
            argtypes = [argtype.type for argtype in self.argtypes]
        else:
            argtypes = [get_type(arg) for arg in args]
        argtypes = tuple(argtypes)

        if (restype, argtypes) not in self.signatures:
            self.create(restype, argtypes)

        args = [get_value(arg) for arg in args]
        result = self.func(*args)
        return result

# --------------------------------------------------------------------
# Function loader

def is_special_name(name):
    return name.startswith('__') and name.endswith('__')

class pinvoke_dll:

    method_class = None

    def __init__(self, name):
        self.name = name

    def __repr__(self):
        clsname = self.__class__.__name__
        return "<%s '%s'>" % (clsname, self.name)

    def __getattr__(self, name):
        if is_special_name(name):
            raise AttributeError(name)
        method = self.method_class(self.name, name)
        setattr(self, name, method)
        return method

class CDLL(pinvoke_dll):
    class method_class(pinvoke_method):
        calling_convention = CallingConvention.Cdecl
        return_type = c_int

# --------------------------------------------------------------------
# Library loader

class LibraryLoader(object):

    def __init__(self, dlltype):
        self.dlltype = dlltype

    def __getattr__(self, name):
        if is_special_name(name):
            raise AttributeError(name)
        dll = self.dlltype(name)
        setattr(self, name, dll)
        return dll

    def LoadLibrary(self, name):
        return self.dlltype(name)

cdll = LibraryLoader(CDLL)
