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
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;

/**
 * A class to hold the attributes of a non-positional group member
 * from the point of view of the voice plugin.
 */
public class GroupMember {

    /**
     * GroupMember constructor
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
    public GroupMember(VoiceGroup group, long memberOid, int priority, boolean allowedSpeaker, 
                       boolean currentSpeaker, VoiceConnection memberCon, int maxVoiceChannels) {
        this.group = group;
        this.memberOid = memberOid;
        this.priority = priority;
        this.allowedSpeaker = allowedSpeaker;
        this.currentSpeaker = currentSpeaker;
        this.listening = false;
        this.memberCon = memberCon;
        this.index = indexCounter++;
        voiceNumberToSpeaker = new GroupMember[maxVoiceChannels];
        if (Log.loggingDebug)
            Log.debug(logString("GroupMember constructor"));

    }

    /**
     * Return a string describing the member.
     * @return A short descriptive string for the GroupMember.
     */
    public String toString() {
        return "GroupMember(oid " + memberOid + ", con " + memberCon + ")";
    }
    
    /**
     * Provide a string description of the instance suitable for logging.
     * @param intro The descriptive string which will prepend the log entry.
     * @return The intro followed by the GroupMember description string.
     */
    public String logString(String intro) {
        return intro + ": oid " + memberOid + ", listenToYourself " + 
            memberCon.listenToYourself + ", allowedSpeaker " + allowedSpeaker + 
            ", currentSpeaker " + currentSpeaker;
    }

    /**
     * Create a voice channel using number VoiceNumber between 
     * this acting as a listener and the supplied speaker.
     * @param voiceNumber The number of the voice channel opened to 
     * the listener, unique within an single client.
     * @param speaker The GroupMember instance representing the speaker.
     */
    public void setSpeakerForVoiceNumber(byte voiceNumber, GroupMember speaker) {
        GroupMember listener = this;
        if (speaker == null) {
            GroupMember oldSpeaker = voiceNumberToSpeaker[voiceNumber];
            if (oldSpeaker == null)
                Log.dumpStack("GroupMember.setSpeakerForVoiceNumber: Setting speaker to null for voiceNumber " + voiceNumber +
                    ", but voiceNumberToSpeaker[voiceNumber] is already null!");
            if (listener.listenerVoicesMap.remove(oldSpeaker) == null)
                Log.dumpStack("GroupMember.setSpeakerForVoiceNumber: Setting speaker to null for listener " + listener + 
                    ", didn't find speaker " + oldSpeaker + " in listenerVoicesMap");
            if (!oldSpeaker.listenersToMe.remove(listener))
                Log.dumpStack("GroupMember.setSpeakerForVoiceNumber: For  speaker " + oldSpeaker + ", didn't find listener " + listener + " in listenersToMe");
        }
        else {
            Byte oldVoiceNumber = listener.listenerVoicesMap.put(speaker, voiceNumber);
            if (oldVoiceNumber != null)
                Log.dumpStack("GroupMember.setSpeakerForVoiceNumber: For listener " + listener + 
                    " and speaker, when adding voiceNumber " + voiceNumber + ", found " + oldVoiceNumber + " in listenerVoicesMap");
            if (!speaker.listenersToMe.add(listener))
            Log.dumpStack("GroupMember.setSpeakerForVoiceNumber: listener " + 
                listener + " was already in speaker " + speaker + " listenersToMe list!");
            if (voiceNumberToSpeaker[voiceNumber] != null)
                Log.dumpStack("GroupMember.setSpeakerForVoiceNumber: For  speaker " + speaker + ", voiceNumber " + voiceNumber +
                    ", voiceNumberToSpeaker[voiceNumber] " + voiceNumberToSpeaker[voiceNumber] + " is non-null");
        }
        voiceNumberToSpeaker[voiceNumber] = speaker;
    }
    
    /**
     * Return the group to which this member belongs.
     */
    public VoiceGroup getGroup() {
        return group;
    }
    
