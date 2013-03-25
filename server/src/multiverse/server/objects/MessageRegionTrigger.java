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


import java.util.Map;
import java.util.HashMap;
import java.util.HashSet;
import java.io.Serializable;
import java.util.Set;

import multiverse.msgsys.*;
import multiverse.server.util.Log;
import multiverse.server.util.DebugUtils;
import multiverse.server.plugins.WorldManagerClient;
import multiverse.server.plugins.WorldManagerClient.TargetedPropertyMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.messages.PropertyMessage;
import multiverse.server.engine.Engine;

/** Publish a message when object enters or leaves a custom region.
Register with WorldManagerPlugin.registerRegionTrigger(String,RegionTrigger).
<p>
The trigger has two modes: subject and target.  In subject mode, the
trigger publishes a SubjectMessage sub-class which will be received by all
perceivers of the subject (the player).  In target mode, the trigger
publishes a TargetMessage sub-class which will apply only to the object itself.
<p>
The trigger can add a fixed set of properties to each message.  And the
properties can be filtered by a set of property exclusions.  The message
will always have a "regionAction" property set to either "onEnter" or
"onLeave" indicating which action triggered the message.
<p>
The remaining trigger behavior is controled by custom region properties:
<ul>
<li>messageType - The message type name.  For example "mv.PROPERTY".
<li>messageClass - When set to "extension" an ExtensionMessage or
TargetedExtesionMessage is published depending on the trigger mode.  When
set to "property" a PropertyMessage or TargetedPropertyMessage is
published depending on the trigger mode.
<li>messageRegionProperties - Comma delimited list of region properties to
add to the message.  Set to "ALL" to add all the region properties.
<li>messageObjectProperties - Comma delimited list of object (player)
properties to add to the message.  Set to "ALL" to add all the object
properties.  Note that the object is the player's world manager sub-object,
so only properties on that sub-object are available.
</ul>
*/
public class MessageRegionTrigger implements RegionTrigger
{
    public MessageRegionTrigger()
    {
    }

    /** Publish a 'targeted' message (TargetMessage sub-class). */
    public static final int TARGET_MODE = 1;
    /** Publish a 'subject' message (SubjectMessage sub-class). */
    public static final int SUBJECT_MODE = 2;

    public MessageRegionTrigger(int mode)
    {
        setMode(mode);
    }

    public MessageRegionTrigger(int mode,
        Map<String,Serializable> messageProperties,
        Set<String> propertyExclusions)
    {
        setMode(mode);
        setMessageProperties(messageProperties);
        setPropertyExclusions(propertyExclusions);
    }

    /** Get the trigger mode.
        @return One of TARGET_MODE or SUBJECT_MODE
    */
    public int getMode()
    {
        return mode;
    }

    /** Set the trigger mode to TARGET_MODE or SUBJECT_MODE.
    */
    public void setMode(int mode)
    {
        this.mode = mode;
    }

    /** Get the additional message properties.
    */
    public Map<String,Serializable> getMessageProperties()
    {
        return messageProperties;
    }

    /** Set the additional message properties.  These properties will
        be added to all messages sent by this trigger.  The property
        exclusions do not apply to the additional properties.
    */
    public void setMessageProperties(Map<String,Serializable> messageProperties)
    {
        this.messageProperties = messageProperties;
    }

    /** Get the property exclusions.
    */
    public Set<String> getPropertyExclusions()
    {
        return propertyExclusions;
    }

    /** Set the property exclusions.  The named properties will be
        excluded from messages.  Useful in combination with
        messageRegionProperties=ALL or messageObjectProperties=ALL
    */
    public void setPropertyExclusions(Set<String> propertyExclusions)
    {
        this.propertyExclusions = propertyExclusions;
    }

    /** Send message when object enters a region.  The message will have
        a "regionAction" property set to "onEnter".
    */
    public void enter(MVObject obj, Region region)
    {
        Message message = makeMessage(obj, region);
        if (message == null) {
            Log.error("MessageRegionTrigger: can't build message for "+obj+
                " entering region "+region);
            return;
        }

        configureMessage(message, obj, region, "onEnter");

        Engine.getAgent().sendBroadcast(message);
    }

