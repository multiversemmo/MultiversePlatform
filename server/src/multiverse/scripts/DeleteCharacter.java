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

import java.util.*;

import multiverse.server.engine.*;
import multiverse.server.util.*;

/**
 * this program deletes an avatar or character in the database
 * <p>
 * usage: DeleteCharacter <database_host> <database_name> <database_user> <database_password> <multiverse_id> <world_id>
 */

public class DeleteCharacter {
    public static void main(String[] args) {
        try {
            if (args.length != 6) {
                System.err.println("need dbhost, dbname, dbuser, dbpassword, multiverseid, world_name");
                System.exit(1);
            }
            Database db = new Database();

            String host = args[0];
            String dbname = args[1];
            String user = args[2];
            String password = args[3];
	    
            int multiverseID = Integer.parseInt(args[4]);
            String worldName = args[5];
            
            //Assume MySQL
    	    String dburl = "jdbc:mysql://" + host + "/" + dbname;
    	    db.connect(dburl, user, password);
    	                
            Engine.setOIDManager(new OIDManager(db));
            Engine.getOIDManager().defaultChunkSize = 1;

            List<Long> gameIDs = db.getGameIDs(worldName, multiverseID);
            Iterator iter = gameIDs.iterator();
            while (iter.hasNext()) {
                Long gameId = (Long)iter.next();
                db.deleteObjectData(gameId.longValue());
                db.deletePlayerCharacter(gameId.longValue());
                if (Log.loggingDebug)
                    Log.debug("deleted obj: " + gameId.longValue());
            }
        }
        catch(Exception e) {
	    Log.exception("DeleteCharacter.main got exception", e);
        }
        Log.debug("Shutting down");
    }
    
}
