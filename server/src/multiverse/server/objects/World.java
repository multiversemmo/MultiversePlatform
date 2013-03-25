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

package multiverse.server.objects;

import multiverse.server.math.*;
import multiverse.server.util.*;
import java.util.*;
import java.util.concurrent.locks.*;

/**
 * properties for the world object and its behavior
 */
public class World {
    private World() {
    }

    public static long DEBUG_OID = 2221998;

    /**
     * sets the world geometry - the quad tree will be 'this' big
     */
    public static void setGeometry(Geometry g) {
	worldGeometry = g;
    }
    public static Geometry getGeometry() {
	return worldGeometry;
    }
    private static Geometry worldGeometry = null;

    public static void setLocalGeometry(Geometry g) {
	localGeo = g;
    }
    public static Geometry getLocalGeometry() {
	return localGeo;
    }
    private static Geometry localGeo = null;

    /**
     * used to set the UI theme for the game.
     * the server sends this list of UI elements over to the client
     * when a new player logs in.  see the UI Theme event in the protocol
     */
    public static void addTheme(String theme) {
        staticLock.lock();
        try {
            uiThemes.add(theme);
        }
        finally {
            staticLock.unlock();
        }
    }
    
    /**
     * replaces any existing themes set with the passed in theme.
     */
    public static void setTheme(String theme) {
        staticLock.lock();
        try {
            uiThemes.clear();
            uiThemes.add(theme);
        }
        finally {
            staticLock.unlock();
        }
    }
    /**
     * removes all themes
     *
     */
    public static void clearThemes() {
        staticLock.lock();
        try {
            uiThemes.clear();
        }
        finally{
            staticLock.unlock();
        }
    }
    
    public static List<String> getThemes() {
        staticLock.lock();
        try {
            return new LinkedList<String>(uiThemes);
        }
        finally {
            staticLock.unlock();
        }
    }
    private static List<String> uiThemes = new LinkedList<String>();
    
    /**
     * temporarily used to override whether the player follows terrain
     */
    public static Boolean FollowsTerrainOverride = null;
    
    /**
     * returns the max location diff between server & client before
     * server forces reset of client loc
     */
    public static void setLocTolerance(int dist) {
	locTolerance = dist;
    }
    /**
     * sets the max location diff between server & client before
     * server forces reset of client loc
     */
    public static int getLocTolerance() {
	return locTolerance;
    }

    public static int perceiverRadius = 100000;

    /**
     * the max diff between what the server and client thinks
     * are the client's location before the server forces client
     * to move.  default is 30000 mm.
     */
    private static int locTolerance = 30000;

    public static void setDefaultPermission(PermissionFactory factory) {
	defaultPermissionFactory = factory;
    }
    public static PermissionFactory getDefaultPermission() {
	return defaultPermissionFactory;
    }
    private static PermissionFactory defaultPermissionFactory = null;

    private static Lock staticLock = LockFactory.makeLock("StaticWorldLock");
}
