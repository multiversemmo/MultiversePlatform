/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

package multiverse.server.engine;

import multiverse.server.util.*;
import multiverse.server.math.*;
import multiverse.server.objects.*;

import javax.sql.rowset.serial.SerialBlob;
import java.sql.*;
import java.util.*;
import java.io.*;
import java.beans.XMLDecoder;
import java.beans.XMLEncoder;
import java.beans.ExceptionListener;
import java.util.concurrent.locks.*;

// checked for locks
// avoid importing com.mysql.jdbc.* 

/**
 * Access to Entities in database - default is MySQL
 * All Entity access requires both an object oid and a Namespace.
 */
public class Database {

    /**
     * Constructor starts the keepalive thread
     */
    public Database() {
    	
        try {
            // The newInstance() call is a work around for some
            // broken Java implementations
            Class.forName("com.mysql.jdbc.Driver").newInstance();
        } catch (Exception e) {
            throw new MVRuntimeException("could not find class: " + e);
        }
        
        // start up the keepalive class
        Log.debug("Database: starting keepalive");
        Thread keepAliveThread = new Thread(new KeepAlive(), "DBKeepalive");
        keepAliveThread.start();
    }
    
    /**
     * General database connection.  Create JDBC database for with the
     * supplied string representing the driver class,
     * e.g. "com.mysql.jdbc.Driver".  Also starts the keepalive
     * thread.
     * @author rand
    */    
    public Database(String sDriver) {        	
        if (Log.loggingDebug) {
            Log.debug("Initializing Database with driver " + sDriver);
            Log.debug("classpath = " + System.getProperty("java.class.path"));
        }
        try {
            Class.forName(sDriver).newInstance();
            if (Log.loggingDebug)
                Log.debug( sDriver + " driver loaded");
        } catch (Exception e) {
            throw new MVRuntimeException("could not find class: " + sDriver);
        }

        // start up the keepalive class
        Log.debug("Database: starting keepalive");
        Thread keepAliveThread = new Thread(new KeepAlive(), "DBKeepalive");
        keepAliveThread.start();
    }    

    /**
     * Class used to ping the database periodically to keep the database connection alive
     * @author cedeno
     */
    class KeepAlive implements Runnable {
        KeepAlive() {
        }
        
        public void run() {
            while (true) {
                try {
                    Thread.sleep(60000);
                } catch (InterruptedException e) {
                    Log.exception("Database.KeepAlive: interrupted", e);
                }
                try {
                    if (Database.this.conn != null) {
                        Database.this.ping();
                    }
                } catch (MVRuntimeException e) {
                    Log.exception("Database.KeepAlive: ping caught exception", e);
                }
            }
        }
    }
    
    /**
     * Connect to the database at the host given by the url, using the
     * given username and password
     * @param url Specifies the host to connect to.
     * @param username The account name used to log into the database.
     * @param password The password to use to log in.
     */ 
    public void connect(String url, String username, String password) {
        try {
            dbLock.lock();

            if (Log.loggingDebug)
                Log.debug("*** url = " + url + " username = " + username + " password = "+ password);
            try {
                conn = DriverManager.getConnection(url, username, password);
            } catch (Exception e) {
                throw new MVRuntimeException("could not connect to database: " + e);
            }
        } finally {
            dbLock.unlock();
        }
    }
            
    /**
     * Run the update statement in the string arg.
     * @param update An SQL update statement, or any other statement
     * that does not return values.
     */
    public void executeUpdate(String update) {
        Statement stmt = null;
        try {
            dbLock.lock();
            try {
                stmt = conn.createStatement();
                stmt.executeUpdate(update);
            } catch (Exception e) {
                Log.exception("Database.executeUpdate: Running update " + update, e);
            }
        } finally {
            dbLock.unlock();
        }
    }
    
    /**
     * Run the series of SQL statements in the list argument.
     * @param statements A list of SQL statements to be executed.
     */
    public void executeBatch(List<String> statements) {
        Statement stmt = null;
        try {
            dbLock.lock();
            try {
                stmt = conn.createStatement();
                for (String statement : statements)
                    stmt.addBatch(statement);
                stmt.executeBatch();
            } catch (Exception e) {
                Log.exception("Database.executeBatch: Running statements " + statements, e);
            }
        } finally {
            dbLock.unlock();
        }
    }
    
