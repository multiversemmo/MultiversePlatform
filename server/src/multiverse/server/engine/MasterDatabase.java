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

import java.sql.*;
import java.util.*;
import java.util.concurrent.locks.*;

// checked for locks
// avoid importing com.mysql.jdbc.* 

/**
 * Access to Entities in database - default is MySQL
 * All Entity access requires both an object oid and a Namespace.
 */
public class MasterDatabase {

    /**
     * Constructor starts the keepalive thread
     */
    public MasterDatabase() {
    	
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
    public MasterDatabase(String sDriver) {        	
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
                    if (MasterDatabase.this.conn != null) {
                        MasterDatabase.this.ping();
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
            Log.error("MasterDatabase.close: unable to close connection");
        } finally {
            dbLock.unlock();
        }

    }

    /**
     * Method to print the set of registered developers, which are
     * stored in the master server database.
     */
    public void printDevelopers() {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT * FROM developer";
            rs = stmt.executeQuery(query);

            // do we have any results?
            while (rs.next()) {
                int devId = rs.getInt("dev_id");
                String email = rs.getString("email");
                String company = rs.getString("company");
                String skill = rs.getString("skill");
                String prior = rs.getString("prior");
                String genre = rs.getString("genre");
                String idea = rs.getString("idea");
                System.out.println("devId=" + devId + "\nemail=" + email
                        + "\ncompany=" + company + "\nskill=" + skill
                        + "\nprior=" + prior + "\ngenre=" + genre + "\nidea="
                        + idea);
            }
        } catch (Exception e) {
            Log.exception("printDevelopers", e);
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
     * Returns the multiverse account id if the username and password
     * match. otherwise, returns -1.  Used by the master server.
     * @param username The name of the user account.
     * @param password The user's account password.
     * @return The account id associated with the username.
     */
    public int MVAcctPasswdCheck(String username, String password) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT account_id, password FROM account WHERE username = '"
                    + username + "' AND activated > 0 AND suspended = 0";
            rs = stmt.executeQuery(query);

            // do we have any results?
            if (!rs.next()) {
                return -1;
            }
            String realPassword = rs.getString(2);
            if (!realPassword.equals(password)) {
                return -1;
            }
            int uid = rs.getInt(1);
            if (Log.loggingDebug)
                Log.debug("username=" + username + ", uid=" + uid);
            return uid;
        } catch (Exception e) {
            Log.exception("MVAcctPasswdCheck", e);
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
     * A class to represent a world - world name, world mgr server,
     * hostname, port, mediaURL
     */
    public static class WorldInfo {
        WorldInfo(String worldName, String svrHostName, int port, String patcherURL, String mediaURL) {
            this.worldName = worldName;
            this.svrHostName = svrHostName;
            this.port = port;
	    this.patcherURL = patcherURL;
	    this.mediaURL = mediaURL;
        }

        public String worldName = null;

        public String svrHostName = null;

        public int port = -1;

        public String patcherURL = null;

        public String mediaURL = null;
    }

    /**
     * Resolves the passed-in world ID to a servername and port.  The
     * master server usually does this lookup to tell clients where to
     * go to connect to a particular worldName.
     * @param worldName The string name of the world.
     * @return The servername and port in inetsocketaddress object,
     * null if there is no match
     */
    public WorldInfo resolveWorldID(String worldName) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT server_name, server_port, patcher_URL, media_URL FROM world WHERE world_name = '"
                    + worldName + "'";
            rs = stmt.executeQuery(query);

            // do we have any results?
            if (!rs.next()) {
                return null;
            }

            String hostname = rs.getString(1);
            int port = rs.getInt(2);
             String patcherURL = rs.getString(3);
             String mediaURL = rs.getString(4);

            WorldInfo worldInfo = new WorldInfo(worldName, hostname, port, patcherURL, mediaURL);
            return worldInfo;
        } catch (Exception e) {
            Log.exception("resolveWorldID", e);
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

    public String getPassword(String username) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT account_id, password FROM account WHERE username = '" +
                username + "' AND activated > 0 AND suspended = 0";
            rs = stmt.executeQuery(query);

            // do we have any results?
            if (!rs.next()) {
                return null;
            }
            return rs.getString(2);
        } catch (Exception e) {
            Log.exception("getPassword", e);
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

    public Integer getAccountId(String username) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT account_id FROM account WHERE username = '" +
                username + "' AND activated > 0 AND suspended = 0";
            rs = stmt.executeQuery(query);

            // do we have any results?
            if (!rs.next()) {
                return null;
            }
            return rs.getInt(1);
        } catch (Exception e) {
            Log.exception("getAccountId", e);
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
     * Checks password, returns the user OID if matches, otherwise
     * returns -1.
     * @param username The username string.
     * @param password The password string.
     * @return The oid associated with the username.
     */
    public long passwordCheck(String username, String password) {
        Statement stmt = null;
        ResultSet rs = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String query = "SELECT account_id, password FROM account WHERE username = '"
                    + username + "' AND activated > 0 AND suspended = 0";
            rs = stmt.executeQuery(query);

            // do we have any results?
            if (!rs.next()) {
                return -1;
            }
            String realPassword = rs.getString(2);
            if (!realPassword.equals(password)) {
                return -1;
            }
            long uid = rs.getInt(1);
            if (Log.loggingDebug)
                Log.debug("username=" + username + ", uid=" + uid);
            return uid;
        } catch (Exception e) {
            Log.exception("passwordCheck", e);
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
     * Create a user - returns false if failed - usually because
     * username is not unique.
     * @param username The username string.
     * @param password The password string.
     * @return True if the user/password pair could be created; false
     * otherwise.
     */
    public boolean createUser(String username, String password) {
        Statement stmt = null;
        try {
            dbLock.lock();
            stmt = conn.createStatement();
            String update = "INSERT INTO account (username, password) VALUES (\""
                    + username + "\", \"" + password + "\")";
            int rows = stmt.executeUpdate(update);
            return (rows >= 1);
        } catch (Exception e) {
            Log.exception("createUser", e);
            Log.error("database error: " + e);
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
     * A main program that exercises database functionality.
     * @param args Command-line args with the format 
     * "java Database <username> <password> <count> <namespace>"
     */
    public static void main(String[] args) {
        try {
            if (args.length != 3) {
                System.err
                        .println("creates <count> users named <username>0 <username>1 ... <username><count-1> all with same password");
                System.err
                        .println("java Database <username> <password> <count> <namespace>");
                System.exit(1);
            }
            String username = args[0];
            String password = args[1];

            int count = Integer.valueOf(args[2]).intValue();

            //## I don't think this should be the default behavior -
            //## not every server needs db connection.
            MasterDatabase db = new MasterDatabase(Engine.getDBDriver());
            db.connect(Engine.getDBUrl(), Engine.getDBUser(),
                Engine.getDBPassword());

            for (int i = 0; i < count; i++) {
                String uname = username + i;
                if (db.createUser(uname, password)) {
                    System.out.println("created user ok: " + uname);
                } else {
                    System.out.println("creation failed");
                }

                long oid = db.passwordCheck(uname, password);
                if (oid == -1) {
                    System.out.println("password check failed in database");
                } else {
                    System.out.println("password check passed, oid=" + oid);
                }
            }
        }
        catch (Exception e) {
            Log.error("Database: " + e);
        }
    }
    
    private Connection conn = null;

    transient Lock dbLock = LockFactory.makeLock("masterDBLock");
}
