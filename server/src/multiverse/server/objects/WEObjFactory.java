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
import multiverse.server.plugins.*;
import multiverse.server.engine.*;
import java.util.*;
import java.lang.reflect.*;

/**
 * object factory for producing objects spawned from world editor spawn generators
 */
public class WEObjFactory extends ObjectFactory {
    /**
     * creates a new object
     */
    public WEObjFactory() {
	super();
    }

    public ObjectStub makeObject(SpawnData spawnData, long instanceOid,
        Point loc)
    {
 	String templateName = spawnData.getTemplateName();
	String behavNames = (String)spawnData.getProperty("Behaviors");
        if (Log.loggingDebug)
            Log.debug("WEObjFactory.makeObject: templateName="+templateName+
                " instanceOid="+instanceOid+" Behaviors="+behavNames +
                " propsize="+spawnData.getPropertyMap().size());
        ObjectStub obj = MobManagerPlugin.createObject(templateName,
            spawnData.getInstanceOid(), loc, spawnData.getOrientation());
	if (behavNames != null) {
	    for (String behavName : behavNames.split(",")) {
		try {
                    behavName = behavName.trim();
                    if (behavName.length() == 0)
                        continue;
		    Class behavClass = behavClassMap.get(behavName);
                    if (behavClass == null) {
                        Log.error("WEObjFactory.makeObject: unknown behavior="+
                            behavName + ", templateName="+templateName+
                            " instanceOid="+instanceOid+
                            " Behaviors="+behavNames);
                        continue;
                    }
		    Constructor<Behavior> constructor =
                        behavClass.getConstructor(constructorArgs);
                    if (constructor == null) {
                        Log.error("WEObjFactory.makeObject: missing constructor with signature (SpawnData) on class "+behavClass);
                        continue;
                    }
		    Object[] args = { spawnData };
		    Behavior behav = constructor.newInstance(args);
		    obj.addBehavior(behav);
		} catch (Exception e) {
		    throw new MVRuntimeException("can't create behavior", e);
		}
	    }
	}
	return obj;
    }

    public static void registerBehaviorClass(String name, String className) {
	try {
	    Class<Behavior> behavClass = (Class<Behavior>)Class.forName(className);
	    behavClassMap.put(name, behavClass);
	}
	catch (ClassNotFoundException e) {
	    throw new MVRuntimeException("behavior class not found", e);
	}
    }

    protected static Map<String, Class<Behavior>> behavClassMap = new HashMap<String, Class<Behavior>>();
    private final static Class[] constructorArgs = { SpawnData.class };
    protected SpawnData spawnData = null;
}
