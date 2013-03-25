#
#
#  The Multiverse Platform is made available under the MIT License.
#
#  Copyright (c) 2012 The Multiverse Foundation
#
#  Permission is hereby granted, free of charge, to any person 
#  obtaining a copy of this software and associated documentation 
#  files (the "Software"), to deal in the Software without restriction, 
#  including without limitation the rights to use, copy, modify, 
#  merge, publish, distribute, sublicense, and/or sell copies 
#  of the Software, and to permit persons to whom the Software 
#  is furnished to do so, subject to the following conditions:
#
#  The above copyright notice and this permission notice shall be 
#  included in all copies or substantial portions of the Software.
#
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
#  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
#  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
#  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
#  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
#  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
#  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
#  OR OTHER DEALINGS IN THE SOFTWARE.
#
#  

from java.lang import *
from multiverse.mars import *
from multiverse.mars.objects import *
from multiverse.mars.core import *
from multiverse.mars.events import *
from multiverse.mars.util import *
from multiverse.mars.effects import *
from multiverse.mars.abilities import *
from multiverse.server.math import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from multiverse.server.plugins.WorldManagerClient import TargetedExtensionMessage
True=1
False=0

effect = HealEffect("minor heal effect")
effect.setHealProperty("health")
effect.setMinInstantHeal(10)
effect.setMaxInstantHeal(20)
Mars.EffectManager.register(effect.getName(), effect)

effect = HealEffect("heal effect")
effect.setMinInstantHeal(100)
effect.setMaxInstantHeal(100)
Mars.EffectManager.register(effect.getName(), effect)

effect = StunEffect("stun effect")
effect.setDuration(7000)
Mars.EffectManager.register(effect.getName(), effect)

effect = StatEffect("armor effect")
effect.setDuration(15000)
effect.setStat("armor", 20)
Mars.EffectManager.register(effect.getName(), effect)

effect = TeachAbilityEffect("teach heal effect")
effect.setAbilityName("heal")
Mars.EffectManager.register(effect.getName(), effect)

effect = TeachAbilityEffect("teach stun effect")
effect.setAbilityName("stun")
Mars.EffectManager.register(effect.getName(), effect)

effect = TeachAbilityEffect("teach armor effect")
effect.setAbilityName("armor")
Mars.EffectManager.register(effect.getName(), effect)

effect = TeachAbilityEffect("teach fireball effect")
effect.setAbilityName("fireball")
Mars.EffectManager.register(effect.getName(), effect)

effect = DamageEffect("attack effect")
effect.setMinInstantDamage(10)
effect.setMaxInstantDamage(15)
effect.setDamageType("Physical")
Mars.EffectManager.register(effect.getName(), effect)

effect = DamageEffect("player attack effect")
effect.setMinInstantDamage(15)
effect.setMaxInstantDamage(30)
effect.setDamageType("Physical")
Mars.EffectManager.register(effect.getName(), effect)

effect = HealEffect("health regen effect")
effect.setMinPulseHeal(2)
effect.setMaxPulseHeal(2)
effect.isPersistent(True)
effect.isPeriodic(True)
effect.setDuration(1000000)
effect.setNumPulses(500)
Mars.EffectManager.register(effect.getName(), effect)

effect = HealEffect("stamina regen effect")
effect.setHealProperty("stamina")
effect.setMinPulseHeal(2)
effect.setMaxPulseHeal(2)
effect.isPersistent(True)
effect.isPeriodic(True)
effect.setDuration(1000000)
effect.setNumPulses(500)
Mars.EffectManager.register(effect.getName(), effect)

effect = HealEffect("mana regen effect")
effect.setHealProperty("mana")
effect.setMinPulseHeal(2)
effect.setMaxPulseHeal(2)
effect.isPersistent(True)
effect.isPeriodic(True)
effect.setDuration(1000000)
effect.setNumPulses(500)
Mars.EffectManager.register(effect.getName(), effect)

effect = DamageEffect("fireball effect")
effect.setMinInstantDamage(100)
effect.setMaxInstantDamage(100)
effect.setDamageType("Fire")
Mars.EffectManager.register(effect.getName(), effect)

effect = HealEffect("renew effect");
effect.setMinInstantHeal(40);
effect.setMaxInstantHeal(40);
effect.setMinPulseHeal(10);
effect.setMaxPulseHeal(10);
effect.isPersistent(True);
effect.isPeriodic(True);
effect.setDuration(20000);
effect.setNumPulses(10);
Mars.EffectManager.register(effect.getName(), effect); 

