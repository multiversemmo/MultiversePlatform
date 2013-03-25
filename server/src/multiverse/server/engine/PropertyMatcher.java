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

import java.util.Map;
import java.util.Set;


public class PropertyMatcher implements Matcher
{
    public PropertyMatcher(SearchClause query)
    {
        queryProps = ((PropertySearch)query).getProperties();
    }

    public boolean match(Object object)
    {
        Map target = (Map) object;
        if (target == null) {
            if (queryProps.size() == 0)
                return true;
            else
                return false;
        }

        for (Map.Entry queryProp : (Set<Map.Entry>) queryProps.entrySet()) {
            Object queryKey = queryProp.getKey();
            Object queryValue = queryProp.getValue();
            Object targetValue = target.get(queryKey);
            if (targetValue == null) {
                if (! target.containsKey(queryKey) || queryValue != null)
                    return false;
            }
            if (queryValue == null)
                return false;
            if (! targetValue.equals(queryValue))
                return false;
        }
        return true;
    }

    public static class Factory implements MatcherFactory
    {
        public Factory()
        {
        }
        
        public Matcher createMatcher(SearchClause query)
        {
            return new PropertyMatcher(query);
        }
    }

    private Map queryProps;
}

