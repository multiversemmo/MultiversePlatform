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

import java.util.concurrent.locks.*;
import java.util.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.VoiceClient;

/**
 * This class implements VoiceGroup interface, and is used to
 * implement two important cases of voice groups: presentations
 * and "raid groups".
 */
abstract public class BasicVoiceGroup implements VoiceGroup {

    /**
     * Create a BasicVoiceGroup
     * @param groupOid The oid of the group, which is unique across all 
     * voice groups.
     * @param association An object-valued data member that is unused
     * in BasicVoiceGroup, but available to derived classes.
     * @param voiceSender The abstraction that allows the voice group to 
     * send messages to listeners.
     * @param maxVoices The maximum number of voice channels that may 
     * ever be simultaneously in use by any client
     */
    public BasicVoiceGroup(long groupOid, Object association, VoiceSender voiceSender, int maxVoices) {
        this.groupOid = groupOid;
        this.association = association;
        this.voiceSender = voiceSender;
        this.maxVoices = maxVoices;
        members = new HashMap<Long, GroupMember>();
    }
    
    /**
     * Return the group oid of a voice group.
     * @return The groupOid of the voice group.
     */
    public long getGroupOid() {
        return groupOid;
    }
    
    /**
     * Determine if the oid is that of a member of the group.
     * @param memberOid The oid of possible group member.
     * @param authToken A string giving the voice client's
     * authentication credentials.
     * @return True if the memberOid is a member of the group or is
     * allowed to be a member of the group, and false otherwise.
     */
    public boolean addMemberAllowed(long memberOid) {
        // For now, just test that they are both non-zero
        if (memberOid == 0) {
            if (Log.loggingDebug)
                Log.debug("BasicVoiceGroup.addMemberAllowed: memberOid is zero, so member not allowed");
            return false;
        }
        if (allowedMembers != null) {
            boolean allowed = allowedMembers.contains(memberOid);
            if (!allowed && Log.loggingDebug)
                Log.debug("BasicVoiceGroup.addMemberAllowed: allowedMembers does not contain memberOid " + memberOid + ", so member not allowed");
            return allowed;
        }
        else
            return true;
    }
    
    /**
     * Add a member to a group
     * @param memberOid The oid of the newly-created member
     * @param memberCon The VoiceConnection object that embodies 
     * the connection to the voice server.
     * @return The newly-created group member.
     */
    public GroupMember addMember(long memberOid, VoiceConnection memberCon) {
        return addMember(memberOid, memberCon, getDefaultPriority(), true);
    }

    /**
     * Supply a list of oids of players allowed to be in the group
     * @param allowedMembers The set of oids of possible group members.
     */
    public void setAllowedMembers(Set<Long> allowedMembers) {
        this.allowedMembers = allowedMembers;
    }

    /**
     * Return the allowed members
     * @return A Set<Long> of allowed member oids.
     */
    public Set<Long> getAllowedMembers() {
        return allowedMembers;
    }
    
    /**
     * Create a member with the given oid, and associate it with memberCon.
     * @param memberOid The oid of the member.
     * @param memberCon The VoiceConnection object connecting the 
     * voice server with the client
     * @param priority The speaking priority that the member should be
     * assigned; speakers with higher priorities will be heard over
     * speakers with lower priorities.
     * @param allowedSpeaker  If true, the new member is allowed to 
     * speak in the group; if false they will not be heard by members 
     * by members of the group.
     */
    abstract public GroupMember addMember(long memberOid, VoiceConnection memberCon, int priority, boolean allowedSpeaker);

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
    public void onAfterAddMember(long memberOid, long groupOid, boolean allowedSpeaker, byte micVoiceNumber, boolean listenToYourself) {
        ExtensionMessage msg = new ExtensionMessage();
        msg.setMsgType(VoiceClient.MSG_TYPE_VOICE_MEMBER_ADDED);
        msg.setProperty("memberOid", memberOid);
        msg.setProperty("groupOid", groupOid);
        msg.setProperty("allowedSpeaker", allowedSpeaker);
        msg.setProperty("micVoiceNumber", (int)micVoiceNumber);
        msg.setProperty("listenToYourself", listenToYourself);
        voiceSender.sendExtensionMessage(msg);
    }

    /**
     * Return true if the group is a positional group; that is, if
     * membership in the group is determined by being near the
     * listener, and the priority of speakers is determined by how
     * near any speaker is.
     * @return True if the group is positional.
     */
    abstract public boolean isPositional();

