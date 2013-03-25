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

import java.util.Map;
import java.util.HashMap;
import java.util.Iterator;

import multiverse.server.util.Log;
import multiverse.server.plugins.InstanceClient;

public class InstanceTimeout
{
    public InstanceTimeout(int defaultTimeout)
    {
        this.defaultTimeout = defaultTimeout;
    }

    public void start()
    {
        (new Thread(new ThreadRun(), "InstanceTimeout")).start();
    }

    public int getDefaultTimeout()
    {
        return defaultTimeout;
    }

    public void setDefaultTimeout(int timeout)
    {
        defaultTimeout = timeout;
    }

    private class ThreadRun implements Runnable
    {
        public void run()
        {
            while (true) {
                try {
                    scanInstances();
                }
                catch (Exception e) {
                    Log.exception("InstanceTimeout", e);
                }
                try {
                    Thread.sleep((defaultTimeout*1000)/2);
                }
                catch (InterruptedException e) { /* ignore */ }
            }
        }
    }

    private void scanInstances()
    {
        Entity[] entities =
            (Entity[]) EntityManager.getAllEntitiesByNamespace(
                InstanceClient.NAMESPACE);

        long now = System.currentTimeMillis();
        for (Entity entity : entities) {
            Instance instance = (Instance) entity;
            if (readyForTimeout(instance)) {
                Long emptyTime = emptyInstances.get(instance.getOid());
                if (emptyTime == null) {
                    emptyInstances.put(instance.getOid(), now);
                }
                else if ((now - emptyTime)/1000 >= defaultTimeout) {
                    unloadInstance(instance);
                    now = System.currentTimeMillis();
                }
            }
        }

        Iterator<Long> iterator = emptyInstances.keySet().iterator();
        while (iterator.hasNext()) {
            Entity entity = EntityManager.getEntityByNamespace(
                iterator.next(), InstanceClient.NAMESPACE);
            if (entity == null)
                iterator.remove();
        }
    }

    private void unloadInstance(Instance instance)
    {
        if (instance.getState() == Instance.STATE_AVAILABLE) {
            Log.info("InstancePlugin: INSTANCE_TIMEOUT instanceOid="+instance.getOid()+
                " name="+instance.getName());
            InstanceClient.unloadInstance(instance.getOid());
            emptyInstances.remove(instance.getOid());
        }
    }

    public boolean readyForTimeout(Instance instance)
    {
        return instance.getPlayerPopulation() == 0;
    }
              
    private Map<Long,Long> emptyInstances = new HashMap<Long,Long>();
    private int defaultTimeout;
}


