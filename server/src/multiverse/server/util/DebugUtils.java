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

package multiverse.server.util;

import java.util.Map;
import java.io.Serializable;

import multiverse.server.util.Log;

import multiverse.server.network.MVByteBuffer;

public class DebugUtils {
    
    public static String byteArrayToHexString(MVByteBuffer buf) {
        String bytes = byteArrayToHexString(buf.copyBytes());
        buf.rewind();
        return bytes;
    }
    
    public static String byteArrayToHexString(byte in[]) {
        byte ch = 0x00;
        int i = 0; 
        if (in == null || in.length <= 0)
            return null;
        String pseudo[] = {"0", "1", "2",
                           "3", "4", "5", "6", "7", "8",
                           "9", "A", "B", "C", "D", "E",
                           "F"};
        StringBuffer out = new StringBuffer(in.length * 2);
        StringBuffer chars = new StringBuffer(in.length);
        while (i < in.length) {
            ch = (byte) (in[i] & 0xF0); // Strip off high nibble
            ch = (byte) (ch >>> 4); // shift the bits down
            ch = (byte) (ch & 0x0F); // must do this if high order bit is on!
            out.append(pseudo[ (int) ch]); // convert the nibble to a String Character
            ch = (byte) (in[i] & 0x0F); // Strip off low nibble 
            out.append(pseudo[ (int) ch]); // convert the nibble to a String Character
            if (in[i] >= 32 && in[i] <= 126)
                chars.append((char)in[i]);
            else
                chars.append("*");
            i++;
        }
        return new String(out) + " == " + new String(chars);
    } 
    
    public static void logDebugMap(Map<String, Serializable> map)
    {
        if (! Log.loggingDebug)
            return;
        Log.debug("PRINTMAP START");
        for (Map.Entry<String, Serializable> e : map.entrySet()) {
            Object key = e.getKey();
            Object val = e.getValue();
            if (Log.loggingDebug)
                Log.debug("entry: key=" + key.toString() + ", value="
                          + val.toString());
        }
        Log.debug("PRINTMAP END");
    }

    public static String mapToString(Map<String, Serializable> map)
    {
        if (map == null)
            return "null";
        String result = "[";
        for (Map.Entry<String, Serializable> entry : map.entrySet()) {
            if (result.length() > 1)
                result+= ",";
            result += entry.getKey() + "=" + entry.getValue();
        }
        result += "]";
        return result;
    }


}
