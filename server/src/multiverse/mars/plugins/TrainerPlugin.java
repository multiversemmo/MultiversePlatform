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
import java.util.HashMap;
import java.util.Map;

import multiverse.msgsys.Message;
import multiverse.msgsys.MessageTypeFilter;
import multiverse.server.engine.Engine;
import multiverse.server.engine.EnginePlugin;
import multiverse.server.engine.Hook;
import multiverse.server.engine.Namespace;
import multiverse.server.objects.Entity;
import multiverse.server.objects.EntityManager;
import multiverse.server.objects.Template;
import multiverse.server.plugins.WorldManagerClient.ExtensionMessage;
import multiverse.server.plugins.WorldManagerClient.TargetedExtensionMessage;
import multiverse.server.util.Log;
import multiverse.server.util.Logger;

//
//TrainerPlugin is a generic framework for processing training related messages that allow player's to learn new skills through a Trainer NPC
//
public class TrainerPlugin  extends EnginePlugin {
    public TrainerPlugin() {
        super("Trainer");
        setPluginType("Trainer");
    }

    private static final Logger log = new Logger("TrainerPlugin");

    public void onActivate() {
        super.onActivate();
        //register message hooks
        registerHooks();
        //subscribe to training messages
        MessageTypeFilter filter = new MessageTypeFilter();
        filter.addType(TrainerClient.MSG_TYPE_REQ_TRAINER_INFO);
        filter.addType(TrainerClient.MSG_TYPE_REQ_SKILL_TRAINING);
        Engine.getAgent().createSubscription(filter, this);

        this.registerPluginNamespace(TrainerClient.NAMESPACE, new TrainerSubObjectHook());

        if (Log.loggingDebug)
            log.debug("TrainerPlugin activated");
    }

    public void registerHooks() {
        getHookManager().addHook(TrainerClient.MSG_TYPE_REQ_TRAINER_INFO, new ReqTrainerInfoHook());
        getHookManager().addHook(TrainerClient.MSG_TYPE_REQ_SKILL_TRAINING, new ReqSkillTrainingHook());
    }

    // Creates the TrainerClient.NAMESPACE sub-object for storing trainer specific values such as list of skills that the trainer offers
    public class TrainerSubObjectHook extends GenerateSubObjectHook {
        public TrainerSubObjectHook() { super(TrainerPlugin.this); }

        public SubObjData generateSubObject(Template template, Namespace namespace, Long masterOid) {
            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid=" + masterOid
                          + ", template=" + template);

            if(masterOid == null) {
                log.error("GenerateSubObjectHook: no master oid");
                return null;
            }

            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: masterOid="+masterOid+", template="+template);

            Map<String, Serializable> props = template.getSubMap(TrainerClient.NAMESPACE);

            if (props == null) {
                Log.warn("GenerateSubObjectHook: no props in ns "
                         + TrainerClient.NAMESPACE);
                return null;
            }

            // generate the subobject
            Entity tinfo = new Entity(masterOid);
            tinfo.setName(template.getName());

            // copy properties from template to object
            for (Map.Entry<String, Serializable> entry : props.entrySet()) {
                String key = entry.getKey();
                Serializable value = entry.getValue();
                if (!key.startsWith(":")) {
                    tinfo.setProperty(key, value);
                }
            }

            if (Log.loggingDebug)
                log.debug("GenerateSubObjectHook: created entity " + tinfo);

            // register the entity
            EntityManager.registerEntityByNamespace(tinfo, TrainerClient.NAMESPACE);

            //send a response message
            return new SubObjData();
        }
    }

    // Processes messages that request information about the NPC trainer
    public class ReqTrainerInfoHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            if (Log.loggingDebug)
                log.debug("Processing ReqTrainerInfoHook - Send SKill Info");
            ExtensionMessage reqMsg = (ExtensionMessage) msg;

            sendTrainerInfo((Long)reqMsg.getProperty("playerOid"), (Long)reqMsg.getProperty("npcOid"));

            return true;
        }
    }

    // Send NPC trainer information back to the client
    protected void sendTrainerInfo(Long playerOid, Long trainerOid) {

        Entity e = EntityManager.getEntityByNamespace(trainerOid, TrainerClient.NAMESPACE);
        String skills = (String)e.getProperty("skills");
        skills = (skills==null)?"":skills;

        Map<String, Serializable> props = new HashMap<String, Serializable>();
        props.put("ext_msg_subtype", "mv.TRAINING_INFO");
        props.put("trainerOid", trainerOid);
        props.put("playerOid", playerOid);
        props.put("skills", skills);

        TargetedExtensionMessage msg = new TargetedExtensionMessage(TrainerClient.MSG_TYPE_TRAINING_INFO,
                                                                    playerOid, trainerOid, false, props);
        Engine.getAgent().sendBroadcast(msg);
    }

    // Process Skill training request.
    public class ReqSkillTrainingHook implements Hook {
        public boolean processMessage(Message msg, int flags) {
            if (Log.loggingDebug)
                log.debug("Processing ReqTrainerInfoHook - Send SKill Info");
            ExtensionMessage reqMsg = (ExtensionMessage) msg;

            Map<String, Serializable> props = new HashMap<String, Serializable>();
            props.put("skill", (String)reqMsg.getProperty("skill"));
            props.put("playerOid", (Long)reqMsg.getProperty("playerOid"));

            ExtensionMessage addSkillMsg = new ExtensionMessage(CombatClient.MSG_TYPE_ADD_SKILL,
                                                                (Long)reqMsg.getProperty("playerOid"),props);

            Engine.getAgent().sendBroadcast(addSkillMsg);
            return true;
        }
    }

}
