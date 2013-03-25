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

import java.io.*;
import java.util.Map;
import java.util.HashMap;


/** Message with properties and data object.
*/
public class GenericMessage extends Message
{
    public GenericMessage() {
    }

    /** Create message of the given message type.
    */
    public GenericMessage(MessageType msgType) {
        super(msgType);
    }

    /** Get property value. */
    public Serializable getProperty(String key) {
        if (properties == null)
            return null;
        return properties.get(key);
    }

    /** Set property value. */
    public void setProperty(String key, Serializable value)
    {
        if (properties == null)
            properties = new HashMap<String, Serializable>();
        properties.put(key, value);
    }

    /** Get property map. */
    public Map<String,Serializable> getProperties()
    {
        return properties;
    }

    /** Set property map.  The property map is not copied. */
    public void setProperties(Map<String,Serializable> props)
    {
        properties = props;
    }

    /** Add properties. */
    public void addProperties(Map<String,Serializable> props)
    {
        properties.putAll(props);
    }

    /** Get data object. */
    public Serializable getData()
    {
        return this.data;
    }

    /** Set data object. */
    public void setData(Serializable data)
    {
        this.data = data;
    }

    protected Serializable data;
    protected Map<String, Serializable> properties;
    
    private static final long serialVersionUID = 1L;
}