    /**
     * Tests to see if the table of the given name contain the given
     * row in the current database.  Used by Engine as an indication
     * the version of database schema, since normally we add tables
     * with new schema versions.
     * @param tableName The name of the table.
     * @param columnName The name of the column.
     * @return True if the column is contained in the table; false
     * otherwise.
     */
    public boolean databaseTableContainsColumn(String dbName, String tableName, String columnName) { 
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            try {
                stmt = conn.createStatement();
                String query = "SHOW COLUMNS FROM " + dbName + "." + tableName + " LIKE '" + columnName + "'";
                if (Log.loggingDebug)
                    Log.debug("Database.databaseTableContainsColumn query: " + query);
                rs = stmt.executeQuery(query);
                if (!rs.next())
                    return false;
                else {
                    return true;
                }
            } catch (Exception e) {
                throw new MVRuntimeException("Could not run select statement to determine the presence of the '" +
                    columnName + "' in table '" + tableName + "': " + e);
            }
        } finally {
            dbLock.unlock();
        }
    }

    /**
     * Tests to see if the table of the given name is present in a
     * database.  Used by Engine as an indication the version of
     * database schema, since normally we add tables with new schema
     * versions.  However, this method requires MySQL version 5, and 
     * since some customers have MySQL 4, we can't use it.
     * @param dbName The name of the database.
     * @param tableName The name of the table.
     * @return True if the table is contained in the database; false otherwise.
     */
    public boolean databaseContainsTable(String dbName, String tableName) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            try {
                stmt = conn.createStatement();
                String query = "SELECT count(*) FROM information_schema.tables WHERE table_schema = '" + dbName +
                    "' AND table_name = '" + tableName + "'";
                rs = stmt.executeQuery(query);
                if (!rs.next())
                    return false;
                else {
                    int count = rs.getInt(1);
                    return count == 1;
                }
            } catch (Exception e) {
                throw new MVRuntimeException("Exception running select statement to find table " + tableName + ": " + e);
            }
        } finally {
            dbLock.unlock();
        }
    }

    /**
     * Close the database connection.
     */
    public void close() {
        try {
            dbLock.lock();
            if (conn != null) {
                conn.close();
                conn = null;
            }
        } catch (Exception e) {
            Log.error("Database.close: unable to close connection");
        } finally {
            dbLock.unlock();
        }

    }

    /**
     * Encache the mapping between namespace strings and namespace
     * integers.
     */
    public void encacheNamespaceMapping() {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT namespace_string, namespace_int FROM namespaces";
            rs = stmt.executeQuery(query);
            while (rs.next()) {
                String nsString = rs.getString("namespace_string");
                Integer nsInt = rs.getInt("namespace_int");
                Namespace ns = Namespace.getNamespaceFromInt(nsInt);
                if (ns == null)
                    Namespace.addDBNamespace(nsString, nsInt);
                else if (!ns.getName().equals(nsString))
                    throw new MVRuntimeException("Database.encacheNamespaceMapping: Encached namespace " + 
                        ns + " doesn't have the right string " + nsString);
                if (nsInt > largestNamespaceInt)
                    largestNamespaceInt = nsInt;
            }
        } catch (Exception e) {
            Log.exception("encacheNamespaceMapping", e);
            throw new MVRuntimeException("database error: " + e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }
    
    /**
     * Find an existing namespace string in the database.  If it
     * exists, return the namespace object; else return null.  This is
     * called only if we haven't already encached the namespace.
     * @param nsString The string name of the namespace.
     * @return The namespace gotten from the database, or null.
     */
    public Namespace findExistingNamespace(String nsString) {
        encacheNamespaceMapping();
        return Namespace.getNamespaceIfExists(nsString);
    }
    
    /**
     * Find an existing namespace int in the database.  If it exists,
     * return the namespace object; else return null.  This is called
     * only if we haven't already encached the namespace.
     * @param nsInt The int associated with the namespace.
     * @return The namespace gotten from the database, or null.
     */
    public Namespace findExistingNamespace(Integer nsInt) {
        encacheNamespaceMapping();
        return Namespace.getNamespaceFromInt(nsInt);
    }

    /**
     * Store the mapping between the given namespace string and
     * integer in the namespaces table.
     * @param nsString The namespace string for the new namespace.
     * @return A newly-created Namespace, either from a row already in
     * the namespaces table, or from a new row.
     */
    public Namespace createNamespace(String nsString) {
        Statement stmt = null;
        dbLock.lock();
        try {
            // If the namespace is already in the database, return its int
            Namespace ns = findExistingNamespace(nsString);
            if (ns != null)
                return ns;
            if (largestNamespaceInt >= 31) {
                Log.error("Database.createNamespace: There are " + largestNamespaceInt +
                    " namespaces already, so you can't create another one");
                throw new MVRuntimeException("When creating namespace " + nsString + ", too many Namespaces");
            }
            // It isn't there, so add it.;
            stmt = conn.createStatement();
            int nsInt = largestNamespaceInt + 1;
            String update = "INSERT INTO namespaces (namespace_string, namespace_int) VALUES ("
                + "'" + nsString + "'" + ", " + nsInt + ")";
            int rows = 0;
            try {
                rows = stmt.executeUpdate(update);
            }
            catch (java.sql.SQLException ex) {
                // We couldn't insert the new namespace definition.
                // Perhaps the reason is that some other process has
                // already done so.
                // So one last time, try again to encache the namespace.
                ns = findExistingNamespace(nsString);
                if (ns != null)
                    return ns;
            }
            if (rows != 1) {
                throw new MVRuntimeException("Could not create namespace '" +
                        nsString + "'");
            }

            largestNamespaceInt = nsInt;
            if (Log.loggingDebug)
                Log.debug("Database.getOrCreateNamespaceInt: string " + nsString + " <=> " + nsInt);
            return Namespace.addDBNamespace(nsString, nsInt);
        } catch (Exception e) {
            Log.exception("createNamespace", e);
            return null;
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }
    
    /**
     * Creates this character as an entry in the database.  Will also populate
     * the mvid -> gameid table. will set the mvid for the user object if it
     * hasn't been set already
     * @param worldName The string name of the world in which to create the character.
     * @param mvid The Multiverse account id for the user.
     * @param user The Entity representing the user character
     * @param namespace The namespace into which to save the created
     * character object.
     */
    public void createCharacter(String worldName, int mvid, MVObject user, Namespace namespace) {
        try {
            dbLock.lock();

            user.multiverseID(mvid);
            this.saveObject(null, user, namespace);
            // int dbid = user.getDBid();
            // Log.debug("Database.createCharacter: dbid=" + dbid);
            mapMultiverseID(worldName, mvid, user.getOid());
            // return dbid;
        } finally {
            dbLock.unlock();
        }
    }

    /**
     * Adds a multiverse ID -> player OID mapping to the player_character table.
     * @param worldName The string name of the world in which to create the character.
     * @param multiverseID The Multiverse account id for the user.
     * @param objID The oid of the Entity representing the user character
     */
    public void mapMultiverseID(String worldName, int multiverseID, long objID) {
        Statement stmt = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String update = "INSERT INTO player_character (account_id, obj_id, namespace_int, world_name) VALUES ("
                + multiverseID + ", " + objID + ", " + Namespace.WORLD_MANAGER.getNumber() + ", '" + worldName + "')";
            int rows = stmt.executeUpdate(update);
            if (rows != 1) {
                throw new MVRuntimeException("failed to map multiverseid");
            }
            if (Log.loggingDebug)
                Log.debug("Database.mapMultiverseID: mapping mvid " + multiverseID
                          + " to objID " + objID);
        } catch (Exception e) {
            Log.exception("mapMultiverseID", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Returns the set of oid's for objects managed by the namespace
     * that are persisted in the geometry passed in, and all objects
     * that have no geometry.  Since for now the only objects that
     * have location are world manager objects, this will return the
     * null set for any namespace other than Nammespace.WORLD_MANAGER.
     * @param namespace The namespace containing the object to be
     * returned.
     * @param g The geometry containing the locations of objects to be
     * returned.
     */
    public Set<Long> getPersistedObjects(Namespace namespace, Geometry g) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT obj_id FROM objstore WHERE"
                    + " world_name='" + Engine.getWorldName() + "'"
                    + " AND namespace_int = " + namespace.getNumber()
                    + " AND ((locX > " + g.getMinX()
                    + " AND locX < " + g.getMaxX() + " AND locZ > "
                    + g.getMinZ() + " AND locZ < " + g.getMaxZ()
                    + ") OR (locX IS NULL))";

            rs = stmt.executeQuery(query);

            Set<Long> l = new HashSet<Long>();
            while (rs.next()) {
                long oid = rs.getLong(1);
                l.add(new Long(oid));
            }
            if (Log.loggingDebug)
                Log.debug("Database.getPersistedObjects: found " + l.size()
                          + " persisted objects for geometry " + g);
            return l;
        } catch (Exception e) {
            Log.exception("getPersistedObjects", e);
            throw new MVRuntimeException("database: " + e);
        } finally {
            if (rs != null) {
                try {
                    rs.close();
                } catch (SQLException sqlEx) {
                    rs = null;
                }
                if (stmt != null) {
                    try {
                        stmt.close();
                    } catch (SQLException sqlEx) {
                        stmt = null;
                    }
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Retrieves the user - returns null if user with OID does not
     * exist.  Deserializes the data, using the retrieveObjectData()
     * method. the dbid is the same as a gameid. This method creates a
     * world node for the user object, but not spawned.
     * @param oid The oid of the of user character.
     * @param namespace The namespace containing entity to be
     * returned.
     * @return The entity representing the user with the oid, or null
     * if it can't be found.
     */
    public Entity loadEntity(Long oid, Namespace namespace) {
        try {
            // dont need to synchronize since you are
            // creating the user for the first time

            InputStream is = this.retrieveEntityDataByOidAndNamespace(oid, namespace);
            if (is == null) {
                return null;
            }
            XMLDecoder decoder = new XMLDecoder(is, null, new XMLExceptionListener(), 
                                                this.getClass().getClassLoader());
            Entity entity = (Entity) decoder.readObject();
            decoder.close();
            // user.setPersistenceFlag(true);

            return entity;
        } catch (Exception e) {
            throw new MVRuntimeException("database.loadObject", e);
        }
    }

    /**
     * Logs the error encountered during de-serialization.
     */
    public static class XMLExceptionListener implements ExceptionListener {
        public void exceptionThrown(Exception e) {
            Log.exception("Database.loadEntity", e);
        }
    }

    /**
     * Load the entity with the given key.  Only one subobject in the
     * objstore table has the key.
     * @param persistenceKey The string key, which provides a "handle"
     * by which the object to be loaded is named.
     * @return The Entity associated with that key, or null if none exists.
     */
    public Entity loadEntity(String persistenceKey) {
        // dont need to synchronize since you are
        // creating the user for the first time

        InputStream is = retrieveEntityDataByPersistenceKey(persistenceKey);
        if (is == null) {
            return null;
        }
        XMLDecoder decoder = new XMLDecoder(is, null, new XMLExceptionListener(), 
                                            this.getClass().getClassLoader());
        Entity entity = (Entity) decoder.readObject();
        decoder.close();
        // user.setPersistenceFlag(true);

        return entity;
    }
    
    
    /**
     * Loads an object's serialization data from the database.
     * @param oid The object oid of the object to be loaded.
     * @param namespace The namespace containing the object.
     * @return an InputStream that is the object data as stored by
     * serialization.  This can really be any type, such as Mob, Npc,
     * User, Structure, or Item.  If object with oid is not found, an
     * MVRuntimeException is thrown.
     */
    public InputStream retrieveEntityDataByOidAndNamespace(long oid, Namespace namespace) {
        Statement stmt = null;
        int nsInt = namespace.getNumber();
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            ResultSet rs = stmt
                    .executeQuery("SELECT data FROM objstore WHERE world_name='"
                            + Engine.getWorldName() + "' AND obj_id = " + oid + " AND namespace_int=" + nsInt);
            if (!rs.next()) {
                Log.error("retrieveEntityDataByOidAndNamespace: not found oid="
                        + oid + " namespace="+namespace);
                return null;
            }
            Blob dataBlob = rs.getBlob("data");
            long blobLen = dataBlob.length();
            byte[] blobBytes = dataBlob.getBytes(1, (int) blobLen);
            ByteArrayInputStream bis = new ByteArrayInputStream(blobBytes);
            if (Log.loggingDebug)
                Log.debug("retrieveEntityDataByOidAndNamespace: oid="
                        + oid + " size=" + blobLen);

            return bis;
        } catch (Exception e) {
            Log.exception("retrieveEntityDataByOidAndNamespace", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Loads the  object  with  the given   persistence key  from  the
     * database.  Persistence keys provide a way to  name an object in
     * the database so as a game developer you  don't need to hardwire
     * knowledge of particular oids.
     * @param persistenceKey The string key used to locate the object
     * in the objstore table.
     * @return an InputStream that is the object data as stored by
     * serialization.  This can really be any type, such as Mob, Npc,
     * User, Structure, or Item.  If object with oid is not found, an
     * MVRuntimeException is thrown.
     */
    public InputStream retrieveEntityDataByPersistenceKey(String persistenceKey) {
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            ResultSet rs = stmt
                    .executeQuery("SELECT data FROM objstore WHERE world_name='"
                            + Engine.getWorldName() + "' AND persistence_key='" + persistenceKey + "'");
            if (!rs.next()) {
                Log.error("retrieveEntityDataByPersistenceKey: not found key="
                        + persistenceKey);
                return null;
            }
            Blob dataBlob = rs.getBlob("data");
            long blobLen = dataBlob.length();
            byte[] blobBytes = dataBlob.getBytes(1, (int) blobLen);
            ByteArrayInputStream bis = new ByteArrayInputStream(blobBytes);
            if (Log.loggingDebug)
                Log.debug("retrieveEntityDataByPersistenceKey: key=" +
                        persistenceKey + " size=" + blobLen);

            return bis;
        } catch (Exception e) {
            Log.exception("retrieveEntityDataByPersistenceKey", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    public Long getOidByName(String name, Namespace namespace)
    {
        return getOidByName(name,namespace,null);
    }

    public Long getOidByName(String name, Namespace namespace, Long instanceOid)
    {
        Statement stmt = null;
        int nsInt = namespace.getNumber();
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            String query = "SELECT obj_id FROM objstore " +
                        "WHERE world_name='" + Engine.getWorldName() + "'" +
                        " AND namespace_int=" + nsInt +
                        " AND name='" + name + "'";
            if (instanceOid != null)
                query += " AND instance=" + instanceOid;
            ResultSet rs = stmt.executeQuery(query);
            if (!rs.next()) {
                Log.debug("getOidByName: unknown name=" + name +
                        " namespace="+namespace+
                        " instanceOid="+instanceOid);
                return null;
            }
            return rs.getLong(1);
        } catch (Exception e) {
            Log.exception("getOidByName", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /** Get object name.  Name is taken from the database 'name' column.
        @return Object name or null if oid or namespace is not found.
    */
    public String getObjectName(long oid, Namespace namespace)
    {
        Statement stmt = null;
        int nsInt = namespace.getNumber();
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            String query = "SELECT name FROM objstore " +
                        "WHERE world_name='" + Engine.getWorldName() + "'" +
                        " AND namespace_int=" + nsInt +
                        " AND obj_id=" + oid;
            ResultSet rs = stmt.executeQuery(query);
            if (!rs.next()) {
                Log.debug("getObjectName: unknown oid=" + oid +
                        " namespace="+namespace);
                return null;
            }
            return rs.getString(1);
        } catch (Exception e) {
            Log.exception("getObjectName", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Get the oids and names matching the name supplied, base on the
     * name string to match, and whether to return based on a starting
     * match of an exact match. In any case, it returns a list of two
     * lists, the first being the oids and the second being the names.
     * @param playerName The name string to match.
     * @param exactMatch If true, the names returned must be an exact
     * match.  If false, the names returned must start with the
     * playerName.
     * @return A List<Object> whose first element is the List<Long> of
     * oids, and whose second element is the List<String> of names
     * matching.
     */
    public List<Object> getOidsAndNamesMatchingName(String playerName, boolean exactMatch) {
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            ResultSet rs = stmt.executeQuery("SELECT obj_id, name from objstore " +
                "WHERE world_name = '" + Engine.getWorldName() + "'" +
                " AND namespace_int = " + Namespace.WORLD_MANAGER.getNumber() + 
                " AND name" + (exactMatch ? "=" : " LIKE ") + "'" + playerName + (exactMatch ? "" : "%") + "'");
            List<Long> oids = new LinkedList<Long>();
            List<String> names = new LinkedList<String>();
            while (rs.next()) {
                oids.add(rs.getLong("obj_id"));
                names.add(rs.getString("name"));
            }
            List<Object> result = new LinkedList<Object>();
            result.add(oids);
            result.add(names);
            if (Log.loggingDebug)
            	Log.debug("Database.getOidsAndNamesMatching: For playerName '" + playerName + "'" +
                    ", found " + oids.size() + " oids: " + makeOidCollectionString(oids) + " and " +
                    names.size() + " names: " + makeNameCollectionString(names));
            return result;
        } catch (Exception e) {
            Log.exception("getOidsAndNamesMatchingName", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }
    
    /** Get object names for the given oids.  One object name is returned
        for each oid in <code>inputOids</code>.  The value of
        <code>unknownName</code> is returned for non-existent objects.
        @param inputOids The oids for which to get names.
        @param namespace The sub-object namespace.
        @param unknownName Value returned for non-existent objects.
        @return Object names ordered to matching the <code>inputOids</code>.
     */
    public List<String> getObjectNames(List<Long> inputOids,
        Namespace namespace, String unknownName)
    {
        List<String> names = new LinkedList<String>();
        if (inputOids == null || inputOids.size() == 0) {
            if (Log.loggingDebug)
                Log.debug("Database.getObjectNames: No oids in inputOids so returning empty name list");
            return names;
        }
        String whereList = "";
        for (long oid : inputOids) {
            if (whereList != "")
                whereList += " OR ";
            whereList += "(obj_id = " + oid + ")";
        }
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            ResultSet rs = stmt.executeQuery("SELECT obj_id, name from objstore WHERE world_name = '" + Engine.getWorldName() + "'" +
                " AND namespace_int = " + namespace.getNumber() + " AND (" + whereList + ")");
            List<Long> readOids = new LinkedList<Long>();
            List<String> readNames = new LinkedList<String>();
            while (rs.next()) {
                readOids.add(rs.getLong("obj_id"));
                readNames.add(rs.getString("name"));
            }
            List<String> returnedNames = new LinkedList<String>();
            for (long oid : inputOids) {
                int index = readOids.indexOf(oid);
                String name = unknownName;
                if (index != -1)
                    name = readNames.get(index);
                returnedNames.add(name);  	
            }
            if (Log.loggingDebug)
                Log.debug("Database.getObjectNames: For oids " +
                    makeOidCollectionString(inputOids) + 
                    ", returning names " +
                    makeNameCollectionString(returnedNames));
            return returnedNames;
        } catch (Exception e) {
            Log.exception("getObjectNames", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }
    
    public static String makeOidCollectionString(Collection<Long> oids) {
    	String oidString = "";
    	for (Long oid : oids) {
            if (oidString == "")
                oidString += oid;
            else
                oidString += "," + oid;
    	}
    	return oidString;
    }
    
    public static String makeNameCollectionString(Collection<String> names) {
    	String nameString = "";
        for (String name : names) {
            if (nameString != "")
                nameString += ",";
            nameString += "'" + name + "'";
        }
        return nameString;
    }
    
    public List<Namespace> getObjectNamespaces(long oid)
    {
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            ResultSet rs = stmt
                    .executeQuery("SELECT namespace_int FROM objstore " +
                        "WHERE world_name='" + Engine.getWorldName() + "'" +
                        " AND obj_id=" + oid);
            List<Namespace> result = new ArrayList<Namespace>(6);
            while (rs.next()) {
                int nsInt = rs.getInt(1);
                Namespace namespace = Namespace.getNamespaceFromInt(nsInt);
                if (namespace != null)
                    result.add(namespace);
                else
                    Log.error("getObjectNamespaces: unknown namespace id for" +
                        " oid=" + oid + " nsInt="+nsInt);
            }
            if (result.size() == 0)
                return null;
            return result;
        } catch (Exception e) {
            Log.exception("getOidByName", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    public List<Long> getInstanceContent(long instanceOid, ObjectType exclusion)
    {
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            String queryString = "SELECT obj_id FROM objstore " +
                "WHERE world_name='" + Engine.getWorldName() + "'" +
                " AND instance=" + instanceOid +
                " AND namespace_int=" + Namespace.WORLD_MANAGER.getNumber();
            if (exclusion != null)
                queryString += " AND type<>'"+exclusion.getTypeName()+"'";
                
            ResultSet rs = stmt.executeQuery(queryString);

            List<Long> result = new ArrayList<Long>(100);
            while (rs.next()) {
                long oid = rs.getLong(1);
                result.add(oid);
            }
            if (Log.loggingDebug)
                Log.debug("getInstanceContent: instanceOid=" + instanceOid +
                    " returning " + result.size() + " oids");
            return result;
        } catch (Exception e) {
            Log.exception("getInstanceContent", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Delete object and all its sub-objects from the
     * objstore table
     * @param oid The object oid to be deleted.
     */
    public void deleteObjectData(long oid) {
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement();
            stmt.execute("DELETE FROM objstore WHERE world_name=\""
                    + Engine.getWorldName() + "\" AND obj_id = " + oid);
        } catch (Exception e) {
            Log.exception("deleteObjectData", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Delete a sub-object from the objstore table
     * @param oid The object oid to delete.
     * @param namespace The sub-object namespace to delete.
     */
    public void deleteObjectData(long oid, Namespace namespace) {
        int nsInt = namespace.getNumber();
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement();
            stmt.execute("DELETE FROM objstore WHERE world_name=\""
                    + Engine.getWorldName() + "\" AND obj_id = " + oid +
                    " AND namespace_int=" + nsInt);
        } catch (Exception e) {
            Log.exception("deleteObjectData", e);
            throw new MVRuntimeException("database error: ", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Delete the user with the given oid from the player_character
     * table.  Note: This can only be done _after_ the oid/namespace
     * pair has been deleted from the objstore table.
     * @param oid The oid of the player to be deleted.
     */
    public void deletePlayerCharacter(long oid) {
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection
            stmt = conn.createStatement();
            stmt.execute("DELETE FROM player_character WHERE obj_id = " + oid);
        } catch (Exception e) {
            Log.exception("deletePlayerCharacter", e);
            throw new MVRuntimeException("database error: ", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Save the entity in the given namespace
     * @param entity The Entity object to be saved.
     * @param namespace The namespace in which to save the entity.
     */
    public void saveObject(Entity entity, Namespace namespace) {
        saveObject(null, entity, namespace);
    }
    
    /**
     * Helper method. Deserializes the object into an entity before turning it
     * into XML this may seem backwards, but we need to get some metadata out of
     * the object before we save it.
     * @param persistenceKey The key under which to save the object.
     * @param data The bytes of data to be saved.  They represent an
     * Entity, and the Entity contains its own oid.
     * @param namespace The namespace into which to save the
     * reconstructed entity.
     */
    public void saveObject(String persistenceKey, byte[] data, Namespace namespace) {
        try {
            ByteArrayInputStream bs = new ByteArrayInputStream(data);
            ObjectInputStream ois = new ObjectInputStream(bs);
            Entity entity = null;
            try {
                entity = (Entity) ois.readObject();
            } catch (ClassNotFoundException e) {
                throw new RuntimeException("call not found", e);
            }
            saveObject(persistenceKey, entity, namespace);
        } catch (IOException e) {
            throw new MVRuntimeException("saveObject", e);
        }
    }

    /**
     * FIXME: This happens on the object manager- we dont need to lock the quad
     * tree anymore saves object and all objects owned by this object into the
     * database. if the object already exists, it will overwrite with a newer
     * version. if the object is new, will create a new row and also set the
     * dbid into the obj itself (obj.getDBid() will return the new id). DOES NOT
     * MARK OBJECT AS PERSISTENT. DOES NOT CHECK FLAG.
     */
    public void saveObject(String persistenceKey, Entity entity, Namespace namespace) {
        // the object lock
        Lock entityLock = entity.getLock();

        // the world node lock
        Lock worldNodeLock = null;
        MVObject obj = null;
        if (entity instanceof MVObject) {
            obj = (MVObject) entity;
            WMWorldNode node = (WMWorldNode) obj.worldNode();
            if (node != null) {
                if (node.getQuadNode() != null) {
                    worldNodeLock = node.getQuadNode().getTree().getLock();
                }
            }
        }

        // the object's serialized data
        byte[] data = null;
        try {
            // need to lock transfer because when we serialize, we look
            // at the objects in this user and it will lock the transferlock
            // so we should do it here first to keep the lock ordering correct
            MVObject.transferLock.lock();

            try {
                if (worldNodeLock != null) {
                    worldNodeLock.lock();
                }
                try {
                    // lock the worldnode & obj since we want a "snapshot"
                    entityLock.lock();

        Log.debug("encoding entity type="+entity.getType());
                    // serialize the object
                    ByteArrayOutputStream ba = new ByteArrayOutputStream();
                    encodeEntity(ba, entity);

//                    encoder.setPersistenceDelegate(MVObject.class, new MVObjectPersistenceDelegate("MVObject"));
//                    encoder.setPersistenceDelegate(MarsObject.class, new MVObjectPersistenceDelegate("MarsObject"));
//                    encoder.setPersistenceDelegate(MarsMob.class, new MVObjectPersistenceDelegate("MarsMob"));
//                    encoder.setPersistenceDelegate(MobilePerceiver.class, new MVObjectPersistenceDelegate("MobilePerceiver"));
//                    encoder.setPersistenceDelegate(MarsPermissionCallback.class, new MVObjectPersistenceDelegate("MarsPermCallback"));
//                    encoder.setPersistenceDelegate(WMWorldNode.class, new MVObjectPersistenceDelegate("WMWorldNode"));
//                    encoder.setPersistenceDelegate(InterpolatedWorldNode.class, new MVObjectPersistenceDelegate("InterpolatedWorldNode"));
//                    encoder.setPersistenceDelegate(DisplayContext.class, new MVObjectPersistenceDelegate("DisplayContext"));
//                    encoder.setPersistenceDelegate(DisplayContext.class, new MVObjectPersistenceDelegate("DisplayContext"));
//                     encoder.setPersistenceDelegate(InterpolatedWorldNode.class, new InterpolatedWorldNode.BasicWorldNodePersistenceDelegate());
                    data = ba.toByteArray();
                    if (Log.loggingDebug)
                        Log.debug("Database.saveObject: ns="+namespace+
                                " type=" +entity.getType()+
                                " xml conversion: length="+
                                data.length + ", string=" + (new String(data)));
                } catch (Exception e) {
                    throw new MVRuntimeException("Database.saveObject: failed on "
                            + obj.getName(), e);
                } finally {
                    entityLock.unlock();
                }
            } finally {
                if (worldNodeLock != null) {
                    worldNodeLock.unlock();
                }
            }
        } finally {
            MVObject.transferLock.unlock();
        }

        // we have the serialized data, now save it
        saveObjectHelper(persistenceKey, entity, namespace, data);
    }

    /**
     * Write the representation of the entity into the
     * ByteArrayOutputStream.
     * @param ba The ByteArrayOutputStream to which the entity will be
     * serialized.
     * @param entity The entity to be serialized.
     */
    protected void encodeEntity(ByteArrayOutputStream ba, Entity entity) {
        Thread cur = Thread.currentThread();
        ClassLoader ccl = cur.getContextClassLoader();
        ClassLoader myClassLoader = this.getClass().getClassLoader();
        cur.setContextClassLoader(myClassLoader);
        XMLEncoder encoder = null;
        try {
            encoder = new XMLEncoder(ba);
            encoder.setExceptionListener(new ExceptionListener() {
                    public void exceptionThrown(Exception exception) {
                        Log.exception(exception);
                    }
                });
            encoder.setPersistenceDelegate(ObjectType.class,
                                           new ObjectType.PersistenceDelegate());
            encoder.writeObject(entity);
        } finally {
            if (null != encoder) { 
                encoder.close();
                try {
                    ba.flush();
                }
                catch (Exception e) {
                    Log.exception("Database.encodeEntity", e);
                }
                
            }
            cur.setContextClassLoader(ccl);
        }
    }

    /**
     * A helper method that saves the byte array in the namespace with
     * the oid supplied by the entity.
     * @param persistenceKey The string key to associate with the
     * object.  For most objects, this is null.
     * @param entity The entity whose state is represented by the data.
     * @param namespace The namespace into which to save the object state.
     * @param data The serialized state of the entity.
     */
    public void saveObjectHelper(String persistenceKey, Entity entity, Namespace namespace, byte[] data) {
        MVObject obj = null;
        Point loc = null;
        Long instanceOid = null;
        int nsInt = namespace.getNumber();
        if (entity instanceof MVObject) {
            obj = (MVObject) entity;
            loc = obj.getLoc();
            if (obj.worldNode() != null)
                instanceOid = obj.worldNode().getInstanceOid();
        }
        Statement stmt = null;
        try {
            dbLock.lock(); // lock the connection

            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_SENSITIVE,
                    ResultSet.CONCUR_UPDATABLE);

            ResultSet uprs = stmt
                    .executeQuery("SELECT * FROM objstore WHERE world_name='"
                            + Engine.getWorldName() + "' AND obj_id=" + entity.getOid() + " AND namespace_int=" + nsInt);

            // set the cursor to the appropriate row
            boolean prevSaved = uprs.first(); // has this obj been saved
                                                // before
            if (!prevSaved) {
                uprs.moveToInsertRow();
                if (Log.loggingDebug)
                    Log.debug("Database.saveObject: obj not in database, moved to insert row: "
                              + obj);
            }

            // helper function which updates the row with the obj's data
            if (Log.loggingDebug)
                Log.debug("Database.saveObjectHelper: saving obj: "
                          + entity.getName());
            updateRow(uprs, instanceOid, loc, entity.getOid(),
                    nsInt, data, entity.getName(),
                    entity.getType().getTypeName(), persistenceKey);

            // save the data into the database
            if (prevSaved) {
                uprs.updateRow();
            } else {
                uprs.insertRow();

                // // set the database id for the object
                // uprs.last();
                // int newDbID = uprs.getInt("obj_id");
                // Log.debug("Database.saveObject: new db obj dbid=" + newDbID);
                // obj.setDBid(newDbID);
            }
            Log.debug("done with saving char to the database");
        } catch (Exception e) {
            Log.exception("saveObjectHelper", e);
            throw new MVRuntimeException("database error", e);
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Helper function to saving object data.  Updates the row in the
     * result set with the object's values.  The result set's cursor
     * should be positioned appropriately.  For persistObject (first
     * time), it should be on the insert row, and for updating an
     * object, it should already be on the object's row.  Assumes the
     * database connection is already locked if needed.
     * @param uprs The ResultSet object against which the updates
     * should be run.
     * @param loc The object location.
     * @param oid The object oid.
     * @param nsInt The integer identifying the namespace in which the
     * object should be saved.
     * @param data The serialized data for the object.
     * @param name The object name; only a few objects have names.
     * @param type The object type.
     * @param persistenceKey The string key associated with the
     * object.  Only a few objects have keys.
     */
    private void updateRow(ResultSet uprs, Long instanceOid, Point loc,
        long oid, int nsInt, byte[] data,
        String name, String type, String persistenceKey)
        throws SQLException, IOException
    {
        if (Log.loggingDebug)
            Log.debug("byte array length=" + data.length);

        // create blob
        Log.debug("Database.updateRow: creating blob from byte stream");
        Blob blob = new SerialBlob(data);
        if (Log.loggingDebug)
            Log.debug("Database.updateRow: created blob, datalength=" + data.length
                      + ", bloblength=" + blob.length());

        // update the database
        uprs.updateLong("obj_id", oid);
        uprs.updateInt("namespace_int", nsInt);
        uprs.updateString("world_name", Engine.getWorldName());
        if (instanceOid != null)
            uprs.updateLong("instance", instanceOid);
        else
            uprs.updateNull("instance");
        if (loc != null) {
            uprs.updateInt("locX", loc.getX());
            uprs.updateInt("locY", loc.getY());
            uprs.updateInt("locZ", loc.getZ());
        } else {
            uprs.updateNull("locX");
            uprs.updateNull("locY");
            uprs.updateNull("locZ");
        }
        uprs.updateString("type", type);
        uprs.updateString("name", name);
        if (persistenceKey != null) {
            uprs.updateString("persistence_key", persistenceKey);
        }
        uprs.updateBlob("data", blob);
    }

    /**
     * Each Multiverse account may have more than 1 character for a
     * given world.  This function returns a list of "game_id"s each
     * of which represent an individual character.
     * @param worldName The name of the world containing the
     * characters.
     * @param multiverseID The Multiverse account id of the user.
     * @return a list of Integer. the game_id is the dbid in the
     * database record.
     */
    public List<Long> getGameIDs(String worldName, int multiverseID) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT obj_id FROM player_character WHERE account_id = "
                    + multiverseID + " AND world_name = '" + worldName + "'";
            rs = stmt.executeQuery(query);

            LinkedList<Long> l = new LinkedList<Long>();
            while (rs.next()) {
                long gameId = rs.getLong(1);
                l.add(new Long(gameId));
                if (Log.loggingDebug)
                    Log.debug("getgameid: multiverseid " + multiverseID
                              + " maps to gameID=" + gameId);
            }
            if (l.isEmpty()) {
                if (Log.loggingDebug)
                    Log.debug("getgameid: found no mapping gameids for multiverseid "
                              + multiverseID + " and worldName " + worldName);
            }
            return l;
        } catch (Exception e) {
            Log.exception("getGameIDs", e);
            throw new MVRuntimeException("database: ", e);
        } finally {
            if (rs != null) {
                try {
                    rs.close();
                } catch (SQLException sqlEx) {
                    rs = null;
                }
                if (stmt != null) {
                    try {
                        stmt.close();
                    } catch (SQLException sqlEx) {
                        stmt = null;
                    }
                }
            }
            dbLock.unlock();
        }
    }

    public int getAccountCount(String worldName)
    {
        Statement stmt = null;
        ResultSet rs = null;
        int count = -1;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT COUNT(*) FROM player_character WHERE" +
                    " world_name = '" + worldName + "'";
            rs = stmt.executeQuery(query);
            if (rs.next()) {
                count = rs.getInt(1);
            }
            return count;
        } catch (Exception e) {
            Log.exception("getAccountCount", e);
            throw new MVRuntimeException("database: ", e);
        } finally {
            if (rs != null) {
                try {
                    rs.close();
                } catch (SQLException sqlEx) {
                    rs = null;
                }
                if (stmt != null) {
                    try {
                        stmt.close();
                    } catch (SQLException sqlEx) {
                        stmt = null;
                    }
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Get user name.  Checks ID, returns user name if there is a
     * match, otherwise returns null.
     * @param uid The user oid.
     * @return The string username corresponding to the oid, or null
     * if not found.
    */
    public String getUserName(long uid) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT username FROM account WHERE account_id = " + uid;
            rs = stmt.executeQuery(query);

            // do we have any results?
            if (!rs.next()) {
                return null;
            }
            String name = rs.getString(1);
            if (Log.loggingDebug)
                Log.debug("uid:" + uid + "=" + name);
            return name;
        } catch (Exception e) {
            Log.warn("Database.getUserName: unable to get username, this is ok if you are not on production server: " + e);
            return null;
        } finally {
            if (rs != null) {
                try {
                    rs.close();
                } catch (SQLException sqlEx) {
                    rs = null;
                }
                if (stmt != null) {
                    try {
                        stmt.close();
                    } catch (SQLException sqlEx) {
                        stmt = null;
                    }
                }
            }
            dbLock.unlock();
        }
    }

    /** Get object location.  The namespace is usually
        WorldManagerClient.NAMESPACE.
        @param oid Object oid.
        @param ns Namespace from which to get location.
        @param location Returns the object location.
        @return Object instance oid, or null if object does not exist.
    */
    public Long getLocation(long oid, Namespace ns, Point location)
    {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();

            stmt = conn.createStatement();
            rs = stmt.executeQuery("SELECT locX,locY,locZ,instance " +
                            "FROM objstore " +
                            "WHERE " +
                            "obj_id=" + oid +
                            " AND namespace_int=" + ns.getNumber());

            if (!rs.next()) {
                return null;
            }

            location.setX(rs.getInt(1));
            location.setY(rs.getInt(2));
            location.setZ(rs.getInt(3));

            return rs.getLong(4);
        }
        catch (SQLException ex) {
            Log.exception("Database.getLocation()", ex);
            return null;
        }
        finally {
            if (rs != null) {
                try {
                    rs.close();
                } catch (SQLException sqlEx) {
                    rs = null;
                }
                if (stmt != null) {
                    try {
                        stmt.close();
                    } catch (SQLException sqlEx) {
                        stmt = null;
                    }
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Returns a block of oids, and record the fact that they have
     * been allocated in the database.
     * @param chunkSize The number oids to return.
     * @return An OidChunk instance giving the first and last oid
     * allocated.
     */
    public OidChunk getOidChunk(int chunkSize) {
        Statement stmt = null;
        try {
            dbLock.lock();
            conn.setAutoCommit(false);
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            ResultSet rs = stmt
                    .executeQuery("SELECT free_oid FROM oid_manager WHERE token = 1");
            if (!rs.next()) {
                throw new MVRuntimeException("Database.getOidChunk: no free chunks");
            }
            long freeOid = rs.getLong("free_oid");
            stmt.close();

            //
            // now update the free oid
            //
            stmt = conn.createStatement();
            String update = "UPDATE oid_manager SET free_oid = "
                    + (freeOid + chunkSize);
            /* int rows = */ stmt.executeUpdate(update);
            conn.commit();

            return new OidChunk(freeOid, freeOid + chunkSize - 1);
        } catch (Exception e) {
            throw new MVRuntimeException("Database.getOidChunk", e);
        } finally {
            try {
                conn.setAutoCommit(true);
            } catch (SQLException sqlEx) {
            }

            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Register a plugin
     */
    public boolean registerStatusReportingPlugin(EnginePlugin plugin,
        long runId)
    {
        Statement stmt = null;
        unregisterStatusReportingPlugin(plugin);
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String update = "INSERT INTO plugin_status " +
                "(world_name, agent_name, plugin_name, plugin_type, host_name, pid, run_id, percent_cpu_load, last_update_time, next_update_time, status, info)" +
                " VALUES (" +
                "'" + Engine.getWorldName() + "', " +
                "'" + Engine.getAgent().getName() + "', " +
                "'" + plugin.getName() + "', " +
                "'" + plugin.getPluginType() + "', " +
                "'" + Engine.getEngineHostName() + "', " +
                "0, " + // ??? TBD: How do you get a process id
                runId + ", " +
                "'" + plugin.getPercentCPULoad() + "', " +
                System.currentTimeMillis() + ", " +
                "0, " +
                "'" + StringEscaper.escapeString(plugin.getPluginStatus()) + "', " +
                "'" + StringEscaper.escapeString(plugin.getPluginInfo()) + "' " + 
                ")";
            int rows = stmt.executeUpdate(update);
            return (rows >= 1);
        } catch (Exception e) {
            Log.exception("Database.registerStatusReportingPlugin", e);
            return false;
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Unregister the plugin for status reporting
     */
    public boolean unregisterStatusReportingPlugin(EnginePlugin plugin) {
        Statement stmt = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String update = "DELETE FROM plugin_status WHERE " +
                "world_name='" + Engine.getWorldName() + "' AND " +
                "plugin_name='" + plugin.getName() + "'";
            int rows = stmt.executeUpdate(update);
            return (rows >= 1);
        } catch (Exception e) {
            Log.exception("Database.unregisterStatusReportingPlugin", e);
            return false;
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Update the status of a plugin
     */
    public boolean updatePluginStatus(EnginePlugin plugin, long nextUpdateTime) {
        Statement stmt = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            long now = System.currentTimeMillis();
            String update = "UPDATE plugin_status SET " +
                "last_update_time=" + now + ", " +
                "next_update_time=" + nextUpdateTime + ", " +
                "status='" + StringEscaper.escapeString(plugin.getPluginStatus()) + "', " +
                "percent_cpu_load='" + plugin.getPercentCPULoad() + "' " +
                "WHERE world_name='" + Engine.getWorldName() + "' AND " +
                    "plugin_name='" + plugin.getName() + "'";
            int rows = stmt.executeUpdate(update);
            return (rows >= 1);
        } catch (Exception e) {
            Log.exception("updatePluginStatus", e);
            return false;
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Get the contents of the database row into a PluginStatus
     * instance.  If the plugin is non-null, get for that plugin, else
     * get for all plugins
     */
    public List<PluginStatus> getPluginStatus(String pluginType)
    {
        Statement stmt = null;
        List<PluginStatus> statusList = new LinkedList<PluginStatus>();
        try {
            dbLock.lock();
            conn.setAutoCommit(false);
            stmt = conn.createStatement(ResultSet.TYPE_SCROLL_INSENSITIVE,
                    ResultSet.CONCUR_READ_ONLY);
            String select = "SELECT * FROM plugin_status " +
                "WHERE world_name='" + Engine.getWorldName() + "'";
            if (pluginType != null)
                select += " AND plugin_type='" + pluginType + "'";
            ResultSet rs = stmt.executeQuery(select);
            while(rs.next()) {
                PluginStatus status = new PluginStatus();
                statusList.add(status);
                status.world_name = rs.getString("world_name");
                status.agent_name = rs.getString("agent_name");
                status.plugin_name = rs.getString("plugin_name");
                status.plugin_type = rs.getString("plugin_type");
                status.host_name = rs.getString("host_name");
                status.pid = rs.getInt("pid");
                status.run_id = rs.getLong("run_id");
                status.percent_cpu_load = rs.getInt("percent_cpu_load");
                status.last_update_time = rs.getLong("last_update_time");
                status.next_update_time = rs.getLong("next_update_time");
                // We don't need to unescape these strings, because MySQL
                // does it for us.
                status.status = rs.getString("status");
                status.info = rs.getString("info");
            }
            return statusList;
        } catch (Exception e) {
            Log.exception("Database.getPluginStatus", e);
            return statusList;
        } finally {
            try {
                conn.setAutoCommit(true);
            } catch (SQLException sqlEx) {
            }

            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }
        
    public static class StringEscaper {
    
        private static Map<Character, Character> toStringSequences = null;
        private static Map<Character, Character> fromStringSequences = null;
    
        private StringEscaper() {
            toStringSequences = new HashMap<Character, Character>();
            fromStringSequences = new HashMap<Character, Character>();
            add('\0', '0');      // An ASCII 0 (NUL) character.
            add('\'', '\'');     // A single quote (') character.
            add('\"', '\"');     // A double quote (") character.
            add('\b', 'b');      // A backspace character.
            add('\r', 'r');      // A newline (linefeed) character.
            add('\n', 'n');      // A carriage return character.
            add('\t', 't');      // A tab character.
            add('\u001A', 'Z');  // ASCII 26 (Control-Z).
            add('\\', '\\');     // A backslash (\) character.
            add('%', '%');       // A % character.
        }
            
        private void add(char from, char to) {
            toStringSequences.put(from, to);
            fromStringSequences.put(to, from);
        }

        public static String escapeString(String input) {
            if (instance == null)
                instance = new StringEscaper();
            int length = (input == null ? 0 : input.length());
            StringBuilder sb = new StringBuilder(length + 10);
            for (int i=0; i<length; i++) {
                char ch = input.charAt(i);
                Character replacement = StringEscaper.toStringSequences.get(ch);
                if (replacement != null) {
                    sb.append('\\');
                    sb.append(replacement);
                }
                else
                    sb.append(ch);
            }
            return sb.toString();
        }

        public static String unescapeString(String input) {
            if (instance == null)
                instance = new StringEscaper();
            StringBuilder sb = new StringBuilder(input.length() + 10);
            int i = 0;
            while (i < input.length()) {
                char ch = input.charAt(i);
                if (ch == '\\') {
                    i++;
                    ch = input.charAt(i);
                    char replacement = StringEscaper.fromStringSequences.get(ch);
                    sb.append(replacement);
                }
                else
                    sb.append(ch);
            }
            return sb.toString();
        }

        private static StringEscaper instance = null;
    }

    /**
     * Runs a select statement to make sure we can still talk to the
     * database.
     */
    public void ping() {
        Log.debug("Database: ping");
        Statement stmt = null;
        dbLock.lock();
        try {
            String sql = "SELECT 1 from player_character";
            stmt = conn.createStatement();
            stmt.executeQuery(sql);
        } catch (Exception e) {
            reconnect();
        } finally {
            if (stmt != null) {
                try {
                    stmt.close();
                } catch (SQLException sqlEx) {
                    stmt = null;
                }
            }
            dbLock.unlock();
        }
    }

    /**
     * Reestablish contact with the database, or throw an error if we
     * can't.
     */
    void reconnect() {
        // looks like the database connection went away, re-establish
        Log.error("Database reconnect: url=" + Engine.getDBUrl());

        int failCount=0;
        dbLock.lock();
        try {
            while (true) {
                try {
                    conn = DriverManager.getConnection(Engine.getDBUrl(),
                        Engine.getDBUser(), Engine.getDBPassword());
                    Log.info("Database: reconnected to "+Engine.getDBUrl());
                    return;
                } catch (Exception e) {
                    try {
                        if (failCount == 0)
                            Log.exception("Database reconnect failed, retrying",e);
                        else if (failCount % 300 == 299)
                            Log.error("Database reconnect failed, retrying: "+e);
                        failCount++;
                        Thread.sleep(1000);
                    } catch (InterruptedException ie) {
                        /* ignore */
                    }
                }
            }
        } finally {
            dbLock.unlock();
        }
    }

    /**
     * A class to represent a range of allocated oids.
     */
    public static class OidChunk {
        public OidChunk(long begin, long end) {
            this.begin = begin;
            this.end = end;
        }

        public long begin;

        public long end;
    }

    private Connection conn = null;

    transient Lock dbLock = LockFactory.makeLock("databaseLock");

    private static int largestNamespaceInt = 0;

}
