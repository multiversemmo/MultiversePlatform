# Copyright (c) 2006 Seo Sanghyeon
# Copyright (c) 2006 Mark Rees
# Copyright (c) 2006 O Richter

# 2006-03-23 sanxiyn Created
# 2006-04-20 sanxiyn Added Cursor.description
# 2006-09-11 sanxiyn Merged changes from Mark Rees
#  * 2006-09-06 mark Added support for transactions
#  * 2006-09-07 mark Execution of non-query statements
#                    Added fetchone/fetchmany
# 2006-09-12 sanxiyn Added Cursor.close
#                    Added parameter binding
# 2006-09-18 mark    Explicitly set .Transaction attribute
# 2006-09-19 orichter Iterable cursor
# 2007-10-11 carsten Added DBNull handling

import clr
clr.AddReference('System.Data')

from System import Array, Object, DBNull
from System.Data.Common import DbConnectionStringBuilder

def _load_type(assembly, typename):
    import clr
    clr.AddReference(assembly)
    type = __import__(typename)
    for component in typename.split('.')[1:]:
        type = getattr(type, component)
    return type

def _build_string(dict):
    builder = DbConnectionStringBuilder()
    for key, value in dict.items():
        builder[key] = value
    return builder.ConnectionString

def connect(assembly, typename, data):
    if isinstance(data, dict):
        data = _build_string(data)
    connector = _load_type(assembly, typename)
    connection = connector(data)
    return Connection(connection)

generic_connect = connect

class Connection:

    def __init__(self, connection):
        self.connection = connection
        self.connection.Open()
        self._begin()

    def _begin(self):
        self.transaction = self.connection.BeginTransaction()

    def cursor(self):
        return Cursor(self)

    def commit(self):
        self.transaction.Commit()
        self._begin()

    def rollback(self):
        self.transaction.Rollback()
        self._begin()

    def close(self):
        self.transaction.Rollback()
        self.connection.Close()

def _schema_row_to_tuple(row):
    name = row['ColumnName']
    type = row['DataType']
    return (name, type, None, None, None, None, None)

import re
P_IS_QUERY = re.compile('^[ \r\n]*SELECT ',re.IGNORECASE)

class Cursor:

    def __init__(self, connection):
        self.connection = connection
        self.description = None
        self.arraysize = 1
        self.rowcount = -1
        self.reader = None

    def _reset(self):
        if self.reader:
            self.reader.Close()
            self.reader = None
    
    def execute(self, operation, parameters=None):
        self._reset()
        command = self.connection.connection.CreateCommand()
        command.CommandText = operation
        command.Transaction = self.connection.transaction
        if parameters is not None:
            for parameter in parameters:
                if parameter is None: parameter = DBNull.Value
                p = command.CreateParameter()
                p.Value = parameter
                command.Parameters.Add(p)
        if self._is_query(operation):
            self.reader = command.ExecuteReader()
            self._set_description()
        else:
            command.ExecuteNonQuery()
            self.description = None

    def _is_query(self, operation):
        '''Identify whether an operation is a query or not'''
        if P_IS_QUERY.match(operation):
            return True
        else:
            return False

    def _set_description(self):
        schema = self.reader.GetSchemaTable()
        self.description = map(_schema_row_to_tuple, schema.Rows)

    def _dbnull_to_none(self, x):
        if x == DBNull.Value: return None
        else: return x

    def _row_to_tuple(self):
        reader = self.reader
        values = Array.CreateInstance(Object, reader.FieldCount)
        reader.GetValues(values)
        return tuple(self._dbnull_to_none(x) for x in values)

    def fetchone(self):
        '''Fetch a single row from the cursor'''
        if self.reader.Read():
            return self._row_to_tuple()
        else:
            return None

    def fetchmany(self, size=None):
        '''Fetch up to size rows from the cursor

        Result set may be smaller than size. If size is not given,
        cursor.arraysize is used.
        '''
        if size is None:
            size = self.arraysize
        result = []
        reader = self.reader
        rowcount = 0
        while rowcount < size:
            if not reader.Read():
                break
            result.append(self._row_to_tuple())
            rowcount += 1
        return result

    def fetchall(self):
        '''Fetch all available rows from the cursor'''
        result = []
        reader = self.reader
        while reader.Read():
            result.append(self._row_to_tuple())
        return result

    def __iter__(self):
        return self

    def next(self):
        row = self.fetchone()
        if row:
            return row
        else:
            raise StopIteration

    def close(self):
        '''Close the cursor. No further queries will be possible.'''
        self._reset()
        if self.connection:
            self.connection = None

# --------------------------------------------------------------------
# connect() helpers

def translate_key(dict, keymap):
    result = {}
    for key, value in dict.items():
        key = keymap[key]
        result[key] = value
    return result

def parse_dsn(dsn):
    result = {}
    parts = dsn.split()
    for part in parts:
        key, value = part.split('=')
        result[key] = value
    return result
