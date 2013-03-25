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

import java.io.Serializable;
import java.util.*;

import multiverse.mars.objects.CombatInfo;
import multiverse.mars.core.*;
import multiverse.msgsys.MessageType;
import multiverse.server.engine.Engine;
import multiverse.server.engine.Namespace;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.util.Logger;

public class ClassAbilityClient {

    public static final MessageType MSG_TYPE_STAT_XP_UPDATE = MessageType.intern("mv.STAT_XP_UPDATE");
    public static final MessageType MSG_TYPE_HANDLE_EXP = MessageType.intern("mv.HANDLE_EXP");

    public static Namespace NAMESPACE = null;

    private static final Logger log = new Logger("ClassesPlugin");

    // Message types that are sent by the ClassAbility process


    private ClassAbilityClient(){}

    public static void sendXPUpdate(Long oid, String statName, int statCurrentValue){
        // we need to notify the client that a stat has increased.
        log.debug("Sending Client Stat XP Increase Message");

        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.STAT_XP_UPDATE");
        props.put("stat", "Stat XP Increased: " + statName + " : " + statCurrentValue);
        props.put("playerOid", oid);

        TargetedExtensionMessage sendXPUpdate = new TargetedExtensionMessage(ClassAbilityClient.MSG_TYPE_STAT_XP_UPDATE, oid, oid, false, props);

        Engine.getAgent().sendBroadcast(sendXPUpdate);
    }

    public static void CheckSkillAbilities(long playerOid, String skill, int level){
        if(skill == null)
            log.warn("ClassAbilityClient.CheckSkillAbilities - Skill is null");

        //First find a list of abilities we should have for this skill and skill level
        ArrayList<MarsAbility> skillAbilities = new ArrayList<MarsAbility>();

        Collection<MarsAbility> abilities = Mars.AbilityManager.getMap().values();
        for(MarsAbility ability : abilities){
            if(ability.getRequiredSkill() == null)
                log.warn("ClassAbilityClient.CheckSkillAbilities - Required Skill for ability "+ability.getName()+" is null");
            else{
                if(ability.getRequiredSkill().getName().equals(skill) && ability.getRequiredSkillLevel() <= level){
                    log.debug("ClassAbilityClient.CheckSkillAbilities: Adding ability to skillAbilities : " + ability.getName());
                    skillAbilities.add(ability);
                }
            }
        }

        //Get player object
        CombatInfo player = CombatPlugin.getCombatInfo(playerOid);
        //get list of currently known abilities
        ArrayList<String> currentAbilities = player.getAbilities();
        //Check to see if any of the item in our skillAbilities list is not in our currentAbilities
        for(MarsAbility ability : skillAbilities){
            if(!currentAbilities.contains(ability.getName())){
                log.debug("ClassAbilityClient.CheckSkillAbilities: Adding new ability : " + ability.getName());
                //Ability is not currently in our list so lets add the new ability to the player
                player.addAbility(ability.getName());
                CombatPlugin.sendAbilityUpdate(player);
            }
        }
        log.debug("ClassAbilityClient.CheckSkillAbilities: Finished");
    }
}
