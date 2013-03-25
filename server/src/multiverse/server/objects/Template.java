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

import multiverse.server.util.*;
import multiverse.server.engine.Namespace;
import java.util.*;
import java.io.*;

/**
 * used to generate an entity
 */
public class Template extends NamedPropertyClass implements Cloneable {
    public Template() {
    }

    public Template(String name) {
	setName(name);
    }

    public String getType() {
	return "Template";
    }
    
    public String toString() {
        String s = "[Template: name=" + getName() + " ";
        lock.lock();
        try {
            for (Map.Entry<Namespace, Map<String, Serializable>> entry : propMap.entrySet()) {
                Namespace ns = entry.getKey();
                Map<String, Serializable> subMap = entry.getValue();
                for (Map.Entry<String, Serializable> sEntry : subMap.entrySet()) {
                    String key = sEntry.getKey();
                    Serializable val = sEntry.getValue();
                    s += "(ns=" + ns.getName() + ", key=" + key + ", val=" + val + ")";
                }
            }
            return s;
        }
        finally {
            lock.unlock();
        }
    }
    
    public Object clone() throws CloneNotSupportedException {
        lock.lock();
        try {
            Template res = (Template) super.clone();

            // copy the map
            res.propMap = new HashMap<Namespace, Map<String, Serializable>>();
            for (Map.Entry<Namespace, Map<String, Serializable>> entry : propMap
                    .entrySet()) {
                res.propMap.put(entry.getKey(), new HashMap<String, Serializable>(
                        entry.getValue()));
            }
            return res;
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * adds the key and value, within the passed in namespace.
     * keys have to be unique within a given namespace.
     */
    public void put(Namespace namespace, String key, Serializable value) {
        lock.lock();
        try {
            Map<String,Serializable> subMap = propMap.get(namespace);
            if (subMap == null) {
                // make one
                subMap = new HashMap<String,Serializable>();
                propMap.put(namespace, subMap);
            }
            subMap.put(key, value);
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * the old overloading that uses the string name of the namespace.
     */
    public void put(String namespaceString, String key, Serializable value) {
        put(Namespace.intern(namespaceString), key, value);
    }
    
    public Serializable get(Namespace namespace, String key) {
        lock.lock();
        try {
            Map<String,Serializable> subMap = propMap.get(namespace);
            if (subMap == null) {
                return null;
            }
            return subMap.get(key);
        }
        finally {
            lock.unlock();
        }
    }
    
    public Set<Namespace> getNamespaces() {
        lock.lock();
        try {
            Set<Namespace> ns = new HashSet<Namespace>();
            for (Namespace namespace : propMap.keySet())
                ns.add(namespace);
            return ns;
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * returns a copy of the submap, restricted to the namespace
     */
    public Map<String, Serializable> getSubMap(Namespace namespace) {
        lock.lock();
        try {
            Map<String, Serializable> subMap = propMap.get(namespace);
            if (subMap != null)
                return new HashMap<String, Serializable>(subMap);
            else
                return null;
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * returns a template with only the passed in namespace parameters
     */
    public Template restrict(Namespace namespace) {
        lock.lock();
        try {
            Template t = new Template(this.getName());
            
            // returns a copy
            Map<String,Serializable> subMap = this.getSubMap(namespace);

            t.propMap.put(namespace, subMap);
            return t;
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * this "merges" the override template with the current template,
     * and returns the result.  this template is not modified.
     */
    public Template merge(Template overrideTemplate) {
        Template newTempl;
        try {
            newTempl = (Template) this.clone();
        } catch (CloneNotSupportedException e1) {
            throw new RuntimeException("merge", e1);
        }
        
        for (Namespace namespace : overrideTemplate.getNamespaces()) {
            Map<String,Serializable> subMap = overrideTemplate.getSubMap(namespace);
            for (Map.Entry<String, Serializable> entry : subMap.entrySet()) {
                String key = entry.getKey();
                Serializable val = entry.getValue();
                newTempl.put(namespace, key, val);
            }
        }
        return newTempl;
    }
    
    /**
     * templates might not be stored in the database, so we
     * check equality based on the template's name
     */
    public boolean equals(Serializable other) {
	if (! (other instanceof Template)) {
	    return false;
	}
	Template oTempl = (Template) other;
	if (this.getName() == null) {
	    return false;
	}
	return this.getName().equals(oTempl.getName());
    }
    
    public int hashCode() {
	if (this.getName() == null) {
	    throw new RuntimeException("hashCode fails for null name");
	}
	return this.getName().hashCode();
    }

    public Entity generate() {
        throw new MVRuntimeException("generate not implemented");
    }

    private Map<Namespace, Map<String, Serializable>> propMap = new HashMap<Namespace, Map<String, Serializable>>();

    private static final long serialVersionUID = 1L;
}
