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

import java.util.Collection;
import java.util.List;
import java.util.Map;
import java.util.HashMap;
import java.util.LinkedList;
import java.io.Serializable;

import multiverse.server.engine.SearchClause;
import multiverse.server.engine.SearchSelection;
import multiverse.server.engine.Matcher;
import multiverse.server.engine.SearchManager;
import multiverse.server.engine.Searchable;


/** Generic searchable collection for registered entities.  Supports
PropertySearch matches against any registered entities of a given
ObjectType.  Searches the entity properties and returns selected
property values.  The object key is the entity oid.  Supports selection
options RESULT_KEYED and RESULT_KEY_ONLY.
@see multiverse.server.engine.SearchManager#registerSearchable
*/
public class EntitySearchable implements Searchable
{
    public EntitySearchable(ObjectType objectType)
    {
        this.objectType = objectType;
    }

    public Collection runSearch(SearchClause search,
        SearchSelection selection)
    {
        Matcher matcher = SearchManager.getMatcher(search, Entity.class);
        if (matcher == null)
            return null;
        List<Object> resultList = new LinkedList<Object>();
        synchronized (EntityManager.entitiesByNamespace) {
            for (Map<Long, Entity> namespaceEntities :
                        EntityManager.entitiesByNamespace.values()) {
                for (Map.Entry<Long, Entity> entry : namespaceEntities.entrySet()) {
                    boolean rc = false;
                    Entity entity = entry.getValue();
                    if (entity.getType() != objectType)
                        continue;
                    entity.lock();
                    if (entity.getTransientDataRef() != null)
                        rc = matcher.match(entity.getTransientDataRef());
                    if (! rc)
                        rc = matcher.match(entity.getPropertyMapRef());
                    if (rc) {
                        selectProperties(entry.getKey(),entity,
                            selection,resultList);
                    }
                    entity.unlock();
                }
            }
        }
        return resultList;
    }

    void selectProperties(Long oid, Entity entity,
        SearchSelection selection, List<Object> resultList)
    {
        if (selection.getResultOption() ==
                SearchSelection.RESULT_KEY_ONLY) {
            resultList.add(oid);
            return;
        }

        Map<String,Serializable> result;
        if (selection.getAllProperties()) {
            result = new HashMap<String,Serializable>(entity.getPropertyMapRef());
            if (entity.getTransientDataRef() != null)
                result.putAll(entity.getTransientDataRef());
        }
        else {
            result = new HashMap<String,Serializable>();
            for (String key : selection.getProperties()) {
                Serializable value = entity.getProperty(key);
                if (value != null)
                    result.put(key,value);
            }
        }

        if (selection.getResultOption() == SearchSelection.RESULT_KEYED)
            resultList.add(new SearchEntry(oid, result));
        else if (result.size() > 0)
            resultList.add(result);

    }

    private ObjectType objectType;
}

