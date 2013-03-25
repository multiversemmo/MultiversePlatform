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
import java.nio.BufferUnderflowException;
import java.util.Arrays;
import java.util.Set;
import java.util.Map;
import java.util.HashSet;
import java.util.HashMap;
import java.util.TreeMap;
import java.util.Date;
import java.text.DateFormat;
import java.security.PublicKey;
import java.security.PrivateKey;
import java.security.Signature;
import java.security.SignatureException;
import javax.crypto.SecretKey;
import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.security.KeyPair;
import java.security.KeyFactory;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.PKCS8EncodedKeySpec;
import java.security.spec.X509EncodedKeySpec;
import java.security.spec.EncodedKeySpec;
import multiverse.server.network.MVByteBuffer;

/**
 * The SecureTokenManager manages the creation and validation of
 * secure tokens. It keeps track of tokens that have been used,
 * and enforces single-use and expiration. It also manages the
 * keys used for token creation and validation.
 * <p>
 * To initialize the manager to decode master tokens, you must
 * supply at least one key to registerMasterPublicKey(). To
 * generate master tokens, you must use initMaster() to provide a
 * master private key capable of signing tokens. To process domain
 * tokens, initDomain() must be called with a domain key.
 * <p>
 * It is possible to call initDomain() to rekey the server. Old
 * keys will still be available for validating tokens, but new
 * tokens will only be created with the new key. There is not
 * currently any mechanism to expire old domain keys while a
 * server is running.
 * <p>
 * To generate a token, create a SecureTokenSpec object and pass
 * it to generateToken().
 * <p>
 * To import a token, pass the data to importToken(), then check
 * the valid flag to ensure it is a valid token.
 *
 */
public class SecureTokenManager {
    protected SecureTokenManager() {}

    protected static SecureTokenManager instance = null;
    public static SecureTokenManager getInstance() {
        if (instance == null) {
            instance = new SecureTokenManager();
        }
        return instance;
    }

