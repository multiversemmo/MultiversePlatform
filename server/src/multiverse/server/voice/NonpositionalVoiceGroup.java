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
import multiverse.server.util.*;


/**
 * This class implements VoiceGroup interface, and is used to
 * implement two important cases of voice groups: presentations
 * and "raid groups".  In a non-positional voice group, all listeners
 * in the group hear the same set of speakers.
 */
public class NonpositionalVoiceGroup extends BasicVoiceGroup {

    /**
     * Create a NonpositionalVoiceGroup
     * @param groupOid The oid of the group, which is unique across all 
     * voice groups.
     * @param association An object-valued data member that is unused
     * in BasicVoiceGroup, but available to derived classes.
     * @param voiceSender The abstraction that allows the voice group to 
     * send messages to listeners.
     * @param maxVoices The maximum number of voice channels that may 
     * ever be simultaneously in use by any client
     */
    public NonpositionalVoiceGroup(long groupOid, Object association, VoiceSender voiceSender, int maxVoices) {
        super(groupOid, association, voiceSender, maxVoices);
        listeners = new HashSet<GroupMember>();
        comparePriorities = new ComparePriorities();
        currentSpeakers = new LinkedList<GroupMember>();
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
    public GroupMember addMember(long memberOid, VoiceConnection memberCon, int priority, boolean allowedSpeaker) {
        lock.lock();
        try {
            GroupMember member = members.get(memberOid);
            if (member != null)
                Log.dumpStack("NonpositionalVoiceGroup.addMember: Member " + memberOid + " is already a member of voice group " + groupOid);
            else {
                member = new GroupMember(this, memberOid, priority, allowedSpeaker, false, memberCon, maxVoices);
                members.put(memberOid, member);
            }
            onAfterAddMember(memberOid, groupOid, allowedSpeaker, memberCon.micVoiceNumber, memberCon.listenToYourself);
            return member;
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Comparator class that compares GroupMembers by priority, and then by index.
     */
    public static class ComparePriorities implements Comparator<GroupMember> {

        public int compare(GroupMember m1, GroupMember m2) {
            if (m1 == m2)
                return 0;
            else if (m1.priority < m2.priority)
                return -1;
            else if (m1.priority > m2.priority)
                return 1;
            else                
                // The priorities are equal, so just compare their
                // index, which are guaranteed to be distinct.
                return (m1.index < m2.index ? -1 : 1);
        }
        
        public boolean equals(Object other) {
            return this == other;
        }
    }
    
    /**
     * @return False, because this is a non-positional voice group.
     */
    public boolean isPositional() {
        return false;
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
     * Change a speaker from not speaking to speaking, or vice versa
     * @param speaker A GroupMember instance whose speaking state is to be changed.
     * @param add If true, change the speaker from not speaking to speaking, if 
     * false, from speaking to not speaking.
     */
    protected void changeSpeaking(GroupMember speaker, boolean add) {
        if (Log.loggingDebug)
            Log.debug("NonpositionalVoiceGroup.changeSpeaking entering " + addString(add) + ": listeners.size() " + 
                listeners.size() + ", speaker " + speaker + ", speaker.voiceNumber " + speaker.voiceNumber);
        lock.lock();
        try {
            speaker.currentSpeaker = add;
            if (add) {
                if (!currentSpeakers.add(speaker))
                    Log.error("NonpositionalVoiceGroup.changeSpeaking start: currentSpeakers already contains speaker " + speaker);
            }
            else {
                if (!currentSpeakers.remove(speaker))
                    Log.error("NonpositionalVoiceGroup.changeSpeaking stop: currentSpeakers doesn't contain speaker " + speaker);
            }
            speakingStatusChanged();
        }
        finally {
            lock.unlock();
        }
    }
    
    /**
     * Change a listener from not listening to listening, or vice versa
     * @param listener A GroupMember instance whose listening state is to be changed.
     * @param add If true, change the listener from not listening to listening, if 
     * false, from listening to not listening.
     */
    protected void changeListening(GroupMember listener, boolean add) {
        if (Log.loggingDebug)
            Log.debug("NonpositionalVoiceGroup.changeListening " + addString(add) + ": listener " + listener);
        lock.lock();
        try {
            if (add) {
                if (!listeners.add(listener))
                    Log.error("NonpositionalVoiceGroup.changeListening " + addString(add) + ": listener " + listener + " already in listeners");
                recomputeListenerVoices(listener);
            }
            else {
                if (!listeners.remove(listener))
                    Log.error("NonpositionalVoiceGroup.changeListening " + addString(add) + ": listener " + listener + " not in listeners");
                for (byte voiceNumber=0; voiceNumber<maxVoices; voiceNumber++) {
                    GroupMember speaker = listener.getSpeakerForVoiceNumber(voiceNumber);
                    if (speaker != null)
                        endListeningToSpeaker(speaker, listener, voiceNumber);
                }
                if (listener.voiceCount() > 0)
                    Log.dumpStack("NonpositionalVoiceGroup.changeListening stop: After removing, listener.voiceCount() " + 
                        listener.voiceCount());
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Recompute the voice channels to be sent to the specified listener.
     * @param listener The group member for which voice channels are to be recomputed.
     */
    public void recomputeListenerVoices(GroupMember listener) {
        recomputeListenerVoices(listener, currentSpeakers);
    }

    /**
     * Recompute the voice channels to be sent to the specified listener, where
     * speakers are chosen from membersToConsider.
     * @param listener The group member for which voice channels are to be recomputed.
     * @param membersToConsider The list of speakers.
     */
    protected void recomputeListenerVoices(GroupMember listener, List<GroupMember> membersToConsider) {
        Iterator<GroupMember> speakerIter = membersToConsider.iterator();
        int speakerCount = membersToConsider.size();
        int iterCount = Math.min(speakerCount, maxVoices);
        recomputeVoicesFromSpeakerIterator(listener, speakerIter, iterCount);
    }
    
    /**
     * Some speaker change from speaking to not speaker, or vice versa.
     * So recompute the voices for all listeners.
     */
    protected void speakingStatusChanged() {
        // Lock the lock, and sort the list.
        lock.lock();
        try {
            Collections.sort(currentSpeakers, comparePriorities);
        }
        finally {
            lock.unlock();
        }
        for (GroupMember listener : listeners)
            recomputeListenerVoices(listener);
    }

    /**
     * The listeners for this non-positional voice group.  Most of the time
     * all members are listeners.
     */
    protected Set<GroupMember> listeners;
    
    /**
     * The list of speakers for the group.  All listeners hear the same set
     * of speakers, which are typically a smaller subset of the set of members.
     */
    private List<GroupMember> currentSpeakers;

    /**
     * The instance used to compare member priorities.
     */
    private Comparator<GroupMember> comparePriorities;
}
