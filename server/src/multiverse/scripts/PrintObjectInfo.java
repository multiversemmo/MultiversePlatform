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

package multiverse.scripts;

import multiverse.server.engine.*;
import multiverse.server.util.*;
import multiverse.server.objects.*;

import java.io.*;

/**
 * this program loads a character from the database and prints out what it can
 * <p>
 * usage: PrintObjectInfo <databasehost> <databasename> <database_user> <database_password> <objid>
 */

public class PrintObjectInfo {
    public static void main(String[] args) {
	try {
	    if (args.length != 5) {
		System.err.println("need dbhost, dbname, user, password, dbid, namespace");
		System.exit(1);
	    }
	    String host = args[0];
	    String dbname = args[1];
	    String user = args[2];
	    String password = args[3];
	    int dbid = Integer.parseInt(args[4]);
            Namespace namespace = Namespace.intern(args[5]);

	    Database db = new Database();
	    
	    String dburl = "jdbc:mysql://" + host + "/" + dbname;
	    db.connect(dburl, user, password);

	    // load the obj data from the database
	    InputStream is = db.retrieveEntityDataByOidAndNamespace(dbid, namespace);
	    Log.debug("retrieved character data");

	    // deserialize the data
	    ObjectInputStream ois = new ObjectInputStream(is);
	    Log.debug("deserializing the object now");
	    MVObject obj = (MVObject) ois.readObject();

            if (Log.loggingDebug)
                Log.debug("deserialized object: " + obj);
	}
	catch(Exception e) {
	    Log.exception("PrintObjectInfo.main got exception", e);
	}
    }
}