    /**
     * import a serialized token, decoding it into a SecureToken
     * object and checking it for validity.
     * <p>
     * The caller must check the valid flag on the generated token
     * before doing anything with it.
     * <p>
     * A token may only be imported once. Attempting to import the
     * same encodedToken again will return a SecureToken object with
     * the valid flag set to false.
     *
     */
    public SecureToken importToken(byte[] encodedToken) {
        MVByteBuffer buf = new MVByteBuffer(encodedToken);
        byte version = 0;
        byte type = 0;
        String issuerId = null;
        long tokenId = 0;
        long keyId = 0;
        long expiry = 0;
        TreeMap<String, Serializable> properties = new TreeMap<String, Serializable>();
        byte[] authenticator = null;
        boolean valid = true;
        int authedLength = 0; // number of bytes before authenticator
        
        // We have no control over what data gets sent to us and we
        // don't want to crash if we get a misformatted token. So we
        // wrap this stuff in a try/catch block and log it if
        // something gets thrown.
        try {
            version = buf.getByte();
            type = buf.getByte();
            issuerId = buf.getString();
            tokenId = buf.getLong();
            keyId = buf.getLong();
            expiry = buf.getLong();
            properties = (TreeMap) buf.getEncodedObject();
            authedLength = buf.position();
            authenticator = new byte[buf.remaining()];
            buf.getBytes(authenticator, 0, authenticator.length);
        }
        catch (BufferUnderflowException e) {
            Log.exception("SecureTokenManager.importToken: caught exception when decoding token.", e);
            valid = false;
        }
        catch (RuntimeException e) {
            Log.exception("SecureTokenManager.importToken: caught exception when decoding token.", e);
            valid = false;
        }

        // Check version
        if (version != SecureToken.TOKEN_VERSION) {
            Log.error("SecureTokenManager.importToken: token version mismatch tokenId=0x" + Long.toHexString(tokenId) +
                      " version=" + version);
            valid = false;
        }

        // Check the expiry
        if (valid) {
            if (expiry <= System.currentTimeMillis()) {
                valid = false;
                Log.error("SecureTokenManager.importToken: token expired tokenId=0x" + Long.toHexString(tokenId) +
                          " expiry=<" + DateFormat.getInstance().format(new Date(expiry)) + ">");
            }
        }

        // Check if this token has already been used before doing the
        // expensive crypto, but don't mark it as used until the
        // authenticator has been validated.
        if (valid) {
            synchronized (this) {
                if (issuerAlreadyUsed(issuerId, tokenId)) {
                    valid = false;
                    Log.error("SecureTokenManager.importToken: token already used tokenId=0x" + Long.toHexString(tokenId));
                }
            }
        }

        SecureTokenSpec spec = new SecureTokenSpec(type, issuerId, expiry, properties);
        if (authenticator == null || authenticator.length == 0) {
            // this is the case when running without a master server,
            // so we don't want to generate an exception.
            Log.info("SecureTokenManager.importToken: token has no authenticator tokenId=0x" +
                     Long.toHexString(tokenId));
            valid = false;
        }
        if (valid) {
            // rewind the buffer and get the authenticated data out, separate from the authenticator
            buf.rewind();
            byte[] authedData = new byte[authedLength];
            buf.getBytes(authedData, 0, authedData.length);

            switch (type) {
            case SecureTokenSpec.TOKEN_TYPE_MASTER:
                PublicKey pubKey;
                synchronized (this) {
                    pubKey = masterPublicKeys.get(keyId);
                }
                valid = validateMasterAuthenticator(pubKey, authedData, authenticator);
                break;
            case SecureTokenSpec.TOKEN_TYPE_DOMAIN:
                SecretKey secretKey;
                synchronized (this) {
                    secretKey = domainKeys.get(keyId);
                }
                valid = validateDomainAuthenticator(secretKey, authedData, authenticator);
                break;
            default:
                Log.error("SecureTokenManager.importToken: invalid type=" + type);
                valid = false;
                break;
            }
        }

        if (valid) {
            synchronized(this) {
                if (issuerAlreadyUsed(issuerId, tokenId)) {
                    valid = false;
                    Log.error("SecureTokenManager.importToken: token already used tokenId=0x" +
                              Long.toHexString(tokenId));
                }
                else {
                    issuerAddToken(issuerId, tokenId, expiry);
                }
                issuerCleanup(issuerId, System.currentTimeMillis());
            }
        }

        SecureToken token = new SecureToken(spec, version, tokenId, keyId, authenticator, valid);
        return token;
    }

    public SecureToken importToken(MVByteBuffer tokenBuf) {
        byte[] encodedToken = new byte[tokenBuf.remaining()];
        tokenBuf.getBytes(encodedToken, 0, encodedToken.length);
        if (Log.loggingDebug) {
            Log.debug("SecureTokenManager.importToken: token=" + Arrays.toString(encodedToken));
        }
        return importToken(encodedToken);
    }


