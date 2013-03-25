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

package multiverse.mars.plugins;


import multiverse.server.engine.*;
import multiverse.server.util.Log;
import multiverse.server.util.SecureToken;
import multiverse.server.util.SecureTokenManager;
import multiverse.server.util.MVRuntimeException;
import multiverse.server.objects.Entity;
import multiverse.server.objects.DisplayContext;
import multiverse.server.objects.MVObject;
import multiverse.server.network.MVByteBuffer;
import multiverse.server.plugins.ObjectManagerClient;
import multiverse.server.worldmgr.LoginPlugin;
import multiverse.server.worldmgr.CharacterFactory;
import multiverse.server.engine.Namespace;
import java.util.*;
import java.io.*;


/** Mars LoginPlugin implementation.  Reads and stores characters using
    the {@link multiverse.server.engine.Engine} database.

*/
public class MarsLoginPlugin extends LoginPlugin {


    /** Return character list.  For each character, the following
        properties are returned:
        <li>Long characterId: character oid</li>
        <li>String characterName</li>
        <li>String displayContext: the character's display context.  The
          DisplayContext is serialized into a string format understood
          by the client's default login script.</li>
        <p>
        This implementation does not truly authorize the auth token.
	The auth token should contain a 32-bit integer which is taken as
	the account id.  (bitwise negated if SecureToken is true).
        <p>
        The returned world token is a fixed place holder.
        @param message Character request message.
        @param clientSocket Socket to the client.
    */
    protected CharacterResponseMessage handleCharacterRequestMessage(
                CharacterRequestMessage message, SocketHandler clientSocket)
    {
        MVByteBuffer authToken = message.getAuthToken();

        CharacterResponseMessage response = new CharacterResponseMessage();
        int uid;

        if (clientSocket.getAccountId() == null) {
            SecureToken token = SecureTokenManager.getInstance().importToken(authToken);
            boolean valid = true;

            if (LoginPlugin.SecureToken) {
                valid = token.getValid();
            }
            if (LoginPlugin.WorldId != null && token.getProperty("world_id").equals(LoginPlugin.WorldId)) {
                valid = false;
            }
            if (!token.getIssuerId().equals("master")) {
                valid = false;
            }

            uid = (Integer)token.getProperty("account_id");

            if (!valid) {
                response.setErrorMessage("invalid master token");
                return response;
            }

            clientSocket.setAccountId(uid);
        }
        else {
            uid = clientSocket.getAccountId();
        }

        // get character data out of DB
        Database db = Engine.getDatabase();
        String worldName = Engine.getWorldName();
        List<Long> charIds = db.getGameIDs(worldName, uid);

        // send the list of characters back to the client
        // so far, just the name
        int characterCount = 0;
        String characterNames = "";
        for (long oid : charIds) {
            if (Log.loggingDebug)
                Log.debug("MarsLoginPlugin: character oid: " + oid);

            // load the user from the database
            Entity entity = Engine.getDatabase().loadEntity(oid, Namespace.WORLD_MANAGER);
            if (Log.loggingDebug)
                Log.debug("MarsLoginPlugin: loaded character from db: " + entity);

            // prepare character info
            // OID and name
            Map<String,Serializable> charInfo = new HashMap<String,Serializable>();
            charInfo.put("characterId", entity.getOid());
            charInfo.put("characterName", entity.getName());
            characterNames += entity.getName()+"("+entity.getOid()+"),";
            characterCount++;

            // Get the display context
            DisplayContext displayContext = getDisplayContext(entity);
            if (displayContext != null) {
                charInfo.put("displayContext",
                            marshallDisplayContext(displayContext));
            }

	    setCharacterProperties(charInfo, entity);

            response.addCharacter(charInfo);
        }

        Log.info("LoginPlugin: GET_CHARACTERS remote=" + clientSocket.getRemoteSocketAddress() +
            " account=" + uid +
            " count=" + characterCount + " names=" + characterNames);

        clientSocket.setCharacterInfo(response.getCharacters());

        return response;
    }