    /**
     * Get the oid of the group to which this member belongs.
     * @return The oid of the member's group.
     */
    public long getGroupOid() {
        return group.getGroupOid();
    }
    
    /**
     * Return the speaker that this is currently listening to  using the 
     * voice channel number voiceNumber, or null if there is none.
     * @param voiceNumber The number of the voice channel for which the speaker is to be returned.
     * @return The speaker associated with the voice channel, or null.
     */
    public GroupMember getSpeakerForVoiceNumber(byte voiceNumber) {
        return voiceNumberToSpeaker[voiceNumber];
    }

    /**
     * Return a voice number that is not currently associated 
     * with a speaker, or null if there is none.
     * @return The available voice number.
     */
    public Byte findFreeVoiceNumber() {
        for (byte i=0; i<voiceNumberToSpeaker.length; i++) {
            if (voiceNumberToSpeaker[i] == null)
                return i;
        }
        return null;
    }
    
    /**
     * Return the voice number associated with the given speaker, or
     * null if there is none.
     * @param speaker The GroupMember for the speaker.
     * @return The voice number corresponding to that speaker in the 
     * voice channels of this, or null.
     */
    public Byte findVoiceNumberForSpeaker(GroupMember speaker) {
        return listenerVoicesMap.get(speaker);
    }
    
    /**
     * Return the number of voice channels on which this is currently listening.
     * @return The listener's voice channel count.
     */
    public int voiceCount() {
        return listenerVoicesMap.size();
    }
    
    /**
     * Return true if this is listening to the the given speaker; 
     * false otherwise
     * @param speaker The GroupMember for the speaker.
     * @return True if this is listening to the speaker.
     */
    public boolean nowListeningTo(GroupMember speaker) {
        return speaker.listenersToMe.contains(this);
    }
    
    /**
     * Return a copy of the list of members listening to this.
     * @return A List<GroupMember> holding the members current listening to this.
     */
    public List<GroupMember> membersListeningToSpeaker() {
        return new LinkedList<GroupMember>(listenersToMe);
    }
    
    /**
     * Returns true if the speaker has been ignored by the listener
     * @param speaker The GroupMember for the speaker.
     * @return True if the speaker has been ignored.
     */
    public boolean speakerIgnored(GroupMember speaker) {
        return ignoredSpeakerOids != null && ignoredSpeakerOids.contains(speaker.memberOid);
    }
    
    /**
     * Initializes the list of ignored speakers, and logs an error if it is already
     * initialized.
     * @param ignored A List<Long> representing the initial contents
     * of ignoredSpeakerOids.
     */
    public void initializeIgnoredSpeakers(List<Long> ignored) {
        if (ignoredSpeakerOids != null)
            Log.error("GroupMember.initializeIgnoredSpeakers: ignoredSpeakerOids for member " + memberOid + " is already initialized!");
        else {
            ignoredSpeakerOids = new HashSet<Long>();
            ignoredSpeakerOids.addAll(ignored);
            if (pendingIgnoreUpdateMessages != null) {
                for (ExtensionMessage extMsg : pendingIgnoreUpdateMessages)
                    applyIgnoreUpdateMessageInternal(extMsg);
                pendingIgnoreUpdateMessages = null;
            }
        }
    }
    
    /**
     * If ignoredSpeakerOids is initialized, apply the updates in the
     * extMsg.  If not, add the message to
     * pendingIgnoreUpdateMessages.
     * @param extMsg The ExtensionMessage containing the ignore list changes.
     */
    public void applyIgnoreUpdateMessage(ExtensionMessage extMsg) {
        if (ignoredSpeakerOids != null)
            applyIgnoreUpdateMessageInternal(extMsg);
        else {
            if (pendingIgnoreUpdateMessages == null)
                pendingIgnoreUpdateMessages = new LinkedList<ExtensionMessage>();
            pendingIgnoreUpdateMessages.add(extMsg);
        }
    }