    /**
     * Genenerate and encode a new token from the supplied spec.
     *
     */
    public byte[] generateToken(SecureTokenSpec spec) {
        MVByteBuffer buf = new MVByteBuffer(512);

        SecretKey domainKey = null;
        PrivateKey masterKey = null;
        long keyId;
        byte type = spec.getType();

        synchronized (this) {
            switch (type) {
            case SecureTokenSpec.TOKEN_TYPE_MASTER:
                if (masterKeyId == -1) {
                    Log.error("SecureTokenManager.generateToken: master key not initialized");
                    throw new RuntimeException("master key not initialized");
                }
                keyId = masterKeyId;
                masterKey = masterPrivateKey;
                break;
            case SecureTokenSpec.TOKEN_TYPE_DOMAIN:
                if (domainKeyId == -1) {
                    Log.error("SecureTokenManager.generateToken: domain key not initialized");
                    throw new RuntimeException("domain key not initialized");
                }
                keyId = domainKeyId;
                domainKey = domainKeys.get(keyId);
                break;
            default:
                Log.error("SecureTokenManager.generateToken: invalid token type=" + type);
                throw new RuntimeException("invalid token type=" + type);
            }
        }

        // Serialize token data        
        buf.putByte(SecureToken.TOKEN_VERSION);
        buf.putByte(type);
        buf.putString(spec.getIssuerId());
        buf.putLong(nextTokenId());
        buf.putLong(keyId);
        buf.putLong(spec.getExpiry());

        TreeMap<String, Serializable> properties = spec.getPropertyMap();
        buf.putEncodedObject(properties);

        int authedDataLen = buf.position();
        buf.flip();
        byte[] authedData = new byte[authedDataLen];
        buf.getBytes(authedData, 0, authedData.length);

        // Generate authenticator
        byte[] authenticator;
        switch (type) {
            case SecureTokenSpec.TOKEN_TYPE_MASTER:
                authenticator = generateMasterAuthenticator(masterKey, authedData);
                break;
            case SecureTokenSpec.TOKEN_TYPE_DOMAIN:
                authenticator = generateDomainAuthenticator(domainKey, authedData);
                break;
            default:
                // should never get here
                Log.error("SecureTokenManager.generateToken: invalid token type=" + type);
                throw new RuntimeException("invalid token type=" + type);
        }
        
        if (authenticator == null) {
            Log.error("SecureTokenManager.generateToken: null authenticator");
            return null;
        }

        buf.putBytes(authenticator, 0, authenticator.length);
        byte[] token = new byte[buf.position()];
        buf.flip();
        buf.getBytes(token, 0, token.length);
        return token;
    }

    protected byte[] generateDomainAuthenticator(SecretKey key, byte[] data) {
        if (key == null) {
            return null;
        }
        try {
            Mac mac = Mac.getInstance(key.getAlgorithm());
            mac.init(key);
            return mac.doFinal(data);
        }
        catch (NoSuchAlgorithmException e) {
            // bad key
            return null;
        }
        catch (InvalidKeyException e) {
            // should never happen
            Log.exception("SecureTokenManager.generateDomainAuthenticator: invalid key", e);
            throw new RuntimeException(e);
        }
        catch (IllegalStateException e) {
            // should never happen
            Log.exception("SecureTokenManager.generateDomainAuthenticator: illegal state", e);
            throw new RuntimeException(e);
        }
    }

    protected boolean validateDomainAuthenticator(SecretKey key, byte[] data, byte[] authenticator) {
        byte[] newAuthenticator = generateDomainAuthenticator(key, data);
        return Arrays.equals(newAuthenticator, authenticator);
    }

    protected byte[] generateMasterAuthenticator(PrivateKey key, byte[] data) {
        if (key == null) {
            Log.error("SecureTokenManager.generateMasterAuthenticator: null key");
            return null;
        }
        try {
            Signature sig = Signature.getInstance(key.getAlgorithm());
            sig.initSign(key);
            sig.update(data);
            return sig.sign();
        }
        catch (NoSuchAlgorithmException e) {
            Log.exception("SecureTokenManager.generateMasterAuthenticator: bad key", e);
            // bad key
            return null;
        }
        catch (InvalidKeyException e) {
            // should never happen
            Log.exception("SecureTokenManager.generateMasterAuthenticator: invalid key", e);
            throw new RuntimeException(e);
        }
        catch (SignatureException e) {
            // should never happen
            Log.exception("SecureTokenManager.generateMasterAuthenticator: illegal signature state", e);
            throw new RuntimeException(e);
        }
    }

