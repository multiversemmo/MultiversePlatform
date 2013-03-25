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

import java.util.List;
import java.util.LinkedList;

/** Select information to return from an object search.  Information is
selected by bit-flags and/or property names.  The supported technique
varies with each Searchable collection.
<p>
In addition, the form of the search results can be specified.  By
default, searches should return a collection of matching objects.
Two additional options are supported:
<ul>
<li>{@link #RESULT_KEYED} - Result is a collection of {@link multiverse.server.objects.SearchEntry SearchEntry} objects.  The SearchEntry key is the object key or
name and the value is the object itself.
<li>{@link #RESULT_KEY_ONLY} - The result is a collection of object keys or names.
</ul>
How these options are supported, if at all, is defined by each searchable
collection.
*/
public class SearchSelection
{
    public SearchSelection()
    {
    }

    /** Select information by bit-flag.
    */
    public SearchSelection(long propFlags)
    {
        setPropFlags(propFlags);
    }

    /** Select information by bit-flag and format results according to
        given option.
    */
    public SearchSelection(long propFlags, int resultOption)
    {
        setPropFlags(propFlags);
        setResultOption(resultOption);
    }

    /** Select information by property names.
    */
    public SearchSelection(List<String> properties)
    {
        setProperties(properties);
    }

    /** Select information by property names and format results according to
        given option.
    */
    public SearchSelection(List<String> properties, int resultOption)
    {
        setProperties(properties);
        setResultOption(resultOption);
    }

    /** Format results as a collection of {@link multiverse.server.objects.SearchEntry SearchEntry} objects.
    */
    public static final int RESULT_KEYED = 1;

    /** Format results as a collection of object keys or names.
    */
    public static final int RESULT_KEY_ONLY = 2;

    /** Get the result formatting option. */
    public int getResultOption()
    {
        return resultOption;
    }

    /** Set the result formatting option.
        @param option One of {@link #RESULT_KEYED} or {@link #RESULT_KEY_ONLY}.
    */
    public void setResultOption(int option)
    {
        resultOption = option;
    }
    
    /** Get the selected property names.
    */
    public List<String> getProperties()
    {
        return properties;
    }

    /** Set the selected property names.
    */
    public void setProperties(List<String> props)
    {
        properties = props;
    }

    /** Add a selected property.
    */
    public void addProperty(String property)
    {
        if (properties == null)
            properties = new LinkedList<String>();
        properties.add(property);
    }

    /** Remove a selected property.
    */
    public void removeProperty(String property)
    {
        if (properties != null)
            properties.remove(property);
    }

    /** Return get-all-properties flag.
    */
    public boolean getAllProperties()
    {
        return selectAllProperties ;
    }

    /** Set get-all-properties flag.
    */
    public void setAllProperties(boolean selectAllProperties)
    {
        this.selectAllProperties = selectAllProperties;
    }

    /** Get property selection bitmask.
    */
    public long getPropFlags()
    {
        return propFlags;
    }

    /** Set property selection bitmask.
    */
    public void setPropFlags(long flags)
    {
        propFlags = flags;
    }

    /** Add property selection.
    */
    public void addPropFlag(long flag)
    {
        propFlags |= flag;
    }

    /** Remove property selection.
    */
    public void removePropFlag(long flag)
    {
        propFlags &= (~flag);
    }

    private int resultOption;
    private boolean selectAllProperties;
    private long propFlags;
    private List<String> properties;
}

