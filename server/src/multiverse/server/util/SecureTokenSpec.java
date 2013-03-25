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

import java.io.Serializable;
import java.util.Map;
import java.util.TreeMap;
import java.util.Date;
import java.text.DateFormat;

/**
 * A specification of the contents of a secure token. This object
 * is used to create by the SecureTokenManger to create a secure
 * token.
 *
 */
public final class SecureTokenSpec {
    public SecureTokenSpec(byte type, String issuerId, long expiry) {
	this.type = type;
	this.issuerId = issuerId;
	this.expiry = expiry;
    }
    public SecureTokenSpec(byte type, String issuerId, long expiry, Map<String, Serializable> properties) {
	this.type = type;
	this.issuerId = issuerId;
	this.expiry = expiry;
	this.properties.putAll(properties);
    }

    private final byte type;
    private final String issuerId;
    private final long expiry;
    private TreeMap<String, Serializable> properties = new TreeMap<String, Serializable>();

    public String toString() {
        String str = "type=" + type + " issuerId=" + issuerId + " expiry=<" +
            DateFormat.getInstance().format(new Date(expiry)) + "> props:";
        for (String key : properties.keySet()) {
            str += " " + key + ":" + properties.get(key).toString();
        }
        return str;
    }

    public byte getType() {
	return type;
    }
    public String getIssuerId() {
	return issuerId;
    }
    public long getExpiry() {
	return expiry;
    }
    public Serializable getProperty(String key) {
	return properties.get(key);
    }
    public void setProperty(String key, Serializable value) {
	properties.put(key, value);
    }

    public TreeMap<String, Serializable> getPropertyMap() {
        return properties;
    }

    public final static byte TOKEN_TYPE_MASTER = 1;
    public final static byte TOKEN_TYPE_DOMAIN = 2;
}
