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
from java.util import *
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
True=1
False=0

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

effect = DamageEffect("poison effect")
effect.isPeriodic(1)
effect.isPersistent(1)
effect.setMinPulseDamage(12)
effect.setMaxPulseDamage(12)
effect.setNumPulses(10)
effect.setDuration(50000)
effect.setDamageType("Poison")
Mars.EffectManager.register(effect.getName(), effect)

effect = HealEffect("health regen effect")
effect.setHealProperty("health")
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
effect.setDamageProperty("health")
effect.setMinInstantDamage(40)
effect.setMaxInstantDamage(60)
effect.setDamageType("Fire")
Mars.EffectManager.register(effect.getName(), effect)

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
fireballCastingEffect.putArgument("castingTime", Integer(1500))
fireballCastingEffect.putArgument("decalTexture", "fire_ring_decal.dds")

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

ability = EffectAbility("poison potion")
ability.setTargetType(MarsAbility.TargetType.SELF)
ability.setActivationEffect(Mars.EffectManager.get("poison effect"))
ability.addCooldown(Cooldown("GLOBAL", 1500))
ability.addCooldown(Cooldown("POTION", 15000))
ability.addReagent("Poison Potion")
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, healTargetEffect)
Mars.AbilityManager.register(ability.getName(), ability)

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
ability.setActivationTime(1500)
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

rand = Random()

class FantasyCombatEffect (MarsEffect):
    def apply(self, state):
        MarsEffect.apply(self, state)
        params = state.getParams()
        result = params.get("result")
        baseDmg = params.get("damage")
        target = state.getObject()

        if (result == "miss"):
            dmgType = "miss"
        elif (result == "hit"):
            dmgType = "Physical"
        elif (result == "crit"):
            dmgType = "critical Physical"
        else:
            Log.error("FantasyCombatEffect.apply: unknown result")

        penetration = params.get("penetration")
        armor = target.statGetCurrentValue("armor")
        dmgEff = (1.0 * penetration) / (armor + penetration)
        if (dmgEff < 0.25):
            dmgEff = 0.25
        dmg = dmgEff * baseDmg
        dmg = int(dmg)

        if (dmg == 0 and dmgType != "miss"):
            dmg = 1

        target.statModifyBaseValue("health", -dmg)
        target.sendStatusUpdate()
        Log.debug("FantasyCombatEffect.apply: sending damage message target=" + target.toString()
                  + " caster=" + state.getCaster().toString() + " dmg=" + str(dmg) + " dmgType=" + dmgType)
        Engine.getAgent().sendBroadcast(CombatClient.DamageMessage(target.getOwnerOid(),
                                                      state.getCaster().getOwnerOid(),
                                                      dmg, dmgType))

effect = FantasyCombatEffect("attack effect")
Mars.EffectManager.register(effect.getName(), effect)


class FantasyCombatAbility (CombatAbility):
    def resolveHit(self, state):
        params = HashMap()
        caster = state.getObject()
        target = state.getTarget()

        defense = target.statGetCurrentValue("defense")
        accuracy = caster.statGetCurrentValue("accuracy")
        agility = caster.statGetCurrentValue("agility")
        atkPower = caster.statGetCurrentValue("attack power")
        atkDelay = caster.getAttackDelay()

        missChance = 0.2 + (defense-accuracy)/1000.0
        if (missChance < 0.05):
            missChance = 0.05
        critChance = missChance + 0.05 + agility/1000.0

        roll = rand.nextFloat()

        bonusDmg = (atkPower * atkDelay) / 10000.0
        baseWeapDmg = caster.getProperty("weaponBaseDmg")
        varWeapDmg = caster.getProperty("weaponVarDmg")
        dmg = bonusDmg + baseWeapDmg + rand.nextFloat() * varWeapDmg

        if (roll < missChance):
            dmg = 0
            params.put("result", "miss")
        elif (roll < critChance):
            dmg *= 1.5
            params.put("result", "crit")
        else:
            params.put("result", "hit")

        params.put("damage", int(dmg))

        penetration = caster.statGetCurrentValue("offense skill")
        params.put("penetration", penetration)
        return params

ability = FantasyCombatAbility("attack ability")
ability.setMaxRange(5000)
ability.setTargetType(MarsAbility.TargetType.ENEMY)
ability.setActivationEffect(Mars.EffectManager.get("attack effect"))
ability.addCoordEffect(MarsAbility.ActivationState.COMPLETED, attackEffect)
Mars.AbilityManager.register(ability.getName(), ability)
