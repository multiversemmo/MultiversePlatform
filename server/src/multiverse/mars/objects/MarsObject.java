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

package multiverse.mars.objects;

import multiverse.server.objects.*;
import multiverse.mars.core.*;
import multiverse.server.util.*;
import java.util.*;

public class MarsObject extends MVObject {

    public MarsObject() {
    }
    
    public MarsObject(Long oid) {
        super(oid);
    }

    /**
     * Checks if the object is a MARS object and if it is, returns the reference
     * as a MarsObject.
     * @return MarsObject     
     */
    public static MarsObject convert(Entity obj) {
        if (! (obj instanceof MarsObject)) {
            throw new MVRuntimeException("MarsObject.convert: obj is not a marsobject: " + obj);
        }
        return (MarsObject) obj;
    }

    public void setTemplateName(String templateName) {
        this.templateName = templateName;
    }
    public String getTemplateName() {
        return templateName;
    }
    protected String templateName = null;
    
    /**
     * Base display context - the inventory plugin adds soft/hard attachments
     * to this base mesh.
     */
    public DisplayContext baseDC() {
        return (DisplayContext) getProperty(baseDCKey);
    }
    public void baseDC(DisplayContext dc) {
        setProperty(baseDCKey, dc);
    }
    public static String baseDCKey = "marsobj.basedc";

    public DCMap dcMap() {
        lock.lock();
        try {
            DCMap map = (DCMap)getProperty(dcMapKey);
            if (map == null) {
                map = new DCMap();
                dcMap(map);
            }
            return map;
        }
        finally {
            lock.unlock();
        }
    }
    public void dcMap(DCMap dcMap) {
        setProperty(dcMapKey, dcMap);
    }
    public static String dcMapKey = multiverse.server.plugins.InventoryClient.TEMPL_DCMAP;

    // helper method
    public void addDCMapping(DisplayContext base, DisplayContext target) {
        DCMap dcMap = dcMap();
        dcMap.add(base, target);
    }
    /**
     * Returns a copy of the matching display context.
     */
    public DisplayContext getDCMapping(DisplayContext base) {
        return (DisplayContext)dcMap().get(base).clone();
    }    

    public int getDCV() {
        return 0;
    }

    public int getResistantPD() {
        return 0;
    }

    public int getPD() {
        return 0;
    }

    public void setStun(int stun) {
        lock.lock();
        try {
            this.stun = stun;
            if (currentStun > stun) {
                currentStun = stun;
            }
        }
        finally {
            lock.unlock();
        }
    }
    public int getStun() {
        return stun;
    }
    public void modifyStun(int delta) {
        lock.lock();
        try {
            int stun = getStun();
            setStun(stun + delta);
        }
        finally {
            lock.unlock();
        }
    }
    int stun;
        
    public void setBody(int body) {
        lock.lock();
        try {
            this.body = body;
            if (currentBody > body) {
                currentBody = body;
            }
        }
        finally {
            lock.unlock();
        }
    }
    public int getBody() {
        return body;
    }
    public void modifyBody(int delta) {
        lock.lock();
        try {
            int body = getBody();
            setBody(body + delta);
        }
        finally {
            lock.unlock();
        }
    }
    int body;

    public void setCurrentStun(int stun) {
        currentStun = stun;
    }
    public int getCurrentStun() {
        return currentStun;
    }
    public void modifyCurrentStun(int delta) {
        lock.lock();
        try {
            int stun = getCurrentStun();
            setCurrentStun(stun + delta);
        }
        finally {
            lock.unlock();
        }
    }
    int currentStun;
        
    public void setCurrentBody(int body) {
        currentBody = body;
    }
    public int getCurrentBody() {
        return currentBody;
    }
    public void modifyCurrentBody(int delta) {
        lock.lock();
        try {
            int body = getCurrentBody();
            setCurrentBody(body + delta);
        }
        finally {
            lock.unlock();
        }
    }
    int currentBody;

    /**
     * Sets whether this mob is attackable by a user.
     * Backends into MVObject.setState(MarsStates.attackable).
     */
    public void attackable(boolean val) {
        String stateName = MarsStates.Attackable.toString();
        setState(stateName, new BinaryState(stateName, val));
    }

    /**
     * Returns whether this mob is attackable by a user.
     * Backends into MVObject.getState(MarsStates.attackable).
     */
    public boolean attackable() {
        if (isDead()) {
            return false;
        }

        BinaryState attackable = 
            (BinaryState) getState(MarsStates.Attackable.toString());
        return ((attackable != null) && attackable.isSet());
    }

