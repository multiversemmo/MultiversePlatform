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

package multiverse.simpleclient;

import java.awt.Polygon;
import java.awt.Rectangle;
import multiverse.server.util.*;
import multiverse.server.events.*;
import multiverse.server.math.*;
import multiverse.server.plugins.*;
import multiverse.server.engine.*;
import multiverse.server.network.*;
import java.util.Random;
import java.util.Map;
import java.util.HashMap;
import java.io.Serializable;

public class PlayerClient {
    
    protected PlayerClient() {
    }
    
    public static class PlayerThread implements Runnable {

        public PlayerThread(BehaviorParms parms, SimpleClient sc)
        {
            if (SimpleClient.logCounters && playerClientStatsLogger == null) {
                playerClientStatsLogger = new Thread(new PlayerClientStatsLogger());
                playerClientStatsLogger.start();
            }
            this.parms = parms;
            this.sc = sc;
        }
        
        protected SimpleClient sc = null;
        protected BehaviorParms parms;
        protected float playerYaw = 180f;
        protected MVVector playerPosition = new MVVector(0,0,0);
        protected MVVector playerDestination = null;   // Used by the test program
        MVVector startingPosition = new MVVector(0,0,0);
        protected MVVector playerDir = new MVVector(1.0f,0,0);
        protected MVVector playerAccel = new MVVector(7000.0f,0,0);
        protected Quaternion playerOrientation = new Quaternion();
        protected BasicWorldNode playerCorrection = null;
        protected boolean newBehavior = true;
        MVVector yAxis = new MVVector(0, 1f, 0);
        MVVector zAxis = new MVVector(0f, 0f, 1f);
        ExtensionWait extensionWait = new ExtensionWait();

        class ExtensionWait {
            public String subType = "";
        }

        public void run() {
            playerThreadIterations++;
            Thread.currentThread().setName("PC-" + sc.accountID);
            sc.dispatcher.registerHandler(2, new HandleWorldNodeCorrectMessage ());
            sc.dispatcher.registerHandler(3, new HandleComMessage ());
            sc.dispatcher.registerHandler(79, new HandleDirLocOrientMessage ());
            sc.dispatcher.registerHandler(83, new HandleExtensionMessage ());

            // Wait for the player entity to exist and for login to complete
            extensionWait.subType = "mv.SCENE_END";
            synchronized (sc) {
                while (! sc.loggedIn) {
                    try {
                        sc.wait();
                    } catch(InterruptedException e) {
                        Log.exception(e);
                    }
                }
            }
            synchronized (extensionWait) {
                while (! extensionWait.subType.equals("")) {
                    try {
                        extensionWait.wait();
                    } catch (InterruptedException e) { /* ignore */ }
                }
            }
            Log.info("Login and scene complete, oid="+sc.charOid);

            if (parms.instance != null) {
                instancePlayer(parms.instance);
            }

            if (parms.polygonRegionName != null) {
                String cmd = ("/getregion -pxz " + parms.polygonRegionName);
                synchronized (parms.polygonRegionName) {
                    sc.sendCommandMessage(cmd, sc.charOid);
                    try {
                        parms.polygonRegionName.wait();
                    } catch (InterruptedException e) { /* ignore */ }
                }
            }
            if (parms.randomStart && parms.polygon != null) {
                parms.playerPosition = calcRandomPosition(parms.polygon);
                parms.playerPosition.setY(playerPosition.getY());
                Log.info("Starting player " + sc.charOid + " at " + parms.playerPosition);
            }
            if (parms.playerPosition.length() != 0) {
                Point pointPosition = new Point(parms.playerPosition);
                String cmd = ("/setloc " +
                    pointPosition.getX() + " " +
                    pointPosition.getY() + " " + 
                    pointPosition.getZ());
                sc.sendCommandMessage(cmd, sc.charOid);
            }
            startingPosition = (parms.playerPosition.length() > 0 ? (MVVector)parms.playerPosition.clone() : new MVVector(0,0,0));
            boolean loggedOut = StopAndStartBehavior(parms);
            if (loggedOut && !SimpleClient.scExitPending) {
		Log.info("Recreating playerThread");
		Thread playerThread = new Thread(new PlayerThread(parms, sc));
		playerThread.start();
	    }
        }