    /** Send message when object leaves a region.  The message will have
        a "regionAction" property set to "onLeave".
    */
    public void leave(MVObject obj, Region region)
    {
        Message message = makeMessage(obj, region);
        if (message == null) {
            Log.error("MessageRegionTrigger: can't build message for "+obj+
                " leaving region "+region);
            return;
        }

        configureMessage(message, obj, region, "onLeave");

        Engine.getAgent().sendBroadcast(message);
    }

    protected Message makeMessage(MVObject obj, Region region)
    {
        MessageType type = null;
        String typeName = (String) region.getProperty("messageType");
        if (typeName != null && ! typeName.equals("")) {
            type = MessageCatalog.getMessageType(typeName);
            if (type == null) {
                Log.error("MessageRegionTrigger: unknown messageType="+typeName);
                return null;
            }
        }

        String messageClass = (String) region.getProperty("messageClass");
        if (messageClass == null || messageClass.equals(""))
            messageClass = "extension";

        String extensionType = (String) region.getProperty("messageExtensionType");

        Message message = null;
        if (messageClass.equals("extension")) {
            if (mode == TARGET_MODE) {
                TargetedExtensionMessage extMessage =
                    new TargetedExtensionMessage(obj.getOid(), obj.getOid());
                if (extensionType != null)
                    extMessage.setExtensionType(extensionType);
                message = extMessage;
            }
            else if (mode == SUBJECT_MODE) {
                ExtensionMessage extMessage =
                    new ExtensionMessage(obj.getOid());
                if (extensionType != null)
                    extMessage.setExtensionType(extensionType);
                message = extMessage;
            }
        }
        else if (messageClass.equals("property")) {
            if (mode == TARGET_MODE) {
                message = new WorldManagerClient.TargetedPropertyMessage(
                    obj.getOid(), obj.getOid());
            }
            else if (mode == SUBJECT_MODE) {
                message = new PropertyMessage(type, obj.getOid());
            }
        }

        if (message != null && type != null)
            message.setMsgType(type);

        return message;
    }

    protected void configureMessage(Message message, MVObject obj,
        Region region, String action)
    {
        Map<String, Serializable> messageMap = null;
        if (message instanceof PropertyMessage)
            messageMap = ((PropertyMessage)message).getPropertyMapRef();
        else if (message instanceof TargetedPropertyMessage)
            messageMap = ((TargetedPropertyMessage)message).getPropertyMapRef();

        if (messageMap != null) {
            if (action != null)
                messageMap.put("regionAction", action);
            if (messageProperties != null)
                messageMap.putAll(messageProperties);
        }

        String messageRegionProperties =
            (String) region.getProperty("messageRegionProperties");
        if (messageRegionProperties != null) {
            if (messageMap != null)
                copyProperties(messageRegionProperties,
                    region.getPropertyMapRef(), messageMap);
        }

        String objectProperties =
            (String) region.getProperty("messageObjectProperties");
        if (objectProperties != null) {
            if (messageMap != null)
                copyProperties(objectProperties, obj.getPropertyMap(),
                    messageMap);
        }

        if (Log.loggingDebug && messageMap != null) {
            Log.debug("MessageRegionTrigger: properties="+
                DebugUtils.mapToString(messageMap));
        }

    }

    protected void copyProperties(String propertyNames,
        Map<String, Serializable> source,
        Map<String, Serializable> destination)
    {
        propertyNames = propertyNames.trim();
        if (propertyNames.equals("ALL")) {
            for (String prop : source.keySet()) {
                if (! propertyExclusions.contains(prop)) {
                    Serializable value = source.get(prop);
                    if (value != null) {
                        destination.put(prop,value);
                    }
                }
            }
        }
        else {
            String[] props = propertyNames.split(",");
            for (String prop : props) {
                prop = prop.trim();
                if (! propertyExclusions.contains(prop)) {
                    Serializable value = source.get(prop);
                    if (value != null) {
                        destination.put(prop,value);
                    }
                }
            }
        }
    }

    private int mode = TARGET_MODE;
    private Map<String,Serializable> messageProperties =
        new HashMap<String,Serializable>();
    private Set<String> propertyExclusions = new HashSet<String>();
}