    /** Delete a character.  On success, the returned
        properties is the supplied properties plus new properties:
        <li>Boolean status: TRUE</li>
        <li>whatever the character factory adds to the properties</li>
        <p>
        On an internal failure, the returned properties contain only:
        <li>Boolean status: FALSE</li>
        <li>String errorMessage</li>
        @param message Character delete message.
        @param clientSocket Socket to the client.
    */
    protected CharacterDeleteResponseMessage handleCharacterDeleteMessage(
                CharacterDeleteMessage message,
                SocketHandler clientSocket)
    {
        CharacterDeleteResponseMessage response =
                new CharacterDeleteResponseMessage();

        Map<String,Serializable> props = message.getProperties();
        Map<String,Serializable> errorProps = new HashMap<String,Serializable>();

        if (Log.loggingDebug)  {
            Log.debug("MarsLoginPlugin: delete character properties: ");
            for (Map.Entry<String,Serializable> entry : props.entrySet()) {
                Log.debug("character property "+entry.getKey() + "=" +
                        entry.getValue());
            }
        }

        errorProps.put("status", Boolean.FALSE);
        response.setProperties(errorProps);

        if (clientSocket.getAccountId() == null) {
            errorProps.put("errorMessage", "Permission denied");
            return response;
        }

        int uid = clientSocket.getAccountId();

	Long oid = (Long)props.get("characterId");

        Database db = Engine.getDatabase();
        List<Long> characterOids = null;
        try {
            characterOids = db.getGameIDs(Engine.getWorldName(), uid);
        }
        catch (MVRuntimeException ex) {
            errorProps.put("errorMessage", ex.toString());
            return response;
        }

        if (! characterOids.contains(oid)) {
            errorProps.put("errorMessage", "Character does not exist.");
            return response;
        }

        CharacterFactory factory= getCharacterGenerator().getCharacterFactory();
        if (factory == null) {
            Log.error("MarsLoginPlugin: missing character factory");
            errorProps.put("errorMessage", "Missing character factory.");
            return response;
        }

        String errorMessage;
        try {
            errorMessage = factory.deleteCharacter(Engine.getWorldName(),
                uid, oid, props);
        }
        catch (Exception ex) {
            Log.exception("Exception deleting character", ex);
            errorProps.put("errorMessage", ex.toString());
            return response;
        }

        if (errorMessage != null) {
            errorProps.put("errorMessage", errorMessage);
            return response;
        }

        String characterName = db.getObjectName(oid,
            Namespace.OBJECT_MANAGER);

        try {
            db.deletePlayerCharacter(oid);
        }
        catch (Exception ex) {
            errorProps.put("errorMessage", ex.toString());
            return response;
        }

        try {
            db.deleteObjectData(oid);
        }
        catch (Exception ex) { }

        Log.info("LoginPlugin: CHARACTER_DELETE remote=" + clientSocket.getRemoteSocketAddress() +
            " account=" + uid +
            " oid=" + oid + " name=" + characterName);

        props.put("status", Boolean.TRUE);

        response.setProperties(props);
        return response;
    }

