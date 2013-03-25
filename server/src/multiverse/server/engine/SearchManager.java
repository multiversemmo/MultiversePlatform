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

package multiverse.server.engine;

import java.util.Collection;
import java.util.Map;
import java.util.HashMap;
import java.util.LinkedList;

import multiverse.server.util.Log;
import multiverse.server.objects.ObjectType;
import multiverse.server.engine.Searchable;
import multiverse.server.messages.SearchMessageFilter;
import multiverse.server.messages.SearchMessage;
import multiverse.msgsys.*;

/** Object search framework.  You can search for objects matching your
criteria.
<p>
The platform provides the following searchable collections:
<ul>
<li>Markers - Search for markers in an instance by marker properties.  Use
SearchClause {@link multiverse.server.objects.Marker.Search Marker.Search}
<li>Regions - Search for regions in an instance by region properties.  Use
SearchClause {@link multiverse.server.objects.Region.Search Region.Search}
<li>Instances - Search for instances by instance properties.  Use
SearchClass {@link multiverse.server.engine.PropertySearch PropertySearch}
</ul>
<p>
Plugins can register searchable object collections using
{@link #registerSearchable registerSearchable()}.

*/
public class SearchManager
{
    private SearchManager() {
    }

    /** Search for matching objects and get selected information.
        Search occurs for a single ObjectType.  Information indicated
        in the 'SearchSelection' is returned for objects matching
        the 'SearchClause'.
        <p>
        The search clause is a SearchClass sub-class designed specifically
        for an object type (or class of object types).
        @param objectType Search for this object type
        @param searchClause Object matching criteria
        @param selection Information selection; only the indicated
                information will be returned.
        @return Collection of matching objects.
    */
    public static Collection searchObjects(ObjectType objectType,
        SearchClause searchClause, SearchSelection selection)
    {
        SearchMessage message = new SearchMessage(objectType,
            searchClause, selection);

        Collector collector = new Collector(message);
        return collector.getResults();
    }

    static class Collector implements ResponseCallback
    {
        public Collector(SearchMessage message)
        {
            searchMessage = message;
        }
        
        public Collection getResults()
        {
            int expectedResponses = Engine.getAgent().sendBroadcastRPC(searchMessage, this);
            synchronized (this) {
                responders += expectedResponses;
                while (responders != 0) {
                    try {
                        this.wait();
                    } catch (InterruptedException e) {
                    }
                }
            }
            return results;
        }

        public synchronized void handleResponse(ResponseMessage rr)
        {
            responders --;

            GenericResponseMessage response = (GenericResponseMessage) rr;
            
            Collection list = (Collection)response.getData();
            if (list != null)
                results.addAll(list);

            if (responders == 0)
                this.notify();
        }

        Collection results = new LinkedList();
        SearchMessage searchMessage;
        int responders = 0;
    }

    /** Register a new searchable object collection.
        @param objectType Collection object type.
        @param searchable Object search implementation.
    */
    public static void registerSearchable(ObjectType objectType,
        Searchable searchable)
    {
        SearchMessageFilter filter = new SearchMessageFilter(objectType);
        Engine.getAgent().createSubscription(filter,
            new SearchMessageCallback(searchable), MessageAgent.RESPONDER);
    }

    /** Register object match factory.  A MatcherFactory returns a
        Matcher object that works for the given search clause and
        object class.
        @param searchClauseClass A SearchClause sub-class.
        @param instanceClass Instance object class.
        @param matcherFactory Can return a Matcher object capable of
                running the SearchClause against the instance object.
    */
    public static void registerMatcher(Class searchClauseClass,
        Class instanceClass, MatcherFactory matcherFactory)
    {
        matchers.put(new MatcherKey(searchClauseClass,instanceClass),
            matcherFactory);
    }

    /** Get object matcher that can apply 'searchClause' to objects of
        'instanceClass'.
        @param searchClause The matching criteria.
        @param instanceClass Instance object class.
    */
    public static Matcher getMatcher(SearchClause searchClause,
        Class instanceClass)
    {
        MatcherFactory matcherFactory;
        matcherFactory = matchers.get(new MatcherKey(searchClause.getClass(),
            instanceClass));
        if (matcherFactory == null) {
            Log.error("runSearch: No matcher for "+searchClause.getClass()+" "+instanceClass);
            return null;
        }
        return matcherFactory.createMatcher(searchClause);
    }

    static class MatcherKey {
        public MatcherKey(Class qt, Class it)
        {
            queryType = qt;
            instanceType = it;
        }
        public Class queryType;
        public Class instanceType;
        public boolean equals(Object key)
        {
            return (((MatcherKey)key).queryType == queryType) &&
                (((MatcherKey)key).instanceType == instanceType);
        }
        public int hashCode()
        {
            return queryType.hashCode() + instanceType.hashCode();
        }
    }

    static class SearchMessageCallback implements MessageCallback
    {
        public SearchMessageCallback(Searchable searchable)
        {
            this.searchable = searchable;
        }

        public void handleMessage(Message msg, int flags)
        {
            SearchMessage message = (SearchMessage) msg;

            Collection result = null;
            try {
                result = searchable.runSearch(
                    message.getSearchClause(), message.getSearchSelection());
            }
            catch (Exception e) {
                Log.exception("runSearch failed", e);
            }

            Engine.getAgent().sendObjectResponse(message, result);
        }

        Searchable searchable;
    }

    static Map<MatcherKey,MatcherFactory> matchers =
        new HashMap<MatcherKey,MatcherFactory>();
}