    protected boolean validateMasterAuthenticator(PublicKey key, byte[] data, byte[] authenticator) {
        if (key == null) {
            Log.error("SecureTokenManager.validateMasterAuthenticator: key is null");
            return false;
        }
        try {
            Signature sig = Signature.getInstance(key.getAlgorithm());
            sig.initVerify(key);
            sig.update(data);
            boolean rv = sig.verify(authenticator);
            if (Log.loggingDebug) {
                Log.debug("SecureTokenManager.validateMasterAuthenticator rv=" + rv);
            }
            return rv;
        }
        catch (NoSuchAlgorithmException e) {
            // bad key
            Log.exception("SecureTokenManager.validateMasterAuthenticator: bad key", e);
            return false;
        }
        catch (InvalidKeyException e) {
            // should never happen
            Log.exception("SecureTokenManager.validateMasterAuthenticator: invalid key", e);
            throw new RuntimeException(e);
        }
        catch (SignatureException e) {
            // bad signature
            Log.exception("SecureTokenManager.validateMasterAuthenticator: bad signature", e);
            return false;
        }
    }

    /**
     * Register a master public key, for use in validating master
     * tokens.
     *
     */
    public void registerMasterPublicKey(byte[] encodedPubKey) {

        MVByteBuffer buf = new MVByteBuffer(encodedPubKey);
        EncodedKeySpec keySpec;

        long keyId = buf.getLong();
        String algorithm = buf.getString();
        if (Log.loggingDebug) {
            Log.debug("SecureTokenManager.registerMasterPublicKey: decoding public key keyId=0x" +
                      Long.toHexString(keyId) + " algorithm=" + algorithm);
        }

        byte[] keyData = new byte[buf.remaining()];
        buf.getBytes(keyData, 0, keyData.length);

        keySpec = new X509EncodedKeySpec(keyData);

        if (masterPublicKeys.containsKey(keyId)) {
            Log.error("SecureTokenManager.registerMasterPublicKey: key already exists in table keyId=0x" +
                      Long.toHexString(keyId));
            throw new IllegalArgumentException("master public already exists in table keyId=0x" +
                                               Long.toHexString(keyId));
        }

        KeyFactory factory;
        try {
            factory = KeyFactory.getInstance(algorithm);
        }
        catch (NoSuchAlgorithmException e) {
            Log.exception("SecureTokenManager.registerMasterPublicKey: could not get KeyFactory instance. keyId=0x" +
                          Long.toHexString(keyId) + " algorithm=" + algorithm, e);
            throw new RuntimeException(e);
        }

        PublicKey pubKey;
        try {
            pubKey = factory.generatePublic(keySpec);
        }
        catch (InvalidKeySpecException e) {
            Log.exception("SecureTokenManager.registerMasterPublicKey: invalid master public key. keyId=0x" +
                          Long.toHexString(keyId), e);
            throw new RuntimeException(e);
        }

        masterPublicKeys.put(keyId, pubKey);
    }

    /**
     * Initialize master private key to generate master tokens, used
     * only by the master server.
     *
     */
    public void initMaster(byte[] encodedPrivKey) {
        MVByteBuffer buf = new MVByteBuffer(encodedPrivKey);
        EncodedKeySpec keySpec;

        long keyId = buf.getLong();
        String algorithm = buf.getString();
        if (Log.loggingDebug) {
            Log.debug("SecureTokenManager.initMaster: master key keyId=0x" + Long.toHexString(keyId) +
                      " algorithm=" + algorithm);
        }
        byte[] keyData = new byte[buf.remaining()];
        buf.getBytes(keyData, 0, keyData.length);
        keySpec = new PKCS8EncodedKeySpec(keyData);

        KeyFactory factory;
        try {
            factory = KeyFactory.getInstance(algorithm);
        }
        catch (NoSuchAlgorithmException e) {
            Log.exception("SecureTokenManager.initMaster: could not get KeyFactory instance. algorithm=" + algorithm +
                          " for keyId=0x" + Long.toHexString(keyId), e);
            throw new RuntimeException(e);
        }
        try {
            synchronized (this) {
                masterPrivateKey = factory.generatePrivate(keySpec);
                masterKeyId = keyId;
            }
        }
        catch (InvalidKeySpecException e) {
            Log.exception("SecureTokenManager.initMaster: invalid master private key. keyId=0x" +
                          Long.toHexString(keyId), e);
            throw new RuntimeException(e);
        }
    }