    /** Create a character.  The character properties are passed to the
        global character generator.  On success, the returned
        properties is the supplied properties plus new properties:
        <li>Boolean status: TRUE</li>
        <li>Long characterId: new character's oid</li>
        <li>whatever the character factory adds to the properties</li>
        <p>
	If the properties contains "errorMessage" after calling the
	character factory, the character is not saved, and the
	properties are returned to the client.
        <p>
        On an internal failure, the returned properties contain only:
        <li>Boolean status: FALSE</li>
        <li>String errorMessage</li>
        @param message Character create message.
        @param clientSocket Socket to the client.
    */
    protected CharacterCreateResponseMessage handleCharacterCreateMessage(
                CharacterCreateMessage message,
                SocketHandler clientSocket)
    {
        CharacterCreateResponseMessage response =
                new CharacterCreateResponseMessage();

        Map<String,Serializable> props = message.getProperties();

        if (clientSocket.getAccountId() == null) {
            props.clear();
            props.put("status", Boolean.FALSE);
            props.put("errorMessage", "Permission denied");
            response.setProperties(props);
            return response;
        }

        int uid = clientSocket.getAccountId();

        String propertyText = "";
        for (Map.Entry<String,Serializable> entry : props.entrySet()) {
            propertyText += "[" + entry.getKey() + "=" + entry.getValue() + "] ";
        }
        Log.info("LoginPlugin: CHARACTER_CREATE remote=" + clientSocket.getRemoteSocketAddress() +
            " account=" + uid + " properties=" + propertyText);

        // try to create default character
        Database db = Engine.getDatabase();
        String worldName = Engine.getWorldName();

        if (getCharacterGenerator().getCharacterFactory() != null) {
            if (Log.loggingDebug)
                Log.debug("MarsLoginPlugin: creating character");
            Long oid = null;
            try {
                oid = getCharacterGenerator().getCharacterFactory().createCharacter(worldName, uid, props);
            } catch (Exception e) {
                Log.exception("Caught exception in character factory: ", e);
                props.clear();
                props.put("errorMessage", "Internal error");
            }

            if (oid == null) {
                Log.error("Character factory returned null OID");
                if (props.get("errorMessage") == null)
                    props.put("errorMessage", "Internal error");
            }

            if (props.get("errorMessage") != null) {
                Log.error("MarsLoginPlugin: character creation failed, account=" + uid +
                    " errorMessage="+props.get("errorMessage"));
                props.put("status", Boolean.FALSE);
                response.setProperties(props);
                return response;
            }

            if (Log.loggingDebug)
                Log.debug("MarsLoginPlugin: saving oid " + oid);
            boolean success = false;
            if (oid != null)
                success = ObjectManagerClient.saveObject(oid);

            if (success) {
                if (Log.loggingDebug)
                    Log.debug("MarsLoginPlugin: saved oid " + oid);

                // map the mv id to the game id
                db.mapMultiverseID(worldName, uid, oid);

                props.put("status", Boolean.TRUE);
                props.put("characterId", oid);

                Entity entity = Engine.getDatabase().loadEntity(oid,
                    Namespace.WORLD_MANAGER);
                props.put("characterName", entity.getName());
                DisplayContext displayContext = getDisplayContext(entity);
                if (displayContext != null) {
                    props.put("displayContext",
                                marshallDisplayContext(displayContext));
                }

                setCharacterProperties(props, entity);

                clientSocket.getCharacterInfo().add(props);

                Log.info("LoginPlugin: CHARACTER_CREATE remote=" + clientSocket.getRemoteSocketAddress() +
                    " account=" + uid +
                    " oid=" + oid + " name=" + entity.getName());
            }
            else {
                Log.error("MarsLoginPlugin: failed to save oid " + oid);
                props.clear();
                props.put("status", Boolean.FALSE);
                props.put("errorMessage", "Failed to save new character");
            }
        }
        else {
            Log.error("MarsLoginPlugin: missing character factory");
            props.clear();
            props.put("status", Boolean.FALSE);
            props.put("errorMessage", "Missing character factory");
        }

        response.setProperties(props);
        return response;
    }

    private DisplayContext getDisplayContext(Entity entity)
    {
        DisplayContext displayContext = null;
        MVObject mvObject =
            (MVObject) Engine.getDatabase().loadEntity(entity.getOid(), Namespace.WORLD_MANAGER);
        displayContext = mvObject.displayContext();

        if (displayContext != null) {
            if (Log.loggingDebug)
                Log.debug("Display context for '"+entity.getName()+"': "+
                    displayContext);
        }

        return displayContext;
    }

    private String marshallDisplayContext(DisplayContext displayContext)
    {
        String result = displayContext.getMeshFile();
        for (DisplayContext.Submesh submesh : displayContext.getSubmeshes()) {
            result += "\002" + submesh.getName() + "\002" + submesh.getMaterial();
        }
        Map<String, DisplayContext> childDCs=displayContext.getChildDCMap();
        for (Map.Entry<String,DisplayContext> entry : childDCs.entrySet()) {
            DisplayContext childDC = entry.getValue();
            result += "\001" + entry.getKey() + "\002" + childDC.getMeshFile();
            for (DisplayContext.Submesh submesh : childDC.getSubmeshes()) {
                result += "\002" + submesh.getName() + "\002" + submesh.getMaterial();
            }
        }
        return result;
    }

    protected void setCharacterProperties(Map<String,Serializable> props, Entity entity) {
	for (Map.Entry<Namespace, Set<String>> entry : characterProps.entrySet()) {
	    Namespace namespace = entry.getKey();
	    Entity subObj = Engine.getDatabase().loadEntity(entity.getOid(), namespace);
	    for (String propName : entry.getValue()) {
	        Serializable propValue = subObj.getProperty(propName);
	        if (propValue != null) {
	            props.put(propName, propValue);
	        }
	    }
	}
    }

    public static void registerCharacterProperty(Namespace namespace, String propName) {
	Set<String> propSet = characterProps.get(namespace);
	if (propSet == null) {
	    propSet = new HashSet<String>();
	    characterProps.put(namespace, propSet);
	}
	propSet.add(propName);
    }
    protected static Map<Namespace, Set<String>> characterProps = new HashMap<Namespace, Set<String>>();
}

