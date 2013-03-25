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

package multiverse.msgsys;

import java.util.*;

/** Collection of filters with optimized message matching.  Implement a
sub-class to provide custom optimization for certain kinds of filters.
If a filter does not provide a FilterTable, then a {@link DefaultFilterTable}
is used.
<p>
A filter table is used by {@link MessageAgent} to collect filters
together so message matching can be optimized.  Filters are placed
into one of four filter table instances:
<li> send: filters matched when agent sends a message
<li> receive: filter matched when message received from another agent
<li> responder send: filters matched when agent sends an RPC
<li> responder receive: filters matched when RPC received from another agent
<p>
Each {@link Filter} instance can provide FilterTables to use.  If
none are provided (they return 'null'), then a {@link DefaultFilterTable}
is used.
*/
public abstract class FilterTable
{
    /** Add subscription to filter table.
    */
    public abstract void addFilter(Subscription sub, Object object);

    /** Remove subscription from filter table.
    */
    public abstract void removeFilter(Subscription sub, Object object);

    /** Match message against filters.
        @param message Message to match.
        @param matches Will be populated with subscription's associated
        objects.
        @param triggers Will be populated with subscriptions whose
        triggers should be run.
        @return Number of unique values in {@code matches}.
    */
    public abstract int match(Message message, Set<Object> matches,
        List<Subscription> triggers);

}

