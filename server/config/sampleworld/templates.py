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
        human_female_base_DC.addSubmesh(DisplayContext.Submesh("bodyShape-lib.0", "human_female.skin_material"))
        human_female_base_DC.addSubmesh(DisplayContext.Submesh("head_aShape-lib.0", "human_female.head_a_material"))
        
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
        
        zombie_base_DC = DisplayContext("zombie.mesh", True)
        zombie_base_DC.addSubmesh(DisplayContext.Submesh("Zombie_Body2-obj.0", "Zombie.Zombie_Body"))
        zombie_base_DC.addSubmesh(DisplayContext.Submesh("Zombie_Clothes2-obj.0", "Zombie.Zombie_Clothes"))
        
        orc_base_DC = DisplayContext("orc.mesh", True)
        
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
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, DisplayContext("wolf.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "Wolf Skin; Wolf Bones")
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 20))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 20))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", 20))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 20))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(20)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(20)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(20)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(20)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(20) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(20)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 1))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 10);
        tmpl.put(WorldManagerClient.NAMESPACE, "clickCommand", "/click")
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Coyote Template
        #
        tmpl = Template("Coyote")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, DisplayContext("wolf.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_SCALE, MVVector(0.75, 0.75, 0.75))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 60))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 60))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", 60))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 60))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(60)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(60) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 1))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 10);
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Crocodile Template
        #
        tmpl = Template("Crocodile")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                 DisplayContext("crocodile.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 60))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 60))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", 60))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 60))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(60)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(60) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 1))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 10);
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Zombie Template
        #
        tmpl = Template("Zombie")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, zombie_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 60))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 60))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", 60))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 60))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(60)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(60) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 2))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 20);
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "*sword8")
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Brax Template
        #
        tmpl = Template("Brax")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT,
                 DisplayContext("brax.mesh", True))
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 60))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 60))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", 60))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 60))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(60)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(60) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 2))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 20);
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Orc Warrior Template
        #
        tmpl = Template("Orc Warrior")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, orc_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS, "*sword10")
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 60))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 60))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom", 60))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 60))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(60)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(60)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(60) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(60)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 2))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 20);
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Human Female Leather Template
        #
        tmpl = Template("Human Female Leather")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, human_female_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS,
                 "*Leather Tunic; *Leather Pants; *Leather Boots")
        ObjectManagerClient.registerTemplate(tmpl)

        #
        # Human Female Trainer Template
        #
        tmpl = Template("Human Female Trainer")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, human_female_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.mob)
        tmpl.put(InventoryClient.NAMESPACE, InventoryClient.TEMPL_ITEMS,
                 "*Leather Tunic; *Leather Pants; *Leather Boots")
        tmpl.put(TrainerClient.NAMESPACE, "skills", "Sword;Axe;Dagger;First Aid")
        tmpl.put(CombatClient.NAMESPACE, "istrainer", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "strength", MarsStat("strength", 160))
        tmpl.put(CombatClient.NAMESPACE, "dexterity", MarsStat("dexterity", 160))
        tmpl.put(CombatClient.NAMESPACE, "wisdom", MarsStat("wisdom",160))
        tmpl.put(CombatClient.NAMESPACE, "intelligence", MarsStat("intelligence", 160))
        tmpl.put(CombatClient.NAMESPACE, "stamina", MarsStat("stamina", int(int(160)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "stamina-max", MarsStat("stamina-max", int(int(160)*1.5)))
        tmpl.put(CombatClient.NAMESPACE, "mana", MarsStat("mana", int(160)*2))
        tmpl.put(CombatClient.NAMESPACE, "mana-max", MarsStat("mana-max", int(160)* 2))
        tmpl.put(CombatClient.NAMESPACE, "health", MarsStat("health", int(160) * 2))
        tmpl.put(CombatClient.NAMESPACE, "health-max", MarsStat("health-max", int(160)*2))
        tmpl.put(CombatClient.NAMESPACE, "experience", MarsStat("experience", 0))
        tmpl.put(CombatClient.NAMESPACE, "level", MarsStat("level", 25))
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_AUTOATTACK_ABILITY, "attack ability")
        tmpl.put(CombatClient.NAMESPACE, CombatInfo.COMBAT_PROP_REGEN_EFFECT, "regen effect")
        tmpl.put(CombatClient.NAMESPACE, "attackable", Boolean(False))
        tmpl.put(CombatClient.NAMESPACE, "combat.mobflag", Boolean(True))
        tmpl.put(CombatClient.NAMESPACE, "kill_exp", 20);
        ObjectManagerClient.registerTemplate(tmpl)

	#
	# House
	#
        house_base_DC = DisplayContext("human_house_open.mesh", True)
        house_base_DC.addSubmesh(DisplayContext.Submesh("human_house_open_porchshape-obj.0", "human_house_open._01_-_Default"))
        house_base_DC.addSubmesh(DisplayContext.Submesh("human_house_open_rampshape-obj.0", "human_house_open._01_-_Default"))
        house_base_DC.addSubmesh(DisplayContext.Submesh("human_house_openshape-obj.0", "human_house_open._01_-_Default"))
        tmpl = Template("House")
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_DISPLAY_CONTEXT, house_base_DC)
        tmpl.put(WorldManagerClient.NAMESPACE, WorldManagerClient.TEMPL_OBJECT_TYPE, ObjectTypes.structure)
        ObjectManagerClient.registerTemplate(tmpl)
        
        #
        # Weapon Templates
        #
        equipInfo = MarsEquipInfo("weapon")
        equipInfo.addEquipSlot(MarsEquipSlot.PRIMARYWEAPON)
        
        dc = DisplayContext("sword.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("Bronze Longsword")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_basic.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword1")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_broad.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword2")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_katar.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword3")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_katareen.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword4")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_leaf.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword5")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_pointy.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword6")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_sabre.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword7")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_serpent.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword8")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_short.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
        tmpl = Template("sword9")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ICON,
                 "Interface\FantasyWorldIcons\WEAPON_sword_A")
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_ACTIVATE_HOOK, EquipActivateHook())
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_EQUIP_INFO, equipInfo)
        tmpl.put(InventoryClient.ITEM_NAMESPACE, InventoryClient.TEMPL_DCMAP, dcMap)
        ObjectManagerClient.registerTemplate(tmpl)
        
        dc = DisplayContext("FW_sword_human_stone.mesh")
        dc.setAttachInfo(DisplayState.IN_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dc.setAttachInfo(DisplayState.NON_COMBAT, MarsEquipSlot.PRIMARYWEAPON, MarsAttachSocket.PRIMARYWEAPON)
        dcMap = DCMap()
        dcMap.add(human_female_base_DC, dc)
        dcMap.add(orc_base_DC, dc)
        dcMap.add(zombie_base_DC, dc)
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
        
        item = Template("Wolf Bones")
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