        // We handle WorldNodeCorrectMessage for our player
        public class HandleWorldNodeCorrectMessage implements multiverse.server.engine.MessageHandler {
            
            public HandleWorldNodeCorrectMessage() {
            }
            
            public String getName() {
                return "PlayerClientWNCorrectHandler";
            }

            public Event handleMessage (ClientConnection con, MVByteBuffer buf) {
                WorldManagerClient.WorldNodeCorrectMessage msg = new WorldManagerClient.WorldNodeCorrectMessage();
                msg.fromBuffer(buf);
                if (sc.charOid == msg.getSubject())
                    applyCorrection(msg.getWorldNode());
                return null;
            }
        }
        
        // We handle DirLocOrientMessage for our player
        public class HandleDirLocOrientMessage implements multiverse.server.engine.MessageHandler {
            
            public HandleDirLocOrientMessage() {
            }
            
            public String getName() {
                return "PlayerClientDirLocOrient";
            }

            public Event handleMessage (ClientConnection con, MVByteBuffer buf) {
                WorldManagerClient.DirLocOrientMessage msg = new WorldManagerClient.DirLocOrientMessage();
                msg.fromBuffer(buf);
                if (sc.charOid == msg.getSubject())
                    applyCorrection(msg.getWorldNode());
                return null;
            }
        }

        // Monitor Com messages
        // Current used for --polygon_region
        public class HandleComMessage implements multiverse.server.engine.MessageHandler {
            
            public HandleComMessage() {
            }
            
            public String getName() {
                return "PlayerClientCom";
            }

            public Event handleMessage(ClientConnection con, MVByteBuffer buf) {
                WorldManagerClient.TargetedComMessage msg =
                    new WorldManagerClient.TargetedComMessage();
                msg.fromBuffer(buf);
                if (sc.charOid == msg.getSubject()) {
                    String text = msg.getString();
                    if (Log.loggingDebug)
                        Log.debug("Got COM message: "+text);
                    if (text.startsWith("POINTS:")) {
                        String pointsString = text.split(" ",2)[1];
                        synchronized (parms.polygonRegionName) {
                            parsePolygon(pointsString,parms);
                            parms.polygonRegionName.notify();
                        }
                    }
                }
                return null;
            }
        }

        public class HandleExtensionMessage
            implements multiverse.server.engine.MessageHandler
        {
            public HandleExtensionMessage() {
            }
            
            public String getName() {
                return "PlayerClientExtension";
            }

            public Event handleMessage(ClientConnection con, MVByteBuffer buf) {
                //WorldManagerClient.TargetedExtensionMessage msg =
                //    new WorldManagerClient.TargetedExtensionMessage();
                ExtensionMessageEvent msg = new ExtensionMessageEvent();
                msg.parseBytes(buf);
                String subType = msg.getExtensionType();
                if (Log.loggingDebug)
                    Log.debug("got ExtensionMessage subType="+subType);
                synchronized (extensionWait) {
                    if (subType.equals(extensionWait.subType)) {
                        extensionWait.subType = "";
                        extensionWait.notify();
                    }
                }
                return null;
            }
        }

        protected void applyCorrection(BasicWorldNode node) {
            // The correction is for us - - send
            // the next iteration of AsyncBehavior
            playerCorrection = node;
            playerPosition = new MVVector(playerCorrection.getLoc());
            playerDir = playerCorrection.getDir();
            if (Log.loggingDebug)
                Log.debug("Applying WorldNodeCorrectMessage, new position " + playerPosition + ", new dir " + playerDir);
            // Null out the playerCorrection, so we don't see
            // it next iteration
            setDirLocOrient(playerDir, playerPosition, playerOrientation);
        }

