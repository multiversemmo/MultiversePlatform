from System.Security.Cryptography import MD5
from hashlib import make_new

new = make_new(MD5)
del make_new
md5 = new

digest_size = 16
