from dbapi import generic_connect, translate_key, parse_dsn

assembly = 'Npgsql'
typename = 'Npgsql.NpgsqlConnection'
keymap = {
    'host': 'Server',
    'port': 'Port',
    'dbname': 'Database',
    'user': 'User ID',
    'password': 'Password',
}

def connect(dsn):
    params = translate_key(parse_dsn(dsn), keymap)
    return generic_connect(assembly, typename, params)