        public boolean StopAndStartBehavior(BehaviorParms parms) {
            boolean playerActive = false;
            // we start many player clients, they don't run in lockstep.
            long seed = ((long)sc.accountID) * 1111;
            Log.info("Initializing PlayerClient random number generator to " + seed);
            Random moveRand = new Random(seed);
            // Start out moving in a randon direction
            playerYaw = 360f * moveRand.nextFloat();
            playerPosition = (MVVector)parms.playerPosition.clone();
            setDirAndOrientation();
            try {
                Thread.sleep(parms.postLoginSleep);
            }
            catch(InterruptedException e) {
                Log.exception(e);
            }
            // Tell SimpleClient where we're starting
            setDirLocOrient(playerDir, playerPosition, playerOrientation);
            Log.info("Player startingPosition = " + startingPosition);
            // Loop forever
            while (true) {
		stopAndStartIterations++;
                if (SimpleClient.scExitPending)
                    return false;
                if (sc.charOid == -1) {
		    // player logout
		    return true;
		}
                encacheDirLocOrient();
                int nextPeriodMS = 0;
                if (playerActive) {
                    if (parms.polygon != null)
                        nextPeriodMS = chooseBasedOnPolygon(parms, moveRand);
                    else {
                        nextPeriodMS = (int)(moveRand.nextFloat() * parms.maxActiveTime);
                        chooseRandomDirection(parms, moveRand);
                    }
                }
                else {
                    nextPeriodMS = (int)(moveRand.nextFloat() * (parms.maxIdleTime/3))+1;
                    nextPeriodMS += (int)(moveRand.nextFloat() * (parms.maxIdleTime/3))+1;
                    nextPeriodMS += (int)(moveRand.nextFloat() * (parms.maxIdleTime/3))+1;
                    playerDir = new MVVector(0,0,0);
                }
                setDirLocOrient(playerDir, playerPosition, playerOrientation);
                try {
                    Thread.sleep(nextPeriodMS);
                }
                catch(InterruptedException e) {
                    Log.exception(e);
                }
                playerActive = !playerActive;
            }
        }
        
        // Toss random numbers choosing X/Y coordinates of a
        // destination within the bounding box until the destination
        // is contained within the polygon, and the path from the
        // current position to the destination stays in the polygon.
        // Based on the vector from the current position to the
        // destination, set the direction, and return the number of
        // milliseconds till we reach the destination.
        protected int chooseBasedOnPolygon(BehaviorParms parms, Random moveRand) {
            Rectangle rect = parms.polygon.getBounds();
            int npoints = parms.polygon.npoints;
            int xpoints[] = parms.polygon.xpoints;
            int zpoints[] = parms.polygon.ypoints;
            int playerX = (int)playerPosition.getX();
            int playerZ = (int)playerPosition.getZ();
            // If the player is not currently in the polygon, we'll
            // omit the intersection test because it will always fail.
            boolean inPolygon = parms.polygon.contains(playerX, playerZ);
                
            for (int i=0; i<1000; i++) {
                choosePolygonIterations++;                
                int tryX = (int)(rect.getX() + (int)(moveRand.nextFloat() * Math.min(parms.moveMaximum * 1000f, rect.getWidth())));
                int tryZ = (int)(rect.getY() + (int)(moveRand.nextFloat() * Math.min(parms.moveMaximum * 1000f, rect.getHeight())));
                if (tryX == playerX && tryZ == playerZ)
                    continue;
                if (parms.polygon.contains(tryX, tryZ)) {
                    // Iterate over the sides of the polygon to
                    // determine if the path intersects one of the
                    // sides of the polygon
                    boolean intersects = false;
                    // If not inPolygon, don't do the side intersection tests
                    if (inPolygon) {
                        for (int j=0; j<npoints; j++) {
                            int k = (j == npoints - 1 ? 0 : j + 1);
                            if (intersectSegments(playerX, playerZ, tryX, tryZ, xpoints[j], zpoints[j], xpoints[k], zpoints[k])) {
                                intersects = true;
                                break;
                            }
                        }
                    }
                    if (intersects)
                        continue;
                    else {
                        if (i > 50)
                            Log.warn("PlayerClient.choosePolygonDestination: Finding the polygon destination took " + i + " iterations!");
                        // Calculate the direction, orientation and time
                        MVVector v = new MVVector((float)(tryX - playerX), 0f, (float)(tryZ - playerZ));
                        playerDir = new MVVector(v).normalize();
                        playerOrientation = Quaternion.fromVectorRotation(new MVVector(0,0,1), playerDir);
                        playerDir.multiply(parms.playerSpeed);
                        // Used by the main() test program
                        playerDestination = new MVVector(tryX, playerPosition.getY(), tryZ);
                        int time = (int)(v.length() * 1000f / parms.playerSpeed);
                        return time;
                    }
                }
            }
            Log.warn("PlayerClient.choosePolygonDestination: Didn't find the polygon destination in 1000 iterations!");
            // Return 100 ms, because we don't know what else to do.
            return 100;
        }

