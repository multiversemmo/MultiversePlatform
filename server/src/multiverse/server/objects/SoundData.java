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

import java.io.Serializable;
import java.util.Map;
import java.util.HashMap;
import java.io.*;

/**
 * Sound information.  A sound is a (file) name, a type (ambient or point),
 * and a set of properties.  The properties are key-value string pairs.
 * @see multiverse.server.plugins.WorldManagerClient.SoundMessage SoundMessage
 * 
 */
public class SoundData implements Serializable {

    public SoundData() {
        super();
    }
    
    public SoundData(String fileName, String type,
                 Map<String,String> properties) {
        super();
        setFileName(fileName);
        setType(type);
        setProperties(properties);
    }

    public String toString() {
        return "[SoundData: " +
        "FileName=" + getFileName() +
        ", Type=" + getType() +
        ", Properties=" + getProperties() +
        "]";        
    }

    public void setFileName(String fileName) {
        this.fileName = fileName;
    }
    public String getFileName() {
        return this.fileName;
    }
    
    public void setType(String type) {
        this.type = type;
    }
    public String getType() {
        return type;
    }
    public void setProperties(Map<String,String> properties) {
        this.properties = properties;
    }
    public Map<String,String> getProperties() {
        return properties;
    }
    public void addProperty(String key, String value) {
	if (properties == null)
	    properties = new HashMap<String,String>();
	properties.put(key,value);
    }
    
    private void writeObject(ObjectOutputStream out)
	throws IOException, ClassNotFoundException {
        out.defaultWriteObject();
    }
    
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        in.defaultReadObject();
    }
    
    private String fileName = null;
    private String type = null;
    private Map<String,String> properties = null;

    private static final long serialVersionUID = 1L;
}
