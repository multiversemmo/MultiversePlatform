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

from multiverse.mars import *
from multiverse.mars.objects import *
from multiverse.mars.core import *
from multiverse.mars.events import *
from multiverse.mars.util import *
from multiverse.mars.plugins import *
from multiverse.server.plugins import *
from multiverse.server.math import *
from multiverse.server.events import *
from multiverse.server.objects import *
from multiverse.server.engine import *
from java.lang import *

True=1
False=0

class TemplateHook(EnginePlugin.PluginActivateHook):
    def activate(self):

        ############################################################
        #
        # DisplayContexts
        #
        ############################################################

        human_female_base_DC = DisplayContext("human_female.mesh", True)
        human_female_base_DC.addSubmesh(DisplayContext.Submesh("bodyShape-lib.0",
                                                               "human_female.skin_material"))
        human_female_base_DC.addSubmesh(DisplayContext.Submesh("head_aShape-lib.0",
                                                               "human_female.head_a_material"))

        human_female_leather_pantsDC = DisplayContext()
        human_female_leather_pantsDC.setMeshFile("human_female.mesh")
        human_female_leather_pantsDC.addSubmesh(DisplayContext.Submesh("leather_a_pantsShape-lib.0",
                                                                       "human_female.leather_a_material"))
        human_female_leather_pantsDC.addSubmesh(DisplayContext.Submesh("leather_a_beltShape-lib.0",
                                                                       "human_female.leather_a_material"))

        human_female_leather_tunicDC = DisplayContext()
        human_female_leather_tunicDC.setMeshFile("human_female.mesh")
        human_female_leather_tunicDC.addSubmesh(DisplayContext.Submesh("leather_a_tunicShape-lib.0",
                                                                       "human_female.leather_a_material"))

        human_female_leather_bootsDC = DisplayContext()
        human_female_leather_bootsDC.setMeshFile("human_female.mesh")
        human_female_leather_bootsDC.addSubmesh(DisplayContext.Submesh("leather_a_bootsShape-lib.0",
                                                                       "human_female.leather_a_material"))

        human_female_leather_glovesDC = DisplayContext()
        human_female_leather_glovesDC.setMeshFile("human_female.mesh")
        human_female_leather_glovesDC.addSubmesh(DisplayContext.Submesh("leather_a_bracersShape-lib.0",
                                                                        "human_female.leather_a_material"))

        human_female_plate_tunicDC = DisplayContext()
        human_female_plate_tunicDC.setMeshFile("human_female.mesh")
        human_female_plate_tunicDC.addSubmesh(DisplayContext.Submesh("plate_b_tunicShape-lib.0",
                                                                     "human_female.plate_b_material"))

        zombie_base_DC = DisplayContext("zombie.mesh")
        zombie_base_DC.addSubmesh(DisplayContext.Submesh("Zombie_Body2-obj.0", "Zombie.Zombie_Body"))
        zombie_base_DC.addSubmesh(DisplayContext.Submesh("Zombie_Clothes2-obj.0", "Zombie.Zombie_Clothes"))

        orc_base_DC = DisplayContext("orc.mesh", True)

        human_female_ruth_base_DC = DisplayContext("human_female_ruth.mesh", True)
        human_female_ruth_base_DC.addSubmesh(DisplayContext.Submesh("human_female_body_ruth-mesh.0",
                                                                "human_female_ruth.ruth_body_clothed_mat"))
        human_female_ruth_base_DC.addSubmesh(DisplayContext.Submesh("human_female_head_ruth-mesh.0",
                                                                "human_female_ruth.ruth_head_mat"))
        human_female_ruth_base_DC.addSubmesh(DisplayContext.Submesh("human_female_head_ruth_hair-mesh.0",
                                                                "human_female_ruth.ruth_hair_mat"))

        human_female_ruth_leather_pants_DC = DisplayContext("human_female_ruth.mesh")
        human_female_ruth_leather_pants_DC.addSubmesh(DisplayContext.Submesh("leather_armor_legs-mesh.0",
                                                                         "human_female_ruth.leather_armor_mat"))
        human_female_ruth_leather_pants_DC.addSubmesh(DisplayContext.Submesh("leather_armor_belt-mesh.0",
                                                                         "human_female_ruth.leather_armor_mat"))

        human_female_ruth_leather_tunic_DC = DisplayContext("human_female_ruth.mesh")
        human_female_ruth_leather_tunic_DC.addSubmesh(DisplayContext.Submesh("leather_armor_chest-mesh.0",
                                                                         "human_female_ruth.leather_armor_mat"))

        human_female_ruth_leather_boots_DC = DisplayContext("human_female_ruth.mesh")
        human_female_ruth_leather_boots_DC.addSubmesh(DisplayContext.Submesh("leather_armor_boots-mesh.0",
                                                                         "human_female_ruth.leather_armor_mat"))

        human_female_ruth_leather_gloves_DC = DisplayContext("human_female_ruth.mesh")
        human_female_ruth_leather_gloves_DC.addSubmesh(DisplayContext.Submesh("leather_armor_bracer_rt-mesh.0",
                                                                          "human_female_ruth.leather_armor_mat"))
        human_female_ruth_leather_gloves_DC.addSubmesh(DisplayContext.Submesh("leather_armor_bracer_lt-mesh.0",
                                                                          "human_female_ruth.leather_armor_mat"))


        human_male_DC = DisplayContext("human_male.mesh", True)
        human_male_DC.addSubmesh(DisplayContext.Submesh("head_bShape-lib.0",
                                                        "human_male.head_a_material"))
        human_male_DC.addSubmesh(DisplayContext.Submesh("bodyShape-lib.0",
                                                        "human_male.skin_material"))
        human_male_DC.addSubmesh(DisplayContext.Submesh("cloth_a_pantsShape-lib.0",
                                                        "human_male.cloth_a_material"))
        human_male_DC.addSubmesh(DisplayContext.Submesh("cloth_a_bootsShape-lib.0",
                                                        "human_male.cloth_a_material"))
        human_male_DC.addSubmesh(DisplayContext.Submesh("cloth_a_shirtShape-lib.0",
                                                        "human_male.cloth_a_material"))

        female_player_01_base_DC = DisplayContext("human_female_fantasy.mesh", True)
        female_player_01_base_DC.addSubmesh(DisplayContext.Submesh("human_female_body-mesh.0",
                                                                   "human_female_fantasy.body_01_clothed_mat"))
        female_player_01_base_DC.addSubmesh(DisplayContext.Submesh("human_female_head_01-mesh.0",
                                                                   "human_female_fantasy.head_01_mat"))
        female_player_01_base_DC.addSubmesh(DisplayContext.Submesh("human_female_head_01_hair-mesh.0",
                                                                   "human_female_fantasy.head_01_hair_01_mat"))

        female_player_02_base_DC = DisplayContext("human_female_fantasy.mesh", True)
        female_player_02_base_DC.addSubmesh(DisplayContext.Submesh("human_female_body-mesh.0",
                                                                   "human_female_fantasy.body_02_clothed_mat"))
        female_player_02_base_DC.addSubmesh(DisplayContext.Submesh("human_female_head_02-mesh.0",
                                                                   "human_female_fantasy.head_02_mat"))
        female_player_02_base_DC.addSubmesh(DisplayContext.Submesh("human_female_head_02_hair_01-mesh.0",
                                                                   "human_female_fantasy.head_02_hair_01_mat"))

        female_player_leather_pants_DC = DisplayContext("human_female_fantasy.mesh")
        female_player_leather_pants_DC.addSubmesh(DisplayContext.Submesh("leather_armor_legs-mesh.0",
                                                                         "human_female_fantasy.leather_armor_mat"))
        female_player_leather_pants_DC.addSubmesh(DisplayContext.Submesh("leather_armor_belt-mesh.0",
                                                                         "human_female_fantasy.leather_armor_mat"))

        female_player_leather_tunic_DC = DisplayContext("human_female_fantasy.mesh")
        female_player_leather_tunic_DC.addSubmesh(DisplayContext.Submesh("leather_armor_chest-mesh.0",
                                                                         "human_female_fantasy.leather_armor_mat"))
        female_player_leather_tunic_DC.addSubmesh(DisplayContext.Submesh("leather_armor_jewels-mesh.0",
                                                                         "human_female_fantasy.leather_armor_mat"))

        female_player_leather_boots_DC = DisplayContext("human_female_fantasy.mesh")
        female_player_leather_boots_DC.addSubmesh(DisplayContext.Submesh("leather_armor_boot_lt-mesh.0",
                                                                         "human_female_fantasy.leather_armor_mat"))
        female_player_leather_boots_DC.addSubmesh(DisplayContext.Submesh("leather_armor_boot_rt-mesh.0",
                                                                         "human_female_fantasy.leather_armor_mat"))

        female_player_leather_gloves_DC = DisplayContext("human_female_fantasy.mesh")
        female_player_leather_gloves_DC.addSubmesh(DisplayContext.Submesh("leather_armor_bracer_rt-mesh.0",
                                                                          "human_female_fantasy.leather_armor_mat"))
        female_player_leather_gloves_DC.addSubmesh(DisplayContext.Submesh("leather_armor_bracer_lt-mesh.0",
                                                                          "human_female_fantasy.leather_armor_mat"))

        male_player_01_base_DC = DisplayContext("human_male_fantasy.mesh", True)
        male_player_01_base_DC.addSubmesh(DisplayContext.Submesh("human_male_body-mesh.0",
                                                                 "human_male_fantasy.human_male_body_01"))
        male_player_01_base_DC.addSubmesh(DisplayContext.Submesh("human_male_head_01-mesh.0",
                                                                 "human_male_fantasy.human_male_head_01"))
        male_player_01_base_DC.addSubmesh(DisplayContext.Submesh("male_head_01_hair_01-mesh.0",
                                                                 "human_male_fantasy.human_male_head_01_hair_01"))

        male_player_02_base_DC = DisplayContext("human_male_fantasy.mesh", True)
        male_player_02_base_DC.addSubmesh(DisplayContext.Submesh("human_male_body-mesh.0",
                                                                 "human_male_fantasy.human_male_body_02"))
        male_player_02_base_DC.addSubmesh(DisplayContext.Submesh("human_male_head_02-mesh.0",
                                                                 "human_male_fantasy.human_male_head_02"))
        male_player_02_base_DC.addSubmesh(DisplayContext.Submesh("human_male_02_hair_01-mesh.0",
                                                                 "human_male_fantasy.human_male_head_02_hair_01"))

        male_player_leather_pants_DC = DisplayContext("human_male_fantasy.mesh")
        male_player_leather_pants_DC.addSubmesh(DisplayContext.Submesh("male_leather_b_legs-mesh.0",
                                                                       "human_male_fantasy.human_male_armor_leather_b"))

        male_player_leather_tunic_DC = DisplayContext("human_male_fantasy.mesh")
        male_player_leather_tunic_DC.addSubmesh(DisplayContext.Submesh("male_leather_b_chest-mesh.0",
                                                                       "human_male_fantasy.human_male_armor_leather_b"))

        male_player_leather_boots_DC = DisplayContext("human_male_fantasy.mesh")
        male_player_leather_boots_DC.addSubmesh(DisplayContext.Submesh("male_leather_b_feet-mesh.0",
                                                                       "human_male_fantasy.human_male_armor_leather_b"))

        male_player_leather_gloves_DC = DisplayContext("human_male_fantasy.mesh")
        male_player_leather_gloves_DC.addSubmesh(DisplayContext.Submesh("male_leather_b_hands-mesh.0",
                                                                        "human_male_fantasy.human_male_armor_leather_b"))

        hilldale_scout_DC = DisplayContext("human_female_ruth.mesh", True)
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("human_female_body_ruth-mesh.0",
                                                            "human_female_ruth.ruth_body_clothed_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("human_female_head_ruth-mesh.0",
                                                            "human_female_ruth.ruth_head_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("human_female_head_ruth_hair-mesh.0",
                                                            "human_female_ruth.ruth_hair_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_legs-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_belt-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_chest-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_jewel-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_boots-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_bracer_rt-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))
        hilldale_scout_DC.addSubmesh(DisplayContext.Submesh("leather_armor_bracer_lt-mesh.0",
                                                            "human_female_ruth.leather_armor_mat"))

        constable_dillon_DC = DisplayContext("human_female_ruth.mesh", True)
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("human_female_body_ruth-mesh.0",
                                                              "human_female_ruth.ruth_body_clothed_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("human_female_head_ruth-mesh.0",
                                                              "human_female_ruth.ruth_head_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxChest-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxHelmetArmor-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxLegs-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxBracerLt-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxBracerRt-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxShoulderLt-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxShoulderRt-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxBelt-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))
        constable_dillon_DC.addSubmesh(DisplayContext.Submesh("braxBoots-mesh.0",
                                                              "human_female_ruth.brax_armor_mat"))


        brax_DC = DisplayContext("brax.mesh", True)
        brax_DC.addSubmesh(DisplayContext.Submesh("braxHeadShape.0", "brax.braxBodyMat"))
        brax_DC.addSubmesh(DisplayContext.Submesh("braxBodyShape.0", "brax.braxBodyMat"))
        brax_DC.addSubmesh(DisplayContext.Submesh("braxTailShape.0", "brax.braxBodyMat"))
        brax_DC.addSubmesh(DisplayContext.Submesh("brax_shell1Shape.0", "brax.braxBodyMat"))
        brax_DC.addSubmesh(DisplayContext.Submesh("brax_shell2Shape.0", "brax.braxBodyMat"))

        #############################################################
        #
        # mob templates
        #
        #############################################################
        defaultSlots = MarsEquipInfo("default")
        defaultSlots.addEquipSlot(MarsEquipSlot.PRIMARYWEAPON)

        #
        # Wolf Template
        #
        tmpl = Template("Wolf")
        tmpl.put(WorldManagerClient.NAMESPACE,
                 WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                 DisplayContext("wolf.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "Wolf Skin")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
        tmpl.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 10))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 10))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        tmpl.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 0))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "weaponBaseDmg", 10)
        tmpl.put(CombatClient.NAMESPACE, "weaponVarDmg", 5)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Coyote Template
        #
        tmpl = Template("Coyote")
        tmpl.put(WorldManagerClient.NAMESPACE,
                 WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                 DisplayContext("wolf.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_SCALE, MVVector(0.75, 0.75, 0.75))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
        tmpl.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 10))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 10))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        tmpl.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 0))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "weaponBaseDmg", 10)
        tmpl.put(CombatClient.NAMESPACE, "weaponVarDmg", 5)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Crocodile Template
        #
        tmpl = Template("Crocodile")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                 DisplayContext("crocodile.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_RUN_THRESHOLD, Float(7000))
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
        tmpl.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 10))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 10))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        tmpl.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 0))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "weaponBaseDmg", 10)
        tmpl.put(CombatClient.NAMESPACE, "weaponVarDmg", 5)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Zombie Template
        #
        tmpl = Template("Zombie")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, zombie_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "Zombie Dust")
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
        tmpl.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 10))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 10))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        tmpl.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 0))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "weaponBaseDmg", 10)
        tmpl.put(CombatClient.NAMESPACE, "weaponVarDmg", 5)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Brax Template
        #
        tmpl = Template("Brax")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, brax_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_RUN_THRESHOLD, Float(7000))
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
        tmpl.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 10))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 10))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        tmpl.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 0))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "weaponBaseDmg", 10)
        tmpl.put(CombatClient.NAMESPACE, "weaponVarDmg", 5)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Orc Warrior Template
        #
        tmpl = Template("Orc Warrior")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, orc_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "*sword10")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 18))
        tmpl.put(CombatClient.NAMESPACE, "agility", MarsStat("agility", 18))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", 10))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 10))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", 100))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", 100))
        tmpl.put(CombatClient.NAMESPACE, "offense skill", MarsStat("offense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "defense skill", MarsStat("defense skill", 5))
        tmpl.put(CombatClient.NAMESPACE, "armor", MarsStat("armor", 0))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "weaponBaseDmg", 10)
        tmpl.put(CombatClient.NAMESPACE, "weaponVarDmg", 5)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Hilldale Scout
        #
        tmpl = Template("Hilldale Scout")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, hilldale_scout_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE,
                 WorldManagerClient.TEMPL_ORIENT,
                 Quaternion(0, 0.468, 0, 0.884))
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS,
                 "*Leather Tunic; *Leather Pants; *Leather Boots; *sword4")
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Constable Dillon
        #
        tmpl = Template("Constable Dillon")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, constable_dillon_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE,
                 WorldManagerClient.TEMPL_ORIENT,
                 Quaternion(0, 0.857, 0, -0.515))
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Cyrus Blackfire
        #
        tmpl = Template("Cyrus Blackfire")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, human_male_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, "Cyrus Blackfire")
        tmpl.put(WorldManagerClient.NAMESPACE,
                 WorldManagerClient.TEMPL_ORIENT,
                 Quaternion(0, 0.857, 0, -0.515))
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Weapon Templates
        #
        equipInfo = MarsEquipInfo("weapon")
        equipInfo.addEquipSlot(MarsEquipSlot.PRIMARYWEAPON)

        dc = DisplayContext("sword.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("Bronze Longsword")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_basic.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword1")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_broad.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword2")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_katar.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword3")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_katareen.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword4")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_leaf.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword5")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_pointy.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword6")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_sabre.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword7")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_serpent.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword8")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_short.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword9")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        dc = DisplayContext("FW_sword_human_stone.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT,
                         MarsEquipSlot.PRIMARYWEAPON,
                         MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        dcMap.add(human_female_ruth_base_DC, dc)
        dcMap.add(female_player_01_base_DC, dc)
        dcMap.add(female_player_02_base_DC, dc)
        dcMap.add(male_player_01_base_DC, dc)
        dcMap.add(male_player_02_base_DC, dc)
        tmpl = Template("sword10")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Armor Templates
        #
        equipInfo = MarsEquipInfo("armor")
        equipInfo.addEquipSlot(MarsEquipSlot.CHEST)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, human_female_leather_tunicDC)
        dcMap.add(human_female_ruth_base_DC, human_female_ruth_leather_tunic_DC)
        dcMap.add(female_player_01_base_DC, female_player_leather_tunic_DC)
        dcMap.add(female_player_02_base_DC, female_player_leather_tunic_DC)
        dcMap.add(male_player_01_base_DC, male_player_leather_tunic_DC)
        dcMap.add(male_player_02_base_DC, male_player_leather_tunic_DC)
        item = Template("Leather Tunic")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ARMOR_leather_A_chest")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(item)

        equipInfo = MarsEquipInfo("pants")
        equipInfo.addEquipSlot(MarsEquipSlot.LEGS)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, human_female_leather_pantsDC)
        dcMap.add(human_female_ruth_base_DC, human_female_ruth_leather_pants_DC)
        dcMap.add(female_player_01_base_DC, female_player_leather_pants_DC)
        dcMap.add(female_player_02_base_DC, female_player_leather_pants_DC)
        dcMap.add(male_player_01_base_DC, male_player_leather_pants_DC)
        dcMap.add(male_player_02_base_DC, male_player_leather_pants_DC)
        item = Template("Leather Pants")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ARMOR_leather_A_legs")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(item)

        equipInfo = MarsEquipInfo("boots")
        equipInfo.addEquipSlot(MarsEquipSlot.FEET)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, human_female_leather_bootsDC)
        dcMap.add(human_female_ruth_base_DC, human_female_ruth_leather_boots_DC)
        dcMap.add(female_player_01_base_DC, female_player_leather_boots_DC)
        dcMap.add(female_player_02_base_DC, female_player_leather_boots_DC)
        dcMap.add(male_player_01_base_DC, male_player_leather_boots_DC)
        dcMap.add(male_player_02_base_DC, male_player_leather_boots_DC)
        item = Template("Leather Boots")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ARMOR_leather_A_feet")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(item)

        equipInfo = MarsEquipInfo("gloves")
        equipInfo.addEquipSlot(MarsEquipSlot.HANDS)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, human_female_leather_glovesDC)
        dcMap.add(human_female_ruth_base_DC, human_female_ruth_leather_gloves_DC)
        dcMap.add(female_player_01_base_DC, female_player_leather_gloves_DC)
        dcMap.add(female_player_02_base_DC, female_player_leather_gloves_DC)
        dcMap.add(male_player_01_base_DC, male_player_leather_gloves_DC)
        dcMap.add(male_player_02_base_DC, male_player_leather_gloves_DC)
        item = Template("Leather Gloves")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ARMOR_leather_A_hands")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(item)

        #
        # Item Templates
        #
        item = Template("Wolf Skin")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\Icons\INV_hide_basic-pelt")
        ObjectManagerClient.registerTemplate(item)

        item = Template("Zombie Dust")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\Icons\INV_mined_ironore")
        ObjectManagerClient.registerTemplate(item)

        item = Template("Healing Potion")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ITEM_potion_A")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK,
                 AbilityActivateHook("heal potion"))
        ObjectManagerClient.registerTemplate(item)

        item = Template("Mana Potion")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ITEM_potion_A")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK,
                 AbilityActivateHook("restore mana potion"))
        ObjectManagerClient.registerTemplate(item)

        item = Template("Poison Potion")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ITEM_potion_A")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK,
                 AbilityActivateHook("poison potion"))
        ObjectManagerClient.registerTemplate(item)

        item = Template("Tome of Heal")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ITEM_book_C")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK,
                 AbilityActivateHook("teach self heal ability"))
        ObjectManagerClient.registerTemplate(item)

        item = Template("Tome of Fireball")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\ITEM_book_A")
        item.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK,
                 AbilityActivateHook("teach self fireball ability"))
        ObjectManagerClient.registerTemplate(item)

        #
        # Teleporter
        #
        tmpl = Template("Teleporter")
        tmpl.put(WorldManagerClient.NAMESPACE,
                 WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                 DisplayContext("tiny_cube.mesh"))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_NAME, "")
        ObjectManagerClient.registerTemplate(tmpl)