ability = EffectAbility("stun")
ability.setActivationCost(10)
ability.setCostProperty("mana")
ability.setMaxRange(10000)
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setActivationEffect(Mars.EffectManager.get("stun effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("armor")
ability.setActivationCost(30)
ability.setCostProperty("mana")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("armor effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
Mars.AbilityManager.register(ability.getName(), ability)

healCastingEffect = CoordinatedEffect("SpellCastingEffect")
healCastingEffect.sendSourceOid(True)
healCastingEffect.putArgument("castingTime", Integer(3000))
healCastingEffect.putArgument("decalTexture", "eight-hearts.png")

healTargetEffect = CoordinatedEffect("SpellTargetEffect")
healTargetEffect.sendTargetOid(True)

fireballCastingEffect = CoordinatedEffect("SpellCastingEffect")
fireballCastingEffect.sendSourceOid(True)
fireballCastingEffect.putArgument("castingTime", Integer(5000))
fireballCastingEffect.putArgument("decalTexture", "eight-hearts.png")

fireballTargetEffect = CoordinatedEffect("MvFantasyFireball")
fireballTargetEffect.sendSourceOid(True)
fireballTargetEffect.sendTargetOid(True)

attackEffect = CoordinatedEffect("AttackEffect")
attackEffect.sendSourceOid(True)
attackEffect.sendTargetOid(True)

ability = EffectAbility("heal")
ability.setActivationTime(5000)
ability.setActivationCost(10)
ability.setCostProperty("mana")
ability.setMaxRange(20000)
ability.setIcon("Interface\FantasyWorldIcons\SPELL_heal_A")
ability.setTargetType(MarsAbility.TargetType.FRIEND)
ability.setActivationEffect(Mars.EffectManager.get("heal effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addCoordEffect(MarsAbility.ActivationState.ACTIVATING, healCastingEffect)
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, healTargetEffect)
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("heal potion")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("heal effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addCooldown(Cooldown("POTION", 15000))
ability.addReagent("Healing Potion")
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, healTargetEffect)
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("renew potion");
ability.setTargetType(MarsAbility.TargetType.SELF);
ability.setActivationEffect(Mars.EffectManager.get("renew effect"));
ability.addCooldown(Cooldown("GLOBAL", 1500));
ability.addCooldown(Cooldown("POTION", 15000));
Mars.AbilityManager.register(ability.getName(), ability); 

ability = EffectAbility("heal scroll")
ability.setTargetType(MarsAbility.TargetType.FRIEND)
ability.setActivationEffect(Mars.EffectManager.get("heal effect"))
ability.setMaxRange(20000)
ability.setActivationTime(3000)
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addReagent("Healing Scroll")
ability.addCoordEffect(MarsAbility.ActivationState.ACTIVATING, healCastingEffect)
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, healTargetEffect)
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("teach self heal ability")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("teach heal effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addReagent("Tome of Heal")
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("teach self stun ability")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("teach stun effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("teach self armor ability")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("teach armor effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("teach self fireball ability")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("teach fireball effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addReagent("Tome of Fireball")
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("attack ability")
ability.setMaxRange(5000)
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setActivationEffect(Mars.EffectManager.get("attack effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("player attack ability")
ability.setMaxRange(5000)
ability.setCostProperty("stamina")
ability.setActivationCost(10)
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setActivationEffect(Mars.EffectManager.get("player attack effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
Mars.AbilityManager.register(ability.getName(), ability)

ability = CreateItemAbility("leather tanning")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationTime(3000)
ability.setItem("Finished Leather")
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setCompleteSound("swordhit.wav")
ability.addReagent("Wolf Skin")
ability.addReagent("Wolf Skin")
Mars.AbilityManager.register(ability.getName(), ability)

ability = CreateItemAbility("make healing potion")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationTime(0)
ability.setItem("Healing Potion")
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setCompleteSound("swordhit.wav")
Mars.AbilityManager.register(ability.getName(), ability)

ability = CreateItemAbility("make healing scroll")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationTime(0)
ability.setItem("Healing Scroll")
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setCompleteSound("swordhit.wav")
Mars.AbilityManager.register(ability.getName(), ability)

ability = EffectAbility("fireball")
ability.setActivationTime(5000)
ability.setActivationCost(10)
ability.setCostProperty("mana")
ability.setMaxRange(40000)
ability.setIcon("Interface\FantasyWorldIcons\SPELL_fireball_A")
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setActivationEffect(Mars.EffectManager.get("fireball effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addCoordEffect(MarsAbility.ActivationState.ACTIVATING, fireballCastingEffect)
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, fireballTargetEffect)
Mars.AbilityManager.register(ability.getName(), ability)

effect = HealEffect("restore mana effect")
effect.setHealProperty("mana")
effect.setMinInstantHeal(100)
effect.setMaxInstantHeal(100)
Mars.EffectManager.register(effect.getName(), effect)

ability = EffectAbility("restore mana potion")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("restore mana effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addCooldown(Cooldown("POTION", 15000))
ability.addReagent("Mana Potion")
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, healTargetEffect)
Mars.AbilityManager.register(ability.getName(), ability)

##
## Below are basic examples of how we can extend the Mars combat system.
## Additional complexity could be added as necessary based on your world's requirements.            
##

class CombatEffect (DamageEffect):
    def apply(self, state):
        params = state.getParams()
        result = params.get("result")
        target = state.getObject()

        if (result == "miss"):
            ##Throw a missed msg to the client
            ##We could also just invoke a DamageMessage with a zero dmg value
            msg = TargetedExtensionMessage(target.getOid());
            msg.setProperty("ext_msg_subtype", "mv.COMBAT_ABILITY_MISSED");
            msg.setProperty("target", target.getOid());
            msg.setProperty("attacker", state.getCaster().getOid());			
            Engine.getAgent().sendBroadcast(msg);
        else:
            #Since we extend DamageEffect we could modify the damage here
            # and then send our own DamageMessage
            MarsEffect.apply(self, state)


class MeleeCombatAbility (CombatAbility):
    def resolveHit(self, state):
        params = HashMap()
        attacker = state.getObject()
        target = state.getTarget()

        #compute chance to hit by calculating an attack and defense stength
        attackerDexterity = int(attacker.statGetCurrentValue("dexterity"))
        attacherStrength = int(attacker.statGetCurrentValue("strength"))
        attackerLevel = int(attacker.statGetCurrentValue("level"))
        attackerAttackStrength = attackerDexterity + attacherStrength

        targetDexterity = int(target.statGetCurrentValue("dexterity"))
        targetStrength = int(target.statGetCurrentValue("strength"))
        targetLevel = int(target.statGetCurrentValue("level"))
        targetDefenseStrength = targetDexterity + targetStrength

        #Calc a hit bonus by comparing level difference and attacker's attack strength with target's defense strength
        hitBonus = (attackerAttackStrength + (attackerLevel*5)) - (targetDefenseStrength + (targetLevel*5))

        dice = Dice(100)
        roll = dice.roll(1)

        if roll + hitBonus > 80:
            params.put("result","hit")
        else:
            params.put("result","miss")

        return params

class RangedCombatAbility (CombatAbility):
    def resolveHit(self, state):
        params = HashMap()
        attacker = state.getObject()
        target = state.getTarget()

        #Calculate hit chance based on dexterity and strength
        # We could also create ranged specific attack stats instead so they wouldnt need to be calculated on the fly.
        attackerDexterity = int(attacker.statGetCurrentValue("dexterity"))
        attacherStrength = int(attacker.statGetCurrentValue("strength"))
        attackerLevel = int(attacker.statGetCurrentValue("level"))
        #In this case dexterity is more important than strength in determining attack strength
        attackerAttackStrength = attackerDexterity * 2 + attacherStrength

        #When trying to dodge or block a ranged attack dexterity may be your only hope
        targetDefenseStrength = int(target.statGetCurrentValue("dexterity")) * 2
        targetLevel = int(target.statGetCurrentValue("level"))

        hitBonus = (attackerAttackStrength + (attackerLevel*5)) - (targetDefenseStrength + (targetLevel*5))

        dice = Dice(100)
        roll = dice.roll(1)

        if roll + hitBonus > 80:
            params.put("result","hit")
        else:
            params.put("result","miss")

        return params


##***********************************************


effect = CombatEffect("default weaponskill effect")
effect.setMinInstantDamage(5)
effect.setMaxInstantDamage(15)
effect.setDamageType("Physical")
Mars.EffectManager.register(effect.getName(), effect)


# Define Survival Abilities
ability = EffectAbility("Lesser Bandages")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setMaxRange(1000)
ability.setActivationEffect(Mars.EffectManager.get("minor heal effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, healCastingEffect)
ability.addCooldown(Cooldown("FIRSTAID", 15000));
ability.setActivationCost(5)
ability.setCostProperty("stamina")
ability.setActivationTime(5000)
ability.setRequiredSkill(Mars.SkillManager.get("First Aid"), 1)
ability.setIcon("Interface\FantasyWorldIcons\SPELL_heal_A")
ability.setExperiencePerUse(1);
ability.setBaseExpThreshold(10);
ability.setMaxRank(3);
# define a leveling map so that one can control how it gains levels
lblm = LevelingMap()
lblm.setAllLevelModification(0.5, 0)
ability.setLevelingMap(lblm)
Mars.AbilityManager.register(ability.getName(), ability)

# Define Weapon Abilities
ability = MeleeCombatAbility("Wounding Thrust")
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setMaxRange(5000)
ability.setActivationEffect(Mars.EffectManager.get("default weaponskill effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setActivationCost(5)
ability.setCostProperty("stamina")
ability.setCompleteSound("swordhit.wav")
ability.setActivationTime(1000)
ability.setRequiredSkill(Mars.SkillManager.get("Sword"), 1)
ability.setIcon("Interface\FantasyWorldIcons\WEAPON_sword_A")
ability.setExperiencePerUse(1)
ability.setBaseExpThreshold(10)
ability.setMaxRank(3)

# define a leveling map so that one can control how it gains levels
woundinglm = LevelingMap()
woundinglm.setAllLevelModification(0.0, 10)
ability.setLevelingMap(woundinglm)
Mars.AbilityManager.register(ability.getName(), ability)

ability = MeleeCombatAbility("Whirling Dervish")
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setMaxRange(5000)
ability.setActivationEffect(Mars.EffectManager.get("default weaponskill effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setActivationCost(5)
ability.setCostProperty("stamina")
ability.setCompleteSound("swordhit.wav")
ability.setActivationTime(1000)
ability.setRequiredSkill(Mars.SkillManager.get("Sword"), 2)
ability.setIcon("Interface\FantasyWorldIcons\WEAPON_sword_A")
ability.setExperiencePerUse(1)
ability.setBaseExpThreshold(10)
ability.setMaxRank(3)

# define a leveling map so that one can control how it gains levels
whirlinglm = LevelingMap()
whirlinglm.setAllLevelModification(0.0, 10)
ability.setLevelingMap(whirlinglm)
Mars.AbilityManager.register(ability.getName(), ability)

ability = MeleeCombatAbility("Cleave")
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setMaxRange(5000)
ability.setActivationEffect(Mars.EffectManager.get("default weaponskill effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setActivationCost(5)
ability.setCostProperty("stamina")
ability.setCompleteSound("swordhit.wav")
ability.setActivationTime(1000)
ability.setRequiredSkill(Mars.SkillManager.get("Axe"), 1)
ability.setIcon("Interface\FantasyWorldIcons\WEAPON_axe_A")
ability.setExperiencePerUse(1)
ability.setBaseExpThreshold(10)
ability.setMaxRank(3)
# define a leveling map so that one can control how it gains levels
cleavelm = LevelingMap()
cleavelm.setAllLevelModification(0.0, 10)
ability.setLevelingMap(cleavelm)
Mars.AbilityManager.register(ability.getName(), ability)

ability = MeleeCombatAbility("Pierce")
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setMaxRange(5000)
ability.setActivationEffect(Mars.EffectManager.get("default weaponskill effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setActivationCost(5)
ability.setCostProperty("stamina")
ability.setCompleteSound("swordhit.wav")
ability.setActivationTime(1000)
ability.setRequiredSkill(Mars.SkillManager.get("Dagger"), 1)
ability.setIcon("Interface\FantasyWorldIcons\WEAPON_dagger_A")
ability.setExperiencePerUse(1)
ability.setBaseExpThreshold(10)
ability.setMaxRank(3)
# define a leveling map so that one can control how it gains levels
piercelm = LevelingMap()
piercelm.setAllLevelModification(0.0, 10)
ability.setLevelingMap(piercelm)
Mars.AbilityManager.register(ability.getName(), ability)

ability = RangedCombatAbility("Flying Dagger")
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setMaxRange(10000)
ability.setActivationEffect(Mars.EffectManager.get("default weaponskill effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
ability.setActivationCost(5)
ability.setCostProperty("stamina")
ability.setCompleteSound("swordhit.wav")
ability.setActivationTime(1000)
ability.setRequiredSkill(Mars.SkillManager.get("Thrown Weapons"), 1)
ability.setIcon("Interface\FantasyWorldIcons\WEAPON_dagger_A")
ability.setExperiencePerUse(1)
ability.setBaseExpThreshold(10)
# define a leveling map so that one can control how it gains levels
fdlm = LevelingMap()
fdlm.setAllLevelModification(0.0, 10)
ability.setLevelingMap(fdlm)
Mars.AbilityManager.register(ability.getName(), ability)