    /**
     * Initialize domain key to generate and validate domain
     * tokens. This should be called by every server that will deal
     * with tokens after fetching the domain key from the domain
     * server.
     *
     */
    public synchronized void initDomain(byte[] domainKey) {
        MVByteBuffer buf = new MVByteBuffer(domainKey);
        long domainKeyId = buf.getLong();
        String algorithm = buf.getString();
        if (Log.loggingDebug) {
            Log.debug("SecureTokenManager.initDomain: reading domain key. keyId=0x" +
                      Long.toHexString(domainKeyId) + " algorithm=" + algorithm);
        }
        byte[] keyData = new byte[buf.remaining()];
        buf.getBytes(keyData, 0, buf.remaining());

        if (domainKeys.containsKey(domainKeyId)) {
            Log.error("SecureTokenManager.initDomain: domain key already exists in table keyId=0x" +
                      Long.toHexString(domainKeyId));
            throw new IllegalArgumentException("domain key already exists in table keyId=0x" +
                                               Long.toHexString(domainKeyId));
        }
        
        SecretKeySpec keySpec = new SecretKeySpec(keyData, algorithm);
        this.domainKeyId = domainKeyId;
        domainKeys.put(domainKeyId, keySpec);
    }

    // XXX This should be persistent
    protected long lastTokenId = 1;
    protected synchronized long nextTokenId() {
        lastTokenId++;
        return lastTokenId;
    }

    // used for generating and validating domain tokens
    protected long domainKeyId = -1;
    protected Map<Long, SecretKey> domainKeys = new HashMap<Long, SecretKey>();

    // used for validating master tokens
    protected Map<Long, PublicKey> masterPublicKeys = new HashMap<Long, PublicKey>();

    // for for generating master tokens (master server only)
    protected PrivateKey masterPrivateKey = null;
    protected long masterKeyId = -1;

    protected Map<String, IssuerHistory> issuerHistories = new HashMap<String, IssuerHistory>();

    public byte[] getEncodedDomainKey() {
        return SecureTokenUtil.encodeDomainKey(domainKeyId, domainKeys.get(domainKeyId));
    }

    // to determine whether a domainKey has been set
    public boolean hasDomainKey()
    {
        boolean result = true;
        if (domainKeyId == -1) {
            result = false;
        }
        return result;
    }

    protected boolean issuerAlreadyUsed(String issuerId, long tokenId) {
        IssuerHistory issuer = issuerHistories.get(issuerId);
        if (issuer == null) {
            return false;
        }
        return issuer.alreadyUsed(tokenId);
    }

    protected void issuerAddToken(String issuerId, long tokenId, long expiry) {
        IssuerHistory issuer = issuerHistories.get(issuerId);
        if (issuer == null) {
            issuer = new IssuerHistory(issuerId);
            issuerHistories.put(issuerId, issuer);
        }
        issuer.addToken(tokenId, expiry);
    }

    protected void issuerCleanup(String issuerId, long time) {
        IssuerHistory issuer = issuerHistories.get(issuerId);
        if (issuer == null) {
            return;
        }
        issuer.cleanup(time);
    }

    protected class IssuerHistory {
        protected IssuerHistory(String issuerId) {
            this.issuerId = issuerId;
        }
        protected final String issuerId;

        // used tokenIds
        protected final Set<Long> usedTokenIds = new HashSet<Long>();

        // keep sorted index of expirys and their tokenId so we can iterate to clean up
        protected final TreeMap<Long, Set<Long>> usedTokens = new TreeMap<Long, Set<Long>>();

        public boolean alreadyUsed(long tokenId) {
            return usedTokenIds.contains(tokenId);
        }