    public void isDead(boolean val) {
        setState(MarsStates.Dead.toString(), 
                 new BinaryState(MarsStates.Dead.toString(), val));
    }
    public boolean isDead() {
        BinaryState dead = (BinaryState)getState(MarsStates.Dead.toString());
        return ((dead!=null) && dead.isSet());
    }

    // sound - dont need to worry about the persistencedelegate for this one
    // because it uses the underlying property system
    public void setSound(String name, String value) {
        setProperty("mars.sound." + name, value);
    }
    public String getSound(String name) {
        return (String) getProperty("mars.sound." + name);
    }

    /**
     * @return the owner of this object, null if no owner.
     */
    public Long getOwnerOID() {
        return ownerOID;
    }
    public void setOwnerOID(long ownerOID) {
        this.ownerOID = new Long(ownerOID);
    }
    public void setOwnerOID(Long ownerOID) {
        this.ownerOID = ownerOID;
    }
    Long ownerOID = null;

    public void addAbility(MarsAbility ability, String category) {
        lock.lock();
        try {
            MarsAbility.Entry entry = new MarsAbility.Entry(ability, category);
            abilityMap.put(entry.getAbilityName(), entry);
        }
        finally {
            lock.unlock();
        }
    }
    public boolean hasAbility(MarsAbility ability) {
        lock.lock();
        try {
            MarsAbility.Entry entry = abilityMap.get(ability.getName());
            return (entry != null);
        }
        finally {
            lock.unlock();
        }
    }
    public boolean hasAbilities() {
        lock.lock();
        try {
            return (abilityMap.size() > 0);
        }
        finally {
            lock.unlock();
        }
    }
    public Set<MarsAbility.Entry> findAbilitiesByCategory(String category) {
        lock.lock();
        try {
            Set<MarsAbility.Entry> abilities = new HashSet<MarsAbility.Entry>();
            for (MarsAbility.Entry entry : abilityMap.values()) {
                if (entry.getCategory().equals(category)) {
                    abilities.add(entry);
                }
            }
            return abilities;
        }
        finally {
            lock.unlock();
        }
    }
    public Map<String, MarsAbility.Entry> getAbilityMap() {
        lock.lock();
        try {
            return new HashMap<String, MarsAbility.Entry>(abilityMap);
        }
        finally {
            lock.unlock();
        }
    }
    public void setAbilityMap(Map<String, MarsAbility.Entry> map) {
        lock.lock();
        try {
            abilityMap = new HashMap<String, MarsAbility.Entry>(map);
        }
        finally {
            lock.unlock();
        }
    }
    protected Map<String, MarsAbility.Entry> abilityMap = new HashMap<String, MarsAbility.Entry>();

    public void addCooldownState(Cooldown.State cd) {
        lock.lock();
        try {
            if (Log.loggingDebug)
                Log.debug("MarsObject.addCooldownState id=" + cd.getID());
            Cooldown.State oldcd = cooldownStateMap.get(cd.getID());
            if (oldcd != null)
                oldcd.cancel();
            cooldownStateMap.put(cd.getID(), cd);
        }
        finally {
            lock.unlock();
        }
    }
    public Cooldown.State removeCooldownState(Cooldown.State cd) {
        lock.lock();
        try {
            return cooldownStateMap.remove(cd.getID());
        }
        finally {
            lock.unlock();
        }
    }
    public Cooldown.State getCooldownState(String id) {
        lock.lock();
        try {
            return cooldownStateMap.get(id);
        }
        finally {
            lock.unlock();
        }
    }
    public Map<String, Cooldown.State> getCooldownStateMap() {
        lock.lock();
        try {
            return new HashMap<String, Cooldown.State>(cooldownStateMap);
        }
        finally {
            lock.unlock();
        }
    }
    public void setCooldownStateMap(Map<String, Cooldown.State> map) {
        lock.lock();
        try {
            cooldownStateMap = new HashMap<String, Cooldown.State>(map);
        }
        finally {
            lock.unlock();
        }
    }
    protected Map<String, Cooldown.State> cooldownStateMap = new HashMap<String, Cooldown.State>();

    public int getStunCounter() { return stunCounter; }
    protected void setStunCounter(int cnt) { stunCounter = cnt; }
    public void addStun() { stunCounter++; };
    public void removeStun() { stunCounter--; };
    public boolean isStunned() { return (stunCounter > 0); }
    private int stunCounter = 0;

    private static final long serialVersionUID = 1L;
}