        protected boolean intersectSegments(int p1x, int p1z, int p2x, int p2z, int p3x, int p3z, int p4x, int p4z) {
            float den = ((p4z - p3z) * (p2x - p1x)) - ((p4x - p3x) * (p2z - p1z));
            float t1num = ((p4x - p3x) * (p1z - p3z)) - ((p4z - p3z) * (p1x - p3x));
            float t2num = ((p2x - p1x) * (p1z - p3z)) - ((p2z - p1z) * (p1x - p3x));

            if ( den == 0 ) {
                return false;
            }

            float t1 = t1num / den;
            float t2 = t2num / den;

            // note that we include the endpoint of the second line in the intersection
            // test, but not the endpoint of the first line.
            if ((t1 >= 0) && (t1 < 1) && (t2 >= 0) && (t2 <= 1))
                return true;
            else
                return false;
        }

        protected void chooseRandomDirection(BehaviorParms parms, Random moveRand) {
            playerYaw = -180f + 360f * moveRand.nextFloat();
            boolean back = setDirAndOrientation();
//             Log.info("Testing reverse: playerYaw " + playerYaw + ", back " + back + ", playerDir " + playerDir + ", playerPosition " + playerPosition);
            if (!back && twoDLength(playerPosition.minus(startingPosition)) > parms.moveMaximum * 1000f) {
                if (Log.loggingDebug)
                    Log.debug("Reversing direction because the character is more than " +
                        parms.moveMaximum + " meters from the starting position");
                playerYaw += 180f;
                setDirAndOrientation();
            }
        }
        
        // Returns true if the direction is toward the starting point
        protected boolean setDirAndOrientation() {
            playerOrientation = Quaternion.fromAngleAxisDegrees(playerYaw, yAxis);
            playerDir = Quaternion.multiply(playerOrientation, zAxis);
            if (parms.zeroY)
                playerDir.setY(0);
            playerDir.normalize();
            playerDir.multiply(parms.playerSpeed);
            MVVector p = playerPosition.minus(startingPosition);
            if (parms.zeroY)
                p.setY(0);
//             if (Log.loggingDebug)
//                 Log.debug("setDirAndOrientation: playerYaw " + playerYaw + ", orient " + playerOrientation + ", playerDir " + playerDir + 
//                     ", playerPosition " + playerPosition + ", startingPosition " + startingPosition + ", p " + p);
            return playerDir.dotProduct(p) < 0;
        }
        
        protected float twoDLength(MVVector v) {
            return (float) Math.sqrt(v.getX() * v.getX() + v.getZ() * v.getZ());
        }
        
        protected void positionPlayer(BehaviorParms parms, float xMult, float zMult, float rotateMult) {
            // reset acceleration zero
            playerAccel = new MVVector(xMult,0,zMult);
            playerYaw += rotateMult * parms.rotateSpeed * parms.moveInterval / 1000f;
            if (Log.loggingDebug)
                Log.debug("positionPlayer: xMult = " + xMult +
                    ", zMult = " + zMult + ", rotateMult = " + rotateMult +
                    ", playerYaw = " + playerYaw);
            playerOrientation = Quaternion.fromAngleAxisDegrees(playerYaw, new MVVector(0, 1000, 0));
            playerDir = Quaternion.multiply(playerOrientation, playerAccel);
            playerDir.normalize();
            playerDir.multiply(parms.playerSpeed);
            playerPosition.add(playerDir.times(parms.moveInterval / 1000f));
            if (parms.zeroY)
                playerPosition.setY(0);
        }
        
        protected void movePlayer(BehaviorParms parms, float xMult, float zMult, float rotateMult, boolean sendUpdate) {
            // Now handle movement and stuff

            positionPlayer(parms, xMult, zMult, rotateMult);
            if (Log.loggingDebug)
                Log.debug("PlayerClient: Moving player dir " + playerDir +
                    ", pos " + playerPosition + 
                    ", orient " + playerOrientation + ", update " + sendUpdate);
            if (sendUpdate)
                setDirLocOrient(playerDir, playerPosition, playerOrientation);
        }
        