    /**
     * Change a speaker from not speaking to speaking, or vice versa
     * @param speaker A GroupMember instance whose speaking state is to be changed.
     * @param add If true, change the speaker from not speaking to speaking, if 
     * false, from speaking to not speaking.
     */
    abstract protected void changeSpeaking(GroupMember speaker, boolean add);

    /**
     * Change a listener from not listening to listening, or vice versa
     * @param listener A GroupMember instance whose listening state is to be changed.
     * @param add If true, change the listener from not listening to listening, if 
     * false, from listening to not listening.
     */
    abstract protected void changeListening(GroupMember listener, boolean add);

    /**
     * The set of speakers who should now be heard by this listener
     * (may have) changed; send appropriate voice deallocates and allocates 
     * to reflect those changes
     * @param listener A GroupMember instance whose voices will be recomputed.
     */
    abstract protected void recomputeListenerVoices(GroupMember listener);

    /**
     * Sever all speaker/listener relationships involving the member, and 
     * remove the member identified by memberOid.  
     * @param memberOid The oid of the member to be removed from the group.
     * @return True member was found; false otherwise
     */
    public boolean removeMember(long memberOid) {
        if (Log.loggingDebug)
            Log.debug("BasicVoiceGroup.removeMember: For group " + groupOid + ", called to remove member " + memberOid);
        lock.lock();
        try {
            GroupMember member = isMember(memberOid);
            if (member != null) {
                if (member.allowedSpeaker)
                    setAllowedSpeaker(member, false);
                if (member.listening) {
                    setListener(member, false);
                }
                if (members.remove(memberOid) == null)
                    Log.error("BasicVoiceGroup.removeMember: For group " + groupOid + ", didn't find member " + memberOid + " in member map!");
                else if (Log.loggingDebug)
                    Log.debug("BasicVoiceGroup.removeMember: For group " + groupOid + ", removed member " + memberOid);
            }
            else
                Log.info("BasicVoiceGroup.removeMember: For group " + groupOid + ", member " + memberOid + " not found!");
            if (member != null)
                onAfterRemoveMember(memberOid, groupOid, member.allowedSpeaker);
            return member != null;
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * A call to this method is made after a removeMember() operation.
     * @param memberOid The oid of the newly-created member
     * @param groupOid The groupOid of the voice group.
     * @param allowedSpeaker True if the member is allowed to speak;
     * false otherwise.
     */
    public void onAfterRemoveMember(long memberOid, long groupOid, boolean allowedSpeaker) {
        ExtensionMessage msg = new ExtensionMessage();
        msg.setMsgType(VoiceClient.MSG_TYPE_VOICE_MEMBER_REMOVED);
        msg.setProperty("memberOid", memberOid);
        msg.setProperty("groupOid", groupOid);
        msg.setProperty("allowedSpeaker", allowedSpeaker);
        voiceSender.sendExtensionMessage(msg);
    }

    /**
     * Return the member identified by memberOid, or null if there is
     * no such member.
     * Does not need locking, since there are no modifications and the
     * status could change immediately after the call anyway.
     * @param memberOid The oid of the member to be returned
     * @return The member with memberOid, or null if it doesn't exist.
     */
    public GroupMember isMember(long memberOid) {
        return members.get(memberOid);
    }

    /**
     * Return the default priority of members
     * @return The default priority of newly-created members
     */
    public int getDefaultPriority() {
        return defaultPriority;
    }
    
    /**
     * Sets whether the member is allowed ot speak.
     * @param memberOid The oid of the member to change.
     * @param add True if the member should be allowed
     * to speak; false otherwise.
     */
    public void setAllowedSpeaker(long memberOid, boolean add) {
        lock.lock();
        try {
            GroupMember member = getMember(memberOid);
            if (member != null)
                setAllowedSpeaker(member, add);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Change whether a member is allowed to speak or not, and 
     * if it was formerly speaking, sever it's speaker to listener
     * relationships by sending voice channel deallocation messages.
     * @param member The GroupMember instance to change.
     * @param add True if the member should be allowed
     * to speak; false otherwise.
     */
    protected void setAllowedSpeaker(GroupMember member, boolean add) {
        lock.lock();
        try {
            if (member.allowedSpeaker == add)
                Log.error("BasicVoiceGroup.setAllowedSpeaker: Group " + groupOid + " member " + member.memberOid + 
                    ", add " + add + ".  Condition already true!");
            else {
//                 if (Log.loggingDebug)
//                     log.debug("BasicVoiceGroup.setAllowedSpeaker: memberOid " + member.memberOid + ", add " + add);
                if (add)
                    member.allowedSpeaker = true;
                else {
                    if (member.currentSpeaker)
                        changeSpeaking(member, false);
                    member.allowedSpeaker = false;
                }
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Does not need locking, since there are no modifications and the
     * status could change immediately after the call anyway.
     * @param memberOid The oid of the member whose allowed speaker 
     * status will be returned.
     * @return True if the member can be found in the group and 
     * is allowed to be a speaker; false otherwise.
     */
    public boolean isAllowedSpeaker(long memberOid) {
        GroupMember member = getMember(memberOid);
        if (member == null)
            return false;
        else
            return member.allowedSpeaker;
    }

    /**
     * Change a member that is allowed to speak from not speaking 
     * to speaking, or vice versa, and do whatever voice deallocation 
     * and/or allocation is required as a result.
     * @param memberOid The GroupMember instance to change.
     * @param add True if the member should be allowed
     * to speak; false otherwise.
     */
    public void setMemberSpeaking(long memberOid, boolean add) {
        lock.lock();
        try {
            GroupMember member = getMember(memberOid);
            if (member == null)
                Log.error("BasicVoiceGroup.setMemberSpeaking: memberOid " + memberOid + ", add " + add + " could not be found in group " + groupOid);
            else if (member.allowedSpeaker && member.currentSpeaker == add)
                Log.dumpStack("BasicVoiceGroup.setMemberSpeaking: Group " + groupOid + " member " + member.memberOid + 
                    ", add " + add + ".  Condition already true!");
            else if (add) {
                if (member.allowedSpeaker)
                    changeSpeaking(member, true);
            }
            else
                changeSpeaking(member, false);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Does not need locking, since there are no modifications and the
     * status could change immediately after the call anyway.
     * @param memberOid The oid of the member whose speaking 
     * status will be returned.
     * @return True if the member can be found in the group and 
     * is now speaking; false otherwise.
     */
    public boolean isMemberSpeaking(long memberOid) {
        GroupMember member = getMember(memberOid);
        if (member == null)
            return false;
        else
            return member.currentSpeaker;
    }

    /**
     * Does not need locking, since there are no modifications and the
     * status could change immediately after the call anyway.
     * @param memberOid The oid of the member whose listening 
     * status will be returned.
     * @return True if the member can be found in the group and 
     * is now listening to the group; false otherwise.
     */
    public boolean isListener(long memberOid) {
        GroupMember member = getMember(memberOid);
        if (member == null)
            return false;
        else
            return member.listening;
    }

    /**
     * Change the member with the given oid to be a listener if add is
     * true, or not a listener if add is false.  Sends out any 
     * required allocation or deallocation messages to adjust 
     * speaker/listener relationships.
     * @param memberOid The oid of the member whose whose listening status should
     * change.
     * @param add If true, make the member a listener in the group,
     * else stop the member listening.
     */
    public void setListener(long memberOid, boolean add) {
        lock.lock();
        try {
            GroupMember member = getMember(memberOid);
            if (member != null)
                setListener(member, add);
            else
                Log.error("BasicVoiceGroup.setListener: Group " + groupOid + " member " + memberOid + 
                    ", could not find member!");
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Change the member supplied to be a listener if add is
     * true, or not a listener if add is false.  Sends out any 
     * required allocation or deallocation messages to adjust 
     * speaker/listener relationships.
     * @param member The member whose whose listening status should
     * change.
     * @param add If true, make the member a listener in the group,
     * else stop the member listening.
     */
    public void setListener(GroupMember member, boolean add) {
        lock.lock();
        try {
            if (member.listening == add)
                Log.error("BasicVoiceGroup.setListener: Group " + groupOid + " member " + member.memberOid + 
                    ", add " + add + ".  Condition already true!");
            else {
                member.listening = add;
                changeListening(member, add);
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Terminate the voice channel, whose voice channel number is
     * voiceNumber, between the given speaker and listener by 
     * sending a deallocate message to the listener's client.
     * @param speaker The member that is the source of the voice channel.
     * @param listener The member that will receive the voice channel deallocation message.
     * @param voiceNumber The number of the voice channel opened to the client, unique
     * within an single client.
     */
    protected void endListeningToSpeaker(GroupMember speaker, GroupMember listener, byte voiceNumber) {
        listener.setSpeakerForVoiceNumber(voiceNumber, null);
        if (Log.loggingDebug)
            Log.debug("BasicVoiceGroup.endListeningToSpeaker: Sending dealloc of speaker " + speaker + 
                " to listener " + listener + ", voiceNumber " + voiceNumber);
        voiceSender.sendDeallocateVoice(speaker.memberCon, listener.memberCon, voiceNumber);
    }

    /**
     * A default implementation of a filter that determines if we 
     * are allowed to form a voice channel from the given speaker 
     * to the given listener.  This will likely be overriden by
     * many virtual world builder.
     * @param speaker The member that is the source of the voice channel.
     * @param listener The member that will receive the voice channel deallocation message.
     */
    protected boolean eligibleSpeakerListenerPair(GroupMember speaker, GroupMember listener) {
    	boolean sameMember = speaker == listener;
    	boolean sameOid = speaker.getMemberOid() == listener.getMemberOid();
    	if (sameOid && !sameMember)
    		Log.warn("BasicVoiceGroup.eligibleSpeakerListenerPair: Speaker and listener both have memberOid " +
                    speaker.getMemberOid() + " but they are not the same object. speaker.expunged " +
                    speaker.getExpunged() + ", listener.expunged " + listener.getExpunged());
        return !listener.speakerIgnored(speaker) && (!sameOid || speaker.memberCon.listenToYourself);
    }
    
    protected GroupMember getMember(long oid) {
        GroupMember member = members.get(oid);
        if (member == null) {
            return null;
        }
        else
            return member;
    }

    /**
     * Get all the members of the group.  Used by the VoicePlugin's
     * update thread to collect all the positional group members that
     * must be updated.
     */
    public void getAllMembers(List<GroupMember> memberList) {
        memberList.addAll(members.values());
    }
    
    /** 
     * Send a voice frame originating with the speaker with the given
     * oid, and contained in the first dataSize bytes of buf, to all
     * eligible listeners
     * @param speakerOid The oid of the speaker that is the source of the voice frame(s)
     * @param buf The buffer containing data packet; typically a data aggregation packet
     * @param opcode The opcode to use when sending the frame(s).  Typically
     * VoicePlugin.opcodeAggregatedData.
     * @param pktSize The size of the payload.
     */
    public void sendVoiceFrameToListeners(long speakerOid, MVByteBuffer buf, byte opcode, int pktSize) {
        lock.lock();
        try {
            GroupMember speaker = (GroupMember)getMember(speakerOid);
            if (speaker != null && speaker.allowedSpeaker) {
                List<GroupMember> listenersToSpeaker = speaker.membersListeningToSpeaker();
                for (GroupMember listener : listenersToSpeaker) {
                    if (eligibleSpeakerListenerPair(speaker, listener)) {
                        Byte voiceNumber = listener.findVoiceNumberForSpeaker(speaker);
                        if (voiceNumber == null)
                            Log.error("PositionalVoiceGroup.sendVoiceFrameToListeners: Voice number for speaker " + 
                                speaker + " and listener " + listener + " is null!");
                        else
                            voiceSender.sendVoiceFrame(speaker.memberCon, listener.memberCon, 
                                opcode, voiceNumber, buf, (short)pktSize);
                    }
                }
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * This is central method that recalculates the voices that should
     * be heard by a listener, based on a potential set of voices
     * currently speaking, sending out allocations and deallocations 
     * as a result.
     * @param listener The group member, currently a listener, that should 
     * have it's voice channels recomputed.
     * @param memberIterator An iterator used to run down the group members 
     * in "priority order".
     * @param count The number of elements from the iterator to consider.
     */
    protected void recomputeVoicesFromSpeakerIterator(GroupMember listener, Iterator<GroupMember> memberIterator, int count) {
//         if (Log.loggingDebug)
//             Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: Entered method; listener " + listener + ", count " + count);
        if (!listener.listening) {
            for (byte i=0; i<maxVoices; i++) {
                GroupMember speaker = listener.getSpeakerForVoiceNumber(i);
                if (speaker != null && eligibleSpeakerListenerPair(speaker, listener))
                    // Does the elgibleSpeakerListenerPair call ensure
                    // that we continue to get our own voice frames when
                    // in "listen to ourselves" mode?  If not, why is it there?
                    endListeningToSpeaker(speaker, listener, i);
            }
            if (Log.loggingDebug)
                Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: Returning because !listener.listening");
            return;
        }
        GroupMember[] newVoiceNumberToMember = new GroupMember[maxVoices];

        // Mark current speakers heard by this listener in priority order
        byte priorityCount = 0;
        while (priorityCount < count && memberIterator.hasNext()) {
            GroupMember speaker = memberIterator.next();
            if (loggingRecomputeVoices && Log.loggingDebug)
                Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: In while loop; priorityCount " +
                    priorityCount + ", speaker " + speaker + ", listener " + listener);
            // Don't include ineligible speakers
            if (eligibleSpeakerListenerPair(speaker, listener)) {
                if (loggingRecomputeVoices && Log.loggingDebug)
                    Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: Eligible!: equal " + (speaker == listener) + ", speaker listenToYourself " + speaker.memberCon.listenToYourself);
               newVoiceNumberToMember[priorityCount] = speaker;
               speaker.priorityIndex = priorityCount;
               priorityCount++;
            }
        }
        // Deallocate any speakers for this listener that didn't get encountered by priority marking
        int deallocCount = 0;
        for (byte voiceNumber=0; voiceNumber<maxVoices; voiceNumber++) {
            GroupMember speaker = listener.getSpeakerForVoiceNumber(voiceNumber);
            if (speaker != null) {
                // Does the elgibleSpeakerListenerPair call ensure
                // that we continue to get our own voice frames when
                // in "listen to ourselves" mode?  If not, why is it there?
                if (speaker.priorityIndex == -1 && eligibleSpeakerListenerPair(speaker, listener)) {
                    endListeningToSpeaker(speaker, listener, voiceNumber);
                    deallocCount++;
                }
            }
        }
        if (Log.loggingDebug)
            Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: listener " + listener + 
                ", count " + count + ", deallocCount " + deallocCount + ", priorityCount " + priorityCount);
        // Now on behalf of this listener, allocate voice numbers for
        // the speakers left in the priority queue, in they aren't
        // already allocated
        for (int i=0; i<priorityCount; i++) {
            GroupMember speaker = newVoiceNumberToMember[i];
            if (speaker == null) {
                Log.error("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: speaker newVoiceNumberToMember[" + i + "] is null!");
                continue;
            }
            // Fix up for next call to this method
            speaker.priorityIndex = -1;
            if (loggingRecomputeVoices && Log.loggingDebug)
                Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: listener " + listener + ", speaker[" + i + "] " + speaker);
            if (listener.findVoiceNumberForSpeaker(speaker) == null) {
                Byte voiceNumber = listener.findFreeVoiceNumber();
                if (loggingRecomputeVoices && Log.loggingDebug)
                    Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: For speaker " + 
                        speaker + ", found voiceNumber " + voiceNumber);
                if (voiceNumber != null) {
                    listener.setSpeakerForVoiceNumber(voiceNumber, speaker);
                    voiceSender.sendAllocateVoice(speaker.memberCon, listener.memberCon, voiceNumber, isPositional());
                }
                else
                    Log.error("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: In listener " + listener + 
                            ", didn't find unused voiceNumber for speaker " + speaker.memberCon);
            }
        }
        if (loggingRecomputeVoices && Log.loggingDebug)
            Log.debug("BasicVoiceGroup.recomputeVoicesFromSpeakerIterator: Exiting for listener " + listener);
    }

    /**
     * Utility method used in logging.
     */
    protected String addString(boolean add) {
        return add ? "start" : "stop";
    }

    /**
     * Make a string 
     */
    public String toString() {
        return "group[oid " + groupOid + "]";
    }
    
    /**
     * The oid of the group.
     */
    protected long groupOid;

    /**
     * An Object-valued slot, not used by BasicVoiceManager,
     * to allow derived classes to provide an assoiciated object.
     */
    protected Object association;
    
    /**
     * The instance used to send messages to listeners.
     */
    protected VoiceSender voiceSender = null;

    /**
     * The maximum number of voice channels any single client can have
     * simultaneously transmitting to the client.
     */
    protected int maxVoices;

    /**
     * The default priority of newly-created members of the group.
     */
    public int defaultPriority = 0;

    /**
     * A set of longs representing the oids of players allowed to join
     * the group.  If null, then anyone can join the group, or 
     * method addMemberAllowed() got overridden.
     */
    protected Set<Long> allowedMembers = null;
    
    /**
     * A map from member oid to GroupMember instance for this group.
     */
    protected Map<Long, GroupMember> members;

    /**
     * To enable detailed logging of the recompute voices algorithm.
     */
    protected static boolean loggingRecomputeVoices = false;
    
    /**
     * A lock used by operations on the group
     */
    protected transient Lock lock = LockFactory.makeLock("BasicVoiceGroup");

}
