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

import java.util.List;
import java.util.LinkedList;
import java.util.Set;
import java.util.HashSet;
import java.util.Collection;
import multiverse.server.network.ClientConnection;
import multiverse.server.engine.Event;
import multiverse.server.engine.Namespace;
import multiverse.server.engine.EnginePlugin;
import multiverse.server.engine.BasicWorldNode;
import multiverse.server.util.*;
import multiverse.server.math.Point;
import multiverse.server.math.MVVector;
import multiverse.server.math.Quaternion;

/** Internal use only - used by ProxyPlugin
*/
public class Player
{
    public Player(long playerOid, ClientConnection conn)
    {
        oid = playerOid;
        connection = conn;
        lastLocUpdate = new BasicWorldNode();
        lastLocUpdate.setLoc(new Point(0,0,0));
        lastLocUpdate.setDir(new MVVector(0,0,0));
        lastLocUpdate.setOrientation(new Quaternion(0,0,0,0));
    }

    public String toString()
    {
        return "[oid="+oid+" name="+name+" status="+statusToString(status)+"]";
    }

    public String getName()
    {
        return name;
    }
    
    public void setName(String name) 
    {
        this.name = name;
    }
    
    public long getOid()
    {
        return oid;
    }

    public ClientConnection getConnection()
    {
        return connection;
    }

    public String getVersion()
    {
        return version;
    }

    public void setVersion(String vers)
    {
        version = vers;
    }

    public List<String> getCapabilities()
    {
        return capabilities;
    }

    public boolean hasCapability(String cap)
    {
        if (capabilities == null)
            return false;
        return capabilities.contains(cap);
    }

    public void setCapabilities(List<String> caps)
    {
        capabilities = caps;
    }

    public static final int STATUS_UNKNOWN = 0;
    public static final int STATUS_LOGIN_PENDING = 1;
    public static final int STATUS_LOGIN_OK = 2;
    public static final int STATUS_LOGOUT = 3;

    public int getStatus()
    {
        return status;
    }

    public void setStatus(int s)
    {
        status = s;
    }

    public static String statusToString(int s)
    {
        switch (s) {
        case STATUS_UNKNOWN: return "UNKNOWN";
        case STATUS_LOGIN_PENDING: return "LOGIN_PENDING";
        case STATUS_LOGIN_OK: return "OK";
        case STATUS_LOGOUT: return "LOGOUT";
        default: return s+" (??)";
        }
    }

    public static final int LOAD_PENDING = 0;
    public static final int LOAD_COMPLETE = 1;

    public int getLoadingState()
    {
        return loadingState;
    }

    public void setLoadingState(int state)
    {
        loadingState = state;
    }

    public List<Event> getDeferredEvents()
    {
        return deferredEvents;
    }

    public void setDeferredEvents(List<Event> events)
    {
        deferredEvents = events;
    }

    public long getLoginTime()
    {
        return loginTime;
    }

    public void setLoginTime(long time_ms)
    {
        loginTime = time_ms;
    }

    public long getLastActivityTime()
    {
        return lastActivityTime;
    }

    public void setLastActivityTime(long time_ms)
    {
        lastActivityTime = time_ms;
        lastContactTime = time_ms;
    }

    public long getLastContactTime()
    {
        return lastContactTime;
    }

    public void setLastContactTime(long time_ms)
    {
        lastContactTime = time_ms;
    }

    synchronized public void updateIgnoredOids(List<Long> nowIgnored, List<Long> noLongerIgnored)
    {
        if (noLongerIgnored != null)
            ignoredOids.removeAll(noLongerIgnored);
        if (nowIgnored != null)
            ignoredOids.addAll(nowIgnored);
        setIgnoredOidsProperty();
    }
    
    synchronized public void setIgnoredOids(Collection<Long> newIgnoredOids)
    {
        initializeIgnoredOids(newIgnoredOids);
        setIgnoredOidsProperty();
    }

    synchronized public void setIgnoredOidsProperty() {
        EnginePlugin.setObjectProperty(oid, Namespace.WORLD_MANAGER, "ignored_oids", (HashSet<Long>)ignoredOids);
    }
    
    synchronized public boolean oidIgnored(long oid)
    {
    	return ignoredOids != null && ignoredOids.contains(oid);
    }
    
    synchronized public int ignoredOidCount()
    {
        return ignoredOids == null ? 0 : ignoredOids.size();
    }
    
    public void initializeIgnoredOids(Collection<Long> ignoredOids)
    {
        this.ignoredOids = new HashSet<Long>();
        if (ignoredOids != null)
            this.ignoredOids.addAll(ignoredOids);
    }
    
    synchronized public List<Long> getIgnoredOids()
    {
    	List<Long> oids = new LinkedList<Long>();
        if (ignoredOids == null)
            return oids;
    	oids.addAll(ignoredOids);
    	return oids;
    }

    private String name = "";
    private long oid;
    private ClientConnection connection;
    private String version;
    private List<String> capabilities;
    private int status;
    private int loadingState = LOAD_PENDING;
    private List<Event> deferredEvents;
    private long loginTime;
    private long lastActivityTime;
    private long lastContactTime;
    BasicWorldNode lastLocUpdate;
    private Set<Long> ignoredOids;

    protected static final Logger log = new Logger("Player");
}

