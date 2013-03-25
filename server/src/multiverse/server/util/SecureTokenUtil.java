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

import javax.crypto.SecretKey;
import javax.crypto.KeyGenerator;
import java.security.KeyPair;
import java.security.KeyPairGenerator;
import java.security.PublicKey;
import java.security.PrivateKey;
import java.security.NoSuchAlgorithmException;
import multiverse.server.network.MVByteBuffer;

/**
 * Some utility functions for key generation and encoding. They are
 * all static and objects of this class should never be instantiated.
 *
 */
public class SecureTokenUtil {
    private SecureTokenUtil() {
    }

    /**
     * Generate a key to be used as the domain key. This should only
     * be called by the domain server.
     *
     */
    public static SecretKey generateDomainKey() {
        KeyGenerator keyGen;
        try {
            keyGen = KeyGenerator.getInstance("HmacSHA1");
        }
        catch (NoSuchAlgorithmException e) {
            Log.exception("SecureTokenManager.generateDomainKey: could not get KeyGenerator instance.", e);
            throw new RuntimeException(e);
        }

        keyGen.init(160);
        SecretKey key = keyGen.generateKey();
        return key;
    }

    /**
     * Encode a key as a byte array for transmission over a network
     * connection. This is used by the domain server to prepare the
     * domain key for transmission to other servers.
     *
     */
    public static byte[] encodeDomainKey(long keyId, SecretKey key) {
        MVByteBuffer buf = new MVByteBuffer(256);
        buf.putLong(keyId);
        buf.putString(key.getAlgorithm());
        byte[] encodedKey = key.getEncoded();
        buf.putBytes(encodedKey, 0, encodedKey.length);
        buf.flip();
        byte[] outKey = new byte[buf.remaining()];
        buf.getBytes(outKey, 0, outKey.length);
        return outKey;
    }

    /**
     * Generate a public/private keypair to be used as a master key.
     *
     */
    public static KeyPair generateMasterKeyPair() {
        KeyPairGenerator keyGen;
        try {
            keyGen = KeyPairGenerator.getInstance("DSA");
        }
        catch (NoSuchAlgorithmException e) {
            Log.exception("SecureTokenManager.generateMasterKeyPair: could not get DSA KeyPairGenerator instance.", e);
            throw new RuntimeException(e);
        }

        keyGen.initialize(1024);
        KeyPair pair = keyGen.generateKeyPair();
        return pair;
    }

    /**
     * Encode a master private key for storage. This can be base64
     * encoded and placed in a property file.
     *
     */
    public static byte[] encodeMasterPrivateKey(long keyId, PrivateKey privKey) {
        MVByteBuffer buf = new MVByteBuffer(1024);
        buf.putLong(keyId);
        buf.putString(privKey.getAlgorithm());
        byte[] encodedKey = privKey.getEncoded();
        buf.putBytes(encodedKey, 0, encodedKey.length);
        buf.flip();
        byte[] outKey = new byte[buf.remaining()];
        buf.getBytes(outKey, 0, outKey.length);
        return outKey;
    }

    /**
     * Encode a master public key for storage. This can be base64
     * encoded and placed in a property file.
     *
     */
    public static byte[] encodeMasterPublicKey(long keyId, PublicKey pubKey) {
        MVByteBuffer buf = new MVByteBuffer(1024);
        buf.putLong(keyId);
        buf.putString(pubKey.getAlgorithm());
        byte[] encodedKey = pubKey.getEncoded();
        buf.putBytes(encodedKey, 0, encodedKey.length);
        buf.flip();
        byte[] outKey = new byte[buf.remaining()];
        buf.getBytes(outKey, 0, outKey.length);
        return outKey;
    }

    /**
     * Generate a new master key with the id specified in args
     */
    public static void main(String args[]) {
        Log.init();
        if (args.length != 1) {
            System.exit(-1);
        }
        Integer keyId = Integer.parseInt(args[0]);

        KeyPair pair = SecureTokenUtil.generateMasterKeyPair();
        PrivateKey priv = pair.getPrivate();
        PublicKey pub = pair.getPublic();

        System.out.println("master key id = " + keyId);
        System.out.println("");

        byte[] encodedPrivKey = SecureTokenUtil.encodeMasterPrivateKey(keyId, pair.getPrivate());
        System.out.println("encoded private key:");
        System.out.println(Base64.encodeBytes(encodedPrivKey));
        System.out.println("");

        byte[] encodedPubKey = SecureTokenUtil.encodeMasterPublicKey(keyId, pair.getPublic());
        System.out.println("encoded public key:");
        System.out.println(Base64.encodeBytes(encodedPubKey));
        System.out.println("");
        System.exit(0);
    }
}