    /**
     * Extract the "now_ignored" and "no_longer_ignored" oids lists
     * for the message, and apply them to the member's ignore list.
     * @param extMsg The ExtensionMessage containing the ignore list changes.
     */
    private void applyIgnoreUpdateMessageInternal(ExtensionMessage extMsg) {
        List<Long> nowIgnored = (LinkedList<Long>)extMsg.getProperty("now_ignored");
        List<Long> noLongerIgnored = (LinkedList<Long>)extMsg.getProperty("no_longer_ignored");
        if (noLongerIgnored != null)
            removeIgnoredSpeakerOids(noLongerIgnored);
        if (nowIgnored != null)
            addIgnoredSpeakerOids(nowIgnored);
    }

    /**
     * Adds the oids to the list of ignoredSpeaker oids
     * @param speakerOids
     */
    public void addIgnoredSpeakerOids(List<Long> speakerOids) {
        if (ignoredSpeakerOids == null)
            Log.error("GroupMember.addIgnoredSpeakerOids: ignoredSpeakerOids for member " + memberOid + " is not yet initialized!");
        else
            ignoredSpeakerOids.addAll(speakerOids);
    }
    
    /**
     * Removes the oids from the list of ignoredSpeaker oids
     * @param speakerOids
     */
    public void removeIgnoredSpeakerOids(List<Long> speakerOids) {
        if (ignoredSpeakerOids == null)
            Log.error("GroupMember.removeIgnoredSpeakerOids: ignoredSpeakerOids for member " + memberOid + " is not yet initialized!");
        else
            ignoredSpeakerOids.removeAll(speakerOids);
    }
    
    /**
     * Return the memberOid
     */
    public long getMemberOid() {
        return memberOid;
    }

    /**
     * Set this member to be expunged, which tells all users not to use it.
     */
    public void setExpunged() {
        expunged = true;
    }
    

    /**
     * Get the expunged status of this member.
     */
    public boolean getExpunged() {
        return expunged;
    }

    /**
     * The oid of the member.
     */
    protected long memberOid;

    /**
     * The priority of the member.
     */
    protected int priority;

    /**
     * True if the member is allowed to speak; false otherwise.
     */
    protected boolean allowedSpeaker;

    /**
     * True if the member is currently speaking; false otherwise.
     */
    protected boolean currentSpeaker;

    /**
     * True if the member is currently listening; false otherwise.
     */
    protected boolean listening;

    /**
     * The voice number of the member when viewed as a speaker,
     * present in the data frames sent by the client.  Not used for
     * anything important.
     */
    protected byte voiceNumber = -1;

    /**
     * The VoiceConnection object associated with the member.
     */
    protected VoiceConnection memberCon;

    /**
     * Allocated from a counter, and used to impose a total ordering
     * of GroupMembers.
     */
    protected int index;

    /**
     * A per-member temp used in the BasicVoiceGroup 
     */
    protected int priorityIndex = -1;
    
    /**
     * The group that the member belongs to.
     */
    protected VoiceGroup group;
    
    /**
     * Used to disambiguate what would otherwise be a 
     * equal comparator results
     */
    protected static int indexCounter = 0;

    /**
     * A set of those members listening to this member
     */
    private Set<GroupMember> listenersToMe = new HashSet<GroupMember>();

    /**
     * Map of the members and voice numbers this member is listening to
     */
    private Map<GroupMember, Byte> listenerVoicesMap = new HashMap<GroupMember, Byte>();

    /**
     * A set of those members this member doesn't want to listen to.
     */
    private Set<Long> ignoredSpeakerOids = null;

    /**
     * A list of pending ExtensionMessages containing updates to the ignore list.
     */
    private List<ExtensionMessage> pendingIgnoreUpdateMessages;
    
    /**
     * An array indexed by voice number giving the speaker speaking
     * using that voice number, or null if the voice number is
     * unassigned.
     * TBD: ??? We should do away with this redundant mapping
     */
    private GroupMember[] voiceNumberToSpeaker;

    /**
     * True if this group member should no longer be referenced.
     */
    protected boolean expunged = false;
}

