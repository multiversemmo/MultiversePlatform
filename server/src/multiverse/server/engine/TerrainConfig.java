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

import java.io.Serializable;

import multiverse.server.messages.ClientMessage;
import multiverse.server.network.MVByteBuffer;

/**
 * terrain configuration, can be either xml string or filename reference
 * @author cedeno
 *
 */
public class TerrainConfig implements Serializable, ClientMessage {
    public TerrainConfig() {
        super();
    }
    
    public String toString() {
        if (getConfigType() == configTypeFILE)
            return "[TerrainConfig type=" + getConfigType() + " file="+getConfigData() + "]";
        else if (getConfigType() == configTypeXMLSTRING)
            return "[TerrainConfig type=" + getConfigType() +
            " size=" + ((getConfigData() == null)?-1:getConfigData().length()) + "]";
        else
            return "[TerrainConfig null]";
    }
    
    public void setConfigType(String type) {
        this.configType = type;
    }
    public String getConfigType() {
        return configType;
    }
    
    public String getConfigData() {
        return configData;
    }
    public void setConfigData(String configData) {
        this.configData = configData;
    }
    
    public MVByteBuffer toBuffer() {
        MVByteBuffer buf = new MVByteBuffer(500);
        buf.putLong(0); 
        buf.putInt(66);
        buf.putString(getConfigType());
        buf.putString(getConfigData());
        buf.flip();
        return buf;
    }
    
    private String configType;
    private String configData;

    public static final String configTypeFILE = "file";
    public static final String configTypeXMLSTRING = "xmlstring";
    
    private static final long serialVersionUID = 1L;
}
