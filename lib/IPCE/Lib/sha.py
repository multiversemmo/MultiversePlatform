from System.Security.Cryptography import SHA1
from hashlib import make_new

new = make_new(SHA1)
del make_new

digest_size = 20
