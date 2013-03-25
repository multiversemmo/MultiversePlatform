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

package multiverse.mars.objects;

import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;

import multiverse.mars.plugins.GroupClient;
import multiverse.server.objects.Entity;
import multiverse.server.plugins.VoiceClient;
import multiverse.server.util.Log;

public class MarsGroup extends Entity {
    // properties
    private Hashtable<Long, MarsGroupMember> _groupMembers;
    private static final long serialVersionUID = 1L;
    private long _groupLeaderOid;
    private Boolean _groupMuted = false;

    // constructors
    public MarsGroup() {
        super("");
        _groupMembers = new Hashtable<Long, MarsGroupMember>();
        if(Log.loggingDebug)
            log.debug("MarsGroup - creating new group " + this.getOid().toString());
        SetupVoiceGroup();        
    }

    // events

    // methods
    public long GetGroupOid() {
        return this.getOid();
    }

    public MarsGroupMember AddGroupMember(CombatInfo combatInfo) {
        MarsGroupMember newMember = new MarsGroupMember(combatInfo, this.getOid());
        _groupMembers.put(newMember.getOid(), newMember);
        combatInfo.setGroupOid(this.GetGroupOid());
        combatInfo.setGroupMemberOid(newMember.getOid());
        GroupClient.SendGroupEventMessage(GroupClient.GroupEventType.JOINED, this, combatInfo.getOwnerOid());
        return newMember;
    }

    public void RemoveGroupMember(CombatInfo combatInfo) {
        GroupClient.SendGroupEventMessage(GroupClient.GroupEventType.LEFT, this, combatInfo.getOwnerOid());
        _groupMembers.remove(combatInfo.getGroupMemberOid());
        combatInfo.setGroupMemberOid(0);
        combatInfo.setGroupOid(0);
        //If no more members are in the group, then group is disolving and we need to clean up the voice group
        if(_groupMembers.size() == 0)
            RemoveVoiceGroup();
    }

    public Hashtable<Long, MarsGroupMember> GetGroupMembers() {
        return _groupMembers;
    }

    public int GetNumGroupMembers() {
        return _groupMembers.size();
    }

    public long GetGroupLeaderOid() {
        return _groupLeaderOid;
    }
    
    public MarsGroupMember GetGroupMember(Long groupMemberOid){
        for(MarsGroupMember groupMember : _groupMembers.values()){
            if(groupMember.GetGroupMemberOid() == groupMemberOid)
                return groupMember;
        }
        return null;
    }
    
    public void SetGroupLeaderOid(long value) {
        _groupLeaderOid = value;
        if(value > 0)
            GroupClient.SendGroupEventMessage(GroupClient.GroupEventType.LEADERCHANGED, this, value);
        //Default to remaining person until group is cleaned up and removed
        if(_groupMembers.size() == 1){
            List<MarsGroupMember> groupMembers = new ArrayList<MarsGroupMember>(_groupMembers.values());
            _groupLeaderOid = groupMembers.get(0).GetGroupMemberOid();
        }
    }
    
    protected void SetupVoiceGroup(){
        int error = 0;

        // Create a new voice chat group specific to this group that is non-positional
        error = VoiceClient.addVoiceGroup(this.GetGroupOid(), false, 4);
        if(error != VoiceClient.SUCCESS){
            Log.error("MarsGroup.SetupGroupVoice : Create Voice Group Response - " + VoiceClient.errorString(error));            
        }
    }
    
    public void RemoveVoiceGroup(){
        int error = 0;
        // Remove voice group voice server
        error = VoiceClient.removeVoiceGroup(this.GetGroupOid());
        if(error != VoiceClient.SUCCESS){
            Log.error("MarsGroup.RemoveVoiceGroup : Remove Voice Group Response - " + VoiceClient.errorString(error));            
        }  
    }
    
    public void SetGroupMuted(Boolean value){
        this._groupMuted = value;
    }
    
    public Boolean GetGroupMuted(){
        return this._groupMuted;
    }
}
