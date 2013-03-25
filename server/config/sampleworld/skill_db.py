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
True=1
False=0


# Define Weapon Skills

skill = MarsCombatSkill("Sword")
skill.setDefaultAbility("Wounding Thrust")
skill.setExperiencePerUse(1)
skill.setBaseExpThreshold(10)
skill.setMaxRank(3)
# create a leveling map for this skill
swordlm = LevelingMap()
swordlm.setAllLevelModification(0.5, 0)
skill.setLevelingMap(swordlm)
Mars.SkillManager.register(skill.getName(), skill)
skill = MarsCombatSkill("Axe")
skill.setDefaultAbility("Cleave")
skill.setExperiencePerUse(1)
skill.setBaseExpThreshold(10)
skill.setMaxRank(3)
# create a leveling map for this skill
axelm = LevelingMap()
axelm.setAllLevelModification(0.0, 10)
skill.setLevelingMap(axelm)
Mars.SkillManager.register(skill.getName(), skill)
skill = MarsCombatSkill("Dagger")
skill.setDefaultAbility("Pierce")
skill.setExperiencePerUse(1)
skill.setBaseExpThreshold(10)
skill.setMaxRank(3)
# create a leveling map for this skill
daggerlm = LevelingMap()
daggerlm.setAllLevelModification(0.0, 10)
skill.setLevelingMap(daggerlm)
Mars.SkillManager.register(skill.getName(), skill)
skill = MarsCombatSkill("Thrown Weapons")
skill.setDefaultAbility("Flying Dagger")
skill.setExperiencePerUse(1)
skill.setBaseExpThreshold(10)
skill.setMaxRank(3)
# create a leveling map for this skill
thrownlm = LevelingMap()
thrownlm.setAllLevelModification(0.0, 10)
skill.setLevelingMap(thrownlm)
Mars.SkillManager.register(skill.getName(), skill)
# Define Survival Skills
skill = MarsSkill("First Aid")
skill.setDefaultAbility("Lesser Bandages")
skill.setExperiencePerUse(1)
skill.setBaseExpThreshold(10)
skill.setMaxRank(3)
# create a leveling map for this skill
firstaidlm = LevelingMap()
firstaidlm.setAllLevelModification(0.0, 10)
skill.setLevelingMap(firstaidlm)
Mars.SkillManager.register(skill.getName(), skill)