        protected void addToken(long tokenId, long expiry) {
            usedTokenIds.add(tokenId);
            Set<Long> tokenIdList = usedTokens.get(expiry);
            if (tokenIdList == null) {
                tokenIdList = new HashSet<Long>();
                usedTokens.put(expiry, tokenIdList);
            }
            tokenIdList.add(tokenId);
        }

        // cleans up tokens that expired before time because there's no need to remember them anymore
        protected void cleanup(long time) {
            while(true) {
                // if no tokens in history, nothing to clean up
                if (usedTokens.size() == 0) {
                    break;
                }
                Long expiry = usedTokens.firstKey();
                // if no tokens older than time, nothing to clean up
                if (expiry >= time) {
                    break;
                }
                // clean up first entry in history
                for (Long tokenId : usedTokens.get(expiry)) {
                    usedTokenIds.remove(tokenId);
                }
                usedTokens.remove(expiry);
            }
        }

    }

    public static void main(String args[]) {
        Log.init();
        SecretKey key = SecureTokenUtil.generateDomainKey();
        System.out.println("domain key:");
        System.out.println(key.getFormat() + ", " + key.getAlgorithm());
        System.out.println(Base64.encodeBytes(key.getEncoded()));
        System.out.println("");

        KeyPair pair = SecureTokenUtil.generateMasterKeyPair();
        PrivateKey priv = pair.getPrivate();
        System.out.println("private key:");
        System.out.println(priv.getFormat() + ", " + priv.getAlgorithm());
        System.out.println(Base64.encodeBytes(priv.getEncoded()));
        System.out.println("");
        PublicKey pub = pair.getPublic();
        System.out.println("public key:");
        System.out.println(pub.getFormat() + ", " + pub.getAlgorithm());
        System.out.println(Base64.encodeBytes(pub.getEncoded()));
        System.out.println("");

        byte[] encodedPrivKey = SecureTokenUtil.encodeMasterPrivateKey(12, pair.getPrivate());
        System.out.println("encoded private key:");
        System.out.println(Base64.encodeBytes(encodedPrivKey));
        System.out.println("");

        byte[] encodedPubKey = SecureTokenUtil.encodeMasterPublicKey(12, pair.getPublic());
        System.out.println("encoded public key:");
        System.out.println(Base64.encodeBytes(encodedPubKey));
        System.out.println("");

        byte[] encodedDomainKey = SecureTokenUtil.encodeDomainKey(24, key);
        System.out.println("encoded domain key:");
        System.out.println(Base64.encodeBytes(encodedDomainKey));
        System.out.println("");

        SecureTokenManager.getInstance().registerMasterPublicKey(encodedPubKey);
        SecureTokenManager.getInstance().initMaster(encodedPrivKey);
        SecureTokenManager.getInstance().initDomain(encodedDomainKey);

        SecureTokenSpec masterSpec = new SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_MASTER, "test",
                                                         System.currentTimeMillis() + 10000);
        masterSpec.setProperty("prop1", "value1");

        byte[] masterTokenData = SecureTokenManager.getInstance().generateToken(masterSpec);
        System.out.println("master token data:");
        System.out.println(Base64.encodeBytes(masterTokenData));
        System.out.println("");
 
        SecureToken masterToken = SecureTokenManager.getInstance().importToken(masterTokenData);
        System.out.println("imported master token:");
        System.out.println(masterToken.toString());

        SecureTokenSpec domainSpec = new SecureTokenSpec(SecureTokenSpec.TOKEN_TYPE_DOMAIN, "test",
                                                         System.currentTimeMillis() + 10000);
        domainSpec.setProperty("prop1", "value1");

        byte[] domainTokenData = SecureTokenManager.getInstance().generateToken(domainSpec);
        System.out.println("domain token data:");
        System.out.println(Base64.encodeBytes(domainTokenData));
        System.out.println("");
 
        SecureToken domainToken = SecureTokenManager.getInstance().importToken(domainTokenData);
        System.out.println("imported domain token:");
        System.out.println(domainToken.toString());
 }
}
