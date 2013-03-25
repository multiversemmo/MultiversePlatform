from dbapi import generic_connect

assembly = 'System.Data'
typename = 'System.Data.Odbc.OdbcConnection'

def connect(dsn):
    return generic_connect(assembly, typename, dsn)