        protected void setDirLocOrient(MVVector dir, MVVector loc, Quaternion orientation) {
            sc.userLock.lock();
            try {
		if (! sc.interpReady)
		    return;
                Point p = new Point(loc);
                if (parms.zeroY)
                    p.setY(0);
                sc.loc = p;
                sc.dir = new MVVector(dir);
                sc.orientation = (Quaternion)orientation.clone();
                sc.lastUpdated = System.currentTimeMillis();
                sc.locDirty = true;
                if (Log.loggingDebug)
                    Log.debug("setDirLocOrient: yaw " + playerYaw +
                        ", dir " + dir + ", loc " + sc.loc +
                        ", orient " + orientation);
            }
            finally {
                sc.userLock.unlock();
            }
        }
        
        protected void encacheDirLocOrient() {
            sc.interpolateNow();
            sc.userLock.lock();
            try {
		if (! sc.interpReady || sc.loc == null)
		    return;
                playerPosition = new MVVector(sc.loc);
                playerDir = (MVVector)sc.dir.clone();
                playerOrientation = (Quaternion)sc.orientation.clone();
            }
            finally {
                sc.userLock.unlock();
            }
        }
        
        protected void sendCommMessage(String s) {
            sc.userLock.lock();
            try {
		if (sc.charOid == -1)
		    return;
                ComEvent comEvent = new ComEvent();
                comEvent.setObjectOid(sc.charOid);
                comEvent.setMessage(s);
                comEvent.setChannelId(1);
                try {
//                     sc.proxyCon.send(comEvent.toBytes());
                }
                catch (Exception e) {
                    throw new RuntimeException(e.getMessage());
                }
            }
            finally {
                sc.userLock.unlock();
            }
        }

        // This is custom for Places instancing
        boolean instancePlayer(String instance)
        {
            Map<String, Serializable> properties =
                new HashMap<String, Serializable>();

            properties.put("ext_msg_subtype","proxy.DYNAMIC_INSTANCE");
            properties.put("command","instance");
            try {
                // Check if 'instance' is a number
                /* int accountId = */ Integer.valueOf(instance);
                properties.put("owner",instance);
            }
            catch (NumberFormatException e) {
                properties.put("instanceName",instance);
            }

            if (Log.loggingDebug)
                Log.debug("Instancing to owner="+properties.get("owner"));
                
            extensionWait.subType = "mv.SCENE_END";
            sc.sendExtensionMessage(null,properties);
            synchronized (extensionWait) {
                while (! extensionWait.subType.equals("")) {
                    try {
                        extensionWait.wait();
                    } catch (InterruptedException e) { /* ignore */ }
                }
            }
            if (Log.loggingDebug)
                Log.debug("Instance complete");
            return true;
        }

    } // end PlayerThread

    public class BehaviorParms {
        public long dirUpdateInterval = 100;    // time in ticks between direction updates to the server
        public long orientUpdateInterval = 100; // time in ticks between orientation updates to the server
        public int moveInterval = 100;          // Milliseconds between moves or rotates
        public int maxMoveTime = 3000;          // The maximum amount of time we'll continue to move in the same direction
        public int moveMaximum = 200;           // The maximum distance we'll move from the starting point, in meters
        public float forwardFraction = .5f;     // Move forward 50% of the time
        public float backFraction = .2f;        // Move back 20% of the time
        public float rotateFraction = .05f;     // Rotate 5% of the time 
        public float sideFraction = .25f;       // Move to one side or another 25% of the time
        public float playerSpeed = 7.0f * 1000; // The speed at which the player moves: 7 m/s
        public float rotateSpeed = 90.0f;       // Rotate speed in degree when user hits a rotate key
        public int playerUpdateInterval = 5000; // Send player data every 5 seconds even if it hasn't changed
        public int maxActiveTime = 5000;        // The player is active for a random time between 0 and 5 seconds
        public int maxIdleTime = 18000;         // The player is idle for a random time between 0 and 15 seconds
        public MVVector playerPosition = new MVVector(0,0,0);
        public Polygon polygon = null;          // If non-null, a polygon in which the player must be contained
        public String polygonRegionName = null;  // If non-null, a server region from which to extract containing polygon
        public boolean zeroY = false;           // Set player's Y-coord to zero
        public boolean randomStart = false;     // If true, the player starts out in a random location in the square or polygon.
        public int postLoginSleep = 0;          // Sleep just after login, ms
        public String instance = null;
    }
    
