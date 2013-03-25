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
import multiverse.server.math.*;
import multiverse.server.engine.*;


/**
 * This class implements VoiceGroup interface, and is used to
 * implement positional voice groups.  In a positional voice group,
 * each listener hears the players who are nearby, and the speaker 
 * volume falls off as the distance between speaker and listener 
 * increases.
 */
public class PositionalVoiceGroup extends BasicVoiceGroup {

    /**
     * Create a PositionalVoiceGroup
     * @param groupOid The oid of the group, which is unique across all 
     * voice groups.
     * @param association An object-valued data member that is unused
     * in BasicVoiceGroup, but available to derived classes.
     * @param voiceSender The abstraction that allows the voice group to 
     * send messages to listeners.
     * @param maxVoices The maximum number of voice channels that may 
     * ever be simultaneously in use by any client
     * @param audibleRadius The maximum distance in millimeters between players in
     * a positional voice group at which they can hear each other, adjusted by
     * the hystericalMargin.
     * @param hystericalMargin The distance in millimeters of hysteresis in 
     * initiating or terminating a pair of players hearing each other.  The
     * hysterical margin ensures that when a player moves back and forth slightly, 
     * we don't continuously initiate and terminate the voice channel to other
     * players.
     */
    public PositionalVoiceGroup(long groupOid, Object association, VoiceSender voiceSender, int maxVoices, float audibleRadius, float hystericalMargin) {
        super(groupOid, association, voiceSender, maxVoices);
        this.audibleRadius = audibleRadius;
        this.hystericalMargin = hystericalMargin;
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
                Log.dumpStack("PositionalVoiceGroup.addMember: Member " + memberOid + " is already a member of voice group " + groupOid);
            else {
                member = new PositionalGroupMember(this, memberOid, priority, allowedSpeaker, false, memberCon, maxVoices);
                members.put(memberOid, member);
                if (Log.loggingDebug)
                    Log.debug("PositionalVoiceGroup.addMember: For group " + groupOid + ", adding member " + memberOid);
            }
            onAfterAddMember(memberOid, groupOid, allowedSpeaker, memberCon.micVoiceNumber, memberCon.listenToYourself);
            return member;
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Change a speaker from not speaking to speaking, or vice versa
     * @param gspeaker A GroupMember instance whose speaking state is to be changed.
     * @param add If true, change the speaker from not speaking to speaking, if 
     * false, from speaking to not speaking.
     */
    protected void changeSpeaking(GroupMember gspeaker, boolean add) {
        PositionalGroupMember speaker = (PositionalGroupMember)gspeaker;
        if (Log.loggingDebug)
            Log.debug("PositionalVoiceGroup.changeSpeaking " + addString(add) + ": speaker " + speaker);
        lock.lock();
        try {
            speaker.currentSpeaker = add;
            if (add)
                recomputeListenersInRadius(speaker);
            else {
                List<GroupMember> listenersToSpeaker = speaker.membersListeningToSpeaker();
                for (GroupMember listener : listenersToSpeaker) {
                    Byte voiceNumber = listener.findVoiceNumberForSpeaker(speaker);
                    if (Log.loggingDebug)
                        Log.debug("PositionalVoiceGroup.changeSpeaking " + addString(add) + ": listeners cnt " + listenersToSpeaker.size() +
                            ", speaker " + speaker + ", voiceNumber " + voiceNumber + ", listener " + listener);
                    if (voiceNumber == null)
                        Log.error("PositionalVoiceGroup.changeSpeaking " + addString(add) + ": Voice number for speaker " + 
                            speaker + " and listener " + listener + " is null!");
                    else
                        recomputeListenerVoices(listener);
                }
            }
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
            Log.debug("PositionalVoiceGroup.changeListening " + addString(add) + ": listener " + listener);
        lock.lock();
        try {
            if (add)
                recomputeListenerVoices(listener);
            else {
                for (byte voiceNumber=0; voiceNumber<maxVoices; voiceNumber++) {
                    GroupMember speaker = listener.getSpeakerForVoiceNumber(voiceNumber);
                    if (speaker != null)
                        endListeningToSpeaker(speaker, listener, voiceNumber);
                }
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Called by the voice plugin when an UpdateWorldNode message is received for a
     * player in the group.
     * @param perceiverMember The member whose position/direction/orientation should be updated.
     * @param bwnode The BasicWorldNode containing the new position/direction/orientation.
     */
    public void updateWorldNode(PositionalGroupMember perceiverMember, BasicWorldNode bwnode) {
        if (perceiverMember.wnode != null) {
            perceiverMember.previousLoc = perceiverMember.lastLoc;
            perceiverMember.wnode.setDirLocOrient(bwnode);
            perceiverMember.wnode.setInstanceOid(bwnode.getInstanceOid());
            perceiverMember.lastLoc = perceiverMember.wnode.getLoc();
            for (long perceivedOid : perceiverMember.perceivedOids) {
                if (perceiverMember.memberOid == perceivedOid)
                    continue;
                PositionalGroupMember perceivedMember = (PositionalGroupMember)getMember(perceivedOid);
                if (perceivedMember != null && perceivedMember.wnode != null)
                    testProximity(perceiverMember, perceivedMember, false, true);
            }
        }
        else
            Log.error("PositionalVoiceGroup.updateWorldNode: In UpdateWorldNodeMessage for oid " + 
                perceiverMember.memberOid + ", perceiverMember.wnode is null!");
    }

    /**
     * Test if the perceived object has come in or out of range of the
     * perceiver object; if so, we change the inRangeOids set for the
     * perceiver and the perceived members.
     * @param perceiverMember The member who received the UpdateWorldNodeMessage or PerceptionMessage
     * @param perceivedMember A member in the perceivedOids set of the perceiverMember.
     * @param interpolatePerceiver If true, we should interpolate the location of the perceiver;
     * if false, we should use the lastLoc of the perceiver.
     * @param interpolatePerceived If true, we should interpolate the location of the perceived;
     * if false, we should use the lastLoc of the perceived.
     */
    public void testProximity(PositionalGroupMember perceiverMember, PositionalGroupMember perceivedMember,
        boolean interpolatePerceiver, boolean interpolatePerceived) {
        Point perceiverLoc = interpolatePerceiver ? perceiverMember.wnode.getLoc() : perceiverMember.lastLoc;
        Point perceivedLoc = interpolatePerceived ? perceivedMember.wnode.getLoc() : perceivedMember.lastLoc;
        if (perceiverLoc == null) {
            Log.dumpStack("PositionalVoiceGroup.testProximity: perceiver " + perceiverMember.getMemberOid() + " loc is null!");
            return;
        }
        if (perceivedLoc == null) {
            Log.dumpStack("PositionalVoiceGroup.testProximity: perceived " + perceivedMember.getMemberOid() + " loc is null!");
            return;
        }
        float distance = Point.distanceTo(perceiverLoc, perceivedLoc);
        long perceiverInstance = perceiverMember.wnode.getInstanceOid();
        long perceivedInstance = perceivedMember.wnode.getInstanceOid();
        boolean sameInstance = perceiverInstance == perceivedInstance;
        boolean inRadius = sameInstance && (distance < audibleRadius);
        boolean wasInRadius = perceiverMember.membersInRadius.contains(perceivedMember);
        if (Log.loggingDebug)
            Log.debug("PositionalVoiceGroup.testProximity: perceiver " + perceiverMember.getMemberOid() + ", perceiverLoc = " + perceiverLoc + 
                ", perceived " + perceivedMember.getMemberOid() + ", perceivedLoc = " + perceivedLoc + 
                ", distance " + distance + ", audibleRadius " + audibleRadius + ", perceiverInstance " + perceiverInstance +
                ", perceivedInstance " + perceivedInstance + ", inRadius " + inRadius + ", wasInRadius " + wasInRadius);
        if (inRadius == wasInRadius)
            return;
        if (sameInstance && hystericalMargin != 0f) {
            if (wasInRadius)
                inRadius = distance < (audibleRadius + hystericalMargin);
            else
                inRadius = distance < (audibleRadius - hystericalMargin);
            // If they are the same after hysteresis was applied, skip.
            if (inRadius == wasInRadius)
                return;
        }
        handlePositionalSpeakerChange(perceiverMember, perceivedMember, inRadius);
        handlePositionalSpeakerChange(perceivedMember, perceiverMember, inRadius);
    }

    /**
     * Handle a positional speaker moving in and out of the audible
     * radius of positional listener.
     * @param speaker The speaker member whose position has changed.
     * @param listener The listener member who may be inside the
     * radius of the speaker.
     * @param inRadius True if the speaker and listener are now in
     * listening range of each other; false otherwise.
     */
    public void handlePositionalSpeakerChange(PositionalGroupMember speaker, PositionalGroupMember listener, boolean inRadius) {
        lock.lock();
        try {
            // If either is not a member of this group, ignore the change
            if (Log.loggingDebug)
                Log.debug("PositionalVoiceGroup.handlePositionalSpeakerChange: speakerOid " + speaker.memberOid + 
                    ", speaker " + speaker + ", listenerOid " + listener.memberOid + ", listeneer " + listener + ", inRadius " + inRadius);
            if (inRadius && !listener.speakerIgnored(speaker))
                addSpeakerListenerPair(speaker, listener);
            else 
                removeSpeakerListenerPair(speaker, listener);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * @return True, because this is a positional voice group.
     */
    public boolean isPositional() {
        return true;
    }

    /**
     * Create a positional speaker/listener pair.
     * @param speaker The speaker group member of the pair.
     * @param listener The listener group member of the pair.
     */
    protected void addSpeakerListenerPair(PositionalGroupMember speaker, PositionalGroupMember listener) {
        Set<PositionalGroupMember> membersInRadius = listener.membersInRadius;
        if (Log.loggingDebug)
            Log.debug("PositionalVoiceGroup.addSpeakerListenerPair: speaker + " + speaker +
                ", listener " + listener + ", membersInRadius.size() " + membersInRadius.size());
        if (speaker.getExpunged()) {
            Log.warn("PositionalVoiceGroup.addSpeakerListenerPair: For listener " + listener.getMemberOid() + 
                ", speaker " + speaker.getMemberOid() + " is expunged!");
            return;
        }
        if (listener.getExpunged()) {
            Log.warn("PositionalVoiceGroup.addSpeakerListenerPair: For speaker " + speaker.getMemberOid() + 
                ", listener " + listener.getMemberOid() + " is expunged!");
            return;
        }
        if (!membersInRadius.add(speaker)) {
            if (Log.loggingDebug)
                Log.debug("PositionalVoiceGroup.addSpeakerListenerPair: listener " + listener +
                    " already in membersInRadiusOfSpeakerMap for speaker " + speaker);
        }
        else
            recomputeListenerVoices(listener);
    }

    /**
     * Remove a positional speaker/listener pair.
     * @param speaker The speaker group member of the pair.
     * @param listener The listener group member of the pair.
     */
    protected void removeSpeakerListenerPair(PositionalGroupMember speaker, PositionalGroupMember listener) {
        Set<PositionalGroupMember> membersInRadius = listener.membersInRadius;
        boolean found = false;
        if (membersInRadius != null) {
            if (membersInRadius.remove(speaker))
                found = true;
        }
        if (!found) {
            if (Log.loggingDebug)
                Log.debug("PositionalVoiceGroup.removeSpeakerListenerPair: listener " + listener +
                    " is not in membersInRadiusOfSpeakerMap for speaker " + speaker);
        }
        else if (Log.loggingDebug)
            Log.debug("PositionalVoiceGroup.removeSpeakerListenerPair: listener " + listener + " removed from membersInRadius of speaker " + speaker);
        recomputeListenerVoices(listener);
    }

    /**
     * For each listener in radius of the speaker, recompute the
     * voices for the listener.
     * @param speaker The speaker group member of the pair.
     */
    protected void recomputeListenersInRadius(PositionalGroupMember speaker) {
        for (PositionalGroupMember listener : speaker.membersInRadius) {
            if (listener.listening)
                recomputeListenerVoices(listener);
        }
    }

    /**
     * Recompute the voice channels for the given listener
     * @param glistener The listener group member whose voice channels will be recomputed.
     */
    protected void recomputeListenerVoices(GroupMember glistener) {
        PositionalGroupMember listener = (PositionalGroupMember)glistener;
        // Take this opportunity to remove any expunged members
        List<PositionalGroupMember> expungedMembers = null;
        for (PositionalGroupMember member : listener.membersInRadius) {
        	if (member.getExpunged()) {
        		if (expungedMembers == null)
        			expungedMembers = new LinkedList<PositionalGroupMember>();
        		if (Log.loggingDebug)
                    Log.debug("PositionalVoiceGroup:recomputeListenerVoices: listener " + listener.getMemberOid() + 
                        " memberInRadius " + member.getMemberOid() + " is expunged; removing.");
        		expungedMembers.add(member);
        	}
        }
        if (expungedMembers != null)
        	listener.membersInRadius.removeAll(expungedMembers);
        recomputeListenerVoices(listener, listener.membersInRadius);
    }

    /**
     * Recompute the voice channels for the given listener, drawing
     * potential speakers from the membersToConsider.
     * @param listener The listener group member whose voice channels will be recomputed.
     * @param membersToConsider The set of speakers within radius from the listener.
     */
    protected void recomputeListenerVoices(PositionalGroupMember listener, Set<PositionalGroupMember> membersToConsider) {
        if (Log.loggingDebug)
            Log.debug("PositionalVoiceGroup.recomputeListenerVoices: listener " + listener + 
                ", membersToConsider.size() " + membersToConsider.size());
        List<GroupMember> currentSpeakersForListener = new LinkedList<GroupMember>();
        for (PositionalGroupMember speaker : membersToConsider) {
            if (speaker.currentSpeaker)
                currentSpeakersForListener.add(speaker);
        }
        Collections.sort(currentSpeakersForListener, new CompareLocations(listener.getCurrentLoc()));
        Iterator<GroupMember> speakerIter = currentSpeakersForListener.iterator();
        int speakerCount = currentSpeakersForListener.size();
        int iterCount = Math.min(speakerCount, maxVoices);
        recomputeVoicesFromSpeakerIterator(listener, speakerIter, iterCount);
    }
    
    /**
     * Allocate a listener voice number for the given sound
     * speaker.  This doesn't send a message; just modifies the
     * lists of listeners.
     * @param speaker The speaker group member of the speaker/listener pair.
     * @param listener The listener group member of the speaker/listener pair.
     * @return The voiceNumber of the added voice from the point of
     * view of the listener.
     */
    public byte addListenerVoice(PositionalGroupMember speaker, PositionalGroupMember listener) {
        if (Log.loggingDebug)
            Log.debug("PositionalGroupMember.addListenerVoice:  speaker " + speaker + ", listener " + listener);
        lock.lock();
        try {
            Byte voiceNumber = listener.findFreeVoiceNumber();
            if (voiceNumber == null) {
                Log.error("PositionalGroupMember.addListenerVoice: Too many voices allocating voice for member " + listener);
                return -1;
            }
            listener.setSpeakerForVoiceNumber(voiceNumber, speaker);
            return voiceNumber;
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Return a list of members listening to this member.
     * Currently unused
     * @param speaker The speaker group member whose listeners should be returned.
     * @return The list of members listening to the speaker.
     */
    public List<GroupMember> membersListeningToSpeaker(PositionalGroupMember speaker) {
        lock.lock();
        try {
            return speaker.membersListeningToSpeaker();
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Return true if the listener is now listening to the
     * speaker.
     * Currently unused
     * @param speaker The speaker group member of the pair.
     * @param listener The listener group member of the pair.
     * @return True if the listener is now listening to the speaker.
     */
    public boolean nowListeningTo(PositionalGroupMember speaker, PositionalGroupMember listener) {
        lock.lock();
        try {
            return listener.nowListeningTo(speaker);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Called by the VoicePlugin when an instance is terminated.  At that point, there
     * should be no members in the instance.
     * @param instanceOid The oid of the instance being unloaded.
     */
    public void unloadInstance(long instanceOid) {
        lock.lock();
        try {
            Set<PositionalGroupMember> members = instanceMembers.remove(instanceOid);
            if (members != null && members.size() > 0) {
                Log.warn("PositionalVoiceGroup.unloadInstance: Group " + groupOid + " in instance " + instanceOid + 
                        ", has active members " + makeOidStringFromMembers(members));
                for (PositionalGroupMember member : members)
                    clearMembersPerceived(member);
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Called by the VoicePlugin to indicate that a member's location
     * should be tracked when a member is added to a positional group
     * @param perceiverMember The member to be tracked.
     * @param instanceOid The oid of the instance to which the member belongs.
     */
    public void addTrackedPerceiver(PositionalGroupMember perceiverMember, long instanceOid) {
        lock.lock();
        try {
            // Add from the list of tracked members for this instance
            Set<PositionalGroupMember> members = instanceMembers.get(instanceOid);
            if (members == null) {
                members = new HashSet<PositionalGroupMember>();
                instanceMembers.put(instanceOid, members);
            }
            if (!members.add(perceiverMember))
                Log.error("PositionalVoiceGroup.addTrackedPerceiver: Member " + perceiverMember.getMemberOid() + 
                    " is already a member of group " + groupOid + ", instanceOid " + instanceOid);
            else
                clearMembersPerceived(perceiverMember);
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Return a string of comma-separated oids from a collection of
     * oids, used for logging.
     * @param oids A collection of oids to be formatted.
     * @return A string containing the comma-separated oids.
     */
    protected String makeOidString(Collection<Long> oids) {
        String oidString = "";
        for (long oid : oids) {
            if (oidString.length() > 0)
                oidString += ", ";
            oidString += oid;
        }
        return oidString;
    }

    /**
     * Return a string of comma-separated oids from a collection of
     * members, used for logging.
     * @param members A collection of members whose oids are to be formatted.
     * @return A string containing the comma-separated oids.
     */
    protected String makeOidStringFromMembers(Collection<PositionalGroupMember> members) {
        String oidString = "";
        for (PositionalGroupMember member : members) {
            if (oidString.length() > 0)
                oidString += ", ";
            oidString += member.getMemberOid();
        }
        return oidString;
    }
    
    /**
     * Clear the set of perceivedOids and membersInRadius for a
     * member, but if the member itself is in the membersInRadius, add
     * it back, because this is how the listenToYourself mechanism
     * works.
     * @param perceiverMember The member whose perceivedOids and membersInRadius sets should be cleared.
     */
    protected void clearMembersPerceived(PositionalGroupMember perceiverMember) {
        boolean listeningToHimself = perceiverMember.membersInRadius.contains(perceiverMember);
        perceiverMember.perceivedOids.clear();
        perceiverMember.membersInRadius.clear();
        if (listeningToHimself)
            perceiverMember.membersInRadius.add(perceiverMember);
        recomputeListenerVoices(perceiverMember);
    }

    /**
     * Called by the VoicePlugin to indicate that a member's location
     * should be no longer be tracked when a member is removed from a
     * positional group, or the member changes instances.
     * @param perceiverOid The oid of the member that should no longer be tracked.
     */
    public void removeTrackedPerceiver(long perceiverOid) {
        PositionalGroupMember perceiverMember = (PositionalGroupMember)isMember(perceiverOid);
        if (perceiverMember != null)
            removeTrackedPerceiver(perceiverMember);
        else
            Log.error("PositionalVoiceGroup.removeTrackedPerceiver: Could not find member " + perceiverOid);
    }

    /**
     * Called by the VoicePlugin to indicate that a member's location
     * should be no longer be tracked when a member is removed from a
     * positional group, or the member changes instances.
     * @param perceiverMember The member that should no longer be tracked.
     */
    public void removeTrackedPerceiver(PositionalGroupMember perceiverMember) {
        lock.lock();
        try {
            Long instanceOid = perceiverMember.getInstanceOid();
            if (instanceOid != null) {
                perceiverMember.wnode = null;
                perceiverMember.lastLoc = null;
                perceiverMember.previousLoc = null;
                // Remove from the list of tracked members for this instance
                Set<PositionalGroupMember> members = instanceMembers.get(instanceOid);
                if (members == null)
                    Log.error("PositionalVoiceGroup.removeTrackedPerceiver: For perceiver " + 
                        perceiverMember.getMemberOid() + ", instanceMembers.get(" + instanceOid + ") is null!");
                else if (!members.remove(perceiverMember))
                    Log.error("PositionalVoiceGroup.removeTrackedPerceiver: Member " + perceiverMember.getMemberOid() + 
                        " is not a member of group " + groupOid + ", instanceOid " + instanceOid);
                long perceiverOid = perceiverMember.getMemberOid();
                // Now iterate over the perceived oids, removing from the other side
                for (long perceivedOid : perceiverMember.perceivedOids) {
                    PositionalGroupMember perceivedMember = (PositionalGroupMember)isMember(perceivedOid);
                    if (perceivedMember != null) {
                        perceivedMember.membersInRadius.remove(perceiverOid);
                        if (!perceivedMember.perceivedOids.remove(perceiverOid)) {
                            if (Log.loggingDebug)
                                Log.debug("PositionalVoiceGroup.removeTrackedPerceiver: Member " + perceiverMember.getMemberOid() + 
                                    " is not perceived by perceived member " + perceivedOid);
                        }
                        recomputeListenerVoices(perceivedMember);
                    }
                    else if (Log.loggingDebug)
                        Log.debug("PositionalVoiceGroup.removeTrackedPerceiver: Member " + perceivedOid + " could not be found!");
                }
                clearMembersPerceived(perceiverMember);
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Called by the VoicePlugin in response to a PerceptionMessage to
     * indicate that another member should be added or removed from
     * the member's perceivedOids.
     * @param perceiverMember The member whose perceivedOids should be adjusted.
     * @param perceivedOid The oid of the member that should be added
     * or removed from the perceiverMember's perceivedOids.
     * @param added If true, the perceivedOid should be added; if false, removed.
     */
    public void maybeChangePerceivedObject(PositionalGroupMember perceiverMember, long perceivedOid, boolean added) {
        long perceiverOid = perceiverMember.getMemberOid();
        if (Log.loggingDebug)
            Log.debug("PositionalVoiceGroup.maybeChangePerceivedObject: " + (added ? "gain" : "loss") + ", oid=" + perceivedOid +
                " detected by " + perceiverOid + ", instanceOid=" + perceiverMember.getInstanceOid());
        lock.lock();
        try {
            if (added) {
                if (!perceiverMember.perceivedOids.add(perceivedOid))
                    Log.error("PositionalVoiceGroup.maybeChangePerceivedObject: Adding member " + perceivedOid +
                        " for perceiver " + perceiverOid + "; already in perceivedOids");
            }
            else {
                if (!perceiverMember.perceivedOids.remove(perceivedOid)) {
//                     Log.error("PositionalVoiceGroup.maybeChangePerceivedObject: Removing member " + perceivedOid +
//                         " for perceiver " + perceiverOid + "; not in perceivedOids");
                }
            }
            PositionalGroupMember perceivedMember = (PositionalGroupMember)members.get(perceivedOid);
            if (perceiverMember.wnode != null && perceivedMember != null && perceivedMember.wnode != null) {
                perceivedMember.previousLoc = perceivedMember.lastLoc;
                perceivedMember.lastLoc = perceivedMember.wnode.getLoc();
                testProximity(perceiverMember, perceivedMember, true, false);
            }
        }
        finally {
            lock.unlock();
        }
    }

    /**
     * Initialized by the constructor; the maximum distance in
     * millimeters between players in a positional voice group at
     * which they can hear each other, adjusted by the
     * hystericalMargin.
     */
    protected float audibleRadius;

    /**
     * The hysteresis constant: don't change whether a pair of
     * positonal group members are in-radius of each other if the
     * distance in millimeters is within this distance of the
     * audibleRadius.
     */
    protected float hystericalMargin;

    /**
     * The map from instanceOid to the set of members tracked in that
     * instance for this group.
     */
    private Map<Long, Set<PositionalGroupMember>> instanceMembers =
        new HashMap<Long, Set<PositionalGroupMember>>();
    
    /**
     * A comparator  class that compares locations of speakers
     * to order the closest ones near the front of the list.
     */
    public static class CompareLocations implements Comparator<GroupMember> {

        public CompareLocations(Point center) {
            this.center = center;
        }
        
        /**
         * Which speaker member is closer to the Listener whose location is center?
         * Compares first locations, and if they are equal, compares the index values
         * of the speaker members.
         * @param m1 The first member.
         * @param m2 The second member.
         */
        public int compare(GroupMember m1, GroupMember m2) {
            if (m1 == m2)
                return 0;
            Point m1Loc = ((PositionalGroupMember)m1).getCurrentLoc();
            Point m2Loc = ((PositionalGroupMember)m2).getCurrentLoc();
            if (m1Loc == null) {
                if (Log.loggingDebug)
                    Log.debug("PositionalVoiceGroup.CompareLocations.compare: For member " + m1.getMemberOid() + ", currentLoc is null");
                return -1;
            }
            if (m2Loc == null) {
                Log.debug("PositionalVoiceGroup.CompareLocations.compare: For member " + m2.getMemberOid() + ", currentLoc is null");
                return -1;
            }    
            float d1Squared = Point.distanceToSquared(center, m1Loc);
            float d2Squared = Point.distanceToSquared(center, m2Loc);
            if (d1Squared < d2Squared)
                return -1;
            else if (d1Squared > d2Squared)
                return 1;
            else
                // The distances are equal, so just compare their
                // index, which are guaranteed to be distinct.
                return (m1.index < m2.index ? -1 : 1);
        }
        
        public boolean equals(Object other) {
            return this == other;
        }

        private Point center;

        public Point getCenter() {
            return center;
        }

        public void setCenter(Point center) {
            this.center = center;
        }
    }
    
}
