from dbapi import connect as generic_connect, translate_key

assembly = 'MySql.Data'
typename = 'MySql.Data.MySqlClient.MySqlConnection'
keymap = {
    'host': 'Server',
    'user': 'User ID',
    'passwd': 'Password',
    'db': 'Database',
    'port': 'Port',
}

def connect(**kwargs):
    params = translate_key(kwargs, keymap)
    return generic_connect(assembly, typename, params)
