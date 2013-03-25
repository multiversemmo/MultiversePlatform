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

package multiverse.server.voice;

import java.util.*;
import multiverse.server.engine.*;
import multiverse.server.math.*;


/**
 * A class that extends GroupMmeber to hold the members in radius
 * of this member.
 */
public class PositionalGroupMember extends GroupMember {

    /**
     * PositionalGroupMember constructor
     * @param group The group to which the member belongs.
     * @param memberOid The oid of the new member.
     * @param priority The speaking priority that the member should be
     * assigned; speakers with higher priorities will be heard over
     * speakers with lower priorities.
     * @param allowedSpeaker True if the new member should be allowed
     * to speak; false otherwise.
     * @param currentSpeaker True if the new member should be created speaking.
     * @param memberCon The VoiceConnection object that embodies 
     * the connection to the voice server.
     * @param maxVoiceChannels The maximum number of simultaneous voice 
     * channels going to any listener.
     */
    public PositionalGroupMember(VoiceGroup group, long memberOid, int priority, boolean allowedSpeaker, 
                                 boolean currentSpeaker, VoiceConnection memberCon, int maxVoiceChannels) {
        super(group, memberOid, priority, allowedSpeaker, currentSpeaker, memberCon, maxVoiceChannels);
        if (memberCon.listenToYourself)
            membersInRadius.add(this);
    }

    /**
     * Return the instance oid, if the member has a wnode.
     * @return The instance oid of the instance the member belongs to.
     */
    public Long getInstanceOid() {
        if (wnode != null)
            return wnode.getInstanceOid();
        else
            return null;
    }

    /**
     * Return the member's current location, or null, if he has none.
     * @return The location of the member.
     */
    public Point getCurrentLoc() {
        if (wnode != null)
            return wnode.getCurrentLoc();
        else
            return null;
    }
    
    /**
     * The world node for this group member.  If it is null, then the
     * member has despawned, presumably because it is moving to a
     * different instance.
     */
    public InterpolatedWorldNode wnode;

    /**
     * The last interpolated location of the world node.
     */
    public Point lastLoc;

    /**
     * The previous interpolated location of the entity, used to
     * detect if the entity has moved.
     */
    public Point previousLoc;

    /**
     * The set of object oids perceived by this member.
     */
    public Set<Long> perceivedOids = new HashSet<Long>();

    /**
     * The set of GroupMembers within the audible radius of this member
     */
    public Set<PositionalGroupMember> membersInRadius = new HashSet<PositionalGroupMember>();
}

    
