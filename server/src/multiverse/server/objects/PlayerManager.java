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

import java.util.*;
import multiverse.server.network.ClientConnection;
import multiverse.server.messages.PerceptionMessage.ObjectNote;
import multiverse.server.util.SquareQueue;
import multiverse.server.util.Log;
import multiverse.server.engine.Event;
import multiverse.server.engine.BasicWorldNode;
import multiverse.server.events.AuthorizedLoginEvent;
import multiverse.server.events.DirLocOrientEvent;


public class PlayerManager
{
    public PlayerManager()
    {
    }

    public synchronized boolean addPlayer(Player player)
    {
        if (players.containsKey(player.getOid()))
            return false;
        players.put(player.getOid(), player);
        conMap.put(player.getConnection(), player);
        if (players.size() > peakPlayerCount)
            peakPlayerCount = players.size();
        return true;
    }

    public synchronized Player getPlayer(long oid)
    {
        return players.get(oid);
    }

    public synchronized Player getPlayer(ClientConnection conn)
    {
        return conMap.get(conn);
    }

    public synchronized Player removePlayer(long oid)
    {
        Player player = players.remove(oid);
        if (player == null)
            return null;

        conMap.remove(player.getConnection());

        Iterator<Map.Entry<Long,List<Player>>> iterator;
        iterator = perception.entrySet().iterator();
        while (iterator.hasNext()) {
            Map.Entry<Long,List<Player>> entry = iterator.next();
            entry.getValue().remove(player);
            if (entry.getValue().size() == 0)
                iterator.remove();
        }

        return player;
    }

    public synchronized int getPlayerCount()
    {
        return players.size();
    }

    public int getPeakPlayerCount()
    {
        return peakPlayerCount;
    }

    public int getLoginCount()
    {
        return loginCount;
    }

    public int getLogoutCount()
    {
        return logoutCount;
    }

    public synchronized int getLoginSeconds()
    {
        int seconds = 0;
        long now = System.currentTimeMillis();
        for (Player player : players.values()) {
            if (player.getStatus() == Player.STATUS_LOGIN_OK)
                seconds += (now - player.getLoginTime())/1000;
        }
        return seconds;
    }

    public synchronized void getPlayers(Collection<Player> pp)
    {
        pp.addAll(players.values());
    }

    public synchronized void addPerception(long playerOid, long subjectOid)
    {
        Player player = getPlayer(playerOid);
        if (player == null)
            return;
        List<Player> perceivers = perception.get(subjectOid);
        if (perceivers == null) {
            perceivers = new LinkedList<Player>();
            perception.put(subjectOid, perceivers);
        }
        perceivers.add(player);
    }

    public synchronized void removePerception(long playerOid, long subjectOid)
    {
        Player player = getPlayer(playerOid);
        if (player == null)
            return;
        List<Player> perceivers = perception.get(subjectOid);
        if (perceivers == null) {
            Log.error("removePerception: playerOid="+playerOid+
                " duplicate lost oid="+subjectOid);
            return;
        }
        if (perceivers.remove(player)) {
            if (perceivers.size() == 0) {
                perception.remove(subjectOid);
            }
        }
        else {
            Log.error("removePerception: playerOid="+playerOid+
                " does not perceive oid="+subjectOid);
        }
    }

    public synchronized void addPerception(Player player,
        Collection<ObjectNote> objectNotes,
        List<Long> newSubjects)
    {
        for (ObjectNote objectNote : objectNotes) {
            List<Player> perceivers = perception.get(objectNote.getSubject());
            if (perceivers == null) {
                perceivers = new LinkedList<Player>();
                perception.put(objectNote.getSubject(), perceivers);
                newSubjects.add(objectNote.getSubject());
            }
            if (perceivers.contains(player))
                Log.error("addPerception: playerOid="+player.getOid()+
                    " already perceives oid="+objectNote.getSubject());
            perceivers.add(player);
        }
    }