    protected void parseArgs(String argString, BehaviorParms parms) {
        String [] args = argString.split(" ");
        for (int i = 0; i < args.length; i++) {
            Log.info("parseArgs arg " + args[i] + (i + 1 < args.length ? ", value " + args[i+1] : ""));
            if (args[i].equals("--dir_update_interval")) {
                assert i + 1 < args.length;
                parms.dirUpdateInterval = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--orient_update_interval")) {
                assert i + 1 < args.length;
                parms.orientUpdateInterval = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--move_interval")) {
                assert i + 1 < args.length;
                parms.moveInterval = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--max_move_time")) {
                assert i + 1 < args.length;
                parms.maxMoveTime = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--move_maximum")) {
                assert i + 1 < args.length;
                parms.moveMaximum = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--forward_fraction")) {
                assert i + 1 < args.length;
                parms.forwardFraction = Float.parseFloat(args[++i]);
            } else if (args[i].equals("--back_fraction")) {
                assert i + 1 < args.length;
                parms.backFraction = Float.parseFloat(args[++i]);
            } else if (args[i].equals("--rotate_fraction")) {
                assert i + 1 < args.length;
                parms.rotateFraction = Float.parseFloat(args[++i]);
            } else if (args[i].equals("--side_fraction")) {
                assert i + 1 < args.length;
                parms.sideFraction = Float.parseFloat(args[++i]);
            } else if (args[i].equals("--player_speed")) {
                assert i + 1 < args.length;
                parms.playerSpeed = Float.parseFloat(args[++i]);
            } else if (args[i].equals("--rotate_speed")) {
                assert i + 1 < args.length;
                parms.rotateSpeed = Float.parseFloat(args[++i]);
            } else if (args[i].equals("--player_update_interval")) {
                assert i + 1 < args.length;
                parms.playerUpdateInterval = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--max_active_time")) {
                assert i + 1 < args.length;
                parms.maxActiveTime = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--max_idle_time")) {
                assert i + 1 < args.length;
                parms.maxIdleTime = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--zero_y")) {
                parms.zeroY = true;
            } else if (args[i].equals("--position")) {
                Log.info("Got --position; doing assert");
                assert i + 1 < args.length;
                Log.info("Done assert, now parsing MVVector");
                parms.playerPosition = MVVector.parsePoint(args[++i]);
                Log.info("parms.playerPosition = " + parms.playerPosition);
            } else if (args[i].equals("--polygon")) {
                assert i + 1 < args.length;
                String coordString = args[++i];
                parsePolygon(coordString, parms);
                Log.info("parms.polygon = " + logPolygon(parms.polygon));
            } else if (args[i].equals("--polygon_region")) {
                assert i + 1 < args.length;
                parms.polygonRegionName = args[++i];
            } else if (args[i].equals("--square_side")) {
                assert i + 1 < args.length;
                Integer side = Integer.parseInt(args[++i]);
                parms.polygon = new Polygon();
                Integer halfSide = side / 2;
                Point p = new Point(parms.playerPosition);
                int px = p.getX();
                int pz = p.getZ();
                parms.polygon.addPoint(px - halfSide, pz - halfSide);
                parms.polygon.addPoint(px + halfSide, pz - halfSide);
                parms.polygon.addPoint(px + halfSide, pz + halfSide);
                parms.polygon.addPoint(px - halfSide, pz + halfSide);
                Log.info("parms.polygon = " + logPolygon(parms.polygon));
            } else if (args[i].equals("--random_start")) {
                parms.randomStart = true;
            } else if (args[i].equals("--post_login_sleep")) {
                parms.postLoginSleep = Integer.parseInt(args[++i]);
            } else if (args[i].equals("--instance")) {
                parms.instance = args[++i];
            }
            
        }
    }

    protected static boolean parsePolygon(String coordString, BehaviorParms parms) {
        // The format of the arg is a sequence of
        // comma-separated x/z coordinate pairs with no spaces
        String [] coords = coordString.split(",");
        if ((coords.length & 1) != 0) {
            Log.error("Odd number of coordinates in --polygon: '" + coordString + "'");
            return false;
        }
        if (coords.length < 6) {
            Log.error("Fewer than 3 points in --polygon: '" + coordString + "'");
            return false;
        }
        parms.polygon = new Polygon();
        for (int j=0; j<coords.length; j+=2)
            parms.polygon.addPoint(Integer.parseInt(coords[j]), Integer.parseInt(coords[j + 1]));
        return true;
    }
    
