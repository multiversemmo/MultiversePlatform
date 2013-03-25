from dbapi import generic_connect

assembly = 'System.Data'
typename = 'System.Data.SqlClient.SqlConnection'

def connect(dsn):
    return generic_connect(assembly, typename, dsn)