    public synchronized void removePerception(Player player,
        Collection<ObjectNote> objectNotes,
        List<Long> deleteSubjects)
    {
        for (ObjectNote objectNote : objectNotes) {
            List<Player> perceivers = perception.get(objectNote.getSubject());
            if (perceivers == null) {
                Log.error("removePerception: playerOid="+player.getOid()+
                    " duplicate lost oid="+objectNote.getSubject());
                continue;
            }
            if (perceivers.remove(player)) {
                if (perceivers.size() == 0) {
                    perception.remove(objectNote.getSubject());
                    deleteSubjects.add(objectNote.getSubject());
                }
            }
            else {
                Log.error("removePerception: playerOid="+player.getOid()+
                    " does not perceive oid="+objectNote.getSubject());
            }
        }
    }

    public synchronized List<Player> getPerceivers(long subjectOid)
    {
        List<Player> perceivers = perception.get(subjectOid);
        if (perceivers == null)
            return null;
        return new ArrayList<Player>(perceivers);
    }

    public void processEvent(Player player, Event event,
                SquareQueue<Player,Event> eventQQ)
    {
        long now = System.currentTimeMillis();
        event.setEnqueueTime(now);
        synchronized (this) {
            if (Log.loggingDebug)
                Log.debug("processEvent player "+player+" "+
                        event.getClass().getName());

            if (player == null)  {
                if (! (event instanceof AuthorizedLoginEvent))
                    return;
            }
            else if (player.getStatus() == Player.STATUS_LOGIN_PENDING) {
                if (player.getDeferredEvents() == null) {
                    player.setDeferredEvents(new LinkedList<Event>());
                }
                player.getDeferredEvents().add(event);
                return;
            }
            else if (player.getStatus() == Player.STATUS_LOGOUT) {
                // player is logging out, drop the event
                return;
            }
            else if (event instanceof DirLocOrientEvent) {
                DirLocOrientEvent dloEvent = (DirLocOrientEvent) event;
                BasicWorldNode wnode =
                    new BasicWorldNode(0L, dloEvent.getDir(), dloEvent.getLoc(),
                        dloEvent.getQuaternion());
                Log.debug("DLO "+wnode);
                if (! player.lastLocUpdate.equals(wnode)) {
                    player.setLastActivityTime(now);
                    player.lastLocUpdate = wnode;
                }
                else
                    player.setLastContactTime(now);
            }
            else
                player.setLastActivityTime(now);
        }
        eventQQ.insert(player, event);
    }

    public synchronized void loginComplete(Player player,
                SquareQueue<Player,Event> eventQQ)
    {
        if (player.getStatus() == Player.STATUS_LOGOUT)
            return;
        player.setStatus(Player.STATUS_LOGIN_OK);
        player.setLoginTime(System.currentTimeMillis());
        player.setLastActivityTime(player.getLoginTime());
        if (player.getDeferredEvents() != null) {
            for (Event event : player.getDeferredEvents()) {
                eventQQ.insert(player,event);
            }
            player.setDeferredEvents(null);
        }
        loginCount++;
    }

    public synchronized boolean logout(Player player)
    {
        Player existingPlayer = players.get(player.getOid());
        if (existingPlayer == null) {
            Log.error("PlayerManager.logout: player not found: player="+
                player);
            return false;
        }

        if (existingPlayer != player) {
            Log.error("PlayerManager.logout: player instance mis-match");
            return false;
        }

        if (player.getStatus() == Player.STATUS_LOGOUT)
            return false;

        player.setStatus(Player.STATUS_LOGOUT);
        logoutCount++;

        return true;
    }

    public synchronized List<Player> getTimedoutPlayers(long activityTimeoutMS,
        long contactTimeoutMS)
    {
        long now = System.currentTimeMillis();
        List<Player> timedout = new ArrayList<Player>(10);
        for (Player player : players.values()) {
            if (player.getStatus() == Player.STATUS_LOGIN_OK &&
                    ( (now - player.getLastContactTime()) > contactTimeoutMS ||
                    (now - player.getLastActivityTime()) > activityTimeoutMS) )
                timedout.add(player);
        }
        return timedout;
    }

    private Map<Long,Player> players = new HashMap<Long,Player>();

    private Map<ClientConnection,Player> conMap = new HashMap<ClientConnection,Player>();

    //## consider HashSet<Player> to improve remove performance
    private Map<Long,List<Player>> perception = new HashMap<Long,List<Player>>();

    private int peakPlayerCount = 0;
    private int loginCount = 0;
    private int logoutCount = 0;
}


