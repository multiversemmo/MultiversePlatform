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

/**
 * A read-only representation of a secure token. This object is
 * produced by the SecureTokenManager when decoding a token coming
 * from another server.
 *
 */
public final class SecureToken {
    SecureToken(SecureTokenSpec spec, byte version, long tokenId, long keyId,
                byte[] authenticator, boolean valid) {
	this.spec = spec;
        this.version = version;
        this.tokenId = tokenId;
        this.keyId = keyId;
        this.authenticator = authenticator;
        this.valid = valid;
    }

    protected final SecureTokenSpec spec;
    protected final byte version;
    protected final long tokenId;
    protected final long keyId;
    protected final byte[] authenticator;
    protected final boolean valid;

    public String toString() {
        return "[SecureToken: version=" + version + " tokenId=0x" + Long.toHexString(tokenId) +
            " keyId=0x" + Long.toHexString(keyId) + " valid=" + valid + " " + spec.toString() + "]";
    }

    public byte getType() {
	return spec.getType();
    }
    public String getIssuerId() {
	return spec.getIssuerId();
    }
    public long getExpiry() {
	return spec.getExpiry();
    }
    public Serializable getProperty(String key) {
	return spec.getProperty(key);
    }
    public byte getVersion() {
	return version;
    }
    public long getTokenId() {
	return tokenId;
    }
    public long getKeyId() {
	return keyId;
    }
    public boolean getValid() {
	return valid;
    }

    protected final static byte TOKEN_VERSION = 1;
}