    protected String logPolygon(Polygon polygon) {
        String s = "";
        for (int i=0; i<polygon.npoints; i++) {
            if (s != "")
                s += ", ";
            s += polygon.xpoints[i] + "," + polygon.ypoints[i];
        }
        return "(" + s + ")";
    }
    
    public PlayerClient(String argString) {
        sc = SimpleClient.getInstantiatingSimpleClient();
        if (sc == null) {
            Log.error("PlayerClient.PlayerClient: SimpleClient.getInstantiatingSimpleClient() returned null!");
            return;
        }
        if (sc.getExtraArgs() != null)
            argString += " " + sc.getExtraArgs();
        BehaviorParms parms = new BehaviorParms();
        Log.info("Parsing PlayerClient args from '" + argString + "'");
        parseArgs(argString, parms);
        Log.info("Creating playerThread");
        Thread playerThread = new Thread(new PlayerThread(parms, sc));
        playerThread.start();
    }

    public static MVVector calcRandomPosition(Polygon polygon)
    {
        Random moveRand = new Random(System.currentTimeMillis());
        Rectangle rect = polygon.getBounds();
        while (true) {
            int tryX = rect.x + (int)(moveRand.nextFloat() * rect.width);
            int tryZ = rect.y + (int)(moveRand.nextFloat() * rect.height);
            if (polygon.contains(tryX, tryZ)) {
                return new MVVector(tryX, 0, tryZ);
            }
        }
    }

    public static class PlayerClientStatsLogger implements Runnable {
        public void run() {
            while (!SimpleClient.scExitPending) {
                try {
                    Thread.sleep(1000);
                    Log.warn("PlayerClient iterations last second/total: " + 
                        "PlayerThread " + (playerThreadIterations - lastPlayerThreadIterations) + "/" + playerThreadIterations + 
                        ", StopAndStartThread " + (stopAndStartIterations - lastStopAndStartIterations) + "/" + stopAndStartIterations +
                        ", ChoosePolygonDest " + (choosePolygonIterations - lastChoosePolygonIterations) + "/" + choosePolygonIterations);
                    lastPlayerThreadIterations = playerThreadIterations;
                    lastStopAndStartIterations = stopAndStartIterations;
                    lastChoosePolygonIterations = choosePolygonIterations;
                }
                catch (Exception e) {
                    Log.exception("PlayerClient.PlayerClientStatsLogger.run thread interrupted", e);
                }
            }
        }
    }

    protected SimpleClient sc = null;
    protected static long playerThreadIterations = 0;
    protected static long stopAndStartIterations = 0;
    protected static long choosePolygonIterations = 0;
    protected static long lastPlayerThreadIterations = 0;
    protected static long lastStopAndStartIterations = 0;
    protected static long lastChoosePolygonIterations = 0;
    protected static Thread playerClientStatsLogger = null;

    public static void main(String[] args) {
        // Test to see how many times we can choose a point in the
        // polygon per second
        PlayerClient pc = new PlayerClient();
        pc.testPolygonMoves();
    }
    
    protected void testPolygonMoves() {
        BehaviorParms parms = new BehaviorParms();
        parsePolygon("54848,315218,53685,284092,-69679,284014,-69527,314322", parms);
        PlayerThread pt = new PlayerThread(parms, null);
        pt.playerPosition = new MVVector(63505f,71222f,300303f);
        Random moveRand = new Random();
        for (int iteration=0; iteration<5; iteration++) {
            int chooseCount = 0;
            long startTime = System.currentTimeMillis();
            while(true) {
                pt.chooseBasedOnPolygon(parms, moveRand);
                pt.playerPosition = pt.playerDestination;
                chooseCount++;
                long nowTime = System.currentTimeMillis();
                if ((nowTime - startTime) >= 1000L) {
                    System.out.println("Second " + iteration + ", chose " + chooseCount + " destinations in one second");
                    break;
                }
            }
        }
    }
}

