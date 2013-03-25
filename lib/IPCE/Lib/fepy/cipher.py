# Copyright (c) 2006 Seo Sanghyeon

# 2006-11-12 sanxiyn Created

from System.Security.Cryptography import (
    DES, Rijndael, TripleDES)

from System.Text import Encoding
raw = Encoding.GetEncoding('iso-8859-1')

from System.Security.Cryptography import CipherMode

MODE_ECB = CipherMode.ECB
MODE_CBC = CipherMode.CBC

_mode_mapping = {
    'MODE_ECB': MODE_ECB,
    'MODE_CBC': MODE_CBC,
}

class PythonCipher:

    def __init__(self, context):
        self.context = context

    def encrypt(self, string):
        bytes = raw.GetBytes(string)
        transform = self.context.CreateEncryptor()
        transform.TransformBlock(bytes, 0, len(bytes), bytes, 0)
        return raw.GetString(bytes)

    def decrypt(self, string):
        bytes = raw.GetBytes(string)
        transform = self.context.CreateDecryptor()
        transform.TransformBlock(bytes, 0, len(bytes), bytes, 0)
        return raw.GetString(bytes)

def make_new(algorithm):
    def new(key, mode, iv):
        context = algorithm.Create()
        context.Key = raw.GetBytes(key)
        context.Mode = mode
        context.IV = raw.GetBytes(iv)
        cipher = PythonCipher(context)
        return cipher
    return new

def install_cipher(name, algorithm, block_size, key_size):
    modname = 'Crypto.Cipher.' + name
    import imp
    module = imp.new_module(modname)
    module.new = make_new(algorithm)
    module.block_size = block_size
    module.key_size = key_size
    for mode in _mode_mapping:
        setattr(module, mode, _mode_mapping[mode])
    import sys
    sys.modules[modname] = module
    import Crypto.Cipher
    setattr(Crypto.Cipher, name, module)

def install():
    install_cipher('AES', Rijndael, 16, 16)
    install_cipher('DES', DES, 8, 8)
    install_cipher('DES3', TripleDES, 8, 8)
