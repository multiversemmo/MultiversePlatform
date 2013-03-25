from dbapi import generic_connect

assembly = 'Mono.Data.SqliteClient'
typename = 'Mono.Data.SqliteClient.SqliteConnection'

# As of Mono 1.1.17.1, version can't be passed as a connection string
# parameter. Mono bug #79632.

def connect(filename):
    params = {}
    params['URI'] = 'file:' + filename + ',Version=3'
    return generic_connect(assembly, typename, params)
