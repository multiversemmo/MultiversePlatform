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

import multiverse.server.network.*;

/** 
 * This is the interface that all voice groups must satisfy, and it's
 * based exclusively on oids.  The only players represented in the
 * group at this level are those who are currently connected and have
 * chosen this group as their current voice group.
 */
public interface VoiceGroup {
    
    /**
     * Return true if the group is a positional group; that is, if
     * membership in the group is determined by being near the
     * listener, and the priority of speakers is determined by how
     * near any speaker is.
     */
    public boolean isPositional();

    /**
     * Return the group oid of a voice group.
     * @return The groupOid of the voice group.
     */
    public long getGroupOid();

    /**
     * Determine if the oid is that of a member of the group.
     * @param memberOid The oid of possible group member.
     * @return True if the memberOid is a member of the group or is
     * allowed to be a member of the group, and false otherwise.
     */
    public boolean addMemberAllowed(long memberOid);
    
    /**
     * Supply a list of oids of players allowed to be in the group
     * @param allowedMembers The set of oids of possible group members.
     */
    public void setAllowedMembers(Set<Long> allowedMembers);
    
    /**
     * Return the allowed members
     * @return A Set<Long> of allowed member oids.
     */
    public Set<Long> getAllowedMembers();

    /**
     * Add a member to a group
     * @param memberOid The oid of the newly-created member
     * @param memberCon The VoiceConnection object that embodies 
     * the connection to the voice server.
     * @return The newly-created group member.
     */
    public GroupMember addMember(long memberOid, VoiceConnection memberCon);

    /**
     * Get all members of the group.
     * @param memberList The list of the members to which the groups
     * members should be added.
     */
    public void getAllMembers(List<GroupMember> memberList);
    
    /**
     * Add a member to a group
     * @param memberOid The oid of the newly-created member
     * @param memberCon The VoiceConnection object that embodies 
     * the connection to the voice server.
     * @param priority The speaking priority that the member should be
     * assigned; speakers with higher priorities will be heard over
     * speakers with lower priorities.
     * @param allowedSpeaker True if the new member should be allowed
     * to speak; false otherwise.
     * @return The newly-created group member.
     */
    public GroupMember addMember(long memberOid, VoiceConnection memberCon, int priority, boolean allowedSpeaker);

    /**
     * A call to this method is made after an addMember() operation.
     * @param memberOid The oid of the newly-created member
     * @param groupOid The groupOid of the voice group.
     * @param allowedSpeaker True if the member is allowed to speak;
     * false otherwise.
     * @param micVoiceNumber The voice number of the incoming voice
     * packets.  For now, this is always zero.
     * @param listenToYourself True if the member wants his own voice
     * frames sent back to him.  This is only used for testing
     * purposes.
     */
    public void onAfterAddMember(long memberOid, long groupOid, boolean allowedSpeaker, byte micVoiceNumber, boolean listenToYourself);

    /** 
     * Find a member in the group by oid.
     * @param memberOid The oid of the member to find
     * @return The member whose oid is memberOid, or null if there is no such member.
     */
    public GroupMember isMember(long memberOid);

    /**
     * Remove the member of the group identified by the given oid, and
     * return true if that member was found in the group.
     * @param memberOid The oid of the member to remove.
     */
    public boolean removeMember(long memberOid);
    
    /**
     * A call to this method is made after a removeMember() operation.
     * @param memberOid The oid of the newly-created member
     * @param groupOid The groupOid of the voice group.
     * @param allowedSpeaker True if the member is allowed to speak;
     * false otherwise.
     */
    public void onAfterRemoveMember(long memberOid, long groupOid, boolean allowedSpeaker);

    /**
     * Get the default priority that should be applied to new members 
     * if no priority is supplied
     * @return The default member priority.
     */
    public int getDefaultPriority();

    /**
     * Change the member with the given oid to be an allowed speaker
     * if add is true, or not an allowed speaker if add is false.
     * @param memberOid The member whose allowed speaker status should
     * change.
     * @param add If true, make the member an allowed speaker; if
     * false, stop allowing the member to be a speaker.
     */
    public void setAllowedSpeaker(long memberOid, boolean add);

    /**
     * Change the member with the given oid, who must be an allowed
     * speaker, to speak if add is true, or to stop speaking, if add
     * is false.  Reflect any changes in who is and isn't speaking by
     * sending out the appropriate voice deallocations and voice
     * allocations to users.  Calls to this method are what generates
     * voice allocation and deallocation traffic to the clients in
     * response to speakers starting and stopping speaking.
     * @param memberOid The member whose speaking status should
     * change.
     * @param add If true, make the member a speaker in the group,
     * if add is True, else stop the member speaking.
     */
    public void setMemberSpeaking(long memberOid, boolean add);

    /**
     * Change the member with the given oid to be a listener if add is
     * true, or not a listener if add is false.
     * @param memberOid The member whose whose status should
     * change.
     * @param add If true, make the member a listener in the group,
     * if add is True, else stop the member listening.
     */
    public void setListener(long memberOid, boolean add);
        
    /** 
     * Send a voice frame originating with the speaker with the given
     * oid, and contained in the first dataSize bytes of buf, to all
     * eligible listeners
     * @param speakerOid The oid of the speaker that is the source of the voice frame(s)
     * @param buf The buffer containing data packet; typically a data aggregation packet
     * @param opcode The opcode to use when sending the frame(s).  Typically
     * VoicePlugin.opcodeAggregatedData.
     * @param pktSize The size of the data packet.
     */
    public void sendVoiceFrameToListeners(long speakerOid, MVByteBuffer buf, byte opcode, int pktSize);

    /*
     * These are unreferenced right now.  Should they be eliminated,
     * since their value can change immediately afterwards anyway?
     */

    /** 
     * Is the member allowed to speak?
     * @param memberOid The oid of the member who allowed speaking status should be returned.
     * @return True if the member with the given oid is allowed to
     * speak if he wants, false otherwise.
     */
    public boolean isAllowedSpeaker(long memberOid);

    /** 
     * Is the member speaking?
     * @param memberOid The oid of the member who speaking status should be returned.
     * @return True if the member with the given oid is speaking; false otherwise
     */
    public boolean isMemberSpeaking(long memberOid);
        
    /** 
     * Is the member listening?
     * @param memberOid The oid of the member who listening status should be returned.
     * @return True if the member with the given oid is listening; false otherwise
     */
    public boolean isListener(long memberOid);

}

