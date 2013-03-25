from dbapi import generic_connect

assembly = 'Mono.Data.SqliteClient'
typename = 'Mono.Data.SqliteClient.SqliteConnection'

def connect(filename):
    params = {}
    params['URI'] = 'file:' + filename
    return generic_connect(assembly, typename, params)
